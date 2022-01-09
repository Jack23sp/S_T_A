using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelectionCameraPositioning : MonoBehaviour
{
    public Vector3 bodyCameraPositioning;
    public Quaternion bodyCameraRotation;
    public Transform bodyCamera;

    public Player player;

    public UICharacterSelection UICharacterSelection;

    void Update()
    {
        if(player)
        {
            if (player.isClient) Destroy(this);
            if(!UICharacterSelection) UICharacterSelection = FindObjectOfType<UICharacterSelection>();
            if (UICharacterSelection && UICharacterSelection.panel.activeInHierarchy)
            {
                if (!bodyCamera) bodyCamera = player.avatarCamera.transform;
                else
                {
                    bodyCamera.transform.localPosition = bodyCameraPositioning;
                    bodyCamera.transform.localRotation = bodyCameraRotation;
                    Destroy(this);
                }
            }
        }
    }
}
