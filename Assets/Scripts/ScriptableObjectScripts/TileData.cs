using UnityEngine;

public enum TileType { Start, Property, Mystery };

[CreateAssetMenu(fileName = "NewTile", menuName = "Objects/BoardGame/Tile")]
public class TileData : ScriptableObject
{
    public string tileName;
    public Sprite tileLogoSprite;
    public TileType tileType;

    public virtual void OnLand(Player player)
    {
        Debug.Log($"{player.name} landed on {tileName}. Type: {tileType}");
    }
}