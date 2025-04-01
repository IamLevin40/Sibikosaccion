using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public TileData tileData;
    public PropertyData propertyData;
    public PropertyData runtimePropertyData;
    public SpriteRenderer propertyImage;

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
}