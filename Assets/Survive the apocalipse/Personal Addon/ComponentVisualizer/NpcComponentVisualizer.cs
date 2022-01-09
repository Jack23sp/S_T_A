using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NpcComponentVisualizer : ComponentVisualizer
{
    public NetworkIdentity identity;

    //public Animator animator;
    public SortByDepth sortByDepth;
    public AudioSource audioSource;
    public Collider2D circleCollider;
    public Rigidbody2D rigidbody2D;
    public Npc npc;
    public Entity entity;

    public bool isServer;

    public void Awake()
    {
        entity = GetComponent<Entity>();
        identity = GetComponent<NetworkIdentity>();
        isServer = identity.isServer;
        if (!npc) npc = GetComponent<Npc>();
        if (!sortByDepth) sortByDepth = GetComponent<SortByDepth>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!circleCollider) circleCollider = GetComponent<Collider2D>();
        if (!rigidbody2D) rigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    public void VisibilityZero()
    {
        //if (!isServer) Destroy(this);

        if (identity.observers.Count == 0)
        {
            //if (tree.enabled) tree.enabled = false;
            //if (sortByDepth.enabled) sortByDepth.enabled = false;
            //if (audioSource.enabled) audioSource.enabled = false;
            //if (boxCollider.enabled) boxCollider.enabled = false;

            npc.enabled = false;
            sortByDepth.enabled = false;
            audioSource.enabled = false;
            circleCollider.enabled = false;
            if (rigidbody2D) Destroy(rigidbody2D);
        }
    }

    public void VisibilityDifferntZero()
    {
        //if (!isServer) Destroy(this);

        if (identity.observers.Count > 0)
        {
            //if (!tree.enabled) tree.enabled = true;
            //if (!sortByDepth.enabled) sortByDepth.enabled = true;
            //if (!audioSource.enabled) audioSource.enabled = true;
            //if (!boxCollider.enabled) boxCollider.enabled = true;

            npc.enabled = true;
            sortByDepth.enabled = true;
            audioSource.enabled = true;
            circleCollider.enabled = true;
            if (!rigidbody2D)
            { 
                Rigidbody2D rigid2D = entity.gameObject.AddComponent(typeof(Rigidbody2D)) as Rigidbody2D;
                rigid2D.bodyType = RigidbodyType2D.Kinematic;
                rigid2D.simulated = true;
                rigidbody2D = rigid2D;
            }
        }
    }
}
