using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrinkAndEatManager : MonoBehaviour
{
    public Player player;
    public AudioSource eatDrinkSource;

    public void AbleEat()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        for (int i = 0; i < player.animators.Count; i++)
        {
            player.animators[i].SetBool("EAT", true);
        }
    }
    public void DisableEat()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        for (int i = 0; i < player.animators.Count; i++)
        {
            player.animators[i].SetBool("EAT", false);
        }
    }

    public void AbleDrink()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        for (int i = 0; i < player.animators.Count; i++)
        {
            player.animators[i].SetBool("DRINK", true);
        }
    }
    public void DisableDrink()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        for (int i = 0; i < player.animators.Count; i++)
        {
            player.animators[i].SetBool("DRINK", false);
        }
    }

    public void PlayDrinkSound()
    {
        eatDrinkSource.clip = SoundManager.singleton.drinkSound;
        eatDrinkSource.Play();
    }

    public void PlayEatSound()
    {
        eatDrinkSource.clip = SoundManager.singleton.eatSound;
        eatDrinkSource.Play();
    }
}
