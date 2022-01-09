using UnityEngine;
using System.Text;

public partial class ScriptableItem
{
    [Header("GFF Item Rarity")]
    public int ItemRarity;
}

public partial struct Item
{
    void ToolTip_itemRarity(StringBuilder tip)
    {
        if (GffItemRarity.singleton != null)
        {
            for (int i = 0; i < GffItemRarity.singleton.ItemRarityList.Count; i++)
            {
                if (i == data.ItemRarity)
                {
                    string color = "<color=#" + ColorUtility.ToHtmlStringRGBA(GffItemRarity.singleton.ItemRarityList[i].color) + ">";
                    tip.Replace("{ITEMRARITY}", "<b>" + color + GffItemRarity.singleton.ItemRarityList[i].name + "</color></b>");
                }
            }
        }
    }
}
