using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDancePurchase : MonoBehaviour
{
    public ScriptableDance dance;
    public RawImage danceImage;
    public Button closeDance;
    public Button buyWithCoin;
    public Button buyWithGold;

    public GameObject weapon;
    public PlayerPlaceholderWeapon playerPlaceholderWeapon;

    // Start is called before the first frame update
    void Start()
    {
        
        if(Player.localPlayer.playerItemEquipment.firstWeapon.amount > 0)
        {
            Player.localPlayer.prevAnimator = ((WeaponItem)Player.localPlayer.playerItemEquipment.firstWeapon.item.data).animatorToSet;
        }
        else
        {
            Player.localPlayer.prevAnimator = GeneralManager.singleton.defaultAnimatorController;
        }

        for (int i = 0; i < Player.localPlayer.animators.Count; i++)
        {
            Player.localPlayer.animators[i].runtimeAnimatorController = dance.animator;
        }

        playerPlaceholderWeapon = Player.localPlayer.playerMove.bodyPlayer.GetComponent<PlayerPlaceholderWeapon>();

        for(int i = 0; i < playerPlaceholderWeapon.placeholderWeapon.Count; i++)
        {
            int index = i;
            if(playerPlaceholderWeapon.placeholderWeapon[index].childCount > 0)
            {
                weapon = playerPlaceholderWeapon.placeholderWeapon[index].GetChild(0).gameObject;
            }
        }
        if(weapon)
            weapon.SetActive(false);
    }

    void Update()
    {
        if (dance) danceImage.texture = Player.localPlayer.playerBody.texture;

        if (Player.localPlayer.health == 0)
            closeDance.onClick.Invoke();

        buyWithCoin.interactable = dance && Player.localPlayer && Player.localPlayer.coins >= dance.coinToBuy;
        buyWithGold.interactable = dance && Player.localPlayer && Player.localPlayer.gold >= dance.goldToBuy;

        closeDance.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
            for (int i = 0; i < Player.localPlayer.animators.Count; i++)
            {
                Player.localPlayer.animators[i].runtimeAnimatorController = Player.localPlayer.prevAnimator;
            }
            Player.localPlayer.playerDance.danceIndex = -1;
            if (weapon) weapon.SetActive(true);
        });

        buyWithCoin.onClick.SetListener(() =>
        {
            Player.localPlayer.playerDance.CmdAddDance(dance.name, 0);
            closeDance.onClick.Invoke();
        });

        buyWithGold.onClick.SetListener(() =>
        {
            Player.localPlayer.playerDance.CmdAddDance(dance.name, 1);
            closeDance.onClick.Invoke();
        });

    }
}
