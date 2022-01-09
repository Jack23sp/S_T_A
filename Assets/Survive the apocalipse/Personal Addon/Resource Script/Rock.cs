using UnityEngine;
using Mirror;

[RequireComponent(typeof(Animator))]
public partial class Rock : Entity
{
    public ScriptableItem rock;

    protected override void Awake()
    {
        base.Awake();
    }

    public void DisableRockAnimation()
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
        rockObject = this;
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
