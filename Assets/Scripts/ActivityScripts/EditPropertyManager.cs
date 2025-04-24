using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class EditPropertyManager : MonoBehaviour
{
    public Text tileNameText;
    public Image tileLogoImage;
    public InputField marketPriceInput;
    public InputField taxRateInput;
    public ShowStatsPropertyManager showStatsPropertyManager;

    private PropertyData propertyData;
    private static readonly Regex digitsOnly = new Regex(@"^\d*$");

    public void Initialize(Tile tile)
    {
        propertyData = tile.runtimePropertyData;
        tileNameText.text = tile.tileData.tileName;
        tileLogoImage.sprite = tile.tileData.tileLogoSprite;
        marketPriceInput.text = propertyData.marketPrice.ToString();
        taxRateInput.text = propertyData.taxRate.ToString();

        marketPriceInput.onValueChanged.AddListener(FilterDigitsOnly);
        marketPriceInput.onEndEdit.AddListener(ValidateMarketPrice);

        taxRateInput.onValueChanged.AddListener(FilterDigitsOnly);
        taxRateInput.onEndEdit.AddListener(ValidateTaxRate);
    }

    private void FilterDigitsOnly(string input)
    {
        InputField field = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject?.GetComponent<InputField>();
        if (field != null && !digitsOnly.IsMatch(input))
        {
            string filtered = Regex.Replace(input, @"\D", "");
            field.text = filtered;
        }
    }

    private void ValidateMarketPrice(string input)
    {
        if (int.TryParse(input, out int result) && result >= 0)
        {
            propertyData.marketPrice = result;
        }
        else
        {
            propertyData.marketPrice = 0;
        }
        marketPriceInput.text = propertyData.marketPrice.ToString();
    }

    private void ValidateTaxRate(string input)
    {
        if (int.TryParse(input, out int result) && result >= 0 && result <= 100)
        {
            propertyData.taxRate = result;
        }
        else if (int.TryParse(input, out result) && result < 0)
        {
            propertyData.taxRate = 0;
        }
        else if (int.TryParse(input, out result) && result > 100)
        {
            propertyData.taxRate = 100;
        }
        else
        {
            propertyData.taxRate = 0;
        }
        taxRateInput.text = propertyData.taxRate.ToString();
    }
}
