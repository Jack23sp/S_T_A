using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CustomType;

[CreateAssetMenu(menuName = "uMMORPG Item/Building", order = 999)]
public partial class ScriptableBuilding : ScriptableItem
{

    public bool groupWarehouse;
    [Header("Claim part")]
    public List<CustomItem> itemToClaim = new List<CustomItem>();
    public int coinToClaim;
    public int goldToClaim;
    public ScriptableAbility abilityToClaim;
    public int abilityLevelToClaim;
    [Header("Upgrade part")]
    public List<CustomItem> itemToUpgrade = new List<CustomItem>();
    public int coinToUpgrade;
    public int goldToUpgrade;
    public ScriptableAbility abilityToUpgrade;
    public int abilityLevelToUpgrade;
    [Header("Repair part")]
    public List<CustomItem> itemToRepair = new List<CustomItem>();
    public int coinToRepair;
    public int goldToRepair;
    public ScriptableAbility abilityToRepair;
    public int abilityLevelToRepair;
    [Header("Cut part")]
    public int coinToHalve;
    public int goldToHalve;
    public ScriptableAbility abilityToHalve;
    [Header("Building Experience")]
    public int buildingExperience;

    [Header("Craftable item")]
    public List<ItemInBuilding> craftableItem = new List<ItemInBuilding>();

    public bool isObstacle;
    [Header("Countdown to create/upgrade")]
    public LinearInt countdownToCreate;

    [Header("Prefab to instantiate")]
    public List<BuildingToCreate> buildingList = new List<BuildingToCreate>();

    public bool isBasement;
    public bool isWall;
    public bool isDoor;
    public bool modularAccessory;
    public bool modularForniture;

    public string necessaryTagObject;


    public bool CanUse(Player player, int inventoryIndex)
    {
        if(groupWarehouse)
        {
            return player.InGuild() && player.playerBuilding.building && player.playerBuilding.building == null;
        }
        return player.playerBuilding.building.name == name && player.playerBuilding.actualBuilding.name == name;
    }

    public bool CanUpgrade(Player player, int currencyType)
    {
        foreach(CustomItem customItem in itemToUpgrade)
        {
            if (customItem.items)
            {
                int count = -1;
                count = player.InventoryCount(new Item(customItem.items));
                if (count < customItem.amount)
                {
                    return false;
                }
            }
            else
            {
                Debug.LogError("You probably forget to assign a items in itemToUpgrade of the object : " + name);
            }
        }
        
        if(currencyType == 0)
        {
            if (player.coins < coinToUpgrade) return false;
        }
        
        if(currencyType == 1)
        {
            if (player.gold < goldToUpgrade) return false;
        }

        if (player.generalPart.FindNetworkAbility(player, abilityToUpgrade.name).level < abilityLevelToUpgrade) return false;

        return true;

    }

    public bool CanClaim(Player player, int currencyType)
    {
        foreach (CustomItem customItem in itemToClaim)
        {
            if (customItem.items)
            {
                int count = -1;
                count = player.InventoryCount(new Item(customItem.items));
                if (count < customItem.amount)
                {
                    return false;
                }
            }
            else
            {
                Debug.LogError("You probably forget to assign a items in itemToClaim of the object : " + name);
            }
        }

        if (currencyType == 0)
        {
            if (player.coins < coinToClaim) return false;
        }

        if (currencyType == 1)
        {
            if (player.gold < goldToClaim) return false;
        }

        if (player.generalPart.FindNetworkAbility(player, abilityToClaim.name).level < abilityLevelToClaim) return false;

        return true;

    }

    public bool CanRepair(Player player, int currencyType)
    {
        foreach (CustomItem customItem in itemToRepair)
        {
            if (customItem.items)
            {
                int count = -1;
                count = player.InventoryCount(new Item(customItem.items));
                if (count < customItem.amount)
                {
                    return false;
                }
            }
            else
            {
                Debug.LogError("You probably forget to assign a items in itemToRepair of the object : " + name);
            }
        }

        if (currencyType == 0)
        {
            if (player.coins < coinToRepair) return false;
        }

        if (currencyType == 1)
        {
            if (player.gold < goldToRepair) return false;
        }

        if (player.generalPart.FindNetworkAbility(player, abilityToRepair.name).level < abilityLevelToRepair) return false;

        return true;
    }

    // caching /////////////////////////////////////////////////////////////////
    // we can only use Resources.Load in the main thread. we can't use it when
    // declaring static variables. so we have to use it as soon as 'dict' is
    // accessed for the first time from the main thread.
    // -> we save the hash so the dynamic item part doesn't have to contain and
    //    sync the whole name over the network
    static Dictionary<int, ScriptableBuilding> cache;
    public static Dictionary<int, ScriptableBuilding> dict
    {
        get
        {
            // not loaded yet?
            if (cache == null)
            {
                // get all ScriptableItems in resources
                ScriptableBuilding[] items = Resources.LoadAll<ScriptableBuilding>("");

                // check for duplicates, then add to cache
                List<string> duplicates = items.ToList().FindDuplicates(item => item.name);
                if (duplicates.Count == 0)
                {
                    cache = items.ToDictionary(item => item.name.GetStableHashCode(), item => item);
                }
                else
                {
                    foreach (string duplicate in duplicates)
                        Debug.LogError("Resources folder contains multiple ScriptableAbility with the name " + duplicate + ". If you are using subfolders like 'Warrior/Ring' and 'Archer/Ring', then rename them to 'Warrior/(Warrior)Ring' and 'Archer/(Archer)Ring' instead.");
                }
            }
            return cache;
        }
    }

    // validation //////////////////////////////////////////////////////////////
    void OnValidate()
    {

    }
}