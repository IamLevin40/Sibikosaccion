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

    public void Initialize(Tile tile, Transform exitPoint, System.Action onComplete)
    {
        propertyTile = tile;
        endPoint = exitPoint;
        onExitComplete = onComplete;
        StartCoroutine(MoveToProperty());
    }

    private IEnumerator MoveToProperty()
    {
        while (Vector3.Distance(transform.position, propertyTile.propertyImage.transform.position) > 0.1f)
        {
            transform.position = Vector3.Lerp(transform.position, propertyTile.propertyImage.transform.position, Time.deltaTime * 2f);
            yield return null;
        }

        image.enabled = false;
        EvaluateProperty();

        yield return new WaitForSeconds(0.5f);
        image.enabled = true;
        StartCoroutine(MoveToExit());
    }

    private void EvaluateProperty()
    {
        if (propertyTile.runtimePropertyData.marketPrice <= propertyTile.runtimePropertyData.reasonableMarketPrice)
        {
            emotion = Emotion.Love;
            propertyTile.runtimePropertyData.revenue += propertyTile.runtimePropertyData.marketPrice;
        }
        else
        {
            emotion = Emotion.Angry;
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