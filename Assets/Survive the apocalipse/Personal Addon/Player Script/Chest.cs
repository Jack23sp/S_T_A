// The Chest class has a few different features that all aim to make Chests
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

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NetworkNavMeshAgent2D))]
public partial class Chest : Entity
{

    // save the start position for random movement distance and respawning
    Vector2 startPosition;

    // the last skill that was casted, to decide which one to cast next
    int lastSkill = -1;

    // networkbehaviour ////////////////////////////////////////////////////////
    protected override void Awake()
    {
        base.Awake();

        // addon system hooks
        Utils.InvokeMany(typeof(Chest), this, "Awake_");
    }

    public override void OnStartServer()
    {
        // call Entity's OnStartServer
        base.OnStartServer();
    }

    protected override void Start()
    {
        base.Start();

        // remember start position in case we need to respawn later
        startPosition = transform.position;

    }

    void LateUpdate()
    {
    }



    [Server]
    protected override string UpdateServer()
    {
        if (health == 0 || !HasLoot()) NetworkServer.Destroy(this.gameObject);
        return "IDLE";
        Debug.LogError("invalid state:" + state);
    }

    // finite state machine - client ///////////////////////////////////////////
    [Client]
    protected override void UpdateClient()
    {
    }

    public bool HasLoot()
    {
        // any gold or valid items?
        return gold > 0 || InventorySlotsOccupied() > 0;
    }
}
