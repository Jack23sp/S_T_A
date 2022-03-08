using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public Animator animator;

    public ModularPiece modularPiece;

    public List<Collider2D> playerInside = new List<Collider2D>();

    public bool up, down, left, right;

    public NavMeshObstacle2D navMeshObstacle2D;

    public void Update()
    {
        if (up)
        {
            animator.SetBool("OPEN", modularPiece.playerInsideUpDoor > 0);
            if (navMeshObstacle2D)
                navMeshObstacle2D.enabled = modularPiece.playerInsideUpDoor <= 0;
        }
        if (down)
        {
            animator.SetBool("OPEN", modularPiece.playerInsideDownDoor > 0);
            if (navMeshObstacle2D)
                navMeshObstacle2D.enabled = modularPiece.playerInsideDownDoor <= 0;
        }
        if (left)
        {
            animator.SetBool("OPEN", modularPiece.playerInsideLeftDoor > 0);
            if (navMeshObstacle2D)
                navMeshObstacle2D.enabled = modularPiece.playerInsideLeftDoor <= 0;
        }
        if (right)
        {
            animator.SetBool("OPEN", modularPiece.playerInsideRightDoor > 0);
            if (navMeshObstacle2D)
                navMeshObstacle2D.enabled = modularPiece.playerInsideRightDoor <= 0;
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (modularPiece.isServer)
        {
            if ((collision.CompareTag("Player") && GeneralManager.singleton.CanEnterHome(modularPiece, collision.GetComponent<Player>())) || (collision.CompareTag("Player") && modularPiece.level < GeneralManager.singleton.FindNetworkAbilityLevel("Burglar", collision.GetComponent<Player>().name)))
            {
                if (!playerInside.Contains(collision))
                {
                    playerInside.Add(collision);
                    if (up)
                    {
                        modularPiece.playerInsideUpDoor = playerInside.Count;
                    }
                    if (down)
                    {
                        modularPiece.playerInsideDownDoor = playerInside.Count;
                    }
                    if (left)
                    {
                        modularPiece.playerInsideLeftDoor = playerInside.Count;
                    }
                    if (right)
                    {
                        modularPiece.playerInsideRightDoor = playerInside.Count;
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (modularPiece.isServer)
        {
            if ((collision.CompareTag("Player") && GeneralManager.singleton.CanEnterHome(modularPiece, collision.GetComponent<Player>())) || (collision.CompareTag("Player") && modularPiece.level < GeneralManager.singleton.FindNetworkAbilityLevel("Burglar", collision.GetComponent<Player>().name)))
            {
                if (playerInside.Contains(collision))
                {
                    playerInside.Remove(collision);
                    if (up)
                    {
                        modularPiece.playerInsideUpDoor = playerInside.Count;
                    }
                    if (down)
                    {
                        modularPiece.playerInsideDownDoor = playerInside.Count;
                    }
                    if (left)
                    {
                        modularPiece.playerInsideLeftDoor = playerInside.Count;
                    }
                    if (right)
                    {
                        modularPiece.playerInsideRightDoor = playerInside.Count;
                    }
                }
            }
        }
    }
}
