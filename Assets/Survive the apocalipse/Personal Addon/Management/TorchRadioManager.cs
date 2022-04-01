using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TorchRadioManager : MonoBehaviour
{
    public Player player;

    public GameObject torchObject;
    public GameObject radioObject;

    public Image torchImage;
    public Image radioImage;

    public TextMeshProUGUI torchText;
    public TextMeshProUGUI radioText;

    public Button torchButton;
    public Button radioButton;

    // Start is called before the first frame update
    void Start()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        radioButton.onClick.SetListener(() =>
        {
            player.playerRadio.CmdSetRadio();
        });

        torchButton.onClick.SetListener(() =>
        {
            player.playerTorch.CmdSetTorch();
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        if(player.playerRadio.radioItem.amount > 0)
        {
            radioObject.SetActive(true);
            radioImage.sprite = player.playerRadio.radioItem.item.data.image;
            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                radioText.text = player.playerRadio.radioItem.item.radioCurrentBattery + " / " + ((ScriptableRadio)player.playerRadio.radioItem.item.data).currentBattery.Get(player.playerRadio.radioItem.item.batteryLevel) + "\nStato : ";
                radioText.text += player.playerRadio.isOn ? "ON" : "OFF";
            }
            else
            {
                radioText.text = player.playerRadio.radioItem.item.radioCurrentBattery + " / " + ((ScriptableRadio)player.playerRadio.radioItem.item.data).currentBattery.Get(player.playerRadio.radioItem.item.batteryLevel) + "\nStatus : ";
                radioText.text += player.playerRadio.isOn ? "ON" : "OFF";
            }
        }
        else
        {
            radioObject.SetActive(false);
        }

        if (player.playerTorch.torchItem.amount > 0)
        {
            torchObject.SetActive(true);
            torchImage.sprite = player.playerTorch.torchItem.item.data.image;
            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                torchText.text = player.playerTorch.torchItem.item.torchCurrentBattery + " / " + ((ScriptableTorch)player.playerTorch.torchItem.item.data).currentBattery.Get(player.playerTorch.torchItem.item.batteryLevel) + "\nStato : ";
                torchText.text += player.playerTorch.isOn ? "ON" : "OFF";
            }
            else
            {
                torchText.text = player.playerTorch.torchItem.item.torchCurrentBattery + " / " + ((ScriptableTorch)player.playerTorch.torchItem.item.data).currentBattery.Get(player.playerTorch.torchItem.item.batteryLevel) + "\nStatus : ";
                torchText.text += player.playerTorch.isOn ? "ON" : "OFF";
            }
        }
        else
        {
            torchObject.SetActive(false);
        }
    }
}
