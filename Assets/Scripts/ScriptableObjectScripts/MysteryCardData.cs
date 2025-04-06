using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CardType { Corrupt, NonCorrupt }

[CreateAssetMenu(fileName = "NewMysteryCard", menuName = "Objects/BoardGame/MysteryCard")]
public class MysteryCardData : ScriptableObject
{
    public string cardName;
    public string description;
    public Sprite frontImage;
    public Sprite backImage;
    public CardType cardType;

    public void ActivateEffect(Player player, MonoBehaviour caller)
    {
        switch (cardName)
        {
            case "Community Grant":
                ApplyCommunityGrant(player, caller);
                break;

            case "Volunteer Initiative":
                ApplyVolunteerInitiative(player);
                break;

            case "Infrastructure Investment":
                ApplyInfrastructureInvestment(player);
                break;
            
            case "Youth Start-up Aid":
                ApplyYouthStartupAid(player);
                break;
            
            case "Spirit of Bayanihan":
                ApplySpiritOfBayanihan(player);
                break;

            case "Forced Eviction":
                player.AddCorruptValue(2);
                ApplyForcedEviction(player, caller);
                break;
            
            case "Cash Overflow":
                player.AddCorruptValue(10);
                ApplyCashOverflow(player);
                break;
            
            case "Ghost Employees":
                player.AddCorruptValue(6);
                ApplyGhostEmployees(player);
                break;
            
            case "Land Grab":
                player.AddCorruptValue(5);
                ApplyLandGrab(player);
                break;

            case "Personal Expenditures":
                player.AddCorruptValue(4);
                ApplyPersonalExpenditures(player);
                break;

            default:
                Debug.LogWarning("Unrecognized card: " + cardName);
                break;
        }
    }

    private void ApplyCommunityGrant(Player player, MonoBehaviour caller)
    {
        foreach (var tile in player.board.tiles)
        {
            var data = tile.runtimePropertyData;
            if (data != null && data.isBought && data.marketPrice <= data.reasonableMarketPrice)
            {
                float bonus = Random.Range(5, 11) * 5;
                caller.StartCoroutine(player.BudgetCollection(bonus, tile.propertyImage.transform.position));
            }
        }
    }

    private void ApplyVolunteerInitiative(Player player)
    {
        player.hasVolunteerInitiative = true;
        Debug.Log("Volunteer Initiative applied. Extra customer on next property land.");
    }

    private void ApplyInfrastructureInvestment(Player player)
    {
        List<Tile> validTiles = new List<Tile>();

        foreach (var tile in player.board.tiles)
        {
            var data = tile.runtimePropertyData;
            if (data != null && data.isBought && !data.hasPermanentPriceIncrease)
            {
                validTiles.Add(tile);
            }
        }

        if (validTiles.Count > 0)
        {
            Tile selectedTile = validTiles[Random.Range(0, validTiles.Count)];
            var propertyData = selectedTile.runtimePropertyData;
            propertyData.hasPermanentPriceIncrease = true;
            player.visualItemManager.PlayVisualItem(VisualItemType.AscendItemPop, "property_invest", 1f, selectedTile.propertyImage.transform.position);

            Debug.Log($"Infrastructure Investment applied to property {selectedTile.tileData.tileName}");
        }
        else
        {
            Debug.Log("No valid property found for Infrastructure Investment.");
        }
    }

    private void ApplyYouthStartupAid(Player player)
    {
        player.hasYouthStartupAid = true;
    }

    private void ApplySpiritOfBayanihan(Player player)
    {
        player.hasBayanihanSpirit = true;
    }

    private void ApplyForcedEviction(Player player, MonoBehaviour caller)
    {
        var availableProperties = player.board.tiles
            .Where(t => t.runtimePropertyData != null && t.runtimePropertyData.isBought)
            .ToList();
        
        if (availableProperties.Count > 0)
        {
            Tile selectedTile = availableProperties[Random.Range(0, availableProperties.Count)];

            float refundAmount = selectedTile.runtimePropertyData.purchaseCost * 1.5f;
            caller.StartCoroutine(player.BudgetCollection(refundAmount, selectedTile.propertyImage.transform.position));
            player.visualItemManager.PlayVisualItem(VisualItemType.DescendItemPop, "property_eviction", 1.5f, selectedTile.propertyImage.transform.position);

            selectedTile.runtimePropertyData.isBought = false;
            selectedTile.UpdatePropertyVisual();
            Debug.Log($"Forced Eviction applied to property {selectedTile.tileData.tileName}");
        }
        else
        {
            Debug.Log("No properties available for Forced Eviction.");
        }
    }

    private void ApplyCashOverflow(Player player)
    {
        player.GetBudgetFromGovernment(5000);
        player.hasSkipNextTax = true;
        
        foreach (var tile in player.board.tiles)
        {
            var data = tile.runtimePropertyData;
            if (data != null && data.isBought)
            {
                player.visualItemManager.PlayVisualItem(VisualItemType.AscendItemPop, "community_opinion", 2.5f, tile.propertyImage.transform.position);
            }
        }
    }

    private void ApplyGhostEmployees(Player player)
    {
        player.GetBudgetFromGovernment(2500);
        player.hasGhostEmployeeEffect = true;

        foreach (var tile in player.board.tiles)
        {
            var data = tile.runtimePropertyData;
            if (data != null && data.isBought)
            {
                player.visualItemManager.PlayVisualItem(VisualItemType.DescendItemPop, "ghost_employee", 1.5f, tile.propertyImage.transform.position);
            }
        }
    }

    private void ApplyLandGrab(Player player)
    {
        var unownedProperties = player.board.tiles
            .Where(t => t.runtimePropertyData != null && !t.runtimePropertyData.isBought)
            .ToList();

        if (unownedProperties.Count > 0)
        {
            Tile selectedTile = unownedProperties[Random.Range(0, unownedProperties.Count)];

            selectedTile.runtimePropertyData.isCursedByLandGrab = true;
            selectedTile.runtimePropertyData.isBought = true;
            selectedTile.UpdatePropertyVisual();
            player.visualItemManager.PlayVisualItem(VisualItemType.DescendItemPop, "property_grab", 1.5f, selectedTile.propertyImage.transform.position);

            Debug.Log($"Land Grab applied. Took over {selectedTile.tileData.tileName} for free.");
        }
        else
        {
            Debug.Log("No unowned properties available for Land Grab.");
        }
    }

    private void ApplyPersonalExpenditures(Player player)
    {
        player.GetBudgetFromGovernment(1000);
        player.reducedRevenueTurns = 4;

        Debug.Log("Personal Expenditures applied. Gained ₱1000, but 10% less revenue for next 4 rolls.");
    }
}
