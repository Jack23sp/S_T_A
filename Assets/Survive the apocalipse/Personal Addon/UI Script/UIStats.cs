using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIStats : MonoBehaviour
{
    public static UIStats singleton;
    public Image hungryImage;
    public TextMeshProUGUI hungryText;

    public Image thirstImage;
    public TextMeshProUGUI thirstText;

    public Image weightImage;
    public TextMeshProUGUI weightText;

    public Image coinImage;
    public TextMeshProUGUI coinText;

    public Image goldImage;
    public TextMeshProUGUI goldText;

    public Button goldButton;
    public Button gemsButton;

    private Player player;

    private GameObject itemMallObject;

    public GeneralManager generalManager;

    void Start()
    {
        if (!singleton) singleton = this;
        SyncCurrency();
        hungryImage.sprite = generalManager.hungry;
        thirstImage.sprite = generalManager.thirsty;
        weightImage.sprite = generalManager.temperatureImage;
        coinImage.sprite = generalManager.coinsImage;
        goldImage.sprite = generalManager.goldImage;

        goldButton.onClick.AddListener(() =>
        {
            OpenItemMall();
        });

        gemsButton.onClick.AddListener(() =>
        {
            OpenItemMall();
        });
    }

    public void SyncCurrency()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        transform.GetChild(0).gameObject.SetActive(player);
        goldText.text = player.gold.ToString();
        coinText.text = player.coins.ToString();

        if (player.equipment[7].amount > 0)
        {
            weightImage.sprite = player.equipment[7].item.image;
        }
        else
        {
            weightImage.sprite = generalManager.temperatureImage;
        }

        hungryText.text = string.Concat(player.playerHungry.currentHungry, " / ", player.playerHungry.maxHungry);
        thirstText.text = string.Concat(player.playerThirsty.currentThirsty, " / ", player.playerThirsty.maxThirsty);
        weightText.text = string.Concat(player.playerWeight.currentWeight, " / ", player.playerWeight.maxWeight);
    }

    public void OpenItemMall()
    {
        itemMallObject = Instantiate(generalManager.itemMallPanel, generalManager.canvas);
    }
}
