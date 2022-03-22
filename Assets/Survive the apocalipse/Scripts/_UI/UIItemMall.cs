using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;
using TMPro;
using System.Linq;
using System.Collections.Generic;

public partial class UIItemMall : MonoBehaviour
{
    public static UIItemMall singleton;

    public KeyCode hotKey = KeyCode.X;
    public GameObject panel;
    public Transform contentCategory;
    public UICategorySlot categorySlot;
    public UIItemMallSlot itemMallSlot;
    public Item item;

    public Button items;
    public Button bundle;
    public Button coins;

    public bool itemsCat;
    public bool bundleCat;
    public bool coinsCat;

    public IAPManager IAPManager;

    public GameObject multipleBuyItemPanel;
    public UISelectedItemMultiple multipleItem;

    public Button closeButton;

    TimeSpan difference;

    public List<ItemMallCategory> totalCategory = new List<ItemMallCategory>();

    private bool combined = false;

    void ScrollToBeginning()
    {
        Canvas.ForceUpdateCanvases();
    }

    void Start()
    {
        if (!singleton) singleton = this;

        IAPManager = FindObjectOfType<IAPManager>();

        closeButton.onClick.SetListener(() =>
        {
            Destroy(closeButton.gameObject);
        });

        Player player = Player.localPlayer;
        if (player != null)
        {
            items.onClick.SetListener(() =>
            {
                itemsCat = true;
                bundleCat = false;
                coinsCat = false;
            });
            bundle.onClick.SetListener(() =>
            {
                itemsCat = false;
                bundleCat = true;
                coinsCat = false;
            });
            coins.onClick.SetListener(() =>
            {
                itemsCat = false;
                bundleCat = false;
                coinsCat = true;
            });

            if (itemsCat)
            {
                if (!combined)
                {
                    if (player.playerCreation.sex == 0)
                    {
                        totalCategory = GeneralManager.singleton.itemMallCategories.ToList().Union(GeneralManager.singleton.manClothes.ToList()).ToList();
                    }
                    else
                    {
                        totalCategory = GeneralManager.singleton.itemMallCategories.ToList().Union(GeneralManager.singleton.womanClothes.ToList()).ToList();
                    }
                    combined = true;
                }
                UIUtils.BalancePrefabs(categorySlot.gameObject, totalCategory.Count, contentCategory);

                for (int i = 0; i < totalCategory.Count; i++)
                {
                    int currentCategory = i;
                    UICategorySlot category = contentCategory.GetChild(i).GetComponent<UICategorySlot>();
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        category.categoryName.text = totalCategory[i].categoryIta;
                    }
                    else
                    {
                        category.categoryName.text = totalCategory[i].category;
                    }

                    UIUtils.BalancePrefabs(itemMallSlot.gameObject, totalCategory[currentCategory].items.Length, category.content);
                    for (int e = 0; e < totalCategory[currentCategory].items.Length; e++)
                    {
                        if (player.playerBoost.networkBoost.Count > 0 && !string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer))
                            difference = DateTime.Parse(player.playerBoost.networkBoost[0].hiddenIslandTimer.ToString()) - System.DateTime.Now;
                        int category_e = e;
                        UIItemMallSlot mallSlot = category.content.GetChild(category_e).GetComponent<UIItemMallSlot>();
                        ScriptableItem item = totalCategory[currentCategory].items[category_e];
                        mallSlot.coinText.text = item.coinPrice.ToString();
                        mallSlot.image.sprite = item.image;
                        mallSlot.goldText.text = player.playerBoost.networkBoost.Count > 0 && !string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer) && Convert.ToInt32(difference.TotalSeconds) > 0 ? (Convert.ToInt32(item.goldPrice / 2)).ToString() : item.goldPrice.ToString();
                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                        {
                            mallSlot.nameText.text = item.italianName;
                        }
                        else
                        {
                            mallSlot.nameText.text = item.name;
                        }
                        mallSlot.unlockButton.onClick.SetListener(() =>
                        {
                            multipleBuyItemPanel.SetActive(true);
                            multipleItem.item = new Item(item);
                        });
                    }
                }
            }
            if(bundleCat)
            {

            }
            if(coinsCat)
            {

            }
        }
    }
}
