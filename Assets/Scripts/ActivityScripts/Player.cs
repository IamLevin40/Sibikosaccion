using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Movement Info")]
    public DiceRoller dice;
    public int currentTileIndex = 0;
    public Board board;
    public bool isMoving = false;
    public int budget = 2500;
    public int maxBudget = 25000;

    [Header("Corruption Info")]
    public List<GameObject> corruptBars;
    public List<Image> corruptBarImages;
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
    public EditPropertyManager editPropertyUI;
    public Transform governmentSpawnPoint;
    public Text gameOverReasonText;
    public Button successGoHomeButton;
    public Button gameOverGoHomeButton;

    [Header("Managers")]
    public SpawnItemCollectionManager itemCollectionManager;
    public MysteryCardManager mysteryCardManager;
    public VisualItemManager visualItemManager;

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
    private bool isGameOver = false;

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
        insufficientFundsUI.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(() => TriggerGameOver(2));
        insufficientFundsUI.SetActive(false);
        sellPropertyUI.SetActive(false);
        noMorePropertyUI.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => TriggerGameOver(2));
        noMorePropertyUI.SetActive(false);
        gameOverUI.SetActive(false);
        successUI.SetActive(false);
        editPropertyUI.gameObject.SetActive(false);
        successGoHomeButton.onClick.AddListener(() => GoHome());
        gameOverGoHomeButton.onClick.AddListener(() => GoHome());
    }

    public void ActivateDice()
    {
        if (isGameOver) return;
        dice.rollButton.interactable = true;
        dice.rollText.SetActive(true);
    }

    public void Move(int steps)
    {
        if (isGameOver || isMoving) return;
        StartCoroutine(MoveStepByStep(steps));
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
                    foreach (var tile in board.tiles)
                    {
                        var data = tile.runtimePropertyData;
                        if (data != null && data.isBought)
                        {
                            visualItemManager.PlayVisualItem(VisualItemType.AscendItemPop, "property_no_tax", 2.5f, tile.propertyImage.transform.position);
                        }
                    }
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
            if (hasVolunteerInitiative)
            {
                visualItemManager.PlayVisualItem(VisualItemType.AscendItemPop, "volunteer_initiative", 1.5f, tile.propertyImage.transform.position);
            }
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
        editPropertyUI.showStatsPropertyManager.Initialize(this);
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
                visualItemManager.PlayVisualItem(VisualItemType.AscendItemPop, "property_sale", 1.5f, currentTile.propertyImage.transform.position);
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
        visualItemManager.PlayVisualItem(VisualItemType.AscendItemPop, "property_purchased", 1.5f, currentTile.propertyImage.transform.position);

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

        tile.runtimePropertyData.revenue = 0;
        tile.runtimePropertyData.isBought = false;
        UpdateAllPropertiesVisual();
        visualItemManager.PlayVisualItem(VisualItemType.DescendItemPop, "property_abandoned", 1.5f, tile.propertyImage.transform.position);
        yield return new WaitForSeconds(1f);

        int earnings = Mathf.RoundToInt(tile.runtimePropertyData.purchaseCost * tile.runtimePropertyData.sellRate);
        yield return StartCoroutine(BudgetCollection(earnings, tile.propertyImage.transform.position));
    
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
        CheckForExceedBudget();
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
            TriggerGameOver(1);
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
        if (corruptBars == null || corruptBars.Count != 10 || corruptBarImages.Count != 10)
            return;

        float progress = (float)corruptValue / maxCorruptValue;

        for (int i = 0; i < 10; i++)
        {
            float threshold = (i + 1) * 0.1f;

            bool shouldBeActive = progress >= threshold;
            corruptBars[i].SetActive(shouldBeActive);

            if (shouldBeActive && corruptBarImages[i] != null)
            {
                float stepProgress = (i + 1) / 10f;

                Color lowColor = corruptColors[0];
                Color midColor = corruptColors[1];
                Color highColor = corruptColors[2];

                Color lerpedColor;
                if (stepProgress < 0.5f)
                {
                    lerpedColor = Color.Lerp(lowColor, midColor, stepProgress / 0.5f);
                }
                else
                {
                    lerpedColor = Color.Lerp(midColor, highColor, (stepProgress - 0.5f) / 0.5f);
                }

                corruptBarImages[i].color = lerpedColor;
            }
        }
    }

    public void CheckForExceedBudget()
    {
        if (budget > maxBudget)
        {
            TriggerGameOver(0);
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

        if (ownedPropertyCount >= 13 && satisfiedPropertyCount >= 13 && budget <= maxBudget)
        {
            TriggerGameSuccess();
        }
    }

    private void TriggerGameOver(int reason)
    {
        isGameOver = true;
        Time.timeScale = 0f;
        
        noMorePropertyUI.SetActive(false);
        insufficientFundsUI.SetActive(false);
        gameOverUI.SetActive(true);

        string reasonText = reason switch
        {
            0 => "Corrupt! You have exceeded the budget.",
            1 => "Corrupt! You have made too much corruption move.",
            2 => "Bankrupt! You have no more budget.",
            _ => "Game Over!"
        };
        gameOverReasonText.text = reasonText;
    }

    private void TriggerGameSuccess()
    {
        isGameOver = true;
        Time.timeScale = 0f;
        
        successUI.SetActive(true);
    }

    private void GoHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
