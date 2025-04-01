using UnityEngine;
using UnityEngine.UI;

public class EditProperty : MonoBehaviour
{
    public Text tileNameText;
    public Image tileLogoImage;
    public InputField marketPriceInput;
    public InputField taxRateInput;

    private PropertyData propertyData;

    public void Initialize(Tile tile)
    {
        propertyData = tile.runtimePropertyData;
        tileNameText.text = tile.tileData.tileName;
        tileLogoImage.sprite = tile.tileData.tileLogoSprite;
        marketPriceInput.text = propertyData.marketPrice.ToString();
        taxRateInput.text = propertyData.taxRate.ToString();

        marketPriceInput.onValueChanged.AddListener(UpdateMarketPrice);
        taxRateInput.onValueChanged.AddListener(UpdateTaxRate);
    }

    private void UpdateMarketPrice(string value)
    {
        if (int.TryParse(value, out int result) && result >= 0)
        {
            propertyData.marketPrice = result;
        }
        else
        {
            marketPriceInput.text = propertyData.marketPrice.ToString();
        }
    }

    private void UpdateTaxRate(string value)
    {
        if (int.TryParse(value, out int result) && result >= 0 && result <= 100)
        {
            propertyData.taxRate = result;
        }
        else
        {
            taxRateInput.text = propertyData.taxRate.ToString();
        }
    }
}