// The Item struct only contains the dynamic item properties, so that the static
// properties can be read from the scriptable object.
//
// Items have to be structs in order to work with SyncLists.
//
// Use .Equals to compare two items. Comparing the name is NOT enough for cases
// where dynamic stats differ. E.g. two pets with different levels shouldn't be
// merged.
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Mirror;

[Serializable]
public partial struct Item
{
    // hashcode used to reference the real ScriptableItem (can't link to data
    // directly because synclist only supports simple types). and syncing a
    // string's hashcode instead of the string takes WAY less bandwidth.
    public int hash;

    // dynamic stats (cooldowns etc. later)
    public GameObject summoned; // summonable that's currently summoned
    public int summonedHealth; // stored in item while summonable unsummoned
    public int summonedLevel; // stored in item while summonable unsummoned
    public long summonedExperience; // stored in item while summonable unsummoned

    // constructors
    public Item(ScriptableItem data)
    {
        hash = data.name.GetStableHashCode();
        summoned = null;
        summonedHealth = data is SummonableItem ? ((SummonableItem)data).summonPrefab.healthMax : 0;
        summonedLevel = data is SummonableItem ? 1 : 0;
        //summonedExperience = data is SummonableItem ? ((SummonableItem)data).summonPrefab.healthMax : 0;
        summonedExperience = 0;
        accuracyLevel = 1;
        missLevel = 1;
        armorLevel = 1;
        chargeLevel = 1;
        batteryLevel = 1;
        weightLevel = 1;
        durabilityLevel = 1;
        unsanityLevel = 1;
        bagLevel = 1;

        currentArmor = data is EquipmentItem && ((EquipmentItem)data).armor.baseValue > 0 ? ((EquipmentItem)data).armor.Get(armorLevel) : 0;
        alreadyShooted = 0;
        totalAlreadyShooted = 0;
        durability = data is ScriptableItem && data.maxDurability.baseValue > 0 ? ((ScriptableItem)data).maxDurability.Get(durabilityLevel) : 0;
        currentUnsanity = data is FoodItem ? ((FoodItem)data).maxUnsanity.Get(unsanityLevel) : 0;
        currentArmor = data is EquipmentItem ? ((EquipmentItem)data).armor.Get(armorLevel) : 0;
        radioCurrentBattery = data is ScriptableRadio ? ((ScriptableRadio)data).currentBattery.Get(batteryLevel) : 0;
        torchCurrentBattery = data is ScriptableTorch ? ((ScriptableTorch)data).currentBattery.Get(batteryLevel) : 0;
        weight = data is ScriptableItem ? ((ScriptableItem)data).maxWeight.Get(weightLevel) : 0;

        gasolineContainer = 0;
        honeyContainer = 0;
        waterContainer = 0;

        cookCountdown = data is FoodItem ? ((FoodItem)data).maxAmountCook : 0;
        wet = 0;
        isSummoned = false;

    }

    // wrappers for easier access
    public ScriptableItem data
    {
        get
        {
            // show a useful error message if the key can't be found
            // note: ScriptableItem.OnValidate 'is in resource folder' check
            //       causes Unity SendMessage warnings and false positives.
            //       this solution is a lot better.
            if (!ScriptableItem.dict.ContainsKey(hash))
                throw new KeyNotFoundException("There is no ScriptableItem with hash=" + hash + ". Make sure that all ScriptableItems are in the Resources folder so they are loaded properly.");
            return ScriptableItem.dict[hash];
        }
    }
    public string name => data.name;
    public int maxStack => data.maxStack;
    public long buyPrice => data.buyPrice;
    public long sellPrice => data.sellPrice;
    public long goldPrice => data.goldPrice;
    public long coinPrice => data.coinPrice;
    public bool sellable => data.sellable;
    public bool tradable => data.tradable;
    public bool destroyable => data.destroyable;
    public Sprite image => data.image;

    // tooltip
    public string ToolTip()
    {
        StringBuilder tip = new StringBuilder(data.ToolTip());
        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
        {
            tip.AppendLine("<color=green><b>" + data.italianName.ToString() + "</b></color>");
            tip.AppendLine("Quantita' massima : " + data.maxStack.ToString());
            tip.AppendLine("{AMOUNT}" + "\n");
            if (data is PetItem)
            {
                tip.AppendLine("Vita : " + summonedHealth.ToString());
                tip.AppendLine("Livello : " + summonedLevel.ToString());
                tip.AppendLine("Esperienza : " + summonedExperience.ToString() + "\n");
            }

            if (data is EquipmentItem)
            {
                if (((EquipmentItem)data).maxAccuracyLevel > 0)
                {
                    tip.AppendLine("Livello di accuratezza : " + "" + accuracyLevel.ToString() + " / " + ((WeaponItem)data).maxAccuracyLevel + "\n");
                }

                if (((EquipmentItem)data).maxMissLevel > 0)
                {
                    tip.AppendLine("Livello di evasione : " + "" + missLevel.ToString() + " / " + ((WeaponItem)data).maxMissLevel + "\n");
                }

                if (((EquipmentItem)data).maxChargeLevel > 0)
                {
                    tip.AppendLine("Livello di carica munizioni : " + "" + chargeLevel.ToString() + " / " + ((WeaponItem)data).maxChargeLevel);
                    tip.AppendLine("Munizioni massime : " + ((EquipmentItem)data).chargeMunition.Get(chargeLevel) + "\n");
                }
                else if (((EquipmentItem)data).maxChargeLevel <= 0)
                {
                    if (((EquipmentItem)data).chargeMunition.baseValue > 0)
                    {
                        tip.AppendLine("Munizioni massime : " + ((EquipmentItem)data).chargeMunition.Get(chargeLevel) + "\n");
                    }
                }

                if (((EquipmentItem)data).maxArmorLevel > 0)
                {
                    tip.AppendLine("Livello armatura : " + armorLevel.ToString() + " / " + ((EquipmentItem)data).maxArmorLevel);
                    tip.AppendLine("Aggiunge  " + ((EquipmentItem)data).armor.Get(armorLevel) + " ad armatura ");
                    tip.AppendLine("Armatura attuale : " + currentArmor + "\n");
                }
                else if (((EquipmentItem)data).maxArmorLevel <= 0)
                {
                    if (((EquipmentItem)data).armor.baseValue > 0)
                    {
                        tip.AppendLine("Armatura attuale : " + currentArmor + " / " + ((EquipmentItem)data).armor.Get(armorLevel) + "\n");
                    }
                }

                if (((EquipmentItem)data).maxBagLevel > 0)
                {
                    tip.AppendLine("Livello della borsa : " + bagLevel + " / " + ((EquipmentItem)data).maxBagLevel);
                    tip.AppendLine("Slot addizionali correnti : " + ((EquipmentItem)data).additionalSlot.Get(bagLevel));
                    tip.AppendLine("Slot protetti : " + ((EquipmentItem)data).protectedSlot.Get(bagLevel));
                    tip.AppendLine("Puo' trasportare un peso di : " + ((EquipmentItem)data).possibleBagWeight.Get(bagLevel) + "\n");
                }
                else if (((EquipmentItem)data).maxBagLevel <= 0)
                {
                    if (((EquipmentItem)data).additionalSlot.baseValue > 0)
                    {
                        tip.AppendLine("Slot addizionali correnti : " + ((EquipmentItem)data).additionalSlot.baseValue);
                    }
                    if (((EquipmentItem)data).protectedSlot.baseValue > 0)
                    {
                        tip.AppendLine("Slot protetti : " + ((EquipmentItem)data).protectedSlot.baseValue);
                    }
                    if (((EquipmentItem)data).possibleBagWeight.baseValue > 0)
                    {
                        tip.AppendLine("Puo' trasportare un peso di : " + ((EquipmentItem)data).possibleBagWeight.Get(bagLevel));
                    }
                    tip.AppendLine();
                }
            }

            if (data is PotionItem)
            {
                if (((PotionItem)data).usageHealth > 0) tip.AppendLine("Agggiunge " + ((PotionItem)data).usageHealth.ToString() + " alla vita");
                if (((PotionItem)data).usageMana > 0) tip.AppendLine("Agggiunge " + ((PotionItem)data).usageMana.ToString() + " alla stamina");
                if (((PotionItem)data).usagePetHealth > 0) tip.AppendLine("Agggiunge " + ((PotionItem)data).usagePetHealth.ToString() + " alla vita del pet");
                if (((PotionItem)data).percMana > 0) tip.AppendLine("Agggiunge " + ((PotionItem)data).percMana.ToString() + " % alla stamina");
                if (((PotionItem)data).percHealth > 0) tip.AppendLine("Agggiunge " + ((PotionItem)data).percHealth.ToString() + " % alla health");
                if (((PotionItem)data).usageExperience > 0) tip.AppendLine("L'uso di questo oggetto di dara' " + ((PotionItem)data).usageExperience.ToString() + " esperienza");
                tip.AppendLine();
            }

            if (data is WeaponItem)
            {
                if (((WeaponItem)data).ammoItems.Count > 0)
                {
                    tip.AppendLine();
                    tip.AppendLine("Munizioni concesse : ");
                    if (((WeaponItem)data).ammoItems.Count >= 0) tip.AppendLine("   * " + ((WeaponItem)data).ammoItems[0].italianName);
                    if (((WeaponItem)data).ammoItems.Count >= 1) tip.AppendLine("   * " + ((WeaponItem)data).ammoItems[1].italianName);
                    if (((WeaponItem)data).ammoItems.Count >= 2) tip.AppendLine("   * " + ((WeaponItem)data).ammoItems[2].italianName);
                    if (((WeaponItem)data).ammoItems.Count >= 3) tip.AppendLine("   * " + ((WeaponItem)data).ammoItems[3].italianName);
                }
            }

            if (data is ScriptableTorch)
            {
                if (((ScriptableTorch)data).currentBattery.baseValue > 0)
                {
                    tip.AppendLine("Livello batteria attuale : " + torchCurrentBattery + " / " + ((ScriptableTorch)data).currentBattery.Get(batteryLevel) + "\n");
                }
            }

            if (data is ScriptableRadio)
            {
                if (((ScriptableRadio)data).currentBattery.baseValue > 0)
                {
                    tip.AppendLine("Livello batteria attuale : " + torchCurrentBattery + " / " + ((ScriptableRadio)data).currentBattery.Get(batteryLevel) + "\n");
                }
            }

            if (data.maxWeightLevel > 0)
            {
                tip.AppendLine("Livello del peso : " + weightLevel + " / " + data.maxWeightLevel.ToString());
                tip.AppendLine("Peso attuale : " + weight + "\n");
            }
            else
            {
                if (data.maxWeight.baseValue > 0)
                    tip.AppendLine("Peso attuale : " + weight + "\n");
            }

            if (data is FoodItem)
            {
                if (((FoodItem)data).maxUnsanityLevel > 0)
                {
                    tip.AppendLine("Livello di insanita' : " + unsanityLevel + " / " + ((FoodItem)data).maxUnsanityLevel.ToString());
                    tip.Append("Attuale insanita' : " + currentUnsanity + " / " + ((FoodItem)data).maxUnsanity.Get(unsanityLevel) + "\n");
                }
                else
                {
                    tip.AppendLine("Attuale insanita' : " + currentUnsanity + " / " + ((FoodItem)data).maxUnsanity.Get(unsanityLevel) + "\n");
                }

                if (((FoodItem)data).waterToAdd > 0)
                {
                    tip.AppendLine("Agginge : " + ((FoodItem)data).waterToAdd.ToString() + " acqua" + "\n");
                }

                if (((FoodItem)data).foodToAdd > 0)
                {
                    tip.AppendLine("Aggiunge : " + ((FoodItem)data).foodToAdd.ToString() + " cibo" + "\n");
                }
            }

            if (data.maxDurabilityLevel > 0)
            {
                tip.AppendLine("Livello di durabilita' : " + durabilityLevel + " / " + data.maxDurabilityLevel);
                tip.AppendLine("Durabilita' attuale : " + durability + " / " + data.maxDurability.Get(durabilityLevel) + "\n");
            }
            else
            {
                if (data.maxDurability.baseValue > 0)
                {
                    tip.AppendLine("Durabilita' attuale : " + durability + " / " + data.maxDurability.Get(durabilityLevel) + "\n");
                }
            }

            if (data is ScriptablePlant)
            {
                if (((ScriptablePlant)data).waterToAdd > 0)
                    if (((ScriptablePlant)data)) tip.AppendLine("Aggiunge : " + ((ScriptablePlant)data).waterToAdd.ToString() + " acqua");

                if (((ScriptablePlant)data).foodToAdd > 0)
                    if (((ScriptablePlant)data)) tip.AppendLine("Aggiunge : " + ((ScriptablePlant)data).foodToAdd.ToString() + " cibo" + "\n");

                if (((ScriptablePlant)data).GrowSeason != String.Empty)
                    if (((ScriptablePlant)data)) tip.AppendLine("Stagione di crescita : " + ((ScriptablePlant)data).GrowSeason.ToString() + "\n");


                tip.AppendLine();
            }

            if (data is ScriptableItem)
            {
                if (gasolineContainer > 0)
                    tip.AppendLine("Carburante nel contenitore : " + gasolineContainer.ToString() + " / " + data.generalLiquidContainer + "\n");
                if (honeyContainer > 0)
                    tip.AppendLine("Miele nel contenitore : " + honeyContainer.ToString() + " / " + data.generalLiquidContainer + "\n");
                if (waterContainer > 0)
                    tip.AppendLine("Acqua nel contenitore : " + waterContainer.ToString() + " / " + data.generalLiquidContainer + "\n");
            }
        }
        else
        {
            Debug.Log("Item English");

            tip.AppendLine("<color=green><b>" + data.name.ToString() + "</b></color>");
            tip.AppendLine("Max stack : " + data.maxStack.ToString());
            tip.AppendLine("{AMOUNT}" + "\n");
            if (data is PetItem)
            {
                tip.AppendLine("Health : " + summonedHealth.ToString());
                tip.AppendLine("Level : " + summonedLevel.ToString());
                tip.AppendLine("Experience : " + summonedExperience.ToString() + "\n");
            }

            if (data is EquipmentItem)
            {
                if (((EquipmentItem)data).maxAccuracyLevel > 0)
                {
                    tip.AppendLine("Accuracy level : " + "" + accuracyLevel.ToString() + " / " + ((WeaponItem)data).maxAccuracyLevel + "\n");
                }

                if (((EquipmentItem)data).maxMissLevel > 0)
                {
                    tip.AppendLine("Dodge level : " + "" + missLevel.ToString() + " / " + ((WeaponItem)data).maxMissLevel + "\n");
                }

                if (((EquipmentItem)data).maxChargeLevel > 0)
                {
                    tip.AppendLine("Charge level : " + "" + chargeLevel.ToString() + " / " + ((WeaponItem)data).maxChargeLevel);
                    tip.AppendLine("Max munition in charge : " + ((EquipmentItem)data).chargeMunition.Get(chargeLevel) + "\n");
                }
                else if (((EquipmentItem)data).maxChargeLevel <= 0)
                {
                    if (((EquipmentItem)data).chargeMunition.baseValue > 0)
                    {
                        tip.AppendLine("Max munition in charge : " + ((EquipmentItem)data).chargeMunition.Get(chargeLevel) + "\n");
                    }
                }

                if (((EquipmentItem)data).maxArmorLevel > 0)
                {
                    tip.AppendLine("Armor level : " + armorLevel.ToString() + " / " + ((EquipmentItem)data).maxArmorLevel);
                    tip.AppendLine("Add  " + ((EquipmentItem)data).armor.Get(armorLevel) + " armor ");
                    tip.AppendLine("Current armor : " + currentArmor + "\n");
                }
                else if (((EquipmentItem)data).maxArmorLevel <= 0)
                {
                    if (((EquipmentItem)data).armor.baseValue > 0)
                    {
                        tip.AppendLine("Current armor : " + currentArmor + " / " + ((EquipmentItem)data).armor.Get(armorLevel) + "\n");
                    }
                }

                if (((EquipmentItem)data).maxBagLevel > 0)
                {
                    tip.AppendLine("Bag Level : " + bagLevel + " / " + ((EquipmentItem)data).maxBagLevel);
                    tip.AppendLine("Current additional slot : " + ((EquipmentItem)data).additionalSlot.Get(bagLevel));
                    tip.AppendLine("Current Protect slot : " + ((EquipmentItem)data).protectedSlot.Get(bagLevel));
                    tip.AppendLine("Can transport a weight of : " + ((EquipmentItem)data).possibleBagWeight.Get(bagLevel) + "\n");
                }
                else if (((EquipmentItem)data).maxBagLevel <= 0)
                {
                    if (((EquipmentItem)data).additionalSlot.baseValue > 0)
                    {
                        tip.AppendLine("Current additional slot : " + ((EquipmentItem)data).additionalSlot.baseValue);
                    }
                    if (((EquipmentItem)data).protectedSlot.baseValue > 0)
                    {
                        tip.AppendLine("Current Protect slot : " + ((EquipmentItem)data).protectedSlot.baseValue);
                    }
                    if (((EquipmentItem)data).possibleBagWeight.baseValue > 0)
                    {
                        tip.AppendLine("Can transport a weight of : " + ((EquipmentItem)data).possibleBagWeight.Get(bagLevel));
                    }
                    tip.AppendLine();
                }
            }

            if (data is PotionItem)
            {
                if (((PotionItem)data).usageHealth > 0) tip.AppendLine("Add " + ((PotionItem)data).usageHealth.ToString() + " to health");
                if (((PotionItem)data).usageMana > 0) tip.AppendLine("Add " + ((PotionItem)data).usageMana.ToString() + " to stamina");
                if (((PotionItem)data).usagePetHealth > 0) tip.AppendLine("Add " + ((PotionItem)data).usagePetHealth.ToString() + " to pet health");
                if (((PotionItem)data).percMana > 0) tip.AppendLine("Add " + ((PotionItem)data).percMana.ToString() + " % to stamina");
                if (((PotionItem)data).percHealth > 0) tip.AppendLine("Add " + ((PotionItem)data).percHealth.ToString() + " % to health");
                if (((PotionItem)data).usageExperience > 0) tip.AppendLine("The usage of this item give you " + ((PotionItem)data).usageExperience.ToString() + " exp");
                tip.AppendLine();
            }

            if (data is WeaponItem)
            {
                if (((WeaponItem)data).ammoItems.Count > 0)
                {
                    tip.AppendLine();
                    tip.AppendLine("Allowed munition : ");
                    if (((WeaponItem)data).ammoItems.Count >= 0) tip.AppendLine("   * " + ((WeaponItem)data).ammoItems[0].name);
                    if (((WeaponItem)data).ammoItems.Count >= 1) tip.AppendLine("   * " + ((WeaponItem)data).ammoItems[1].name);
                    if (((WeaponItem)data).ammoItems.Count >= 2) tip.AppendLine("   * " + ((WeaponItem)data).ammoItems[2].name);
                    if (((WeaponItem)data).ammoItems.Count >= 3) tip.AppendLine("   * " + ((WeaponItem)data).ammoItems[3].name);
                }
            }

            if (data is ScriptableTorch)
            {
                if (((ScriptableTorch)data).currentBattery.baseValue > 0)
                {
                    tip.AppendLine("Current torch battery : " + torchCurrentBattery + " / " + ((ScriptableTorch)data).currentBattery.Get(batteryLevel) + "\n");
                }
            }

            if (data is ScriptableRadio)
            {
                if (((ScriptableRadio)data).currentBattery.baseValue > 0)
                {
                    tip.AppendLine("Current radio battery : " + torchCurrentBattery + " / " + ((ScriptableRadio)data).currentBattery.Get(batteryLevel) + "\n");
                }
            }

            if (data.maxWeightLevel > 0)
            {
                tip.AppendLine("Weight level : " + weightLevel + " / " + data.maxWeightLevel.ToString());
                tip.AppendLine("Current weight : " + weight + "\n");
            }
            else
            {
                if (data.maxWeight.baseValue > 0)
                    tip.AppendLine("Current weight : " + weight + "\n");
            }

            if (data is FoodItem)
            {
                if (((FoodItem)data).maxUnsanityLevel > 0)
                {
                    tip.AppendLine("Unsanity level : " + unsanityLevel + " / " + ((FoodItem)data).maxUnsanityLevel.ToString());
                    tip.Append("Current unsanity : " + currentUnsanity + " / " + ((FoodItem)data).maxUnsanity.Get(unsanityLevel) + "\n");
                }
                else
                {
                    tip.AppendLine("Current unsanity : " + currentUnsanity + " / " + ((FoodItem)data).maxUnsanity.Get(unsanityLevel) + "\n");
                }

                if (((FoodItem)data).waterToAdd > 0)
                {
                    tip.AppendLine("Add : " + ((FoodItem)data).waterToAdd.ToString() + " water" + "\n");
                }

                if (((FoodItem)data).foodToAdd > 0)
                {
                    tip.AppendLine("Add : " + ((FoodItem)data).foodToAdd.ToString() + " food" + "\n");
                }
            }

            if (data.maxDurabilityLevel > 0)
            {
                tip.AppendLine("Durability level : " + durabilityLevel + " / " + data.maxDurabilityLevel);
                tip.AppendLine("Actual durability : " + durability + " / " + data.maxDurability.Get(durabilityLevel) + "\n");
            }
            else
            {
                if (data.maxDurability.baseValue > 0)
                {
                    tip.AppendLine("Actual durability : " + durability + " / " + data.maxDurability.Get(durabilityLevel) + "\n");
                }
            }

            if (data is ScriptablePlant)
            {
                if (((ScriptablePlant)data).waterToAdd > 0)
                    if (((ScriptablePlant)data)) tip.AppendLine("Add : " + ((ScriptablePlant)data).waterToAdd.ToString() + " water");

                if (((ScriptablePlant)data).foodToAdd > 0)
                    if (((ScriptablePlant)data)) tip.AppendLine("Add : " + ((ScriptablePlant)data).foodToAdd.ToString() + " food" + "\n");

                if (((ScriptablePlant)data).GrowSeason != String.Empty)
                    if (((ScriptablePlant)data)) tip.AppendLine("Grow season : " + ((ScriptablePlant)data).GrowSeason.ToString() + "\n");


                tip.AppendLine();
            }

            if (data is ScriptableItem)
            {
                if (gasolineContainer > 0)
                    tip.AppendLine("Gasoline in container : " + gasolineContainer.ToString() + " / " + data.generalLiquidContainer + "\n");
                if (honeyContainer > 0)
                    tip.AppendLine("Honey in container : " + honeyContainer.ToString() + " / " + data.generalLiquidContainer + "\n");
                if (waterContainer > 0)
                    tip.AppendLine("Water in container : " + waterContainer.ToString() + " / " + data.generalLiquidContainer + "\n");
            }
        }
        // addon system hooks
        Utils.InvokeMany(typeof(Item), this, "ToolTip_", tip);

        return tip.ToString();
    }
}
