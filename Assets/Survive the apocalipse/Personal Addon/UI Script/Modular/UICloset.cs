using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICloset : MonoBehaviour
{
    public static UICloset singleton;
    public Transform closetContainer;
    public Transform inventoryContainer;

    public GameObject objectToSpawn;
    [HideInInspector] public Player player;

    public List<int> selectedInventoryIndex = new List<int>();
    public List<int> selectedClosetIndex = new List<int>();

    //public List<int> clothesAmount = new List<int>();

    public Button CloseButton;
    public Button switchButton;

    public Closet closet;


    void Start()
    {
        if (!singleton) singleton = this;
    }

    void Update()
    {
        CloseButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.health == 0)
            CloseButton.onClick.Invoke();

        switchButton.onClick.SetListener(() =>
        {
            player.CmdSwitchInventoryCloset(selectedClosetIndex.ToArray(), selectedInventoryIndex.ToArray());
            selectedClosetIndex.Clear();
            selectedInventoryIndex.Clear();
        });

        if (!player.playerMove.fornitureClient) return;
        if (!player.playerMove.fornitureClient.GetComponent<Closet>()) return;
        else closet = player.playerMove.fornitureClient.GetComponent<Closet>();

        UIUtils.BalancePrefabs(objectToSpawn, player.inventory.Count, inventoryContainer);
        for (int i = 0; i < player.inventory.Count; i++)
        {
            int icopy = i;
            UIInventorySlot slot = inventoryContainer.GetChild(icopy).GetComponent<UIInventorySlot>();
            ItemSlot itemSlot = player.inventory[icopy];
            
            if (itemSlot.amount > 0)
            {
                if (player.inventory[icopy].item.data is WeaponItem)
                {
                    if (((WeaponItem)player.inventory[icopy].item.data).isClothes)
                    {
                        slot.button.interactable = true;
                    }
                    if (!((WeaponItem)player.inventory[icopy].item.data).isClothes)
                    {
                        slot.button.interactable = false;
                    }
                }
                else
                {
                    slot.button.interactable = false;
                }

                slot.button.onClick.SetListener(() =>
                {
                    if (!selectedInventoryIndex.Contains(icopy))
                    {
                        selectedInventoryIndex.Add(icopy);
                    }
                    else
                    {
                        selectedInventoryIndex.Remove(icopy);
                    }

                });
                slot.image.color = Color.white;
                slot.image.sprite = itemSlot.item.image;
                slot.cooldownCircle.fillAmount = 0;
                slot.amountOverlay.SetActive(itemSlot.amount > 1);
                slot.amountText.text = itemSlot.amount.ToString();
                slot.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                slot.GetComponent<Image>().color = GffItemRarity.singleton.rarityColor(true, itemSlot.item);
            }
            else
            {
                slot.button.onClick.RemoveAllListeners();
                slot.image.color = Color.clear;
                slot.image.sprite = null;
                slot.cooldownCircle.fillAmount = 0;
                slot.amountOverlay.SetActive(false);
                slot.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                slot.GetComponent<Image>().color = GffItemRarity.singleton.ColorNull;
            }
        }
        for (int e = 0; e < player.inventory.Count; e++)
        {
            int index = e;
            if (selectedInventoryIndex.Contains(index)) inventoryContainer.GetChild(index).GetComponent<Outline>().enabled = true;
            else inventoryContainer.GetChild(index).GetComponent<Outline>().enabled = false;
        }
        for (int e = 0; e < closetContainer.childCount; e++)
        {
            int index = e;
            if (selectedClosetIndex.Contains(index)) closetContainer.GetChild(index).GetComponent<Outline>().enabled = true;
            else closetContainer.GetChild(index).GetComponent<Outline>().enabled = false;
        }

        UIUtils.BalancePrefabs(objectToSpawn, closet.maxSlotAmount, closetContainer);
        for (int a = 0; a < closetContainer.childCount; a++)
        {
            UIInventorySlot slot2 = closetContainer.GetChild(a).GetComponent<UIInventorySlot>();
            ItemSlot itemSlot2 = closet.inventory[a];

            if (itemSlot2.amount > 0)
            {
                int icopy = a;
                slot2.button.onClick.SetListener(() =>
                {
                    if (!selectedClosetIndex.Contains(icopy))
                    {
                        selectedClosetIndex.Add(icopy);
                    }
                    else
                    {
                        selectedClosetIndex.Remove(icopy);
                    }

                });
                slot2.image.color = Color.white;
                slot2.image.sprite = itemSlot2.item.image;
                slot2.cooldownCircle.fillAmount = 0;
                slot2.amountOverlay.SetActive(itemSlot2.amount > 1);
                slot2.amountText.text = itemSlot2.amount.ToString();
                slot2.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                slot2.GetComponent<Image>().color = GffItemRarity.singleton.rarityColor(true, itemSlot2.item);
            }
            else
            {
                slot2.button.onClick.RemoveAllListeners();
                slot2.image.color = Color.clear;
                slot2.image.sprite = null;
                slot2.cooldownCircle.fillAmount = 0;
                slot2.amountOverlay.SetActive(false);
                slot2.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                slot2.GetComponent<Image>().color = GffItemRarity.singleton.ColorNull;
            }
        }
    }
}
