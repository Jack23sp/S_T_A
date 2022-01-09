using CustomType;
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CustomType
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;
    using Mirror;

    #region Custom SyncListStruct and SynclistClass


    [System.Serializable]
    public partial struct Product
    {
        public string id;
        public string name;
        public Sprite itemImage;
    }

    [Serializable]
    public partial struct CategorySubcategory
    {
        public string category;
        public string subcategory;
    }

    [Serializable]
    public partial struct GoldToReturn
    {
        public long goldToReturn;
        public string userToReturn;
    }

    [Serializable]
    public partial struct AuctionItem
    {
        public int buildingID;
        public int id;
        public string sellerName;
        public ItemSlot itemSlot;
        public bool upgrade;
        public long actualBid;
        public long buyNowBid;
        public string Category;
        public long subCategory;
        public int remainingEstimateTime;
        public int startBidPrice;

        public bool alreadyBidded;

        public GoldToReturn[] userThatBidThisObject;
    }

    [Serializable]
    public partial struct PetExp
    {
        public int timer;
        public int remainingTimer;
        public string owner;
        public ItemSlot petItem;
        public long experienceToAdd;
        public int selectedFood;
        public int index;
        public int level;
        public string timeBegin;
        public string timeEnd;
        public string timeEndServer;
    }

    [Serializable]
    public partial struct ButtonClick
    {
        public Button buttonToClick;
        public AudioClip audioClip;
    }

    [Serializable]
    public partial struct AudioCategory
    {
        public string category;
        public AudioClip AudioForThisCategory;
    }


    [Serializable]
    public partial struct PlayerRank
    {
        public int points;
        public GameObject rankImage;
    }

    [Serializable]
    public partial struct CultivableFood
    {
        public string plantName;
        public float dimension;
        public string season;
        public bool alreadyGrown;
        public float grownQuantityX;
        public float grownQuantityY;
        public int seeds;
        public int plantAmount;
        public int timebeforeTakeMultipleSeeds;
        public bool releaseSeeds;
    }

    [Serializable]
    public partial struct CountdownTimer
    {
        public ScriptableBuilding building;
        public LinearInt countdownEachLevel;
    }

    [Serializable]
    public partial struct AnimalImage
    {
        public ScriptableAnimal currentAnimal;
        public AnimalFatherImage fatherImage;
        public AnimalChildImage childImage;
    }

    [Serializable]
    public partial struct AnimalFatherImage
    {
        public GameObject leftImage;
        public GameObject rightImage;
    }

    [Serializable]
    public partial struct AnimalChildImage
    {
        public GameObject leftImage;
        public GameObject rightImage;
    }


    [Serializable]
    public partial struct BeeContainer
    {
        public int totalBee;
        public int totalHoney;
    }

    [Serializable]
    public partial struct UpgradeRepairItem
    {
        public ItemSlot item;
        public int itemIndex;
        public string playerName;
        public string type;
        public int index;
        public int remainingTime;
        public int totalTime;
        public string timeBegin;
        public string timeBeginServer;
        public string timeEnd;
        public string timeEndServer;
        public string operationType;
    }

    [Serializable]
    public partial struct UpgradeItem
    {
        public int inventoryIndex;
        public int actualLevel;
        public int maxLevel;
        public string upgradeType;
    }


    [Serializable]
    public partial struct LeaderboardItem
    {
        public int personalPoint;
        public int groupPoint;
        public int allyPoint;
        public ScriptableItem items;
        public int amount;
    }

    [Serializable]
    public partial class ResourceImage
    {
        public string resourceType;
        public Sprite spriteImage;
    }

    [Serializable]
    public partial class ResourceItem
    {
        public ScriptableItem resource;
        public List<ScriptableItem> allowedWeapons = new List<ScriptableItem>();
        public ScriptableItem normalRewards;
        public ScriptableItem specialRewards;
        public ScriptableItem superSpecialRewards;
    }

    [Serializable]
    public partial class CarItem
    {
        public ScriptableItem car;
        public List<ScriptableItem> carItems = new List<ScriptableItem>();
    }

    [Serializable]
    public partial struct ItemInBuilding
    {
        public IngredientBuilding itemToCraft;
        public int buildingLevel;
        public List<IngredientBuilding> craftablengredient;
    }

    [Serializable]
    public partial struct BuildingItem
    {
        public ScriptableItem specificBuilding;
        public List<ItemInBuilding> buildingItem;
    }


    [Serializable]
    public partial struct IngredientBuilding
    {
        public ScriptableItem item;
        public int amount;
    }

    [Serializable]
    public partial struct CraftItem
    {
        public int index;
        public string itemName;
        public int amount;
        public int remainingTime;
        public int totalTime;
        public string owner;
        public string guildName;
        public string timeBegin;
        public string timeEnd;
        public string timeEndServer;
    }

    [Serializable]
    public partial struct FriendToRemove
    {
        public string mainCharacter;
        public string friendToRemove;
    }


    [Serializable]
    public partial struct BuildingSpawn
    {
        public ScriptableBuilding building;
        public GameObject uiToSpawn;
    }


    [Serializable]
    public partial struct SimpleInstantiate
    {
        public Transform placeToSpawn;
        public Entity entity;
        public GameObject objectToSpawn;
    }


    [Serializable]
    public partial struct NpcTeleporter
    {
        public Npc npc;
        public Transform teleportPoint;
    }

    [Serializable]
    public partial struct BuildingToCreate
    {
        public string buildingName;
        public GameObject buildingObject;
    }

    [Serializable]
    public partial struct CustomItem
    {
        public ScriptableItem items;
        public int amount;
    }

    [Serializable]
    public class Rewards
    {
        public string name;
        public ScriptableItemAndAmount[] reward;
        public long gold;
        public int coins;
    }



    [Serializable]
    public partial struct Ability
    {
        public string name;
        public int level;
        public int maxLevel;
        public int baseValue;

        public Ability (string Name , int Level , int MaxLevel , int BaseValue)
        {
            name = Name;
            level = Level;
            maxLevel = MaxLevel;
            baseValue = BaseValue;
        }
    }

    [Serializable]
    public partial struct Boost
    {
        public string name;

        public string velocityTimer;
        public string velocityTimerServer;
        public float velocityPerc;

        public string accuracyTimer;
        public string accuracyTimerServer;
        public float accuracyPerc;

        public string missTimer;
        public string missTimerServer;
        public float missPerc;

        public string hiddenIslandTimer;
        public string hiddenIslandTimerServer;

        public string doubleEXP;
        public string doubleEXPServer;
        //public int petDoubleEXP;
        //public int partyDoubleEXP;
        //public int guildDoubleEXP;

        public string doubleGold;
        public string doubleGoldServer;
        //public int partyDoubleGold;
        //public int guildDoubleGold;

        public string doubleLeaderPoints;
        public string doubleLeaderPointsServer;

        public string doubleDamageToMonster;
        public string doubleDamageToMonsterServer;
        public string doubleDamageToPlayer;
        public string doubleDamageToPlayerServer;
        public string doubleDamageToBuilding;
        public string doubleDamageToBuildingServer;


        public Boost(string Name, DateTime velocity, float VelocityPerc, DateTime accuracy, float AccuracyPerc, DateTime strength, float StrenghtPerc, int repairBuilding, int claimBuilding, int seeHiddenTrap, int craft, DateTime hiddenIsland, DateTime doubleExp, int petDoubleExp, int partyDoubleExp, int guildDoubleExp,
            DateTime DoubleGold, int PartyDoubleGold, int GuildDoubleGold, DateTime DoubleLeaderpoints, DateTime doubleDamageMonster, DateTime doubleDamagePlayer, DateTime doubleDamageBuilding)
        {
            name = Name;

            velocityTimer = velocity.ToString();
            velocityTimerServer = string.Empty;
            velocityPerc = VelocityPerc;

            accuracyTimer = accuracy.ToString();
            accuracyTimerServer = string.Empty;
            accuracyPerc = AccuracyPerc;

            missTimer = strength.ToString();
            missTimerServer = string.Empty;
            missPerc = StrenghtPerc;

            hiddenIslandTimer = hiddenIsland.ToString();
            hiddenIslandTimerServer = string.Empty;

            doubleEXP = doubleExp.ToString();
            doubleEXPServer = string.Empty;
            //petDoubleEXP = petDoubleExp;
            //partyDoubleEXP = partyDoubleExp;
            //guildDoubleEXP = guildDoubleExp;

            doubleGold = DoubleGold.ToString();
            doubleGoldServer = null;
            //partyDoubleGold = PartyDoubleGold;
            //guildDoubleGold = GuildDoubleGold;

            doubleLeaderPoints = DoubleLeaderpoints.ToString();
            doubleLeaderPointsServer = string.Empty;

            doubleDamageToMonster = doubleDamageMonster.ToString();
            doubleDamageToMonsterServer = string.Empty;
            doubleDamageToPlayer = doubleDamagePlayer.ToString();
            doubleDamageToPlayerServer = string.Empty;
            doubleDamageToBuilding = doubleDamageBuilding.ToString();
            doubleDamageToBuildingServer = string.Empty;

        }

        public string GetDescription()
        {
            string description = string.Empty;

            description += name + "\n";

            TimeSpan difference = DateTime.Parse(velocityTimer.ToString()) - System.DateTime.Now;
            description += "Velocity : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds)) + " ( + " + velocityPerc + " %) \n";

            difference = DateTime.Parse(accuracyTimer.ToString()) - System.DateTime.Now;
            description += "Accuracy : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds)) + " ( + " + accuracyPerc + " %) \n";

            difference = DateTime.Parse(missTimer.ToString()) - System.DateTime.Now;
            description += "Miss : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds)) + " ( + " + missPerc + " %) \n";

            difference = DateTime.Parse(hiddenIslandTimer.ToString()) - System.DateTime.Now;
            description += "Premium zone : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds)) + "\n";

            difference = DateTime.Parse(doubleEXP.ToString()) - System.DateTime.Now;
            description += "Double exp. : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds)) + "\n";

            //description += "Pet double exp. : " + GeneralManager.singleton.ConvertToTimer(petDoubleEXP) + "\n";
            //description += "Party double exp. : " + GeneralManager.singleton.ConvertToTimer(partyDoubleEXP) + "\n";
            //description += "Guild double exp. : " + GeneralManager.singleton.ConvertToTimer(guildDoubleEXP) + "\n";
            difference = DateTime.Parse(doubleGold.ToString()) - System.DateTime.Now;
            description += "Double gold : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds)) + "\n";

            //description += "Party double gold : " + GeneralManager.singleton.ConvertToTimer(partyDoubleGold) + "\n";
            //description += "Guild double gold : " + GeneralManager.singleton.ConvertToTimer(guildDoubleGold) + "\n";
            difference = DateTime.Parse(doubleLeaderPoints.ToString()) - System.DateTime.Now;
            description += "Double points : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds)) + "\n";

            difference = DateTime.Parse(doubleDamageToMonster.ToString()) - System.DateTime.Now;
            description += "Double damage to monster : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds)) + "\n";

            difference = DateTime.Parse(doubleDamageToPlayer.ToString()) - System.DateTime.Now;
            description += "Double damage to player : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds)) + "\n";

            difference = DateTime.Parse(doubleDamageToBuilding.ToString()) -  System.DateTime.Now;
            description += "Double damage to building : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds)) + "\n";

            return description;
        }
    }

        [Serializable]
    public partial struct Spawnpoint
    {
        public string name;
        public float spawnPositionx;
        public float spawnPositiony;
        public bool prefered;

        public Spawnpoint(string Name, float SpawnPositionX , float SpawnPositionY, bool Prefered)
        {
            name = Name;
            spawnPositionx = SpawnPositionX;
            spawnPositiony = SpawnPositionY;
            prefered = Prefered;
        }
    }


    [Serializable]
    public partial struct Positioning
    {
        public Transform tPositioning;
        public int index;

        public Positioning(Transform tPositioning, int index)
        {
            this.tPositioning = tPositioning;
            this.index = index;
        }
    }

    [Serializable]
    public struct ChatMessage
    {
        public string sender;
        public string identifier;
        public string message;
        public string replyPrefix; // copied to input when clicking the message
        public GameObject textPrefab;

        public ChatMessage(string sender, string identifier, string message, string replyPrefix, GameObject textPrefab)
        {
            this.sender = sender;
            this.identifier = identifier;
            this.message = message;
            this.replyPrefix = replyPrefix;
            this.textPrefab = textPrefab;
        }

        // construct the message
        public string Construct()
        {
            return "<b>" + sender + identifier + ":</b> " + message;
        }
    }

    [Serializable]
    public class ChannelInfo
    {
        public string command; // /w etc.
        public string identifierOut; // for sending
        public string identifierIn; // for receiving
        public GameObject textPrefab;

        public ChannelInfo(string command, string identifierOut, string identifierIn, GameObject textPrefab)
        {
            this.command = command;
            this.identifierOut = identifierOut;
            this.identifierIn = identifierIn;
            this.textPrefab = textPrefab;
        }
    }

    #endregion

    public class GeneralPart
    {
        #region Ability

        public ScriptableAbility FindNormalAbility(Player player, string abilityName)
        {
            foreach (ScriptableAbility ab in player.playerAbility.abilities)
            {
                if (ab.name == abilityName)
                {
                    return ab;
                }
            }
            return new ScriptableAbility();
        }

        public Ability FindNetworkAbility(Player player, string abilityName)
        {
            foreach (Ability ab in player.playerAbility.networkAbilities)
            {
                if (ab.name == abilityName)
                {
                    return ab;
                }
            }
            return new Ability();
        }

        public void AddNetworkAbility(Player player, string abilityToAdd , int abilityLevel)
        {
            int abilityInt = -1;
            foreach(ScriptableAbility ab in player.playerAbility.abilities)
            {
                if(ab.name == abilityToAdd)
                {
                    abilityInt = player.playerAbility.abilities.IndexOf(ab);
                }
            }
            player.playerAbility.networkAbilities.Add(new Ability(player.playerAbility.abilities[abilityInt].name, abilityLevel , player.playerAbility.abilities[abilityInt].maxLevel , player.playerAbility.abilities[abilityInt].baseValue));
        }

        public void RemoveNetworkAbility(Player player, string abilityToAdd)
        {
            foreach (Ability ab in player.playerAbility.networkAbilities)
            {
                if (ab.name == abilityToAdd)
                {
                    player.playerAbility.networkAbilities.Remove(ab);
                }
            }
        }

        #endregion
    }



}

[Serializable]
public struct ChatMessage
{
    public string sender;
    public string identifier;
    public string message;
    public string replyPrefix; // copied to input when clicking the message
    public GameObject textPrefab;

    public ChatMessage(string sender, string identifier, string message, string replyPrefix, GameObject textPrefab)
    {
        this.sender = sender;
        this.identifier = identifier;
        this.message = message;
        this.replyPrefix = replyPrefix;
        this.textPrefab = textPrefab;
    }

    // construct the message
    public string Construct()
    {
        return "<b>" + sender + identifier + ":</b> " + message;
    }
}

[Serializable]
public class ChannelInfo
{
    public string command; // /w etc.
    public string identifierOut; // for sending
    public string identifierIn; // for receiving
    public GameObject textPrefab;

    public ChannelInfo(string command, string identifierOut, string identifierIn, GameObject textPrefab)
    {
        this.command = command;
        this.identifierOut = identifierOut;
        this.identifierIn = identifierIn;
        this.textPrefab = textPrefab;
    }
}



#region Custom SyncList
public class SyncListAbility : SyncList<Ability> { }
public class SyncListBoost : SyncList<Boost> { }
public class SyncListCraft : SyncList<CraftItem> { }
public class SyncListPetExp : SyncList<PetExp> { }
public class SyncListUpgradeRepair : SyncList<UpgradeRepairItem> { }
public class SyncListBeeContainer : SyncList<BeeContainer> { }
public class SyncListPlant : SyncList<CultivableFood> { }
public class SyncListSpawnPoint : SyncList<Spawnpoint> { }
public class SyncListWarehouse : SyncList<ItemSlot> { }
public class SyncListAuctionHouse : SyncList<AuctionItem> { }

#endregion
