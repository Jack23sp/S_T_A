using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RequestSlot : MonoBehaviour
{
    public Image online;
    public TextMeshProUGUI friendRequestName;
    public Button friendRequestAccept;
    public Button friendRequestRemove;
    public Button seeFriend;

    public void Start()
    {
        online.preserveAspect = true;
    }
}
