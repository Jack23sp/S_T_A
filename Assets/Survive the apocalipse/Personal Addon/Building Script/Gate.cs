using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Gate : NetworkBehaviour
{
    public Entity entity;
    public GatePlayerChecker gatePlayerChecker;

    [SyncVar]
    public int playerInside = 0;

    public int prevPlayerInside = -1;

    public AudioSource audioSource;
    public AudioClip openClip;
    public AudioClip closeClip;

    // Update is called once per frame
    void Update()
    {
        if (prevPlayerInside != playerInside)
        {
            if (playerInside > 0)
            {
                entity.animator.SetBool("OPEN", true);
            }
            else
            {
                entity.animator.SetBool("OPEN", false);
            }

            if(playerInside > 0 && entity.animator.GetBool("OPEN") == true && prevPlayerInside <= 0)
            {
                PlayOpenSound();
            }
            else if (playerInside == 0 && entity.animator.GetBool("OPEN") == false && prevPlayerInside > 0)
            {
                PlayCloseSound();
            }
            prevPlayerInside = playerInside;
        }
    }

    public void PlayOpenSound()
    {
        audioSource.clip = openClip;
        audioSource.PlayOneShot(audioSource.clip);
    }

    public void PlayCloseSound()
    {
        audioSource.clip = closeClip;
        audioSource.PlayOneShot(audioSource.clip);
    }
}
