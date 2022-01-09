// The Entity class is rather simple. It contains a few basic entity properties
// like health, mana and level that all inheriting classes like Players and
// Monsters can use.
//
// Entities also have a _target_ Entity that can't be synchronized with a
// SyncVar. Instead we created a EntityTargetSync component that takes care of
// that for us.
//
// Entities use a deterministic finite state machine to handle IDLE/MOVING/DEAD/
// CASTING etc. states and events. Using a deterministic FSM means that we react
// to every single event that can happen in every state (as opposed to just
// taking care of the ones that we care about right now). This means a bit more
// code, but it also means that we avoid all kinds of weird situations like 'the
// monster doesn't react to a dead target when casting' etc.
// The next state is always set with the return value of the UpdateServer
// function. It can never be set outside of it, to make sure that all events are
// truly handled in the state machine and not outside of it. Otherwise we may be
// tempted to set a state in CmdBeingTrading etc., but would likely forget of
// special things to do depending on the current state.
//
// Entities also need a kinematic Rigidbody so that OnTrigger functions can be
// called. Note that there is currently a Unity bug that slows down the agent
// when having lots of FPS(300+) if the Rigidbody's Interpolate option is
// enabled. So for now it's important to disable Interpolation - which is a good
// idea in general to increase performance.
using System;
using UnityEngine;
using Mirror;
using System.Collections;
using TMPro;
using System.Collections.Generic;

public enum DamageType : byte { Normal, Block, Crit, Fired, Poisoned, Explosive, Miss, Shock };

// note: no animator required, towers, dummies etc. may not have one
//[RequireComponent(typeof(Rigidbody2D))] // kinematic, only needed for OnTrigger
//[RequireComponent(typeof(VisibilityManager))]
//[RequireComponent(typeof(NavMeshAgent2D))]
[RequireComponent(typeof(AudioSource))]
public abstract partial class Entity : NetworkBehaviourNonAlloc
{
    [Header("Components")]
    public NavMeshAgent2D agent;
    public NetworkProximityChecker proxchecker;
    public Animator animator;
#pragma warning disable CS0109 // member does not hide accessible member
    new public Collider2D collider;
#pragma warning restore CS0109 // member does not hide accessible member
    public AudioSource audioSource;

    // finite state machine
    // -> state only writable by entity class to avoid all kinds of confusion
    [Header("State")]
    [SyncVar, SerializeField] public string _state = "IDLE";
    public string state => _state;

    // it's useful to know an entity's last combat time (did/was attacked)
    // e.g. to prevent logging out for x seconds after combat
    [SyncVar] public double lastCombatTime;

    // 'Entity' can't be SyncVar and NetworkIdentity causes errors when null,
    // so we use [SyncVar] GameObject and wrap it for simplicity
    [Header("Target")]
    [SyncVar] public GameObject _target;
    public Entity target
    {
        get { return _target != null ? _target.GetComponent<Entity>() : null; }
        set { _target = value != null ? value.gameObject : null; }
    }

    [Header("Level")]
    [SyncVar] public int level = 1;

    [Header("Health")]
    [SerializeField] public LinearInt _healthMax = new LinearInt { baseValue = 100 };
    public virtual int healthMax
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            //int passiveBonus = 0;
            //foreach (Skill skill in skills)
            //    if (skill.level > 0 && skill.data is PassiveSkill)
            //        passiveBonus += ((PassiveSkill)skill.data).healthMaxBonus.Get(skill.level);

            //int buffBonus = 0;
            //for (int i = 0; i < buffs.Count; ++i)
            //    buffBonus += buffs[i].healthMaxBonus;

            // base + passives + buffs
            //return _healthMax.Get(level) + passiveBonus + buffBonus;
            return _healthMax.Get(level);
        }
    }
    public bool invincible = false; // GMs, Npcs, ...
    [SyncVar] public int _health = 1;
    public int health
    {
        get { return Mathf.Min(_health, healthMax); } // min in case hp>hpmax after buff ends etc.
        set { _health = Mathf.Clamp(value, 0, healthMax); }
    }

    public bool healthRecovery = true; // can be disabled in combat etc.
    [SerializeField] protected LinearInt _healthRecoveryRate = new LinearInt { baseValue = 1 };
    public virtual int healthRecoveryRate
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            float passivePercent = 0;
            foreach (Skill skill in skills)
                if (skill.level > 0 && skill.data is PassiveSkill)
                    passivePercent += ((PassiveSkill)skill.data).healthPercentPerSecondBonus.Get(skill.level);

            float buffPercent = 0;
            for (int i = 0; i < buffs.Count; ++i)
                buffPercent += buffs[i].healthPercentPerSecondBonus;

            // base + passives + buffs
            return _healthRecoveryRate.Get(level) + Convert.ToInt32(passivePercent * healthMax) + Convert.ToInt32(buffPercent * healthMax);
        }
    }

    [Header("Mana")]
    [SerializeField] public LinearInt _manaMax = new LinearInt { baseValue = 100 };
    public virtual int manaMax
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            //int passiveBonus = 0;
            //foreach (Skill skill in skills)
            //    if (skill.level > 0 && skill.data is PassiveSkill)
            //        passiveBonus += ((PassiveSkill)skill.data).manaMaxBonus.Get(skill.level);

            //int buffBonus = 0;
            //for (int i = 0; i < buffs.Count; ++i)
            //    buffBonus += buffs[i].manaMaxBonus;

            //// base + passives + buffs
            //return _manaMax.Get(level) + passiveBonus + buffBonus;
            return _manaMax.Get(level);
        }
    }
    [SyncVar] int _mana = 1;
    public int mana
    {
        get { return Mathf.Min(_mana, manaMax); } // min in case hp>hpmax after buff ends etc.
        set { _mana = Mathf.Clamp(value, 0, manaMax); }
    }

    public bool manaRecovery = true; // can be disabled in combat etc.
    [SerializeField] protected LinearInt _manaRecoveryRate = new LinearInt { baseValue = 1 };
    public int manaRecoveryRate
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            float passivePercent = 0;
            foreach (Skill skill in skills)
                if (skill.level > 0 && skill.data is PassiveSkill)
                    passivePercent += ((PassiveSkill)skill.data).manaPercentPerSecondBonus.Get(skill.level);

            float buffPercent = 0;
            foreach (Buff buff in buffs)
                buffPercent += buff.manaPercentPerSecondBonus;

            // base + passives + buffs
            return _manaRecoveryRate.Get(level) + Convert.ToInt32(passivePercent * manaMax) + Convert.ToInt32(buffPercent * manaMax);
        }
    }

    [Header("Damage")]
    [SerializeField] public LinearInt _damage = new LinearInt { baseValue = 1 };
    public virtual int damage
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int passiveBonus = 0;
            foreach (Skill skill in skills)
                if (skill.level > 0 && skill.data is PassiveSkill)
                    passiveBonus += ((PassiveSkill)skill.data).damageBonus.Get(skill.level);

            int buffBonus = 0;
            foreach (Buff buff in buffs)
                buffBonus += buff.damageBonus;

            // base + passives + buffs
            return _damage.Get(level) + passiveBonus + buffBonus;
        }
    }

    [Header("Defense")]
    [SerializeField] public LinearFloat _defense = new LinearFloat { baseValue = 0.1f };
    public virtual float defense
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            float passiveBonus = 0;
            foreach (Skill skill in skills)
                if (skill.level > 0 && skill.data is PassiveSkill)
                    passiveBonus += ((PassiveSkill)skill.data).defenseBonus.Get(skill.level);

            float buffBonus = 0;
            foreach (Buff buff in buffs)
                buffBonus += buff.defenseBonus;

            // base + passives + buffs
            return _defense.Get(level) + passiveBonus + buffBonus;
        }
    }

    [Header("Block")]
    [SerializeField] protected LinearFloat _blockChance;
    public virtual float blockChance
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            float passiveBonus = 0;
            foreach (Skill skill in skills)
                if (skill.level > 0 && skill.data is PassiveSkill)
                    passiveBonus += ((PassiveSkill)skill.data).blockChanceBonus.Get(skill.level);

            float buffBonus = 0;
            foreach (Buff buff in buffs)
                buffBonus += buff.blockChanceBonus;

            // base + passives + buffs
            return _blockChance.Get(level) + passiveBonus + buffBonus;
        }
    }

    [Header("Critical")]
    [SerializeField] protected LinearFloat _criticalChance;
    public virtual float criticalChance
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            float passiveBonus = 0;
            foreach (Skill skill in skills)
                if (skill.level > 0 && skill.data is PassiveSkill)
                    passiveBonus += ((PassiveSkill)skill.data).criticalChanceBonus.Get(skill.level);

            float buffBonus = 0;
            foreach (Buff buff in buffs)
                buffBonus += buff.criticalChanceBonus;

            // base + passives + buffs
            return _criticalChance.Get(level) + passiveBonus + buffBonus;
        }
    }

    [Header("Speed")]
    [SerializeField] protected LinearFloat _speed = new LinearFloat { baseValue = 3 };
    public virtual float speed
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            float passiveBonus = 0;
            foreach (Skill skill in skills)
                if (skill.level > 0 && skill.data is PassiveSkill)
                    passiveBonus += ((PassiveSkill)skill.data).speedBonus.Get(skill.level);

            float buffBonus = 0;
            foreach (Buff buff in buffs)
                buffBonus += buff.speedBonus;

            // base + passives + buffs
            return _speed.Get(level) + passiveBonus + buffBonus;
        }
    }

    [Header("Damage Popup")]
    public GameObject damagePopupPrefab;

    // skill system for all entities (players, monsters, npcs, towers, ...)
    // 'skillTemplates' are the available skills (first one is default attack)
    // 'skills' are the loaded skills with cooldowns etc.
    [Header("Skills & Buffs")]
    public ScriptableSkill[] skillTemplates;
    public SyncListSkill skills = new SyncListSkill();
    public SyncListBuff buffs = new SyncListBuff(); // active buffs
    // current skill (synced because we need it as an animation parameter)
    [SyncVar, HideInInspector] public int currentSkill = -1;

    // effect mount is where the arrows/fireballs/etc. are spawned
    // -> can be overwritten, e.g. for mages to set it to the weapon's effect
    //    mount
    // -> assign to right hand or to self if in doubt!
#pragma warning disable CS0649 // Field is never assigned to
    [SerializeField] Transform _effectMount;
#pragma warning restore CS0649 // Field is never assigned to
    public virtual Transform effectMount { get { return _effectMount; } }

    // all entities should have an inventory, not just the player.
    // useful for monster loot, chests, etc.
    [Header("Inventory")]
    public SyncListItemSlot inventory = new SyncListItemSlot();

    // equipment needs to be in Entity because arrow shooting skills need to
    // check if the entity has enough arrows
    [Header("Equipment")]
    public SyncListItemSlot equipment = new SyncListItemSlot();

    // all entities should have gold, not just the player
    // useful for monster loot, chests etc.
    // note: int is not enough (can have > 2 mil. easily)
    [Header("Gold")]
    [SyncVar, SerializeField] long _gold = 0;
    public long gold { get { return _gold; } set { _gold = Math.Max(value, 0); } }

    // 3D text mesh for name above the entity's head
    [Header("Text Meshes")]
    public TextMesh stunnedOverlay;

    // every entity can be stunned by setting stunEndTime
    protected double stunTimeEnd;

    // safe zone flag
    // -> needs to be in Entity because both player and pet need it
    [HideInInspector] public bool inSafeZone;

    // look direction for animations and targetless skills
    // (NavMeshAgent itself just moves without actually looking anywhere)
    // => should always be normalized so that the animator doesn't do blending
    public Vector2 lookDirection = Vector2.down; // down by default

    public SpriteRenderer spriteRenderer;
    private Color color;

    [HideInInspector] public bool isServerObject;
    [HideInInspector] public bool isClientObject;
    [HideInInspector] public bool isServerOnlyObject;
    [HideInInspector] public bool isHiddenObject;
    [HideInInspector] public bool setted;

    [HideInInspector] public Building buildingObject;
    [HideInInspector] public Tree treeObject;
    [HideInInspector] public Rock rockObject;
    [HideInInspector] public Plant plantObject;
    [HideInInspector] public Monster monsterObject;

    public int spawnManager;

    // networkbehaviour ////////////////////////////////////////////////////////
    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        proxchecker = GetComponent<NetworkProximityChecker>();
        // addon system hooks
        Utils.InvokeMany(typeof(Entity), this, "Awake_");
    }

    public override void OnStartServer()
    {
        // addon system hooks
        Utils.InvokeMany(typeof(Entity), this, "OnStartServer_");
    }

    protected virtual void Start()
    {
        // health recovery every second
        if (buildingObject)
            InvokeRepeating(nameof(Recover), 5.0f, 5.0f);

        // dead if spawned without health
        if (health == 0) _state = "DEAD";



        // disable animator on server. this is a huge performance boost and
        // definitely worth one line of code (1000 monsters: 22 fps => 32 fps)
        // (!isClient because we don't want to do it in host mode either)
        // (OnStartServer doesn't know isClient yet, Start is the only option)
        if (!isClient) animator.enabled = false;
    }

    // monsters, npcs etc. don't have to be updated if no player is around
    // checking observers is enough, because lonely players have at least
    // themselves as observers, so players will always be updated
    // and dead monsters will respawn immediately in the first update call
    // even if we didn't update them in a long time (because of the 'end'
    // times)
    // -> update only if:
    //    - observers are null (they are null in clients)
    //    - if they are not null, then only if at least one (on server)
    //    - if the entity is hidden, otherwise it would never be updated again
    //      because it would never get new observers
    // -> can be overwritten if necessary (e.g. pets might be too far from
    //    observers but should still be updated to run to owner)
    public virtual bool IsWorthUpdating()
    {
        return netIdentity.observers == null ||
               netIdentity.observers.Count > 0 ||
               isHiddenObject;
    }

    // entity logic will be implemented with a finite state machine
    // -> we should react to every state and to every event for correctness
    // -> we keep it functional for simplicity
    // note: can still use LateUpdate for Updates that should happen in any case
    void Update()
    {
        if (!setted)
        {
            isServerObject = isServer;
            isServerOnlyObject = isServer && !isClient;
            isClientObject = isClient;
            isHiddenObject = IsHidden();
            setted = true;
        }

        if ((!buildingObject && !treeObject && !rockObject && !plantObject) && IsWorthUpdating())
        {
            // always apply speed to agent
            if (agent) agent.speed = speed;

            if (isClientObject)
            {
                UpdateClient();
            }
            if (isServerObject)
            {
                if (target != null && target.IsHidden()) target = null;
                _state = UpdateServer();
            }

            if (agent)
            {
                if (agent.velocity != Vector2.zero)
                    lookDirection = Utils.OrthonormalVector2(agent.velocity, lookDirection);
                else if (target != null)
                    lookDirection = Utils.OrthonormalVector2(target.transform.position - transform.position, lookDirection);
            }



            if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();

            if (!isServerOnlyObject) UpdateOverlays();
        }

    }

    // update for server. should return the new state.
    protected abstract string UpdateServer();

    // update for client.
    protected abstract void UpdateClient();

    // can be overwritten for more overlays
    protected virtual void UpdateOverlays()
    {
        if (buildingObject || treeObject || rockObject || plantObject) return;
        if (stunnedOverlay != null)
            stunnedOverlay.gameObject.SetActive(state == "STUNNED");
    }

    // visibility //////////////////////////////////////////////////////////////
    // hide a entity
    // note: using SetActive won't work because its not synced and it would
    //       cause inactive objects to not receive any info anymore
    // note: this won't be visible on the server as it always sees everything.
    [Server]
    public void Hide()
    {
        proxchecker.forceHidden = true;
    }

    [Server]
    public void Show()
    {
        proxchecker.forceHidden = false;
    }

    // is the entity currently hidden?
    // note: usually the server is the only one who uses forceHidden, the
    //       client usually doesn't know about it and simply doesn't see the
    //       GameObject.
    public bool IsHidden() => proxchecker.forceHidden;

    public float VisRange() => proxchecker.visRange;

    // -> agent.hasPath will be true if stopping distance > 0, so we can't
    //    really rely on that.
    // -> pathPending is true while calculating the path, which is good
    // -> remainingDistance is the distance to the last path point, so it
    //    also works when clicking somewhere onto a obstacle that isn't
    //    directly reachable.
    // -> velocity is the best way to detect WASD movement
    public bool IsMoving()
    {
        if (buildingObject || treeObject || rockObject || plantObject) return false;

        return agent && agent.pathPending ||
             agent.remainingDistance > agent.stoppingDistance ||
             agent.velocity != Vector2.zero;
    }
    // health & mana ///////////////////////////////////////////////////////////
    public float HealthPercent()
    {
        return (health != 0 && healthMax != 0) ? (float)health / (float)healthMax : 0;
    }

    [Server]
    public void Revive(float healthPercentage = 1)
    {
        health = Mathf.RoundToInt(healthMax * healthPercentage);
    }

    public float ManaPercent()
    {
        return (mana != 0 && manaMax != 0) ? (float)mana / (float)manaMax : 0;
    }

    // combat //////////////////////////////////////////////////////////////////
    // deal damage at another entity
    // (can be overwritten for players etc. that need custom functionality)
    //[Server]
    //public virtual void DealDamageAt(Entity entity, int amount, float stunChance=0, float stunTime=0)
    //{
    //    int damageDealt = 0;
    //    DamageType damageType = DamageType.Normal;

    //    // don't deal any damage if entity is invincible
    //    if (!entity.invincible)
    //    {
    //        // block? (we use < not <= so that block rate 0 never blocks)
    //        if (UnityEngine.Random.value < entity.blockChance)
    //        {
    //            damageType = DamageType.Block;
    //        }
    //        // deal damage
    //        else
    //        {
    //            // subtract defense (but leave at least 1 damage, otherwise
    //            // it may be frustrating for weaker players)
    //            damageDealt = Mathf.Max(amount - entity.defense, 1);

    //            // critical hit?
    //            if (UnityEngine.Random.value < criticalChance)
    //            {
    //                damageDealt *= 2;
    //                damageType = DamageType.Crit;
    //            }

    //            // deal the damage
    //            entity.health -= damageDealt;

    //            // stun?
    //            if (UnityEngine.Random.value < stunChance)
    //            {
    //                // dont allow a short stun to overwrite a long stun
    //                // => if a player is hit with a 10s stun, immediately
    //                //    followed by a 1s stun, we don't want it to end in 1s!
    //                double newStunEndTime = NetworkTime.time + stunTime;
    //                entity.stunTimeEnd = Math.Max(newStunEndTime, stunTimeEnd);
    //            }
    //        }
    //    }

    //    // let's make sure to pull aggro in any case so that archers
    //    // are still attacked if they are outside of the aggro range
    //    entity.OnAggro(this);

    //    // show effects on clients
    //    entity.RpcOnDamageReceived(damageDealt, damageType);

    //    // reset last combat time for both
    //    lastCombatTime = NetworkTime.time;
    //    entity.lastCombatTime = NetworkTime.time;

    //    // addon system hooks
    //    Utils.InvokeMany(typeof(Entity), this, "DealDamageAt_", entity, amount);
    //}

    // no need to instantiate damage popups on the server
    // -> calculating the position on the client saves server computations and
    //    takes less bandwidth (4 instead of 12 byte)
    [Client]
    void ShowDamagePopup(int amount, DamageType damageType)
    {
        if (buildingObject) return;
        // spawn the damage popup (if any) and set the text
        if (damagePopupPrefab != null)
        {
            // showing it above their head looks best, and we don't have to use
            // a custom shader to draw world space UI in front of the entity
            Bounds bounds = collider.bounds;
            Vector2 position = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);

            GameObject popup = Instantiate(damagePopupPrefab, position, Quaternion.identity);
            if (damageType == DamageType.Normal)
                popup.GetComponent<TextMeshPro>().text = amount.ToString();
            else if (damageType == DamageType.Block)
                popup.GetComponent<TextMeshPro>().text = "<i>Block!</i>";
            else if (damageType == DamageType.Crit)
                popup.GetComponent<TextMeshPro>().text = amount + " Crit!";
            else if (damageType == DamageType.Miss)
                popup.GetComponent<TextMeshPro>().text = amount + " Miss!";
            else if (damageType == DamageType.Fired)
                popup.GetComponent<TextMeshPro>().text = amount + " Fired!";
            else if (damageType == DamageType.Shock)
                popup.GetComponent<TextMeshPro>().text = amount + " Shock!";
            else if (damageType == DamageType.Poisoned)
                popup.GetComponent<TextMeshPro>().text = amount + " Poisoned!";
        }
    }

    [ClientRpc]
    void RpcOnDamageReceived(int amount, DamageType damageType)
    {
        if (buildingObject) return;

        // show popup above receiver's head in all observers via ClientRpc
        ShowDamagePopup(amount, damageType);
        if (target is Monster)
        {
            ChangeColor();
        }
            // addon system hooks
        Utils.InvokeMany(typeof(Entity), this, "OnDamageReceived_", amount, damageType);
    }

    public void ChangeColor()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Color c = spriteRenderer.color;
        c = Color.red;
        spriteRenderer.color = c; ;
    }
    // recovery ////////////////////////////////////////////////////////////////
    // recover health and mana once a second
    // note: when stopping the server with the networkmanager gui, it will
    //       generate warnings that Recover was called on client because some
    //       entites will only be disabled but not destroyed. let's not worry
    //       about that for now.
    [Server]
    public void Recover()
    {
        if (buildingObject) return;

        if (enabled && health > 0)
        {
            if (healthRecovery) health += healthRecoveryRate;
            if (manaRecovery) mana += manaRecoveryRate;
        }
    }

    // aggro ///////////////////////////////////////////////////////////////////
    // this function is called by the AggroArea (if any) on clients and server
    public virtual void OnAggro(Entity entity) { }

    // skill system ////////////////////////////////////////////////////////////
    // helper function to find a skill index
    public int GetSkillIndexByName(string skillName)
    {
        // (avoid FindIndex to minimize allocations)
        for (int i = 0; i < skills.Count; ++i)
            if (skills[i].name == skillName)
                return i;
        return -1;
    }

    // helper function to find a buff index
    public int GetBuffIndexByName(string buffName)
    {
        // (avoid FindIndex to minimize allocations)
        for (int i = 0; i < buffs.Count; ++i)
            if (buffs[i].name == buffName)
                return i;
        return -1;
    }

    // we need a function to check if an entity can attack another.
    // => overwrite to add more cases like 'monsters can only attack players'
    //    or 'player can attack pets but not own pet' etc.
    // => raycast NavMesh to prevent attacks through walls, while allowing
    //    attacks through steep hills etc. (unlike Physics.Raycast). this is
    //    very important to prevent exploits where someone might try to attack a
    //    boss monster through a dungeon wall, etc.
    public virtual bool CanAttack(Entity entity)
    {
        if (entity is Building)
        {
            return health > 0 &&
            entity.health > 0 &&
            entity != this;
        }

        if (entity is Car)
        {
            return false;
        }

        return health > 0 &&
               entity.health > 0 &&
               entity != this &&
               !inSafeZone && !entity.inSafeZone;
    }

    // the first check validates the caster
    // (the skill won't be ready if we check self while casting it. so the
    //  checkSkillReady variable can be used to ignore that if needed)
    // has a weapon (important for projectiles etc.), no cooldown, hp, mp?
    public bool CastCheckSelf(Skill skill, bool checkSkillReady = true) =>
        skill.CheckSelf(this, checkSkillReady);

    // the second check validates the target and corrects it for the skill if
    // necessary (e.g. when trying to heal an npc, it sets target to self first)
    // (skill shots that don't need a target will just return true if the user
    //  wants to cast them at a valid position)
    public bool CastCheckTarget(Skill skill) =>
        skill.CheckTarget(this);

    // the third check validates the distance between the caster and the target
    // (target entity or target position in case of skill shots)
    // note: castchecktarget already corrected the target (if any), so we don't
    //       have to worry about that anymore here
    public bool CastCheckDistance(Skill skill, out Vector2 destination) =>
        skill.CheckDistance(this, out destination);

    // starts casting
    public void StartCastSkill(Skill skill)
    {
        // start casting and set the casting end time
        skill.castTimeEnd = NetworkTime.time + skill.castTime;

        // save modifications
        skills[currentSkill] = skill;

        // rpc for client sided effects
        // -> pass that skill because skillIndex might be reset in the mean
        //    time, we never know
        RpcSkillCastStarted(skill);
    }

    // cancel a skill cast properly
    [Server]
    public void CancelCastSkill()
    {
        // reset cast time, otherwise if a buff has a 10s cast time and we
        // cancel the cast after 1s, then we would have to wait 9 more seconds
        // before we can attempt to cast it again.
        // -> we cancel it in any case. players will have to wait for 'casttime'
        //    when attempting another cast anyway.
        if (currentSkill != -1)
        {
            Skill skill = skills[currentSkill];
            skill.castTimeEnd = NetworkTime.time - skill.castTime;
            skills[currentSkill] = skill;

            // reset current skill
            currentSkill = -1;
        }
    }

    // finishes casting. casting and waiting has to be done in the state machine
    public void FinishCastSkill(Skill skill)
    {
        // * check if we can currently cast a skill (enough mana etc.)
        // * check if we can cast THAT skill on THAT target
        // note: we don't check the distance again. the skill will be cast even
        //   if the target walked a bit while we casted it (it's simply better
        //   gameplay and less frustrating)
        if (CastCheckSelf(skill, false) && CastCheckTarget(skill))
        {
            // let the skill template handle the action
            skill.Apply(this);

            // rpc for client sided effects
            // -> pass that skill because skillIndex might be reset in the mean
            //    time, we never know
            RpcSkillCastFinished(skill);

            // decrease mana in any case
            mana -= skill.manaCosts;

            // start the cooldown (and save it in the struct)
            skill.cooldownEnd = NetworkTime.time + skill.cooldown;

            // save any skill modifications in any case
            skills[currentSkill] = skill;
        }
        else
        {
            // not all requirements met. no need to cast the same skill again
            currentSkill = -1;
        }
    }

    // helper function to add or refresh a buff
    public void AddOrRefreshBuff(Buff buff)
    {
        // reset if already in buffs list, otherwise add
        int index = GetBuffIndexByName(buff.name);
        if (index != -1) buffs[index] = buff;
        else buffs.Add(buff);
    }

    // skill cast started rpc for client sided effects
    // note: no need to pass skillIndex, currentSkill is synced anyway
    [ClientRpc]
    public void RpcSkillCastStarted(Skill skill)
    {
        // validate: still alive?
        if (health > 0)
        {
            // call scriptableskill event
            skill.data.OnCastStarted(this);
        }
    }

    // skill cast finished rpc for client sided effects
    // note: no need to pass skillIndex, currentSkill is synced anyway
    [ClientRpc]
    public void RpcSkillCastFinished(Skill skill)
    {
        // validate: still alive?
        if (health > 0)
        {
            // call scriptableskill event
            skill.data.OnCastFinished(this);

            // maybe some other component needs to know about it too
            SendMessage("OnSkillCastFinished", skill, SendMessageOptions.DontRequireReceiver);
        }
    }

    // inventory ///////////////////////////////////////////////////////////////
    // helper function to find an item in the inventory
    public int GetInventoryIndexByName(string itemName)
    {
        // (avoid FindIndex to minimize allocations)
        for (int i = 0; i < inventory.Count; ++i)
        {
            ItemSlot slot = inventory[i];
            if (slot.amount > 0 && slot.item.name == itemName)
                return i;
        }
        return -1;
    }

    public int GetBeltIndexByName(string itemName)
    {
        // (avoid FindIndex to minimize allocations)
        for (int i = 0; i < ((Player)this).playerBelt.belt.Count; ++i)
        {
            ItemSlot slot = ((Player)this).playerBelt.belt[i];
            if (slot.amount > 0 && slot.item.name == itemName)
                return i;
        }
        return -1;
    }

    // helper function to count the free slots
    public int InventorySlotsFree()
    {
        // count manually. Linq is HEAVY(!) on GC and performance
        int free = 0;
        foreach (ItemSlot slot in inventory)
            if (slot.amount == 0)
                ++free;
        return free;
    }

    // helper function to calculate the occupied slots
    public int InventorySlotsOccupied()
    {
        // count manually. Linq is HEAVY(!) on GC and performance
        int occupied = 0;
        foreach (ItemSlot slot in inventory)
            if (slot.amount > 0)
                ++occupied;
        return occupied;
    }

    // helper function to calculate the total amount of an item type in inventory
    // note: .Equals because name AND dynamic variables matter (petLevel etc.)
    public int InventoryCount(Item item)
    {
        // count manually. Linq is HEAVY(!) on GC and performance
        int amount = 0;
        foreach (ItemSlot slot in inventory)
            if (slot.amount > 0 && slot.item.Equals(item))
                amount += slot.amount;
        return amount;
    }

    // helper function to remove 'n' items from the inventory
    public bool InventoryRemove(Item item, int amount)
    {
        for (int i = 0; i < inventory.Count; ++i)
        {
            ItemSlot slot = inventory[i];
            // note: .Equals because name AND dynamic variables matter (petLevel etc.)
            if (slot.amount > 0 && slot.item.Equals(item))
            {
                // take as many as possible
                amount -= slot.DecreaseAmount(amount);
                inventory[i] = slot;

                // are we done?
                if (amount == 0) return true;
            }
        }

        // if we got here, then we didn't remove enough items
        return false;
    }

    // helper function to check if the inventory has space for 'n' items of type
    // -> the easiest solution would be to check for enough free item slots
    // -> it's better to try to add it onto existing stacks of the same type
    //    first though
    // -> it could easily take more than one slot too
    // note: this checks for one item type once. we can't use this function to
    //       check if we can add 10 potions and then 10 potions again (e.g. when
    //       doing player to player trading), because it will be the same result
    public bool InventoryCanAdd(Item item, int amount)
    {
        // go through each slot
        for (int i = 0; i < inventory.Count; ++i)
        {
            // empty? then subtract maxstack
            if (inventory[i].amount == 0)
                amount -= item.maxStack;
            // not empty. same type too? then subtract free amount (max-amount)
            // note: .Equals because name AND dynamic variables matter (petLevel etc.)
            else if (inventory[i].item.Equals(item))
                amount -= (inventory[i].item.maxStack - inventory[i].amount);

            // were we able to fit the whole amount already?
            if (amount <= 0) return true;
        }

        // if we got here than amount was never <= 0
        return false;
    }

    // helper function to put 'n' items of a type into the inventory, while
    // trying to put them onto existing item stacks first
    // -> this is better than always adding items to the first free slot
    // -> function will only add them if there is enough space for all of them
    public bool InventoryAdd(Item item, int amount)
    {
        // we only want to add them if there is enough space for all of them, so
        // let's double check
        if (InventoryCanAdd(item, amount))
        {
            // add to same item stacks first (if any)
            // (otherwise we add to first empty even if there is an existing
            //  stack afterwards)
            for (int i = 0; i < inventory.Count; ++i)
            {
                // not empty and same type? then add free amount (max-amount)
                // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                if (inventory[i].amount > 0 && inventory[i].item.Equals(item))
                {
                    ItemSlot temp = inventory[i];
                    amount -= temp.IncreaseAmount(amount);
                    inventory[i] = temp;
                }

                // were we able to fit the whole amount already? then stop loop
                if (amount <= 0) return true;
            }

            // add to empty slots (if any)
            for (int i = 0; i < inventory.Count; ++i)
            {
                // empty? then fill slot with as many as possible
                if (inventory[i].amount == 0)
                {
                    int add = Mathf.Min(amount, item.maxStack);
                    inventory[i] = new ItemSlot(item, add);
                    amount -= add;
                }

                // were we able to fit the whole amount already? then stop loop
                if (amount <= 0) return true;
            }
            // we should have been able to add all of them
            if (amount != 0) Debug.LogError("inventory add failed: " + item.name + " " + amount);
        }
        return false;
    }

    // equipment ///////////////////////////////////////////////////////////////
    public int GetEquipmentIndexByName(string itemName)
    {
        return equipment.FindIndex(slot => slot.amount > 0 && slot.item.name == itemName);
    }

    // helper function to find the equipped weapon index
    // -> works for all entity types. returns -1 if no weapon equipped.
    public int GetEquippedWeaponIndex()
    {
        return equipment.FindIndex(slot => slot.amount > 0 &&
                                           slot.item.data is WeaponItem);
    }

    // get currently equipped weapon category to check if skills can be casted
    // with this weapon. returns "" if none.
    public string GetEquippedWeaponCategory()
    {
        // find the weapon slot
        int index = GetEquippedWeaponIndex();
        return index != -1 ? ((WeaponItem)equipment[index].item.data).category : "";
    }

    // death ///////////////////////////////////////////////////////////////////
    // universal OnDeath function that takes care of all the Entity stuff.
    // should be called by inheriting classes' finite state machine on death.
    [Server]
    protected virtual void OnDeath()
    {
        // clear movement/buffs/target/cast
        if (agent)
            agent.ResetMovement();


        target = null;
        CancelCastSkill();

        // clear buffs that shouldn't remain after death
        for (int i = 0; i < buffs.Count; ++i)
        {
            if (!buffs[i].remainAfterDeath)
            {
                buffs.RemoveAt(i);
                --i;
            }
        }

        // addon system hooks
        Utils.InvokeMany(typeof(Entity), this, "OnDeath_");
    }

    // ontrigger ///////////////////////////////////////////////////////////////
    // protected so that inheriting classes can use OnTrigger too, while also
    // calling those here via base.OnTriggerEnter/Exit
    protected virtual void OnTriggerEnter2D(Collider2D col)
    {
        // check if trigger first to avoid GetComponent tests for environment
        if (col.isTrigger && col.GetComponent<SafeZone>())
            inSafeZone = true;
    }

    protected virtual void OnTriggerExit2D(Collider2D col)
    {
        // check if trigger first to avoid GetComponent tests for environment
        if (col.isTrigger && col.GetComponent<SafeZone>())
            inSafeZone = false;
    }

    [Command]
    public void CmdDestroyDesiredBuilding(GameObject objectToExplode)
    {
        if (objectToExplode)
            NetworkServer.Destroy(objectToExplode);
    }

    [Server]
    public void DestroyObject()
    {
        NetworkServer.Destroy(this.gameObject);
    }

    List<int> durabilityItem = new List<int>();

    [Server]
    public virtual void DealDamageAt(Entity entity, int amount, float stunChance = 0, float stunTime = 0)
    {
        int damageDealt = 0;
        DamageType damageType = DamageType.Normal;

        if (entity is Rock)
        {
            if (this is Player)
            {
                if (!entity.invincible)
                {
                    // deal the damage
                    entity.health -= GeneralManager.singleton.damageToResource;

                    if (((Player)this).playerItemEquipment.firstWeapon.amount > 0)
                    {
                        if ((((Player)this).playerItemEquipment.firstWeapon.item.data).maxDurability.baseValue > 0 && ((Player)this).playerItemEquipment.firstWeapon.item.durability > 0)
                        {
                            ItemSlot slot = ((Player)this).equipment[0];
                            slot.item.durability--;
                            ((Player)this).equipment[0] = slot;
                        }
                    }

                    if (!((Player)this).playerOptions.blockSound)
                    {
                        if (((WeaponItem)((Player)this).playerItemEquipment.firstWeapon.item.data).ammoItems.Count == 0)
                        {
                            GeneralManager.singleton.shotAudioSource.clip = GeneralManager.singleton.meleeToRock;
                            GeneralManager.singleton.shotAudioSource.Play();
                        }
                    }

                    ((Player)this).RpcInstantiateRock(entity.transform);

                    int rand = UnityEngine.Random.Range(0, GeneralManager.singleton.FindNetworkAbilityLevel(((Player)this).playerRock.ability.name, ((Player)this).name) + 1);
                    if (rand <= 30)
                    {
                        if (((Player)this).InventoryCanAdd(GeneralManager.singleton.GetRockRewardsItem(((Rock)entity), "Normal"), 1))
                        {
                            ((Player)this).playerRock.TargetDamage("RockNormal", GeneralManager.singleton.GetRockRewards(((Rock)entity), "Normal"));
                            ((Player)this).InventoryAdd(GeneralManager.singleton.GetRockRewardsItem(((Rock)entity), "Normal"), 1);
                        }
                    }
                    if (rand > 30 && rand <= 45)
                    {
                        if (((Player)this).InventoryCanAdd(GeneralManager.singleton.GetRockRewardsItem(((Rock)entity), "Normal"), 2))
                        {
                            ((Player)this).playerRock.TargetDamage("RockNormal", GeneralManager.singleton.GetRockRewards(((Rock)entity), "Normal"));
                            ((Player)this).InventoryAdd(GeneralManager.singleton.GetRockRewardsItem(((Rock)entity), "Normal"), 2);
                        }
                    }
                    if (rand > 45 && rand <= 50)
                    {
                        if (((Player)this).InventoryCanAdd(GeneralManager.singleton.GetRockRewardsItem(((Rock)entity), "Normal"), 4))
                        {
                            ((Player)this).playerRock.TargetDamage("RockNormal", GeneralManager.singleton.GetRockRewards(((Rock)entity), "Normal"));
                            ((Player)this).InventoryAdd(GeneralManager.singleton.GetRockRewardsItem(((Rock)entity), "Normal"), 4);
                        }
                    }
                }
            }
            // show effects on clients
            //entity.RpcOnDamageReceived(amount, damageType);
        }
        else if (entity is Tree)
        {
            if (this is Player)
            {
                if (!entity.invincible)
                {
                    // deal the damage
                    entity.health -= GeneralManager.singleton.damageToResource;

                    if (((Player)this).playerItemEquipment.firstWeapon.amount > 0)
                    {
                        if ((((Player)this).playerItemEquipment.firstWeapon.item.data).maxDurability.baseValue > 0 && ((Player)this).playerItemEquipment.firstWeapon.item.durability > 0)
                        {
                            ItemSlot slot = ((Player)this).equipment[0];
                            slot.item.durability--;
                            ((Player)this).equipment[0] = slot;
                        }
                    }

                    if (!((Player)this).playerOptions.blockSound)
                    {
                        if (((WeaponItem)((Player)this).playerItemEquipment.firstWeapon.item.data).ammoItems.Count == 0)
                        {
                            GeneralManager.singleton.shotAudioSource.clip = GeneralManager.singleton.meleeToTree;
                            GeneralManager.singleton.shotAudioSource.Play();
                        }
                    }

                    ((Player)this).RpcInstantiateTree(entity.transform);

                    int rand = UnityEngine.Random.Range(0, GeneralManager.singleton.FindNetworkAbilityLevel(((Player)this).playerTree.ability.name, ((Player)this).name) + 1);
                    if (rand <= 30)
                    {
                        if (((Player)this).InventoryCanAdd(GeneralManager.singleton.GetTreeRewardsItem(((Tree)entity), "Normal"), 1))
                        {
                            ((Player)this).playerTree.TargetDamage("TreeNormal", GeneralManager.singleton.GetTreeRewards(((Tree)entity), "Normal"));
                            ((Player)this).InventoryAdd(GeneralManager.singleton.GetTreeRewardsItem(((Tree)entity), "Normal"), 1);
                        }
                    }
                    if (rand > 30 && rand <= 45)
                    {
                        if (((Player)this).InventoryCanAdd(GeneralManager.singleton.GetTreeRewardsItem(((Tree)entity), "Normal"), 2))
                        {
                            ((Player)this).playerTree.TargetDamage("TreeNormal", GeneralManager.singleton.GetTreeRewards(((Tree)entity), "Normal"));
                            ((Player)this).InventoryAdd(GeneralManager.singleton.GetTreeRewardsItem(((Tree)entity), "Normal"), 2);
                        }
                    }
                    if (rand > 45 && rand <= 50)
                    {
                        if (((Player)this).InventoryCanAdd(GeneralManager.singleton.GetTreeRewardsItem(((Tree)entity), "Normal"), 4))
                        {
                            ((Player)this).playerTree.TargetDamage("TreeNormal", GeneralManager.singleton.GetTreeRewards(((Tree)entity), "Normal"));
                            ((Player)this).InventoryAdd(GeneralManager.singleton.GetTreeRewardsItem(((Tree)entity), "Normal"), 4);
                        }
                    }

                }
                // show effects on clients
                //entity.RpcOnDamageReceived(amount, damageType);
            }
        }
        else if (entity is Monster || entity is Pet || entity is Mount)
        {
            if (this is Player)
            {
                TimeSpan difference;

                if (((Player)this).playerBoost.networkBoost.Count > 0 && !string.IsNullOrEmpty(((Player)this).playerBoost.networkBoost[0].doubleDamageToMonsterServer))
                    difference = DateTime.Parse(((Player)this).playerBoost.networkBoost[0].doubleDamageToMonsterServer.ToString()) - DateTime.Now;

                if (((Player)this).playerBoost.networkBoost.Count > 0 && !string.IsNullOrEmpty(((Player)this).playerBoost.networkBoost[0].doubleDamageToMonster) && difference.TotalSeconds > 0)
                {
                    amount *= 2;
                }
                // don't deal any damage if entity is invincible
                if (!entity.invincible)
                {
                    // block? (we use < not <= so that block rate 0 never blocks)
                    if (UnityEngine.Random.value < entity.blockChance)
                    {
                        damageType = DamageType.Block;
                    }
                    // deal damage
                    else
                    {
                        // subtract defense (but leave at least 1 damage, otherwise
                        // it may be frustrating for weaker players)
                        damageDealt = Mathf.Max(amount - Convert.ToInt32(entity.defense), 1);

                        // critical hit?
                        if (UnityEngine.Random.value < criticalChance)
                        {
                            damageDealt *= 2;
                            damageType = DamageType.Crit;
                        }

                        // deal the damage
                        entity.health -= damageDealt;

                        // stun?
                        if (UnityEngine.Random.value < stunChance)
                        {
                            // dont allow a short stun to overwrite a long stun
                            // => if a player is hit with a 10s stun, immediately
                            //    followed by a 1s stun, we don't want it to end in 1s!
                            double newStunEndTime = NetworkTime.time + stunTime;
                            entity.stunTimeEnd = Math.Max(newStunEndTime, stunTimeEnd);
                        }
                    }
                }

                // let's make sure to pull aggro in any case so that archers
                // are still attacked if they are outside of the aggro range
                entity.OnAggro(this);
                if (((Player)this).activePet != null && ((Player)this).activePet.autoAttack == true)
                {
                    ((Player)this).activePet.OnAggro(entity);
                }

                // show effects on clients
                entity.RpcOnDamageReceived(damageDealt, damageType);

                // reset last combat time for both
                lastCombatTime = NetworkTime.time;
                entity.lastCombatTime = NetworkTime.time;
            }
            if (this is Pet)
            {
                entity.health -= amount;
            }
        }
        else
        {
            if (this is Player)
            {
                if (!entity.invincible)
                {
                    if (entity is Player)
                    {
                        TimeSpan difference;
                        if (((Player)this).playerBoost.networkBoost.Count > 0 && !string.IsNullOrEmpty(((Player)this).playerBoost.networkBoost[0].doubleDamageToPlayerServer))
                            difference = DateTime.Parse(((Player)this).playerBoost.networkBoost[0].doubleDamageToPlayerServer.ToString()) - DateTime.Now;

                        if (((Player)this).playerBoost.networkBoost.Count > 0 && !string.IsNullOrEmpty(((Player)this).playerBoost.networkBoost[0].doubleDamageToPlayer) && difference.TotalSeconds > 0)
                        {
                            amount *= 2;
                        }

                        // block? (we use < not <= so that block rate 0 never blocks)
                        if (((Player)this).playerAccuracy.accuracy >= ((Player)entity).playerMiss.maxMiss)
                        {
                            if (((WeaponItem)equipment[equipment.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Weapon"))].item.data).ammoItems.Count == 0)
                            {
                                if (!entity.invincible)
                                {
                                    // block? (we use < not <= so that block rate 0 never blocks)
                                    if (UnityEngine.Random.value < entity.blockChance)
                                    {
                                        damageType = DamageType.Block;
                                    }
                                    // deal damage
                                    else
                                    {
                                        if (((WeaponItem)equipment[equipment.FindIndex(slot => slot.amount > 0 && ((WeaponItem)slot.item.data).category.StartsWith("Weapon"))].item.data).poisonedAttack)
                                        {
                                            ((Player)entity).playerPoisoning.currentPoisoning++;
                                            ((Player)entity).ManageDamageArmorHealth(amount);
                                            ((Player)this).RpcInstantiatePoisoned(entity.name);

                                            // let's make sure to pull aggro in any case so that archers
                                            // are still attacked if they are outside of the aggro range
                                            entity.OnAggro(this);
                                            if (((Player)this).activePet != null && ((Player)this).activePet.autoAttack == true)
                                            {
                                                ((Player)this).activePet.OnAggro(this);
                                            }

                                            // show effects on clients
                                            entity.RpcOnDamageReceived(amount, DamageType.Poisoned);

                                            // reset last combat time for both
                                            lastCombatTime = NetworkTime.time;
                                            entity.lastCombatTime = NetworkTime.time;
                                        }
                                        else if (((WeaponItem)equipment[equipment.FindIndex(slot => slot.amount > 0 && ((WeaponItem)slot.item.data).category.StartsWith("Weapon"))].item.data).armorBreak)
                                        {
                                            //if (UnityEngine.Random.value < criticalChance)
                                            //{
                                            amount *= 2;
                                            damageType = DamageType.Crit;
                                            //}

                                            ((Player)entity).ManageDamageArmorHealth(amount);
                                            // let's make sure to pull aggro in any case so that archers
                                            // are still attacked if they are outside of the aggro range
                                            entity.OnAggro(this);
                                            if (((Player)this).activePet != null && ((Player)this).activePet.autoAttack == true)
                                            {
                                                ((Player)this).activePet.OnAggro(this);
                                            }
                                            // show effects on clients
                                            entity.RpcOnDamageReceived(amount, damageType);

                                            // reset last combat time for both
                                            lastCombatTime = NetworkTime.time;
                                            entity.lastCombatTime = NetworkTime.time;
                                        }
                                        else if (((WeaponItem)equipment[equipment.FindIndex(slot => slot.amount > 0 && ((WeaponItem)slot.item.data).category.StartsWith("Weapon"))].item.data).electricWeapon)
                                        {
                                            ((Player)entity).playerElectric.amountElectric += GeneralManager.singleton.cycleAmountElectricToAdd;
                                            ((Player)this).RpcInstantiateElectric(entity.name);
                                            ((Player)entity).ManageDamageArmorHealth(amount);
                                            // let's make sure to pull aggro in any case so that archers
                                            // are still attacked if they are outside of the aggro range
                                            entity.OnAggro(this);
                                            if (((Player)this).activePet != null && ((Player)this).activePet.autoAttack == true)
                                            {
                                                ((Player)this).activePet.OnAggro(this);
                                            }

                                            // show effects on clients
                                            entity.RpcOnDamageReceived(amount, DamageType.Shock);

                                            // reset last combat time for both
                                            lastCombatTime = NetworkTime.time;
                                            entity.lastCombatTime = NetworkTime.time;
                                        }
                                        else if (((WeaponItem)equipment[equipment.FindIndex(slot => slot.amount > 0 && ((WeaponItem)slot.item.data).category.StartsWith("Weapon"))].item.data).firedWeapon)
                                        {
                                            ((Player)entity).playerFired.amountFired += GeneralManager.singleton.cycleAmountFireToAdd;
                                            ((Player)this).RpcInstantiateFire(entity.name);
                                            ((Player)entity).ManageDamageArmorHealth(amount);
                                            // let's make sure to pull aggro in any case so that archers
                                            // are still attacked if they are outside of the aggro range
                                            entity.OnAggro(this);
                                            if (((Player)this).activePet != null && ((Player)this).activePet.autoAttack == true)
                                            {
                                                ((Player)this).activePet.OnAggro(this);
                                            }

                                            // show effects on clients
                                            entity.RpcOnDamageReceived(amount, DamageType.Fired);

                                            // reset last combat time for both
                                            lastCombatTime = NetworkTime.time;
                                            entity.lastCombatTime = NetworkTime.time;
                                        }
                                        else
                                        {
                                            ((Player)entity).ManageDamageArmorHealth(amount);
                                            // let's make sure to pull aggro in any case so that archers
                                            // are still attacked if they are outside of the aggro range
                                            entity.OnAggro(this);
                                            if (((Player)this).activePet != null && ((Player)this).activePet.autoAttack == true)
                                            {
                                                ((Player)this).activePet.OnAggro(this);
                                            }

                                            // show effects on clients
                                            entity.RpcOnDamageReceived(amount, DamageType.Normal);

                                            // reset last combat time for both
                                            lastCombatTime = NetworkTime.time;
                                            entity.lastCombatTime = NetworkTime.time;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (((AmmoItem)equipment[equipment.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Ammo"))].item.data).armorBreaker)
                                {
                                    ((Player)entity).ManageDamageArmorHealth(amount * 2);
                                    // let's make sure to pull aggro in any case so that archers
                                    // are still attacked if they are outside of the aggro range
                                    entity.OnAggro(this);
                                    if (((Player)this).activePet != null && ((Player)this).activePet.autoAttack == true)
                                    {
                                        ((Player)this).activePet.OnAggro(entity);
                                    }

                                    // show effects on clients
                                    entity.RpcOnDamageReceived(amount, DamageType.Crit);

                                    // reset last combat time for both
                                    lastCombatTime = NetworkTime.time;
                                    entity.lastCombatTime = NetworkTime.time;

                                    // stun?
                                    if (UnityEngine.Random.value < stunChance)
                                    {
                                        // dont allow a short stun to overwrite a long stun
                                        // => if a player is hit with a 10s stun, immediately
                                        //    followed by a 1s stun, we don't want it to end in 1s!
                                        double newStunEndTime = NetworkTime.time + stunTime;
                                        entity.stunTimeEnd = Math.Max(newStunEndTime, stunTimeEnd);
                                    }
                                }
                                else if (((AmmoItem)equipment[equipment.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Ammo"))].item.data).poisoned)
                                {
                                    if (((Player)entity).playerPoisoning.currentPoisoning < ((Player)entity).playerPoisoning.maxPoisoning) ((Player)entity).playerPoisoning.currentPoisoning++;
                                    ((Player)entity).ManageDamageArmorHealth(amount);
                                    ((Player)this).RpcInstantiatePoisoned(entity.name);
                                    // let's make sure to pull aggro in any case so that archers
                                    // are still attacked if they are outside of the aggro range
                                    entity.OnAggro(this);
                                    if (((Player)this).activePet != null && ((Player)this).activePet.autoAttack == true)
                                    {
                                        ((Player)this).activePet.OnAggro(entity);
                                    }

                                    // show effects on clients
                                    entity.RpcOnDamageReceived(amount, DamageType.Poisoned);

                                    // reset last combat time for both
                                    lastCombatTime = NetworkTime.time;
                                    entity.lastCombatTime = NetworkTime.time;

                                    // stun?
                                    if (UnityEngine.Random.value < stunChance)
                                    {
                                        // dont allow a short stun to overwrite a long stun
                                        // => if a player is hit with a 10s stun, immediately
                                        //    followed by a 1s stun, we don't want it to end in 1s!
                                        double newStunEndTime = NetworkTime.time + stunTime;
                                        entity.stunTimeEnd = Math.Max(newStunEndTime, stunTimeEnd);
                                    }
                                }
                                else if (((AmmoItem)equipment[equipment.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Ammo"))].item.data).fired)
                                {
                                    ((Player)entity).playerFired.amountFired += GeneralManager.singleton.cycleAmountFireToAdd;
                                    ((Player)this).RpcInstantiateFire(entity.name);
                                    ((Player)entity).ManageDamageArmorHealth(amount);
                                    // let's make sure to pull aggro in any case so that archers
                                    // are still attacked if they are outside of the aggro range
                                    entity.OnAggro(this);
                                    if (((Player)this).activePet != null && ((Player)this).activePet.autoAttack == true)
                                    {
                                        ((Player)this).activePet.OnAggro(entity);
                                    }

                                    // show effects on clients
                                    entity.RpcOnDamageReceived(amount, DamageType.Fired);

                                    // reset last combat time for both
                                    lastCombatTime = NetworkTime.time;
                                    entity.lastCombatTime = NetworkTime.time;

                                    // stun?
                                    if (UnityEngine.Random.value < stunChance)
                                    {
                                        // dont allow a short stun to overwrite a long stun
                                        // => if a player is hit with a 10s stun, immediately
                                        //    followed by a 1s stun, we don't want it to end in 1s!
                                        double newStunEndTime = NetworkTime.time + stunTime;
                                        entity.stunTimeEnd = Math.Max(newStunEndTime, stunTimeEnd);
                                    }
                                }
                                else if (!((AmmoItem)equipment[equipment.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Ammo"))].item.data).armorBreaker &&
                                        !((AmmoItem)equipment[equipment.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Ammo"))].item.data).poisoned &&
                                        !((AmmoItem)equipment[equipment.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Ammo"))].item.data).fired)
                                {
                                    ((Player)entity).ManageDamageArmorHealth(amount);
                                    // let's make sure to pull aggro in any case so that archers
                                    // are still attacked if they are outside of the aggro range
                                    entity.OnAggro(this);
                                    if (((Player)this).activePet != null && ((Player)this).activePet.autoAttack == true)
                                    {
                                        ((Player)this).activePet.OnAggro(entity);
                                    }

                                    // show effects on clients
                                    entity.RpcOnDamageReceived(amount, DamageType.Normal);

                                    // reset last combat time for both
                                    lastCombatTime = NetworkTime.time;
                                    entity.lastCombatTime = NetworkTime.time;

                                    // stun?
                                    if (UnityEngine.Random.value < stunChance)
                                    {
                                        // dont allow a short stun to overwrite a long stun
                                        // => if a player is hit with a 10s stun, immediately
                                        //    followed by a 1s stun, we don't want it to end in 1s!
                                        double newStunEndTime = NetworkTime.time + stunTime;
                                        entity.stunTimeEnd = Math.Max(newStunEndTime, stunTimeEnd);
                                    }
                                }

                            }
                        }
                        else
                        {

                            // let's make sure to pull aggro in any case so that archers
                            // are still attacked if they are outside of the aggro range
                            entity.OnAggro(this);
                            if (((Player)this).activePet != null && ((Player)this).activePet.autoAttack == true)
                            {
                                ((Player)this).activePet.OnAggro(entity);
                            }

                            // show effects on clients
                            entity.RpcOnDamageReceived(damageDealt, DamageType.Miss);

                            // reset last combat time for both
                            lastCombatTime = NetworkTime.time;
                            entity.lastCombatTime = NetworkTime.time;
                        }
                    }
                    else
                    {

                        if (!entity.invincible)
                        {
                            // block? (we use < not <= so that block rate 0 never blocks)
                            if (UnityEngine.Random.value < entity.blockChance)
                            {
                                damageType = DamageType.Block;
                            }
                            // deal damage
                            else
                            {
                                // subtract defense (but leave at least 1 damage, otherwise
                                // it may be frustrating for weaker players)
                                damageDealt = Mathf.Max(amount - Convert.ToInt32(entity.defense), 1);

                                // critical hit?
                                if (UnityEngine.Random.value < criticalChance)
                                {
                                    damageDealt *= 2;
                                    damageType = DamageType.Crit;
                                }

                                if (entity is Building)
                                {
                                    if (!((Player)this).playerOptions.blockSound)
                                    {
                                        if (((WeaponItem)((Player)this).playerItemEquipment.firstWeapon.item.data).ammoItems.Count == 0)
                                        {
                                            GeneralManager.singleton.shotAudioSource.clip = GeneralManager.singleton.meleeToBuilding;
                                            GeneralManager.singleton.shotAudioSource.Play();
                                        }
                                    }

                                    ((Player)this).RpcInstantiateBuilding(entity.transform);
                                }

                                // deal the damage
                                if (entity is Player) ((Player)entity).ManageDamageArmorHealth(damageDealt);
                                else entity.health -= damageDealt;

                                if (((Player)this).activePet != null && ((Player)this).activePet.autoAttack == true)
                                {
                                    ((Player)this).activePet.OnAggro(entity);
                                }

                                if (entity is Building)
                                {
                                    entity.RpcOnDamageReceived(damageDealt, DamageType.Normal);
                                }

                                // stun?
                                if (UnityEngine.Random.value < stunChance)
                                {
                                    // dont allow a short stun to overwrite a long stun
                                    // => if a player is hit with a 10s stun, immediately
                                    //    followed by a 1s stun, we don't want it to end in 1s!
                                    double newStunEndTime = NetworkTime.time + stunTime;
                                    entity.stunTimeEnd = Math.Max(newStunEndTime, stunTimeEnd);
                                }
                            }
                        }
                    }
                }
            }
            else if (this is Monster)
            {
                if (entity is Player)
                {
                    if (!entity.invincible)
                    {
                        // block? (we use < not <= so that block rate 0 never blocks)
                        if (UnityEngine.Random.value < entity.blockChance)
                        {
                            damageType = DamageType.Block;
                        }
                        // deal damage
                        else
                        {
                            // subtract defense (but leave at least 1 damage, otherwise
                            // it may be frustrating for weaker players)
                            damageDealt = amount;

                            // critical hit?
                            if (UnityEngine.Random.value < criticalChance)
                            {
                                damageDealt *= 2;
                                damageType = DamageType.Crit;
                            }

                            // deal the damage
                            if (entity is Player)
                            {
                                ((Player)entity).ManageDamageArmorHealth(damageDealt);
                            }
                            else entity.health -= damageDealt;

                            if (((Player)entity).activePet != null && ((Player)entity).activePet.defendOwner == true)
                            {
                                ((Player)entity).activePet.OnAggro(entity);
                            }

                            entity.RpcOnDamageReceived(damageDealt, DamageType.Normal);

                            // stun?
                            if (UnityEngine.Random.value < stunChance)
                            {
                                // dont allow a short stun to overwrite a long stun
                                // => if a player is hit with a 10s stun, immediately
                                //    followed by a 1s stun, we don't want it to end in 1s!
                                double newStunEndTime = NetworkTime.time + stunTime;
                                entity.stunTimeEnd = Math.Max(newStunEndTime, stunTimeEnd);
                            }
                        }
                    }
                }
            }
        }

        if (this is Monster && entity is Player)
        {
            //if (((Player)entity).playerItemEquipment.firstWeapon.amount > 0)
            //{
            //    if ((((Player)entity).playerItemEquipment.firstWeapon.item.data).maxDurability.baseValue > 0 && ((Player)entity).playerItemEquipment.firstWeapon.item.durability > 0)
            //    {
            //        ItemSlot slot = ((Player)entity).equipment[0];
            //        slot.item.durability--;
            //        ((Player)entity).equipment[0] = slot;
            //    }
            //}
            durabilityItem.Clear();

            for (int i = 0; i < equipment.Count; i++)
            {
                if (equipment[i].amount > 0)
                {
                    if (equipment[i].item.durability > 0)
                    {
                        durabilityItem.Add(i);
                    }
                }
            }
            if (durabilityItem.Count > 0)
            {
                int random = UnityEngine.Random.Range(0, durabilityItem.Count);
                if (equipment[durabilityItem[random]].item.durability > 0)
                {
                    ItemSlot slot = equipment[durabilityItem[random]];
                    slot.item.durability--;
                    equipment[durabilityItem[random]] = slot;
                }
            }
        }
    }

    public int GetEquippedAmmoIndex()
    {
        return equipment.FindIndex(slot => slot.amount > 0 &&
                                           slot.item.data is AmmoItem);
    }

}
