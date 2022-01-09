using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPlantSelection : MonoBehaviour
{
    public static UIPlantSelection singleton;
    public GameObject objectToSpawn;
    public Transform content;
    public Button closeButton;

    public List<int> inventoryPlantIndex = new List<int>();
    public int selectPlant;

    void Start()
    {
        if (!singleton) singleton = this;
    }

    void Update()
    {
        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

        if (Player.localPlayer.health == 0)
            closeButton.onClick.Invoke();

        for(int i = 0; i < Player.localPlayer.inventory.Count; i++)
        {
            int index = i;
            if(Player.localPlayer.inventory[index].amount > 0)
            {
                if(Player.localPlayer.inventory[index].item.data is ScriptablePlant && ((ScriptablePlant)Player.localPlayer.inventory[index].item.data).GrowSeason == TemperatureManager.singleton.season)
                {
                    if(!inventoryPlantIndex.Contains(index))
                    {
                        inventoryPlantIndex.Add(index);
                    }
                }
            }
        }

        UIUtils.BalancePrefabs(objectToSpawn, inventoryPlantIndex.Count, content);
        for(int e = 0; e < inventoryPlantIndex.Count; e++)
        {
            int index = e;
            PlantListSlot slot = content.GetChild(index).GetComponent<PlantListSlot>();
            slot.plantImage.sprite = Player.localPlayer.inventory[inventoryPlantIndex[index]].item.image;
            slot.plantButton.onClick.SetListener(() =>
            {
                Player.localPlayer.CmdPlant(inventoryPlantIndex[index], selectPlant);
                closeButton.onClick.Invoke();
            });
            if(GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
                slot.plantButtonText.text = "Pianta!";
            else
                slot.plantButtonText.text = "Plant!";

            slot.plantText.text = Player.localPlayer.inventory[inventoryPlantIndex[index]].amount + " / " + Player.localPlayer.inventory[inventoryPlantIndex[index]].amount;

        }
    }
}
