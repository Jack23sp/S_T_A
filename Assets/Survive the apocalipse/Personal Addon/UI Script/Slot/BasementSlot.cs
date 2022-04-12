using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BasementSlot : MonoBehaviour
{
    public Button slotButton;
    public Image slotImage;
    public TextMeshProUGUI amountText;

    public void Start()
    {
        slotImage.preserveAspect = true;
    }
}
