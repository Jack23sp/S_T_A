using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Furnace : ModularObject
{
    public SyncList<ItemSlot> furnaceSlot = new SyncList<ItemSlot>();

    [SyncVar]
    public bool isActive;

    void Start()
    {
        if(isServer)
        {
            if(furnaceSlot.Count == 0)
            {
                for(int i = 0; i < 7; i++)
                {
                    furnaceSlot.Add(new ItemSlot());
                }
            }
        }
    }

    void Update()
    {
        
    }
}
