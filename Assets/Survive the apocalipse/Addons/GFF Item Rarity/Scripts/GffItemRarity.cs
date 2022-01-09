using System;
using System.Collections.Generic;
using UnityEngine;

public class GffItemRarity : MonoBehaviour
{
    [Header("Settings : Sprites")]
    public bool useRarityTypeA;
    public bool useRarityTypeB;
    public Sprite raritySpriteA;
    public Sprite raritySpriteB;

    [Header("Settings : Names and Colors")]
    public Color ColorNull;

    public List<ItemTypes> ItemRarityList = new List<ItemTypes>() { new ItemTypes { name = "Rarity", color = Color.white } };
    [Serializable]
    public class ItemTypes
    {
        public string name;
        public Color color;
    }

    public Sprite rarityType()
    {
        if (useRarityTypeA) return raritySpriteA;
        else if (useRarityTypeB) return raritySpriteB;
        else return null;
    }

    public Color rarityColor(bool slotEmpty, Item item)
    {
        if (!slotEmpty) return ColorNull;
        else return ItemRarityList[item.data.ItemRarity].color;
    }

    public Color rarityColor(bool slotEmpty, ScriptableItem item)
    {
        if (!slotEmpty) return ColorNull;
        else return ItemRarityList[item.ItemRarity].color;
    }

    //singleton
    public static GffItemRarity singleton;
    public GffItemRarity() { singleton = this; }
}