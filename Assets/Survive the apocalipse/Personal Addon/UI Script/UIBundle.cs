using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBundle : MonoBehaviour
{
    public Transform content;
    public BundleSlot budleSlot;

    public BundleItem clothesItem;
    public List<BundleItem> allItems;

    public void Start()
    {
        if (Player.localPlayer)
        {
            if (Player.localPlayer.playerCreation.sex == 0)
                clothesItem = GeneralManager.singleton.manVerticalItem;
            else
                clothesItem = GeneralManager.singleton.womanVerticalItem;

            allItems = GeneralManager.singleton.allItems;
        }
    }
}
