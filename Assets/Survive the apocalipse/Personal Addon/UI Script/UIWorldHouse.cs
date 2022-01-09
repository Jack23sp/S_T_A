using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWorldHouse : MonoBehaviour
{
    public static UIWorldHouse singleton;
    public GameObject objectToSpawn;
    public Transform content;
    public Button closeButton;

    private Player player;
    private WorldHouse target;

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

        if (!player) player = Player.localPlayer;
        if (!player) return;
        else
        {
            if(player.target && player.target.GetComponent<WorldHouse>())
            {
                target = player.target.GetComponent<WorldHouse>();
            }
        }

        if (player.health == 0)
            closeButton.onClick.Invoke();

        UIUtils.BalancePrefabs(objectToSpawn, target.items.Count, content);
        for(int i = 0; i < target.items.Count; i++)
        {
            int index = i;
            WorldHouseSlot slot = content.GetChild(index).GetComponent<WorldHouseSlot>();
            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                slot.nameText.text = target.items[index].item.data.italianName + " ( " + target.items[index].amount + " )";
            }
            else
            {
                slot.nameText.text = target.items[index].item.name + " ( " + target.items[index].amount + " )";

            }

            if (ScriptableItem.dict.TryGetValue(target.items[index].item.name.GetStableHashCode(), out ScriptableItem item))
            {
                int rarity = item.ItemRarity;
                string rarityText = string.Empty;
                switch (rarity)
                {
                    case 0:
                        rarityText = "Normal";
                        break;
                    case 1:
                        rarityText = "Rare";
                        break;
                    case 2:
                        rarityText = "Elite";
                        break;
                    case 3:
                        rarityText = "Epic";
                        break;
                    case 4:
                        rarityText = "Legendary";
                        break;

                }
                slot.Rarity.text = "Rarity : " + rarityText;
                slot.ItemImage.sprite = item.image;
                slot.takeButton.interactable = player.InventoryCanAdd(target.items[index].item, target.items[index].amount);
                slot.takeButton.onClick.SetListener(() =>
                {
                    player.CmdTakeProductFromHouse(index);
                });
            }
        }
    }
}
