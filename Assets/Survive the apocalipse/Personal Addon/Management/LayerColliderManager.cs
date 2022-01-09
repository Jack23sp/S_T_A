using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerColliderManager : MonoBehaviour
{
    public LayerMask layermask;

    public Collider2D[] colliders;

    public bool canSpawn = false;

    public Entity entity;


    public void Start()
    {
        entity = GetComponent<Entity>();
    }

    public void CheckObstacle()
    {
        if (entity)
        {
            colliders = Physics2D.OverlapBoxAll(transform.position, new Vector2(((BoxCollider2D)entity.collider).size.x, ((BoxCollider2D)entity.collider).size.y), 0, layermask);
        }
        else
        {
            colliders = Physics2D.OverlapBoxAll(transform.position, new Vector2(GetComponent<BoxCollider2D>().size.x, GetComponent<BoxCollider2D>().size.y), 0, layermask);
        }
    }
}
