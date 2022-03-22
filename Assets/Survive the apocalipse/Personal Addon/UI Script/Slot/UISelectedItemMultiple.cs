using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using TMPro;
using System;

public class UISelectedItemMultiple : MonoBehaviour
{
    public static UISelectedItemMultiple singleton;
    public TextMeshProUGUI description;
    public Image itemImage;
    public Button goldButton;
    public TextMeshProUGUI goldText;
    public Button coinButton;
    public TextMeshProUGUI coinText;
    public Button closeItemButton;
    public TMP_InputField inputFieldAmount;

    public Item item;

    public int amount = -1;

    public Player player;

    public TimeSpan difference;

    public void Start()
    {
        if (!singleton) singleton = this;

        closeItemButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

    }

    public void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (inputFieldAmount.text != string.Empty)
        {
            amount = 0;
        }

        if (player.health == 0) Destroy(this.gameObject);

        if (inputFieldAmount.text != string.Empty) amount = Convert.ToInt32(inputFieldAmount.text);
        else amount = 0;

        description.text = item.ToolTip().Replace("{AMOUNT}", "");

        itemImage.sprite = item.image;

        coinText.text = (item.data.coinPrice * amount).ToString();

        if (player.playerBoost.networkBoost.Count > 0 && !string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer))
        {
            difference = DateTime.Parse(player.playerBoost.networkBoost[0].hiddenIslandTimer.ToString()) - System.DateTime.Now;
        }

        if (player.playerBoost.networkBoost.Count > 0 && !string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer) && difference.TotalSeconds > 0) goldButton.GetComponentInChildren<TextMeshProUGUI>().text = (Convert.ToInt32(item.data.goldPrice / 2) * amount).ToString();
        else goldButton.GetComponentInChildren<TextMeshProUGUI>().text = (item.data.goldPrice * amount).ToString();


        goldButton.interactable = item.name != null && amount > 0 && (Player.localPlayer.gold >= (item.data.goldPrice * amount));
        goldButton.onClick.SetListener(() =>
        {
            if (Convert.ToInt32(inputFieldAmount) != 0)
            {
                if (player.CheckBuyUpgradeRepairItem(item.name, amount, 0))
                {
                    Player.localPlayer.CmdBuyItemMallItem(item.name, amount, 0);
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                        ScreenNotificationManager.singleton.SpawnNotification("Grazie per l'acquisto!", "Item");
                    else
                        ScreenNotificationManager.singleton.SpawnNotification("Thanks for the purchase!", "Item");
                }
                else
                {
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                        ScreenNotificationManager.singleton.SpawnNotification("Sembra tu non possa comprare questo oggetto, controlla il tuo inventario", "Item");
                    else
                        ScreenNotificationManager.singleton.SpawnNotification("Seems you can't buy this item, check your inventary", "Item");

                }
            }
            closeItemButton.onClick.Invoke();
        });

        coinButton.interactable = item.name != null && amount > 0 && (Player.localPlayer.coins >= (item.data.coinPrice * amount));
        coinButton.onClick.SetListener(() =>
        {
            if (player.CheckBuyUpgradeRepairItem(item.name, amount, 1))
            {
                Player.localPlayer.CmdBuyItemMallItem(item.name, amount, 1);
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    ScreenNotificationManager.singleton.SpawnNotification("Grazie per l'acquisto!", "Item");
                else
                    ScreenNotificationManager.singleton.SpawnNotification("Thanks for the purchase!", "Item");
            }
            else
            {
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    ScreenNotificationManager.singleton.SpawnNotification("Sembra tu non possa comprare questo oggetto, controlla il tuo inventario", "Item");
                else
                    ScreenNotificationManager.singleton.SpawnNotification("Seems you can't buy this item, check your inventary", "Item");

            }
            closeItemButton.onClick.Invoke();
        });
    }
}
