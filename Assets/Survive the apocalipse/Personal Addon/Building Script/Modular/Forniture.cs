using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Forniture : NetworkBehaviour
{
    [SyncVar]
    public int objectIndex;

    public ScriptableBuilding scriptableBuilding;

}
