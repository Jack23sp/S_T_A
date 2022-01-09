using System;
using System.Collections.Generic;
//using Mono.Data.Sqlite;

public partial class Database
{
    /*
    public void Connect_DailyRewards()
    {
        ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS character_dailyRewards (
                            account varchar(30) NOT NULL,
                            day INTEGER NOT NULL,
                            get INTEGER NOT NULL,
                            PRIMARY KEY(account, day))");
    }

    void CharacterLoad_DailyRewards(Player player)
    {
        List<List<object>> table = ExecuteReader("SELECT * FROM character_dailyRewards WHERE account=@account", new SqliteParameter("@account", player.account));
        foreach (List<object> row in table)
        {
            DailyRewardsStruct go = new DailyRewardsStruct();
            go.day = Convert.ToInt32(row[1]);
            if (Convert.ToInt32(row[2]) == 1) go.get = true;
            else  go.get = false;

            player.dailyRewards.Add(go);
        }
    }

    public void CharacterSave_DailyRewards(Player player)
    {
        ExecuteNonQuery("DELETE FROM character_dailyRewards WHERE account=@account", new SqliteParameter("@account", player.account));
        for (int i = 0; i < player.dailyRewards.Count; ++i)
        {
            ExecuteNonQuery("INSERT INTO character_dailyRewards VALUES (@account, @day, @get)",
                new SqliteParameter("@account", player.account),
                new SqliteParameter("@day", player.dailyRewards[i].day),
                new SqliteParameter("@get", player.dailyRewards[i].get));
        }
    }
*/
}

