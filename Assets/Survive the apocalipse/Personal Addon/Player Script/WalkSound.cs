using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WalkSound : MonoBehaviour
{
    public Player player;
    public AudioSource audioSource;


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
                        audioSource.volume = SoundManager.singleton.clipVolumeSneak;
                        audioSource.clip = SoundManager.singleton.sneakSound;
                        audioSource.PlayOneShot(audioSource.clip);
                    }
                    else if (!player.playerMove.sneak && !player.playerMove.run)
                    {
                        audioSource.volume = SoundManager.singleton.clipVolumeWalk;
                        audioSource.clip = SoundManager.singleton.walkSound;
                        audioSource.PlayOneShot(audioSource.clip);
                    }
                    else if (player.playerMove.run)
                    {
                        audioSource.volume = SoundManager.singleton.clipVolumeRun;
                        audioSource.clip = SoundManager.singleton.runSound;
                        audioSource.PlayOneShot(audioSource.clip);
                    }
                }
                else
                {
                    if (player.playerMove.sneak)
                    {
                        audioSource.volume = SoundManager.singleton.clipVolumeSneak;
                        audioSource.clip = SoundManager.singleton.sneakSoundHome;
                        audioSource.PlayOneShot(audioSource.clip);
                    }
                    else if (!player.playerMove.sneak && !player.playerMove.run)
                    {
                        audioSource.volume = SoundManager.singleton.clipVolumeWalk;
                        audioSource.clip = SoundManager.singleton.walkSoundHome;
                        audioSource.PlayOneShot(audioSource.clip);
                    }
                    else if (player.playerMove.run)
                    {
                        audioSource.volume = SoundManager.singleton.clipVolumeRun;
                        audioSource.clip = SoundManager.singleton.runSoundHome;
                        audioSource.PlayOneShot(audioSource.clip);
                    }
                }
            }
        }
    }
}
