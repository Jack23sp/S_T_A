using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Warehouse : NetworkBehaviour
{
    public SyncListWarehouse one = new SyncListWarehouse();
    public SyncListWarehouse two = new SyncListWarehouse();
    public SyncListWarehouse three = new SyncListWarehouse();
    public SyncListWarehouse four = new SyncListWarehouse();
    public SyncListWarehouse five = new SyncListWarehouse();
    public SyncListWarehouse six = new SyncListWarehouse();

    [SyncVar]
    public bool personal;

    public Entity entity;
    // Start is called before the first frame update
    public void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (NetworkServer.active && isServer)
        {
            if (personal)
            {
                if (one.Count == 0)
                {
                    if (entity.level > 0)
                    {
                        for (int i = 0; i < GeneralManager.singleton.personalWarehouseSlot; i++)
                        {
                            one.Add(new ItemSlot());
                        }
                    }
                }
                if (two.Count == 0)
                {
                    if(entity.level > 10)
                    {
                        for (int i = 0; i < GeneralManager.singleton.personalWarehouseSlot; i++)
                        {
                            two.Add(new ItemSlot());
                        }
                    }
                }
                if (three.Count == 0)
                {
                    if (entity.level > 20)
                    {
                        for (int i = 0; i < GeneralManager.singleton.personalWarehouseSlot; i++)
                        {
                            three.Add(new ItemSlot());
                        }
                    }
                }
                if (four.Count == 0)
                {
                    if (entity.level > 30)
                    {
                        for (int i = 0; i < GeneralManager.singleton.personalWarehouseSlot; i++)
                        {
                            four.Add(new ItemSlot());
                        }
                    }
                }
                if (five.Count == 0)
                {
                    if (entity.level > 40)
                    {
                        for (int i = 0; i < GeneralManager.singleton.personalWarehouseSlot; i++)
                        {
                            five.Add(new ItemSlot());
                        }
                    }
                }
                if (six.Count == 0)
                {
                    if (entity.level == 50)
                    {
                        for (int i = 0; i < GeneralManager.singleton.personalWarehouseSlot; i++)
                        {
                            six.Add(new ItemSlot());
                        }
                    }
                }
            }
            else
            {
                if (one.Count == 0)
                {
                    for (int i = 0; i < GeneralManager.singleton.groupWarehouseSlot; i++)
                    {
                        one.Add(new ItemSlot());
                    }
                }
                if (two.Count == 0)
                {
                    for (int i = 0; i < GeneralManager.singleton.groupWarehouseSlot; i++)
                    {
                        two.Add(new ItemSlot());
                    }
                }
                if (three.Count == 0)
                {
                    for (int i = 0; i < GeneralManager.singleton.groupWarehouseSlot; i++)
                    {
                        three.Add(new ItemSlot());
                    }
                }
                if (four.Count == 0)
                {
                    for (int i = 0; i < GeneralManager.singleton.groupWarehouseSlot; i++)
                    {
                        four.Add(new ItemSlot());
                    }
                }
                if (five.Count == 0)
                {
                    for (int i = 0; i < GeneralManager.singleton.groupWarehouseSlot; i++)
                    {
                        five.Add(new ItemSlot());
                    }
                }
                if (six.Count == 0)
                {
                    for (int i = 0; i < GeneralManager.singleton.groupWarehouseSlot; i++)
                    {
                        six.Add(new ItemSlot());
                    }
                }
            }

        }
    }
}
