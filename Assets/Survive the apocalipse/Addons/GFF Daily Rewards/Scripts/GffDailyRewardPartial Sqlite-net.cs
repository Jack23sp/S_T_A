

public partial class Database
{
    
    public class character_dailyRewards
    {
        public string account { get; set; }
        public int day { get; set; }
        public bool get { get; set; }
    }

    void Connect_DailyRewards()
    {
        // create tables if they don't exist yet or were deleted
        connection.CreateTable<character_dailyRewards>();
        connection.CreateIndex(nameof(character_dailyRewards), new[] { "account", "day" });
    }

    void CharacterLoad_DailyRewards(Player player)
    {
        // then load valid equipment and put into their slots
        // (one big query is A LOT faster than querying each slot separately)
        foreach (character_dailyRewards row in connection.Query<character_dailyRewards>("SELECT * FROM character_dailyRewards WHERE account=?", player.account))
        {
            DailyRewardsStruct go = new DailyRewardsStruct();
            go.day = row.day;
            go.get = row.get;

            player.dailyRewards.Add(go);
        }
    }

    public void CharacterSave_DailyRewards(Player player)
    {
        // quests: remove old entries first, then add all new ones
        connection.Execute("DELETE FROM character_dailyRewards WHERE account=?", player.account);

        for (int i = 0; i < player.dailyRewards.Count; ++i)
        {
            connection.InsertOrReplace(new character_dailyRewards
            {
                account = player.account,
                day = player.dailyRewards[i].day,
                get = player.dailyRewards[i].get
            });
        }
    }
    
}
