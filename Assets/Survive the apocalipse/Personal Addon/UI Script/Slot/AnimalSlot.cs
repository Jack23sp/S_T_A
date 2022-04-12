using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimalSlot : MonoBehaviour
{
    public Button selectButton;
    public Text descriptionText;
    public Image animalImage;
    public Image pregnantImage;
    public Image takeImage;
    public Button killButton;

    public void Start()
    {
        animalImage.preserveAspect = true;
        pregnantImage.preserveAspect = true;
        takeImage.preserveAspect = true;
    }
}
