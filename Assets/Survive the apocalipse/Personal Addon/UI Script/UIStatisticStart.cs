using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStatisticStart : MonoBehaviour
{
    public static UIStatisticStart singleton;
    public GameObject slotToInstantiate;
    public Transform content;
    private Player player;

    void Start()
    {
        if (!singleton) singleton = this;
    }

    public void SetStatistics()
    {

        UIUtils.BalancePrefabs(slotToInstantiate, 16, content);
        for (int i = 0; i < 16; i++)
        {
            int index = i;
            StatisticSlot slot = content.GetChild(index).GetComponent<StatisticSlot>();
            ManageStatType(slot, index);
            slot.InstantiateOnStart();
        }
    }


    public void ManageStatType(StatisticSlot slot, int index)
    {
        if (index == 0) { slot.armor = true; return; }
        if (index == 1) { slot.health = true; return; }
        //if (index == 2) { slot.damage = true; return; }
        if (index == 2) { slot.adrenaline = true; return; }
        if (index == 3) { slot.defense = true; return; }
        if (index == 4) { slot.accuracy = true; return; }
        if (index == 5) { slot.miss = true; return; }
        if (index == 6) { slot.critPerc = true; return; }
        if (index == 7) { slot.weight = true; return; }
        if (index == 8) { slot.poisoned = true; return; }
        if (index == 9) { slot.hungry = true; return; }
        if (index == 10) { slot.thirsty = true; return; }
        if (index == 11) { slot.blood = true; return; }
        if (index == 12) { slot.marriage = true; return; }
        if (index == 13) { slot.defenseBonusPerc = true; return; }
        if (index == 14) { slot.manaBonusPerc = true; return; }
        if (index == 15) { slot.healthBonusPerc = true; return; }
    }
}
