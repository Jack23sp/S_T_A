// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using TMPro;

public partial class UIGuildInvite : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Player player;
    public GameObject panel;

    public Button AcceptButton;
    public Button DeclineButton;

    void Update()
    {
        player = Player.localPlayer;

        if (player)
        {
            if (player.health == 0)
                panel.SetActive(false);

            if (player != null && player.guildInviteFrom != "")
            {
                panel.SetActive(true);
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    nameText.text = player.guildInviteFrom + " ti ha mandato un invito al suo gruppo. \nVuoi entrare? ";
                }
                else
                {
                    nameText.text = player.guildInviteFrom + " sent you an invite to his/her group. \nDo you want join? ";
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
        player.CmdGuildInviteAccept();
    }

    public void DeclineGuildIvite()
    {
        player.CmdGuildInviteDecline();
    }
}
