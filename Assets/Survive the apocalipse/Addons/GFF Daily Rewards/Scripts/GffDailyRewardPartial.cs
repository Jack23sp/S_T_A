using Mirror;
using System;
using UnityEngine;
using UnityEngine.UI;

public partial class UIShortcuts
{
    [Header("GFF Daily Rewards")]
    public GameObject panelDailyRewards;
    public Button buttonDailyRewards;

    public void Update_DailyRewards()
    {
        if (panelDailyRewards != null && buttonDailyRewards != null)
        {
            buttonDailyRewards.onClick.SetListener(() => {
                //Player player = Player.localPlayer;
                //player.PlaySoundOpenPanel();
                panelDailyRewards.SetActive(!panelDailyRewards.activeSelf);
            });
        }
    }
}

public partial class Player
{
    [Header("DailyRewards")]
    public SyncListDailyRewards dailyRewards = new SyncListDailyRewards();

    void OnStartServer_DailyReward()
    {
        if (FindDayInList(DateTime.Now.Day) == -1)
        {
            //to receive a reward need to spend time in the game
            if (GffDaily.singleton.useTimeSpentInTheGame) Invoke("DailyRewardComplete", GffDaily.singleton.timeInGame);
            else DailyRewardComplete();
        }
    }

    void DailyRewardComplete()
    {
        DailyRewardsStruct row = new DailyRewardsStruct();
        row.day = DateTime.Now.Day;
        row.get = false;
        dailyRewards.Add(row);

        if (GffDaily.singleton.autoOpenRewardsPanel) RpcDailyRewardComplete(DateTime.Now.Day);
    }

    [ClientRpc]
    void RpcDailyRewardComplete(int day)
    {
        GffDaily.singleton.AutoOpenRewardPanel(this, day);
    }

    public int FindDayInList(int day)
    {
        for (int i = 0; i < dailyRewards.Count; i++)
        {
            if (dailyRewards[i].day == day) return i;
        }
        return -1;
    }

    [Command]
    public void CmdUpdateDailyReward(int value, bool state)
    {
        DailyRewardsStruct go = dailyRewards[value];
        go.get = state;
        dailyRewards[value] = go;
    }
}
