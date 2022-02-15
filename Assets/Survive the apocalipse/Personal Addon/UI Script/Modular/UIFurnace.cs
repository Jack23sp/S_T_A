using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UIFurnace : NetworkBehaviour
{
    public Button onButton;
    public Transform content;
    public Transform workingContent;
    public GameObject slotToInstantiate;
    public GameObject slotToInstantiateInFurnace;

    public Furnace furnace;

    private Player player;

    void Start()
    {
        furnace = Player.localPlayer.playerMove.fornitureClient.GetComponent<Furnace>();
        player = Player.localPlayer;
    }

    void Update()
    {
        UIUtils.BalancePrefabs(slotToInstantiate, player.inventory.Count, content);
        for (int i = 0; i < player.inventory.Count; i++)
        {
            int index = i;
            UIInventorySlot slot = content.GetChild(index).GetComponent<UIInventorySlot>();
            if (player.inventory[index].amount > 0)
            {
                slot.image.color = Color.white;
                slot.image.sprite = player.inventory[index].item.image;
                slot.cooldownCircle.fillAmount = 0;
                slot.amountOverlay.SetActive(player.inventory[index].amount > 1);
                slot.amountText.text = player.inventory[index].amount.ToString();
                slot.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                slot.GetComponent<Image>().color = GffItemRarity.singleton.rarityColor(true, player.inventory[index].item);

                if (!player.inventory[index].item.data.canUseFurnace)
                {
                    slot.button.interactable = false;
                }
                else
                {
                    slot.button.interactable = true;
                }
                slot.button.onClick.SetListener(() =>
                {
                    if(player.inventory[index].item.data.name == "Wood")
                    {
                        player.CmdInsertWood(index);
                    }
                    else
                    {
                        player.CmdInsertObjectInFurnace(index);
                    }
                });
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

        UIUtils.BalancePrefabs(slotToInstantiateInFurnace, 7, workingContent);
        for (int i = 0; i < 7; i++)
        {
            int index = i;
            UIInventorySlot slot = workingContent.GetChild(index).GetComponent<UIInventorySlot>();
            if (furnace.furnaceSlot[index].amount > 0)
            {
                slot.image.color = Color.white;
                slot.image.sprite = furnace.furnaceSlot[index].item.image;
                slot.cooldownCircle.fillAmount = 0;
                slot.amountOverlay.SetActive(furnace.furnaceSlot[index].amount > 1);
                slot.amountText.text = furnace.furnaceSlot[index].amount.ToString();
                slot.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                slot.GetComponent<Image>().color = GffItemRarity.singleton.rarityColor(true, player.inventory[index].item);

                slot.button.onClick.SetListener(() =>
                {

                });
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

        onButton.interactable = furnace.furnaceSlot[0].amount > 0;
    }
}
