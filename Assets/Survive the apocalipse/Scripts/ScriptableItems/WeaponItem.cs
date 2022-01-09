using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName="uMMORPG Item/Weapon", order=999)]
public partial class WeaponItem : EquipmentItem
{
    [Header("Weapon")]
    public AmmoItem requiredAmmo; // null if no ammo is required

    [Header("Sounds")]
    public AudioClip shotClip;
    public AudioClip reloadClip;
    public AudioClip bullettDrops;

    public bool stopMusic;

    // tooltip
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        //if (requiredAmmo != null)
        //    tip.Replace("{REQUIREDAMMO}", requiredAmmo.name);
        //if(ammoItems.Count > 0)
        //{
        //    tip.AppendLine("Allowed munition : ");
        //    if (ammoItems.Count >= 0) tip.AppendLine("   * " + ammoItems[0].name);
        //    if (ammoItems.Count >= 1) tip.AppendLine("   * " + ammoItems[1].name);
        //    if (ammoItems.Count >= 2) tip.AppendLine("   * " + ammoItems[2].name);
        //    if (ammoItems.Count >= 3) tip.AppendLine("   * " + ammoItems[3].name);
        //}
        return tip.ToString();
    }
}