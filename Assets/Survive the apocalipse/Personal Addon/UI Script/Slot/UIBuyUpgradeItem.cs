using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using TMPro;
using System;

public class UIBuyUpgradeItem : MonoBehaviour
{
    public static UIBuyUpgradeItem singleton;
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

    public void Start()
    {
        if (!singleton) singleton = this;
    }

    public void Update()
    {
        if (inputFieldAmount.text != string.Empty)
        {
            amount = 0;
        }
        closeItemButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

        if (Player.localPlayer.health == 0) Destroy(this.gameObject);

        if (inputFieldAmount.text != string.Empty) amount = Convert.ToInt32(inputFieldAmount.text);
        else amount = 0;

        description.text = item.ToolTip().Replace("{AMOUNT}", "" );

        itemImage.sprite = item.image;

        goldText.text = (item.goldPrice * amount).ToString();
        coinText.text = (item.coinPrice * amount).ToString();

        goldButton.interactable = item.name != null && amount > 0 && (Player.localPlayer.gold >= (item.goldPrice * amount));
        goldButton.onClick.SetListener(() =>
        {
            if(item.name != null && Convert.ToInt32(inputFieldAmount) != 0)
            {
                Player.localPlayer.CmdBuyUpgradeRepairItem(item.name, amount, 0);
            }
            closeItemButton.onClick.Invoke();
        });

        coinButton.interactable = item.name != null && amount > 0 && (Player.localPlayer.coins >= (item.coinPrice * amount));
        coinButton.onClick.SetListener(() =>
        {
            Player.localPlayer.CmdBuyUpgradeRepairItem(item.name, amount, 1);
            closeItemButton.onClick.Invoke();
        });
    }
}
