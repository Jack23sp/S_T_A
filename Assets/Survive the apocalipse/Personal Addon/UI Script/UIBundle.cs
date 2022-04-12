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

    public BundleSlot clothesEquipItem;
    public BundleItem clothesEquipBundleItem;

    public void OnEnable()
    {
        if (Player.localPlayer)
        {
            if (Player.localPlayer.playerCreation.sex == 0)
            {
                verticalItems = GeneralManager.singleton.manVerticalItem;
                clothesEquipBundleItem = GeneralManager.singleton.manOutfit[GeneralManager.singleton.equipmentIndex];
            }
            else
            {
                verticalItems = GeneralManager.singleton.womanVerticalItem;
                clothesEquipBundleItem = GeneralManager.singleton.womanOutfit[GeneralManager.singleton.equipmentIndex];
            }
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

            BundleSlot bundleSlotEquipment = clothesEquipItem;
            bundleSlotEquipment.bundleName.text = "";
            bundleSlotEquipment.buyButtonText.text = clothesEquipBundleItem.coins.ToString();
            bundleSlotEquipment.confirmButton.gameObject.SetActive(false);

            bundleSlotEquipment.buyButton.onClick.SetListener(() =>
            {
                bundleSlotEquipment.confirmButton.gameObject.SetActive(true);
            });
            UIUtils.BalancePrefabs(budleSlot.gameObject, clothesEquipBundleItem.bundleitems.Count, bundleSlotEquipment.content);
            for (int a = 0; a < bundleSlotEquipment.content.childCount; a++)
            {
                int index_a = a;
                BundleSlotInternal bundleSlotInternal = bundleSlotEquipment.content.GetChild(index_a).GetComponent<BundleSlotInternal>();
                bundleSlotInternal.image.sprite = clothesEquipBundleItem.bundleitems[index_a].item.image;
                bundleSlotInternal.plus.SetActive(index_a < (clothesEquipBundleItem.bundleitems.Count - 1));
                if (clothesEquipBundleItem.bundleitems[index_a].quantity > 1)
                {
                    bundleSlotInternal.amountObject.SetActive(true);
                    bundleSlotInternal.textAmount.text = clothesEquipBundleItem.bundleitems[index_a].quantity.ToString();
                }
                else
                {
                    bundleSlotInternal.amountObject.SetActive(false);
                }
            }

            bundleSlotEquipment.confirmButton.onClick.SetListener(() =>
            {
                bool canAdd = true;
                for (int u = 0; u < clothesEquipBundleItem.bundleitems.Count; u++)
                {
                    int index_u = u;
                    if (!Player.localPlayer.InventoryCanAdd(new Item(clothesEquipBundleItem.bundleitems[index_u].item), clothesEquipBundleItem.bundleitems[index_u].quantity))
                    {
                        canAdd = false;
                    }
                }
                if (canAdd) if (Player.localPlayer.coins < clothesEquipBundleItem.coins) canAdd = false;
                if (canAdd)
                {
                    Player.localPlayer.CmdBuyBundleEquipment(clothesEquipBundleItem.name, Player.localPlayer.playerCreation.sex);
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
                bundleSlotEquipment.confirmButton.gameObject.SetActive(false);
            });
        }
    }
}
