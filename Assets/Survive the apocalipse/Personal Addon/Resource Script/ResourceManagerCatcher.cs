using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class ResourceManagerCatcher : MonoBehaviour
{
    public Player player;
    public Collider2D catchCollider;

    public LayerMask nearEntityLayer;
    public Collider2D[] nearEntity;



    public void Start()
    {
        catchCollider.enabled = true;
        if (player.isClient)
            InvokeRepeating(nameof(CheckNearEntity), 1.0f, 1.0f);
    }

    public void CheckNearEntity()
    {
        player.playerMove.nearEntity = Physics2D.OverlapCircleAll(transform.position, 15, nearEntityLayer);
        player.playerMove.nearEntity = player.playerMove.nearEntity.Where(item => item != null).ToArray();
    }

}
