using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotIngredient : MonoBehaviour
{
    public Image image;
    public Button slotButton;
    public Text ingredientName;
    public Text ingredientAmount;
    public int ingredientIndex;

    public void Start()
    {
        image.preserveAspect = true;
    }
}
