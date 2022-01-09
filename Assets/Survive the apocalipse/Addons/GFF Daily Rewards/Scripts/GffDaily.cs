using CustomType;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GffDaily : MonoBehaviour
{
    [Header("Settings")]
    public bool openPanelThenRewardAvailable;
    public bool useTimeSpentInTheGame;
    public float timeInGame = 10f;
    public bool autoAddRewards;
    public bool autoOpenRewardsPanel;

    public List<Rewards> ListRewards = new List<Rewards>();

    [Header("Settings : addons")]
    public bool useRarityAddon;

    [Header("Colors")]
    public Color colorSelect;
    public Color colorAvailable;
    public Color colorNonSelect;

    [Header(" ")]
    public GameObject mainPanel;
    public Transform ContentDays;
    public Transform ContentRewards;

    public GameObject prefab;
    public TextMeshProUGUI textDaysValue;
    public TextMeshProUGUI textGoldValue;
    public TextMeshProUGUI textCoinsValue;

    public Button buttonClaim;

    public TextMeshProUGUI textinfo;

    public int selectedDay = -1;

    public int alreadySelected = -1;
    //singleton
    public static GffDaily singleton;
    public GffDaily() { singleton = this; }

    public Button closeButton;

    public ButtonAudioPlayer buttonAudio;
    void Update()
    {
        if (!Player.localPlayer || (Player.localPlayer && !Player.localPlayer.isLocalPlayer)) return;
        closeButton.onClick.SetListener(() =>
        {
            GetComponent<Image>().raycastTarget = false;
            transform.GetChild(0).gameObject.SetActive(false);
            //DestroyAfter dA = gameObject.AddComponent(typeof(DestroyAfter)) as DestroyAfter;
            //dA.time = 5;
            SoundManager.singleton.PlaySound(GetComponent<AudioSource>(), 1);
        });

        if (mainPanel.activeSelf)
        {
            transform.SetAsLastSibling();
            GetComponent<Image>().raycastTarget = true;
            Player player = Player.localPlayer;
            if (player != null)
            {
                ListRewards = GeneralManager.singleton.ListRewards;
                int amountDays = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

                //check all days
                for (int i = 0; i < ListRewards.Count; i++)
                {

                    if (i >= amountDays) ContentDays.GetChild(i).gameObject.SetActive(false);


                    //paint border color
                    if (selectedDay == i) ContentDays.GetChild(i).GetComponent<Image>().color = colorSelect;
                    else
                    {
                        int dayInList = player.FindDayInList(i + 1);
                        if (dayInList != -1 && player.dailyRewards[dayInList].get == false) ContentDays.GetChild(i).GetComponent<Image>().color = colorAvailable;
                        else ContentDays.GetChild(i).GetComponent<Image>().color = colorNonSelect;

                        //if the award has already been received
                        if (dayInList != -1 && player.dailyRewards[dayInList].get) ContentDays.GetChild(i).gameObject.transform.GetChild(1).gameObject.SetActive(true);
                    }

                    //button on day
                    int icopy = i;

                    if (alreadySelected == -1)
                    {
                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                        {
                            textDaysValue.text = "Ricompense per " + (selectedDay + 1) + " giorni di gioco";
                        }
                        else
                        {
                            textDaysValue.text = "Reward for " + (selectedDay + 1) + " day(s) in the game";
                        }
                        //items
                        if (ListRewards[selectedDay].reward.Length > 0)
                        {
                            UIUtils.BalancePrefabs(prefab.gameObject, ListRewards[selectedDay].reward.Length, ContentRewards);
                            for (int x = 0; x < ListRewards[selectedDay].reward.Length; ++x)
                            {
                                if (ListRewards[selectedDay].reward[x].item != null)
                                {
                                    GFFUniversalSlot slot = ContentRewards.GetChild(x).GetComponent<GFFUniversalSlot>();
                                    slot.dragAndDropable.dragable = false;
                                    slot.image.color = Color.white;
                                    slot.button.interactable = true;
                                    slot.image.sprite = ListRewards[selectedDay].reward[x].item.image;

                                    //amount
                                    int amount = ListRewards[selectedDay].reward[x].amount;
                                    slot.amountOverlay.SetActive(amount > 1);
                                    slot.amountText.text = amount.ToString();

                                    //toolTip
                                    slot.tooltip.enabled = true;
                                    slot.tooltip.text = new ItemSlot(new Item(ListRewards[selectedDay].reward[x].item), amount).ToolTip(); // ItemSlot so that {AMOUNT} is replaced too
                                    /*slot.tooltip.text = new ItemSlot(new Item(ListRewards[icopy].reward[x].item), amount,
                                        ListRewards[icopy].reward[x].item.durability,
                                        0, 0, new int[Upgrade.MaxUpgrade]).ToolTip(); // ItemSlot so that {AMOUNT} is replaced too*/

                                    //paint rarity color
                                    /*if (useRarityAddon && GffItemRarity.singleton != null)
                                    {
                                        slot.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                                        slot.GetComponent<Image>().color = GffItemRarity.singleton.ItemRarityList[ListRewards[icopy].reward[x].item.ItemRarity].color;
                                    }*/
                                }
                            }
                        }
                        else
                        {
                            UIUtils.BalancePrefabs(prefab.gameObject, 0, ContentRewards);
                        }
                        alreadySelected = 0;
                    }

                    ContentDays.GetChild(icopy).GetComponent<Button>().onClick.SetListener(() =>
                    {
                        selectedDay = icopy;
                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                        {
                            textDaysValue.text = "Ricompense per " + (selectedDay + 1) + " giorni di gioco";
                        }
                        else
                        {
                            textDaysValue.text = "Reward for " + (selectedDay + 1) + " day(s) in the game";
                        }

                        //items
                        if (ListRewards[icopy].reward.Length > 0)
                        {
                            UIUtils.BalancePrefabs(prefab.gameObject, ListRewards[icopy].reward.Length, ContentRewards);
                            for (int x = 0; x < ListRewards[icopy].reward.Length; ++x)
                            {
                                if (ListRewards[icopy].reward[x].item != null)
                                {
                                    GFFUniversalSlot slot = ContentRewards.GetChild(x).GetComponent<GFFUniversalSlot>();
                                    slot.dragAndDropable.dragable = false;
                                    slot.image.color = Color.white;
                                    slot.button.interactable = true;
                                    slot.image.sprite = ListRewards[icopy].reward[x].item.image;

                                    //amount
                                    int amount = ListRewards[icopy].reward[x].amount;
                                    slot.amountOverlay.SetActive(amount > 1);
                                    slot.amountText.text = amount.ToString();

                                    //toolTip
                                    slot.tooltip.enabled = true;
                                    slot.tooltip.text = new ItemSlot(new Item(ListRewards[icopy].reward[x].item), amount).ToolTip(); // ItemSlot so that {AMOUNT} is replaced too
                                    /*slot.tooltip.text = new ItemSlot(new Item(ListRewards[icopy].reward[x].item), amount,
                                        ListRewards[icopy].reward[x].item.durability,
                                        0, 0, new int[Upgrade.MaxUpgrade]).ToolTip(); // ItemSlot so that {AMOUNT} is replaced too*/

                                    //paint rarity color
                                    /*if (useRarityAddon && GffItemRarity.singleton != null)
                                    {
                                        slot.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                                        slot.GetComponent<Image>().color = GffItemRarity.singleton.ItemRarityList[ListRewards[icopy].reward[x].item.ItemRarity].color;
                                    }*/
                                }
                            }
                        }
                        else
                        {
                            UIUtils.BalancePrefabs(prefab.gameObject, 0, ContentRewards);
                        }

                        textGoldValue.text = ListRewards[icopy].gold.ToString();
                        textCoinsValue.text = ListRewards[icopy].coins.ToString();

                        //info
                        int dayInList = player.FindDayInList(icopy + 1);
                        if (dayInList != -1)
                        {
                            if (player.dailyRewards[dayInList].get == false)
                            {
                                buttonClaim.interactable = true;
                                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                                {
                                    textinfo.text = "Puoi prendere una ricompensa";
                                }
                                else
                                {
                                    textinfo.text = "You can get a reward";
                                }

                            }
                            else if (player.dailyRewards[dayInList].get == true)
                            {
                                buttonClaim.interactable = false;
                                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                                {
                                    textinfo.text = "Questa ricompensa è già stata ricevuta";
                                }
                                else
                                {
                                    textinfo.text = "The reward has already been received";
                                }
                            }
                        }
                        else
                        {
                            buttonClaim.interactable = false;
                            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            {
                                textinfo.text = "Non puoi ancora ricevere questa ricompensa";
                            }
                            else
                            {
                                textinfo.text = "The time of receipt has not yet come";
                            }
                        }
                    });
                }

                //button claim rewards
                buttonClaim.onClick.SetListener(() =>
                {
                    buttonClaim.interactable = false;
                    AddReward(player);
                    ContentDays.GetChild(selectedDay).gameObject.transform.GetChild(1).gameObject.SetActive(true);
                });
            }
        }
    }

    public void AddReward(Player player)
    {
        //if inventory is ok
        if (CheckInventoryFreeSpace(player, selectedDay))
        {
            //add gold
            player.CmdGoldOnCharacter(player.gold + ListRewards[selectedDay].gold);

            //add coins
            player.CmdCoinsOnCharacter(player.coins + ListRewards[selectedDay].coins);

            //add items
            for (int x = 0; x < ListRewards[selectedDay].reward.Length; ++x)
            {
                if (ListRewards[selectedDay].reward[x].item != null)
                {
                    ItemSlot temp = new ItemSlot();
                    temp.item = new Item(ListRewards[selectedDay].reward[x].item);
                    temp.amount = ListRewards[selectedDay].reward[x].amount;
                    player.CmdAddSlotToInventory(temp);
                }
            }

            //update state
            player.CmdUpdateDailyReward(player.FindDayInList(selectedDay + 1), true);
        }
        else
        {
            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                player.chat.AddMsgInfo("L'inventario e' pieno.");
            }
            else
            {
                player.chat.AddMsgInfo("Inventory is full.");
            }
        }
    }

    public void AutoOpenRewardPanel(Player player, int day)
    {
        if (!player.isLocalPlayer) return;
        selectedDay = day - 1;

        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
        {
            textDaysValue.text = "Ricompense per " + (day) + " giorni di gioco";
        }
        else
        {
            textDaysValue.text = "Reward for " + (day) + " day(s) in the game";
        }

        //items
        if (ListRewards[selectedDay].reward.Length > 0)
        {
            UIUtils.BalancePrefabs(prefab.gameObject, ListRewards[selectedDay].reward.Length, ContentRewards);
            for (int x = 0; x < ListRewards[selectedDay].reward.Length; ++x)
            {
                GFFUniversalSlot slot = ContentRewards.GetChild(x).GetComponent<GFFUniversalSlot>();

                if (ListRewards[selectedDay].reward[x].item != null)
                {
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.white;
                    slot.button.interactable = true;
                    slot.image.sprite = ListRewards[selectedDay].reward[x].item.image;

                    //amount
                    int amount = ListRewards[selectedDay].reward[x].amount;
                    slot.amountOverlay.SetActive(amount > 1);
                    slot.amountText.text = amount.ToString();

                    //ToolTip
                    slot.tooltip.enabled = true;
                    slot.tooltip.text = new ItemSlot(new Item(ListRewards[selectedDay].reward[x].item), 1).ToolTip(); // ItemSlot so that {AMOUNT} is replaced too
                    //slot.tooltip.text = new ItemSlot(new Item(ListRewards[selectedDay].reward[x].item), amount, ListRewards[selectedDay].reward[x].item.durability, 0, 0, new int[Upgrade.MaxUpgrade]).ToolTip(); // ItemSlot so that {AMOUNT} is replaced too

                    //paint rarity color
                    /*if (useRarityAddon && GffItemRarity.singleton != null)
                    {
                        slot.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                        slot.GetComponent<Image>().color = GffItemRarity.singleton.ItemRarityList[ListRewards[selectedDay].reward[x].item.ItemRarity].color;
                    }*/
                }
            }
        }

        textGoldValue.text = ListRewards[selectedDay].gold.ToString();
        textCoinsValue.text = ListRewards[selectedDay].coins.ToString();

        mainPanel.SetActive(true);
    }

    bool CheckInventoryFreeSpace(Player player, int day)
    {
        //check all rewards by select day
        for (int x = 0; x < ListRewards[day].reward.Length; ++x)
        {
            if (ListRewards[day].reward[x].item != null && !player.InventoryCanAdd(new Item(ListRewards[day].reward[x].item), ListRewards[day].reward[x].amount))
            {
                return false;
            }
        }
        return true;
    }
}