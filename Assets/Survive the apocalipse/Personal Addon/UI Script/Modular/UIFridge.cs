using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFridge : MonoBehaviour
{
    public static UIFridge singleton;
    public Transform fridgeContainer;
    public Transform inventoryContainer;

    public GameObject objectToSpawn;
    [HideInInspector] public Player player;

    public List<int> selectedInventoryIndex = new List<int>();
    public List<int> selectedFoodIndex = new List<int>();

    //public List<int> foodAmount = new List<int>();

    public Button CloseButton;
    public Button switchButton;

    public Fridge fridge;


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
            player.CmdSwitchInventoryFridge(selectedFoodIndex.ToArray(), selectedInventoryIndex.ToArray());
            selectedFoodIndex.Clear();
            selectedInventoryIndex.Clear();
        });

        if (!player.playerMove.fornitureClient) return;
        if (!player.playerMove.fornitureClient.GetComponent<Fridge>()) return;
        else fridge = player.playerMove.fornitureClient.GetComponent<Fridge>();

        //for (int i = 0; i < player.inventory.Count; i++)
        //{
        //    int index = i;
        //    if (player.inventory[index].amount > 0)
        //    {
        //        if (player.inventory[index].item.data is FoodItem)
        //        {
        //            if (((FoodItem)player.inventory[index].item.data).maxDurability.baseValue > 0 && player.inventory[index].item.durability > 0 || (((FoodItem)player.inventory[index].item.data).maxDurability.baseValue == 0))
        //            {
        //                if (!foodAmount.Contains(index))
        //                    foodAmount.Add(index);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (foodAmount.Contains(index))
        //            foodAmount.Remove(index);
        //    }
        //}


        UIUtils.BalancePrefabs(objectToSpawn, player.inventory.Count, inventoryContainer);
        for (int i = 0; i < player.inventory.Count; i++)
        {
            int icopy = i;
            UIInventorySlot slot = inventoryContainer.GetChild(icopy).GetComponent<UIInventorySlot>();
            ItemSlot itemSlot = player.inventory[icopy];

            if (itemSlot.amount > 0)
            {
                if (player.inventory[icopy].item.data is FoodItem || player.inventory[icopy].item.data is ScriptablePlant)
                {
                    slot.button.interactable = true;
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
        for (int e = 0; e < fridgeContainer.childCount; e++)
        {
            int index = e;
            if (selectedFoodIndex.Contains(index)) fridgeContainer.GetChild(index).GetComponent<Outline>().enabled = true;
            else fridgeContainer.GetChild(index).GetComponent<Outline>().enabled = false;
        }

        UIUtils.BalancePrefabs(objectToSpawn, fridge.maxSlotAmount, fridgeContainer);
        for (int a = 0; a < fridgeContainer.childCount; a++)
        {
            UIInventorySlot slot2 = fridgeContainer.GetChild(a).GetComponent<UIInventorySlot>();
            ItemSlot itemSlot2 = fridge.inventory[a];

            if (itemSlot2.amount > 0)
            {
                int icopy = a;
                slot2.button.onClick.SetListener(() =>
                {
                    if (!selectedFoodIndex.Contains(icopy))
                    {
                        selectedFoodIndex.Add(icopy);
                    }
                    else
                    {
                        selectedFoodIndex.Remove(icopy);
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
