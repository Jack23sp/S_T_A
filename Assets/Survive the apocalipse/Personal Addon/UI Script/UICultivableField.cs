using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICultivableField : MonoBehaviour
{
    public static UICultivableField singleton;
    public GameObject objectToSpawn;
    public Transform content;
    public Button closeButton;

    private Player player;
    private CultivableField cultivableField;

    // Start is called before the first frame update
    void Start()
    {
        if (!singleton) singleton = this;
    }

    // Update is called once per frame
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
        if (!player.target.GetComponent<CultivableField>()) return;
        if (!cultivableField) cultivableField = player.target.GetComponent<CultivableField>();


        UIUtils.BalancePrefabs(objectToSpawn, cultivableField.currentPlant.Count, content);
        for (int i = 0; i < cultivableField.currentPlant.Count; i++)
        {
            int index = i;
            PlantListSlot slot = content.GetChild(index).GetComponent<PlantListSlot>();
            if (cultivableField.currentPlant[index].plantName == string.Empty || cultivableField.currentPlant[index].plantName == "Undefined")
            {
                slot.plantImage.gameObject.SetActive(false);
                slot.plantButton.interactable = true; // check if has item to plant for this season
                slot.plantButton.onClick.SetListener(() =>
                {
                    // plant panel here
                    GameObject g = Instantiate(GeneralManager.singleton.plantSelectorPanel, GeneralManager.singleton.canvas);
                    g.GetComponent<UIPlantSelection>().selectPlant = index;

                });
                if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                {
                    slot.plantButtonText.text = "Pianta!";
                }
                else
                {
                    slot.plantButtonText.text = "Plant!";
                }
                slot.plantText.text = string.Empty;
            }
            else
            {
                slot.plantImage.gameObject.SetActive(true);
                if (cultivableField.currentPlant[index].plantName == "Undefined") continue;
                if (ScriptablePlant.dict.TryGetValue(cultivableField.currentPlant[index].plantName.GetStableHashCode(), out ScriptablePlant plant))
                {
                    if (cultivableField.currentPlant[index].alreadyGrown)
                    {
                        if (cultivableField.currentPlant[index].grownQuantityX < plant.scaleDimension.x || cultivableField.currentPlant[index].grownQuantityY < plant.scaleDimension.y)
                        {
                            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            {
                                slot.plantButtonText.text = "Appassimento!";
                            }
                            else
                            {
                                slot.plantButtonText.text = "Whitering!";
                            }
                        }
                        else
                        {
                            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                            {
                                slot.plantButtonText.text = "Prendi!";
                            }
                            else
                            {
                                slot.plantButtonText.text = "Take it!";
                            }
                        }
                        slot.plantButton.onClick.SetListener(() =>
                        {
                            Player.localPlayer.CmdTakePlant(index);
                        });
                    }
                    else
                    {
                        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                        {
                            slot.plantButtonText.text = "Crescendo!";
                        }
                        else
                        {
                            slot.plantButtonText.text = "Growing!";
                        }
                    }

                    slot.plantImage.sprite = plant.image;
                    slot.plantText.text = plant.name;
                }
            }
        }
    }
}
