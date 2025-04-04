using UnityEngine;

public enum CardType { Corrupt, NonCorrupt }

[CreateAssetMenu(fileName = "NewMysteryCard", menuName = "Objects/BoardGame/MysteryCard")]
public class MysteryCardData : ScriptableObject
{
    public string cardName;
    public string description;
    public Sprite image;
    public CardType cardType;

    public void ActivateEffect(Player player)
    {
        switch (cardName)
        {
            case "Bribe Collector 1":
                player.AddCorruptValue(2);
                break;

            case "Bribe Collector 2":
                player.AddCorruptValue(4);
                break;

            case "Bribe Collector 3":
                player.AddCorruptValue(10);
                break;

            case "Tax Refund":
                player.budget += 500;
                player.UpdateBudgetUI();
                break;

            case "Marketing Boost":
                player.budget += 250;
                player.UpdateBudgetUI();
                break;

            default:
                Debug.LogWarning("Unrecognized card: " + cardName);
                break;
        }
    }
}
