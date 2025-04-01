using UnityEngine;

[CreateAssetMenu(fileName = "NewProperty", menuName = "Objects/BoardGame/Property")]
public class PropertyData : ScriptableObject
{
    [SerializeField] public bool isBought = false;
    public int purchaseCost;
    public float sellRate = 0.8f;
    public Sprite abandonSprite;
    public Sprite boughtSprite;
    public int pricePerProduct;
    public int taxRate;
    public int revenue;
}