using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIUpgradeSlot : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI itemText;
    public TextMeshProUGUI itemLevel;
    public Button itemButton;
    public Image progressBar;

    public void Start()
    {
        image.preserveAspect = true;
        progressBar.preserveAspect = true;
    }
}
