using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class FlooreBasementOccupied : MonoBehaviour
{
    public bool up, down, left, right;
    public SpriteRenderer spriteRenderer;
    public ModularPiece mainModularPiece;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("FloorBasement"))
        {
            ModularPiece piece = collision.GetComponentInParent<ModularPiece>();
            if (piece.isServer && piece != mainModularPiece && piece.GetComponent<NetworkIdentity>().netId != 0)
            {
                if (up)
                {
                    piece.occupiedDOWN = true;
                    mainModularPiece.occupiedUP = true;
                }
                else if (down)
                {
                    piece.occupiedUP = true;
                    mainModularPiece.occupiedDOWN = true;
                }
                else if (left)
                {
                    piece.occupiedRIGHT = true;
                    mainModularPiece.occupiedLEFT = true;
                }
                else
                {
                    piece.occupiedLEFT = true;
                    mainModularPiece.occupiedRIGHT = true;
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
