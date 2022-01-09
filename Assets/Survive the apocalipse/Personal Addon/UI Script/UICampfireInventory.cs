using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICampfireInventory : MonoBehaviour
{
    private Player player;

    public GameObject cookSlot;
    public Transform inventoryContent;
    public Transform actuallyCookedContent;

    public Campfire campfire;

    public List<int> inventoryWoodIndex = new List<int>();

    public Button closeButtton;


    void Update()
    {
        closeButtton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

        if (!player) player = Player.localPlayer;
        if (!player) return;
        if (player.health == 0) closeButtton.onClick.Invoke();
        if (!player.target) return;
        campfire = player.target.GetComponent<Campfire>();
        if (!campfire) return;


        CheckWoodInInventory();

        UIUtils.BalancePrefabs(cookSlot, inventoryWoodIndex.Count, inventoryContent);
        for (int i = 0; i < inventoryWoodIndex.Count; i++)
        {
            int index = i;
            CookedSlot slot = inventoryContent.GetChild(index).GetComponent<CookedSlot>();
            slot.cookedImage.sprite = player.inventory[inventoryWoodIndex[index]].item.data.image;
            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                slot.cookedText.text = player.inventory[inventoryWoodIndex[index]].item.data.italianName;
            }
            else
            {
                slot.cookedText.text = player.inventory[inventoryWoodIndex[index]].item.name;
            }
            slot.progressSlider.gameObject.SetActive(false);
            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                slot.takeCookedButton.GetComponentInChildren<TextMeshProUGUI>().text = "Cuoci";
            }
            else
            {
                slot.takeCookedButton.GetComponentInChildren<TextMeshProUGUI>().text = "Cook";
            }
            slot.takeCookedButton.interactable = campfire.items.Count < GeneralManager.singleton.maxCookedAmount.Get(player.target.level);
            slot.takeCookedButton.onClick.SetListener(() =>
            {
                player.CmdAddCampfireItemToCook(inventoryWoodIndex[index]);
                inventoryWoodIndex.Remove(inventoryWoodIndex[index]);
            });
        }
        UIUtils.BalancePrefabs(cookSlot, campfire.items.Count, actuallyCookedContent);
        for (int i = 0; i < campfire.items.Count; i++)
        {
            int index = i;
            CookedSlot slot = actuallyCookedContent.GetChild(index).GetComponent<CookedSlot>();
            slot.cookedImage.sprite = campfire.items[index].item.data.image;
            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                slot.cookedText.text = campfire.items[index].item.data.italianName;
            }
            else
            {
                slot.cookedText.text = campfire.items[index].item.name;
            }
            slot.progressSlider.gameObject.SetActive(true);
            slot.progressSlider.value = campfire.CookPercent(campfire.items[index]);
            slot.progressSlider.GetComponentInChildren<Text>().text = Convert.ToInt32((campfire.CookPercent(campfire.items[index]) * 100)).ToString();
            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                slot.takeCookedButton.GetComponentInChildren<TextMeshProUGUI>().text = "Prendi";
            }
            else
            {
                slot.takeCookedButton.GetComponentInChildren<TextMeshProUGUI>().text = "Take";
            }
            
            slot.takeCookedButton.interactable = campfire.items[index].item.cookCountdown <= 0 && player.InventoryCanAdd(campfire.items[index].item,1);
            slot.takeCookedButton.onClick.SetListener(() =>
            {
                player.CmdTakeCampfireItems(index);
            });
        }
    }


    public void CheckWoodInInventory()
    {
        for(int i = 0; i < player.inventory.Count; i++)
        {
            int index = i;
            if(player.inventory[index].amount > 0 && player.inventory[index].item.data is FoodItem && ((FoodItem)player.inventory[index].item.data).cookedItem != null)
            {
                if (!inventoryWoodIndex.Contains(index))
                    inventoryWoodIndex.Add(index);
            }
        }

    }
}
