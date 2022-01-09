using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class UILeaderboardButton : MonoBehaviour
{
    private Player player;
    public static UILeaderboardButton singleton;
    public Button leaderboardButton;


    // Start is called before the first frame update
    void Start()
    {
        if (!singleton) singleton = this;
        if (!leaderboardButton) leaderboardButton = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;


        leaderboardButton.onClick.SetListener(() =>
        {
            Instantiate(GeneralManager.singleton.leaderboardPanelToSpawn, GeneralManager.singleton.canvas);
        });  
    }
}
