using UnityEngine;

[CreateAssetMenu(fileName = "NewProperty", menuName = "Objects/BoardGame/Property")]
public class PropertyData : ScriptableObject
{
    [Header("Property Info")]
    [SerializeField] public bool isBought = false;
    public int purchaseCost;
    public float sellRate = 0.8f;
    public Sprite abandonSprite;
    public Sprite boughtSprite;
    public int marketPrice;
    public int reasonableMarketPrice;
    public int taxRate;
    public int reasonableTaxRate;
    public int revenue;
    public int minCustomer = 1;
    public int maxCustomer = 4;

    [Header("Mystery Upgrades")]
    public int permanentPriceIncreaseRate = 10;
    public bool hasPermanentPriceIncrease = false;
    public bool isCursedByLandGrab = false;
}