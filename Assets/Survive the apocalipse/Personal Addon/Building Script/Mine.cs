using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class Mine : NetworkBehaviour
{
    public List<Collider2D> colliders = new List<Collider2D>();
    public Collider2D[] nearColliders;
    public NetworkIdentity mainID;
    public Entity mineEntity;
    public Building building;

    public Player player;

    public SpriteRenderer mainRenderer;

    [SyncVar]
    public bool activated;
    public List<GameObject> objectToManage = new List<GameObject>();

    public bool alreadyExplode = false;

    public bool prevActivation;

    public Sprite mineImage;

    public bool isThisAServer;
    public bool canManage;

    [SyncVar]
    public int numberOfObserver = 0;

    // Start is called before the first frame update
    void Start()
    {
        mainRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!player) player = Player.localPlayer;

        if (activated != prevActivation)
        {
            building.animator.SetBool("Active", activated);
            prevActivation = activated;
        }

        //if (mineEntity.health == 0) Destroy(this.gameObject);
        if (mainID.netId == 0) return;

        isThisAServer = mainID.isServer;

        if (mainID.isServer)
        {
            numberOfObserver = mainID.observers.Count;

            if (colliders.Count > 0)
            {
                if (activated)
                {
                    GameObject g = Instantiate(GeneralManager.singleton.explosionPrefab, transform.position, Quaternion.identity);
                    NetworkServer.Spawn(g);
                    colliders.Clear();
                    nearColliders = Physics2D.OverlapCircleAll(transform.position, GeneralManager.singleton.nearEntityExplosionRange, GeneralManager.singleton.affectedByMineExplosion);
                    for (int i = 0; i < nearColliders.Length; i++)
                    {
                        int index = i;
                        Entity entity = nearColliders[index].GetComponent<Entity>();


                        if (entity && entity != mineEntity && entity.health > 0 && entity.GetComponent<Mine>())
                        {
                            entity.GetComponent<Mine>().Explode();
                        }
                        if (entity && entity != mineEntity && entity.health > 0 && entity.GetComponent<Dynamite>())
                        {
                            entity.GetComponent<Dynamite>().Explode();
                        }

                        if (entity && entity != mineEntity && entity.health > 0 && entity.GetComponent<Player>())
                        {
                            if (!((Player)entity).playerMonsterGrab.shakeCamera)
                            {
                                ((Player)entity).playerMonsterGrab.shakeCamera = true;
                            }
                        }

                        if (entity)
                        {
                            if (entity is Player)
                            {
                                if (GeneralManager.singleton.MineCanDamagePlayer(building, ((Player)entity)))
                                {
                                    ((Player)entity).ManageDamageArmorExplosionHealth(GeneralManager.singleton.mineDamage.Get(entity.level));
                                }
                            }
                            else if (entity is Building)
                            {
                                if (GeneralManager.singleton.MineCanDamageBuilding(building, ((Building)entity)))
                                {
                                    entity.health -= GeneralManager.singleton.mineDamage.Get(entity.level);
                                }
                            }
                            else if (entity is Monster)
                            {
                                entity.health -= GeneralManager.singleton.mineDamage.Get(mineEntity.level);
                            }
                        }
                        if (entity && entity.target == mineEntity) entity.target = null;
                    }
                    nearColliders = new Collider2D[0];
                    mineEntity.health = 0;
                    BuildingManager.singleton.RemoveFromList(this.gameObject);
                    NetworkServer.Destroy(this.gameObject);

                }
            }
        }
        if (numberOfObserver > 0)
        {
            if (!mineEntity.isServer && !mineEntity.isClient)
            {
                GeneralManager.singleton.ShowBuilding(mainRenderer, building.textMesh.gameObject, objectToManage);
            }
            else if (mainID.isClient)
            {
                if (player)
                {
                    if (GeneralManager.singleton.CanManageExplosiveBuilding(building, player))
                    {
                        canManage = true;
                        GeneralManager.singleton.ShowBuilding(mainRenderer, building.textMesh.gameObject, objectToManage);
                    }
                    else
                    {
                        canManage = false;
                        GeneralManager.singleton.HideBuilding(mainRenderer, building.textMesh.gameObject, objectToManage);
                    }
                }
            }
        }
    }


    public void Explode()
    {
        if (mainID.isServer)
        {
            List<Entity> nearEntity = new List<Entity>();
            GameObject g = Instantiate(GeneralManager.singleton.explosionPrefab, transform.position, Quaternion.identity);
            NetworkServer.Spawn(g);
            colliders.Clear();
            nearColliders = Physics2D.OverlapCircleAll(transform.position, GeneralManager.singleton.nearEntityExplosionRange, GeneralManager.singleton.affectedByMineExplosion);

            for (int i = 0; i < nearColliders.Length; i++)
            {
                int index = i;
                Entity entity = nearColliders[index].GetComponent<Entity>();

                if (entity && entity != mineEntity && entity.health > 0 && entity.GetComponent<Mine>())
                {
                    GeneralManager.singleton.objectToExplode.Add((entity));
                }
                if (entity && entity != mineEntity && entity.health > 0 && entity.GetComponent<Dynamite>())
                {
                    GeneralManager.singleton.objectToExplode.Add((entity));
                }

                if (entity && entity != mineEntity && entity.health > 0 && entity.GetComponent<Player>())
                {
                    if (!((Player)entity).playerMonsterGrab.shakeCamera)
                    {
                        ((Player)entity).playerMonsterGrab.shakeCamera = true;
                    }
                }

                if (entity)
                {
                    if (entity is Player)
                    {
                        if (GeneralManager.singleton.MineCanDamagePlayer(building, ((Player)entity)))
                        {
                            ((Player)entity).ManageDamageArmorExplosionHealth(GeneralManager.singleton.mineDamage.Get(entity.level));
                        }
                    }
                    else if (entity is Building)
                    {
                        if (GeneralManager.singleton.MineCanDamageBuilding(building, ((Building)entity)))
                        {
                            entity.health -= GeneralManager.singleton.mineDamage.Get(entity.level);
                        }
                    }
                    else if (entity is Monster)
                    {
                        entity.health -= GeneralManager.singleton.mineDamage.Get(mineEntity.level);
                    }
                }

                if (entity && entity.target == mineEntity) entity.target = null;
            }
            colliders.Clear();
            mineEntity.health = 0;
            BuildingManager.singleton.RemoveFromList(this.gameObject);
            NetworkServer.Destroy(this.gameObject);
        }
    }

    public void OnTriggerStay2D(Collider2D col)
    {
        if (mainID.isServer && activated && mineEntity.health > 0)
        {
            if (col.GetComponent<Entity>() is Player)
            {
                // if is not ally
                if (!GeneralManager.singleton.CanManageExplosiveBuilding(building, col.GetComponent<Player>()))
                {
                    if (!colliders.Contains(col)) colliders.Add(col);
                }
            }

            if (col.GetComponent<Entity>() is Monster)
            {
                if (!colliders.Contains(col)) colliders.Add(col);
            }
        }
    }

    public void OnTriggerExit2D(Collider2D col)
    {
        if (colliders.Contains(col)) colliders.Remove(col);
    }
}
