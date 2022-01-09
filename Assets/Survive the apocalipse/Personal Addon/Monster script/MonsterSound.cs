using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSound : MonoBehaviour
{
    public Monster monster;
    public AudioSource audioSource;

    public string prevState = string.Empty;
    public int moveSound = -1;

    public string state;
    // Start is called before the first frame update
    void Start()
    {
        audioSource.volume = 0.0f;
        audioSource.Play();
        audioSource.loop = true;
        moveSound = UnityEngine.Random.Range(0, GeneralManager.singleton.moveSound.Count);
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.localPlayer)
        {
            state = monster.state;
            if (prevState != state)
            {
                if (!Player.localPlayer.playerOptions.blockSound)
                {
                    if (state == "IDLE")
                    {
                        audioSource.Stop();
                        audioSource.clip = GeneralManager.singleton.idleSound;
                        audioSource.volume = 0.4f;
                        audioSource.pitch = 0.7f;
                        audioSource.Play();
                    }
                    else if (state == "MOVING")
                    {
                        audioSource.Stop();
                        audioSource.clip = GeneralManager.singleton.moveSound[moveSound];
                        audioSource.volume = 0.4f;
                        audioSource.pitch = 0.7f;
                        audioSource.Play();
                    }
                    else if (state == "DEAD")
                    {
                        audioSource.Stop();
                        audioSource.loop = false;
                        audioSource.clip = GeneralManager.singleton.deathSound;
                        audioSource.volume = 0.4f;
                        audioSource.pitch = 0.7f;
                        audioSource.Play();
                    }
                }
                prevState = state;
            }
        }
    }
}
