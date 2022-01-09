using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen singleton;
    public Animation animation;
    public bool canTeleport;

    public void Start()
    {
        if (!singleton) singleton = this;
    }

    public void Update()
    {
        transform.SetAsLastSibling();
    }
    public void CallTeleport()
    {
        SpawnManagerList.singleton.CheckPlayerInsideNormalMapZone(Player.localPlayer);

        if (Player.localPlayer)
            Player.localPlayer.playerPremiumZoneManager.CmdMoveToPremiumZone(GeneralManager.singleton.premiumZoneSpawn.position);
    }

    public void AbleTeleport()
    {
        canTeleport = true;
    }

    public void DisableTeleport()
    {
        canTeleport = false;
    }
}
