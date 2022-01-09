using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using UnityEngine.UI;
using TMPro;

public class UITeleport : MonoBehaviour
{
    public static UITeleport singleton;
    public TextMeshProUGUI nameText;
    public Player player;
    public Button acceptButton;
    public Button declineButton;

    void Start()
    {
        if (!singleton) singleton = this;
    }

    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.health == 0)
            declineButton.onClick.Invoke();

        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
        {
            nameText.text = "Teleport to " + player.playerTeleport.inviterName + " ?\n Second remaining : " + player.playerTeleport.countdown;
        }
        else
        {
            nameText.text = "Trasportati da " + player.playerTeleport.inviterName + " ?\n Tempo rimanente : " + player.playerTeleport.countdown;
        }
        if (player.isLocalPlayer && player.playerTeleport.countdown == 0)
        {
            Destroy(this.gameObject);
        }
        acceptButton.onClick.SetListener(() =>
        {
            player.playerTeleport.CmdTeleportToFriends();
        });

        declineButton.onClick.SetListener(() =>
        {
            player.playerTeleport.CmdTeleportDecline();
        });

    }
}
