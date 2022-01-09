using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class Dynamite : NetworkBehaviour
{
    public Collider2D[] nearColliders;
    public NetworkIdentity mainID;
    public Entity mineEntity;
    public Building building;

    public Player player;

    public SpriteRenderer mainRenderer;

    public List<GameObject> objectToManage = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        mainRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (mainID.netId == 0) return;
        if (mainID.observers.Count > 0)
        {
            if (!mineEntity.isServer && !mineEntity.isClient)
            {
                GeneralManager.singleton.ShowBuilding(mainRenderer, building.textMesh.gameObject, objectToManage);
            }
            else
            {
                if (player)
                {
                    if (GeneralManager.singleton.CanManageExplosiveBuilding(building, player))
                    {
                        GeneralManager.singleton.ShowBuilding(mainRenderer, building.textMesh.gameObject, objectToManage);
                    }
                    else
                    {
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
            GameObject g = Instantiate(GeneralManager.singleton.explosionPrefab, transform.position, Quaternion.identity);
            NetworkServer.Spawn(g);
            nearColliders = Physics2D.OverlapCircleAll(transform.position, GeneralManager.singleton.nearEntityExplosionRange, GeneralManager.singleton.affectedByMineExplosion);
            for (int i = 0; i < nearColliders.Length; i++)
            {
                int index = i;
                Entity entity = nearColliders[index].GetComponent<Entity>();

                if (entity == ((Entity)player))
                {
                    CustomCameraShake.singleton.animator.SetBool("SHAKE", true);
                }

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
            nearColliders = new Collider2D[0];
            mineEntity.health = 0;
            BuildingManager.singleton.RemoveFromList(this.gameObject);
            NetworkServer.Destroy(this.gameObject);

        }
    }


}
