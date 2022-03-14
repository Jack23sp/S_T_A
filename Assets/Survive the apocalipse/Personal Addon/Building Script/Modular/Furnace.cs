using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Furnace : Building
{

    [SyncVar]
    public bool isActive;

    List<int> freeSlot = new List<int>();
    List<int> rockSlot = new List<int>();
    List<int> sulfurSlot = new List<int>();
    List<int> highMetalSlot = new List<int>();

    public override void Start()
    {
        base.Start();
        Invoke(nameof(PopulateInventory), 1.0f);
        Invoke(nameof(Cook), 10.0f);
    }

    public void PopulateInventory()
    {
       
        if(isServer)
        {
            if(inventory.Count == 0)
            {
                for(int i = 0; i < 7; i++)
                {
                    inventory.Add(new ItemSlot());
                }
            }
        }
    }

    public void Cook()
    {
        freeSlot.Clear();
        rockSlot.Clear();
        sulfurSlot.Clear();
        highMetalSlot.Clear();

        if (isActive)
        {
            if (inventory[0].amount > 0)
            {
                for (int i = 1; i < inventory.Count; i++)
                {
                    int index = i;
                    if (inventory[index].amount == 0)
                    {
                        if (!freeSlot.Contains(index)) freeSlot.Add(index);
                    }
                    else
                    {
                        if (inventory[index].item.data.name == "Rock")
                        {
                            if (!rockSlot.Contains(index)) rockSlot.Add(index);
                        }
                        if (inventory[index].item.data.name == "Sulfur")
                        {
                            if (!sulfurSlot.Contains(index)) sulfurSlot.Add(index);
                        }
                        if (inventory[index].item.data.name == "High quality metal")
                        {
                            if (!highMetalSlot.Contains(index)) highMetalSlot.Add(index);
                        }
                    }
                }

                int randomToAdd = UnityEngine.Random.Range(0, 5);
                if (InventoryCanAdd(new Item(GeneralManager.singleton.coal), randomToAdd))
                {
                    InventoryAdd(new Item(GeneralManager.singleton.coal), randomToAdd);
                }

                if (rockSlot.Count > 0)
                {
                    int randomType = UnityEngine.Random.Range(0, 2);
                    randomToAdd = UnityEngine.Random.Range(0, 5);

                    if (sulfurSlot.Count == highMetalSlot.Count)
                    {
                        if (randomType == 0)
                        {
                            if (InventoryCanAdd(new Item(GeneralManager.singleton.highMetal), randomToAdd))
                            {
                                InventoryAdd(new Item(GeneralManager.singleton.highMetal), randomToAdd);
                            }
                        }
                        else if (randomType == 1)
                        {
                            if (InventoryCanAdd(new Item(GeneralManager.singleton.sulfur), randomToAdd))
                            {
                                InventoryAdd(new Item(GeneralManager.singleton.sulfur), randomToAdd);
                            }
                        }
                    }
                    else
                    {
                        if (highMetalSlot.Count > sulfurSlot.Count)
                        {
                            if (InventoryCanAdd(new Item(GeneralManager.singleton.sulfur), randomToAdd))
                            {
                                InventoryAdd(new Item(GeneralManager.singleton.sulfur), randomToAdd);
                            }
                        }
                        else
                        {
                            if (InventoryCanAdd(new Item(GeneralManager.singleton.highMetal), randomToAdd))
                            {
                                InventoryAdd(new Item(GeneralManager.singleton.highMetal), randomToAdd);
                            }
                        }
                    }

                    ItemSlot rock = inventory[rockSlot[0]];
                    rock.DecreaseAmount(-7);
                    inventory[rockSlot[0]] = rock;
                }

                ItemSlot wood = inventory[0];
                wood.DecreaseAmount(-7);
                inventory[0] = wood;
            }
        }
        Invoke(nameof(Cook), 10.0f);
    }
}
