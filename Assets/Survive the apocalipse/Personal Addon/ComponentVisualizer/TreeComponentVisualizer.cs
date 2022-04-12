using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TreeComponentVisualizer : ComponentVisualizer
{
    public NetworkIdentity identity;

    //public Animator animator;
    //public SortByDepth sortByDepth;
    public AudioSource audioSource;
    public Collider2D boxCollider;
    public Rigidbody2D rigidbody2D;
    public Tree tree;
    public SnowShaderVisualizer snowShaderVisualizer;
    public NavMeshObstacle2D navMeshObstacle2D;
    public Entity entity;

    public int snowShaderAmount;
    public SnowManager snowManager;

    public bool isServer;

    public void Awake()
    {
        entity = GetComponent<Entity>();
        identity = GetComponent<NetworkIdentity>();
        isServer = identity.isServer;
        if (!tree) tree = GetComponent<Tree>();
        //if (!sortByDepth) sortByDepth = GetComponent<SortByDepth>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!boxCollider) boxCollider = GetComponent<Collider2D>();
        if (!rigidbody2D) rigidbody2D = GetComponent<Rigidbody2D>();
        if (!snowManager) snowManager = FindObjectOfType<SnowManager>();
        if (!snowShaderVisualizer) snowShaderVisualizer = GetComponent<SnowShaderVisualizer>();
        if (!navMeshObstacle2D) navMeshObstacle2D = GetComponent<NavMeshObstacle2D>();

    }

    // Update is called once per frame
    public void VisibilityZero()
    {
            if (tree.enabled) tree.enabled = false;
            //if (sortByDepth.enabled) sortByDepth.enabled = false;
            //if (audioSource.enabled) audioSource.enabled = false;
            if (boxCollider.enabled) boxCollider.enabled = false;

            tree.enabled = false;
            //sortByDepth.enabled = false;
            audioSource.enabled = false;
            boxCollider.enabled = false;
            snowShaderVisualizer.enabled = false;
            if (rigidbody2D) Destroy(rigidbody2D);
            if (navMeshObstacle2D)
            {
                navMeshObstacle2D.go.SetActive(false);
                navMeshObstacle2D.enabled = false;
            }
    }

    public void VisibilityDifferntZero()
    {
            if (!tree.enabled) tree.enabled = true;
            //if (!sortByDepth.enabled) sortByDepth.enabled = true;
            //if (!audioSource.enabled) audioSource.enabled = true;
            if (!boxCollider.enabled) boxCollider.enabled = true;

            tree.enabled = true;
            //sortByDepth.enabled = true;
            audioSource.enabled = true;
            boxCollider.enabled = true;
            if (!rigidbody2D)
            { 
                Rigidbody2D rigid2D = entity.gameObject.AddComponent(typeof(Rigidbody2D)) as Rigidbody2D;
                rigid2D.bodyType = RigidbodyType2D.Kinematic;
                rigid2D.simulated = true;
                rigidbody2D = rigid2D;
            }
            snowShaderVisualizer.enabled = true;
            if (navMeshObstacle2D && !navMeshObstacle2D.enabled)
            {
                navMeshObstacle2D.go.SetActive(true);
                navMeshObstacle2D.enabled = true;
            }
    }
}
