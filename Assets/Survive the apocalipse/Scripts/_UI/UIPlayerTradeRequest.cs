// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class UIPlayerTradeRequest : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public GameObject panel;
    public Player player;

    public Button AcceptButton;
    public Button DeclineButton;

    public void Start()
    {
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
                if (player != null && player.tradeRequestFrom != "" && player.state != "TRADING")
                {
                    panel.SetActive(true);
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        nameText.text = player.tradeRequestFrom + " vuole iniziare uno scambio con te.\nVuoi accettare?";
                    }
                    else
                    {
                        nameText.text = player.tradeRequestFrom + " want start a trade with you.\nDo you want accept?";
                    }
                transform.SetAsLastSibling();
                }
                else
                {
                    panel.SetActive(false);
                }


                transform.SetAsLastSibling();
        }

        if (player && player.state == "TRADING" && !GeneralManager.singleton.spawnedTradingPanel)
        {
            GeneralManager.singleton.spawnedTradingPanel = Instantiate(GeneralManager.singleton.tradingPanelToSpawn, GeneralManager.singleton.canvas);
        }

        if (player && player.state != "TRADING" && GeneralManager.singleton.spawnedTradingPanel)
        {
            Destroy(GeneralManager.singleton.spawnedTradingPanel);
        }
    }

    public void AcceptGuildIvite()
    {
        player.CmdTradeRequestAccept();
    }

    public void DeclineGuildIvite()
    {
        player.CmdTradeRequestDecline();
    }
}
