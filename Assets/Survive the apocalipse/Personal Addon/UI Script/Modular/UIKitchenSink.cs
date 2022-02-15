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

    public Button drinkButton;
    public Button fillButton;

    public bool drink;

    private Player player;

    public int aquiferIndex;

    public void Start()
    {
        kitchenSink = Player.localPlayer.playerMove.fornitureClient.GetComponent<KitchenSink>();

        float distance = 10000000.0f;
        for(int i = 0; i < TemperatureManager.singleton.actualAcquifer.Count; i++)
        {
            int index = i;
            float actualDistance = Vector2.Distance(kitchenSink.transform.position, TemperatureManager.singleton.actualAcquifer[index].transform.position);
            if (actualDistance < distance)
            {
                distance = actualDistance;
                aquiferIndex = index;
            }
        }
    }

    void Update()
    {
        closeButton.onClick.SetListener(() =>
        {
            player.CmdRemoveForniture();
            Destroy(this.gameObject);
        });

        buttonWater.onClick.SetListener(() =>
        {
            if (drink)
            {
                if(player.playerThirsty.currentThirsty < player.playerThirsty.maxThirsty)
                    player.CmdDrink(aquiferIndex);
            }
            else
                player.CmdTakeWaterFromFurniture(Convert.ToInt32(waterSlider.value), aquiferIndex);
        });

        drinkButton.onClick.SetListener(() =>
        {
            drink = true;
            selected.enabled = minWater.enabled = maxWater.enabled = false;
            waterSlider.gameObject.SetActive(false);
        });
        fillButton.onClick.SetListener(() =>
        {
            drink = false;
            selected.enabled = minWater.enabled = maxWater.enabled = waterSlider.enabled = true;
            waterSlider.gameObject.SetActive(true);
        });

        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.health == 0)
            closeButton.onClick.Invoke();

        if (!kitchenSink) kitchenSink = player.playerMove.fornitureClient.GetComponent<KitchenSink>();
        if (!kitchenSink) return;

        description.text = string.Empty;

        if (!drink)
        {
            if (kitchenSink.maxWater == 0)
            {
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    description.text += "Devi aspettare che piova per ricaricare la falda acquifera";
                }
                else
                {
                    description.text += "You need to wait until rain to recharge aquifer";
                }
            }
            else
            {
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    description.text += "Acqua presente nella falda acquifera : " + TemperatureManager.singleton.actualAcquifer[aquiferIndex].actualWater + " / " + TemperatureManager.singleton.actualAcquifer[aquiferIndex].maxWater;
                    description.text += "\nPuoi prendere un massimo di : " + player.GetEmptyWaterBootle();
                    selected.text = "Acqua selezionata : " + Convert.ToInt32(waterSlider.value);
                    minWater.text = "0";
                }
                else
                {
                    description.text += "Actual water presnt in aquifer : " + TemperatureManager.singleton.actualAcquifer[aquiferIndex].actualWater + " / " + TemperatureManager.singleton.actualAcquifer[aquiferIndex].maxWater;
                    description.text += "\nYou can withdraw a maximum of : " + player.GetEmptyWaterBootle();
                    selected.text = "Selected water : " + Convert.ToInt32(waterSlider.value);
                    minWater.text = "0";
                }
            }
        }
        else
        {
            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                description.text += "Acqua presente nella falda acquifera : " + TemperatureManager.singleton.actualAcquifer[aquiferIndex].actualWater + " / " + TemperatureManager.singleton.actualAcquifer[aquiferIndex].maxWater;
            }
            else
            {
                description.text += "Actual water presnt in aquifer : " + TemperatureManager.singleton.actualAcquifer[aquiferIndex].actualWater + " / " + TemperatureManager.singleton.actualAcquifer[aquiferIndex].maxWater;
            }
        }
        maxWater.text = TemperatureManager.singleton.actualAcquifer[aquiferIndex].actualWater + " / " + TemperatureManager.singleton.actualAcquifer[aquiferIndex].maxWater.ToString();
        waterSlider.minValue = 0;
        waterSlider.maxValue = TemperatureManager.singleton.actualAcquifer[aquiferIndex].maxWater;

        buttonWater.interactable = waterSlider.value <= player.GetEmptyWaterBootle() && waterSlider.value <= TemperatureManager.singleton.actualAcquifer[aquiferIndex].actualWater;
    }

    public float waterPercent()
    {
        return (kitchenSink.currentWater != 0 && kitchenSink.maxWater != 0) ? (float)kitchenSink.currentWater / (float)kitchenSink.maxWater : 0;
    }
}
