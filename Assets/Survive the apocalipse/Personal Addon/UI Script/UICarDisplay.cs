using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICarDisplay : MonoBehaviour
{
    public Text passengers;
    public GameObject passengerObject;

    void Update()
    {
        passengerObject.SetActive(Player.localPlayer && Player.localPlayer.playerCar._car);

        if (passengerObject.activeInHierarchy && Player.localPlayer && Player.localPlayer.playerCar._car)
        {
            passengers.text = string.Empty;
            passengers.text += "*  " + Player.localPlayer.playerCar.car._pilot + "\n";
            passengers.text += "*  " + Player.localPlayer.playerCar.car._coPilot + "\n";
            passengers.text += "*  " + Player.localPlayer.playerCar.car._rearSxPassenger + "\n";
            passengers.text += "*  " + Player.localPlayer.playerCar.car._rearCenterPassenger + "\n";
            passengers.text += "*  " + Player.localPlayer.playerCar.car._rearDxPassenger;
        }
    }
}
