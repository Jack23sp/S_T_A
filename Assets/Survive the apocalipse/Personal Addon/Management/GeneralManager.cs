using CustomType;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;
using SpriteShadersURP;
using System.Globalization;

public class GeneralManager : MonoBehaviour
{
    private Player player;
    public static GeneralManager singleton;

    public LanguagesManager languagesManager;

    public bool ableBasemodeEditor;

    //public string timeZone = "Eastern Standard Time";
    public string timeZone = "Eastern Standard Time";

    public CultureInfo culture;

    public bool allowDebug;

    [Header("Variable refresh rate")]
    public float wetInterval = 0.5f;
    public float weightInterval = 2.0f;

    [Header("Server ms")]
    public int goodServerPick = 150;
    public int mediumServicePick = 400;


    [Header("Building part")]
    public float buildingSensibility = 0.1f;
    public GameObject buildingManager;
    public LayerMask buildingCheckObstacle;
    public LayerMask buildingCheckSpawn;
    public long buildingExperience;
    public GameObject smokePrefab;
    public GameObject hitPrefab;

    [Header("Flag part")]
    public GameObject flagManager;
    public List<Sprite> flagSprite = new List<Sprite>();

    public GameObject spawnedBuildingObject;
    public GameObject spawnedAttackObject;

    public Color canSpawn;
    public Color notSpawn;

    public Transform canvas;


    [Header("Player")]
    public SimpleInstantiate emojiSpawner;
    public List<ScriptableBoost> listCompleteOfBoost = new List<ScriptableBoost>();
    public List<ScriptableEmoji> listCompleteOfEmoji = new List<ScriptableEmoji>();
    public List<ScriptableDance> listCompleteOfDance = new List<ScriptableDance>();
    public GameObject emojiToSpawn;

    [Header("Premium zone")]
    public Transform premiumZoneSpawn;
    public Sprite ticketImage;

    [Header("Radio/Torch")]
    public Sprite radioImg;
    public Sprite torchImg;

    [Header("Invoke cycle")]
    public float torchInvoke = 60.0f;
    public float radioInvoke = 60.0f;
    public float boostInvoke = 1.0f;
    public float unsanityInvoke = 60.0f;
    public float hungryInvoke = 60.0f;
    public float poisoningInvoke = 60.0f;
    public float thirstyInvoke = 60.0f;
    public float temperatureInvoke = 60.0f;

    [Header("Spawnpoint")]
    public Sprite prefered;
    public Sprite notPrefered;
    public ScriptableItem Instantresurrect;
    public GameObject spawnpointPanelToCreate;
    public string messageInDescription;
    public string messageInDescriptionIta;

    [Header("Spawn On Death")]
    public GameObject chestItem;
    public GameObject uiChestPanel;

    [Header("Spawn at start")]
    public GameObject spawnAtStart;

    [Header("Trading panel")]
    public GameObject tradingPanelToSpawn;
    [HideInInspector] public GameObject spawnedTradingPanel;


    [Header("Ability")]
    public List<ScriptableAbility> abilityList = new List<ScriptableAbility>();
    public ScriptableAbility allianceAbility;
    public ScriptableAbility accuracyAbility;
    public ScriptableAbility missAbility;
    //public ScriptableAbility plantAbility;
    public ScriptableAbility rockAbility;
    public ScriptableAbility treeAbility;
    public ScriptableAbility conservativeAbility;
    public ScriptableAbility spawnpointAbility;

    [Header("Fire Ammo")]
    public GameObject firedEffectToSpawnAtPlayer;
    public int firedDamage = 10;
    public int cycleAmountFireToAdd = 3;
    public float firePlayerInvoke = 2.0f;

    [Header("Electric Ammo")]
    public GameObject electricEffectToSpawnAtPlayer;
    public int electricDamage = 10;
    public int cycleAmountElectricToAdd = 3;
    public float electricPlayerInvoke = 2.0f;

    [Header("Poisoned Ammo")]
    public GameObject poisonedEffectToSpawnAtPlayer;

    [Header("Spawn object when attack building")]
    public GameObject spawnEffectWhenAttackBuilding;
    public AudioClip meleeToBuilding;

    [Header("Spawn object when attack rock")]
    public GameObject spawnEffectWhenAttackRock;
    public AudioClip meleeToRock;

    [Header("Spawn object when attack tree")]
    public GameObject spawnEffectWhenAttackTree;
    public AudioClip meleeToTree;

    [Header("Stats part")]
    public Sprite armor;
    public Sprite health;
    public Sprite adrenaline;
    public Sprite damage;
    public Sprite defense;
    public Sprite accuracy;
    public Sprite miss;
    public Sprite critPerc;
    public Sprite weight;
    public Sprite poisoned;
    public Sprite hungry;
    public Sprite thirsty;
    public Sprite blood;
    public Sprite marriage;
    public Sprite cover;
    public Sprite activeBoost;
    public Sprite defenseBonus;
    public Sprite healthBonus;
    public Sprite manaBonus;
    public Sprite trap;
    public Sprite ill;
    public Sprite lostable;

    [Header("Marriage bonus")]
    public int marriageHealth;
    public int marriageDefense;
    public int marriageMana;
    public float activeMarriageBonusPerc = 0.3f;
    public GameObject marriageRemovePanel;

    public bool alreadyRefresh;

    [Header("Building Spawn")]
    public List<BuildingSpawn> buildingSpawn = new List<BuildingSpawn>();

    [Header("Move Mode")]
    public float runMultiplier = 1.5f;
    public float sneakMultiplier = 0.5f;
    public float normalMultiplier = 1.0f;
    public float initialSpeed = 5.0f;
    public float carMultiplier = 2.5f;

    [Header("Friends")]
    public int maxFriends = 20;
    public int maxFriendRequest = 20;
    public Color onlineColor;
    public Color offlineColor;
    public GameObject friendMessageInvite;
    public GameObject spawnedUIInvite;

    [Header("Animal")]
    public ScriptableAnimal[] allAnimal = new ScriptableAnimal[0];

    [Header("Options")]
    public GameObject issueSuggestion;

    [Header("Blood")]
    public int maxBlood;
    public float decreaseBloodTimer;

    [Header("Item mall")]
    public GameObject itemMallPanel;
    [SerializeField] public GameObject uiItemMallPanel;
    public GameObject selectedItemMallMultiple;

    [Header("Daily rewards")]
    public List<Rewards> ListRewards = new List<Rewards>() { new Rewards { } };

    [Header("Craft")]
    public List<BuildingItem> buildingItems = new List<BuildingItem>();
    public GameObject alreadyCraftPanel;

    [Header("Car")]
    public GameObject carPanelToSpawn;
    public GameObject passengerDummy;
    public GameObject carManager;
    public GameObject carInventory;
    public float gasolineDecreaseTimer = 1.0f;
    public List<CarItem> carItem = new List<CarItem>();

    [Header("Plant")]
    public List<ResourceItem> plants = new List<ResourceItem>();

    [Header("Rock")]
    public List<ResourceItem> rocks = new List<ResourceItem>();
    public int damageToResource;

    [Header("Tree")]
    public List<ResourceItem> trees = new List<ResourceItem>();

    [Header("Resource")]
    public List<ResourceImage> resourceRewardImage = new List<ResourceImage>();

    [Header("Points")]
    public int plantPoint;
    public int rockPoint;
    public int treePoint;
    public int monsterPoint;
    public int bossKill;
    public int craftItemPoint;
    public int playerPoint;
    public int buyBoostPoint;

    public int buildingCreatePoint;
    public int buildingClaimPoint;
    public int buildingUpgradePoint;
    public int buildingRepairPoint;

    [Header("Leaderboard")]
    public int personalPoints;
    public int groupPoint;
    public int AlliancePoint;
    public List<LeaderboardItem> leaderboardReward = new List<LeaderboardItem>();
    public GameObject leaderboardPanelToSpawn;
    public Color personalColor;
    public Color groupColor;
    public Color allyClor;

    [Header("Bee Manager")]
    public int maxBeeForContainer = 200;
    public int maxHoneyForContainer = 200;
    public int maxBeeForHours = 11;
    public float beeInvoke = 40000.0f;

    [Header("Water Well")]
    public LinearInt levelWater;
    public float waterInvoke;

    [Header("Breeding")]
    public float intervalPregnantGrow;
    public float intervalYear;
    public float intervalWater;
    public float intervalFood;
    public float intervalDistanceBetweenSex;
    public float intervalTake;
    public float intervalCheckAfterDeath;
    public float intervalGrownPlant;
    public float intervalGrownPlantOnClient;
    public float intervalConsumeWoodCampfire;
    public float intervalDryCampfire;
    public float intervalIllness;

    [Header("Animal")]
    public GameObject addAnimalPanel;
    public GameObject animalListPanel;
    public int cow = 2;
    public int horse = 2;
    public int sheep = 5;
    public int pig = 3;
    public int chicken = 10;
    public List<AnimalImage> animalImage = new List<AnimalImage>();

    [Header("Building manager stuff")]
    public GameObject buildingManagerPanel;
    public GameObject plantSelectorPanel;

    [Header("Warehouse")]
    public int groupWarehouseSlot;
    public int personalWarehouseSlot;

    [Header("Building item spawner")]
    public List<CustomItem> itemSpawner = new List<CustomItem>();
    public int maxItemInWorldHouse = 7;
    public float intervalCheckWorldHouse = 1.0f;

    [Header("Gas Station")]
    public int maxGasStationGasoline;
    public float intervalChargeGasStation;
    public int chargeGasolineAmount;

    [Header("Mine")]
    public GameObject explosionPrefab;
    public LayerMask affectedByMineExplosion;
    public LinearInt mineDamage;
    public int nearEntityExplosionRange;
    public GameObject confirmDeletePanel;
    [HideInInspector] public List<Entity> objectToExplode = new List<Entity>();

    [Header("Totem")]
    public GameObject totemPrefab;
    public GameObject uiTotem;

    [Header("Campfire")]
    public LinearInt woodAmount;
    public LinearInt maxCookedAmount;
    public GameObject cookedItemPanel;
    public LayerMask campfireLayerMask;


    [Header("Tesla")]
    public LinearInt teslaDamage;

    [Header("Upgrade/Repair Material Panel")]
    public GameObject upgradeRepairMaterialPanel;
    public GameObject upgradebuyMaterial;

    [Header("Stats image")]
    public Sprite temperatureImage;
    public Sprite coinsImage;
    public Sprite goldImage;

    [Header("Illness")]
    public int decreaseHealthIfWet;

    [Header("Character Creation")]
    public GameObject popUpAutomicaticName;

    private bool panelIsSpawn;

    [Header("Player Rank")]
    public List<PlayerRank> playerRank = new List<PlayerRank>();

    [Header("Equip to elude tesla")]
    public List<EquipmentItem> teslaItem = new List<EquipmentItem>();


    [Header("Teleport item parameters")]
    public int teleportSeconds;
    public GameObject teleportInviteSlot;
    public GameObject spawnedTeleport;
    public GameObject teleportInviter;
    public GameObject spawnedteleportInviter;

    [Header("Pet experience from trainer")]
    public float experienceForEachFoodItem;
    public int timeToUpgradeOfOneLevel;
    public ScriptableItem foodToUpgradeLevelOfItem;
    public GameObject petTrainingInProgress;

    [Header("Pet cure and revive")]
    public GameObject perReviveAndCure;
    public ScriptableItem curePetItem;
    public ScriptableItem revivePetItem;

    [Header("Panel building crafter")]
    public GameObject buildingCrafterPanel;

    [Header("Thanks panel")]
    public GameObject thanksPanel;

    [Header("Create group Panel")]
    public GameObject createGroupPanel;

    [Header("Item mall")]
    public ItemMallCategory[] itemMallCategories;
    public ItemMallCategory[] manClothes;
    public ItemMallCategory[] womanClothes;

    public AudioSource shotAudioSource;
    public AudioSource bulletsDropAudioSource;
    public AudioSource reloadAudioSource;

    [Header("Skillbar selected item panel")]
    public GameObject itemDisplayer;

    [Header("Marriage prefab")]
    public GameObject makeMarriage;
    public GameObject breakMarriage;

    [Header("Info button message")]
    public List<string> infoMessage = new List<string>();

    [Header("Render texture")]
    public RenderTexture sampleTexture;

    [Header("Prefab")]
    public GameObject malePrefab;
    public GameObject male;
    public GameObject femalePrefab;
    public GameObject female;

    public float minHeight = -0.3f;
    public float maxHeight = 0.05f;

    [Header("Neutral header")]
    public RuntimeAnimatorController defaultAnimatorController;

    [Header("Layer")]
    public LayerMask localPlayer;
    public LayerMask notLocalPlayer;

    [Header("Monster sound")]
    public AudioClip idleSound;
    public List<AudioClip> moveSound = new List<AudioClip>();
    public AudioClip deathSound;


    [Header("Entity check layer")]
    public LayerMask entityLayerMask;
    //public GameObject basementObject;
    //public GameObject chatObject;
    //public GameObject customPanel;
    //public GameObject basementPlacer;
    //public List<ScriptableBuilding> verticalWall;
    //public List<ScriptableBuilding> horizontalWall;s

    [Header("Modular Building")]
    public GameObject modularBuildingManager;
    //public LayerMask modularLayerMask;
    public LayerMask modularFloorCheckSpawn;
    public LayerMask modularObjectLayerMask;
    public LayerMask modularObjectNeedBaseLayerMask;
    public LayerMask modularObjectWallLayerMask;


    public LayerMask modularObjecObstacleLayerMask;
    public LayerMask modularObjecObstaclePlacementLayerMask;

    public LayerMask searchFornitureLayerMask;
    public LayerMask modularWallMinePositioning;

    [Header("Furnace items")]
    public ScriptableItem sulfur;
    public ScriptableItem highMetal;
    public ScriptableItem coal;

    [Header("Electic box")]
    public GameObject electricBoxObject;
    public GameObject instantiatedElecteicBox;
    public GameObject deleteObject;
    public GameObject instantiatedDeleteObject;

    public DateTime ChangeServerToClientTime(DateTime time, int seconds = 0)
    {
        return DateTime.ParseExact(time.ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
    }

    public DateTime ChangeClientToServerTime(DateTime time, int seconds = 0)
    {
        return DateTime.ParseExact(time.ToString(), "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

    }

    public DateTime ChangeServerToClientTime(string time, int seconds = 0)
    {
        return DateTime.ParseExact(time, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
    }

    public DateTime ChangeClientToServerTime(string time, int seconds = 0)
    {
        return DateTime.ParseExact(time, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

    }

    public Joystick joystick;

    public void SetChildLayer(Transform parent, string childName, int layer)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                child.gameObject.layer = layer;
                SetChildLayer(child, childName, layer);
            }
            else
            {
                SetChildLayer(child, childName, layer);
            }
        }
    }

    public void Start()
    {
        if (!singleton) singleton = this;

        culture = new CultureInfo("es-ES");

        Debug.unityLogger.logEnabled = allowDebug;
        languagesManager.GetDefaultLanguage();
    }

    public void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player && !panelIsSpawn)
        {
            Instantiate(spawnAtStart, canvas);
            panelIsSpawn = true;
        }

        if (objectToExplode.Count > 0)
        {
            for (int i = 0; i < objectToExplode.Count; i++)
            {
                if (objectToExplode[i].isServer && objectToExplode[i].GetComponent<Mine>())
                {
                    objectToExplode[i].GetComponent<Mine>().Explode();
                }
                if (objectToExplode[i].isServer && objectToExplode[i].GetComponent<Dynamite>())
                {
                    objectToExplode[i].GetComponent<Dynamite>().Explode();
                }
                objectToExplode.RemoveAt(i);
            }
        }

        if (!alreadyRefresh)
        {

            emojiSpawner.entity = player;
            GameObject emoji = Instantiate(emojiSpawner.objectToSpawn, emojiSpawner.entity.transform);

            allAnimal = Resources.LoadAll<ScriptableAnimal>("");

            alreadyRefresh = true;
        }


    }

    public void AbleBasementEditor(bool trigger)
    {
        ableBasemodeEditor = trigger;
    }

    public bool TeslaEquipment(Player player)
    {
        for (int i = 0; i < player.equipment.Count; i++)
        {
            if (player.equipment[i].amount == 0) continue;
            if (player.equipment[i].item.data is EquipmentItem && player.equipment[i].amount > 0)
            {
                if (teslaItem.Contains((EquipmentItem)player.equipment[i].item.data))
                {
                    return true;
                }
            }
        }
        return false;
    }

    //public bool HoneyEquipment(Player player)
    //{
    //    for (int i = 0; i < player.equipment.Count; i++)
    //    {
    //        if (player.equipment[i].amount == 0) continue;
    //        if (player.equipment[i].item.data is EquipmentItem && player.equipment[i].amount > 0)
    //        {
    //            if (honeyItem.Contains((EquipmentItem)player.equipment[i].item.data))
    //            {
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}

    public string ConvertToTimer(int totalSecond)
    {
        int day = 86400;
        int hour = 3600;
        int minutes = 60;

        int tDay = 0;
        int tHours = 0;
        int tMinutes = 0;


        tDay = totalSecond / day;
        totalSecond = (totalSecond - (tDay * day));

        tHours = totalSecond / hour;
        totalSecond = (totalSecond - (tHours * hour));

        tMinutes = totalSecond / minutes;
        totalSecond = (totalSecond - (tMinutes * minutes));

        string Sday = tDay < 10 ? "0" + tDay : tDay.ToString();
        string Shours = tHours < 10 ? "0" + tHours : tHours.ToString();
        string SMinute = tMinutes < 10 ? "0" + tMinutes : tMinutes.ToString();
        string SSeconds = totalSecond < 10 ? "0" + totalSecond : totalSecond.ToString();

        return Sday + " : " + Shours + " : " + SMinute + " : " + SSeconds;

    }

    public string ConvertToTimer(long totalSecond)
    {
        long day = 86400;
        long hour = 3600;
        long minutes = 60;

        long tDay = 0;
        long tHours = 0;
        long tMinutes = 0;


        tDay = totalSecond / day;
        totalSecond = (totalSecond - (tDay * day));

        tHours = totalSecond / hour;
        totalSecond = (totalSecond - (tHours * hour));

        tMinutes = totalSecond / minutes;
        totalSecond = (totalSecond - (tMinutes * minutes));

        string Sday = tDay < 10 ? "0" + tDay : tDay.ToString();
        string Shours = tHours < 10 ? "0" + tHours : tHours.ToString();
        string SMinute = tMinutes < 10 ? "0" + tMinutes : tMinutes.ToString();
        string SSeconds = totalSecond < 10 ? "0" + totalSecond : totalSecond.ToString();

        return Sday + " : " + Shours + " : " + SMinute + " : " + SSeconds;

    }

    public string GetDescription(Boost boost)
    {
        string description = string.Empty;

        description += name + "\n";
        TimeSpan difference;


        if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].velocityTimer))
            difference = DateTime.Parse(player.playerBoost.networkBoost[0].velocityTimer.ToString()) - DateTime.Now;

        if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].velocityTimer))
            description += "Velocity : " + ConvertToTimer(Convert.ToInt64(difference.TotalSeconds)) + " ( + " + boost.velocityPerc + " %) \n";
        else
            description += "Velocity : " + ConvertToTimer(0) + " ( + " + boost.velocityPerc + " %) \n";

        if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].accuracyTimer))
            difference = DateTime.Parse(player.playerBoost.networkBoost[0].accuracyTimer.ToString()) - DateTime.Now;

        if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].accuracyTimer))
            description += "Accuracy : " + ConvertToTimer(Convert.ToInt64(difference.TotalSeconds)) + " ( + " + boost.accuracyPerc + " %) \n";
        else
            description += "Accuracy : " + ConvertToTimer(0) + " ( + " + boost.accuracyPerc + " %) \n";


        if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].missTimer))
            difference = DateTime.Parse(player.playerBoost.networkBoost[0].missTimer.ToString()) - DateTime.Now;

        if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].missTimer))
            description += "Miss : " + ConvertToTimer(Convert.ToInt64(difference.TotalSeconds)) + " ( + " + boost.missPerc + " %) \n";
        else
            description += "Miss : " + ConvertToTimer(0) + " ( + " + boost.missPerc + " %) \n";


        if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer))
            difference = DateTime.Parse(player.playerBoost.networkBoost[0].hiddenIslandTimer.ToString()) - DateTime.Now;

        if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer))
            description += "Premium zone : " + ConvertToTimer(Convert.ToInt64(difference.TotalSeconds)) + "\n";
        else
            description += "Premium zone : " + ConvertToTimer(0) + "\n";

        if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleEXP))
            difference = DateTime.Parse(player.playerBoost.networkBoost[0].doubleEXP.ToString()) - DateTime.Now;

        if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleEXP))
            description += "Double exp. : " + ConvertToTimer(Convert.ToInt64(difference.TotalSeconds)) + "\n";
        else
            description += "Double exp. : " + ConvertToTimer(0) + "\n";

        if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleGold))
            difference = DateTime.Parse(player.playerBoost.networkBoost[0].doubleGold.ToString()) - DateTime.Now;

        if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleGold))
            description += "Double gold : " + ConvertToTimer(Convert.ToInt64(difference.TotalSeconds)) + "\n";
        else
            description += "Double gold : " + ConvertToTimer(0) + "\n";

        if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleLeaderPoints))
            difference = DateTime.Parse(player.playerBoost.networkBoost[0].doubleLeaderPoints.ToString()) - DateTime.Now;

        if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleLeaderPoints))
            description += "Double points : " + ConvertToTimer(Convert.ToInt64(difference.TotalSeconds)) + "\n";
        else
            description += "Double points : " + ConvertToTimer(0) + "\n";

        if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleDamageToMonster))
            difference = DateTime.Parse(player.playerBoost.networkBoost[0].doubleDamageToMonster.ToString()) - DateTime.Now;

        if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleDamageToMonster))
            description += "Double damage to monster : " + ConvertToTimer(Convert.ToInt64(difference.TotalSeconds)) + "\n";
        else
            description += "Double damage to monster : " + ConvertToTimer(0) + "\n";

        if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleDamageToPlayer))
            difference = DateTime.Parse(player.playerBoost.networkBoost[0].doubleDamageToPlayer.ToString()) - DateTime.Now;

        if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleDamageToPlayer))
            description += "Double damage to monster : " + ConvertToTimer(Convert.ToInt64(difference.TotalSeconds)) + "\n";
        else
            description += "Double damage to monster : " + ConvertToTimer(0) + "\n";


        return description;
    }

    public string GetDescription(ScriptableBoost boost)
    {
        string description = string.Empty;

        description += name + "\n";
        description += "Velocity : " + ConvertToTimer(boost.velocityTimer) + " ( + " + boost.velocityPerc + " %) \n";
        description += "Accuracy : " + ConvertToTimer(boost.accuracyTimer) + " ( + " + boost.accuracyPerc + " %) \n";
        description += "Miss : " + ConvertToTimer(boost.missTimer) + " ( + " + boost.missPerc + " %) \n";
        description += "Premium zone : " + ConvertToTimer(boost.hiddenIslandTimer) + "\n";
        description += "Double exp. : " + ConvertToTimer(boost.doubleEXP) + "\n";
        description += "Pet double exp. : " + ConvertToTimer(boost.petDoubleEXP) + "\n";
        description += "Party double exp. : " + ConvertToTimer(boost.partyDoubleEXP) + "\n";
        description += "Guild double exp. : " + ConvertToTimer(boost.guildDoubleEXP) + "\n";
        description += "Double gold : " + ConvertToTimer(boost.doubleGold) + "\n";
        description += "Party double gold : " + ConvertToTimer(boost.partyDoubleGold) + "\n";
        description += "Guild double gold : " + ConvertToTimer(boost.guildDoubleGold) + "\n";
        description += "Double points : " + ConvertToTimer(boost.doubleLeaderPoints) + "\n";
        description += "Double damage to monster : " + ConvertToTimer(boost.doubleDamageToMonster) + "\n";
        description += "Double damage to player : " + ConvertToTimer(boost.doubleDamageToPlayer) + "\n";
        description += "Double damage to building : " + ConvertToTimer(boost.doubleDamageToBuilding) + "\n";

        return description;
    }

    public ScriptableBoost GetBoostTemplate(Boost boost, int index)
    {
        return listCompleteOfBoost[index];
    }

    public int AddSecondsToBoost(ScriptableBoost boost)
    {
        if (boost.velocityTimer != 0)
        {
            return boost.velocityTimer;
        }
        if (boost.accuracyTimer != 0)
        {
            return boost.accuracyTimer;
        }
        if (boost.missTimer != 0)
        {
            return boost.missTimer;
        }
        if (boost.hiddenIslandTimer != 0)
        {
            return boost.hiddenIslandTimer;
        }
        if (boost.doubleEXP != 0)
        {
            return boost.doubleEXP;
        }
        if (boost.doubleGold != 0)
        {
            return boost.doubleGold;
        }
        if (boost.doubleLeaderPoints != 0)
        {
            return boost.doubleLeaderPoints;
        }
        if (boost.doubleDamageToMonster != 0)
        {
            return boost.doubleDamageToMonster;
        }
        if (boost.doubleDamageToPlayer != 0)
        {
            return boost.doubleDamageToPlayer;
        }
        if (boost.doubleDamageToBuilding != 0)
        {
            return boost.doubleDamageToBuilding;
        }

        return 0;
    }


    public int FindNetworkEmoji(string emojiName, string playerName)
    {
        Player player;
        int index = -1;
        if (Player.onlinePlayers.TryGetValue(playerName, out player))
        {
            for (int i = 0; i < player.playerEmoji.networkEmoji.Count; i++)
            {
                if (player.playerEmoji.networkEmoji[i] == emojiName)
                {
                    return i;
                }
            }
        }

        return index;
    }

    public int FindNetworkDance(string danceName, string playerName)
    {
        Player player;
        int index = -1;
        if (Player.onlinePlayers.TryGetValue(playerName, out player))
        {
            for (int i = 0; i < player.playerDance.networkDance.Count; i++)
            {
                if (player.playerDance.networkDance[i] == danceName)
                {
                    return i;
                }
            }
        }

        return index;
    }

    public int FindNetworkAbility(string abilityName, string playerName)
    {
        Player player;
        int index = -1;
        if (Player.onlinePlayers.TryGetValue(playerName, out player))
        {

            for (int i = 0; i < player.playerAbility.networkAbilities.Count; i++)
            {
                if (player.playerAbility.networkAbilities[i].name == abilityName)
                {
                    return i;
                }
            }
        }

        return index;
    }

    public int FindNetworkAbilityLevel(string abilityName, string playerName)
    {
        Player player;
        if (Player.onlinePlayers.TryGetValue(playerName, out player))
        {
            for (int i = 0; i < player.playerAbility.networkAbilities.Count; i++)
            {
                if (player.playerAbility.networkAbilities[i].name == abilityName)
                {
                    return player.playerAbility.networkAbilities[i].level;
                }
            }
        }
        return 0;
    }

    public int FindNetworkAbilityMaxLevel(string abilityName, string playerName)
    {
        Player player;
        if (Player.onlinePlayers.TryGetValue(playerName, out player))
        {
            for (int i = 0; i < player.playerAbility.networkAbilities.Count; i++)
            {
                if (player.playerAbility.networkAbilities[i].name == abilityName)
                {
                    return player.playerAbility.networkAbilities[i].maxLevel;
                }
            }
        }
        return -1;
    }

    public int FindNetworkBoostTime(string boostName, string playerName)
    {
        Player player;
        TimeSpan difference;
        if (Player.onlinePlayers.TryGetValue(playerName, out player))
        {
            for (int i = 0; i < player.playerBoost.networkBoost.Count; i++)
            {
                if (boostName == "Accuracy")
                {
                    if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].accuracyTimer))
                    {
                        difference = DateTime.Parse(player.playerBoost.networkBoost[0].accuracyTimer.ToString()) - DateTime.Now;
                        return Convert.ToInt32(difference.TotalSeconds);
                    }
                    else
                    {
                        return 0;
                    }
                }
                if (boostName == "Double damage to monster")
                {
                    if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleDamageToMonster))
                    {
                        difference = DateTime.Parse(player.playerBoost.networkBoost[0].doubleDamageToMonsterServer.ToString()) - DateTime.Now;
                        return Convert.ToInt32(difference.TotalSeconds);
                    }
                    else
                    {
                        return 0;
                    }
                }
                if (boostName == "Double damage to player")
                {
                    if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleDamageToPlayer))
                    {
                        difference = DateTime.Parse(player.playerBoost.networkBoost[0].doubleDamageToPlayer.ToString()) - DateTime.Now;
                        return Convert.ToInt32(difference.TotalSeconds);
                    }
                    else
                    {
                        return 0;
                    }
                }
                if (boostName == "Double experience")
                {
                    if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleEXP))
                    {
                        difference = DateTime.Parse(player.playerBoost.networkBoost[0].doubleEXP.ToString()) - DateTime.Now;
                        return Convert.ToInt32(difference.TotalSeconds);
                    }
                    else
                    {
                        return 0;
                    }
                }
                if (boostName == "Double gold")
                {
                    if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleGold))
                    {
                        difference = DateTime.Parse(player.playerBoost.networkBoost[0].doubleGold.ToString()) - DateTime.Now;
                        return Convert.ToInt32(difference.TotalSeconds);
                    }
                    else
                    {
                        return 0;
                    }
                }
                if (boostName == "Double points")
                {
                    if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleLeaderPoints))
                    {
                        difference = DateTime.Parse(player.playerBoost.networkBoost[0].doubleLeaderPoints.ToString()) - DateTime.Now;
                        return Convert.ToInt32(difference.TotalSeconds);
                    }
                    else
                    {
                        return 0;
                    }
                }
                if (boostName == "Evasion")
                {
                    if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].missTimer))
                    {
                        difference = DateTime.Parse(player.playerBoost.networkBoost[0].missTimer.ToString()) - DateTime.Now;
                        return Convert.ToInt32(difference.TotalSeconds);
                    }
                    else
                    {
                        return 0;
                    }
                }
                if (boostName == "Premium")
                {
                    if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer))
                    {
                        difference = DateTime.Parse(player.playerBoost.networkBoost[0].hiddenIslandTimer.ToString()) - DateTime.Now;
                        return Convert.ToInt32(difference.TotalSeconds);
                    }
                    else
                    {
                        return 0;
                    }
                }
                if (boostName == "Speed")
                {
                    if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].velocityTimer))
                    {
                        difference = DateTime.Parse(player.playerBoost.networkBoost[0].velocityTimer.ToString()) - DateTime.Now;
                        return Convert.ToInt32(difference.TotalSeconds);
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }
        return -1;
    }

    public int FindNetworkServerBoostTime(string boostName, string playerName)
    {
        Player player;
        TimeSpan difference;
        if (Player.onlinePlayers.TryGetValue(playerName, out player))
        {
            for (int i = 0; i < player.playerBoost.networkBoost.Count; i++)
            {
                if (boostName == "Accuracy")
                {
                    if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].accuracyTimerServer))
                    {
                        difference = DateTime.Parse(player.playerBoost.networkBoost[0].accuracyTimerServer.ToString()) - DateTime.Now;
                        return Convert.ToInt32(difference.TotalSeconds);
                    }
                    else
                    {
                        return 0;
                    }
                }
                if (boostName == "Double damage to monster")
                {
                    if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleDamageToMonsterServer))
                    {
                        difference = DateTime.Parse(player.playerBoost.networkBoost[0].doubleDamageToMonsterServer.ToString()) - DateTime.Now;
                        return Convert.ToInt32(difference.TotalSeconds);
                    }
                    else
                    {
                        return 0;
                    }
                }
                if (boostName == "Double damage to player")
                {
                    if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleDamageToPlayerServer))
                    {
                        difference = DateTime.Parse(player.playerBoost.networkBoost[0].doubleDamageToPlayerServer.ToString()) - DateTime.Now;
                        return Convert.ToInt32(difference.TotalSeconds);
                    }
                    else
                    {
                        return 0;
                    }
                }
                if (boostName == "Double experience")
                {
                    if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleEXPServer))
                    {
                        difference = DateTime.Parse(player.playerBoost.networkBoost[0].doubleEXPServer.ToString()) - DateTime.Now;
                        return Convert.ToInt32(difference.TotalSeconds);
                    }
                    else
                    {
                        return 0;
                    }
                }
                if (boostName == "Double gold")
                {
                    if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleGoldServer))
                    {
                        difference = DateTime.Parse(player.playerBoost.networkBoost[0].doubleGoldServer.ToString()) - DateTime.Now;
                        return Convert.ToInt32(difference.TotalSeconds);
                    }
                    else
                    {
                        return 0;
                    }
                }
                if (boostName == "Double points")
                {
                    if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleLeaderPointsServer))
                    {
                        difference = DateTime.Parse(player.playerBoost.networkBoost[0].doubleLeaderPointsServer.ToString()) - DateTime.Now;
                        return Convert.ToInt32(difference.TotalSeconds);
                    }
                    else
                    {
                        return 0;
                    }
                }
                if (boostName == "Evasion")
                {
                    if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].missTimerServer))
                    {
                        difference = DateTime.Parse(player.playerBoost.networkBoost[0].missTimerServer.ToString()) - DateTime.Now;
                        return Convert.ToInt32(difference.TotalSeconds);
                    }
                    else
                    {
                        return 0;
                    }
                }
                if (boostName == "Premium")
                {
                    if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimerServer))
                    {
                        difference = DateTime.Parse(player.playerBoost.networkBoost[0].hiddenIslandTimerServer.ToString()) - DateTime.Now;
                        return Convert.ToInt32(difference.TotalSeconds);
                    }
                    else
                    {
                        return 0;
                    }
                }
                if (boostName == "Speed")
                {
                    if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].velocityTimerServer))
                    {
                        difference = DateTime.Parse(player.playerBoost.networkBoost[0].velocityTimerServer.ToString()) - DateTime.Now;
                        return Convert.ToInt32(difference.TotalSeconds);
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }
        return -1;
    }

    public int FindZombieQuest(Monster monster, ScriptableQuest quest)
    {
        int i = 0;
        foreach (ZombieKill zombieKill in quest.killZombie)
        {
            i++;
            if (zombieKill.monster.name == monster.name.Replace("(Clone)", ""))
            {
                return i;
            }
        }
        return -1;
    }

    public ScriptableAnimal FindAnimal(string animalName)
    {
        for (int i = 0; i < allAnimal.Length; i++)
        {
            int index = i;
            if (allAnimal[index].name == animalName)
                return allAnimal[index];
        }

        return null;
    }

    public int FindItemToCraft(ScriptableItem building)
    {
        for (int i = 0; i < buildingItems.Count; i++)
        {
            int index = i;
            if (buildingItems[index].specificBuilding.name == building.name)
            {
                return buildingItems[index].buildingItem.Count;
            }
        }
        return -1;
    }

    public int ListOfBuildingItem(ScriptableItem building)
    {
        for (int i = 0; i < buildingItems.Count; i++)
        {
            int index = i;
            if (buildingItems[index].specificBuilding.name == building.name)
            {
                return index;
            }
        }
        return -1;
    }

    public ScriptableItem CraftingBuildItem(string buildingItem)
    {
        for (int i = 0; i < buildingItems.Count; i++)
        {
            int index = i;
            if (buildingItems[index].specificBuilding.name == buildingItem)
            {
                return buildingItems[index].specificBuilding;
            }
        }
        return null;
    }

    public ItemInBuilding CraftingInternalBuilding(ScriptableItem building, int itemIndex)
    {
        for (int i = 0; i < buildingItems.Count; i++)
        {
            int index = i;
            if (buildingItems[index].specificBuilding.name == building.name)
            {
                return buildingItems[index].buildingItem[itemIndex];
            }
        }
        return new ItemInBuilding();
    }

    public void GetCarItems(Car carName)
    {
        for (int i = 0; i < carItem.Count; i++)
        {
            int index = i;
            if (carItem[index].car.name == carName.gameObject.name)
            {
                carName.carItems = carItem[index].carItems;
            }
        }
    }

    public void GetInventoryItems(Car carName)
    {
        for (int i = 0; i < carItem.Count; i++)
        {
            int index = i;

            if (carItem[index].car.name.Contains(carName.gameObject.name))
            {
                carName.maxInventoryItem = carItem[index].car.maxCarItemInventory;
            }
        }
    }

    #region Rock
    public bool RockAllowedWeapon(Player player, WeaponItem weapon)
    {
        if (!(player.target is Rock)) return false;
        return GetRockAllowedWeapons(((Rock)player.target), weapon);
    }

    public bool GetRockAllowedWeapons(Rock rock, WeaponItem weapon)
    {
        for (int i = 0; i < rocks.Count; i++)
        {
            int index = i;
            if (rocks[index].resource == rock.rock)
            {
                if (rocks[index].allowedWeapons.Contains(weapon))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        return false;
    }

    public string GetRockRewards(Rock rock, string rewardType)
    {
        for (int i = 0; i < rocks.Count; i++)
        {
            int index = i;
            if (rocks[index].resource.name == rock.gameObject.name.Replace("(Clone)", ""))
            {
                if (rewardType == "Normal")
                {
                    return "1 " + rocks[index].normalRewards.name;
                }
                if (rewardType == "Special")
                {
                    return "2 " + rocks[index].specialRewards.name;
                }
                if (rewardType == "SuperSpecial")
                {
                    return "1 " + rocks[index].superSpecialRewards.name;
                }
            }
        }
        return null;
    }

    public Item GetRockRewardsItem(Rock rock, string rewardType)
    {
        for (int i = 0; i < rocks.Count; i++)
        {
            int index = i;
            if (rocks[index].resource == rock.rock)
            {
                if (rewardType == "Normal")
                {
                    return new Item(rocks[index].normalRewards);
                }
                if (rewardType == "Special")
                {
                    return new Item(rocks[index].specialRewards);
                }
                if (rewardType == "SuperSpecial")
                {
                    return new Item(rocks[index].superSpecialRewards);
                }
            }
        }
        return new Item();
    }

    #endregion

    #region Tree
    public bool TreeAllowedWeapon(Player player, WeaponItem weapon)
    {
        if (!(player.target is Tree)) return false;
        return GetTreeAllowedWeapons(((Tree)player.target), weapon);
    }

    public bool GetTreeAllowedWeapons(Tree rock, WeaponItem weapon)
    {
        for (int i = 0; i < trees.Count; i++)
        {
            int index = i;
            if (trees[index].resource == rock.tree)
            {
                if (trees[index].allowedWeapons.Contains(weapon))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        return false;
    }

    public string GetTreeRewards(Tree rock, string rewardType)
    {
        for (int i = 0; i < trees.Count; i++)
        {
            int index = i;
            if (trees[index].resource == rock.tree)
            {
                if (rewardType == "Normal")
                {
                    return "1 " + trees[index].normalRewards.name;
                }
                if (rewardType == "Special")
                {
                    return "2 " + trees[index].specialRewards.name;
                }
                if (rewardType == "SuperSpecial")
                {
                    return "1 " + trees[index].superSpecialRewards.name;
                }
            }
        }
        return null;
    }

    public Item GetTreeRewardsItem(Tree rock, string rewardType)
    {
        for (int i = 0; i < trees.Count; i++)
        {
            int index = i;
            if (trees[index].resource == rock.tree)
            {
                if (rewardType == "Normal")
                {
                    return new Item(trees[index].normalRewards);
                }
                if (rewardType == "Special")
                {
                    return new Item(trees[index].specialRewards);
                }
                if (rewardType == "SuperSpecial")
                {
                    return new Item(trees[index].superSpecialRewards);
                }
            }
        }
        return new Item();
    }

    #endregion

    #region Plant
    public bool PlantAllowedWeapon(Player player, ScriptableItem weapon)
    {
        if (!(player.target is Plant)) return false;
        return GetPlantAllowedWeapons(((Plant)player.target), player.equipment[0].item.data);
    }

    public bool GetPlantAllowedWeapons(Plant rock, ScriptableItem weapon)
    {
        for (int i = 0; i < plants.Count; i++)
        {
            int index = i;
            if (plants[index].resource == rock.plant)
            {
                if (plants[index].allowedWeapons.Contains(weapon))
                {
                    if (rock.dimensionX >= ((ScriptablePlant)plants[index].resource).scaleDimension.x && rock.dimensionY >= ((ScriptablePlant)plants[index].resource).scaleDimension.y)
                        return true;
                    else
                        return false;
                }
                else
                {
                    return false;
                }
            }
        }
        return false;
    }

    public string GetPlantRewards(Plant rock, string rewardType)
    {
        for (int i = 0; i < plants.Count; i++)
        {
            int index = i;
            if (plants[index].resource.name == rock.gameObject.name.Replace("(Clone)", ""))
            {
                if (rewardType == "Normal")
                {
                    return "1 " + plants[index].normalRewards.name;
                }
                if (rewardType == "Special")
                {
                    return "2 " + plants[index].specialRewards.name;
                }
                if (rewardType == "SuperSpecial")
                {
                    return "1 " + plants[index].superSpecialRewards.name;
                }
            }
        }
        return null;
    }

    public Item GetPlantRewardsItem(Plant rock, string rewardType)
    {
        for (int i = 0; i < plants.Count; i++)
        {
            int index = i;
            if (plants[index].resource.name == rock.gameObject.name.Replace("(Clone)", ""))
            {
                if (rewardType == "Normal")
                {
                    return new Item(plants[index].normalRewards);
                }
                if (rewardType == "Special")
                {
                    return new Item(plants[index].specialRewards);
                }
                if (rewardType == "SuperSpecial")
                {
                    return new Item(plants[index].superSpecialRewards);

                }
            }
        }
        return new Item();
    }

    #endregion


    public Sprite GetRewardImage(string rewardImagType)
    {
        foreach (ResourceImage resourceImg in resourceRewardImage)
        {
            if (resourceImg.resourceType == rewardImagType)
            {
                return resourceImg.spriteImage;
            }
        }
        return null;
    }

    public GameObject GetAnimalImage(string animalName, int age)
    {
        foreach (AnimalImage animImage in animalImage)
        {
            if (animImage.currentAnimal.name == animalName)
            {
                if (ScriptableAnimal.dict.TryGetValue(animImage.currentAnimal.name.GetStableHashCode(), out ScriptableAnimal itemData))
                {
                    if (age < ((ScriptableAnimal)itemData).toAdultAge)
                    {
                        int random = UnityEngine.Random.Range(0, 2);
                        if (random == 0)
                        {
                            return animImage.childImage.leftImage;
                        }
                        else
                        {
                            return animImage.childImage.rightImage;
                        }
                    }
                    else
                    {
                        int random = UnityEngine.Random.Range(0, 2);
                        if (random == 0)
                        {
                            return animImage.fatherImage.leftImage;
                        }
                        else
                        {
                            return animImage.fatherImage.rightImage;
                        }
                    }
                }

            }
        }
        return null;
    }

    public string UpgradeBuildingMessage(List<string> stringParameters, string additionalMessage)
    {
        //Level (0)
        //Coin (1)
        //Gold (2)
        //Experience (3)
        //Point Leaderboards (4)
        string returnString = string.Empty;

        if (languagesManager.defaultLanguages == "Italian")
        {

            if (stringParameters[0] == "50")
            {
                return "Congratulazioni! hai ricevuto il massimo livello per questo edificio!";
            }
            else
            {
                returnString += string.Format("Per aumentare il livello di questo edificio al livello {0} gli oggetti necessari sono : \n", (Convert.ToInt32(stringParameters[0]) + 1));
                if (stringParameters[1] != "0")
                {
                    returnString += string.Format("  -  Gemme : {0}\n", stringParameters[1]);
                }
                else
                {
                    returnString += string.Format("  -  Oro : {0}\n", stringParameters[2]);
                }
                returnString += string.Format("Otterrai {0} punti XP\n", stringParameters[3]);
                //returnString += string.Format("You will get {0} Leaderboard points\n", stringParameters[4]);
            }

            returnString += string.Format("E' necessaria l'abilita' {0} al livello {1} per aumentare il livello di questo edificio", stringParameters[5], stringParameters[0]);

            returnString += additionalMessage;
        }
        else
        {

            if (stringParameters[0] == "50")
            {
                return "Congratulations! you reach the maximum level for this building!";
            }
            else
            {
                returnString += string.Format("To Upgrade this building to level {0} the necessary stuff are : \n", (Convert.ToInt32(stringParameters[0]) + 1));
                if (stringParameters[1] != "0")
                {
                    returnString += string.Format("  -  Coins : {0}\n", stringParameters[1]);
                }
                else
                {
                    returnString += string.Format("  -  Gold : {0}\n", stringParameters[2]);
                }
                returnString += string.Format("You will get {0} XP points\n", stringParameters[3]);
                //returnString += string.Format("You will get {0} Leaderboard points\n", stringParameters[4]);
            }

            returnString += string.Format("You need ability {0} to level {1} to upgrade this building", stringParameters[5], stringParameters[0]);

            returnString += additionalMessage;
        }


        return returnString;
    }
    public string RepairBuildingMessage(List<string> stringParameters, string additionalMessage)
    {
        //Level (0)
        //Coin (1)
        //Gold (2)
        //Experience (3)
        //Point Leaderboards (4)
        string returnString = string.Empty;

        if (languagesManager.defaultLanguages == "Italian")
        {
            returnString += string.Format("Per riparare questo edificio gli oggetti necessari sono : \n", stringParameters[0], (Convert.ToInt32(stringParameters[0]) + 1));
            if (stringParameters[1] != "0")
            {
                returnString += string.Format("  -  Gemme : {0}\n", stringParameters[1]);
            }
            else
            {
                returnString += string.Format("  -  Oro : {0}\n", stringParameters[2]);
            }
            returnString += string.Format("Otterrai {0} punti XP\n", stringParameters[3]);
            //returnString += string.Format("You will get {0} Leaderboard points\n", stringParameters[4]);
            //}

            returnString += string.Format("E' necessaria l'abilita' {0} al livello {1} per riparare questo edificio", stringParameters[5], stringParameters[0]);

            returnString += additionalMessage;
        }
        else
        {
            returnString += string.Format("To Repair this building the necessary stuff are : \n", stringParameters[0], (Convert.ToInt32(stringParameters[0]) + 1));
            if (stringParameters[1] != "0")
            {
                returnString += string.Format("  -  Coins : {0}\n", stringParameters[1]);
            }
            else
            {
                returnString += string.Format("  -  Gold : {0}\n", stringParameters[2]);
            }
            returnString += string.Format("You will get {0} XP points\n", stringParameters[3]);
            //returnString += string.Format("You will get {0} Leaderboard points\n", stringParameters[4]);
            //}

            returnString += string.Format("You need ability {0} to level {1} to repair this building", stringParameters[5], stringParameters[0]);

            returnString += additionalMessage;

        }
        return returnString;
    }
    public string ClaimBuildingMessage(List<string> stringParameters, string additionalMessage)
    {
        //Level (0)
        //Coin (1)
        //Gold (2)
        //Experience (3)
        //Point Leaderboards (4)
        string returnString = string.Empty;

        if (languagesManager.defaultLanguages == "Italian")
        {
            returnString += string.Format("Per riscuotere questo edificio gli oggetti necessari sono : \n", stringParameters[0], (Convert.ToInt32(stringParameters[0]) + 1));
            if (stringParameters[1] != "0")
            {
                returnString += string.Format("  -  Gemme : {0}\n", stringParameters[1]);
            }
            else
            {
                returnString += string.Format("  -  Oro : {0}\n", stringParameters[2]);
            }
            returnString += string.Format("Otterrai {0} punti XP\n", stringParameters[3]);
            //returnString += string.Format("You will get {0} Leaderboard points\n", stringParameters[4]);

            returnString += string.Format("E' necessaria l'abilita' {0} al livello {1} per riscuotere questo edificio", stringParameters[5], stringParameters[0]);

            returnString += additionalMessage;
        }
        else
        {
            returnString += string.Format("To Claim this building the necessary stuff are : \n", stringParameters[0], (Convert.ToInt32(stringParameters[0]) + 1));
            if (stringParameters[1] != "0")
            {
                returnString += string.Format("  -  Coins : {0}\n", stringParameters[1]);
            }
            else
            {
                returnString += string.Format("  -  Gold : {0}\n", stringParameters[2]);
            }
            returnString += string.Format("You will get {0} XP points\n", stringParameters[3]);
            //returnString += string.Format("You will get {0} Leaderboard points\n", stringParameters[4]);

            returnString += string.Format("You need ability {0} to level {1} to claim this building", stringParameters[5], stringParameters[0]);

            returnString += additionalMessage;

        }
        return returnString;
    }
    public string CutBuildingMessage(List<string> stringParameters, string additionalMessage)
    {
        //Level (0)
        //Coin (1)
        //Gold (2)
        //Experience (3)
        //Point Leaderboards (4)
        string returnString = string.Empty;

        returnString += string.Format("To Halve the building timer the necessary stuff are : \n", stringParameters[0]);

        returnString += string.Format("  -  Coins : {0}\n", stringParameters[1]);
        returnString += string.Format("  -  Gold : {0}\n", stringParameters[2]);
        returnString += string.Format("You will get {0} XP points\n", stringParameters[3]);
        returnString += string.Format("You will get {0} Leaderboard points\n", stringParameters[4]);


        returnString += additionalMessage;

        return returnString;
    }

    public void HideBuilding(SpriteRenderer mainRender, GameObject textMesh, List<GameObject> objectToManage)
    {
        if (mainRender.GetComponent<Mine>())
        {
            mainRender.sprite = null;
        }
        else
        {
            mainRender.enabled = false;
        }
        SpriteRenderer[] childRenderer = mainRender.transform.GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < childRenderer.Length; i++)
        {
            int index = i;
            childRenderer[index].enabled = false;
        }
        textMesh.SetActive(false);
        if (objectToManage.Count > 0)
        {
            foreach (GameObject g in objectToManage)
            {
                g.SetActive(false);
            }
        }
    }

    public void ShowBuilding(SpriteRenderer mainRender, GameObject textMesh, List<GameObject> objectToManage)
    {
        if (mainRender.GetComponent<Mine>())
        {
            mainRender.sprite = mainRender.GetComponent<Mine>().mineImage;
        }
        else
        {
            mainRender.enabled = true;
        }
        SpriteRenderer[] childRenderer = mainRender.transform.GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < childRenderer.Length; i++)
        {
            int index = i;
            childRenderer[index].enabled = true;
        }
        textMesh.SetActive(true);
        if (objectToManage.Count > 0)
        {
            foreach (GameObject g in objectToManage)
            {
                g.SetActive(true);
            }
        }
    }


    public bool CanInteractBuilding(Building building, Player player)
    {
        if (building.guild != string.Empty)
        {
            if (player.guild.name == building.guild)
            {
                return false;
            }
            else
            {
                if (player.playerAlliance.guildAlly.Contains(building.guild))
                {
                    return false;
                }
            }
        }
        else
        {
            if (player.name == building.owner)
            {
                return false;
            }
        }
        return true;
    }

    public bool CanClaimBuilding(Building building, Player player)
    {
        if (!player) return false;

        //Premium
        if (building.guild == string.Empty && building.owner == string.Empty)
        {
            return false;
        }

        // building with guild
        if (building.guild != string.Empty)
        {
            // if un guild check
            if (player.InGuild())
            {
                // if building of my guild
                if (building.guild == player.guild.name)
                {
                    return false;
                }
                else
                {
                    // if building of my ally guild
                    if (player.playerAlliance.guildAlly.Contains(building.guild))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        else
        {
            if (building.owner == player.name)
            {
                return false;
            }
            return true;
        }
        return true;
    }

    public bool CanDoOtherActionBuilding(Building building, Player player)
    {
        if (!player) return false;

        //Premium
        if (building.guild == string.Empty && building.owner == string.Empty)
        {
            return false;
        }

        // building with guild
        if (building.guild != string.Empty)
        {

            // if un guild check
            if (player.InGuild())
            {
                // if building of my guild
                if (building.guild == player.guild.name)
                {
                    return true;
                }
                else
                {
                    // if building of my ally guild
                    if (player.playerAlliance.guildAlly.Contains(building.guild))
                    {
                        return true;
                    }
                }
            }
        }
        else
        {
            if (building.owner == player.name)
            {
                return true;
            }
        }

        return false;
    }

    public bool CanDoOtherActionFloor(ModularPiece building, Player player)
    {
        if (!player) return false;

        //Premium
        if (building.guild == string.Empty && building.owner == string.Empty)
        {
            return false;
        }

        // building with guild
        if (building.guild != string.Empty)
        {

            // if un guild check
            if (player.InGuild())
            {
                // if building of my guild
                if (building.guild == player.guild.name)
                {
                    return true;
                }
                else
                {
                    // if building of my ally guild
                    if (player.playerAlliance.guildAlly.Contains(building.guild))
                    {
                        return true;
                    }
                }
            }
        }
        else
        {
            if (building.owner == player.name)
            {
                return true;
            }
        }

        return false;
    }

    public bool CanDoOtherActionModularObject(ModularObject building, Player player)
    {
        if (!player) return false;

        //Premium
        if (building.guild == string.Empty && building.owner == string.Empty)
        {
            return false;
        }

        // building with guild
        if (building.guild != string.Empty)
        {

            // if un guild check
            if (player.InGuild())
            {
                // if building of my guild
                if (building.guild == player.guild.name)
                {
                    return true;
                }
                else
                {
                    // if building of my ally guild
                    if (player.playerAlliance.guildAlly.Contains(building.guild))
                    {
                        return true;
                    }
                }
            }
        }
        else
        {
            if (building.owner == player.name)
            {
                return true;
            }
        }

        return false;
    }


    public bool CanManageExplosiveBuilding(Building building, Player player)
    {
        if (!player) return false;

        //Premium
        if (building.guild == string.Empty && building.owner == string.Empty)
        {
            return false;
        }

        // building with guild
        if (building.guild != string.Empty)
        {
            //// i am the owner
            //if(building.owner == player.name)
            //{
            //    return true;
            //}

            // if un guild check
            if (player.InGuild())
            {
                // if building of my guild
                if (building.guild == player.guild.name)
                {
                    return true;
                }
                else
                {
                    // if building of my ally guild
                    if (player.playerAlliance.guildAlly.Contains(building.guild))
                    {
                        return true;
                    }
                }
            }
        }
        else
        {
            if (building.owner == player.name)
            {
                return true;
            }
        }

        return false;
    }

    public bool CanUseTheGate(Building building, Player player)
    {
        if (!player) return false;

        //Premium
        if (building.guild == string.Empty && building.owner == string.Empty)
        {
            return true;
        }

        // building with guild
        if (building.guild != string.Empty)
        {
            // if un guild check
            if (player.InGuild())
            {
                // if building of my guild
                if (building.guild == player.guild.name)
                {
                    return true;
                }
                else
                {
                    // if building of my ally guild
                    if (player.playerAlliance.guildAlly.Contains(building.guild))
                    {
                        return true;
                    }
                }
            }
        }
        else
        {
            if (building.owner == player.name)
            {
                return true;
            }
        }
        return false;
    }

    public bool CanEnterHome( ModularPiece piece, Player player)
    {
        if (!player) return false;

        //Premium
        if (piece.guild == string.Empty && piece.owner == string.Empty)
        {
            return true;
        }

        // building with guild
        if (piece.guild != string.Empty)
        {
            // if un guild check
            if (player.InGuild())
            {
                // if building of my guild
                if (piece.guild == player.guild.name)
                {
                    return true;
                }
                else
                {
                    // if building of my ally guild
                    if (player.playerAlliance.guildAlly.Contains(piece.guild))
                    {
                        return true;
                    }
                }
            }
        }
        else
        {
            if (piece.owner == player.name)
            {
                return true;
            }
        }
        return false;
    }

    public bool MineCanDamagePlayer(Building mine, Player player)
    {
        if (player.InGuild())
        {
            if (mine.guild != string.Empty)
            {
                if (mine.guild == player.guild.name)
                {
                    return false;
                }
                else
                {
                    if (player.playerAlliance.guildAlly.Contains(mine.guild))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (mine.owner == player.name)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        else
        {
            if (mine.name != string.Empty)
            {
                if (mine.owner != player.name)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        return true;
    }

    public bool MineCanDamageBuilding(Building mine, Building building)
    {
        if (building.guild != string.Empty)
        {
            if (mine.guild != string.Empty)
            {
                if (mine.guild == building.guild)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if (mine.owner == building.owner)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        else
        {
            if (mine.owner != string.Empty)
            {
                if (mine.owner != building.owner)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void ManageBuildingVisibility(NetworkIdentity identity, List<Collider2D> colliders, Building building)
    {
        if (identity.isServer)
        {
            if (colliders.Count <= 0)
            {
                building.isHide = true;
            }
            else
            {
                building.isHide = false;
            }
        }
    }

    public void ResetBuildingCredential(string guild, string owner)
    {
        for (int i = 0; i < BuildingManager.singleton.buildings.Count; i++)
        {
            int index = i;
            if (BuildingManager.singleton.buildings[index].owner == owner)
            {
                BuildingManager.singleton.buildings[index].guild = guild;
            }
        }
    }

    public bool HasItemToUpgrade()
    {
        for (int i = 0; i < Player.localPlayer.target.GetComponent<Building>().building.itemToUpgrade.Count; i++)
        {
            if (Player.localPlayer.InventoryCount(new Item(Player.localPlayer.target.GetComponent<Building>().building.itemToUpgrade[i].items)) < Player.localPlayer.target.GetComponent<Building>().building.itemToUpgrade[i].amount * Player.localPlayer.target.GetComponent<Building>().level)
            {
                return false;
            }
        }
        return true;
    }

    public bool HasItemToRepair()
    {
        for (int i = 0; i < Player.localPlayer.target.GetComponent<Building>().building.itemToRepair.Count; i++)
        {
            if (Player.localPlayer.InventoryCount(new Item(Player.localPlayer.target.GetComponent<Building>().building.itemToRepair[i].items)) < Player.localPlayer.target.GetComponent<Building>().building.itemToRepair[i].amount * Player.localPlayer.target.GetComponent<Building>().level)
            {
                return false;
            }
        }
        return true;
    }

    public Color hexToColor(string hex)
    {
        hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
        hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
        byte a = 255;//assume fully visible unless specified in hex
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        //Only use alpha if the string has enough characters
        if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }
        return new Color32(r, g, b, a);
    }


    public GameObject GetPlayerRank(int rankPoints)
    {
        GameObject tempObject = null;
        for (int i = 0; i < playerRank.Count; i++)
        {
            if (rankPoints >= playerRank[i].points)
            {
                tempObject = playerRank[i].rankImage;
            }
        }
        return tempObject;
    }

    public bool IsBetween(double testValue, double bound1, double bound2)
    {
        if (bound1 > bound2)
            return testValue >= bound2 && testValue <= bound1;
        return testValue >= bound1 && testValue <= bound2;
    }

    public bool IsInside(BoxCollider2D enterableCollider, BoxCollider2D enteringCollider)
    {
        Bounds enterableBounds = enterableCollider.bounds;
        Bounds enteringBounds = enteringCollider.bounds;

        Vector2 center = enteringBounds.center;
        Vector2 extents = enteringBounds.extents;
        Vector2[] enteringVerticles = new Vector2[4];

        enteringVerticles[0] = new Vector2(center.x + extents.x, center.y + extents.y);
        enteringVerticles[1] = new Vector2(center.x - extents.x, center.y + extents.y);
        enteringVerticles[2] = new Vector2(center.x + extents.x, center.y - extents.y);
        enteringVerticles[3] = new Vector2(center.x - extents.x, center.y - extents.y);

        foreach (Vector2 verticle in enteringVerticles)
        {
            if (!enterableBounds.Contains(verticle))
            {
                return false;
            }
        }
        return true;
    }

    public bool IsInsideGeneric(Collider2D enterableCollider, BoxCollider2D enteringCollider)
    {
        Bounds enterableBounds = enterableCollider.bounds;
        Bounds enteringBounds = enteringCollider.bounds;

        Vector2 center = enteringBounds.center;
        Vector2 extents = enteringBounds.extents;
        Vector2[] enteringVerticles = new Vector2[4];

        enteringVerticles[0] = new Vector2(center.x + extents.x, center.y + extents.y);
        enteringVerticles[1] = new Vector2(center.x - extents.x, center.y + extents.y);
        enteringVerticles[2] = new Vector2(center.x + extents.x, center.y - extents.y);
        enteringVerticles[3] = new Vector2(center.x - extents.x, center.y - extents.y);

        foreach (Vector2 verticle in enteringVerticles)
        {
            if (!enterableBounds.Contains(verticle))
            {
                return false;
            }
        }
        return true;
    }
}

public partial class Player
{
    [Command]
    public void CmdAddCoins(int amountOfCoins)
    {
        coins += amountOfCoins;
    }

    [Command]
    public void CmdAddGold(int amountOfGold)
    {
        gold += amountOfGold;
    }

    public int CheckInitialPosition()
    {
        agent.Warp(transform.position + new Vector3(UnityEngine.Random.Range(-5.0f, 5.0f), UnityEngine.Random.Range(-5.0f, 5.0f)));

        List<Collider2D> colliders = new List<Collider2D>();
        Physics2D.OverlapBoxAll(transform.position, new Vector2(1.0f, 1.0f), 0, GeneralManager.singleton.buildingCheckObstacle);


        return colliders.Count;
    }
}