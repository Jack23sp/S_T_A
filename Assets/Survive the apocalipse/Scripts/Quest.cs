// The Quest struct only contains the dynamic quest properties, so that the
// static properties can be read from the scriptable object. The benefits are
// low bandwidth and easy Player database saving (saves always refer to the
// scriptable quest, so we can change that any time).
//
// Quests have to be structs in order to work with SyncLists.
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Mirror;

[Serializable]
public partial struct Quest
{
    // hashcode used to reference the real ScriptableQuest (can't link to data
    // directly because synclist only supports simple types). and syncing a
    // string's hashcode instead of the string takes WAY less bandwidth.
    public int hash;

    public string itaTitle;

    // the progress field can be used by inheriting from ScriptableQuests
    // -> the field can be:
    //    * kill counters for 1 monster
    //    * kill counters for 4 monsters (split into 4 bytes, so 4 x 255 kills)
    //    * simple boolean checks (1/0)
    //    * checklists (by setting the 32 bits to 1/0)
    // -> could use long for 64 bits if needed later, or even multiple fields
    public int progress;

    // a quest is complete after finishing it at the npc and getting rewards
    public bool completed;

    public bool checkEnterPremium;
    public bool checkCreateBuilding;
    public bool checkEquipWeapon;
    public bool checkEquipBag;
    public bool checkCreateGuild;
    public bool checkCreateParty;
    public bool checkMakeGroupAlly;
    public bool checkDrink;
    public bool checkEat;
    public bool checkRun;
    public bool checkSneak;
    public bool checkMakeMarriage;
    public bool checkSendAMessage;
    public bool checkBuyEmoji;
    public bool checkOpenShop;
    public int checkAmountPlayerToKill;
    public int checkWoodToGather;
    public int checkRockToGather;

    public bool checkUseTeleport;
    public bool checkUseInstantResurrect;
    public bool checkMakeATrade;
    public int checkCreateASpawnpoint;
    public bool checkFriendCount;

    public int checkBiohazard;
    public int totalHairZombie;
    public int checkMechanic;
    public int totalPirateZombie;
    public int checkInfected;
    public int totalHatZombie;
    public int checkPolice;
    public int totalCountryZombie;

    // constructors
    public Quest(ScriptableQuest data)
    {
        hash = data.name.GetStableHashCode();
        itaTitle = data.italianTitle;
        progress = 0;
        completed = false;
        checkEnterPremium = false;
        checkCreateBuilding = false;
        checkEquipWeapon = false;
        checkEquipBag = false;
        checkCreateGuild = false;
        checkCreateParty = false;
        checkMakeGroupAlly = false;
        checkDrink = false;
        checkEat = false;
        checkRun = false;
        checkSneak = false;
        checkMakeMarriage = false;
        checkSendAMessage = false;
        checkBuyEmoji = false;
        checkOpenShop = false;
        checkAmountPlayerToKill = 0;
        checkWoodToGather = 0;
        checkRockToGather = 0;

        checkUseTeleport = false;
        checkUseInstantResurrect = false;
        checkMakeATrade = false;
        checkCreateASpawnpoint = 0;

        checkBiohazard = 0;
        checkMechanic = 0;
        checkInfected = 0;
        checkPolice = 0;

        totalHairZombie = 0;
        totalPirateZombie = 0;
        totalHatZombie = 0;
        totalCountryZombie = 0;

        checkFriendCount = false;

        for (int i = 0; i < data.killZombie.Count; i++)
        {
            if(data.killZombie[i].monster.name == "Country Zombie")
            {
                totalCountryZombie = data.killZombie[i].quantity;
            }
            if (data.killZombie[i].monster.name == "Hair Zombie")
            {
                totalHairZombie = data.killZombie[i].quantity;
            }
            if (data.killZombie[i].monster.name == "Hat Zombie")
            {
                totalHatZombie = data.killZombie[i].quantity;
            }
            if (data.killZombie[i].monster.name == "Pirate Zombie")
            {
                totalPirateZombie = data.killZombie[i].quantity;
            }
        }
    }

    // wrappers for easier access
    public ScriptableQuest data
    {
        get
        {
            // show a useful error message if the key can't be found
            // note: ScriptableQuest.OnValidate 'is in resource folder' check
            //       causes Unity SendMessage warnings and false positives.
            //       this solution is a lot better.
            if (!ScriptableQuest.dict.ContainsKey(hash))
                throw new KeyNotFoundException("There is no ScriptableQuest with hash=" + hash + ". Make sure that all ScriptableQuests are in the Resources folder so they are loaded properly.");
            return ScriptableQuest.dict[hash];
        }
    }
    public string name => data.name;
    public int requiredLevel => data.requiredLevel;
    public string predecessor => data.predecessor != null ? data.predecessor.name : "";
    public long rewardGold => data.rewardGold;
    public long rewardExperience => data.rewardExperience;
    public ScriptableItem rewardItem => data.rewardItem;

    // events
    public void OnKilled(Player player, int questIndex, Entity victim) { data.OnKilled(player, questIndex, victim); }
    public void OnLocation(Player player, int questIndex, Collider2D location) { data.OnLocation(player, questIndex, location); }

    // completion
    public bool IsFulfilled(Player player) { return data.IsFulfilled(player, this); }
    public void OnCompleted(Player player) { data.OnCompleted(player, this); }

    public List<QuestAbility> ability => data.ability;
    public List<QuestBoost> Boosts => data.Boosts;
    public bool enterPremium => data.enterPremium;
    public bool createBuilding => data.createBuilding;
    public bool equipWeapon => data.equipWeapon;
    public bool equipBag => data.equipBag;
    public bool createGuild => data.createGuild;
    public bool createParty => data.createParty;
    public bool makeGroupAlly => data.makeGroupAlly;
    public bool drink => data.drink;
    public bool eat => data.eat;
    public bool run => data.run;
    public bool sneak => data.sneak;
    public bool makeMarriage => data.makeMarriage;
    public bool buyEmoji => data.buyEmoji;
    public bool openShop => data.openShop;
    public int amountPlayerToKill => data.amountPlayerToKill;
    public List<ZombieKill> killZombie => data.killZombie;
    public List<UpgradeBuilding> buildingUpgrade => data.buildingUpgrade;
    public List<UpgradeItems> itemUpgrade => data.itemUpgrade;
    public SetSpawnpoint setSpawnpoint => data.setSpawnpoint;

    public ScriptableQuest nextQuest => data.nextQuest;

    // fill in all variables into the tooltip
    // this saves us lots of ugly string concatenation code. we can't do it in
    // ScriptableQuest because some variables can only be replaced here, hence we
    // would end up with some variables not replaced in the string when calling
    // Tooltip() from the data.
    // -> note: each tooltip can have any variables, or none if needed
    public string ToolTip(Player player)
    {
        // we use a StringBuilder so that addons can modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        // note: field0 tooltip part is done in the scriptable quest, because it
        //       might be a number, might be 'Yes'/'No', etc.
        StringBuilder tip = new StringBuilder(data.ToolTip(player, this));
        tip.Replace("{ABILITY}", "{ABILITY}Abilities to Upgrade :</br>");
        tip.Replace("{BOOST}", "{BOOST}Boosts to Upgrade :</br>");
        tip.Replace("{PREMIUM}", "{PREMIUM}Click left upper corner to teleport to premium zone</br>");
        tip.Replace("{BUILDING}", "{BUILDING}Create this building</br>");
        tip.Replace("{WEAPON}", "{WEAPON}Equip this weapon</br>");
        tip.Replace("{BAG}", "{BAG}Equip a bag</br>");
        tip.Replace("{CREATEGUILD}", "{CREATEGUILD}Create a guild</br>");
        tip.Replace("{CREATEPARTY}", "{CREATEPARTY}Create a party</br>");
        tip.Replace("{MAKEGROUPALLY}", "{MAKEGROUPALLY}Create group ally</br>");
        tip.Replace("{DRINK}", "{DRINK}Take a Drink</br>");
        tip.Replace("{EAT}", "{EAT}Take a break with some food</br>");
        tip.Replace("{RUN}", "{RUN}Make some jogging with right down button</br>");
        tip.Replace("{SNEAK}", "{SNEAK}Make sneak with right down button</br>");
        tip.Replace("{MARRIAGE}", "{MARRIAGE}Make marriage with some</br>");
        tip.Replace("{EMOJI}", "{EMOJI}Buy an emoji with down center button</br>");
        tip.Replace("{SHOP}", "{SHOP}Open shop</br>");
        tip.Replace("{PLAYERTOKILL}", "{PLAYERTOKILL}Player to kill : " + amountPlayerToKill + "</br>");
        tip.Replace("{ZOMBIE}", "{ZOMBIE}Zombie to kill : </br>");
        tip.Replace("{UPGRADEBUILDING}", "{UPGRADEBUILDING}Upgrade follow building : </br>");
        tip.Replace("{UPGRADEITEMS}", "{UPGRADEITEMS}Upgrade items to desire level : </br>");
        tip.Replace("{SPAWNPOINT}", "{SPAWNPOINT}Amount of spawnpoint to set : " + setSpawnpoint.numberOfSpawnpointToCreate + "</br>");
        tip.Replace("{STATUS}", IsFulfilled(player) ? "<i>Complete!</i>" : "");

        // addon system hooks
        Utils.InvokeMany(typeof(Quest), this, "ToolTip_", tip);

        return tip.ToString();
    }
}

public class SyncListQuest : SyncList<Quest> {}
