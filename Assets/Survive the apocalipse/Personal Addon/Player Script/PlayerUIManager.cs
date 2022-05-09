using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{
    public Player player;

    public int
        prevArmor, armor, //
        prevMaxArmor, maxArmor, //
        prevHealth, health, //
        prevHealthMax, healthMax, //
        prevMana, mana, //
        prevManaMax, manaMax, //
        prevPoisoned, poisoned,//
        prevHungry, hungry,//
        prevThirsty, thirsty,//
        prevBlood, blood,//
        prevDamage, damage;//
    public float
        prevWeight, weight, //
        prevMaxWeight, maxWeight, //
        prevDefense, defense, //
        prevMiss, miss, //
        prevAccuracy, accuracy,//
        prevBlockChange, blockChange,//
        prevCritChace, critChance,//
        prevSpeed, speed;//
    public string prevPartner, partner; //
    public long
        prevGold, gold,
        prevCoins, coins;


    void Start()
    {
        if (player.isLocalPlayer)
        {
            Invoke(nameof(CheckValueChange), 0.5f);
        }
        else
            Destroy(this);
    }

    void CheckValueChange()
    {
        if (prevArmor != armor ||
           prevMaxArmor != maxArmor ||
           prevHealth != health ||
           prevHealthMax != healthMax ||
           prevMana != mana ||
           prevManaMax != manaMax ||
           prevPoisoned != poisoned ||
           prevHungry != hungry ||
           prevThirsty != thirsty ||
           prevBlood != blood ||
           prevDamage != damage ||
           prevWeight != weight ||
           prevMaxWeight != maxWeight ||
           prevDefense != defense ||
           prevMiss != miss ||
           prevAccuracy != accuracy ||
           prevBlockChange != blockChange ||
           prevCritChace != critChance ||
           prevSpeed != speed ||
           prevPartner != partner||
           prevGold != gold ||
           prevCoins != coins)
        {
            if (UIStats.singleton) UIStats.singleton.SyncCurrency();
            if (UIStatistics.singleton) UIStatistics.singleton.SpawnStats();

            SyncToOld();
        }
        Invoke(nameof(CheckValueChange), 0.5f);
    }

    public void SyncToOld()
    {
        prevArmor = armor;
        prevMaxArmor = maxArmor;
        prevHealth = health;
        prevHealthMax = healthMax;
        prevMana = mana;
        prevManaMax = manaMax;
        prevPoisoned = poisoned;
        prevHungry = hungry;
        prevThirsty = thirsty;
        prevBlood = blood;
        prevDamage = damage;
        prevWeight = weight;
        prevDefense = defense;
        prevMiss = miss;
        prevAccuracy = accuracy;
        prevBlockChange = blockChange;
        prevCritChace = critChance;
        prevSpeed = speed;
        prevPartner = partner;
    }
}
