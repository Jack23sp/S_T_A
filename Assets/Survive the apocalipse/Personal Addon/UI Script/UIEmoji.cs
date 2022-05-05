using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIEmoji : MonoBehaviour
{
    public static UIEmoji singleton;
    public GameObject emojiToDisplay;
    public GameObject danceToDisplay;
    public Transform emojiContent;
    public GameObject emojiMainContent;
    public Transform danceContent;
    public GameObject danceMainContent;

    public GameObject PurchaseEmojiPanel;
    public GameObject PurchaseDancePanel;

    public Button buttonOpenPanel;
    public Animator animator;

    public Button emoji;
    public Button dance;

    public bool isOpen = false;

    public void Start()
    {
        if (!singleton) singleton = this;
        buttonOpenPanel.onClick.SetListener(() =>
        {
            animator.SetBool("OPEN", !animator.GetBool("OPEN"));
            isOpen = !isOpen;
            danceMainContent.gameObject.SetActive(false);
            emojiMainContent.gameObject.SetActive(true);
            ManageOpenPanel();
        });

        emoji.onClick.SetListener(() =>
        {
            danceMainContent.gameObject.SetActive(false);
            emojiMainContent.gameObject.SetActive(true);
            ManageOpenPanel();
        });
        dance.onClick.SetListener(() =>
        {
            danceMainContent.gameObject.SetActive(true);
            emojiMainContent.gameObject.SetActive(false);
            ManageOpenPanelDance();
        });

    }

    public void ManageOpenPanelDance()
    {
        UIUtils.BalancePrefabs(danceToDisplay, GeneralManager.singleton.listCompleteOfDance.Count, danceContent);
        for (int i = 0; i < danceContent.childCount; i++)
        {
            int index = i;
            DanceSlot slot = danceContent.GetChild(index).GetComponent<DanceSlot>();
            slot.openPurchaseDance.gameObject.SetActive(GeneralManager.singleton.FindNetworkDance(GeneralManager.singleton.listCompleteOfDance[index].name, Player.localPlayer.name) == -1);
            slot.padLock.gameObject.SetActive(GeneralManager.singleton.FindNetworkDance(GeneralManager.singleton.listCompleteOfDance[index].name, Player.localPlayer.name) == -1);

            if (Player.localPlayer.playerCreation.sex == 0)
                slot.image.sprite = GeneralManager.singleton.listCompleteOfDance[index].maleImage;
            else
                slot.image.sprite = GeneralManager.singleton.listCompleteOfDance[index].femaleImage;

            slot.spawnDance.onClick.SetListener(() =>
            {
                Player.localPlayer.playerDance.CmdSpawnDance(GeneralManager.singleton.listCompleteOfDance[index].name, Player.localPlayer.name, index);
            });

            slot.openPurchaseDance.onClick.SetListener(() =>
            {
                GameObject g = Instantiate(PurchaseDancePanel, GeneralManager.singleton.canvas);
                UIDancePurchase purchase = g.GetComponent<UIDancePurchase>();
                purchase.dance = GeneralManager.singleton.listCompleteOfDance[index];
                Player.localPlayer.playerDance.danceIndex = index;
                purchase.buyWithCoin.GetComponentInChildren<TextMeshProUGUI>().text = GeneralManager.singleton.listCompleteOfDance[index].coinToBuy.ToString();
                purchase.buyWithGold.GetComponentInChildren<TextMeshProUGUI>().text = GeneralManager.singleton.listCompleteOfDance[index].goldToBuy.ToString();
            });
        }
    }

    public void ManageOpenPanel()
    {
        UIUtils.BalancePrefabs(emojiToDisplay, GeneralManager.singleton.listCompleteOfEmoji.Count, emojiContent);
        for (int i = 0; i < emojiContent.childCount; i++)
        {
            int index = i;
            EmojiSlot slot = emojiContent.GetChild(index).GetComponent<EmojiSlot>();
            slot.openPurchaseEmoji.gameObject.SetActive(GeneralManager.singleton.FindNetworkEmoji(GeneralManager.singleton.listCompleteOfEmoji[index].name, Player.localPlayer.name) == -1);
            slot.padLock.gameObject.SetActive(GeneralManager.singleton.FindNetworkEmoji(GeneralManager.singleton.listCompleteOfEmoji[index].name, Player.localPlayer.name) == -1);
            slot.emojiImg.sprite = GeneralManager.singleton.listCompleteOfEmoji[index].emojiImg;
            slot.spawnEmoji.onClick.SetListener(() =>
            {
                    Player.localPlayer.playerEmoji.CmdSpawnEmoji(GeneralManager.singleton.listCompleteOfEmoji[index].name, Player.localPlayer.name);
            });

            slot.openPurchaseEmoji.onClick.SetListener(() =>
            {
                GameObject g = Instantiate(PurchaseEmojiPanel, GeneralManager.singleton.canvas);
                UIEmojiPurchase purchase = g.GetComponent<UIEmojiPurchase>();
                purchase.emoji = GeneralManager.singleton.listCompleteOfEmoji[index];
                purchase.buyWithCoin.GetComponentInChildren<TextMeshProUGUI>().text = GeneralManager.singleton.listCompleteOfEmoji[index].coinToBuy.ToString();
                purchase.buyWithGold.GetComponentInChildren<TextMeshProUGUI>().text = GeneralManager.singleton.listCompleteOfEmoji[index].goldToBuy.ToString();
            });
        }
    }
}
