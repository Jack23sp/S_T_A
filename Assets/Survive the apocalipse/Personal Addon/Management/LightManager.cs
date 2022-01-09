using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class LightManager : MonoBehaviour
{
    public static LightManager singleton;
    public Light2D globalLight;
    public Player player;
    public Color color;
    // Start is called before the first frame update
    void Start()
    {
        if (!singleton) singleton = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (!player) player = Player.localPlayer;
        
        if(player)
        {
            if(player.playerPremiumZoneManager.inPremiumZone)
            {
                Color c = globalLight.color;
                if (ColorUtility.TryParseHtmlString("#FFFFFF", out color))
                {
                    c = color;
                    globalLight.color = c;
                }
            }
            else
            {
                Color c = globalLight.color;
                if (ColorUtility.TryParseHtmlString("#" + TemperatureManager.singleton.colorSync, out color))
                {
                    c = color;
                    globalLight.color = c;
                }
            }           
        }
        else
        {
            Color c = globalLight.color;
            if (ColorUtility.TryParseHtmlString("#FFFFFF", out color))
            {
                c = color;
                globalLight.color = c;
            }
        }
    }
}
