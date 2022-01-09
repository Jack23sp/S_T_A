using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIAddAnimal : MonoBehaviour
{
    public static UIAddAnimal singleton;
    public AnimalCategory animalCategory;
    public Transform content;
    public GameObject animalPrefab;
    public TextMeshProUGUI titleText;

    public Button closeButton;

    public List<int> cowAnimals = new List<int>();
    public List<int> chickenAnimals = new List<int>();
    public List<int> horseAnimals = new List<int>();
    public List<int> goatAnimals = new List<int>();
    public List<int> sheepAnimals = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        if (!singleton) singleton = this;
    }

    // Update is called once per frame
    void Update()
    {

        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

        if (animalCategory == AnimalCategory.sheep)
        {
            UIUtils.BalancePrefabs(animalPrefab, sheepAnimals.Count, content);
            for (int i = 0; i < sheepAnimals.Count; i++)
            {
                int index = i;
                AddAnimalSlot slot = content.GetChild(index).GetComponent<AddAnimalSlot>();
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    slot.nameText.text = ((ScriptableAnimal)Player.localPlayer.inventory[sheepAnimals[index]].item.data).italianName;
                }
                else
                {
                    slot.nameText.text = ((ScriptableAnimal)Player.localPlayer.inventory[sheepAnimals[index]].item.data).name;
                }
                //slot.nameText.text = ((ScriptableAnimal)Player.localPlayer.inventory[sheepAnimals[index]].item.data).name;
                if (ScriptableAnimal.dict.TryGetValue(((ScriptableAnimal)Player.localPlayer.inventory[sheepAnimals[index]].item.data).name.GetStableHashCode(), out ScriptableAnimal tempAnimal))
                {
                    slot.animalImage.sprite = tempAnimal.image;
                }
                if (animalCategory == AnimalCategory.sheep)
                {
                    slot.addButton.onClick.SetListener(() =>
                    {
                        Player.localPlayer.CmdAddSheep(sheepAnimals[index]);
                        sheepAnimals.Clear();
                        GetAnimal();
                        UIBreedingPanel.singleton.InstantiateAnimals();
                    });
                }
            }
        }
        if (animalCategory == AnimalCategory.chicken)
        {
            UIUtils.BalancePrefabs(animalPrefab, chickenAnimals.Count, content);
            for (int i = 0; i < chickenAnimals.Count; i++)
            {
                int index = i;
                AddAnimalSlot slot = content.GetChild(index).GetComponent<AddAnimalSlot>();
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    slot.nameText.text = ((ScriptableAnimal)Player.localPlayer.inventory[chickenAnimals[index]].item.data).italianName;
                }
                else
                {
                    slot.nameText.text = ((ScriptableAnimal)Player.localPlayer.inventory[chickenAnimals[index]].item.data).name;
                }
                if (ScriptableAnimal.dict.TryGetValue(((ScriptableAnimal)Player.localPlayer.inventory[chickenAnimals[index]].item.data).name.GetStableHashCode(), out ScriptableAnimal tempAnimal))
                {
                    slot.animalImage.sprite = tempAnimal.image;
                }
                if (animalCategory == AnimalCategory.chicken)
                {
                    slot.addButton.onClick.SetListener(() =>
                    {
                        Player.localPlayer.CmdAddChicken(chickenAnimals[index]);
                        chickenAnimals.Clear();
                        GetAnimal();
                        UIBreedingPanel.singleton.InstantiateAnimals();
                    });
                }
            }
        }
        if (animalCategory == AnimalCategory.cow)
        {
            UIUtils.BalancePrefabs(animalPrefab, cowAnimals.Count, content);
            for (int i = 0; i < cowAnimals.Count; i++)
            {
                int index = i;
                AddAnimalSlot slot = content.GetChild(index).GetComponent<AddAnimalSlot>();
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    slot.nameText.text = ((ScriptableAnimal)Player.localPlayer.inventory[cowAnimals[index]].item.data).italianName;
                }
                else
                {
                    slot.nameText.text = ((ScriptableAnimal)Player.localPlayer.inventory[cowAnimals[index]].item.data).name;
                }

                if (ScriptableAnimal.dict.TryGetValue(((ScriptableAnimal)Player.localPlayer.inventory[cowAnimals[index]].item.data).name.GetStableHashCode(), out ScriptableAnimal tempAnimal))
                {
                    slot.animalImage.sprite = tempAnimal.image;
                }
                if (animalCategory == AnimalCategory.cow)
                {
                    slot.addButton.onClick.SetListener(() =>
                    {
                        Player.localPlayer.CmdAddCow(cowAnimals[index]);
                        cowAnimals.Clear();
                        GetAnimal();
                        UIBreedingPanel.singleton.InstantiateAnimals();
                    });
                }
            }
        }
        if (animalCategory == AnimalCategory.horse)
        {
            UIUtils.BalancePrefabs(animalPrefab, horseAnimals.Count, content);
            for (int i = 0; i < horseAnimals.Count; i++)
            {
                int index = i;
                AddAnimalSlot slot = content.GetChild(index).GetComponent<AddAnimalSlot>();
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    slot.nameText.text = ((ScriptableAnimal)Player.localPlayer.inventory[horseAnimals[index]].item.data).italianName;
                }
                else
                {
                    slot.nameText.text = ((ScriptableAnimal)Player.localPlayer.inventory[horseAnimals[index]].item.data).name;
                }
                if (ScriptableAnimal.dict.TryGetValue(((ScriptableAnimal)Player.localPlayer.inventory[horseAnimals[index]].item.data).name.GetStableHashCode(), out ScriptableAnimal tempAnimal))
                {
                    slot.animalImage.sprite = tempAnimal.image;
                }
                if (animalCategory == AnimalCategory.horse)
                {
                    slot.addButton.onClick.SetListener(() =>
                    {
                        Player.localPlayer.CmdAddHorse(horseAnimals[index]);
                        horseAnimals.Clear();
                        GetAnimal();
                        UIBreedingPanel.singleton.InstantiateAnimals();
                    });
                }
            }
        }
        if (animalCategory == AnimalCategory.goat)
        {
            UIUtils.BalancePrefabs(animalPrefab, goatAnimals.Count, content);
            for (int i = 0; i < goatAnimals.Count; i++)
            {
                int index = i;
                AddAnimalSlot slot = content.GetChild(index).GetComponent<AddAnimalSlot>();
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    slot.nameText.text = ((ScriptableAnimal)Player.localPlayer.inventory[goatAnimals[index]].item.data).italianName;
                }
                else
                {
                    slot.nameText.text = ((ScriptableAnimal)Player.localPlayer.inventory[goatAnimals[index]].item.data).name;
                }
                if (ScriptableAnimal.dict.TryGetValue(((ScriptableAnimal)Player.localPlayer.inventory[goatAnimals[index]].item.data).name.GetStableHashCode(), out ScriptableAnimal tempAnimal))
                {
                    slot.animalImage.sprite = tempAnimal.image;
                }
                if (animalCategory == AnimalCategory.goat)
                {
                    slot.addButton.onClick.SetListener(() =>
                    {
                        Player.localPlayer.CmdAddPig(goatAnimals[index]);
                        goatAnimals.Clear();
                        GetAnimal();
                        UIBreedingPanel.singleton.InstantiateAnimals();
                    });
                }
            }
        }
    }

    public void GetAnimal()
    {
        if (animalCategory != AnimalCategory.nothing)
        {
            if (animalCategory == AnimalCategory.sheep)
            {
                for (int i = 0; i < Player.localPlayer.inventory.Count; i++)
                {
                    int index = i;
                    if (Player.localPlayer.inventory[index].amount > 0)
                    {
                        if (Player.localPlayer.inventory[index].item.data is ScriptableAnimal)
                        {
                            if (((ScriptableAnimal)Player.localPlayer.inventory[index].item.data).animalCategory == AnimalCategory.sheep)
                            {
                                if (!sheepAnimals.Contains(index)) sheepAnimals.Add(index);
                            }
                        }
                    }
                }
            }
            if (animalCategory == AnimalCategory.chicken)
            {
                for (int i = 0; i < Player.localPlayer.inventory.Count; i++)
                {
                    int index = i;
                    if (Player.localPlayer.inventory[index].amount > 0)
                    {
                        if (Player.localPlayer.inventory[index].item.data is ScriptableAnimal)
                        {
                            if (((ScriptableAnimal)Player.localPlayer.inventory[index].item.data).animalCategory == AnimalCategory.chicken)
                            {
                                if (!chickenAnimals.Contains(index)) chickenAnimals.Add(index);
                            }
                        }
                    }
                }
            }
            if (animalCategory == AnimalCategory.cow)
            {
                for (int i = 0; i < Player.localPlayer.inventory.Count; i++)
                {
                    int index = i;
                    if (Player.localPlayer.inventory[index].amount > 0)
                    {
                        if (Player.localPlayer.inventory[index].item.data is ScriptableAnimal)
                        {
                            if (((ScriptableAnimal)Player.localPlayer.inventory[index].item.data).animalCategory == AnimalCategory.cow)
                            {
                                if (!cowAnimals.Contains(index)) cowAnimals.Add(index);
                            }
                        }
                    }
                }
            }
            if (animalCategory == AnimalCategory.horse)
            {
                for (int i = 0; i < Player.localPlayer.inventory.Count; i++)
                {
                    int index = i;
                    if (Player.localPlayer.inventory[index].amount > 0)
                    {
                        if (Player.localPlayer.inventory[index].item.data is ScriptableAnimal)
                        {
                            if (((ScriptableAnimal)Player.localPlayer.inventory[index].item.data).animalCategory == AnimalCategory.horse)
                            {
                                if (!horseAnimals.Contains(index)) horseAnimals.Add(index);
                            }
                        }
                    }
                }
            }
            if (animalCategory == AnimalCategory.goat)
            {
                for (int i = 0; i < Player.localPlayer.inventory.Count; i++)
                {
                    int index = i;
                    if (Player.localPlayer.inventory[index].amount > 0)
                    {
                        if (Player.localPlayer.inventory[index].item.data is ScriptableAnimal)
                        {
                            if (((ScriptableAnimal)Player.localPlayer.inventory[index].item.data).animalCategory == AnimalCategory.goat)
                            {
                                if (!goatAnimals.Contains(index)) goatAnimals.Add(index);
                            }
                        }
                    }
                }
            }
        }
    }
}
