using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BuildingWaterWell : NetworkBehaviour
{
    [SyncVar]
    public int currentWater;
    public int maxWater;
    public Entity building;

    // Start is called before the first frame update
    void Start()
    {
        if (!building) building = GetComponent<Entity>();
        InvokeRepeating("TakeWater", 0.0f, GeneralManager.singleton.waterInvoke);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TakeWater()
    {
        maxWater = GeneralManager.singleton.levelWater.Get(building.level);
        if (currentWater < maxWater)
        {
            if (TemperatureManager.singleton.isRainy)
            {
                currentWater++;
            }
        }
    }
}
