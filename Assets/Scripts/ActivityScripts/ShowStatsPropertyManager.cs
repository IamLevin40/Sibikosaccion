using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class ShowStatsPropertyManager : MonoBehaviour
{
    public GameObject propertyStatsPrefab;
    public Transform propertyStatsParent;
    public GameObject statsButtonPrefab;
    public Transform statsButtonParent;

    private GameObject activeDisplay;
    private Player playerRef;

    public void Initialize(Player player)
    {
        playerRef = player;
        ClearUI();

        foreach (Tile tile in player.board.tiles)
        {
            if (tile.runtimePropertyData != null && tile.runtimePropertyData.isBought)
            {
                GameObject statsButton = Instantiate(statsButtonPrefab, statsButtonParent);
                statsButton.transform.position = tile.propertyImage.transform.position;

                statsButton.GetComponent<Button>().onClick.AddListener(() =>
                {
                    ShowPropertyInfo(tile);
                });
            }
        }
    }

    private void ShowPropertyInfo(Tile tile)
    {
        if (activeDisplay != null)
            Destroy(activeDisplay);

        activeDisplay = Instantiate(propertyStatsPrefab, propertyStatsParent);
        activeDisplay.transform.position = tile.propertyImage.transform.position;
        activeDisplay.transform.GetChild(0).GetComponent<Image>().sprite = tile.tileData.tileLogoSprite;
        Text[] texts = activeDisplay.GetComponentsInChildren<Text>();
        texts[0].text = $"P {tile.runtimePropertyData.marketPrice}";
        texts[1].text = $"{tile.runtimePropertyData.taxRate}%";
        texts[2].text = $"P {tile.runtimePropertyData.revenue}";
    }

    public void ClearUI()
    {
        foreach (Transform child in statsButtonParent)
            Destroy(child.gameObject);

        if (activeDisplay != null)
            Destroy(activeDisplay);
    }
}
