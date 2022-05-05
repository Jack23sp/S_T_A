using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingComponentVisualizer : ComponentVisualizer
{
    public NetworkIdentity identity;

    [HideInInspector] public Animator animator;
    public SortByDepth sortByDepth;
    public AudioSource audioSource;
    public Collider2D boxCollider;
    public Rigidbody2D rigidbody2D;
    public Building building;
    public Entity entity;
    public GameObject teslaDamageObject;
    public Flag flag;
    public NavMeshObstacle2D navMeshObstacle2D;
    public SnowShaderVisualizer snowShaderVisualizer;
    private StreetLamp streetLamp;
    public bool hasAnimator = false;


    public bool isServer;

    public void Awake()
    {
        entity = GetComponent<Entity>();
        identity = GetComponent<NetworkIdentity>();
        isServer = identity.isServer;
        streetLamp = GetComponent<StreetLamp>();
        if (!building) building = GetComponent<Building>();
        if (!sortByDepth) sortByDepth = GetComponent<SortByDepth>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!boxCollider) boxCollider = GetComponent<Collider2D>();
        if (!rigidbody2D) rigidbody2D = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponent<Animator>();
        if (!navMeshObstacle2D) navMeshObstacle2D = GetComponent<NavMeshObstacle2D>();
        if (!flag) flag = GetComponent<Flag>();
    }


    public void VisibilityZero()
    {
        //if (!isServer) Destroy(this);

        if (identity.observers.Count == 0)
        {
            if (building && building.enabled) building.enabled = false;
            if (sortByDepth && sortByDepth.enabled) sortByDepth.enabled = false;
            if (audioSource && audioSource.enabled) audioSource.enabled = false;
            if (boxCollider && boxCollider.enabled) boxCollider.enabled = false;
            if (rigidbody2D) Destroy(rigidbody2D);
            if (hasAnimator && animator && animator.enabled) animator.enabled = false;
            if (navMeshObstacle2D && navMeshObstacle2D.enabled)
            {
                if(navMeshObstacle2D.go) navMeshObstacle2D.go.SetActive(false);
                navMeshObstacle2D.enabled = false;
            }
            if(((building.isServer && building.isClient) || building.isServer) && teslaDamageObject) teslaDamageObject.SetActive(false);
            if (streetLamp) streetLamp.enabled = false;
            if (flag) flag.enabled = false;
        }
    }

    public void VisibilityDifferntZero()
    {
        //if (!isServer) Destroy(this);
        if (identity.observers.Count > 0)
        {
            if (building && !building.enabled) building.enabled = true;
            if (sortByDepth && !sortByDepth.enabled) sortByDepth.enabled = true;
            if (audioSource && !audioSource.enabled) audioSource.enabled = true;
            if (boxCollider && !boxCollider.enabled) boxCollider.enabled = true;
            if (hasAnimator && animator && !animator.enabled) animator.enabled = true;
            if (!rigidbody2D)
            {
                Rigidbody2D rigid2D = entity.gameObject.AddComponent(typeof(Rigidbody2D)) as Rigidbody2D;
                rigid2D.bodyType = RigidbodyType2D.Kinematic;
                rigid2D.simulated = true;
                rigidbody2D = rigid2D;
            }
            if (navMeshObstacle2D && !navMeshObstacle2D.enabled)
            {
                if (navMeshObstacle2D.go) navMeshObstacle2D.go.SetActive(true);
                navMeshObstacle2D.enabled = true;
            }
            if (streetLamp) streetLamp.enabled = true;
            if (flag) flag.enabled = true;
        }
    }
}
