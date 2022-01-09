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
    // Start is called before the first frame update
    void Start()
    {
        if (!singleton) singleton = this;
        description.text = string.Empty;


    }

    // Update is called once per frame
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
            //coin.gameObject.SetActive(true);
            //coin.interactable = player.coins >= boost.coin;
            coin.onClick.SetListener(() =>
            {
                if (player.coins < boost.coin)
                {
                    if (!GeneralManager.singleton.uiItemMallPanel)
                    {
                        GeneralManager.singleton.uiItemMallPanel = Instantiate(GeneralManager.singleton.itemMallPanel, GeneralManager.singleton.canvas);
                        GeneralManager.singleton.uiItemMallPanel.GetComponentInChildren<UIItemMall>().premium = true;
                    }
                }
                else
                {
                    player.playerBoost.CmdAddBoost(GeneralManager.singleton.listCompleteOfBoost.IndexOf(boost), 0, System.DateTime.Now.AddSeconds(GeneralManager.singleton.AddSecondsToBoost(boost)).ToString());
                }
            });

            gold.GetComponentInChildren<TextMeshProUGUI>().text = boost.gold.ToString();
            //gold.gameObject.SetActive(true);
            //gold.interactable = player.gold >= boost.gold;
            gold.onClick.SetListener(() =>
            {
                if (player.gold < boost.gold)
                {
                    if (!GeneralManager.singleton.uiItemMallPanel)
                    {
                        GeneralManager.singleton.uiItemMallPanel = Instantiate(GeneralManager.singleton.itemMallPanel, GeneralManager.singleton.canvas);
                        GeneralManager.singleton.uiItemMallPanel.GetComponentInChildren<UIItemMall>().premium = true;
                    }
                }
                else
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
