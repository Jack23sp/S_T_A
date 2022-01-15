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
            if (piece.isServer && piece.GetComponent<NetworkIdentity>().netId != 0)
            {
                if (up)
                {
                    piece.occupiedUP = true;
                    piece.upPart = GetComponentInParent<ModularPiece>().modularIndex;
                }
                else if (down)
                {
                    piece.occupiedDOWN = true;
                    piece.downPart = GetComponentInParent<ModularPiece>().modularIndex;
                }
                else if (left)
                {
                    piece.occupiedLEFT = true;
                    piece.leftPart = GetComponentInParent<ModularPiece>().modularIndex;
                }
                else
                {
                    piece.occupiedRIGHT = true;
                    piece.rightPart = GetComponentInParent<ModularPiece>().modularIndex;
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
                    piece.upPart = -1;
                }
                else if (down)
                {
                    piece.occupiedDOWN = false;
                    piece.downPart = -1;
                }
                else if (left)
                {
                    piece.occupiedLEFT = false;
                    piece.leftPart = -1;
                }
                else
                {
                    piece.occupiedRIGHT = false;
                    piece.rightPart = -1;
                }
            }
        }
    }
}
