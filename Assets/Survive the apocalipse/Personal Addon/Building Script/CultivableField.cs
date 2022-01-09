using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using CustomType;

public class CultivableField : NetworkBehaviour
{
    public SyncListPlant currentPlant = new SyncListPlant();
    public int cycleAmount = 0;
    // Start is called before the first frame update
    void Start()
    {
        if (isServer)
        {
            if (currentPlant.Count == 0)
            {
                for (int i = 0; i < 42; i++)
                {
                    CultivableFood food = new CultivableFood();
                    food.plantName = "Undefined";
                    food.grownQuantityX = 0.0f;
                    food.grownQuantityY = 0.0f;
                    currentPlant.Add(food);
                }
            }
            InvokeRepeating("ManagePlant", 0.0f, GeneralManager.singleton.intervalGrownPlant);
            if (isServer && isClient) InvokeRepeating("ManagePlantOnClient", 0.0f, GeneralManager.singleton.intervalGrownPlantOnClient);
        }
        else
        {
            InvokeRepeating("ManagePlantOnClient", 0.0f, GeneralManager.singleton.intervalGrownPlantOnClient);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ManagePlant()
    {
        cycleAmount++;
        for (int i = 0; i < currentPlant.Count; i++)
        {
            int index = i;
            if (currentPlant[index].plantName != string.Empty)
            {
                if (currentPlant[index].plantName == "Undefined") continue;
                if (ScriptablePlant.dict.TryGetValue(currentPlant[index].plantName.GetStableHashCode(), out ScriptablePlant plant))
                {
                    if (currentPlant[index].alreadyGrown)
                    {
                        CultivableFood foodPlant = currentPlant[index];
                        if (TemperatureManager.singleton.season == currentPlant[index].season)
                        {
                            if (currentPlant[index].grownQuantityX < plant.scaleDimension.x)
                            {
                                foodPlant.grownQuantityX = foodPlant.grownQuantityX + plant.GrowAmount;
                            }
                            if (currentPlant[index].grownQuantityY < plant.scaleDimension.y)
                            {
                                foodPlant.grownQuantityY = foodPlant.grownQuantityY + plant.GrowAmount;
                            }
                            currentPlant[index] = foodPlant;
                        }
                        else
                        {
                            foodPlant.grownQuantityX = foodPlant.grownQuantityX - plant.GrowAmount;
                            foodPlant.grownQuantityY = foodPlant.grownQuantityY - plant.GrowAmount;
                            currentPlant[index] = foodPlant;

                            if (foodPlant.grownQuantityX < 0.0f || foodPlant.grownQuantityY < 0.0f)
                            {
                                foodPlant = new CultivableFood();
                                foodPlant.plantName = "Undefined";
                                foodPlant.grownQuantityX = 0.0f;
                                foodPlant.grownQuantityY = 0.0f;
                                currentPlant[index] = foodPlant;
                            }
                        }
                    }
                    else
                    {
                        if (TemperatureManager.singleton.season == plant.GrowSeason)
                        {
                            CultivableFood foodPlant = currentPlant[index];
                            if (currentPlant[index].grownQuantityX < plant.scaleDimension.x)
                            {
                                foodPlant.grownQuantityX = foodPlant.grownQuantityX + plant.GrowAmount;
                            }
                            if (currentPlant[index].grownQuantityY < plant.scaleDimension.y)
                            {
                                foodPlant.grownQuantityY = foodPlant.grownQuantityY + plant.GrowAmount;
                            }

                            if (currentPlant[index].grownQuantityX >= plant.scaleDimension.x && currentPlant[index].grownQuantityY >= plant.scaleDimension.y)
                            {
                                foodPlant.alreadyGrown = true;
                            }


                            currentPlant[index] = foodPlant;
                        }
                        else
                        {
                            CultivableFood foodPlant = currentPlant[index];

                            foodPlant.grownQuantityX = foodPlant.grownQuantityX - plant.GrowAmount;
                            foodPlant.grownQuantityY = foodPlant.grownQuantityY - plant.GrowAmount;
                            currentPlant[index] = foodPlant;

                            if (currentPlant[index].grownQuantityX < 0.0f || currentPlant[index].grownQuantityY < 0.0f)
                            {
                                foodPlant = new CultivableFood();
                                foodPlant.grownQuantityX = 0.0f;
                                foodPlant.grownQuantityY = 0.0f;
                                foodPlant.plantName = "Undefined";
                                currentPlant[index] = foodPlant;
                            }
                        }
                        transform.GetChild(index + 5).GetComponent<SpriteRenderer>().sprite = plant.image;
                    }
                }
            }
            else
            {
                transform.GetChild(index + 5).GetComponent<SpriteRenderer>().sprite = null;
            }


        }
    }

    public void ManagePlantOnClient()
    {

        for (int i = 0; i < currentPlant.Count; i++)
        {
            int index = i;
            if (currentPlant[index].plantName == string.Empty || currentPlant[index].plantName == "Undefinded")
            {
                transform.GetChild(index + 5).GetComponent<SpriteRenderer>().sprite = null;
                continue;
            }
            if (ScriptablePlant.dict.TryGetValue(currentPlant[index].plantName.GetStableHashCode(), out ScriptablePlant plant))
            {
                transform.GetChild(index + 5).GetComponent<SpriteRenderer>().sprite = plant.image;
                transform.GetChild(index + 5).transform.localScale = new Vector3(currentPlant[index].grownQuantityX, currentPlant[index].grownQuantityY, 0);
                transform.GetChild(index + 5).GetComponent<SpriteRenderer>().sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 1;
            }
        }
    }
}
