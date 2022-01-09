using System.Text;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "uMMORPG Item/Food", order = 999)]
public partial class FoodItem : UsableItem
{
    [Header("Equipment")]
    public int foodToAdd;
    public int waterToAdd;
    public int armorToAdd;
    public int durabilityToAdd;

    // usage
    // -> can we equip this into any slot?
    public override bool CanUse(Player player, int inventoryIndex)
    {
        return true;
    }

    public override void Use(Player player, int inventoryIndex)
    {
        // always call base function too
        base.Use(player, inventoryIndex);

        if (player.inventory[inventoryIndex].item.waterContainer > 0)
        {
            int currentThirsty = player.playerThirsty.maxThirsty - player.playerThirsty.currentThirsty;
            if (currentThirsty <= player.inventory[inventoryIndex].item.waterContainer)
            {
                player.playerThirsty.currentThirsty += currentThirsty;
                ItemSlot currentSlot = player.inventory[inventoryIndex];
                currentSlot.item.waterContainer -= currentThirsty;
                player.inventory[inventoryIndex] = currentSlot;
            }
            else
            {
                player.playerThirsty.currentThirsty += player.inventory[inventoryIndex].item.waterContainer;
                ItemSlot currentSlot = player.inventory[inventoryIndex];
                currentSlot.item.waterContainer -= currentThirsty;
                player.inventory[inventoryIndex] = currentSlot;
            }
        }
        if (foodToAdd > 0 || waterToAdd > 0 || armorToAdd > 0 || durabilityToAdd > 0)
        {
            player.playerHungry.currentHungry += foodToAdd;
            if (player.playerHungry.currentHungry > player.playerHungry.maxHungry)
            {
                player.playerHungry.currentHungry = player.playerHungry.maxHungry;
            }

            player.playerThirsty.currentThirsty += waterToAdd;
            if (player.playerThirsty.currentThirsty > player.playerThirsty.maxThirsty)
            {
                player.playerThirsty.currentThirsty = player.playerThirsty.maxThirsty;
            }

            int addArmor = armorToAdd;
            for (int i = 0; i < player.equipment.Count; i++)
            {
                int index = i;
                if (player.equipment[index].amount > 0)
                {
                    if (player.equipment[index].item.data is EquipmentItem)
                    {
                        if (((EquipmentItem)player.equipment[index].item.data).armor.baseValue > 0)
                        {
                            if (player.equipment[index].item.currentArmor < ((EquipmentItem)player.equipment[index].item.data).armor.Get(player.equipment[index].item.armorLevel))
                            {
                                ItemSlot armorEquip = player.equipment[index];
                                int diffArmor = ((EquipmentItem)player.equipment[index].item.data).armor.Get(player.equipment[index].item.armorLevel) - player.equipment[index].item.currentArmor;
                                if (diffArmor > 0)
                                {
                                    if (diffArmor <= addArmor)
                                    {
                                        armorEquip.item.currentArmor = ((EquipmentItem)player.equipment[index].item.data).armor.Get(player.equipment[index].item.armorLevel);
                                        addArmor -= diffArmor;
                                    }
                                    else
                                    {
                                        armorEquip.item.currentArmor += addArmor;
                                        addArmor = 0;
                                    }
                                    player.equipment[index] = armorEquip;
                                }
                            }
                        }
                    }
                }
            }

            int adddurability = durabilityToAdd;
            for (int i = 0; i < player.equipment.Count; i++)
            {
                int index = i;
                if (player.equipment[index].amount > 0)
                {
                    if (player.equipment[index].item.data.maxDurability.baseValue > 0)
                    {
                        if (player.equipment[index].item.durability < ((EquipmentItem)player.equipment[index].item.data).maxDurability.Get(player.equipment[index].item.durabilityLevel))
                        {
                            ItemSlot armorEquip = player.equipment[index];
                            int diffArmor = player.equipment[index].item.data.maxDurability.Get(player.equipment[index].item.durabilityLevel) - player.equipment[index].item.durability;
                            if (diffArmor > 0)
                            {
                                if (diffArmor <= addArmor)
                                {
                                    armorEquip.item.durability = ((EquipmentItem)player.equipment[index].item.data).maxDurability.Get(player.equipment[index].item.durabilityLevel);
                                    addArmor -= diffArmor;
                                }
                                else
                                {
                                    armorEquip.item.durability += addArmor;
                                    addArmor = 0;
                                }
                                player.equipment[index] = armorEquip;
                            }
                        }
                    }
                }
            }


            ItemSlot slot = player.inventory[inventoryIndex];
            slot.DecreaseAmount(1);
            player.inventory[inventoryIndex] = slot;
        }
    }

    // tooltip
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        //if(foodToAdd > 0)tip.Replace("{FOODTOADD}", "Add : " + foodToAdd.ToString() + " to food \n");
        //if (waterToAdd > 0) tip.Replace("{WATERTOADD}", "Add : " + waterToAdd.ToString() + " to water\n");
        //if(armorToAdd > 0)tip.Replace("{ARMORTOADD}", "Add : " + armorToAdd.ToString() + " to armor\n");
        //if(maxUnsanity.baseValue > 0)tip.Replace("{UNSANITY}", "Has : " + maxUnsanity.baseValue.ToString() + " unsanity\n");
        return tip.ToString();
    }
}
