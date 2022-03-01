using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWeaponStorage : MonoBehaviour
{
    public static UIWeaponStorage singleton;
    public List<UIInventorySlot> weapon = new List<UIInventorySlot>();

    public WeaponStorage weaponStorage;

    private Player player;

    public Button closeButton;

    void Start()
    {
        if (!singleton) singleton = this;
        player = Player.localPlayer;
        weaponStorage = player.playerMove.fornitureClient.GetComponent<WeaponStorage>();

        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });
    }

    void Update()
    {
        if (player.health == 0) closeButton.onClick.Invoke();

        for(int i = 0; i < weaponStorage.weapon.Count ; i++)
        {
            int index = i;
            UIInventorySlot slot = weapon[index];
            if (weaponStorage.weapon[index].amount > 0)
            {
                slot.gameObject.transform.parent.GetComponent<Image>().enabled = false;
                slot.gameObject.SetActive(true);
                slot.image.color = Color.white;
                slot.image.sprite = weaponStorage.weapon[index].item.image;
                slot.cooldownCircle.fillAmount = 0;
                slot.amountOverlay.SetActive(weaponStorage.weapon[index].amount > 1);
                slot.amountText.text = weaponStorage.weapon[index].amount.ToString();
                slot.button.onClick.SetListener(() =>
                {
                    player.CmdAddToInventoryFromWeaponStorage(index);
                });
            }
            else
            {
                slot.gameObject.transform.parent.GetComponent<Image>().enabled = true;
                slot.gameObject.SetActive(false);
            }

        }
    }
}
