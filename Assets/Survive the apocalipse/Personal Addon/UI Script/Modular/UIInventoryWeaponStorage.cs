using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInventoryWeaponStorage : MonoBehaviour
{
    public static UIInventoryWeaponStorage singleton;
    public UIInventorySlot slotPrefab;
    public Transform content;

    [HideInInspector] public Player player;

    public void Start()
    {
        if (!singleton) singleton = this;
    }

    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;


        if (player != null)
        {
            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.inventory.Count, content);

            for (int i = 0; i < player.inventory.Count; i++)
            {
                int index = i;
                UIInventorySlot slot = content.GetChild(index).GetComponent<UIInventorySlot>();
                ItemSlot itemSlot = player.inventory[index];

                if (itemSlot.amount > 0)
                {
                    if((itemSlot.item.data is WeaponItem && (((WeaponItem)itemSlot.item.data).canUsePistolStorage || ((WeaponItem)itemSlot.item.data).canUseWeaponStorage)) || itemSlot.item.data is AmmoItem)
                    {
                        slot.button.interactable = true;
                    }
                    else
                    {
                        slot.button.interactable = false;
                    }

                    slot.button.onClick.SetListener(() =>
                    {
                        if (itemSlot.item.data is WeaponItem && ((WeaponItem)itemSlot.item.data).canUseWeaponStorage)
                        {
                            player.CmdAddWeaponToWeaponStorage(index, 0);
                        }
                        if (itemSlot.item.data is WeaponItem && ((WeaponItem)itemSlot.item.data).canUsePistolStorage)
                        {
                            player.CmdAddWeaponToWeaponStorage(index, 1);
                        }
                        if (itemSlot.item.data is AmmoItem)
                        {
                            player.CmdAddWeaponToWeaponStorage(index, 2);
                        }
                    });
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.white;
                    slot.image.sprite = itemSlot.item.image;
                    if (index < player.playerConservative.AmountOfItemProtected()) slot.protectedImage.gameObject.SetActive(true);
                    else slot.protectedImage.gameObject.SetActive(false);
                    slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(itemSlot.amount > 1);
                    slot.amountText.text = itemSlot.amount.ToString();
                    slot.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                    slot.GetComponent<Image>().color = GffItemRarity.singleton.rarityColor(true, itemSlot.item);
                }
                else
                {
                    slot.button.onClick.RemoveAllListeners();
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                    slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(false);
                    if (index <= player.playerConservative.AmountOfItemProtected()) slot.protectedImage.gameObject.SetActive(true);
                    else slot.protectedImage.gameObject.SetActive(false);
                    slot.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                    slot.GetComponent<Image>().color = GffItemRarity.singleton.ColorNull;

                }
            }
        }
    }
}
