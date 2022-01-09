using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CustomType;

public class UIPetTradingProgress : MonoBehaviour
{
    public GameObject gameObjectToSpawn;
    public Transform inProgressContent;
    public Transform completedContent;

    //public Button switchButton;
    public Button closeButton;

    public bool personal;

    private Player player;
    private PetTrainer petTrainer;
    private TimeSpan difference;

    public List<PetExp> progressPet = new List<PetExp>();
    public List<PetExp> finishedPet = new List<PetExp>();

    void Start()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;
        if (!player.target) return;
        if (!player.target.GetComponent<PetTrainer>()) return;
        if (!petTrainer) petTrainer = player.target.GetComponent<PetTrainer>();
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

        if (!player.target) return;
        if (!petTrainer) return;


        progressPet.Clear();
        finishedPet.Clear();

        for (int i = 0; i < petTrainer.petTraining.Count; i++)
        {
            int index = i;
            difference = DateTime.Parse(petTrainer.petTraining[index].timeEnd) - DateTime.Now;

            PetExp petExp = petTrainer.petTraining[index];
            petExp.index = index;
            petExp.remainingTimer = Convert.ToInt32(difference.TotalSeconds);

            if (difference.TotalSeconds > 0)
            {
                progressPet.Add(petExp);
            }
            else
            {
                finishedPet.Add(petExp);
            }
        }


        UIUtils.BalancePrefabs(gameObjectToSpawn, progressPet.Count, inProgressContent);
        for (int i = 0; i < inProgressContent.childCount; i++)
        {
            int index = i;
            if (index >= progressPet.Count) continue;
            CraftProgressSlot slot = inProgressContent.GetChild(index).GetComponent<CraftProgressSlot>();

            Player insidePlayer;
            if (Player.onlinePlayers.TryGetValue(progressPet[index].owner, out insidePlayer))
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
                        slot.itemOwnerGuild.text = "Owner : " + progressPet[index].owner;
                    }

                }
                else
                {
                    Color c = slot.onlinePlayer.color;
                    c = GeneralManager.singleton.offlineColor;
                    slot.onlinePlayer.color = c;
                    slot.itemOwnerGuild.text = "Owner : " + progressPet[index].owner;
                }
            }

            if(GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                slot.itemName.text = progressPet[index].petItem.item.data.italianName;
            }
            else
            {
                slot.itemName.text = progressPet[index].petItem.item.name;
            }
            slot.itemImage.sprite = progressPet[index].petItem.item.data.image;

            slot.GetItem.image.enabled = false;
            slot.GetItem.GetComponentInChildren<LanguageTranslator>().enabled = false;
            slot.GetItem.GetComponentInChildren<TextMeshProUGUI>().text = GeneralManager.singleton.ConvertToTimer(progressPet[index].remainingTimer);

        }

        UIUtils.BalancePrefabs(gameObjectToSpawn, finishedPet.Count, completedContent);
        for (int i = 0; i < completedContent.childCount; i++)
        {
            int index = i;
            if (index >= finishedPet.Count) continue;
            CraftProgressSlot slot = completedContent.GetChild(index).GetComponent<CraftProgressSlot>();
            slot.craftProgressIndex = index;

            Debug.Log("Player : " + finishedPet[index].owner);
            Player insidePlayer;
            if (Player.onlinePlayers.TryGetValue(finishedPet[index].owner, out insidePlayer))
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
                        slot.itemOwnerGuild.text = "Owner : " + finishedPet[index].owner;
                    }

                }
                else
                {
                    Color c = slot.onlinePlayer.color;
                    c = GeneralManager.singleton.offlineColor;
                    slot.onlinePlayer.color = c;
                    slot.itemOwnerGuild.text = "Owner : " + finishedPet[index].owner;
                }
            }

            if (ScriptableItem.dict.TryGetValue(finishedPet[index].petItem.item.name.GetStableHashCode(), out ScriptableItem itemData))
            {
                slot.itemImage.sprite = itemData.image;
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    slot.itemName.text = itemData.italianName;
                }
                else
                {
                    slot.itemName.text = finishedPet[index].petItem.item.name;
                }
                slot.GetItem.interactable = (finishedPet[index].owner == player.name && player.InventoryCanAdd(new Item(itemData), 1));

            }

            slot.GetItem.GetComponentInChildren<LanguageTranslator>().enabled = true;
            slot.GetItem.image.enabled = true;

            slot.GetItem.onClick.SetListener(() =>
            {
                player.CmdAddToInventoryPetTrainer(finishedPet[index].petItem.item.name, 1, finishedPet[index].index, player.name);

            });
        }
    }
}
