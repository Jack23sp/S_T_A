using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class UIGuildAllyInvite : MonoBehaviour
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

            if (player != null && player.guildAllyInviteName != "")
            {
                panel.SetActive(true);
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    nameText.text = player.guildAllyInviteName + " ti ha mandato un invito al loro gruppo di alleanze. \nVuoi entrare? ";
                }
                else
                {
                    nameText.text = player.guildAllyInviteName + " sent you an invite to their group of alliance. \nDo you want join? ";
                }
                AcceptButton.onClick.SetListener(() =>
                {
                    AcceptGuildInvite();
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

    public void AcceptGuildInvite()
    {
        player.playerAlliance.CmdAcceptInviteToAlliance();
    }

    public void DeclineGuildIvite()
    {
        player.playerAlliance.CmdDeclineInviteToAlliance();
    }
}

