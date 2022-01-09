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
    public Button categorySlotPrefab;
    public Transform categoryContent;
    public Transform upperPremiumContent;
    public GameObject upperPremiumContentParent;
    public Transform lowerPremiumContent;
    public GameObject lowerPremiumContentParent;
    public UIItemMallSlot itemSlotPrefab;
    public GameObject itemSlotPrefabPremium;
    public Transform itemContent;
    public GameObject itemContentParent;
    public string buyUrl = "http://unity3d.com/";
    int currentCategory = 0;
    public Text nameText;
    public Text levelText;
    public Text currencyAmountText;
    public Button buyButton;
    public InputField couponInput;
    public Button couponButton;
    public GameObject inventoryPanel;
    public Button normalButton;
    public Button premiumButton;
    public GameObject categoriesText;
    public bool premium;

    public bool settedUpperItem;

    public Item item;

    public IAPManager IAPManager;

    public GameObject multipleBuyItemPanel;

    public Button closeButton;

    TimeSpan difference;

    public List<ItemMallCategory> totalCategory = new List<ItemMallCategory>();

    private bool combined = false;

    public void Start()
    {
        if (!singleton) singleton = this;

        IAPManager = FindObjectOfType<IAPManager>();

        itemContent.gameObject.SetActive(true);
        upperPremiumContent.gameObject.SetActive(false);
        lowerPremiumContent.gameObject.SetActive(false);

        itemSlotPrefabPremium.SetActive(true);
        upperPremiumContentParent.SetActive(false);
        lowerPremiumContentParent.SetActive(false);
    }

    void ScrollToBeginning()
    {
        // update first so we don't ignore recently added messages, then scroll
        Canvas.ForceUpdateCanvases();
        //scrollRect.verticalNormalizedPosition = 1;
    }

    void Update()
    {
        closeButton.onClick.SetListener(() =>
        {
            Destroy(closeButton.gameObject);
        });

        normalButton.onClick.SetListener(() =>
        {
            premium = false;

            itemContent.gameObject.SetActive(true);
            upperPremiumContent.gameObject.SetActive(false);
            lowerPremiumContent.gameObject.SetActive(false);

            itemSlotPrefabPremium.SetActive(true);
            upperPremiumContentParent.SetActive(false);
            lowerPremiumContentParent.SetActive(false);

        });
        premiumButton.onClick.SetListener(() =>
        {
            premium = true;

            itemContent.gameObject.SetActive(false);
            upperPremiumContent.gameObject.SetActive(true);
            lowerPremiumContent.gameObject.SetActive(true);

            itemSlotPrefabPremium.SetActive(false);
            upperPremiumContentParent.SetActive(true);
            lowerPremiumContentParent.SetActive(true);

            if (!settedUpperItem)
            {
                for (int i = 0; i < IAPManager.singleton.goldProducts.Count; ++i)
                {
                    Instantiate(itemSlotPrefabPremium, upperPremiumContent);
                }
                for (int i = 0; i < IAPManager.singleton.gemsProducts.Count; ++i)
                {
                    Instantiate(itemSlotPrefabPremium, lowerPremiumContent);
                }

                settedUpperItem = true;
            }
        });
        Player player = Player.localPlayer;
        if (player != null)
        {
            categoriesText.SetActive(!premium);
            if (!premium)
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
                // instantiate/destroy enough category slots
                UIUtils.BalancePrefabs(categorySlotPrefab.gameObject, totalCategory.Count, categoryContent);

                // refresh all category buttons
                for (int i = 0; i < totalCategory.Count; i++)
                {
                    Button button = categoryContent.GetChild(i).GetComponent<Button>();
                    button.interactable = i != currentCategory;
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        button.GetComponentInChildren<TextMeshProUGUI>().text = totalCategory[i].categoryIta;
                    }
                    else
                    {
                        button.GetComponentInChildren<TextMeshProUGUI>().text = totalCategory[i].category;
                    }
                    int icopy = i; // needed for lambdas, otherwise i is Count
                    button.onClick.SetListener(() =>
                    {
                        itemContent.GetComponent<RectTransform>().anchoredPosition = new Vector2(GetComponent<RectTransform>().anchoredPosition.x, 0.0f);
                            // set new category and then scroll to the top again
                            currentCategory = icopy;
                            //ScrollToBeginning();
                        });
                }

                if (player.itemMallCategories.Length > 0)
                {
                    // instantiate/destroy enough item slots for that category
                    ScriptableItem[] items = totalCategory[currentCategory].items;
                    UIUtils.BalancePrefabs(itemSlotPrefab.gameObject, items.Length, itemContent);

                    // refresh all items in that category
                    for (int i = 0; i < items.Length; i++)
                    {
                        int index = i;
                        if (player.playerBoost.networkBoost.Count > 0 && !string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer))
                            difference = DateTime.Parse(player.playerBoost.networkBoost[0].hiddenIslandTimer.ToString()) - System.DateTime.Now;

                        UIItemMallSlot slot = itemContent.GetChild(index).GetComponent<UIItemMallSlot>();
                        ScriptableItem itemData = items[index];

                        slot.image.color = Color.white;
                        slot.image.sprite = itemData.image;
                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                        {
                            slot.nameText.text = itemData.italianName;
                        }
                        else
                        {
                            slot.nameText.text = itemData.name;
                        }
                        slot.goldText.text = player.playerBoost.networkBoost.Count > 0 && !string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer) && Convert.ToInt32(difference.TotalSeconds) > 0 ? (Convert.ToInt32(itemData.goldPrice / 2)).ToString() : itemData.goldPrice.ToString();
                        slot.coinText.text = itemData.coinPrice.ToString();
                        int icopy = index; // needed for lambdas, otherwise i is Count

                        slot.unlockButton.onClick.SetListener(() =>
                        {
                            GameObject g = Instantiate(GeneralManager.singleton.selectedItemMallMultiple, GeneralManager.singleton.canvas);
                            item = new Item(itemData);
                            slot.tooltip.text = item.ToolTip();
                            g.GetComponent<UISelectedItemMultiple>().item = new Item(totalCategory[currentCategory].items[icopy]);
                        });
                    }
                }
            }
            else
            {
                UIUtils.BalancePrefabs(categorySlotPrefab.gameObject, 0, categoryContent);

                for (int i = 0; i < upperPremiumContent.childCount; i++)
                {
                    int index = i;
                    UIItemMallSlotPremium slot = upperPremiumContent.GetChild(index).GetComponent<UIItemMallSlotPremium>();
                    slot.gameObject.SetActive(true);
                    slot.nameText.text = IAPManager.singleton.goldProducts[index].Name;
                    if (IAPManager.singleton.goldProducts[index].Price.Contains("."))
                    {
                        slot.priceText.text = "€ " + IAPManager.singleton.goldProducts[index].Price;

                    }
                    else
                    {
                        slot.priceText.text = "€ " + IAPManager.singleton.goldProducts[index].Price + ".00";
                    }

                    slot.image.sprite = IAPManager.singleton.GetPremiumItemImage(slot.nameText.text);
                    slot.unlockButton.onClick.SetListener(() =>
                    {
                        IAPManager.singleton.Purchase(IAPManager.singleton.goldProducts[index].Id);
                    });
                }
                for (int i = 0; i < lowerPremiumContent.childCount; ++i)
                {
                    int index = i;
                    UIItemMallSlotPremium slot = lowerPremiumContent.GetChild(index).GetComponent<UIItemMallSlotPremium>();
                    slot.gameObject.SetActive(true);
                    slot.nameText.text = IAPManager.singleton.gemsProducts[index].Name;
                    if (IAPManager.singleton.goldProducts[index].Price.Contains("."))
                    {
                        slot.priceText.text = "€ " + IAPManager.singleton.gemsProducts[index].Price;
                    }
                    else
                    {
                        slot.priceText.text = "€ " + IAPManager.singleton.gemsProducts[index].Price + ".00";
                    }
                    slot.image.sprite = IAPManager.singleton.GetPremiumItemImage(slot.nameText.text);
                    slot.unlockButton.onClick.SetListener(() =>
                    {
                        IAPManager.singleton.Purchase(IAPManager.singleton.gemsProducts[index].Id);
                    });
                }
            }

            // overview
            nameText.text = player.name;
            levelText.text = "Lv. " + player.level;
            currencyAmountText.text = player.coins.ToString();
            buyButton.onClick.SetListener(() => { Application.OpenURL(buyUrl); });
            couponInput.interactable = NetworkTime.time >= player.nextRiskyActionTime;
            couponButton.interactable = NetworkTime.time >= player.nextRiskyActionTime;
            couponButton.onClick.SetListener(() =>
            {
                if (!string.IsNullOrWhiteSpace(couponInput.text))
                    player.CmdEnterCoupon(couponInput.text);
                couponInput.text = "";
            });
        }
    }
}
