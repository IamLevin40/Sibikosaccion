using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpawnItemCollectionManager : MonoBehaviour
{
    private float bounceDuration = 1f;
    private float moveDuration = 1f;
    
    public void Initialize(Sprite itemSprite, Transform parent, Vector3 spawnPosition, Vector3 endPosition)
    {
        StartCoroutine(AnimateItem(itemSprite, parent, spawnPosition, endPosition));
    }

    private IEnumerator AnimateItem(Sprite itemSprite, Transform parent, Vector3 spawnPosition, Vector3 endPosition)
    {
        GameObject spawnedItem = new GameObject("SpawnedItem");
        spawnedItem.transform.SetParent(parent, false);

        Image image = spawnedItem.AddComponent<Image>();
        image.sprite = itemSprite;

        RectTransform rectTransform = spawnedItem.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(70, 70);
        rectTransform.position = spawnPosition;

        float elapsed = 0f;
        Vector3 bouncePeak = spawnPosition + Vector3.up * 0.25f;
        
        while (elapsed < bounceDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / bounceDuration;
            spawnedItem.transform.position = Vector3.Lerp(spawnPosition, bouncePeak, Mathf.Sin(t * Mathf.PI));
            yield return null;
        }
        
        elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            spawnedItem.transform.position = Vector3.Lerp(spawnPosition, endPosition, t);
            yield return null;
        }
        
        Destroy(spawnedItem);
    }
}
