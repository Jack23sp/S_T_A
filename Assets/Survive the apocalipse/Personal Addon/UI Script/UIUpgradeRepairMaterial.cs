using CustomType;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIUpgradeRepairMaterial : MonoBehaviour
{
    public static UIUpgradeRepairMaterial singleton;
    public Transform content;
    public GameObject objectToSpawn;

    public Button coinButton;
    public Button goldButton;

    public Player player;
    public BuildingUpgradeRepair buildingUpgradeRepair;

    public int selectedItem;
    public ItemSlot selectedItemOfInventory;

    public Button closeButton;

    public UpgradeRepairItem upgradeItem;
    public string operationType;

    public bool upgrade;

    public bool canUpgrade = true;
    public bool canRepair = true;

    void Start()
    {
        if (!singleton) singleton = this;
    }

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

        if (!buildingUpgradeRepair) buildingUpgradeRepair = player.playerMove.fornitureClient.GetComponent<BuildingUpgradeRepair>();
        if (!buildingUpgradeRepair) return;

        if (selectedItem != -1)
        {
            selectedItemOfInventory = player.inventory[selectedItem];
        }
        if (selectedItemOfInventory.amount == 0) Destroy(this.gameObject);


        if (upgrade)
        {
            UIUtils.BalancePrefabs(objectToSpawn, selectedItemOfInventory.item.data.upgradeItems.Count, content);
            for (int i = 0; i < selectedItemOfInventory.item.data.upgradeItems.Count; i++)
            {
                int index = i;
                UIUpgradeSlot slot = content.GetChild(index).GetComponent<UIUpgradeSlot>();
                slot.image.sprite = selectedItemOfInventory.item.data.upgradeItems[index].items.image;
                slot.itemText.text = selectedItemOfInventory.item.data.upgradeItems[index].items.name;
                slot.itemLevel.text = player.InventoryCount(new Item(selectedItemOfInventory.item.data.upgradeItems[index].items)) + " / " + selectedItemOfInventory.item.data.upgradeItems[index].amount;
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    slot.itemButton.GetComponentInChildren<TextMeshProUGUI>().text = "Compra!";
                    slot.itemText.text = selectedItemOfInventory.item.data.upgradeItems[index].items.italianName;
                }
                else
                {
                    slot.itemButton.GetComponentInChildren<TextMeshProUGUI>().text = "Buy!";
                    slot.itemText.text = selectedItemOfInventory.item.data.upgradeItems[index].items.name;
                }
                if (slot.itemButton.gameObject.activeInHierarchy)
                {

                    slot.itemButton.onClick.SetListener(() =>
                    {
                        GameObject g = Instantiate(GeneralManager.singleton.upgradebuyMaterial, GeneralManager.singleton.canvas);
                        g.GetComponent<UIBuyUpgradeItem>().item = new Item(selectedItemOfInventory.item.data.upgradeItems[index].items);

                    });
                }
            }

            for (int i = 0; i < selectedItemOfInventory.item.data.upgradeItems.Count; i++)
            {
                if (player.InventoryCount(new Item(selectedItemOfInventory.item.data.upgradeItems[i].items)) < selectedItemOfInventory.item.data.upgradeItems[i].amount)
                {
                    canUpgrade = false;
                }
            }
            if (canUpgrade)
            {
                if (player.gold < selectedItemOfInventory.item.data.goldsToUpgrade)
                {
                    goldButton.interactable = false;
                }
            }
            goldButton.GetComponentInChildren<TextMeshProUGUI>().text = selectedItemOfInventory.item.data.goldsToUpgrade.ToString();
            goldButton.onClick.SetListener(() =>
            {
                if (Player.localPlayer.playerMove.forniture && Player.localPlayer.playerMove.forniture.GetComponent<BuildingUpgradeRepair>())
                {
                    Player.localPlayer.CmdUpgradeItem(upgradeItem, upgradeItem.typeOfUpgrade, 0, System.DateTime.Now.ToString(), System.DateTime.Now.AddSeconds(upgradeItem.totalTime).ToString(), selectedItem);
                    UIUpgradeRepair.singleton.CleanList();
                    closeButton.onClick.Invoke();

                }
            });
            if (canUpgrade)
            {
                if (player.coins < selectedItemOfInventory.item.data.coinsToUpgrade)
                {
                    coinButton.interactable = false;
                }
            }

            coinButton.GetComponentInChildren<TextMeshProUGUI>().text = selectedItemOfInventory.item.data.coinsToUpgrade.ToString();
            coinButton.onClick.SetListener(() =>
            {
                if (Player.localPlayer.playerMove.forniture && Player.localPlayer.playerMove.forniture.GetComponent<BuildingUpgradeRepair>())
                {
                    Player.localPlayer.CmdUpgradeItem(upgradeItem, operationType, 1, System.DateTime.Now.ToString(), System.DateTime.Now.AddSeconds(Convert.ToInt32(upgradeItem.totalTime /2)).ToString(), selectedItem);
                    UIUpgradeRepair.singleton.CleanList();
                    closeButton.onClick.Invoke();
                }
            });
        }
        else
        {
            UIUtils.BalancePrefabs(objectToSpawn, selectedItemOfInventory.item.data.repairItems.Count, content);
            for (int i = 0; i < selectedItemOfInventory.item.data.repairItems.Count; i++)
            {
                int index = i;
                UIUpgradeSlot slot = content.GetChild(index).GetComponent<UIUpgradeSlot>();
                slot.image.sprite = selectedItemOfInventory.item.data.repairItems[index].items.image;
                slot.itemText.text = selectedItemOfInventory.item.data.repairItems[index].items.name;
                slot.itemLevel.text = player.InventoryCount(new Item(selectedItemOfInventory.item.data.repairItems[index].items)) + " / " + selectedItemOfInventory.item.data.repairItems[index].amount;
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    slot.itemButton.GetComponentInChildren<TextMeshProUGUI>().text = "Compra!";
                    slot.itemText.text = selectedItemOfInventory.item.data.repairItems[index].items.italianName;
                }
                else
                {
                    slot.itemButton.GetComponentInChildren<TextMeshProUGUI>().text = "Buy!";
                    slot.itemText.text = selectedItemOfInventory.item.data.repairItems[index].items.name;
                }
                if (slot.itemButton.gameObject.activeInHierarchy)
                {
                    slot.itemButton.onClick.SetListener(() =>
                    {
                        GameObject g = Instantiate(GeneralManager.singleton.upgradebuyMaterial, GeneralManager.singleton.canvas);
                        g.GetComponent<UIBuyUpgradeItem>().item = new Item(selectedItemOfInventory.item.data.repairItems[index].items);

                    });
                }
            }
            for (int i = 0; i < selectedItemOfInventory.item.data.repairItems.Count; i++)
            {
                if (player.InventoryCount(new Item(selectedItemOfInventory.item.data.repairItems[i].items)) < selectedItemOfInventory.item.data.repairItems[i].amount)
                {
                    canRepair = false;
                }
            }
            if (canRepair)
            {
                if (player.gold < selectedItemOfInventory.item.data.goldsToRepair)
                {
                    goldButton.interactable = false;
                }
            }
            goldButton.GetComponentInChildren<TextMeshProUGUI>().text = selectedItemOfInventory.item.data.goldsToRepair.ToString();
            goldButton.onClick.SetListener(() =>
            {
                if (Player.localPlayer.playerMove.forniture && Player.localPlayer.playerMove.forniture.GetComponent<BuildingUpgradeRepair>())
                {
                    Player.localPlayer.CmdRepairItem(upgradeItem, 0, System.DateTime.Now.ToString(), System.DateTime.Now.AddSeconds(Convert.ToInt32(upgradeItem.totalTime)).ToString());
                    UIUpgradeRepair.singleton.CleanList();
                    closeButton.onClick.Invoke();

                }
            });
            if (canRepair)
            {
                if (player.coins < selectedItemOfInventory.item.data.coinsToRepair)
                {
                    coinButton.interactable = false;
                }
            }

            coinButton.GetComponentInChildren<TextMeshProUGUI>().text = selectedItemOfInventory.item.data.coinsToRepair.ToString();
            coinButton.onClick.SetListener(() =>
            {
                if (Player.localPlayer.playerMove.forniture && Player.localPlayer.playerMove.forniture.GetComponent<BuildingUpgradeRepair>())
                {
                    Player.localPlayer.CmdRepairItem(upgradeItem, 1, System.DateTime.Now.ToString(), System.DateTime.Now.AddSeconds(Convert.ToInt32(upgradeItem.totalTime / 2)).ToString());
                    UIUpgradeRepair.singleton.CleanList();
                    closeButton.onClick.Invoke();

                }
            });
        }


    }
}
