using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIKitchenSink : MonoBehaviour
{
    public Slider waterSlider;
    public TextMeshProUGUI description;
    public TextMeshProUGUI selected;
    public TextMeshProUGUI minWater;
    public TextMeshProUGUI maxWater;
    public Button buttonWater;

    public Button closeButton;

    public KitchenSink kitchenSink;

    private Player player;

    void Update()
    {
        closeButton.onClick.SetListener(() =>
        {
            player.CmdRemoveForniture();
            Destroy(this.gameObject);
        });

        buttonWater.onClick.SetListener(() =>
        {
            player.CmdTakeWaterFromFurniture(Convert.ToInt32(waterSlider.value));
        });


        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.health == 0)
            closeButton.onClick.Invoke();

        if (!kitchenSink) kitchenSink = player.playerMove.forniture.GetComponent<KitchenSink>();
        if (!kitchenSink) return;

        description.text = string.Empty;

        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
        {
            description.text += "Acqua attuale : " + kitchenSink.currentWater + " / " + kitchenSink.maxWater;
            description.text += "\nPuoi prendere un massimo di : " + player.GetEmptyWaterBootle();
            selected.text = "Acqua selezionata : " + Convert.ToInt32(waterSlider.value);
            minWater.text = "0";
        }
        else
        {
            description.text += "Actual water : " + kitchenSink.currentWater + " / " + kitchenSink.maxWater;
            description.text += "\nYou can withdraw a maximum of : " + player.GetEmptyWaterBootle();
            selected.text = "Selected water : " + Convert.ToInt32(waterSlider.value);
            minWater.text = "0";
        }
        maxWater.text = kitchenSink.currentWater + " / " + kitchenSink.maxWater.ToString();
        waterSlider.minValue = 0;
        waterSlider.maxValue = kitchenSink.maxWater;

        buttonWater.interactable = waterSlider.value <= player.GetEmptyWaterBootle() && waterSlider.value <= kitchenSink.currentWater;
    }

    public float waterPercent()
    {
        return (kitchenSink.currentWater != 0 && kitchenSink.maxWater != 0) ? (float)kitchenSink.currentWater / (float)kitchenSink.maxWater : 0;
    }
}
