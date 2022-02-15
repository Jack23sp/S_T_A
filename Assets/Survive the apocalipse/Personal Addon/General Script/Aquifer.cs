using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Aquifer : NetworkBehaviour
{
    [SyncVar]
    public int actualWater;

    public int maxWater = 500;
}
