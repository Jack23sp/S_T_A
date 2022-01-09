using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomType;

public class SoundManager : MonoBehaviour
{
    public static SoundManager singleton;
    public TemperatureManager temperatureManager;
    public List<AudioClip> buttonToClick = new List<AudioClip>();
    public AudioListener audioListener;
    public List<AudioCategory> buttonAudioCategory = new List<AudioCategory>();

    public AudioSource ambientListener;
    public AudioSource weatherListener;

    public AudioClip nightClip;
    public AudioClip dayClip;
    public AudioClip rainyClip;

    public bool shoot;


    // Start is called before the first frame update
    void Start()
    {
        if (!singleton) singleton = this;
        InvokeRepeating(nameof(SetMusic), 0.0f, 1.5f);
        InvokeRepeating(nameof(SetWeatherMusic), 0.0f, 1.5f);
    }

    public void Update()
    {

    }


    public void PlaySound(AudioSource audioSource, int audioCategory)
    {
        if(Player.localPlayer && !Player.localPlayer.playerOptions.blockSound)
        {
            if(!Player.localPlayer.playerOptions.blockButtonSounds)
            {
                if (audioCategory < buttonAudioCategory.Count)
                {
                    audioSource.clip = buttonAudioCategory[audioCategory].AudioForThisCategory;
                    audioSource.Play();
                }
            }
        }
    }

    public void Shoot()
    {
        if(Player.localPlayer.mazzleFlashSpawner) Player.localPlayer.mazzleFlashSpawner.Able();

        QuietAmbientMusic();
    }

    public void QuietAmbientMusic()
    {
        CancelInvoke(nameof(AbleAmbientMusic));
        if (ambientListener.volume != 0.0f)
        {
            ambientListener.volume = Mathf.Lerp(ambientListener.volume, 0, 2.0f);
        }

        Invoke(nameof(AbleAmbientMusic), 2.0f);
    }

    public void AbleAmbientMusic()
    {
        ambientListener.volume = Mathf.Lerp(ambientListener.volume, 1, 3.0f);
    }

    public void DisableTotallyMusic()
    {
        ambientListener.volume = 0;
        weatherListener.volume = 0;
    }

    public void SetMusic()
    {        
        if (temperatureManager.nightMusic)
        {
            ambientListener.clip = nightClip;
            if (Player.localPlayer && !Player.localPlayer.playerOptions.blockSound)
            {
                if(!ambientListener.isPlaying)
                    ambientListener.Play();
            }
            else
            {
                ambientListener.Stop();
            }
        }
        else
        {
            ambientListener.clip = dayClip;
            if (Player.localPlayer && !Player.localPlayer.playerOptions.blockSound)
            {
                if (!ambientListener.isPlaying)
                    ambientListener.Play();
            }
            else
            {
                ambientListener.Stop();
            }
        }
    }

    public void SetWeatherMusic()
    {
        if(temperatureManager.isRainy)
        {
            if (weatherListener.volume != 1 && Player.localPlayer && !Player.localPlayer.playerOptions.blockSound)
            {
                weatherListener.volume = Mathf.Lerp(weatherListener.volume, 1, 1.5f);
                if (!weatherListener.isPlaying) weatherListener.Play();
            }
        }
        else
        {
            weatherListener.volume = Mathf.Lerp(weatherListener.volume, 0, 1.5f);
        }
    }

    public void SetAmbientVolume(float volume, float timeToChange)
    {
        ambientListener.volume = Mathf.Lerp(ambientListener.volume, volume, timeToChange);
    }

    public void SetWeatherVolume (float volume, float timeToChange)
    {
        weatherListener.volume = Mathf.Lerp(weatherListener.volume, volume, timeToChange);
    }
}
