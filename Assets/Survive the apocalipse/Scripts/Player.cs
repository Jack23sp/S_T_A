// All player logic was put into this class. We could also split it into several
// smaller components, but this would result in many GetComponent calls and a
// more complex syntax.
//
// The default Player class takes care of the basic player logic like the state
// machine and some properties like damage and defense.
//
// The Player class stores the maximum experience for each level in a simple
// array. So the maximum experience for level 1 can be found in expMax[0] and
// the maximum experience for level 2 can be found in expMax[1] and so on. The
// player's health and mana are also level dependent in most MMORPGs, hence why
// there are hpMax and mpMax arrays too. We can find out a players's max health
// in level 1 by using hpMax[0] and so on.
//
// The class also takes care of selection handling, which detects 3D world
// clicks and then targets/navigates somewhere/interacts with someone.
//
// Animations are not handled by the NetworkAnimator because it's still very
// buggy and because it can't really react to movement stops fast enough, which
// results in moonwalking. Not synchronizing animations over the network will
// also save us bandwidth
using UnityEngine;
using Mirror;
using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using AdvancedCustomizableSystem;
using CustomType;

public enum TradeStatus : byte { Free, Locked, Accepted }
public enum CraftingState : byte { None, InProgress, Success, Failed }

[Serializable]
public partial struct SkillbarEntry
{
    public string reference;
    public KeyCode hotKey;
}

[Serializable]
public partial struct EquipmentInfo
{
    public string requiredCategory;
    public SubAnimation location;
    public ScriptableItemAndAmount defaultItem;
}

[Serializable]
public partial struct ItemMallCategory
{
    public string category;
    public string categoryIta;
    public ScriptableItem[] items;
}

public class SyncDictionaryIntDouble : SyncDictionary<int, double> { }

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerChat))]
[RequireComponent(typeof(NetworkName))]
[RequireComponent(typeof(NetworkNavMeshAgentRubberbanding2D))]
[RequireComponent(typeof(PlayerAbility))]
[RequireComponent(typeof(PlayerAccuracy))]
[RequireComponent(typeof(PlayerMiss))]
[RequireComponent(typeof(PlayerAlliance))]
[RequireComponent(typeof(PlayerArmor))]
[RequireComponent(typeof(PlayerBoost))]
[RequireComponent(typeof(PlayerEmoji))]
[RequireComponent(typeof(PlayerFoodUnsanity))]
[RequireComponent(typeof(PlayerHungry))]
[RequireComponent(typeof(PlayerThirsty))]
[RequireComponent(typeof(PlayerPoisoning))]
[RequireComponent(typeof(PlayerTemperature))]
[RequireComponent(typeof(PlayerLeaderPoints))]
[RequireComponent(typeof(PlayerPremiumZoneManager))]
[RequireComponent(typeof(PlayerRadio))]
[RequireComponent(typeof(PlayerTorch))]
[RequireComponent(typeof(PlayerWeight))]
[RequireComponent(typeof(PlayerSpawnpoint))]
[RequireComponent(typeof(PlayerBuilding))]
[RequireComponent(typeof(PlayerMunitionManager))]
[RequireComponent(typeof(PlayerFired))]
[RequireComponent(typeof(PlayerMarriage))]
[RequireComponent(typeof(PlayerMove))]
[RequireComponent(typeof(PlayerFriend))]
[RequireComponent(typeof(PlayerOptions))]
[RequireComponent(typeof(PlayerBlood))]
[RequireComponent(typeof(PlayerConservative))]
[RequireComponent(typeof(PlayerElectric))]
[RequireComponent(typeof(PlayerMonsterGrab))]
[RequireComponent(typeof(PlayerCar))]
[RequireComponent(typeof(PlayerRock))]
[RequireComponent(typeof(PlayerTree))]
[RequireComponent(typeof(PlayerCreation))]
[RequireComponent(typeof(PlayerBelt))]
[RequireComponent(typeof(PlayerItemPoint))]
[RequireComponent(typeof(PlayerFootPrint))]
[RequireComponent(typeof(PlayerQuest))]
[RequireComponent(typeof(PlayerDance))]
public partial class Player : Entity
{
    [Header("Scriptable Player")]
    public ScriptableItem scriptablePlayer;

    [Header("Components")]
    public PlayerChat chat;
    public Camera avatarCamera;
    public Camera faceCamera;
    public NetworkNavMeshAgentRubberbanding2D rubberbanding;

    [Header("Text Meshes")]
    public TextMeshProUGUI nameOverlay;
    public Color nameOverlayDefaultColor = Color.white;
    public Color nameOverlayOffenderColor = Color.magenta;
    public Color nameOverlayMurdererColor = Color.red;
    public Color nameOverlayBoostColor = Color.blue;
    public Color nameOverlayPartyColor = new Color(0.341f, 0.965f, 0.702f);
    public TextMeshProUGUI guildOverlay;
    public string guildOverlayPrefix = "[";
    public string guildOverlaySuffix = "]";

    [Header("Icons")]
    public Sprite classIcon; // for character selection
    public Sprite portraitIcon; // for top left portrait

    // some meta info
    [HideInInspector] public string account = "";
    [HideInInspector] public string className = "";


    // localPlayer singleton for easier access from UI scripts etc.
    public static Player localPlayer;

    // health
    public override int healthMax
    {
        get
        {
            // calculate equipment bonus
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int equipmentBonus = 0;
            foreach (ItemSlot slot in equipment)
                if (slot.amount > 0)
                    equipmentBonus += ((EquipmentItem)slot.item.data).healthBonus;

            // calculate strength bonus (1 strength means 1% of hpMax bonus)
            int attributeBonus = Convert.ToInt32(_healthMax.Get(level));

            // base (health + buff) + equip + attributes
            return attributeBonus + equipmentBonus;
        }
    }

    // mana
    public override int manaMax
    {
        get
        {
            // calculate equipment bonus
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int equipmentBonus = 0;
            foreach (ItemSlot slot in equipment)
                if (slot.amount > 0)
                    equipmentBonus += ((EquipmentItem)slot.item.data).manaBonus;

            // calculate intelligence bonus (1 intelligence means 1% of hpMax bonus)
            int attributeBonus = Convert.ToInt32(_manaMax.Get(level));

            // base (mana + buff) + equip + attributes
            return base.manaMax + equipmentBonus;
        }
    }

    // damage
    public override int damage
    {
        get
        {
            // calculate equipment bonus
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int equipmentBonus = 0;
            foreach (ItemSlot slot in equipment)
                if (slot.amount > 0)
                    equipmentBonus += ((EquipmentItem)slot.item.data).damageBonus;

            // return base (damage + buff) + equip
            return base.damage + equipmentBonus;
        }
    }

    // defense
    public override float defense
    {
        get
        {
            // calculate equipment bonus
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            float equipmentBonus = 0;
            foreach (ItemSlot slot in equipment)
                if (slot.amount > 0)
                    equipmentBonus += ((EquipmentItem)slot.item.data).defenseBonus;

            // return base (defense + buff) + equip
            return base.defense + equipmentBonus;
        }
    }

    // block
    public override float blockChance
    {
        get
        {
            // calculate equipment bonus
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            float equipmentBonus = 0;
            foreach (ItemSlot slot in equipment)
                if (slot.amount > 0)
                    equipmentBonus += ((EquipmentItem)slot.item.data).blockChanceBonus;

            // return base (blockChance + buff) + equip
            return base.blockChance + equipmentBonus;
        }
    }

    // crit
    public override float criticalChance
    {
        get
        {
            // calculate equipment bonus
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            float equipmentBonus = 0;
            foreach (ItemSlot slot in equipment)
                if (slot.amount > 0)
                    equipmentBonus += ((EquipmentItem)slot.item.data).criticalChanceBonus;

            // return base (criticalChance + buff) + equip
            return base.criticalChance + equipmentBonus;
        }
    }

    // speed
    public override float speed
    {
        get
        {
            // mount speed if mounted, regular speed otherwise
            return activeMount != null && activeMount.health > 0 ? activeMount.speed : agent.speed;
        }
    }

    [Header("Attributes")]
    [SyncVar] public int strength = 0;
    [SyncVar] public int intelligence = 0;

    [Header("Experience")] // note: int is not enough (can have > 2 mil. easily)
    public int maxLevel = 1;
    [SyncVar, SerializeField] long _experience = 0;
    public long experience
    {
        get { return _experience; }
        set
        {
            if (value <= _experience)
            {
                // decrease
                _experience = Math.Max(value, 0);
            }
            else
            {
                // increase with level ups
                // set the new value (which might be more than expMax)
                _experience = value;

                // now see if we leveled up (possibly more than once too)
                // (can't level up if already max level)
                while (_experience >= experienceMax && level < maxLevel)
                {
                    // subtract current level's required exp, then level up
                    _experience -= experienceMax;
                    ++level;

                    // addon system hooks
                    Utils.InvokeMany(typeof(Player), this, "OnLevelUp_");
                }

                // set to expMax if there is still too much exp remaining
                if (_experience > experienceMax) _experience = experienceMax;
            }
        }
    }

    // required experience grows by 10% each level (like Runescape)
    [SerializeField] protected ExponentialLong _experienceMax = new ExponentialLong { multiplier = 100, baseValue = 1.1f };
    public long experienceMax { get { return _experienceMax.Get(level); } }

    [Header("Skill Experience")]
    [SyncVar] public long skillExperience = 0;

    [Header("Indicator")]
    public GameObject indicatorPrefab;
    [HideInInspector] public GameObject indicator;

    [Header("Inventory")]
    public int inventorySizeDefault = 30;

    public int inventorySize
    {
        get
        {
            // calculate equipment bonus
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int equipmentBonus = 0;
            foreach (ItemSlot slot in equipment)
            {
                if (slot.amount > 0 && slot.item.data is EquipmentItem)
                {
                    equipmentBonus += ((EquipmentItem)slot.item.data).additionalSlot.Get(slot.item.bagLevel);
                }
            }
            return inventorySizeDefault + equipmentBonus;
        }
    }

    public ScriptableItemAndAmount[] defaultItems;
    public KeyCode[] inventorySplitKeys = { KeyCode.LeftShift, KeyCode.RightShift };

    // item cooldowns
    // it's based on a 'cooldownCategory' that can be set in ScriptableItems.
    // -> they can use their own name for a cooldown that only applies to them
    // -> they can use a category like 'HealthPotion' for a shared cooldown
    //    amongst all health potions
    // => we use hash(category) as key to significantly reduce bandwidth!
    SyncDictionaryIntDouble itemCooldowns = new SyncDictionaryIntDouble();

    [Header("Trash")]
    [SyncVar] public ItemSlot trash;

    [Header("Equipment Info")]
    public EquipmentInfo[] equipmentInfo =
    {
        new EquipmentInfo{requiredCategory="Weapon", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Head", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Chest", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Legs", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Shield", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Shoulders", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Hands", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Feet", location=null, defaultItem=new ScriptableItemAndAmount()}
    };

    [Header("Skillbar")]
    public SkillbarEntry[] skillbar =
    {
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha1},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha2},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha3},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha4},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha5},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha6},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha7},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha8},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha9},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha0},
    };

    [Header("Quests")] // contains active and completed quests (=all)
    public int activeQuestLimit = 10;
    public SyncListQuest quests = new SyncListQuest();

    [Header("Interaction")]
    public float interactionRange = 1;
    public KeyCode targetNearestKey = KeyCode.Tab;
    public bool localPlayerClickThrough = true; // click selection goes through localplayer. feels best.
    public KeyCode cancelActionKey = KeyCode.Escape;

    [Header("PvP")]
    public BuffSkill offenderBuff;
    public BuffSkill murdererBuff;

    [Header("Trading")]
    [SyncVar] public string tradeRequestFrom = "";
    [SyncVar, HideInInspector] public TradeStatus tradeStatus = TradeStatus.Free;
    [SyncVar, HideInInspector] public long tradeOfferGold = 0;
    public SyncListInt tradeOfferItems = new SyncListInt(); // inventory indices

    [Header("Crafting")]
    public List<int> craftingIndices = Enumerable.Repeat(-1, ScriptableRecipe.recipeSize).ToList();
    [HideInInspector] public CraftingState craftingState = CraftingState.None; // client sided
    ScriptableRecipe craftingRecipe; // currently crafted recipe. cached to avoid searching ALL recipes in Craft()
    [SyncVar, HideInInspector] public double craftingTimeEnd; // double for long term precision

    [Header("Item Mall")]
    public ItemMallCategory[] itemMallCategories; // the items that can be purchased in the item mall
    [SyncVar] public long coins = 0;
    public float couponWaitSeconds = 3;

    [Header("Guild")]
    [SyncVar] public string guildInviteFrom = "";
    [SyncVar, HideInInspector] public Guild guild; // TODO SyncToOwner later
    public float guildInviteWaitSeconds = 3;

    // .party is a copy for easier reading/syncing. Use PartySystem to manage
    // parties!
    [Header("Party")]
    [SyncVar, HideInInspector] public Party party; // TODO SyncToOwner later
    [SyncVar, HideInInspector] public string partyInviteFrom = "";
    public float partyInviteWaitSeconds = 3;

    [Header("Pet")]
    [SyncVar] GameObject _activePet;
    public Pet activePet
    {
        get { return _activePet != null ? _activePet.GetComponent<Pet>() : null; }
        set { _activePet = value != null ? value.gameObject : null; }
    }
    // pet's destination should always be right next to player, not inside him
    // -> we use a helper property so we don't have to recalculate it each time
    // -> we offset the position by exactly 1 x bounds to the left because dogs
    //    are usually trained to walk on the left of the owner. looks natural.
    public Vector2 petDestination
    {
        get
        {
            Bounds bounds = collider.bounds;
            return transform.position - transform.right * bounds.size.x;
        }
    }

    // 'Mount' can't be SyncVar so we use [SyncVar] GameObject and wrap it
    [SyncVar] GameObject _activeMount;
    public Mount activeMount
    {
        get { return _activeMount != null ? _activeMount.GetComponent<Mount>() : null; }
        set { _activeMount = value != null ? value.gameObject : null; }
    }

    // when moving into attack range of a target, we always want to move a
    // little bit closer than necessary to tolerate for latency and other
    // situations where the target might have moved away a little bit already.
    [Header("Movement")]
    [Range(0.1f, 1)] public float attackToMoveRangeRatio = 0.8f;

    [Header("Death")]
    public float deathExperienceLossPercent = 0.05f;

    // some commands should have delays to avoid DDOS, too much database usage
    // or brute forcing coupons etc. we use one riskyAction timer for all.
    [SyncVar, HideInInspector] public double nextRiskyActionTime = 0; // double for long term precision

    // the next target to be set if we try to set it while casting
    // 'Entity' can't be SyncVar and NetworkIdentity causes errors when null,
    // so we use [SyncVar] GameObject and wrap it for simplicity
    [SyncVar] GameObject _nextTarget;
    public Entity nextTarget
    {
        get { return _nextTarget != null ? _nextTarget.GetComponent<Entity>() : null; }
        set { _nextTarget = value != null ? value.gameObject : null; }
    }

    // cache players to save lots of computations
    // (otherwise we'd have to iterate NetworkServer.objects all the time)
    // => on server: all online players
    // => on client: all observed players
    public static Dictionary<string, Player> onlinePlayers = new Dictionary<string, Player>();

    // first allowed logout time after combat
    public double allowedLogoutTime => lastCombatTime + ((NetworkManagerMMO)NetworkManager.singleton).combatLogoutDelay;
    public double remainingLogoutTime => NetworkTime.time < allowedLogoutTime ? (allowedLogoutTime - NetworkTime.time) : 0;

    // helper variable to remember which skill to use when we walked close enough
    int useSkillWhenCloser = -1;
    bool settedNameColor;

    // Camera.main calls FindObjectWithTag each time. cache it!
    [HideInInspector] public Camera cam;

    public GameObject levelUp;

    double seconds = 0;

    [HideInInspector] public NavMeshObstacleSimulator navMeshObstacleSimulator;

    public RawImage playerBody;

    [SyncVar]
    public ItemSlot petItem;

    public GameObject prefabPreview;

    public RuntimeAnimatorController prevAnimator;
    [HideInInspector] public MazzleFlashSpawner mazzleFlashSpawner;

    public UIVisibleHealthArmor UIVisibleHealthArmor;

    public Transform enemyDetector;

    // networkbehaviour ////////////////////////////////////////////////////////
    protected override void Awake()
    {
        // cache base components
        base.Awake();

        // addon system hooks
        Utils.InvokeMany(typeof(Player), this, "Awake_");
    }

    public override void OnStartLocalPlayer()
    {
        // set singleton
        localPlayer = this;

        // find main camera
        // only for local player. 'Camera.main' is expensive (FindObjectWithTag)
        cam = Camera.main;

        // make camera follow the local player. we don't just set .parent
        // because the player might be destroyed, but the camera never should be
        cam.GetComponent<CameraMMO2D>().target = transform;
        //GameObject.FindWithTag("MinimapCamera").GetComponent<CopyPosition>().target = transform;
        if (avatarCamera) avatarCamera.enabled = true; // avatar camera for local player

        // load skillbar after player data was loaded
        LoadSkillbar();

        if (!playerOptions.blockSound)
        {
            SoundManager.singleton.SetAmbientVolume(0, 0.1f);
            SoundManager.singleton.SetWeatherVolume(0, 0.1f);
            SoundManager.singleton.SetMusic();
            SoundManager.singleton.SetWeatherMusic();
        }
        else
        {
            SoundManager.singleton.SetAmbientVolume(1, 0.1f);
            SoundManager.singleton.SetWeatherVolume(1, 0.1f);
            SoundManager.singleton.SetMusic();
            SoundManager.singleton.SetWeatherMusic();
        }

        // addon system hooks
        Utils.InvokeMany(typeof(Player), this, "OnStartLocalPlayer_");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // setup synclist callbacks on client. no need to update and show and
        // animate equipment on server
        equipment.Callback += OnEquipmentChanged;

        // refresh all locations once (on synclist changed won't be called
        // for initial lists)
        // -> needs to happen before ProximityChecker's initial SetVis call,
        //    otherwise we get a hidden character with visible equipment
        //    (hence OnStartClient and not Start)
        for (int i = 0; i < equipment.Count; ++i)
            RefreshLocation(i);

        playerItemEquipment.CheckFirstWeapon();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        // initialize trade item indices
        for (int i = 0; i < 6; ++i) tradeOfferItems.Add(-1);

        //InvokeRepeating(nameof(ProcessCoinOrders), 5, 5);

        // addon system hooks
        Utils.InvokeMany(typeof(Player), this, "OnStartServer_");
    }

    protected override void Start()
    {
        if (avatarCamera.activeTexture == null)
        {
            var tex = new RenderTexture(GeneralManager.singleton.sampleTexture);
            avatarCamera.targetTexture = tex;
            playerBody.texture = avatarCamera.activeTexture;

        }
        if (faceCamera.activeTexture == null)
        {
            var tex = new RenderTexture(GeneralManager.singleton.sampleTexture);
            faceCamera.targetTexture = tex;
        }
        // do nothing if not spawned (=for character selection previews)
        if (!isServer && !isClient) return;

        base.Start();
        onlinePlayers[name] = this;

        if (playerCreation.sex == 0)
            prefabPreview = Instantiate(GeneralManager.singleton.malePrefab, transform.position, transform.rotation);
        else
            prefabPreview = Instantiate(GeneralManager.singleton.femalePrefab, transform.position, transform.rotation);

        prefabPreview.transform.SetParent(playerCreation.dummyPresentation.transform);
        prefabPreview.transform.localPosition = new Vector3(7.960999f, -1.264f, 7.457145f);
        prefabPreview.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

        if (!playerCreation.characterCustomization) playerCreation.characterCustomization = prefabPreview.GetComponent<CharacterCustomization>();
        playerCreation.characterCustomization.ChangeTag(this, avatarCamera);


        if (isServer)
            SetGuildOnline(true);

        if (generalPart == null) generalPart = new CustomType.GeneralPart();

        for (int i = 0; i < equipment.Count; i++)
        {
            if (equipment[i].amount > 0)
                RefreshLocation(i);
        }

        ChangeAnimatorAndWeapon();
        InvokeRepeating(nameof(CheckJoystick), 0.0f, 1.0f);
        InvokeRepeating(nameof(UpdateOverlays), 0.0f, 2.0f);

    }

    public void CheckJoystick ()
    {
        if (isClient)
        {
            useJoystick = Application.isMobilePlatform;
            if (useJoystick == false)
            {
                if (GeneralManager.singleton)
                    joystick = GeneralManager.singleton.joystick;
                else
                    return;
                if (joystick)
                {
                    joystick.gameObject.SetActive(useJoystick);
                    CancelInvoke(nameof(CheckJoystick));
                }
            }
            else
            {
                if (GeneralManager.singleton)
                    joystick = GeneralManager.singleton.joystick;
                else
                    return;
                if (joystick)
                {
                    joystick.gameObject.SetActive(useJoystick);
                    CancelInvoke(nameof(CheckJoystick));
                }
            }
        }
    }

    void LateUpdate()
    {
        // pass parameters to animation state machine
        // => passing the states directly is the most reliable way to avoid all
        //    kinds of glitches like movement sliding, attack twitching, etc.
        // => make sure to import all looping animations like idle/run/attack
        //    with 'loop time' enabled, otherwise the client might only play it
        //    once
        // => MOVING state is set to local IsMovement result directly. otherwise
        //    we would see animation latencies for rubberband movement if we
        //    have to wait for MOVING state to be received from the server
        // => MOVING checks if !CASTING because there is a case in UpdateMOVING
        //    -> SkillRequest where we still slide to the final position (which
        //    is good), but we should show the casting animation then.
        // => skill names are assumed to be boolean parameters in animator
        //    so we don't need to worry about an animation number etc.
        if (isClient) // no need for animations on the server
        {
            //if (PlayerValueCatcher.singleton.AnimationManagerMOVING() != null)
            //    animator.SetBool("isMoving", IsMoving() && state != "CASTING" && !IsMounted());
            //if (PlayerValueCatcher.singleton.AnimationManagerCASTING() != null)
            //    animator.SetBool("CASTING", state == "CASTING");

            if (playerMove.bodyPlayer)
            {
                AnimationManagerMOVING(this);
                AnimationManagerRUNNING(this);


                AnimationManagerCASTING(this);

                foreach (Skill skill in skills)
                    if (skill.level > 0 && !(skill.data is PassiveSkill))
                        skill.CastTimeRemainingAnimator(this);

                //PlayerValueCatcher.singleton.SetAnimation();
                //if (PlayerValueCatcher.singleton.AnimationManagerSTUNNED() != null)
                //    animator.SetBool("STUNNED", state == "STUNNED");
                //AnimationManagerSTUNNED(this);

                //if (PlayerValueCatcher.singleton.AnimationManagerDEAD() != null)
                //    animator.SetBool("DEAD", state == "DEAD");
                AnimationManagerDEAD(this);
                //if (PlayerValueCatcher.singleton.AnimationManagerMOVEX() > -2.0f)
                //animator.SetFloat("moveX", lookDirection.x);
                //if (PlayerValueCatcher.singleton.AnimationManagerMOVEY() > -2.0f)
                //animator.SetFloat("LookY", lookDirection.y);
                AnimationManagerMOVEX(this);
                AnimationManagerMOVEY(this);
            }
            if (spriteRenderer) playerBody.canvas.sortingOrder = spriteRenderer.sortingOrder;
        }


        // addon system hooks
        //Utils.InvokeMany(typeof(Player), this, "LateUpdate_");
    }

    void OnDestroy()
    {
        // do nothing if not spawned (=for character selection previews)
        if (!isServerObject && !isClient) return;

        // Unity bug: isServerObject is false when called in host mode. only true when
        // called in dedicated mode. so we need a workaround:
        if (NetworkServer.active) // isServerObject
        {
            // leave party (if any)
            if (InParty())
            {
                // dismiss if master, leave otherwise
                if (party.master == name)
                    PartyDismiss();
                else
                    PartyLeave();
            }

            // notify guild members that we are offline
            SetGuildOnline(false);
        }

        if (isLocalPlayer) // requires at least Unity 5.5.1 bugfix to work
        {
            Destroy(indicator);
            SaveSkillbar();
            localPlayer = null;
        }

        onlinePlayers.Remove(name);

        // addon system hooks
        Utils.InvokeMany(typeof(Player), this, "OnDestroy_");
    }

    // finite state machine events - status based //////////////////////////////
    // status based events
    bool EventDied()
    {
        return health == 0;
    }

    bool EventTargetDisappeared()
    {
        return target == null;
    }

    bool EventTargetDied()
    {
        return target != null && target.health == 0;
    }

    bool EventSkillRequest()
    {
        return 0 <= currentSkill && currentSkill < skills.Count;
    }

    bool EventSkillFinished()
    {
        return 0 <= currentSkill && currentSkill < skills.Count &&
               skills[currentSkill].CastTimeRemaining() == 0;
    }

    bool EventMoveStart()
    {
        return state != "MOVING" && IsMoving(); // only fire when started moving
    }

    bool EventMoveEnd()
    {
        return state == "MOVING" && !IsMoving(); // only fire when stopped moving
    }

    bool EventTradeStarted()
    {
        // did someone request a trade? and did we request a trade with him too?
        Player player = FindPlayerFromTradeInvitation();
        return player != null && player.tradeRequestFrom == name;
    }

    bool EventTradeDone()
    {
        // trade canceled or finished?
        return state == "TRADING" && tradeRequestFrom == "";
    }

    bool craftingRequested;
    bool EventCraftingStarted()
    {
        bool result = craftingRequested;
        craftingRequested = false;
        return result;
    }

    bool EventCraftingDone()
    {
        return state == "CRAFTING" && NetworkTime.time > craftingTimeEnd;
    }

    bool EventStunned()
    {
        return NetworkTime.time <= stunTimeEnd;
    }

    // finite state machine events - command based /////////////////////////////
    // client calls command, command sets a flag, event reads and resets it
    // => we use a set so that we don't get ultra long queues etc.
    // => we use set.Return to read and clear values
    HashSet<string> cmdEvents = new HashSet<string>();

    [Command]
    public void CmdRespawn() { cmdEvents.Add("Respawn"); }
    bool EventRespawn() { return cmdEvents.Remove("Respawn"); }

    [Command]
    public void CmdCancelAction() { cmdEvents.Add("CancelAction"); }
    bool EventCancelAction() { return cmdEvents.Remove("CancelAction"); }

    // finite state machine - server ///////////////////////////////////////////
    [Server]
    string UpdateServer_IDLE()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied())
        {
            // we died.
            OnDeath();
            return "DEAD";
        }
        if (EventStunned())
        {
            rubberbanding.ResetMovement();
            return "STUNNED";
        }
        if (EventCancelAction())
        {
            // the only thing that we can cancel is the target
            target = null;
            return "IDLE";
        }
        if (EventTradeStarted())
        {
            // cancel casting (if any), set target, go to trading
            CancelCastSkill(); // just in case
            target = FindPlayerFromTradeInvitation();
            return "TRADING";
        }
        if (EventCraftingStarted())
        {
            // cancel casting (if any), go to crafting
            CancelCastSkill(); // just in case
            return "CRAFTING";
        }
        if (EventMoveStart())
        {
            // cancel casting (if any)
            CancelCastSkill();
            return "MOVING";
        }
        if (EventSkillRequest())
        {
            // don't cast while mounted
            // (no MOUNTED state because we'd need MOUNTED_STUNNED, etc. too)
            if (!IsMounted())
            {
                // user wants to cast a skill.
                // check self (alive, mana, weapon etc.) and target and distance
                Skill skill = skills[currentSkill];
                // nextTarget = target; // return to this one after any corrections by CastCheckTarget
                Vector2 destination;
                if (CastCheckSelf(skill) && CastCheckTarget(skill) && CastCheckDistance(skill, out destination))
                {
                    // start casting and cancel movement in any case
                    // (player might move into attack range * 0.8 but as soon as we
                    //  are close enough to cast, we fully commit to the cast.)
                    rubberbanding.ResetMovement();
                    StartCastSkill(skill);
                    return "CASTING";
                }
                else
                {
                    // checks failed. reset the attempted current skill.
                    currentSkill = -1;
                    nextTarget = null; // nevermind, clear again (otherwise it's shown in UITarget)
                    return "IDLE";
                }
            }
        }
        if (EventSkillFinished()) { } // don't care
        if (EventMoveEnd()) { } // don't care
        if (EventTradeDone()) { } // don't care
        if (EventCraftingDone()) { } // don't care
        if (EventRespawn()) { } // don't care
        if (EventTargetDied()) { } // don't care
        if (EventTargetDisappeared()) { } // don't care

        return "IDLE"; // nothing interesting happened
    }

    [Server]
    string UpdateServer_MOVING()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied())
        {
            // we died.
            OnDeath();
            return "DEAD";
        }
        if (EventStunned())
        {
            rubberbanding.ResetMovement();
            return "STUNNED";
        }
        if (EventMoveEnd())
        {
            // finished moving. do whatever we did before.
            return "IDLE";
        }
        if (EventCancelAction())
        {
            // cancel casting (if any) and stop moving
            CancelCastSkill();
            //rubberbanding.ResetMovement(); <- done locally. doing it here would reset localplayer to the slightly behind server position otherwise
            return "IDLE";
        }
        if (EventTradeStarted())
        {
            // cancel casting (if any), stop moving, set target, go to trading
            CancelCastSkill();
            rubberbanding.ResetMovement();
            target = FindPlayerFromTradeInvitation();
            return "TRADING";
        }
        if (EventCraftingStarted())
        {
            // cancel casting (if any), stop moving, go to crafting
            CancelCastSkill();
            rubberbanding.ResetMovement();
            return "CRAFTING";
        }
        // SPECIAL CASE: Skill Request while doing rubberband movement
        // -> we don't really need to react to it
        // -> we could just wait for move to end, then react to request in IDLE
        // -> BUT player position on server always lags behind in rubberband movement
        // -> SO there would be a noticeable delay before we start to cast
        //
        // SOLUTION:
        // -> start casting as soon as we are in range
        // -> BUT don't ResetMovement. instead let it slide to the final position
        //    while already starting to cast
        // -> NavMeshAgentRubberbanding won't accept new positions while casting
        //    anyway, so this is fine
        if (EventSkillRequest())
        {
            // don't cast while mounted
            // (no MOUNTED state because we'd need MOUNTED_STUNNED, etc. too)
            if (!IsMounted())
            {
                Vector2 destination;
                Skill skill = skills[currentSkill];
                if (CastCheckSelf(skill) && CastCheckTarget(skill) && CastCheckDistance(skill, out destination))
                {
                    //Debug.Log("MOVING->EventSkillRequest: early cast started while sliding to destination...");
                    // rubberbanding.ResetMovement(); <- DO NOT DO THIS.
                    StartCastSkill(skill);
                    return "CASTING";
                }
            }
        }
        if (EventMoveStart()) { } // don't care
        if (EventSkillFinished()) { } // don't care
        if (EventTradeDone()) { } // don't care
        if (EventCraftingDone()) { } // don't care
        if (EventRespawn()) { } // don't care
        if (EventTargetDied()) { } // don't care
        if (EventTargetDisappeared()) { } // don't care

        return "MOVING"; // nothing interesting happened
    }

    //void UseNextTargetIfAny()
    //{
    //    // use next target if the user tried to target another while casting
    //    // (target is locked while casting so skill isn't applied to an invalid
    //    //  target accidentally)
    //    if (nextTarget != null)
    //    {
    //        target = nextTarget;
    //        nextTarget = null;
    //    }
    //}

    [Server]
    string UpdateServer_CASTING()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        //
        // IMPORTANT: nextTarget might have been set while casting, so make sure
        // to handle it in any case here. it should definitely be null again
        // after casting was finished.
        // => this way we can reliably display nextTarget on the client if it's
        //    != null, so that UITarget always shows nextTarget>target
        //    (this just feels better)
        if (EventDied())
        {
            // we died.
            OnDeath();
            //UseNextTargetIfAny(); // if user selected a new target while casting
            return "DEAD";
        }
        if (EventStunned())
        {
            CancelCastSkill();
            rubberbanding.ResetMovement();
            return "STUNNED";
        }
        if (EventMoveStart())
        {
            // we do NOT cancel the cast if the player moved, and here is why:
            // * local player might move into cast range and then try to cast.
            // * server then receives the Cmd, goes to CASTING state, then
            //   receives one of the last movement updates from the local player
            //   which would cause EventMoveStart and cancel the cast.
            // * this is the price for rubberband movement.
            // => if the player wants to cast and got close enough, then we have
            //    to fully commit to it. there is no more way out except via
            //    cancel action. any movement in here is to be rejected.
            //    (many popular MMOs have the same behaviour too)
            //

            // we do NOT reset movement either. allow sliding to final position.
            // (NavMeshAgentRubberbanding doesn't accept new ones while CASTING)
            //rubberbanding.ResetMovement(); <- DO NOT DO THIS

            // we do NOT return "CASTING". EventMoveStart would constantly fire
            // while moving for skills that allow movement. hence we would
            // always return "CASTING" here and never get to the castfinished
            // code below.
            //return "CASTING";
        }
        if (EventCancelAction())
        {
            // cancel casting
            CancelCastSkill();
            //UseNextTargetIfAny(); // if user selected a new target while casting
            return "IDLE";
        }
        if (EventTradeStarted())
        {
            // cancel casting (if any), stop moving, set target, go to trading
            CancelCastSkill();
            rubberbanding.ResetMovement();

            // set target to trade target instead of next target (clear that)
            target = FindPlayerFromTradeInvitation();
            nextTarget = null;
            return "TRADING";
        }
        if (EventTargetDisappeared())
        {
            // cancel if the target matters for this skill
            if (skills[currentSkill].cancelCastIfTargetDied)
            {
                CancelCastSkill();
                //UseNextTargetIfAny(); // if user selected a new target while casting
                return "IDLE";
            }
        }
        if (EventTargetDied())
        {
            // cancel if the target matters for this skill
            if (skills[currentSkill].cancelCastIfTargetDied)
            {
                CancelCastSkill();
                //UseNextTargetIfAny(); // if user selected a new target while casting
                return "IDLE";
            }
        }
        if (EventSkillFinished())
        {
            // apply the skill after casting is finished
            // note: we don't check the distance again. it's more fun if players
            //       still cast the skill if the target ran a few steps away
            Skill skill = skills[currentSkill];

            // apply the skill on the target
            FinishCastSkill(skill);

            // clear current skill for now
            currentSkill = -1;

            // use next target if the user tried to target another while casting
            //UseNextTargetIfAny();

            // go back to IDLE
            return "IDLE";
        }
        if (EventMoveEnd()) { } // don't care
        if (EventTradeDone()) { } // don't care
        if (EventCraftingStarted()) { } // don't care
        if (EventCraftingDone()) { } // don't care
        if (EventRespawn()) { } // don't care
        if (EventSkillRequest()) { } // don't care

        return "CASTING"; // nothing interesting happened
    }

    [Server]
    string UpdateServer_STUNNED()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied())
        {
            // we died.
            OnDeath();
            return "DEAD";
        }
        if (EventStunned())
        {
            return "STUNNED";
        }

        // go back to idle if we aren't stunned anymore and process all new
        // events there too
        return "IDLE";
    }

    [Server]
    string UpdateServer_TRADING()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied())
        {
            // we died, stop trading. other guy will receive targetdied event.
            OnDeath();
            TradeCleanup();
            return "DEAD";
        }
        if (EventStunned())
        {
            // stop trading
            CancelCastSkill();
            rubberbanding.ResetMovement();
            TradeCleanup();
            return "STUNNED";
        }
        if (EventMoveStart())
        {
            // reject movement while trading
            rubberbanding.ResetMovement();
            return "TRADING";
        }
        if (EventCancelAction())
        {
            // stop trading
            TradeCleanup();
            return "IDLE";
        }
        if (EventTargetDisappeared())
        {
            // target disconnected, stop trading
            TradeCleanup();
            return "IDLE";
        }
        if (EventTargetDied())
        {
            // target died, stop trading
            TradeCleanup();
            return "IDLE";
        }
        if (EventTradeDone())
        {
            // someone canceled or we finished the trade. stop trading
            TradeCleanup();
            return "IDLE";
        }
        if (EventMoveEnd()) { } // don't care
        if (EventSkillFinished()) { } // don't care
        if (EventCraftingStarted()) { } // don't care
        if (EventCraftingDone()) { } // don't care
        if (EventRespawn()) { } // don't care
        if (EventTradeStarted()) { } // don't care
        if (EventSkillRequest()) { } // don't care

        return "TRADING"; // nothing interesting happened
    }

    [Server]
    string UpdateServer_CRAFTING()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied())
        {
            // we died, stop crafting
            OnDeath();
            return "DEAD";
        }
        if (EventStunned())
        {
            // stop crafting
            rubberbanding.ResetMovement();
            return "STUNNED";
        }
        if (EventMoveStart())
        {
            // reject movement while crafting
            rubberbanding.ResetMovement();
            return "CRAFTING";
        }
        if (EventCraftingDone())
        {
            // finish crafting
            Craft();
            return "IDLE";
        }
        if (EventCancelAction()) { } // don't care. user pressed craft, we craft.
        if (EventTargetDisappeared()) { } // don't care
        if (EventTargetDied()) { } // don't care
        if (EventMoveEnd()) { } // don't care
        if (EventSkillFinished()) { } // don't care
        if (EventRespawn()) { } // don't care
        if (EventTradeStarted()) { } // don't care
        if (EventTradeDone()) { } // don't care
        if (EventCraftingStarted()) { } // don't care
        if (EventSkillRequest()) { } // don't care

        return "CRAFTING"; // nothing interesting happened
    }

    [Server]
    string UpdateServer_DEAD()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventRespawn())
        {
            // revive to closest spawn, with 50% health, then go to idle
            Transform start = NetworkManagerMMO.GetNearestStartPosition(transform.position);
            agent.Warp(start.position); // recommended over transform.position
            Revive(0.5f);
            return "IDLE";
        }
        if (EventMoveStart())
        {
            // this should never happen, rubberband should prevent from moving
            // while dead.
            Debug.LogWarning("Player " + name + " moved while dead. This should not happen.");
            return "DEAD";
        }
        if (EventMoveEnd()) { } // don't care
        if (EventSkillFinished()) { } // don't care
        if (EventDied()) { } // don't care
        if (EventCancelAction()) { } // don't care
        if (EventTradeStarted()) { } // don't care
        if (EventTradeDone()) { } // don't care
        if (EventCraftingStarted()) { } // don't care
        if (EventCraftingDone()) { } // don't care
        if (EventTargetDisappeared()) { } // don't care
        if (EventTargetDied()) { } // don't care
        if (EventSkillRequest()) { } // don't care

        return "DEAD"; // nothing interesting happened
    }

    [Server]
    protected override string UpdateServer()
    {
        //if (!navMeshObstacleSimulator.canMove)
        //{
        //    rubberbanding.ResetMovement();
        //}

        if (state == "IDLE") return UpdateServer_IDLE();
        if (state == "MOVING") return UpdateServer_MOVING();
        if (state == "CASTING") return UpdateServer_CASTING();
        if (state == "STUNNED") return UpdateServer_STUNNED();
        if (state == "TRADING") return UpdateServer_TRADING();
        if (state == "CRAFTING") return UpdateServer_CRAFTING();
        if (state == "DEAD") return UpdateServer_DEAD();

        Debug.LogError("invalid state:" + state);
        return "IDLE";
    }

    // finite state machine - client ///////////////////////////////////////////
    [Client]
    protected override void UpdateClient()
    {
        if (state == "IDLE" || state == "MOVING")
        {
            if (isLocalPlayer)
            {
                // simply accept input
                SelectionHandling();
                WASDHandling();
                //TargetNearest();

                // cancel action if escape key was pressed
                if (Input.GetKeyDown(cancelActionKey))
                {
                    agent.ResetPath(); // reset locally because we use rubberband movement
                    CmdCancelAction();
                }

                // trying to cast a skill on a monster that wasn't in range?
                // then check if we walked into attack range by now
                if (useSkillWhenCloser != -1)
                {
                    // can we still attack the target? maybe it was switched.
                    if (CanAttack(target))
                    {
                        // in range already?
                        // -> we don't use CastCheckDistance because we want to
                        // move a bit closer (attackToMoveRangeRatio)
                        float range = skills[useSkillWhenCloser].castRange * attackToMoveRangeRatio;
                        if (Utils.ClosestDistance(collider, target.collider) <= range)
                        {
                            // then stop moving and start attacking
                            CmdUseSkill(useSkillWhenCloser, lookDirection);

                            // reset
                            useSkillWhenCloser = -1;
                        }
                        // otherwise keep walking there. the target might move
                        // around or run away, so we need to keep adjusting the
                        // destination all the time
                        else
                        {
                            //Debug.Log("walking closer to target...");
                            agent.stoppingDistance = range;
                            //if (navMeshObstacleSimulator.canMove) agent.destination = target.collider.ClosestPointOnBounds(transform.position);

                            agent.destination = target.collider.ClosestPointOnBounds(transform.position) - new Vector2(range, range);
                        }
                    }
                    // otherwise reset
                    else useSkillWhenCloser = -1;
                }
            }
        }
        else if (state == "CASTING")
        {
            if (isLocalPlayer)
            {
                // simply accept input and reset any client sided movement
                SelectionHandling();
                WASDHandling(); // still call this to set pendingVelocity for after cast
                //TargetNearest();
                agent.ResetMovement();
                // cancel action if escape key was pressed
                if (Input.GetKeyDown(cancelActionKey)) CmdCancelAction();
            }
        }
        else if (state == "STUNNED")
        {
            if (isLocalPlayer)
            {
                // simply accept input and reset any client sided movement
                SelectionHandling();
                //TargetNearest();
                agent.ResetMovement();
                // cancel action if escape key was pressed
                if (Input.GetKeyDown(cancelActionKey)) CmdCancelAction();
            }
        }
        else if (state == "TRADING") { }
        else if (state == "CRAFTING") { }
        else if (state == "DEAD") { }
        else Debug.LogError("invalid state:" + state);
    }

    private TimeSpan difference;

    // overlays ////////////////////////////////////////////////////////////////
    protected override void UpdateOverlays()
    {
        base.UpdateOverlays();

        if (nameOverlay != null)
        {
            // only players need to copy names to name overlay. it never changes
            // for monsters / npcs.
            nameOverlay.text = "[" + level + "]  " + name;
            if (Player.localPlayer && !settedNameColor)
            {
                nameOverlay.color = Color.white;
                settedNameColor = true;
            }
            else if (!Player.localPlayer && !settedNameColor)
            {
                nameOverlay.color = Color.black;
                settedNameColor = true;
            }

            // find local player (null while in character selection)
            if (localPlayer != null)
            {
                seconds = 0;
                if (playerBoost.networkBoost.Count > 0)
                {
                    if (!string.IsNullOrEmpty(playerBoost.networkBoost[0].hiddenIslandTimer))
                    {
                        difference = DateTime.Parse(playerBoost.networkBoost[0].hiddenIslandTimer.ToString()) - System.DateTime.Now;
                        seconds = difference.TotalSeconds;
                    }
                    else
                        seconds = 0;
                }

                // member of the same party
                else if (localPlayer.InParty() && localPlayer.party.Contains(name))
                    nameOverlay.color = nameOverlayPartyColor;
                // otherwise default
                else if (playerBoost && playerBoost.networkBoost.Count > 0 && seconds > 0)
                    nameOverlay.color = nameOverlayBoostColor;
                else
                    nameOverlay.color = Color.white;
            }
        }
        if (guildOverlay != null)
            guildOverlay.text = InGuild() && playerCar._car == null ? guildOverlayPrefix + guild.name + guildOverlaySuffix : "";
    }

    // skill finished event & pending actions //////////////////////////////////
    // pending actions while casting. to be applied after cast.
    int pendingSkill = -1;
    Vector2 pendingDestination;
    bool pendingDestinationValid;
    Vector3 pendingVelocity;
    bool pendingVelocityValid;

    // client event when skill cast finished on server
    // -> useful for follow up attacks etc.
    //    (doing those on server won't really work because the target might have
    //     moved, in which case we need to follow, which we need to do on the
    //     client)
    [Client]
    void OnSkillCastFinished(Skill skill)
    {
        if (!isLocalPlayer) return;

        // tried to click move somewhere?
        if (pendingDestinationValid)
        {
            agent.stoppingDistance = 0;
            agent.destination = pendingDestination;
        }
        // tried to wasd move somewhere?
        else if (pendingVelocityValid)
        {
            agent.velocity = pendingVelocity;
        }
        // user pressed another skill button?
        else if (pendingSkill != -1)
        {
            TryUseSkill(pendingSkill, true);
        }
        // otherwise do follow up attack if no interruptions happened
        else if (skill.followupDefaultAttack)
        {
            TryUseSkill(0, true);
        }

        // clear pending actions in any case
        pendingSkill = -1;
        pendingDestinationValid = false;
        pendingVelocityValid = false;
    }

    // attributes //////////////////////////////////////////////////////////////
    public static int AttributesSpendablePerLevel = 2;

    public int AttributesSpendable()
    {
        // calculate the amount of attribute points that can still be spent
        // -> 'AttributesSpendablePerLevel' points per level
        // -> we don't need to store the points in an extra variable, we can
        //    simply decrease the attribute points spent from the level
        return (level * AttributesSpendablePerLevel);
    }

    [Command]
    public void CmdIncreaseStrength()
    {
        // validate
        if (health > 0 && AttributesSpendable() > 0) ++strength;
    }

    [Command]
    public void CmdIncreaseIntelligence()
    {
        // validate
        if (health > 0 && AttributesSpendable() > 0) ++intelligence;
    }

    // combat //////////////////////////////////////////////////////////////////
    // helper function to calculate the experience rewards for sharing parties
    public static long CalculatePartyExperienceShare(long total, int memberCount, float bonusPercentagePerMember, int memberLevel, int killedLevel)
    {
        // bonus percentage based on how many members there are
        float bonusPercentage = (memberCount - 1) * bonusPercentagePerMember;

        // calculate the share via ceil, so that uneven numbers still result in
        // at least 'total' in the end. for example:
        //   4/2=2 (good)
        //   5/2=2 (bad. 1 point got lost)
        //   ceil(5/(float)2) = 3 (good!)
        long share = (long)Mathf.Ceil(total / (float)memberCount);

        // balance experience reward for the receiver's level. this is important
        // to avoid crazy power leveling where a level 1 hero would get a LOT of
        // level ups if his friend kills a level 100 monster once.
        long balanced = BalanceExpReward(share, memberLevel, killedLevel);
        long bonus = Convert.ToInt64(balanced * bonusPercentage);

        return balanced + bonus;
    }

    // experience //////////////////////////////////////////////////////////////
    public float ExperiencePercent()
    {
        return (experience != 0 && experienceMax != 0) ? (float)experience / (float)experienceMax : 0;
    }

    // players gain exp depending on their level. if a player has a lower level
    // than the monster, then he gains more exp (up to 100% more) and if he has
    // a higher level, then he gains less exp (up to 100% less)
    // -> test with monster level 20 and expreward of 100:
    //   BalanceExpReward( 1, 20, 100)); => 200
    //   BalanceExpReward( 9, 20, 100)); => 200
    //   BalanceExpReward(10, 20, 100)); => 200
    //   BalanceExpReward(11, 20, 100)); => 190
    //   BalanceExpReward(12, 20, 100)); => 180
    //   BalanceExpReward(13, 20, 100)); => 170
    //   BalanceExpReward(14, 20, 100)); => 160
    //   BalanceExpReward(15, 20, 100)); => 150
    //   BalanceExpReward(16, 20, 100)); => 140
    //   BalanceExpReward(17, 20, 100)); => 130
    //   BalanceExpReward(18, 20, 100)); => 120
    //   BalanceExpReward(19, 20, 100)); => 110
    //   BalanceExpReward(20, 20, 100)); => 100
    //   BalanceExpReward(21, 20, 100)); =>  90
    //   BalanceExpReward(22, 20, 100)); =>  80
    //   BalanceExpReward(23, 20, 100)); =>  70
    //   BalanceExpReward(24, 20, 100)); =>  60
    //   BalanceExpReward(25, 20, 100)); =>  50
    //   BalanceExpReward(26, 20, 100)); =>  40
    //   BalanceExpReward(27, 20, 100)); =>  30
    //   BalanceExpReward(28, 20, 100)); =>  20
    //   BalanceExpReward(29, 20, 100)); =>  10
    //   BalanceExpReward(30, 20, 100)); =>   0
    //   BalanceExpReward(31, 20, 100)); =>   0
    public static long BalanceExpReward(long reward, int attackerLevel, int victimLevel)
    {
        int levelDiff = Mathf.Clamp(victimLevel - attackerLevel, -20, 20);
        float multiplier = 1 + levelDiff * 0.1f;
        return Convert.ToInt64(reward * multiplier);
    }

    // aggro ///////////////////////////////////////////////////////////////////
    // this function is called by entities that attack us
    [ServerCallback]
    public override void OnAggro(Entity entity)
    {
        // forward to pet if it's supposed to defend us
        if (activePet != null && activePet.defendOwner)
            activePet.OnAggro(entity);
    }

    // death ///////////////////////////////////////////////////////////////////
    protected override void OnDeath()
    {
        // take care of entity stuff
        base.OnDeath();
        string message = string.Empty;
        // rubberbanding needs a custom reset
        rubberbanding.ResetMovement();

        // lose experience
        long loss = Convert.ToInt64(experienceMax * deathExperienceLossPercent);
        experience -= loss;

        // send an info chat message
        if(GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
        {
            message = "Sei morto e hai perso " + loss + " esperienza.";

        }
        else
        {
            message = "You died and lost " + loss + " experience.";

        }
        chat.TargetMsgInfo(message);

        // addon system hooks
        Utils.InvokeMany(typeof(Player), this, "OnDeath_");
    }

    // loot ////////////////////////////////////////////////////////////////////
    [Command]
    public void CmdTakeLootGold()
    {
        // validate: dead monster and close enough?
        // use collider point(s) to also work with big entities
        if ((state == "IDLE" || state == "MOVING" || state == "CASTING") &&
            target != null && target is Monster && target.health == 0 &&
            Utils.ClosestDistance(collider, target.collider) <= interactionRange)
        {
            // distribute reward through party or to self
            if (InParty() && party.shareGold)
            {
                // find all party members in observer range
                // (we don't distribute it all across the map. standing
                //  next to each other is a better experience. players
                //  can't just stand safely in a city while gaining exp)
                List<Player> closeMembers = GetPartyMembersInProximity();

                // calculate the share via ceil, so that uneven numbers
                // still result in at least total gold in the end.
                // e.g. 4/2=2 (good); 5/2=2 (1 gold got lost)
                long share = (long)Mathf.Ceil((float)target.gold / (float)closeMembers.Count);

                // now distribute
                foreach (Player member in closeMembers)
                    member.gold += share;
            }
            else
            {
                gold += target.gold;
            }

            // reset target gold
            target.gold = 0;
        }
    }

    [Command]
    public void CmdTakeLootItem(int index)
    {
        // validate: dead monster and close enough and valid loot index?
        // use collider point(s) to also work with big entities
        if ((state == "IDLE" || state == "MOVING" || state == "CASTING") &&
            target != null && target is Monster && target.health == 0 &&
            Utils.ClosestDistance(collider, target.collider) <= interactionRange &&
            0 <= index && index < target.inventory.Count &&
            target.inventory[index].amount > 0)
        {
            ItemSlot slot = target.inventory[index];

            // try to add it to the inventory, clear monster slot if it worked
            if (InventoryAdd(slot.item, slot.amount))
            {
                slot.amount = 0;
                target.inventory[index] = slot;
            }
        }
    }

    // inventory ///////////////////////////////////////////////////////////////
    // are inventory operations like swap, merge, split allowed at the moment?
    public bool InventoryOperationsAllowed()
    {
        return state == "IDLE" ||
               state == "MOVING" ||
               state == "CASTING" ||
               (state == "TRADING" && tradeStatus == TradeStatus.Free);
    }

    [Command]
    public void CmdSwapInventoryTrash(int inventoryIndex)
    {
        // dragging an inventory item to the trash always overwrites the trash
        if (InventoryOperationsAllowed() &&
            0 <= inventoryIndex && inventoryIndex < inventory.Count)
        {
            // inventory slot has to be valid and destroyable and not summoned
            ItemSlot slot = inventory[inventoryIndex];
            if (slot.amount > 0 && slot.item.destroyable && !slot.item.summoned)
            {
                // overwrite trash
                trash = slot;

                // clear inventory slot
                slot.amount = 0;
                inventory[inventoryIndex] = slot;
            }
        }
    }

    [Command]
    public void CmdSwapTrashInventory(int inventoryIndex)
    {
        if (InventoryOperationsAllowed() &&
            0 <= inventoryIndex && inventoryIndex < inventory.Count)
        {
            // inventory slot has to be empty or destroyable
            ItemSlot slot = inventory[inventoryIndex];
            if (slot.amount == 0 || slot.item.destroyable)
            {
                // swap them
                inventory[inventoryIndex] = trash;
                trash = slot;
            }
        }
    }

    [Command]
    public void CmdSwapInventoryInventory(int fromIndex, int toIndex)
    {
        // note: should never send a command with complex types!
        // validate: make sure that the slots actually exist in the inventory
        // and that they are not equal
        if (InventoryOperationsAllowed() &&
            0 <= fromIndex && fromIndex < inventory.Count &&
            0 <= toIndex && toIndex < inventory.Count &&
            fromIndex != toIndex)
        {
            // swap them
            ItemSlot temp = inventory[fromIndex];
            inventory[fromIndex] = inventory[toIndex];
            inventory[toIndex] = temp;
        }
    }

    [Command]
    public void CmdInventorySplit(int fromIndex, int toIndex)
    {
        // note: should never send a command with complex types!
        // validate: make sure that the slots actually exist in the inventory
        // and that they are not equal
        if (InventoryOperationsAllowed() &&
            0 <= fromIndex && fromIndex < inventory.Count &&
            0 <= toIndex && toIndex < inventory.Count &&
            fromIndex != toIndex)
        {
            // slotFrom needs at least two to split, slotTo has to be empty
            ItemSlot slotFrom = inventory[fromIndex];
            ItemSlot slotTo = inventory[toIndex];
            if (slotFrom.amount >= 2 && slotTo.amount == 0)
            {
                // split them serversided (has to work for even and odd)
                slotTo = slotFrom; // copy the value

                slotTo.amount = slotFrom.amount / 2;
                slotFrom.amount -= slotTo.amount; // works for odd too

                // put back into the list
                inventory[fromIndex] = slotFrom;
                inventory[toIndex] = slotTo;
            }
        }
    }

    [Command]
    public void CmdInventoryMerge(int fromIndex, int toIndex)
    {
        if (InventoryOperationsAllowed() &&
            0 <= fromIndex && fromIndex < inventory.Count &&
            0 <= toIndex && toIndex < inventory.Count &&
            fromIndex != toIndex)
        {
            // both items have to be valid
            ItemSlot slotFrom = inventory[fromIndex];
            ItemSlot slotTo = inventory[toIndex];
            if (slotFrom.amount > 0 && slotTo.amount > 0)
            {
                // make sure that items are the same type
                // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                if (slotFrom.item.Equals(slotTo.item))
                {
                    // merge from -> to
                    // put as many as possible into 'To' slot
                    int put = slotTo.IncreaseAmount(slotFrom.amount);
                    slotFrom.DecreaseAmount(put);

                    // put back into the list
                    inventory[fromIndex] = slotFrom;
                    inventory[toIndex] = slotTo;
                }
            }
        }
    }

    [ClientRpc]
    public void RpcUsedItem(Item item)
    {
        // validate
        if (item.data is UsableItem)
        {
            UsableItem itemData = (UsableItem)item.data;
            itemData.OnUsed(this);
        }
    }

    [Command]
    public void CmdUseInventoryItem(int index)
    {
        // validate
        if (InventoryOperationsAllowed() &&
            0 <= index && index < inventory.Count && inventory[index].amount > 0 &&
            inventory[index].item.data is UsableItem)
        {
            if (inventory[index].item.data is ScriptableBuilding)
                if (playerCar.car || playerPremiumZoneManager.inPremiumZone)
                    return;

            // use item
            // note: we don't decrease amount / destroy in all cases because
            // some items may swap to other slots in .Use()
            UsableItem itemData = (UsableItem)inventory[index].item.data;
            if (itemData.CanUse(this, index))
            {
                if (inventory[index].amount > 0)
                {
                    if (inventory[index].item.data is PetItem)
                    {
                        ItemSlot slot = inventory[index];
                        slot.item.isSummoned = true;
                        inventory[index] = slot;
                    }
                }

                if (itemData is WeaponItem)
                {
                    for (int i = 0; i < quests.Count; i++)
                    {
                        Quest quest = quests[i];
                        if (quest.data.equipWeapon == true)
                        {
                            quest.checkEquipWeapon = true;
                            quests[i] = quest;
                        }
                    }
                    if (((WeaponItem)itemData).ammoItems.Count > 0)
                    {
                        bool alreadySetted = false;

                        for (int amm = 0; amm < ((WeaponItem)itemData).ammoItems.Count; amm++)
                        {
                            if (equipment[5].amount > 0 && equipment[5].item.name == ((WeaponItem)itemData).ammoItems[amm].name)
                            {
                                alreadySetted = true;
                            }
                        }
                        if (alreadySetted == false)
                        {
                            alreadySetted = false;
                            for (int amm = 0; amm < ((WeaponItem)itemData).ammoItems.Count; amm++)
                            {
                                if (alreadySetted == false && GetAmmoIndex(((WeaponItem)itemData).ammoItems[amm].name) > -1)
                                {
                                    SetAmmo(((WeaponItem)itemData).ammoItems[amm].name);
                                    alreadySetted = true;
                                }
                            }
                        }
                    }
                }

                if (itemData is EquipmentItem && itemData.possibleBagWeight.baseValue > 0)
                {
                    for (int i = 0; i < quests.Count; i++)
                    {
                        Quest quest = quests[i];
                        if (quest.data.equipBag == true)
                        {
                            quest.checkEquipBag = true;
                            quests[i] = quest;
                        }
                    }
                }

                if (itemData is FoodItem)
                {
                    for (int i = 0; i < quests.Count; i++)
                    {
                        Quest quest = quests[i];
                        if (quest.data.eat == true)
                        {
                            quest.checkEat = true;
                            quests[i] = quest;
                        }
                    }
                }

                if (itemData is TeleportItem)
                {
                    for (int i = 0; i < quests.Count; i++)
                    {
                        Quest quest = quests[i];
                        if (quest.data.useTeleport == true)
                        {
                            quest.checkUseTeleport = true;
                            quests[i] = quest;
                        }
                    }
                }

                if (itemData is UsableItem && itemData.name == GeneralManager.singleton.Instantresurrect.name)
                {
                    for (int i = 0; i < quests.Count; i++)
                    {
                        Quest quest = quests[i];
                        if (quest.data.useInstantResurrect == true)
                        {
                            quest.checkUseInstantResurrect = true;
                            quests[i] = quest;
                        }
                    }
                }

                // .Use might clear the slot, so we backup the Item first for the Rpc
                Item item = inventory[index].item;
                itemData.Use(this, index);
                RpcUsedItem(item);
            }
        }
    }

    // item cooldowns //////////////////////////////////////////////////////////
    // get remaining item cooldown, or 0 if none
    public float GetItemCooldown(string cooldownCategory)
    {
        // get stable hash to reduce bandwidth
        int hash = cooldownCategory.GetStableHashCode();

        // find cooldown for that category
        if (itemCooldowns.TryGetValue(hash, out double cooldownEnd))
        {
            return NetworkTime.time >= cooldownEnd ? 0 : (float)(cooldownEnd - NetworkTime.time);
        }

        // none found
        return 0;
    }

    // reset item cooldown
    public void SetItemCooldown(string cooldownCategory, float cooldown)
    {
        // get stable hash to reduce bandwidth
        int hash = cooldownCategory.GetStableHashCode();

        // save end time
        itemCooldowns[hash] = NetworkTime.time + cooldown;
    }

    // equipment ///////////////////////////////////////////////////////////////
    void OnEquipmentChanged(SyncListItemSlot.Operation op, int index, ItemSlot oldSlot, ItemSlot newSlot)
    {
        // update the equipment
        RefreshLocation(index);
    }

    public void RefreshLocation(int index)
    {
        if (!playerMove.bodyPlayer)
        {
            if (playerMove.dummyBodyPlayer.childCount > 0)
                playerMove.bodyPlayer = playerMove.dummyBodyPlayer.GetChild(0);
        }

        if (!playerMove.bodyPlayer) return;

        ItemSlot slot = equipment[index];
        EquipmentInfo info = equipmentInfo[index];

        ChangeAnimatorAndWeapon();

        if (index == 0)
        {
            if (slot.amount > 0)
            {
                SpawnWeapon(((EquipmentItem)slot.item.data).clothes, ((EquipmentItem)slot.item.data).indexToSpawnWeapon, index);
            }
            else
            {
                Destroy(playerItemEquipment.weapon.gameObject);
                playerItemEquipment.weapon = null;
            }
            playerItemEquipment.CheckFirstWeapon();
        }
        if (index == 2)
        {
            if (slot.amount > 0)
            {
                playerCreation.upper = ((WeaponItem)slot.item.data).indexShirt;
            }
            else
            {
                playerCreation.upper = -1;
            }
        }
        if (index == 3)
        {
            if (equipment[2].amount > 0 && ((EquipmentItem)equipment[2].item.data).ignorePants == true)
            {
                playerCreation.down = -1;
            }
            else
            {
                if (slot.amount > 0)
                {

                    playerCreation.down = ((WeaponItem)slot.item.data).indexPants;
                }
                else
                {
                    playerCreation.down = -1;
                }
            }
        }
        if(index == 4)
        {
            playerRadio.CheckRadio();
        }
        if(index == 5)
        {
            playerMunitionManager.CheckMunition();
        }
        if(index == 9)
        {
            playerMunitionManager.CheckWeapon();
        }
        if (index == 6)
        {
            playerCreation.CheckHat();
            if (slot.amount > 0)
            {
                playerCreation.hats = ((WeaponItem)slot.item.data).indexHat;
            }
            else
            {
                playerCreation.hats = -1;
            }
        }
        if (index == 7)
        {
            if (isServer)
            {
                //something inventory related was modified, do we need to add or remove slots
                if (inventorySize > inventory.Count)
                {
                    //count too small, add slots
                    for (int i = inventory.Count; i <= inventorySize; i++)
                    {
                        inventory.Add(new ItemSlot());
                    }
                }
                else if (inventorySize < inventory.Count)
                {
                    //count too small, remove slots
                    //loop over inventory backwards while count is greater than size
                    for (int i = inventory.Count - 1; inventory.Count > inventorySize && i >= 0; i--)
                    {
                        //if inventory not valid, remove the slot
                        if (inventory[i].amount == 0)
                        {
                            inventory.RemoveAt(i);
                        }
                    }
                }
            }
        }
        if (index == 8)
        {
            if (slot.amount > 0)
            {
                playerCreation.shoes = ((WeaponItem)slot.item.data).indexShoes;
            }
            else
            {
                playerCreation.shoes = -1;
            }
        }
        if(index == 9)
        {
            playerTorch.CheckTorch();
        }
        if (index == 10)
        {
            playerCreation.CheckAccessory();
            if (slot.amount > 0)
            {
                playerCreation.accessory = ((WeaponItem)slot.item.data).indexAccessory;
            }
            else
            {
                playerCreation.accessory = -1;
            }
        }
    }

    public void SpawnWeapon(GameObject toSpawn, int index, int equipmenteIndex)
    {
        PlayerPlaceholderWeapon playerPlaceholderWeapon = playerMove.bodyPlayer.GetComponent<PlayerPlaceholderWeapon>();
        List<GameObject> toEliminate = new List<GameObject>();
            for (int i = 0; i < playerPlaceholderWeapon.placeholderWeapon.Count; i++)
            {
                int placeholderWeaponIndex = i;
                for (int e = 0; e < playerPlaceholderWeapon.placeholderWeapon[placeholderWeaponIndex].childCount; e++)
                {
                    toEliminate.Add(playerPlaceholderWeapon.placeholderWeapon[placeholderWeaponIndex].GetChild(e).gameObject);
                }
            }

            for (int a = 0; a < toEliminate.Count; a++)
            {
                Destroy(toEliminate[a]);
            }

            playerItemEquipment.weapon = Instantiate(toSpawn, playerMove.bodyPlayer.GetComponent<PlayerPlaceholderWeapon>().placeholderWeapon[index]);
            playerItemEquipment.weapon.transform.localRotation = new Quaternion(0.0f, 0.0f, 0.0f, 0);
            if (!isLocalPlayer)
            {
                ItemLayerSelector itemLayerSelector = playerItemEquipment.weapon.GetComponent<ItemLayerSelector>();
                itemLayerSelector.player = this;
                itemLayerSelector.SetLayer();
            }
    }

    [Server]
    public void SwapInventoryEquip(int inventoryIndex, int equipmentIndex)
    {
        // validate: make sure that the slots actually exist in the inventory
        // and in the equipment
        if (InventoryOperationsAllowed() &&
            0 <= inventoryIndex && inventoryIndex < inventory.Count &&
            0 <= equipmentIndex && equipmentIndex < equipment.Count)
        {
            // item slot has to be empty (unequip) or equipable
            ItemSlot slot = inventory[inventoryIndex];
            if (slot.amount == 0 ||
                slot.item.data is EquipmentItem itemData &&
                itemData.CanEquip(this, inventoryIndex, equipmentIndex))
            {
                // swap them
                //ItemSlot temp = equipment[equipmentIndex];
                //equipment[equipmentIndex] = slot;
                //inventory[inventoryIndex] = temp;
                if (equipment[equipmentIndex].amount == 0 || CanUnEquip(equipment[equipmentIndex].item))
                {
                    // swap them
                    var temp = equipment[equipmentIndex];
                    equipment[equipmentIndex] = inventory[inventoryIndex];
                    inventory[inventoryIndex] = temp;
                }
                playerTorch.CheckTorch();
                if (playerTorch.torchItem.amount == 0)
                {
                    playerTorch.isOn = false;
                }

                playerRadio.CheckRadio();
                if (playerRadio.radioItem.amount == 0)
                {
                    playerRadio.isOn = false;
                }
                if (slot.amount > 0)
                {
                    if (slot.item.data is WeaponItem)
                    {
                        if (((WeaponItem)slot.item.data).indexHat > -1)
                        {
                            playerCreation.hats = ((WeaponItem)slot.item.data).indexHat;
                        }
                        if (((WeaponItem)slot.item.data).indexAccessory > -1)
                        {
                            playerCreation.accessory = ((WeaponItem)slot.item.data).indexAccessory;
                        }
                        if (((WeaponItem)slot.item.data).indexShirt > -1)
                        {
                            playerCreation.upper = ((EquipmentItem)slot.item.data).indexShirt;
                        }
                        if (((WeaponItem)slot.item.data).indexPants > -1)
                        {
                            if (equipment[2].amount > 0 && ((WeaponItem)equipment[2].item.data).ignorePants == true)
                            {
                                playerCreation.down = -1;
                            }
                            else
                            {
                                playerCreation.down = ((WeaponItem)slot.item.data).indexPants;
                            }
                        }
                        if (((WeaponItem)slot.item.data).indexShoes > -1)
                        {
                            playerCreation.shoes = ((WeaponItem)slot.item.data).indexShoes;
                        }
                    }


                    if (slot.item.data is WeaponItem)
                    {
                        for (int i = 0; i < quests.Count; i++)
                        {
                            Quest quest = quests[i];
                            if (quest.data.equipWeapon == true)
                            {
                                quest.checkEquipWeapon = true;
                                quests[i] = quest;
                            }
                        }
                        if (((WeaponItem)slot.item.data).ammoItems.Count > 0)
                        {
                            bool alreadySetted = false;

                            for (int amm = 0; amm < ((WeaponItem)slot.item.data).ammoItems.Count; amm++)
                            {
                                if (equipment[5].amount > 0 && equipment[5].item.name == ((WeaponItem)slot.item.data).ammoItems[amm].name)
                                {
                                    alreadySetted = true;
                                }
                            }
                            if (alreadySetted == false)
                            {
                                alreadySetted = false;
                                for (int amm = 0; amm < ((WeaponItem)slot.item.data).ammoItems.Count; amm++)
                                {
                                    if (alreadySetted == false && GetAmmoIndex(((WeaponItem)slot.item.data).ammoItems[amm].name) > -1)
                                    {
                                        SetAmmo(((WeaponItem)slot.item.data).ammoItems[amm].name);
                                        alreadySetted = true;
                                    }
                                }
                            }
                        }
                    }

                    if (slot.item.data is EquipmentItem && slot.item.data.possibleBagWeight.baseValue > 0)
                    {
                        for (int i = 0; i < quests.Count; i++)
                        {
                            Quest quest = quests[i];
                            if (quest.data.equipBag == true)
                            {
                                quest.checkEquipBag = true;
                                quests[i] = quest;
                            }
                        }
                    }

                    if (slot.item.data is FoodItem)
                    {
                        for (int i = 0; i < quests.Count; i++)
                        {
                            Quest quest = quests[i];
                            if (quest.data.eat == true)
                            {
                                quest.checkEat = true;
                                quests[i] = quest;
                            }
                        }
                    }

                    if (slot.item.data is TeleportItem)
                    {
                        for (int i = 0; i < quests.Count; i++)
                        {
                            Quest quest = quests[i];
                            if (quest.data.useTeleport == true)
                            {
                                quest.checkUseTeleport = true;
                                quests[i] = quest;
                            }
                        }
                    }

                    if (slot.item.data is UsableItem && slot.item.data.name == GeneralManager.singleton.Instantresurrect.name)
                    {
                        for (int i = 0; i < quests.Count; i++)
                        {
                            Quest quest = quests[i];
                            if (quest.data.useInstantResurrect == true)
                            {
                                quest.checkUseInstantResurrect = true;
                                quests[i] = quest;
                            }
                        }
                    }
                }
            }
        }
    }

    [Command]
    public void CmdSwapInventoryEquip(int inventoryIndex, int equipmentIndex)
    {
        SwapInventoryEquip(inventoryIndex, equipmentIndex);
    }

    [Server]
    public void MergeInventoryEquip(int inventoryIndex, int equipmentIndex)
    {
        if (InventoryOperationsAllowed() &&
            0 <= inventoryIndex && inventoryIndex < inventory.Count &&
            0 <= equipmentIndex && equipmentIndex < equipment.Count)
        {
            // both items have to be valid
            // note: no 'is EquipmentItem' check needed because we already
            //       checked when equipping 'slotTo'.
            ItemSlot slotFrom = inventory[inventoryIndex];
            ItemSlot slotTo = equipment[equipmentIndex];
            if (slotFrom.amount > 0 && slotTo.amount > 0)
            {
                // make sure that items are the same type
                // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                if (slotFrom.item.Equals(slotTo.item))
                {
                    // merge from -> to
                    // put as many as possible into 'To' slot
                    int put = slotTo.IncreaseAmount(slotFrom.amount);
                    slotFrom.DecreaseAmount(put);

                    // put back into the list
                    inventory[inventoryIndex] = slotFrom;
                    equipment[equipmentIndex] = slotTo;
                }
            }
        }
    }

    [Command]
    public void CmdMergeInventoryEquip(int equipmentIndex, int inventoryIndex)
    {
        MergeInventoryEquip(equipmentIndex, inventoryIndex);
    }

    [Command]
    public void CmdMergeEquipInventory(int equipmentIndex, int inventoryIndex)
    {
        if (InventoryOperationsAllowed() &&
            0 <= inventoryIndex && inventoryIndex < inventory.Count &&
            0 <= equipmentIndex && equipmentIndex < equipment.Count)
        {
            // both items have to be valid
            ItemSlot slotFrom = equipment[equipmentIndex];
            ItemSlot slotTo = inventory[inventoryIndex];
            if (slotFrom.amount > 0 && slotTo.amount > 0)
            {
                // make sure that items are the same type
                // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                if (slotFrom.item.Equals(slotTo.item))
                {
                    // merge from -> to
                    // put as many as possible into 'To' slot
                    int put = slotTo.IncreaseAmount(slotFrom.amount);
                    slotFrom.DecreaseAmount(put);

                    // put back into the list
                    equipment[equipmentIndex] = slotFrom;
                    inventory[inventoryIndex] = slotTo;
                }
            }
        }
    }

    // skills //////////////////////////////////////////////////////////////////
    // we use 'is' instead of 'GetType' so that it works for inherited types too
    public override bool CanAttack(Entity entity)
    {
        if (base.CanAttack(entity))
        {
            return PlayerCanAttack(this);
        }

        return false;
        //return base.CanAttack(entity) &&
        //       (entity is Monster ||
        //        entity is Player ||
        //        (entity is Pet && entity != activePet) ||
        //        (entity is Mount && entity != activeMount));
    }

    // always include latest look direction when casting a skill.
    // otherwise if the player makes a tiny move to change the look direction,
    // the server might not register that move and not change the look direction
    // resulting the slash skill to be cast into the wrong direction on the
    // server.
    // => this method is fail safe.
    // => this way we only update look direction when it matters.
    [Command]
    public void CmdUseSkill(int skillIndex, Vector2 direction)
    {
        playerItemEquipment.CheckFirstWeapon();
        skillIndex = FindNetworkSkill(((WeaponItem)playerItemEquipment.firstWeapon.item.data).requiredSkill);
        // validate
        if ((state == "IDLE" || state == "MOVING" || state == "CASTING") &&
            0 <= skillIndex && skillIndex < skills.Count)
        {
            // skill learned and can be casted?
            if (skills[skillIndex].level > 0 && skills[skillIndex].IsReady())
            {
                currentSkill = skillIndex;
                lookDirection = direction;
            }
        }
    }

    // helper function: try to use a skill and walk into range if necessary
    [Client]
    public void TryUseSkill(int skillIndex, bool ignoreState = false)
    {
        playerItemEquipment.CheckFirstWeapon();
        // only if not casting already
        // (might need to ignore that when coming from pending skill where
        //  CASTING is still true)
        if (state != "CASTING" || ignoreState)
        {
            Skill skill = skills[skillIndex];
            if (CastCheckSelf(skill) && CastCheckTarget(skill))
            {
                // check distance between self and target
                Vector2 destination;
                if (CastCheckDistance(skill, out destination))
                {
                    // cast
                    CmdUseSkill(skillIndex, lookDirection);
                }
                else
                {
                    // move to the target first
                    // (use collider point(s) to also work with big entities)
                    agent.stoppingDistance = skill.castRange * attackToMoveRangeRatio;
                    agent.destination = destination;

                    // use skill when there
                    useSkillWhenCloser = skillIndex;
                }
            }
        }
        else
        {
            pendingSkill = skillIndex;
        }
    }

    public bool HasLearnedSkill(string skillName)
    {
        // has this skill with at least level 1 (=learned)?
        return HasLearnedSkillWithLevel(skillName, 1);
    }

    public bool HasLearnedSkillWithLevel(string skillName, int skillLevel)
    {
        // (avoid Linq because it is HEAVY(!) on GC and performance)
        foreach (Skill skill in skills)
            if (skill.level >= skillLevel && skill.name == skillName)
                return true;
        return false;
    }

    // helper function for command and UI
    // -> this is for learning and upgrading!
    public bool CanUpgradeSkill(Skill skill)
    {
        return skill.level < skill.maxLevel &&
               level >= skill.upgradeRequiredLevel &&
               skillExperience >= skill.upgradeRequiredSkillExperience &&
               (skill.predecessor == null || (HasLearnedSkillWithLevel(skill.predecessor.name, skill.predecessorLevel)));
    }

    // -> this is for learning and upgrading!
    [Command]
    public void CmdUpgradeSkill(int skillIndex)
    {
        // validate
        if ((state == "IDLE" || state == "MOVING" || state == "CASTING") &&
            0 <= skillIndex && skillIndex < skills.Count)
        {
            // can be upgraded?
            Skill skill = skills[skillIndex];
            if (CanUpgradeSkill(skill))
            {
                // decrease skill experience
                skillExperience -= skill.upgradeRequiredSkillExperience;

                // upgrade
                ++skill.level;
                skills[skillIndex] = skill;
            }
        }
    }

    // skillbar ////////////////////////////////////////////////////////////////
    //[Client] <- disabled while UNET OnDestroy isLocalPlayer bug exists
    void SaveSkillbar()
    {
        // save skillbar to player prefs (based on player name, so that
        // each character can have a different skillbar)
        for (int i = 0; i < skillbar.Length; ++i)
            PlayerPrefs.SetString(name + "_skillbar_" + i, skillbar[i].reference);

        // force saving playerprefs, otherwise they aren't saved for some reason
        PlayerPrefs.Save();
    }

    [Client]
    void LoadSkillbar()
    {
        print("loading skillbar for " + name);
        List<Skill> learned = skills.Where(skill => skill.level > 0).ToList();
        for (int i = 0; i < skillbar.Length; ++i)
        {
            // try loading an existing entry
            if (PlayerPrefs.HasKey(name + "_skillbar_" + i))
            {
                string entry = PlayerPrefs.GetString(name + "_skillbar_" + i, "");

                // is this a valid item/equipment/learned skill?
                // (might be an old character's playerprefs)
                // => only allow learned skills (in case it's an old character's
                //    skill that we also have, but haven't learned yet)
                if (HasLearnedSkill(entry) ||
                    GetInventoryIndexByName(entry) != -1 ||
                    GetEquipmentIndexByName(entry) != -1)
                {
                    skillbar[i].reference = entry;
                }
            }
            // otherwise fill with default skills for a better first impression
            else if (i < learned.Count)
            {
                skillbar[i].reference = learned[i].name;
            }
        }
    }

    // quests //////////////////////////////////////////////////////////////////
    public int GetQuestIndexByName(string questName)
    {
        // (avoid Linq because it is HEAVY(!) on GC and performance)
        for (int i = 0; i < quests.Count; ++i)
            if (quests[i].name == questName)
                return i;
        return -1;
    }

    // helper function to check if the player has completed a quest before
    public bool HasCompletedQuest(string questName)
    {
        // (avoid Linq because it is HEAVY(!) on GC and performance)
        foreach (Quest quest in quests)
            if (quest.name == questName && quest.completed)
                return true;
        return false;
    }

    // count the completed quests
    public int CountIncompleteQuests()
    {
        int count = 0;
        foreach (Quest quest in quests)
            if (!quest.completed)
                ++count;
        return count;
    }

    // helper function to check if a player has an active (not completed) quest
    public bool HasActiveQuest(string questName)
    {
        // (avoid Linq because it is HEAVY(!) on GC and performance)
        foreach (Quest quest in quests)
            if (quest.name == questName && !quest.completed)
                return true;
        return false;
    }

    [Server]
    public void QuestsOnKilled(Entity victim)
    {
        // call OnKilled in all active (not completed) quests
        for (int i = 0; i < quests.Count; ++i)
            if (!quests[i].completed)
                quests[i].OnKilled(this, i, victim);
    }

    [ServerCallback] // called by OnTriggerEnter on client and server. use callback.
    public void QuestsOnLocation(Collider2D location)
    {
        // call OnLocation in all active (not completed) quests
        for (int i = 0; i < quests.Count; ++i)
            if (!quests[i].completed)
                quests[i].OnLocation(this, i, location);
    }

    // helper function to check if the player can accept a new quest
    // note: no quest.completed check needed because we have a'not accepted yet'
    //       check
    public bool CanAcceptQuest(ScriptableQuest quest)
    {
        // not too many quests yet?
        // has required level?
        // not accepted yet?
        // has finished predecessor quest (if any)?
        return CountIncompleteQuests() < activeQuestLimit &&
               level >= quest.requiredLevel &&          // has required level?
               GetQuestIndexByName(quest.name) == -1 && // not accepted yet?
               (quest.predecessor == null || HasCompletedQuest(quest.predecessor.name));
    }

    [Command]
    public void CmdAcceptQuest(int npcQuestIndex)
    {
        // validate
        // use collider point(s) to also work with big entities
        if (state == "IDLE" &&
            target != null &&
            target.health > 0 &&
            target is Npc &&
            0 <= npcQuestIndex && npcQuestIndex < ((Npc)target).quests.Length &&
            Utils.ClosestDistance(collider, target.collider) <= interactionRange)
        {
            ScriptableQuestOffer npcQuest = ((Npc)target).quests[npcQuestIndex];
            if (npcQuest.acceptHere && CanAcceptQuest(npcQuest.quest))
                quests.Add(new Quest(npcQuest.quest));
        }
    }

    // helper function to check if the player can complete a quest
    public bool CanCompleteQuest(string questName)
    {
        // has the quest and not completed yet?
        int index = GetQuestIndexByName(questName);
        if (index != -1 && !quests[index].completed)
        {
            // fulfilled?
            Quest quest = quests[index];
            if (quest.IsFulfilled(this))
            {
                // enough space for reward item (if any)?
                return quest.rewardItem == null || InventoryCanAdd(new Item(quest.rewardItem), 1);
            }
        }
        return false;
    }

    [Command]
    public void CmdCompleteQuest(int npcQuestIndex)
    {
        // validate
        // use collider point(s) to also work with big entities
        if (state == "IDLE" &&
            target != null &&
            target.health > 0 &&
            target is Npc &&
            0 <= npcQuestIndex && npcQuestIndex < ((Npc)target).quests.Length &&
            Utils.ClosestDistance(collider, target.collider) <= interactionRange)
        {
            ScriptableQuestOffer npcQuest = ((Npc)target).quests[npcQuestIndex];
            if (npcQuest.completeHere)
            {
                int index = GetQuestIndexByName(npcQuest.quest.name);
                if (index != -1)
                {
                    // can complete it? (also checks inventory space for reward, if any)
                    Quest quest = quests[index];
                    if (CanCompleteQuest(quest.name))
                    {
                        // call quest.OnCompleted to remove quest items from
                        // inventory, etc.
                        quest.OnCompleted(this);

                        // gain rewards
                        gold += quest.rewardGold;
                        experience += quest.rewardExperience;
                        if (quest.rewardItem != null)
                            InventoryAdd(new Item(quest.rewardItem), 1);

                        // complete quest
                        quest.completed = true;
                        quests[index] = quest;
                    }
                }
            }
        }
    }

    // npc trading /////////////////////////////////////////////////////////////
    [Command]
    public void CmdNpcBuyItem(int index, int amount)
    {
        // validate: close enough, npc alive and valid index?
        // use collider point(s) to also work with big entities
        if (state == "IDLE" &&
            target != null &&
            target.health > 0 &&
            target is Npc &&
            Utils.ClosestDistance(collider, target.collider) <= interactionRange &&
            0 <= index && index < ((Npc)target).saleItems.Length)
        {
            // valid amount?
            Item npcItem = new Item(((Npc)target).saleItems[index]);
            if (1 <= amount && amount <= npcItem.maxStack)
            {
                long price = npcItem.buyPrice * amount;

                if (playerPremiumZoneManager.inPremiumZone)
                    price = price / 2;

                // enough gold and enough space in inventory?
                if (gold >= price && InventoryCanAdd(npcItem, amount))
                {
                    // pay for it, add to inventory
                    gold -= price;
                    InventoryAdd(npcItem, amount);
                }
            }
        }
    }

    [Command]
    public void CmdNpcSellItem(int index, int amount)
    {
        // validate: close enough, npc alive and valid index and valid item?
        // use collider point(s) to also work with big entities
        if (state == "IDLE" &&
            target != null &&
            target.health > 0 &&
            target is Npc &&
            Utils.ClosestDistance(collider, target.collider) <= interactionRange &&
            0 <= index && index < inventory.Count)
        {
            // sellable?
            ItemSlot slot = inventory[index];
            if (slot.amount > 0 && slot.item.sellable && !slot.item.summoned)
            {
                // valid amount?
                if (1 <= amount && amount <= slot.amount)
                {
                    // sell the amount
                    long price = slot.item.sellPrice * amount;
                    gold += price;
                    slot.DecreaseAmount(amount);
                    inventory[index] = slot;
                }
            }
        }
    }

    // npc teleport ////////////////////////////////////////////////////////////
    [Command]
    public void CmdNpcTeleport()
    {
        // validate
        if (state == "IDLE" &&
            target != null &&
            target.health > 0 &&
            target is Npc &&
            Utils.ClosestDistance(collider, target.collider) <= interactionRange &&
            ((Npc)target).teleportTo != null)
        {
            // using agent.Warp is recommended over transform.position
            // (the latter can cause weird bugs when using it with an agent)
            agent.Warp(((Npc)target).teleportTo.position);

            // clear target. no reason to keep targeting the npc after we
            // teleported away from it
            target = null;
        }
    }

    // player to player trading ////////////////////////////////////////////////
    // how trading works:
    // 1. A invites his target with CmdTradeRequest()
    //    -> sets B.tradeInvitationFrom = A;
    // 2. B sees a UI window and accepts (= invites A too)
    //    -> sets A.tradeInvitationFrom = B;
    // 3. the TradeStart event is fired, both go to 'TRADING' state
    // 4. they lock the trades
    // 5. they accept, then items and gold are swapped

    public bool CanStartTrade()
    {
        // a player can only trade if he is not trading already and alive
        return health > 0 && state != "TRADING";
    }


    // request a trade with the target player.
    [Command]
    public void CmdTradeRequestSend()
    {
        // validate
        if (CanStartTradeWith(target))
        {
            // send a trade request to target
            ((Player)target).tradeRequestFrom = name;
            print(name + " invited " + target.name + " to trade");
        }
    }

    // helper function to find the guy who sent us a trade invitation
    [Server]
    Player FindPlayerFromTradeInvitation()
    {
        if (tradeRequestFrom != "" &&
            onlinePlayers.TryGetValue(tradeRequestFrom, out Player sender))
        {
            return sender;
        }
        return null;
    }

    // accept a trade invitation by simply setting 'requestFrom' for the other
    // person to self
    //[Command]
    //public void CmdTradeRequestAccept()
    //{
    //    Player sender = FindPlayerFromTradeInvitation();
    //    if (sender != null) {
    //        if (CanStartTradeWith(sender)) {
    //            // also send a trade request to the person that invited us
    //            sender.tradeRequestFrom = name;
    //            print(name + " accepted " + sender.name + "'s trade request");
    //        }
    //    }
    //}

    // decline a trade invitation
    [Command]
    public void CmdTradeRequestDecline()
    {
        tradeRequestFrom = "";
    }

    [Server]
    void TradeCleanup()
    {
        // clear all trade related properties
        tradeOfferGold = 0;
        for (int i = 0; i < tradeOfferItems.Count; ++i) tradeOfferItems[i] = -1;
        tradeStatus = TradeStatus.Free;
        tradeRequestFrom = "";
    }

    [Command]
    public void CmdTradeCancel()
    {
        // validate
        if (state == "TRADING")
        {
            // clear trade request for both guys. the FSM event will do the rest
            Player player = FindPlayerFromTradeInvitation();
            if (player != null) player.tradeRequestFrom = "";
            tradeRequestFrom = "";
        }
    }

    [Command]
    public void CmdTradeOfferLock()
    {
        // validate
        if (state == "TRADING")
            tradeStatus = TradeStatus.Locked;
    }

    [Command]
    public void CmdTradeOfferGold(long amount)
    {
        // validate
        if (state == "TRADING" && tradeStatus == TradeStatus.Free &&
            0 <= amount && amount <= gold)
            tradeOfferGold = amount;
    }

    [Command]
    public void CmdTradeOfferItem(int inventoryIndex, int offerIndex)
    {
        // validate
        if (state == "TRADING" && tradeStatus == TradeStatus.Free &&
            0 <= offerIndex && offerIndex < tradeOfferItems.Count &&
            !tradeOfferItems.Contains(inventoryIndex) && // only one reference
            0 <= inventoryIndex && inventoryIndex < inventory.Count)
        {
            ItemSlot slot = inventory[inventoryIndex];
            if (slot.amount > 0 && slot.item.tradable && !slot.item.summoned)
                tradeOfferItems[offerIndex] = inventoryIndex;
        }
    }

    [Command]
    public void CmdTradeOfferItemClear(int offerIndex)
    {
        // validate
        if (state == "TRADING" && tradeStatus == TradeStatus.Free &&
            0 <= offerIndex && offerIndex < tradeOfferItems.Count)
            tradeOfferItems[offerIndex] = -1;
    }

    [Server]
    bool IsTradeOfferStillValid()
    {
        // not enough gold? then invalid
        if (gold < tradeOfferGold)
            return false;

        // all offered items are -1 or valid?
        // (avoid Linq because it is HEAVY(!) on GC and performance)
        foreach (int index in tradeOfferItems)
        {
            if (index == -1 || (0 <= index && index < inventory.Count && inventory[index].amount > 0))
            {
                // good
            }
            else
            {
                // invalid item
                return false;
            }
        }
        return true;
    }

    [Server]
    int TradeOfferItemSlotAmount()
    {
        // (avoid Linq because it is HEAVY(!) on GC and performance)
        int count = 0;
        foreach (int index in tradeOfferItems)
            if (index != -1)
                ++count;
        return count;
    }

    [Server]
    int InventorySlotsNeededForTrade()
    {
        // if other guy offers 2 items and we offer 1 item then we only need
        // 2-1 = 1 slots. and the other guy would need 1-2 slots and at least 0.
        if (target != null && target is Player)
        {
            Player other = (Player)target;
            int otherAmount = other.TradeOfferItemSlotAmount();
            int myAmount = TradeOfferItemSlotAmount();
            return Mathf.Max(otherAmount - myAmount, 0);
        }
        return 0;
    }




    // crafting ////////////////////////////////////////////////////////////////
    // the crafting system is designed to work with all kinds of commonly known
    // crafting options:
    // - item combinations: wood + stone = axe
    // - weapon upgrading: axe + gem = strong axe
    // - recipe items: axerecipe(item) + wood(item) + stone(item) = axe(item)
    //
    // players can craft at all times, not just at npcs, because that's the most
    // realistic option

    // craft a recipe with the combination of items and put result into inventory
    // => we pass the recipe name so that we don't have to search ALL the
    //    recipes. this would slow down the server if we have lots of recipes.
    // => we just let the client do the searching!
    [Command]
    public void CmdCraft(string recipeName, int[] indices)
    {
        // validate: between 1 and 6, all valid, no duplicates?
        // -> can be IDLE or MOVING (in which case we reset the movement)
        if ((state == "IDLE" || state == "MOVING") &&
            indices.Length == ScriptableRecipe.recipeSize)
        {
            // find valid indices that are not '-1' and make sure there are no
            // duplicates
            List<int> validIndices = indices.Where(index => 0 <= index && index < inventory.Count && inventory[index].amount > 0).ToList();
            if (validIndices.Count > 0 && !validIndices.HasDuplicates())
            {
                // find recipe
                if (ScriptableRecipe.dict.TryGetValue(recipeName, out ScriptableRecipe recipe) &&
                    recipe.result != null)
                {
                    // enough space?
                    Item result = new Item(recipe.result);
                    if (InventoryCanAdd(result, 1))
                    {
                        // cache recipe so we don't have to search for it again
                        // in Craft()
                        craftingRecipe = recipe;

                        // store the crafting indices on the server. no need for
                        // a SyncList and unnecessary broadcasting.
                        // we already have a 'craftingIndices' variable anyway.
                        craftingIndices = indices.ToList();

                        // start crafting
                        craftingRequested = true;
                        craftingTimeEnd = NetworkTime.time + recipe.craftingTime;
                    }
                }
            }
        }
    }

    // finish the crafting
    [Server]
    void Craft()
    {
        // should only be called while CRAFTING and if recipe still valid
        // (no one should touch 'craftingRecipe', but let's just be sure.
        // -> we already validated everything in CmdCraft. let's just craft.
        if (state == "CRAFTING" &&
            craftingRecipe != null &&
            craftingRecipe.result != null)
        {
            // enough space?
            Item result = new Item(craftingRecipe.result);
            if (InventoryCanAdd(result, 1))
            {
                // remove the ingredients from inventory in any case
                foreach (ScriptableItemAndAmount ingredient in craftingRecipe.ingredients)
                    if (ingredient.amount > 0 && ingredient.item != null)
                        InventoryRemove(new Item(ingredient.item), ingredient.amount);

                // roll the dice to decide if we add the result or not
                // IMPORTANT: we use rand() < probability to decide.
                // => UnityEngine.Random.value is [0,1] inclusive:
                //    for 0% probability it's fine because it's never '< 0'
                //    for 100% probability it's not because it's not always '< 1', it might be == 1
                //    and if we use '<=' instead then it won't work for 0%
                // => C#'s Random value is [0,1) exclusive like most random
                //    functions. this works fine.
                if (new System.Random().NextDouble() < craftingRecipe.probability)
                {
                    // add result item to inventory
                    InventoryAdd(new Item(craftingRecipe.result), 1);
                    TargetCraftingSuccess();
                }
                else
                {
                    TargetCraftingFailed();
                }

                // clear indices afterwards
                // note: we set all to -1 instead of calling .Clear because
                //       that would clear all the slots in host mode.
                // (don't clear in host mode, otherwise it clears the crafting
                //  UI for the player and we have to drag items into it again)
                if (!isLocalPlayer)
                    for (int i = 0; i < ScriptableRecipe.recipeSize; ++i)
                        craftingIndices[i] = -1;

                // clear recipe
                craftingRecipe = null;
            }
        }
    }

    // two rpcs for results to save 1 byte for the actual result
    [TargetRpc] // only send to one client
    public void TargetCraftingSuccess()
    {
        craftingState = CraftingState.Success;
    }

    [TargetRpc] // only send to one client
    public void TargetCraftingFailed()
    {
        craftingState = CraftingState.Failed;
    }

    // pvp murder system ///////////////////////////////////////////////////////
    // attacking someone innocent results in Offender status
    //   (can be attacked without penalty for a short time)
    // killing someone innocent results in Murderer status
    //   (can be attacked without penalty for a long time + negative buffs)
    // attacking/killing a Offender/Murderer has no penalty
    //
    // we use buffs for the offender/status because buffs have all the features
    // that we need here.
    public bool IsOffender()
    {
        return offenderBuff != null && GetBuffIndexByName(offenderBuff.name) != -1;
    }

    public bool IsMurderer()
    {
        return murdererBuff != null && GetBuffIndexByName(murdererBuff.name) != -1;
    }

    public void StartOffender()
    {
        if (offenderBuff != null) AddOrRefreshBuff(new Buff(offenderBuff, 1));
    }

    public void StartMurderer()
    {
        if (murdererBuff != null) AddOrRefreshBuff(new Buff(murdererBuff, 1));
    }

    // item mall ///////////////////////////////////////////////////////////////
    [Command]
    public void CmdEnterCoupon(string coupon)
    {
        // only allow entering one coupon every few seconds to avoid brute force
        if (NetworkTime.time >= nextRiskyActionTime)
        {
            // YOUR COUPON VALIDATION CODE HERE
            // coins += ParseCoupon(coupon);
            Debug.Log("coupon: " + coupon + " => " + name + "@" + NetworkTime.time);
            nextRiskyActionTime = NetworkTime.time + couponWaitSeconds;
        }
    }

    [TargetRpc]
    public void TargetEnableThaksPanel()
    {
        Instantiate(GeneralManager.singleton.thanksPanel, GeneralManager.singleton.canvas);
    }

    [Command]
    public void CmdUnlockItem(int categoryIndex, int itemIndex, int currentType)
    {
        // validate: only if alive so people can't buy resurrection potions
        // after dieing in a PvP fight etc.
        if (health > 0 &&
            0 <= categoryIndex && categoryIndex <= GeneralManager.singleton.itemMallCategories.Length &&
            0 <= itemIndex && itemIndex <= GeneralManager.singleton.itemMallCategories[categoryIndex].items.Length)
        {
            Item item = new Item(GeneralManager.singleton.itemMallCategories[categoryIndex].items[itemIndex]);

            if (currentType == 0)
            {
                if (0 < item.coinPrice && item.coinPrice <= coins)
                {
                    // try to add it to the inventory, subtract costs from coins
                    if (InventoryAdd(item, 1))
                    {
                        coins -= item.coinPrice;
                        playerItemPoint.point += 50;
                        if (item.coinPrice >= 50)
                        {
                            if (playerItemPoint.point >= playerItemPoint.maxPoint)
                            {
                                playerItemPoint.point = 0;
                                gold += 10000;
                            }
                            TargetEnableThaksPanel();
                        }
                        // NOTE: item mall purchases need to be persistent, yet
                        // resaving the player here is not necessary because if the
                        // server crashes before next save, then both the inventory
                        // and the coins will be reverted anyway.
                    }
                }
            }
            if (currentType == 1)
            {
                TimeSpan difference;
                if (!string.IsNullOrEmpty(playerBoost.networkBoost[0].hiddenIslandTimerServer))
                    difference = DateTime.Parse(playerBoost.networkBoost[0].hiddenIslandTimerServer.ToString()) - System.DateTime.Now;

                if (playerBoost.networkBoost.Count > 0 && !string.IsNullOrEmpty(playerBoost.networkBoost[0].hiddenIslandTimer) && Convert.ToInt32(difference.TotalSeconds) > 0)
                {
                    if (0 < Convert.ToInt32(item.goldPrice) && Convert.ToInt32(item.goldPrice) <= gold)
                    {
                        // try to add it to the inventory, subtract costs from coins
                        if (InventoryAdd(item, 1))
                        {
                            gold -= Convert.ToInt32(item.goldPrice / 2);
                            // NOTE: item mall purchases need to be persistent, yet
                            // resaving the player here is not necessary because if the
                            // server crashes before next save, then both the inventory
                            // and the coins will be reverted anyway.
                        }
                    }
                }
                else
                {
                    if (0 < item.goldPrice && item.goldPrice <= gold)
                    {
                        // try to add it to the inventory, subtract costs from coins
                        if (InventoryAdd(item, 1))
                        {
                            gold -= item.goldPrice;
                            // NOTE: item mall purchases need to be persistent, yet
                            // resaving the player here is not necessary because if the
                            // server crashes before next save, then both the inventory
                            // and the coins will be reverted anyway.
                        }
                    }
                }

            }
        }
    }

    // coins can't be increased by an external application while the player is
    // ingame. we use an additional table to store new orders in and process
    // them every few seconds from here. this way we can even notify the player
    // after his order was processed successfully.
    //
    // note: the alternative is to keep player.coins in the database at all
    // times, but then we need RPCs and the client needs a .coins value anyway.
    [Server]
    void ProcessCoinOrders()
    {
        List<long> orders = Database.singleton.GrabCharacterOrders(name);
        foreach (long reward in orders)
        {
            coins += reward;
            Debug.Log("Processed order for: " + name + ";" + reward);
            string message = "Processed order for: " + reward;
            chat.TargetMsgInfo(message);
        }
    }

    // guild ///////////////////////////////////////////////////////////////////
    public bool InGuild()
    {
        return !string.IsNullOrWhiteSpace(guild.name);
    }

    [Server]
    public void SetGuildOnline(bool online)
    {
        // validate
        if (InGuild())
            GuildSystem.SetGuildOnline(guild.name, name, online);
    }



    [Command]
    public void CmdGuildInviteAccept()
    {
        // valid invitation, sender exists and is in a guild?
        // note: no distance check because sender might be far away already
        if (!InGuild() && guildInviteFrom != "" &&
            onlinePlayers.TryGetValue(guildInviteFrom, out Player sender) &&
            sender.InGuild())
        {
            // try to add. GuildSystem does all the checks.
            GuildSystem.AddToGuild(sender.guild.name, sender.name, name, level);
            GeneralManager.singleton.ResetBuildingCredential(sender.guild.name, name);
        }


        // reset guild invite in any case
        guildInviteFrom = "";
    }

    [Command]
    public void CmdGuildInviteDecline()
    {
        guildInviteFrom = "";
    }

    [Command]
    public void CmdGuildKick(string memberName)
    {
        // validate
        if (InGuild())
            GuildSystem.KickFromGuild(guild.name, name, memberName);
    }

    [Command]
    public void CmdGuildPromote(string memberName)
    {
        // validate
        if (InGuild())
            GuildSystem.PromoteMember(guild.name, name, memberName);
    }

    [Command]
    public void CmdGuildDemote(string memberName)
    {
        // validate
        if (InGuild())
            GuildSystem.DemoteMember(guild.name, name, memberName);
    }

    [Command]
    public void CmdSetGuildNotice(string notice)
    {
        // validate
        // (only allow changes every few seconds to avoid bandwidth issues)
        if (InGuild() && NetworkTime.time >= nextRiskyActionTime)
        {
            // try to set notice
            GuildSystem.SetGuildNotice(guild.name, name, notice);

        }

        // reset risky time no matter what. even if set notice failed, we don't
        // want people to spam attempts all the time.
        nextRiskyActionTime = NetworkTime.time + GuildSystem.NoticeWaitSeconds;
    }

    // helper function to check if we are near a guild manager npc
    public bool IsGuildManagerNear()
    {
        return target != null &&
               target is Npc &&
               ((Npc)target).offersGuildManagement &&
               Utils.ClosestDistance(collider, target.collider) <= interactionRange;
    }

    [Command]
    public void CmdTerminateGuild()
    {
        Database.singleton.DeleteGuildContaindAlly(guild.name);

        foreach (KeyValuePair<string, Player> players in onlinePlayers)
        {
            Player playerOnline = players.Value;
            //Now you can access the key and value both separately from this attachStat as:
            if (playerOnline.playerAlliance.guildAlly.Contains(guild.name))
            {
                playerOnline.playerAlliance.guildAlly.Remove(guild.name);
            }
        }
        // validate
        if (InGuild())
            //IsGuildManagerNear())
            GuildSystem.TerminateGuild(guild.name, name);
    }

    [Command]
    public void CmdCreateGuild(string guildName)
    {
        // validate
        if (health > 0 && gold >= GuildSystem.CreationPrice &&
            !InGuild())
        {
            // try to create the guild. pay for it if it worked.
            if (GuildSystem.CreateGuild(name, level, guildName))
            {
                for (int i = 0; i < quests.Count; i++)
                {
                    Quest quest = quests[i];
                    if (quest.checkCreateGuild == true)
                    {
                        quest.checkCreateGuild = true;
                        quests[i] = quest;
                    }
                }
                GeneralManager.singleton.ResetBuildingCredential(guildName, name);
                gold -= GuildSystem.CreationPrice;
            }
            else
                chat.TargetMsgInfo("Guild name invalid!");
        }
    }

    [Command]
    public void CmdLeaveGuild()
    {
        // validate
        if (InGuild())
            GuildSystem.LeaveGuild(guild.name, name);
    }

    // party ///////////////////////////////////////////////////////////////////
    public bool InParty()
    {
        // 0 means no party, because default party struct's partyId is 0.
        return party.partyId > 0;
    }

    // find party members in proximity for item/exp sharing etc.
    public List<Player> GetPartyMembersInProximity()
    {
        List<Player> players = new List<Player>();
        if (InParty())
        {
            // (avoid Linq because it is HEAVY(!) on GC and performance)
            foreach (NetworkConnection conn in netIdentity.observers.Values)
            {
                Player player = conn.identity.GetComponent<Player>();
                if (party.Contains(player.name))
                    players.Add(player);
            }
        }
        return players;
    }


    [Command]
    public void CmdPartyInviteAccept()
    {
        // valid invitation?
        // note: no distance check because sender might be far away already
        if (!InParty() && partyInviteFrom != "" &&
            onlinePlayers.TryGetValue(partyInviteFrom, out Player sender))
        {
            // is in party? then try to add
            if (sender.InParty())
            {
                for (int i = 0; i < sender.quests.Count; i++)
                {
                    Quest quest = sender.quests[i];
                    if (quest.createParty == true)
                    {
                        quest.checkCreateParty = true;
                    }
                    quests[i] = quest;
                }
                for (int i = 0; i < quests.Count; i++)
                {
                    Quest quest = quests[i];
                    if (quest.createParty == true)
                    {
                        quest.checkCreateParty = true;
                    }
                    quests[i] = quest;
                }
                PartySystem.AddToParty(sender.party.partyId, name);
                // otherwise try to form a new one
            }
            else
            {
                PartySystem.FormParty(sender.name, name);
                for (int i = 0; i < sender.quests.Count; i++)
                {
                    Quest quest = sender.quests[i];
                    if (quest.createParty == true)
                    {
                        quest.checkCreateParty = true;
                    }
                    quests[i] = quest;
                }
                for (int i = 0; i < quests.Count; i++)
                {
                    Quest quest = quests[i];
                    if (quest.createParty == true)
                    {
                        quest.checkCreateParty = true;
                    }
                    quests[i] = quest;
                }
            }
        }

        // reset party invite in any case
        partyInviteFrom = "";
    }

    [Command]
    public void CmdPartyInviteDecline()
    {
        partyInviteFrom = "";
    }

    [Command]
    public void CmdPartyKick(string member)
    {
        // try to kick. party system will do all the validation.
        PartySystem.KickFromParty(party.partyId, name, member);
    }

    // version without cmd because we need to call it from the server too
    public void PartyLeave()
    {
        // try to leave. party system will do all the validation.
        PartySystem.LeaveParty(party.partyId, name);
    }
    [Command]
    public void CmdPartyLeave() { PartyLeave(); }

    // version without cmd because we need to call it from the server too
    public void PartyDismiss()
    {
        // try to dismiss. party system will do all the validation.
        PartySystem.DismissParty(party.partyId, name);
    }
    [Command]
    public void CmdPartyDismiss() { PartyDismiss(); }

    [Command]
    public void CmdPartySetExperienceShare(bool value)
    {
        // try to set. party system will do all the validation.
        PartySystem.SetPartyExperienceShare(party.partyId, name, value);
    }

    [Command]
    public void CmdPartySetGoldShare(bool value)
    {
        // try to set. party system will do all the validation.
        PartySystem.SetPartyGoldShare(party.partyId, name, value);
    }

    // pet /////////////////////////////////////////////////////////////////////
    // helper function for command and UI
    public bool CanUnsummonPet()
    {
        // only while pet and owner aren't fighting
        return activePet != null &&
               (state == "IDLE" || state == "MOVING") &&
               (activePet.state == "IDLE" || activePet.state == "MOVING" || activePet.state == "DEAD");
    }

    [Command]
    public void CmdPetUnsummon()
    {
        // validate
        if (CanUnsummonPet())
        {
            activePet.SyncToOwner();
            // destroy from world. item.summoned and activePet will be null.
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i].amount == 0) continue;
                if (inventory[i].item.data is PetItem)
                {
                    ItemSlot petItem = inventory[i];
                    if (petItem.item.isSummoned)
                    {
                        petItem.item.isSummoned = false;
                        inventory[i] = petItem;
                    }
                }
            }

            for (int i = 0; i < playerBelt.belt.Count; i++)
            {
                if (playerBelt.belt[i].amount == 0) continue;
                if (playerBelt.belt[i].item.data is PetItem)
                {
                    ItemSlot petItem = playerBelt.belt[i];
                    if (petItem.item.isSummoned)
                    {
                        petItem.item.isSummoned = false;
                        playerBelt.belt[i] = petItem;
                    }
                }
            }
            NetworkServer.Destroy(activePet.gameObject);
            activePet = null;
        }
    }

    [Command]
    public void CmdNpcReviveSummonable(int index)
    {
        // validate: close enough, npc alive and valid index and valid item?
        // use collider point(s) to also work with big entities
        if (state == "IDLE" &&
            target != null &&
            target.health > 0 &&
            target is Npc &&
            ((Npc)target).offersSummonableRevive &&
            Utils.ClosestDistance(collider, target.collider) <= interactionRange &&
            0 <= index && index < inventory.Count)
        {
            ItemSlot slot = inventory[index];
            if (slot.amount > 0 && slot.item.data is SummonableItem)
            {
                // verify the pet status
                SummonableItem itemData = (SummonableItem)slot.item.data;
                if (slot.item.summonedHealth == 0 && itemData.summonPrefab != null)
                {
                    // enough gold?
                    if (gold >= itemData.revivePrice)
                    {
                        // pay for it, revive it
                        gold -= itemData.revivePrice;
                        slot.item.summonedHealth = itemData.summonPrefab.healthMax;
                        inventory[index] = slot;
                    }
                }
            }
        }
    }

    // mounts //////////////////////////////////////////////////////////////////
    public bool IsMounted()
    {
        return activeMount != null && activeMount.health > 0;
    }

    // selection handling //////////////////////////////////////////////////////
    public void SetIndicatorViaParent(Transform parent)
    {
        if (!indicator) indicator = Instantiate(indicatorPrefab);
        //indicator.transform.SetParent(parent, true);
        indicator.transform.position = parent.position;
    }

    public void SetIndicatorViaPosition(Vector2 pos)
    {
        if (!indicator) indicator = Instantiate(indicatorPrefab);
        indicator.transform.parent = null;
        indicator.transform.position = pos;
    }

    // clear indicator if there is one, and if it's not on a target
    public void ClearIndicatorIfNoParent()
    {
        if (indicator != null && indicator.transform.parent == null)
            Destroy(indicator);
    }

    [Command]
    public void CmdSetTarget(NetworkIdentity ni)
    {
        // validate
        if (ni != null)
        {
            // can directly change it, or change it after casting?
            if (state == "IDLE" || state == "MOVING" || state == "STUNNED")
            {
                if (ni.GetComponent<Building>())
                {
                    Building building = ni.GetComponent<Building>();

                    if (building && GeneralManager.singleton.CanManageExplosiveBuilding(building, this))
                        target = ni.GetComponent<Entity>();
                    else if (building && CanInteractBuildingTarget(building, this))
                    {
                        target = ni.GetComponent<Entity>();
                    }
                }
                else
                {
                    target = ni.GetComponent<Entity>();
                }
            }
            else if (state == "CASTING")
            {
                if (ni.GetComponent<Building>())
                {
                    Building building = ni.GetComponent<Building>();

                    if (building && GeneralManager.singleton.CanManageExplosiveBuilding(building, this))
                        target = ni.GetComponent<Entity>();
                    else if (building && CanInteractBuildingTarget(building, this))
                    {
                        target = ni.GetComponent<Entity>();
                    }
                }
            }
        }
    }

    public void SetTarget(NetworkIdentity ni)
    {
        // validate
        if (ni != null)
        {
            // can directly change it, or change it after casting?
            if (state == "IDLE" || state == "MOVING" || state == "STUNNED")
            {
                if (ni.GetComponent<Building>())
                {
                    Building building = ni.GetComponent<Building>();

                    if (building && GeneralManager.singleton.CanManageExplosiveBuilding(building, this))
                        target = ni.GetComponent<Entity>();
                    else if (building && CanInteractBuildingTarget(building, this))
                    {
                        target = ni.GetComponent<Entity>();
                    }
                }
                else
                {
                    target = ni.GetComponent<Entity>();
                }
            }
            else if (state == "CASTING")
            {
                if (ni.GetComponent<Building>())
                {
                    Building building = ni.GetComponent<Building>();

                    if (building && GeneralManager.singleton.CanManageExplosiveBuilding(building, this))
                        target = ni.GetComponent<Entity>();
                    else if (building && CanInteractBuildingTarget(building, this))
                    {
                        target = ni.GetComponent<Entity>();
                    }
                }
            }
        }
    }

    public Vector3 lastDirection;
    public float horizontal;
    public float vertical;
    //public Vector2 oldDirection;
    //[SyncVar]
    public Vector2 direction;
    [Client]
    void WASDHandling()
    {

        if (playerCar._car)
        {
            if (!playerCar.car.On) return;

            if (playerCar.car._pilot != string.Empty && playerCar.car._pilot != name)
            {
                return;
            }
            if (playerCar.car.currentGasoline == 0) return;
        }

        // don't move if currently typing in an input
        // we check this after checking h and v to save computations
        if (useJoystick)
        {
            horizontal = joystick.output.x;
            vertical = joystick.output.z;
        }
        else
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
        }

        if (horizontal != 0 || vertical != 0)
        {
            // create direction, normalize in case of diagonal movement
            direction = new Vector2(horizontal, vertical);
            if (direction.magnitude > 1) direction = direction.normalized;

            //if (oldDirection != direction)
            //    CmdSetDirection(oldDirection.x, oldDirection.y, 0.0f);

            // draw direction for debugging
            Debug.DrawLine(transform.position, transform.position + (Vector3)direction, Color.green, 0, false);

            // clear indicator if there is one, and if it's not on a target
            // (simply looks better)
            if (direction != Vector2.zero)
                ClearIndicatorIfNoParent();

            // cancel path if we are already doing click movement, otherwise
            // we will slide
            agent.ResetMovement();


            // casting? then set pending velocity
            if (state == "CASTING")
            {
                pendingVelocity = direction * speed;
                pendingVelocityValid = true;
            }
            else
            {
                agent.velocity = direction * speed;
            }

            // clear requested skill in any case because if we clicked
            // somewhere else then we don't care about it anymore
            useSkillWhenCloser = -1;
        }
        //else
        //{
        //    agent.ResetMovement();
        //}
        //}
    }

    // simple tab targeting
    [Client]
    void TargetNearest()
    {
        if (Input.GetKeyDown(targetNearestKey))
        {
            // find all monsters that are alive, sort by distance
            GameObject[] objects = GameObject.FindGameObjectsWithTag("Monster");
            List<Monster> monsters = objects.Select(go => go.GetComponent<Monster>()).Where(m => m.health > 0).ToList();
            List<Monster> sorted = monsters.OrderBy(m => Vector2.Distance(transform.position, m.transform.position)).ToList();

            // target nearest one
            if (sorted.Count > 0)
            {
                //SetIndicatorViaParent(sorted[0].transform);
                CmdSetTarget(sorted[0].netIdentity);
            }
        }
    }

    public void ChangeAnimatorAndWeapon()
    {
        if (!playerMove.bodyPlayer)
        {
            if (playerMove.dummyBodyPlayer.childCount > 0)
                playerMove.bodyPlayer = playerMove.dummyBodyPlayer.GetChild(0);
        }

        playerItemEquipment.CheckFirstWeapon();

        if (playerItemEquipment.firstWeapon.amount > 0)
        {
            PlayerAnimatorList playerAnimatorList = playerMove.bodyPlayer.GetComponent<PlayerAnimatorList>();
            for (int i = 0; i < playerAnimatorList.animatorController.Count; i++)
            {
                int index = i;
                playerAnimatorList.animatorController[index].runtimeAnimatorController = ((EquipmentItem)(playerItemEquipment.firstWeapon.item.data)).animatorToSet;
            }
        }
        else
        {
            PlayerAnimatorList playerAnimatorList = playerMove.bodyPlayer.GetComponent<PlayerAnimatorList>();
            for (int i = 0; i < playerAnimatorList.animatorController.Count; i++)
            {
                int index = i;
                playerAnimatorList.animatorController[index].runtimeAnimatorController = GeneralManager.singleton.defaultAnimatorController;
            }
        }
    }

    // ontrigger ///////////////////////////////////////////////////////////////
    protected override void OnTriggerEnter2D(Collider2D col)
    {
        // call base function too
        base.OnTriggerEnter2D(col);

        // quest location?
        // (we use .CompareTag to avoid .tag allocations)
        if (col.CompareTag("QuestLocation"))
            QuestsOnLocation(col);
    }

    // drag and drop ///////////////////////////////////////////////////////////
    void OnDragAndDrop_InventorySlot_InventorySlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo

        // merge? check Equals because name AND dynamic variables matter (petLevel etc.)
        if (inventory[slotIndices[0]].amount > 0 && inventory[slotIndices[1]].amount > 0 &&
            inventory[slotIndices[0]].item.Equals(inventory[slotIndices[1]].item))
        {
            CmdInventoryMerge(slotIndices[0], slotIndices[1]);
        }
        // split?
        else if (Utils.AnyKeyPressed(inventorySplitKeys))
        {
            CmdInventorySplit(slotIndices[0], slotIndices[1]);
        }
        // swap?
        else
        {
            CmdSwapInventoryInventory(slotIndices[0], slotIndices[1]);
        }
    }

    void OnDragAndDrop_InventorySlot_TrashSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        CmdSwapInventoryTrash(slotIndices[0]);
    }

    void OnDragAndDrop_InventorySlot_EquipmentSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo

        // merge? check Equals because name AND dynamic variables matter (petLevel etc.)
        // => merge is important when dragging more arrows into an arrow slot!
        if (inventory[slotIndices[0]].amount > 0 && equipment[slotIndices[1]].amount > 0 &&
            inventory[slotIndices[0]].item.Equals(equipment[slotIndices[1]].item))
        {
            CmdMergeInventoryEquip(slotIndices[0], slotIndices[1]);
        }
        // swap?
        else
        {
            CmdSwapInventoryEquip(slotIndices[0], slotIndices[1]);
        }
    }

    //void OnDragAndDrop_InventorySlot_SkillbarSlot(int[] slotIndices)
    //{
    //    // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
    //    skillbar[slotIndices[1]].reference = inventory[slotIndices[0]].item.name; // just save it clientsided
    //}

    void OnDragAndDrop_InventorySlot_SkillbarSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo

        // merge? check Equals because name AND dynamic variables matter (petLevel etc.)
        // => merge is important when dragging more arrows into an arrow slot!
        if (inventory[slotIndices[0]].amount > 0 && playerBelt.belt[slotIndices[1]].amount > 0 &&
            inventory[slotIndices[0]].item.Equals(playerBelt.belt[slotIndices[1]].item))
        {
            playerBelt.CmdMergeInventoryBelt(slotIndices[0], slotIndices[1]);
        }
        // swap?
        else
        {
            playerBelt.CmdSwapInventoryBelt(slotIndices[0], slotIndices[1]);
        }
    }

    void OnDragAndDrop_SkillbarSlot_SkillbarSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo

        // merge? check Equals because name AND dynamic variables matter (petLevel etc.)
        if (playerBelt.belt[slotIndices[0]].amount > 0 && playerBelt.belt[slotIndices[1]].amount > 0 &&
            playerBelt.belt[slotIndices[0]].item.Equals(playerBelt.belt[slotIndices[1]].item))
        {
            playerBelt.CmdBeltMerge(slotIndices[0], slotIndices[1]);
        }
        // split?
        else if (Utils.AnyKeyPressed(inventorySplitKeys))
        {
            playerBelt.CmdBeltSplit(slotIndices[0], slotIndices[1]);
        }
        // swap?
        else
        {
            playerBelt.CmdSwapBeltBelt(slotIndices[0], slotIndices[1]);
        }
    }

    void OnDragAndDrop_SkillbarSlot_InventorySlot(int[] slotIndices)
    {
        if (playerBelt.belt[slotIndices[0]].amount > 0 && inventory[slotIndices[1]].amount > 0 &&
            playerBelt.belt[slotIndices[0]].item.Equals(inventory[slotIndices[1]].item))
        {
            //CmdMergeEquipInventory(slotIndices[0], slotIndices[1]);
            playerBelt.CmdMergeBeltInventory(slotIndices[0], slotIndices[1]);
        }
        // swap?
        else
        {
            //CmdSwapInventoryEquip(slotIndices[1], slotIndices[0]); // reversed
            playerBelt.CmdSwapInventoryBelt(slotIndices[1], slotIndices[0]); // reversed
        }
    }

    void OnDragAndDrop_InventorySlot_NpcSellSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        ItemSlot slot = inventory[slotIndices[0]];
        if (slot.item.sellable && !slot.item.summoned)
        {
            UINpcTrading.singleton.sellIndex = slotIndices[0];
            UINpcTrading.singleton.sellAmountInput.text = slot.amount.ToString();
        }
    }

    void OnDragAndDrop_InventorySlot_TradingSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        if (inventory[slotIndices[0]].item.tradable)
            CmdTradeOfferItem(slotIndices[0], slotIndices[1]);
    }

    void OnDragAndDrop_InventorySlot_CraftingIngredientSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        // only if not crafting right now
        if (craftingState != CraftingState.InProgress)
        {
            if (!craftingIndices.Contains(slotIndices[0]))
            {
                craftingIndices[slotIndices[1]] = slotIndices[0];
                craftingState = CraftingState.None; // reset state
            }
        }
    }

    void OnDragAndDrop_TrashSlot_InventorySlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        CmdSwapTrashInventory(slotIndices[1]);
    }

    void OnDragAndDrop_EquipmentSlot_InventorySlot(int[] slotIndices)
    {
        // merge? check Equals because name AND dynamic variables matter (petLevel etc.)
        // => merge is important when dragging more arrows into an arrow slot!
        if (equipment[slotIndices[0]].amount > 0 && inventory[slotIndices[1]].amount > 0 &&
            equipment[slotIndices[0]].item.Equals(inventory[slotIndices[1]].item))
        {
            CmdMergeEquipInventory(slotIndices[0], slotIndices[1]);
        }
        // swap?
        else
        {
            CmdSwapInventoryEquip(slotIndices[1], slotIndices[0]); // reversed
        }
    }

    void OnDragAndDrop_EquipmentSlot_SkillbarSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        skillbar[slotIndices[1]].reference = equipment[slotIndices[0]].item.name; // just save it clientsided
    }

    void OnDragAndDrop_SkillsSlot_SkillbarSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        skillbar[slotIndices[1]].reference = skills[slotIndices[0]].name; // just save it clientsided
    }

    //void OnDragAndDrop_SkillbarSlot_SkillbarSlot(int[] slotIndices)
    //{
    //    // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
    //    // just swap them clientsided
    //    string temp = skillbar[slotIndices[0]].reference;
    //    skillbar[slotIndices[0]].reference = skillbar[slotIndices[1]].reference;
    //    skillbar[slotIndices[1]].reference = temp;
    //}

    void OnDragAndDrop_CraftingIngredientSlot_CraftingIngredientSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        // only if not crafting right now
        if (craftingState != CraftingState.InProgress)
        {
            // just swap them clientsided
            int temp = craftingIndices[slotIndices[0]];
            craftingIndices[slotIndices[0]] = craftingIndices[slotIndices[1]];
            craftingIndices[slotIndices[1]] = temp;
            craftingState = CraftingState.None; // reset state
        }
    }

    void OnDragAndDrop_InventorySlot_NpcReviveSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        if (inventory[slotIndices[0]].item.data is PetItem)
            UINpcRevive.singleton.itemIndex = slotIndices[0];
    }

    void OnDragAndClear_SkillbarSlot(int slotIndex)
    {
        skillbar[slotIndex].reference = "";
    }

    void OnDragAndClear_TradingSlot(int slotIndex)
    {
        CmdTradeOfferItemClear(slotIndex);
    }

    void OnDragAndClear_NpcSellSlot(int slotIndex)
    {
        UINpcTrading.singleton.sellIndex = -1;
    }

    void OnDragAndClear_CraftingIngredientSlot(int slotIndex)
    {
        // only if not crafting right now
        if (craftingState != CraftingState.InProgress)
        {
            craftingIndices[slotIndex] = -1;
            craftingState = CraftingState.None; // reset state
        }
    }

    void OnDragAndClear_NpcReviveSlot(int slotIndex)
    {
        UINpcRevive.singleton.itemIndex = -1;
    }

    // validation //////////////////////////////////////////////////////////////
    void OnValidate()
    {
        // make sure that the NetworkNavMeshAgentRubberbanding2D component is
        // ABOVE the player component, so that it gets updated before Player.cs.
        // -> otherwise it overwrites player's WASD velocity for local player
        //    hosts
        // -> there might be away around it, but a warning is good for now
        Component[] components = GetComponents<Component>();
        if (Array.IndexOf(components, GetComponent<NetworkNavMeshAgentRubberbanding2D>()) >
            Array.IndexOf(components, this))
            Debug.LogWarning(name + "'s NetworkNavMeshAgentRubberbanding2D component is below the Player component. Please drag it above the Player component in the Inspector, otherwise there might be WASD movement issues due to the Update order.");

        // equipment slots:
        // it's easy to set a default item and forget to set amount from 0 to 1
        // -> let's do this automatically.
        for (int i = 0; i < equipmentInfo.Length; ++i)
            if (equipmentInfo[i].defaultItem.item != null && equipmentInfo[i].defaultItem.amount == 0)
                equipmentInfo[i].defaultItem.amount = 1;

        // inventory default items:
        // it's easy to set a default item and forget to set amount from 0 to 1
        // -> let's do this automatically.
        for (int i = 0; i < defaultItems.Length; ++i)
            if (defaultItems[i].item != null && defaultItems[i].amount == 0)
                defaultItems[i].amount = 1;
    }

    [Command]
    public void CmdTraceShopQuest()
    {
        for (int i = 0; i < quests.Count; i++)
        {
            Quest quest = quests[i];
            if (quest.data.openShop == true)
            {
                quest.checkOpenShop = true;
            }
            quests[i] = quest;
        }
    }

    #region Custom part

    [HideInInspector] public CustomType.GeneralPart generalPart;
    [Header("Guild ally")]
    [SyncVar] public string guildAllyInviteName;
    [SyncVar] public string guildAllyInviteGuildName;

    #region Player custom Start
    [Header("CE part")]
    public PlayerAbility playerAbility;
    public PlayerAccuracy playerAccuracy;
    public PlayerMiss playerMiss;
    public PlayerAlliance playerAlliance;
    public PlayerArmor playerArmor;
    public PlayerBoost playerBoost;
    public PlayerConservative playerConservative;
    public PlayerEmoji playerEmoji;
    public PlayerFoodUnsanity playerFoodUnsanity;
    public PlayerHungry playerHungry;
    public PlayerThirsty playerThirsty;
    public PlayerPoisoning playerPoisoning;
    public PlayerTemperature playerTemperature;
    public PlayerItemEquipment playerItemEquipment;
    public PlayerLeaderPoints playerLeaderPoints;
    public PlayerPremiumZoneManager playerPremiumZoneManager;
    public PlayerRadio playerRadio;
    public PlayerTorch playerTorch;
    public PlayerWeight playerWeight;
    public PlayerSpawnpoint playerSpawnpoint;
    public PlayerBuilding playerBuilding;
    public PlayerMunitionManager playerMunitionManager;
    public PlayerMonsterGrab playerMonsterGrab;
    public PlayerFired playerFired;
    public PlayerElectric playerElectric;
    public PlayerMarriage playerMarriage;
    public PlayerMove playerMove;
    public PlayerFriend playerFriend;
    public PlayerOptions playerOptions;
    public PlayerBlood playerBlood;
    public PlayerCar playerCar;
    public PlayerRock playerRock;
    public PlayerTree playerTree;
    public EntityObstacleCheck playerObstacleCheck;
    public PlayerWet playerWet;
    public PlayerTeleport playerTeleport;
    public PlayerBelt playerBelt;
    public PlayerItemPoint playerItemPoint;
    public PlayerFootPrint playerFootPrint;
    public PlayerCreation playerCreation;
    public PlayerQuest playerQuest;
    public PlayerDance playerDance;

    // joystick
    public Joystick joystick;
    public bool useJoystick = false;

    #endregion

    #region Player Net Part

    #region Item
    [Command]
    public void CmdDeleteItem(int index)
    {
        if (inventory[index].amount > 0 && inventory[index].item.data.destroyable)
        {
            ItemSlot slot = new ItemSlot();
            slot.amount = 0;
            inventory[index] = slot;
        }
    }

    [Command]
    public void CmdUseWaterItem(int liquidToAdd, int inventoryIndex, bool belt)
    {
        if (!belt)
        {
            if (inventory[inventoryIndex].item.waterContainer >= liquidToAdd)
            {
                playerThirsty.currentThirsty += liquidToAdd;
                if (playerThirsty.currentThirsty > playerThirsty.maxThirsty) playerThirsty.currentThirsty = playerThirsty.maxThirsty;
                ItemSlot slot = inventory[inventoryIndex];
                slot.item.waterContainer -= liquidToAdd;
                inventory[inventoryIndex] = slot;

                for (int i = 0; i < quests.Count; i++)
                {
                    Quest quest = quests[i];
                    if (quest.data.drink == true)
                    {
                        quest.checkDrink = true;
                    }
                    quests[i] = quest;
                }

            }
        }
        else
        {
            if (playerBelt.belt[inventoryIndex].item.waterContainer >= liquidToAdd)
            {
                playerThirsty.currentThirsty += liquidToAdd;
                if (playerThirsty.currentThirsty > playerThirsty.maxThirsty) playerThirsty.currentThirsty = playerThirsty.maxThirsty;
                ItemSlot slot = playerBelt.belt[inventoryIndex];
                slot.item.waterContainer -= liquidToAdd;
                playerBelt.belt[inventoryIndex] = slot;

                for (int i = 0; i < quests.Count; i++)
                {
                    Quest quest = quests[i];
                    if (quest.data.drink == true)
                    {
                        quest.checkDrink = true;
                    }
                    quests[i] = quest;
                }

            }

        }
    }

    [Command]
    public void CmdUseHoneyItem(int liquidToAdd, int inventoryIndex, bool belt)
    {
        if (!belt)
        {
            if (inventory[inventoryIndex].item.honeyContainer >= liquidToAdd)
            {
                playerThirsty.currentThirsty += liquidToAdd;
                if (playerThirsty.currentThirsty > playerThirsty.maxThirsty) playerThirsty.currentThirsty = playerThirsty.maxThirsty;
                playerHungry.currentHungry += Convert.ToInt32(liquidToAdd / 3);
                if (playerHungry.currentHungry > playerHungry.maxHungry) playerHungry.currentHungry = playerHungry.maxHungry;

                ItemSlot slot = inventory[inventoryIndex];
                slot.item.honeyContainer -= liquidToAdd;
                inventory[inventoryIndex] = slot;

                for (int i = 0; i < quests.Count; i++)
                {
                    Quest quest = quests[i];
                    if (quest.data.drink == true)
                    {
                        quest.checkDrink = true;
                    }
                    quests[i] = quest;
                }

            }
        }
        else
        {
            if (playerBelt.belt[inventoryIndex].item.honeyContainer >= liquidToAdd)
            {
                playerThirsty.currentThirsty += liquidToAdd;
                if (playerThirsty.currentThirsty > playerThirsty.maxThirsty) playerThirsty.currentThirsty = playerThirsty.maxThirsty;
                playerHungry.currentHungry += Convert.ToInt32(liquidToAdd / 3);
                if (playerHungry.currentHungry > playerHungry.maxHungry) playerHungry.currentHungry = playerHungry.maxHungry;

                ItemSlot slot = playerBelt.belt[inventoryIndex];
                slot.item.honeyContainer -= liquidToAdd;
                playerBelt.belt[inventoryIndex] = slot;

                for (int i = 0; i < quests.Count; i++)
                {
                    Quest quest = quests[i];
                    if (quest.data.drink == true)
                    {
                        quest.checkDrink = true;
                    }
                    quests[i] = quest;
                }

            }

        }
    }

    #endregion

    #region Ammo

    [Command]
    public void CmdSetAmmo(string ammoIndex)
    {
        int index = GetInventoryIndexByName(ammoIndex);
        // validate
        if (InventoryOperationsAllowed() &&
            0 <= index && index < inventory.Count && inventory[index].amount > 0 &&
            inventory[index].item.data is UsableItem)
        {
            // use item
            // note: we don't decrease amount / destroy in all cases because
            // some items may swap to other slots in .Use()
            UsableItem itemData = (UsableItem)inventory[index].item.data;
            if (itemData.CanUse(this, index))
            {
                // .Use might clear the slot, so we backup the Item first for the Rpc
                Item item = inventory[index].item;
                itemData.Use(this, index);
                RpcUsedItem(item);
            }
            if (equipment[0].amount > 0 && ((WeaponItem)equipment[0].item.data).ammoItems.Count > 0)
            {
                ItemSlot slot = equipment[0];
                slot.item.alreadyShooted = 0;
                equipment[0] = slot;
            }
            return;
        }

        // not finded in inventory try in belt
        if (index == -1)
        {
            index = GetBeltIndexByName(ammoIndex);
            if (InventoryOperationsAllowed() &&
            0 <= index && index < playerBelt.belt.Count && playerBelt.belt[index].amount > 0 &&
            playerBelt.belt[index].item.data is UsableItem)
            {
                // use item
                // note: we don't decrease amount / destroy in all cases because
                // some items may swap to other slots in .Use()
                UsableItem itemData = (UsableItem)playerBelt.belt[index].item.data;
                if (itemData.CanUse(this, index))
                {
                    // .Use might clear the slot, so we backup the Item first for the Rpc
                    Item item = playerBelt.belt[index].item;
                    itemData.Use(this, index);
                    RpcUsedItem(item);
                }
                if (equipment[0].amount > 0 && ((WeaponItem)equipment[0].item.data).ammoItems.Count > 0)
                {
                    ItemSlot slot = equipment[0];
                    slot.item.alreadyShooted = 0;
                    equipment[0] = slot;
                }
                return;
            }
        }
    }


    public void SetAmmo(string ammoIndex)
    {
        int index = GetInventoryIndexByName(ammoIndex);
        // validate
        if (InventoryOperationsAllowed() &&
            0 <= index && index < inventory.Count && inventory[index].amount > 0 &&
            inventory[index].item.data is UsableItem)
        {
            // use item
            // note: we don't decrease amount / destroy in all cases because
            // some items may swap to other slots in .Use()
            UsableItem itemData = (UsableItem)inventory[index].item.data;
            if (itemData.CanUse(this, index))
            {
                // .Use might clear the slot, so we backup the Item first for the Rpc
                Item item = inventory[index].item;
                itemData.Use(this, index);
                RpcUsedItem(item);
            }
            if (equipment[0].amount > 0 && ((WeaponItem)equipment[0].item.data).ammoItems.Count > 0)
            {
                ItemSlot slot = equipment[0];
                slot.item.alreadyShooted = 0;
                equipment[0] = slot;
            }
            return;
        }

        // not finded in inventory try in belt
        if (index == -1)
        {
            index = GetBeltIndexByName(ammoIndex);
            if (InventoryOperationsAllowed() &&
            0 <= index && index < playerBelt.belt.Count && playerBelt.belt[index].amount > 0 &&
            playerBelt.belt[index].item.data is UsableItem)
            {
                // use item
                // note: we don't decrease amount / destroy in all cases because
                // some items may swap to other slots in .Use()
                UsableItem itemData = (UsableItem)playerBelt.belt[index].item.data;
                if (itemData.CanUse(this, index))
                {
                    // .Use might clear the slot, so we backup the Item first for the Rpc
                    Item item = playerBelt.belt[index].item;
                    itemData.Use(this, index);
                    RpcUsedItem(item);
                }
                if (equipment[0].amount > 0 && ((WeaponItem)equipment[0].item.data).ammoItems.Count > 0)
                {
                    ItemSlot slot = equipment[0];
                    slot.item.alreadyShooted = 0;
                    equipment[0] = slot;
                }
                return;
            }
        }
    }

    public int GetAmmoIndex(string ammoIndex)
    {
        int index = GetInventoryIndexByName(ammoIndex);

        if (index > -1)
        {
            return index;
        }
        else
        {
            index = GetBeltIndexByName(ammoIndex);
            if (InventoryOperationsAllowed() &&
            0 <= index && index < playerBelt.belt.Count && playerBelt.belt[index].amount > 0 &&
            playerBelt.belt[index].item.data is UsableItem)
            {
                // use item
                // note: we don't decrease amount / destroy in all cases because
                // some items may swap to other slots in .Use()
                UsableItem itemData = (UsableItem)playerBelt.belt[index].item.data;
                if (itemData.CanUse(this, index))
                {
                    // .Use might clear the slot, so we backup the Item first for the Rpc
                    Item item = playerBelt.belt[index].item;
                    itemData.Use(this, index);
                    RpcUsedItem(item);
                }
                if (equipment[0].amount > 0 && ((WeaponItem)equipment[0].item.data).ammoItems.Count > 0)
                {
                    ItemSlot slot = equipment[0];
                    slot.item.alreadyShooted = 0;
                    equipment[0] = slot;
                }
                return index;
            }
        }

        return -1;
    }



    public void AddShootedAmmo()
    {
        ItemSlot slot = equipment[0];
        if (((WeaponItem)slot.item.data).ammoItems.Count > 0)
        {
            if (((WeaponItem)slot.item.data).chargeMunition.Get(slot.item.chargeLevel) > slot.item.alreadyShooted)
            {
                slot.item.alreadyShooted++;
                equipment[0] = slot;
            }
        }
    }

    #endregion

    #region Equip
    [Command]
    public void CmdSwapEquipEquip(int inventoryIndex, int equipmentIndex)
    {
        SwapEquipEquip(inventoryIndex, equipmentIndex);
    }

    [Server]
    public void SwapEquipEquip(int inventoryIndex, int equipmentIndex)
    {
        // validate: make sure that the slots actually exist in the inventory
        // and in the equipment
        if (health > 0 &&
             0 <= inventoryIndex && inventoryIndex < equipment.Count &&
             0 <= equipmentIndex && equipmentIndex < equipment.Count)
        {
            if ((inventoryIndex > 1) || (equipmentIndex > 1))
            {
                return;
            }

            // item slot has to be empty (unequip) or equipable
            ItemSlot slot = equipment[inventoryIndex];

            ItemSlot temp = equipment[equipmentIndex];
            equipment[inventoryIndex] = temp;
            equipment[equipmentIndex] = slot;
        }
    }

    [Command]
    public void CmdUnequip(int equipmentIndex)
    {
        UnequipItem(equipmentIndex);
    }

    [Server]
    public void UnequipItem(int equipmentIndex)
    {
        if (FindFirstFreeSlotInventory() != -1)
        {
            if (equipment[equipmentIndex].item.data is EquipmentItem)
            {
                if (CanUnEquip(equipment[equipmentIndex].item))
                {
                    inventory[FindFirstFreeSlotInventory()] = equipment[equipmentIndex];
                    equipment[equipmentIndex] = new ItemSlot();
                }
            }
            else
            {
                inventory[FindFirstFreeSlotInventory()] = equipment[equipmentIndex];
                equipment[equipmentIndex] = new ItemSlot();
            }
        }
    }

    public int FindFirstFreeSlotInventory()
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].amount == 0)
            {
                return i;
            }
        }
        return -1;
    }


    void OnDragAndDrop_EquipmentSlot_EquipmentSlot(int[] slotIndices)
    {
        CmdSwapEquipEquip(slotIndices[1], slotIndices[0]); // reversed
    }

    public int GetEquipmentIndexByCategory(string categoryName)
    {
        for (int i = 0; i < equipmentInfo.Length; i++)
        {
            int index = i;
            if (equipmentInfo[index].requiredCategory.StartsWith(categoryName))
            {
                return index;
            }
        }
        return -1;
    }

    #endregion

    #region Conservative

    [Command]
    public void CmdTakeLootItemFromChest(int index)
    {
        // validate: dead monster and close enough and valid loot index?
        // use collider point(s) to also work with big entities
        if (target != null && target is Chest &&
            Utils.ClosestDistance(collider, target.collider) <= interactionRange &&
            0 <= index && index < target.inventory.Count)
        {
            ItemSlot slot = target.inventory[index];

            // try to add it to the inventory, clear monster slot if it worked
            if (InventoryAdd(slot.item, slot.amount))
            {
                target.inventory.Remove(slot);
            }
            if (target.inventory.Count == 0) Destroy(target.gameObject);
        }
    }

    [Command]
    public void CmdTakeLootGoldFromChest()
    {
        // validate: dead monster and close enough?
        // use collider point(s) to also work with big entities
        if ((state == "IDLE" || state == "MOVING" || state == "CASTING") &&
            target != null && target is Chest && target.health == 0 &&
            Utils.ClosestDistance(collider, target.collider) <= interactionRange)
        {
            gold += target.gold;

            // reset target gold
            target.gold = 0;
        }
    }

    [Client]
    void SelectionHandling()
    {
        // click raycasting if not over a UI element & not pinching on mobile
        // note: this only works if the UI's CanvasGroup blocks Raycasts
        if (Input.GetMouseButtonDown(0) && !Utils.IsCursorOverUserInterface() && Input.touchCount <= 1 && useJoystick == false)
        {
            // cast a 3D ray from the camera towards the 2D scene.
            // Physics2D.Raycast isn't made for that, we use GetRayIntersection.
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            // raycast with local player ignore option
            RaycastHit2D hit = localPlayerClickThrough ? Utils.Raycast2DWithout(ray, gameObject) : Physics2D.GetRayIntersection(ray);

            // clear requested skill in any case because if we clicked
            // somewhere else then we don't care about it anymore
            useSkillWhenCloser = -1;

            // valid target?
            Entity entity = hit.transform != null ? hit.transform.GetComponent<Entity>() : null;
            if (entity)
            {
                // set indicator
                SetIndicatorViaParent(hit.transform);

                // clicked last target again? and is not self or pet?
                if (entity == target && entity != this && entity != activePet)
                {
                    if (entity.GetComponent<Building>() && !CanInteractBuildingTarget(entity.GetComponent<Building>(), ((Player)this)) && entity.GetComponent<Totem>())
                    {
                        if (GeneralManager.singleton.uiTotem == null)
                        {
                            GeneralManager.singleton.uiTotem = Instantiate(GeneralManager.singleton.totemPrefab, GeneralManager.singleton.canvas);
                            return;
                        }
                    }

                    if (entity.GetComponent<Building>() && CanInteractBuildingTarget(entity.GetComponent<Building>(), ((Player)this)))
                    {
                        CanInteract(entity.GetComponent<Building>(), this);
                        return;
                    }

                    if (this.CanAttack(entity))
                    {
                        // then try to use that one
                        TryUseSkill(FindNetworkSkill(((WeaponItem)playerItemEquipment.firstWeapon.item.data).requiredSkill));
                    }
                    // npc, alive, close enough? => talk
                    // use collider point(s) to also work with big entities
                    if (entity is Npc && entity.health > 0 &&
                             Utils.ClosestDistance(collider, entity.collider) <= interactionRange)
                    {
                        UINpcDialogue.singleton.Show();
                    }
                    else if (entity is Car && entity.health > 0 &&
                            Utils.ClosestDistance(collider, entity.collider) <= 2)
                    {
                        GameObject g = Instantiate(GeneralManager.singleton.carPanelToSpawn, GeneralManager.singleton.canvas);
                        playerBuilding.building = null;
                        Destroy(playerBuilding.actualBuilding);
                        playerBuilding.inventoryIndex = -1;
                        Destroy(GeneralManager.singleton.spawnedBuildingObject);
                    }


                    // monster, dead, has loot, close enough? => loot
                    // use collider point(s) to also work with big entities
                    else if (entity is Monster && entity.health == 0 &&
                             Utils.ClosestDistance(collider, entity.collider) <= interactionRange &&
                             ((Monster)entity).HasLoot())
                    {
                        UILoot.singleton.Show();
                    }
                    else if (entity is Chest &&
                             Utils.ClosestDistance(collider, entity.collider) <= interactionRange &&
                             ((Chest)entity).HasLoot())
                    {
                        GeneralManager.singleton.uiChestPanel.SetActive(true);
                    }
                    // not attackable, lootable, talkable, etc., but it's
                    // still an entity and double clicking it without doing
                    // anything would be strange.
                    // (e.g. if we are in a safe zone and click on a
                    //  monster. it's not attackable, but we should at least
                    //  move there, otherwise double click feels broken)
                    else
                    {
                        // use collider point(s) to also work with big entities
                        //agent.stoppingDistance = interactionRange;
                        //agent.destination = entity.collider.ClosestPointOnBounds(transform.position);
                    }

                    // addon system hooks
                    Utils.InvokeMany(typeof(Player), this, "OnSelect_", entity);
                    // clicked a new target
                }
                else
                {
                    // target it
                    CmdSetTarget(entity.netIdentity);
                }
            }
            // if we hit nothing then we want to move somewhere
            else
            {
                if (playerCar._car)
                {
                    if (!playerCar.car.On) return;

                    if (playerCar.car._pilot != string.Empty && playerCar.car._pilot != name)
                    {
                        return;
                    }
                    if (playerCar.car.currentGasoline == 0) return;
                }
            }
        }
    }

    // simple tab targeting
    [Client]
    public void TargetNearestButton()
    {
        // find all monsters that are alive, sort by distance
        Entity[] objects = FindObjectsOfType<Entity>();
        List<Entity> monsters = objects.Select(go => go.GetComponent<Entity>()).Where(m => m.health > 0).ToList();
        List<Entity> sorted = monsters.OrderBy(m => Vector2.Distance(transform.position, m.transform.position)).ToList();


        if (target)
        {
            if (target == sorted[1])
            {
                ButtonSelectionHandling();
            }
            else
            {
                if (sorted[1].GetComponent<Building>())
                {
                    if (!sorted[1].GetComponent<Building>().isHide)
                    {
                        CmdSetTarget(sorted[1].netIdentity);
                        //SetIndicatorViaParent(sorted[1].transform);
                    }
                }
                else
                {
                    CmdSetTarget(sorted[1].netIdentity);
                    //SetIndicatorViaParent(sorted[1].transform);
                }
            }
        }
        else
        {
            // target nearest one
            if (sorted.Count > 0)
            {

                if (sorted[1].GetComponent<Building>())
                {
                    if (!sorted[1].GetComponent<Building>().isHide)
                    {
                        CmdSetTarget(sorted[1].netIdentity);
                        //SetIndicatorViaParent(sorted[1].transform);
                    }
                }
                else
                {
                    CmdSetTarget(sorted[1].netIdentity);
                    //SetIndicatorViaParent(sorted[1].transform);
                }
            }
        }
    }


    // simple tab targeting
    [Client]
    public void TargetNearestEntityButton()
    {
        // find all monsters that are alive, sort by distance
        Entity[] objects = FindObjectsOfType<Entity>();
        List<Entity> monsters = objects.Select(go => go.GetComponent<Entity>()).Where(m => m.health > 0).ToList();
        List<Entity> sorted = monsters.OrderBy(m => Vector2.Distance(transform.position, m.transform.position)).ToList();

        if (sorted.Count > 0)
        {
            for (int i = 0; i < sorted.Count; i++)
            {
                if (sorted[i] == this) continue;
                if (sorted[i] == activePet) continue;
                if (sorted[i].GetComponent<Mine>())
                {
                    if (!GeneralManager.singleton.CanManageExplosiveBuilding(((Building)sorted[i]), this))
                    {
                        continue;
                    }
                }
                CmdSetTarget(sorted[i].netIdentity);
                return;
            }
        }
    }

    [Client]
    public void ButtonSelectionHandling()
    {
        // clear requested skill in any case because if we clicked
        // somewhere else then we don't care about it anymore
        useSkillWhenCloser = -1;
        Entity entity = null;
        if (target) entity = target;
        else return;
        //// clicked last target again? and is not self or pet?
        //if (entity == target && entity != this && entity != activePet)
        //{
        // attackable and has skills? => attack
        if (CanAttack(entity) && skills.Count > 0)
        {
            if (entity.GetComponent<Building>() && entity.GetComponent<Totem>())
            {
                if (GeneralManager.singleton.uiTotem == null)
                {
                    GeneralManager.singleton.uiTotem = Instantiate(GeneralManager.singleton.totemPrefab, GeneralManager.singleton.canvas);
                }
            }
            else
            {
                // then try to use that one
                TryUseSkill(FindNetworkSkill(((WeaponItem)playerItemEquipment.firstWeapon.item.data).requiredSkill));
                //Debug.Log("Skill name :" + FindNetworkSkill(((WeaponItem)playerItemEquipment.firstWeapon.item.data).requiredSkill));

            }
        }
        // npc, alive, close enough? => talk
        // use collider point(s) to also work with big entities
        else if (entity is Npc && entity.health > 0 &&
                 Utils.ClosestDistance(collider, entity.collider) <= interactionRange)
        {
            UINpcDialogue.singleton.Show();
        }
        else if (entity is Car && entity.health > 0 &&
                Utils.ClosestDistance(collider, entity.collider) <= 2)
        {
            GameObject g = Instantiate(GeneralManager.singleton.carPanelToSpawn, GeneralManager.singleton.canvas);
            playerBuilding.building = null;
            Destroy(playerBuilding.actualBuilding);
            playerBuilding.inventoryIndex = -1;
            Destroy(GeneralManager.singleton.spawnedBuildingObject);
        }

        // monster, dead, has loot, close enough? => loot
        // use collider point(s) to also work with big entities
        else if (entity is Monster && entity.health == 0 &&
                 Utils.ClosestDistance(collider, entity.collider) <= interactionRange &&
                 ((Monster)entity).HasLoot())
        {
            UILoot.singleton.Show();
        }
        else if (entity is Chest &&
                 Utils.ClosestDistance(collider, entity.collider) <= interactionRange &&
                 ((Chest)entity).HasLoot())
        {
            GeneralManager.singleton.uiChestPanel.SetActive(true);
        }
        // not attackable, lootable, talkable, etc., but it's
        // still an entity and double clicking it without doing
        // anything would be strange.
        // (e.g. if we are in a safe zone and click on a
        //  monster. it's not attackable, but we should at least
        //  move there, otherwise double click feels broken)
        else
        {
            if (playerCar._car)
            {
                if (!playerCar.car.On) return;

                if (playerCar.car._pilot != string.Empty && playerCar.car._pilot != name)
                {
                    return;
                }
                if (playerCar.car.currentGasoline == 0) return;
            }
        }

        // addon system hooks
        Utils.InvokeMany(typeof(Player), this, "OnSelect_", entity);
        // clicked a new target
        //}
    }


    #endregion

    #region Player Trading


    public bool CanStartTradeWith(Entity entity)
    {
        // can we trade? can the target trade? are we close enough?
        return entity != null && entity is Player && entity != this &&
               CanStartTrade() && ((Player)entity).CanStartTrade() &&
               Utils.ClosestDistance(collider, entity.collider) <= interactionRange && !((Player)entity).playerOptions.blockTrade;
    }

    [Command]
    public void CmdTradeOfferAccept()
    {
        // validate
        // note: distance check already done when starting the trade
        if (state == "TRADING" && tradeStatus == TradeStatus.Locked &&
            target != null && target is Player)
        {
            Player other = (Player)target;

            // other has locked?
            if (other.tradeStatus == TradeStatus.Locked)
            {
                //  simply accept and wait for the other guy to accept too
                tradeStatus = TradeStatus.Accepted;
                print("first accept by " + name);
            }
            // other has accepted already? then both accepted now, start trade.
            else if (other.tradeStatus == TradeStatus.Accepted)
            {
                // accept
                tradeStatus = TradeStatus.Accepted;
                print("second accept by " + name);

                // both offers still valid?
                if (IsTradeOfferStillValid() && other.IsTradeOfferStillValid())
                {
                    // both have enough inventory slots?
                    // note: we don't use InventoryCanAdd here because:
                    // - current solution works if both have full inventories
                    // - InventoryCanAdd only checks one slot. here we have
                    //   multiple slots though (it could happen that we can
                    //   not add slot 2 after we did add slot 1's items etc)
                    if (InventorySlotsFree() >= InventorySlotsNeededForTrade() &&
                        other.InventorySlotsFree() >= other.InventorySlotsNeededForTrade())
                    {
                        // exchange the items by first taking them out
                        // into a temporary list and then putting them
                        // in. this guarantees that exchanging even
                        // works with full inventories

                        // take them out
                        Queue<ItemSlot> tempMy = new Queue<ItemSlot>();
                        foreach (int index in tradeOfferItems)
                        {
                            if (index != -1)
                            {
                                ItemSlot slot = inventory[index];
                                slot.item.alreadyShooted = 0;
                                tempMy.Enqueue(slot);
                                slot.amount = 0;
                                inventory[index] = slot;
                            }
                        }

                        Queue<ItemSlot> tempOther = new Queue<ItemSlot>();
                        foreach (int index in other.tradeOfferItems)
                        {
                            if (index != -1)
                            {
                                ItemSlot slot = other.inventory[index];
                                slot.item.alreadyShooted = 0;
                                tempOther.Enqueue(slot);
                                slot.amount = 0;
                                other.inventory[index] = slot;
                            }
                        }

                        // put them into the free slots
                        for (int i = 0; i < inventory.Count; ++i)
                            if (inventory[i].amount == 0 && tempOther.Count > 0)
                                inventory[i] = tempOther.Dequeue();

                        for (int i = 0; i < other.inventory.Count; ++i)
                            if (other.inventory[i].amount == 0 && tempMy.Count > 0)
                                other.inventory[i] = tempMy.Dequeue();

                        // exchange the gold
                        gold -= tradeOfferGold;
                        other.gold -= other.tradeOfferGold;

                        gold += other.tradeOfferGold;
                        other.gold += tradeOfferGold;
                    }
                    for (int i = 0; i < quests.Count; i++)
                    {
                        Quest quest = quests[i];
                        if (quest.data.makeATrade == true)
                        {
                            quest.checkMakeATrade = true;
                        }
                        quests[i] = quest;
                    }
                }
                else print("trade canceled (invalid offer)");

                // clear trade request for both guys. the FSM event will do the
                // rest
                tradeRequestFrom = "";
                other.tradeRequestFrom = "";
            }
        }
    }

    [Command]
    public void CmdTradeRequestAccept()
    {
        Player sender = FindPlayerFromTradeInvitation();
        if (sender != null)
        {
            if (CanStartTradeWith(sender))
            {
                // also send a trade request to the person that invited us
                sender.tradeRequestFrom = name;
                print(name + " accepted " + sender.name + "'s trade request");
            }
        }
    }

    #endregion

    #region Can Attack
    public bool PlayerCanAttack(Player player)
    {
        if (!player.target) return false;
        //if (player.playerCar._car != null) return false;
        if (player.target is Chest) return false;
        if (player.target.GetComponent<CultivableField>())
        {
            return CanInteractUniversalBuilding(((Building)target), player);
        }
        if (target is Building)
        {
            if (CanInteractBuildingTarget(((Building)target), player) == true)
            {
                return CanInteract(((Building)target), player);
            }
            else
            {
                if (player.equipment[0].amount == 0) return false;
                if (player.playerCar._car) return false;
                if (player.playerObstacleCheck && !player.playerObstacleCheck.CheckObstacle(Vector2.zero)) return false;
                if (player.equipment[0].item.data.maxDurability.baseValue > 0 && player.equipment[0].item.durability == 0) return false;
                if (player.equipment[0].item.data is WeaponItem &&
                   ((WeaponItem)player.equipment[0].item.data).ammoItems.Count > 0 &&
                   player.equipment[player.GetEquipmentIndexByCategory("Ammo")].amount > 0 &&
                   !((WeaponItem)player.equipment[0].item.data).ammoItems.Contains((AmmoItem)player.equipment[player.GetEquipmentIndexByCategory("Ammo")].item.data)) return false;
                if (player.equipment[0].item.data is WeaponItem &&
                   ((WeaponItem)player.equipment[0].item.data).ammoItems.Count > 0 &&
                   player.equipment[0].item.alreadyShooted >= ((WeaponItem)playerItemEquipment.firstWeapon.item.data).chargeMunition.Get(playerItemEquipment.firstWeapon.item.chargeLevel)) return false;
                if (player.target.health <= 0) return false;
                return CanInteract((Building)player.target, player);
            }
        }
        else
        {
            if (player.equipment[0].amount == 0) return false;
            if (player.playerCar._car) return false;
            if (player.playerObstacleCheck && !player.playerObstacleCheck.CheckObstacle(Vector2.zero))
            {
                if (((WeaponItem)player.playerItemEquipment.firstWeapon.item.data).ammoItems.Count > 0)
                {
                    return false;
                }
            }
            if (player.equipment[0].item.data.maxDurability.baseValue > 0 && player.equipment[0].item.durability == 0) return false;
            if (player.equipment[0].item.data is WeaponItem &&
               ((WeaponItem)player.equipment[0].item.data).ammoItems.Count > 0 &&
               player.equipment[player.GetEquipmentIndexByCategory("Ammo")].amount > 0 &&
               !((WeaponItem)player.equipment[0].item.data).ammoItems.Contains((AmmoItem)player.equipment[player.GetEquipmentIndexByCategory("Ammo")].item.data)) return false;
            if (player.equipment[0].item.data is WeaponItem &&
               ((WeaponItem)player.equipment[0].item.data).ammoItems.Count > 0 &&
               player.equipment[0].item.alreadyShooted >= ((WeaponItem)playerItemEquipment.firstWeapon.item.data).chargeMunition.Get(playerItemEquipment.firstWeapon.item.chargeLevel)) return false;
            if (player.target.health <= 0) return false;

            switch (player.target)
            {
                case Player playerT:
                    return CanAttackPlayer(player);
                case Monster monster:
                    return true;
                case Pet pet:
                    return false;
                case Mount mount:
                    return false;
                case Building building:
                    return CanInteract(building, player);
                case Npc npc:
                    return false;
                case Rock rock:
                    return CanAttackRock(player);
                case Tree tree:
                    return CanAttackTree(player);
                case Plant plant:
                    return CanAttackPlant(player);
            }
        }
        return true;
    }

    public bool CanAttackPlayer(Player player)
    {
        if (player.InGuild())
        {
            if (((Player)player.target).InGuild() &&
                ((Player)player.target).guild.name == player.guild.name) return false;

            if (((Player)player.target).InGuild() &&
                ((Player)player.target).guild.name != player.guild.name)
            {
                return !player.playerAlliance.guildAlly.Contains(((Player)player.target).guild.name);
            }
        }
        else
        {
            return true;
        }
        return true;
    }
    public bool CanAttackRock(Player player)
    {
        if (player.equipment[0].amount == 0) return false;
        return GeneralManager.singleton.GetRockAllowedWeapons(((Rock)player.target), ((WeaponItem)player.equipment[0].item.data));
    }
    public bool CanAttackPlant(Player player)
    {
        if (player.equipment[0].amount == 0) return false;
        return GeneralManager.singleton.GetPlantAllowedWeapons(((Plant)player.target), ((WeaponItem)player.equipment[0].item.data));
    }
    public bool CanAttackTree(Player player)
    {
        if (player.equipment[0].amount == 0) return false;
        return GeneralManager.singleton.GetTreeAllowedWeapons(((Tree)player.target), ((WeaponItem)player.equipment[0].item.data));
    }

    #endregion

    #region Can interact Building

    public bool CanInteract(Building building, Player player)
    {
        if (!player) return false;

        if (!player.InGuild() && building.owner != string.Empty)
        {
            if (building.owner == player.name)
            {
                if (SearchUiToSpawnInManager(building.building) != null)
                {
                    if (player.isLocalPlayer) Instantiate(SearchUiToSpawnInManager(building.building), GeneralManager.singleton.canvas);
                    return false;
                }
                return false;
            }
            return true;
        }
        if (building.owner == string.Empty)
        {
            if (SearchUiToSpawnInManager(building.building) != null)
            {
                if (player.isLocalPlayer) Instantiate(SearchUiToSpawnInManager(building.building), GeneralManager.singleton.canvas);
                return false;
            }
            return false;
        }
        if (player.InGuild() && building.guild != string.Empty)
        {
            if (building.guild == player.guild.name)
            {
                if (SearchUiToSpawnInManager(building.building) != null)
                {
                    if (player.isLocalPlayer) Instantiate(SearchUiToSpawnInManager(building.building), GeneralManager.singleton.canvas);
                    return false;
                }
                return false;
            }
            if (building.guild != player.guild.name)
            {

                if (building.owner == player.name)
                {
                    if (SearchUiToSpawnInManager(building.building) != null)
                    {
                        if (player.isLocalPlayer) Instantiate(SearchUiToSpawnInManager(building.building), GeneralManager.singleton.canvas);
                        return false;
                    }
                    return false;
                }
                if (player.playerAlliance.guildAlly.Contains(building.guild))
                {
                    if (SearchUiToSpawnInManager(building.building) != null)
                    {
                        if (player.isLocalPlayer) Instantiate(SearchUiToSpawnInManager(building.building), GeneralManager.singleton.canvas);
                        return false;
                    }
                    return false;
                }
                if (!player.playerAlliance.guildAlly.Contains(building.guild))
                {
                    return true;
                }
            }
        }
        if (player.InGuild() && building.guild == string.Empty)
        {
            if (building.owner == player.name)
            {
                if (SearchUiToSpawnInManager(building.building) != null)
                {
                    if (player.isLocalPlayer) Instantiate(SearchUiToSpawnInManager(building.building), GeneralManager.singleton.canvas);
                    return false;
                }
                return false;
            }
        }
        return true;
    }

    public bool CanInteractUniversalBuilding(Building building, Player player)
    {
        if (player.isLocalPlayer) Instantiate(SearchUiToSpawnInManager(building.building), GeneralManager.singleton.canvas);
        return false;
    }

    public bool CanInteractBuildingTarget(Building building, Player player)
    {
        if (!player) return false;

        if (building.owner == player.name)
        {
            return true;
        }

        if (building.owner == string.Empty)
        {
            return true;
        }
        if (player.InGuild())
        {
            if (building.guild == player.guild.name)
            {
                return true;
            }
            if (building.guild != player.guild.name)
            {
                if (player.playerAlliance.guildAlly.Contains(building.guild))
                {
                    return true;
                }
                return false;
            }
        }
        return false;
    }

    public GameObject SearchUiToSpawnInManager(ScriptableBuilding building)
    {
        for (int i = 0; i < GeneralManager.singleton.buildingSpawn.Count; i++)
        {
            int index = i;
            if (GeneralManager.singleton.buildingSpawn[index].building.name.Contains(building.name))
            {
                return GeneralManager.singleton.buildingSpawn[index].uiToSpawn;
            }
        }

        return null;
    }

    public int FindNetworkSkill(ScriptableSkill skill)
    {
        for (int i = 0; i < skills.Count; i++)
        {
            int index = i;
            if (skills[index].name == skill.name)
            {
                //Debug.Log("Trovata la skill");
                return index;
            }
        }
        return -1;
    }

    #endregion


    #region Deal Damage at

    [ClientRpc]
    public void RpcInstantiateFire(string entity)
    {
        Player onlinePlayer;
        if (Player.onlinePlayers.TryGetValue(entity, out onlinePlayer))
        {
            // call TargetRpc on that GameObject for that connection
            if (!onlinePlayer.playerFired.firedSpawn) onlinePlayer.playerFired.firedSpawn = Instantiate(GeneralManager.singleton.firedEffectToSpawnAtPlayer, onlinePlayer.transform);
            onlinePlayer.playerFired.firedSpawn.transform.SetParent(onlinePlayer.transform);
        }
    }

    [ClientRpc]
    public void RpcInstantiateBuilding(Transform target)
    {
        // call TargetRpc on that GameObject for that connection
        GameObject g = Instantiate(GeneralManager.singleton.spawnEffectWhenAttackBuilding, target.transform);
        target.SetParent(g.transform);
        g.transform.localPosition = Vector3.zero;
    }

    [ClientRpc]
    public void RpcInstantiateRock(Transform target)
    {
        Animator animator = target.GetComponent<Animator>();
        animator.SetBool("PLAY", true);
        // call TargetRpc on that GameObject for that connection
        GameObject g = Instantiate(GeneralManager.singleton.spawnEffectWhenAttackRock, target.transform);
        target.SetParent(g.transform);
        g.transform.localPosition = Vector3.zero;
    }

    [ClientRpc]
    public void RpcInstantiateTree(Transform target)
    {
        Animator animator = target.GetComponent<Animator>();
        animator.SetBool("PLAY", true);
        // call TargetRpc on that GameObject for that connection
        GameObject g = Instantiate(GeneralManager.singleton.spawnEffectWhenAttackTree, target.transform);
        target.SetParent(g.transform);
        g.transform.localPosition = Vector3.zero;
    }

    [ClientRpc]
    public void RpcInstantiateElectric(string entity)
    {
        Player onlinePlayer;
        if (Player.onlinePlayers.TryGetValue(entity, out onlinePlayer))
        {
            // call TargetRpc on that GameObject for that connection
            if (!onlinePlayer.playerElectric.electricSpawn) onlinePlayer.playerElectric.electricSpawn = Instantiate(GeneralManager.singleton.electricEffectToSpawnAtPlayer, onlinePlayer.transform);
            onlinePlayer.playerElectric.electricSpawn.transform.SetParent(onlinePlayer.transform);
        }

    }

    [ClientRpc]
    public void RpcInstantiatePoisoned(string entity)
    {
        Player onlinePlayer;
        if (Player.onlinePlayers.TryGetValue(entity, out onlinePlayer))
        {
            // call TargetRpc on that GameObject for that connection
            if (!onlinePlayer.playerPoisoning.poisonedObject) onlinePlayer.playerPoisoning.poisonedObject = Instantiate(GeneralManager.singleton.poisonedEffectToSpawnAtPlayer, onlinePlayer.transform);
            onlinePlayer.playerPoisoning.poisonedObject.transform.SetParent(onlinePlayer.transform);
        }

    }

    public void ManageDamageArmorHealth(int amount)
    {
        if (playerArmor.currentArmor > 0)
        {
            for (int i = 0; i < equipment.Count; i++)
            {
                if (equipment[i].item.currentArmor > 0 && amount > 0)
                {
                    int remainingArmor = equipment[i].item.currentArmor;
                    if (amount >= remainingArmor)
                    {
                        ItemSlot slot = equipment[i];
                        amount -= remainingArmor;
                        slot.item.currentArmor = 0;
                        equipment[i] = slot;
                        continue;
                    }
                    else
                    {
                        ItemSlot slot = equipment[i];
                        slot.item.currentArmor -= amount;
                        amount = 0;
                        equipment[i] = slot;
                        continue;
                    }

                }
            }
            health -= Mathf.Max(amount - Convert.ToInt32(defense), 1);
            if (health < 0) health = 0;
        }
        else
        {
            health -= Mathf.Max(amount - Convert.ToInt32(defense), 1);
        }
    }

    public void ManageDamageArmorExplosionHealth(int amount)
    {
        if (playerArmor.currentArmor > 0)
        {
            for (int i = 0; i < equipment.Count; i++)
            {
                if (equipment[i].item.currentArmor > 0 && amount > 0)
                {
                    int remainingArmor = equipment[i].item.currentArmor;
                    if (amount >= remainingArmor)
                    {
                        ItemSlot slot = equipment[i];
                        amount -= remainingArmor;
                        slot.item.currentArmor = 0;
                        equipment[i] = slot;
                        continue;
                    }
                    else
                    {
                        ItemSlot slot = equipment[i];
                        slot.item.currentArmor -= amount;
                        amount = 0;
                        equipment[i] = slot;
                        continue;
                    }

                }
            }
            health -= (int)amount;
            if (health < 0) health = 0;
        }
        else
        {
            health -= amount;
        }
    }

    [Command]
    public void CmdReloadWeapon()
    {
        if (playerItemEquipment.firstWeapon.amount > 0 && ((WeaponItem)playerItemEquipment.firstWeapon.item.data).ammoItems.Count > 0)
        {
            playerMunitionManager.Charge();
        }
    }

    [Server]
    public void OnDamageDealtToMonster(Monster monster)
    {
        TimeSpan difference;

        // did we kill it?
        if (monster.health == 0)
        {
            if (playerBoost.networkBoost.Count > 0 && !string.IsNullOrEmpty(playerBoost.networkBoost[0].doubleEXP))
                difference = DateTime.Parse(playerBoost.networkBoost[0].doubleEXPServer.ToString()) - DateTime.Now;

            bool biohazard = false;
            bool infected = false;
            bool mecanic = false;
            bool police = false;

            if (monster.name.Contains("Infected"))
                infected = true;
            if (monster.name.Contains("BioHazard"))
                biohazard = true;
            if (monster.name.Contains("Mechanic"))
                mecanic = true;
            if (monster.name.Contains("Policeman"))
                police = true;

            // share kill rewards with party or only for self
            List<Player> closeMembers = InParty() ? GetPartyMembersInProximity() : new List<Player>();

            // share experience & skill experience
            // note: bonus only applies to exp. share parties, otherwise
            //       there's an unnecessary pressure to always join a
            //       party when leveling alone too.
            // note: if monster.rewardExp is 10 then it's possible that
            //       two members only receive 2 exp each (= 4 total).
            //       this happens because of exp balancing by level and
            //       is as intended.
            if (InParty() && party.shareExperience)
            {

                if (playerBoost.networkBoost.Count > 0 && !string.IsNullOrEmpty(playerBoost.networkBoost[0].doubleEXP) && difference.TotalSeconds > 0)
                {
                    foreach (Player member in closeMembers)
                    {
                        member.experience += CalculatePartyExperienceShare(
                            (monster.rewardExperience * 2),
                            closeMembers.Count,
                            Party.BonusExperiencePerMember,
                            member.level,
                            monster.level
                        );
                        member.skillExperience += CalculatePartyExperienceShare(
                            (monster.rewardExperience * 2),
                            closeMembers.Count,
                            Party.BonusExperiencePerMember,
                            member.level,
                            monster.level
                        );
                    }
                }
                else
                {
                    foreach (Player member in closeMembers)
                    {
                        member.experience += CalculatePartyExperienceShare(
                            monster.rewardExperience,
                            closeMembers.Count,
                            Party.BonusExperiencePerMember,
                            member.level,
                            monster.level
                        );
                        member.skillExperience += CalculatePartyExperienceShare(
                            monster.rewardExperience,
                            closeMembers.Count,
                            Party.BonusExperiencePerMember,
                            member.level,
                            monster.level
                        );
                    }
                }


            }
            else
            {
                //skillExperience += BalanceExpReward(monster.rewardSkillExperience, level, monster.level);
                if (playerBoost.networkBoost.Count > 0 && !string.IsNullOrEmpty(playerBoost.networkBoost[0].doubleEXP) && difference.TotalSeconds > 0)
                    experience += (BalanceExpReward(monster.rewardExperience, level, monster.level) * 2);
                else
                    experience += BalanceExpReward(monster.rewardExperience, level, monster.level);
            }

            // give pet the same exp without dividing it, but balance it
            // => AFTER player exp reward! pet can only ever level up to player
            //    level, so it's best if the player gets exp and level-ups
            //    first, then afterwards we try to level up the pet.
            if (activePet != null)
                activePet.experience += BalanceExpReward(monster.rewardExperience, activePet.level, monster.level);

            // increase quest kill counter for all party members
            if (InParty())
            {
                foreach (Player member in closeMembers)
                {
                    for (int i = 0; i < member.quests.Count; i++)
                    {
                        Quest quest = member.quests[i];
                        if (biohazard) quest.checkBiohazard++;
                        if (infected) quest.checkInfected++;
                        if (mecanic) quest.checkMechanic++;
                        if (police) quest.checkPolice++;
                        member.quests[i] = quest;
                    }
                }
            }
            else
            {
                for (int i = 0; i < quests.Count; i++)
                {
                    Quest quest = quests[i];
                    if (biohazard) quest.checkBiohazard++;
                    if (infected) quest.checkInfected++;
                    if (mecanic) quest.checkMechanic++;
                    if (police) quest.checkPolice++;
                    quests[i] = quest;
                }
            }
        }
    }

    [Server]
    public void OnDamageDealtToPlayer(Player player)
    {
        // did we kill the player?
        if (player.health == 0)
        {
            // increase quest kill counter for all party members
            // (in case someone implements a kill-player quest type!)
            if (InParty())
            {
                List<Player> closeMembers = GetPartyMembersInProximity();
                foreach (Player member in closeMembers)
                {
                    for (int i = 0; i < member.quests.Count; i++)
                    {
                        Quest quest = member.quests[i];
                        if (quest.data.amountPlayerToKill > 0)
                        {
                            quest.checkAmountPlayerToKill++;
                        }
                        member.quests[i] = quest;
                    }
                }
            }
            else
            {
                for (int i = 0; i < quests.Count; i++)
                {
                    Quest quest = quests[i];
                    if (quest.data.amountPlayerToKill > 0)
                    {
                        quest.checkAmountPlayerToKill++;
                    }
                    quests[i] = quest;
                }
            }
        }
    }

    [Server]
    public void OnDamageDealtToPet(Pet pet)
    {

    }

    // custom DealDamageAt function that also rewards experience if we killed
    // the monster
    [Server]
    public override void DealDamageAt(Entity entity, int amount, float stunChance = 0, float stunTime = 0)
    {
        // deal damage with the default function
        base.DealDamageAt(entity, amount, stunChance, stunTime);
        playerMonsterGrab.TargetNearest();
        if (this is Player)
        {
            playerMove.lastHit = NetworkTime.time;
            playerMove.lastHitEntity = entity;
        }

        TimeSpan difference;

        // a monster?
        if (entity is Monster)
        {
            OnDamageDealtToMonster((Monster)entity);
            if (((Monster)entity).health == 0)
            {
                if (playerBoost.networkBoost.Count > 0 && !string.IsNullOrEmpty(playerBoost.networkBoost[0].doubleGoldServer))
                    difference = DateTime.Parse(playerBoost.networkBoost[0].doubleGoldServer.ToString()) - System.DateTime.Now;

                if (((Monster)entity).isBoss)
                {
                    int index = 0;
                    for (int i = 0; i < SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualMonster.Count; i++)
                    {
                        index = i;
                        if (SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualMonster[index].entity == entity)
                        {
                            SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualMonster.Remove(SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualMonster[index]);
                        }
                    }
                    //if (SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualMonster.Contains(entity.gameObject)) SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualMonster.Remove(entity.gameObject);
                    //SpawnManagerList.singleton.spawnManagers[entity.spawnManager].spawnedAlready--;
                    playerLeaderPoints.bossKill += GeneralManager.singleton.monsterPoint;
                    if (playerBoost.networkBoost.Count > 0 && !string.IsNullOrEmpty(playerBoost.networkBoost[0].doubleGold) && difference.TotalSeconds > 0)
                    {
                        gold += entity.gold * 2;
                    }
                    else
                        gold += entity.gold;
                }
                else
                {
                    int index = 0;
                    for (int i = 0; i < SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualMonster.Count; i++)
                    {
                        index = i;
                        if (SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualMonster[index].entity == entity)
                        {
                            SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualMonster.Remove(SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualMonster[index]);
                        }
                    }
                    //if (SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualBoss.Contains(entity.gameObject)) SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualBoss.Remove(entity.gameObject);
                    playerLeaderPoints.monsterKill += GeneralManager.singleton.bossKill;
                    SpawnManagerList.singleton.spawnManagers[entity.spawnManager].spawnedAlready--;
                }
            }
        }
        // a player?
        // (see murder code section comments to understand the system)
        else if (entity is Player)
        {
            OnDamageDealtToPlayer((Player)entity);
            if (((Player)entity).health == 0)
            {
                if (playerBoost.networkBoost.Count > 0 && !string.IsNullOrEmpty(playerBoost.networkBoost[0].doubleLeaderPointsServer))
                    difference = DateTime.Parse(playerBoost.networkBoost[0].doubleLeaderPointsServer.ToString()) - System.DateTime.Now;

                if (playerBoost.networkBoost.Count > 0 && !string.IsNullOrEmpty(playerBoost.networkBoost[0].doubleLeaderPoints) && difference.TotalSeconds > 0)
                    playerLeaderPoints.playerKill += (GeneralManager.singleton.playerPoint * 2);
                else
                    playerLeaderPoints.playerKill += GeneralManager.singleton.playerPoint;
            }
        }
        // a pet?
        // (see murder code section comments to understand the system)
        else if (entity is Pet)
        {
            OnDamageDealtToPet((Pet)entity);
        }
        else if (entity is Plant)
        {
            if (((Plant)entity).health == 0)
            {
                //if (SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualPlant.Contains(entity.gameObject)) SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualPlant.Remove(entity.gameObject);
                //SpawnManagerList.singleton.spawnManagers[entity.spawnManager].spawnedAlready--;
                RpgEffectOnEntityDeath(new Vector3(entity.gameObject.transform.position.x, entity.transform.position.y + 1.34f, entity.transform.position.z));
                playerLeaderPoints.plantPoint += GeneralManager.singleton.plantPoint;
                entity.Invoke("DestroyObject", 0.5f);
            }
            else
            {

            }
        }
        else if (entity is Rock)
        {
            if (((Rock)entity).health == 0)
            {
                int index = 0;
                for (int i = 0; i < SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualRock.Count; i++)
                {
                    index = i;
                    if (SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualRock[index].entity == entity)
                    {
                        SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualRock.Remove(SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualRock[index]);
                    }
                }
                //if (SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualRock.Contains(entity.gameObject)) SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualRock.Remove(entity.gameObject);
                //SpawnManagerList.singleton.spawnManagers[entity.spawnManager].spawnedAlready--;
                RpgEffectOnEntityDeath(new Vector3(entity.gameObject.transform.position.x, entity.transform.position.y + 1.34f, entity.transform.position.z));
                playerLeaderPoints.rockPoint += GeneralManager.singleton.rockPoint;
                entity.Invoke("DestroyObject", 0.5f);
            }
            else
            {
                for (int i = 0; i < quests.Count; i++)
                {
                    Quest quest = quests[i];
                    if (quest.data.amountRockToGather > 0)
                    {
                        quest.checkRockToGather++;
                    }
                    quests[i] = quest;
                }
            }
        }
        else if (entity is Tree)
        {
            if (((Tree)entity).health == 0)
            {
                int index = 0;
                for (int i = 0; i < SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualTrees.Count; i++)
                {
                    index = i;
                    if (SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualTrees[index].entity == entity)
                    {
                        SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualTrees.Remove(SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualTrees[index]);
                    }
                }
                //if (SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualTrees.Contains(entity.gameObject)) SpawnManagerList.singleton.spawnManagers[entity.spawnManager].actualTrees.Remove(entity.gameObject);
                //SpawnManagerList.singleton.spawnManagers[entity.spawnManager].spawnedAlready--;
                RpgEffectOnEntityDeath(new Vector3(entity.gameObject.transform.position.x, entity.transform.position.y + 1.34f, entity.transform.position.z));
                playerLeaderPoints.treePoint += GeneralManager.singleton.treePoint;
                entity.Invoke("DestroyObject", 0.5f);
            }
            else
            {
                for (int i = 0; i < quests.Count; i++)
                {
                    Quest quest = quests[i];
                    if (quest.data.amountWoodToGather > 0)
                    {
                        quest.checkWoodToGather++;
                    }
                    quests[i] = quest;
                }
            }
        }
        else if (entity is Building)
        {
            if (((Building)entity).health == 0)
            {
                RpgEffectOnEntityDeath(new Vector3(entity.gameObject.transform.position.x, entity.transform.position.y + 1.34f, entity.transform.position.z));
                entity.Invoke("DestroyObject", 0.5f);
            }
            else
            {

            }
        }

        AddShootedAmmo();

        // addon system hooks
        Utils.InvokeMany(typeof(Player), this, "DealDamageAt_", entity, amount);
    }

    [ClientRpc]
    public void RpgEffectOnEntityDeath(Vector3 entityPosition)
    {
        GameObject g = Instantiate(GeneralManager.singleton.smokePrefab);
        g.transform.position = entityPosition;
    }

    #endregion

    #region Party

    // party invite by name (not by target) so that chat commands are possible
    // if needed
    [Command]
    public void CmdPartyInvite(string otherName)
    {
        // validate: is there someone with that name, and not self?
        if (otherName != name &&
            onlinePlayers.TryGetValue(otherName, out Player other) &&
            NetworkTime.time >= nextRiskyActionTime && !((Player)target).playerOptions.blockParty)
        {
            // can only send invite if no party yet or party isn't full and
            // have invite rights and other guy isn't in party yet
            if ((!InParty() || !party.IsFull()) && !other.InParty())
            {
                // send a invite
                other.partyInviteFrom = name;

                print(name + " invited " + other.name + " to party");
            }
        }

        // reset risky time no matter what. even if invite failed, we don't want
        // players to be able to spam the invite button and mass invite random
        // players.
        nextRiskyActionTime = NetworkTime.time + partyInviteWaitSeconds;
    }


    #endregion

    #region Guild from friends
    [Command]
    public void CmdGuildInviteTargetFromFriends(string otherPlayer)
    {
        Player onlinePlayer;
        if (Player.onlinePlayers.TryGetValue(otherPlayer, out onlinePlayer))
        {
        }
        // validate
        if (onlinePlayer != null && onlinePlayer is Player &&
            InGuild() && !((Player)onlinePlayer).InGuild() &&
            guild.CanInvite(name, onlinePlayer.name) &&
            NetworkTime.time >= nextRiskyActionTime)
        {
            // send an invite
            ((Player)onlinePlayer).guildInviteFrom = name;

            print(name + " invited " + target.name + " to guild");
        }

        // reset risky time no matter what. even if invite failed, we don't want
        // players to be able to spam the invite button and mass invite random
        // players.
        nextRiskyActionTime = NetworkTime.time + guildInviteWaitSeconds;
    }
    #endregion


    // BUILDING


    #region Craft add to Inventory
    [Command]
    public void CmdAddToInventory(string itenName, int amount, string owner, string timeEnd, int finishedIndex)
    {
        if (ScriptableItem.dict.TryGetValue(itenName.GetStableHashCode(), out ScriptableItem item))
        {
            BuildingCraft buildingCraft = target.GetComponent<BuildingCraft>();
            if (buildingCraft.craftItem[finishedIndex].itemName != itenName || buildingCraft.craftItem[finishedIndex].amount != amount || buildingCraft.craftItem[finishedIndex].owner != owner || buildingCraft.craftItem[finishedIndex].timeEnd != timeEnd) return;
            if (buildingCraft.craftItem[finishedIndex].owner == name && DateTime.Parse(buildingCraft.craftItem[finishedIndex].timeEndServer) < System.DateTime.Now)
            {
                InventoryAdd(new Item(item), amount);
                buildingCraft.craftItem.RemoveAt(finishedIndex);
            }
        }
    }
    #endregion

    #region UpgradeItem
    [Command]
    public void CmdUpgradeItem(UpgradeRepairItem upgradeItem, string accuracy, int currencyType, string dateBegin, string dateEnd)
    {
        if (target && target.GetComponent<BuildingUpgradeRepair>())
        {
            BuildingUpgradeRepair buildingUpgradeRepair = target.GetComponent<BuildingUpgradeRepair>();
            upgradeItem.timeBegin = DateTime.Parse(dateBegin).ToString();
            upgradeItem.timeBeginServer = DateTime.Now.ToString();

            if (currencyType == 1)
            {
                upgradeItem.timeEnd = DateTime.Parse(dateEnd).ToString();
                upgradeItem.timeEndServer = DateTime.Now.ToString();
            }
            else
            {
                upgradeItem.timeEnd = DateTime.Parse(dateEnd).ToString();
                upgradeItem.timeEndServer = DateTime.Now.ToString();
            }

            if (ScriptableItem.dict.TryGetValue(inventory[upgradeItem.index].item.name.GetStableHashCode(), out ScriptableItem itemData))
            {
                foreach (CustomItem custoItem in itemData.upgradeItems)
                {
                    if (InventoryCount(new Item(custoItem.items)) < custoItem.amount)
                    {
                        return;
                    }
                }
                if (currencyType == 0 && gold < itemData.goldsToUpgrade) return;
                if (currencyType == 1 && coins < itemData.coinsToUpgrade) return;

                foreach (CustomItem custoItem in itemData.upgradeItems)
                {
                    InventoryRemove(new Item(custoItem.items), custoItem.amount);
                }

                if (currencyType == 0)
                    gold -= itemData.goldsToUpgrade;
                if (currencyType == 1)
                    coins -= itemData.coinsToUpgrade;

                Item itm = new Item(itemData);
                ItemSlot toRemove = inventory[upgradeItem.index];
                toRemove.amount--;
                inventory[upgradeItem.index] = toRemove;

                if (currencyType == 0)
                {
                    buildingUpgradeRepair.upgradeItem.Add(upgradeItem);
                }
                else
                {
                    upgradeItem.remainingTime = Convert.ToInt32(upgradeItem.totalTime / 2);
                    buildingUpgradeRepair.upgradeItem.Add(upgradeItem);
                }
            }
        }
    }

    [Command]
    public void CmdClaimUpgradeItem(int index, string itenName, int amount, string owner, string timeEnd)
    {
        if (target && target.GetComponent<BuildingUpgradeRepair>())
        {
            BuildingUpgradeRepair buildingUpgradeRepair = target.GetComponent<BuildingUpgradeRepair>();
            if (buildingUpgradeRepair.upgradeItem[index].item.item.name != itenName || buildingUpgradeRepair.upgradeItem[index].item.amount != amount || buildingUpgradeRepair.upgradeItem[index].playerName != owner || buildingUpgradeRepair.upgradeItem[index].timeEnd != timeEnd) return;

            ItemSlot slot = buildingUpgradeRepair.upgradeItem[index].item;

            Debug.Log("Type : " + buildingUpgradeRepair.upgradeItem[index].type);

            if (buildingUpgradeRepair.upgradeItem[index].type == "Accuracy" || buildingUpgradeRepair.upgradeItem[index].type == "accuratezza")
            {
                slot.item.accuracyLevel++;
            }
            if (buildingUpgradeRepair.upgradeItem[index].type == "Evasion" || buildingUpgradeRepair.upgradeItem[index].type == "evasione")
            {
                slot.item.missLevel++;
            }
            if (buildingUpgradeRepair.upgradeItem[index].type == "Armor" || buildingUpgradeRepair.upgradeItem[index].type == "armatura")
            {
                slot.item.armorLevel++;
                if (slot.item.currentArmor > 0) slot.item.currentArmor += ((EquipmentItem)slot.item.data).armor.bonusPerLevel;
                if (slot.item.currentArmor > ((EquipmentItem)slot.item.data).armor.Get(slot.item.armorLevel)) slot.item.currentArmor = ((EquipmentItem)slot.item.data).armor.Get(slot.item.armorLevel);
            }
            if (buildingUpgradeRepair.upgradeItem[index].type == "Munition charge" || buildingUpgradeRepair.upgradeItem[index].type == "carica munizioni")
            {
                slot.item.chargeLevel++;
            }
            if (buildingUpgradeRepair.upgradeItem[index].type == "Radio battery" || buildingUpgradeRepair.upgradeItem[index].type == "batteria radio")
            {
                slot.item.batteryLevel++;
                if (slot.item.radioCurrentBattery > 0) slot.item.radioCurrentBattery += ((RadioItem)slot.item.data).currentBattery.bonusPerLevel;
                if (slot.item.radioCurrentBattery > ((RadioItem)slot.item.data).currentBattery.Get(slot.item.batteryLevel)) slot.item.radioCurrentBattery = ((RadioItem)slot.item.data).currentBattery.Get(slot.item.batteryLevel);
            }
            if (buildingUpgradeRepair.upgradeItem[index].type == "Torch battery" || buildingUpgradeRepair.upgradeItem[index].type == "batteria torcia")
            {
                slot.item.batteryLevel++;
                if (slot.item.torchCurrentBattery > 0) slot.item.torchCurrentBattery += ((TorchItem)slot.item.data).currentBattery.bonusPerLevel;
                if (slot.item.torchCurrentBattery > ((TorchItem)slot.item.data).currentBattery.Get(slot.item.batteryLevel)) slot.item.torchCurrentBattery = ((TorchItem)slot.item.data).currentBattery.Get(slot.item.batteryLevel);
            }
            if (buildingUpgradeRepair.upgradeItem[index].type == "Bag Slot" || buildingUpgradeRepair.upgradeItem[index].type == "slot borsa")
            {
                slot.item.bagLevel++;
            }
            if (buildingUpgradeRepair.upgradeItem[index].type == "Bag Protected Slot" || buildingUpgradeRepair.upgradeItem[index].type == "slot protetti borsa")
            {
                slot.item.bagLevel++;
            }
            if (buildingUpgradeRepair.upgradeItem[index].type == "Durability" || buildingUpgradeRepair.upgradeItem[index].type == "durabilita'")
            {
                slot.item.durabilityLevel++;
                if (slot.item.durability > 0) slot.item.durability += ((EquipmentItem)slot.item.data).maxDurability.bonusPerLevel;
                if (slot.item.durability > ((EquipmentItem)slot.item.data).maxDurability.Get(slot.item.durabilityLevel)) slot.item.durability = ((EquipmentItem)slot.item.data).maxDurability.Get(slot.item.durabilityLevel);
            }
            if (buildingUpgradeRepair.upgradeItem[index].type == "Unsanity" || buildingUpgradeRepair.upgradeItem[index].type == "sanita' oggetto")
            {
                slot.item.unsanityLevel++;
            }
            if (buildingUpgradeRepair.upgradeItem[index].type == "Item Weight" || buildingUpgradeRepair.upgradeItem[index].type == "peso oggetto")
            {
                slot.item.weightLevel++;
            }

            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i].amount == 0)
                {
                    inventory[i] = slot;
                    buildingUpgradeRepair.upgradeItem.Remove(buildingUpgradeRepair.upgradeItem[index]);
                    return;
                }
            }
        }
    }

    [Command]
    public void CmdBuyUpgradeRepairItem(string itemName, int amount, int currency)
    {
        if (ScriptableItem.dict.TryGetValue(itemName.GetStableHashCode(), out ScriptableItem itemData))
        {
            if (currency == 0)
            {
                if (InventoryCanAdd(new Item(itemData), amount))
                {
                    if (gold >= (itemData.goldPrice * amount))
                    {
                        InventoryAdd(new Item(itemData), amount);
                        gold -= itemData.goldPrice * amount;
                    }
                }
            }
            if (currency == 1)
            {
                if (InventoryCanAdd(new Item(itemData), amount))
                {
                    if (coins >= (itemData.coinPrice * amount))
                    {
                        InventoryAdd(new Item(itemData), amount);
                        coins -= itemData.coinPrice * amount;
                    }
                }
            }
        }
    }

    public bool CheckBuyUpgradeRepairItem(string itemName, int amount, int currency)
    {
        if (ScriptableItem.dict.TryGetValue(itemName.GetStableHashCode(), out ScriptableItem itemData))
        {
            if (currency == 0)
            {
                if (InventoryCanAdd(new Item(itemData), amount))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (currency == 1)
            {
                if (InventoryCanAdd(new Item(itemData), amount))
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

    #endregion

    #region RepairItem
    [Command]
    public void CmdRepairItem(UpgradeRepairItem upgrade, int currencyType, string dateBegin, string dateEnd)
    {
        if (target && target.GetComponent<BuildingUpgradeRepair>())
        {
            BuildingUpgradeRepair buildingUpgradeRepair = target.GetComponent<BuildingUpgradeRepair>();
            upgrade.timeBegin = DateTime.Parse(dateBegin).ToString();
            upgrade.timeEnd = DateTime.Parse(dateEnd).ToString();

            upgrade.timeBeginServer = DateTime.Now.ToString();
            upgrade.timeEndServer = DateTime.Now.AddSeconds(upgrade.totalTime).ToString();

            if (ScriptableItem.dict.TryGetValue(inventory[upgrade.index].item.name.GetStableHashCode(), out ScriptableItem itemData))
            {
                foreach (CustomItem custoItem in itemData.repairItems)
                {
                    if (InventoryCount(new Item(custoItem.items)) < custoItem.amount)
                    {
                        return;
                    }
                }
                if (currencyType == 0 && gold < itemData.goldsToRepair) return;
                if (currencyType == 1 && coins < itemData.coinsToRepair) return;

                foreach (CustomItem custoItem in itemData.repairItems)
                {
                    InventoryRemove(new Item(custoItem.items), custoItem.amount);
                }

                if (currencyType == 0)
                    gold -= itemData.goldsToUpgrade;
                if (currencyType == 1)
                    coins -= itemData.coinsToUpgrade;

                Item itm = new Item(itemData);
                ItemSlot toRemove = inventory[upgrade.index];
                toRemove.amount--;
                inventory[upgrade.index] = toRemove;

                buildingUpgradeRepair.repairItem.Add(upgrade);

                if (currencyType == 0)
                {
                    buildingUpgradeRepair.repairItem.Add(upgrade);
                }
                else
                {
                    upgrade.remainingTime = Convert.ToInt32(upgrade.totalTime / 2);
                    buildingUpgradeRepair.repairItem.Add(upgrade);
                }

            }
        }
    }

    [Command]
    public void CmdClaimRepairItem(int index, string itenName, int amount, string owner, string timeEnd)
    {
        if (target && target.GetComponent<BuildingUpgradeRepair>())
        {
            BuildingUpgradeRepair buildingUpgradeRepair = target.GetComponent<BuildingUpgradeRepair>();
            if (buildingUpgradeRepair.repairItem[index].item.item.name != itenName || buildingUpgradeRepair.repairItem[index].item.amount != amount || buildingUpgradeRepair.repairItem[index].playerName != owner || buildingUpgradeRepair.repairItem[index].timeEnd != timeEnd) return;

            ItemSlot slot = buildingUpgradeRepair.repairItem[index].item;
            slot.item.durability = buildingUpgradeRepair.repairItem[index].item.item.data.maxDurability.Get(buildingUpgradeRepair.repairItem[index].item.item.durabilityLevel);

            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i].amount == 0)
                {
                    inventory[i] = slot;
                    buildingUpgradeRepair.repairItem.Remove(buildingUpgradeRepair.repairItem[index]);
                    return;
                }
            }
        }
    }

    #endregion

    #region Honey Container
    public int GetEmptyHoneyBootle()
    {
        int containerFree = 0;
        for (int i = 0; i < inventory.Count; i++)
        {
            int index = i;
            if (inventory[index].amount > 0)
            {
                if (inventory[index].item.data.generalLiquidContainer > 0)
                {
                    if (inventory[index].item.gasolineContainer > 0) continue;
                    if (inventory[index].item.waterContainer > 0) continue;
                    containerFree += inventory[index].item.data.generalLiquidContainer - inventory[index].item.honeyContainer;
                }
            }
        }
        return containerFree;
    }

    public int GetHoneyInInventory()
    {
        int containerFree = 0;
        for (int i = 0; i < inventory.Count; i++)
        {
            int index = i;
            if (inventory[index].amount > 0)
            {
                if (inventory[index].item.honeyContainer > 0)
                {
                    containerFree += inventory[index].item.honeyContainer;
                }
            }
        }
        return containerFree;
    }

    [Command]
    public void CmdTakeHoney(int amountHoney, int honeyContainers)
    {
        int totalHoney = amountHoney;
        int freeInventoryHoney = amountHoney;
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].amount > 0)
            {
                if (inventory[i].item.waterContainer > 0) continue;
                if (inventory[i].item.gasolineContainer > 0) continue;

                freeInventoryHoney = inventory[i].item.data.generalLiquidContainer - inventory[i].item.honeyContainer;
                if (amountHoney <= freeInventoryHoney)
                {
                    ItemSlot itm = new ItemSlot();
                    itm = inventory[i];
                    itm.item.honeyContainer += amountHoney;
                    inventory[i] = itm;
                    BeeContainer containers = target.GetComponent<BeeKeeper>().beeContainers[honeyContainers];
                    containers.totalHoney -= amountHoney;
                    target.GetComponent<BeeKeeper>().beeContainers[honeyContainers] = containers;
                    amountHoney = 0;
                }
                else
                {
                    ItemSlot itm = new ItemSlot();
                    itm = inventory[i];
                    itm.item.honeyContainer = itm.item.data.generalLiquidContainer;
                    inventory[i] = itm;
                    BeeContainer containers = target.GetComponent<BeeKeeper>().beeContainers[honeyContainers];
                    containers.totalHoney -= freeInventoryHoney;
                    target.GetComponent<BeeKeeper>().beeContainers[honeyContainers] = containers;
                    amountHoney -= freeInventoryHoney;
                }

            }
        }
    }

    #endregion

    #region Water Container
    public int GetEmptyWaterBootle()
    {
        int containerFree = 0;
        for (int i = 0; i < inventory.Count; i++)
        {
            int index = i;
            if (inventory[index].amount > 0)
            {
                if (inventory[index].item.data.generalLiquidContainer > 0)
                {
                    if (inventory[index].item.gasolineContainer > 0) continue;
                    if (inventory[index].item.honeyContainer > 0) continue;
                    if (inventory[index].item.waterContainer < inventory[index].item.data.generalLiquidContainer)
                    {
                        containerFree += inventory[index].item.data.generalLiquidContainer - inventory[index].item.waterContainer;
                    }
                }
            }
        }
        return containerFree;
    }

    public int GetWaterInInventory()
    {
        int containerFree = 0;
        for (int i = 0; i < inventory.Count; i++)
        {
            int index = i;
            if (inventory[index].amount > 0)
            {
                if (inventory[index].item.data.generalLiquidContainer > 0)
                {
                    containerFree += inventory[index].item.waterContainer;
                }
            }
        }
        return containerFree;
    }

    [Command]
    public void CmdTakeWater(int amountHoney)
    {
        int totalHoney = amountHoney;
        int freeInventoryHoney = amountHoney;
        if (!target.GetComponent<BuildingWaterWell>()) return;
        if (amountHoney > GetEmptyWaterBootle()) return;
        if (amountHoney > target.GetComponent<BuildingWaterWell>().currentWater) return;
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].amount > 0)
            {
                if (inventory[i].item.data.generalLiquidContainer > 0)
                {
                    freeInventoryHoney = inventory[i].item.data.generalLiquidContainer - inventory[i].item.waterContainer;
                    if (amountHoney <= freeInventoryHoney)
                    {
                        ItemSlot itm = new ItemSlot();
                        itm = inventory[i];
                        target.GetComponent<BuildingWaterWell>().currentWater -= amountHoney;
                        itm.item.waterContainer += amountHoney;
                        inventory[i] = itm;
                        amountHoney = 0;
                    }
                    else
                    {
                        ItemSlot itm = new ItemSlot();
                        itm = inventory[i];
                        itm.item.waterContainer = itm.item.data.generalLiquidContainer;
                        inventory[i] = itm;
                        amountHoney -= freeInventoryHoney;
                        target.GetComponent<BuildingWaterWell>().currentWater -= freeInventoryHoney;
                    }
                }
            }
        }
    }

    #endregion

    #region Breeding
    [Command]
    public void CmdAddCow(int index)
    {
        if (!target.GetComponent<Breeding>()) return;

        int cow = GeneralManager.singleton.cow;
        int additionaToLevel = Convert.ToInt32(target.level / 10);
        if (additionaToLevel > 0)
        {
            additionaToLevel = additionaToLevel * 2;
        }
        int result = cow + additionaToLevel;

        if (target.GetComponent<Breeding>().cow.Count >= result) return;

        Animal animal = new Animal();

        animal.sex = ((ScriptableAnimal)inventory[index].item.data).sex.ToString();
        animal.otherSexName = ((ScriptableAnimal)inventory[index].item.data).otherSex.sex.ToString();
        animal.age = 0;
        animal.maxAge = ((ScriptableAnimal)inventory[index].item.data).maxAge;
        animal.toAdultAge = ((ScriptableAnimal)inventory[index].item.data).toAdultAge;
        animal.amountFoodNeeded = ((ScriptableAnimal)inventory[index].item.data).amountFoodNeeded;
        animal.amountWaterNeeded = ((ScriptableAnimal)inventory[index].item.data).amountWaterNeeded;
        animal.animalCategory = ((ScriptableAnimal)inventory[index].item.data).animalCategory.ToString();
        animal.distanceBeetweenBorn = ((ScriptableAnimal)inventory[index].item.data).distanceBeetweenBorn;
        animal.health = ((ScriptableAnimal)inventory[index].item.data).health;
        animal.lastBorn = 0;
        animal.lastTake = 0;
        animal.maxHealth = ((ScriptableAnimal)inventory[index].item.data).health;
        animal.name = ((ScriptableAnimal)inventory[index].item.data).name;
        animal.timeToTake = ((ScriptableAnimal)inventory[index].item.data).timeToTake;

        target.GetComponent<Breeding>().cow.Add(animal);
        ItemSlot slot = inventory[index];
        slot.amount--;
        inventory[index] = slot;
    }
    [Command]
    public void CmdAddSheep(int index)
    {
        if (!target.GetComponent<Breeding>()) return;

        int cow = GeneralManager.singleton.sheep;
        int additionaToLevel = Convert.ToInt32(target.level / 10);
        if (additionaToLevel > 0)
        {
            additionaToLevel = additionaToLevel * 2;
        }
        int result = cow + additionaToLevel;

        if (target.GetComponent<Breeding>().sheep.Count >= result) return;

        Animal animal = new Animal();

        animal.sex = ((ScriptableAnimal)inventory[index].item.data).sex.ToString();
        animal.otherSexName = ((ScriptableAnimal)inventory[index].item.data).otherSex.sex.ToString();
        animal.age = 0;
        animal.maxAge = ((ScriptableAnimal)inventory[index].item.data).maxAge;
        animal.toAdultAge = ((ScriptableAnimal)inventory[index].item.data).toAdultAge;
        animal.amountFoodNeeded = ((ScriptableAnimal)inventory[index].item.data).amountFoodNeeded;
        animal.amountWaterNeeded = ((ScriptableAnimal)inventory[index].item.data).amountWaterNeeded;
        animal.animalCategory = ((ScriptableAnimal)inventory[index].item.data).animalCategory.ToString();
        animal.distanceBeetweenBorn = ((ScriptableAnimal)inventory[index].item.data).distanceBeetweenBorn;
        animal.health = ((ScriptableAnimal)inventory[index].item.data).health;
        animal.lastBorn = 0;
        animal.lastTake = 0;
        animal.maxHealth = ((ScriptableAnimal)inventory[index].item.data).health;
        animal.name = ((ScriptableAnimal)inventory[index].item.data).name;
        animal.timeToTake = ((ScriptableAnimal)inventory[index].item.data).timeToTake;

        target.GetComponent<Breeding>().sheep.Add(animal);
        ItemSlot slot = inventory[index];
        slot.amount--;
        inventory[index] = slot;
    }
    [Command]
    public void CmdAddChicken(int index)
    {
        if (!target.GetComponent<Breeding>()) return;

        int cow = GeneralManager.singleton.chicken;
        int additionaToLevel = Convert.ToInt32(target.level / 10);
        if (additionaToLevel > 0)
        {
            additionaToLevel = additionaToLevel * 2;
        }
        int result = cow + additionaToLevel;

        if (target.GetComponent<Breeding>().chicken.Count >= result) return;

        Animal animal = new Animal();

        animal.sex = ((ScriptableAnimal)inventory[index].item.data).sex.ToString();
        animal.otherSexName = ((ScriptableAnimal)inventory[index].item.data).otherSex.sex.ToString();
        animal.age = 0;
        animal.maxAge = ((ScriptableAnimal)inventory[index].item.data).maxAge;
        animal.toAdultAge = ((ScriptableAnimal)inventory[index].item.data).toAdultAge;
        animal.amountFoodNeeded = ((ScriptableAnimal)inventory[index].item.data).amountFoodNeeded;
        animal.amountWaterNeeded = ((ScriptableAnimal)inventory[index].item.data).amountWaterNeeded;
        animal.animalCategory = ((ScriptableAnimal)inventory[index].item.data).animalCategory.ToString();
        animal.distanceBeetweenBorn = ((ScriptableAnimal)inventory[index].item.data).distanceBeetweenBorn;
        animal.health = ((ScriptableAnimal)inventory[index].item.data).health;
        animal.lastBorn = 0;
        animal.lastTake = 0;
        animal.maxHealth = ((ScriptableAnimal)inventory[index].item.data).health;
        animal.name = ((ScriptableAnimal)inventory[index].item.data).name;
        animal.timeToTake = ((ScriptableAnimal)inventory[index].item.data).timeToTake;

        target.GetComponent<Breeding>().chicken.Add(animal);
        ItemSlot slot = inventory[index];
        slot.amount--;
        inventory[index] = slot;
    }
    [Command]
    public void CmdAddPig(int index)
    {
        if (!target.GetComponent<Breeding>()) return;

        int cow = GeneralManager.singleton.pig;
        int additionaToLevel = Convert.ToInt32(target.level / 10);
        if (additionaToLevel > 0)
        {
            additionaToLevel = additionaToLevel * 2;
        }
        int result = cow + additionaToLevel;

        if (target.GetComponent<Breeding>().goat.Count >= result) return;

        Animal animal = new Animal();

        animal.sex = ((ScriptableAnimal)inventory[index].item.data).sex.ToString();
        animal.otherSexName = ((ScriptableAnimal)inventory[index].item.data).otherSex.sex.ToString();
        animal.age = 0;
        animal.maxAge = ((ScriptableAnimal)inventory[index].item.data).maxAge;
        animal.toAdultAge = ((ScriptableAnimal)inventory[index].item.data).toAdultAge;
        animal.amountFoodNeeded = ((ScriptableAnimal)inventory[index].item.data).amountFoodNeeded;
        animal.amountWaterNeeded = ((ScriptableAnimal)inventory[index].item.data).amountWaterNeeded;
        animal.animalCategory = ((ScriptableAnimal)inventory[index].item.data).animalCategory.ToString();
        animal.distanceBeetweenBorn = ((ScriptableAnimal)inventory[index].item.data).distanceBeetweenBorn;
        animal.health = ((ScriptableAnimal)inventory[index].item.data).health;
        animal.lastBorn = 0;
        animal.lastTake = 0;
        animal.maxHealth = ((ScriptableAnimal)inventory[index].item.data).health;
        animal.name = ((ScriptableAnimal)inventory[index].item.data).name;
        animal.timeToTake = ((ScriptableAnimal)inventory[index].item.data).timeToTake;

        target.GetComponent<Breeding>().goat.Add(animal);
        ItemSlot slot = inventory[index];
        slot.amount--;
        inventory[index] = slot;
    }
    [Command]
    public void CmdAddHorse(int index)
    {
        if (!target.GetComponent<Breeding>()) return;

        int cow = GeneralManager.singleton.horse;
        int additionaToLevel = Convert.ToInt32(target.level / 10);
        if (additionaToLevel > 0)
        {
            additionaToLevel = additionaToLevel * 2;
        }
        int result = cow + additionaToLevel;

        if (target.GetComponent<Breeding>().horse.Count >= result) return;

        Animal animal = new Animal();

        animal.sex = ((ScriptableAnimal)inventory[index].item.data).sex.ToString();
        animal.otherSexName = ((ScriptableAnimal)inventory[index].item.data).otherSex.sex.ToString();
        animal.age = 0;
        animal.maxAge = ((ScriptableAnimal)inventory[index].item.data).maxAge;
        animal.toAdultAge = ((ScriptableAnimal)inventory[index].item.data).toAdultAge;
        animal.amountFoodNeeded = ((ScriptableAnimal)inventory[index].item.data).amountFoodNeeded;
        animal.amountWaterNeeded = ((ScriptableAnimal)inventory[index].item.data).amountWaterNeeded;
        animal.animalCategory = ((ScriptableAnimal)inventory[index].item.data).animalCategory.ToString();
        animal.distanceBeetweenBorn = ((ScriptableAnimal)inventory[index].item.data).distanceBeetweenBorn;
        animal.health = ((ScriptableAnimal)inventory[index].item.data).health;
        animal.lastBorn = 0;
        animal.lastTake = 0;
        animal.maxHealth = ((ScriptableAnimal)inventory[index].item.data).health;
        animal.name = ((ScriptableAnimal)inventory[index].item.data).name;
        animal.timeToTake = ((ScriptableAnimal)inventory[index].item.data).timeToTake;

        target.GetComponent<Breeding>().horse.Add(animal);
        ItemSlot slot = inventory[index];
        slot.amount--;
        inventory[index] = slot;
    }
    #endregion

    #region Building target managing

    [Command]
    public void CmdUpgradeBuilding()
    {
        Building building = null;
        if (target is Building)
        {
            building = ((Building)target);
        }
        if (!building || ((Building)target).level == 50 || building.countdown > 0 || CanInteractBuildingTarget(building, this) == false || playerAbility.networkAbilities[GeneralManager.singleton.FindNetworkAbility(building.building.abilityToUpgrade.name, this.name)].level < ((Building)target).level || gold < (building.building.goldToUpgrade * ((Building)target).level)) return;
        for (int i = 0; i < building.building.itemToUpgrade.Count; i++)
        {
            if (InventoryCount(new Item(building.building.itemToUpgrade[i].items)) < building.building.itemToUpgrade[i].amount * ((Building)target).level)
            {
                return;
            }
        }

        for (int e = 0; e < building.building.itemToUpgrade.Count; e++)
        {
            InventoryRemove(new Item(building.building.itemToUpgrade[e].items), building.building.itemToUpgrade[e].amount * ((Building)target).level);
        }

        ((Building)target).level++;
        ((Building)target).health += ((Building)target)._healthMax.bonusPerLevel;
        playerLeaderPoints.buildinPoint += GeneralManager.singleton.buildingUpgradePoint;
        experience += GeneralManager.singleton.buildingExperience;
        gold -= building.building.goldToUpgrade * ((Building)target).level;
        //if (building.building.coinToUpgrade >= (building.building.coinToUpgrade * ((Building)target).level)) coins -= building.building.coinToUpgrade * ((Building)target).level;

        for (int i = 0; i < quests.Count; i++)
        {
            Quest quest = quests[i];
            for (int e = 0; e < quest.buildingUpgrade.Count; e++)
            {
                for (int a = 0; a < quest.buildingUpgrade[e].buildingType.buildingList.Count; a++)
                {
                    if (quest.buildingUpgrade[e].buildingType.buildingList[a].buildingObject.name == ((Building)target).building.name)
                    {
                        UpgradeBuilding upgrade = quest.buildingUpgrade[e];
                        upgrade.itemLevelToReach = ((Building)target).level++;
                        quest.buildingUpgrade[e] = upgrade;
                    }
                }
            }
            quests[i] = quest;
        }
    }

    [Command]
    public void CmdRepairBuilding()
    {
        Building building = null;
        if (target is Building)
        {
            building = ((Building)target);
        }
        if (!building || building.countdown > 0 || CanInteractBuildingTarget(building, this) == false || playerAbility.networkAbilities[GeneralManager.singleton.FindNetworkAbility(building.building.abilityToRepair.name, this.name)].level < ((Building)target).level) return;
        for (int i = 0; i < building.building.itemToRepair.Count; i++)
        {
            if (InventoryCount(new Item(building.building.itemToRepair[i].items)) < building.building.itemToRepair[i].amount * ((Building)target).level)
            {
                return;
            }
        }

        for (int e = 0; e < building.building.itemToRepair.Count; e++)
        {
            InventoryRemove(new Item(building.building.itemToRepair[e].items), building.building.itemToRepair[e].amount * ((Building)target).level);
        }
        target.health = target.healthMax;
        playerLeaderPoints.buildinPoint += GeneralManager.singleton.buildingRepairPoint;
        experience += GeneralManager.singleton.buildingExperience;
    }

    [Command]
    public void CmdClaimBuilding()
    {
        Building building = null;
        if (target is Building)
        {
            building = ((Building)target);
        }
        //Debug.Log("Building : " + building);
        //Debug.Log("Building countdown: " + building.countdown);
        //Debug.Log("Can Interact: " + CanInteractBuildingTarget(building, this));
        //Debug.Log("Networkability : " + (playerAbility.networkAbilities[GeneralManager.singleton.FindNetworkAbility(building.building.abilityToClaim.name, this.name)].level < ((Building)target).level));
        //Debug.Log("Is premium zone : " + (building.owner != string.Empty && building.guild != string.Empty));
        if (!building || building.countdown > 0 || CanInteractBuildingTarget(building, this) == true || playerAbility.networkAbilities[GeneralManager.singleton.FindNetworkAbility(building.building.abilityToClaim.name, this.name)].level < ((Building)target).level ||
            (building.owner == string.Empty && building.guild == string.Empty) == true) return;

        building.owner = this.name;
        building.guild = guild.name;
        playerLeaderPoints.buildinPoint += GeneralManager.singleton.buildingRepairPoint;
        experience += GeneralManager.singleton.buildingExperience;
    }

    [Command]
    public void CmdRenameBuilding(string buildngName)
    {
        Building building = null;
        if (target is Building)
        {
            building = ((Building)target);
        }
        if (!building || building.countdown > 0 || !CanInteractBuildingTarget(building, this)) return;

        building.buildingName = buildngName;
    }

    [Command]
    public void CmdHalveBuilding(int currencyType)
    {
        Building building = null;
        if (target is Building)
        {
            building = ((Building)target);
        }
        if (!building || building.countdown == 0 || !CanInteractBuildingTarget(building, this)) return;

        if (currencyType == 0)
        {
            if (building.building.coinToHalve <= coins)
            {
                coins -= building.building.coinToHalve;
                building.countdown = Convert.ToInt32(building.countdown / 2);
                return;
            }
        }
        else
        {
            if (building.building.goldToHalve <= gold)
            {
                gold -= building.building.goldToHalve;
                building.countdown = Convert.ToInt32(building.countdown / 2);
                return;
            }
        }
    }

    [Command]
    public void CmdDestroyBuilding()
    {
        if (target && target is Building && !target.GetComponent<Plant>() && !target.GetComponent<Tree>() && !target.GetComponent<Rock>() && GeneralManager.singleton.CanManageExplosiveBuilding(((Building)target), this))
        {
            NetworkServer.Destroy(target.gameObject);
            if (((Building)target).navMeshObstacle2D)
            {
                Destroy(((Building)target).navMeshObstacle2D.go);
            }
            target = null;
        }
    }


    #endregion

    #region Plant to harvest

    [Command]
    public void CmdPlant(int index, int plantSlot)
    {
        CultivableField cultivableField;
        if (!target.GetComponent<CultivableField>()) return;
        else cultivableField = target.GetComponent<CultivableField>();

        if (inventory[index].amount > 0 && inventory[index].item.data is ScriptablePlant && ((ScriptablePlant)inventory[index].item.data).GrowSeason == TemperatureManager.singleton.season)
        {
            if (cultivableField.currentPlant[plantSlot].plantName == string.Empty || cultivableField.currentPlant[plantSlot].plantName == "Undefined")
            {
                CultivableFood food = cultivableField.currentPlant[plantSlot];
                food.plantName = inventory[index].item.name;
                food.dimension = 0;
                food.alreadyGrown = false;
                food.grownQuantityX = 0.0f;
                food.grownQuantityY = 0.0f;
                food.season = ((ScriptablePlant)inventory[index].item.data).GrowSeason;
                food.seeds = UnityEngine.Random.Range(1, ((ScriptablePlant)inventory[index].item.data).maxSeeds + 1);
                food.plantAmount = UnityEngine.Random.Range(1, ((ScriptablePlant)inventory[index].item.data).plantAmountHarvest + 1);
                cultivableField.currentPlant[plantSlot] = food;

                ItemSlot slot = inventory[index];
                slot.amount--;
                inventory[index] = slot;
            }
        }
    }

    [Command]
    public void CmdTakeSeeds(int plantSlot)
    {
        CultivableField cultivableField;
        if (!target || !target.GetComponent<CultivableField>()) return;
        else cultivableField = target.GetComponent<CultivableField>();
        if (ScriptablePlant.dict.TryGetValue(cultivableField.currentPlant[plantSlot].plantName.GetStableHashCode(), out ScriptablePlant plant))
        {
            if (!cultivableField.currentPlant[plantSlot].releaseSeeds)
            {
                if (InventoryCanAdd(new Item(plant.harvestSeeds), 1))
                {
                    InventoryAdd(new Item(plant.harvestSeeds), 1);
                    CultivableFood food = cultivableField.currentPlant[plantSlot];
                    food = new CultivableFood();
                    food.plantName = "Undefined";
                    food.grownQuantityX = 0.0f;
                    food.grownQuantityY = 0.0f;
                    cultivableField.currentPlant[plantSlot] = food;
                }
            }
            else
            {
                if (InventoryCanAdd(new Item(plant.harvestSeeds), cultivableField.currentPlant[plantSlot].seeds))
                {
                    InventoryAdd(new Item(plant.harvestSeeds), cultivableField.currentPlant[plantSlot].seeds);
                    CultivableFood food = cultivableField.currentPlant[plantSlot];
                    food = new CultivableFood();
                    food.plantName = "Undefined";
                    food.grownQuantityX = 0.0f;
                    food.grownQuantityY = 0.0f;
                    cultivableField.currentPlant[plantSlot] = food;
                }
            }
        }
    }
    [Command]
    public void CmdTakePlant(int plantSlot)
    {
        CultivableField cultivableField;
        if (!target || !target.GetComponent<CultivableField>()) return;
        else cultivableField = target.GetComponent<CultivableField>();
        if (ScriptablePlant.dict.TryGetValue(cultivableField.currentPlant[plantSlot].plantName.GetStableHashCode(), out ScriptablePlant plant))
        {
            if (cultivableField.currentPlant[plantSlot].alreadyGrown)
            {
                if (InventoryCanAdd(new Item(plant.harvestPlant), cultivableField.currentPlant[plantSlot].plantAmount))
                {
                    InventoryAdd(new Item(plant.harvestPlant), cultivableField.currentPlant[plantSlot].plantAmount);
                    CultivableFood food = cultivableField.currentPlant[plantSlot];
                    food = new CultivableFood();
                    food.plantName = "Undefined";
                    food.grownQuantityX = 0.0f;
                    food.grownQuantityY = 0.0f;
                    cultivableField.currentPlant[plantSlot] = food;
                    GeneralManager.singleton.plantPoint++;
                }
            }
        }
    }

    #endregion

    #region Warehouse
    [Command]
    public void CmdSwitchInventoryWarehouse(int[] warehouseInventory, int[] playerInventory, int warehouseContainer)
    {
        Warehouse warehouse;
        if (!target) return;
        if (!target.GetComponent<Warehouse>()) return;
        else warehouse = target.GetComponent<Warehouse>();

        int index = 0;
        List<int> inventoryL = playerInventory.ToList();
        List<int> warehouseL = warehouseInventory.ToList();
        List<int> warehouseSlotFree = new List<int>();
        List<int> inventorySlotFree = new List<int>();

        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].amount == 0)
            {
                inventorySlotFree.Add(i);
            }
        }

        if (warehouseContainer == 1)
        {
            for (int i = 0; i < warehouse.one.Count; i++)
            {
                if (warehouse.one[i].amount == 0)
                {
                    warehouseSlotFree.Add(i);
                }
            }
        }
        if (warehouseContainer == 2)
        {
            for (int i = 0; i < warehouse.two.Count; i++)
            {
                if (warehouse.two[i].amount == 0)
                {
                    warehouseSlotFree.Add(i);
                }
            }
        }
        if (warehouseContainer == 3)
        {
            for (int i = 0; i < warehouse.three.Count; i++)
            {
                if (warehouse.three[i].amount == 0)
                {
                    warehouseSlotFree.Add(i);
                }
            }
        }
        if (warehouseContainer == 4)
        {
            for (int i = 0; i < warehouse.four.Count; i++)
            {
                if (warehouse.four[i].amount == 0)
                {
                    warehouseSlotFree.Add(i);
                }
            }
        }
        if (warehouseContainer == 5)
        {
            for (int i = 0; i < warehouse.five.Count; i++)
            {
                if (warehouse.five[i].amount == 0)
                {
                    warehouseSlotFree.Add(i);
                }
            }
        }
        if (warehouseContainer == 6)
        {
            for (int i = 0; i < warehouse.six.Count; i++)
            {
                if (warehouse.six[i].amount == 0)
                {
                    warehouseSlotFree.Add(i);
                }
            }
        }
        if (InventorySlotsFree() < warehouseL.Count) return;
        if (warehouseSlotFree.Count < inventoryL.Count) return;

        if (inventoryL.Count > 0)
        {
            for (int i = 0; i < inventoryL.Count; i++)
            {
                index = i;
                if (warehouseContainer == 1)
                {
                    ItemSlot slot = warehouse.one[warehouseSlotFree[0]];
                    slot = inventory[inventoryL[i]];
                    warehouse.one[warehouseSlotFree[0]] = slot;
                    slot = new ItemSlot();
                    inventory[inventoryL[i]] = slot;
                    //inventoryL.RemoveAt(0);
                    warehouseSlotFree.RemoveAt(0);
                }
                if (warehouseContainer == 2)
                {
                    ItemSlot slot = warehouse.two[warehouseSlotFree[0]];
                    slot = inventory[inventoryL[i]];
                    warehouse.two[warehouseSlotFree[0]] = slot;
                    slot = new ItemSlot();
                    inventory[inventoryL[i]] = slot;
                    //inventoryL.RemoveAt(0);
                    warehouseSlotFree.RemoveAt(0);
                }
                if (warehouseContainer == 3)
                {
                    ItemSlot slot = warehouse.three[warehouseSlotFree[0]];
                    slot = inventory[inventoryL[i]];
                    warehouse.three[warehouseSlotFree[0]] = slot;
                    slot = new ItemSlot();
                    inventory[inventoryL[i]] = slot;
                    //inventoryL.RemoveAt(0);
                    warehouseSlotFree.RemoveAt(0);
                }
                if (warehouseContainer == 4)
                {
                    ItemSlot slot = warehouse.four[warehouseSlotFree[0]];
                    slot = inventory[inventoryL[i]];
                    warehouse.four[warehouseSlotFree[0]] = slot;
                    slot = new ItemSlot();
                    inventory[inventoryL[i]] = slot;
                    //inventoryL.RemoveAt(0);
                    warehouseSlotFree.RemoveAt(0);
                }
                if (warehouseContainer == 5)
                {
                    ItemSlot slot = warehouse.five[warehouseSlotFree[0]];
                    slot = inventory[inventoryL[i]];
                    warehouse.five[warehouseSlotFree[0]] = slot;
                    slot = new ItemSlot();
                    inventory[inventoryL[i]] = slot;
                    //inventoryL.RemoveAt(0);
                    warehouseSlotFree.RemoveAt(0);
                }
                if (warehouseContainer == 6)
                {
                    ItemSlot slot = warehouse.six[warehouseSlotFree[0]];
                    slot = inventory[inventoryL[i]];
                    warehouse.six[warehouseSlotFree[0]] = slot;
                    slot = new ItemSlot();
                    inventory[inventoryL[i]] = slot;
                    //inventoryL.RemoveAt(0);
                    warehouseSlotFree.RemoveAt(0);
                }
            }
        }
        if (warehouseL.Count > 0)
        {
            for (int i = 0; i < warehouseL.Count; i++)
            {
                index = i;
                if (warehouseContainer == 1)
                {
                    ItemSlot slot = inventory[inventorySlotFree[0]];
                    slot = warehouse.one[warehouseL[i]];
                    inventory[inventorySlotFree[0]] = slot;
                    slot = new ItemSlot();
                    warehouse.one[warehouseL[i]] = slot;
                    inventorySlotFree.RemoveAt(0);
                    //warehouseSlotFree.RemoveAt(0);
                }
                if (warehouseContainer == 2)
                {
                    ItemSlot slot = inventory[inventorySlotFree[0]];
                    slot = warehouse.two[warehouseL[i]];
                    inventory[inventorySlotFree[0]] = slot;
                    slot = new ItemSlot();
                    warehouse.two[warehouseL[i]] = slot;
                    inventorySlotFree.RemoveAt(0);
                    //warehouseSlotFree.RemoveAt(0);
                }
                if (warehouseContainer == 3)
                {
                    ItemSlot slot = inventory[inventorySlotFree[0]];
                    slot = warehouse.three[warehouseL[i]];
                    inventory[inventorySlotFree[0]] = slot;
                    slot = new ItemSlot();
                    warehouse.three[warehouseL[i]] = slot;
                    inventorySlotFree.RemoveAt(0);
                    //warehouseSlotFree.RemoveAt(0);
                }
                if (warehouseContainer == 4)
                {
                    ItemSlot slot = inventory[inventorySlotFree[0]];
                    slot = warehouse.four[warehouseL[i]];
                    inventory[inventorySlotFree[0]] = slot;
                    slot = new ItemSlot();
                    warehouse.four[warehouseL[i]] = slot;
                    inventorySlotFree.RemoveAt(0);
                    //warehouseSlotFree.RemoveAt(0);
                }
                if (warehouseContainer == 5)
                {
                    ItemSlot slot = inventory[inventorySlotFree[0]];
                    slot = warehouse.five[warehouseL[i]];
                    inventory[inventorySlotFree[0]] = slot;
                    slot = new ItemSlot();
                    warehouse.five[warehouseL[i]] = slot;
                    inventorySlotFree.RemoveAt(0);
                    //warehouseSlotFree.RemoveAt(0);
                }
                if (warehouseContainer == 6)
                {
                    ItemSlot slot = inventory[inventorySlotFree[0]];
                    slot = warehouse.six[warehouseL[i]];
                    inventory[inventorySlotFree[0]] = slot;
                    slot = new ItemSlot();
                    warehouse.six[warehouseL[i]] = slot;
                    inventorySlotFree.RemoveAt(0);
                    //warehouseSlotFree.RemoveAt(0);
                }
            }
        }
    }

    #endregion

    #region World House
    [Command]
    public void CmdTakeProductFromHouse(int index)
    {
        if (target && target.GetComponent<WorldHouse>())
        {
            WorldHouse house = target.GetComponent<WorldHouse>();
            if (house.items.Count >= index)
            {
                InventoryAdd(house.items[index].item, house.items[index].amount);
                house.items.Remove(house.items[index]);
            }
        }
    }
    #endregion

    #region Dynamite
    [Command]
    public void CmdActiveMine()
    {
        if (!target) return;

        if (target && target is Building && target.GetComponent<Mine>() && GeneralManager.singleton.CanManageExplosiveBuilding(((Building)target), this))
        {
            target.GetComponent<Mine>().activated = !target.GetComponent<Mine>().activated;
        }
    }

    [Command]
    public void CmdExplodeDynamite()
    {
        if (!target) return;

        if (target && target is Building && target.GetComponent<Dynamite>() && GeneralManager.singleton.CanManageExplosiveBuilding(((Building)target), this))
        {
            target.GetComponent<Dynamite>().Explode();
        }
    }
    #endregion

    #region Gas Station 
    [Command]
    public void CmdGetGasolineFromStation(int getGasoline)
    {
        if (!target || !target.GetComponent<GasStation>()) return;

        GasStation gasStation = target.GetComponent<GasStation>();

        if (playerCar.GetEmptyGasolineBootle() < getGasoline) return;

        for (int i = 0; i < inventory.Count; i++)
        {
            int index = i;
            if (inventory[index].amount > 0)
            {
                if (inventory[index].item.data.generalLiquidContainer > 0)
                {
                    if (inventory[index].item.honeyContainer > 0)
                        continue;
                    if (inventory[index].item.waterContainer > 0)
                        continue;

                    if (inventory[index].item.gasolineContainer >= 0)
                    {
                        ItemSlot slot = inventory[index];
                        int emptyGasoline = slot.item.data.generalLiquidContainer - slot.item.gasolineContainer;
                        if (getGasoline >= emptyGasoline)
                        {
                            slot.item.gasolineContainer += emptyGasoline;
                            gasStation.currentGasoline -= emptyGasoline;
                            getGasoline -= emptyGasoline;
                            inventory[index] = slot;
                            continue;
                        }
                        else
                        {
                            slot.item.gasolineContainer += getGasoline;
                            gasStation.currentGasoline -= getGasoline;
                            getGasoline = 0;
                            inventory[index] = slot;
                            continue;
                        }
                    }
                }
            }
        }
        if (gasStation.currentGasoline > GeneralManager.singleton.maxGasStationGasoline) gasStation.currentGasoline = GeneralManager.singleton.maxGasStationGasoline;
    }

    #endregion

    #region Totem
    [Command]
    public void CmdSetMessage(string meassage)
    {
        if (target && target.GetComponent<Totem>() && CanInteractBuildingTarget(((Building)target), this))
        {
            target.GetComponent<Totem>().message = meassage;
        }
    }
    #endregion

    #region Campfire
    public int GetWoodInInventory()
    {
        int wood = 0;
        for (int i = 0; i < inventory.Count; i++)
        {
            int index = i;
            if (inventory[index].amount > 0)
            {
                if (inventory[index].item.data is ScriptableWood)
                {
                    wood += inventory[index].amount;
                }
            }
        }
        return wood;
    }

    [Command]
    public void CmdPutWood(int putWood)
    {
        if (!(target.GetComponent<Campfire>())) return;

        if (!CanInteractBuildingTarget(((Building)target), this)) return;

        Campfire campfire = target.GetComponent<Campfire>();

        if (campfire.currentWood + putWood > GeneralManager.singleton.woodAmount.Get(target.level)) return;

        for (int i = 0; i < inventory.Count; i++)
        {
            int index = i;
            if (inventory[index].amount > 0)
            {
                if (inventory[index].item.data is ScriptableWood)
                {
                    if (inventory[index].amount >= putWood)
                    {
                        ItemSlot slot = inventory[index];
                        campfire.currentWood += putWood;
                        slot.amount -= putWood;
                        inventory[index] = slot;
                        continue;
                    }
                    if (inventory[index].amount < putWood)
                    {
                        ItemSlot slot = inventory[index];
                        campfire.currentWood += slot.amount;
                        putWood -= slot.amount;
                        slot.amount = 0;
                        inventory[index] = slot;
                        continue;
                    }
                }
            }
        }
        if (campfire.currentWood > GeneralManager.singleton.woodAmount.Get(target.level)) campfire.currentWood = GeneralManager.singleton.woodAmount.Get(target.level);
    }

    [Command]
    public void CmdTakeCampfireItems(int index)
    {
        if (!CanInteractBuildingTarget(target.GetComponent<Building>(), this)) return;

        if (target && target.GetComponent<Campfire>())
        {
            Campfire campfire = target.GetComponent<Campfire>();
            if (campfire.items[index].item.cookCountdown == 0 && InventoryCanAdd(campfire.items[index].item, 1))
            {
                InventoryAdd(campfire.items[index].item, 1);
                campfire.items.Remove(campfire.items[index]);
            }
        }
    }

    [Command]
    public void CmdAddCampfireItemToCook(int index)
    {
        if (!CanInteractBuildingTarget(((Building)target), this)) return;

        if (target && target.GetComponent<Campfire>())
        {
            Campfire campfire = target.GetComponent<Campfire>();
            if (inventory[index].amount > 0 && inventory[index].item.data is FoodItem)
            {
                if (campfire.items.Count < GeneralManager.singleton.maxCookedAmount.Get(target.level))
                {
                    ItemSlot slot = inventory[index];
                    ItemSlot cookedSlot = new ItemSlot(new Item(((FoodItem)slot.item.data).cookedItem));
                    campfire.items.Add(cookedSlot);
                    slot.amount--;
                    inventory[index] = slot;
                    //TargetInventoryIndex(index);
                }
            }
        }
    }

    [TargetRpc]
    public void TargetInventoryIndex(int index)
    {
        GetComponent<UICampfireInventory>().inventoryWoodIndex.Remove(index);
    }

    [Command]
    public void CmdActiveCampfire()
    {
        if (target && target.GetComponent<Campfire>())
        {
            if (!CanInteractBuildingTarget(((Building)target), this) || target.GetComponent<Building>().isPremiumZone) return;

            Campfire campfire = target.GetComponent<Campfire>();
            campfire.active = !campfire.active;
        }
    }

    #endregion

    #region PetTrainer

    [Command]
    public void CmdAddPetToExpList(PetExp petExp, int inventoryIndex, string petName)
    {
        //TimeSpan difference;
        //if (!string.IsNullOrEmpty(playerBoost.networkBoost[0].hiddenIslandTimer))
        //    difference = DateTime.Parse(playerBoost.networkBoost[0].hiddenIslandTimer.ToString()) - TimeZoneInfo.ConvertTime(System.DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById(GeneralManager.singleton.timeZone));

        petExp.timeEndServer = DateTime.Now.AddSeconds(GeneralManager.singleton.timeToUpgradeOfOneLevel * petExp.selectedFood).ToString();
        Debug.Log("Pet time end server " + petExp.timeEndServer);
        if (health == 0 || !target || !target.GetComponent<PetTrainer>() || InventoryCount(new Item(GeneralManager.singleton.foodToUpgradeLevelOfItem)) < petExp.selectedFood) return;
        if (inventory[inventoryIndex].amount > 0 && inventory[inventoryIndex].item.data is PetItem && inventory[inventoryIndex].item.name == petName)
        {
            target.GetComponent<PetTrainer>().petTraining.Add(petExp);
            ItemSlot slot = inventory[inventoryIndex];
            slot.amount--;
            inventory[inventoryIndex] = slot;

            InventoryRemove(new Item(GeneralManager.singleton.foodToUpgradeLevelOfItem), petExp.selectedFood);
        }
    }

    [Command]
    public void CmdAddToInventoryPetTrainer(string itenName, int amount, int finishedIndex, string playerName)
    {
        PetExp petExp = target.GetComponent<PetTrainer>().petTraining[finishedIndex];
        Debug.Log("Pet end server" + petExp.timeEndServer);
        if (DateTime.Now >= DateTime.Parse(petExp.timeEndServer) && petExp.owner == playerName)
        {
            if (ScriptableItem.dict.TryGetValue(itenName.GetStableHashCode(), out ScriptableItem item))
            {
                Pet newPet = new Pet();
                newPet._experience = (target.GetComponent<PetTrainer>().petTraining[finishedIndex].experienceToAdd);
                newPet.level = target.GetComponent<PetTrainer>().petTraining[finishedIndex].level;
                newPet.RecalculateExperience(newPet.experience);
                newPet._health = newPet._healthMax.Get(newPet.level);

                PetExp listItem = target.GetComponent<PetTrainer>().petTraining[finishedIndex];
                listItem.petItem.item.summonedLevel = newPet.level;
                listItem.petItem.item.summonedExperience = newPet._experience;
                listItem.petItem.item.summonedHealth = newPet._health;
                target.GetComponent<PetTrainer>().petTraining[finishedIndex] = listItem;

                InventoryAdd(target.GetComponent<PetTrainer>().petTraining[finishedIndex].petItem.item, amount);
                target.GetComponent<PetTrainer>().petTraining.RemoveAt(finishedIndex);
            }
        }
    }

    #endregion

    [Command]
    public void CmdChangeCoinGold(int coin)
    {
        if (coins >= coin)
        {
            coins -= coin;
            gold += coin * 5;
        }
    }

    [Command]
    public void CmdTakeClaimedItem(int index, string itemName, int itemAmount, string user, string seller)
    {
        AuctionHouse auctionHouse = target.GetComponent<AuctionHouse>();
        if (!auctionHouse) return;
        if (index > auctionHouse.FinshedBid.Count) return;
        if (auctionHouse.FinshedBid[index].itemSlot.item.name != itemName) return;
        if (auctionHouse.FinshedBid[index].itemSlot.amount != itemAmount) return;
        if (auctionHouse.FinshedBid[index].sellerName != seller) return;

        List<GoldToReturn> goldL = auctionHouse.FinshedBid[index].userThatBidThisObject.ToList();

        if (goldL.Count > 1)
        {
            for (int i = 0; i < goldL.Count - 1; i++)
            {
                Player.onlinePlayers.TryGetValue(goldL[i].userToReturn, out Player sellerPlayer);
                if (sellerPlayer)
                {
                    sellerPlayer.gold += goldL[i].goldToReturn;
                    goldL.RemoveAt(i);
                }
            }
        }

        if (InventoryCanAdd(auctionHouse.FinshedBid[index].itemSlot.item, auctionHouse.FinshedBid[index].itemSlot.amount))
        {
            InventoryAdd(auctionHouse.FinshedBid[index].itemSlot.item, auctionHouse.FinshedBid[index].itemSlot.amount);
            auctionHouse.FinshedBid.RemoveAt(index);
        }

        AuctionItem item = auctionHouse.FinshedBid[index];
        item.userThatBidThisObject = goldL.ToArray();
        auctionHouse.FinshedBid[index] = item;

        if (auctionHouse.FinshedBid[index].userThatBidThisObject.Length > 0)
            auctionHouse.itemToRestituite.Add(auctionHouse.FinshedBid[index]);
    }

    [Command]
    public void CmdBidActualItem(int index, int amount)
    {
        AuctionHouse auctionHouse = target.GetComponent<AuctionHouse>();
        if (!auctionHouse) return;
        if (index > auctionHouse.actualInBid.Count) return;

        if (auctionHouse.actualInBid[index].alreadyBidded) return;

        if (auctionHouse.actualInBid[index].remainingEstimateTime == 0) return;

        if ((auctionHouse.actualInBid[index].userThatBidThisObject.Length > 0 && auctionHouse.actualInBid[index].userThatBidThisObject.Last().goldToReturn >= amount) || amount == 0) return;

        if (amount <= auctionHouse.actualInBid[index].startBidPrice) return;

        AuctionItem item = auctionHouse.actualInBid[index];

        List<GoldToReturn> goldL = item.userThatBidThisObject.ToList();

        if (item.remainingEstimateTime > 0)
        {
            for (int i = 0; i < goldL.Count; i++)
            {
                Player.onlinePlayers.TryGetValue(goldL[i].userToReturn, out Player seller);
                if (seller)
                {
                    seller.gold += goldL[i].goldToReturn;
                    goldL.RemoveAt(i);
                }
            }
            GoldToReturn goldReturn = new GoldToReturn();
            goldReturn.goldToReturn = amount;
            goldReturn.userToReturn = name;
            goldL.Add(goldReturn);

            item.userThatBidThisObject = goldL.ToArray();
            auctionHouse.actualInBid[index] = item;
            gold -= amount;
        }
        else
        {
            AuctionItem item2 = auctionHouse.actualInBid[index];

            List<GoldToReturn> goldL2 = item.userThatBidThisObject.ToList();

            for (int i = 0; i < goldL2.Count; i++)
            {
                Player.onlinePlayers.TryGetValue(goldL2[i].userToReturn, out Player seller);
                if (seller)
                {
                    seller.gold += goldL2[i].goldToReturn;
                    goldL2.RemoveAt(i);
                }
            }
            auctionHouse.itemToRestituite.Add(auctionHouse.actualInBid[index]);
            auctionHouse.actualInBid.Remove(auctionHouse.actualInBid[index]);
        }
    }

    [Command]
    public void CmdBidoutItem(int index)
    {
        AuctionHouse auctionHouse = target.GetComponent<AuctionHouse>();
        if (!auctionHouse) return;
        if (index > auctionHouse.actualInBid.Count) return;

        if (auctionHouse.actualInBid[index].alreadyBidded) return;

        AuctionItem item = auctionHouse.actualInBid[index];

        List<GoldToReturn> goldL = item.userThatBidThisObject.ToList();


        for (int i = 0; i < goldL.Count; i++)
        {
            Player.onlinePlayers.TryGetValue(goldL[i].userToReturn, out Player seller);
            if (seller)
            {
                seller.gold += goldL[i].goldToReturn;
                goldL.RemoveAt(i);
            }
        }
        GoldToReturn itemGold = new GoldToReturn();
        itemGold.goldToReturn = auctionHouse.actualInBid[index].buyNowBid;
        itemGold.userToReturn = name;

        goldL.Add(itemGold);
        item.remainingEstimateTime = 0;
        item.alreadyBidded = true;
        item.userThatBidThisObject = goldL.ToArray();
        auctionHouse.actualInBid[index] = item;
    }

    #endregion

    #region Level up Player

    private bool justInstantiate = true;

    public void Start_LevelUp()
    {
        Invoke("ActiveLeveling", 5.0f);
    }

    public void ActiveLeveling()
    {
        justInstantiate = false;
        CancelInvoke("ActiveLeveling");
    }

    public void OnLevelUp_Player()
    {
        if ((isServer || isClient) && !justInstantiate)
        {
            levelUp.GetComponent<ParticleSystem>().Play();
            ParticleSystem[] particleSystems;
            particleSystems = GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem particleSystem in particleSystems)
            {
                particleSystem.Play();
            }
            UIPortrait.singleton.RefreshLevel();
        }
    }


    #endregion

    [Command]
    public void CmdAddCoin(int desiredCoins)
    {
        coins += desiredCoins;
    }

    public void SpawnFriendObject(string message)
    {
        UINotificationManager.singleton.SpawnFriendObject(message);
    }

    #endregion
}
