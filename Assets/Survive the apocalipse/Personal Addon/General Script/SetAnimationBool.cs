using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAnimationBool : MonoBehaviour
{
    public Animator animator;

    public void SetOpenPanel()
    {
        animator.SetBool("OPEN",!animator.GetBool("OPEN")); 
    }
}
