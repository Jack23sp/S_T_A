using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Furnace : Building
{

    [SyncVar]
    public bool isActive;

    public override void Start()
    {
        base.Start();
        Invoke(nameof(PopulateInventory), 1.0f);
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

}
