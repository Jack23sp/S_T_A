using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnimalListSlot : MonoBehaviour
{
    public Image animalImage;
    public TextMeshProUGUI nameSexText;
    public TextMeshProUGUI agePregnatText;
    public Button takeProductButton;
    public Button killButton;

    public void Start()
    {
        animalImage.preserveAspect = true;
    }
}
