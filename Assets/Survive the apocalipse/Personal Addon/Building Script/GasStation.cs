using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GasStation : NetworkBehaviour
{
    [SyncVar]
    public int currentGasoline;
    // Start is called before the first frame update
    void Start()
    {
        bool premium = GetComponent<Building>().isPremiumZone;
        if (isServer && !premium)
        {
            currentGasoline = GeneralManager.singleton.maxGasStationGasoline;
            InvokeRepeating("IncreaseGasoline", GeneralManager.singleton.intervalChargeGasStation, GeneralManager.singleton.intervalChargeGasStation);
        }
        if (isServer && premium)
        {
            currentGasoline = GeneralManager.singleton.maxGasStationGasoline;
            InvokeRepeating("IncreaseGasoline", GeneralManager.singleton.intervalChargeGasStation/2, GeneralManager.singleton.intervalChargeGasStation/2);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IncreaseGasoline()
    {
        currentGasoline += GeneralManager.singleton.chargeGasolineAmount;
        if (currentGasoline > GeneralManager.singleton.maxGasStationGasoline)
            currentGasoline = GeneralManager.singleton.maxGasStationGasoline;
    }
}
