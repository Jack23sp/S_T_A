using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISelectedItemMallItem : MonoBehaviour
{
    public static UISelectedItemMallItem singleton;
    public TextMeshProUGUI description;
    public Image itemImage;
    public Button CoinButton;
    public Button GoldButton;
    public Button closeItemButton;
    public Item item;
    private Player player;

    public string tooltip;

    public int currentCategory = -1;
    public int icopy = -1;


    // Start is called before the first frame update
    void Start()
    {
        closeItemButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

    }

    // Update is called once per frame
    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.health == 0) Destroy(this.gameObject);

        if (item.name != null)
        {
            TimeSpan difference;
            itemImage.sprite = item.image;
            //CoinButton.interactable = player.coins >= item.coinPrice;
            //GoldButton.interactable = player.gold >= item.goldPrice;
            CoinButton.interactable = player.InventoryCanAdd(UIItemMall.singleton.item, 1);
            GoldButton.interactable = player.InventoryCanAdd(UIItemMall.singleton.item, 1);
            CoinButton.GetComponentInChildren<TextMeshProUGUI>().text = item.coinPrice.ToString();

            if(!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer))
                difference = DateTime.Parse(player.playerBoost.networkBoost[0].hiddenIslandTimer.ToString()) - System.DateTime.Now;


            if (player.playerBoost.networkBoost.Count > 0 && !string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer) && difference.TotalSeconds > 0) GoldButton.GetComponentInChildren<TextMeshProUGUI>().text =  Convert.ToInt32(item.goldPrice / 2).ToString();
            else GoldButton.GetComponentInChildren<TextMeshProUGUI>().text = item.goldPrice.ToString();

            description.text = tooltip;

            if (!CoinButton.interactable)
                description.text += "\n\nOps seems that your inventory is full! Free some slot to buy this item!";



            CoinButton.onClick.SetListener(() =>
            {
                if (player.coins < item.coinPrice)
                {
                    if (!GeneralManager.singleton.uiItemMallPanel)
                    {
                        GeneralManager.singleton.uiItemMallPanel = Instantiate(GeneralManager.singleton.itemMallPanel, GeneralManager.singleton.canvas);
                        GeneralManager.singleton.uiItemMallPanel.GetComponentInChildren<UIItemMall>().premium = true;
                        Destroy(this.gameObject);
                    }
                }
                else
                {
                    player.CmdUnlockItem(currentCategory, icopy, 0);
                    Destroy(this.gameObject);
                }
            });
            GoldButton.onClick.SetListener(() =>
            {
                if (player.gold < item.goldPrice)
                {
                    if (!GeneralManager.singleton.uiItemMallPanel)
                    {
                        GeneralManager.singleton.uiItemMallPanel = Instantiate(GeneralManager.singleton.itemMallPanel, GeneralManager.singleton.canvas);
                        GeneralManager.singleton.uiItemMallPanel.GetComponentInChildren<UIItemMall>().premium = true;
                        Destroy(this.gameObject);
                    }
                }
                else
                {
                    player.CmdUnlockItem(currentCategory, icopy, 1);
                    Destroy(this.gameObject);
                }
            });
        }


    }
}
