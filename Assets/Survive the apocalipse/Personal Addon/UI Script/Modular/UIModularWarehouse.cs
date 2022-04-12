using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIModularWarehouse : MonoBehaviour
{
    public static UIModularWarehouse singleton;
    public Transform warehouseContainer;
    public Transform inventoryContainer;

    public GameObject objectToSpawn;
    [HideInInspector] public Player player;

    public List<int> selectedInventoryIndex = new List<int>();
    public List<int> selectedWarehouseIndex = new List<int>();

    public Button CloseButton;
    public Button switchButton;

    public ModularPersonalWareHouse warehouse;

    public void Awake()
    {
        if (!Player.localPlayer ||
           !Player.localPlayer.playerMove.fornitureClient ||
           !Player.localPlayer.playerMove.fornitureClient.GetComponent<ModularPersonalWareHouse>())
        {
            Destroy(this.gameObject);
        }
    }

    void Start()
    {
        if (!singleton) singleton = this;
        warehouse = Player.localPlayer.playerMove.fornitureClient.GetComponent<ModularPersonalWareHouse>();
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
            player.CmdSwapInventoryModularWarehouse(selectedWarehouseIndex.ToArray(), selectedInventoryIndex.ToArray());
            selectedWarehouseIndex.Clear();
            selectedInventoryIndex.Clear();
        });

        //if (!player.playerMove.fornitureClient) return;
        //if (!player.playerMove.fornitureClient.GetComponent<ModularPersonalWareHouse>()) return;
        //else warehouse = player.playerMove.fornitureClient.GetComponent<ModularPersonalWareHouse>();

        UIUtils.BalancePrefabs(objectToSpawn, player.inventory.Count, inventoryContainer);
        for (int i = 0; i < player.inventory.Count; i++)
        {
            UIInventorySlot slot = inventoryContainer.GetChild(i).GetComponent<UIInventorySlot>();
            slot.dragAndDropable.name = i.ToString();
            ItemSlot itemSlot = player.inventory[i];

            if (itemSlot.amount > 0)
            {

                slot.button.interactable = itemSlot.item.data.canUseWarehouse;
                int icopy = i;
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
                slot.tooltip.enabled = true;
                if (slot.tooltip.IsVisible())
                    slot.tooltip.text = itemSlot.ToolTip();
                slot.image.color = Color.white;
                slot.image.sprite = itemSlot.item.image;
                if (itemSlot.item.data is UsableItem usable2)
                {
                    float cooldown = player.GetItemCooldown(usable2.cooldownCategory);
                    slot.cooldownCircle.fillAmount = usable2.cooldown > 0 ? cooldown / usable2.cooldown : 0;
                }
                else slot.cooldownCircle.fillAmount = 0;
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

        UIUtils.BalancePrefabs(objectToSpawn, warehouse.inventory.Count, warehouseContainer);

        for (int a = 0; a < warehouseContainer.childCount; a++)
        {
            UIInventorySlot slot2 = warehouseContainer.GetChild(a).GetComponent<UIInventorySlot>();
            slot2.dragAndDropable.name = a.ToString();
            ItemSlot itemSlot2 = new ItemSlot();

            itemSlot2 = warehouse.inventory[a];

            if (itemSlot2.amount > 0)
            {
                int icopy = a;
                slot2.button.onClick.SetListener(() =>
                {
                    if (!selectedWarehouseIndex.Contains(icopy))
                    {
                        selectedWarehouseIndex.Add(icopy);
                    }
                    else
                    {
                        selectedWarehouseIndex.Remove(icopy);
                    }

                });
                slot2.tooltip.enabled = true;
                if (slot2.tooltip.IsVisible())
                    slot2.tooltip.text = itemSlot2.ToolTip();
                slot2.image.color = Color.white;
                slot2.image.sprite = itemSlot2.item.image;
                if (itemSlot2.item.data is UsableItem usable2)
                {
                    float cooldown = player.GetItemCooldown(usable2.cooldownCategory);
                    slot2.cooldownCircle.fillAmount = usable2.cooldown > 0 ? cooldown / usable2.cooldown : 0;
                }
                else slot2.cooldownCircle.fillAmount = 0;
                slot2.amountOverlay.SetActive(itemSlot2.amount > 1);
                slot2.amountText.text = itemSlot2.amount.ToString();
                slot2.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                slot2.GetComponent<Image>().color = GffItemRarity.singleton.rarityColor(true, itemSlot2.item);
            }
            else
            {
                slot2.button.onClick.RemoveAllListeners();
                slot2.tooltip.enabled = false;
                slot2.dragAndDropable.dragable = false;
                slot2.image.color = Color.clear;
                slot2.image.sprite = null;
                slot2.cooldownCircle.fillAmount = 0;
                slot2.amountOverlay.SetActive(false);
                slot2.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                slot2.GetComponent<Image>().color = GffItemRarity.singleton.ColorNull;
            }

            for (int e = 0; e < warehouse.inventory.Count; e++)
            {
                int index = e;
                if (selectedWarehouseIndex.Contains(index)) warehouseContainer.GetChild(index).GetComponent<Outline>().enabled = true;
                else warehouseContainer.GetChild(index).GetComponent<Outline>().enabled = false;
            }
        }
    }
}
