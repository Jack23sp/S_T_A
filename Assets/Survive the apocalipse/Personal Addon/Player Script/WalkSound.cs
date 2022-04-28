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
    public AudioClip sneakSoundHome;
    public AudioClip walkSoundHome;
    public AudioClip runSoundHome;

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
                if (!ModularBuildingManager.singleton.inThisCollider)
                {
                    if (player.playerMove.sneak)
                    {
                        audioSource.volume = 0.05f;
                        audioSource.clip = sneakSound;
                        audioSource.PlayOneShot(audioSource.clip);
                    }
                    else if (!player.playerMove.sneak && !player.playerMove.run)
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
                else
                {
                    if (player.playerMove.sneak)
                    {
                        audioSource.volume = 0.05f;
                        audioSource.clip = sneakSoundHome;
                        audioSource.PlayOneShot(audioSource.clip);
                    }
                    else if (!player.playerMove.sneak && !player.playerMove.run)
                    {
                        audioSource.volume = 0.05f;
                        audioSource.clip = walkSoundHome;
                        audioSource.PlayOneShot(audioSource.clip);
                    }
                    else if (player.playerMove.run)
                    {
                        audioSource.volume = 0.05f;
                        audioSource.clip = runSoundHome;
                        audioSource.PlayOneShot(audioSource.clip);
                    }
                }
            }
        }
    }
}
