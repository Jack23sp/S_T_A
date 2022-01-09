using Mirror;
using System;

[Serializable]
public struct DailyRewardsStruct
{
    public int day;
    public bool get;

    // constructors
    public DailyRewardsStruct(int _day, bool _get)
    {
        this.day = _day;
        this.get = _get;
    }
}

public class SyncListDailyRewards : SyncList<DailyRewardsStruct> { }