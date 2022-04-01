using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBundle : MonoBehaviour
{
    public Transform content;
    public BundleSlotInternal budleSlot;

    public GameObject clothesItem;
    public List<BundleItem> allItems;
    public BundleItem verticalItems;

    public void OnEnable()
    {
        if (Player.localPlayer)
        {
            if (Player.localPlayer.playerCreation.sex == 0)
                verticalItems = GeneralManager.singleton.manVerticalItem;
            else
                verticalItems = GeneralManager.singleton.womanVerticalItem;

            allItems = GeneralManager.singleton.allItems;

            UIUtils.BalancePrefabs(clothesItem, allItems.Count, content);
            for (int i = 0; i < allItems.Count; i++)
            {
                int index = i;
                BundleSlot bundleSlot = content.GetChild(index).GetComponent<BundleSlot>();
                bundleSlot.bundleName.text = allItems[index].name;
                bundleSlot.buyButtonText.text = allItems[index].coins.ToString();
                bundleSlot.confirmButton.gameObject.SetActive(false);
                bundleSlot.buyButton.onClick.SetListener(() =>
                {
                    bundleSlot.confirmButton.gameObject.SetActive(true);
                    for (int e = 0; e < content.childCount; e++)
                    {
                        int index_e = e;
                        if (index_e != index)
                        {
                            content.GetChild(index_e).GetComponent<BundleSlot>().confirmButton.gameObject.SetActive(false);
                        }
                    }
                });
                bundleSlot.confirmButton.onClick.SetListener(() =>
                {
                    bool canAdd = true;
                    for (int u = 0; u < allItems[index].bundleitems.Count; u++)
                    {
                        int index_u = u;
                        if (!Player.localPlayer.InventoryCanAdd(new Item(allItems[index].bundleitems[index_u].item), allItems[index].bundleitems[index_u].quantity))
                        {
                            canAdd = false;
                        }
                    }
                    if (canAdd) if (Player.localPlayer.coins < allItems[index].coins) canAdd = false;
                    if (canAdd)
                    {
                        Player.localPlayer.CmdBuyBundle(allItems[index].name);
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
                    Player.localPlayer.CmdBuyBundle(allItems[index].name);
                    bundleSlot.confirmButton.gameObject.SetActive(false);
                });
                UIUtils.BalancePrefabs(budleSlot.gameObject, allItems[index].bundleitems.Count, bundleSlot.content);
                for (int a = 0; a < bundleSlot.content.childCount; a++)
                {
                    int index_a = a;
                    BundleSlotInternal bundleSlotInternal = bundleSlot.content.GetChild(index_a).GetComponent<BundleSlotInternal>();
                    bundleSlotInternal.image.sprite = allItems[index].bundleitems[index_a].item.image;
                    bundleSlotInternal.plus.SetActive(index_a < (allItems[index].bundleitems.Count - 1));
                    if (allItems[index].bundleitems[index_a].quantity > 1)
                    {
                        bundleSlotInternal.amountObject.SetActive(true);
                        bundleSlotInternal.textAmount.text = allItems[index].bundleitems[index_a].quantity.ToString();
                    }
                    else
                    {
                        bundleSlotInternal.amountObject.SetActive(false);
                    }
                }
            }
        }
    }
}
