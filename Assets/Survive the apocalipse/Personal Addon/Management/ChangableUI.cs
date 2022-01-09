using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UIType { Panel, Party, PetSlot, HealthMana, BackgroundJoystick, FrontJoystick, ChatButton, ChatButtonSend, ChatBubble, Confirm, AbilitySlot, CustomItemMall, CustomItemMallPremium, PanelVertical, DeclineButton,CreateButton }
public class ChangableUI : MonoBehaviour
{
    public UIType desiredUI;
    public Image desiredImage;

    // Start is called before the first frame update
    void Awake()
    {
        ChangableUIManager changableUIManager = FindObjectOfType<ChangableUIManager>();
        if(GetComponent<Text>()) GetComponent<Text>().font = changableUIManager.fontStyle;
        if (GetComponentInChildren<Text>()) GetComponentInChildren<Text>().font = changableUIManager.fontStyle;

        if (desiredImage)
        {
            for (int i = 0; i < changableUIManager.change.Count; i++)
            {
                int index = i;
                if (changableUIManager.change[index].active)
                {
                    desiredImage.sprite = changableUIManager.change[index].changeUITypes[Convert.ToInt32(desiredUI)].spriteImg;
                    Destroy(this);
                }
            }
        }
    }
}
