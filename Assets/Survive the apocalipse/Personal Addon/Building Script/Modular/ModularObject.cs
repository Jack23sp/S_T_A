using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using CustomType;
using System.Linq;

public class ModularObject : NetworkBehaviour
{
    public SpriteRenderer placement;
    public BoxCollider2D collider;

    public List<BoxCollider2D> allColliders;
    public List<BoxCollider2D> wallColliders;
    public List<BoxCollider2D> obstacleColliders;
    public ScriptableBuilding scriptableBuilding;

    public NetworkIdentity identity;

    public bool canSpawn;

    public bool needALayerUnder;
    public bool isWallObject;

    public NavMeshObstacle2D navMeshObstacle2D;

    public void Start()
    {
        if (!isClient || !isServer)
        {
            placement.enabled = true;
            InvokeRepeating(nameof(CheckPlacement), 0.5f, 1.0f);
        }
        else
            if (navMeshObstacle2D) navMeshObstacle2D.enabled = true;
    }

    public void CheckPlacement()
    {
        if (isClient || isServer)
        {
            Destroy(placement.gameObject);
            CancelInvoke(nameof(CheckPlacement));
        }
        else
        {
            placement.color = canSpawn ? GeneralManager.singleton.canSpawn : GeneralManager.singleton.notSpawn;
        }
    }

    public void OnDestroy()
    {
        CancelInvoke(nameof(CheckPlacement));
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
    public void chengePerspective(Positioning positioning)
    {

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
