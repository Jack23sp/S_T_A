using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldHouseSlot : MonoBehaviour
{
    public Image ItemImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI Rarity;
    public Button takeButton;

    public void Start()
    {
        ItemImage.preserveAspect = true;
    }
}
