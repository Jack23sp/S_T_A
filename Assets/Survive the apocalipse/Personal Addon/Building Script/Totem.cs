using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Totem : NetworkBehaviour
{
    [TextArea(30,30)]
    public string defaultMessage;
    [TextArea(30, 30)]
    public string defaultMessageIta;

    [SyncVar]
    public string message;
}
