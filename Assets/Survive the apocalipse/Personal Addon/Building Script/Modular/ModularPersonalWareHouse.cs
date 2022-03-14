using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ModularPersonalWareHouse : ModularObject
{
    public SyncListWarehouse inventory = new SyncListWarehouse();

    public new void Start()
    {
        base.Start();
        if (NetworkServer.active && isServer)
        {
            for (int i = 0; i < GeneralManager.singleton.personalWarehouseSlot; i++)
            {
                inventory.Add(new ItemSlot());
            }
        }
    }
}
