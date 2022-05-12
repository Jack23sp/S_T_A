using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIItemMallButton : MonoBehaviour
{
    public static UIItemMallButton singleton;
    public Button openItemMall;

    void Start()
    {
        if (!singleton) singleton = this;
        openItemMall.onClick.AddListener(() =>
        {
            if (!GeneralManager.singleton.uiItemMallPanel)
            {
                GeneralManager.singleton.uiItemMallPanel = Instantiate(GeneralManager.singleton.itemMallPanel, GeneralManager.singleton.canvas);
                Player.localPlayer.CmdTraceShopQuest();
            }
        });
    }

}
