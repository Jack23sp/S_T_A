using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public Animator animator;

    public ModularPiece modularPiece;

    public bool up, down, left, right;

    public NavMeshObstacle2D navMeshObstacle2D;

    public void Update()
    {
        if (up)
        {
            animator.SetBool("OPEN", modularPiece.doorUpOpen == true);
            if (navMeshObstacle2D)
                navMeshObstacle2D.enabled = modularPiece.doorUpOpen == false;
        }
        if (down)
        {
            animator.SetBool("OPEN", modularPiece.doorDownOpen == true);
            if (navMeshObstacle2D)
                navMeshObstacle2D.enabled = modularPiece.doorDownOpen == false;
        }
        if (left)
        {
            animator.SetBool("OPEN", modularPiece.doorLeftOpen == true);
            if (navMeshObstacle2D)
                navMeshObstacle2D.enabled = modularPiece.doorLeftOpen == false;
        }
        if (right)
        {
            animator.SetBool("OPEN", modularPiece.doorRightOpen == true);
            if (navMeshObstacle2D)
                navMeshObstacle2D.enabled = modularPiece.doorRightOpen == false;
        }
    }
}
