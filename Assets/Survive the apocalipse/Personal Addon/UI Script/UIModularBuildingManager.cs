using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UIModularBuildingManager : MonoBehaviour
{
    public static UIModularBuildingManager singleton;

    public ModularPiece piece;

    public Button left;
    public Button up;
    public Button right;
    public Button down;
    public Button center;

    public Button cancelButton;
    public Button confirmButton;

    public GameObject confirmPanel;

    public bool clickedLeft;
    public bool clickedUp;
    public bool clickedRight;
    public bool clickedDown;
    public bool clickedCenter;

    void Start()
    {
        if (!singleton) singleton = this;    
    }

    void Update()
    {
        piece = ModularBuildingManager.singleton.selectedPiece;
        left.interactable = piece.leftComponent != -5;
        up.interactable = piece.upComponent != -5;
        right.interactable = piece.rightComponent != -5;
        down.interactable = piece.downComponent != -5;

        left.onClick.SetListener(() =>
        {
            clickedLeft = true;
            clickedUp = false;
            clickedRight = false;
            clickedDown = false;
            clickedCenter = false;
        });

        up.onClick.SetListener(() =>
        {
            clickedLeft = false;
            clickedUp = true;
            clickedRight = false;
            clickedDown = false;
            clickedCenter = false;
        });

        right.onClick.SetListener(() =>
        {
            clickedLeft = false;
            clickedUp = false;
            clickedRight = true;
            clickedDown = false;
            clickedCenter = false;
        });

        down.onClick.SetListener(() =>
        {
            clickedLeft = false;
            clickedUp = false;
            clickedRight = false;
            clickedDown = true;
            clickedCenter = false;
        });

        center.onClick.SetListener(() =>
        {
            clickedLeft = false;
            clickedUp = false;
            clickedRight = false;
            clickedDown = false;
            clickedCenter = true;
        });

        confirmButton.interactable = (clickedLeft || clickedRight || clickedDown || clickedUp || clickedCenter);
        confirmButton.onClick.SetListener(() =>
        {
            Instantiate(confirmPanel, GeneralManager.singleton.canvas);
        });

        cancelButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });
    }

    public void CheckClickedButton()
    {
        if (clickedLeft)
            Player.localPlayer.playerBuilding.CmdClearWallDoor(piece.GetComponent<NetworkIdentity>(), piece.upComponent, piece.downComponent, -5, piece.rightComponent);
        else if (clickedUp)
            Player.localPlayer.playerBuilding.CmdClearWallDoor(piece.GetComponent<NetworkIdentity>(), -5, piece.downComponent, piece.leftComponent, piece.rightComponent);
        else if (clickedRight)
            Player.localPlayer.playerBuilding.CmdClearWallDoor(piece.GetComponent<NetworkIdentity>(), piece.upComponent, piece.downComponent, piece.leftComponent, -5);
        else if (clickedDown)
            Player.localPlayer.playerBuilding.CmdClearWallDoor(piece.GetComponent<NetworkIdentity>(), piece.upComponent, -5, piece.leftComponent, piece.rightComponent);
        else if (clickedCenter)
            Player.localPlayer.playerBuilding.CmdDestroyBasement(piece.GetComponent<NetworkIdentity>());
    }
}
