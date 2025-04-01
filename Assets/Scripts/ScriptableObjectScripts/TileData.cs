using UnityEngine;

[CreateAssetMenu(fileName = "NewTile", menuName = "Objects/BoardGame/Tile")]
public class TileData : ScriptableObject
{
    public string tileName;
    public Color tileColor;
    public string tileEffect;

    public virtual void OnLand(Player player)
    {
        Debug.Log($"{player.name} landed on {tileName}. Effect: {tileEffect}");
    }
}