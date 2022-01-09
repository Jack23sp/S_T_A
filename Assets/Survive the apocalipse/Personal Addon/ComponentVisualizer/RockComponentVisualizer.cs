using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RockComponentVisualizer : ComponentVisualizer
{
    public NetworkIdentity identity;

    public Animator animator;
    public SortByDepth sortByDepth;
    public AudioSource audioSource;
    public Collider2D boxCollider;
    public Rigidbody2D rigidbody2D;
    public Rock rock;
    public SnowShaderVisualizer snowShaderVisualizer;
    public Entity entity;


    public bool isServer;

    public void Awake()
    {
        entity = GetComponent<Entity>();
        identity = GetComponent<NetworkIdentity>();
        isServer = identity.isServer;
        if (!rock) rock = GetComponent<Rock>();
        if (!sortByDepth) sortByDepth = GetComponent<SortByDepth>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!boxCollider) boxCollider = GetComponent<Collider2D>();
        if (!rigidbody2D) rigidbody2D = GetComponent<Rigidbody2D>();
        if (!snowShaderVisualizer) snowShaderVisualizer = GetComponent<SnowShaderVisualizer>();
    }

    public void VisibilityZero()
    {
        //if (!isServer) Destroy(this);

        if (identity.observers.Count == 0)
        {
            if (rock.enabled) rock.enabled = false;
            if (sortByDepth.enabled) sortByDepth.enabled = false;
            if (audioSource.enabled) audioSource.enabled = false;
            if (boxCollider.enabled) boxCollider.enabled = false;
            if (snowShaderVisualizer.enabled) snowShaderVisualizer.enabled = false;
            if (rigidbody2D) Destroy(rigidbody2D);
        }
    }

    public void VisibilityDifferntZero()
    {
        //if (!isServer) Destroy(this);

        if (identity.observers.Count > 0)
        {
            if (!rock.enabled) rock.enabled = true;
            if (!sortByDepth.enabled) sortByDepth.enabled = true;
            if (!audioSource.enabled) audioSource.enabled = true;
            if (!boxCollider.enabled) boxCollider.enabled = true;
            if (!rigidbody2D)
            {
                Rigidbody2D rigid2D = entity.gameObject.AddComponent(typeof(Rigidbody2D)) as Rigidbody2D;
                rigid2D.bodyType = RigidbodyType2D.Kinematic;
                rigid2D.simulated = true;
                rigidbody2D = rigid2D;
            }
            if (!snowShaderVisualizer.enabled) snowShaderVisualizer.enabled = true;
        }
    }
}
