using CustomType;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPetTrainer : MonoBehaviour
{
    public Transform petContent;
    public GameObject objectToSpawn;
    public TextMeshProUGUI minFood;
    public TextMeshProUGUI maxFood;
    public TextMeshProUGUI currentFood;

    public Button trainButton;
    public TextMeshProUGUI currentLevel;
    public TextMeshProUGUI maxLevel;
    public TextMeshProUGUI infoText;
    public Button petInTradingButton;
    public Button closeButton;
    public Slider foodSlider;
    public ItemSlot selectedPet;

    public List<int> petIndex = new List<int>();
    public int foodIndexAmount;
    private Player player;
    private PetTrainer petTrainer;

    public TimeSpan difference;

    public int selectedPetSlot;

    // Start is called before the first frame update
    void Awake()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

    }

    // Update is called once per frame
    void Update()
    {
        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

        if (!player) player = Player.localPlayer;
        if (!player) return;
        if(player.target)
        {
            if (petTrainer == null) petTrainer = player.target.GetComponent<PetTrainer>();
        }
        else
        {
            petTrainer = null;
        }
        if (player.health == 0)
            closeButton.onClick.Invoke();


        petInTradingButton.onClick.SetListener(() =>
        {
            Instantiate(GeneralManager.singleton.petTrainingInProgress, GeneralManager.singleton.canvas);
        });


        if (player.activePet)
        {
            foodIndexAmount = 0;
            for (int i = 0; i < player.inventory.Count; i++)
            {
                //FOOD
                if (player.inventory[i].amount > 0 && player.inventory[i].item.name == GeneralManager.singleton.foodToUpgradeLevelOfItem.name)
                {
                    foodIndexAmount += player.inventory[i].amount;
                }
            }
            minFood.text = "0";
            maxFood.text = foodIndexAmount.ToString();
            foodSlider.minValue = 0;
            foodSlider.maxValue = foodIndexAmount;
            if(GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                infoText.text = "Controlla che non ci siano pet attivi al momento";
            else
                infoText.text = "Make sure there are not friendly active pet at the moment";
        }
        else
        {
            foodIndexAmount = 0;
            for (int i = 0; i < player.inventory.Count; i++)
            {
                //PET IN INVENTORY
                if (player.inventory[i].amount > 0 && player.inventory[i].item.data is PetItem)
                {
                    if (!petIndex.Contains(i))
                    {
                        petIndex.Add(i);
                    }
                }
                else
                {
                    if (petIndex.Contains(i))
                    {
                        petIndex.Remove(i);
                    }
                }
                //FOOD
                if (player.inventory[i].amount > 0 && player.inventory[i].item.data is PotionItem && ((PotionItem)player.inventory[i].item.data).usagePetHealth > 0)
                {
                    foodIndexAmount += player.inventory[i].amount;
                }
            }
            minFood.text = "0";
            maxFood.text = foodIndexAmount.ToString();
            foodSlider.minValue = 0;
            foodSlider.maxValue = foodIndexAmount;
            infoText.text = string.Empty;
        }

        if (selectedPetSlot >= 0)
        {
            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                currentLevel.text = "Livello corrente del pet : " + player.inventory[petIndex[selectedPetSlot]].item.summonedLevel;
            }
            else
            {
                currentLevel.text = "Current pet level : " + player.inventory[petIndex[selectedPetSlot]].item.summonedLevel;
            }
            if (player.inventory[petIndex[selectedPetSlot]].item.data is PetItem)
            {
                if (player.inventory[petIndex[selectedPetSlot]].item.summonedLevel >= ((PetItem)player.inventory[petIndex[selectedPetSlot]].item.data).maxPetLevel)
                {
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        maxLevel.text = "Questo pet ha raggiunto il livello massimo!";
                    }
                    else
                    {
                        maxLevel.text = "This pet reach the max level!";
                    }
                }
                else
                {
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        maxLevel.text = "Questo pet otterra' " + GeneralManager.singleton.experienceForEachFoodItem * foodSlider.value + "xp\nTempo stimato : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(GeneralManager.singleton.timeToUpgradeOfOneLevel * foodSlider.value));
                    }
                    else
                    {
                        maxLevel.text = "This pet will be obtain " + GeneralManager.singleton.experienceForEachFoodItem * foodSlider.value + "xp\nEstimated time : " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(GeneralManager.singleton.timeToUpgradeOfOneLevel * foodSlider.value));
                    }
                }
            }
        }
        else
        {
            currentLevel.text = string.Empty;
        }
        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
        {
            currentFood.text = "Cibo selezionato : " + foodSlider.value;
        }
        else
        {
            currentFood.text = "Selected food : " + foodSlider.value;
        }
        
        trainButton.interactable = foodSlider.value > 0 && selectedPetSlot >= 0 && player.inventory[petIndex[selectedPetSlot]].item.data is PetItem && player.inventory[petIndex[selectedPetSlot]].item.summonedLevel < ((PetItem)player.inventory[petIndex[selectedPetSlot]].item.data).maxPetLevel;
        trainButton.onClick.SetListener(() =>
        {
            if (player.inventory[petIndex[selectedPetSlot]].item.data is PetItem && player.inventory[petIndex[selectedPetSlot]].item.summonedLevel < ((PetItem)player.inventory[petIndex[selectedPetSlot]].item.data).maxPetLevel)
            {
                DateTime time = DateTime.Now;
                PetExp petexp = new PetExp();
                ItemSlot item = player.inventory[petIndex[selectedPetSlot]];
                //item.item.summonedExperience += Convert.ToInt64(GeneralManager.singleton.experienceForEachFoodItem * foodSlider.value);
                //player.inventory[petIndex[selectedPetSlot]] = item;
                petexp.timer = Convert.ToInt32(GeneralManager.singleton.timeToUpgradeOfOneLevel * foodSlider.value);
                petexp.remainingTimer = petexp.timer;
                petexp.level = item.item.summonedLevel;
                petexp.petItem = player.inventory[petIndex[selectedPetSlot]];
                petexp.owner = player.name;
                petexp.experienceToAdd = Convert.ToInt64(GeneralManager.singleton.experienceForEachFoodItem * foodSlider.value);
                petexp.selectedFood = Convert.ToInt32(foodSlider.value);
                petexp.timeBegin = time.ToString();
                petexp.timeEnd = time.AddSeconds(GeneralManager.singleton.timeToUpgradeOfOneLevel * foodSlider.value).ToString();
                player.CmdAddPetToExpList(petexp, petIndex[selectedPetSlot], player.inventory[petIndex[selectedPetSlot]].item.name);
                selectedPetSlot = -1;
                maxLevel.text = string.Empty;
            }
        });



        UIUtils.BalancePrefabs(objectToSpawn, petIndex.Count, petContent);
        for (int i = 0; i < petIndex.Count; i++)
        {
            int index = i;
            UIPetSlot slot = petContent.GetChild(index).GetComponent<UIPetSlot>();
            slot.petImage.sprite = player.inventory[petIndex[index]].item.data.image;
            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                slot.nameLevelText.text = player.inventory[petIndex[index]].item.data.name + " \nLivello : " + player.inventory[petIndex[index]].item.summonedLevel;
            }
            else
            {
                slot.nameLevelText.text = player.inventory[petIndex[index]].item.data.name + " \nLevel : " + player.inventory[petIndex[index]].item.summonedLevel;
            }
            slot.selectedPet.onClick.SetListener(() =>
            {
                selectedPetSlot = index;
                selectedPet = player.inventory[index];
                slot.outline.enabled = true;
                for (int e = 0; e < petContent.childCount; e++)
                {
                    if (e != index)
                    {
                        petContent.GetChild(index).GetComponent<UIPetSlot>().outline.enabled = false;
                    }
                }
            });
        }

        petInTradingButton.gameObject.SetActive(petTrainer.petTraining.Count > 0 || petTrainer.finishedTraining.Count > 0);
    }
}
