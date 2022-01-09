using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEmojiPurchase : MonoBehaviour
{
    public ScriptableEmoji emoji;
    public Image emojiImage;
    public Button closeEmoji;
    public Button buyWithCoin;
    public Button buyWithGold;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (emoji) emojiImage.sprite = emoji.emojiImg;

        if (Player.localPlayer.health == 0)
            closeEmoji.onClick.Invoke();

        buyWithCoin.interactable = emoji && Player.localPlayer && Player.localPlayer.coins >= emoji.coinToBuy;
        buyWithGold.interactable = emoji && Player.localPlayer && Player.localPlayer.gold >= emoji.goldToBuy;
        closeEmoji.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });
        buyWithCoin.onClick.SetListener(() =>
        {
            Player.localPlayer.playerEmoji.CmdAddEmoji(emoji.name, 0);
            closeEmoji.onClick.Invoke();
        });
        buyWithGold.onClick.SetListener(() =>
        {
            Player.localPlayer.playerEmoji.CmdAddEmoji(emoji.name, 1);
            closeEmoji.onClick.Invoke();
        });

    }
}
