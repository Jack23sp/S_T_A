using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class FlooreBasementOccupied : MonoBehaviour
{
    public bool up, down, left, right;
    public SpriteRenderer spriteRenderer;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("FloorBasement"))
        {
            ModularPiece piece = collision.GetComponentInParent<ModularPiece>();
            ModularPiece modularPiece = GetComponentInParent<ModularPiece>();
            if (piece.isServer && piece.GetComponent<NetworkIdentity>().netId != 0)
            {
                if (up)
                {
                    piece.occupiedDOWN = true;
                    modularPiece.occupiedUP = true;
                    modularPiece.upPart = collision.GetComponentInParent<ModularPiece>().modularIndex;
                    piece.downPart = GetComponentInParent<ModularPiece>().modularIndex;
                }
                else if (down)
                {
                    piece.occupiedUP = true;
                    modularPiece.occupiedDOWN = true;
                    modularPiece.downPart = collision.GetComponentInParent<ModularPiece>().modularIndex;
                    piece.upPart = GetComponentInParent<ModularPiece>().modularIndex;
                }
                else if (left)
                {
                    piece.occupiedRIGHT = true;
                    modularPiece.occupiedLEFT = true;
                    modularPiece.leftPart = collision.GetComponentInParent<ModularPiece>().modularIndex;
                    piece.rightPart = GetComponentInParent<ModularPiece>().modularIndex;
                }
                else
                {
                    piece.occupiedLEFT = true;
                    modularPiece.occupiedRIGHT = true;
                    modularPiece.rightPart = collision.GetComponentInParent<ModularPiece>().modularIndex;
                    piece.leftPart = GetComponentInParent<ModularPiece>().modularIndex;
                }
            }
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("FloorBasement"))
        {
            ModularPiece piece = collision.GetComponentInParent<ModularPiece>();
            if (piece.isServer && piece.GetComponent<NetworkIdentity>().netId != 0)
            {
                if (up)
                {
                    piece.occupiedUP = false;
                    piece.upPart = -5;
                }
                else if (down)
                {
                    piece.occupiedDOWN = false;
                    piece.downPart = -5;
                }
                else if (left)
                {
                    piece.occupiedLEFT = false;
                    piece.leftPart = -5;
                }
                else
                {
                    piece.occupiedRIGHT = false;
                    piece.rightPart = -5;
                }
            }
        }
    }
}
