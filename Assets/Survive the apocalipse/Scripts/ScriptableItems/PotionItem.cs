using System;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName="uMMORPG Item/Potion", order=999)]
public class PotionItem : UsableItem
{
    [Header("Potion")]
    public int usageHealth;
    public int usageMana;
    public int usageExperience;
    public int usagePetHealth; // to heal pet
    public int percHealth;
    public int percMana;

    // usage
    public override void Use(Player player, int inventoryIndex)
    {
        // always call base function too
        base.Use(player, inventoryIndex);

        // increase health/mana/etc.
        player.health += usageHealth;
        player.mana += usageMana;
        player.health += Convert.ToInt32(((float)player.healthMax/100.0f)* (float)percHealth);
        player.mana += Convert.ToInt32(((float)player.manaMax/100.0f)* (float)percMana);

        player.experience += usageExperience;
        if (player.activePet != null) player.activePet.health += usagePetHealth;

        // decrease amount
        ItemSlot slot = player.inventory[inventoryIndex];
        slot.DecreaseAmount(1);
        player.inventory[inventoryIndex] = slot;
    }

    public override void UseBelt(Player player, int inventoryIndex)
    {
        // always call base function too
        base.UseBelt(player, inventoryIndex);

        // increase health/mana/etc.
        player.health += usageHealth;
        player.mana += usageMana;
        player.health += Convert.ToInt32(((float)player.healthMax / 100.0f) * (float)percHealth);
        player.mana += Convert.ToInt32(((float)player.manaMax / 100.0f) * (float)percMana);

        player.experience += usageExperience;
        if (player.activePet != null) player.activePet.health += usagePetHealth;

        // decrease amount
        ItemSlot slot = player.playerBelt.belt[inventoryIndex];
        slot.DecreaseAmount(1);
        player.playerBelt.belt[inventoryIndex] = slot;
    }


    // tooltip
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        //tip.Replace("{USAGEHEALTH}", "Add " + usageHealth.ToString() + " to health");
        //tip.Replace("{USAGEMANA}", "Add " + usageMana.ToString() + " to stamina");
        //tip.Replace("{USAGEEXPERIENCE}", "The usage of this item give you " + usageExperience.ToString() + " exp");
        //tip.Replace("{USAGEPETHEALTH}", "Add " + usagePetHealth.ToString() + " to pet health");
        //tip.Replace("{PERCMANA}", "Add " + percMana.ToString() + " % to stamina \n");
        //tip.Replace("{PERCHEALTH}", "Add " + percHealth.ToString() + " % to health" );
        return tip.ToString();
    }
}
