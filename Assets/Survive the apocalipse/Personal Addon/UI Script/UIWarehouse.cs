using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIWarehouse : MonoBehaviour
{
    public static UIWarehouse singleton;
    public Transform warehouseContainer;
    public Transform inventoryContainer;

    public GameObject objectToSpawn;
    [HideInInspector] public Player player;

    public List<int> selectedInventoryIndex = new List<int>();
    public List<int> selectedWarehouseIndex = new List<int>();
    public int selectedWarehouseContainer;

    public Button containerOne;
    public Button containerTwo;
    public Button containerThree;
    public Button containerFour;
    public Button containerFive;
    public Button containerSix;

    public Button CloseButton;
    public Button switchButton;

    public Warehouse warehouse;

    public TextMeshProUGUI titleText;
    // Start is called before the first frame update
    
    public void Awake()
    {
        if(!Player.localPlayer || 
           !Player.localPlayer.target || 
           !Player.localPlayer.target.GetComponent<Warehouse>() || 
           (Player.localPlayer.target.GetComponent<Warehouse>().personal && Player.localPlayer.name != Player.localPlayer.target.GetComponent<Building>().owner) ||
           (!Player.localPlayer.target.GetComponent<Warehouse>().personal && !Player.localPlayer.CanInteractBuildingTarget(Player.localPlayer.target.GetComponent<Building>() , Player.localPlayer)))
        {
            Destroy(this.gameObject);
        }
    }
    
    void Start()
    {
        if (!singleton) singleton = this;
        selectedWarehouseContainer = 1;
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
            player.CmdSwitchInventoryWarehouse(selectedWarehouseIndex.ToArray(), selectedInventoryIndex.ToArray(), selectedWarehouseContainer);
            selectedWarehouseIndex.Clear();
            selectedInventoryIndex.Clear();
        });

        if (!player.target) return;
        if (!player.target.GetComponent<Warehouse>()) return;
        else warehouse = player.target.GetComponent<Warehouse>();

        if(warehouse.GetComponent<Building>().buildingName != string.Empty)
        {
            titleText.text = warehouse.GetComponent<Building>().buildingName;
        }
        else
        {
            titleText.text = warehouse.GetComponent<Building>().name;
        }


        containerOne.interactable = warehouse.GetComponent<Building>().level >= 1;
        containerTwo.interactable = warehouse.GetComponent<Building>().level >= 10;
        containerThree.interactable = warehouse.GetComponent<Building>().level >= 20;
        containerFour.interactable = warehouse.GetComponent<Building>().level >= 30;
        containerFive.interactable = warehouse.GetComponent<Building>().level >= 40;
        containerSix.interactable = warehouse.GetComponent<Building>().level == 50;
        containerOne.onClick.SetListener(() =>
        {
            selectedWarehouseContainer = 1;
            selectedWarehouseIndex.Clear();
            selectedInventoryIndex.Clear();
        });
        containerTwo.onClick.SetListener(() =>
        {
            selectedWarehouseContainer = 2;
            selectedWarehouseIndex.Clear();
            selectedInventoryIndex.Clear();
        });
        containerThree.onClick.SetListener(() =>
        {
            selectedWarehouseContainer = 3;
            selectedWarehouseIndex.Clear();
            selectedInventoryIndex.Clear();
        });
        containerFour.onClick.SetListener(() =>
        {
            selectedWarehouseContainer = 4;
            selectedWarehouseIndex.Clear();
            selectedInventoryIndex.Clear();
        });
        containerFive.onClick.SetListener(() =>
        {
            selectedWarehouseContainer = 5;
            selectedWarehouseIndex.Clear();
            selectedInventoryIndex.Clear();
        });
        containerSix.onClick.SetListener(() =>
        {
            selectedWarehouseContainer = 6;
            selectedWarehouseIndex.Clear();
            selectedInventoryIndex.Clear();
        });

        UIUtils.BalancePrefabs(objectToSpawn, player.inventory.Count, inventoryContainer);
        for (int i = 0; i < player.inventory.Count; i++)
        {
            UIInventorySlot slot = inventoryContainer.GetChild(i).GetComponent<UIInventorySlot>();
            slot.dragAndDropable.name = i.ToString();
            ItemSlot itemSlot = player.inventory[i];

            if (itemSlot.amount > 0)
            {
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
        if(selectedWarehouseContainer == 1)
        {
            UIUtils.BalancePrefabs(objectToSpawn, warehouse.one.Count, warehouseContainer);
        }
        if (selectedWarehouseContainer == 2)
        {
            UIUtils.BalancePrefabs(objectToSpawn, warehouse.two.Count, warehouseContainer);
        }
        if (selectedWarehouseContainer == 3)
        {
            UIUtils.BalancePrefabs(objectToSpawn, warehouse.three.Count, warehouseContainer);
        }
        if (selectedWarehouseContainer == 4)
        {
            UIUtils.BalancePrefabs(objectToSpawn, warehouse.four.Count, warehouseContainer);
        }
        if (selectedWarehouseContainer == 5)
        {
            UIUtils.BalancePrefabs(objectToSpawn, warehouse.five.Count, warehouseContainer);
        }
        if (selectedWarehouseContainer == 6)
        {
            UIUtils.BalancePrefabs(objectToSpawn, warehouse.six.Count, warehouseContainer);
        }
        for (int a = 0; a < warehouseContainer.childCount; a++)
        {
            UIInventorySlot slot2 = warehouseContainer.GetChild(a).GetComponent<UIInventorySlot>();
            slot2.dragAndDropable.name = a.ToString();
            ItemSlot itemSlot2 = new ItemSlot();
            
            if (selectedWarehouseContainer == 1)
            {
                itemSlot2 = warehouse.one[a];
            }
            if (selectedWarehouseContainer == 2)
            {
                itemSlot2 = warehouse.two[a];
            }
            if (selectedWarehouseContainer == 3)
            {
                itemSlot2 = warehouse.three[a];
            }
            if (selectedWarehouseContainer == 4)
            {
                itemSlot2 = warehouse.four[a];
            }
            if (selectedWarehouseContainer == 5)
            {
                itemSlot2 = warehouse.five[a];
            }
            if (selectedWarehouseContainer == 6)
            {
                itemSlot2 = warehouse.six[a];
            }

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
            if (selectedWarehouseContainer == 1)
            {
                for (int e = 0; e < warehouse.one.Count; e++)
                {
                    int index = e;
                    if (selectedWarehouseIndex.Contains(index)) warehouseContainer.GetChild(index).GetComponent<Outline>().enabled = true;
                    else warehouseContainer.GetChild(index).GetComponent<Outline>().enabled = false;
                }
            }
            if (selectedWarehouseContainer == 2)
            {
                for (int e = 0; e < warehouse.two.Count; e++)
                {
                    int index = e;
                    if (selectedWarehouseIndex.Contains(index)) warehouseContainer.GetChild(index).GetComponent<Outline>().enabled = true;
                    else warehouseContainer.GetChild(index).GetComponent<Outline>().enabled = false;
                }
            }
            if (selectedWarehouseContainer == 3)
            {
                for (int e = 0; e < warehouse.three.Count; e++)
                {
                    int index = e;
                    if (selectedWarehouseIndex.Contains(index)) warehouseContainer.GetChild(index).GetComponent<Outline>().enabled = true;
                    else warehouseContainer.GetChild(index).GetComponent<Outline>().enabled = false;
                }
            }
            if (selectedWarehouseContainer == 4)
            {
                for (int e = 0; e < warehouse.four.Count; e++)
                {
                    int index = e;
                    if (selectedWarehouseIndex.Contains(index)) warehouseContainer.GetChild(index).GetComponent<Outline>().enabled = true;
                    else warehouseContainer.GetChild(index).GetComponent<Outline>().enabled = false;
                }
            }
            if (selectedWarehouseContainer == 5)
            {
                for (int e = 0; e < warehouse.five.Count; e++)
                {
                    int index = e;
                    if (selectedWarehouseIndex.Contains(index)) warehouseContainer.GetChild(index).GetComponent<Outline>().enabled = true;
                    else warehouseContainer.GetChild(index).GetComponent<Outline>().enabled = false;
                }
            }
            if (selectedWarehouseContainer == 6)
            {
                for (int e = 0; e < warehouse.six.Count; e++)
                {
                    int index = e;
                    if (selectedWarehouseIndex.Contains(index)) warehouseContainer.GetChild(index).GetComponent<Outline>().enabled = true;
                    else warehouseContainer.GetChild(index).GetComponent<Outline>().enabled = false;
                }
            }
        }
    }
}
