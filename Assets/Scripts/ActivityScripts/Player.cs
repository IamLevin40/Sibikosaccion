using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public DiceRoller dice;
    public int currentTileIndex = 0;
    public Board board;
    public bool isMoving = false;
    public int budget = 2500;

    public Text budgetText;
    public GameObject buyPropertyUI;
    public GameObject insufficientFundsUI;
    public GameObject sellPropertyUI;
    public GameObject sellButtonPrefab;
    public Transform sellButtonParent;
    public GameObject noMorePropertyUI;
    public GameObject gameOverUI;
    public GameObject successUI;

    private Tile currentTile;

    public void Start()
    {
        currentTile = board.tiles[currentTileIndex];
        UpdateUI();
    }

    private void UpdateUI()
    {
        UpdateBudgetUI();
        buyPropertyUI.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => BuyProperty());
        buyPropertyUI.SetActive(false);
        insufficientFundsUI.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => OpenSellPropertyUI());
        insufficientFundsUI.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => TriggerGameOver());
        insufficientFundsUI.SetActive(false);
        sellPropertyUI.SetActive(false);
        noMorePropertyUI.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => TriggerGameOver());
        noMorePropertyUI.SetActive(false);
        gameOverUI.SetActive(false);
        successUI.SetActive(false);
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
        }
        
        currentTile = board.tiles[currentTileIndex];
        currentTile.tileData.OnLand(this);

        if (currentTile.tileData.tileType == TileType.Property && currentTile.runtimePropertyData != null && !currentTile.runtimePropertyData.isBought)
        {
            CheckPropertyPurchase();
        }
        else
        {
            dice.rollButton.interactable = true;
        }
        
        isMoving = false;
    }

    private void UpdateBudgetUI()
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

    private void CheckPropertyPurchase()
    {
        if (budget >= currentTile.runtimePropertyData.purchaseCost)
        {
            buyPropertyUI.SetActive(true);
            buyPropertyUI.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = $"P {currentTile.runtimePropertyData.purchaseCost}";
        }
        else
        {
            if (HasOwnedProperties())
            {
                insufficientFundsUI.transform.GetChild(0).GetComponent<Text>().text =  $"NEED P {currentTile.runtimePropertyData.purchaseCost - budget} MORE";
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
        budget -= currentTile.runtimePropertyData.purchaseCost;
        UpdateBudgetUI();
        
        currentTile.runtimePropertyData.isBought = true;
        currentTile.UpdatePropertyVisual();
        buyPropertyUI.SetActive(false);
        dice.rollButton.interactable = true;

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
                button.GetComponent<Button>().onClick.AddListener(() => SellProperty(tile.runtimePropertyData));
                button.transform.GetChild(0).GetComponent<Text>().text = $"+P{Mathf.RoundToInt(tile.runtimePropertyData.purchaseCost * tile.runtimePropertyData.sellRate)}";
            }
        }
    }

    public void SellProperty(PropertyData property)
    {
        budget += Mathf.RoundToInt(property.purchaseCost * property.sellRate);
        UpdateBudgetUI();

        property.isBought = false;
        UpdateAllPropertiesVisual();
        
        sellPropertyUI.SetActive(false);
        CheckPropertyPurchase();
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

    private void TriggerGameOver()
    {
        noMorePropertyUI.SetActive(false);
        insufficientFundsUI.SetActive(false);
        gameOverUI.SetActive(true);
    }
}
