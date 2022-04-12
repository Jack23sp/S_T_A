using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftProgressSlot : MonoBehaviour
{
    public int index;
    public Image onlinePlayer;
    public Image itemImage;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemOwnerGuild;
    public Button GetItem;
    public Slider sliderTimer;
    public int craftProgressIndex;
    public string timeBegin;
    public string timeEnd;

    public void Start()
    {
        itemImage.preserveAspect = true;
    }
}
