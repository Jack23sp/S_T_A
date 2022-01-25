using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using CustomType;
using System.Linq;

public enum ModularType
{
    furnace,
    medicalTable,
    warehouse,
    groupWarehouse,
    waterWell,
    stove,
    fridge,
    sewingTable,
    upgradeTable,
    airConditioner,
    armChair,
    bathroomSink,
    bed,
    bedsideTable,
    cabinet,
    kitchenSink,
    chair,
    table,
    paint,
    shower,
    sofa,
    trashCan,
    tv
}

public class ModularObject : Forniture
{
    public ModularType fornitureType;

    public SpriteRenderer placement;
    public BoxCollider2D collider;

    public List<BoxCollider2D> allColliders;
    public List<BoxCollider2D> wallColliders;
    public List<BoxCollider2D> obstacleColliders;


    public NetworkIdentity identity;

    public bool canSpawn;

    public bool needALayerUnder;
    public bool isWallObject;

    public NavMeshObstacle2D navMeshObstacle2D;

    public int oldPositioning = 0;

    public void OnDisable()
    {
        CancelInvoke();
    }

    public void Start()
    {
        Invoke(nameof(ManagePlacement), 0.5f);
    }

    public void ManagePlacement()
    {
        if (!identity.isClient && !identity.isServer)
        {
            placement.enabled = true;
        }
        else
            if (navMeshObstacle2D) navMeshObstacle2D.enabled = true;
    }

    public void Update()
    {
        if (identity.isClient || identity.isServer)
        {
            if (placement) Destroy(placement.gameObject);
        }
        else
        {
            if (placement) placement.color = canSpawn ? GeneralManager.singleton.canSpawn : GeneralManager.singleton.notSpawn;
        }
    }

    public void OnDestroy()
    {
        CancelInvoke();
    }

    public void DestroyBuilding()
    {
        Player player = Player.localPlayer;

        if (player.playerBuilding.actualBuilding) Destroy(player.playerBuilding.actualBuilding);
        player.playerBuilding.actualBuilding = null;
        player.playerBuilding.building = null;
        player.playerBuilding.inventoryIndex = -1;
        if (GeneralManager.singleton.spawnedBuildingObject) Destroy(GeneralManager.singleton.spawnedBuildingObject);
    }

    public void Up()
    {
        Vector3 pos = transform.position;
        pos.y += GeneralManager.singleton.buildingSensibility;
        transform.position = pos;
    }

    public void Left()
    {
        Vector3 pos = transform.position;
        pos.x -= GeneralManager.singleton.buildingSensibility;
        transform.position = pos;
    }

    public void Down()
    {
        Vector3 pos = transform.position;
        pos.y -= GeneralManager.singleton.buildingSensibility;
        transform.position = pos;
    }

    public void Right()
    {
        Vector3 pos = transform.position;
        pos.x += GeneralManager.singleton.buildingSensibility;
        transform.position = pos;
    }

    public void chengePerspective()
    {
        oldPositioning++;
        if (oldPositioning > (Player.localPlayer.playerBuilding.building.buildingList.Count -1) ) oldPositioning = 0;

        Vector3 pos = Player.localPlayer.playerBuilding.actualBuilding.transform.position;
        Destroy(Player.localPlayer.playerBuilding.actualBuilding);
        GameObject g = Instantiate(Player.localPlayer.playerBuilding.building.buildingList[oldPositioning].buildingObject, pos, Quaternion.identity);
        Player.localPlayer.playerBuilding.actualBuilding = g;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isClient && !isServer)
        {
            if (needALayerUnder)
            {
                if ((GeneralManager.singleton.modularObjectNeedBaseLayerMask.value & (1 << collision.gameObject.layer)) > 0)
                {
                    if (!allColliders.Contains(((BoxCollider2D)collision)))
                    {
                        allColliders.Add(((BoxCollider2D)collision));
                    }
                }
                if (allColliders.Count > 1) { canSpawn = false; return; }
                else
                {
                    if (allColliders.Count == 1)
                    {
                        if (GeneralManager.singleton.IsInsideGeneric(allColliders[0], collider))
                        {
                            canSpawn = false; return;
                        }
                    }

                    canSpawn = false; return;
                }

            }
            else if (isWallObject)
            {
                if ((GeneralManager.singleton.modularObjectWallLayerMask.value & (1 << collision.gameObject.layer)) > 0)
                {
                    if (!allColliders.Contains(((BoxCollider2D)collision)))
                    {
                        allColliders.Add(((BoxCollider2D)collision));
                    }
                }

                if ((GeneralManager.singleton.modularObjecObstacleLayerMask.value & (1 << collision.gameObject.layer)) > 0)
                {
                    if (!obstacleColliders.Contains(((BoxCollider2D)collision)))
                    {
                        obstacleColliders.Add(((BoxCollider2D)collision));
                    }
                }

                if (obstacleColliders.Count > 0) { canSpawn = false; return; }
                else
                {
                    if (allColliders.Count > 1) { canSpawn = false; return; }
                    else
                    {
                        if (allColliders.Count == 1)
                        {
                            if (GeneralManager.singleton.IsInsideGeneric(allColliders[0], collider))
                            {
                                canSpawn = true; return;
                            }
                        }
                        canSpawn = false; return;
                    }
                }
            }
            else
            {
                if ((GeneralManager.singleton.modularObjectLayerMask.value & (1 << collision.gameObject.layer)) > 0)
                {
                    if (!allColliders.Contains(((BoxCollider2D)collision)))
                    {
                        allColliders.Add(((BoxCollider2D)collision));
                    }
                }

                if ((GeneralManager.singleton.modularObjecObstaclePlacementLayerMask.value & (1 << collision.gameObject.layer)) > 0)
                {
                    if (!obstacleColliders.Contains(((BoxCollider2D)collision)))
                    {
                        obstacleColliders.Add(((BoxCollider2D)collision));
                    }
                }

                List<BoxCollider2D> listCollider = obstacleColliders.ToList();
                for (int i = 0; i < listCollider.Count; i++)
                {
                    int index = i;
                    if (listCollider[index] == collider)
                    {
                        listCollider.Remove(listCollider[index]);
                    }
                }

                obstacleColliders = listCollider;

                if (obstacleColliders.Count > 0) { canSpawn = false; return; }
                else
                {
                    if (allColliders.Count > 1) { canSpawn = false; return; }
                    else
                    {
                        if (allColliders.Count == 1)
                        {
                            if (GeneralManager.singleton.IsInside(allColliders[0], collider))
                            {
                                canSpawn = true; 
                                return;
                            }
                        }
                        canSpawn = false; return;
                    }
                }
            }
        }
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (!isClient && !isServer)
        {
            if (needALayerUnder)
            {
                if ((GeneralManager.singleton.modularObjectNeedBaseLayerMask.value & (1 << collision.gameObject.layer)) > 0)
                {
                    if (!allColliders.Contains(((BoxCollider2D)collision)))
                    {
                        allColliders.Add(((BoxCollider2D)collision));
                    }
                }
                if (allColliders.Count > 1) { canSpawn = false; return; }
                else
                {
                    if (allColliders.Count == 1)
                    {
                        if (GeneralManager.singleton.IsInsideGeneric(allColliders[0], collider))
                        {
                            canSpawn = false; return;
                        }
                    }

                    canSpawn = false; return;
                }

            }
            else if (isWallObject)
            {
                if ((GeneralManager.singleton.modularObjectWallLayerMask.value & (1 << collision.gameObject.layer)) > 0)
                {
                    if (!allColliders.Contains(((BoxCollider2D)collision)))
                    {
                        allColliders.Add(((BoxCollider2D)collision));
                    }
                }

                if ((GeneralManager.singleton.modularObjecObstacleLayerMask.value & (1 << collision.gameObject.layer)) > 0)
                {
                    if (!obstacleColliders.Contains(((BoxCollider2D)collision)))
                    {
                        obstacleColliders.Add(((BoxCollider2D)collision));
                    }
                }

                if (obstacleColliders.Count > 0) { canSpawn = false; return; }
                else
                {
                    if (allColliders.Count > 1) { canSpawn = false; return; }
                    else
                    {
                        if (allColliders.Count == 1)
                        {
                            if (GeneralManager.singleton.IsInsideGeneric(allColliders[0], collider))
                            {
                                canSpawn = true; return;
                            }
                        }
                        canSpawn = false; return;
                    }
                }
            }
            else
            {
                if ((GeneralManager.singleton.modularObjectLayerMask.value & (1 << collision.gameObject.layer)) > 0)
                {
                    if (!allColliders.Contains(((BoxCollider2D)collision)))
                    {
                        allColliders.Add(((BoxCollider2D)collision));
                    }
                }

                if ((GeneralManager.singleton.modularObjecObstaclePlacementLayerMask.value & (1 << collision.gameObject.layer)) > 0)
                {
                    if (!obstacleColliders.Contains(((BoxCollider2D)collision)))
                    {
                        obstacleColliders.Add(((BoxCollider2D)collision));
                    }
                }

                List<BoxCollider2D> listCollider = obstacleColliders.ToList();
                for (int i = 0; i < listCollider.Count; i++)
                {
                    int index = i;
                    if (listCollider[index] == collider)
                    {
                        listCollider.Remove(listCollider[index]);
                    }
                }

                obstacleColliders = listCollider;

                if (obstacleColliders.Count > 0) { canSpawn = false; return; }
                else
                {
                    if (allColliders.Count > 1) { canSpawn = false; return; }
                    else
                    {
                        if (allColliders.Count == 1)
                        {
                            if (GeneralManager.singleton.IsInside(allColliders[0], collider))
                            {
                                canSpawn = true;
                                return;
                            }
                        }
                        canSpawn = false; return;
                    }
                }
            }
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (allColliders.Contains(((BoxCollider2D)collision)))
        {
            allColliders.Remove(((BoxCollider2D)collision));
        }

        if (obstacleColliders.Contains(((BoxCollider2D)collision)))
        {
            obstacleColliders.Remove(((BoxCollider2D)collision));
        }
    }
}
