using Mirror;
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
    public SelectableCharacter selectable;

    public UICharacterSelection UICharacterSelection;

    public bool setted = false;
    public Quaternion defaultLocalRotation;

    public void Start()
    {
        player = GetComponentInParent<Player>();
        if(player) selectable = player.GetComponent<SelectableCharacter>();
    }

    void Update()
    {
        if(player)
        {
            if (player.isClient) Destroy(this);
            if (!setted)
            {
                if (!UICharacterSelection) UICharacterSelection = FindObjectOfType<UICharacterSelection>();
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
                        playerDimension.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                        bodyCamera.orthographicSize = bodyCameraSize;
                        setted = true;
                    }
                }
            }
            if (((NetworkManagerMMO)NetworkManager.singleton).selection == selectable.index)
            {
                transform.Rotate(.0f, 2.0f, 0.0f);
            }
        }
    }

    public void ResetRotation()
    {
        transform.localRotation = defaultLocalRotation;
    }
}
