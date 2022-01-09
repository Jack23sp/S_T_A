using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WalkSound : MonoBehaviour
{
    public Player player;
    public AudioSource audioSource;
    public AudioClip sneakSound;
    public AudioClip walkSound;
    public AudioClip runSound;

    public void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void FootstepSound()
    {
        if (!player) player = Player.localPlayer;
        if (player)
        {
            if (!player.playerOptions.blockFootstep)
            {
                if (player.playerMove.sneak)
                {
                    audioSource.volume = 0.05f;
                    audioSource.clip = sneakSound;
                    audioSource.PlayOneShot(audioSource.clip);
                }
                else if(!player.playerMove.sneak && !player.playerMove.run)
                {
                    audioSource.volume = 0.05f;
                    audioSource.clip = walkSound;
                    audioSource.PlayOneShot(audioSource.clip);
                }
                else if (player.playerMove.run)
                {
                    audioSource.volume = 0.05f;
                    audioSource.clip = runSound;
                    audioSource.PlayOneShot(audioSource.clip);
                }
            }
        }
    }
}
