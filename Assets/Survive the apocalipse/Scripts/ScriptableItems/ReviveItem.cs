using System;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName="uMMORPG Item/ReviveItem", order=999)]
public class ReviveITem : UsableItem
{

    // usage
    public override void Use(Player player, int inventoryIndex)
    {
        if (player.activePet == null) return;
        // always call base function too
        base.Use(player, inventoryIndex);

        player.activePet.health = player.activePet.healthMax;
        player.activePet._state = "IDLE";

        // decrease amount
        ItemSlot slot = player.inventory[inventoryIndex];
        slot.DecreaseAmount(1);
        player.inventory[inventoryIndex] = slot;
    }

    public override void UseBelt(Player player, int inventoryIndex)
    {
        if (player.activePet == null) return;
        // always call base function too
        base.UseBelt(player, inventoryIndex);

        player.activePet.health = player.activePet.healthMax;
        player.activePet._state = "IDLE";

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
