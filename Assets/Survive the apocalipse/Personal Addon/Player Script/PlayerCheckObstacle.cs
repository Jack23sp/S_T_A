using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCheckObstacle : MonoBehaviour
{
    public Player player;

    public LayerMask layer;

    public Collider2D[] nearEntity;

    public NavMeshObstacleSimulator obstacleSimulator;

    public void FixedUpdate()
    {
        if (!player)
        {
            Destroy(this.gameObject);
            return;
        }
        nearEntity = Physics2D.OverlapCircleAll(transform.position, transform.localScale.x, layer);

        if (!obstacleSimulator) obstacleSimulator = player.GetComponent<NavMeshObstacleSimulator>();

        if(obstacleSimulator) obstacleSimulator.canMove = (nearEntity.Length == 0);
    }
}
