using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIElectricBox : MonoBehaviour
{
    public TextMeshProUGUI description;
    public Button closeButton;
    public Button button;
    public TextMeshProUGUI buttonText;

    private Player player;

    public ModularPiece modularPiece;

    void Start()
    {
        player = Player.localPlayer;
    }

    void Update()
    {
        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

        if (modularPiece)
        {
            if (GeneralManager.singleton.CanDoOtherActionFloor(modularPiece, player))
            {
                if (modularPiece.level < 50 && modularPiece.level < GeneralManager.singleton.FindNetworkAbilityLevel("Burglar", player.name))
                {
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        description.text = "Vuoi alzare il livello di questo edificio?";
                        buttonText.text = "Aumenta!";
                    }
                    else if (GeneralManager.singleton.languagesManager.defaultLanguages == "English")
                    {
                        description.text = "Do you want to raise the level of this building?";
                        buttonText.text = "Upgrade!";
                    }
                    button.interactable = true;
                    button.onClick.SetListener(() =>
                    {
                        player.CmdRaiseBuildingLevel(player.name);
                    });
                }
                else
                {
                    if (modularPiece.level == 50)
                    {
                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                        {
                            description.text = "Hai raggiunto il livello massimo di questo edificio";
                            buttonText.text = "Congratulazioni!";
                        }
                        else if (GeneralManager.singleton.languagesManager.defaultLanguages == "English")
                        {
                            description.text = "You reach the maximimum level of this building";
                            buttonText.text = "Congratulation!";
                        }
                    }
                    else
                    {
                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                        {
                            description.text = "Alza la tua abilita' Burglar per compiere azioni su questo edificio";
                            buttonText.text = "";
                        }
                        else if (GeneralManager.singleton.languagesManager.defaultLanguages == "English")
                        {
                            description.text = "Upgrade you Burglar ability to take action on this building";
                            buttonText.text = "";
                        }
                    }
                    button.interactable = false;
                }
            }
            else
            {
                if (modularPiece.level < GeneralManager.singleton.FindNetworkAbilityLevel("Burglar", player.name))
                {
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        description.text = "Vuoi prendere possesso di questo edificio?";
                        buttonText.text = "Conferma!";
                    }
                    else if (GeneralManager.singleton.languagesManager.defaultLanguages == "English")
                    {
                        description.text = "Do you want claim this building?";
                        buttonText.text = "Confirm!";
                    }
                    button.onClick.SetListener(() =>
                    {
                        player.CmdClaimPlayerBuildingOwner(player.name, modularPiece.netIdentity);
                    });
                    button.interactable = true;
                }
                else
                {
                    if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                    {
                        description.text = "Alza la tua abilita' Burglar per compiere azioni su questo edificio";
                        buttonText.text = "";
                    }
                    else if (GeneralManager.singleton.languagesManager.defaultLanguages == "English")
                    {
                        description.text = "Upgrade you Burglar ability to take action on this building";
                        buttonText.text = "";
                    }
                }
            }
        }
    }
}
