using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowShaderVisualizer : MonoBehaviour
{
    public SpriteRenderer snowRenderer;

    public SpriteRenderer mainRenderer;

    void Start()
    {
        if (!mainRenderer)
        {
            mainRenderer = GetComponent<SpriteRenderer>();
            snowRenderer.sortingOrder = mainRenderer.sortingOrder + 1;
        }
    }

}
