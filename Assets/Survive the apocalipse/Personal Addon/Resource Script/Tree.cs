using UnityEngine;
using Mirror;

[RequireComponent(typeof(Animator))]
public partial class Tree : Entity
{
    public ScriptableItem tree;

    protected override void Awake()
    {
        base.Awake();
    }

    public void DisableTreeAnimation()
    {
        animator.SetBool("PLAY", false);
    }

    public override void OnStartServer()
    {
        // call Entity's OnStartServer
        base.OnStartServer();

        health = healthMax;
        mana = manaMax;
    }

    protected override void Start()
    {
        base.Start();
        treeObject = this;
    }

    [Server]
    protected override string UpdateServer()
    {
        return "IDLE";
    }

    // finite state machine - client ///////////////////////////////////////////
    [Client]
    protected override void UpdateClient()
    {
    }
}
