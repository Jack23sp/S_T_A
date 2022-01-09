using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class MonsterComponentVisualizer : ComponentVisualizer
{
    public NetworkIdentity identity;

    public Animator animator;
    public SortByDepth sortByDepth;
    public AudioSource audioSource;
    public Collider2D boxCollider;
    public Rigidbody2D rigidbody2D;
    public Monster monster;
    public NavMeshAgent2D navMeshAgent2D;
    public NetworkNavMeshAgent2D networkNav;
    public EntityObstacleCheck entityObstacleCheck;
    public Entity entity;


    public bool isServer;

    public void Awake()
    {
        entity = GetComponent<Entity>();
        identity = GetComponent<NetworkIdentity>();
        isServer = identity.isServer;
        if (!monster) monster = GetComponent<Monster>();
        if (!animator) animator = GetComponent<Animator>();
        if (!sortByDepth) sortByDepth = GetComponent<SortByDepth>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!boxCollider) boxCollider = GetComponent<Collider2D>();
        if (!navMeshAgent2D) navMeshAgent2D = GetComponent<NavMeshAgent2D>();
        if (!networkNav) networkNav = GetComponent<NetworkNavMeshAgent2D>();
        if (!entityObstacleCheck) entityObstacleCheck = GetComponent<EntityObstacleCheck>();
        if (!rigidbody2D) rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void VisibilityZero()
    {
        //if (!isServer) Destroy(this);

        if (identity.observers.Count == 0)
        {
            if (monster.enabled) monster.enabled = false;
            if (animator.enabled) animator.enabled = false;
            if (sortByDepth.enabled) sortByDepth.enabled = false;
            if (audioSource.enabled) audioSource.enabled = false;
            if (boxCollider.enabled) boxCollider.enabled = false;
            if (navMeshAgent2D.enabled) navMeshAgent2D.enabled = false;
            if (networkNav.enabled) networkNav.enabled = false;
            if (entityObstacleCheck.enabled) entityObstacleCheck.enabled = false;
            if (rigidbody2D) Destroy(rigidbody2D);
            if (navMeshAgent2D.go.activeInHierarchy) navMeshAgent2D.go.SetActive(false);
        }
    }

    public void VisibilityDifferntZero()
    {
        //if (!isServer) Destroy(this);

        if (identity.observers.Count > 0)
        {
            if (!monster.enabled) monster.enabled = true;
            if (!animator.enabled) animator.enabled = true;
            if (!sortByDepth.enabled) sortByDepth.enabled = true;
            if (!audioSource.enabled) audioSource.enabled = true;
            if (!boxCollider.enabled) boxCollider.enabled = true;
            if (!navMeshAgent2D.enabled) navMeshAgent2D.enabled = true;
            if (!networkNav.enabled) networkNav.enabled = true;
            if (!entityObstacleCheck.enabled) entityObstacleCheck.enabled = true;
            if (!rigidbody2D)
            {
                Rigidbody2D rigid2D = entity.gameObject.AddComponent(typeof(Rigidbody2D)) as Rigidbody2D;
                rigid2D.bodyType = RigidbodyType2D.Kinematic;
                rigid2D.simulated = true;
                rigidbody2D = rigid2D;
            }
            navMeshAgent2D.go.SetActive(true);
            //navMeshAgent2D.InstantiateNavMesh();
        }
    }
}
