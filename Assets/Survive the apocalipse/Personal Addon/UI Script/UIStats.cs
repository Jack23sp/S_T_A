using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIStats : MonoBehaviour
{
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

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating(nameof(SyncCurrency), 1.0f, 1.0f);
        hungryImage.sprite = generalManager.hungry;
        thirstImage.sprite = generalManager.thirsty;
        weightImage.sprite = generalManager.temperatureImage;
        coinImage.sprite = generalManager.coinsImage;
        goldImage.sprite = generalManager.goldImage;
    }

    public void Update()
    {
        goldButton.onClick.SetListener(() =>
        {
            OpenItemMall();
        });

        gemsButton.onClick.SetListener(() =>
        {
            OpenItemMall();
        });
    }

    public void SyncCurrency()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player)
        {
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

            hungryText.text = string.Concat(PlayerValueCatcher.singleton.hungry, " / ", player.playerHungry.maxHungry);
            thirstText.text = string.Concat(PlayerValueCatcher.singleton.thirsty, " / ", player.playerThirsty.maxThirsty);
            weightText.text = string.Concat(player.playerWeight.currentWeight, " / ", player.playerWeight.maxWeight);

        }
    }

    public void OpenItemMall()
    {
        itemMallObject = Instantiate(generalManager.itemMallPanel, generalManager.canvas);
    }
}
