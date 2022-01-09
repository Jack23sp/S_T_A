using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class ButtonAudioPlayer : MonoBehaviour
{
    [HideInInspector] public AudioSource audioSource;
    public int audioCategory = 0;
    [HideInInspector]public  Button button;

    public bool manualCalled = false;

    public void Update()
    {
        if (!manualCalled)
        {
            if (!button) button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                if (!audioSource) audioSource = GetComponent<AudioSource>();
                SoundManager.singleton.PlaySound(audioSource, audioCategory);
            });
        }
    }

    public void PlaySound()
    {
        if (!button) button = GetComponent<Button>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        SoundManager.singleton.PlaySound(audioSource, audioCategory);
    }
}
