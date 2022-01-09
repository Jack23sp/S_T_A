using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using CustomType;
using UnityEngine.UI;
using System;

public class Building : Entity
{
    private Transform buildingTransform;
    public ScriptableBuilding building;

    [SyncVar] public string buildingName;
    [SyncVar] public int countdown;

    [Header("Appartain part")]
    [SyncVar] public string owner;
    [SyncVar] public string guild;

    [Header("Obstacle part")]
    [SyncVar]
    public bool obstacle;
    public NavMeshObstacle2D navMeshObstacle2D;

    [Header("Prefab to instantiate")]
    public int actualBuildinigRotation = 0;

    public List<Collider2D> colliders = new List<Collider2D>();

    public TextMesh textMesh;

    [SyncVar]
    public bool isHide;

    private Color pColor;
    public Entity mainEntity;

    public bool isPremiumZone;
    public GameObject placement;


    public void ManageObstacleObject()
    {
        List<GameObject> childObjects = new List<GameObject>();
        childObjects.Add(transform.GetChild(1).gameObject);
        childObjects.Add(transform.GetChild(2).gameObject);
        childObjects.Add(transform.GetChild(3).gameObject);
        childObjects.Add(transform.GetChild(4).gameObject);
        for (int i = 0; i < childObjects.Count; i++)
        {
            int index = i;
            NavMeshObstacle2D[] navMeshObstacle2Ds = childObjects[index].GetComponents<NavMeshObstacle2D>();
            if (childObjects[index].activeInHierarchy == true)
            {
                for (int e = 0; e < navMeshObstacle2Ds.Length; e++)
                {
                    navMeshObstacle2Ds[e].OnEnable();
                }
            }
            else
            {
                for (int e = 0; e < navMeshObstacle2Ds.Length; e++)
                {
                    navMeshObstacle2Ds[e].OnDisable();
                }
            }
        }
    }

    protected override void Awake()
    {
        buildingObject = this;
        base.Awake();

        buildingTransform = GetComponent<Transform>();
        name.Replace("(Clone)", "");
        if (isClient || isServer) Destroy(transform.GetChild(0).gameObject);
        if (isServer)
        {
            health = healthMax;
            CancelInvoke(nameof(Recover));
        }
        if (!mainEntity) mainEntity = GetComponent<Entity>();
        if (!placement) placement = transform.GetChild(0).GetChild(0).gameObject;

        InvokeRepeating(nameof(ManageBuilding), 0.3f, 0.3f);
        obstacle = building && building.isObstacle;
        if (navMeshObstacle2D && obstacle)
        {
            navMeshObstacle2D.Awake();
            navMeshObstacle2D.enabled = true;
        }

    }

    void Update()
    {
        if (isServer)
        {

            if (mainEntity.health == 0)
            {
                GameObject g = Instantiate(GeneralManager.singleton.smokePrefab, transform.position, Quaternion.identity);
                NetworkServer.Spawn(g);
                BuildingManager.singleton.RemoveFromList(this.gameObject);
                NetworkServer.Destroy(this.gameObject);
            }
        }
    }


    public void ManageBuilding()
    {
        if (placement && placement.activeInHierarchy)
            SetColor();
        else
            CancelInvoke(nameof(ManageBuilding));
    }

    public void DecreaseCountdown()
    {
        if (countdown > 0)
        {
            countdown--;
        }
    }

    public void Up()
    {
        Vector3 pos = buildingTransform.position;
        pos.y += GeneralManager.singleton.buildingSensibility;

        buildingTransform.position = pos;
    }
    public void Left()
    {
        Vector3 pos = buildingTransform.position;
        pos.x -= GeneralManager.singleton.buildingSensibility;

        buildingTransform.position = pos;
    }
    public void Down()
    {
        Vector3 pos = buildingTransform.position;
        pos.y -= GeneralManager.singleton.buildingSensibility;

        buildingTransform.position = pos;
    }
    public void Right()
    {
        Vector3 pos = buildingTransform.position;
        pos.x += GeneralManager.singleton.buildingSensibility;

        buildingTransform.position = pos;
    }
    public void chengePerspective(Positioning positioning)
    {
        Player player = Player.localPlayer;
        if (!player) return;

        if (positioning.index == -1)
        {
            if (player.playerBuilding.actualBuilding.GetComponent<Building>().actualBuildinigRotation == 0)
            {
                Vector3 pos = player.playerBuilding.actualBuilding.transform.position;
                Destroy(player.playerBuilding.actualBuilding);
                GameObject g = Instantiate(player.playerBuilding.building.buildingList[1].buildingObject, pos, Quaternion.identity);
                player.playerBuilding.actualBuilding = g;
                return;
            }
            else
            {
                Vector3 pos = player.playerBuilding.actualBuilding.transform.position;
                Destroy(player.playerBuilding.actualBuilding);
                GameObject g = Instantiate(player.playerBuilding.building.buildingList[0].buildingObject, pos, Quaternion.identity);
                player.playerBuilding.actualBuilding = g;
            }
        }
        else
        {
            if (positioning.index == 0)
            {
                Vector3 pos = player.playerBuilding.actualBuilding.transform.position;
                Destroy(player.playerBuilding.actualBuilding);
                GameObject g = Instantiate(player.playerBuilding.building.buildingList[0].buildingObject, positioning.tPositioning.position, Quaternion.identity);
                player.playerBuilding.actualBuilding = g;
                return;
            }
            else
            {
                Vector3 pos = player.playerBuilding.actualBuilding.transform.position;
                Destroy(player.playerBuilding.actualBuilding);
                GameObject g = Instantiate(player.playerBuilding.building.buildingList[1].buildingObject, positioning.tPositioning.position, Quaternion.identity);
                player.playerBuilding.actualBuilding = g;
            }
        }

    }

    public void DestroyBuilding()
    {
        Player player = Player.localPlayer;

        Destroy(player.playerBuilding.actualBuilding);
        player.playerBuilding.actualBuilding = null;
        player.playerBuilding.building = null;
        player.playerBuilding.inventoryIndex = -1;
        Destroy(GeneralManager.singleton.spawnedBuildingObject);
    }

    public void SpawnBuilding()
    {
        Player player = Player.localPlayer;

        if (CanSpawn())
        {
            if ((((ScriptableBuilding)player.playerBuilding.building).groupWarehouse && player.InGuild()) || !(((ScriptableBuilding)player.playerBuilding.building).groupWarehouse))
            {
                player.playerBuilding.CmdSpawnBuilding(player.playerBuilding.inventoryIndex, player.playerBuilding.building.name, player.playerBuilding.actualBuilding.GetComponent<Building>().actualBuildinigRotation, new Vector2(player.playerBuilding.actualBuilding.transform.position.x, player.playerBuilding.actualBuilding.transform.position.y), player.playerBuilding.invBelt , player.playerBuilding.flagSelectedNation);
                DestroyBuilding();
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (!colliders.Contains(collider))
        {
            if((GeneralManager.singleton.buildingCheckSpawn.value & (1 << collider.gameObject.layer)) > 0)
                colliders.Add(collider);
        }
    }

    public void OnTriggerStay2D(Collider2D collider)
    {
        if (!colliders.Contains(collider))
        {
            if ((GeneralManager.singleton.buildingCheckSpawn.value & (1 << collider.gameObject.layer)) > 0)
                colliders.Add(collider);
        }
    }
    public void OnTriggerExit2D(Collider2D collider)
    {
        colliders.Remove(collider);
    }

    public void SetColor()
    {
        if (placement)
        {
            pColor = placement.GetComponent<SpriteRenderer>().color;
            //placement.GetComponent<SpriteRenderer>().sortingOrder = mainEntity.GetComponent<SpriteRenderer>().sortingOrder - 1;

            if (colliders.Count <= 0)
            {
                pColor = GeneralManager.singleton.canSpawn;
                placement.gameObject.GetComponent<SpriteRenderer>().color = pColor;
            }
            else
            {
                pColor = GeneralManager.singleton.notSpawn;
                placement.gameObject.GetComponent<SpriteRenderer>().color = pColor;
            }
        }
    }

    public bool CanSpawn()
    {
        return colliders.Count <= 0;
    }

    [Server]
    protected override string UpdateServer()
    {
        return "IDLE";
    }

    [Client]
    protected override void UpdateClient()
    {

    }
}
