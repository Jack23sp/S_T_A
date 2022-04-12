using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlagSlot : MonoBehaviour
{
    public Button button;
    public Image flagImage;
    public TextMeshProUGUI flagName;

    public void Start()
    {
        flagImage.preserveAspect = true;
    }
}
