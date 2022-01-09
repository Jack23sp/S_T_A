using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using UnityEngine.UI;
using TMPro;

public class UITeleportInviter : MonoBehaviour
{
    public static UITeleport singleton;
    public TMP_InputField nameText;
    public Player player;
    public Button acceptButton;
    public Button declineButton;
    // Start is called before the first frame update
    void Start()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        acceptButton.onClick.SetListener(() =>
        {
            player.playerTeleport.CmdSendTeleportInvite(nameText.text);
            Destroy(this.gameObject);
        });
        declineButton.onClick.SetListener(() =>
        {
            player.playerTeleport.CmdRemoveTeleport();
            Destroy(this.gameObject);
        });

    }

    // Update is called once per frame
    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.health == 0) Destroy(this.gameObject);

        if(nameText.text != string.Empty)
        {
                acceptButton.interactable = true;
        }
        else
        {
            acceptButton.interactable = false;
        }

    }
}
