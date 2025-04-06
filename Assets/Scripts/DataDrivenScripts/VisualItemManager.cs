using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum VisualItemType { AscendItemPop, DescendItemPop }

[System.Serializable]
public class VisualItem
{
    public string itemName;
    public Sprite itemSprite;
}

public class VisualItemManager : MonoBehaviour
{
    [Header("Visual Item Settings")]
    public List<VisualItem> visualItems;
    public Transform visualItemParent;
    public GameObject visualItemPrefab;

    private Dictionary<string, Sprite> visualItemDictionary;

    private void Awake()
    {
        visualItemDictionary = new Dictionary<string, Sprite>();
        foreach (VisualItem visualItem in visualItems)
        {
            if (!visualItemDictionary.ContainsKey(visualItem.itemName))
            {
                visualItemDictionary.Add(visualItem.itemName, visualItem.itemSprite);
            }
        }
    }

    public void PlayVisualItem(VisualItemType visualItemType, string itemName, float duration, Vector3 spawnPosition)
    {
        if (!visualItemDictionary.TryGetValue(itemName, out Sprite itemSprite))
        {
            Debug.LogWarning($"VisualItem '{itemName}' not found!");
            return;
        }

        GameObject spawnedVisualItem = Instantiate(visualItemPrefab, visualItemParent);
        spawnedVisualItem.transform.position = spawnPosition;

        Image imageComponent = spawnedVisualItem.GetComponent<Image>();
        if (imageComponent == null)
        {
            Debug.LogError("VisualItem prefab must have an Image component.");
            Destroy(spawnedVisualItem);
            return;
        }

        imageComponent.sprite = itemSprite;

        switch (visualItemType)
        {
            case VisualItemType.AscendItemPop:
                StartCoroutine(AnimateAscendItem(spawnedVisualItem, imageComponent, duration, spawnPosition));
                break;
            case VisualItemType.DescendItemPop:
                StartCoroutine(AnimateDescendItem(spawnedVisualItem, imageComponent, duration, spawnPosition));
                break;
        }
    }

    private IEnumerator AnimateAscendItem(GameObject visualItemObject, Image imageComponent, float duration, Vector3 startPosition)
    {
        float elapsedTime = 0f;
        Vector3 endPosition = startPosition + new Vector3(0, 0.8f, 0);
        Color originalColor = imageComponent.color;

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            float easedProgress = 1f - Mathf.Pow(1f - progress, 2f);

            visualItemObject.transform.position = Vector3.Lerp(startPosition, endPosition, easedProgress);

            float alpha = 1f;
            if (progress >= 0.8f)
            {
                float fadeProgress = (progress - 0.8f) / 0.2f;
                alpha = Mathf.Lerp(1f, 0f, fadeProgress);
            }
            imageComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(visualItemObject);
    }

    private IEnumerator AnimateDescendItem(GameObject visualItemObject, Image imageComponent, float duration, Vector3 endPosition)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = endPosition + new Vector3(0, 0.8f, 0);
        Color originalColor = imageComponent.color;

        imageComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        visualItemObject.transform.position = startPosition;

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            float easedProgress = 1f - Mathf.Pow(1f - progress, 2f);

            visualItemObject.transform.position = Vector3.Lerp(startPosition, endPosition, easedProgress);

            float alpha = 1f;
            if (progress >= 0.8f)
            {
                float fadeProgress = (progress - 0.8f) / 0.2f;
                alpha = Mathf.Lerp(1f, 0f, fadeProgress);
            }
            imageComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        Destroy(visualItemObject);
    }
}
