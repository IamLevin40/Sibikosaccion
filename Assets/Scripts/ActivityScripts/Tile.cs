using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public TileData tileData;
    public PropertyData propertyData;
    [HideInInspector] public PropertyData runtimePropertyData;
    public SpriteRenderer propertyImage;
    public Transform customerSpawnPoint;
    public Transform customerEndPoint;
    public Transform customersUI;
    public Transform messageMarkersUI;
    public GameObject messageMarkerPrefab;
    public Sprite lendMoneySprite;
    public Sprite angrySprite;
    public GameObject[] customerVariants;

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

    public void SpawnCustomers(System.Action onComplete)
    {
        int customerCount = Random.Range(runtimePropertyData.minCustomer, runtimePropertyData.maxCustomer + 1);
        customersRemaining = customerCount;
        onCustomersFinished = onComplete;
        StartCoroutine(SpawnCustomerRoutine(customerCount));
    }

    private IEnumerator SpawnCustomerRoutine(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject customerPrefab = customerVariants[Random.Range(0, customerVariants.Length)];
            GameObject newCustomer = Instantiate(customerPrefab, customersUI);
            Vector3 spawnOffset = new Vector3(Random.Range(0f, 0.25f), Random.Range(0f, 0.25f), 0);
            newCustomer.transform.position = customerSpawnPoint.position + spawnOffset;
            newCustomer.GetComponent<Customer>().Initialize(this, customerEndPoint, OnCustomerExit);
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
        if (tileData.tileType != TileType.Property || runtimePropertyData == null || !runtimePropertyData.isBought) yield break;

        GameObject marker = Instantiate(messageMarkerPrefab, messageMarkersUI);
        marker.transform.position = propertyImage.transform.position + (Vector3.up * 0.5f);
        Image markerImage = marker.transform.GetChild(0).GetComponent<Image>();

        float maxTax = runtimePropertyData.reasonableTaxRate * 1.2f;
        float angryChance = Mathf.Clamp01((runtimePropertyData.taxRate - runtimePropertyData.reasonableTaxRate) / (maxTax - runtimePropertyData.reasonableTaxRate));

        if (Random.value < angryChance)
        {
            runtimePropertyData.isBought = false;
            UpdatePropertyVisual();
            markerImage.sprite = angrySprite;
        }
        else
        {
            float earnings = runtimePropertyData.revenue * (runtimePropertyData.taxRate / 100f);
            player.budget += Mathf.RoundToInt(earnings);
            player.UpdateBudgetUI();
            markerImage.sprite = lendMoneySprite;
        }

        yield return new WaitForSeconds(1.5f);
        Destroy(marker);
    }
}