// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class UIPartyInvite : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public GameObject panel;
    public Player player;

    public Button AcceptButton;
    public Button DeclineButton;

    public void Start()
    {
        player = Player.localPlayer;

        AcceptButton.onClick.SetListener(() =>
        {
            AcceptGuildIvite();
        });

        DeclineButton.onClick.SetListener(() =>
        {
            DeclineGuildIvite();
        });
    }

    void Update()
    {
        player = Player.localPlayer;

        if (player)
        {
            if (player.health == 0)
                panel.SetActive(false);

            if (player != null && player.partyInviteFrom != "")
            {
                panel.SetActive(true);
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    nameText.text = player.partyInviteFrom + " ti ha mandato un invito ad un party. \nVuoi entrare? ";
                }
                else
                {
                    nameText.text = player.partyInviteFrom + " sent you an invite to a party. \nDo you want join? ";
                }
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
        player.CmdPartyInviteAccept();
    }

    public void DeclineGuildIvite()
    {
        player.CmdPartyInviteDecline();
    }
}
