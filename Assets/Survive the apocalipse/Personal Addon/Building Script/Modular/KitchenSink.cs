using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class KitchenSink : ModularObject
{
    [SyncVar]
    public int currentWater;
    public int maxWater = 250;

    public void Start()
    {
        InvokeRepeating(nameof(TakeWater), 0.0f, 5.0f);
    }

    public void TakeWater()
    {
        if (identity.isServer)
        {
            if (currentWater < maxWater)
            {
                currentWater++;
            }
        }
        else
        {
            CancelInvoke();
        }
    }
}
