using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class UIPartnerInvite : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public GameObject panel;
    public Player player;

    public Button AcceptButton;
    public Button DeclineButton;

    void Update()
    {
        player = Player.localPlayer;

        if (player)
        {
            if (player.health == 0)
                panel.SetActive(false);

            if (player != null && player.playerMarriage.inviter != "")
            {
                panel.SetActive(true);
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    nameText.text = player.playerMarriage.inviter + " vuole essere il tuo compagno. \nVuoi accettare? ";
                }
                else
                {
                    nameText.text = player.playerMarriage.inviter + " want to be your partner. \nDo you want accept? ";
                }
                AcceptButton.onClick.SetListener(() =>
                {
                    AcceptGuildIvite();
                });

                DeclineButton.onClick.SetListener(() =>
                {
                    DeclineGuildIvite();
                });

                transform.SetAsLastSibling();
            }
            else
            {
                panel.SetActive(false);
            }
        }
    }

    public void AcceptGuildIvite()
    {
        player.playerMarriage.CmdAcceptInvitePartner();
    }

    public void DeclineGuildIvite()
    {
        player.playerMarriage.CmdDeclineInvitePartner();
    }
}
