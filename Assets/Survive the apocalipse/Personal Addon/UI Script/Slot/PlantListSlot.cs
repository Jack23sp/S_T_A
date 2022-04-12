using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlantListSlot : MonoBehaviour
{
    public Image plantImage;
    public TextMeshProUGUI plantText;
    public Button plantButton;
    public TextMeshProUGUI plantButtonText;

    public void Start()
    {
        plantImage.preserveAspect = true;
    }
}
