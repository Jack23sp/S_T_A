using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISelectedBoost : MonoBehaviour
{
    private Player player;
    public static UISelectedBoost singleton;

    public TextMeshProUGUI description;
    public Button coin;
    public Button gold;

    public Button closeButton;

    void Start()
    {
        if (!singleton) singleton = this;
        description.text = string.Empty;


    }


    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.health == 0) Destroy(this.gameObject);

        if (UIBoost.singleton && UIBoost.singleton.selectedBoost && description.text == string.Empty)
        {
            ScriptableBoost boost = UIBoost.singleton.selectedBoost;
            description.text = boost.GetDescription();

            coin.GetComponentInChildren<TextMeshProUGUI>().text = boost.coin.ToString();
            coin.interactable = player.coins >= boost.coin;
            coin.onClick.SetListener(() =>
            {
                if (player.coins >= boost.coin)
                {
                    player.playerBoost.CmdAddBoost(GeneralManager.singleton.listCompleteOfBoost.IndexOf(boost), 0, System.DateTime.Now.AddSeconds(GeneralManager.singleton.AddSecondsToBoost(boost)).ToString());
                }
            });

            gold.GetComponentInChildren<TextMeshProUGUI>().text = boost.gold.ToString();
            gold.interactable = player.gold >= boost.gold;
            gold.onClick.SetListener(() =>
            {
                if (player.gold < boost.gold)
                {
                    player.playerBoost.CmdAddBoost(GeneralManager.singleton.listCompleteOfBoost.IndexOf(boost), 1, System.DateTime.Now.AddSeconds(GeneralManager.singleton.AddSecondsToBoost(boost)).ToString());
                }
            });
        }
        closeButton.onClick.SetListener(() =>
        {
            UIBoost.singleton.selectedBoost = null;
            Destroy(this.gameObject);
        });

    }
}
