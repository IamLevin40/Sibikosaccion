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
        GameObject item = new GameObject("SpawnedItem");
        item.transform.SetParent(parent, false);

        Image img = item.AddComponent<Image>();
        img.sprite = itemSprite;
        item.transform.position = spawnPosition;

        float elapsed = 0f;
        Vector3 bouncePeak = spawnPosition + Vector3.up * 0.5f;
        
        while (elapsed < bounceDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / bounceDuration;
            item.transform.position = Vector3.Lerp(spawnPosition, bouncePeak, Mathf.Sin(t * Mathf.PI));
            yield return null;
        }
        
        elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            item.transform.position = Vector3.Lerp(spawnPosition, endPosition, t);
            yield return null;
        }
        
        Destroy(item);
    }
}
