using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Movement Info")]
    public DiceRoller dice;
    public int currentTileIndex = 0;
    public Board board;
    public bool isMoving = false;
    public int budget = 2500;

    [Header("Corruption Info")]
    public Slider corruptBar;
    public Image corruptFillImage;
    public Color[] corruptColors;   // 0 - low, 1 - mid, 2 - high
    public int corruptValue = 0;
    public int maxCorruptValue = 50;

    [Header("UI Elements")]
    public Text budgetText;
    public Sprite budgetIcon;
    public Transform budgetItemTransform;
    public Transform budgetTransform;
    public GameObject buyPropertyUI;
    public GameObject insufficientFundsUI;
    public GameObject sellPropertyUI;
    public GameObject sellButtonPrefab;
    public Transform sellButtonParent;
    public GameObject noMorePropertyUI;
    public GameObject gameOverUI;
    public GameObject successUI;
    public EditProperty editPropertyUI;
    public Transform governmentSpawnPoint;

    [Header("Managers")]
    public SpawnItemCollectionManager itemCollectionManager;
    public MysteryCardManager mysteryCardManager;

    [Header("Player Info")]
    public int extraCustomerCount = 1;
    public bool hasVolunteerInitiative = false;
    public bool hasYouthStartupAid = false;
    public bool hasBayanihanSpirit = false;
    public bool hasSkipNextTax = false;
    public bool hasGhostEmployeeEffect = false;
    public int reducedRevenueTurns = 0;

    private Tile currentTile;
    private int totalTilesWithCustomersRemaining;

    public void Start()
    {
        currentTile = board.tiles[currentTileIndex];
        UpdateUI();
    }

    private void UpdateUI()
    {
        UpdateBudgetUI();
        UpdateCorruptBarUI();
        buyPropertyUI.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() => BuyProperty());
        buyPropertyUI.SetActive(false);
        insufficientFundsUI.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() => OpenSellPropertyUI());
        insufficientFundsUI.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(() => TriggerGameOver());
        insufficientFundsUI.SetActive(false);
        sellPropertyUI.SetActive(false);
        noMorePropertyUI.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => TriggerGameOver());
        noMorePropertyUI.SetActive(false);
        gameOverUI.SetActive(false);
        successUI.SetActive(false);
        editPropertyUI.gameObject.SetActive(false);
    }

    public void ActivateDice()
    {
        dice.rollButton.interactable = true;
        dice.rollText.SetActive(true);
    }

    public void Move(int steps)
    {
        if (!isMoving)
        {
            StartCoroutine(MoveStepByStep(steps));
        }
    }

    private IEnumerator MoveStepByStep(int steps)
    {
        isMoving = true;
        for (int i = 0; i < steps; i++)
        {
            currentTileIndex = (currentTileIndex + 1) % board.tiles.Length;
            Vector3 targetPosition = board.tiles[currentTileIndex].transform.position + (Vector3.up * 0.2f);
            while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);
                yield return null;
            }
            transform.position = targetPosition;
            yield return new WaitForSeconds(0.2f);

            if (board.tiles[currentTileIndex].tileData.tileType == TileType.Start)
            {
                if (hasSkipNextTax)
                {
                    hasSkipNextTax = false;
                    Debug.Log("Cash Overflow effect active... skipping tax collection.");
                }
                else
                {
                    yield return StartCoroutine(ProcessAllProperties());
                }

                CheckForGameSuccess();
            }
        }
        isMoving = false;
        
        currentTile = board.tiles[currentTileIndex];
        currentTile.tileData.OnLand(this);

        if (board.tiles[currentTileIndex].tileData.tileType == TileType.Property)
        {
            InitializeCustomers();
        }
        else if (board.tiles[currentTileIndex].tileData.tileType == TileType.Mystery)
        {
            mysteryCardManager.ShowMysteryCards(this);
        }
        else
        {
            ActivateDice();
        }
    }

    private void InitializeCustomers()
    {
        totalTilesWithCustomersRemaining = 0;
        List<Tile> boughtTiles = new List<Tile>();
        foreach (var tile in board.tiles)
        {
            if (tile.runtimePropertyData != null && tile.runtimePropertyData.isBought)
            {
                boughtTiles.Add(tile);
                totalTilesWithCustomersRemaining++;
            }
        }

        if (totalTilesWithCustomersRemaining == 0)
        {
            OnCustomersFinished();
            return;
        }

        foreach (var tile in boughtTiles)
        {
            tile.SpawnCustomers(this, (hasVolunteerInitiative) ? extraCustomerCount : 0, OnCustomersFinished);
        }
        hasVolunteerInitiative = false;
    }

    private void OnCustomersFinished()
    {
        totalTilesWithCustomersRemaining--;
        if (totalTilesWithCustomersRemaining > 0) return;
        
        if (currentTile.tileData.tileType == TileType.Property && currentTile.runtimePropertyData != null)
        {
            if (currentTile.runtimePropertyData.isBought)
            {
                ActivateDice();
                UpdateEditPropertyVisual();
            }
            else
            {
                CheckPropertyPurchase();
            }
        }
        else
        {
            ActivateDice();
        }
    }

    private IEnumerator ProcessAllProperties()
    {
        foreach (Tile tile in board.tiles)
        {
            if (tile.runtimePropertyData == null || !tile.runtimePropertyData.isBought) continue;
            
            yield return new WaitForSeconds(0.2f);
            StartCoroutine(tile.ProcessTax(this));
        }
        yield return new WaitForSeconds(2f);
    }

    public void UpdateBudgetUI()
    {
        budgetText.text = $"P {budget}";
    }

    private void UpdateAllPropertiesVisual()
    {
        foreach (Tile tile in board.tiles)
        {
            tile.UpdatePropertyVisual();
        }
    }

    private void UpdateEditPropertyVisual()
    {
        editPropertyUI.gameObject.SetActive(true);
        editPropertyUI.Initialize(currentTile);
    }

    private void CheckPropertyPurchase()
    {
        if (budget >= currentTile.runtimePropertyData.purchaseCost)
        {
            buyPropertyUI.SetActive(true);
            buyPropertyUI.transform.GetChild(0).GetComponent<Text>().text = currentTile.tileData.tileName;
            buyPropertyUI.transform.GetChild(1).GetComponent<Image>().sprite = currentTile.tileData.tileLogoSprite;

            int price = currentTile.runtimePropertyData.purchaseCost;
            if (hasYouthStartupAid)
            {
                price /= 2;
                buyPropertyUI.transform.GetChild(3).GetChild(0).GetComponent<Text>().text = $"P {price} (50% OFF)";
            }
            else
            {
                buyPropertyUI.transform.GetChild(3).GetChild(0).GetComponent<Text>().text = $"P {price}";
            }
        }
        else
        {
            if (HasOwnedProperties())
            {
                insufficientFundsUI.transform.GetChild(0).GetComponent<Text>().text = currentTile.tileData.tileName;
                insufficientFundsUI.transform.GetChild(1).GetComponent<Image>().sprite = currentTile.tileData.tileLogoSprite;
                insufficientFundsUI.transform.GetChild(2).GetComponent<Text>().text =  $"NEED P {currentTile.runtimePropertyData.purchaseCost - budget} MORE";
                insufficientFundsUI.SetActive(true);
                GenerateSellButtons();
            }
            else
            {
                noMorePropertyUI.SetActive(true);
            }
        }
    }

    public void BuyProperty()
    {
        int price = currentTile.runtimePropertyData.purchaseCost;
        if (hasYouthStartupAid)
        {
            price /= 2;
            hasYouthStartupAid = false;
        }
        
        budget -= price;
        UpdateBudgetUI();
        
        currentTile.runtimePropertyData.isBought = true;
        currentTile.UpdatePropertyVisual();
        buyPropertyUI.SetActive(false);
        UpdateEditPropertyVisual();

        ActivateDice();

        Debug.Log($"{name} bought {currentTile.tileData.tileName} for {currentTile.runtimePropertyData.purchaseCost}. Remaining budget: {budget}");
    }

    public void OpenSellPropertyUI()
    {
        insufficientFundsUI.SetActive(false);
        sellPropertyUI.SetActive(true);
    }

    private void GenerateSellButtons()
    {
        foreach (Transform child in sellButtonParent)
        {
            Destroy(child.gameObject);
        }

        foreach (Tile tile in board.tiles)
        {
            if (tile.runtimePropertyData != null && tile.runtimePropertyData.isBought)
            {
                GameObject button = Instantiate(sellButtonPrefab, sellButtonParent);
                button.transform.position = tile.propertyImage.transform.position;
                button.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(SellProperty(tile)));
                button.transform.GetChild(0).GetComponent<Text>().text = $"+P{Mathf.RoundToInt(tile.runtimePropertyData.purchaseCost * tile.runtimePropertyData.sellRate)}";
            }
        }
    }

    public IEnumerator SellProperty(Tile tile)
    {
        foreach (Transform child in sellButtonParent)
        {
            Destroy(child.gameObject);
        }
        int earnings = Mathf.RoundToInt(tile.runtimePropertyData.purchaseCost * tile.runtimePropertyData.sellRate);
        yield return StartCoroutine(BudgetCollection(earnings, tile.propertyImage.transform.position));

        tile.runtimePropertyData.revenue = 0;
        tile.runtimePropertyData.isBought = false;
        UpdateAllPropertiesVisual();
        
        sellPropertyUI.SetActive(false);
        CheckPropertyPurchase();
    }

    public void GetBudgetFromGovernment(int amount)
    {
        StartCoroutine(BudgetCollection(amount, governmentSpawnPoint.position));
    }

    public IEnumerator BudgetCollection(float earnings, Vector3 spawnPosition)
    {
        itemCollectionManager.Initialize(budgetIcon, budgetItemTransform, spawnPosition, budgetTransform.position);
        yield return new WaitForSeconds(2f);

        budget += Mathf.RoundToInt(earnings);
        UpdateBudgetUI();
    }

    private bool HasOwnedProperties()
    {
        foreach (Tile tile in board.tiles)
        {
            if (tile.runtimePropertyData != null && tile.runtimePropertyData.isBought)
            {
                return true;
            }
        }
        return false;
    }

    public void AddCorruptValue(int value)
    {
        corruptValue += value;
        corruptValue = Mathf.Clamp(corruptValue, 0, maxCorruptValue);
        UpdateCorruptBarUI();

        if (corruptValue >= maxCorruptValue)
        {
            TriggerGameOver();
        }
    }

    public void SubtractCorruptValue(int value)
    {
        corruptValue -= value;
        corruptValue = Mathf.Clamp(corruptValue, 0, maxCorruptValue);
        UpdateCorruptBarUI();
    }

    private void UpdateCorruptBarUI()
    {
        if (corruptBar != null)
        {
            float progress = (float)corruptValue / maxCorruptValue;
            corruptBar.value = progress;

            if (corruptFillImage != null)
            {
                Color lowColor = corruptColors[0];
                Color midColor = corruptColors[1];
                Color highColor = corruptColors[2];

                Color lerpedColor;
                if (progress < 0.5f)
                {
                    lerpedColor = Color.Lerp(lowColor, midColor, progress / 0.5f);
                }
                else
                {
                    lerpedColor = Color.Lerp(midColor, highColor, (progress - 0.5f) / 0.5f);
                }

                corruptFillImage.color = lerpedColor;
            }
        }
    }

    private void CheckForGameSuccess()
    {
        int ownedPropertyCount = 0;
        int satisfiedPropertyCount = 0;

        foreach (var tile in board.tiles)
        {
            var data = tile.runtimePropertyData;
            if (data != null && data.isBought)
            {
                ownedPropertyCount++;

                if (data.marketPrice <= data.reasonableMarketPrice &&
                    data.taxRate <= data.reasonableTaxRate)
                {
                    satisfiedPropertyCount++;
                }
            }
        }

        if (ownedPropertyCount >= 13 && satisfiedPropertyCount >= 13)
        {
            TriggerGameSuccess();
        }
    }

    private void TriggerGameOver()
    {
        noMorePropertyUI.SetActive(false);
        insufficientFundsUI.SetActive(false);
        gameOverUI.SetActive(true);
    }

    private void TriggerGameSuccess()
    {
        successUI.SetActive(true);
    }
}
