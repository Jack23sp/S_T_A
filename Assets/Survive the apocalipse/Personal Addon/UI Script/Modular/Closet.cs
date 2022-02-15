using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class Closet : ModularObject
{
    public SyncListItemSlot inventory = new SyncListItemSlot();

    public int maxSlotAmount = 25;

    public new void Start()
    {
        base.Start();
        if(isServer)
        {
            if(inventory.Count != maxSlotAmount)
            {
                for(int i = inventory.Count; i < maxSlotAmount; i++)
                {
                    inventory.Add(new ItemSlot());
                }
            }
        }
    }
}