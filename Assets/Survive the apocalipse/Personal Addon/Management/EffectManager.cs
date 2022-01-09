using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public AudioClip rockEffect;
    public AudioClip treeEffect;
    public AudioClip buildingEffect;

    public AudioSource audioSource;
    
    // Start is called before the first frame update
    void Start()
    {
        if(Player.localPlayer.target is Rock)
        {
            audioSource.clip = rockEffect;
            audioSource.Play();
        }
        else if(Player.localPlayer.target is Tree)
        {
            audioSource.clip = treeEffect;
            audioSource.Play();
        }
        else if(Player.localPlayer.target is Building)
        {
            audioSource.clip = buildingEffect;
            audioSource.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
