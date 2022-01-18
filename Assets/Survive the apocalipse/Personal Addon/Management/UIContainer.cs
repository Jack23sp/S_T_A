using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using CustomType;
using System;
using System.Linq;
using Michsky.UI.ModernUIPack;
using TMPro;
using AdvancedCustomizableSystem;

public partial class UITarget
{
    public GameObject panel;
    public Slider healthSlider;
    public Text nameText;
    public Transform buffsPanel;
    public UIBuffSlot buffSlotPrefab;
    public Button tradeButton;
    public Button guildInviteButton;
    public Button partyInviteButton;
    public Button allyInviteButton;
    public Button partnerInviteButton;
    public Button friendInviteButton;

    void Update()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            Entity target = player.target;
            if (target != null && target != player)
            {
                float distance = Utils.ClosestDistance(player.collider, target.collider);

                // name and health
                panel.SetActive(true);
                healthSlider.value = target.HealthPercent();
                nameText.text = target.name;


                // trade button
                if (target is Player)
                {
                    tradeButton.gameObject.SetActive(true);
                    tradeButton.interactable = player.CanStartTradeWith(target);
                    tradeButton.onClick.SetListener(() =>
                    {
                        player.CmdTradeRequestSend();
                    });
                }
                else tradeButton.gameObject.SetActive(false);

                // guild invite button
                if (target is Player && player.InGuild())
                {
                    guildInviteButton.gameObject.SetActive(true);
                    guildInviteButton.interactable = !((Player)target).InGuild() &&
                                                     player.guild.CanInvite(player.name, target.name) &&
                                                     NetworkTime.time >= player.nextRiskyActionTime &&
                                                     distance <= player.interactionRange;
                    guildInviteButton.onClick.SetListener(() =>
                    {
                        player.playerAlliance.CmdGuildInviteTarget();
                    });
                }
                else guildInviteButton.gameObject.SetActive(false);

                // ally guild invite button
                if (target is Player && player.InGuild())
                {
                    allyInviteButton.gameObject.SetActive(true);
                    allyInviteButton.interactable = ((Player)target).InGuild() &&
                                                     player.playerAlliance.CanInviteToAlliance() &&
                                                     NetworkTime.time >= player.nextRiskyActionTime;
                    allyInviteButton.onClick.SetListener(() =>
                    {
                        player.playerAlliance.CmdInviteToAlliance();
                    });
                }
                else allyInviteButton.gameObject.SetActive(false);

                // partner invite button
                if (target is Player
                    && ((Player)target).playerMarriage.partnerName == string.Empty
                    && player.playerMarriage.partnerName == string.Empty)
                {
                    partnerInviteButton.gameObject.SetActive(true);
                    partnerInviteButton.interactable = NetworkTime.time >= player.nextRiskyActionTime;
                    partnerInviteButton.onClick.SetListener(() =>
                    {
                        player.playerMarriage.CmdInvitePartner();
                    });
                }
                else partnerInviteButton.gameObject.SetActive(false);

                // friend invite button
                if (target is Player
                    && !player.playerFriend.playerFriends.Contains(target.name))
                {
                    friendInviteButton.gameObject.SetActive(true);
                    friendInviteButton.interactable = NetworkTime.time >= player.nextRiskyActionTime;
                    friendInviteButton.onClick.SetListener(() =>
                    {
                        player.playerFriend.CmdSendFriendRequest();
                    });
                }
                else friendInviteButton.gameObject.SetActive(false);

                // party invite button
                if (target is Player)
                {
                    partyInviteButton.gameObject.SetActive(true);
                    partyInviteButton.interactable = (!player.InParty() || !player.party.IsFull()) &&
                                                     !((Player)target).InParty() &&
                                                     NetworkTime.time >= player.nextRiskyActionTime &&
                                                     distance <= player.interactionRange;
                    partyInviteButton.onClick.SetListener(() =>
                    {
                        player.CmdPartyInvite(target.name);
                    });
                }
                else partyInviteButton.gameObject.SetActive(false);
            }
            else panel.SetActive(false);
        }
        else panel.SetActive(false);
    }
}

public partial class UIChatManager : MonoBehaviour
{
    public static UIChatManager singleton;
    public Button infoBubble;
    public Button localBubble;
    public Button whisperBubble;
    public Button partyBubble;
    public Button groupBubble;
    public Button allianceBubble;

    public Button infoChatMain;
    public Button localChatMain;
    public Button whisperChatMain;
    public Button partyChatMain;
    public Button groupChatMain;
    public Button allianceChatMain;

    public Button closeButton;

    public Animator anim;

    private UIChat chat;

    private Player player;
    public string lastSelected;

    public string selectedCategory;

    public Text infoChatText;
    public Text localChatText;
    public Text whisperChatText;
    public Text partyChatText;
    public Text groupChatText;
    public Text allianceChatText;

    public Text infoBubbleText;
    public Text localBubbleText;
    public Text whisperBubbleText;
    public Text partyBubbleText;
    public Text groupBubbleText;
    public Text allianceBubbleText;

    void Start()
    {
        chat = GetComponent<UIChat>();
        if (!singleton) singleton = this;
    }

    public void CheckActivation()
    {
        infoChatMain.transform.GetChild(1).gameObject.SetActive(player.chat.infoMessage > 0);
        localChatMain.transform.GetChild(1).gameObject.SetActive(player.chat.localMessage > 0);
        whisperChatMain.transform.GetChild(1).gameObject.SetActive(player.chat.whisperMessage > 0);
        partyChatMain.transform.GetChild(1).gameObject.SetActive(player.chat.partyMessage > 0);
        groupChatMain.transform.GetChild(1).gameObject.SetActive(player.chat.guildMessage > 0);
        allianceChatMain.transform.GetChild(1).gameObject.SetActive(player.chat.allyMessage > 0);

        infoChatText.text = player.chat.infoMessage.ToString();
        localChatText.text = player.chat.localMessage.ToString();
        whisperChatText.text = player.chat.whisperMessage.ToString();
        partyChatText.text = player.chat.partyMessage.ToString();
        groupChatText.text = player.chat.guildMessage.ToString();
        allianceChatText.text = player.chat.allyMessage.ToString();

        infoBubbleText.text = player.chat.infoMessage.ToString();
        localBubbleText.text = player.chat.localMessage.ToString();
        whisperBubbleText.text = player.chat.whisperMessage.ToString();
        partyBubbleText.text = player.chat.partyMessage.ToString();
        groupBubbleText.text = player.chat.guildMessage.ToString();
        allianceBubbleText.text = player.chat.allyMessage.ToString();

    }

    // Update is called once per frame
    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        CheckActivation();

        infoBubble.onClick.SetListener(() =>
        {
            CleanChat();
            foreach (ChatMessage c in player.chat.infoChannel)
            {
                chat.AddMessage(c);
            }
            //infoChatMain.transform.GetChild(1).gameObject.SetActive(false);
            lastSelected = "info";
            anim.SetBool("OPEN", !anim.GetBool("OPEN"));
            player.chat.infoMessage = 0;
        });

        localBubble.onClick.SetListener(() =>
        {
            CleanChat();
            foreach (ChatMessage c in player.chat.localChannel)
            {
                chat.AddMessage(c);
            }
            //localChatMain.transform.GetChild(1).gameObject.SetActive(false);
            lastSelected = "local";
            anim.SetBool("OPEN", !anim.GetBool("OPEN"));
            selectedCategory = "local";
            player.chat.localMessage = 0;
        });

        whisperBubble.onClick.SetListener(() =>
        {
            CleanChat();
            foreach (ChatMessage c in player.chat.whisperChannel)
            {
                chat.AddMessage(c);
            }
            //whisperChatMain.transform.GetChild(1).gameObject.SetActive(false);
            lastSelected = "whisper";
            anim.SetBool("OPEN", !anim.GetBool("OPEN"));
            selectedCategory = "whisper";
            player.chat.whisperMessage = 0;

        });
        partyBubble.onClick.SetListener(() =>
        {
            CleanChat();
            foreach (ChatMessage c in player.chat.partyChannel)
            {
                chat.AddMessage(c);
            }
            //partyChatMain.transform.GetChild(1).gameObject.SetActive(false);
            lastSelected = "party";
            anim.SetBool("OPEN", !anim.GetBool("OPEN"));
            selectedCategory = "party";
            player.chat.partyMessage = 0;

        });
        groupBubble.onClick.SetListener(() =>
        {
            CleanChat();
            foreach (ChatMessage c in player.chat.guildChannel)
            {
                chat.AddMessage(c);
            }
            //groupChatMain.transform.GetChild(1).gameObject.SetActive(false);
            lastSelected = "group";
            anim.SetBool("OPEN", !anim.GetBool("OPEN"));
            selectedCategory = "group";
            player.chat.guildMessage = 0;
        });
        allianceBubble.onClick.SetListener(() =>
        {
            CleanChat();
            foreach (ChatMessage c in player.chat.allyChannel)
            {
                chat.AddMessage(c);
            }
            //allianceChatMain.transform.GetChild(1).gameObject.SetActive(false);
            lastSelected = "alliance";
            anim.SetBool("OPEN", !anim.GetBool("OPEN"));
            selectedCategory = "alliance";
            player.chat.allyMessage = 0;
        });



        infoChatMain.onClick.SetListener(() =>
        {
            CleanChat();
            foreach (ChatMessage c in player.chat.infoChannel)
            {
                chat.AddMessage(c);
            }
            //infoChatMain.transform.GetChild(1).gameObject.SetActive(false);
            lastSelected = "info";
            player.chat.infoMessage = 0;
        });
        localChatMain.onClick.SetListener(() =>
        {
            CleanChat();
            foreach (ChatMessage c in player.chat.localChannel)
            {
                chat.AddMessage(c);
            }
            //localChatMain.transform.GetChild(1).gameObject.SetActive(false);
            lastSelected = "local";
            selectedCategory = "local";
            player.chat.localMessage = 0;
        });
        whisperChatMain.onClick.SetListener(() =>
        {
            CleanChat();
            foreach (ChatMessage c in player.chat.whisperChannel)
            {
                chat.AddMessage(c);
            }
            //whisperChatMain.transform.GetChild(1).gameObject.SetActive(false);
            lastSelected = "whisper";
            selectedCategory = "whisper";
            player.chat.whisperMessage = 0;
        });
        partyChatMain.onClick.SetListener(() =>
        {
            CleanChat();
            foreach (ChatMessage c in player.chat.partyChannel)
            {
                chat.AddMessage(c);
            }
            //partyChatMain.transform.GetChild(1).gameObject.SetActive(false);
            lastSelected = "party";
            selectedCategory = "party";
            player.chat.partyMessage = 0;
        });
        groupChatMain.onClick.SetListener(() =>
        {
            CleanChat();
            foreach (ChatMessage c in player.chat.guildChannel)
            {
                chat.AddMessage(c);
            }
            //groupChatMain.transform.GetChild(1).gameObject.SetActive(false);
            lastSelected = "group";
            selectedCategory = "group";
            player.chat.guildMessage = 0;
        });
        allianceChatMain.onClick.SetListener(() =>
        {
            CleanChat();
            foreach (ChatMessage c in player.chat.allyChannel)
            {
                chat.AddMessage(c);
            }
            //allianceChatMain.transform.GetChild(1).gameObject.SetActive(false);
            lastSelected = "alliance";
            selectedCategory = "alliance";
            player.chat.allyMessage = 0;
        });

        closeButton.onClick.SetListener(() =>
        {
            selectedCategory = "";
            CleanChat();
            anim.SetBool("OPEN", !anim.GetBool("OPEN"));
            lastSelected = "";
        });


        if (lastSelected == "info")
        {
            CleanChat();
            foreach (ChatMessage c in player.chat.infoChannel)
            {
                chat.AddMessage(c);
            }
            //infoChatMain.transform.GetChild(1).gameObject.SetActive(false);
            lastSelected = "info";
            player.chat.infoMessage = 0;
        }
        if (lastSelected == "local")
        {
            CleanChat();
            foreach (ChatMessage c in player.chat.localChannel)
            {
                chat.AddMessage(c);
            }
            //localChatMain.transform.GetChild(1).gameObject.SetActive(false);
            lastSelected = "local";
            selectedCategory = "local";
            player.chat.localMessage = 0;
        }
        if (lastSelected == "whisper")
        {
            CleanChat();
            foreach (ChatMessage c in player.chat.whisperChannel)
            {
                chat.AddMessage(c);
            }
            //whisperChatMain.transform.GetChild(1).gameObject.SetActive(false);
            lastSelected = "whisper";
            selectedCategory = "whisper";
            player.chat.whisperMessage = 0;
        }
        if (lastSelected == "party")
        {
            CleanChat();
            foreach (ChatMessage c in player.chat.partyChannel)
            {
                chat.AddMessage(c);
            }
            //partyChatMain.transform.GetChild(1).gameObject.SetActive(false);
            lastSelected = "party";
            selectedCategory = "party";
            player.chat.partyMessage = 0;
        }
        if (lastSelected == "group")
        {
            CleanChat();
            foreach (ChatMessage c in player.chat.guildChannel)
            {
                chat.AddMessage(c);
            }
            //groupChatMain.transform.GetChild(1).gameObject.SetActive(false);
            lastSelected = "group";
            selectedCategory = "group";
            player.chat.guildMessage = 0;
        }
        if (lastSelected == "alliance")
        {
            CleanChat();
            foreach (ChatMessage c in player.chat.allyChannel)
            {
                chat.AddMessage(c);
            }
            //allianceChatMain.transform.GetChild(1).gameObject.SetActive(false);
            lastSelected = "alliance";
            selectedCategory = "alliance";
            player.chat.allyMessage = 0;
        }

    }

    public void CleanChat()
    {
        foreach (Transform g in chat.content)
        {
            Destroy(g.gameObject);
        }
    }
}

public partial class UIAbilities
{
    private Player player;
    public Transform content;
    public GameObject gameObjectToSpawn;
    public int selectedAbilities = -1;
    public TextMeshProUGUI description;
    public Button UpgradeButton;

    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        UpgradeButton.onClick.SetListener(() =>
        {
            player.playerAbility.CmdIncreaseAbility(selectedAbilities);
            content.GetChild(selectedAbilities).GetComponent<AbilitySlot>().button.onClick.Invoke();
        });

        UIUtils.BalancePrefabs(gameObjectToSpawn, player.playerAbility.networkAbilities.Count, content);
        for (int i = 0; i < player.playerAbility.networkAbilities.Count; i++)
        {
            int index = i;
            AbilitySlot slot = content.GetChild(index).GetComponent<AbilitySlot>();
            slot.statName.text = player.playerAbility.networkAbilities[index].name;
            slot.image.sprite = player.playerAbility.abilities[index].image;
            slot.statAmount.text = player.playerAbility.networkAbilities[index].level + " / " + player.playerAbility.networkAbilities[index].maxLevel;
            slot.button.gameObject.SetActive(true);
            slot.button.onClick.SetListener(() =>
            {
                selectedAbilities = index;

            });

            if (selectedAbilities > -1)
            {
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    description.text = player.playerAbility.abilities[selectedAbilities].DescriptionIta + "\n\n" + "Il livello attuale dell'abilita' e' : " + player.playerAbility.networkAbilities[selectedAbilities].level;
                    if (selectedAbilities > -1 && player.playerAbility.CanUpgradeAbilities(selectedAbilities))
                    {
                        description.text += "\n\nPer effettuare l'upgrade al prossimo livello e' necessario : " + player.playerAbility.networkAbilities[selectedAbilities].baseValue * (player.playerAbility.networkAbilities[selectedAbilities].level + 1) + " oro!";
                    }
                    else if (selectedAbilities > -1 && !player.playerAbility.CanUpgradeAbilities(selectedAbilities))
                    {
                        description.text += "\n\nCongratulazioni, hai raggiunto il livello massimo per questa abilita'!";
                    }
                }
                else
                {
                    description.text = player.playerAbility.abilities[selectedAbilities].Description + "\n\n" + "Current ability level is : " + player.playerAbility.networkAbilities[selectedAbilities].level;
                    if (selectedAbilities > -1 && player.playerAbility.CanUpgradeAbilities(selectedAbilities))
                    {
                        description.text += "\n\nTo upgrade to the next level you need : " + player.playerAbility.networkAbilities[selectedAbilities].baseValue * (player.playerAbility.networkAbilities[selectedAbilities].level + 1) + " gold!";
                    }
                    else if (selectedAbilities > -1 && !player.playerAbility.CanUpgradeAbilities(selectedAbilities))
                    {
                        description.text += "\n\nCongratulations, you reach the maximum level for this ability!";
                    }

                }
            }

        }

        UpgradeButton.gameObject.SetActive(selectedAbilities > -1 && player.playerAbility.CanUpgradeAbilities(selectedAbilities));
    }
}

public partial class UIBoost
{
    public static UIBoost singleton;
    private Player player;
    public Transform boostContent;
    public Transform generalBoostContent;
    public GameObject objectToSpawn;
    public GameObject boostSlot;

    public ScriptableBoost selectedBoost;
    public GameObject uiToSpawn;

    public Button personalBoost;

    public GameObject instantiatedPanel;
    public bool changeBoost;

    TimeSpan difference;

    void Start()
    {
        if (!singleton) singleton = this;
        changeBoost = false;
    }

    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        personalBoost.onClick.SetListener(() =>
        {
            changeBoost = !changeBoost;
        });

        personalBoost.gameObject.SetActive(player && player.playerBoost.networkBoost.Count > 0);

        boostContent.gameObject.SetActive(!changeBoost);
        generalBoostContent.gameObject.SetActive(changeBoost);
        if (!changeBoost)
        {
            UIUtils.BalancePrefabs(objectToSpawn, GeneralManager.singleton.listCompleteOfBoost.Count, boostContent);
            for (int i = 0; i < boostContent.childCount; i++)
            {
                int index = i;

                BuyBoostSlot slot = boostContent.GetChild(index).GetComponent<BuyBoostSlot>();
                slot.title.text = GeneralManager.singleton.listCompleteOfBoost[index].name;
                slot.coins.gameObject.SetActive(true);
                slot.gold.gameObject.SetActive(true);
                slot.coins.text = GeneralManager.singleton.listCompleteOfBoost[index].coin.ToString();
                slot.gold.text = GeneralManager.singleton.listCompleteOfBoost[index].gold.ToString();
                slot.boostImage.sprite = GeneralManager.singleton.listCompleteOfBoost[index].image;
                slot.boostButton.onClick.SetListener(() =>
                {
                    if (instantiatedPanel) Destroy(instantiatedPanel);
                    selectedBoost = GeneralManager.singleton.listCompleteOfBoost[index];
                    instantiatedPanel = Instantiate(uiToSpawn, GeneralManager.singleton.canvas);
                });
            }
        }
        else
        {
            if (player.playerBoost.networkBoost.Count > 0)
            {
                UIUtils.BalancePrefabs(boostSlot, 12, generalBoostContent);
                for (int i = 0; i < generalBoostContent.childCount; i++)
                {
                    int index = i;
                    UIBoostSlot slot = generalBoostContent.GetChild(index).GetComponent<UIBoostSlot>();
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        if (index == 0)
                        {
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].velocityTimer))
                                difference = DateTime.Parse(player.playerBoost.networkBoost[0].velocityTimer) - DateTime.Now;
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].velocityTimer) && difference.TotalSeconds > 0)
                                slot.text.text = "  Timer velocita' : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds));
                            else
                                slot.text.text = "  Timer velocita' : " + GeneralManager.singleton.ConvertToTimer(0);

                        }
                        if (index == 1)
                        {
                            slot.text.text = "  Velocita'      : " + player.playerBoost.networkBoost[0].velocityPerc + " % ";
                        }
                        if (index == 2)
                        {
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].accuracyTimer))
                                difference = DateTime.Parse(player.playerBoost.networkBoost[0].accuracyTimer) - DateTime.Now;
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].accuracyTimer) && difference.TotalSeconds > 0)
                                slot.text.text = "  Timer Accuratezza : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds));
                            else
                                slot.text.text = "  Timer Accuratezza : " + GeneralManager.singleton.ConvertToTimer(0);
                        }
                        if (index == 3)
                        {
                            slot.text.text = "  Accuratezza       : " + player.playerBoost.networkBoost[0].accuracyPerc + " % ";
                        }
                        if (index == 4)
                        {
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].missTimer))
                                difference = DateTime.Parse(player.playerBoost.networkBoost[0].missTimer) - DateTime.Now;
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].missTimer) && difference.TotalSeconds > 0)
                                slot.text.text = "  Timer Evasione : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds));
                            else
                                slot.text.text = "  Timer Evasione : " + GeneralManager.singleton.ConvertToTimer(0);
                        }
                        if (index == 5)
                        {
                            slot.text.text = "  Evasione       : " + player.playerBoost.networkBoost[0].missPerc + " % ";
                        }
                        if (i == 6)
                        {
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer))
                                difference = DateTime.Parse(player.playerBoost.networkBoost[0].hiddenIslandTimer) - DateTime.Now;
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer) && difference.TotalSeconds > 0)
                                slot.text.text = "  Timer premium : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds));
                            else
                                slot.text.text = "  Timer premium : " + GeneralManager.singleton.ConvertToTimer(0);
                        }
                        if (i == 7)
                        {
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleEXP))
                                difference = DateTime.Parse(player.playerBoost.networkBoost[0].doubleEXP) - System.DateTime.Now;
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleEXP) && difference.TotalSeconds > 0)
                                slot.text.text = "  Timer doppia esperienza : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds));
                            else
                                slot.text.text = "  Timer doppia esperienza : " + GeneralManager.singleton.ConvertToTimer(0);
                        }
                        if (i == 8)
                        {
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleGold))
                                difference = DateTime.Parse(player.playerBoost.networkBoost[0].doubleGold) - DateTime.Now;
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleGold) && difference.TotalSeconds > 0)
                                slot.text.text = "  Timer doppio oro : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds));
                            else
                                slot.text.text = "  Timer doppio oro : " + GeneralManager.singleton.ConvertToTimer(0);
                        }
                        if (i == 9)
                        {
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleLeaderPoints))
                                difference = DateTime.Parse(player.playerBoost.networkBoost[0].doubleLeaderPoints) - DateTime.Now;
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleLeaderPoints) && difference.TotalSeconds > 0)
                                slot.text.text = "  Timer doppi punti : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds));
                            else
                                slot.text.text = "  Timer doppi punti : " + GeneralManager.singleton.ConvertToTimer(0);
                        }
                        if (i == 10)
                        {
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleDamageToMonster))
                                difference = DateTime.Parse(player.playerBoost.networkBoost[0].doubleDamageToMonster) - DateTime.Now;
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleDamageToMonster) && difference.TotalSeconds > 0)
                                slot.text.text = "  Timer doppi danni zombie : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds));
                            else
                                slot.text.text = "  Timer doppi danni zombie : " + GeneralManager.singleton.ConvertToTimer(0);
                        }
                        if (i == 11)
                        {
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleDamageToPlayer))
                                difference = DateTime.Parse(player.playerBoost.networkBoost[0].doubleDamageToPlayer) - DateTime.Now;
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleDamageToPlayer) && difference.TotalSeconds > 0)
                                slot.text.text = "  Timer doppi danni a giocatori : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds));
                            else
                                slot.text.text = "  Timer doppi danni a giocatori : " + GeneralManager.singleton.ConvertToTimer(0);
                        }
                    }
                    else
                    {
                        if (index == 0)
                        {
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].velocityTimer))
                                difference = DateTime.Parse(player.playerBoost.networkBoost[0].velocityTimer) - DateTime.Now;
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].velocityTimer) && difference.TotalSeconds > 0)
                                slot.text.text = "  Velocity timer : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds));
                            else
                                slot.text.text = "  Velocity timer : " + GeneralManager.singleton.ConvertToTimer(0);

                        }
                        if (index == 1)
                        {
                            slot.text.text = "  Velocity      : " + player.playerBoost.networkBoost[0].velocityPerc + " % ";
                        }
                        if (index == 2)
                        {
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].accuracyTimer))
                                difference = DateTime.Parse(player.playerBoost.networkBoost[0].accuracyTimer) - DateTime.Now;
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].accuracyTimer) && difference.TotalSeconds > 0)
                                slot.text.text = "  Accuracy timer : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds));
                            else
                                slot.text.text = "  Accuracy timer : " + GeneralManager.singleton.ConvertToTimer(0);
                        }
                        if (index == 3)
                        {
                            slot.text.text = "  Accuracy       : " + player.playerBoost.networkBoost[0].accuracyPerc + " % ";
                        }
                        if (index == 4)
                        {
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].missTimer))
                                difference = DateTime.Parse(player.playerBoost.networkBoost[0].missTimer) - DateTime.Now;
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].missTimer) && difference.TotalSeconds > 0)
                                slot.text.text = "  Evasion timer : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds));
                            else
                                slot.text.text = "  Evasion timer : " + GeneralManager.singleton.ConvertToTimer(0);
                        }
                        if (index == 5)
                        {
                            slot.text.text = "  Evasion       : " + player.playerBoost.networkBoost[0].missPerc + " % ";
                        }
                        if (i == 6)
                        {
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer))
                                difference = DateTime.Parse(player.playerBoost.networkBoost[0].hiddenIslandTimer) - DateTime.Now;
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer) && difference.TotalSeconds > 0)
                                slot.text.text = "  Premium timer : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds));
                            else
                                slot.text.text = "  Premium timer : " + GeneralManager.singleton.ConvertToTimer(0);
                        }
                        if (i == 7)
                        {
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleEXP))
                                difference = DateTime.Parse(player.playerBoost.networkBoost[0].doubleEXP) - DateTime.Now;
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleEXP) && difference.TotalSeconds > 0)
                                slot.text.text = "  Exp 2x timer : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds));
                            else
                                slot.text.text = "  Exp 2x timer : " + GeneralManager.singleton.ConvertToTimer(0);
                        }
                        if (i == 8)
                        {
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleGold))
                                difference = DateTime.Parse(player.playerBoost.networkBoost[0].doubleGold) - DateTime.Now;
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleGold) && difference.TotalSeconds > 0)
                                slot.text.text = "  Gold 2x timer : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds));
                            else
                                slot.text.text = "  Gold 2x timer : " + GeneralManager.singleton.ConvertToTimer(0);
                        }
                        if (i == 9)
                        {
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleLeaderPoints))
                                difference = DateTime.Parse(player.playerBoost.networkBoost[0].doubleLeaderPoints) - DateTime.Now;
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleLeaderPoints) && difference.TotalSeconds > 0)
                                slot.text.text = "  Leaderboard 2x timer : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds));
                            else
                                slot.text.text = "  Leaderboard 2x timer : " + GeneralManager.singleton.ConvertToTimer(0);
                        }
                        if (i == 10)
                        {
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleDamageToMonster))
                                difference = DateTime.Parse(player.playerBoost.networkBoost[0].doubleDamageToMonster) - DateTime.Now;
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleDamageToMonster) && difference.TotalSeconds > 0)
                                slot.text.text = "  Monster damage 2x timer : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds));
                            else
                                slot.text.text = "  Monster damage 2x timer : " + GeneralManager.singleton.ConvertToTimer(0);
                        }
                        if (i == 11)
                        {
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleDamageToPlayer))
                                difference = DateTime.Parse(player.playerBoost.networkBoost[0].doubleDamageToPlayer) - DateTime.Now;
                            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].doubleDamageToPlayer) && difference.TotalSeconds > 0)
                                slot.text.text = "  Player damage 2x timer : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds));
                            else
                                slot.text.text = "  Player damage 2x timer : " + GeneralManager.singleton.ConvertToTimer(0);
                        }
                    }
                }
            }
        }
    }
}

public partial class UIInventory
{
    public static UIInventory singleton;
    public KeyCode hotKey = KeyCode.I;
    public GameObject panel;
    public UIInventorySlot slotPrefab;
    public Transform content;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI coinText;
    public UIDragAndDropable trash;
    public Image trashImage;
    public GameObject trashOverlay;
    public TextMeshProUGUI trashAmountText;

    public int selectedItem;
    public GameObject itemDisplayer;
    [HideInInspector] public Player player;

    public void Start()
    {
        if (!singleton) singleton = this;
    }

    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        // gold
        goldText.text = player.gold.ToString();
        coinText.text = player.coins.ToString();

        if (player != null)
        {
            // hotkey (not while typing in chat, etc.)
            if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
                panel.SetActive(!panel.activeSelf);

            // only update the panel if it's active
            if (panel.activeSelf)
            {
                // instantiate/destroy enough slots
                UIUtils.BalancePrefabs(slotPrefab.gameObject, player.inventory.Count, content);

                // refresh all items
                for (int i = 0; i < player.inventory.Count; ++i)
                {
                    UIInventorySlot slot = content.GetChild(i).GetComponent<UIInventorySlot>();
                    slot.dragAndDropable.name = i.ToString(); // drag and drop index
                    ItemSlot itemSlot = player.inventory[i];

                    if (itemSlot.amount > 0)
                    {
                        // refresh valid item
                        int icopy = i; // needed for lambdas, otherwise i is Count
                        slot.button.onClick.SetListener(() =>
                        {
                            if (player.playerBuilding.actualBuilding) Destroy(player.playerBuilding.actualBuilding);
                            player.playerBuilding.actualBuilding = null;
                            player.playerBuilding.building = null;
                            player.playerBuilding.inventoryIndex = -1;
                            if (GeneralManager.singleton.spawnedBuildingObject) Destroy(GeneralManager.singleton.spawnedBuildingObject);
                            ModularBuildingManager.singleton.ableModificationMode = false;
                            ModularBuildingManager.singleton.ableModificationWallMode = false;

                            player.playerBuilding.invBelt = false;
                            selectedItem = icopy;
                            UIOrderManager.singleton.SingleInstantePanel(itemDisplayer);
                            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                                UIOrderManager.singleton.singleTimePanel[UIOrderManager.singleton.singleTimePanel.Count - 1].GetComponent<UISelectedItem>().description.text = player.inventory[selectedItem].ToolTip().Replace("{AMOUNT}", "Quantita' :" + player.inventory[selectedItem].amount);
                            else
                                UIOrderManager.singleton.singleTimePanel[UIOrderManager.singleton.singleTimePanel.Count - 1].GetComponent<UISelectedItem>().description.text = player.inventory[selectedItem].ToolTip().Replace("{AMOUNT}", "Quantity :" + player.inventory[selectedItem].amount);
                            UIOrderManager.singleton.singleTimePanel[UIOrderManager.singleton.singleTimePanel.Count - 1].GetComponent<UISelectedItem>().itemImage.sprite = player.inventory[selectedItem].item.data.image;
                        });
                        // only build tooltip while it's actually shown. this
                        // avoids MASSIVE amounts of StringBuilder allocations.
                        slot.tooltip.enabled = false;
                        //if (slot.tooltip.IsVisible())
                        //    slot.tooltip.text = itemSlot.ToolTip();

                        slot.dragAndDropable.dragable = true;

                        slot.image.color = Color.white;
                        slot.image.sprite = itemSlot.item.image;
                        if (icopy < player.playerConservative.AmountOfItemProtected()) slot.protectedImage.gameObject.SetActive(true);
                        else slot.protectedImage.gameObject.SetActive(false);
                        // cooldown if usable item
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
                        // refresh invalid item
                        slot.button.onClick.RemoveAllListeners();
                        slot.tooltip.enabled = false;
                        slot.dragAndDropable.dragable = false;
                        slot.image.color = Color.clear;
                        slot.image.sprite = null;
                        slot.cooldownCircle.fillAmount = 0;
                        slot.amountOverlay.SetActive(false);
                        if (i <= player.playerConservative.AmountOfItemProtected()) slot.protectedImage.gameObject.SetActive(true);
                        else slot.protectedImage.gameObject.SetActive(false);
                        slot.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                        slot.GetComponent<Image>().color = GffItemRarity.singleton.ColorNull;

                    }
                }



                // trash (tooltip always enabled, dropable always true)
                //trash.dragable = player.trash.amount > 0;
                //if (player.trash.amount > 0)
                //{
                //    // refresh valid item
                //    trashImage.color = Color.white;
                //    trashImage.sprite = player.trash.item.image;
                //    trashOverlay.SetActive(player.trash.amount > 1);
                //    trashAmountText.text = player.trash.amount.ToString();
                //}
                //else
                //{
                //    // refresh invalid item
                //    trashImage.color = Color.clear;
                //    trashImage.sprite = null;
                //    trashOverlay.SetActive(false);
                //}
            }
        }
        else panel.SetActive(false);
    }

}

public partial class UIBuilding : MonoBehaviour
{
    private Player player;
    public static UIBuilding singleton;
    public Button up;
    public Button left;
    public Button down;
    public Button right;
    public Button spawn;
    public Button cancel;
    public Button changePerspective;
    public Button modularBuildingManager;

    public WoodWall selectedWoodwall;

    public List<Positioning> childpositioning = new List<Positioning>();

    public int currentPositioning = 0;

    void Start()
    {
        if (!singleton) singleton = this;
    }

    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.health == 0)
        {
            cancel.onClick.Invoke();
        }

        up.onClick.SetListener(() =>
        {
            if (player.playerBuilding.actualBuilding)
            {
                if (player.playerBuilding.actualBuilding.GetComponent<Building>())
                    player.playerBuilding.actualBuilding.GetComponent<Building>().Up();
                else
                    player.playerBuilding.actualBuilding.GetComponent<ModularPiece>().Up();
            }
        });

        left.onClick.SetListener(() =>
        {
            if (player.playerBuilding.actualBuilding)
            {
                if (player.playerBuilding.actualBuilding.GetComponent<Building>())
                    player.playerBuilding.actualBuilding.GetComponent<Building>().Left();
                else
                    player.playerBuilding.actualBuilding.GetComponent<ModularPiece>().Left();
            }
        });

        down.onClick.SetListener(() =>
        {
            if (player.playerBuilding.actualBuilding)
            {
                if (player.playerBuilding.actualBuilding.GetComponent<Building>())
                    player.playerBuilding.actualBuilding.GetComponent<Building>().Down();
                else
                    player.playerBuilding.actualBuilding.GetComponent<ModularPiece>().Down();
            }
        });

        right.onClick.SetListener(() =>
        {
            if (player.playerBuilding.actualBuilding)
            {
                if (player.playerBuilding.actualBuilding.GetComponent<Building>())
                    player.playerBuilding.actualBuilding.GetComponent<Building>().Right();
                else
                    player.playerBuilding.actualBuilding.GetComponent<ModularPiece>().Right();
            }
        });

        spawn.onClick.SetListener(() =>
        {
            if (ModularBuildingManager.singleton.selectedPoint && ModularBuildingManager.singleton.ableModificationWallMode)
            {
                ModularPiece piece = ModularBuildingManager.singleton.selectedPoint.GetComponentInParent<ModularPiece>();
                player.playerBuilding.CmdSyncWallDoor(piece.GetComponent<NetworkIdentity>(), piece.clientupComponent, piece.clientdownComponent, piece.clientleftComponent, piece.clientrightComponent, player.playerBuilding.invBelt, player.playerBuilding.inventoryIndex);
                DestroyBuilding();
            }
            else
            {
                if (player.playerBuilding.actualBuilding)
                {
                    if (player.playerBuilding.actualBuilding.GetComponent<Building>())
                        player.playerBuilding.actualBuilding.GetComponent<Building>().SpawnBuilding();
                    else
                        player.playerBuilding.actualBuilding.GetComponent<ModularPiece>().SpawnBuilding();
                }
            }
        });

        cancel.onClick.SetListener(() =>
        {
            if (player.playerBuilding.actualBuilding)
            {
                if (player.playerBuilding.actualBuilding.GetComponent<Building>())
                    player.playerBuilding.actualBuilding.GetComponent<Building>().DestroyBuilding();
                else
                    player.playerBuilding.actualBuilding.GetComponent<ModularPiece>().DestroyBuilding();
            }
            ModularBuildingManager.singleton.selectedPoint = null;
            ModularBuildingManager.singleton.ResetComponent(ModularBuildingManager.singleton.modularPiece);
            DestroyBuilding();
        });

        changePerspective.gameObject.SetActive(player.playerBuilding.building && player.playerBuilding.building.buildingList.Count > 1);
        changePerspective.onClick.SetListener(() =>
        {
            if (childpositioning.Count > 0)
            {
                currentPositioning++;
                if (currentPositioning > childpositioning.Count - 1) currentPositioning = 0;

                player.playerBuilding.actualBuilding.GetComponent<Building>().chengePerspective(childpositioning[currentPositioning]);
            }
            else
            {
                player.playerBuilding.actualBuilding.GetComponent<Building>().chengePerspective(new Positioning(null, -1));
            }
        });

        modularBuildingManager.gameObject.SetActive(ModularBuildingManager.singleton && ModularBuildingManager.singleton.selectedPiece);
        modularBuildingManager.onClick.SetListener(() =>
        {
            Instantiate(GeneralManager.singleton.modularBuildingManager, GeneralManager.singleton.canvas);
        });

        if(ModularBuildingManager.singleton && ModularBuildingManager.singleton.ableModificationWallMode)
        {
            if(ModularBuildingManager.singleton.selectedPoint)
            {
                spawn.interactable = true;
            }
        }

    }

    public void DisableButton()
    {
        up.gameObject.SetActive(false);
        left.gameObject.SetActive(false);
        down.gameObject.SetActive(false);
        right.gameObject.SetActive(false);
        changePerspective.gameObject.SetActive(false);
        spawn.interactable = false;
    }

    public void DestroyBuilding()
    {
        Player player = Player.localPlayer;

        if (player.playerBuilding.actualBuilding) Destroy(player.playerBuilding.actualBuilding);
        player.playerBuilding.actualBuilding = null;
        player.playerBuilding.building = null;
        player.playerBuilding.inventoryIndex = -1;
        if (GeneralManager.singleton.spawnedBuildingObject) Destroy(GeneralManager.singleton.spawnedBuildingObject);
        ModularBuildingManager.singleton.ableModificationMode = false;
        ModularBuildingManager.singleton.ableModificationWallMode = false;
        ModularBuildingManager.singleton.DisableNearestWall();
    }

}

public partial class UIEquipmentCustom
{
    public KeyCode hotKey = KeyCode.E;
    public UIEquipmentSlotCustom slotPrefab;
    public Transform content;
    public bool settedCategory;

    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            // instantiate/destroy enough slots
            UIUtils.BalancePrefabs(slotPrefab.gameObject, 2, content);

            // refresh all
            for (int i = 0; i < 2; ++i)
            {
                UIEquipmentSlotCustom slot = content.GetChild(i).GetComponent<UIEquipmentSlotCustom>();
                slot.dragAndDropable.name = i.ToString(); // drag and drop slot
                ItemSlot itemSlot = player.equipment[i];

                // set category overlay in any case. we use the last noun in the
                // category string, for example EquipmentWeaponBow => Bow
                // (disabled if no category, e.g. for archer shield slot)
                slot.categoryOverlay.SetActive(false);
                if (!settedCategory)
                {
                    string overlay = Utils.ParseLastNoun(player.equipmentInfo[i].requiredCategory);
                    slot.categoryText.text = overlay != "" ? overlay : "?";
                }

                if (itemSlot.amount > 0)
                {
                    // refresh valid item

                    // only build tooltip while it's actually shown. this
                    // avoids MASSIVE amounts of StringBuilder allocations.
                    slot.tooltip.enabled = false;
                    if (slot.tooltip.IsVisible())
                        slot.tooltip.text = itemSlot.ToolTip();
                    slot.dragAndDropable.dragable = true;
                    slot.image.color = Color.white;
                    slot.image.sprite = itemSlot.item.image;
                    // cooldown if usable item
                    if (itemSlot.item.data is UsableItem usable)
                    {
                        float cooldown = player.GetItemCooldown(usable.cooldownCategory);
                        slot.cooldownCircle.fillAmount = usable.cooldown > 0 ? cooldown / usable.cooldown : 0;
                    }
                    else slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(itemSlot.amount > 1);
                    slot.amountText.text = itemSlot.amount.ToString();
                    slot.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                    slot.GetComponent<Image>().color = GffItemRarity.singleton.rarityColor(true, itemSlot.item);
                }
                else
                {
                    // refresh invalid item
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
            settedCategory = true;
        }
    }
}

public partial class UIEquipment
{
    public KeyCode hotKey = KeyCode.E;
    public GameObject panel;
    public UIEquipmentSlot slotPrefab;
    public Transform content;
    public bool settedCategory;
    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            // hotkey (not while typing in chat, etc.)
            if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
                panel.SetActive(!panel.activeSelf);

            // only update the panel if it's active
            if (panel.activeSelf)
            {
                // instantiate/destroy enough slots
                UIUtils.BalancePrefabs(slotPrefab.gameObject, player.equipment.Count, content);

                // refresh all
                for (int i = 0; i < player.equipment.Count; ++i)
                {
                    int index = i;
                    UIEquipmentSlot slot = content.GetChild(i).GetComponent<UIEquipmentSlot>();
                    slot.dragAndDropable.name = i.ToString(); // drag and drop slot
                    ItemSlot itemSlot = player.equipment[i];

                    slot.unequipButton.interactable = itemSlot.amount > 0;
                    slot.unequipButton.onClick.SetListener(() =>
                    {
                        player.CmdUnequip(index);
                    });

                    slot.categoryOverlay.SetActive(player.equipmentInfo[i].requiredCategory != "");
                    if (!settedCategory)
                    {
                        string overlay = Utils.ParseLastNoun(player.equipmentInfo[i].requiredCategory);
                        slot.categoryText.text = overlay != "" ? overlay : "?";
                    }
                    if (itemSlot.amount > 0)
                    {
                        // refresh valid item

                        // only build tooltip while it's actually shown. this
                        // avoids MASSIVE amounts of StringBuilder allocations.
                        slot.tooltip.enabled = true;
                        if (slot.tooltip.IsVisible())
                            slot.tooltip.text = itemSlot.ToolTip();
                        slot.dragAndDropable.dragable = true;
                        slot.image.color = Color.white;
                        slot.image.sprite = itemSlot.item.image;
                        // cooldown if usable item
                        if (itemSlot.item.data is UsableItem usable)
                        {
                            float cooldown = player.GetItemCooldown(usable.cooldownCategory);
                            slot.cooldownCircle.fillAmount = usable.cooldown > 0 ? cooldown / usable.cooldown : 0;
                        }
                        else slot.cooldownCircle.fillAmount = 0;
                        slot.amountOverlay.SetActive(itemSlot.amount > 1);
                        slot.amountText.text = itemSlot.amount.ToString();
                        slot.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                        slot.GetComponent<Image>().color = GffItemRarity.singleton.rarityColor(true, itemSlot.item);
                    }
                    else
                    {
                        // refresh invalid item
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
                settedCategory = true;
            }
        }
        else panel.SetActive(false);
    }
}

public partial class UISkillbarCustom
{
    //public UISkillbarSlot slotPrefab;
    public Transform content;

    private Player player;

    public void Start()
    {
        InvokeRepeating(nameof(CheckSkillbar), 0.0f, 0.5f);
    }

    void CheckSkillbar()
    {
        if (!player)
            player = Player.localPlayer;

        if (player != null)
        {

            // refresh all
            for (int i = 0; i < player.playerBelt.belt.Count; i++)
            {
                int index = i;
                UISkillbarSlot slot = content.GetChild(i).GetComponent<UISkillbarSlot>();
                slot.dragAndDropable.name = i.ToString(); // drag and drop index
                ItemSlot itemSlot = player.playerBelt.belt[index];

                if (itemSlot.amount > 0)
                {
                    slot.button.onClick.SetListener(() =>
                    {
                        if (player.playerBuilding.actualBuilding) Destroy(player.playerBuilding.actualBuilding);
                        player.playerBuilding.actualBuilding = null;
                        player.playerBuilding.building = null;
                        player.playerBuilding.inventoryIndex = -1;
                        if (GeneralManager.singleton.spawnedBuildingObject) Destroy(GeneralManager.singleton.spawnedBuildingObject);
                        ModularBuildingManager.singleton.ableModificationMode = false;
                        ModularBuildingManager.singleton.ableModificationWallMode = false;

                        player.playerBuilding.invBelt = true;
                        player.playerBuilding.inventoryIndex = index;
                        GameObject g = Instantiate(GeneralManager.singleton.itemDisplayer, GeneralManager.singleton.canvas);
                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            g.GetComponent<UISelectedItem>().description.text = player.playerBelt.belt[index].ToolTip().Replace("{AMOUNT}", "Quantita' :" + player.playerBelt.belt[index].amount);
                        else
                            g.GetComponent<UISelectedItem>().description.text = player.playerBelt.belt[index].ToolTip().Replace("{AMOUNT}", "Quantity :" + player.playerBelt.belt[index].amount);

                        g.GetComponent<UISelectedItem>().itemImage.sprite = player.playerBelt.belt[index].item.data.image;
                    });
                    // only build tooltip while it's actually shown. this
                    // avoids MASSIVE amounts of StringBuilder allocations.
                    slot.tooltip.enabled = false;
                    if (slot.tooltip.IsVisible())
                        slot.tooltip.text = itemSlot.ToolTip();
                    slot.dragAndDropable.dragable = true;
                    slot.image.color = Color.white;
                    slot.image.sprite = itemSlot.item.image;
                    slot.cooldownOverlay.SetActive(false);
                    // cooldown if usable item
                    if (itemSlot.item.data is UsableItem usable)
                    {
                        float cooldown = player.GetItemCooldown(usable.cooldownCategory);
                        slot.cooldownCircle.fillAmount = usable.cooldown > 0 ? cooldown / usable.cooldown : 0;
                    }
                    else slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(itemSlot.amount > 1);
                    slot.amountText.text = itemSlot.amount.ToString();
                    slot.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                    slot.GetComponent<Image>().color = GffItemRarity.singleton.rarityColor(true, itemSlot.item);
                }
                else
                {
                    // clear the outdated reference
                    player.skillbar[i].reference = "";

                    // refresh empty slot
                    slot.button.onClick.RemoveAllListeners();
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                    slot.cooldownOverlay.SetActive(false);
                    slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(false);
                    slot.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                    slot.GetComponent<Image>().color = GffItemRarity.singleton.ColorNull;
                }
            }
        }
    }

}

public partial class UIAttackManager
{
    public static UIAttackManager singleton;
    private Player player;
    public GameObject ammoSelectorShadow;
    public Button ammoSelector;
    public GameObject reloadShadow;
    public Button reloadButton;
    public GameObject ammoSelectorToSpawn;
    public Text reloadText;
    public Slider durabilitySlider;
    public AmmoItem ammoItem;
    public GameObject ammoPanel;
    public Button sneakButton;
    public Button runButton;
    public Button attackButton;
    public Button destroyBuildingButton;
    public Button explodeButton;
    public Button Tab;
    public GameObject carManager;
    public Button badgeButton;
    public Button menuButton;
    public Button modularBuildingButton;

    public GameObject ammoTypeSelector;
    private Building building;

    public Entity target;

    void Start()
    {
        if (!singleton) singleton = this;
    }

    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        badgeButton.transform.parent.gameObject.SetActive(player);

        if (player.target != null) target = player.target;
        else
            target = null;

        runButton.onClick.SetListener(() =>
        {
            player.playerMove.CmdRun();
        });

        Tab.onClick.SetListener(() =>
        {
            player.TargetNearestEntityButton();
        });

        badgeButton.onClick.SetListener(() =>
        {
            player.CmdSpawnRankBadge();
        });

        menuButton.onClick.SetListener(() =>
        {
            if (ammoTypeSelector) Destroy(ammoTypeSelector.gameObject);
        });

        destroyBuildingButton.onClick.SetListener(() =>
        {
            Instantiate(GeneralManager.singleton.confirmDeletePanel, GeneralManager.singleton.canvas);
        });

        explodeButton.onClick.SetListener(() =>
        {
            if (target && target.GetComponent<Dynamite>())
            {
                player.CmdExplodeDynamite();
            }
            if (target && target.GetComponent<Mine>())
                player.CmdActiveMine();
        });

        attackButton.onClick.SetListener(() =>
        {
            player.ButtonSelectionHandling();
        });

        ammoSelector.onClick.SetListener(() =>
        {
            if (ammoTypeSelector == null)
            {
                ammoTypeSelector = Instantiate(ammoSelectorToSpawn, GeneralManager.singleton.canvas);
            }
            else
            {
                Destroy(ammoTypeSelector);
            }
        });
        reloadButton.onClick.SetListener(() =>
        {
            if (player.playerItemEquipment.firstWeapon.amount > 0)
            {
                if (!player.playerOptions.blockSound)
                {
                    if (((WeaponItem)player.playerItemEquipment.firstWeapon.item.data).reloadClip)
                    {
                        GeneralManager.singleton.reloadAudioSource.clip = ((WeaponItem)player.playerItemEquipment.firstWeapon.item.data).reloadClip;
                        GeneralManager.singleton.reloadAudioSource.Play();
                    }
                }
            }
            player.CmdReloadWeapon();
        });

        sneakButton.onClick.SetListener(() =>
        {
            player.playerMove.CmdSneak();
        });

        modularBuildingButton.gameObject.SetActive(ModularBuildingManager.singleton && ModularBuildingManager.singleton.selectedPiece);
        modularBuildingButton.onClick.SetListener(() =>
        {
            Instantiate(GeneralManager.singleton.modularBuildingManager, GeneralManager.singleton.canvas);
        });

        badgeButton.interactable = NetworkTime.time >= player.nextRiskyActionTime;
        carManager.SetActive(player.playerCar._car != null);
        ammoPanel.SetActive(!player.playerBuilding.building && !player.playerCar.car);
        ammoSelector.gameObject.SetActive(player.equipment[0].amount > 0 && ((EquipmentItem)player.equipment[0].item.data).ammoItems.Count > 0);
        ammoSelectorShadow.gameObject.SetActive(ammoSelector.gameObject.activeInHierarchy);
        reloadButton.gameObject.SetActive(player.equipment[0].amount > 0 && ((EquipmentItem)player.equipment[0].item.data).ammoItems.Count > 0);
        reloadShadow.gameObject.SetActive(reloadButton.gameObject.activeInHierarchy);
        reloadText.text = player.equipment[0].amount > 0 && ((EquipmentItem)player.equipment[0].item.data).ammoItems.Count > 0 ? reloadText.text = (((EquipmentItem)player.playerItemEquipment.firstWeapon.item.data).chargeMunition.Get(player.playerItemEquipment.firstWeapon.item.chargeLevel) - player.equipment[0].item.alreadyShooted) + " / " + ((EquipmentItem)player.playerItemEquipment.firstWeapon.item.data).chargeMunition.Get(player.playerItemEquipment.firstWeapon.item.chargeLevel) : string.Empty;
        durabilitySlider.value = DurabilityPercent();
        durabilitySlider.gameObject.SetActive(player.playerItemEquipment.firstWeapon.amount > 0);

        if (target && target is Building && !(target is Tree) && !(target is Rock))
        {
            building = ((Building)target);
        }
        else
        {
            building = null;
        }

        destroyBuildingButton.gameObject.SetActive(target && building && player.CanInteractBuildingTarget(building, player) == true);
        explodeButton.gameObject.SetActive(target && building && player.CanInteractBuildingTarget(building, player) == true && GeneralManager.singleton.CanManageExplosiveBuilding(building, player) && (target.GetComponent<Dynamite>() || target.GetComponent<Mine>()));


        if (player.playerMove.sneak)
            sneakButton.targetGraphic.color = Color.green;
        else
            sneakButton.targetGraphic.color = Color.white;

        if (player.playerMove.run)
            runButton.targetGraphic.color = Color.green;
        else
            runButton.targetGraphic.color = Color.white;

    }

    public float DurabilityPercent()
    {
        if (player.playerItemEquipment.firstWeapon.amount > 0 && ((WeaponItem)player.playerItemEquipment.firstWeapon.item.data).maxDurability.baseValue == 0)
            return 1.0f;
        if (player.playerItemEquipment.firstWeapon.amount > 0 && ((WeaponItem)player.playerItemEquipment.firstWeapon.item.data).maxDurability.baseValue > 0)
            return (float)player.playerItemEquipment.firstWeapon.item.durability / (float)((WeaponItem)player.playerItemEquipment.firstWeapon.item.data).maxDurability.Get(player.playerItemEquipment.firstWeapon.item.durabilityLevel);

        return 1.0f;
    }




}

public partial class UISelectedAmmo : MonoBehaviour
{
    public EquipmentItem weapon;
    private Player player;
    public GameObject ammoToSpawn;
    public Transform content;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.equipment[0].amount > 0) weapon = ((EquipmentItem)player.equipment[0].item.data);

        if (weapon)
        {
            UIUtils.BalancePrefabs(ammoToSpawn, weapon.ammoItems.Count, content);
            for (int i = 0; i < weapon.ammoItems.Count; i++)
            {
                int index = i;
                AmmoSlot slot = content.GetChild(index).GetComponent<AmmoSlot>();
                slot.ammoImage.sprite = weapon.ammoItems[index].image;
                slot.ammoName.text = weapon.ammoItems[index].name;
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    slot.ammoAmount.text = "Muniz. : " + player.InventoryCount(new Item(weapon.ammoItems[index]));
                }
                else
                {
                    slot.ammoAmount.text = "Munit. : " + player.InventoryCount(new Item(weapon.ammoItems[index]));
                }
                slot.notSelectedImage.gameObject.SetActive(player.InventoryCount(new Item(weapon.ammoItems[index])) == 0);
                slot.buttonSelectAmmo.interactable = (player.InventoryCount(new Item(weapon.ammoItems[index])) > 0);
                slot.buttonSelectAmmo.onClick.SetListener(() =>
                {
                    player.CmdSetAmmo(slot.ammoName.text);
                    UIAttackManager.singleton.ammoSelector.onClick.Invoke();
                });
            }
        }
        else
        {
            UIUtils.BalancePrefabs(ammoToSpawn, 0, content);
            UIAttackManager.singleton.ammoItem = null;
        }

    }
}

public partial class UIPremiumZone
{
    private Player player;
    public Button teleportToPremiumZone;
    public Button ticketPremiumZone;

    TimeSpan difference;
    long seconds = 0;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating(nameof(CheckSeconds), 2.0f, 2.0f);
    }

    public void CheckSeconds()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        seconds = 0;
        if (player.playerBoost.networkBoost.Count > 0)
        {
            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer))
            {
                difference = DateTime.Parse(player.playerBoost.networkBoost[0].hiddenIslandTimer.ToString()) - DateTime.Now;
                seconds = Convert.ToInt64(difference.TotalSeconds);
            }
            else
                seconds = 0;
        }
    }

    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        teleportToPremiumZone.onClick.SetListener(() =>
        {
            if (LoadingScreen.singleton.canTeleport)
            {
                if (player.playerPremiumZoneManager.inPremiumZone && player.playerCar._car != null) return;

                if (!player.playerPremiumZoneManager.inPremiumZone)
                {
                    if (Convert.ToInt32(difference.TotalSeconds) > 0)
                    {
                        LoadingScreen.singleton.animation.Play("LoadingPremium");
                    }
                }
                else
                {
                    LoadingScreen.singleton.animation.Play("LoadingPremium");
                }

            }
        });
        ticketPremiumZone.onClick.SetListener(() =>
        {
            UINotificationManager.singleton.SpawnPremiumObject();
        });


        ticketPremiumZone.gameObject.SetActive(player && player.playerBoost.networkBoost.Count > 0 && seconds > 0);
        ticketPremiumZone.image.sprite = GeneralManager.singleton.ticketImage;
        teleportToPremiumZone.gameObject.SetActive(player && player.playerBoost.networkBoost.Count > 0 && seconds > 0);
    }

    public void CallTeleport()
    {
        player.playerPremiumZoneManager.CmdMoveToPremiumZone(GeneralManager.singleton.premiumZoneSpawn.position);
    }
}

public partial class UIRadioTorchManager
{
    private Player player;
    public Button buttonManageTorch;
    public TextMeshProUGUI torchTXT;
    public Button buttonManageRadio;
    public TextMeshProUGUI radioTXT;

    public string status;

    void Start()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;
    }

    void Update()
    {
        buttonManageTorch.onClick.SetListener(() =>
        {
            player.playerTorch.CmdSetTorch();
        });

        buttonManageRadio.onClick.SetListener(() =>
        {
            player.playerRadio.CmdSetRadio();
        });

        buttonManageTorch.interactable = player.playerTorch.torchItem.amount > 0 && NetworkTime.time >= player.playerTorch.nextRiskyActionTime;
        if (player.equipment[9].amount > 0)
        {
            buttonManageTorch.image.sprite = player.equipment[9].item.image;
        }
        else
        {
            buttonManageTorch.image.sprite = GeneralManager.singleton.torchImg;
        }

        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
        {
            status = player.playerTorch.isOn ? "\nStato : ON" : "\nStato : OFF";
            torchTXT.text = player.playerTorch.torchItem.amount > 0 ? "Batteria : " + player.playerTorch.torchItem.item.torchCurrentBattery + " / " + ((ScriptableTorch)player.playerTorch.torchItem.item.data).currentBattery.Get(player.playerTorch.torchItem.item.batteryLevel) + status : "Nessuna radio equipaggiata";
        }
        else
        {
            status = player.playerTorch.isOn ? "\nStatus : ON" : "\nStatus : OFF";
            torchTXT.text = player.playerTorch.torchItem.amount > 0 ? "Battery : " + player.playerTorch.torchItem.item.torchCurrentBattery + " / " + ((ScriptableTorch)player.playerTorch.torchItem.item.data).currentBattery.Get(player.playerTorch.torchItem.item.batteryLevel) + status : "No torch equipped";
        }

        buttonManageRadio.interactable = player.playerRadio.radioItem.amount > 0 && NetworkTime.time >= player.playerRadio.nextRiskyActionTime;

        if (player.equipment[4].amount > 0)
        {
            buttonManageRadio.image.sprite = player.equipment[4].item.image;
        }
        else
        {
            buttonManageRadio.image.sprite = GeneralManager.singleton.radioImg;
        }

        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
        {
            status = player.playerRadio.isOn ? "\nStato : ON" : "\nStato : OFF";
            radioTXT.text = player.playerRadio.radioItem.amount > 0 ? "Batteria : " + player.playerRadio.radioItem.item.radioCurrentBattery + " / " + ((ScriptableRadio)player.playerRadio.radioItem.item.data).currentBattery.Get(player.playerRadio.radioItem.item.batteryLevel) + status : "Nessuna radio equipaggiata";
        }
        else
        {
            status = player.playerRadio.isOn ? "\nStatus : ON" : "\nStatus : OFF";
            radioTXT.text = player.playerRadio.radioItem.amount > 0 ? "Battery : " + player.playerRadio.radioItem.item.radioCurrentBattery + " / " + ((ScriptableRadio)player.playerRadio.radioItem.item.data).currentBattery.Get(player.playerRadio.radioItem.item.batteryLevel) + status : "Nessuna radio equipaggiata";
        }

    }
}

public partial class UISpawnpoint
{
    private Player player;
    public static UISpawnpoint singleton;

    public TextMeshProUGUI description;
    public Button spawnHere;
    public Button spawnSomewhere;
    public GameObject createSpawnpoint;
    public GameObject setSpawnpoint;
    public Button buttonCreateSpawnpoint;
    public Button cancelSpawnpoint;
    public Button confirmSpawnpoint;
    public Button preferedSpawnpoint;
    public InputField inputFieldSpawnpoint;

    public GameObject spawnpoint;
    public Transform spawnpointContent;

    public Button buttonClose;

    public int possibleSpawnpoint;
    public bool prefered;
    public int spawnpointAbility;

    void Start()
    {
        if (!singleton) singleton = this;
    }

    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        spawnpointAbility = Convert.ToInt32(GeneralManager.singleton.FindNetworkAbilityLevel(player.playerSpawnpoint.ability.name, player.name)/10);
        possibleSpawnpoint = spawnpointAbility - player.playerSpawnpoint.spawnpoint.Count;
        createSpawnpoint.SetActive(possibleSpawnpoint > 0);

        spawnHere.interactable = player.health <= 0 && player.InventoryCount(new Item(GeneralManager.singleton.Instantresurrect)) > 0;
        spawnSomewhere.interactable = player.health <= 0;
        buttonCreateSpawnpoint.interactable = (possibleSpawnpoint > 0);
        createSpawnpoint.gameObject.SetActive(!setSpawnpoint.gameObject.activeSelf && possibleSpawnpoint > 0);
   
        if (prefered)
        {
            preferedSpawnpoint.image.sprite = GeneralManager.singleton.prefered;
        }
        else
        {
            preferedSpawnpoint.image.sprite = GeneralManager.singleton.notPrefered;
        }

        confirmSpawnpoint.interactable = inputFieldSpawnpoint.text != string.Empty && possibleSpawnpoint > 0;

        if (possibleSpawnpoint < 0) possibleSpawnpoint = 0;

        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
        {
            description.text = GeneralManager.singleton.messageInDescriptionIta + "\nPuoi impostare ancora : " + possibleSpawnpoint + " punti di rinascita.";
        }
        else
        {
            description.text = GeneralManager.singleton.messageInDescription + "\nYou can set other : " + possibleSpawnpoint + " spawnpoint.";
        }

        UIUtils.BalancePrefabs(spawnpoint, player.playerSpawnpoint.spawnpoint.Count, spawnpointContent);
        for (int i = 0; i < player.playerSpawnpoint.spawnpoint.Count; i++)
        {
            int index = i;
            SpawnpointSlot slot = spawnpointContent.GetChild(index).GetComponent<SpawnpointSlot>();
            slot.prefered = player.playerSpawnpoint.spawnpoint[index].prefered;
            slot.preferButton.image.sprite = player.playerSpawnpoint.spawnpoint[index].prefered ? GeneralManager.singleton.prefered : GeneralManager.singleton.notPrefered;
            slot.preferButton.onClick.SetListener(() =>
            {
                if (!player.isServer) player.playerSpawnpoint.CmdSetPrefered(player.playerSpawnpoint.spawnpoint[index].name);
                else player.playerSpawnpoint.SetPrefered(player.playerSpawnpoint.spawnpoint[index].name);

            });

            slot.spawnpointTitle.GetComponentInChildren<TextMeshProUGUI>().text = player.playerSpawnpoint.spawnpoint[index].name;
            slot.spawnpointTitle.interactable = (player.health == 0);
            slot.spawnpointTitle.onClick.SetListener(() =>
            {
                player.playerSpawnpoint.CmdSpawnAtPoint(player.playerSpawnpoint.spawnpoint[index].spawnPositionx, player.playerSpawnpoint.spawnpoint[index].spawnPositiony);
                buttonClose.onClick.Invoke();
            });

            slot.deleteButton.onClick.SetListener(() =>
            {
                player.playerSpawnpoint.CmdDeleteSpawnpoint(player.playerSpawnpoint.spawnpoint[index].name);
            });
        }

        buttonClose.onClick.SetListener(() =>
        {
            if (player.health > 0)
            {
                Destroy(this.gameObject);
            }
        });

        spawnHere.onClick.SetListener(() =>
        {
            player.playerSpawnpoint.CmdSpawnpointRevive(1.0f);
            Destroy(this.gameObject);
        });

        spawnSomewhere.onClick.SetListener(() =>
        {
            player.playerSpawnpoint.CmdSpawnSomewhere();
            Destroy(this.gameObject);
        });

        buttonCreateSpawnpoint.onClick.SetListener(() =>
        {
            createSpawnpoint.SetActive(false);
            inputFieldSpawnpoint.text = string.Empty;
            preferedSpawnpoint.image.sprite = GeneralManager.singleton.notPrefered;
            setSpawnpoint.SetActive(true);
        });

        cancelSpawnpoint.onClick.SetListener(() =>
        {
            inputFieldSpawnpoint.text = string.Empty;
            preferedSpawnpoint.image.sprite = GeneralManager.singleton.notPrefered;
            createSpawnpoint.SetActive(true);
            setSpawnpoint.SetActive(false);
        });

        confirmSpawnpoint.onClick.SetListener(() =>
        {
            player.playerSpawnpoint.CmdSetSpawnpoint(inputFieldSpawnpoint.text, player.transform.position.x, player.transform.position.y, prefered, player.name);
            prefered = false;
            cancelSpawnpoint.onClick.Invoke();
        });

        preferedSpawnpoint.onClick.SetListener(() =>
        {
            prefered = !prefered;
        });
    }
}

public partial class UIChestLoot
{
    public static UIChestLoot singleton;
    public UILootSlot itemSlotPrefab;
    public Transform content;
    public Button closeButton;
    public Button goldButton;
    public TextMeshProUGUI goldText;

    public UIChestLoot()
    {
        if (singleton == null) singleton = this;
    }

    void Update()
    {
        Player player = Player.localPlayer;

        closeButton.onClick.SetListener(() =>
        {
            this.gameObject.SetActive(false);
        });

        if (!player) return;

        if (player.health == 0)
            closeButton.onClick.Invoke();

        if (player != null &&
            player.target != null &&
            Utils.ClosestDistance(player.collider, player.target.collider) <= player.interactionRange &&
            player.target is Chest)
        {
            goldText.text = player.target.gold.ToString();
            goldButton.onClick.SetListener(() => {

                player.CmdTakeLootGoldFromChest();
            });
            List<ItemSlot> items = player.target.inventory.Where(slot => slot.amount > 0).ToList();
            UIUtils.BalancePrefabs(itemSlotPrefab.gameObject, items.Count, content);

            for (int i = 0; i < items.Count; i++)
            {
                int index = i;
                UILootSlot slot = content.GetChild(i).GetComponent<UILootSlot>();
                slot.button.interactable = player.InventoryCanAdd(items[index].item, items[index].amount);
                slot.button.onClick.SetListener(() =>
                {
                    player.CmdTakeLootItemFromChest(index);
                });
                slot.backgroundImage.color = Color.white;
                slot.itemImage.sprite = items[index].item.image;
                slot.nameText.text = items[index].item.name;
                slot.amountOverlay.SetActive(items[index].amount > 1);
                slot.amountText.text = items[index].amount.ToString();
                slot.backgroundImage.sprite = GffItemRarity.singleton.rarityType();
                slot.backgroundImage.color = GffItemRarity.singleton.rarityColor(true, player.target.inventory[index].item);

            }
        }
    }
}


public partial class UIPlayerTrading
{
    public UIPlayerTradingSlot slotPrefab;

    public Transform otherContent;
    public TextMeshProUGUI otherStatusText;
    public InputField otherGoldInput;

    public Transform myContent;
    public TextMeshProUGUI myStatusText;
    public InputField myGoldInput;

    public Button lockButton;
    public Button acceptButton;
    public Button cancelButton;

    public GameObject panel;

    public TextMeshProUGUI goldText;
    public TextMeshProUGUI coinText;

    public Player player;

    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        // only if trading, otherwise set inactive
        if (player != null &&
            player.state == "TRADING" &&
            player.target != null &&
            player.target is Player other)
        {


            goldText.text = player.gold.ToString();
            coinText.text = player.coins.ToString();
            panel.SetActive(true);
            // OTHER ///////////////////////////////////////////////////////////
            // status text
            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                if (other.tradeStatus == TradeStatus.Accepted) otherStatusText.text = "[ACCETTATA]";
                else if (other.tradeStatus == TradeStatus.Locked) otherStatusText.text = "[BLOCCATA]";
                else otherStatusText.text = "";
            }
            else
            {
                if (other.tradeStatus == TradeStatus.Accepted) otherStatusText.text = "[ACCEPTED]";
                else if (other.tradeStatus == TradeStatus.Locked) otherStatusText.text = "[LOCKED]";
                else otherStatusText.text = "";
            }

            // gold input
            otherGoldInput.text = other.tradeOfferGold.ToString();

            // items
            UIUtils.BalancePrefabs(slotPrefab.gameObject, other.tradeOfferItems.Count, otherContent);
            for (int i = 0; i < other.tradeOfferItems.Count; ++i)
            {
                UIPlayerTradingSlot slot = otherContent.GetChild(i).GetComponent<UIPlayerTradingSlot>();
                int inventoryIndex = other.tradeOfferItems[i];

                slot.dragAndDropable.dragable = false;
                slot.dragAndDropable.dropable = false;

                if (0 <= inventoryIndex && inventoryIndex < other.inventory.Count &&
                    other.inventory[inventoryIndex].amount > 0)
                {
                    ItemSlot itemSlot = other.inventory[inventoryIndex];

                    // refresh valid item

                    // only build tooltip while it's actually shown. this
                    // avoids MASSIVE amounts of StringBuilder allocations.
                    slot.tooltip.enabled = true;
                    if (slot.tooltip.IsVisible())
                        slot.tooltip.text = itemSlot.ToolTip();
                    slot.image.color = Color.white;
                    slot.image.sprite = itemSlot.item.image;
                    slot.amountOverlay.SetActive(itemSlot.amount > 1);
                    slot.amountText.text = itemSlot.amount.ToString();
                    slot.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                    slot.GetComponent<Image>().color = GffItemRarity.singleton.rarityColor(true, itemSlot.item);
                }
                else
                {
                    // refresh invalid item
                    slot.tooltip.enabled = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                    slot.amountOverlay.SetActive(false);
                }
            }

            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                // SELF ////////////////////////////////////////////////////////////
                // status text
                if (player.tradeStatus == TradeStatus.Accepted) myStatusText.text = "[ACCETTATA]";
                else if (player.tradeStatus == TradeStatus.Locked) myStatusText.text = "[BLOCCATA]";
                else myStatusText.text = "";
            }
            else
            {
                if (player.tradeStatus == TradeStatus.Accepted) myStatusText.text = "[ACCEPTED]";
                else if (player.tradeStatus == TradeStatus.Locked) myStatusText.text = "[LOCKED]";
                else myStatusText.text = "";

            }
            // gold input
            if (player.tradeStatus == TradeStatus.Free)
            {
                myGoldInput.interactable = true;
                myGoldInput.onValueChanged.SetListener(val =>
                {
                    long goldOffer = Utils.Clamp(val.ToLong(), 0, player.gold);
                    myGoldInput.text = goldOffer.ToString();
                    player.CmdTradeOfferGold(goldOffer);
                });
            }
            else
            {
                myGoldInput.interactable = false;
                myGoldInput.text = player.tradeOfferGold.ToString();
            }

            // items
            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.tradeOfferItems.Count, myContent);
            for (int i = 0; i < player.tradeOfferItems.Count; ++i)
            {
                UIPlayerTradingSlot slot = myContent.GetChild(i).GetComponent<UIPlayerTradingSlot>();
                slot.dragAndDropable.name = i.ToString(); // drag and drop index
                int inventoryIndex = player.tradeOfferItems[i];

                if (0 <= inventoryIndex && inventoryIndex < player.inventory.Count &&
                    player.inventory[inventoryIndex].amount > 0)
                {
                    ItemSlot itemSlot = player.inventory[inventoryIndex];

                    // refresh valid item

                    // only build tooltip while it's actually shown. this
                    // avoids MASSIVE amounts of StringBuilder allocations.
                    slot.tooltip.enabled = true;
                    if (slot.tooltip.IsVisible())
                        slot.tooltip.text = itemSlot.ToolTip();
                    slot.dragAndDropable.dragable = player.tradeStatus == TradeStatus.Free;
                    slot.image.color = Color.white;
                    slot.image.sprite = itemSlot.item.image;
                    slot.amountOverlay.SetActive(itemSlot.amount > 1);
                    slot.amountText.text = itemSlot.amount.ToString();
                }
                else
                {
                    // refresh invalid item
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                    slot.amountOverlay.SetActive(false);
                }
            }

            // buttons /////////////////////////////////////////////////////////
            // lock
            lockButton.interactable = player.tradeStatus == TradeStatus.Free;

            // accept (only if both have locked the trade & if not accepted yet)
            // accept (if not accepted yet & other has locked or accepted)
            acceptButton.interactable = player.tradeStatus == TradeStatus.Locked &&
                                        other.tradeStatus != TradeStatus.Free;

            // cancel
            lockButton.onClick.SetListener(() =>
            {
                player.CmdTradeOfferLock();
            });

            acceptButton.onClick.SetListener(() =>
            {
                player.CmdTradeOfferAccept();
            });

            cancelButton.onClick.SetListener(() =>
            {
                player.CmdTradeCancel();
            });

        }
        else
        {
            panel.SetActive(false);
            myGoldInput.text = "0"; // reset
        }
    }
}


public partial class UIStatistics
{
    public static UIStatistics singleton;
    public GameObject slotToInstantiate;
    public Transform content;
    private Player player;
    // Start is called before the first frame update
    void Start()
    {
        if (!singleton) singleton = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        UIUtils.BalancePrefabs(slotToInstantiate, 16, content);
        for (int i = 0; i < 16; i++)
        {
            int index = i;
            StatisticSlot slot = content.GetChild(index).GetComponent<StatisticSlot>();
            ManageStatType(slot, index);
        }
    }


    public void ManageStatType(StatisticSlot slot, int index)
    {
        if (index == 0) { slot.armor = true; return; }
        if (index == 1) { slot.health = true; return; }
        //if (index == 2) { slot.damage = true; return; }
        if (index == 2) { slot.adrenaline = true; return; }
        if (index == 3) { slot.defense = true; return; }
        if (index == 4) { slot.accuracy = true; return; }
        if (index == 5) { slot.miss = true; return; }
        if (index == 6) { slot.critPerc = true; return; }
        if (index == 7) { slot.weight = true; return; }
        if (index == 8) { slot.poisoned = true; return; }
        if (index == 9) { slot.hungry = true; return; }
        if (index == 10) { slot.thirsty = true; return; }
        if (index == 11) { slot.blood = true; return; }
        if (index == 12) { slot.marriage = true; return; }
        if (index == 13) { slot.defenseBonusPerc = true; return; }
        if (index == 14) { slot.manaBonusPerc = true; return; }
        if (index == 15) { slot.healthBonusPerc = true; return; }
    }
}

public partial class UIRemovePartner
{
    public static UIRemovePartner singleton;
    public TextMeshProUGUI description;
    public Button deleteButton;
    public Button closeItemButton;

    private Player player;

    public void Start()
    {
        if (!singleton) singleton = this;
    }

    public void Update()
    {

        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.health == 0)
            Destroy(gameObject);

        closeItemButton.onClick.SetListener(() =>
        {
            Destroy(gameObject);
        });

        deleteButton.onClick.SetListener(() =>
        {
            player.playerMarriage.CmdRemovePartner();
            closeItemButton.onClick.Invoke();
        });

        description.text = GeneralManager.singleton.languagesManager.defaultLanguages == "Italian" ? "Vuoi interrrompere la relazione con \n " + player.playerMarriage.partnerName + " ? " : "Do you really want end relation with \n" + player.playerMarriage.partnerName + " ? ";


    }

}

public partial class UIFriends
{
    public Player player;
    public static UIFriends singleton;
    public Transform requestTransform;
    public Transform friendTransform;

    public GameObject requestFriendSlot;
    public GameObject friendSlot;

    public SeeFriendSlot requestPanel;
    public SeeFriendSlot friendPanel;

    public Player selectedFriend;
    public NetworkManagerMMO manager;

    public int selectedRequest = -1;

    void Start()
    {
        if (!singleton) singleton = this;
        manager = FindObjectOfType<NetworkManagerMMO>();
    }

    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.health == 0) UIOrderManager.singleton.btnCloseLastPanel.onClick.Invoke();

        UIUtils.BalancePrefabs(requestFriendSlot, player.playerFriend.playerRequest.Count, requestTransform);
        for (int i = 0; i < player.playerFriend.playerRequest.Count; i++)
        {
            int indexi = i;
            RequestSlot slot = requestTransform.GetChild(indexi).GetComponent<RequestSlot>();
            Player requestOnlinePlayer;
            if (Player.onlinePlayers.TryGetValue(player.playerFriend.playerRequest[indexi], out requestOnlinePlayer))
            {
            }

            if (!requestOnlinePlayer)
            {
                slot.online.color = GeneralManager.singleton.offlineColor;
                slot.friendRequestName.text = player.playerFriend.playerRequest[indexi];
                slot.friendRequestAccept.interactable = false;
                slot.friendRequestRemove.onClick.SetListener(() =>
                {
                    player.playerFriend.CmdRemoveRequestFriends(player.playerFriend.playerRequest[indexi]);
                });
                //slot.seeFriend.onClick.SetListener(() =>
                //{
                //    Destroy(selectedFriend);
                //    selectedFriend = null;
                //    selectedRequest = 0;
                //    player.playerFriend.CmdLoadFriendStat(player.playerFriend.playerRequest[indexi], 0);
                //});
                requestPanel.closeButton.onClick.SetListener(() =>
                {
                    requestPanel.gameObject.SetActive(false);
                    Destroy(selectedFriend);
                    selectedFriend = null;
                    selectedRequest = -1;
                    UICharacterCreationCustom.singleton.maleCharacterCustomization.gameObject.SetActive(false);
                    UICharacterCreationCustom.singleton.femaleCharacterCustomization.gameObject.SetActive(false);
                });
            }
            else
            {
                slot.online.color = GeneralManager.singleton.onlineColor;
                slot.friendRequestName.text = player.playerFriend.playerRequest[indexi];
                slot.friendRequestAccept.interactable = true;
                slot.friendRequestAccept.onClick.SetListener(() =>
                {
                    player.playerFriend.CmdAcceptFriends(player.playerFriend.playerRequest[indexi]);
                });
                slot.friendRequestRemove.onClick.SetListener(() =>
                {
                    player.playerFriend.CmdRemoveRequestFriends(player.playerFriend.playerRequest[indexi]);
                });
                slot.seeFriend.onClick.SetListener(() =>
                {
                    Destroy(selectedFriend);
                    selectedFriend = null;
                    selectedRequest = 0;
                    requestPanel.gameObject.SetActive(true);
                    if (requestOnlinePlayer.playerCreation.sex == 0)
                    {
                        UICharacterCreationCustom.singleton.maleCharacterCustomization.gameObject.SetActive(true);
                        UICharacterCreationCustom.singleton.femaleCharacterCustomization.gameObject.SetActive(false);
                        BuildFriendPreview(requestOnlinePlayer, UICharacterCreationCustom.singleton.maleCharacterCustomization.gameObject);
                    }
                    else
                    {
                        UICharacterCreationCustom.singleton.maleCharacterCustomization.gameObject.SetActive(false);
                        UICharacterCreationCustom.singleton.femaleCharacterCustomization.gameObject.SetActive(true);
                        BuildFriendPreview(requestOnlinePlayer, UICharacterCreationCustom.singleton.femaleCharacterCustomization.gameObject);
                    }
                    requestPanel.friendName.text = "[" + requestOnlinePlayer.level + "] " + player.playerFriend.playerRequest[indexi];
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        requestPanel.health.text = "Vita : " + requestOnlinePlayer.health + " / " + requestOnlinePlayer.healthMax;
                        requestPanel.stamina.text = "Stamina : " + requestOnlinePlayer.mana + " / " + requestOnlinePlayer.manaMax;
                        requestPanel.defense.text = "Difesa : " + requestOnlinePlayer.defense;
                        requestPanel.partner.text = "Partner : " + requestOnlinePlayer.playerMarriage.partnerName;
                        requestPanel.guildName.text = "Gruppo : " + requestOnlinePlayer.guild.name;
                    }
                    else
                    {
                        requestPanel.health.text = "Health : " + requestOnlinePlayer.health + " / " + requestOnlinePlayer.healthMax;
                        requestPanel.stamina.text = "Stamina : " + requestOnlinePlayer.mana + " / " + requestOnlinePlayer.manaMax;
                        requestPanel.defense.text = "Defense : " + requestOnlinePlayer.defense;
                        requestPanel.partner.text = "Partner : " + requestOnlinePlayer.playerMarriage.partnerName;
                        requestPanel.guildName.text = "Group : " + requestOnlinePlayer.guild.name;
                    }
                    requestPanel.closeButton.onClick.SetListener(() =>
                    {
                        requestPanel.gameObject.SetActive(false);
                        Destroy(selectedFriend);
                        selectedFriend = null;
                        selectedRequest = -1;
                    });
                    UIUtils.BalancePrefabs(requestPanel.guildSlot, requestOnlinePlayer.playerAlliance.guildAlly.Count, requestPanel.content);
                    for (int g = 0; g < requestOnlinePlayer.playerAlliance.guildAlly.Count; g++)
                    {
                        GroupSlot groupSlot = requestPanel.content.GetChild(g).GetComponent<GroupSlot>();
                        groupSlot.statName.text = requestOnlinePlayer.playerAlliance.guildAlly[g];
                        groupSlot.statButton.onClick.SetListener(() =>
                        {
                            player.playerAlliance.CmdLoadGuild(requestOnlinePlayer.playerAlliance.guildAlly[g]);
                            UIOrderManager.singleton.SingleInstantePanel(requestPanel.groupPanelToInstantiate);
                            UIOrderManager.singleton.SingleInstantePanel(requestPanel.groupPanelToInstantiate).gameObject.GetComponent<UIGuild>().notMyGroup = true;
                        });
                    }
                });
            }


        }



        UIUtils.BalancePrefabs(friendSlot, player.playerFriend.playerFriends.Count, friendTransform);
        for (int i = 0; i < player.playerFriend.playerFriends.Count; i++)
        {
            int index = i;
            FriendSlot slot = friendTransform.GetChild(index).GetComponent<FriendSlot>();
            Player friendOnlinePlayer;
            if (Player.onlinePlayers.TryGetValue(player.playerFriend.playerFriends[index], out friendOnlinePlayer))
            {
            }

            if (!friendOnlinePlayer)
            {
                slot.online.color = GeneralManager.singleton.offlineColor;
                slot.playerFriendName.text = player.playerFriend.playerFriends[index];
                slot.removeFriend.onClick.SetListener(() =>
                {
                    player.playerFriend.CmdRemoveFriends(player.playerFriend.playerFriends[index]);
                });
                //slot.seeFriend.onClick.SetListener(() =>
                //{
                //    Destroy(selectedFriend);
                //    selectedFriend = null;
                //    selectedRequest = 1;
                //    player.playerFriend.CmdLoadFriendStat(player.playerFriend.playerFriends[index], 1);

                //    friendPanel.closeButton.onClick.SetListener(() =>
                //    {
                //        friendPanel.gameObject.SetActive(false);
                //        Destroy(selectedFriend);
                //        selectedFriend = null;
                //        selectedRequest = -1;
                //        UICharacterCreationCustom.singleton.maleCharacterCustomization.gameObject.SetActive(false);
                //        UICharacterCreationCustom.singleton.femaleCharacterCustomization.gameObject.SetActive(false);
                //    });
                //});

            }
            else
            {
                slot.online.color = GeneralManager.singleton.onlineColor;
                slot.playerFriendName.text = player.playerFriend.playerFriends[index];

                slot.removeFriend.onClick.SetListener(() =>
                {
                    player.playerFriend.CmdRemoveFriends(player.playerFriend.playerFriends[index]);
                });
                slot.messageFriend.onClick.SetListener(() =>
                {
                    GeneralManager.singleton.spawnedUIInvite = Instantiate(GeneralManager.singleton.friendMessageInvite, GeneralManager.singleton.canvas);
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        GeneralManager.singleton.spawnedUIInvite.GetComponent<UISendMessage>().sendTo.text = "Manda messaggio a : " + player.playerFriend.playerFriends[index];
                    }
                    else
                    {
                        GeneralManager.singleton.spawnedUIInvite.GetComponent<UISendMessage>().sendTo.text = "Send message to : " + player.playerFriend.playerFriends[index];

                    }
                    GeneralManager.singleton.spawnedUIInvite.GetComponent<UISendMessage>().receiver = player.playerFriend.playerFriends[index];
                });
                slot.partyFriend.interactable = !friendOnlinePlayer.InParty();
                slot.partyFriend.onClick.SetListener(() =>
                {
                    player.CmdPartyInvite(friendOnlinePlayer.name);
                });
                slot.guildFriend.interactable = !friendOnlinePlayer.InGuild() && player.InGuild();
                slot.guildFriend.onClick.SetListener(() =>
                {
                    player.CmdGuildInviteTargetFromFriends(player.playerFriend.playerFriends[index]);
                });
                slot.seeFriend.onClick.SetListener(() =>
                {
                    Destroy(selectedFriend);
                    selectedFriend = null;
                    selectedRequest = 1;
                    friendPanel.gameObject.SetActive(true);
                    if (friendOnlinePlayer.playerCreation.sex == 0)
                    {
                        UICharacterCreationCustom.singleton.maleCharacterCustomization.gameObject.SetActive(true);
                        UICharacterCreationCustom.singleton.femaleCharacterCustomization.gameObject.SetActive(false);
                        BuildFriendPreview(friendOnlinePlayer, UICharacterCreationCustom.singleton.maleCharacterCustomization.gameObject);
                    }
                    else
                    {
                        UICharacterCreationCustom.singleton.maleCharacterCustomization.gameObject.SetActive(false);
                        UICharacterCreationCustom.singleton.femaleCharacterCustomization.gameObject.SetActive(true);
                        BuildFriendPreview(friendOnlinePlayer, UICharacterCreationCustom.singleton.femaleCharacterCustomization.gameObject);
                    }
                    friendPanel.friendName.text = "[" + friendOnlinePlayer.level + "] " + player.playerFriend.playerFriends[index];
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        friendPanel.health.text = "Vita : " + friendOnlinePlayer.health + " / " + friendOnlinePlayer.healthMax;
                        friendPanel.stamina.text = "Stamina : " + friendOnlinePlayer.mana + " / " + friendOnlinePlayer.manaMax;
                        friendPanel.defense.text = "Difesa : " + friendOnlinePlayer.defense;
                        friendPanel.partner.text = "Partner : " + friendOnlinePlayer.playerMarriage.partnerName;
                        friendPanel.guildName.text = "Gruppo : " + friendOnlinePlayer.guild.name;
                    }
                    else
                    {
                        friendPanel.health.text = "Health : " + friendOnlinePlayer.health + " / " + friendOnlinePlayer.healthMax;
                        friendPanel.stamina.text = "Stamina : " + friendOnlinePlayer.mana + " / " + friendOnlinePlayer.manaMax;
                        friendPanel.defense.text = "Defense : " + friendOnlinePlayer.defense;
                        friendPanel.partner.text = "Partner : " + friendOnlinePlayer.playerMarriage.partnerName;
                        friendPanel.guildName.text = "Group : " + friendOnlinePlayer.guild.name;
                    }

                    friendPanel.closeButton.onClick.SetListener(() =>
                    {
                        friendPanel.gameObject.SetActive(false);
                        Destroy(selectedFriend);
                        selectedFriend = null;
                        selectedRequest = -1;
                        UICharacterCreationCustom.singleton.maleCharacterCustomization.gameObject.SetActive(false);
                        UICharacterCreationCustom.singleton.femaleCharacterCustomization.gameObject.SetActive(false);
                    });

                    UIUtils.BalancePrefabs(friendPanel.guildSlot, friendOnlinePlayer.playerAlliance.guildAlly.Count, friendPanel.content);
                    for (int g = 0; g < friendOnlinePlayer.playerAlliance.guildAlly.Count; g++)
                    {
                        GroupSlot groupSlot = friendPanel.content.GetChild(g).GetComponent<GroupSlot>();
                        groupSlot.statName.text = friendOnlinePlayer.playerAlliance.guildAlly[g];
                        groupSlot.statButton.onClick.SetListener(() =>
                        {
                            player.playerAlliance.CmdLoadGuild(friendOnlinePlayer.playerAlliance.guildAlly[g]);
                            UIOrderManager.singleton.SingleInstantePanel(friendPanel.groupPanelToInstantiate);
                            UIOrderManager.singleton.SingleInstantePanel(friendPanel.groupPanelToInstantiate).gameObject.GetComponent<UIGuild>().notMyGroup = true;
                        });
                    }

                });

            }
        }

        if (selectedRequest == 1)
        {
            if (selectedFriend != null)
            {
                friendPanel.gameObject.SetActive(true);
                requestPanel.gameObject.SetActive(false);
                friendPanel.friendName.text = "[" + selectedFriend.level + "] " + selectedFriend.name;
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    friendPanel.health.text = "Vita : " + selectedFriend.health + " / " + selectedFriend.healthMax;
                    friendPanel.stamina.text = "Stamina : " + selectedFriend.mana + " / " + selectedFriend.manaMax;
                    friendPanel.defense.text = "Difesa : " + selectedFriend.defense;
                    friendPanel.partner.text = "Partner : " + selectedFriend.playerMarriage.partnerName;
                    friendPanel.guildName.text = "Gruppo : " + selectedFriend.guild.name;
                }
                else
                {
                    friendPanel.health.text = "Health : " + selectedFriend.health + " / " + selectedFriend.healthMax;
                    friendPanel.stamina.text = "Stamina : " + selectedFriend.mana + " / " + selectedFriend.manaMax;
                    friendPanel.defense.text = "Defense : " + selectedFriend.defense;
                    friendPanel.partner.text = "Partner : " + selectedFriend.playerMarriage.partnerName;
                    friendPanel.guildName.text = "Group : " + selectedFriend.guild.name;
                }
                if (selectedFriend.playerCreation.sex == 0)
                {
                    UICharacterCreationCustom.singleton.maleCharacterCustomization.gameObject.SetActive(true);
                    UICharacterCreationCustom.singleton.femaleCharacterCustomization.gameObject.SetActive(false);
                    BuildFriendPreview(selectedFriend, UICharacterCreationCustom.singleton.maleCharacterCustomization.gameObject);
                }
                else
                {
                    UICharacterCreationCustom.singleton.maleCharacterCustomization.gameObject.SetActive(false);
                    UICharacterCreationCustom.singleton.femaleCharacterCustomization.gameObject.SetActive(true);
                    BuildFriendPreview(selectedFriend, UICharacterCreationCustom.singleton.femaleCharacterCustomization.gameObject);
                }
                UIUtils.BalancePrefabs(friendPanel.guildSlot, selectedFriend.playerAlliance.guildAlly.Count, friendPanel.content);
                for (int g = 0; g < selectedFriend.playerAlliance.guildAlly.Count; g++)
                {
                    GroupSlot groupSlot = friendPanel.content.GetChild(g).GetComponent<GroupSlot>();
                    groupSlot.statName.text = selectedFriend.playerAlliance.guildAlly[g];
                    groupSlot.statButton.onClick.SetListener(() =>
                    {
                        player.playerAlliance.CmdLoadGuild(selectedFriend.playerAlliance.guildAlly[g]);
                        UIOrderManager.singleton.SingleInstantePanel(friendPanel.groupPanelToInstantiate);
                        UIOrderManager.singleton.SingleInstantePanel(friendPanel.groupPanelToInstantiate).gameObject.GetComponent<UIGuild>().notMyGroup = true;
                    });
                }

            }
        }
        else if (selectedRequest == 0)
        {
            if (selectedFriend != null)
            {
                requestPanel.gameObject.SetActive(true);
                friendPanel.gameObject.SetActive(false);
                requestPanel.friendName.text = "[" + selectedFriend.level + "] " + selectedFriend.name;
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    requestPanel.health.text = "Vita : " + selectedFriend.health + " / " + selectedFriend.healthMax;
                    requestPanel.stamina.text = "Stamina : " + selectedFriend.mana + " / " + selectedFriend.manaMax;
                    requestPanel.defense.text = "Difesa : " + selectedFriend.defense;
                    requestPanel.partner.text = "Partner : " + selectedFriend.playerMarriage.partnerName;
                    requestPanel.guildName.text = "Gruppo : " + selectedFriend.guild.name;
                }
                else
                {
                    requestPanel.health.text = "Health : " + selectedFriend.health + " / " + selectedFriend.healthMax;
                    requestPanel.stamina.text = "Stamina : " + selectedFriend.mana + " / " + selectedFriend.manaMax;
                    requestPanel.defense.text = "Defense : " + selectedFriend.defense;
                    requestPanel.partner.text = "Partner : " + selectedFriend.playerMarriage.partnerName;
                    requestPanel.guildName.text = "Group : " + selectedFriend.guild.name;
                }

                if (selectedFriend.playerCreation.sex == 0)
                {
                    UICharacterCreationCustom.singleton.maleCharacterCustomization.gameObject.SetActive(true);
                    UICharacterCreationCustom.singleton.femaleCharacterCustomization.gameObject.SetActive(false);
                    BuildFriendPreview(selectedFriend, UICharacterCreationCustom.singleton.maleCharacterCustomization.gameObject);
                }
                else
                {
                    UICharacterCreationCustom.singleton.maleCharacterCustomization.gameObject.SetActive(false);
                    UICharacterCreationCustom.singleton.femaleCharacterCustomization.gameObject.SetActive(true);
                    BuildFriendPreview(selectedFriend, UICharacterCreationCustom.singleton.femaleCharacterCustomization.gameObject);
                }

                UIUtils.BalancePrefabs(requestPanel.guildSlot, selectedFriend.playerAlliance.guildAlly.Count, requestPanel.content);
                for (int g = 0; g < selectedFriend.playerAlliance.guildAlly.Count; g++)
                {
                    GroupSlot groupSlot = requestPanel.content.GetChild(g).GetComponent<GroupSlot>();
                    groupSlot.statName.text = selectedFriend.playerAlliance.guildAlly[g];
                    groupSlot.statButton.onClick.SetListener(() =>
                    {
                        player.playerAlliance.CmdLoadGuild(selectedFriend.playerAlliance.guildAlly[g]);
                        UIOrderManager.singleton.SingleInstantePanel(requestPanel.groupPanelToInstantiate);
                        UIOrderManager.singleton.SingleInstantePanel(requestPanel.groupPanelToInstantiate).gameObject.GetComponent<UIGuild>().notMyGroup = true;
                    });
                }
            }
        }
    }

    public void BuildFriendPreview(Player player, GameObject prefab)
    {
        CharacterCustomization characterCustomization = prefab.GetComponent<CharacterCustomization>();

        if (player.playerCreation.sex == 0)
        {
            characterCustomization.SetHairByIndex(player.playerCreation.hairType);
            characterCustomization.SetBeardByIndex(player.playerCreation.beard);
            Color newCol;
            if (ColorUtility.TryParseHtmlString(player.playerCreation.hairColor, out newCol))
                characterCustomization.SetBodyColor(BodyColorPart.Hair, newCol);
            if (ColorUtility.TryParseHtmlString(player.playerCreation.underwearColor, out newCol))
                characterCustomization.SetBodyColor(BodyColorPart.Underpants, newCol);
            if (ColorUtility.TryParseHtmlString(player.playerCreation.eyesColor, out newCol))
                characterCustomization.SetBodyColor(BodyColorPart.Eye, newCol);
            if (ColorUtility.TryParseHtmlString(player.playerCreation.skinColor, out newCol))
                characterCustomization.SetBodyColor(BodyColorPart.Skin, newCol);
            characterCustomization.SetBodyShape(BodyShapeType.Fat, player.playerCreation.fat);
            characterCustomization.SetBodyShape(BodyShapeType.Thin, player.playerCreation.thin);
            characterCustomization.SetBodyShape(BodyShapeType.Muscles, player.playerCreation.muscle);

            manager.DressSelectablePlayer(player, characterCustomization);

            characterCustomization.SetBodyShape(BodyShapeType.BreastSize, player.playerCreation.breast);


            characterCustomization.SetHeight(player.playerCreation.height);

        }
        else
        {
            characterCustomization.SetHairByIndex(player.playerCreation.hairType);
            characterCustomization.SetBeardByIndex(player.playerCreation.beard);
            Color newCol;
            if (ColorUtility.TryParseHtmlString(player.playerCreation.hairColor, out newCol))
                characterCustomization.SetBodyColor(BodyColorPart.Hair, newCol);
            if (ColorUtility.TryParseHtmlString(player.playerCreation.underwearColor, out newCol))
                characterCustomization.SetBodyColor(BodyColorPart.Underpants, newCol);
            if (ColorUtility.TryParseHtmlString(player.playerCreation.eyesColor, out newCol))
                characterCustomization.SetBodyColor(BodyColorPart.Eye, newCol);
            if (ColorUtility.TryParseHtmlString(player.playerCreation.skinColor, out newCol))
                characterCustomization.SetBodyColor(BodyColorPart.Skin, newCol);
            characterCustomization.SetBodyShape(BodyShapeType.Fat, player.playerCreation.fat);
            characterCustomization.SetBodyShape(BodyShapeType.Thin, player.playerCreation.thin);
            characterCustomization.SetBodyShape(BodyShapeType.Muscles, player.playerCreation.muscle);

            manager.DressSelectablePlayer(player, characterCustomization);

            characterCustomization.SetBodyShape(BodyShapeType.BreastSize, player.playerCreation.breast);


            characterCustomization.SetHeight(player.playerCreation.height);
        }
    }

}

public partial class UISendMessage
{
    public static UISendMessage singleton;
    public InputField inputField;
    public Button SendButton;
    public Button closeButton;
    public Text sendTo;
    public string receiver;

    private Player player;

    void Start()
    {
        if (!singleton) singleton = this;
    }

    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.health == 0)
            Destroy(GeneralManager.singleton.spawnedUIInvite);

        SendButton.interactable = inputField.text != string.Empty;
        SendButton.onClick.SetListener(() =>
        {
            player.GetComponent<PlayerChat>().CmdMsgWhisper(receiver, inputField.text);
            Destroy(GeneralManager.singleton.spawnedUIInvite);
        });
        closeButton.onClick.SetListener(() =>
        {
            Destroy(GeneralManager.singleton.spawnedUIInvite);
        });

    }

}

public partial class UIOptions
{
    //public TextMeshProUGUI blockTextMarriage;
    public Toggle blockMarriage;
    public Button buttonSliderMarriage;

    //public TextMeshProUGUI blockTextParty;
    public Toggle blockParty;
    public Button buttonSliderParty;

    //public TextMeshProUGUI blockTextGroup;
    public Toggle blockGroup;
    public Button buttonSliderGroup;

    //public TextMeshProUGUI blockTextAlly;
    public Toggle blockAlly;
    public Button buttonSliderAlly;

    //public TextMeshProUGUI blockTextTrade;
    public Toggle blockTrade;
    public Button buttonSliderTrade;

    //public TextMeshProUGUI blockTextFriend;
    public Toggle blockFriend;
    public Button buttonSliderFriend;


    //public TextMeshProUGUI blockTextFootstep;
    public Toggle blockFootstep;
    public Button buttonSliderFootstep;

    //public TextMeshProUGUI blockTextSound;
    public Toggle blockSound;
    public Button buttonSliderSound;

    public Toggle blockButtonSound;
    public Button buttonSliderButtonSound;

    public Button issueSuggestion;

    private Player player;

    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.health == 0)
        {
            UIOrderManager.singleton.btnCloseLastPanel.onClick.Invoke();
        }

        blockMarriage.isOn = player.playerOptions.blockMarriage;
        blockParty.isOn = player.playerOptions.blockParty;
        blockGroup.isOn = player.playerOptions.blockGroup;
        blockAlly.isOn = player.playerOptions.blockAlly;
        blockTrade.isOn = player.playerOptions.blockTrade;
        blockFriend.isOn = player.playerOptions.blockFriend;
        blockFootstep.isOn = player.playerOptions.blockFootstep;
        blockSound.isOn = player.playerOptions.blockSound;
        blockButtonSound.isOn = player.playerOptions.blockButtonSounds;
        buttonSliderMarriage.onClick.SetListener(() =>
        {
            if (NetworkTime.time >= player.nextRiskyActionTime)
                player.playerOptions.CmdBlockMarriage();
        });
        buttonSliderParty.onClick.SetListener(() =>
        {
            if (NetworkTime.time >= player.nextRiskyActionTime)
                player.playerOptions.CmdBlockParty();
        });

        buttonSliderGroup.onClick.SetListener(() =>
        {
            if (NetworkTime.time >= player.nextRiskyActionTime)
                player.playerOptions.CmdBlockGroup();
        });

        buttonSliderAlly.onClick.SetListener(() =>
        {
            if (NetworkTime.time >= player.nextRiskyActionTime)
                player.playerOptions.CmdBlockAlly();
        });

        buttonSliderTrade.onClick.SetListener(() =>
        {
            if (NetworkTime.time >= player.nextRiskyActionTime)
                player.playerOptions.CmdBlockTrade();
        });

        buttonSliderFriend.onClick.SetListener(() =>
        {
            if (NetworkTime.time >= player.nextRiskyActionTime)
                player.playerOptions.CmdBlockFriends();
        });

        buttonSliderFootstep.onClick.SetListener(() =>
        {
            if (NetworkTime.time >= player.nextRiskyActionTime)
                player.playerOptions.CmdBlockFootstep();
        });

        buttonSliderSound.onClick.SetListener(() =>
        {
            if (NetworkTime.time >= player.nextRiskyActionTime)
                player.playerOptions.CmdBlockSound();
        });

        buttonSliderButtonSound.onClick.SetListener(() =>
        {
            if (NetworkTime.time >= player.nextRiskyActionTime)
                player.playerOptions.CmdBlockButtonSound();
        });

        issueSuggestion.onClick.SetListener(() =>
        {
            Instantiate(GeneralManager.singleton.issueSuggestion, GeneralManager.singleton.canvas);
        });

    }
}

public partial class UIBugSuggestion
{
    public Button selectedIssueType;
    public Slider sliderISsue;
    public InputField description;
    public Button sendButton;
    public Button closeButton;

    public TextMeshProUGUI sendButtonText;

    public Player player;

    void Update()
    {
        if (Player.localPlayer.health == 0)
            Destroy(this.gameObject);

        if (sendButton.interactable)
        {
            if (LanguagesManager.singleton.defaultLanguages == "Italian")
            {
                sendButtonText.text = "Invia!";
            }
            else
            {
                sendButtonText.text = "Send!";
            }
        }
        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

        selectedIssueType.onClick.SetListener(() =>
        {
            if (sliderISsue.value == sliderISsue.minValue) { sliderISsue.value = sliderISsue.maxValue; return; }
            else
                sliderISsue.value = sliderISsue.minValue;
        });

        sendButton.onClick.SetListener(() =>
        {
            if (description.text != string.Empty)
            {
                sendButton.interactable = false;
                return;
            }

            if (sliderISsue.value == 0)
            {
                player.playerOptions.CmdSaveIssue(player.name, "Bug", description.text);
            }
            if (sliderISsue.value == 1)
            {
                player.playerOptions.CmdSaveIssue(player.name, "Suggestion", description.text);
            }

            if (LanguagesManager.singleton.defaultLanguages == "Italian")
            {
                sendButtonText.text = "Grazie!";
            }
            else
            {
                sendButtonText.text = "Thanks!";
            }
        });


    }
}

public partial class UIBuildingCrafting
{
    public GameObject itemToCraft;
    public GameObject itemIngredient;

    public Transform itemToCraftContent;
    public Transform ingredientContent;

    //public Button craftButton;
    public Button closeButton;
    public Button alreadyInCraftButton;

    public Button goldButton;
    public TextMeshProUGUI goldText;

    public Button gemsButton;
    public TextMeshProUGUI gemsText;

    public TextMeshProUGUI description;
    public TextMeshProUGUI moreLevelDescription;


    private Player player;

    public Building buildingTarget;
    public ScriptableItem selectedItem;

    public BuildingCraft buildingCraft;

    public List<CraftItem> progressItem = new List<CraftItem>();
    public List<CraftItem> finishedItem = new List<CraftItem>();
    private TimeSpan difference;

    public bool canCraft;
    private int selectedIndex;

    public void Start()
    {
        buildingTarget = Player.localPlayer.target.GetComponent<Building>();
        buildingCraft = buildingTarget.GetComponent<BuildingCraft>();
    }

    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;
        if (player.health == 0)
            Destroy(this.gameObject);
        if (!player.target) return;
        if (!buildingTarget) return;

        progressItem.Clear();
        finishedItem.Clear();

        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

        alreadyInCraftButton.onClick.SetListener(() =>
        {
            Instantiate(GeneralManager.singleton.alreadyCraftPanel, GeneralManager.singleton.canvas);
        });

        goldButton.onClick.SetListener(() =>
        {
            DateTime time = DateTime.Now;

            if (buildingCraft && buildingTarget && buildingTarget.isPremiumZone)
            {
                player.playerBuilding.CmdCraftItem(buildingTarget.building.name, selectedIndex, 0, time.ToString());
                itemToCraftContent.GetChild(selectedIndex).GetComponent<Button>().onClick.Invoke();
            }
            else if ((progressItem.Count < (buildingTarget.level - 1) || progressItem.Count == 0) && buildingCraft && buildingTarget && !buildingTarget.isPremiumZone)
            {
                player.playerBuilding.CmdCraftItem(buildingTarget.building.name, selectedIndex, 0, time.ToString());
                itemToCraftContent.GetChild(selectedIndex).GetComponent<Button>().onClick.Invoke();
            }
        });

        gemsButton.onClick.SetListener(() =>
        {
            DateTime time = DateTime.Now;

            player.playerBuilding.CmdCraftItem(buildingTarget.building.name, selectedIndex, 1, time.ToString());
            itemToCraftContent.GetChild(selectedIndex).GetComponent<Button>().onClick.Invoke();
        });

        for (int i = 0; i < buildingCraft.craftItem.Count; i++)
        {
            int index = i;
            difference = DateTime.Parse(buildingCraft.craftItem[index].timeEnd) - DateTime.Now;

            if (difference.TotalSeconds > 0)
            {
                progressItem.Add(new CraftItem());
            }

        }
        moreLevelDescription.gameObject.SetActive((progressItem.Count >= (buildingTarget.level - 1) && progressItem.Count != 0) && buildingCraft && buildingTarget && !buildingTarget.isPremiumZone);

        UIUtils.BalancePrefabs(itemToCraft, GeneralManager.singleton.FindItemToCraft(buildingTarget.building), itemToCraftContent);
        for (int i = 0; i < itemToCraftContent.childCount; i++)
        {
            int index = i;
            SlotIngredient slot = itemToCraftContent.GetChild(index).GetComponent<SlotIngredient>();
            if (GeneralManager.singleton.buildingItems[GeneralManager.singleton.ListOfBuildingItem(buildingTarget.building)].buildingItem[index].itemToCraft.item.image)
            {
                slot.image.sprite = GeneralManager.singleton.buildingItems[GeneralManager.singleton.ListOfBuildingItem(buildingTarget.building)].buildingItem[index].itemToCraft.item.image;
            }
            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                slot.ingredientName.text = GeneralManager.singleton.buildingItems[GeneralManager.singleton.ListOfBuildingItem(buildingTarget.building)].buildingItem[index].itemToCraft.item.italianName;
                slot.ingredientAmount.text = " x " + GeneralManager.singleton.buildingItems[GeneralManager.singleton.ListOfBuildingItem(buildingTarget.building)].buildingItem[index].itemToCraft.amount;
            }
            else
            {
                slot.ingredientName.text = GeneralManager.singleton.buildingItems[GeneralManager.singleton.ListOfBuildingItem(buildingTarget.building)].buildingItem[index].itemToCraft.item.name;
                slot.ingredientAmount.text = " x " + GeneralManager.singleton.buildingItems[GeneralManager.singleton.ListOfBuildingItem(buildingTarget.building)].buildingItem[index].itemToCraft.amount;

            }
            slot.slotButton.onClick.SetListener(() =>
            {
                selectedIndex = index;
                selectedItem = GeneralManager.singleton.buildingItems[GeneralManager.singleton.ListOfBuildingItem(buildingTarget.building)].buildingItem[index].itemToCraft.item;
                description.text = string.Empty;
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    description.text += GeneralManager.singleton.buildingItems[GeneralManager.singleton.ListOfBuildingItem(buildingTarget.building)].buildingItem[index].itemToCraft.item.italianName + "\n";
                    description.text += "Quantita' : " + GeneralManager.singleton.buildingItems[GeneralManager.singleton.ListOfBuildingItem(buildingTarget.building)].buildingItem[index].itemToCraft.amount + "\n";
                }
                else
                {
                    description.text += GeneralManager.singleton.buildingItems[GeneralManager.singleton.ListOfBuildingItem(buildingTarget.building)].buildingItem[index].itemToCraft.item.name + "\n";
                    description.text += "Amount : " + GeneralManager.singleton.buildingItems[GeneralManager.singleton.ListOfBuildingItem(buildingTarget.building)].buildingItem[index].itemToCraft.amount + "\n";
                }

                UIUtils.BalancePrefabs(itemIngredient, GeneralManager.singleton.buildingItems[GeneralManager.singleton.ListOfBuildingItem(buildingTarget.building)].buildingItem[index].craftablengredient.Count, ingredientContent);
                {
                    canCraft = true;
                    for (int e = 0; e < ingredientContent.childCount; e++)
                    {
                        int secondindex = e;
                        SlotIngredient ingredientSlot = ingredientContent.GetChild(secondindex).GetComponent<SlotIngredient>();
                        ingredientSlot.image.sprite = GeneralManager.singleton.buildingItems[GeneralManager.singleton.ListOfBuildingItem(buildingTarget.building)].buildingItem[index].craftablengredient[secondindex].item.image;
                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                        {
                            ingredientSlot.ingredientName.text = GeneralManager.singleton.buildingItems[GeneralManager.singleton.ListOfBuildingItem(buildingTarget.building)].buildingItem[index].craftablengredient[secondindex].item.italianName;
                        }
                        else
                        {
                            ingredientSlot.ingredientName.text = GeneralManager.singleton.buildingItems[GeneralManager.singleton.ListOfBuildingItem(buildingTarget.building)].buildingItem[index].craftablengredient[secondindex].item.name;
                        }
                        int invCount = player.InventoryCount(new Item(GeneralManager.singleton.buildingItems[GeneralManager.singleton.ListOfBuildingItem(buildingTarget.building)].buildingItem[index].craftablengredient[secondindex].item));
                        ingredientSlot.ingredientAmount.text = invCount + " / " + GeneralManager.singleton.buildingItems[GeneralManager.singleton.ListOfBuildingItem(buildingTarget.building)].buildingItem[index].craftablengredient[secondindex].amount.ToString();
                        if (invCount < GeneralManager.singleton.buildingItems[GeneralManager.singleton.ListOfBuildingItem(buildingTarget.building)].buildingItem[index].craftablengredient[secondindex].amount)
                            canCraft = false;
                    }
                }
                goldText.text = GeneralManager.singleton.buildingItems[GeneralManager.singleton.ListOfBuildingItem(buildingTarget.building)].buildingItem[index].itemToCraft.item.goldPrice.ToString();
                gemsText.text = GeneralManager.singleton.buildingItems[GeneralManager.singleton.ListOfBuildingItem(buildingTarget.building)].buildingItem[index].itemToCraft.item.coinPrice.ToString();
            });
        }


        goldButton.interactable = canCraft;

        gemsButton.interactable = canCraft;

    }
}

public partial class UICraftInProgress
{
    public GameObject gameObjectToSpawn;
    public Transform inProgressContent;
    public Transform completedContent;

    public Button switchButton;
    public Button closeButton;

    public bool personal;

    private Player player;
    private BuildingCraft buildingCraft;

    public List<CraftItem> progressItem = new List<CraftItem>();
    public List<CraftItem> finishedItem = new List<CraftItem>();

    private LanguageTranslator languageTranslator;
    private CraftItem runtimeItem;

    // Start is called before the first frame update
    void Start()
    {
        if (!Player.localPlayer.target) return;
        if (!Player.localPlayer.target.GetComponent<BuildingCraft>()) return;
        if (!buildingCraft) buildingCraft = Player.localPlayer.target.GetComponent<BuildingCraft>();

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
            Destroy(this.gameObject);

        if (!player.target) return;
        if (!buildingCraft) return;


        switchButton.onClick.SetListener(() =>
        {
            personal = !personal;
        });

        progressItem.Clear();
        finishedItem.Clear();

        for (int i = 0; i < buildingCraft.craftItem.Count; i++)
        {
            int index = i;
            TimeSpan difference = DateTime.Parse(buildingCraft.craftItem[index].timeEnd) - System.DateTime.Now;
            runtimeItem = new CraftItem();
            runtimeItem.index = index;
            runtimeItem.itemName = buildingCraft.craftItem[index].itemName;
            runtimeItem.amount = buildingCraft.craftItem[index].amount;
            runtimeItem.owner = buildingCraft.craftItem[index].owner;
            runtimeItem.guildName = player.guild.name;
            runtimeItem.timeBegin = buildingCraft.craftItem[index].timeBegin;
            runtimeItem.timeEnd = buildingCraft.craftItem[index].timeEnd;
            runtimeItem.timeEndServer = buildingCraft.craftItem[index].timeEndServer;
            player.playerLeaderPoints.craftItemPoint += GeneralManager.singleton.craftItemPoint;

            Debug.Log("Item name : " + runtimeItem.itemName + " difference seconds : " + difference.TotalSeconds);
            Debug.Log("Time end : " + buildingCraft.craftItem[index].timeEnd);

            if (difference.TotalSeconds > 0)
            {

                progressItem.Add(runtimeItem);
            }
            else
            {
                finishedItem.Add(runtimeItem);
            }

        }

        UIUtils.BalancePrefabs(gameObjectToSpawn, progressItem.Count, inProgressContent);
        for (int i = 0; i < progressItem.Count; i++)
        {
            int index = i;
            CraftProgressSlot slot = inProgressContent.GetChild(index).GetComponent<CraftProgressSlot>();

            Player insidePlayer;
            if (Player.onlinePlayers.TryGetValue(progressItem[index].owner, out insidePlayer))
            {
                if (insidePlayer)
                {
                    Color c = slot.onlinePlayer.color;
                    c = GeneralManager.singleton.onlineColor;
                    slot.onlinePlayer.color = c;

                    if (insidePlayer.InGuild())
                    {
                        slot.itemOwnerGuild.text = "Owner : " + insidePlayer.name + "   Group : " + insidePlayer.guild.name;
                    }
                    else
                    {
                        slot.itemOwnerGuild.text = "Owner : " + progressItem[index].owner + "   Group : " + progressItem[index].guildName;
                    }

                }
                else
                {
                    Color c = slot.onlinePlayer.color;
                    c = GeneralManager.singleton.offlineColor;
                    slot.onlinePlayer.color = c;
                    slot.itemOwnerGuild.text = "Owner : " + progressItem[index].owner + "   Group : " + progressItem[index].guildName;
                }
            }

            if (ScriptableItem.dict.TryGetValue(progressItem[index].itemName.GetStableHashCode(), out ScriptableItem itemData))
            {
                slot.itemImage.sprite = itemData.image;
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    slot.itemName.text = itemData.italianName + " x " + progressItem[index].amount;
                }
                else
                {
                    slot.itemName.text = progressItem[index].itemName + " x " + progressItem[index].amount;
                }

            }
            slot.GetItem.gameObject.GetComponent<Image>().enabled = false;
            TimeSpan difference = DateTime.Parse(progressItem[index].timeEnd) - System.DateTime.Now;
            if (Convert.ToInt32(difference.TotalSeconds) < 0)
            {
                slot.GetItem.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = GeneralManager.singleton.ConvertToTimer(0);
            }
            else
            {
                slot.GetItem.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds));
            }
        }

        UIUtils.BalancePrefabs(gameObjectToSpawn, finishedItem.Count, completedContent);
        for (int i = 0; i < completedContent.childCount; i++)
        {
            int index = i;
            if (index >= finishedItem.Count) continue;
            CraftProgressSlot slot = completedContent.GetChild(index).GetComponent<CraftProgressSlot>();
            slot.craftProgressIndex = index;

            Player insidePlayer;
            if (Player.onlinePlayers.TryGetValue(finishedItem[index].owner, out insidePlayer))
            {
                if (insidePlayer)
                {
                    Color c = slot.onlinePlayer.color;
                    c = GeneralManager.singleton.onlineColor;
                    slot.onlinePlayer.color = c;

                    if (insidePlayer.InGuild())
                    {
                        slot.itemOwnerGuild.text = "Owner : " + insidePlayer.name + "   Group : " + insidePlayer.guild.name;
                    }
                    else
                    {
                        slot.itemOwnerGuild.text = "Owner : " + finishedItem[index].owner + "   Group : " + finishedItem[index].guildName;
                    }

                }
                else
                {
                    Color c = slot.onlinePlayer.color;
                    c = GeneralManager.singleton.offlineColor;
                    slot.onlinePlayer.color = c;
                    slot.itemOwnerGuild.text = "Owner : " + finishedItem[index].owner + "   Group : " + finishedItem[index].guildName;
                }
            }

            if (ScriptableItem.dict.TryGetValue(finishedItem[index].itemName.GetStableHashCode(), out ScriptableItem itemData))
            {
                slot.itemImage.sprite = itemData.image;
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    slot.itemName.text = itemData.italianName + " x " + finishedItem[index].amount;
                }
                else
                {
                    slot.itemName.text = finishedItem[index].itemName + " x " + finishedItem[index].amount;
                }
                slot.GetItem.interactable = (finishedItem[index].owner == player.name && player.InventoryCanAdd(new Item(itemData), finishedItem[index].amount));
                slot.GetItem.GetComponentInChildren<LanguageTranslator>().enabled = true;

            }

            slot.GetItem.gameObject.GetComponent<Image>().enabled = true;

            slot.GetItem.onClick.SetListener(() =>
            {
                player.CmdAddToInventory(finishedItem[index].itemName, finishedItem[index].amount, finishedItem[index].owner, finishedItem[index].timeEnd, finishedItem[index].index);

            });
        }

        for (int i = 0; i < finishedItem.Count; i++)
        {
            int index = i;
            if (personal && player.name != finishedItem[index].owner)
            {
                completedContent.GetChild(index).gameObject.SetActive(false);
            }
            else if (!personal)
            {
                completedContent.GetChild(index).gameObject.SetActive(true);
            }
        }
    }

    public bool CanInteractCraftItem(Player player, string playerName, string guildName)
    {
        if (!player) return false;

        if (!player.InGuild() && playerName != string.Empty)
        {
            if (playerName == player.name)
            {
                return true;
            }
            return false;
        }
        if (!player.InGuild() && playerName == string.Empty)
        {
            return true;
        }
        if (player.InGuild())
        {
            if (guildName == player.guild.name)
            {
                return true;
            }
            if (guildName != player.guild.name)
            {
                if (player.playerAlliance.guildAlly.Contains(guildName))
                {
                    return true;
                }
                if (!player.playerAlliance.guildAlly.Contains(guildName))
                {
                    return false;
                }
            }
        }
        return false;
    }

}

public partial class UIQuests
{
    public KeyCode hotKey = KeyCode.Q;
    public Transform content;
    public UIQuestSlot slotPrefab;
    public GameObject slotDetailsPrefab;
    public UIQuestSlot questSlot;

    public string expandPrefix = "<b>+</b> ";
    public string hidePrefix = "<b>-</b> ";

    public bool created = false;

    public Button nameButton;
    public GameObject childObject;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI experienceText;
    public Transform itemContent;
    public Transform itemContentDetails;
    public UIInventorySlot toSpawn;
    public Button claimButton;

    public bool canClaim = true;
    public int selectedQuest = -1;

    public Quest quest;
    public ScriptableQuest scriptableQuest;

    public List<Quest> activeQuests = new List<Quest>();

    public Player player;

    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player != null)
        {
            claimButton.onClick.SetListener(() =>
            {
                int prevSelectedQuest = selectedQuest;
                if (activeQuests.Count - 1 == 0)
                {
                    Reset();
                }
                else
                {
                    if (selectedQuest == 0)
                    {
                        if (activeQuests.Count > 1)
                        {
                            content.GetChild(1).GetComponent<UIQuestSlot>().nameButton.onClick.Invoke();
                        }
                    }
                    else
                    {
                        content.GetChild(selectedQuest - 1).GetComponent<UIQuestSlot>().nameButton.onClick.Invoke();
                    }
                }
                player.playerQuest.CmdGetQuestReward(prevSelectedQuest);
            });


            activeQuests = player.quests.Where(q => !q.completed).ToList();

            UIUtils.BalancePrefabs(slotPrefab.gameObject, activeQuests.Count, content);

            for (int i = 0; i < activeQuests.Count; i++)
            {

                int index = i;
                UIQuestSlot slot = content.GetChild(i).GetComponent<UIQuestSlot>();
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    slot.nameButton.GetComponentInChildren<TextMeshProUGUI>().text = activeQuests[i].itaTitle;
                }
                else
                {
                    slot.nameButton.GetComponentInChildren<TextMeshProUGUI>().text = activeQuests[i].name;
                }

                slot.nameButton.onClick.SetListener(() =>
                {

                    quest = activeQuests[index];
                    scriptableQuest = quest.data;
                    selectedQuest = index;

                    canClaim = true;
                    questSlot.childObject.SetActive(true);
                    foreach (Transform t in itemContent)
                    {
                        Destroy(t.gameObject);
                    }
                    UIUtils.BalancePrefabs(toSpawn.gameObject, activeQuests[index].data.itemRewards.Count, itemContentDetails);
                    for (int a = 0; a < activeQuests[index].data.itemRewards.Count; a++)
                    {
                        UIInventorySlot inventorySlot = itemContentDetails.GetChild(index).GetComponent<UIInventorySlot>();

                        inventorySlot.amountOverlay.SetActive(activeQuests[index].data.itemRewards[a].amount > 1);
                        inventorySlot.amountText.text = activeQuests[index].data.itemRewards[a].amount.ToString();
                        inventorySlot.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                        inventorySlot.GetComponent<Image>().color = GffItemRarity.singleton.rarityColor(true, activeQuests[index].data.itemRewards[a].items);
                        inventorySlot.image.color = Color.white;
                        inventorySlot.image.sprite = activeQuests[index].data.itemRewards[a].items.image;
                        inventorySlot.cooldownCircle.gameObject.SetActive(false);
                    }

                    goldText.text = scriptableQuest.rewardGold.ToString();
                    coinText.text = scriptableQuest.rewardCoin.ToString();
                    experienceText.text = "EXP:   " + scriptableQuest.rewardExperience.ToString();

                    if (scriptableQuest.ability.Count > 0)
                    {
                        for (int a = 0; a < scriptableQuest.ability.Count; a++)
                        {
                            GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                            QuestDetails questDetails = g.GetComponent<QuestDetails>();

                            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                                questDetails.questDetailText.text = "Aumenta l'abilita' " + scriptableQuest.ability[a].ability.name + " al livello " + scriptableQuest.ability[a].level + "  (" + GeneralManager.singleton.FindNetworkAbilityLevel(scriptableQuest.ability[a].ability.name, player.name) + "/" + GeneralManager.singleton.FindNetworkAbilityMaxLevel(scriptableQuest.ability[a].ability.name, player.name) + ")";
                            else
                                questDetails.questDetailText.text = "Upgrade ability " + scriptableQuest.ability[a].ability.name + " to level " + scriptableQuest.ability[a].level + "  (" + GeneralManager.singleton.FindNetworkAbilityLevel(scriptableQuest.ability[a].ability.name, player.name) + "/" + GeneralManager.singleton.FindNetworkAbilityMaxLevel(scriptableQuest.ability[a].ability.name, player.name) + ")";

                            if (GeneralManager.singleton.FindNetworkAbilityLevel(scriptableQuest.ability[a].ability.name, player.name) < scriptableQuest.ability[a].level)
                            {
                                questDetails.checkmarkImage.sprite = questDetails.crossRed;
                                questDetails.checkmarkImage.color = Color.red;
                                canClaim = false;
                            }
                            else
                            {
                                questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                                questDetails.checkmarkImage.color = Color.white;
                            }
                        }
                    }

                    if (scriptableQuest.Boosts.Count > 0)
                    {
                        for (int a = 0; a < scriptableQuest.Boosts.Count; a++)
                        {
                            GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                            QuestDetails questDetails = g.GetComponent<QuestDetails>();

                            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                                questDetails.questDetailText.text = "Compra il boost " + scriptableQuest.Boosts[a].boosts.name;
                            else
                                questDetails.questDetailText.text = "Buy " + scriptableQuest.Boosts[a].boosts.name + " boost";

                            if (GeneralManager.singleton.FindNetworkBoostTime(scriptableQuest.Boosts[a].boosts.name, player.name) <= 0)
                            {
                                questDetails.checkmarkImage.sprite = questDetails.crossRed;
                                questDetails.checkmarkImage.color = Color.red;
                                canClaim = false;
                            }
                            else
                            {
                                questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                                questDetails.checkmarkImage.color = Color.white;
                            }
                        }
                    }

                    if (scriptableQuest.reachAmountOfFriends > 0)
                    {
                        GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                        QuestDetails questDetails = g.GetComponent<QuestDetails>();

                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            questDetails.questDetailText.text = "Stringi amicizia con " + (scriptableQuest.reachAmountOfFriends == 1 ? scriptableQuest.reachAmountOfFriends + " sopravissuto" : scriptableQuest.reachAmountOfFriends + " sopravissuti");
                        else
                            questDetails.questDetailText.text = "Make friendship with " + (scriptableQuest.reachAmountOfFriends == 1 ? scriptableQuest.reachAmountOfFriends + " sopravissuto" : scriptableQuest.reachAmountOfFriends + " sopravissuti");

                        if (player.playerFriend.playerFriends.Count < scriptableQuest.reachAmountOfFriends)
                        {
                            questDetails.checkmarkImage.sprite = questDetails.crossRed;
                            questDetails.checkmarkImage.color = Color.red;
                            canClaim = false;
                        }
                        else
                        {
                            questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                            questDetails.checkmarkImage.color = Color.white;
                        }
                    }

                    if (scriptableQuest.makeGroupAlly == true)
                    {
                        GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                        QuestDetails questDetails = g.GetComponent<QuestDetails>();

                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            questDetails.questDetailText.text = "Stringi alleanza con un altro gruppo di sopravvisuti";
                        else
                            questDetails.questDetailText.text = "Make an ally with other survivors group";

                        if (player.playerAlliance.guildAlly.Count == 0)
                        {
                            questDetails.checkmarkImage.sprite = questDetails.crossRed;
                            questDetails.checkmarkImage.color = Color.red;
                            canClaim = false;
                        }
                        else
                        {
                            questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                            questDetails.checkmarkImage.color = Color.white;
                        }
                    }

                    if (scriptableQuest.enterPremium == true)
                    {
                        GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                        QuestDetails questDetails = g.GetComponent<QuestDetails>();

                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            questDetails.questDetailText.text = "Clicca sull'angolo in alto a destra per accedere alla zona nascosta";
                        else
                            questDetails.questDetailText.text = "Click left upper corner to access hidden zone!";

                        if (quest.checkEnterPremium == true)
                        {
                            questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                            questDetails.checkmarkImage.color = Color.white;
                        }
                        else
                        {
                            questDetails.checkmarkImage.sprite = questDetails.crossRed;
                            questDetails.checkmarkImage.color = Color.red;
                            canClaim = false;
                        }
                    }

                    if (scriptableQuest.createBuilding == true)
                    {
                        GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                        QuestDetails questDetails = g.GetComponent<QuestDetails>();

                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            questDetails.questDetailText.text = "Crea un edificio";
                        else
                            questDetails.questDetailText.text = "Create a building";

                        if (quest.checkCreateBuilding == true)
                        {
                            questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                            questDetails.checkmarkImage.color = Color.white;
                        }
                        else
                        {
                            questDetails.checkmarkImage.sprite = questDetails.crossRed;
                            questDetails.checkmarkImage.color = Color.red;
                            canClaim = false;
                        }
                    }

                    if (scriptableQuest.killZombie.Count > 0)
                    {
                        for (int a = 0; a < scriptableQuest.killZombie.Count; a++)
                        {
                            GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                            QuestDetails questDetails = g.GetComponent<QuestDetails>();

                            if (scriptableQuest.killZombie[a].monster.name == "BioHazard Zombie")
                            {

                                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                                    questDetails.questDetailText.text = "Uccidi " + (activeQuests[index].checkPolice + "/" + scriptableQuest.killZombie[a].quantity) + "  " + scriptableQuest.killZombie[a].monster.name;
                                else
                                    questDetails.questDetailText.text = "Kill " + (activeQuests[index].checkPolice + "/" + scriptableQuest.killZombie[a].quantity) + "  " + scriptableQuest.killZombie[a].monster.name;

                                if (activeQuests[index].checkPolice < scriptableQuest.killZombie[a].quantity)
                                {
                                    questDetails.checkmarkImage.sprite = questDetails.crossRed;
                                    questDetails.checkmarkImage.color = Color.red;
                                    canClaim = false;
                                }
                                else
                                {
                                    questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                                    questDetails.checkmarkImage.color = Color.white;
                                }
                            }

                            if (scriptableQuest.killZombie[a].monster.name == "Infected Zombie")
                            {

                                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                                    questDetails.questDetailText.text = "Uccidi " + (activeQuests[index].checkBiohazard + "/" + scriptableQuest.killZombie[a].quantity) + "  " + scriptableQuest.killZombie[a].monster.name;
                                else
                                    questDetails.questDetailText.text = "Kill " + (activeQuests[index].checkBiohazard + "/" + scriptableQuest.killZombie[a].quantity) + "  " + scriptableQuest.killZombie[a].monster.name;


                                if (activeQuests[index].checkBiohazard < scriptableQuest.killZombie[a].quantity)
                                {
                                    questDetails.checkmarkImage.sprite = questDetails.crossRed;
                                    questDetails.checkmarkImage.color = Color.red;
                                    canClaim = false;
                                }
                                else
                                {
                                    questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                                    questDetails.checkmarkImage.color = Color.white;
                                }
                            }
                            if (scriptableQuest.killZombie[a].monster.name == "Mechanic Zombie")
                            {
                                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                                    questDetails.questDetailText.text = "Uccidi " + (activeQuests[index].checkInfected + "/" + scriptableQuest.killZombie[a].quantity) + "  " + scriptableQuest.killZombie[a].monster.name;
                                else
                                    questDetails.questDetailText.text = "Kill " + (activeQuests[index].checkInfected + "/" + scriptableQuest.killZombie[a].quantity) + "  " + scriptableQuest.killZombie[a].monster.name;


                                if (activeQuests[index].checkInfected < scriptableQuest.killZombie[a].quantity)
                                {
                                    questDetails.checkmarkImage.sprite = questDetails.crossRed;
                                    questDetails.checkmarkImage.color = Color.red;
                                    canClaim = false;
                                }
                                else
                                {
                                    questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                                    questDetails.checkmarkImage.color = Color.white;
                                }
                            }
                            if (scriptableQuest.killZombie[a].monster.name == "Policeman Zombie")
                            {
                                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                                    questDetails.questDetailText.text = "Uccidi " + (activeQuests[index].checkMechanic + "/" + scriptableQuest.killZombie[a].quantity) + "  " + scriptableQuest.killZombie[a].monster.name;
                                else
                                    questDetails.questDetailText.text = "Kill " + (activeQuests[index].checkMechanic + "/" + scriptableQuest.killZombie[a].quantity) + "  " + scriptableQuest.killZombie[a].monster.name;

                                if (activeQuests[index].checkMechanic < scriptableQuest.killZombie[a].quantity)
                                {
                                    questDetails.checkmarkImage.sprite = questDetails.crossRed;
                                    questDetails.checkmarkImage.color = Color.red;
                                    canClaim = false;
                                }
                                else
                                {
                                    questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                                    questDetails.checkmarkImage.color = Color.white;
                                }
                            }
                        }
                    }

                    if (quest.data.equipWeapon == true)
                    {
                        GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                        QuestDetails questDetails = g.GetComponent<QuestDetails>();

                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            questDetails.questDetailText.text = "Equippaggia un arma";
                        else
                            questDetails.questDetailText.text = "Equip a weapon";


                        if (quest.checkEquipWeapon == true)
                        {
                            questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                            questDetails.checkmarkImage.color = Color.white;
                        }
                        else
                        {
                            questDetails.checkmarkImage.sprite = questDetails.crossRed;
                            questDetails.checkmarkImage.color = Color.red;
                            canClaim = false;
                        }
                    }

                    if (quest.data.equipBag == true)
                    {
                        GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                        QuestDetails questDetails = g.GetComponent<QuestDetails>();

                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            questDetails.questDetailText.text = "Equipaggia uno zaino";
                        else
                            questDetails.questDetailText.text = "Equip a bag";

                        if (quest.checkEquipBag == true)
                        {
                            questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                            questDetails.checkmarkImage.color = Color.white;
                        }
                        else
                        {
                            questDetails.checkmarkImage.sprite = questDetails.crossRed;
                            questDetails.checkmarkImage.color = Color.red;
                            canClaim = false;
                        }
                    }

                    if (quest.data.createGuild == true)
                    {
                        GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                        QuestDetails questDetails = g.GetComponent<QuestDetails>();

                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            questDetails.questDetailText.text = "Crea un gruppo";
                        else
                            questDetails.questDetailText.text = "Create a group";

                        if (quest.checkCreateGuild == true)
                        {
                            questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                            questDetails.checkmarkImage.color = Color.white;
                        }
                        else
                        {
                            questDetails.checkmarkImage.sprite = questDetails.crossRed;
                            questDetails.checkmarkImage.color = Color.red;
                            canClaim = false;
                        }
                    }

                    if (quest.data.createParty == true)
                    {
                        GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                        QuestDetails questDetails = g.GetComponent<QuestDetails>();

                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            questDetails.questDetailText.text = "Aggiungiti ad un party";
                        else
                            questDetails.questDetailText.text = "Join to a party";

                        if (quest.checkCreateParty == true)
                        {
                            questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                            questDetails.checkmarkImage.color = Color.white;
                        }
                        else
                        {
                            questDetails.checkmarkImage.sprite = questDetails.crossRed;
                            questDetails.checkmarkImage.color = Color.red;
                            canClaim = false;
                        }
                    }

                    if (quest.data.drink == true)
                    {
                        GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                        QuestDetails questDetails = g.GetComponent<QuestDetails>();

                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            questDetails.questDetailText.text = "Bevi qualcosa";
                        else
                            questDetails.questDetailText.text = "Drink some fluid";

                        if (quest.checkDrink == true)
                        {
                            questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                            questDetails.checkmarkImage.color = Color.white;
                        }
                        else
                        {
                            questDetails.checkmarkImage.sprite = questDetails.crossRed;
                            questDetails.checkmarkImage.color = Color.red;
                            canClaim = false;
                        }
                    }

                    if (quest.data.eat == true)
                    {
                        GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                        QuestDetails questDetails = g.GetComponent<QuestDetails>();

                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            questDetails.questDetailText.text = "Mangia qualcosa";
                        else
                            questDetails.questDetailText.text = "Eat some";

                        if (quest.checkEat == true)
                        {
                            questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                            questDetails.checkmarkImage.color = Color.white;
                        }
                        else
                        {
                            questDetails.checkmarkImage.sprite = questDetails.crossRed;
                            questDetails.checkmarkImage.color = Color.red;
                            canClaim = false;
                        }
                    }

                    if (quest.data.run == true)
                    {
                        GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                        QuestDetails questDetails = g.GetComponent<QuestDetails>();
                        questDetails.questDetailText.text = "Let's make some jogging";

                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            questDetails.questDetailText.text = "Facciamo un po' di jogging";
                        else
                            questDetails.questDetailText.text = "Let's make some jogging";

                        if (quest.checkRun == true)
                        {
                            questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                            questDetails.checkmarkImage.color = Color.white;
                        }
                        else
                        {
                            questDetails.checkmarkImage.sprite = questDetails.crossRed;
                            questDetails.checkmarkImage.color = Color.red;
                            canClaim = false;
                        }
                    }

                    if (quest.data.sneak == true)
                    {
                        GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                        QuestDetails questDetails = g.GetComponent<QuestDetails>();

                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            questDetails.questDetailText.text = "Facciamo un po' di movimennti furtivi";
                        else
                            questDetails.questDetailText.text = "Make sneak moves";

                        if (quest.checkSneak == true)
                        {
                            questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                            questDetails.checkmarkImage.color = Color.white;
                        }
                        else
                        {
                            questDetails.checkmarkImage.sprite = questDetails.crossRed;
                            questDetails.checkmarkImage.color = Color.red;
                            canClaim = false;
                        }
                    }

                    if (quest.data.makeMarriage == true)
                    {
                        GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                        QuestDetails questDetails = g.GetComponent<QuestDetails>();
                        questDetails.questDetailText.text = "Make a partner relationship";

                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            questDetails.questDetailText.text = "Stringi una relazione con qualcuno";
                        else
                            questDetails.questDetailText.text = "Make a partner relationship";

                        if (quest.checkMakeMarriage == true)
                        {
                            questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                            questDetails.checkmarkImage.color = Color.white;
                        }
                        else
                        {
                            questDetails.checkmarkImage.sprite = questDetails.crossRed;
                            questDetails.checkmarkImage.color = Color.red;
                            canClaim = false;
                        }
                    }

                    if (quest.data.sendAMessage == true)
                    {
                        GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                        QuestDetails questDetails = g.GetComponent<QuestDetails>();
                        questDetails.questDetailText.text = "Send some message";

                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            questDetails.questDetailText.text = "Manda qualche messaggio";
                        else
                            questDetails.questDetailText.text = "Send some message";

                        if (quest.checkSendAMessage == true)
                        {
                            questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                            questDetails.checkmarkImage.color = Color.white;
                        }
                        else
                        {
                            questDetails.checkmarkImage.sprite = questDetails.crossRed;
                            questDetails.checkmarkImage.color = Color.red;
                            canClaim = false;
                        }
                    }

                    if (quest.data.buyEmoji == true)
                    {
                        GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                        QuestDetails questDetails = g.GetComponent<QuestDetails>();

                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            questDetails.questDetailText.text = "Compra un emoji";
                        else
                            questDetails.questDetailText.text = "Buy an emoji";

                        if (quest.checkBuyEmoji == true)
                        {
                            questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                            questDetails.checkmarkImage.color = Color.white;
                        }
                        else
                        {
                            questDetails.checkmarkImage.sprite = questDetails.crossRed;
                            questDetails.checkmarkImage.color = Color.red;
                            canClaim = false;
                        }
                    }

                    if (quest.data.openShop == true)
                    {
                        GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                        QuestDetails questDetails = g.GetComponent<QuestDetails>();

                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            questDetails.questDetailText.text = "Apri lo shop";
                        else
                            questDetails.questDetailText.text = "Open shop panel";

                        if (quest.checkOpenShop == true)
                        {
                            questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                            questDetails.checkmarkImage.color = Color.white;
                        }
                        else
                        {
                            questDetails.checkmarkImage.sprite = questDetails.crossRed;
                            questDetails.checkmarkImage.color = Color.red;
                            canClaim = false;
                        }
                    }

                    if (quest.data.amountPlayerToKill > 0)
                    {
                        GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                        QuestDetails questDetails = g.GetComponent<QuestDetails>();

                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            questDetails.questDetailText.text = "Uccidi " + (activeQuests[index].checkAmountPlayerToKill + "/" + scriptableQuest.amountPlayerToKill + (scriptableQuest.amountPlayerToKill == 1 ? " giocatore" : " giocatori"));
                        else
                            questDetails.questDetailText.text = "Kill " + (activeQuests[index].checkAmountPlayerToKill + "/" + scriptableQuest.amountPlayerToKill + (scriptableQuest.amountPlayerToKill == 1 ? " player" : " players"));

                        if (quest.checkAmountPlayerToKill >= quest.data.amountPlayerToKill)
                        {
                            questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                            questDetails.checkmarkImage.color = Color.white;
                        }
                        else
                        {
                            questDetails.checkmarkImage.sprite = questDetails.crossRed;
                            questDetails.checkmarkImage.color = Color.red;
                            canClaim = false;
                        }
                    }

                    if (quest.data.makeATrade == true)
                    {
                        GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                        QuestDetails questDetails = g.GetComponent<QuestDetails>();

                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            questDetails.questDetailText.text = "Faccio uno scambio con un giocatore";
                        else
                            questDetails.questDetailText.text = "Make a trade with a player";

                        if (quest.checkMakeATrade == true)
                        {
                            questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                            questDetails.checkmarkImage.color = Color.white;
                        }
                        else
                        {
                            questDetails.checkmarkImage.sprite = questDetails.crossRed;
                            questDetails.checkmarkImage.color = Color.red;
                            canClaim = false;
                        }
                    }

                    if (quest.data.amountRockToGather > 0)
                    {
                        GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                        QuestDetails questDetails = g.GetComponent<QuestDetails>();

                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            questDetails.questDetailText.text = "Raccogli " + (activeQuests[index].checkRockToGather + "/" + scriptableQuest.amountRockToGather) + " roccie";
                        else
                            questDetails.questDetailText.text = "Collect " + (activeQuests[index].checkRockToGather + "/" + scriptableQuest.amountRockToGather) + " rocks";

                        if (quest.checkRockToGather >= quest.data.amountRockToGather)
                        {
                            questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                            questDetails.checkmarkImage.color = Color.white;
                        }
                        else
                        {
                            questDetails.checkmarkImage.sprite = questDetails.crossRed;
                            questDetails.checkmarkImage.color = Color.red;
                            canClaim = false;
                        }
                    }

                    if (quest.data.amountWoodToGather > 0)
                    {
                        GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                        QuestDetails questDetails = g.GetComponent<QuestDetails>();

                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            questDetails.questDetailText.text = "Raccogli " + (activeQuests[index].checkWoodToGather + "/" + scriptableQuest.amountWoodToGather) + " legname";
                        else
                            questDetails.questDetailText.text = "Collect " + (activeQuests[index].checkWoodToGather + "/" + scriptableQuest.amountWoodToGather) + " woods";

                        if (quest.checkWoodToGather >= quest.data.amountWoodToGather)
                        {
                            questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                            questDetails.checkmarkImage.color = Color.white;
                        }
                        else
                        {
                            questDetails.checkmarkImage.sprite = questDetails.crossRed;
                            questDetails.checkmarkImage.color = Color.red;
                            canClaim = false;
                        }
                    }

                    if (quest.data.useTeleport == true)
                    {
                        GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                        QuestDetails questDetails = g.GetComponent<QuestDetails>();

                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            questDetails.questDetailText.text = "Usa l'item di teletrasporto";
                        else
                            questDetails.questDetailText.text = "Use teleport item";


                        if (quest.checkUseTeleport == true)
                        {
                            questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                            questDetails.checkmarkImage.color = Color.white;
                        }
                        else
                        {
                            questDetails.checkmarkImage.sprite = questDetails.crossRed;
                            questDetails.checkmarkImage.color = Color.red;
                            canClaim = false;
                        }
                    }

                    if (quest.data.useInstantResurrect == true)
                    {
                        GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                        QuestDetails questDetails = g.GetComponent<QuestDetails>();
                        questDetails.questDetailText.text = "Use instant resurrect";

                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            questDetails.questDetailText.text = "Usa l'item di ressurrezione istantanea";
                        else
                            questDetails.questDetailText.text = "Use instant resurrect";

                        if (quest.checkUseInstantResurrect == true)
                        {
                            questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                            questDetails.checkmarkImage.color = Color.white;
                        }
                        else
                        {
                            questDetails.checkmarkImage.sprite = questDetails.crossRed;
                            questDetails.checkmarkImage.color = Color.red;
                            canClaim = false;
                        }
                    }

                    if (quest.data.setSpawnpoint.numberOfSpawnpointToCreate > 0)
                    {
                        GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                        QuestDetails questDetails = g.GetComponent<QuestDetails>();

                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            questDetails.questDetailText.text = "Crea " + (activeQuests[index].checkCreateASpawnpoint + "/" + scriptableQuest.setSpawnpoint.numberOfSpawnpointToCreate) + " spawnpoints";
                        else
                            questDetails.questDetailText.text = "Create " + (activeQuests[index].checkCreateASpawnpoint + "/" + scriptableQuest.setSpawnpoint.numberOfSpawnpointToCreate) + " spawnpoints";

                        if (quest.checkCreateASpawnpoint >= scriptableQuest.setSpawnpoint.numberOfSpawnpointToCreate)
                        {
                            questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                            questDetails.checkmarkImage.color = Color.white;
                        }
                        else
                        {
                            questDetails.checkmarkImage.sprite = questDetails.crossRed;
                            questDetails.checkmarkImage.color = Color.red;
                            canClaim = false;
                        }
                    }

                    if (scriptableQuest.buildingUpgrade.Count > 0)
                    {
                        for (int a = 0; a < scriptableQuest.buildingUpgrade.Count; a++)
                        {
                            GameObject g = Instantiate(slotDetailsPrefab, itemContent);
                            QuestDetails questDetails = g.GetComponent<QuestDetails>();

                            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                                questDetails.questDetailText.text = "Alza l'edificio " + scriptableQuest.buildingUpgrade[a].buildingType.name + " al livello " + (activeQuests[index].buildingUpgrade[a].itemLevelToReach + "/" + activeQuests[index].buildingUpgrade[a].levelToReach);
                            else
                                questDetails.questDetailText.text = "Upgrade " + scriptableQuest.buildingUpgrade[a].buildingType.name + " to level " + (activeQuests[index].buildingUpgrade[a].itemLevelToReach + "/" + activeQuests[index].buildingUpgrade[a].levelToReach);


                            if (activeQuests[index].buildingUpgrade[a].itemLevelToReach < activeQuests[index].buildingUpgrade[a].levelToReach)
                            {
                                questDetails.checkmarkImage.sprite = questDetails.crossRed;
                                questDetails.checkmarkImage.color = Color.red;
                                canClaim = false;
                            }
                            else
                            {
                                questDetails.checkmarkImage.sprite = questDetails.checkmarkGreen;
                                questDetails.checkmarkImage.color = Color.white;
                            }
                        }
                    }

                });
            }


            if (canClaim && selectedQuest > -1)
            {
                for (int w = 0; w < player.quests[selectedQuest].data.itemRewards.Count; w++)
                {
                    if (player.InventoryCanAdd(new Item(player.quests[selectedQuest].data.itemRewards[w].items), player.quests[selectedQuest].data.itemRewards[w].amount) == false)
                    {
                        canClaim = false;
                    }
                }
            }
            else
                canClaim = false;



            claimButton.interactable = canClaim;

        }
    }
    public void Reset()
    {
        for (int a = 0; a < activeQuests[selectedQuest].data.itemRewards.Count; a++)
        {
            UIInventorySlot inventorySlot = itemContentDetails.GetChild(selectedQuest).GetComponent<UIInventorySlot>();
            Destroy(inventorySlot.gameObject);
        }

        goldText.text = coinText.text = experienceText.text = string.Empty;
    }
}

public partial class UIInventoryItemMall
{
    public static UIInventoryItemMall singleton;
    public KeyCode hotKey = KeyCode.None;
    public UIInventorySlot slotPrefab;
    public Transform content;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI coinText;
    public Button closeButton;

    [HideInInspector] public Player player;

    public void Start()
    {
        if (!singleton) singleton = this;
    }

    void Update()
    {
        closeButton.onClick.SetListener(() =>
        {
            Destroy(closeButton.gameObject);
        });


        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.health == 0)
            Destroy(closeButton.gameObject);

        if (player != null)
        {

            // instantiate/destroy enough slots
            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.inventory.Count, content);

            // refresh all items
            for (int i = 0; i < player.inventory.Count; ++i)
            {
                UIInventorySlot slot = content.GetChild(i).GetComponent<UIInventorySlot>();
                slot.dragAndDropable.name = i.ToString(); // drag and drop index
                ItemSlot itemSlot = player.inventory[i];
                if (itemSlot.amount > 0)
                {
                    // refresh valid item
                    int icopy = i; // needed for lambdas, otherwise i is Count

                    // only build tooltip while it's actually shown. this
                    // avoids MASSIVE amounts of StringBuilder allocations.
                    slot.tooltip.enabled = false;
                    //if (slot.tooltip.IsVisible())
                    //    slot.tooltip.text = itemSlot.ToolTip();
                    slot.dragAndDropable.dragable = true;
                    slot.image.color = Color.white;
                    slot.image.sprite = itemSlot.item.image;
                    if (icopy <= player.playerConservative.AmountOfItemProtected()) slot.protectedImage.gameObject.SetActive(true);
                    else slot.protectedImage.gameObject.SetActive(false);
                    // cooldown if usable item
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
                    // refresh invalid item
                    slot.button.onClick.RemoveAllListeners();
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                    slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(false);
                    if (i <= player.playerConservative.AmountOfItemProtected()) slot.protectedImage.gameObject.SetActive(true);
                    else slot.protectedImage.gameObject.SetActive(false);
                    slot.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                    slot.GetComponent<Image>().color = GffItemRarity.singleton.ColorNull;
                }
            }

            // gold
            goldText.text = player.gold.ToString();
            coinText.text = player.coins.ToString();
        }
    }

}

public partial class UISkillbar : MonoBehaviour
{
    public GameObject panel;
    public UISkillbarSlot slotPrefab;
    public Transform content;

    void Update()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            panel.SetActive(true);

            // instantiate/destroy enough slots
            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.skillbar.Length, content);

            // refresh all
            for (int i = 0; i < player.skillbar.Length; ++i)
            {
                UISkillbarSlot slot = content.GetChild(i).GetComponent<UISkillbarSlot>();
                slot.dragAndDropable.name = i.ToString(); // drag and drop index

                // hotkey overlay (without 'Alpha' etc.)
                string pretty = player.skillbar[i].hotKey.ToString().Replace("Alpha", "");
                slot.hotkeyText.text = pretty;

                // skill, inventory item or equipment item?
                int skillIndex = player.GetSkillIndexByName(player.skillbar[i].reference);
                int inventoryIndex = player.GetInventoryIndexByName(player.skillbar[i].reference);
                int equipmentIndex = player.GetEquipmentIndexByName(player.skillbar[i].reference);
                if (skillIndex != -1)
                {
                    Skill skill = player.skills[skillIndex];
                    bool canCast = player.CastCheckSelf(skill);

                    // hotkey pressed and not typing in any input right now?
                    if (Input.GetKeyDown(player.skillbar[i].hotKey) &&
                        !UIUtils.AnyInputActive() &&
                        canCast) // checks mana, cooldowns, etc.) {
                    {
                        // try use the skill or walk closer if needed
                        player.TryUseSkill(skillIndex);
                    }

                    // refresh skill slot
                    slot.button.interactable = canCast; // check mana, cooldowns, etc.
                    slot.button.onClick.SetListener(() =>
                    {
                        // try use the skill or walk closer if needed
                        player.TryUseSkill(skillIndex);
                    });
                    // only build tooltip while it's actually shown. this
                    // avoids MASSIVE amounts of StringBuilder allocations.
                    slot.tooltip.enabled = true;
                    if (slot.tooltip.IsVisible())
                        slot.tooltip.text = skill.ToolTip();
                    slot.dragAndDropable.dragable = true;
                    slot.image.color = Color.white;
                    slot.image.sprite = skill.image;
                    float cooldown = skill.CooldownRemaining();
                    slot.cooldownOverlay.SetActive(cooldown > 0);
                    slot.cooldownText.text = cooldown.ToString("F0");
                    slot.cooldownCircle.fillAmount = skill.cooldown > 0 ? cooldown / skill.cooldown : 0;
                    slot.amountOverlay.SetActive(false);
                }
                else if (inventoryIndex != -1)
                {
                    ItemSlot itemSlot = player.inventory[inventoryIndex];

                    // hotkey pressed and not typing in any input right now?
                    if (Input.GetKeyDown(player.skillbar[i].hotKey) && !UIUtils.AnyInputActive())
                        player.CmdUseInventoryItem(inventoryIndex);

                    // refresh inventory slot
                    slot.button.onClick.SetListener(() =>
                    {
                        player.CmdUseInventoryItem(inventoryIndex);
                    });
                    // only build tooltip while it's actually shown. this
                    // avoids MASSIVE amounts of StringBuilder allocations.
                    slot.tooltip.enabled = true;
                    if (slot.tooltip.IsVisible())
                        slot.tooltip.text = itemSlot.ToolTip();
                    slot.dragAndDropable.dragable = true;
                    slot.image.color = Color.white;
                    slot.image.sprite = itemSlot.item.image;
                    slot.cooldownOverlay.SetActive(false);
                    // cooldown if usable item
                    if (itemSlot.item.data is UsableItem usable)
                    {
                        float cooldown = player.GetItemCooldown(usable.cooldownCategory);
                        slot.cooldownCircle.fillAmount = usable.cooldown > 0 ? cooldown / usable.cooldown : 0;
                    }
                    else slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(itemSlot.amount > 1);
                    slot.amountText.text = itemSlot.amount.ToString();
                    slot.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                    slot.GetComponent<Image>().color = GffItemRarity.singleton.rarityColor(true, itemSlot.item);

                }
                else if (equipmentIndex != -1)
                {
                    ItemSlot itemSlot = player.equipment[equipmentIndex];

                    // refresh equipment slot
                    slot.button.onClick.RemoveAllListeners();
                    // only build tooltip while it's actually shown. this
                    // avoids MASSIVE amounts of StringBuilder allocations.
                    slot.tooltip.enabled = true;
                    if (slot.tooltip.IsVisible())
                        slot.tooltip.text = itemSlot.ToolTip();
                    slot.dragAndDropable.dragable = true;
                    slot.image.color = Color.white;
                    slot.image.sprite = itemSlot.item.image;
                    slot.cooldownOverlay.SetActive(false);
                    // cooldown if usable item
                    if (itemSlot.item.data is UsableItem usable)
                    {
                        float cooldown = player.GetItemCooldown(usable.cooldownCategory);
                        slot.cooldownCircle.fillAmount = usable.cooldown > 0 ? cooldown / usable.cooldown : 0;
                    }
                    else slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(itemSlot.amount > 1);
                    slot.amountText.text = itemSlot.amount.ToString();
                    slot.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                    slot.GetComponent<Image>().color = GffItemRarity.singleton.rarityColor(true, itemSlot.item);
                }
                else
                {
                    // clear the outdated reference
                    player.skillbar[i].reference = "";

                    // refresh empty slot
                    slot.button.onClick.RemoveAllListeners();
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                    slot.cooldownOverlay.SetActive(false);
                    slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(false);
                    slot.GetComponent<Image>().sprite = GffItemRarity.singleton.rarityType();
                    slot.GetComponent<Image>().color = GffItemRarity.singleton.ColorNull;

                }
            }
        }
        else panel.SetActive(false);
    }
}

public partial class UICar
{
    public Button pilotButton;
    public Button coPilotButton;
    public Button passengerSxButton;
    public Button passengerCenterButton;
    public Button passengerDxButton;

    public Text description;
    public Button managerButton;
    public Button closeButton;

    private Player player;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating(nameof(SetButton), 0.0f, 0.3f);
    }

    public void SetButton()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;


        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

        pilotButton.onClick.SetListener(() =>
        {
            player.playerCar.CmdPilot(player.gameObject);
            player.CmdPetUnsummon();
        });

        coPilotButton.onClick.SetListener(() =>
        {
            player.playerCar.CmdCoPilot(player.gameObject);
            player.CmdPetUnsummon();
        });

        passengerSxButton.onClick.SetListener(() =>
        {
            player.playerCar.CmdPassengerSx(player.gameObject);
            player.CmdPetUnsummon();
        });

        passengerCenterButton.onClick.SetListener(() =>
        {
            player.playerCar.CmdPassengerCenter(player.gameObject);
            player.CmdPetUnsummon();
        });

        passengerDxButton.onClick.SetListener(() =>
        {
            player.playerCar.CmdPassengerDx(player.gameObject);
            player.CmdPetUnsummon();
        });

        managerButton.onClick.SetListener(() =>
        {
            Instantiate(GeneralManager.singleton.carManager, GeneralManager.singleton.canvas);
        });

        CancelInvoke(nameof(SetButton));
    }

    // Update is called once per frame
    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.health == 0)
            Destroy(this.gameObject);

        managerButton.interactable = (player.playerCar._car != null && player.target.GetComponent<Car>() && player.playerCar._car == player.target.gameObject || !((Car)player.target).PassengerInside());
        ManagePassengerType();
    }


    public void ManagePassengerType()
    {
        if (!player.target) return;
        if (!(player.target is Car)) return;

        Car car = ((Car)player.target);

        if (car._pilot == string.Empty &&
           car._coPilot == string.Empty &&
           car._rearSxPassenger == string.Empty &&
           car._rearDxPassenger == string.Empty &&
           car._rearCenterPassenger == string.Empty)
        {
            pilotButton.interactable =
            coPilotButton.interactable =
            passengerSxButton.interactable =
            passengerCenterButton.interactable =
            passengerDxButton.interactable = true;

            pilotButton.GetComponentInChildren<Text>().text = "Pilot";
            coPilotButton.GetComponentInChildren<Text>().text = "CoPilot";
            passengerSxButton.GetComponentInChildren<Text>().text = "Passenger";
            passengerCenterButton.GetComponentInChildren<Text>().text = "Passenger";
            passengerDxButton.GetComponentInChildren<Text>().text = "Passenger";
        }
        else
        {
            pilotButton.interactable = player.playerCar.passengerType == "Pilot" ? true : car._pilot == string.Empty ? true : false;
            pilotButton.GetComponentInChildren<Text>().text = car._pilot != string.Empty ? car._pilot : "Pilot";

            coPilotButton.interactable = player.playerCar.passengerType == "CoPilot" ? true : car._coPilot == string.Empty ? true : false;
            coPilotButton.GetComponentInChildren<Text>().text = car._coPilot != string.Empty ? car._coPilot : "CoPilot";

            passengerSxButton.interactable = player.playerCar.passengerType == "RearSx" ? true : car._rearSxPassenger == string.Empty ? true : false;
            passengerSxButton.GetComponentInChildren<Text>().text = car._rearSxPassenger != string.Empty ? car._rearSxPassenger : "Passenger";

            passengerCenterButton.interactable = player.playerCar.passengerType == "RearCenter" ? true : car._rearCenterPassenger == string.Empty ? true : false;
            passengerCenterButton.GetComponentInChildren<Text>().text = car._rearCenterPassenger != string.Empty ? car._rearCenterPassenger : "Passenger";

            passengerDxButton.interactable = player.playerCar.passengerType == "RearDx" ? true : car._rearDxPassenger == string.Empty ? true : false;
            passengerDxButton.GetComponentInChildren<Text>().text = car._rearDxPassenger != string.Empty ? car._rearDxPassenger : "Passenger";

        }

    }
}

public partial class UICarSpecificManager : MonoBehaviour
{
    private Player player;
    public static UICarSpecificManager singleton;

    public Button closeButton;

    public Slider slider;
    public Text currentGasoline;
    public Text maxGasoline;

    public Text selectedGasoline;
    public Text inventoryGasoline;

    public Button buttonSwitch;
    public Button doAction;

    public GameObject pieceSlot;

    public Transform content;
    public Button buttonGetPiece;
    public Button addGetPiece;

    public int pieceIndex;

    public Car car;

    // Start is called before the first frame update
    void Start()
    {
        if (!singleton) singleton = this;
        InvokeRepeating(nameof(SetButton), 0.0f, 0.3f);
    }

    public void SetButton()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        closeButton.onClick.SetListener(() =>
        {
            Destroy(singleton.gameObject);
        });
        buttonGetPiece.onClick.SetListener(() =>
        {
            player.playerCar.CmdGetPieceOfTheCar(pieceIndex);
            pieceIndex = -1;
        });
        addGetPiece.onClick.SetListener(() =>
        {
            Instantiate(GeneralManager.singleton.carInventory, GeneralManager.singleton.canvas);
        });

        doAction.onClick.SetListener(() =>
        {
            if (doAction.GetComponentInChildren<Text>().text == "Get")
            {
                player.playerCar.CmdGetGasoline(((int)slider.value));

            }
            else if (doAction.GetComponentInChildren<Text>().text == "Put")
            {
                player.playerCar.CmdPutGasoline(((int)slider.value));
            }
        });

        buttonSwitch.onClick.SetListener(() =>
        {
            if (doAction.GetComponentInChildren<Text>().text == "Get")
            {
                doAction.GetComponentInChildren<Text>().text = "Put";
            }
            else if (doAction.GetComponentInChildren<Text>().text == "Put")
            {
                doAction.GetComponentInChildren<Text>().text = "Get";
            }

        });

        CancelInvoke(nameof(SetButton));
    }

    // Update is called once per frame
    void Update()
    {

        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.health == 0)
            Destroy(singleton.gameObject);

        if (!player.target) return;
        if (!(player.target is Car)) return;

        Car car = ((Car)player.target);

        if (!car) return;

        slider.minValue = 0;
        slider.maxValue = car.maxGasoline;
        maxGasoline.text = car.maxGasoline.ToString();

        currentGasoline.text = ((int)slider.value).ToString();

        selectedGasoline.text = "Selected Gasoline : " + ((int)slider.value).ToString();

        buttonGetPiece.interactable = pieceIndex != -1;

        addGetPiece.interactable = (player.playerCar._car != null && player.target.GetComponent<Car>() && player.playerCar._car == player.target.gameObject || !((Car)player.target).PassengerInside());

        if (doAction.GetComponentInChildren<Text>().text == "Get")
        {
            if (player.playerCar.GetEmptyGasolineBootle() > car.maxGasoline - (car.maxGasoline - car.currentGasoline))
            {
                inventoryGasoline.text = "You can withdraw a maximum of : " + (car.maxGasoline - (car.maxGasoline - car.currentGasoline)).ToString();
            }
            else
            {
                inventoryGasoline.text = "You can withdraw a maximum of : " + player.playerCar.GetEmptyGasolineBootle().ToString();
            }
        }
        else if (doAction.GetComponentInChildren<Text>().text == "Put")
        {
            if (player.playerCar.GetGasolineInINventory() > car.maxGasoline - car.currentGasoline)
            {
                inventoryGasoline.text = "You can deposit a maximum of : " + (car.maxGasoline - car.currentGasoline).ToString();
            }
            else
            {
                inventoryGasoline.text = "You can deposit a maximum of : " + player.playerCar.GetGasolineInINventory().ToString();
            }
        }
        else
        {
            doAction.interactable = false;
        }

        UIUtils.BalancePrefabs(pieceSlot, car.inventory.Count, content);
        for (int i = 0; i < car.inventory.Count; i++)
        {
            int index = i;
            PieceSlot slot = content.GetChild(index).GetComponent<PieceSlot>();
            if (car.inventory[index].amount == 0)
            {
                Destroy(slot.gameObject);
                continue;
            }
            slot.pieceImage.sprite = car.inventory[index].item.image;
            slot.name.text = car.inventory[index].item.name + "\n(x" + car.inventory[index].amount + ")";
            slot.selectButton.interactable = (player.playerCar._car != null && player.target.GetComponent<Car>() && player.playerCar._car == player.target.gameObject || !((Car)player.target).PassengerInside());
            slot.selectButton.onClick.SetListener(() =>
            {
                pieceIndex = index;
                for (int e = 0; e < content.childCount; e++)
                {
                    content.GetChild(e).GetComponent<PieceSlot>().outline.enabled = false;
                }
                content.GetChild(index).GetComponent<PieceSlot>().outline.enabled = true;
            });
        }
    }


}

public partial class UICarManager
{
    public GameObject childPanel;

    public Button OnOff;
    public Button lightManager;
    public Text carModel;
    public Slider slider;

    public Text zeroGasoline;
    public Text maxGasoline;
    public Text gasolinePerc;
    public Text OnOffText;

    public Button exitButton;

    private Car car;

    private UIAttackManager attackManager;

    private Player player;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;


        Car car = player.playerCar.car;


        childPanel.SetActive(car && car.hasGasoline);
        if (!attackManager) attackManager = UIAttackManager.singleton;

        //if (attackManager) attackManager.gameObject.SetActive(!childPanel.activeInHierarchy);

        if (!car) return;

        if (childPanel.activeInHierarchy)
        {
            if (GeneralManager.singleton.spawnedBuildingObject) Destroy(GeneralManager.singleton.spawnedBuildingObject);


            player.playerBuilding.building = null;
            if (player.playerBuilding.actualBuilding) Destroy(player.playerBuilding.actualBuilding);
            player.playerBuilding.inventoryIndex = -1;

            zeroGasoline.text = car.currentGasoline.ToString();
            maxGasoline.text = car.maxGasoline.ToString();
            gasolinePerc.text = ((int)(GasolinePercent() * 100)).ToString();
            slider.value = GasolinePercent();
            OnOffText.text = car.On ? "ON" : "OFF";
            carModel.text = car.name;

            OnOff.interactable = car && car._pilot == player.name;
            OnOff.onClick.SetListener(() =>
            {
                player.playerCar.CmdCarMode();
            });

            exitButton.onClick.SetListener(() =>
            {
                player.playerCar.CmdExit();
            });

            lightManager.interactable = car && car._pilot == player.name;
            lightManager.GetComponentInChildren<Text>().text = car.lightON ? "ON" : "OFF";
            lightManager.onClick.SetListener(() =>
            {
                player.playerCar.CmdLights();
            });
        }
    }


    public float GasolinePercent()
    {
        if (!player.playerCar.car) return 0.0f;
        return (player.playerCar.car.currentGasoline != 0 && player.playerCar.car.maxGasoline != 0) ? (float)player.playerCar.car.currentGasoline / (float)player.playerCar.car.maxGasoline : 0;
    }

}

public partial class UICarInventory : MonoBehaviour
{
    private Player player;

    public GameObject inventorySlot;
    public Transform carContent;
    public Transform inventoryContent;

    public Car car;

    public List<int> selectedInventoryIndex = new List<int>();
    public List<int> selecedCarIndex = new List<int>();

    public Button switchButton;

    public Button closeButtton;

    // Start is called before the first frame update
    void Start()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        InvokeRepeating(nameof(SetButton), 0.0f, 0.3f);
    }

    public void SetButton()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        switchButton.onClick.SetListener(() =>
        {
            player.playerCar.CmdSwitchInventoryCar(selecedCarIndex.ToArray(), selectedInventoryIndex.ToArray());
            selecedCarIndex.Clear();
            selectedInventoryIndex.Clear();
        });

        closeButtton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

        CancelInvoke(nameof(SetButton));
    }

    // Update is called once per frame
    void Update()
    {

        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.health == 0)
            Destroy(this.gameObject);

        car = player.playerCar.car;
        if (!car) return;

        UIUtils.BalancePrefabs(inventorySlot, player.playerCar.car.inventory.Count, carContent);
        for (int i = 0; i < player.playerCar.car.inventory.Count; i++)
        {
            UIInventorySlot slot = carContent.GetChild(i).GetComponent<UIInventorySlot>();
            slot.dragAndDropable.name = i.ToString(); // drag and drop index
            ItemSlot itemSlot = player.playerCar.car.inventory[i];

            if (itemSlot.amount > 0)
            {
                // refresh valid item
                int icopy = i; // needed for lambdas, otherwise i is Count
                slot.button.onClick.SetListener(() =>
                {
                    if (!selecedCarIndex.Contains(icopy))
                    {
                        selecedCarIndex.Add(icopy);
                    }
                    else
                    {
                        selecedCarIndex.Remove(icopy);
                    }

                });
                // only build tooltip while it's actually shown. this
                // avoids MASSIVE amounts of StringBuilder allocations.
                slot.tooltip.enabled = true;
                if (slot.tooltip.IsVisible())
                    slot.tooltip.text = itemSlot.ToolTip();
                //slot.dragAndDropable.dragable = true;
                slot.image.color = Color.white;
                slot.image.sprite = itemSlot.item.image;
                // cooldown if usable item
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
                // refresh invalid item
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

        UIUtils.BalancePrefabs(inventorySlot, player.inventory.Count, inventoryContent);
        for (int i = 0; i < player.inventory.Count; i++)
        {
            UIInventorySlot slot = inventoryContent.GetChild(i).GetComponent<UIInventorySlot>();
            slot.dragAndDropable.name = i.ToString(); // drag and drop index
            ItemSlot itemSlot = player.inventory[i];

            if (itemSlot.amount > 0)
            {
                // refresh valid item
                int icopy = i; // needed for lambdas, otherwise i is Count
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
                // only build tooltip while it's actually shown. this
                // avoids MASSIVE amounts of StringBuilder allocations.
                slot.tooltip.enabled = true;
                if (slot.tooltip.IsVisible())
                    slot.tooltip.text = itemSlot.ToolTip();
                //slot.dragAndDropable.dragable = true;
                slot.image.color = Color.white;
                slot.image.sprite = itemSlot.item.image;
                // cooldown if usable item
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
                // refresh invalid item
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

        for (int i = 0; i < car.inventory.Count; i++)
        {
            int index = i;
            if (selecedCarIndex.Contains(index)) carContent.GetChild(index).GetComponent<Outline>().enabled = true;
            else carContent.GetChild(index).GetComponent<Outline>().enabled = false;
        }

        for (int i = 0; i < player.inventory.Count; i++)
        {
            int index = i;
            if (selectedInventoryIndex.Contains(index)) inventoryContent.GetChild(index).GetComponent<Outline>().enabled = true;
            else inventoryContent.GetChild(index).GetComponent<Outline>().enabled = false;
        }

        switchButton.interactable = (player.playerCar._car != null && player.target.GetComponent<Car>() && player.playerCar._car == player.target.gameObject || !((Car)player.target).PassengerInside());
    }
}

public partial class UILeaderboard : MonoBehaviour
{
    public static UILeaderboard singleton;

    private Player player;

    public Text playerKillText;
    public Text monsterKillText;
    public Text boosKillText;
    public Text plantText;
    public Text rockText;
    public Text treeText;
    public Text upgradeItemText;
    public Text craftItemText;
    public Text buildingText;
    public Text boostText;

    public Slider personalSlider;
    public Slider groupSlider;
    public Slider allianceSlider;

    public Button closeButton;

    public Text remainingtime;

    public Button personalReward;
    public Button groupReward;
    public Button allianceReward;

    public List<LeaderboardItem> personalItem = new List<LeaderboardItem>();
    public List<LeaderboardItem> groupItem = new List<LeaderboardItem>();
    public List<LeaderboardItem> allyItem = new List<LeaderboardItem>();

    public GameObject leaderboardRewards;


    // Start is called before the first frame update
    void Start()
    {
        if (!singleton) singleton = this;

        InvokeRepeating(nameof(SetButton), 0.0f, 0.3f);
    }

    public void SetButton()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        player.playerLeaderPoints.CmdChargeGuildLeaderboard();

        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

        personalReward.onClick.SetListener(() =>
        {
            Instantiate(leaderboardRewards, GeneralManager.singleton.canvas);
        });
        groupReward.onClick.SetListener(() =>
        {
            Instantiate(leaderboardRewards, GeneralManager.singleton.canvas);
        });
        allianceReward.onClick.SetListener(() =>
        {
            Instantiate(leaderboardRewards, GeneralManager.singleton.canvas);
        });

        CancelInvoke(nameof(SetButton));
    }


    // Update is called once per frame
    void Update()
    {

        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.health == 0)
            Destroy(this.gameObject);

        playerKillText.text = player.playerLeaderPoints.playerKill.ToString();
        monsterKillText.text = player.playerLeaderPoints.monsterKill.ToString();
        boosKillText.text = player.playerLeaderPoints.bossKill.ToString();
        plantText.text = player.playerLeaderPoints.plantPoint.ToString();
        rockText.text = player.playerLeaderPoints.rockPoint.ToString();
        treeText.text = player.playerLeaderPoints.treePoint.ToString();
        upgradeItemText.text = player.playerLeaderPoints.upgradeItemPoint.ToString();
        craftItemText.text = player.playerLeaderPoints.craftItemPoint.ToString();
        buildingText.text = player.playerLeaderPoints.buildinPoint.ToString();
        boostText.text = player.playerLeaderPoints.buyBoostPoint.ToString();

        personalSlider.value = player.playerLeaderPoints.personalPercent();
        groupSlider.value = player.playerLeaderPoints.groupPercent();
        allianceSlider.value = player.playerLeaderPoints.allyPercent();

        personalSlider.transform.Find("TextProgress").GetComponent<Text>().text = player.playerLeaderPoints.personalPercent() * 100 + "%";
        groupSlider.transform.Find("TextProgress").GetComponent<Text>().text = player.playerLeaderPoints.groupPercent() * 100 + "%";
        allianceSlider.transform.Find("TextProgress").GetComponent<Text>().text = player.playerLeaderPoints.allyPercent() * 100 + "%";

        personalReward.gameObject.SetActive(personalItem.Count > 0);
        groupReward.gameObject.SetActive(groupItem.Count > 0);
        allianceReward.gameObject.SetActive(allyItem.Count > 0);

        CheckItem();
    }

    public void CheckItem()
    {
        for (int i = 0; i < GeneralManager.singleton.leaderboardReward.Count; i++)
        {
            int index = i;
            if ((GeneralManager.singleton.leaderboardReward[index].personalPoint) > 0 && !player.playerLeaderPoints.personalClaimed.Contains((GeneralManager.singleton.leaderboardReward[index].personalPoint)))
            {
                if (!personalItem.Contains(GeneralManager.singleton.leaderboardReward[index]) && player.playerLeaderPoints.personalPoint >= GeneralManager.singleton.leaderboardReward[index].personalPoint)
                    personalItem.Add(GeneralManager.singleton.leaderboardReward[index]);
            }
            else
            {
                personalItem.Remove(GeneralManager.singleton.leaderboardReward[index]);
            }
            if ((GeneralManager.singleton.leaderboardReward[index].groupPoint) > 0 && !player.playerLeaderPoints.groupClaimed.Contains((GeneralManager.singleton.leaderboardReward[index].groupPoint)))
            {
                if (!groupItem.Contains(GeneralManager.singleton.leaderboardReward[index]) && player.playerLeaderPoints.groupPoint >= GeneralManager.singleton.leaderboardReward[index].groupPoint)
                    groupItem.Add(GeneralManager.singleton.leaderboardReward[index]);
            }
            else
            {
                groupItem.Remove(GeneralManager.singleton.leaderboardReward[index]);
            }

            if ((GeneralManager.singleton.leaderboardReward[index].allyPoint) > 0 && !player.playerLeaderPoints.allianceClaimed.Contains((GeneralManager.singleton.leaderboardReward[index].allyPoint)))
            {
                if (!allyItem.Contains(GeneralManager.singleton.leaderboardReward[index]) && player.playerLeaderPoints.allyPoint >= GeneralManager.singleton.leaderboardReward[index].allyPoint)
                    allyItem.Add(GeneralManager.singleton.leaderboardReward[index]);
            }
            else
            {
                allyItem.Remove(GeneralManager.singleton.leaderboardReward[index]);
            }
        }
    }

}

public partial class UILeaderboardRewards : MonoBehaviour
{
    public static UILeaderboardRewards singleton;
    private Player player;
    public GameObject rewardToSpawn;
    public Transform content;
    public Button closeButton;
    public List<LeaderboardItem> reward = new List<LeaderboardItem>();

    // Start is called before the first frame update
    void Start()
    {
        if (!singleton) singleton = this;
        for (int i = 0; i < UILeaderboard.singleton.personalItem.Count; i++)
        {
            int index = i;
            if (!reward.Contains(UILeaderboard.singleton.personalItem[index]))
                reward.Add(UILeaderboard.singleton.personalItem[index]);
        }
        for (int e = 0; e < UILeaderboard.singleton.groupItem.Count; e++)
        {
            int index = e;
            if (!reward.Contains(UILeaderboard.singleton.groupItem[index]))
                reward.Add(UILeaderboard.singleton.groupItem[index]);
        }
        for (int a = 0; a < UILeaderboard.singleton.allyItem.Count; a++)
        {
            int index = a;
            if (!reward.Contains(UILeaderboard.singleton.allyItem[index]))
                reward.Add(UILeaderboard.singleton.allyItem[index]);
        }

        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

    }

    // Update is called once per frame
    void Update()
    {


        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.health == 0)
            Destroy(this.gameObject);

        UIUtils.BalancePrefabs(rewardToSpawn, reward.Count, content);
        for (int re = 0; re < reward.Count; re++)
        {
            int index = re;
            LeaderboardRewardSlot rewardSlot = content.GetChild(index).GetComponent<LeaderboardRewardSlot>();
            rewardSlot.index = index;
            if (reward[index].personalPoint > 0)
                rewardSlot.category = 0;
            if (reward[index].groupPoint > 0)
                rewardSlot.category = 1;
            if (reward[index].allyPoint > 0)
                rewardSlot.category = 2;

            //Color c = rewardSlot.imageToColor.GetComponent<Image>().color;

            if (rewardSlot.category == 0)
            {
                //c = GeneralManager.singleton.personalColor;
                //rewardSlot.GetComponent<Image>().color = c;
                rewardSlot.rewardName.text = "Name : " + reward[rewardSlot.index].items.name;
                rewardSlot.rewardAmount.text = "Amount : " + reward[rewardSlot.index].amount;
                rewardSlot.image.sprite = reward[rewardSlot.index].items.image;
            }
            if (rewardSlot.category == 1)
            {
                //c = GeneralManager.singleton.groupColor;
                //rewardSlot.GetComponent<Image>().color = c;
                rewardSlot.rewardName.text = "Name : " + reward[rewardSlot.index].items.name;
                rewardSlot.rewardAmount.text = "Amount : " + reward[rewardSlot.index].amount;
                rewardSlot.image.sprite = reward[rewardSlot.index].items.image;
            }
            if (rewardSlot.category == 2)
            {
                //c = GeneralManager.singleton.allyClor;
                //rewardSlot.GetComponent<Image>().color = c;
                rewardSlot.rewardName.text = "Name : " + reward[rewardSlot.index].items.name;
                rewardSlot.rewardAmount.text = "Amount : " + reward[rewardSlot.index].amount;
                rewardSlot.image.sprite = reward[rewardSlot.index].items.image;
            }

            rewardSlot.rewardButton.onClick.SetListener(() =>
            {
                if (rewardSlot.category == 0)
                    player.playerLeaderPoints.CmdAddLeaderboardItem(rewardSlot.category, reward[rewardSlot.index].items.name, reward[rewardSlot.index].amount, reward[rewardSlot.index].personalPoint);
                if (rewardSlot.category == 1)
                    player.playerLeaderPoints.CmdAddLeaderboardItem(rewardSlot.category, reward[rewardSlot.index].items.name, reward[rewardSlot.index].amount, reward[rewardSlot.index].groupPoint);
                if (rewardSlot.category == 2)
                    player.playerLeaderPoints.CmdAddLeaderboardItem(rewardSlot.category, reward[rewardSlot.index].items.name, reward[rewardSlot.index].amount, reward[rewardSlot.index].allyPoint);
            });
        }
    }

    public void ReworkItem()
    {
        reward.Clear();
        for (int i = 0; i < UILeaderboard.singleton.personalItem.Count; i++)
        {
            int index = i;
            if (!reward.Contains(UILeaderboard.singleton.personalItem[index]))
                reward.Add(UILeaderboard.singleton.personalItem[index]);
        }
        for (int e = 0; e < UILeaderboard.singleton.groupItem.Count; e++)
        {
            int index = e;
            if (!reward.Contains(UILeaderboard.singleton.groupItem[index]))
                reward.Add(UILeaderboard.singleton.groupItem[index]);
        }
        for (int a = 0; a < UILeaderboard.singleton.allyItem.Count; a++)
        {
            int index = a;
            if (!reward.Contains(UILeaderboard.singleton.allyItem[index]))
                reward.Add(UILeaderboard.singleton.allyItem[index]);
        }
    }

}

public partial class UIUpgradeRepair : MonoBehaviour
{
    public static UIUpgradeRepair singleton;
    private Player player;
    public Transform upgradeContent;
    public Transform repairContent;
    public GameObject upgradeslot;
    public Button closeButton;
    public Button openHistotyButton;
    public GameObject historyPanel;

    public List<UpgradeItem> upgradableItems = new List<UpgradeItem>();
    public List<int> repairItems = new List<int>();


    void Update()
    {
        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

        openHistotyButton.onClick.SetListener(() =>
        {
            Instantiate(historyPanel, GeneralManager.singleton.canvas);
        });

        if (!player) player = Player.localPlayer;
        if (!player) return;
        else
        {
            RecalculateItem();
            RecalculateItemRepair();
        }

        if (player.health == 0)
            closeButton.onClick.Invoke();

        RecalculateItem();

        UIUtils.BalancePrefabs(upgradeslot, upgradableItems.Count, upgradeContent);
        for (int i = 0; i < upgradableItems.Count; i++)
        {
            int upgradeItem = i;
            if (player.inventory[upgradableItems[upgradeItem].inventoryIndex].amount <= 0) continue;
            UIUpgradeSlot slot = upgradeContent.GetChild(upgradeItem).GetComponent<UIUpgradeSlot>();
            slot.image.sprite = player.inventory[upgradableItems[upgradeItem].inventoryIndex].item.image;
            slot.itemText.text = player.inventory[upgradableItems[upgradeItem].inventoryIndex].item.name;
            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                slot.itemLevel.text = "Livello " + upgradableItems[upgradeItem].upgradeType + " : " + upgradableItems[upgradeItem].actualLevel + " / " + upgradableItems[upgradeItem].maxLevel;
                slot.itemButton.GetComponentInChildren<TextMeshProUGUI>().text = "Aumenta!";
            }
            else
            {
                slot.itemLevel.text = upgradableItems[upgradeItem].upgradeType + " level : " + upgradableItems[upgradeItem].actualLevel + " / " + upgradableItems[upgradeItem].maxLevel;
                slot.itemButton.GetComponentInChildren<TextMeshProUGUI>().text = "Upgrade!";
            }
            slot.itemButton.onClick.SetListener(() =>
            {
                if (Player.localPlayer.target && Player.localPlayer.target.GetComponent<BuildingUpgradeRepair>())
                {
                    UpgradeRepairItem upgrade = new UpgradeRepairItem();
                    upgrade.item = player.inventory[upgradableItems[upgradeItem].inventoryIndex];
                    upgrade.playerName = Player.localPlayer.name;
                    upgrade.index = upgradableItems[upgradeItem].inventoryIndex;
                    upgrade.totalTime = player.inventory[upgradableItems[upgradeItem].inventoryIndex].item.data.upgradeTimer;
                    upgrade.remainingTime = upgrade.totalTime;
                    upgrade.type = upgradableItems[upgradeItem].upgradeType;
                    upgrade.operationType = "U";

                    GameObject g = Instantiate(GeneralManager.singleton.upgradeRepairMaterialPanel, GeneralManager.singleton.canvas);
                    UIUpgradeRepairMaterial uIUpgradeRepairMaterial = g.GetComponent<UIUpgradeRepairMaterial>();
                    uIUpgradeRepairMaterial.upgradeItem = upgrade;
                    uIUpgradeRepairMaterial.operationType = upgrade.type;
                    uIUpgradeRepairMaterial.upgrade = true;
                    uIUpgradeRepairMaterial.selectedItem = upgrade.index;
                }
            });
        }

        UIUtils.BalancePrefabs(upgradeslot, repairItems.Count, repairContent);
        for (int i = 0; i < repairItems.Count; i++)
        {
            int upgradeItem = i;
            if (player.inventory[repairItems[upgradeItem]].amount <= 0) continue;
            UIUpgradeSlot slot = repairContent.GetChild(upgradeItem).GetComponent<UIUpgradeSlot>();
            slot.image.sprite = player.inventory[repairItems[upgradeItem]].item.image;
            slot.itemText.text = player.inventory[repairItems[upgradeItem]].item.name;
            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                slot.itemLevel.text = "Durabilita' : " + player.inventory[repairItems[upgradeItem]].item.durability + " / " + player.inventory[repairItems[upgradeItem]].item.data.maxDurability.Get(player.inventory[repairItems[upgradeItem]].item.durabilityLevel);
                slot.itemButton.GetComponentInChildren<TextMeshProUGUI>().text = "Ripara!";
            }
            else
            {
                slot.itemLevel.text = "Durability : " + player.inventory[repairItems[upgradeItem]].item.durability + " / " + player.inventory[repairItems[upgradeItem]].item.data.maxDurability.Get(player.inventory[repairItems[upgradeItem]].item.durabilityLevel);
                slot.itemButton.GetComponentInChildren<TextMeshProUGUI>().text = "Repair!";
            }
            slot.itemButton.onClick.SetListener(() =>
            {
                if (Player.localPlayer.target && Player.localPlayer.target.GetComponent<BuildingUpgradeRepair>())
                {
                    UpgradeRepairItem upgrade = new UpgradeRepairItem();
                    upgrade.item = player.inventory[repairItems[upgradeItem]];
                    upgrade.playerName = Player.localPlayer.name;
                    upgrade.index = repairItems[upgradeItem];
                    upgrade.totalTime = player.inventory[repairItems[upgradeItem]].item.data.repairTimer;
                    upgrade.remainingTime = upgrade.totalTime;
                    upgrade.operationType = "R";

                    GameObject g = Instantiate(GeneralManager.singleton.upgradeRepairMaterialPanel, GeneralManager.singleton.canvas);
                    UIUpgradeRepairMaterial uIUpgradeRepairMaterial = g.GetComponent<UIUpgradeRepairMaterial>();
                    uIUpgradeRepairMaterial.upgradeItem = upgrade;
                    uIUpgradeRepairMaterial.operationType = upgrade.type;
                    uIUpgradeRepairMaterial.upgrade = false;
                    uIUpgradeRepairMaterial.selectedItem = upgrade.index;

                    //upgradableItems.Clear();
                    //repairItems.Clear();
                    //RecalculateItem();
                    //RecalculateItemRepair();

                }
            });
        }

    }
    public void OpenHistoryPanel()
    {
        Instantiate(historyPanel, GeneralManager.singleton.canvas);
    }

    public void RecalculateItem()
    {
        for (int i = 0; i < player.inventory.Count; i++)
        {
            int index = i;
            if (player.inventory[index].amount == 0) continue;
            ItemSlot itemSlot = player.inventory[index];

            if ((itemSlot.item.data).maxDurabilityLevel > 0 && itemSlot.item.durability < itemSlot.item.data.maxDurability.Get(itemSlot.item.durabilityLevel))
            {
                if (!repairItems.Contains(index))
                {
                    repairItems.Add(index);
                }
            }
            if ((itemSlot.item.data).maxDurabilityLevel > 0 && itemSlot.item.durability == itemSlot.item.data.maxDurability.Get(itemSlot.item.durabilityLevel))
            {
                if (repairItems.Contains(index))
                {
                    repairItems.Remove(index);
                }
            }

            if (itemSlot.item.data is EquipmentItem)
            {
                if (((EquipmentItem)itemSlot.item.data).maxAccuracyLevel > 0 && itemSlot.item.accuracyLevel < ((EquipmentItem)itemSlot.item.data).maxAccuracyLevel)
                {
                    UpgradeItem item = new UpgradeItem();
                    item.inventoryIndex = index;
                    item.maxLevel = ((EquipmentItem)itemSlot.item.data).maxAccuracyLevel;
                    item.actualLevel = itemSlot.item.accuracyLevel;
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        item.upgradeType = "accuratezza";
                    }
                    else
                    {
                        item.upgradeType = "Accuracy";
                    }
                    if (!upgradableItems.Contains(item))
                    {
                        upgradableItems.Add(item);
                    }
                }
                if (((EquipmentItem)itemSlot.item.data).maxMissLevel > 0 && itemSlot.item.missLevel < ((EquipmentItem)itemSlot.item.data).maxMissLevel)
                {
                    UpgradeItem item = new UpgradeItem();
                    item.inventoryIndex = index;
                    item.maxLevel = ((EquipmentItem)itemSlot.item.data).maxMissLevel;
                    item.actualLevel = itemSlot.item.missLevel;
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        item.upgradeType = "evasione";
                    }
                    else
                    {
                        item.upgradeType = "Evasion";
                    }

                    if (!upgradableItems.Contains(item))
                    {
                        upgradableItems.Add(item);
                    }
                }
                if (((EquipmentItem)itemSlot.item.data).maxArmorLevel > 0 && itemSlot.item.armorLevel < ((EquipmentItem)itemSlot.item.data).maxArmorLevel)
                {
                    UpgradeItem item = new UpgradeItem();
                    item.inventoryIndex = index;
                    item.maxLevel = ((EquipmentItem)itemSlot.item.data).maxArmorLevel;
                    item.actualLevel = itemSlot.item.armorLevel;
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        item.upgradeType = "armatura";
                    }
                    else
                    {
                        item.upgradeType = "Armor";
                    }
                    if (!upgradableItems.Contains(item))
                    {
                        upgradableItems.Add(item);
                    }
                }
                if (((EquipmentItem)itemSlot.item.data).maxChargeLevel > 0 && itemSlot.item.chargeLevel < ((EquipmentItem)itemSlot.item.data).maxChargeLevel)
                {
                    UpgradeItem item = new UpgradeItem();
                    item.inventoryIndex = index;
                    item.maxLevel = ((EquipmentItem)itemSlot.item.data).maxChargeLevel;
                    item.actualLevel = itemSlot.item.chargeLevel;
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        item.upgradeType = "carica munizioni";
                    }
                    else
                    {
                        item.upgradeType = "Munition charge";
                    }
                    if (!upgradableItems.Contains(item))
                    {
                        upgradableItems.Add(item);
                    }
                }
            }
            if (itemSlot.item.data is ScriptableRadio)
            {
                if (((ScriptableRadio)itemSlot.item.data).maxCurrentBattery > 0 && itemSlot.item.radioCurrentBattery < ((ScriptableRadio)itemSlot.item.data).maxCurrentBattery)
                {
                    UpgradeItem item = new UpgradeItem();
                    item.inventoryIndex = index;
                    item.maxLevel = ((ScriptableRadio)itemSlot.item.data).maxCurrentBattery;
                    item.actualLevel = itemSlot.item.radioCurrentBattery;
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        item.upgradeType = "batteria radio";
                    }
                    else
                    {
                        item.upgradeType = "Radio battery";
                    }
                    if (!upgradableItems.Contains(item))
                    {
                        upgradableItems.Add(item);
                    }
                    if (!upgradableItems.Contains(item))
                    {
                        upgradableItems.Add(item);
                    }
                }
            }
            if (itemSlot.item.data is ScriptableTorch)
            {
                if (((ScriptableTorch)itemSlot.item.data).maxCurrentBattery > 0 && itemSlot.item.torchCurrentBattery < ((ScriptableTorch)itemSlot.item.data).maxCurrentBattery)
                {
                    UpgradeItem item = new UpgradeItem();
                    item.inventoryIndex = index;
                    item.maxLevel = ((ScriptableTorch)itemSlot.item.data).maxCurrentBattery;
                    item.actualLevel = itemSlot.item.torchCurrentBattery;
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        item.upgradeType = "batteria torcia";
                    }
                    else
                    {
                        item.upgradeType = "Torch battery";
                    }
                    if (!upgradableItems.Contains(item))
                    {
                        upgradableItems.Add(item);
                    }
                }
            }
            if (itemSlot.item.data is EquipmentItem)
            {
                if (((EquipmentItem)itemSlot.item.data).additionalSlot.baseValue > 0 && itemSlot.item.bagLevel < ((EquipmentItem)itemSlot.item.data).maxBagLevel)
                {
                    UpgradeItem item = new UpgradeItem();
                    item.inventoryIndex = index;
                    item.maxLevel = ((EquipmentItem)itemSlot.item.data).maxBagLevel;
                    item.actualLevel = itemSlot.item.bagLevel;
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        item.upgradeType = "slot borsa";
                    }
                    else
                    {
                        item.upgradeType = "Bag Slot";
                    }
                    if (!upgradableItems.Contains(item))
                    {
                        upgradableItems.Add(item);
                    }
                }
            }
            if (itemSlot.item.data is EquipmentItem)
            {
                if (((EquipmentItem)itemSlot.item.data).protectedSlot.baseValue > 0 && itemSlot.item.bagLevel < ((EquipmentItem)itemSlot.item.data).maxBagLevel)
                {
                    UpgradeItem item = new UpgradeItem();
                    item.inventoryIndex = index;
                    item.maxLevel = ((EquipmentItem)itemSlot.item.data).maxBagLevel;
                    item.actualLevel = itemSlot.item.bagLevel;
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        item.upgradeType = "slot protetti borsa";
                    }
                    else
                    {
                        item.upgradeType = "Bag Protected Slot";
                    }
                    if (!upgradableItems.Contains(item))
                    {
                        upgradableItems.Add(item);
                    }
                }
            }
            if (itemSlot.item.data is ScriptableItem)
            {
                if (((ScriptableItem)itemSlot.item.data).maxDurabilityLevel > 0 && itemSlot.item.durabilityLevel < ((ScriptableItem)itemSlot.item.data).maxDurabilityLevel)
                {
                    UpgradeItem item = new UpgradeItem();
                    item.inventoryIndex = index;
                    item.maxLevel = ((ScriptableItem)itemSlot.item.data).maxDurabilityLevel;
                    item.actualLevel = itemSlot.item.durabilityLevel;
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        item.upgradeType = "durabilita'";
                    }
                    else
                    {
                        item.upgradeType = "Durability";
                    }
                    if (!upgradableItems.Contains(item))
                    {
                        upgradableItems.Add(item);
                    }
                }
            }
            if (itemSlot.item.data is FoodItem)
            {
                if (((FoodItem)itemSlot.item.data).maxUnsanityLevel > 0 && itemSlot.item.unsanityLevel < ((FoodItem)itemSlot.item.data).maxUnsanityLevel)
                {
                    UpgradeItem item = new UpgradeItem();
                    item.inventoryIndex = index;
                    item.maxLevel = ((FoodItem)itemSlot.item.data).maxUnsanityLevel;
                    item.actualLevel = itemSlot.item.unsanityLevel;
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        item.upgradeType = "sanita' oggetto";
                    }
                    else
                    {
                        item.upgradeType = "Unsanity";
                    }

                    if (!upgradableItems.Contains(item))
                    {
                        upgradableItems.Add(item);
                    }
                }
            }
            if (itemSlot.item.data is ScriptableItem)
            {
                if (((ScriptableItem)itemSlot.item.data).maxWeightLevel > 0 && itemSlot.item.weightLevel < ((ScriptableItem)itemSlot.item.data).maxWeightLevel)
                {
                    UpgradeItem item = new UpgradeItem();
                    item.inventoryIndex = index;
                    item.maxLevel = ((ScriptableItem)itemSlot.item.data).maxWeightLevel;
                    item.actualLevel = itemSlot.item.weightLevel;
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        item.upgradeType = "peso oggetto";
                    }
                    else
                    {
                        item.upgradeType = "Item Weight";
                    }
                    if (!upgradableItems.Contains(item))
                    {
                        upgradableItems.Add(item);
                    }
                }
            }
        }
        for (int i = 0; i < upgradableItems.Count; i++)
        {
            if (player.inventory[upgradableItems[i].inventoryIndex].amount == 0)
            {
                upgradableItems.Remove(upgradableItems[i]);
            }
        }
        for (int i = 0; i < repairItems.Count; i++)
        {
            if (player.inventory[repairItems[i]].amount == 0)
            {
                repairItems.Remove(repairItems[i]);
            }
        }
    }

    public void RecalculateItemRepair()
    {
        for (int i = 0; i < player.inventory.Count; i++)
        {
            int index = i;
            if (player.inventory[index].amount <= 0) continue;
            ItemSlot itemSlot = player.inventory[index];

            if ((itemSlot.item.data).maxDurability.baseValue > 0 && itemSlot.item.durability < itemSlot.item.data.maxDurability.Get(itemSlot.item.durabilityLevel))
            {
                if (!repairItems.Contains(index))
                {
                    repairItems.Add(index);
                }
            }
        }
    }

}

public partial class UIUpgradeRepairChild : MonoBehaviour
{
    public static UIUpgradeRepairChild singleton;
    private Player player;
    public Transform upgradeContent;
    public Transform repairContent;
    public GameObject upgradeslot;
    public Button closeButton;
    public Button switchButton;
    public bool complete;

    public BuildingUpgradeRepair building;

    public List<UpgradeRepairItem> upgradeItem = new List<UpgradeRepairItem>();
    public List<UpgradeRepairItem> finishUpgradeItem = new List<UpgradeRepairItem>();
    public List<UpgradeRepairItem> repairItem = new List<UpgradeRepairItem>();
    public List<UpgradeRepairItem> finishRepairItem = new List<UpgradeRepairItem>();

    public UpgradeRepairItem runtimeItem;

    void Update()
    {
        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

        switchButton.onClick.SetListener(() =>
        {
            complete = !complete;
        });

        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.health == 0)
            closeButton.onClick.Invoke();

        if (!player.target) return;
        if (!building) building = player.target.GetComponent<BuildingUpgradeRepair>();
        if (!building) return;

        upgradeItem.Clear();
        finishUpgradeItem.Clear();
        repairItem.Clear();
        finishRepairItem.Clear();

        for (int i = 0; i < building.upgradeItem.Count; i++)
        {
            int index = i;
            runtimeItem = new UpgradeRepairItem();
            runtimeItem = building.upgradeItem[index];
            runtimeItem.index = index;
            TimeSpan difference = DateTime.Parse(runtimeItem.timeEnd) - DateTime.Now;
            if (difference.TotalSeconds > 0)
            {
                upgradeItem.Add(runtimeItem);
            }
            else
            {
                finishUpgradeItem.Add(runtimeItem);
            }
        }

        for (int i = 0; i < building.repairItem.Count; i++)
        {
            int index = i;
            runtimeItem = new UpgradeRepairItem();
            runtimeItem = building.repairItem[index];
            runtimeItem.index = index;
            TimeSpan difference = DateTime.Parse(runtimeItem.timeEnd) - DateTime.Now;
            if (difference.TotalSeconds > 0)
            {
                repairItem.Add(runtimeItem);
            }
            else
            {
                finishRepairItem.Add(runtimeItem);
            }
        }


        if (!complete)
        {
            UIUtils.BalancePrefabs(upgradeslot, upgradeItem.Count, upgradeContent);
            for (int i = 0; i < upgradeItem.Count; i++)
            {
                int index = i;
                UIUpgradeSlot slot = upgradeContent.GetChild(index).GetComponent<UIUpgradeSlot>();
                if (upgradeItem[index].item.item.data is ScriptableBuilding)
                {
                    if (ScriptableBuilding.dict.TryGetValue(((ScriptableBuilding)upgradeItem[index].item.item.data).name.GetStableHashCode(), out ScriptableBuilding tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (upgradeItem[index].item.item.data is ScriptableItem)
                {
                    if (ScriptableItem.dict.TryGetValue(((ScriptableItem)upgradeItem[index].item.item.data).name.GetStableHashCode(), out ScriptableItem tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (upgradeItem[index].item.item.data is FoodItem)
                {
                    if (ScriptableItem.dict.TryGetValue(((ScriptableItem)upgradeItem[index].item.item.data).name.GetStableHashCode(), out ScriptableItem tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (upgradeItem[index].item.item.data is EquipmentItem)
                {
                    if (ScriptableItem.dict.TryGetValue(((ScriptableItem)upgradeItem[index].item.item.data).name.GetStableHashCode(), out ScriptableItem tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (upgradeItem[index].item.item.data is ScriptablePlant)
                {
                    if (ScriptablePlant.dict.TryGetValue(((ScriptablePlant)upgradeItem[index].item.item.data).name.GetStableHashCode(), out ScriptablePlant tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (upgradeItem[index].item.item.data is ScriptableRadio)
                {
                    if (ScriptableRadio.dict.TryGetValue(((ScriptableRadio)upgradeItem[index].item.item.data).name.GetStableHashCode(), out ScriptableRadio tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (upgradeItem[index].item.item.data is ScriptableTorch)
                {
                    if (ScriptableTorch.dict.TryGetValue(((ScriptableTorch)upgradeItem[index].item.item.data).name.GetStableHashCode(), out ScriptableTorch tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (upgradeItem[index].item.item.data is TeleportItem)
                {
                    if (ScriptableItem.dict.TryGetValue(((ScriptableItem)upgradeItem[index].item.item.data).name.GetStableHashCode(), out ScriptableItem tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (upgradeItem[index].item.item.data is TeleportItem)
                {
                    if (ScriptableItem.dict.TryGetValue(((ScriptableItem)upgradeItem[index].item.item.data).name.GetStableHashCode(), out ScriptableItem tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (upgradeItem[index].item.item.data is ScriptableWood)
                {
                    if (ScriptableItem.dict.TryGetValue(((ScriptableItem)upgradeItem[index].item.item.data).name.GetStableHashCode(), out ScriptableItem tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }

                if (upgradeItem[index].type == "Accuracy")
                {
                    slot.itemLevel.text = upgradeItem[index].type + " level : " + (upgradeItem[index].item.item.accuracyLevel + 1) + " / " + ((EquipmentItem)upgradeItem[index].item.item.data).maxAccuracyLevel;
                }
                if (upgradeItem[index].type == "accuratezza")
                {
                    slot.itemLevel.text = "Livello " + upgradeItem[index].type + " : " + (upgradeItem[index].item.item.accuracyLevel + 1) + " / " + ((EquipmentItem)upgradeItem[index].item.item.data).maxAccuracyLevel;
                }

                if (upgradeItem[index].type == "Evasion")
                {
                    slot.itemLevel.text = upgradeItem[index].type + " level : " + (upgradeItem[index].item.item.missLevel + 1) + " / " + ((EquipmentItem)upgradeItem[index].item.item.data).maxMissLevel;
                }
                if (upgradeItem[index].type == "evasione")
                {
                    slot.itemLevel.text = "Livello " + upgradeItem[index].type + " : " + (upgradeItem[index].item.item.missLevel + 1) + " / " + ((EquipmentItem)upgradeItem[index].item.item.data).maxMissLevel;
                }

                if (upgradeItem[index].type == "Armor")
                {
                    slot.itemLevel.text = upgradeItem[index].type + " level : " + (upgradeItem[index].item.item.armorLevel + 1) + " / " + ((EquipmentItem)upgradeItem[index].item.item.data).maxArmorLevel;
                }
                if (upgradeItem[index].type == "armatura")
                {
                    slot.itemLevel.text = "Livello " + upgradeItem[index].type + " : " + (upgradeItem[index].item.item.armorLevel + 1) + " / " + ((EquipmentItem)upgradeItem[index].item.item.data).maxArmorLevel;
                }

                if (upgradeItem[index].type == "Munition charge")
                {
                    slot.itemLevel.text = upgradeItem[index].type + " level : " + (upgradeItem[index].item.item.chargeLevel + 1) + " / " + ((EquipmentItem)upgradeItem[index].item.item.data).maxChargeLevel;
                }
                if (upgradeItem[index].type == "carica munizioni")
                {
                    slot.itemLevel.text = "Livello " + upgradeItem[index].type + " : " + (upgradeItem[index].item.item.chargeLevel + 1) + " / " + ((EquipmentItem)upgradeItem[index].item.item.data).maxChargeLevel;
                }

                if (upgradeItem[index].type == "Radio Battery")
                {
                    slot.itemLevel.text = upgradeItem[index].type + " level : " + (upgradeItem[index].item.item.radioCurrentBattery + 1) + " / " + ((ScriptableRadio)upgradeItem[index].item.item.data).maxCurrentBattery;
                }
                if (upgradeItem[index].type == "batteria radio")
                {
                    slot.itemLevel.text = "Livello " + upgradeItem[index].type + " : " + (upgradeItem[index].item.item.radioCurrentBattery + 1) + " / " + ((ScriptableRadio)upgradeItem[index].item.item.data).maxCurrentBattery;
                }

                if (upgradeItem[index].type == "Torch Battery")
                {
                    slot.itemLevel.text = upgradeItem[index].type + " level : " + (upgradeItem[index].item.item.torchCurrentBattery + 1) + " / " + ((ScriptableTorch)upgradeItem[index].item.item.data).maxCurrentBattery;
                }
                if (upgradeItem[index].type == "batteria torcia")
                {
                    slot.itemLevel.text = "Livello " + upgradeItem[index].type + " : " + (upgradeItem[index].item.item.torchCurrentBattery + 1) + " / " + ((ScriptableTorch)upgradeItem[index].item.item.data).maxCurrentBattery;
                }

                if (upgradeItem[index].type == "Bag Slot")
                {
                    slot.itemLevel.text = upgradeItem[index].type + " level : " + (upgradeItem[index].item.item.bagLevel + 1) + " / " + ((EquipmentItem)upgradeItem[index].item.item.data).maxBagLevel;
                }
                if (upgradeItem[index].type == "slot borsa")
                {
                    slot.itemLevel.text = "Livello " + upgradeItem[index].type + " : " + (upgradeItem[index].item.item.bagLevel + 1) + " / " + ((EquipmentItem)upgradeItem[index].item.item.data).maxBagLevel;
                }

                if (upgradeItem[index].type == "Bag Protected Slot")
                {
                    slot.itemLevel.text = upgradeItem[index].type + " level : " + (upgradeItem[index].item.item.bagLevel + 1) + " / " + ((EquipmentItem)upgradeItem[index].item.item.data).maxBagLevel;
                }
                if (upgradeItem[index].type == "slot protetti borsa")
                {
                    slot.itemLevel.text = "Livello " + upgradeItem[index].type + " : " + (upgradeItem[index].item.item.bagLevel + 1) + " / " + ((EquipmentItem)upgradeItem[index].item.item.data).maxBagLevel;
                }

                if (upgradeItem[index].type == "Durability")
                {
                    slot.itemLevel.text = upgradeItem[index].type + " level : " + (upgradeItem[index].item.item.durabilityLevel + 1) + " / " + ((ScriptableItem)upgradeItem[index].item.item.data).maxDurabilityLevel;
                }
                if (upgradeItem[index].type == "durabilita'")
                {
                    slot.itemLevel.text = "Livello " + upgradeItem[index].type + " : " + (upgradeItem[index].item.item.durabilityLevel + 1) + " / " + ((ScriptableItem)upgradeItem[index].item.item.data).maxDurabilityLevel;
                }

                if (upgradeItem[index].type == "Unsanity")
                {
                    slot.itemLevel.text = upgradeItem[index].type + " level : " + (upgradeItem[index].item.item.unsanityLevel + 1) + " / " + ((FoodItem)upgradeItem[index].item.item.data).maxUnsanityLevel;
                }
                if (upgradeItem[index].type == "sanita' oggetto")
                {
                    slot.itemLevel.text = "Livello " + upgradeItem[index].type + " : " + (upgradeItem[index].item.item.unsanityLevel + 1) + " / " + ((FoodItem)upgradeItem[index].item.item.data).maxUnsanityLevel;
                }

                if (upgradeItem[index].type == "Item Weight")
                {
                    slot.itemLevel.text = upgradeItem[index].type + " level : " + (upgradeItem[index].item.item.weightLevel + 1) + " / " + ((ScriptableItem)upgradeItem[index].item.item.data).maxWeightLevel;
                }
                if (upgradeItem[index].type == "peso oggetto")
                {
                    slot.itemLevel.text = "Livello " + upgradeItem[index].type + " : " + (upgradeItem[index].item.item.weightLevel + 1) + " / " + ((FoodItem)upgradeItem[index].item.item.data).maxWeightLevel;
                }

                TimeSpan difference = DateTime.Parse(upgradeItem[index].timeEnd) - DateTime.Now;
                if (Convert.ToInt32(difference.TotalSeconds) < 0 && upgradeItem[index].playerName == player.name)
                {
                    slot.itemButton.GetComponent<Image>().enabled = true;
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        slot.itemButton.GetComponentInChildren<TextMeshProUGUI>().text = "Prendi";
                    }
                    else
                    {
                        slot.itemButton.GetComponentInChildren<TextMeshProUGUI>().text = "Claim";
                    }
                }
                else
                {
                    slot.itemButton.GetComponent<Image>().enabled = false;
                    slot.itemButton.GetComponentInChildren<TextMeshProUGUI>().text = GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds));
                }
            }

            UIUtils.BalancePrefabs(upgradeslot, repairItem.Count, repairContent);
            for (int i = 0; i < repairItem.Count; i++)
            {
                int upgradeItem = i;
                UIUpgradeSlot slot = repairContent.GetChild(upgradeItem).GetComponent<UIUpgradeSlot>();
                if (repairItem[upgradeItem].item.item.data is ScriptableBuilding)
                {
                    if (ScriptableBuilding.dict.TryGetValue(((ScriptableBuilding)repairItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableBuilding tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (repairItem[upgradeItem].item.item.data is ScriptableItem)
                {
                    if (ScriptableItem.dict.TryGetValue(((ScriptableItem)repairItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableItem tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (repairItem[upgradeItem].item.item.data is FoodItem)
                {
                    if (ScriptableItem.dict.TryGetValue(((ScriptableItem)repairItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableItem tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (repairItem[upgradeItem].item.item.data is EquipmentItem)
                {
                    if (ScriptableItem.dict.TryGetValue(((ScriptableItem)repairItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableItem tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (repairItem[upgradeItem].item.item.data is ScriptablePlant)
                {
                    if (ScriptablePlant.dict.TryGetValue(((ScriptablePlant)repairItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptablePlant tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (repairItem[upgradeItem].item.item.data is ScriptableRadio)
                {
                    if (ScriptableRadio.dict.TryGetValue(((ScriptableRadio)repairItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableRadio tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (repairItem[upgradeItem].item.item.data is ScriptableTorch)
                {
                    if (ScriptableTorch.dict.TryGetValue(((ScriptableTorch)repairItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableTorch tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (repairItem[upgradeItem].item.item.data is TeleportItem)
                {
                    if (ScriptableItem.dict.TryGetValue(((ScriptableItem)repairItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableItem tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (repairItem[upgradeItem].item.item.data is TeleportItem)
                {
                    if (ScriptableItem.dict.TryGetValue(((ScriptableItem)repairItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableItem tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (repairItem[upgradeItem].item.item.data is ScriptableWood)
                {
                    if (ScriptableItem.dict.TryGetValue(((ScriptableItem)repairItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableItem tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    slot.itemLevel.text = "Durabila' : " + repairItem[upgradeItem].item.item.durability +
                           " / " + ((ScriptableItem)repairItem[upgradeItem].item.item.data).maxDurability.Get(repairItem[upgradeItem].item.item.durabilityLevel);

                }
                else
                {
                    slot.itemLevel.text = "Durability : " + repairItem[upgradeItem].item.item.durability +
                        " / " + ((ScriptableItem)repairItem[upgradeItem].item.item.data).maxDurability.Get(repairItem[upgradeItem].item.item.durabilityLevel);

                }
                TimeSpan difference = DateTime.Parse(repairItem[upgradeItem].timeEnd) - DateTime.Now;
                if (Convert.ToInt32(difference.TotalSeconds) < 0 && repairItem[upgradeItem].playerName == player.name)
                {
                    slot.itemButton.GetComponent<Image>().enabled = true;
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        slot.itemButton.GetComponentInChildren<TextMeshProUGUI>().text = "Prendi";
                    }
                    else
                    {
                        slot.itemButton.GetComponentInChildren<TextMeshProUGUI>().text = "Claim";
                    }
                }
                else
                {
                    slot.itemButton.GetComponent<Image>().enabled = false;
                    slot.itemButton.GetComponentInChildren<TextMeshProUGUI>().text = GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds));
                }
            }
        }
        else
        {
            UIUtils.BalancePrefabs(upgradeslot, finishUpgradeItem.Count, upgradeContent);
            for (int i = 0; i < finishUpgradeItem.Count; i++)
            {
                int upgradeItem = i;
                UIUpgradeSlot slot = upgradeContent.GetChild(upgradeItem).GetComponent<UIUpgradeSlot>();
                if (finishUpgradeItem[upgradeItem].item.item.data is ScriptableBuilding)
                {
                    if (ScriptableBuilding.dict.TryGetValue(((ScriptableBuilding)finishUpgradeItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableBuilding tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (finishUpgradeItem[upgradeItem].item.item.data is ScriptableItem)
                {
                    if (ScriptableItem.dict.TryGetValue(((ScriptableItem)finishUpgradeItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableItem tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (finishUpgradeItem[upgradeItem].item.item.data is FoodItem)
                {
                    if (ScriptableItem.dict.TryGetValue(((ScriptableItem)finishUpgradeItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableItem tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (finishUpgradeItem[upgradeItem].item.item.data is EquipmentItem)
                {
                    if (ScriptableItem.dict.TryGetValue(((ScriptableItem)finishUpgradeItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableItem tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (finishUpgradeItem[upgradeItem].item.item.data is ScriptablePlant)
                {
                    if (ScriptablePlant.dict.TryGetValue(((ScriptablePlant)finishUpgradeItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptablePlant tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (finishUpgradeItem[upgradeItem].item.item.data is ScriptableRadio)
                {
                    if (ScriptableRadio.dict.TryGetValue(((ScriptableRadio)finishUpgradeItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableRadio tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (finishUpgradeItem[upgradeItem].item.item.data is ScriptableTorch)
                {
                    if (ScriptableTorch.dict.TryGetValue(((ScriptableTorch)finishUpgradeItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableTorch tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (finishUpgradeItem[upgradeItem].item.item.data is TeleportItem)
                {
                    if (ScriptableItem.dict.TryGetValue(((ScriptableItem)finishUpgradeItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableItem tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (finishUpgradeItem[upgradeItem].item.item.data is TeleportItem)
                {
                    if (ScriptableItem.dict.TryGetValue(((ScriptableItem)finishUpgradeItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableItem tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (finishUpgradeItem[upgradeItem].item.item.data is ScriptableWood)
                {
                    if (ScriptableItem.dict.TryGetValue(((ScriptableItem)finishUpgradeItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableItem tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }

                if (finishUpgradeItem[upgradeItem].type == "Accuracy")
                {
                    slot.itemLevel.text = finishUpgradeItem[upgradeItem].type + " level : " + (finishUpgradeItem[upgradeItem].item.item.accuracyLevel + 1) + " / " + ((EquipmentItem)finishUpgradeItem[upgradeItem].item.item.data).maxAccuracyLevel;
                }
                if (finishUpgradeItem[upgradeItem].type == "accuratezza")
                {
                    slot.itemLevel.text = "Livello " + finishUpgradeItem[upgradeItem].type + " : " + (finishUpgradeItem[upgradeItem].item.item.accuracyLevel + 1) + " / " + ((EquipmentItem)finishUpgradeItem[upgradeItem].item.item.data).maxAccuracyLevel;
                }

                if (finishUpgradeItem[upgradeItem].type == "Evasion")
                {
                    slot.itemLevel.text = finishUpgradeItem[upgradeItem].type + " level : " + (finishUpgradeItem[upgradeItem].item.item.missLevel + 1) + " / " + ((EquipmentItem)finishUpgradeItem[upgradeItem].item.item.data).maxMissLevel;
                }
                if (finishUpgradeItem[upgradeItem].type == "evasione")
                {
                    slot.itemLevel.text = "Livello " + finishUpgradeItem[upgradeItem].type + " : " + (finishUpgradeItem[upgradeItem].item.item.missLevel + 1) + " / " + ((EquipmentItem)finishUpgradeItem[upgradeItem].item.item.data).maxMissLevel;
                }

                if (finishUpgradeItem[upgradeItem].type == "Armor")
                {
                    slot.itemLevel.text = finishUpgradeItem[upgradeItem].type + " level : " + (finishUpgradeItem[upgradeItem].item.item.armorLevel + 1) + " / " + ((EquipmentItem)finishUpgradeItem[upgradeItem].item.item.data).maxArmorLevel;
                }
                if (finishUpgradeItem[upgradeItem].type == "armatura")
                {
                    slot.itemLevel.text = "Livello " + finishUpgradeItem[upgradeItem].type + " : " + (finishUpgradeItem[upgradeItem].item.item.armorLevel + 1) + " / " + ((EquipmentItem)finishUpgradeItem[upgradeItem].item.item.data).maxArmorLevel;
                }

                if (finishUpgradeItem[upgradeItem].type == "Munition charge")
                {
                    slot.itemLevel.text = finishUpgradeItem[upgradeItem].type + " level : " + (finishUpgradeItem[upgradeItem].item.item.chargeLevel + 1) + " / " + ((EquipmentItem)finishUpgradeItem[upgradeItem].item.item.data).maxChargeLevel;
                }
                if (finishUpgradeItem[upgradeItem].type == "carica munizioni")
                {
                    slot.itemLevel.text = "Livello " + finishUpgradeItem[upgradeItem].type + " : " + (finishUpgradeItem[upgradeItem].item.item.chargeLevel + 1) + " / " + ((EquipmentItem)finishUpgradeItem[upgradeItem].item.item.data).maxChargeLevel;
                }

                if (finishUpgradeItem[upgradeItem].type == "Radio Battery")
                {
                    slot.itemLevel.text = finishUpgradeItem[upgradeItem].type + " level : " + (finishUpgradeItem[upgradeItem].item.item.radioCurrentBattery + 1) + " / " + ((ScriptableRadio)finishUpgradeItem[upgradeItem].item.item.data).maxCurrentBattery;
                }
                if (finishUpgradeItem[upgradeItem].type == "batteria radio")
                {
                    slot.itemLevel.text = "Livello " + finishUpgradeItem[upgradeItem].type + " : " + (finishUpgradeItem[upgradeItem].item.item.radioCurrentBattery + 1) + " / " + ((ScriptableRadio)finishUpgradeItem[upgradeItem].item.item.data).maxCurrentBattery;
                }

                if (finishUpgradeItem[upgradeItem].type == "Torch Battery")
                {
                    slot.itemLevel.text = finishUpgradeItem[upgradeItem].type + " level : " + (finishUpgradeItem[upgradeItem].item.item.radioCurrentBattery + 1) + " / " + ((ScriptableTorch)finishUpgradeItem[upgradeItem].item.item.data).maxCurrentBattery;
                }
                if (finishUpgradeItem[upgradeItem].type == "batteria torcia")
                {
                    slot.itemLevel.text = "Livello " + finishUpgradeItem[upgradeItem].type + " : " + (finishUpgradeItem[upgradeItem].item.item.radioCurrentBattery + 1) + " / " + ((ScriptableTorch)finishUpgradeItem[upgradeItem].item.item.data).maxCurrentBattery;
                }

                if (finishUpgradeItem[upgradeItem].type == "Bag Slot")
                {
                    slot.itemLevel.text = finishUpgradeItem[upgradeItem].type + " level : " + (finishUpgradeItem[upgradeItem].item.item.bagLevel + 1) + " / " + ((EquipmentItem)finishUpgradeItem[upgradeItem].item.item.data).maxBagLevel;
                }
                if (finishUpgradeItem[upgradeItem].type == "slot borsa")
                {
                    slot.itemLevel.text = "Livello " + finishUpgradeItem[upgradeItem].type + " : " + (finishUpgradeItem[upgradeItem].item.item.bagLevel + 1) + " / " + ((EquipmentItem)finishUpgradeItem[upgradeItem].item.item.data).maxBagLevel;
                }

                if (finishUpgradeItem[upgradeItem].type == "Bag Protected Slot")
                {
                    slot.itemLevel.text = finishUpgradeItem[upgradeItem].type + " level : " + (finishUpgradeItem[upgradeItem].item.item.bagLevel + 1) + " / " + ((EquipmentItem)finishUpgradeItem[upgradeItem].item.item.data).maxBagLevel;
                }
                if (finishUpgradeItem[upgradeItem].type == "slot protetti borsa")
                {
                    slot.itemLevel.text = "Livello " + finishUpgradeItem[upgradeItem].type + " : " + (finishUpgradeItem[upgradeItem].item.item.bagLevel + 1) + " / " + ((EquipmentItem)finishUpgradeItem[upgradeItem].item.item.data).maxBagLevel;
                }

                if (finishUpgradeItem[upgradeItem].type == "Durability")
                {
                    slot.itemLevel.text = finishUpgradeItem[upgradeItem].type + " level : " + (finishUpgradeItem[upgradeItem].item.item.durabilityLevel + 1) + " / " + ((ScriptableItem)finishUpgradeItem[upgradeItem].item.item.data).maxDurabilityLevel;
                }
                if (finishUpgradeItem[upgradeItem].type == "durabilita'")
                {
                    slot.itemLevel.text = "Livello " + finishUpgradeItem[upgradeItem].type + " : " + (finishUpgradeItem[upgradeItem].item.item.durabilityLevel + 1) + " / " + ((ScriptableItem)finishUpgradeItem[upgradeItem].item.item.data).maxDurabilityLevel;
                }

                if (finishUpgradeItem[upgradeItem].type == "Unsanity")
                {
                    slot.itemLevel.text = finishUpgradeItem[upgradeItem].type + " level : " + (finishUpgradeItem[upgradeItem].item.item.unsanityLevel + 1) + " / " + ((FoodItem)finishUpgradeItem[upgradeItem].item.item.data).maxUnsanityLevel;
                }
                if (finishUpgradeItem[upgradeItem].type == "sanita' oggetto")
                {
                    slot.itemLevel.text = "Livello " + finishUpgradeItem[upgradeItem].type + " : " + (finishUpgradeItem[upgradeItem].item.item.unsanityLevel + 1) + " / " + ((FoodItem)finishUpgradeItem[upgradeItem].item.item.data).maxUnsanityLevel;
                }

                if (finishUpgradeItem[upgradeItem].type == "Item Weight")
                {
                    slot.itemLevel.text = finishUpgradeItem[upgradeItem].type + " level : " + (finishUpgradeItem[upgradeItem].item.item.weightLevel + 1) + " / " + ((ScriptableItem)finishUpgradeItem[upgradeItem].item.item.data).maxWeightLevel;
                }
                if (finishUpgradeItem[upgradeItem].type == "peso oggetto")
                {
                    slot.itemLevel.text = "Livello " + finishUpgradeItem[upgradeItem].type + " : " + (finishUpgradeItem[upgradeItem].item.item.weightLevel + 1) + " / " + ((FoodItem)finishUpgradeItem[upgradeItem].item.item.data).maxWeightLevel;
                }

                slot.itemButton.onClick.SetListener(() =>
                {
                    if (Player.localPlayer.target && Player.localPlayer.target.GetComponent<BuildingUpgradeRepair>())
                    {
                        Player.localPlayer.CmdClaimUpgradeItem(finishUpgradeItem[upgradeItem].index, finishUpgradeItem[upgradeItem].item.item.name, finishUpgradeItem[upgradeItem].item.amount, finishUpgradeItem[upgradeItem].playerName, finishUpgradeItem[upgradeItem].timeEnd);
                    }
                });

                TimeSpan difference = DateTime.Parse(finishUpgradeItem[upgradeItem].timeEnd) - DateTime.Now;
                if (Convert.ToInt32(difference.TotalSeconds) < 0 && finishUpgradeItem[upgradeItem].playerName == player.name)
                {
                    slot.itemButton.GetComponent<Image>().enabled = true;
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        slot.itemButton.GetComponentInChildren<TextMeshProUGUI>().text = "Prendi";
                    }
                    else
                    {
                        slot.itemButton.GetComponentInChildren<TextMeshProUGUI>().text = "Claim";
                    }
                }
                else
                {
                    slot.itemButton.GetComponent<Image>().enabled = false;
                    slot.itemButton.GetComponentInChildren<LanguageTranslator>().enabled = true;
                    slot.itemButton.GetComponentInChildren<TextMeshProUGUI>().text = GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds));
                }

            }

            UIUtils.BalancePrefabs(upgradeslot, finishRepairItem.Count, repairContent);
            for (int i = 0; i < finishRepairItem.Count; i++)
            {
                int upgradeItem = i;
                UIUpgradeSlot slot = repairContent.GetChild(upgradeItem).GetComponent<UIUpgradeSlot>();
                if (finishRepairItem[upgradeItem].item.item.data is ScriptableBuilding)
                {
                    if (ScriptableBuilding.dict.TryGetValue(((ScriptableBuilding)finishRepairItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableBuilding tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (finishRepairItem[upgradeItem].item.item.data is ScriptableItem)
                {
                    if (ScriptableItem.dict.TryGetValue(((ScriptableItem)finishRepairItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableItem tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (finishRepairItem[upgradeItem].item.item.data is FoodItem)
                {
                    if (ScriptableItem.dict.TryGetValue(((ScriptableItem)finishRepairItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableItem tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (finishRepairItem[upgradeItem].item.item.data is EquipmentItem)
                {
                    if (ScriptableItem.dict.TryGetValue(((ScriptableItem)finishRepairItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableItem tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (finishRepairItem[upgradeItem].item.item.data is ScriptablePlant)
                {
                    if (ScriptablePlant.dict.TryGetValue(((ScriptablePlant)finishRepairItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptablePlant tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (finishRepairItem[upgradeItem].item.item.data is ScriptableRadio)
                {
                    if (ScriptableRadio.dict.TryGetValue(((ScriptableRadio)finishRepairItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableRadio tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (finishRepairItem[upgradeItem].item.item.data is ScriptableTorch)
                {
                    if (ScriptableTorch.dict.TryGetValue(((ScriptableTorch)finishRepairItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableTorch tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (finishRepairItem[upgradeItem].item.item.data is TeleportItem)
                {
                    if (ScriptableItem.dict.TryGetValue(((ScriptableItem)finishRepairItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableItem tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (finishRepairItem[upgradeItem].item.item.data is TeleportItem)
                {
                    if (ScriptableItem.dict.TryGetValue(((ScriptableItem)finishRepairItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableItem tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (finishRepairItem[upgradeItem].item.item.data is ScriptableWood)
                {
                    if (ScriptableItem.dict.TryGetValue(((ScriptableItem)finishRepairItem[upgradeItem].item.item.data).name.GetStableHashCode(), out ScriptableItem tempObj))
                    {
                        slot.image.sprite = tempObj.image;
                        slot.itemText.text = tempObj.name;
                    }
                }
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    slot.itemLevel.text = "Durabilita' : " + finishRepairItem[upgradeItem].item.item.durability +
                                            " / " + ((ScriptableItem)finishRepairItem[upgradeItem].item.item.data).maxDurability.Get(finishRepairItem[upgradeItem].item.item.durabilityLevel);
                }
                else
                {
                    slot.itemLevel.text = "Durability : " + finishRepairItem[upgradeItem].item.item.durability +
                        " / " + ((ScriptableItem)finishRepairItem[upgradeItem].item.item.data).maxDurability.Get(finishRepairItem[upgradeItem].item.item.durabilityLevel);

                }

                slot.itemButton.onClick.SetListener(() =>
                {
                    if (Player.localPlayer.target && Player.localPlayer.target.GetComponent<BuildingUpgradeRepair>())
                    {
                        Player.localPlayer.CmdClaimRepairItem(finishRepairItem[upgradeItem].index, finishRepairItem[upgradeItem].item.item.name, finishRepairItem[upgradeItem].item.amount, finishRepairItem[upgradeItem].playerName, finishRepairItem[upgradeItem].timeEnd);
                    }
                });

                TimeSpan difference = DateTime.Parse(finishRepairItem[upgradeItem].timeEnd) - System.DateTime.Now;
                if (Convert.ToInt32(difference.TotalSeconds) < 0 && finishRepairItem[upgradeItem].playerName == player.name)
                {
                    slot.itemButton.GetComponent<Image>().enabled = true;
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        slot.itemButton.GetComponentInChildren<TextMeshProUGUI>().text = "Prendi";
                    }
                    else
                    {
                        slot.itemButton.GetComponentInChildren<TextMeshProUGUI>().text = "Claim";
                    }
                }
                else
                {
                    slot.itemButton.GetComponent<Image>().enabled = false;
                    slot.itemButton.GetComponentInChildren<LanguageTranslator>().enabled = true;
                    slot.itemButton.GetComponentInChildren<TextMeshProUGUI>().text = GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds));
                }
            }

        }
    }
}

public partial class UIHoneyKeeper : MonoBehaviour
{
    public static UIHoneyKeeper singleton;
    public GameObject buttonContainer;
    public TextMeshProUGUI descriptionText;
    public Slider honeySlider;
    public Button takeMoneyButton;
    private Player player;

    public BeeKeeper beeKeeper;
    public Building building;
    //public Npc mainBuilding;

    public int selectedContainer = -1;

    public TextMeshProUGUI txtMinValue;
    public TextMeshProUGUI txtMaxValue;

    public Button closeButton;

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

        if (!beeKeeper || !building)
        {
            if (player.target && player.target is Building)
            {
                beeKeeper = player.target.GetComponent<BeeKeeper>();
                building = player.target.GetComponent<Building>();
                //mainBuilding = building.GetComponent<Npc>();
            }
        }
        if (!beeKeeper || !building) return;

        for (int i = 0; i < 5; i++)
        {
            int index = i;
            if (index < Convert.ToInt32(AvaiableContainer()))
            {
                buttonContainer.transform.GetChild(index).GetComponent<Button>().interactable = true;
                buttonContainer.transform.GetChild(index).GetComponent<Button>().onClick.SetListener(() =>
                {
                    selectedContainer = index;
                });
            }
            else
            {
                buttonContainer.transform.GetChild(index).GetComponent<Button>().interactable = false;
                buttonContainer.transform.GetChild(index).GetComponent<Button>().onClick.SetListener(() =>
                {
                    selectedContainer = -1;
                });
            }
        }

        if (selectedContainer > -1)
        {
            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                descriptionText.text = "Api totali :                     " + beeKeeper.beeContainers[selectedContainer].totalBee + "\n";
                descriptionText.text += "Miele totale :                  " + beeKeeper.beeContainers[selectedContainer].totalHoney + "\n\n\n\n\n\n\n";
                descriptionText.text += "\t\t\t\t\t\t Puoi prendere un massimo di:       " + player.GetEmptyHoneyBootle() + "\n";
                descriptionText.text += "\t\t\t\t\t\t Hai selezionato :                     " + Convert.ToInt32(honeySlider.value).ToString() + "\n";
            }
            else
            {
                descriptionText.text = "Total Bee :                     " + beeKeeper.beeContainers[selectedContainer].totalBee + "\n";
                descriptionText.text += "Total Honey :                  " + beeKeeper.beeContainers[selectedContainer].totalHoney + "\n\n\n\n\n\n\n";
                descriptionText.text += "\t\t\t\t\t\t You can take a maximum of :       " + player.GetEmptyHoneyBootle() + "\n";
                descriptionText.text += "\t\t\t\t\t\t You have selected :                     " + Convert.ToInt32(honeySlider.value).ToString() + "\n";

            }
            honeySlider.minValue = 0;
            honeySlider.maxValue = beeKeeper.beeContainers[selectedContainer].totalHoney;
            txtMinValue.text = "0";
            txtMaxValue.text = beeKeeper.beeContainers[selectedContainer].totalHoney.ToString();


            if (honeySlider.value > player.GetEmptyHoneyBootle())
            {
                takeMoneyButton.interactable = false;
            }
            else
                takeMoneyButton.interactable = true;

            takeMoneyButton.onClick.SetListener(() =>
            {
                player.CmdTakeHoney(Convert.ToInt32(honeySlider.value), selectedContainer);
            });
        }
    }

    public float AvaiableContainer()
    {
        float avaiableContainer = 0;
        avaiableContainer = building.level / 10;
        if (avaiableContainer == 0) avaiableContainer = 1;
        return avaiableContainer;
    }

}

public partial class UIWaterWell : MonoBehaviour
{
    public Slider waterSlider;
    public TextMeshProUGUI description;
    public TextMeshProUGUI selected;
    public TextMeshProUGUI minWater;
    public TextMeshProUGUI maxWater;
    public Button buttonWater;

    public Button closeButton;

    public BuildingWaterWell waterWell;
    public Entity building;

    private Player player;

    void Update()
    {
        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

        buttonWater.onClick.SetListener(() =>
        {
            player.CmdTakeWater(Convert.ToInt32(waterSlider.value));
        });


        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.health == 0)
            closeButton.onClick.Invoke();

        if (!waterWell) waterWell = player.target.GetComponent<BuildingWaterWell>();
        if (!building) building = player.target.GetComponent<Entity>();
        if (!waterWell) return;
        if (!building) return;

        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
        {
            description.text = "Condizione meteo : " + WeatherStringIta() + "\n";
            description.text += "Acqua attuale : " + waterWell.currentWater + " / " + waterWell.maxWater;
            description.text += "\nPuoi prendere un massimo di : " + player.GetEmptyWaterBootle();
            selected.text = "Acqua selezionata : " + Convert.ToInt32(waterSlider.value);
            minWater.text = "0";
        }
        else
        {
            description.text = "Water conditions : " + WeatherString() + "\n";
            description.text += "Actual water : " + waterWell.currentWater + " / " + waterWell.maxWater;
            description.text += "\nYou can withdraw a maximum of : " + player.GetEmptyWaterBootle();
            selected.text = "Selected water : " + Convert.ToInt32(waterSlider.value);
            minWater.text = "0";
        }
        maxWater.text = waterWell.currentWater + " / " + GeneralManager.singleton.levelWater.Get(player.target.GetComponent<Entity>().level).ToString();
        waterSlider.minValue = 0;
        waterSlider.maxValue = GeneralManager.singleton.levelWater.Get(player.target.GetComponent<Entity>().level);

        buttonWater.interactable = waterSlider.value <= player.GetEmptyWaterBootle() && waterSlider.value <= waterWell.currentWater;
    }

    public string WeatherString()
    {
        if (TemperatureManager.singleton.isRainy) return "Rainy";
        if (TemperatureManager.singleton.isSnowy) return "Snowy";
        if (TemperatureManager.singleton.isSunny) return "Sunny";

        return string.Empty;
    }

    public string WeatherStringIta()
    {
        if (TemperatureManager.singleton.isRainy) return "Piovoso";
        if (TemperatureManager.singleton.isSnowy) return "Nevoso";
        if (TemperatureManager.singleton.isSunny) return "Soleggiato";

        return string.Empty;
    }

    public float waterPercent()
    {
        return (waterWell.currentWater != 0 && waterWell.maxWater != 0) ? (float)waterWell.currentWater / (float)waterWell.maxWater : 0;
    }
}

public partial class UIListAnimal
{
    public AnimalCategory animalCategory;
    public Transform content;
    public GameObject gameobjectToSpawn;
    public Button closeButton;
    public TextMeshProUGUI titleText;
    private Player player;
    private Breeding breeding;

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

        if (!breeding) breeding = player.target.GetComponent<Breeding>();
        if (!breeding) return;

        if (animalCategory == AnimalCategory.chicken)
        {
            UIUtils.BalancePrefabs(gameobjectToSpawn, breeding.chicken.Count, content);
            for (int i = 0; i < breeding.chicken.Count; i++)
            {
                int index = i;
                AnimalListSlot slot = content.GetChild(index).GetComponent<AnimalListSlot>();
                if (ScriptableAnimal.dict.TryGetValue(breeding.chicken[index].name.GetStableHashCode(), out ScriptableAnimal tempAnimal))
                {
                    slot.animalImage.sprite = tempAnimal.image;
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        slot.agePregnatText.text = "Eta' : " + breeding.chicken[index].age + " / " + breeding.chicken[index].maxAge;
                        slot.nameSexText.text = "Nome : " + tempAnimal.italianName;
                    }
                    else
                    {
                        slot.agePregnatText.text = "Age : " + breeding.chicken[index].age + " / " + breeding.chicken[index].maxAge;
                        slot.nameSexText.text = "Name : " + breeding.chicken[index].name;
                    }
                }
                slot.killButton.onClick.SetListener(() =>
                {
                    player.CmdKillAnimal(index, animalCategory.ToString());
                    KillAnimalUI(index, animalCategory);
                });
            }
        }
        if (animalCategory == AnimalCategory.cow)
        {
            UIUtils.BalancePrefabs(gameobjectToSpawn, breeding.cow.Count, content);
            for (int i = 0; i < breeding.cow.Count; i++)
            {
                int index = i;
                AnimalListSlot slot = content.GetChild(index).GetComponent<AnimalListSlot>();

                if (ScriptableAnimal.dict.TryGetValue(breeding.cow[index].name.GetStableHashCode(), out ScriptableAnimal tempAnimal))
                {
                    slot.animalImage.sprite = tempAnimal.image;
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        slot.agePregnatText.text = "Eta' : " + breeding.cow[index].age + " / " + breeding.cow[index].maxAge;
                        slot.nameSexText.text = "Nome : " + tempAnimal.italianName;
                    }
                    else
                    {
                        slot.agePregnatText.text = "Age : " + breeding.cow[index].age + " / " + breeding.cow[index].maxAge;
                        slot.nameSexText.text = "Name : " + breeding.cow[index].name;

                    }
                }
                slot.killButton.onClick.SetListener(() =>
                {
                    player.CmdKillAnimal(index, animalCategory.ToString());
                    KillAnimalUI(index, animalCategory);

                });
            }
        }
        if (animalCategory == AnimalCategory.horse)
        {
            UIUtils.BalancePrefabs(gameobjectToSpawn, breeding.horse.Count, content);
            for (int i = 0; i < breeding.horse.Count; i++)
            {
                int index = i;
                AnimalListSlot slot = content.GetChild(index).GetComponent<AnimalListSlot>();

                if (ScriptableAnimal.dict.TryGetValue(breeding.horse[index].name.GetStableHashCode(), out ScriptableAnimal tempAnimal))
                {
                    slot.animalImage.sprite = tempAnimal.image;
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        slot.agePregnatText.text = "Eta' : " + breeding.horse[index].age + " / " + breeding.horse[index].maxAge;
                        slot.nameSexText.text = "Nome : " + tempAnimal.italianName;
                    }
                    else
                    {
                        slot.agePregnatText.text = "Age : " + breeding.horse[index].age + " / " + breeding.horse[index].maxAge;
                        slot.nameSexText.text = "Name : " + breeding.horse[index].name;

                    }
                }
                slot.killButton.onClick.SetListener(() =>
                {
                    player.CmdKillAnimal(index, animalCategory.ToString());
                    KillAnimalUI(index, animalCategory);
                });
            }
        }
        if (animalCategory == AnimalCategory.goat)
        {
            UIUtils.BalancePrefabs(gameobjectToSpawn, breeding.goat.Count, content);
            for (int i = 0; i < breeding.goat.Count; i++)
            {
                int index = i;
                AnimalListSlot slot = content.GetChild(index).GetComponent<AnimalListSlot>();

                if (ScriptableAnimal.dict.TryGetValue(breeding.goat[index].name.GetStableHashCode(), out ScriptableAnimal tempAnimal))
                {
                    slot.animalImage.sprite = tempAnimal.image;
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        slot.agePregnatText.text = "Eta' : " + breeding.goat[index].age + " / " + breeding.goat[index].maxAge;
                        slot.nameSexText.text = "Nome : " + tempAnimal.italianName;
                    }
                    else
                    {
                        slot.agePregnatText.text = "Age : " + breeding.goat[index].age + " / " + breeding.goat[index].maxAge;
                        slot.nameSexText.text = "Name : " + breeding.goat[index].name;

                    }
                }
                slot.killButton.onClick.SetListener(() =>
                {
                    player.CmdKillAnimal(index, animalCategory.ToString());
                    KillAnimalUI(index, animalCategory);
                });
            }
        }
        if (animalCategory == AnimalCategory.sheep)
        {
            UIUtils.BalancePrefabs(gameobjectToSpawn, breeding.sheep.Count, content);
            for (int i = 0; i < breeding.sheep.Count; i++)
            {
                int index = i;
                AnimalListSlot slot = content.GetChild(index).GetComponent<AnimalListSlot>();

                if (ScriptableAnimal.dict.TryGetValue(breeding.sheep[index].name.GetStableHashCode(), out ScriptableAnimal tempAnimal))
                {
                    slot.animalImage.sprite = tempAnimal.image;
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        slot.agePregnatText.text = "Eta' : " + breeding.sheep[index].age + " / " + breeding.sheep[index].maxAge;
                        slot.nameSexText.text = "Nome : " + tempAnimal.italianName;
                    }
                    else
                    {
                        slot.agePregnatText.text = "Age : " + breeding.sheep[index].age + " / " + breeding.sheep[index].maxAge;
                        slot.nameSexText.text = "Name : " + breeding.sheep[index].name;

                    }
                }
                slot.killButton.onClick.SetListener(() =>
                {
                    player.CmdKillAnimal(index, animalCategory.ToString());
                    KillAnimalUI(index, animalCategory);
                });
            }
        }
    }

    public void KillAnimalUI(int index, AnimalCategory animalCategory)
    {
        if (animalCategory == AnimalCategory.chicken)
        {
            Destroy(UIBreedingPanel.singleton.chickenContent.GetChild(index).gameObject);
        }
        if (animalCategory == AnimalCategory.cow)
        {
            Destroy(UIBreedingPanel.singleton.cowContent.GetChild(index).gameObject);
        }
        if (animalCategory == AnimalCategory.horse)
        {
            Destroy(UIBreedingPanel.singleton.horseContent.GetChild(index).gameObject);
        }
        if (animalCategory == AnimalCategory.goat)
        {
            Destroy(UIBreedingPanel.singleton.pigContent.GetChild(index).gameObject);
        }
        if (animalCategory == AnimalCategory.sheep)
        {
            Destroy(UIBreedingPanel.singleton.sheepContent.GetChild(index).gameObject);
        }
    }
}

public partial class UITargetPanel : MonoBehaviour
{
    private Player player;

    public GameObject playerTarget;

    public Button Trade;
    public Button Marriage;
    public Button Party;
    public Button Guild;
    public Button Ally;
    public Button Friends;

    public GameObject playerHealthPanel;
    public Image playerArmor;
    public Image playerHealth;

    public GameObject buildingHealthPanel;
    public Image buildingHealth;
    public Button UpgradeBuilding;
    public Button RenameBuilding;
    public Button RepairBuilding;
    public Button ClaimBuilding;
    public Button CutBuilding;

    public GameObject monsterHealthPanel;
    public Image monsterHealth;

    public GameObject resourceHealthPanel;
    public Image resourceHealth;

    public GameObject petHealthPanel;
    public Image petHealth;
    public Button petAttack;
    public Button petDefense;
    public Button petClose;

    public GameObject targetNameObject;
    public Text targetName;

    public Entity target;

    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        UpgradeBuilding.onClick.SetListener(() =>
        {
            GameObject g = Instantiate(GeneralManager.singleton.buildingManagerPanel, GeneralManager.singleton.canvas);
            g.GetComponent<UIBuildingManager>().titleText.text = "Upgrade";
            g.GetComponent<UIBuildingManager>().UpgradePanel.SetActive(true);
        });

        RenameBuilding.onClick.SetListener(() =>
        {
            GameObject g = Instantiate(GeneralManager.singleton.buildingManagerPanel, GeneralManager.singleton.canvas);
            g.GetComponent<UIBuildingManager>().titleText.text = "Rename";
            g.GetComponent<UIBuildingManager>().RenamePanel.SetActive(true);
        });

        RepairBuilding.onClick.SetListener(() =>
        {
            GameObject g = Instantiate(GeneralManager.singleton.buildingManagerPanel, GeneralManager.singleton.canvas);
            g.GetComponent<UIBuildingManager>().titleText.text = "Repair";
            g.GetComponent<UIBuildingManager>().RepairPanel.SetActive(true);
        });

        ClaimBuilding.onClick.SetListener(() =>
        {
            GameObject g = Instantiate(GeneralManager.singleton.buildingManagerPanel, GeneralManager.singleton.canvas);
            g.GetComponent<UIBuildingManager>().titleText.text = "Claim";
            g.GetComponent<UIBuildingManager>().ClaimPanel.SetActive(true);
        });

        CutBuilding.onClick.SetListener(() =>
        {
            GameObject g = Instantiate(GeneralManager.singleton.buildingManagerPanel, GeneralManager.singleton.canvas);
            g.GetComponent<UIBuildingManager>().titleText.text = "Cut Wait Time";
            g.GetComponent<UIBuildingManager>().CutPanel.SetActive(true);
        });

        petAttack.onClick.SetListener(() =>
        {
            player.activePet.GetComponent<NetworkIdentity>().AssignClientAuthority(player.GetComponent<NetworkIdentity>().connectionToClient);
            player.activePet.CmdSetAutoAttack();
        });
        petDefense.onClick.SetListener(() =>
        {
            player.activePet.GetComponent<NetworkIdentity>().AssignClientAuthority(player.GetComponent<NetworkIdentity>().connectionToClient);
            player.activePet.CmdSetDefendOwner();
        });
        petClose.onClick.SetListener(() =>
        {
            if (player.CanUnsummonPet()) player.CmdPetUnsummon();
        });


        target = player.target;

        playerTarget.SetActive(target && (target is Player));
        resourceHealthPanel.SetActive(target && (target is Rock || target is Tree));
        playerHealthPanel.SetActive(target && target is Player);
        buildingHealthPanel.SetActive(target && target is Building);

        if (target)
        {
            if (target is Building && ((Building)target).isPremiumZone)
            {
                targetNameObject.SetActive(false);
            }
            else if (target is Rock)
            {
                targetNameObject.SetActive(true);
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    targetName.text = "Roccia";
                }
                else
                {
                    targetName.text = "Rock";
                }
            }
            else if (target is Tree)
            {
                targetNameObject.SetActive(true);
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    targetName.text = "Albero";
                }
                else
                {
                    targetName.text = "Tree";
                }
            }
            else if (target is Monster)
            {
                targetNameObject.SetActive(true);
                targetName.text = "Zombie";
            }
            else if (target is Building)
            {
                targetNameObject.SetActive(true);
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    targetName.text = ((Building)player.target).building.italianName;
                }
                else
                {
                    targetName.text = ((Building)player.target).building.name;
                }
            }
            else if (target is Player)
            {
                targetNameObject.SetActive(true);
                targetName.text = ((Player)player.target).name;
            }
        }
        else
        {
            targetNameObject.SetActive(false);
        }

        if (target is Rock)
        {
            resourceHealth.fillAmount = ((Rock)player.target).HealthPercent();
        }

        if (target is Tree)
        {
            resourceHealth.fillAmount = ((Tree)player.target).HealthPercent();
        }


        if (target is Player)
        {
            float distance = Utils.ClosestDistance(player.collider, target.collider);

            Trade.interactable = target && player.CanStartTradeWith(target);

            Marriage.interactable = target && ((Player)target).playerMarriage.partnerName == "";

            Party.interactable = target && (!player.InParty() || !player.party.IsFull()) &&
                                 !((Player)target).InParty() &&
                                 NetworkTime.time >= player.nextRiskyActionTime &&
                                 distance <= player.interactionRange;

            Guild.interactable = target && !((Player)target).InGuild() &&
                                 player.guild.CanInvite(player.name, target.name) &&
                                 NetworkTime.time >= player.nextRiskyActionTime &&
                                 distance <= player.interactionRange;

            Ally.interactable = target && ((Player)target).InGuild() &&
                                 player.playerAlliance.CanInviteToAlliance() &&
                                 NetworkTime.time >= player.nextRiskyActionTime;

            Friends.interactable = target && NetworkTime.time >= player.nextRiskyActionTime && !((Player)target).playerFriend.playerFriends.Contains(player.name) && !((Player)target).playerFriend.playerRequest.Contains(player.name);

            Trade.onClick.SetListener(() =>
            {
                player.CmdTradeRequestSend();
            });

            Marriage.onClick.SetListener(() =>
            {
                player.playerMarriage.CmdInvitePartner();
            });

            Party.onClick.SetListener(() =>
            {
                player.CmdPartyInvite(player.target.name);
            });

            Guild.onClick.SetListener(() =>
            {
                player.playerAlliance.CmdGuildInviteTarget();
            });

            Ally.onClick.SetListener(() =>
            {
                player.playerAlliance.CmdInviteToAlliance();
            });

            Friends.onClick.SetListener(() =>
            {
                player.playerFriend.CmdSendFriendRequest();
            });

            playerArmor.fillAmount = ((Player)target).playerArmor.ArmorPercent();
            playerHealth.fillAmount = ((Player)target).HealthPercent();
        }


        buildingHealthPanel.SetActive(target && target is Building);
        if (buildingHealthPanel.activeInHierarchy)
        {
            UpgradeBuilding.interactable = target && ((Building)target).level < 50 &&
                                           GeneralManager.singleton.CanDoOtherActionBuilding(((Building)target), player) == true &&
                                           (((Building)target).building.coinToUpgrade > 0 ||
                                           ((Building)target).building.goldToUpgrade > 0) &&
                                           (((Building)target).owner != string.Empty || ((Building)target).guild != string.Empty);

            RenameBuilding.interactable = target &&
                                          GeneralManager.singleton.CanDoOtherActionBuilding(((Building)target), player) &&
                                          (((Building)target).owner != string.Empty || ((Building)target).guild != string.Empty);

            RepairBuilding.interactable = target && target.health < target.healthMax &&
                                          GeneralManager.singleton.CanDoOtherActionBuilding(((Building)target), player) == true &&
                                          (((Building)target).building.coinToRepair > 0 ||
                                          ((Building)target).building.goldToRepair > 0) &&
                                          (((Building)target).owner != string.Empty || ((Building)target).guild != string.Empty);

            ClaimBuilding.interactable = target && ((Building)target).level < 50 &&
                                         GeneralManager.singleton.CanClaimBuilding(((Building)target), player) == true &&
                                         (((Building)target).building.coinToClaim > 0 ||
                                         ((Building)target).building.goldToClaim > 0) &&
                                         (((Building)target).owner != string.Empty || ((Building)target).guild != string.Empty);

            CutBuilding.interactable = target && ((Building)target).level <= 50 &&
                                       GeneralManager.singleton.CanDoOtherActionBuilding(((Building)target), player) &&
                                       (((Building)target).building.coinToHalve > 0 ||
                                       ((Building)target).building.goldToHalve > 0) &&
                                       (((Building)target).owner != string.Empty || ((Building)target).guild != string.Empty);

            buildingHealth.fillAmount = target.HealthPercent();
        }

        monsterHealthPanel.SetActive(target && target is Monster);
        if (monsterHealthPanel.activeInHierarchy)
        {
            monsterHealth.fillAmount = target.HealthPercent();
        }

        petHealthPanel.SetActive(target && target is Pet && player.activePet && target == player.activePet);
        if (petHealthPanel.activeInHierarchy)
        {
            petHealth.fillAmount = player.activePet.HealthPercent();
        }
    }
}

public partial class UIBuildingManager
{
    public GameObject gameobjectToSpawn;
    public Text titleText;

    public static UIBuildingManager singleton;

    [Header("Main category Stuff")]
    public GameObject UpgradePanel;
    public GameObject RepairPanel;
    public GameObject ClaimPanel;
    public GameObject RenamePanel;
    public GameObject CutPanel;

    [Header("Upgrade Stuff")]
    public TextMeshProUGUI upgradeText;
    public Transform upgradeContent;
    public Button UpgradeButton;

    [Header("Repair Stuff")]
    public TextMeshProUGUI repairText;
    public Transform reapirContent;
    public Button repairButton;

    [Header("Claim Stuff")]
    public TextMeshProUGUI claimText;
    public Transform claimContent;
    public Button claimButton;

    [Header("Rename Stuff")]
    public TextMeshProUGUI renameText;
    public InputField renameTextbox;
    public Button renameButton;

    public Button closeButton;

    private List<string> upgradeMessage = new List<string>();
    private List<string> repairMessage = new List<string>();
    private List<string> claimMessage = new List<string>();
    private List<string> cutMessage = new List<string>();

    private Player player;

    void Start()
    {
        if (!singleton) singleton = this;
    }

    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.health == 0)
            closeButton.onClick.Invoke();



        if (UpgradePanel.activeInHierarchy)
        {
            upgradeText.text = GeneralManager.singleton.UpgradeBuildingMessage(CreateUpgradeList(), "");
            UpgradeButton.interactable = player.target.GetComponent<Building>().level < 50 &&
                                         player.target.GetComponent<Building>().countdown == 0 &&
                                         player.CanInteractBuildingTarget(player.target.GetComponent<Building>(), player) == true &&
                                         GeneralManager.singleton.HasItemToUpgrade() &&
                                         player.playerAbility.networkAbilities[GeneralManager.singleton.FindNetworkAbility(player.target.GetComponent<Building>().building.abilityToUpgrade.name, player.name)].level >= player.target.level;
        }
        if (RepairPanel.activeInHierarchy)
        {
            repairText.text = GeneralManager.singleton.RepairBuildingMessage(CreateRepairList(), "");
            repairButton.interactable = player.target.health < player.target.healthMax &&
                                        player.target.GetComponent<Building>().countdown == 0 &&
                                        player.CanInteractBuildingTarget(player.target.GetComponent<Building>(), player) == true &&
                                        GeneralManager.singleton.HasItemToRepair() &&
                                        player.playerAbility.networkAbilities[GeneralManager.singleton.FindNetworkAbility(player.target.GetComponent<Building>().building.abilityToRepair.name, player.name)].level >= player.target.level;
        }
        if (ClaimPanel.activeInHierarchy)
        {
            claimText.text = GeneralManager.singleton.ClaimBuildingMessage(CreateClaimList(), "");
            claimButton.interactable = player.target.GetComponent<Building>().countdown == 0 &&
                                       player.CanInteractBuildingTarget(player.target.GetComponent<Building>(), player) == false &&
                                       player.playerAbility.networkAbilities[GeneralManager.singleton.FindNetworkAbility(player.target.GetComponent<Building>().building.abilityToClaim.name, player.name)].level >= player.target.level;
        }
        if (RenamePanel.activeInHierarchy)
        {
            renameText.text = "Insert here the new name that you want to do at this building";
            renameButton.interactable = renameTextbox.text != string.Empty && renameTextbox.text != player.target.GetComponent<Building>().building.name && renameTextbox.text != player.target.GetComponent<Building>().buildingName;
        }

        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

        UpgradeButton.onClick.SetListener(() =>
        {
            player.CmdUpgradeBuilding();
        });

        repairButton.onClick.SetListener(() =>
        {
            player.CmdRepairBuilding();
        });

        claimButton.onClick.SetListener(() =>
        {
            player.CmdClaimBuilding();
        });

        renameButton.onClick.SetListener(() =>
        {
            player.CmdRenameBuilding(renameTextbox.text);
        });
    }


    public List<string> CreateUpgradeList()
    {
        upgradeMessage.Clear();
        upgradeMessage.Add(((Entity)player.target).level.ToString());
        if (ScriptableBuilding.dict.TryGetValue(((ScriptableBuilding)player.target.GetComponent<Building>().building).name.GetStableHashCode(), out ScriptableBuilding tempAnimal))
        {
            upgradeMessage.Add(tempAnimal.coinToUpgrade.ToString());
            upgradeMessage.Add((tempAnimal.goldToUpgrade * ((Entity)player.target).level).ToString());
            upgradeMessage.Add(tempAnimal.buildingExperience.ToString());
            upgradeMessage.Add(GeneralManager.singleton.buildingUpgradePoint.ToString());
            upgradeMessage.Add(tempAnimal.abilityToUpgrade.name);
        }

        UIUtils.BalancePrefabs(gameobjectToSpawn, ((ScriptableBuilding)player.target.GetComponent<Building>().building).itemToUpgrade.Count, upgradeContent);
        for (int i = 0; i < upgradeContent.childCount; i++)
        {
            int index = i;
            UpgradeBuildingSlot slot = upgradeContent.GetChild(index).GetComponent<UpgradeBuildingSlot>();
            slot.upgradeImage.sprite = ((ScriptableBuilding)player.target.GetComponent<Building>().building).itemToUpgrade[index].items.image;
            slot.upgradeName.text = ((ScriptableBuilding)player.target.GetComponent<Building>().building).itemToUpgrade[index].items.name;
            slot.upgradeAmount.text = " x " + ((ScriptableBuilding)player.target.GetComponent<Building>().building).itemToUpgrade[index].amount.ToString();
            if (player.InventoryCanAdd(new Item(((ScriptableBuilding)player.target.GetComponent<Building>().building).itemToUpgrade[index].items), ((ScriptableBuilding)player.target.GetComponent<Building>().building).itemToUpgrade[index].amount * player.target.GetComponent<Building>().level))
            {
                Color c = slot.GetComponent<Image>().color;
                c = Color.white;
                slot.GetComponent<Image>().color = c;
            }
            else
            {
                Color c = slot.GetComponent<Image>().color;
                c = Color.grey;
                slot.GetComponent<Image>().color = c;
            }
        }

        return upgradeMessage;
    }

    public List<string> CreateRepairList()
    {
        repairMessage.Clear();
        repairMessage.Add(((Entity)player.target).level.ToString());
        if (ScriptableBuilding.dict.TryGetValue(((ScriptableBuilding)player.target.GetComponent<Building>().building).name.GetStableHashCode(), out ScriptableBuilding tempAnimal))
        {
            repairMessage.Add(tempAnimal.coinToRepair.ToString());
            repairMessage.Add(tempAnimal.goldToRepair.ToString());
            repairMessage.Add(tempAnimal.buildingExperience.ToString());
            repairMessage.Add(GeneralManager.singleton.buildingClaimPoint.ToString());
            repairMessage.Add(tempAnimal.abilityToRepair.name);

            UIUtils.BalancePrefabs(gameobjectToSpawn, ((ScriptableBuilding)player.target.GetComponent<Building>().building).itemToRepair.Count, upgradeContent);
            for (int i = 0; i < upgradeContent.childCount; i++)
            {
                int index = i;
                UpgradeBuildingSlot slot = upgradeContent.GetChild(index).GetComponent<UpgradeBuildingSlot>();
                slot.upgradeImage.sprite = ((ScriptableBuilding)player.target.GetComponent<Building>().building).itemToRepair[index].items.image;
                slot.upgradeName.text = ((ScriptableBuilding)player.target.GetComponent<Building>().building).itemToRepair[index].items.name;
                slot.upgradeAmount.text = " x " + ((ScriptableBuilding)player.target.GetComponent<Building>().building).itemToRepair[index].amount.ToString();
                if (player.InventoryCanAdd(new Item(((ScriptableBuilding)player.target.GetComponent<Building>().building).itemToRepair[index].items), ((ScriptableBuilding)player.target.GetComponent<Building>().building).itemToRepair[index].amount * player.target.GetComponent<Building>().level))
                {
                    Color c = slot.GetComponent<Image>().color;
                    c = Color.white;
                    slot.GetComponent<Image>().color = c;
                }
                else
                {
                    Color c = slot.GetComponent<Image>().color;
                    c = Color.grey;
                    slot.GetComponent<Image>().color = c;
                }

            }
        }
        return repairMessage;
    }

    public List<string> CreateClaimList()
    {
        claimMessage.Clear();
        claimMessage.Add(((Entity)player.target).level.ToString());
        if (ScriptableBuilding.dict.TryGetValue(((ScriptableBuilding)player.target.GetComponent<Building>().building).name.GetStableHashCode(), out ScriptableBuilding tempAnimal))
        {
            claimMessage.Add(tempAnimal.coinToClaim.ToString());
            claimMessage.Add(tempAnimal.goldToClaim.ToString());
            claimMessage.Add(tempAnimal.buildingExperience.ToString());
            claimMessage.Add(GeneralManager.singleton.buildingClaimPoint.ToString());
            claimMessage.Add(tempAnimal.abilityToClaim.name);
        }
        return claimMessage;
    }
}