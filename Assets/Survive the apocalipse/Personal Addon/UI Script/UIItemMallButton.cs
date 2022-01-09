using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIItemMallButton : MonoBehaviour
{
    public static UIItemMallButton singleton;
    public Button openItemMall;

    // Start is called before the first frame update
    void Start()
    {
        if (!singleton) singleton = this;      
    }

    // Update is called once per frame
    void Update()
    {
        openItemMall.onClick.SetListener(() =>
        {
            if (!GeneralManager.singleton.uiItemMallPanel)
            {
                GeneralManager.singleton.uiItemMallPanel = Instantiate(GeneralManager.singleton.itemMallPanel, GeneralManager.singleton.canvas);
                Player.localPlayer.CmdTraceShopQuest();
            }
        });
    }
}
