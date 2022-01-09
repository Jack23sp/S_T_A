using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using CustomType;
using System;

public class BeeKeeper : NetworkBehaviour
{
    public SyncListBeeContainer beeContainers = new SyncListBeeContainer();
    public BeeKeeper beeKeeper;
    public Building building;
    // Start is called before the first frame update
    void Start()
    {
        building = GetComponent<Building>();
        if (isServer)
        {
            if (beeContainers.Count == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    beeContainers.Add(new BeeContainer());
                }
            }
            if (!beeKeeper) beeKeeper = GetComponent<BeeKeeper>();
            InvokeRepeating("IncreaseBee", 0.0f, GeneralManager.singleton.beeInvoke);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IncreaseBee()
    {
        float usableBeeContainer = 0.0f;
        int beetoIncrease = 0;
        usableBeeContainer = Convert.ToInt32((building.level / 10));
        if (building.level <= 10) usableBeeContainer = 1;


        for (int i = 0; i < usableBeeContainer; i++)
        {
            beetoIncrease = UnityEngine.Random.Range(0, GeneralManager.singleton.maxBeeForHours);
            int honeyToAdd = UnityEngine.Random.Range(0, 2);

            if (beeKeeper.beeContainers[i].totalBee < GeneralManager.singleton.maxBeeForContainer)
            {
                if (beeKeeper.beeContainers[i].totalHoney < GeneralManager.singleton.maxHoneyForContainer)
                {
                    if (honeyToAdd == 0)
                    {
                        BeeContainer beeContainer = beeKeeper.beeContainers[i];
                        beeContainer.totalBee += beetoIncrease;
                        beeContainer.totalHoney += UnityEngine.Random.Range(1, beeContainer.totalBee / 4);
                        beeKeeper.beeContainers[i] = beeContainer;
                    }
                    if (honeyToAdd == 1)
                    {
                        BeeContainer beeContainer = beeKeeper.beeContainers[i];
                        beeContainer.totalBee += beetoIncrease;
                        beeContainer.totalHoney += UnityEngine.Random.Range(1, beeContainer.totalBee / 2);
                        beeKeeper.beeContainers[i] = beeContainer;
                    }
                }
            }
            else
            {
                if (beeKeeper.beeContainers[i].totalHoney < GeneralManager.singleton.maxHoneyForContainer)
                {
                    if (honeyToAdd == 0)
                    {
                        BeeContainer beeContainer = beeKeeper.beeContainers[i];
                        beeContainer.totalHoney += UnityEngine.Random.Range(1, beeContainer.totalBee / 4);
                        beeKeeper.beeContainers[i] = beeContainer;
                    }
                    if (honeyToAdd == 1)
                    {
                        BeeContainer beeContainer = beeKeeper.beeContainers[i];
                        beeContainer.totalHoney += UnityEngine.Random.Range(1, beeContainer.totalBee / 2);
                        beeKeeper.beeContainers[i] = beeContainer;
                    }
                }
            }
        }

    }

}
