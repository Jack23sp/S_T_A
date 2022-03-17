using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasementFloorDetector : MonoBehaviour
{
    public Collider2D floorColliderDetector;
    public Player player;

    public Collider2D[] floorColliders;

    public void Awake()
    {
        floorColliderDetector = GetComponent<Collider2D>();
    }

    public void Update()
    {
        if (player.isClient)
        {
            if (player.agent.velocity != Vector2.zero)
            {
                if (Player.localPlayer && player.name == Player.localPlayer.name)
                {
                    floorColliders = Physics2D.OverlapBoxAll(transform.position, new Vector2(((BoxCollider2D)floorColliderDetector).size.x, ((BoxCollider2D)floorColliderDetector).size.y), 0, GeneralManager.singleton.modularObjectLayerMask);

                    if (floorColliders.Length > 0)
                    {
                            ModularBuildingManager.singleton.DisableRoof();
                    }
                    else
                    {
                        if(ModularBuildingManager.singleton.inThisCollider)
                            ModularBuildingManager.singleton.AbleRoof();
                        ModularBuildingManager.singleton.inThisCollider = null;
                    }
                }
                else
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }
}
