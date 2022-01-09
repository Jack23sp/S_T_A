﻿// The Chest class has a few different features that all aim to make Chests
// behave as realistically as possible.
//
// - **States:** first of all, the Chest has several different states like
// IDLE, ATTACKING, MOVING and DEATH. The Chest will randomly move around in
// a certain movement radius and try to attack any players in its aggro range.
// _Note: Chests use NavMeshAgents to move on the NavMesh._
//
// - **Aggro:** To save computations, we let Unity take care of finding players
// in the aggro range by simply adding a AggroArea _(see AggroArea.cs)_ sphere
// to the Chest's children in the Hierarchy. We then use the OnTrigger
// functions to find players that are in the aggro area. The Chest will always
// move to the nearest aggro player and then attack it as long as the player is
// in the follow radius. If the player happens to walk out of the follow
// radius then the Chest will walk back to the start position quickly.
//
// - **Respawning:** The Chests have a _respawn_ property that can be set to
// true in order to make the Chest respawn after it died. We developed the
// respawn system with simplicity in mind, there are no extra spawner objects
// needed. As soon as a Chest dies, it will make itself invisible for a while
// and then go back to the starting position to respawn. This feature allows the
// developer to quickly drag Chest Prefabs into the scene and place them
// anywhere, without worrying about spawners and spawn areas.
//
// - **Loot:** Dead Chests can also generate loot, based on the _lootItems_
// list. Each Chest has a list of items with their dropchance, so that loot
// will always be generated randomly. Chests can also randomly generate loot
// gold between a minimum and a maximum amount.
using UnityEngine;
using Mirror;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
//[RequireComponent(typeof(NetworkNavMeshAgent2D))]
public partial class Plant : Entity
{
    [Header("Plant Part")]
    public bool alreadyMaxGrow;
    public ScriptablePlant plant;
    [SyncVar]
    public float dimensionX;
    [SyncVar]
    public float dimensionY;

    [Header("Movement")]
    [Range(0, 1)] public float moveProbability = 0.1f; // chance per second
    public float moveDistance = 3;
    // Chests should follow their targets even if they run out of the movement
    // radius. the follow dist should always be bigger than the biggest archer's
    // attack range, so that archers will always pull aggro, even when attacking
    // from far away.
    public float followDistance = 5;
    [Range(0.1f, 1)] public float attackToMoveRangeRatio = 0.8f; // move as close as 0.8 * attackRange to a target

    [Header("Experience Reward")]
    public long rewardExperience = 10;
    public long rewardSkillExperience = 2;

    [Header("Loot")]
    public int lootGoldMin = 0;
    public int lootGoldMax = 10;
    public ItemDropChance[] dropChances;
    // note: Items have a .valid property that can be used to 'delete' an item.
    //       it's better than .RemoveAt() because we won't run into index-out-of
    //       range issues

    [Header("Respawn")]
    public float deathTime = 30f; // enough for animation & looting
    double deathTimeEnd; // double for long term precision
    public bool respawn = true;
    public float respawnTime = 10f;
    double respawnTimeEnd; // double for long term precision

    // save the start position for random movement distance and respawning
    Vector2 startPosition;

    // the last skill that was casted, to decide which one to cast next
    int lastSkill = -1;

    // networkbehaviour ////////////////////////////////////////////////////////
    protected override void Awake()
    {
        //plantObject = this;
        base.Awake();

        transform.name.Replace("(Clone)", "");
        // addon system hooks
        Utils.InvokeMany(typeof(Plant), this, "Awake_");
    }

    public override void OnStartServer()
    {
        // call Entity's OnStartServer
        base.OnStartServer();

        // all Chests should spawn with full health and mana
        health = healthMax;
        mana = manaMax;

        if (agent) agent.ResetPath();

    }

    protected override void Start()
    {
        base.Start();

        plantObject = this;

        // remember start position in case we need to respawn later
        startPosition = transform.position;

        transform.localScale = new Vector3(dimensionX, dimensionY, 0.0f);

        if (isServer)
        {
            InvokeRepeating("GrowPlant", plant.GrowInterval, plant.GrowInterval);
        }
        InvokeRepeating("GrowPlantOnClient", plant.GrowInterval, plant.GrowInterval);

        // addon system hooks
        //Utils.InvokeMany(typeof(Plant), this, "Start_");
    }

    public void GrowPlant()
    {
        if (transform.localScale.x >= plant.scaleDimension.x)
        {
            alreadyMaxGrow = true;
        }

        if (alreadyMaxGrow)
        {
            if (transform.localScale.x <= 0.0f)
            {
                NetworkServer.Destroy(this.gameObject);
            }

            if (TemperatureManager.singleton.season == plant.GrowSeason)
            {
                if (transform.localScale.x < plant.scaleDimension.x)
                {
                    dimensionX = dimensionX + plant.GrowAmount;
                }
                if (transform.localScale.y < plant.scaleDimension.y)
                {
                    dimensionY = dimensionY + plant.GrowAmount;
                }

            }
            else
            {
                dimensionX = dimensionX - plant.GrowAmount;
                dimensionY = dimensionY - plant.GrowAmount;


                if (transform.localScale.x < 0.0f || transform.localScale.y < 0.0f)
                {
                    NetworkServer.Destroy(this.gameObject);
                }
            }
        }
        else
        {
            if (TemperatureManager.singleton.season == plant.GrowSeason)
            {
                if (transform.localScale.x < plant.scaleDimension.x)
                {
                    dimensionX = dimensionX + plant.GrowAmount;
                }
                if (transform.localScale.y < plant.scaleDimension.y)
                {
                    dimensionY = dimensionY + plant.GrowAmount;
                }

            }
            else
            {

                dimensionX = dimensionX - plant.GrowAmount;
                dimensionY = dimensionY - plant.GrowAmount;

                if (transform.localScale.x < 0.0f || transform.localScale.y < 0.0f)
                {
                    NetworkServer.Destroy(this.gameObject);
                }
            }

        }
    }

    public void GrowPlantOnClient()
    {
        transform.localScale = new Vector3(dimensionX, dimensionY, 0.0f);
    }

    [Server]
    protected override string UpdateServer()
    {
        return "IDLE";
    }

    // finite state machine - client ///////////////////////////////////////////
    [Client]
    protected override void UpdateClient()
    {
        // addon system hooks
    }

    // aggro ///////////////////////////////////////////////////////////////////
    // this function is called by people who attack us and by AggroArea
    [ServerCallback]
    public override void OnAggro(Entity entity)
    {

    }

    // loot ////////////////////////////////////////////////////////////////////
    // other scripts need to know if it still has valid loot (to show UI etc.)
    public bool HasLoot()
    {
        // any gold or valid items?
        return false;
    }

    // death ///////////////////////////////////////////////////////////////////
    protected override void OnDeath()
    {
    }

    // skills //////////////////////////////////////////////////////////////////
    // we use 'is' instead of 'GetType' so that it works for inherited types too
    public override bool CanAttack(Entity entity)
    {
        return false;

    }

    // helper function to get the current cast range (if casting anything)
    public float CurrentCastRange()
    {
        return 0;
    }
}