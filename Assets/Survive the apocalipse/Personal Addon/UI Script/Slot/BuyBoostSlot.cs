using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuyBoostSlot : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI coins;
    public TextMeshProUGUI gold;
    public Button boostButton;
    public Image coinImage;
    public Image goldImage;
    public Image boostImage;

    public void Start()
    {
        coinImage.preserveAspect = true;
        goldImage.preserveAspect = true;
        boostImage.preserveAspect = true;
    }
}
