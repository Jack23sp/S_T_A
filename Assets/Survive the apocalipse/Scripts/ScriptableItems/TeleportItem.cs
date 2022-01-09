using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName="uMMORPG Item/Teleport", order=999)]
public class TeleportItem : UsableItem
{
    // usage
    public override void Use(Player player, int inventoryIndex)
    {
        // always call base function too
        base.Use(player, inventoryIndex);

        // increase health/mana/etc.
        //player.health += usageHealth;
        //player.mana += usageMana;
        //player.experience += usageExperience;
        //if (player.activePet != null) player.activePet.health += usagePetHealth;

        if (player.playerTeleport.itemInUse == -1)
            player.playerTeleport.itemInUse = inventoryIndex;
        else
            return;

    }

    public override void UseBelt(Player player, int inventoryIndex)
    {
        // always call base function too
        base.UseBelt(player, inventoryIndex);

        // increase health/mana/etc.
        //player.health += usageHealth;
        //player.mana += usageMana;
        //player.experience += usageExperience;
        //if (player.activePet != null) player.activePet.health += usagePetHealth;

        if (player.playerTeleport.itemInUse == -1)
            player.playerTeleport.itemInUse = inventoryIndex;
        else
            return;

    }

    // tooltip
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        //tip.Replace("{USAGEHEALTH}", usageHealth.ToString());
        //tip.Replace("{USAGEMANA}", usageMana.ToString());
        //tip.Replace("{USAGEEXPERIENCE}", usageExperience.ToString());
        //tip.Replace("{USAGEPETHEALTH}", usagePetHealth.ToString());
        return tip.ToString();
    }
}
