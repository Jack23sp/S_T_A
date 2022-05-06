using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using CustomType;

public partial class EquipmentItem
{
    // Accuracy
    [Header("Accuracy")]
    public LinearFloat accuracy;
    public int maxAccuracyLevel;
    // Miss
    [Header("Miss")]
    public LinearFloat miss;
    public int maxMissLevel;
    // Armor
    [Header("Armor")]
    public LinearInt armor;
    public int maxArmorLevel;

    // Cover temperature
    [Header("Cover temperature")]
    public float coverTemperature;

    [Header("Bag")]
    public LinearInt additionalSlot;
    public LinearInt protectedSlot;
    public int maxBagLevel;

    [Header("Ammo munition")]
    public LinearInt chargeMunition;
    public int maxChargeLevel;
    public List<AmmoItem> ammoItems = new List<AmmoItem>();

    [Header("Weapon custom")]
    public bool poisonedAttack;
    public bool armorBreak;
    public bool electricWeapon;
    public bool firedWeapon;

    [Header("Wet")]
    public float maxWet;

    [Header("Equipment stuff")]
    public GameObject clothes;

    [Header("Animator to set")]
    public RuntimeAnimatorController animatorToSet;

    [Header("Weapon placeholder")]
    public int indexToSpawnWeapon;

    [Header("Equipment")]
    public int indexHat = -1;
    public int indexAccessory = -1;
    public int indexShirt = -1;
    public int indexPants = -1;
    public int indexShoes = -1;
    public int indexBag = -1;
    public bool ignorePants = false;

}

public partial class WeaponItem
{
    [Header("Monster Grab")]
    public int distamceToGrab;

    [Header("Skill required")]
    public ScriptableSkill requiredSkill;

    public bool isClothes = false;
    public bool canUseWeaponStorage = false;
    public bool canUsePistolStorage = false;
}

public partial class AmmoItem
{
    [Header("Ammo")]
    public bool poisoned;
    public bool armorBreaker;
    public bool fired;


}

public partial class FoodItem
{
    [Header("Unsanity")]
    public LinearInt maxUnsanity;
    public int maxUnsanityLevel;


    [Header("Campfire Item")]
    public int maxAmountCook;
    public FoodItem cookedItem;

}

public partial class ScriptableItem
{
    [Header("Is stealable")]
    public bool isStealable;

    [Header("Is lostable")]
    public bool isLostable;

    [Header("Weight System")]
    public int maxWeightLevel;
    public LinearInt maxWeight;
    public LinearInt possibleBagWeight;

    [Header("Durability")]
    public LinearInt maxDurability;
    public int maxDurabilityLevel;

    [Header("Item mall")]
    public long goldPrice;
    public long coinPrice;

    [Header("Craft")]
    [TextArea(15,15)]
    public string craftDescription;
    public int timeToCraft;

    [Header("Gasoline container")]
    public int maxCarItemInventory = 30;

    [Header("Upgrade/Repair item")]
    public int repairTimer;
    public List<CustomItem> repairItems = new List<CustomItem>();
    public int goldsToRepair;
    public int coinsToRepair;

    public int upgradeTimer;
    public List<CustomItem> upgradeItems = new List<CustomItem>();
    public int goldsToUpgrade;
    public int coinsToUpgrade;

    [Header("Liquid container")]
    public int generalLiquidContainer = 0;

    public bool canUseFurnace = false;
    public bool canUseWarehouse = false;

}

public partial struct Item
{
    public int currentArmor;
    public int currentUnsanity;
    public int alreadyShooted;
    public int totalAlreadyShooted;
    public int radioCurrentBattery;
    public int torchCurrentBattery;
    public int durability;
    public int weight;

    public int accuracyLevel;
    public int missLevel;
    public int armorLevel;
    public int chargeLevel;
    public int batteryLevel;
    public int weightLevel;
    public int durabilityLevel;
    public int unsanityLevel;
    public int bagLevel;

    public int gasolineContainer;
    public int honeyContainer;
    public int waterContainer;

    public int cookCountdown;
    public bool isSummoned;
    public float wet;

}



public partial class Player
{
    //Can the player unequip this item?
    [Server]
    public bool CanUnEquip( Item item)
    {
        //if item has inventorySlots, check that they have enough free slots to unequip + 1 for unequipable item
        if (((EquipmentItem)item.data).additionalSlot.baseValue > 0)
        {
            return InventorySlotsFree() > ((EquipmentItem)item.data).additionalSlot.Get(item.bagLevel);
        }
        return true;
    }
}
