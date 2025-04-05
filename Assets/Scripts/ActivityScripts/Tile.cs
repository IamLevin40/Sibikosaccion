using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [Header("Tile Info")]
    public TileData tileData;

    [Header("Property Info")]
    public PropertyData propertyData;
    [HideInInspector] public PropertyData runtimePropertyData;
    public SpriteRenderer propertyImage;
    public Transform customerSpawnPoint;
    public Transform customerEndPoint;
    public Transform customersTransform;
    public Transform messageMarkersTransform;
    public GameObject messageMarkerPrefab;
    public GameObject[] customerVariants;
    public Sprite[] messageTaxSprites;  // 0 - angry, 1 - lend_money

    [Header("Mystery Info")]

    private int customersRemaining;
    private System.Action onCustomersFinished;

    private void Start()
    {
        if (propertyData != null)
        {
            runtimePropertyData = Instantiate(propertyData);
        }
        UpdatePropertyVisual();
    }

    public void UpdatePropertyVisual()
    {
        if (runtimePropertyData != null)
        {
            propertyImage.sprite = runtimePropertyData.isBought ? runtimePropertyData.boughtSprite : runtimePropertyData.abandonSprite;
        }
    }

    public void SpawnCustomers(Player player, int extraCustomer, System.Action onComplete)
    {
        int maxCustomerCount = 0;
        
        if (runtimePropertyData.isCursedByLandGrab)
        {
            runtimePropertyData.maxCustomer--;
            if (runtimePropertyData.maxCustomer <= runtimePropertyData.minCustomer)
            {
                runtimePropertyData.isCursedByLandGrab = false;
                runtimePropertyData.isBought = false;
                UpdatePropertyVisual();

                runtimePropertyData.marketPrice = propertyData.marketPrice;
                runtimePropertyData.taxRate = propertyData.taxRate;
                runtimePropertyData.maxCustomer = propertyData.maxCustomer;

                if (player.currentTileIndex == System.Array.IndexOf(player.board.tiles, this))
                {
                    player.ActivateDice();
                }

                return;
            }
            else
            {
                maxCustomerCount = runtimePropertyData.maxCustomer;
            }
        }
        else
        {
            maxCustomerCount = runtimePropertyData.maxCustomer;
        }
        
        int customerCount = Random.Range(runtimePropertyData.minCustomer, maxCustomerCount + 1) + extraCustomer;
        customersRemaining = customerCount;
        onCustomersFinished = onComplete;
        StartCoroutine(SpawnCustomerRoutine(player, customerCount));
    }

    private IEnumerator SpawnCustomerRoutine(Player player, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject customerPrefab = customerVariants[Random.Range(0, customerVariants.Length)];
            GameObject newCustomer = Instantiate(customerPrefab, customersTransform);
            Vector3 spawnOffset = new Vector3(Random.Range(0f, 0.25f), Random.Range(0f, 0.25f), 0);
            newCustomer.transform.position = customerSpawnPoint.position + spawnOffset;
            newCustomer.GetComponent<Customer>().Initialize(player, this, customerEndPoint, OnCustomerExit);
            yield return new WaitForSeconds(Random.Range(0.25f, 5f / count));
        }
    }

    private void OnCustomerExit()
    {
        customersRemaining--;
        if (customersRemaining <= 0)
        {
            Debug.Log($"Property: {tileData.tileName}, Revenue: {runtimePropertyData.revenue}");
            onCustomersFinished?.Invoke();
        }
    }

    public IEnumerator ProcessTax(Player player)
    {
        if (tileData.tileType != TileType.Property || runtimePropertyData == null || !runtimePropertyData.isBought || runtimePropertyData.revenue == 0) yield break;

        GameObject marker = Instantiate(messageMarkerPrefab, messageMarkersTransform);
        marker.transform.position = propertyImage.transform.position + (Vector3.up * 0.5f);
        Image markerImage = marker.transform.GetChild(0).GetComponent<Image>();
        Vector3 spawnPosition = propertyImage.transform.position;

        float maxTax = runtimePropertyData.reasonableTaxRate * 1.2f;
        float angryChance = Mathf.Clamp01((runtimePropertyData.taxRate - runtimePropertyData.reasonableTaxRate) / (maxTax - runtimePropertyData.reasonableTaxRate));

        if (Random.value < angryChance)
        {
            markerImage.sprite = messageTaxSprites[0];
            runtimePropertyData.isBought = false;
            UpdatePropertyVisual();

            player.AddCorruptValue(5);
        }
        else
        {
            markerImage.sprite = messageTaxSprites[1];
            int totalTaxRate = runtimePropertyData.taxRate + (player.hasBayanihanSpirit ? 5 : 0);
            float earnings = runtimePropertyData.revenue * (totalTaxRate / 100f);

            if (player.hasGhostEmployeeEffect && runtimePropertyData.taxRate > 10f)
            {
                earnings *= 0.75f;
            }

            yield return StartCoroutine(player.BudgetCollection(earnings, spawnPosition));
        }

        runtimePropertyData.revenue = 0;
        player.hasBayanihanSpirit = false;
        Destroy(marker);
    }
}