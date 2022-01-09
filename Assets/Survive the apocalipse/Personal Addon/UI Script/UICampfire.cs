using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UICampfire : MonoBehaviour
{
    private Player player;
    public static UICampfire singleton;

    public Button closeButton;

    public Slider slider;
    public TextMeshProUGUI currentGasoline;
    public TextMeshProUGUI maxGasoline;

    public TextMeshProUGUI selectedGasoline;
    public TextMeshProUGUI inventoryGasoline;

    public TextMeshProUGUI currentWood;

    public Button doAction;
    public Button activeButton;

    public GameObject pieceSlot;

    public Transform content;
    public Button addGetPiece;

    public Campfire campfire;

    // Start is called before the first frame update
    void Start()
    {
        if (!singleton) singleton = this;

        InvokeRepeating(nameof(SetButton), 0.0f, 0.3f);
    }

    public void SetButton()
    {

        if (!player) player = Player.localPlayer;
        if (!player) return;

        CancelInvoke(nameof(SetButton));
    }

    void Update()
    {
        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });
        if (!player) player = Player.localPlayer;
        if (!player) return;
        if (!player.target) return;
        if (!(player.target.GetComponent<Campfire>())) return;

        campfire = (player.target.GetComponent<Campfire>());

        if (!campfire) closeButton.onClick.Invoke();

        if (player.health == 0)
            Destroy(this.gameObject);

        addGetPiece.onClick.SetListener(() =>
        {
            Instantiate(GeneralManager.singleton.cookedItemPanel, GeneralManager.singleton.canvas);
        });

        activeButton.interactable = campfire.currentWood > 0;
        activeButton.onClick.SetListener(() =>
        {
            player.CmdActiveCampfire();
        });

        doAction.onClick.SetListener(() =>
        {
            player.CmdPutWood(((int)slider.value));
        });


 
        if(GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
        {
            currentWood.text = "Legno attuale : " + campfire.currentWood.ToString();
            selectedGasoline.text = "Legno selezionato : " + ((int)slider.value).ToString();
            activeButton.GetComponentInChildren<TextMeshProUGUI>().text = campfire.active ? "Disattivo" : "Attivo";

            if (player.GetWoodInInventory() > GeneralManager.singleton.woodAmount.Get(player.target.level) - campfire.currentWood)
            {
                inventoryGasoline.text = "Puoi usare un massimo di : " + (GeneralManager.singleton.woodAmount.Get(player.target.level) - campfire.currentWood).ToString();
            }
            else
            {
                inventoryGasoline.text = "Puoi usare un massimo di : " + player.GetWoodInInventory().ToString();
            }
        }
        else
        {
            currentWood.text = "Current wood : " + campfire.currentWood.ToString();
            selectedGasoline.text = "Selected wood : " + ((int)slider.value).ToString();
            activeButton.GetComponentInChildren<TextMeshProUGUI>().text = campfire.active ? "Deactive" : "Active";

            if (player.GetWoodInInventory() > GeneralManager.singleton.woodAmount.Get(player.target.level) - campfire.currentWood)
            {
                inventoryGasoline.text = "You can use a maximum of : " + (GeneralManager.singleton.woodAmount.Get(player.target.level) - campfire.currentWood).ToString();
            }
            else
            {
                inventoryGasoline.text = "You can use a maximum of : " + player.GetWoodInInventory().ToString();
            }
        }

        slider.minValue = 0;
        slider.maxValue = GeneralManager.singleton.woodAmount.Get(player.target.level);
        maxGasoline.text = GeneralManager.singleton.woodAmount.Get(player.target.level).ToString();

        currentGasoline.text = ((int)slider.value).ToString();

        UIUtils.BalancePrefabs(pieceSlot, campfire.items.Count, content);
        for (int i = 0; i < campfire.items.Count; i++)
        {
            int index = i;
            PieceSlot slot = content.GetChild(index).GetComponent<PieceSlot>();
            if (campfire.items[index].amount == 0)
            {
                Destroy(slot.gameObject);
                continue;
            }
            slot.pieceImage.sprite = campfire.items[index].item.image;
            slot.name.text = campfire.items[index].item.name;
            slot.selectButton.interactable = campfire.items[index].item.cookCountdown <= 0;
            slot.selectButton.onClick.SetListener(() =>
            {
                player.CmdTakeCampfireItems(index);
            });
        }
    }
}
