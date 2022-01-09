using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UIBreedingPanel : MonoBehaviour
{
    public static UIBreedingPanel singleton;
    public Button cowButton;
    public Button horseButton;
    public Button pigButton;
    public Button chickenButton;
    public Button sheepButton;

    public Button addCowButton;
    public Button listCowButton;
    public Button addHorseButton;
    public Button listHorseButton;
    public Button addPigButton;
    public Button listPigButton;
    public Button addChickenButton;
    public Button listChickenButton;
    public Button addSheepButton;
    public Button lsitSheepButton;

    public Transform cowContent;
    public Transform pigContent;
    public Transform chickenContent;
    public Transform horseContent;
    public Transform sheepContent;

    public Button closeButton;

    public float xDivider = 1.5f;
    public float yDivider = 1.5f;

    void Start()
    {
        if (!singleton) singleton = this;
        InstantiateAnimals();
    }

    void Update()
    {
        if (Player.localPlayer.health == 0)
            closeButton.onClick.Invoke();

        InstantiateAnimals();
        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

        addCowButton.onClick.SetListener(() =>
        {
            GameObject g = Instantiate(GeneralManager.singleton.addAnimalPanel, GeneralManager.singleton.canvas);
            g.GetComponent<UIAddAnimal>().animalCategory = AnimalCategory.cow;
            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                g.GetComponent<UIAddAnimal>().titleText.text = "Aggiungi : <b>Mucche</b>";
            }
            else
            {
                g.GetComponent<UIAddAnimal>().titleText.text = "Add : <b>Cows</b>";
            }
            g.GetComponent<UIAddAnimal>().GetAnimal();
        });
        listCowButton.onClick.SetListener(() =>
        {
            GameObject g = Instantiate(GeneralManager.singleton.animalListPanel, GeneralManager.singleton.canvas);
            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                g.GetComponent<UIListAnimal>().titleText.text = "Lista : <b>Mucche</b>";
            }
            else
            {
                g.GetComponent<UIListAnimal>().titleText.text = "List of <b>Cows</b>";
            }
            g.GetComponent<UIListAnimal>().animalCategory = AnimalCategory.cow;
        });

        addHorseButton.onClick.SetListener(() =>
        {
            GameObject g = Instantiate(GeneralManager.singleton.addAnimalPanel, GeneralManager.singleton.canvas);
            g.GetComponent<UIAddAnimal>().animalCategory = AnimalCategory.horse;

            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                g.GetComponent<UIAddAnimal>().titleText.text = "Aggiungi : <b>Cavalli</b>";
            }
            else
            {
                g.GetComponent<UIAddAnimal>().titleText.text = "Add : <b>Horses</b>";
            }

            g.GetComponent<UIAddAnimal>().GetAnimal();
        });
        listHorseButton.onClick.SetListener(() =>
        {
            GameObject g = Instantiate(GeneralManager.singleton.animalListPanel, GeneralManager.singleton.canvas);

            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                g.GetComponent<UIListAnimal>().titleText.text = "Lista : <b>Cavalli</b>";
            }
            else
            {
                g.GetComponent<UIListAnimal>().titleText.text = "Add : <b>Horses</b>";
            }
            g.GetComponent<UIListAnimal>().animalCategory = AnimalCategory.horse;
        });

        addChickenButton.onClick.SetListener(() =>
        {
            GameObject g = Instantiate(GeneralManager.singleton.addAnimalPanel, GeneralManager.singleton.canvas);
            g.GetComponent<UIAddAnimal>().animalCategory = AnimalCategory.chicken;

            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                g.GetComponent<UIAddAnimal>().titleText.text = "Aggiungi : <b>Galline</b>";
            }
            else
            {
                g.GetComponent<UIAddAnimal>().titleText.text = "Add : <b>Chickens</b>";
            }

            g.GetComponent<UIAddAnimal>().GetAnimal();
        });
        listChickenButton.onClick.SetListener(() =>
        {
            GameObject g = Instantiate(GeneralManager.singleton.animalListPanel, GeneralManager.singleton.canvas);

            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                g.GetComponent<UIListAnimal>().titleText.text = "Aggiungi : <b>Galline</b>";
            }
            else
            {
                g.GetComponent<UIListAnimal>().titleText.text = "Add : <b>Chickens</b>";
            }

            g.GetComponent<UIListAnimal>().animalCategory = AnimalCategory.chicken;
        });

        addPigButton.onClick.SetListener(() =>
        {
            GameObject g = Instantiate(GeneralManager.singleton.addAnimalPanel, GeneralManager.singleton.canvas);
            g.GetComponent<UIAddAnimal>().animalCategory = AnimalCategory.goat;

            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                g.GetComponent<UIAddAnimal>().titleText.text = "Aggiungi : <b>Capre</b>";
            }
            else
            {
                g.GetComponent<UIAddAnimal>().titleText.text = "Add : <b>Goats</b>";
            }

            g.GetComponent<UIAddAnimal>().GetAnimal();
        });
        listPigButton.onClick.SetListener(() =>
        {
            GameObject g = Instantiate(GeneralManager.singleton.animalListPanel, GeneralManager.singleton.canvas);

            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                g.GetComponent<UIListAnimal>().titleText.text = "Aggiungi : <b>Capre</b>";
            }
            else
            {
                g.GetComponent<UIListAnimal>().titleText.text = "Add : <b>Goats</b>";
            }
            g.GetComponent<UIListAnimal>().animalCategory = AnimalCategory.goat;
        });

        addSheepButton.onClick.SetListener(() =>
        {
            GameObject g = Instantiate(GeneralManager.singleton.addAnimalPanel, GeneralManager.singleton.canvas);
            g.GetComponent<UIAddAnimal>().animalCategory = AnimalCategory.sheep;

            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                g.GetComponent<UIAddAnimal>().titleText.text = "Aggiungi : <b>Pecore</b>";
            }
            else
            {
                g.GetComponent<UIAddAnimal>().titleText.text = "Add : <b>Sheeps</b>";
            }

            g.GetComponent<UIAddAnimal>().GetAnimal();
        });
        lsitSheepButton.onClick.SetListener(() =>
        {
            GameObject g = Instantiate(GeneralManager.singleton.animalListPanel, GeneralManager.singleton.canvas);

            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                g.GetComponent<UIListAnimal>().titleText.text = "Aggiungi : <b>Pecore</b>";
            }
            else
            {
                g.GetComponent<UIListAnimal>().titleText.text = "Add : <b>Sheeps</b>";
            }

            g.GetComponent<UIListAnimal>().animalCategory = AnimalCategory.sheep;
        });


    }

    public void InstantiateAnimals()
    {
        InstantiateAnimalInContent(cowContent, AnimalCategory.cow);
        InstantiateAnimalInContent(horseContent, AnimalCategory.horse);
        InstantiateAnimalInContent(chickenContent, AnimalCategory.chicken);
        InstantiateAnimalInContent(sheepContent, AnimalCategory.sheep);
        InstantiateAnimalInContent(pigContent, AnimalCategory.goat);
    }

    public void InstantiateAnimalInContent(Transform content, AnimalCategory animalCategory)
    {
        Breeding breeding = Player.localPlayer.target.GetComponent<Breeding>();
        if (animalCategory == AnimalCategory.chicken)
        {
            for (int i = chickenContent.childCount; i < breeding.chicken.Count; i++)
            {
                int index = i;
                float x = Random.Range(content.position.x - content.GetComponent<RectTransform>().sizeDelta.x / xDivider, content.position.x + content.GetComponent<RectTransform>().sizeDelta.x / xDivider);
                float y = Random.Range(content.position.y - content.GetComponent<RectTransform>().sizeDelta.y / yDivider, content.position.y + content.GetComponent<RectTransform>().sizeDelta.y / yDivider);

                GameObject g = Instantiate(GeneralManager.singleton.GetAnimalImage(breeding.chicken[index].name, breeding.chicken[index].age), chickenContent);
                g.transform.position = new Vector3(x, y, 0.0f);
            }
        }
        if (animalCategory == AnimalCategory.cow)
        {
            for (int i = cowContent.childCount; i < breeding.cow.Count; i++)
            {
                int index = i;
                float x = Random.Range(content.position.x - content.GetComponent<RectTransform>().sizeDelta.x / xDivider, content.position.x + content.GetComponent<RectTransform>().sizeDelta.x / xDivider);
                float y = Random.Range(content.position.y - content.GetComponent<RectTransform>().sizeDelta.y / yDivider, content.position.y + content.GetComponent<RectTransform>().sizeDelta.y / yDivider);

                GameObject g = Instantiate(GeneralManager.singleton.GetAnimalImage(breeding.cow[index].name, breeding.cow[index].age), cowContent);
                g.transform.position = new Vector3(x, y, 0.0f);
            }
        }
        if (animalCategory == AnimalCategory.horse)
        {
            for (int i = horseContent.childCount; i < breeding.horse.Count; i++)
            {
                int index = i;
                float x = Random.Range(content.position.x - content.GetComponent<RectTransform>().sizeDelta.x / xDivider, content.position.x + content.GetComponent<RectTransform>().sizeDelta.x / xDivider);
                float y = Random.Range(content.position.y - content.GetComponent<RectTransform>().sizeDelta.y / yDivider, content.position.y + content.GetComponent<RectTransform>().sizeDelta.y / yDivider);

                GameObject g = Instantiate(GeneralManager.singleton.GetAnimalImage(breeding.horse[index].name, breeding.horse[index].age), horseContent);
                g.transform.position = new Vector3(x, y, 0.0f);
            }
        }
        if (animalCategory == AnimalCategory.goat)
        {
            for (int i = pigContent.childCount; i < breeding.goat.Count; i++)
            {
                int index = i;
                float x = Random.Range(content.position.x - content.GetComponent<RectTransform>().sizeDelta.x / xDivider, content.position.x + content.GetComponent<RectTransform>().sizeDelta.x / xDivider);
                float y = Random.Range(content.position.y - content.GetComponent<RectTransform>().sizeDelta.y / yDivider, content.position.y + content.GetComponent<RectTransform>().sizeDelta.y / yDivider);

                GameObject g = Instantiate(GeneralManager.singleton.GetAnimalImage(breeding.goat[index].name, breeding.goat[index].age), pigContent);
                g.transform.position = new Vector3(x, y, 0.0f);
            }
        }
        if (animalCategory == AnimalCategory.sheep)
        {
            for (int i = sheepContent.childCount; i < breeding.sheep.Count; i++)
            {
                int index = i;
                float x = Random.Range(content.position.x - content.GetComponent<RectTransform>().sizeDelta.x / xDivider, content.position.x + content.GetComponent<RectTransform>().sizeDelta.x / xDivider);
                float y = Random.Range(content.position.y - content.GetComponent<RectTransform>().sizeDelta.y / yDivider, content.position.y + content.GetComponent<RectTransform>().sizeDelta.y / yDivider);

                GameObject g = Instantiate(GeneralManager.singleton.GetAnimalImage(breeding.sheep[index].name, breeding.sheep[index].age), sheepContent);
                g.transform.position = new Vector3(x, y, 0.0f);
            }
        }
    }


}



public partial class Player
{
    [Command]
    public void CmdAddAnimal(int indice)
    {
        Breeding breeding;
        breeding = target.GetComponent<Breeding>();
        if (!breeding) return;

        int sum = breeding.cow.Count + breeding.horse.Count + breeding.goat.Count + breeding.chicken.Count + breeding.sheep.Count;


        if (breeding.animalEachLevel.Get(breeding.GetComponent<Building>().level) == sum) return;

        Animal animal = new Animal();
        animal.name = inventory[indice].item.data.name;
        animal.age = 0;

        animal.timeToTake = ((ScriptableAnimal)inventory[indice].item.data).timeToTake;
        animal.distanceBeetweenBorn = ((ScriptableAnimal)inventory[indice].item.data).distanceBeetweenBorn;
        animal.lastBorn = 0;
        animal.lastTake = 0;
        animal.amountWaterNeeded = ((ScriptableAnimal)inventory[indice].item.data).amountWaterNeeded;
        animal.amountFoodNeeded = ((ScriptableAnimal)inventory[indice].item.data).amountFoodNeeded;
        animal.maxHealth = ((ScriptableAnimal)inventory[indice].item.data).health;
        animal.health = ((ScriptableAnimal)inventory[indice].item.data).health;
        animal.otherSexName = ((ScriptableAnimal)inventory[indice].item.data).name;
        if (((ScriptableAnimal)inventory[indice].item.data).sex == AnimalSex.male)
        {
            animal.sex = "MALE";
        }
        if (((ScriptableAnimal)inventory[indice].item.data).sex == AnimalSex.female)
        {
            animal.sex = "FEMALE";
        }
        if (((ScriptableAnimal)inventory[indice].item.data).animalCategory == AnimalCategory.cow)
        {
            animal.animalCategory = "COW";
            breeding.cow.Add(animal);
        }
        if (((ScriptableAnimal)inventory[indice].item.data).animalCategory == AnimalCategory.horse)
        {
            animal.animalCategory = "HORSE";
            breeding.horse.Add(animal);
        }
        if (((ScriptableAnimal)inventory[indice].item.data).animalCategory == AnimalCategory.goat)
        {
            animal.animalCategory = "GOAT";
            breeding.goat.Add(animal);
        }
        if (((ScriptableAnimal)inventory[indice].item.data).animalCategory == AnimalCategory.chicken)
        {
            animal.animalCategory = "CHICKEN";
            breeding.chicken.Add(animal);
        }
        if (((ScriptableAnimal)inventory[indice].item.data).animalCategory == AnimalCategory.sheep)
        {
            animal.animalCategory = "SHEEP";
            breeding.sheep.Add(animal);
        }

        ItemSlot slot = inventory[indice];
        slot.amount--;
        inventory[indice] = slot;
    }

    [Command]
    public void CmdKillAnimal(int indice, string category)
    {
        SyncListAnimal temp = new SyncListAnimal();
        Breeding breeding = target.GetComponent<Breeding>();
        if (!breeding) return;
        if (category == string.Empty) return;
        Animal animal = null;

        if (category == AnimalCategory.cow.ToString())
        {
            animal = breeding.cow[indice];
        }
        if (category == AnimalCategory.horse.ToString())
        {
            animal = breeding.horse[indice];
        }
        if (category == AnimalCategory.goat.ToString())
        {
            animal = breeding.goat[indice];
        }
        if (category == AnimalCategory.chicken.ToString())
        {
            animal = breeding.chicken[indice];
        }
        if (category == AnimalCategory.sheep.ToString())
        {
            animal = breeding.sheep[indice];
        }

        if (animal != null && animal.age > GeneralManager.singleton.FindAnimal(animal.name).maxAge)
        {
            return;
        }

        ScriptableAnimal item = GeneralManager.singleton.FindAnimal(animal.name);

        for (int i = 0; i < item.finalItem.Count; i++)
        {
            if (InventoryCanAdd(new Item(item.finalItem[i].item), item.finalItem[i].amount))
            {
                InventoryAdd(new Item(item.finalItem[i].item), item.finalItem[i].amount);
            }
        }
        if (category == AnimalCategory.cow.ToString())
        {
            temp = breeding.cow;
            temp.Remove(animal);
            breeding.cow = temp;
        }
        if (category == AnimalCategory.horse.ToString())
        {
            temp = breeding.horse;
            temp.Remove(animal);
            breeding.horse = temp;
        }
        if (category == AnimalCategory.goat.ToString())
        {
            temp = breeding.goat;
            temp.Remove(animal);
            breeding.goat = temp;
        }
        if (category == AnimalCategory.chicken.ToString())
        {
            temp = breeding.chicken;
            temp.Remove(animal);
            breeding.chicken = temp;
        }
        if (category == AnimalCategory.sheep.ToString())
        {
            temp = breeding.sheep;
            temp.Remove(animal);
            breeding.sheep = temp;
        }
    }

    [Command]
    public void CmdTakeFromAnimal(int indice, string category)
    {
        SyncListAnimal temp = new SyncListAnimal();
        Breeding breeding = target.GetComponent<Breeding>();
        if (!breeding) return;
        Animal animal = null;

        if (category == AnimalCategory.cow.ToString())
        {
            animal = breeding.cow[indice];
        }
        if (category == AnimalCategory.horse.ToString())
        {
            animal = breeding.horse[indice];
        }
        if (category == AnimalCategory.goat.ToString())
        {
            animal = breeding.goat[indice];
        }
        if (category == AnimalCategory.chicken.ToString())
        {
            animal = breeding.chicken[indice];
        }
        if (category == AnimalCategory.sheep.ToString())
        {
            animal = breeding.sheep[indice];
        }

        if (animal != null && animal.age >= GeneralManager.singleton.FindAnimal(animal.name).maxAge)
        {
            ScriptableAnimal finalItem = GeneralManager.singleton.FindAnimal(animal.name);

            for (int i = 0; i < finalItem.finalItem.Count; i++)
            {
                if (InventoryCanAdd(new Item(finalItem.finalItem[i].item), finalItem.finalItem[i].amount))
                {
                    InventoryAdd(new Item(finalItem.finalItem[i].item), finalItem.finalItem[i].amount);
                    if (category == AnimalCategory.cow.ToString())
                    {
                        temp = breeding.cow;
                        temp.Remove(animal);
                        breeding.cow = temp;
                    }
                    if (category == AnimalCategory.horse.ToString())
                    {
                        temp = breeding.horse;
                        temp.Remove(animal);
                        breeding.horse = temp;
                    }
                    if (category == AnimalCategory.goat.ToString())
                    {
                        temp = breeding.goat;
                        temp.Remove(animal);
                        breeding.goat = temp;
                    }
                    if (category == AnimalCategory.chicken.ToString())
                    {
                        temp = breeding.chicken;
                        temp.Remove(animal);
                        breeding.chicken = temp;
                    }
                    if (category == AnimalCategory.sheep.ToString())
                    {
                        temp = breeding.sheep;
                        temp.Remove(animal);
                        breeding.sheep = temp;
                    }

                }
            }

            return;
        }
        else if (animal != null && animal.age < GeneralManager.singleton.FindAnimal(animal.name).maxAge && animal.lastTake >= animal.timeToTake)
        {
            ScriptableAnimal intermediateItem = GeneralManager.singleton.FindAnimal(animal.name);

            for (int i = 0; i < intermediateItem.intermediateItem.Count; i++)
            {
                if (InventoryCanAdd(new Item(intermediateItem.intermediateItem[i].item), intermediateItem.intermediateItem[i].amount))
                {
                    InventoryAdd(new Item(intermediateItem.intermediateItem[i].item), intermediateItem.intermediateItem[i].amount);
                    if (animal.animalCategory == AnimalCategory.horse.ToString())
                    {
                        if (category == AnimalCategory.horse.ToString())
                        {
                            temp = breeding.horse;
                            temp.Remove(animal);
                            breeding.horse = temp;
                        }
                    }

                    if (category == AnimalCategory.cow.ToString())
                    {
                        animal.lastTake = 0;
                        breeding.cow[indice] = animal;
                    }
                    if (category == AnimalCategory.goat.ToString())
                    {
                        animal.lastTake = 0;
                        breeding.goat[indice] = animal;
                    }
                    if (category == AnimalCategory.chicken.ToString())
                    {
                        animal.lastTake = 0;
                        breeding.chicken[indice] = animal;
                    }
                    if (category == AnimalCategory.sheep.ToString())
                    {
                        animal.lastTake = 0;
                        breeding.sheep[indice] = animal;
                    }

                }

            }

        }

    }

    [Command]
    public void CmdAddWater()
    {
        if (!target || !target.GetComponent<Breeding>()) return;
        Breeding breeding = target.GetComponent<Breeding>();

        breeding.water += 10;
        if (breeding.water > breeding.currentMaxWater)
        {
            breeding.water = breeding.currentMaxWater;
        }
    }

    [Command]
    public void CmdAddFood()
    {
        if (!target || !target.GetComponent<Breeding>()) return;
        Breeding breeding = target.GetComponent<Breeding>();

        breeding.food += 10;
        if (breeding.food > breeding.currentMaxFood)
        {
            breeding.food = breeding.currentMaxFood;
        }
    }
}