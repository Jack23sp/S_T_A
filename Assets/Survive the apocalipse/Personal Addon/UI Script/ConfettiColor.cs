using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConfettiColor
{
    //Front color value
    public Color frontColor;

    //Background color value
    public Color backColor;

    public ConfettiColor(Color front, Color back)
    {
        frontColor = front;
        backColor = back;
    }
}
