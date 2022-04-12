using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieceSlot : MonoBehaviour
{
    public Image pieceImage;
    public Text name;
    public Text amount;
    public Outline outline;
    public Button selectButton;

    public void Start()
    {
        pieceImage.preserveAspect = true;
    }
}
