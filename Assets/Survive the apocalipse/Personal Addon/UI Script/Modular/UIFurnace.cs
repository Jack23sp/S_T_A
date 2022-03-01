using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class UIFurnace : NetworkBehaviour
{
    public Button onButton;
    public TextMeshProUGUI onButtonText;
    public Transform content;
    public Transform workingContent;
    public GameObject slotToInstantiate;
    public GameObject slotToInstantiateInFurnace;

    public Furnace furnace;

    private Player player;

    public Button closeButton;

    List<int> freeSlot = new List<int>();
    List<int> rockSlot = new List<int>();
    List<int> sulfurSlot = new List<int>();
    List<int> highMetalSlot = new List<int>();

    void Start()
    {
        furnace = Player.localPlayer.target.GetComponent<Furnace>();
        player = Player.localPlayer;

        if(furnace && furnace.isServer)
            Invoke(nameof(Cook), 10.0f);
    }

    public void Cook()
    {
        freeSlot.Clear();
        rockSlot.Clear();
        sulfurSlot.Clear();
        highMetalSlot.Clear();

        if (furnace.isActive)
        {
            if(furnace.inventory[0].amount > 0)
            {
                for(int i = 1; i < furnace.inventory.Count; i++)
                {
                    int index = i;
                    if(furnace.inventory[index].amount == 0)
                    {
                        if (!freeSlot.Contains(index)) freeSlot.Add(index);
                    }
                    else
                    {
                        if(furnace.inventory[index].item.data.name == "Rock")
                        {
                            if (!rockSlot.Contains(index)) rockSlot.Add(index);
                        }
                        if (furnace.inventory[index].item.data.name == "Sulfur")
                        {
                            if (!sulfurSlot.Contains(index)) sulfurSlot.Add(index);
                        }
                        if (furnace.inventory[index].item.data.name == "High quality metal")
                        {
                            if (!highMetalSlot.Contains(index)) highMetalSlot.Add(index);
                        }
                    }
                }

                if(rockSlot.Count > 0)
                {
                    int randomToAdd = UnityEngine.Random.Range(0, 5);
                    int randomType = UnityEngine.Random.Range(0, 2);

                    if (sulfurSlot.Count == highMetalSlot.Count)
                    {
                        if (randomType == 0)
                        {
                            if (furnace.InventoryCanAdd(new Item(GeneralManager.singleton.highMetal), randomToAdd))
                            {
                                furnace.InventoryAdd(new Item(GeneralManager.singleton.highMetal), randomToAdd);
                            }
                        }
                        else if(randomType == 1)
                        {
                            if (furnace.InventoryCanAdd(new Item(GeneralManager.singleton.sulfur), randomToAdd))
                            {
                                furnace.InventoryAdd(new Item(GeneralManager.singleton.sulfur), randomToAdd);
                            }
                        }
                    }
                    else
                    {
                        if (highMetalSlot.Count > sulfurSlot.Count)
                        {
                            if (furnace.InventoryCanAdd(new Item(GeneralManager.singleton.sulfur), randomToAdd))
                            {
                                furnace.InventoryAdd(new Item(GeneralManager.singleton.sulfur), randomToAdd);
                            }                        
                        }
                        else
                        {
                            if (furnace.InventoryCanAdd(new Item(GeneralManager.singleton.highMetal), randomToAdd))
                            {
                                furnace.InventoryAdd(new Item(GeneralManager.singleton.highMetal), randomToAdd);
                            }
                        }
                    }
                }
                ItemSlot wood = furnace.inventory[0];
                wood.amount -= 2;
                furnace.inventory[0] = wood;

                if (rockSlot.Count > 0)
                {
                    ItemSlot rock = furnace.inventory[rockSlot[0]];
                    rock.amount -= 2;
                    furnace.inventory[rockSlot[0]] = rock;
                }
            }
        }
        Invoke(nameof(Cook), 10.0f);
    }

    void Update()
    {
        if (player.health == 0) closeButton.onClick.Invoke();

        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

        if(GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
        {
            if (!furnace.isActive)
                onButtonText.text = "Avvia";
            else
                onButtonText.text = "Spegni";
        }
        else
        {
            if (!furnace.isActive)
                onButtonText.text = "On";
            else
                onButtonText.text = "Off";
        }

        onButton.onClick.SetListener(() =>
        {
            player.CmdManageFurnace();
        });

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
            if (furnace.inventory[index].amount > 0)
            {
                slot.image.color = Color.white;
                slot.image.sprite = furnace.inventory[index].item.image;
                slot.cooldownCircle.fillAmount = 0;
                slot.amountOverlay.SetActive(furnace.inventory[index].amount > 1);
                slot.amountText.text = furnace.inventory[index].amount.ToString();
                slot.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                slot.GetComponent<Image>().color = GffItemRarity.singleton.rarityColor(true, player.inventory[index].item);

                slot.button.interactable = player.InventoryCanAdd(furnace.inventory[index].item, furnace.inventory[index].amount);
                if (index == 0)
                {
                    slot.button.onClick.SetListener(() =>
                    {
                        player.CmdAddWoodToInventoryFromFurnace();
                    });
                }
                else
                {
                    slot.button.onClick.SetListener(() =>
                    {
                        player.CmdAddWToInventoryFromFurnace(index);
                    });                    
                }
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

        onButton.interactable = furnace.inventory[0].amount > 0;
    }
}
