using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum Emotion { Happy, Love, Angry }

public class Customer : MonoBehaviour
{
    public Emotion emotion = Emotion.Happy;
    public Image image;
    public Sprite happySprite;
    public Sprite loveSprite;
    public Sprite angrySprite;
    private Tile propertyTile;
    private Transform endPoint;
    private System.Action onExitComplete;

    public void Initialize(Player player, Tile tile, Transform exitPoint, System.Action onComplete)
    {
        propertyTile = tile;
        endPoint = exitPoint;
        onExitComplete = onComplete;
        StartCoroutine(MoveToProperty(player));
    }

    private IEnumerator MoveToProperty(Player player)
    {
        while (Vector3.Distance(transform.position, propertyTile.propertyImage.transform.position) > 0.1f)
        {
            transform.position = Vector3.Lerp(transform.position, propertyTile.propertyImage.transform.position, Time.deltaTime * 2f);
            yield return null;
        }

        image.enabled = false;
        EvaluateProperty(player);

        yield return new WaitForSeconds(0.5f);
        image.enabled = true;
        StartCoroutine(MoveToExit());
    }

    private void EvaluateProperty(Player player)
    {
        PropertyData property = propertyTile.runtimePropertyData;
        int price = property.marketPrice;
        int reasonablePrice = property.reasonableMarketPrice;
        float maxPrice = reasonablePrice * 1.2f;
        
        float angryChance = Mathf.Clamp01((price - reasonablePrice) / (maxPrice - reasonablePrice));
        
        if (Random.value < angryChance)
        {
            emotion = Emotion.Angry;
            player.AddCorruptValue(1);
        }
        else
        {
            emotion = Emotion.Love;
            int priceIncrease = 0;

            if (player.reducedRevenueTurns > 0)
            {
                price = Mathf.RoundToInt(price * 0.9f);
                player.reducedRevenueTurns--;
            }

            if (property.hasPermanentPriceIncrease)
            {
                priceIncrease = Mathf.RoundToInt(price * ((float)property.permanentPriceIncreaseRate / 100f));
                Debug.Log($"Price increase by {priceIncrease} for the property {propertyTile.tileData.tileName}.");
            }
            
            property.revenue += price + priceIncrease;
        }
        UpdateEmotionVisual();
    }

    private IEnumerator MoveToExit()
    {
        while (Vector3.Distance(transform.position, endPoint.position) > 0.1f)
        {
            transform.position = Vector3.Lerp(transform.position, endPoint.position, Time.deltaTime * 2f);
            yield return null;
        }
        
        Destroy(gameObject);
        onExitComplete?.Invoke();
    }

    private void UpdateEmotionVisual()
    {
        switch (emotion)
        {
            case Emotion.Happy:
                image.sprite = happySprite;
                break;
            case Emotion.Love:
                image.sprite = loveSprite;
                break;
            case Emotion.Angry:
                image.sprite = angrySprite;
                break;
        }
    }
}