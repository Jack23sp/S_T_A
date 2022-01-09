using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class SpawnedBadge : NetworkBehaviour
{
    [SyncVar] public string playerName;

    public void Start()
    {
        Player onlinePlayer;
        if (Player.onlinePlayers.TryGetValue(playerName, out onlinePlayer))
        {
            transform.SetParent(onlinePlayer.transform);
            transform.localPosition = new Vector3(0.0f, 5.0f, 0.0f);
        }
    }
}
