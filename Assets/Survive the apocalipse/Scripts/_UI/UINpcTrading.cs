﻿using System;
using UnityEngine;
using UnityEngine.UI;

public partial class UINpcTrading : MonoBehaviour
{
    public static UINpcTrading singleton;
    public GameObject panel;
    public UINpcTradingSlot slotPrefab;
    public Transform content;
    public UIDragAndDropable buySlot;
    public Image containerSlot;
    public InputField buyAmountInput;
    public Text buyCostsText;
    public Button buyButton;
    public UIDragAndDropable sellSlot;
    public InputField sellAmountInput;
    public Text sellCostsText;
    public Image containerSellSlot;
    public Button sellButton;
    [HideInInspector] public int buyIndex = -1;
    [HideInInspector] public int sellIndex = -1;

    public Button closeButton;

    TimeSpan difference;

    public UINpcTrading()
    {
        // assign singleton only once (to work with DontDestroyOnLoad when
        // using Zones / switching scenes)
        if (singleton == null) singleton = this;
    }

    void Update()
    {
        Player player = Player.localPlayer;

        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

        // use collider point(s) to also work with big entities
        if (player != null &&
            player.target != null &&
            player.target is Npc npc &&
            Utils.ClosestDistance(player.collider, player.target.collider) <= player.interactionRange)
        {
            // items for sale
            UIUtils.BalancePrefabs(slotPrefab.gameObject, npc.saleItems.Length, content);
            for (int i = 0; i < npc.saleItems.Length; ++i)
            {
                UINpcTradingSlot slot = content.GetChild(i).GetComponent<UINpcTradingSlot>();
                ScriptableItem itemData = npc.saleItems[i];

                // show item in UI
                int icopy = i;
                slot.button.onClick.SetListener(() =>
                {
                    buyIndex = icopy;
                });
                slot.image.color = Color.white;
                slot.image.sprite = itemData.image;
                // only build tooltip while it's actually shown. this
                // avoids MASSIVE amounts of StringBuilder allocations.
                slot.tooltip.enabled = true;
                if (slot.tooltip.IsVisible())
                    slot.tooltip.text = new ItemSlot(new Item(itemData)).ToolTip(); // with slot for {AMOUNT}
                slot.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                slot.GetComponent<Image>().color = GffItemRarity.singleton.rarityColor(true, new Item(itemData));
            }

            // buy
            if (buyIndex != -1 && buyIndex < npc.saleItems.Length)
            {
                ScriptableItem itemData = npc.saleItems[buyIndex];

                if(!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer))
                    difference = DateTime.Parse(player.playerBoost.networkBoost[0].hiddenIslandTimer.ToString()) - System.DateTime.Now;

                // make valid amount, calculate price
                int amount = buyAmountInput.text.ToInt();
                amount = Mathf.Clamp(amount, 1, itemData.maxStack);
                long price = amount * itemData.buyPrice;

                // show buy panel with item in UI
                buyAmountInput.text = amount.ToString();
                buySlot.GetComponent<Image>().color = Color.white;
                buySlot.GetComponent<Image>().sprite = itemData.image;
                // only build tooltip while it's actually shown. this
                // avoids MASSIVE amounts of StringBuilder allocations.
                buySlot.GetComponent<UIShowToolTip>().enabled = true;
                if (buySlot.GetComponent<UIShowToolTip>().IsVisible())
                    buySlot.GetComponent<UIShowToolTip>().text = new ItemSlot(new Item(itemData)).ToolTip(); // with slot for {AMOUNT}
                buySlot.dragable = true;
                if (player.playerPremiumZoneManager.inPremiumZone && player.playerBoost.networkBoost.Count > 0 && !string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer) && Convert.ToInt32(difference.TotalSeconds) > 0)
                {
                    buyButton.interactable = amount > 0 && price / 2 <= player.gold &&
                                             player.InventoryCanAdd(new Item(itemData), amount);
                    buyCostsText.text = (price / 2).ToString();
                }
                else
                {
                    buyButton.interactable = amount > 0 && price <= player.gold &&
                         player.InventoryCanAdd(new Item(itemData), amount);
                    buyCostsText.text = price.ToString();
                }
                buyButton.onClick.SetListener(() =>
                {
                    player.CmdNpcBuyItem(buyIndex, amount);
                    buyIndex = -1;
                    buyAmountInput.text = "1";
                });
                containerSlot.sprite = GffItemRarity.singleton.rarityType();
                containerSlot.color = GffItemRarity.singleton.rarityColor(true, new Item(itemData));
            }
            else
            {
                // show default buy panel in UI
                buySlot.GetComponent<Image>().color = Color.clear;
                buySlot.GetComponent<Image>().sprite = null;
                buySlot.GetComponent<UIShowToolTip>().enabled = false;
                containerSlot.sprite = GffItemRarity.singleton.rarityType();
                containerSlot.color = GffItemRarity.singleton.ColorNull;
                buySlot.dragable = false;
                buyCostsText.text = "0";
                buyButton.interactable = false;
            }

            // sell
            if (sellIndex != -1 && sellIndex < player.inventory.Count &&
                player.inventory[sellIndex].amount > 0)
            {
                ItemSlot itemSlot = player.inventory[sellIndex];

                // make valid amount, calculate price
                int amount = sellAmountInput.text.ToInt();
                amount = Mathf.Clamp(amount, 1, itemSlot.amount);
                long price = amount * itemSlot.item.sellPrice;

                // show sell panel with item in UI
                sellAmountInput.text = amount.ToString();
                sellSlot.GetComponent<Image>().color = Color.white;
                sellSlot.GetComponent<Image>().sprite = itemSlot.item.image;
                // only build tooltip while it's actually shown. this
                // avoids MASSIVE amounts of StringBuilder allocations.
                sellSlot.GetComponent<UIShowToolTip>().enabled = true;
                if (sellSlot.GetComponent<UIShowToolTip>().IsVisible())
                    sellSlot.GetComponent<UIShowToolTip>().text = itemSlot.ToolTip();
                sellSlot.dragable = true;
                sellCostsText.text = price.ToString();
                sellButton.interactable = amount > 0;
                sellButton.onClick.SetListener(() =>
                {
                    player.CmdNpcSellItem(sellIndex, amount);
                    sellIndex = -1;
                    sellAmountInput.text = "1";
                });
                containerSellSlot.sprite = GffItemRarity.singleton.rarityType();
                containerSellSlot.color = GffItemRarity.singleton.rarityColor(true, itemSlot.item);
            }
            else
            {
                // show default sell panel in UI
                sellSlot.GetComponent<Image>().color = Color.clear;
                sellSlot.GetComponent<Image>().sprite = null;
                sellSlot.GetComponent<UIShowToolTip>().enabled = false;
                containerSellSlot.sprite = GffItemRarity.singleton.rarityType();
                containerSellSlot.color = GffItemRarity.singleton.ColorNull;
                sellSlot.dragable = false;
                sellCostsText.text = "0";
                sellButton.interactable = false;
            }
        }
    }
}
