using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class KitchenSink : ModularObject
{
    [SyncVar]
    public int currentWater;
    public int maxWater = 250;

}
