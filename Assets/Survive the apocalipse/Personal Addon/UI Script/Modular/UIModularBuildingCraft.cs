using CustomType;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIModularBuildingCraft : MonoBehaviour
{
    public GameObject itemToCraft;
    public GameObject itemIngredient;

    public Transform itemToCraftContent;
    public Transform ingredientContent;

    public Button closeButton;
    public Button changePanel;

    public Button gemsButton;
    public GameObject gemsOverlay;
    public TextMeshProUGUI gemsText;
    public TextMeshProUGUI gemsOverlayText;

    public TextMeshProUGUI description;

    public GameObject inCraftingItem;

    private Player player;

    public BuildingModularCrafting buildingTarget;

    public List<int> progressItem = new List<int>();
    public List<int> finishedItem = new List<int>();
    private TimeSpan difference;

    public bool canCraft;
    private int selectedIndex;

    private CraftItem runtimeItem;

    public Transform progressItemContent;
    public Transform finishedItemContent;

    private ItemInBuilding selectedBuilding;
    private ItemInBuilding mainItem;

    public GameObject upgradePanel;


    public void Start()
    {
        buildingTarget = Player.localPlayer.playerMove.fornitureClient.GetComponent<BuildingModularCrafting>();
        gemsText.text = GeneralManager.singleton.languagesManager.defaultLanguages == "Italian" ? "Crea" : "Craft";
    }

    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;
        if (player.health == 0)
            Destroy(this.gameObject);
        if (!player.playerMove.fornitureClient) return;

        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

        if (changePanel)
        {
            changePanel.onClick.SetListener(() =>
            {
                Instantiate(upgradePanel, GeneralManager.singleton.canvas);
                Destroy(this.gameObject);
            });
        }

        UIUtils.BalancePrefabs(itemToCraft, GeneralManager.singleton.FindItemToCraft(buildingTarget.building), itemToCraftContent);
        for (int i = 0; i < itemToCraftContent.childCount; i++)
        {
            int index = i;
            SlotIngredient slot = itemToCraftContent.GetChild(index).GetComponent<SlotIngredient>();
            mainItem = GeneralManager.singleton.buildingItems[GeneralManager.singleton.ListOfBuildingItem(buildingTarget.building)].buildingItem[index];
            if (mainItem.itemToCraft.item.image)
            {
                slot.image.sprite = mainItem.itemToCraft.item.image;
            }
            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                slot.ingredientName.text = mainItem.itemToCraft.item.italianName;
                slot.ingredientAmount.text = " x " + mainItem.itemToCraft.amount;
            }
            else
            {
                slot.ingredientName.text = mainItem.itemToCraft.item.name;
                slot.ingredientAmount.text = " x " + mainItem.itemToCraft.amount;

            }
            slot.slotButton.onClick.SetListener(() =>
            {
                selectedBuilding = GeneralManager.singleton.buildingItems[GeneralManager.singleton.ListOfBuildingItem(buildingTarget.building)].buildingItem[index];

                selectedIndex = index;
                description.text = new Item(selectedBuilding.itemToCraft.item).ToolTip().Replace("{AMOUNT}","");

                UIUtils.BalancePrefabs(itemIngredient, selectedBuilding.craftablengredient.Count, ingredientContent);
                {
                    canCraft = true;
                    for (int e = 0; e < selectedBuilding.craftablengredient.Count; e++)
                    {
                        int secondindex = e;
                        ModularCraftingSlot ingredientSlot = ingredientContent.GetChild(secondindex).GetComponent<ModularCraftingSlot>();
                        ingredientSlot.image.sprite = selectedBuilding.craftablengredient[secondindex].item.image;
                        int invCount = player.InventoryCount(new Item(selectedBuilding.craftablengredient[secondindex].item));
                        ingredientSlot.xButton.gameObject.SetActive(false);
                        ingredientSlot.amountContainer.SetActive(selectedBuilding.craftablengredient[secondindex].amount > 0);
                        ingredientSlot.progressBar.fillAmount = 0;

                        ingredientSlot.amountText.text = selectedBuilding.craftablengredient[secondindex].amount.ToString();
                        ingredientSlot.amountText.color = invCount < selectedBuilding.craftablengredient[secondindex].amount ? Color.red : Color.white;
                        if (invCount < selectedBuilding.craftablengredient[secondindex].amount)
                            canCraft = false;
                    }
                }
                gemsText.text = GeneralManager.singleton.languagesManager.defaultLanguages == "Italian" ? "Crea" : "Craft";
                gemsOverlayText.text = selectedBuilding.itemToCraft.item.coinPrice.ToString();

                gemsOverlay.gameObject.SetActive(!canCraft);
                gemsButton.onClick.SetListener(() =>
                {
                    DateTime time = DateTime.Now;

                    if (canCraft)
                    {
                        player.playerBuilding.CmdCraftItemForniture(buildingTarget.building.name, selectedIndex, 1, time.ToString());
                        itemToCraftContent.GetChild(selectedIndex).GetComponent<Button>().onClick.Invoke();
                    }
                    else
                    {
                        player.playerBuilding.CmdCraftItemForniture(buildingTarget.building.name, selectedIndex, -1, time.ToString());
                        itemToCraftContent.GetChild(selectedIndex).GetComponent<Button>().onClick.Invoke();
                    }
                });

            });


        }

        progressItem = buildingTarget.craftItem.Select((x, index) => index)
            .Where(x => (DateTime.Parse(buildingTarget.craftItem[x].timeEnd) - DateTime.Now).TotalSeconds > 0).ToList();
        finishedItem = buildingTarget.craftItem.Select((x, index) => index)
            .Where(x => (DateTime.Parse(buildingTarget.craftItem[x].timeEnd) - DateTime.Now).TotalSeconds <= 0).ToList();

        UIUtils.BalancePrefabs(inCraftingItem, progressItem.Count, progressItemContent);
        for (int i = 0; i < progressItem.Count; i++)
        {
            int index = i;
            if (index >= progressItem.Count) return;
            ModularCraftingSlot slot = progressItemContent.GetChild(index).GetComponent<ModularCraftingSlot>();
            slot.itemName = buildingTarget.craftItem[progressItem[index]].itemName;

            if (ScriptableItem.dict.TryGetValue(slot.itemName.GetStableHashCode(), out ScriptableItem itemData))
            {
                slot.image.sprite = itemData.image;
            }
            TimeSpan initialDifference = DateTime.Parse(buildingTarget.craftItem[progressItem[index]].timeEnd) - DateTime.Parse(buildingTarget.craftItem[progressItem[index]].timeBegin);
            TimeSpan difference = DateTime.Parse(buildingTarget.craftItem[progressItem[index]].timeEnd) - System.DateTime.Now;
            slot.progressBar.fillAmount = 1 - (1 - (Convert.ToSingle(difference.TotalSeconds / initialDifference.TotalSeconds)));
            slot.index = progressItem[index];
            slot.xButton.gameObject.SetActive(initialDifference.TotalSeconds > 0);
            slot.selectButton.onClick.SetListener(() =>
            {
                player.playerBuilding.CmdRemoveItemFromCrafting(slot.index, buildingTarget.netIdentity);
                //ManageList();
            });
            slot.amountContainer.SetActive(buildingTarget.craftItem[progressItem[index]].amount > 0);
            slot.amountText.text = buildingTarget.craftItem[progressItem[index]].amount.ToString();
            //if (difference.TotalSeconds <= 0)
            //{
            //    if (!finishedItem.Contains(progressItem[index])) finishedItem.Add(progressItem[index]);
            //    if (progressItem.Contains(progressItem[index])) progressItem.Remove(progressItem[index]);
            //}
        }

        UIUtils.BalancePrefabs(inCraftingItem, finishedItem.Count, finishedItemContent);
        for (int i = 0; i < finishedItem.Count; i++)
        {
            int index = i;
            if (index >= finishedItem.Count) return;
            ModularCraftingSlot slot = finishedItemContent.GetChild(index).GetComponent<ModularCraftingSlot>();

            slot.itemName = buildingTarget.craftItem[finishedItem[index]].itemName;

            if (ScriptableItem.dict.TryGetValue(slot.itemName.GetStableHashCode(), out ScriptableItem itemData))
            {
                slot.image.sprite = itemData.image;
            }
            slot.progressBar.fillAmount = 0;
            slot.index = finishedItem[index];
            slot.xButton.gameObject.SetActive(false);
            slot.selectButton.onClick.SetListener(() =>
            {
                player.CmdAddToInventoryFromForniture(buildingTarget.craftItem[finishedItem[index]].itemName, buildingTarget.craftItem[finishedItem[index]].amount, buildingTarget.craftItem[finishedItem[index]].owner, buildingTarget.craftItem[finishedItem[index]].timeEnd, finishedItem[index]);
                //if (finishedItem.Contains(finishedItem[index])) finishedItem.Remove(finishedItem[index]);
            });

            slot.amountContainer.SetActive(buildingTarget.craftItem[finishedItem[index]].amount > 0);
            slot.amountText.text = buildingTarget.craftItem[finishedItem[index]].amount.ToString();
        }


    }

    public void ManageList()
    {
        for (int i = 0; i < buildingTarget.craftItem.Count; i++)
        {
            int index = i;
            difference = DateTime.Parse(buildingTarget.craftItem[index].timeEnd) - DateTime.Now;

            if (difference.TotalSeconds > 0)
            {
                if (finishedItem.Contains(index)) finishedItem.Remove(index);
                if (!progressItem.Contains(index)) progressItem.Add(index);
            }
            else
            {
                if (progressItem.Contains(index)) progressItem.Remove(index);
                if (!finishedItem.Contains(index)) finishedItem.Add(index);
            }
        }

    }
}
