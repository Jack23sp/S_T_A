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
                }
                else if (down)
                {
                    piece.occupiedUP = true;
                    modularPiece.occupiedDOWN = true;
                }
                else if (left)
                {
                    piece.occupiedRIGHT = true;
                    modularPiece.occupiedLEFT = true;
                }
                else
                {
                    piece.occupiedLEFT = true;
                    modularPiece.occupiedRIGHT = true;
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
                }
                else if (down)
                {
                    piece.occupiedDOWN = false;
                }
                else if (left)
                {
                    piece.occupiedLEFT = false;
                }
                else
                {
                    piece.occupiedRIGHT = false;
                }
            }
        }
    }
}
