using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class SnowManager : NetworkBehaviour
{
    public static SnowManager singleton;

    public float maxSnowLimit = 0;

    public TemperatureManager temperatureManager;

    [SyncVar]
    public float progress = 1;

    public Material snowMaterial;

    public float previousSnowAmount = -1.0f;

    public float amountSnowToModify = 0.1f;
    public float spriteSnowAmountToModify = 0.01f;

    [SyncVar]
    public float spriteSnow = 0.0f;

    public float desiredColorToActivateFootPrint = 0.8f;

    public List<SpriteRenderer> snowSprites = new List<SpriteRenderer>();

    public float spriteSnowLimit = 0.27f;

    public Player player;

    void Start()
    {
        if (!singleton) singleton = this;
        if (isServer)
            InvokeRepeating(nameof(CheckSnow), 1.0f, 1.0f);

        for(int i = 0; i < snowSprites.Count;i++)
        {
            int index = i;
            snowSprites[index].color = Color.white;
            snowSprites[index].flipX = UnityEngine.Random.Range(0, 2) == 1 ? true : false;
            snowSprites[index].flipY = UnityEngine.Random.Range(0, 2) == 1 ? true : false;
            snowSprites[index].material = snowMaterial;
            snowSprites[index].sortingOrder = 32765;
        }

        InvokeRepeating(nameof(SnowOnClient), 1.0f, 1.0f);
    }

    void CheckSnow()
    {
        if (!temperatureManager) temperatureManager = TemperatureManager.singleton;

        if (temperatureManager.isSnowy)
        {
            if (progress < maxSnowLimit)
            {
                progress += amountSnowToModify;
            }
            if (spriteSnow < spriteSnowLimit)
                spriteSnow += spriteSnowAmountToModify;
        }
        else
        {
            if (progress > 0.1f)
            {
                progress -= amountSnowToModify;
            }

            if (spriteSnow > 0)
                spriteSnow -= spriteSnowAmountToModify;
        }
    }

    public void SnowOnClient()
    {
        if (Player.localPlayer)
        {
            if (previousSnowAmount != progress)
            {
                previousSnowAmount = progress;
                snowMaterial.SetFloat("_Alpha", progress);
            }
        }
        else
        {
            player = Player.localPlayer;
        }
    }
}
