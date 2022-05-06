using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelectionCameraPositioning : MonoBehaviour
{
    public Vector3 bodyCameraPositioning;
    public Quaternion bodyCameraRotation;
    public Transform playerDimension;
    public Transform bodyCameraTransform;
    public Camera bodyCamera;
    public float bodyCameraSize = 2.08f;

    public Player player;

    public UICharacterSelection UICharacterSelection;

    public void Start()
    {
        player = GetComponentInParent<Player>();
    }

    void Update()
    {
        if(player)
        {

            if (player.isClient) Destroy(this);
            if(!UICharacterSelection) UICharacterSelection = FindObjectOfType<UICharacterSelection>();
            if (UICharacterSelection && UICharacterSelection.panel.activeInHierarchy)
            {
                if (!bodyCameraTransform)
                {
                    bodyCameraTransform = player.avatarCamera.transform;
                    bodyCamera = player.avatarCamera;
                }
                else
                {
                    bodyCameraTransform.transform.localPosition = bodyCameraPositioning;
                    bodyCameraTransform.transform.localRotation = bodyCameraRotation;
                    playerDimension.transform.localScale = new Vector3(1.0f,1.0f,1.0f);
                    bodyCamera.orthographicSize = bodyCameraSize;
                    Destroy(this);
                }
            }
        }
    }
}
