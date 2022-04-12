using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilitySlot : MonoBehaviour
{
    public TextMeshProUGUI statName;
    public TextMeshProUGUI statAmount;
    public Image image;
    public Button button;

    public void Start()
    {
        image.preserveAspect = true;
    }
}
