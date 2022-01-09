using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using TMPro;

public class UITotem : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bodyMessage;
    public TMP_InputField inputFieldMessage;
    public Button setMessageButton;
    public Button closeButton;

    private Totem totem;
    private Player player;

    void Update()
    {
        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.health == 0)
            closeButton.onClick.Invoke();

        if (!player.target) return;
        if (!totem) totem = player.target.GetComponent<Totem>();
        if (!totem) return;


        setMessageButton.onClick.SetListener(() =>
        {
            player.CmdSetMessage(inputFieldMessage.text);
            inputFieldMessage.text = string.Empty;
        });

        if (totem.GetComponent<Building>().buildingName != string.Empty)
        {
            titleText.text = totem.GetComponent<Building>().buildingName;
        }
        else
        {
            titleText.text = totem.GetComponent<Building>().building.name;
        }

        if (totem.GetComponent<Building>().isPremiumZone)
        {
            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                bodyMessage.text = totem.defaultMessageIta;
            }
            else
            {
                bodyMessage.text = totem.defaultMessage;
            }
        }
        else
        {
            bodyMessage.text = totem.message;
        }

        setMessageButton.gameObject.SetActive(player.CanInteractBuildingTarget(totem.GetComponent<Building>(), player));
        inputFieldMessage.gameObject.SetActive(player.CanInteractBuildingTarget(totem.GetComponent<Building>(), player));
        setMessageButton.interactable = inputFieldMessage.text != string.Empty && inputFieldMessage.text != bodyMessage.text;
    }
}
