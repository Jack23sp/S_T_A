using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StreetLamp : NetworkBehaviourNonAlloc
{
    public TemperatureManager temperatureManager;
    public GameObject light2D;
    public Entity streetLamp;

    void Start()
    {
        if (!temperatureManager) temperatureManager = FindObjectOfType<TemperatureManager>();
        InvokeRepeating(nameof(CheckLight), 0.0f, 3.0f);
    }



    public void CheckLight()
    {
        if (temperatureManager)
        {
            if (temperatureManager.hours >= 17 || temperatureManager.hours <= 5)
            {
                light2D.SetActive(true);
            }
            else
            {
                light2D.SetActive(false);
            }
        }
        else
        {
            temperatureManager = FindObjectOfType<TemperatureManager>();
        }
    }
}
