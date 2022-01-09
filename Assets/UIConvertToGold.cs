using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIConvertToGold : MonoBehaviour
{
    public Text maxCoin;
    public Slider coinSlider;
    public Button changeButton;

    public Text coin;
    public Text gold;

    public Button closeButton;

    private Player player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        if(player.health == 0)
        {
            closeButton.onClick.Invoke();
        }

        maxCoin.text = player.coins.ToString();
        coinSlider.maxValue = player.coins;

        if(coinSlider.value > 0)
        {
            coin.text = "Coins : " + (player.coins - Convert.ToInt32(coinSlider.value)).ToString();
            gold.text = "Gold : " + (player.gold + (Convert.ToInt32(coinSlider.value) * 5)).ToString();
        }
        else
        {
            coin.text = "Coins : " + player.coins;
            gold.text = "Gold : " + player.gold;
        }

        changeButton.interactable = coinSlider.value > 0;
        changeButton.onClick.SetListener(() =>
        {
            player.CmdChangeCoinGold(Convert.ToInt32(coinSlider.value));
        });

        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

    }
}
