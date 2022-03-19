using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceScript : MonoBehaviour
{
    public TextMeshPro textMesh;
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer plantSpriteRenderer;
    public Player player;

    public void AssignAndDecrease( string PrefabText , Sprite Sprite)
    {
        textMesh.text = PrefabText;
        spriteRenderer.sprite =  Sprite;
    }

    public void Destroy()
    {
        Destroy(this.gameObject);
    }
}
