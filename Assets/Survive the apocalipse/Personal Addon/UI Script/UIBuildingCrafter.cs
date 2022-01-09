using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class UIBuildingCrafter : MonoBehaviour
{
    public GameObject itemToCraft;
    public GameObject itemIngredient;

    public Transform itemToCraftContent;
    public Transform ingredientContent;

    public Button craftGold;
    public Button craftCoins;
    public Button closeButton;
    public Button alreadyInCraftButton;

    public TextMeshProUGUI description;


    private Player player;

    public ScriptableItem selectedItem;

    public bool canCraft;
    private int selectedIndex;

    void Update()
    {
        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

        if (!player) player = Player.localPlayer;
        if (!player) return;
        if (player.health == 0)
            closeButton.onClick.Invoke();

        alreadyInCraftButton.onClick.SetListener(() =>
        {
            Instantiate(GeneralManager.singleton.alreadyCraftPanel, GeneralManager.singleton.canvas);
        });

        craftGold.interactable = canCraft;
        craftGold.onClick.SetListener(() =>
        {
            player.playerBuilding.CmdCraftBuildingItem(selectedItem.name, 0);
            itemToCraftContent.GetChild(selectedIndex).GetComponent<Button>().onClick.Invoke();
        });

        craftCoins.interactable = canCraft;
        craftCoins.onClick.SetListener(() =>
        {
            player.playerBuilding.CmdCraftBuildingItem(selectedItem.name, 1);
            itemToCraftContent.GetChild(selectedIndex).GetComponent<Button>().onClick.Invoke();
        });


        craftGold.gameObject.SetActive(selectedItem);
        craftCoins.gameObject.SetActive(selectedItem);

        UIUtils.BalancePrefabs(itemToCraft, GeneralManager.singleton.buildingItems[0].buildingItem.Count, itemToCraftContent);
        for (int i = 0; i < itemToCraftContent.childCount; i++)
        {
            int index = i;
            SlotIngredient slot = itemToCraftContent.GetChild(index).GetComponent<SlotIngredient>();
            slot.image.sprite = GeneralManager.singleton.buildingItems[0].buildingItem[index].itemToCraft.item.image;
            if(GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                slot.ingredientName.text = GeneralManager.singleton.buildingItems[0].buildingItem[index].itemToCraft.item.italianName;
            }
            else
            {
                slot.ingredientName.text = GeneralManager.singleton.buildingItems[0].buildingItem[index].itemToCraft.item.name;
            }
            slot.ingredientAmount.text = " x " + GeneralManager.singleton.buildingItems[0].buildingItem[index].itemToCraft.amount;
            slot.slotButton.onClick.SetListener(() =>
            {
                selectedIndex = index;
                selectedItem = GeneralManager.singleton.buildingItems[0].buildingItem[index].itemToCraft.item;
                description.text = string.Empty;
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    description.text += GeneralManager.singleton.buildingItems[0].buildingItem[index].itemToCraft.item.italianName + "\n";
                    description.text += "Quantita' : " + GeneralManager.singleton.buildingItems[0].buildingItem[index].itemToCraft.amount + "\n";

                }
                else
                {
                    description.text += GeneralManager.singleton.buildingItems[0].buildingItem[index].itemToCraft.item.name + "\n";
                    description.text += "Amount : " + GeneralManager.singleton.buildingItems[0].buildingItem[index].itemToCraft.amount + "\n";
                }
                craftCoins.GetComponentInChildren<TextMeshProUGUI>().text = selectedItem.coinPrice.ToString();
                craftGold.GetComponentInChildren<TextMeshProUGUI>().text = selectedItem.goldPrice.ToString();

                UIUtils.BalancePrefabs(itemIngredient, GeneralManager.singleton.buildingItems[0].buildingItem[index].craftablengredient.Count, ingredientContent);
                {
                    canCraft = true;
                    for (int e = 0; e < ingredientContent.childCount; e++)
                    {
                        int secondindex = e;
                        SlotIngredient ingredientSlot = ingredientContent.GetChild(secondindex).GetComponent<SlotIngredient>();
                        ingredientSlot.image.sprite = GeneralManager.singleton.buildingItems[0].buildingItem[index].craftablengredient[secondindex].item.image;
                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                        {
                            ingredientSlot.ingredientName.text = GeneralManager.singleton.buildingItems[0].buildingItem[index].craftablengredient[secondindex].item.italianName;
                        }
                        else
                        {
                            ingredientSlot.ingredientName.text = GeneralManager.singleton.buildingItems[0].buildingItem[index].craftablengredient[secondindex].item.name;
                        }
                        int invCount = player.InventoryCount(new Item(GeneralManager.singleton.buildingItems[0].buildingItem[index].craftablengredient[secondindex].item));
                        ingredientSlot.ingredientAmount.text = invCount + " / " + GeneralManager.singleton.buildingItems[0].buildingItem[index].craftablengredient[secondindex].amount.ToString();
                        if (invCount < GeneralManager.singleton.buildingItems[0].buildingItem[index].craftablengredient[secondindex].amount)
                            canCraft = false;
                    }
                }
            });
        }

    }
}