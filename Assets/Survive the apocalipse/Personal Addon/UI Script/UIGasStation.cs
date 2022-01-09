using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class UIGasStation : MonoBehaviour
{
    private Player player;
    public static UIGasStation singleton;

    public Button closeButton;

    public Slider slider;
    public TextMeshProUGUI currentGasoline;
    public TextMeshProUGUI maxGasoline;

    public TextMeshProUGUI selectedGasoline;
    public TextMeshProUGUI inventoryGasoline;

    public Button doAction;

    public GasStation gasStation;
    // Start is called before the first frame update
    void Start()
    {
        if (!singleton) singleton = this;

        closeButton.onClick.SetListener(() =>
        {
            Destroy(singleton.gameObject);
        });
    }

    // Update is called once per frame
    void Update()
    {


        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.health == 0)
            closeButton.onClick.Invoke();

        if (!player.target) return;
        if (!gasStation) gasStation = player.target.GetComponent<GasStation>();
        if (!gasStation) return;

        slider.minValue = 0;
        slider.maxValue = gasStation.currentGasoline;
        maxGasoline.text = gasStation.currentGasoline.ToString();

        currentGasoline.text = ((int)slider.value).ToString();

        selectedGasoline.text = "Selected Gasoline : " + ((int)slider.value).ToString();

        if (doAction.GetComponentInChildren<TextMeshProUGUI>().text == "Get")
        {
            if (player.playerCar.GetEmptyGasolineBootle() > GeneralManager.singleton.maxGasStationGasoline - (GeneralManager.singleton.maxGasStationGasoline - gasStation.currentGasoline))
            {
                inventoryGasoline.text = "You can withdraw a maximum of : " + (GeneralManager.singleton.maxGasStationGasoline - (GeneralManager.singleton.maxGasStationGasoline - gasStation.currentGasoline)).ToString();
            }
            else
            {
                inventoryGasoline.text = "You can withdraw a maximum of : " + player.playerCar.GetEmptyGasolineBootle().ToString();
            }
        }
        else if (doAction.GetComponentInChildren<TextMeshProUGUI>().text == "Put")
        {
            if (player.playerCar.GetGasolineInINventory() > GeneralManager.singleton.maxGasStationGasoline - gasStation.currentGasoline)
            {
                inventoryGasoline.text = "You can deposit a maximum of : " + (GeneralManager.singleton.maxGasStationGasoline - gasStation.currentGasoline).ToString();
            }
            else
            {
                inventoryGasoline.text = "You can deposit a maximum of : " + player.playerCar.GetGasolineInINventory().ToString();
            }
        }
        else
        {
            doAction.interactable = false;
        }

        doAction.onClick.SetListener(() =>
        {
            if (doAction.GetComponentInChildren<TextMeshProUGUI>().text == "Get")
            {
                player.CmdGetGasolineFromStation(((int)slider.value));
            }

        });
    }
}
