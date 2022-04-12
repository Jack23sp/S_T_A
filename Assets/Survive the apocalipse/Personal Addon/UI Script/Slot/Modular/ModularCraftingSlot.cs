using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModularCraftingSlot : MonoBehaviour
{
    public int index;
    public string itemName;
    public Image image;
    public Button selectButton;
    public GameObject xButton;
    public GameObject amountContainer;
    public TextMeshProUGUI amountText;
    public Image progressBar;

    public void Start()
    {
        image.preserveAspect = true;
    }
}
