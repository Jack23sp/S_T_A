using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using CustomType;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using SpriteShadersURP;
using AdvancedPeopleSystem;

public partial class PlayerAbility
{
    public Player player;
    public List<ScriptableAbility> abilities = new List<ScriptableAbility>();

    public SyncListAbility networkAbilities = new SyncListAbility();

    public void Start()
    {
        abilities = GeneralManager.singleton.abilityList;
        if (networkAbilities.Count == 0 && player.isServer)
        {
            foreach (ScriptableAbility ab in abilities)
            {
                networkAbilities.Add(new Ability(ab.name, 0, ab.maxLevel, ab.baseValue));
            }
        }
    }

    [Command]
    public void CmdIncreaseAbility(int index)
    {
        if (CanUpgradeAbilities(index))
        {
            Ability netAbility = networkAbilities[index];
            netAbility.level++;
            networkAbilities[index] = netAbility;
            player.gold -= networkAbilities[index].baseValue * networkAbilities[index].level;
        }
    }

    public bool CanUpgradeAbilities(int index)
    {
        return networkAbilities[index].level < networkAbilities[index].maxLevel && player.gold >= (networkAbilities[index].baseValue * networkAbilities[index].level);
    }
}

public partial class PlayerAccuracy
{
    public Player player;

    public ScriptableAbility ability;

    public LinearFloat baseAccuracy;

    public void Start()
    {
        ability = GeneralManager.singleton.accuracyAbility;
    }


    public float accuracy
    {
        get
        {
            float equipmentBonus = 0;
            foreach (ItemSlot slot in player.equipment)
                if (slot.amount > 0)
                    equipmentBonus += ((EquipmentItem)slot.item.data).accuracy.Get(slot.item.accuracyLevel);

            if (player.playerBoost.networkBoost.Count == 0)
            {
                return equipmentBonus + baseAccuracy.Get(player.level) + (0.1f * Convert.ToSingle(GeneralManager.singleton.FindNetworkAbilityLevel(ability.name, player.name)));
            }
            else
            {
                return (equipmentBonus + baseAccuracy.Get(player.level) + (0.1f * Convert.ToSingle(GeneralManager.singleton.FindNetworkAbilityLevel(ability.name, player.name))) +
                       player.playerBoost.networkBoost[0].accuracyPerc);
            }
        }
    }

}

public partial class PlayerMiss
{
    public Player player;
    public ScriptableAbility ability;

    public LinearFloat baseMiss;

    public void Start()
    {
        ability = GeneralManager.singleton.missAbility;
    }
    public float maxMiss
    {
        get
        {
            // calculate equipment bonus
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            float equipmentBonus = 0;
            foreach (ItemSlot slot in player.equipment)
                if (slot.amount > 0)
                    equipmentBonus += ((EquipmentItem)slot.item.data).miss.Get(slot.item.missLevel);

            // base (health + buff) + equip + attributes
            if (player.playerBoost.networkBoost.Count == 0)
            {
                return equipmentBonus + baseMiss.Get(player.level) + (0.1f * Convert.ToSingle(GeneralManager.singleton.FindNetworkAbilityLevel(ability.name, player.name)));
            }
            else
            {
                return (equipmentBonus + baseMiss.Get(player.level) + (0.1f * Convert.ToSingle(GeneralManager.singleton.FindNetworkAbilityLevel(ability.name, player.name))) +
                       player.playerBoost.networkBoost[0].missPerc);

            }
        }
    }
}

public partial class PlayerAlliance
{
    public Player player;
    public ScriptableAbility abilityToLead;

    public SyncList<string> guildAlly = new SyncList<string>();

    public void Start()
    {
        abilityToLead = GeneralManager.singleton.allianceAbility;
    }

    [Command]
    public void CmdLoadGuild(string guildName)
    {
        Guild selectedGuild;
        if (!GuildSystem.guilds.ContainsKey(guildName))
        {
            Guild guild = Database.singleton.LoadGuild(guildName);
            GuildSystem.guilds[guild.name] = guild;
            selectedGuild = guild;
        }
        else
            selectedGuild = GuildSystem.guilds[guildName];

        TargetGuild(selectedGuild);

    }

    [TargetRpc] // only send to one client
    public void TargetGuild(Guild guild)
    {
        UIGroup.singleton.selectedGuild = guild;
    }


    public bool CanInviteToAlliance()
    {
        return player.target &&
               player.target is Player &&
               ((Player)player.target).InGuild() &&
               player.InGuild() &&
               MaxAllianceAmount() > player.playerAlliance.guildAlly.Count &&
               MaxTargetAllianceAmount() > ((Player)player.target).playerAlliance.guildAlly.Count &&
               !player.playerAlliance.guildAlly.Contains(((Player)player.target).guild.name) &&
               !((Player)player.target).playerAlliance.guildAlly.Contains(player.guild.name) &&
               player.guild.CanTerminate(name) &&
               ((Player)player.target).guild.CanTerminate(((Player)player.target).name);
    }

    public int MaxAllianceAmount()
    {
        return Convert.ToInt32(player.generalPart.FindNetworkAbility(player, player.playerAlliance.abilityToLead.name).level / 10);
    }

    public int MaxTargetAllianceAmount()
    {
        return Convert.ToInt32(player.generalPart.FindNetworkAbility(((Player)player.target), ((Player)player.target).playerAlliance.abilityToLead.name).level / 10);
    }

    [Command]
    public void CmdInviteToAlliance()
    {
        if (CanInviteToAlliance() && !((Player)player.target).playerOptions.blockAlly)
        {
            ((Player)player.target).guildAllyInviteName = name;
            ((Player)player.target).guildAllyInviteGuildName = player.guild.name;
        }
    }

    [Command]
    public void CmdAcceptInviteToAlliance()
    {
        if (!string.IsNullOrEmpty(player.guildAllyInviteName) && !string.IsNullOrEmpty(player.guildAllyInviteGuildName))
        {
            if (player.playerAlliance.guildAlly.Count < 5 && Player.onlinePlayers[player.guildAllyInviteName].playerAlliance.guildAlly.Count < 5)
            {
                player.playerAlliance.guildAlly.Add(player.guildAllyInviteGuildName);
                Player.onlinePlayers[player.guildAllyInviteName].playerAlliance.guildAlly.Add(player.guild.name);
            }
        }
        player.guildAllyInviteGuildName = string.Empty;
        player.guildAllyInviteName = string.Empty;
    }

    [Command]
    public void CmdDeclineInviteToAlliance()
    {
        player.guildAllyInviteGuildName = string.Empty;
        player.guildAllyInviteName = string.Empty;
    }


    [Command]
    public void CmdGuildInviteTarget()
    {
        // validate
        if (player.target != null && player.target is Player &&
            player.InGuild() && !((Player)player.target).InGuild() &&
            player.guild.CanInvite(name, player.target.name) &&
            NetworkTime.time >= player.nextRiskyActionTime &&
            Utils.ClosestDistance(player.collider, player.target.collider) <= player.interactionRange && !((Player)player.target).playerOptions.blockGroup)
        {
            // send an invite
            ((Player)player.target).guildInviteFrom = name;

            print(name + " invited " + player.target.name + " to guild");
        }

        // reset risky time no matter what. even if invite failed, we don't want
        // players to be able to spam the invite button and mass invite random
        // players.
        player.nextRiskyActionTime = NetworkTime.time + player.guildInviteWaitSeconds;
    }

    [Command]
    public void CmdRemoveAllyGuild(string guildToSearch, string guildToRemove)
    {
        GuildSystem.TerminateGuildAlly(guildToSearch, guildToRemove);
        GuildSystem.TerminateGuildAlly(guildToRemove, guildToSearch);

        // reset risky time no matter what. even if invite failed, we don't want
        // players to be able to spam the invite button and mass invite random
        // players.
        player.nextRiskyActionTime = NetworkTime.time + player.guildInviteWaitSeconds;
    }
}

public partial class PlayerArmor
{
    public Player player;

    public GameObject armorSlider;
    public GameObject instantiateObject;

    public void Start()
    {
        if (player && player.isLocalPlayer && player.isClient)
        {
            InvokeRepeating(nameof(SpawnMessageRoutine), 0.5f, 0.5f);
        }
    }

    public void SpawnMessageRoutine()
    {
        UIHealthMana.singleton.armorSlider.value = ArmorPercent();
        UIHealthMana.singleton.armorStatus.text = currentArmor + " / " + maxArmor;
    }

    public int currentArmor
    {
        get
        {
            int equipmentBonus = 0;
            foreach (ItemSlot slot in player.equipment)
                if (slot.amount > 0)
                    equipmentBonus += slot.item.currentArmor;

            return equipmentBonus;
        }
    }
    public int maxArmor
    {
        get
        {
            // calculate equipment bonus
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int equipmentBonus = 0;
            foreach (ItemSlot slot in player.equipment)
                if (slot.amount > 0)
                    equipmentBonus += ((EquipmentItem)slot.item.data).armor.Get(slot.item.armorLevel);

            // base (health + buff) + equip + attributes
            return equipmentBonus;
        }
    }

    public float ArmorPercent()
    {
        return (currentArmor != 0 && maxArmor != 0) ? (float)currentArmor / (float)maxArmor : 0;
    }

}

public partial class PlayerBoost
{
    public Player player;

    public SyncListBoost networkBoost = new SyncListBoost();

    public float cycleAmount;

    public void Start()
    {
        if (isServer && player)
        {
            cycleAmount = GeneralManager.singleton.boostInvoke;
            InvokeRepeating(nameof(DecreaseTimer), cycleAmount, cycleAmount);
        }
    }

    [Command]
    public void CmdAddBoost(int boostIndex, int currencyType, String date)
    {
        if (networkBoost.Count == 0)
        {
            networkBoost.Add(new Boost());
        }

        if (currencyType == 0)
        {
            Boost generalBoost = networkBoost[0];
            if (GeneralManager.singleton.GetBoostTemplate(generalBoost, boostIndex).velocityTimer != 0)
            {
                generalBoost.velocityTimer = DateTime.Parse(date).ToString();
                generalBoost.velocityTimerServer = System.DateTime.Now.AddSeconds(GeneralManager.singleton.listCompleteOfBoost[boostIndex].velocityTimer).ToString();
            }
            generalBoost.velocityPerc += GeneralManager.singleton.listCompleteOfBoost[boostIndex].velocityPerc;
            if (generalBoost.velocityPerc > 100) generalBoost.velocityPerc = 100;

            if (GeneralManager.singleton.GetBoostTemplate(generalBoost, boostIndex).accuracyTimer != 0)
            {
                generalBoost.accuracyTimer = DateTime.Parse(date).ToString();
                generalBoost.accuracyTimerServer = System.DateTime.Now.AddSeconds(GeneralManager.singleton.listCompleteOfBoost[boostIndex].accuracyTimer).ToString();
            }
            generalBoost.accuracyPerc += GeneralManager.singleton.listCompleteOfBoost[boostIndex].accuracyPerc;
            if (generalBoost.accuracyPerc > 100) generalBoost.accuracyPerc = 100;

            if (GeneralManager.singleton.GetBoostTemplate(generalBoost, boostIndex).missTimer != 0)
            {
                generalBoost.missTimer = DateTime.Parse(date).ToString();
                generalBoost.missTimerServer = System.DateTime.Now.AddSeconds(GeneralManager.singleton.listCompleteOfBoost[boostIndex].missTimer).ToString();
            }
            generalBoost.missPerc += GeneralManager.singleton.listCompleteOfBoost[boostIndex].missPerc;
            if (generalBoost.missPerc > 100) generalBoost.missPerc = 100;

            if (GeneralManager.singleton.GetBoostTemplate(generalBoost, boostIndex).hiddenIslandTimer != 0)
            {
                generalBoost.hiddenIslandTimer = DateTime.Parse(date).ToString();
                generalBoost.hiddenIslandTimerServer = System.DateTime.Now.AddSeconds(GeneralManager.singleton.listCompleteOfBoost[boostIndex].hiddenIslandTimer).ToString();
            }

            if (GeneralManager.singleton.GetBoostTemplate(generalBoost, boostIndex).doubleEXP != 0)
            {
                generalBoost.doubleEXP = DateTime.Parse(date).ToString();
                generalBoost.doubleEXPServer = System.DateTime.Now.AddSeconds(GeneralManager.singleton.listCompleteOfBoost[boostIndex].doubleEXP).ToString();
            }

            if (GeneralManager.singleton.GetBoostTemplate(generalBoost, boostIndex).doubleGold != 0)
            {
                generalBoost.doubleGold = DateTime.Parse(date).ToString();
                generalBoost.doubleGoldServer = System.DateTime.Now.AddSeconds(GeneralManager.singleton.listCompleteOfBoost[boostIndex].doubleGold).ToString();
            }

            if (GeneralManager.singleton.GetBoostTemplate(generalBoost, boostIndex).doubleLeaderPoints != 0)
            {
                generalBoost.doubleLeaderPoints = DateTime.Parse(date).ToString();
                generalBoost.doubleLeaderPointsServer = System.DateTime.Now.AddSeconds(GeneralManager.singleton.listCompleteOfBoost[boostIndex].doubleLeaderPoints).ToString();
            }

            if (GeneralManager.singleton.GetBoostTemplate(generalBoost, boostIndex).doubleDamageToMonster != 0)
            {
                generalBoost.doubleDamageToMonster = DateTime.Parse(date).ToString();
                generalBoost.doubleDamageToMonsterServer = System.DateTime.Now.AddSeconds(GeneralManager.singleton.listCompleteOfBoost[boostIndex].doubleDamageToMonster).ToString();
            }

            if (GeneralManager.singleton.GetBoostTemplate(generalBoost, boostIndex).doubleDamageToPlayer != 0)
            {
                generalBoost.doubleDamageToPlayer = DateTime.Parse(date).ToString();
                generalBoost.doubleDamageToPlayerServer = System.DateTime.Now.AddSeconds(GeneralManager.singleton.listCompleteOfBoost[boostIndex].doubleDamageToPlayer).ToString();
            }

            if (GeneralManager.singleton.GetBoostTemplate(generalBoost, boostIndex).doubleDamageToPlayer != 0)
            {
                generalBoost.doubleDamageToPlayer = DateTime.Parse(date).ToString();
                generalBoost.doubleDamageToPlayerServer = System.DateTime.Now.AddSeconds(GeneralManager.singleton.listCompleteOfBoost[boostIndex].doubleDamageToPlayer).ToString();
            }

            if (GeneralManager.singleton.GetBoostTemplate(generalBoost, boostIndex).doubleDamageToBuilding != 0)
            {
                generalBoost.doubleDamageToBuilding = DateTime.Parse(date).ToString();
                generalBoost.doubleDamageToBuildingServer = System.DateTime.Now.AddSeconds(GeneralManager.singleton.listCompleteOfBoost[boostIndex].doubleDamageToBuilding).ToString();
            }

            player.coins -= GeneralManager.singleton.listCompleteOfBoost[boostIndex].coin;
            player.playerBoost.networkBoost[0] = generalBoost;

            player.playerLeaderPoints.buyBoostPoint += GeneralManager.singleton.buyBoostPoint;
        }
        if (currencyType == 1)
        {
            Boost generalBoost = networkBoost[0];
            if (GeneralManager.singleton.GetBoostTemplate(generalBoost, boostIndex).velocityTimer != 0)
            {
                generalBoost.velocityTimer = DateTime.Parse(date).ToString();
                generalBoost.velocityTimerServer = System.DateTime.Now.AddSeconds(GeneralManager.singleton.listCompleteOfBoost[boostIndex].velocityTimer).ToString();
            }
            generalBoost.velocityPerc += GeneralManager.singleton.listCompleteOfBoost[boostIndex].velocityPerc;
            if (generalBoost.velocityPerc > 100) generalBoost.velocityPerc = 100;

            if (GeneralManager.singleton.GetBoostTemplate(generalBoost, boostIndex).accuracyTimer != 0)
            {
                generalBoost.accuracyTimer = DateTime.Parse(date).ToString();
                generalBoost.accuracyTimerServer = System.DateTime.Now.AddSeconds(GeneralManager.singleton.listCompleteOfBoost[boostIndex].accuracyTimer).ToString();
            }
            generalBoost.accuracyPerc += GeneralManager.singleton.listCompleteOfBoost[boostIndex].accuracyPerc;
            if (generalBoost.accuracyPerc > 100) generalBoost.accuracyPerc = 100;

            if (GeneralManager.singleton.GetBoostTemplate(generalBoost, boostIndex).missTimer != 0)
            {
                generalBoost.missTimer = DateTime.Parse(date).ToString();
                generalBoost.missTimerServer = System.DateTime.Now.AddSeconds(GeneralManager.singleton.listCompleteOfBoost[boostIndex].missTimer).ToString();
            }
            generalBoost.missPerc += GeneralManager.singleton.listCompleteOfBoost[boostIndex].missPerc;
            if (generalBoost.missPerc > 100) generalBoost.missPerc = 100;

            if (GeneralManager.singleton.GetBoostTemplate(generalBoost, boostIndex).hiddenIslandTimer != 0)
            {
                generalBoost.hiddenIslandTimer = DateTime.Parse(date).ToString();
                generalBoost.hiddenIslandTimerServer = System.DateTime.Now.AddSeconds(GeneralManager.singleton.listCompleteOfBoost[boostIndex].hiddenIslandTimer).ToString();
            }

            if (GeneralManager.singleton.GetBoostTemplate(generalBoost, boostIndex).doubleEXP != 0)
            {
                generalBoost.doubleEXP = DateTime.Parse(date).ToString();
                generalBoost.doubleEXPServer = System.DateTime.Now.AddSeconds(GeneralManager.singleton.listCompleteOfBoost[boostIndex].doubleEXP).ToString();
            }

            if (GeneralManager.singleton.GetBoostTemplate(generalBoost, boostIndex).doubleGold != 0)
            {
                generalBoost.doubleGold = DateTime.Parse(date).ToString();
                generalBoost.doubleGoldServer = System.DateTime.Now.AddSeconds(GeneralManager.singleton.listCompleteOfBoost[boostIndex].doubleGold).ToString();
            }

            if (GeneralManager.singleton.GetBoostTemplate(generalBoost, boostIndex).doubleLeaderPoints != 0)
            {
                generalBoost.doubleLeaderPoints = DateTime.Parse(date).ToString();
                generalBoost.doubleLeaderPointsServer = System.DateTime.Now.AddSeconds(GeneralManager.singleton.listCompleteOfBoost[boostIndex].doubleLeaderPoints).ToString();
            }

            if (GeneralManager.singleton.GetBoostTemplate(generalBoost, boostIndex).doubleDamageToMonster != 0)
            {
                generalBoost.doubleDamageToMonster = DateTime.Parse(date).ToString();
                generalBoost.doubleDamageToMonsterServer = System.DateTime.Now.AddSeconds(GeneralManager.singleton.listCompleteOfBoost[boostIndex].doubleDamageToMonster).ToString();
            }

            if (GeneralManager.singleton.GetBoostTemplate(generalBoost, boostIndex).doubleDamageToPlayer != 0)
            {
                generalBoost.doubleDamageToPlayer = DateTime.Parse(date).ToString();
                generalBoost.doubleDamageToPlayerServer = System.DateTime.Now.AddSeconds(GeneralManager.singleton.listCompleteOfBoost[boostIndex].doubleDamageToPlayer).ToString();
            }

            if (GeneralManager.singleton.GetBoostTemplate(generalBoost, boostIndex).doubleDamageToPlayer != 0)
            {
                generalBoost.doubleDamageToPlayer = DateTime.Parse(date).ToString();
                generalBoost.doubleDamageToPlayerServer = System.DateTime.Now.AddSeconds(GeneralManager.singleton.listCompleteOfBoost[boostIndex].doubleDamageToPlayer).ToString();
            }

            if (GeneralManager.singleton.GetBoostTemplate(generalBoost, boostIndex).doubleDamageToBuilding != 0)
            {
                generalBoost.doubleDamageToBuilding = DateTime.Parse(date).ToString();
                generalBoost.doubleDamageToBuildingServer = System.DateTime.Now.AddSeconds(GeneralManager.singleton.listCompleteOfBoost[boostIndex].doubleDamageToBuilding).ToString();
            }
            player.gold -= GeneralManager.singleton.listCompleteOfBoost[boostIndex].gold;
            player.playerBoost.networkBoost[0] = generalBoost;
        }
    }

    // Inserire direttamente nella firma della variabile
    public void DecreaseTimer()
    {
        if (networkBoost.Count == 0) return;

        Boost generalBoost = networkBoost[0];
        TimeSpan difference;
        TimeSpan premiumDifference;

        if (!string.IsNullOrEmpty(generalBoost.velocityTimerServer))
            difference = DateTime.Parse(generalBoost.velocityTimerServer.ToString()) - DateTime.Now;
        if (!string.IsNullOrEmpty(generalBoost.velocityTimerServer) && (difference.TotalSeconds < 0)) generalBoost.velocityPerc = 0;

        if (!string.IsNullOrEmpty(generalBoost.accuracyTimerServer))
            difference = DateTime.Parse(generalBoost.accuracyTimerServer.ToString()) - DateTime.Now;
        if (!string.IsNullOrEmpty(generalBoost.accuracyTimerServer) && (difference.TotalSeconds < 0)) generalBoost.accuracyPerc = 0;

        if (!string.IsNullOrEmpty(generalBoost.missTimerServer))
            difference = DateTime.Parse(generalBoost.missTimerServer.ToString()) - DateTime.Now;
        if (!string.IsNullOrEmpty(generalBoost.missTimerServer) && (difference.TotalSeconds < 0)) generalBoost.missPerc = 0;

        if (!string.IsNullOrEmpty(generalBoost.hiddenIslandTimerServer))
            premiumDifference = DateTime.Parse(networkBoost[0].hiddenIslandTimerServer.ToString()) - DateTime.Now;

        player.playerBoost.networkBoost[0] = generalBoost;
    }

}

public partial class PlayerConservative
{
    public Player player;
    public ScriptableAbility ability;

    // Start is called before the first frame update
    void Start()
    {
        ability = GeneralManager.singleton.conservativeAbility;
    }

    public int AmountOfItemLostable()
    {
        return GeneralManager.singleton.FindNetworkAbilityLevel(ability.name, player.name) / 10 > 1 ? 5 - GeneralManager.singleton.FindNetworkAbilityLevel(ability.name, player.name) : 5;
    }

    public int AmountOfItemProtected()
    {
        if (player.equipment[7].amount > 0)
        {
            return (((EquipmentItem)player.equipment[7].item.data).protectedSlot.Get(player.equipment[7].item.bagLevel));
        }
        return 1;
    }

    [Server]
    public void SpawnItemOnDeath()
    {
        int amountOfItem = player.playerConservative.AmountOfItemLostable();
        bool lost = false;
        if (amountOfItem > 0)
        {
            GameObject g = Instantiate(GeneralManager.singleton.chestItem, transform.position, Quaternion.identity);
            for (int i = 0; i < amountOfItem; i++)
            {
                for (int e = 0; e < player.inventory.Count; e++)
                {
                    if (player.inventory[e].amount > 0 && player.inventory[e].item.data.isLostable && amountOfItem > 0 && e > player.playerConservative.AmountOfItemProtected())
                    {
                        int amountToLost = 0;
                        ItemSlot slot = player.inventory[e];
                        amountToLost = UnityEngine.Random.Range(1, player.inventory[e].amount - 1);
                        slot.amount = amountToLost;
                        g.GetComponent<Entity>().inventory.Add(slot);
                        amountOfItem--;
                        slot = player.inventory[e];
                        slot.DecreaseAmount(amountToLost);
                        player.inventory[e] = slot;
                        lost = true;
                    }
                }
            }
            Entity entity = g.GetComponent<Entity>();
            NetworkServer.Spawn(g);
            if (lost)
                TargetItem();
        }
    }

    [TargetRpc]
    public void TargetItem()
    {
        UINotificationManager.singleton.SpawnLostableObject();
    }



}

public partial class PlayerEmoji
{
    public Player player;

    public SyncList<string> networkEmoji = new SyncList<string>();

    public override void OnStartClient()
    {
        networkEmoji.Callback += OnEmojiListUpdated;
    }

    void OnEmojiListUpdated(SyncList<string>.Operation op, int index, string oldItem, string newItem)
    {
        switch (op)
        {
            case SyncList<string>.Operation.OP_ADD:
                // index is where it was added into the list
                // newItem is the new item
                if (player.isLocalPlayer)
                {
                    UIEmoji.singleton.ManageOpenPanel();
                }
                break;
            case SyncList<string>.Operation.OP_INSERT:
                // index is where it was inserted into the list
                // newItem is the new item
                if (player.isLocalPlayer)
                {
                    UIEmoji.singleton.ManageOpenPanel();
                }
                break;
            case SyncList<string>.Operation.OP_REMOVEAT:
                // index is where it was removed from the list
                // oldItem is the item that was removed
                break;
            case SyncList<string>.Operation.OP_SET:
                // index is of the item that was changed
                // oldItem is the previous value for the item at the index
                // newItem is the new value for the item at the index
                break;
            case SyncList<string>.Operation.OP_CLEAR:
                // list got cleared
                break;
        }
    }
    [Command]
    public void CmdSpawnEmoji(string emojiName, string playerName)
    {
        if (GeneralManager.singleton.FindNetworkEmoji(emojiName, playerName) >= 0 && player.playerCar.car == null)
        {
            for (int i = 0; i < GeneralManager.singleton.listCompleteOfEmoji.Count; i++)
            {
                if (GeneralManager.singleton.listCompleteOfEmoji[i].name.Contains(emojiName))
                {
                    GameObject g = Instantiate(GeneralManager.singleton.emojiToSpawn);
                    g.GetComponent<SpawnedEmoji>().emojiName = emojiName;
                    g.GetComponent<SpawnedEmoji>().playerName = playerName;
                    NetworkServer.Spawn(g);
                }
            }
        }
    }


    [Command]
    public void CmdAddEmoji(string emojiName, int currencyType)
    {
        ScriptableEmoji emoji = null;
        for (int i = 0; i < GeneralManager.singleton.listCompleteOfEmoji.Count; i++)
        {
            if (GeneralManager.singleton.listCompleteOfEmoji[i].name == emojiName)
            {
                emoji = GeneralManager.singleton.listCompleteOfEmoji[i];
            }
        }
        if (currencyType == 0)
        {
            if (player.coins >= emoji.coinToBuy)
            {
                player.playerEmoji.networkEmoji.Add(emojiName);
                player.coins -= emoji.coinToBuy;
            }
        }
        if (currencyType == 1)
        {
            if (player.gold >= emoji.goldToBuy)
            {
                player.playerEmoji.networkEmoji.Add(emojiName);
                player.gold -= emoji.goldToBuy;
            }
        }

        for (int i = 0; i < player.quests.Count; i++)
        {
            Quest quest = player.quests[i];
            if (quest.data.buyEmoji == true)
            {
                quest.checkBuyEmoji = true;
            }
            player.quests[i] = quest;
        }
    }


}

public partial class PlayerFoodUnsanity
{
    public Player player;

    public float cycleAmount = 60.0f;

    void Start()
    {
        if (isServer && player)
        {
            cycleAmount = GeneralManager.singleton.unsanityInvoke;
            InvokeRepeating(nameof(DecreaseUnsanity), cycleAmount, cycleAmount);
        }

    }

    public void DecreaseUnsanity()
    {
        for (int i = 0; i < player.inventory.Count; i++)
        {
            if (player.inventory[i].amount > 0)
            {
                if (player.inventory[i].item.data is FoodItem)
                {
                    if (player.inventory[i].item.currentUnsanity > 0)
                    {
                        ItemSlot slot = player.inventory[i];
                        slot.item.currentUnsanity--;
                        if (slot.item.currentUnsanity == 0)
                            player.inventory[i] = new ItemSlot();
                        else
                            player.inventory[i] = slot;
                    }
                }
            }
        }
    }

}

public partial class PlayerHungry
{
    public Player player;

    public int maxHungry = 100;
    [SyncVar]
    public int currentHungry;

    public float cycleAmount = 60.0f;

    public int healthToRemove = 5;

    void Start()
    {
        if (isServer)
        {
            cycleAmount = GeneralManager.singleton.hungryInvoke;
            InvokeRepeating(nameof(DecreaseHungry), cycleAmount, cycleAmount);
        }
        else
        {
            if (player.isLocalPlayer)
            {
                InvokeRepeating(nameof(SpawnMessageRoutine), 90.0f, 90.0f);
            }
        }
    }

    public void SpawnMessageRoutine()
    {
        if (currentHungry <= 20)
        {
            UINotificationManager.singleton.SpawnHungryObject();
        }
    }

    public void DecreaseHungry()
    {
        if (player.playerHungry.currentHungry > 0) player.playerHungry.currentHungry--;
        if (player.playerHungry.currentHungry <= 0) player._health -= healthToRemove;
        if (player.health <= 0) player.health = 0;
    }

}

public partial class PlayerPoisoning
{
    public Player player;

    public int maxPoisoning = 100;
    [SyncVar]
    public int currentPoisoning;

    public float cycleAmount = 60.0f;

    public int healthToRemove = 5;

    public GameObject poisonedObject;

    void Start()
    {
        if (isServer)
        {
            cycleAmount = GeneralManager.singleton.poisoningInvoke;
            InvokeRepeating(nameof(DecreasePoisoning), cycleAmount, cycleAmount);
        }
    }

    public void DecreasePoisoning()
    {
        if (player.playerPoisoning.currentPoisoning >= 100)
        {
            player.playerPoisoning.currentPoisoning = 100;
            player._health -= player.playerPoisoning.healthToRemove;
        }
    }
}

public partial class PlayerThirsty
{
    public Player player;

    public int maxThirsty = 100;
    [SyncVar]
    public int currentThirsty;

    public float cycleAmount = 60.0f;

    public int healthToRemove = 5;

    void Start()
    {
        if (isServer)
        {
            cycleAmount = GeneralManager.singleton.thirstyInvoke;
            InvokeRepeating(nameof(DecreaseThirsty), cycleAmount, cycleAmount);
        }
        else
        {
            if (player.isLocalPlayer)
            {
                InvokeRepeating(nameof(SpawnMessageRoutine), 90.0f, 90.0f);
            }
        }
    }

    public void SpawnMessageRoutine()
    {
        if (currentThirsty <= 20)
        {
            UINotificationManager.singleton.SpawnThirstObject();
        }
    }

    public void DecreaseThirsty()
    {
        if (player.playerThirsty.currentThirsty > 0) player.playerThirsty.currentThirsty--;
        if (player.playerThirsty.currentThirsty <= 0) player._health -= healthToRemove;
        if (player.health <= 0) player.health = 0;
    }
}

public partial class PlayerTemperature
{
    public Player player;
    public float cycleAmount;
    public int healthToRemove = 30;

    void Start()
    {
        if (isServer)
        {
            cycleAmount = GeneralManager.singleton.temperatureInvoke;
            InvokeRepeating(nameof(CheckTemperatureCover), cycleAmount, cycleAmount);
        }
        else
        {
            if (player.isLocalPlayer)
            {
                InvokeRepeating(nameof(SpawnMessageRoutine), 90.0f, 90.0f);
            }
        }
    }

    public void SpawnMessageRoutine()
    {
        if (actualCover < TemperatureManager.singleton.actualSafeCover)
        {
            UINotificationManager.singleton.SpawnCoverObject();
        }
    }


    public float actualCover
    {
        get
        {
            float equipmentBonus = 0;
            foreach (ItemSlot slot in player.equipment)
                if (slot.amount > 0)
                    equipmentBonus += ((EquipmentItem)slot.item.data).coverTemperature;

            return equipmentBonus;
        }
    }

    public void CheckTemperatureCover()
    {
        if (player.playerTemperature.actualCover < TemperatureManager.singleton.actualSafeCover)
        {
            player._health -= player.playerTemperature.healthToRemove;
        }
    }
}

public partial class PlayerItemEquipment
{
    public Player player;

    public ItemSlot firstWeapon;
    public ItemSlot secondWeapon;

    public GameObject weapon;

    public void CheckFirstWeapon()
    {
        if (player.equipment[0].amount > 0)
        {
            firstWeapon = player.equipment[0];
        }
        else
        {
            if (firstWeapon.amount > 0)
                firstWeapon = new ItemSlot();
            else
            {
                if (weapon)
                {
                    Destroy(weapon.gameObject);
                    weapon = null;
                }
            }
        }

        if (player.equipment[1].amount > 0)
        {
            secondWeapon = player.equipment[1];
        }
        else
        {
            if (secondWeapon.amount > 0)
                secondWeapon = new ItemSlot();
        }
    }
}

public partial class PlayerLeaderPoints
{
    public Player player;

    [SyncVar]
    public int monsterKill;
    [SyncVar]
    public int bossKill;
    [SyncVar]
    public int plantPoint;
    [SyncVar]
    public int rockPoint;
    [SyncVar]
    public int treePoint;
    [SyncVar]
    public int upgradeItemPoint;
    [SyncVar]
    public int craftItemPoint;
    [SyncVar]
    public int buildinPoint;
    [SyncVar]
    public int buyBoostPoint;
    [SyncVar]
    public int playerKill;

    public List<int> personalClaimed = new List<int>();
    public List<int> groupClaimed = new List<int>();
    public List<int> allianceClaimed = new List<int>();

    public int personalPoint;
    public int groupPoint;
    public int allyPoint;

    public void Start()
    {
        personalPoint = monsterKill + bossKill + plantPoint + rockPoint + treePoint + upgradeItemPoint + craftItemPoint + buildinPoint + buyBoostPoint + playerKill;
    }

    public float personalPercent()
    {
        float personalPoint = monsterKill + bossKill + plantPoint + rockPoint + treePoint + upgradeItemPoint + craftItemPoint + buildinPoint + buyBoostPoint + playerKill;
        float pers = ((float)personalPoint != 0 && (float)GeneralManager.singleton.personalPoints != 0) ? (float)personalPoint / (float)GeneralManager.singleton.personalPoints : 0;
        if (pers < 0.01f) return 0.0f;
        return pers;
    }

    public float groupPercent()
    {
        float group = ((float)groupPoint != 0 && (float)GeneralManager.singleton.groupPoint != 0) ? (float)groupPoint / (float)GeneralManager.singleton.groupPoint : 0;
        if (group < 0.01f) return 0.0f;
        return group;
    }

    public float allyPercent()
    {
        float ally = ((float)allyPoint != 0 && (float)GeneralManager.singleton.AlliancePoint != 0) ? (float)allyPoint / (float)GeneralManager.singleton.AlliancePoint : 0;
        if (ally < 0.01f) return 0.0f;
        return ally;
    }

    public float CalculateTotal()
    {
        return (float)monsterKill + (float)bossKill + (float)plantPoint + (float)rockPoint + (float)treePoint + (float)upgradeItemPoint + (float)craftItemPoint + (float)buildinPoint + (float)buyBoostPoint + (float)playerKill;
    }

    [Command]
    public void CmdChargeGuildLeaderboard()
    {
        groupPoint = Database.singleton.LoadGroupLeaderboardPoint(player);
        allyPoint = Database.singleton.LoadAllyGroupLeaderboardPoint(player);
        player.nextRiskyActionTime += 60;

        //Target here
        TargetLeaderboard(groupPoint, allyPoint);
    }
    [TargetRpc]
    public void TargetLeaderboard(int group, int ally)
    {
        player.playerLeaderPoints.groupPoint = group;
        player.playerLeaderPoints.allyPoint = ally;
    }


    [Command]
    public void CmdAddLeaderboardItem(int type, string itemName, int amount, int point)
    {

        if (ScriptableItem.dict.TryGetValue(itemName.GetStableHashCode(), out ScriptableItem itemToAdd))
        {
            if (itemToAdd)
            {
                int personalPoint = monsterKill + bossKill + plantPoint + rockPoint + treePoint + upgradeItemPoint + craftItemPoint + buildinPoint + buyBoostPoint + playerKill;

                if (type == 0 && personalPoint >= point)
                {
                    if (player.InventoryCanAdd(new Item(itemToAdd), amount))
                    {
                        player.InventoryAdd(new Item(itemToAdd), amount);
                    }
                    player.playerLeaderPoints.personalClaimed.Add(point);
                    TargetAddLeaderboardItem(type, point);
                }
                if (type == 1 && player.playerLeaderPoints.groupPoint >= point)
                {
                    if (player.InventoryCanAdd(new Item(itemToAdd), amount))
                    {
                        player.InventoryAdd(new Item(itemToAdd), amount);
                    }
                    player.playerLeaderPoints.groupClaimed.Add(point);
                    TargetAddLeaderboardItem(type, point);
                }
                if (type == 2 && player.playerLeaderPoints.allyPoint >= point)
                {
                    if (player.InventoryCanAdd(new Item(itemToAdd), amount))
                    {
                        player.InventoryAdd(new Item(itemToAdd), amount);
                    }
                    player.playerLeaderPoints.allianceClaimed.Add(point);
                    TargetAddLeaderboardItem(type, point);
                }

            }
        }


    }
    [TargetRpc]
    public void TargetAddLeaderboardItem(int type, int point)
    {
        UILeaderboardRewards.singleton.ReworkItem();
        if (type == 0)
        {
            player.playerLeaderPoints.personalClaimed.Add(point);
        }
        if (type == 1)
        {
            player.playerLeaderPoints.groupClaimed.Add(point);
        }
        if (type == 2)
        {
            player.playerLeaderPoints.allianceClaimed.Add(point);
        }
    }

}

public partial class PlayerPremiumZoneManager
{
    public Player player;

    public bool settedNormal;
    public bool settedSpecial;

    private bool instantiate;

    public GameObject nameNormal;
    public GameObject nameSpecial;

    public TextMeshProUGUI textNormal;
    public TextMeshProUGUI textSpecial;

    public bool settedColor;

    TimeSpan difference;

    public void Start()
    {
    }

    public void Update()
    {
        if (!settedNormal && player.playerBoost.networkBoost.Count == 0)
        {
            player.nameOverlay = textNormal;
            textNormal.gameObject.SetActive(true);
            textSpecial.gameObject.SetActive(false);
            settedNormal = true;
            settedSpecial = false;
        }

        if (player.playerBoost && player.playerBoost.networkBoost.Count > 0)
        {
            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer))
                difference = DateTime.Parse(player.playerBoost.networkBoost[0].hiddenIslandTimer.ToString()) - DateTime.Now;

            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer) && difference.TotalSeconds == 3600 && !instantiate)
            {
                UINotificationManager.singleton.SpawnTicketObject();
                instantiate = true;
            }
            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer) && difference.TotalSeconds != 3600)
            {
                instantiate = false;
            }

            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer) && difference.TotalSeconds > 0)
            {
                player.nameOverlay = textSpecial;
                textNormal.gameObject.SetActive(false);
                textSpecial.gameObject.SetActive(true);
                settedSpecial = true;
                settedNormal = false;
            }
            else if (string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer) || (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer) && difference.TotalSeconds <= 0))
            {
                player.nameOverlay = textNormal;
                textNormal.gameObject.SetActive(true);
                textSpecial.gameObject.SetActive(false);
                settedNormal = true;
                settedSpecial = false;
                if (!settedColor)
                {
                    textNormal.color = Color.white;
                    settedColor = true;
                }
            }
        }
    }
}

public partial class PlayerMonsterGrab
{
    public Player player;
    public List<Monster> nearMonster = new List<Monster>();
    [SyncVar(hook = nameof(ManageShakeCamera))]
    public bool shakeCamera;
    [SyncVar(hook = nameof(ManageShakeCameraResource))]
    public bool shakeCameraResource;
    public CustomCameraShake customCameraShake;

    void Start()
    {
        if (!customCameraShake) FindObjectOfType<CustomCameraShake>();
    }

    void ManageShakeCamera(bool oldBool, bool newBool)
    {
        if (newBool)
        {
            customCameraShake.animator.SetBool("SHAKE", true);
            if (!isServer) CmdDisableShake();
            shakeCamera = false;
        }
    }

    void ManageShakeCameraResource(bool oldBool, bool newBool)
    {
        if (newBool)
        {
            customCameraShake.animator.SetBool("SHAKERESOURCE", true);
            if (!isServer) CmdDisableShake();
            shakeCameraResource = false;
        }
    }

    [Command]
    public void CmdDisableShake()
    {
        shakeCamera = false;
        shakeCameraResource = false;
    }


    public void TargetNearest()
    {
        if (player.isServer &&
           player.target is Monster &&
           player.playerItemEquipment.firstWeapon.amount > 0 &&
           ((WeaponItem)player.playerItemEquipment.firstWeapon.item.data).distamceToGrab > 0)
        {
            // find all monsters that are alive, sort by distance
            GameObject[] objects = GameObject.FindGameObjectsWithTag("Monster");
            List<Monster> monsters = objects.Select(go => go.GetComponent<Monster>()).Where(m => m.health > 0).ToList();
            nearMonster = monsters.OrderBy(m => Vector2.Distance(transform.position, m.transform.position)).ToList();

            for (int i = 0; i < nearMonster.Count; i++)
            {
                if (Vector2.Distance(player.transform.position, nearMonster[i].transform.position) <= ((WeaponItem)player.playerItemEquipment.firstWeapon.item.data).distamceToGrab)
                {
                    nearMonster[i].moveProbability = 1.0f;
                    nearMonster[i].moveDistance = ((WeaponItem)player.playerItemEquipment.firstWeapon.item.data).distamceToGrab + 5;
                    nearMonster[i].followDistance = ((WeaponItem)player.playerItemEquipment.firstWeapon.item.data).distamceToGrab + 5;
                    nearMonster[i].target = ((Entity)player);
                    //nearMonster[i].monsterGradTimer = NetworkTime.time + 10.0f;
                }
            }
        }
    }


}

public partial class PlayerRadio
{
    public Player player;

    [SyncVar]
    public bool isOn;
    public ItemSlot radioItem;
    public float cycleAmount;

    [SyncVar, HideInInspector] public double nextRiskyActionTime = 0;

    void Start()
    {
        if (isServer)
        {
            cycleAmount = GeneralManager.singleton.radioInvoke;
            InvokeRepeating(nameof(DecreaseRadio), cycleAmount, cycleAmount);
        }
        else
        {
            if (player.isLocalPlayer)
            {
                InvokeRepeating(nameof(SpawnMessageRoutine), 300.0f, 300.0f);
            }
        }
    }

    public void SpawnMessageRoutine()
    {
        if (radioItem.amount > 0)
        {
            if (radioItem.item.radioCurrentBattery <= 20)
            {
                UINotificationManager.singleton.SpawnRadioObject();
            }
        }
    }

    public void CheckRadio()
    {
        if (player.equipment.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Radio")) != -1)
        {
            radioItem = player.equipment[player.equipment.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Radio"))];
        }
        else
        {
            radioItem.amount = 0;
        }
    }

    public void DecreaseRadio()
    {
        if (radioItem.amount > 0 && isOn && radioItem.item.radioCurrentBattery > 0)
        {
            radioItem.item.radioCurrentBattery--;
            player.equipment[player.equipment.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Radio"))] = radioItem;

            isOn = radioItem.item.radioCurrentBattery == 0 ? isOn = false : isOn = true;
        }
    }

    [Command]
    public void CmdSetRadio()
    {
        if (radioItem.item.name != string.Empty && NetworkTime.time >= nextRiskyActionTime)
        {
            isOn = !isOn;
            if (radioItem.item.radioCurrentBattery == 0)
            {
                isOn = false;
            }
            nextRiskyActionTime = NetworkTime.time + 1.5f;
        }
    }

}

public partial class PlayerSpawnpoint
{
    public Player player;
    public SyncListSpawnPoint spawnpoint = new SyncListSpawnPoint();
    public ScriptableAbility ability;

    void Start()
    {
        ability = GeneralManager.singleton.spawnpointAbility;
        if (isServer)
        {
            OrderSpawnpoint();
        }
    }

    public void OrderSpawnpoint()
    {
        if (isServer)
        {
            for (int i = 0; i < spawnpoint.Count; i++)
            {
                int index = i;
                if (spawnpoint[index].prefered)
                {
                    Spawnpoint sp = spawnpoint[FindFirstNotPreferedSpawnpoint()];
                    spawnpoint[FindFirstNotPreferedSpawnpoint()] = spawnpoint[index];
                    spawnpoint[index] = sp;
                }
            }
        }
    }

    public int FindFirstNotPreferedSpawnpoint()
    {
        for (int e = 0; e < spawnpoint.Count; e++)
        {
            int index = e;
            if (!spawnpoint[index].prefered)
            {
                return index;
            }
        }
        return 0;
    }

    public void AdditionRevive()
    {
        player.playerPoisoning.currentPoisoning = 0;
        player.playerHungry.currentHungry = player.playerHungry.maxHungry;
        player.playerThirsty.currentThirsty = player.playerThirsty.maxThirsty;
        player.mana = player.manaMax;
        player.playerCar.Exit();
        player.playerBuilding.building = null;
        if (player.playerBuilding.actualBuilding)
        {
            Destroy(player.playerBuilding.actualBuilding);
            player.playerBuilding.actualBuilding = null;
        }
        if (player.playerMove.forniture)
        {
            Destroy(player.playerMove.forniture);
            player.playerMove.forniture = null;
        }
        player.playerBuilding.inventoryIndex = -1;
        player.playerInjury.injured = false;
        for (int i = 0; i < player.equipment.Count; i++)
        {
            int index = i;
            if (player.equipment[index].amount > 0)
            {
                ItemSlot slot = player.equipment[index];
                slot.item.currentArmor = ((EquipmentItem)slot.item.data).armor.Get(slot.item.armorLevel);
                player.equipment[index] = slot;
            }
        }
    }

    [Command]
    public void CmdSpawnpointRevive(float healthPercentage)
    {
        if (player.health <= 0 && player.InventoryCount(new Item(GeneralManager.singleton.Instantresurrect)) > 0)
        {
            player.health = Mathf.RoundToInt(player.healthMax * healthPercentage);
            player.mana = Mathf.RoundToInt(player.manaMax * healthPercentage);
            SetState("IDLE");
            player.InventoryRemove(new Item(GeneralManager.singleton.Instantresurrect), 1);
            AdditionRevive();
        }
    }

    [Command]
    public void CmdSpawnSomewhere()
    {
        player.playerConservative.SpawnItemOnDeath();
        Transform start = NetworkManagerMMO.GetNearestStartPosition(transform.position);
        player.Revive(1.0f);
        AdditionRevive();
        SetState("IDLE");
        player.agent.Warp(start.position); // recommended over transform.position
    }

    public void SetState(string state)
    {
        player._state = state;
    }

    [Command]
    public void CmdSetSpawnpoint(string name, float x, float y, bool prefered, string playerName)
    {
        int spawnpointAbility = Convert.ToInt32(GeneralManager.singleton.FindNetworkAbilityLevel(player.playerSpawnpoint.ability.name, playerName) / 10);
        int possibleSpawnpoint = spawnpointAbility - player.playerSpawnpoint.spawnpoint.Count;

        if (possibleSpawnpoint > 0)
        {
            Spawnpoint sp = new Spawnpoint(name, x, y, prefered);
            player.playerSpawnpoint.spawnpoint.Add(sp);
        }
        player.playerSpawnpoint.OrderSpawnpoint();

        for (int i = 0; i < player.quests.Count; i++)
        {
            Quest quest = player.quests[i];
            if (quest.data.setSpawnpoint.numberOfSpawnpointToCreate > 0)
            {
                quest.checkCreateASpawnpoint++;
            }
            player.quests[i] = quest;
        }
    }

    [Command]
    public void CmdDeleteSpawnpoint(string spawnpointName)
    {
        for (int i = 0; i < player.playerSpawnpoint.spawnpoint.Count; i++)
        {
            int index = i;
            if (player.playerSpawnpoint.spawnpoint[index].name == spawnpointName)
            {
                player.playerSpawnpoint.spawnpoint.RemoveAt(index);
            }
        }
        player.playerSpawnpoint.OrderSpawnpoint();

        for (int i = 0; i < player.quests.Count; i++)
        {
            Quest quest = player.quests[i];
            if (quest.data.setSpawnpoint.numberOfSpawnpointToCreate > 0)
            {
                quest.checkCreateASpawnpoint--;
            }
            player.quests[i] = quest;
        }
    }

    [Command]
    public void CmdSetPrefered(string spawnpoint)
    {
        for (int i = 0; i < player.playerSpawnpoint.spawnpoint.Count; i++)
        {
            int index = i;
            if (player.playerSpawnpoint.spawnpoint[index].name == spawnpoint)
            {
                Spawnpoint s = player.playerSpawnpoint.spawnpoint[index];
                s.prefered = !s.prefered;
                player.playerSpawnpoint.spawnpoint[index] = s;
            }
        }
        player.playerSpawnpoint.OrderSpawnpoint();
    }
    public void SetPrefered(string spawnpoint)
    {
        for (int i = 0; i < player.playerSpawnpoint.spawnpoint.Count; i++)
        {
            int index = i;
            if (player.playerSpawnpoint.spawnpoint[index].name == spawnpoint)
            {
                Spawnpoint s = player.playerSpawnpoint.spawnpoint[index];
                s.prefered = !s.prefered;
                player.playerSpawnpoint.spawnpoint[index] = s;
            }
        }
        player.playerSpawnpoint.OrderSpawnpoint();
    }


    [Command]
    public void CmdSpawnAtPoint(float x, float y)
    {
        if (player.health == 0)
        {
            player.agent.Warp(new Vector2(x, y));
            player.Revive(1.0f);
            SetState("IDLE");
            AdditionRevive();
        }
    }
}

public partial class PlayerTorch
{
    public Player player;

    [SyncVar(hook = nameof(ManageTorch))]
    public bool isOn;
    public ItemSlot torchItem;
    public GameObject torch;

    public float cycleAmount;

    private bool instantiate;

    [SyncVar, HideInInspector] public double nextRiskyActionTime = 0;

    void Start()
    {
        if (isServer)
        {
            cycleAmount = GeneralManager.singleton.torchInvoke;
            InvokeRepeating(nameof(DecreaseTorch), cycleAmount, cycleAmount);
            ManageTorch(true, isOn);
        }
        else
        {
            if (player.isLocalPlayer)
            {
                InvokeRepeating(nameof(SpawnMessageRoutine), 300.0f, 300.0f);
            }
        }
    }

    public void SpawnMessageRoutine()
    {
        if (torchItem.amount > 0)
        {
            if (torchItem.item.torchCurrentBattery <= 20)
            {
                UINotificationManager.singleton.SpawnTorchObject();
            }
        }
    }

    public void CheckTorch()
    {
        if (player.equipment.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Torch")) != -1)
        {
            torchItem = player.equipment[player.equipment.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Torch"))];
        }
        else
        {
            torchItem.amount = 0;
        }
    }

    void ManageTorch(bool oldBool, bool newBool)
    {

        torch.SetActive(newBool);
    }


    public void DecreaseTorch()
    {
        if (torchItem.amount > 0 && isOn && torchItem.item.torchCurrentBattery > 0 && player.playerCar._car == null)
        {
            torchItem.item.torchCurrentBattery--;
            player.equipment[player.equipment.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Torch"))] = torchItem;

            isOn = torchItem.item.torchCurrentBattery == 0 ? isOn = false : isOn = true;
        }
    }

    [Command]
    public void CmdSetTorch()
    {
        if (torchItem.item.name != string.Empty && NetworkTime.time >= nextRiskyActionTime)
        {
            isOn = !isOn;
            if (torchItem.item.torchCurrentBattery == 0)
            {
                isOn = false;
            }
            nextRiskyActionTime = NetworkTime.time + 1.5f;
        }
    }
}

public partial class PlayerWeight
{
    public Player player;
    [SyncVar]
    public float currentWeight;
    [SyncVar]
    public float maxWeight;
}


public partial class PlayerMunitionManager
{
    public Player player;

    public ItemSlot actualWeapon;
    public AmmoItem selectedAmmo;
    public int currentAmmo;


    public void CheckMunition()
    {
        if (player.equipment.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Ammo")) != -1)
        {
            selectedAmmo = ((AmmoItem)player.equipment[player.equipment.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Ammo"))].item.data);
        }
        else
            selectedAmmo = null;
    }

    public void CheckWeapon()
    {
        if (player.equipment[0].amount > 0)
        {
            actualWeapon = player.equipment[0];
            currentAmmo = player.equipment[0].item.alreadyShooted;
        }
        else
        {
            actualWeapon.amount = 0;
            currentAmmo = 0;
        }
    }

    public void Charge()
    {
        ItemSlot slot = player.equipment[0];
        slot.item.alreadyShooted = 0;
        player.equipment[0] = slot;

    }
}
// da verificare
public partial class PlayerChat
{
    [Header("Components")] // to be assigned in inspector
    public Player player;

    [Header("Channels")]
    public ChannelInfo whisper = new ChannelInfo("/w", "(TO)", "(FROM)", null);
    public ChannelInfo local = new ChannelInfo("", "", "", null);
    public ChannelInfo party = new ChannelInfo("/p", "(Party)", "(Party)", null);
    public ChannelInfo guild = new ChannelInfo("/g", "(Guild)", "(Guild)", null);
    public ChannelInfo ally = new ChannelInfo("/a", "(Ally)", "(Ally)", null);
    public ChannelInfo info = new ChannelInfo("", "(Info)", "(Info)", null);


    public int infoMessage;
    public int whisperMessage;
    public int localMessage;
    public int partyMessage;
    public int guildMessage;
    public int allyMessage;

    [Header("Other")]
    public int maxLength = 70;

    public override void OnStartLocalPlayer()
    {
        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
        {
            infoChannel.Add(new ChatMessage("", info.identifierIn, "Benvenuto su Survive the Apocalipse! ", "", info.textPrefab));
        }
        else
        {
            infoChannel.Add(new ChatMessage("", info.identifierIn, "Welcome to Survive the Apocalipse! ", "", info.textPrefab));
        }
        infoMessage += 1;

        // addon system hooks
        Utils.InvokeMany(typeof(PlayerChat), this, "OnStartLocalPlayer_");
    }

    [Header("Multi Channel message")]
    public List<ChatMessage> infoChannel = new List<ChatMessage>();
    public List<ChatMessage> guildChannel = new List<ChatMessage>();
    public List<ChatMessage> allyChannel = new List<ChatMessage>();
    public List<ChatMessage> partyChannel = new List<ChatMessage>();
    public List<ChatMessage> whisperChannel = new List<ChatMessage>();
    public List<ChatMessage> localChannel = new List<ChatMessage>();

    // submit tries to send the string and then returns the new input text
    [Client]
    public string OnSubmit(string text)
    {
        // not empty and not only spaces?
        if (!string.IsNullOrWhiteSpace(text))
        {
            // command in the commands list?
            // note: we don't do 'break' so that one message could potentially
            //       be sent to multiple channels (see mmorpg local chat)
            string lastcommand = "";
            if (UIChatManager.singleton.selectedCategory == "whisper")
            {
                // whisper
                text.Replace("/w ", "");
                text = "/w " + text;
                string[] parsed = ParsePM(whisper.command, text);
                string user = parsed[0];
                string msg = parsed[1];
                if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(msg))
                {
                    if (user != name)
                    {
                        lastcommand = whisper.command + " " + user + " ";
                        CmdMsgWhisper(user, msg);
                    }
                    else print("cant whisper to self");
                }
                else print("invalid whisper format: " + user + "/" + msg);
            }
            else if (UIChatManager.singleton.lastSelected == "local")
            {
                // local chat is special: it has no command
                lastcommand = "";
                CmdMsgLocal(text);
            }
            else if (UIChatManager.singleton.selectedCategory == "party")
            {
                // party
                if (UIChatManager.singleton.selectedCategory == "party")
                {
                    text.Replace("/p ", "");
                    text = "/p " + text;
                }
                string msg = ParseGeneral(party.command, text);
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    lastcommand = party.command + " ";
                    CmdMsgParty(msg);
                }
            }
            else if (UIChatManager.singleton.selectedCategory == "group")
            {
                // guild
                if (UIChatManager.singleton.selectedCategory == "group")
                {
                    text.Replace("/g ", "");
                    text = "/g " + text;
                }
                string msg = ParseGeneral(guild.command, text);
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    lastcommand = guild.command + " ";
                    CmdMsgGuild(msg);
                }
            }

            else if (UIChatManager.singleton.selectedCategory == "alliance")
            {
                // guild
                if (UIChatManager.singleton.selectedCategory == "alliance")
                {
                    text.Replace("/a ", "");
                    text = "/a " + text;
                }
                string msg = ParseGeneral(ally.command, text);
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    lastcommand = guild.command + " ";
                    CmdMsgAllyGuild(msg);
                }
            }

            // addon system hooks
            Utils.InvokeMany(typeof(PlayerChat), this, "OnSubmit_", text);

            UIChat.singleton.messageInput.text = "";
            // input text should be set to lastcommand
            return lastcommand;
        }

        // input text should be cleared
        return "";
    }

    // parse a message of form "/command message"
    static string ParseGeneral(string command, string msg)
    {
        // return message without command prefix (if any)
        return msg.StartsWith(command + " ") ? msg.Substring(command.Length + 1) : "";
    }

    static string[] ParsePM(string command, string pm)
    {
        // parse to /w content
        string content = ParseGeneral(command, pm);

        // now split the content in "user msg"
        if (content != "")
        {
            // find the first space that separates the name and the message
            int i = content.IndexOf(" ");
            if (i >= 0)
            {
                string user = content.Substring(0, i);
                string msg = content.Substring(i + 1);
                return new string[] { user, msg };
            }
        }
        return new string[] { "", "" };
    }

    // networking //////////////////////////////////////////////////////////////
    [Command]
    void CmdMsgLocal(string message)
    {
        if (message.Length > maxLength) return;

        // it's local chat, so let's send it to all observers via ClientRpc
        //RpcMsgLocal(name, message);
        SendLocalMessage(message);
        for (int i = 0; i < player.quests.Count; i++)
        {
            Quest quest = player.quests[i];
            if (quest.data.sendAMessage == true)
            {
                quest.checkSendAMessage = true;
            }
            player.quests[i] = quest;
        }
    }

    [Command]
    void CmdMsgParty(string message)
    {
        if (message.Length > maxLength) return;

        // send message to all online party members
        if (player.InParty())
        {
            foreach (string member in player.party.members)
            {
                Player onlinePlayer;
                if (Player.onlinePlayers.TryGetValue(member, out onlinePlayer))
                {
                    // call TargetRpc on that GameObject for that connection
                    onlinePlayer.chat.TargetMsgParty(name, message);
                }
            }
            for (int i = 0; i < player.quests.Count; i++)
            {
                Quest quest = player.quests[i];
                if (quest.data.sendAMessage == true)
                {
                    quest.checkSendAMessage = true;
                }
                player.quests[i] = quest;
            }
        }
    }

    [Command]
    void CmdMsgGuild(string message)
    {
        if (message.Length > maxLength) return;

        // send message to all online guild members
        if (player.InGuild())
        {
            foreach (GuildMember member in player.guild.members)
            {
                Player onlinePlayer;
                if (Player.onlinePlayers.TryGetValue(member.name, out onlinePlayer))
                {
                    // call TargetRpc on that GameObject for that connection
                    onlinePlayer.chat.TargetMsgGuild(name, message);
                }
            }
            for (int i = 0; i < player.quests.Count; i++)
            {
                Quest quest = player.quests[i];
                if (quest.data.sendAMessage == true)
                {
                    quest.checkSendAMessage = true;
                }
                player.quests[i] = quest;
            }
        }
    }

    [Command]
    void CmdMsgAllyGuild(string message)
    {
        if (message.Length > maxLength) return;

        // send message to all online guild members
        if (player.InGuild())
        {
            foreach (GuildMember member in player.guild.members)
            {
                Player onlinePlayer;
                if (Player.onlinePlayers.TryGetValue(member.name, out onlinePlayer))
                {
                    // call TargetRpc on that GameObject for that connection
                    onlinePlayer.chat.TargetMsgAllyGuild(name, message);
                }
            }
            if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
            {
                if (player.playerRadio.radioItem.amount == 0 || player.playerRadio.radioItem.item.radioCurrentBattery == 0 || !player.playerRadio.isOn)
                {
                    TargetMsgInfo("Controlla la tua radio per parlare con gli alleati");
                    return;
                }
            }
            else
            {
                if (player.playerRadio.radioItem.amount == 0 || player.playerRadio.radioItem.item.radioCurrentBattery == 0 || !player.playerRadio.isOn)
                {
                    TargetMsgInfo("Check your Radio to comunicate with ally Group");
                    return;
                }

            }
            foreach (string s in player.playerAlliance.guildAlly)
            {
                if (GuildSystem.guilds.ContainsKey(s))
                {
                    Guild guild = GuildSystem.guilds[s];
                    foreach (GuildMember member in guild.members)
                    {
                        Player onlinePlayer;
                        if (Player.onlinePlayers.TryGetValue(member.name, out onlinePlayer))
                        {
                            // call TargetRpc on that GameObject for that connection
                            if (onlinePlayer.playerRadio.radioItem.amount > 0 || onlinePlayer.playerRadio.radioItem.item.radioCurrentBattery > 0 || onlinePlayer.playerRadio.isOn)
                            {
                                onlinePlayer.chat.TargetMsgAllyGuild(name, message);
                            }
                            else
                            {
                                if (Vector2.Distance(transform.position, onlinePlayer.transform.position) < 30.0f)
                                {
                                    onlinePlayer.chat.TargetMsgAllyGuild(name, message);
                                }
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < player.quests.Count; i++)
            {
                Quest quest = player.quests[i];
                if (quest.data.sendAMessage == true)
                {
                    quest.checkSendAMessage = true;
                }
                player.quests[i] = quest;
            }
        }
    }


    [Command]
    public void CmdMsgWhisper(string playerName, string message)
    {
        if (message.Length > maxLength) return;

        // find the player with that name
        Player onlinePlayer;
        if (Player.onlinePlayers.TryGetValue(playerName, out onlinePlayer))
        {
            // receiver gets a 'from' message, sender gets a 'to' message
            // (call TargetRpc on that GameObject for that connection)
            onlinePlayer.chat.TargetMsgWhisperFrom(name, message);
            TargetMsgWhisperTo(playerName, message);
            for (int i = 0; i < player.quests.Count; i++)
            {
                Quest quest = player.quests[i];
                if (quest.data.sendAMessage == true)
                {
                    quest.checkSendAMessage = true;
                }
                player.quests[i] = quest;
            }
        }
    }

    // send a global info message to everyone
    [Server]
    public void SendGlobalMessage(string message)
    {
        foreach (Player onlinePlayer in Player.onlinePlayers.Values)
            player.chat.TargetMsgInfo(message);
        for (int i = 0; i < player.quests.Count; i++)
        {
            Quest quest = player.quests[i];
            if (quest.data.sendAMessage == true)
            {
                quest.checkSendAMessage = true;
            }
            player.quests[i] = quest;
        }
    }

    [Server]
    public void SendLocalMessage(string message)
    {
        foreach (Player onlinePlayer in Player.onlinePlayers.Values)
            onlinePlayer.chat.TargetMsgLocal(player.name, message);
        for (int i = 0; i < player.quests.Count; i++)
        {
            Quest quest = player.quests[i];
            if (quest.data.sendAMessage == true)
            {
                quest.checkSendAMessage = true;
            }
            player.quests[i] = quest;
        }
    }

    // message handlers ////////////////////////////////////////////////////////
    [TargetRpc] // only send to one client
    public void TargetMsgWhisperFrom(string sender, string message)
    {
        // add message with identifierIn
        string identifier = whisper.identifierIn;
        string reply = whisper.command + " " + sender + " "; // whisper
        //UIChat.singleton.AddMessage(new ChatMessage(sender, identifier, message, reply, whisper.textPrefab));

        player.chat.whisperChannel.Add(new ChatMessage(sender, identifier, message, reply, whisper.textPrefab));
        if (UIChatManager.singleton.lastSelected != "whisper") player.chat.whisperMessage++;
    }

    [TargetRpc] // only send to one client
    public void TargetMsgWhisperTo(string receiver, string message)
    {
        // add message with identifierOut
        string identifier = whisper.identifierOut;
        string reply = whisper.command + " " + receiver + " "; // whisper
                                                               //UIChat.singleton.AddMessage(new ChatMessage(receiver, identifier, message, reply, whisper.textPrefab));

        player.chat.whisperChannel.Add(new ChatMessage(receiver, identifier, message, reply, whisper.textPrefab));
        if (UIChatManager.singleton.lastSelected != "whisper") player.chat.whisperMessage++;
    }

    [ClientRpc]
    public void RpcMsgLocal(string sender, string message)
    {
        // add message with identifierIn or Out depending on who sent it
        string identifier = sender != name ? local.identifierIn : local.identifierOut;
        string reply = whisper.command + " " + sender + " "; // whisper
        //UIChat.singleton.AddMessage(new ChatMessage(sender, identifier, message, reply, local.textPrefab));

        player.chat.localChannel.Add(new ChatMessage(sender, identifier, message, reply, local.textPrefab));
        if (UIChatManager.singleton.lastSelected != "local") player.chat.localMessage++;
    }

    [TargetRpc] // only send to one client
    public void TargetMsgGuild(string sender, string message)
    {
        string reply = whisper.command + " " + sender + " "; // whisper
        //UIChat.singleton.AddMessage(new ChatMessage(sender, guild.identifierIn, message, reply, guild.textPrefab));

        player.chat.guildChannel.Add(new ChatMessage(sender, guild.identifierIn, message, reply, guild.textPrefab));
        if (UIChatManager.singleton.lastSelected != "group") player.chat.guildMessage++;
    }

    [TargetRpc] // only send to one client
    public void TargetMsgAllyGuild(string sender, string message)
    {
        string reply = whisper.command + " " + sender + " "; // whisper
        //UIChat.singleton.AddMessage(new ChatMessage(sender, ally.identifierIn, message, reply, ally.textPrefab));

        player.chat.allyChannel.Add(new ChatMessage(sender, ally.identifierIn, message, reply, ally.textPrefab));
        if (UIChatManager.singleton.lastSelected != "alliance") player.chat.allyMessage++;
    }

    [TargetRpc] // only send to one client
    public void TargetMsgParty(string sender, string message)
    {
        string reply = whisper.command + " " + sender + " "; // whisper
        //UIChat.singleton.AddMessage(new ChatMessage(sender, party.identifierIn, message, reply, party.textPrefab));

        player.chat.partyChannel.Add(new ChatMessage(sender, party.identifierIn, message, reply, party.textPrefab));
        if (UIChatManager.singleton.lastSelected != "party") player.chat.partyMessage++;
    }

    [TargetRpc] // only send to one client
    public void TargetMsgInfo(string message)
    {
        //UIChat.singleton.AddMessage(new ChatMessage("", info.identifierIn, message, "", info.textPrefab));
        player.chat.infoChannel.Add(new ChatMessage("", info.identifierIn, message, "", info.textPrefab));
        if (UIChatManager.singleton.lastSelected != "info") player.chat.infoMessage++;
    }

    [TargetRpc]
    public void TargetMsgLocal(string sender, string message)
    {
        // add message with identifierIn or Out depending on who sent it
        string identifier = sender != name ? local.identifierIn : local.identifierOut;
        string reply = whisper.command + " " + sender + " "; // whisper
        //UIChat.singleton.AddMessage(new ChatMessage(sender, identifier, message, reply, local.textPrefab));

        player.chat.localChannel.Add(new ChatMessage(sender, identifier, message, reply, local.textPrefab));
        if (UIChatManager.singleton.lastSelected != "local") player.chat.localMessage++;
    }

    // info message can be added from client too
    public void AddMsgInfo(string message)
    {
        //UIChat.singleton.AddMessage(new ChatMessage("", info.identifierIn, message, "", info.textPrefab));
        player.chat.infoChannel.Add(new ChatMessage("", info.identifierIn, message, "", info.textPrefab));
        if (UIChatManager.singleton.lastSelected != "info") player.chat.infoMessage++;
    }

}

public partial class PlayerBuilding
{
    public Player player;

    public ScriptableBuilding building;
    public int inventoryIndex;
    public GameObject actualBuilding;
    public bool invBelt = true;

    public string flagSelectedNation;

    public GameObject selectedGameObject;
    public int selectedInventoryIndex;
    public string selectedNationOut;
    public string selectedName;
    public string selectedGroup;

    public GameObject g;
    public ModularObject ModularObject;

    public Forniture nearForniture;

    public void CheckSpawnInventory()
    {
        if (selectedGameObject.GetComponent<Building>().CanSpawn())
        {
            if (((ScriptableBuilding)player.inventory[selectedInventoryIndex].item.data).groupWarehouse && player.InGuild() || !((ScriptableBuilding)player.inventory[selectedInventoryIndex].item.data).groupWarehouse)
            {
                BuildingManager.singleton.AddToList(selectedGameObject);
                Building buildingObject = selectedGameObject.GetComponent<Building>();
                buildingObject.owner = name;
                buildingObject.guild = player.InGuild() ? player.guild.name : string.Empty;
                buildingObject.obstacle = ((ScriptableBuilding)player.inventory[selectedInventoryIndex].item.data).isObstacle;
                if (buildingObject.GetComponent<WoodWall>())
                {
                    buildingObject.GetComponent<WoodWall>().side = buildingObject.actualBuildinigRotation;
                }
                if (buildingObject.GetComponent<Flag>() && selectedNationOut != string.Empty)
                {
                    buildingObject.GetComponent<Flag>().selectedNation = selectedNationOut;
                }
                if (buildingObject.GetComponent<Mine>())
                {
                    buildingObject.GetComponent<SpriteRenderer>().sprite = null;
                }
                Destroy(buildingObject.placement.gameObject);
                NetworkServer.Spawn(selectedGameObject);

                ItemSlot slot = player.inventory[selectedInventoryIndex];
                slot.amount--;
                player.inventory[selectedInventoryIndex] = slot;
                player.playerLeaderPoints.buildinPoint += GeneralManager.singleton.buildingCreatePoint;
                for (int i = 0; i < player.quests.Count; i++)
                {
                    Quest quest = player.quests[i];
                    if (quest.createBuilding == true)
                    {
                        quest.checkCreateBuilding = true;
                        player.quests[i] = quest;
                    }
                }
            }
        }
        else
        {
            Destroy(selectedGameObject);
        }
    }

    public void CheckSpawnBelt()
    {
        if (selectedGameObject.GetComponent<Building>().CanSpawn())
        {
            if (((ScriptableBuilding)player.playerBelt.belt[selectedInventoryIndex].item.data).groupWarehouse && player.InGuild() || !((ScriptableBuilding)player.playerBelt.belt[selectedInventoryIndex].item.data).groupWarehouse)
            {
                BuildingManager.singleton.AddToList(selectedGameObject);
                Building buildingObject = selectedGameObject.GetComponent<Building>();
                buildingObject.owner = name;
                buildingObject.guild = player.InGuild() ? player.guild.name : string.Empty;
                buildingObject.obstacle = ((ScriptableBuilding)player.playerBelt.belt[selectedInventoryIndex].item.data).isObstacle;
                if (buildingObject.GetComponent<WoodWall>())
                {
                    buildingObject.GetComponent<WoodWall>().side = buildingObject.actualBuildinigRotation;
                }
                if (buildingObject.GetComponent<Flag>() && selectedNationOut != string.Empty)
                {
                    buildingObject.GetComponent<Flag>().selectedNation = selectedNationOut;
                }
                if (buildingObject.GetComponent<Mine>())
                {
                    buildingObject.GetComponent<SpriteRenderer>().sprite = null;
                }
                Destroy(buildingObject.placement.gameObject);
                NetworkServer.Spawn(selectedGameObject);

                ItemSlot slot = player.playerBelt.belt[selectedInventoryIndex];
                slot.amount--;
                player.playerBelt.belt[selectedInventoryIndex] = slot;
                player.playerLeaderPoints.buildinPoint += GeneralManager.singleton.buildingCreatePoint;
                for (int i = 0; i < player.quests.Count; i++)
                {
                    Quest quest = player.quests[i];
                    if (quest.createBuilding == true)
                    {
                        quest.checkCreateBuilding = true;
                        player.quests[i] = quest;
                    }
                }
            }
        }
        else
        {
            Destroy(selectedGameObject);
        }
    }

    public void CheckSpawnFornitureInventory()
    {
        if (player.playerBuilding.actualBuilding)
        {
            ModularObject modularObject = player.playerBuilding.actualBuilding.GetComponent<ModularObject>();
            if (modularObject)
            {
                if (((ScriptableBuilding)player.inventory[selectedInventoryIndex].item.data).groupWarehouse && player.InGuild() || !((ScriptableBuilding)player.inventory[selectedInventoryIndex].item.data).groupWarehouse)
                {
                    BuildingManager.singleton.AddToList(selectedGameObject);
                    Destroy(modularObject.placement.gameObject);
                    NetworkServer.Spawn(selectedGameObject);

                    ItemSlot slot = player.inventory[selectedInventoryIndex];
                    slot.amount--;
                    player.inventory[selectedInventoryIndex] = slot;
                }
            }
            else
            {
                Destroy(selectedGameObject);
            }
        }
    }

    public void CheckSpawnFornitureBelt()
    {
        if (player.playerBuilding.actualBuilding)
        {
            ModularObject modularObject = player.playerBuilding.actualBuilding.GetComponent<ModularObject>();
            if (modularObject)
            {
                if (((ScriptableBuilding)player.inventory[selectedInventoryIndex].item.data).groupWarehouse && player.InGuild() || !((ScriptableBuilding)player.inventory[selectedInventoryIndex].item.data).groupWarehouse)
                {
                    BuildingManager.singleton.AddToList(selectedGameObject);
                    Destroy(modularObject.placement.gameObject);
                    NetworkServer.Spawn(selectedGameObject);

                    ItemSlot slot = player.inventory[selectedInventoryIndex];
                    slot.amount--;
                    player.inventory[selectedInventoryIndex] = slot;
                }
            }
            else
            {
                Destroy(selectedGameObject);
            }
        }
    }

    public void CheckSpawnFornitureObjectInventory()
    {
        if (ModularObject.canSpawn)
        {
            ModularObject.owner = selectedName;
            ModularObject.guild = selectedGroup;
            ModularObject.objectIndex = ModularBuildingManager.singleton.GetNewIndexForniture();
            BuildingManager.singleton.AddToList(g);
            Destroy(ModularObject.placement.gameObject);
            NetworkServer.Spawn(g);

            ItemSlot slot = player.inventory[selectedInventoryIndex];
            slot.amount--;
            player.inventory[selectedInventoryIndex] = slot;
        }
        else
        {
            Destroy(g);
        }
    }

    public void CheckSpawnFornitureObjectBelt()
    {
        if (ModularObject.canSpawn)
        {
            ModularObject.owner = selectedName;
            ModularObject.guild = selectedGroup;
            BuildingManager.singleton.AddToList(g);
            Destroy(ModularObject.placement.gameObject);
            NetworkServer.Spawn(g);

            ItemSlot slot = player.playerBelt.belt[selectedInventoryIndex];
            slot.amount--;
            player.playerBelt.belt[selectedInventoryIndex] = slot;
        }
        else
        {
            Destroy(g);
        }
    }

    [Command]
    public void CmdSpawnBuilding(int inventoryIndex, string itemName, int buildingRotation, Vector2 buildingTransform, bool inventory, string selectedNation)
    {
        if (!inventory)
        {
            if (player.inventory[inventoryIndex].amount > 0 && player.inventory[inventoryIndex].item.name == itemName && player.inventory[inventoryIndex].item.data is ScriptableBuilding)
            {
                GameObject g = Instantiate(((ScriptableBuilding)player.inventory[inventoryIndex].item.data).buildingList[buildingRotation].buildingObject, buildingTransform, Quaternion.identity);

                selectedGameObject = g;
                selectedInventoryIndex = inventoryIndex;
                selectedNationOut = selectedNation;

                Invoke(nameof(CheckSpawnInventory), 1.0f);
            }
        }
        else
        {
            if (player.playerBelt.belt[inventoryIndex].amount > 0 && player.playerBelt.belt[inventoryIndex].item.name == itemName && player.playerBelt.belt[inventoryIndex].item.data is ScriptableBuilding)
            {
                GameObject g = Instantiate(((ScriptableBuilding)player.playerBelt.belt[inventoryIndex].item.data).buildingList[buildingRotation].buildingObject, buildingTransform, Quaternion.identity);

                selectedGameObject = g;
                selectedInventoryIndex = inventoryIndex;
                selectedNationOut = selectedNation;

                Invoke(nameof(CheckSpawnBelt), 1.0f);
            }
        }
    }

    [Command]
    public void CmdSpawnBasement(int inventoryIndex, string itemName, Vector2 buildingTransform, bool inventory, int buildingType, bool isInitialBasement, int modularIndex, bool isMain)
    {
        if (buildingType < 0) buildingType = 0;

        if (!inventory)
        {
            if (player.inventory[inventoryIndex].amount > 0 && player.inventory[inventoryIndex].item.name == itemName && player.inventory[inventoryIndex].item.data is ScriptableBuilding)
            {
                g = Instantiate(((ScriptableBuilding)player.inventory[inventoryIndex].item.data).buildingList[buildingType].buildingObject, buildingTransform, ((ScriptableBuilding)player.inventory[inventoryIndex].item.data).buildingList[buildingType].buildingObject.transform.rotation);

                ModularPiece modularPiece = g.GetComponent<ModularPiece>();
                ModularObject modularObject = g.GetComponent<ModularObject>();

                if (modularPiece)
                {
                    if (modularPiece.CanSpawn())
                    {
                        BuildingManager.singleton.AddToList(g);
                        ModularPiece buildingObject = modularPiece;
                        buildingObject.owner = name;
                        buildingObject.isMain = isMain;
                        buildingObject.guild = player.InGuild() ? player.guild.name : string.Empty;
                        buildingObject.modularIndex = isInitialBasement ? ModularBuildingManager.singleton.GetNewIndex() : modularIndex == -5 ? ModularBuildingManager.singleton.GetNewIndex() : modularIndex;
                        NetworkServer.Spawn(g);

                        ItemSlot slot = player.inventory[inventoryIndex];
                        slot.amount--;
                        player.inventory[inventoryIndex] = slot;
                        for (int i = 0; i < player.quests.Count; i++)
                        {
                            Quest quest = player.quests[i];
                            if (quest.createBuilding == true)
                            {
                                quest.checkCreateBuilding = true;
                                player.quests[i] = quest;
                            }
                        }
                    }
                    else
                    {
                        Destroy(g);
                    }
                }
                else if (modularObject)
                {

                    selectedGameObject = g;
                    selectedInventoryIndex = inventoryIndex;
                    ModularObject = modularObject;
                    selectedName = player.name;
                    selectedGroup = player.guild.name;

                    Invoke(nameof(CheckSpawnFornitureObjectInventory), 1.0f);

                }
                else
                {
                    Destroy(g);
                }
            }
        }
        else
        {
            if (player.playerBelt.belt[inventoryIndex].amount > 0 && player.playerBelt.belt[inventoryIndex].item.name == itemName && player.playerBelt.belt[inventoryIndex].item.data is ScriptableBuilding)
            {
                g = Instantiate(((ScriptableBuilding)player.playerBelt.belt[inventoryIndex].item.data).buildingList[buildingType].buildingObject, buildingTransform, Quaternion.identity);

                ModularPiece modularPiece = g.GetComponent<ModularPiece>();
                ModularObject modularObject = g.GetComponent<ModularObject>();

                if (modularPiece)
                {
                    if (modularPiece.CanSpawn())
                    {
                        BuildingManager.singleton.AddToList(g);
                        ModularPiece buildingObject = modularPiece;
                        buildingObject.owner = name;
                        buildingObject.isMain = isMain;
                        buildingObject.guild = player.InGuild() ? player.guild.name : string.Empty;
                        buildingObject.modularIndex = isInitialBasement ? ModularBuildingManager.singleton.GetNewIndex() : modularIndex;
                        NetworkServer.Spawn(g);
                        ItemSlot slot = player.playerBelt.belt[inventoryIndex];
                        slot.amount--;
                        player.playerBelt.belt[inventoryIndex] = slot;
                        for (int i = 0; i < player.quests.Count; i++)
                        {
                            Quest quest = player.quests[i];
                            if (quest.createBuilding == true)
                            {
                                quest.checkCreateBuilding = true;
                                player.quests[i] = quest;
                            }
                        }
                    }
                    else
                    {
                        Destroy(g);
                    }
                }
                else if (modularObject)
                {
                    selectedGameObject = g;
                    selectedInventoryIndex = inventoryIndex;
                    ModularObject = modularObject;
                    selectedName = player.name;
                    selectedGroup = player.guild.name;

                    Invoke(nameof(CheckSpawnFornitureObjectBelt), 1.0f);

                }
                else
                {
                    Destroy(g);
                }

            }
        }
    }

    [Command]
    public void CmdSyncWallDoor(NetworkIdentity identity, int up, int down, int left, int right, bool inventory, int inventoryIndex)
    {
        ModularPiece modular = identity.GetComponent<ModularPiece>();
        if (modular)
        {
            if (modular.upComponent == -5) modular.upComponent = up;
            if (modular.downComponent == -5) modular.downComponent = down;
            if (modular.leftComponent == -5) modular.leftComponent = left;
            if (modular.rightComponent == -5) modular.rightComponent = right;

            if (!inventory)
            {
                ItemSlot slot = player.inventory[inventoryIndex];
                slot.amount--;
                player.inventory[inventoryIndex] = slot;
                for (int i = 0; i < player.quests.Count; i++)
                {
                    Quest quest = player.quests[i];
                    if (quest.createBuilding == true)
                    {
                        quest.checkCreateBuilding = true;
                        player.quests[i] = quest;
                    }
                }
            }
            else
            {
                ItemSlot slot = player.playerBelt.belt[inventoryIndex];
                slot.amount--;
                player.playerBelt.belt[inventoryIndex] = slot;
                for (int i = 0; i < player.quests.Count; i++)
                {
                    Quest quest = player.quests[i];
                    if (quest.createBuilding == true)
                    {
                        quest.checkCreateBuilding = true;
                        player.quests[i] = quest;
                    }
                }
            }
        }
    }

    [Command]
    public void CmdClearWallDoor(NetworkIdentity identity, int up, int down, int left, int right)
    {
        ModularPiece modular = identity.GetComponent<ModularPiece>();
        Collider2D[] wallColliders = new Collider2D[0];
        if (modular)
        {
            if (up == -5)
            {
                if (modular.upComponent == 0)
                {
                    wallColliders = Physics2D.OverlapBoxAll(transform.position, new Vector2(((BoxCollider2D)modular.upWall.GetComponent<BoxCollider2D>()).size.x, ((BoxCollider2D)modular.upWall.GetComponent<BoxCollider2D>()).size.y), 0, GeneralManager.singleton.modularObjectToDelete);
                }
                else if (modular.upComponent == 1)
                {
                    wallColliders = Physics2D.OverlapBoxAll(transform.position, new Vector2(((BoxCollider2D)modular.upDoor.GetComponent<BoxCollider2D>()).size.x, ((BoxCollider2D)modular.upDoor.GetComponent<BoxCollider2D>()).size.y), 0, GeneralManager.singleton.modularObjectToDelete);
                }
                for (int i = 0; i < wallColliders.Length; i++)
                {
                    int index = i;
                    BuildingManager.singleton.RemoveFromList(wallColliders[index].gameObject);
                    NetworkServer.Destroy(wallColliders[index].gameObject);
                }
            }
            if (modular.upComponent != -5) modular.upComponent = up;
            if (modular.downComponent != -5) modular.downComponent = down;
            if (modular.leftComponent != -5) modular.leftComponent = left;
            if (modular.rightComponent != -5) modular.rightComponent = right;
        }
    }

    [Command]
    public void CmdDestroyBasement(NetworkIdentity identity)
    {
        ModularPiece modular = identity.GetComponent<ModularPiece>();
        Collider2D[] floorColliders;
        Collider2D[] nextColliders;
        Collider2D[] wallColliders = new Collider2D[0];
        if (modular)
        {
            if (modular.isMain)
            {
                if (modular.occupiedLEFT)
                {
                    nextColliders = Physics2D.OverlapBoxAll(modular.leftFloorPointer.transform.position, new Vector2(((BoxCollider2D)modular.leftFloorPointer.GetComponent<BoxCollider2D>()).size.x, ((BoxCollider2D)modular.leftFloorPointer.GetComponent<BoxCollider2D>()).size.x), 0, GeneralManager.singleton.modularObjectLayerMask);
                    ModularPiece next = nextColliders[0].GetComponent<ModularPiece>();
                    next.isMain = true;
                    next.guild = modular.guild;
                    next.owner = modular.owner;
                    next.RpcRebuildMain(next.GetComponent<NetworkIdentity>(), true);
                }
                else if (modular.occupiedRIGHT)
                {
                    nextColliders = Physics2D.OverlapBoxAll(modular.rightFloorPointer.transform.position, new Vector2(((BoxCollider2D)modular.rightFloorPointer.GetComponent<BoxCollider2D>()).size.x, ((BoxCollider2D)modular.rightFloorPointer.GetComponent<BoxCollider2D>()).size.x), 0, GeneralManager.singleton.modularObjectLayerMask);
                    ModularPiece next = nextColliders[0].GetComponent<ModularPiece>();
                    next.isMain = true;
                    next.guild = modular.guild;
                    next.owner = modular.owner;
                    next.RpcRebuildMain(next.GetComponent<NetworkIdentity>(), true);
                }
                else if (modular.occupiedUP)
                {
                    nextColliders = Physics2D.OverlapBoxAll(modular.upFloorPointer.transform.position, new Vector2(((BoxCollider2D)modular.upFloorPointer.GetComponent<BoxCollider2D>()).size.x, ((BoxCollider2D)modular.upFloorPointer.GetComponent<BoxCollider2D>()).size.x), 0, GeneralManager.singleton.modularObjectLayerMask);
                    ModularPiece next = nextColliders[0].GetComponent<ModularPiece>();
                    next.isMain = true;
                    next.guild = modular.guild;
                    next.owner = modular.owner;
                    next.RpcRebuildMain(next.GetComponent<NetworkIdentity>(), true);
                }
                else if (modular.occupiedDOWN)
                {
                    nextColliders = Physics2D.OverlapBoxAll(modular.downFloorPointer.transform.position, new Vector2(((BoxCollider2D)modular.downFloorPointer.GetComponent<BoxCollider2D>()).size.x, ((BoxCollider2D)modular.downFloorPointer.GetComponent<BoxCollider2D>()).size.x), 0, GeneralManager.singleton.modularObjectLayerMask);
                    ModularPiece next = nextColliders[0].GetComponent<ModularPiece>();
                    next.isMain = true;
                    next.guild = modular.guild;
                    next.owner = modular.owner;
                    next.RpcRebuildMain(next.GetComponent<NetworkIdentity>(), true);
                }
            }

            nextColliders = Physics2D.OverlapBoxAll(modular.leftFloorPointer.transform.position, new Vector2(((BoxCollider2D)modular.leftFloorPointer.GetComponent<BoxCollider2D>()).size.x, ((BoxCollider2D)modular.leftFloorPointer.GetComponent<BoxCollider2D>()).size.x), 0, GeneralManager.singleton.modularObjectLayerMask);
            if (nextColliders.Length > 0)
            {
                nextColliders[0].GetComponent<ModularPiece>().occupiedRIGHT = false;
            }
            nextColliders = Physics2D.OverlapBoxAll(modular.rightFloorPointer.transform.position, new Vector2(((BoxCollider2D)modular.rightFloorPointer.GetComponent<BoxCollider2D>()).size.x, ((BoxCollider2D)modular.rightFloorPointer.GetComponent<BoxCollider2D>()).size.x), 0, GeneralManager.singleton.modularObjectLayerMask);
            if (nextColliders.Length > 0)
            {
                nextColliders[0].GetComponent<ModularPiece>().occupiedLEFT = false;
            }
            nextColliders = Physics2D.OverlapBoxAll(modular.upFloorPointer.transform.position, new Vector2(((BoxCollider2D)modular.upFloorPointer.GetComponent<BoxCollider2D>()).size.x, ((BoxCollider2D)modular.upFloorPointer.GetComponent<BoxCollider2D>()).size.x), 0, GeneralManager.singleton.modularObjectLayerMask);
            if (nextColliders.Length > 0)
            {
                nextColliders[0].GetComponent<ModularPiece>().occupiedDOWN = false;
            }
            nextColliders = Physics2D.OverlapBoxAll(modular.downFloorPointer.transform.position, new Vector2(((BoxCollider2D)modular.downFloorPointer.GetComponent<BoxCollider2D>()).size.x, ((BoxCollider2D)modular.downFloorPointer.GetComponent<BoxCollider2D>()).size.x), 0, GeneralManager.singleton.modularObjectLayerMask);
            if (nextColliders.Length > 0)
            {
                nextColliders[0].GetComponent<ModularPiece>().occupiedUP = false;
            }

            floorColliders = Physics2D.OverlapBoxAll(transform.position, new Vector2(((BoxCollider2D)modular.modularCollider).size.x, ((BoxCollider2D)modular.modularCollider).size.y), 0, GeneralManager.singleton.modularObjectToDelete);

            for (int i = 0; i < floorColliders.Length; i++)
            {
                int index = i;
                BuildingManager.singleton.RemoveFromList(floorColliders[index].gameObject);
                NetworkServer.Destroy(floorColliders[index].gameObject);
            }

            if (modular.upComponent == 0)
            {
                wallColliders = Physics2D.OverlapBoxAll(transform.position, new Vector2(((BoxCollider2D)modular.upWall.GetComponent<Collider2D>()).size.x, ((BoxCollider2D)modular.upWall.GetComponent<Collider2D>()).size.y), 0, GeneralManager.singleton.modularObjectToDelete);
            }
            else if (modular.upComponent == 1)
            {
                wallColliders = Physics2D.OverlapBoxAll(transform.position, new Vector2(((BoxCollider2D)modular.upDoor.GetComponent<Collider2D>()).size.x, ((BoxCollider2D)modular.upDoor.GetComponent<Collider2D>()).size.y), 0, GeneralManager.singleton.modularObjectToDelete);
            }

            for (int i = 0; i < wallColliders.Length; i++)
            {
                int index = i;
                BuildingManager.singleton.RemoveFromList(wallColliders[index].gameObject);
                NetworkServer.Destroy(wallColliders[index].gameObject);
            }
            for (int i = 0; i < modular.buildingColliders.Count; i++)
            {
                int index = i;
                BuildingManager.singleton.RemoveFromList(modular.buildingColliders[index].gameObject);
                NetworkServer.Destroy(modular.buildingColliders[index].gameObject);
            }
            for (int i = 0; i < modular.fornitureColliders.Count; i++)
            {
                int index = i;
                NetworkServer.Destroy(modular.fornitureColliders[index].gameObject);
            }

            NetworkServer.Destroy(modular.gameObject);
        }
    }


    [Command]
    public void CmdCraftItem(string buildingItem, int itemIndex, int currencyType, string actualTime)
    {
        ScriptableItem building = GeneralManager.singleton.CraftingBuildItem(buildingItem);
        ItemInBuilding itemInBuilding = GeneralManager.singleton.CraftingInternalBuilding(building, itemIndex);
        BuildingCraft buildingCraft = player.target.GetComponent<BuildingCraft>();
        Building buildingTarget = player.target.GetComponent<Building>();
        List<CraftItem> progressItem = new List<CraftItem>();
        List<CraftItem> finishedItem = new List<CraftItem>();
        TimeSpan difference;

        for (int i = 0; i < buildingCraft.craftItem.Count; i++)
        {
            int index = i;
            if (DateTime.Parse(buildingCraft.craftItem[index].timeEndServer) < System.DateTime.Now)
            {
                progressItem.Add(new CraftItem());
            }
        }
        if (buildingCraft && buildingTarget && buildingTarget.isPremiumZone)
        {
        }
        else if ((progressItem.Count >= (buildingTarget.level - 1) && progressItem.Count != 0) && buildingCraft && buildingTarget && !buildingTarget.isPremiumZone)
        {
            return;
        }


        if (!player.InventoryCanAdd(new Item(itemInBuilding.itemToCraft.item), 1))
        {
            return;
        }
        for (int i = 0; i < itemInBuilding.craftablengredient.Count; i++)
        {
            int index = i;
            if (player.InventoryCount(new Item(itemInBuilding.craftablengredient[index].item)) < itemInBuilding.craftablengredient[index].amount)
            {
                return;
            }
        }

        if (currencyType == 0)
        {
            if (player.gold >= itemInBuilding.itemToCraft.item.goldPrice)
            {
                player.gold -= itemInBuilding.itemToCraft.item.goldPrice;
                for (int i = 0; i < itemInBuilding.craftablengredient.Count; i++)
                {
                    int index = i;
                    player.InventoryRemove(new Item(itemInBuilding.craftablengredient[index].item), itemInBuilding.craftablengredient[index].amount);
                }
            }
        }
        if (currencyType == 1)
        {
            if (player.coins >= itemInBuilding.itemToCraft.item.coinPrice)
            {
                player.coins -= itemInBuilding.itemToCraft.item.coinPrice;
                for (int i = 0; i < itemInBuilding.craftablengredient.Count; i++)
                {
                    int index = i;
                    player.InventoryRemove(new Item(itemInBuilding.craftablengredient[index].item), itemInBuilding.craftablengredient[index].amount);
                }
            }
        }

        CraftItem crafItem = new CraftItem();
        crafItem.itemName = itemInBuilding.itemToCraft.item.name;
        crafItem.amount = itemInBuilding.itemToCraft.amount;
        crafItem.remainingTime = itemInBuilding.itemToCraft.item.timeToCraft;
        crafItem.totalTime = itemInBuilding.itemToCraft.item.timeToCraft;
        crafItem.owner = name;
        crafItem.guildName = player.guild.name;
        crafItem.timeBegin = actualTime.ToString();
        crafItem.timeEnd = DateTime.Parse(actualTime).AddSeconds(itemInBuilding.itemToCraft.item.timeToCraft).ToString();
        crafItem.timeEndServer = GeneralManager.singleton.ChangeServerToClientTime(DateTime.Parse(System.DateTime.Now.ToString(), GeneralManager.singleton.culture), itemInBuilding.itemToCraft.item.timeToCraft).ToString();
        player.target.GetComponent<BuildingCraft>().craftItem.Add(crafItem);
        player.playerLeaderPoints.craftItemPoint += GeneralManager.singleton.craftItemPoint;
    }

    [Command]
    public void CmdCraftItemForniture(string buildingItem, int itemIndex, int currencyType, string actualTime)
    {
        ScriptableItem building = GeneralManager.singleton.CraftingBuildItem(buildingItem);
        ItemInBuilding itemInBuilding = GeneralManager.singleton.CraftingInternalBuilding(building, itemIndex);
        BuildingModularCrafting modulatCrafting = player.playerMove.forniture.GetComponent<BuildingModularCrafting>();
        List<int> progressItem = new List<int>();
        List<int> finishedItem = new List<int>();
        TimeSpan difference;

        for (int i = 0; i < modulatCrafting.craftItem.Count; i++)
        {
            int index = i;
            if (DateTime.Parse(modulatCrafting.craftItem[index].timeEndServer) < System.DateTime.Now)
            {
                progressItem.Add(index);
            }
        }

        if (currencyType == 0)
        {
            for (int i = 0; i < itemInBuilding.craftablengredient.Count; i++)
            {
                int index = i;
                if (player.InventoryCount(new Item(itemInBuilding.craftablengredient[index].item)) < itemInBuilding.craftablengredient[index].amount)
                {
                    return;
                }
            }
            if (player.gold >= itemInBuilding.itemToCraft.item.goldPrice)
            {
                player.gold -= itemInBuilding.itemToCraft.item.goldPrice;
                for (int i = 0; i < itemInBuilding.craftablengredient.Count; i++)
                {
                    int index = i;
                    player.InventoryRemove(new Item(itemInBuilding.craftablengredient[index].item), itemInBuilding.craftablengredient[index].amount);
                }
            }
            else
            {
                return;
            }
        }
        if (currencyType == 1)
        {
            for (int i = 0; i < itemInBuilding.craftablengredient.Count; i++)
            {
                int index = i;
                if (player.InventoryCount(new Item(itemInBuilding.craftablengredient[index].item)) < itemInBuilding.craftablengredient[index].amount)
                {
                    return;
                }
            }
            if (player.coins >= itemInBuilding.itemToCraft.item.coinPrice)
            {
                player.coins -= itemInBuilding.itemToCraft.item.coinPrice;
                for (int i = 0; i < itemInBuilding.craftablengredient.Count; i++)
                {
                    int index = i;
                    player.InventoryRemove(new Item(itemInBuilding.craftablengredient[index].item), itemInBuilding.craftablengredient[index].amount);
                }
            }
            else
            {
                return;
            }
        }
        if (currencyType == -1)
        {
            if (player.coins >= itemInBuilding.itemToCraft.item.coinPrice)
            {
                player.coins -= itemInBuilding.itemToCraft.item.coinPrice;
            }
            else
            {
                return;
            }
        }

        CraftItem crafItem = new CraftItem();
        crafItem.itemName = itemInBuilding.itemToCraft.item.name;
        crafItem.amount = itemInBuilding.itemToCraft.amount;
        crafItem.remainingTime = itemInBuilding.itemToCraft.item.timeToCraft;
        crafItem.totalTime = itemInBuilding.itemToCraft.item.timeToCraft;
        crafItem.owner = name;
        crafItem.guildName = player.guild.name;
        crafItem.timeBegin = actualTime.ToString();
        crafItem.timeEnd = DateTime.Parse(actualTime).AddSeconds(itemInBuilding.itemToCraft.item.timeToCraft).ToString();
        crafItem.timeEndServer = GeneralManager.singleton.ChangeServerToClientTime(DateTime.Parse(System.DateTime.Now.ToString(), GeneralManager.singleton.culture), itemInBuilding.itemToCraft.item.timeToCraft).ToString();
        player.playerMove.forniture.GetComponent<BuildingModularCrafting>().craftItem.Add(crafItem);
        player.playerLeaderPoints.craftItemPoint += GeneralManager.singleton.craftItemPoint;
    }

    [Command]
    public void CmdRemoveItemFromCrafting(int index, NetworkIdentity identity)
    {
        BuildingModularCrafting buildingModularCrafting = identity.gameObject.GetComponent<BuildingModularCrafting>();

        if (buildingModularCrafting)
        {
            buildingModularCrafting.craftItem.Remove(buildingModularCrafting.craftItem[index]);
        }
    }



    [Command]
    public void CmdCraftBuildingItem(string craftableBuilding, int currencyType)
    {
        ScriptableItem itemToCraft = null;
        ScriptableItem building = GeneralManager.singleton.CraftingBuildItem("Building Crafter");
        if (!player.InventoryCanAdd(new Item(building), 1))
        {
            return;
        }
        for (int i = 0; i < GeneralManager.singleton.buildingItems.Count; i++)
        {
            int index = i;
            if (GeneralManager.singleton.buildingItems[index].specificBuilding.name == building.name)
            {
                for (int e = 0; e < GeneralManager.singleton.buildingItems[index].buildingItem.Count; e++)
                {
                    int indexe = e;
                    if (GeneralManager.singleton.buildingItems[index].buildingItem[indexe].itemToCraft.item.name == craftableBuilding)
                    {
                        itemToCraft = GeneralManager.singleton.buildingItems[index].buildingItem[indexe].itemToCraft.item;
                        for (int a = 0; a < GeneralManager.singleton.buildingItems[index].buildingItem[indexe].craftablengredient.Count; a++)
                        {
                            int indexa = a;
                            if (player.InventoryCount(new Item(GeneralManager.singleton.buildingItems[index].buildingItem[indexe].craftablengredient[a].item)) < GeneralManager.singleton.buildingItems[index].buildingItem[indexe].craftablengredient[a].amount)
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }

        if (currencyType == 0)
        {
            if (player.gold >= itemToCraft.goldPrice)
            {
                for (int i = 0; i < GeneralManager.singleton.buildingItems.Count; i++)
                {
                    int index = i;
                    if (GeneralManager.singleton.buildingItems[index].specificBuilding.name == building.name)
                    {
                        for (int e = 0; e < GeneralManager.singleton.buildingItems[index].buildingItem.Count; e++)
                        {
                            int indexe = e;
                            if (GeneralManager.singleton.buildingItems[index].buildingItem[indexe].itemToCraft.item.name == craftableBuilding)
                            {
                                itemToCraft = GeneralManager.singleton.buildingItems[index].buildingItem[indexe].itemToCraft.item;
                                for (int a = 0; a < GeneralManager.singleton.buildingItems[index].buildingItem[indexe].craftablengredient.Count; a++)
                                {
                                    int indexa = a;
                                    player.InventoryRemove(new Item(GeneralManager.singleton.buildingItems[index].buildingItem[indexe].craftablengredient[indexa].item), GeneralManager.singleton.buildingItems[index].buildingItem[indexe].craftablengredient[indexa].amount);
                                }
                                player.InventoryAdd(new Item(GeneralManager.singleton.buildingItems[index].buildingItem[indexe].itemToCraft.item), 1);
                                player.gold -= GeneralManager.singleton.buildingItems[index].buildingItem[indexe].itemToCraft.item.goldPrice;
                            }
                        }
                    }
                }
            }

        }

        if (currencyType == 1)
        {
            if (player.coins >= itemToCraft.coinPrice)
            {
                for (int i = 0; i < GeneralManager.singleton.buildingItems.Count; i++)
                {
                    int index = i;
                    if (GeneralManager.singleton.buildingItems[index].specificBuilding.name == building.name)
                    {
                        for (int e = 0; e < GeneralManager.singleton.buildingItems[index].buildingItem.Count; e++)
                        {
                            int indexe = e;
                            if (GeneralManager.singleton.buildingItems[index].buildingItem[indexe].itemToCraft.item.name == craftableBuilding)
                            {
                                for (int a = 0; a < GeneralManager.singleton.buildingItems[index].buildingItem[indexe].craftablengredient.Count; a++)
                                {
                                    int indexa = a;
                                    player.InventoryRemove(new Item(GeneralManager.singleton.buildingItems[index].buildingItem[indexe].craftablengredient[indexa].item), GeneralManager.singleton.buildingItems[index].buildingItem[indexe].craftablengredient[indexa].amount);
                                }
                                player.InventoryAdd(new Item(GeneralManager.singleton.buildingItems[index].buildingItem[indexe].itemToCraft.item), 1);
                                player.coins -= GeneralManager.singleton.buildingItems[index].buildingItem[indexe].itemToCraft.item.coinPrice;
                            }
                        }
                    }
                }
            }
        }
    }

}


// Da eliminare
public partial class PlayerItemBuilding
{
    public ScriptableBuilding building;
}

public partial class PlayerFired
{
    public Player player;
    [SyncVar]
    public int amountFired;

    public GameObject firedSpawn;

    // Start is called before the first frame update
    void Start()
    {
        if (player.isServer)
        {
            InvokeRepeating(nameof(DecreaseFire), GeneralManager.singleton.firePlayerInvoke, GeneralManager.singleton.firePlayerInvoke);
        }
        else
        {
            InvokeRepeating(nameof(DecreaseFireOnClient), GeneralManager.singleton.firePlayerInvoke, GeneralManager.singleton.firePlayerInvoke);
        }
    }


    public void DecreaseFire()
    {
        if (player.isServer)
        {
            if (amountFired > 0)
            {
                if (player.health == 0)
                {
                    amountFired = 0;
                    Destroy(firedSpawn.gameObject);
                }
                else
                {
                    player.ManageDamageArmorHealth(GeneralManager.singleton.firedDamage);
                    amountFired--;
                }
            }
        }
    }
    public void DecreaseFireOnClient()
    {
        if (amountFired == 0 || player.health == 0)
        {
            if (firedSpawn) Destroy(firedSpawn.gameObject);
        }
    }
}

public partial class PlayerElectric
{
    public Player player;
    [SyncVar]
    public int amountElectric;

    public GameObject electricSpawn;

    // Start is called before the first frame update
    void Start()
    {
        if (player.isServer)
        {
            InvokeRepeating(nameof(DecreaseElectric), GeneralManager.singleton.electricPlayerInvoke, GeneralManager.singleton.electricPlayerInvoke);
        }
        else
        {
            InvokeRepeating(nameof(DecreaseElectricOnClient), GeneralManager.singleton.electricPlayerInvoke, GeneralManager.singleton.electricPlayerInvoke);
        }
    }


    public void DecreaseElectric()
    {
        if (player.isServer)
        {
            if (amountElectric > 0)
            {
                if (player.health == 0)
                {
                    amountElectric = 0;
                    if (electricSpawn) Destroy(electricSpawn.gameObject);
                }
                else
                {
                    player.ManageDamageArmorHealth(GeneralManager.singleton.electricDamage);
                    amountElectric--;
                }
            }
        }
    }

    public void DecreaseElectricOnClient()
    {
        if (amountElectric == 0 || player.health == 0)
        {
            if (electricSpawn) Destroy(electricSpawn.gameObject);
        }
    }

}
// testato
public partial class PlayerMarriage
{
    public Player player;

    [SyncVar]
    public string inviter;
    [SyncVar]
    public string partnerName;

    [Header("Partner")]
    [SyncVar]
    public GameObject _partner;

    public int defaultHealth;
    public float defaultDefense;
    public int defaultMana;

    public Entity partner
    {
        get { return _partner != null ? _partner.GetComponent<Entity>() : null; }
        set { _partner = value != null ? value.gameObject : null; }
    }

    void Start()
    {
        if (player.isServer)
        {
            if (partnerName != string.Empty)
            {
                if (_partner == null)
                {
                    if (Player.onlinePlayers.ContainsKey(partnerName))
                    {
                        partner = Player.onlinePlayers[partnerName];
                        Player.onlinePlayers[partnerName].playerMarriage.partner = player;
                    }
                }
            }
        }
    }


    public void OnDisconnect()
    {
        if (partnerName != string.Empty)
        {
            if (_partner != null)
            {
                if (Player.onlinePlayers.ContainsKey(partnerName))
                {
                    Player.onlinePlayers[partnerName].playerMarriage.partner = null;
                }
            }
        }
    }

    [Command]
    public void CmdInvitePartner()
    {
        if (player.target is Player && ((Player)player.target).playerMarriage.partnerName == string.Empty && !((Player)player.target).playerOptions.blockMarriage)
        {
            ((Player)player.target).playerMarriage.inviter = name;
        }
    }

    [Command]
    public void CmdAcceptInvitePartner()
    {
        Player onlinePlayer;
        Player _Ppartner;
        if (Player.onlinePlayers.TryGetValue(inviter, out onlinePlayer))
        {
        }

        if (onlinePlayer && onlinePlayer.playerMarriage.partnerName == string.Empty && player.playerMarriage.partnerName == string.Empty)
        {
            onlinePlayer.playerMarriage.partnerName = name;
            player.playerMarriage.partnerName = onlinePlayer.name;

            RpcSpawnFullHeart(name, onlinePlayer.name);


            if (Player.onlinePlayers.TryGetValue(onlinePlayer.playerMarriage.partnerName, out _Ppartner))
            {
                _partner = _Ppartner.gameObject;
            }
            if (_partner)
            {
                for (int i = 0; i < player.playerMarriage._partner.GetComponent<Player>().quests.Count; i++)
                {
                    int index = i;
                    Quest quest = player.playerMarriage._partner.GetComponent<Player>().quests[index];
                    if (quest.data.makeMarriage == true)
                    {
                        quest.checkMakeMarriage = true;
                    }
                    player.playerMarriage._partner.GetComponent<Player>().quests[index] = quest;
                }
            }
        }

        player.playerMarriage.inviter = string.Empty;
    }

    [ClientRpc]
    public void RpcSpawnFullHeart(string target, string me)
    {
        Player mySelf;
        Player myPartner;
        if (Player.onlinePlayers.TryGetValue(target, out mySelf))
        {
            GameObject g = Instantiate(GeneralManager.singleton.makeMarriage, mySelf.transform);
            g.transform.SetParent(mySelf.transform);
            g.transform.localPosition = new Vector3(0.0f, 4.32f, 0.0f);
            g.transform.localScale = new Vector3(0.9f, 0.5f, 0.5f);
        }
        if (Player.onlinePlayers.TryGetValue(me, out myPartner))
        {
            GameObject g = Instantiate(GeneralManager.singleton.makeMarriage, myPartner.transform);
            g.transform.SetParent(myPartner.transform);
            g.transform.localPosition = new Vector3(0.0f, 4.32f, 0.0f);
            g.transform.localScale = new Vector3(0.9f, 0.5f, 0.5f);
        }
    }

    [ClientRpc]
    public void RpcSpawnBreakHeart(string target, string me)
    {
        Player mySelf;
        Player myPartner;
        if (Player.onlinePlayers.TryGetValue(target, out mySelf))
        {
            GameObject g = Instantiate(GeneralManager.singleton.breakMarriage, mySelf.transform);
            g.transform.SetParent(mySelf.transform);
            g.transform.localPosition = new Vector3(0.0f, 4.32f, 0.0f);
            g.transform.localScale = new Vector3(0.9f, 0.5f, 0.5f);
        }
        if (Player.onlinePlayers.TryGetValue(me, out myPartner))
        {
            GameObject g = Instantiate(GeneralManager.singleton.breakMarriage, myPartner.transform);
            g.transform.SetParent(myPartner.transform);
            g.transform.localPosition = new Vector3(0.0f, 4.32f, 0.0f);
            g.transform.localScale = new Vector3(0.9f, 0.5f, 0.5f);
        }
    }

    [Command]
    public void CmdDeclineInvitePartner()
    {
        player.playerMarriage.inviter = string.Empty;
    }

    [Command]
    public void CmdRemovePartner()
    {
        Player myPartner;
        RpcSpawnBreakHeart(name, partnerName);
        if (Player.onlinePlayers.TryGetValue(player.playerMarriage.partnerName, out myPartner))
        {
            myPartner.playerMarriage.partnerName = string.Empty;
        }
        else
        {
            Database.singleton.DeletePartner(player.playerMarriage.partnerName);
        }
        player.playerMarriage.partnerName = string.Empty;
    }
}
// testato
public partial class PlayerMove
{
    public Player player;

    [SyncVar]
    public bool sneak;
    [SyncVar]
    public bool run;
    [SyncVar]
    public float major = 0.0f;
    [SyncVar]
    public float x = 0.0f;
    [SyncVar]
    public float y = 0.0f;

    public float xNoJoystick = 0.0f;
    public float yNoJoystick = 0.0f;

    public float clientMajor = 0.0f;

    private float prevX;
    private float prevY;

    public bool isServerPlayer;

    public float rotationSpeed;

    public Transform dummyBodyPlayer;
    public Transform bodyPlayer;

    public float prevXAxis;
    public float prevYAxis;

    [SyncVar]
    public float serverXAxis;
    [SyncVar]
    public float serverYAxis;

    public double lastHit;
    public Entity lastHitEntity;
    public Collider2D[] nearEntity;


    public List<Entity> sorted = new List<Entity>();
    public Collider2D[] monsters;
    public List<Entity> monstersSorted;

    public List<ModularObject> sortedForniture = new List<ModularObject>();
    public Collider2D[] fornitures;
    public List<ModularObject> fornitureSorted;

    public ModularObject fornitureClient;

    public Vector3 positioningVector = new Vector3(7.960999f, -1.264f, 7.457145f);

    public List<ModularPiece> sortedPieces = new List<ModularPiece>();

    public ModularPiece nearestModularPiece;

    [SyncVar]
    public bool drink;
    [SyncVar]
    public bool eat;

    [SyncVar] public GameObject _forniture;
    public Forniture forniture
    {
        get { return _forniture != null ? _forniture.GetComponent<Forniture>() : null; }
        set { _forniture = value != null ? value.gameObject : null; }
    }

    public float distanceToCheckForniture = 3.0f;
    public float distanceToCheckEntity = 3.0f;


    void Start()
    {
        player.agent.speed = GeneralManager.singleton.initialSpeed;

        if (player.isServer)
        {
            InvokeRepeating(nameof(ConsumeMana), 1.0f, 1.0f);
            InvokeRepeating(nameof(ManageMovement), 0.1f, 0.1f);
        }

        if (player.isClient)
        {
            if (player.isLocalPlayer)
            {
                InvokeRepeating(nameof(SmartTargeting), 0.3f, 0.3f);
            }
            InvokeRepeating(nameof(MovementController), 0.0f, 0.01f);
        }
    }

    public void SyncRotation()
    {
        if (player.isLocalPlayer)
        {
            if (xNoJoystick != 0.0f || yNoJoystick != 0.0f)
            {
                if (prevXAxis != xNoJoystick || prevYAxis != yNoJoystick)
                {
                    CmdSyncRotation(xNoJoystick, yNoJoystick);
                    prevXAxis = xNoJoystick;
                    prevYAxis = yNoJoystick;
                }
            }
        }
        else
        {
            CancelInvoke("SyncRotation");
        }
    }


    public void SyncRotationJoystick()
    {
        if (player.isLocalPlayer)
        {
            if (player.joystick.output.x != 0.0f || player.joystick.output.z != 0.0f && (prevX != player.joystick.output.x || prevY != player.joystick.output.z))
            {
                prevX = player.joystick.output.x;
                prevY = player.joystick.output.y;
                CmdSetJoystickAdapter(clientMajor, player.joystick.output.x, player.joystick.output.z);
                for (int i = 0; i < player.animators.Count; i++)
                {
                    player.animators[i].SetBool("isRunning", run);
                }
            }
        }
        else
        {
            CancelInvoke("SyncRotationJoystick");
        }
    }

    [Command]
    public void CmdSyncRotation(float xRotation, float yRotation)
    {
        serverXAxis = xRotation;
        serverYAxis = yRotation;
    }

    void Update()
    {
        if (!player) return;

        if (isClient)
        {
            if (player.joystick)
            {
                float h = System.Math.Abs(player.joystick.output.x);
                float v = System.Math.Abs(player.joystick.output.z);

                if (h >= v) clientMajor = h;
                else clientMajor = v;
            }

            if (!bodyPlayer)
            {
                if (player.prefabPreview) bodyPlayer = player.prefabPreview.transform;
            }
            else
            {
                if (player.animators.Count == 0)
                {
                    player.animators = bodyPlayer.GetComponents<Animator>().ToList();
                }
                bodyPlayer.transform.localPosition = positioningVector;
            }
            ManageRotation();
        }
    }

    public void ManageRotation()
    {
        if (isClient)
        {
            if (player.useJoystick)
            {
                if (player.joystick)
                {
                    if (player.agent.velocity != Vector2.zero)
                    {
                        float heading = Mathf.Atan2(x, y);
                        bodyPlayer.transform.localRotation = Quaternion.Euler(0f, ((heading * Mathf.Rad2Deg) + 180.0f), 0);
                    }
                    else
                    {
                        if (!player.target || (player.target && player.target.health == 0))
                        {
                            float heading = Mathf.Atan2(x, y);
                            bodyPlayer.transform.localRotation = Quaternion.Euler(0f, ((heading * Mathf.Rad2Deg) + 180.0f), 0);
                        }
                        else if (player.target && player.target.health > 0)
                        {
                            Vector3 distance = player.transform.position - player.target.transform.position;
                            distance = distance.normalized;
                            float heading = Mathf.Atan2(distance.x, distance.y);
                            bodyPlayer.transform.localRotation = Quaternion.Euler(0f, (heading * Mathf.Rad2Deg), 0);
                        }
                    }
                }
            }
            else
            {
                if (player.isLocalPlayer)
                {
                    xNoJoystick = Input.GetAxis("Horizontal");
                    yNoJoystick = Input.GetAxis("Vertical");
                }
                else
                {
                    xNoJoystick = serverXAxis;
                    yNoJoystick = serverYAxis;
                }
                if (player.agent.velocity != Vector2.zero)
                {
                    if (xNoJoystick == 0.0f && yNoJoystick == 0.0f)
                    {
                        if (player.target)
                        {
                            Vector3 distance = player.transform.position - player.target.transform.position;
                            distance = distance.normalized;
                            float lHeading = Mathf.Atan2(distance.x, distance.y);
                            bodyPlayer.transform.localRotation = Quaternion.Euler(0f, (lHeading * Mathf.Rad2Deg), 0);
                            return;
                        }
                    }
                    float heading = Mathf.Atan2(xNoJoystick, yNoJoystick);
                    bodyPlayer.transform.localRotation = Quaternion.Euler(0f, ((heading * Mathf.Rad2Deg) + 180.0f), 0);
                }
                else
                {
                    if (!player.target || (player.target && player.target.health == 0))
                    {
                        float heading = Mathf.Atan2(xNoJoystick, yNoJoystick);
                        bodyPlayer.transform.localRotation = Quaternion.Euler(0f, heading * Mathf.Rad2Deg, 0);
                    }
                    else if (player.target && player.target.health > 0)
                    {
                        Vector3 distance = player.transform.position - player.target.transform.position;
                        distance = distance.normalized;
                        float heading = Mathf.Atan2(distance.x, distance.y);
                        bodyPlayer.transform.localRotation = Quaternion.Euler(0f, (heading * Mathf.Rad2Deg), 0);
                    }
                }
            }
            for (int i = 0; i < player.animators.Count; i++)
            {
                player.animators[i].SetBool("isRunning", run);
            }

        }
    }

    public void ManageMovement()
    {
        if (player.agent.velocity == Vector2.zero) return;

        player.playerMove.forniture = null;

        if (player.mana == 0 && player.playerCar.car == null)
            run = false;

        if (player.playerCar.car)
        {
            player.agent._speed = GeneralManager.singleton.initialSpeed * GeneralManager.singleton.carMultiplier;
        }
        else
        {
            player.JoystickManager(player);
            if (player.playerInjury.injured)
            {
                player.JoystickMultiplierSneak(player);
                player.agent._speed = GeneralManager.singleton.initialSpeed * GeneralManager.singleton.sneakMultiplier;
                return;
            }
            if (!sneak && !run)
            {
                player.JoystickMultiplierNormal(player);
                if ((player.playerWeight.currentWeight <= player.playerWeight.maxWeight && player.playerWeight.maxWeight != 0) || player.playerWeight.currentWeight == 0 && player.playerWeight.maxWeight == 0)
                    player.agent._speed = GeneralManager.singleton.initialSpeed * GeneralManager.singleton.normalMultiplier;
                else
                    player.agent._speed = GeneralManager.singleton.initialSpeed * GeneralManager.singleton.sneakMultiplier;
            }
            if (!sneak && run)
            {
                player.JoystickMultiplierRun(player);
                if (player.playerBoost.networkBoost.Count > 0)
                {
                    player.agent._speed = GeneralManager.singleton.initialSpeed * GeneralManager.singleton.runMultiplier + ((GeneralManager.singleton.initialSpeed / 100) * player.playerBoost.networkBoost[0].velocityPerc);
                }
                else
                {
                    player.agent._speed = GeneralManager.singleton.initialSpeed * GeneralManager.singleton.runMultiplier;
                }
            }
            if (sneak && !run)
            {
                player.JoystickMultiplierSneak(player);
                player.agent._speed = GeneralManager.singleton.initialSpeed * GeneralManager.singleton.sneakMultiplier;
            }
        }
    }

    public void ConsumeMana()
    {
        if (run && player.mana > 0 && player.agent.velocity != Vector2.zero && player.playerCar.car == null)
        {
            player.mana--;
        }
    }

    public void MovementController()
    {
        if (isClient)
        {
            if (player.useJoystick)
            {
                SyncRotationJoystick();
            }
            else
            {
                SyncRotation();
            }
        }
    }

    [Command]
    public void CmdSetJoystickAdapter(float move, float xPar, float yPar)
    {
        major = move;
        x = xPar;
        y = yPar;
    }

    [Command]
    public void CmdRun()
    {
        if ((player.playerWeight.currentWeight == 0 && player.playerWeight.maxWeight == 0) || (player.playerWeight.currentWeight < player.playerWeight.maxWeight))
        {
            if (player.playerCar.car)
            {
                run = true;
                sneak = false;
                return;
            }

            sneak = false;
            for (int i = 0; i < player.quests.Count; i++)
            {
                Quest quest = player.quests[i];
                if (quest.data.run == true)
                {
                    quest.checkRun = true;
                }
                player.quests[i] = quest;
            }
            run = !run;
        }
    }

    [Command]
    public void CmdSneak()
    {
        if ((player.playerWeight.currentWeight == 0 && player.playerWeight.maxWeight == 0) || (player.playerWeight.currentWeight < player.playerWeight.maxWeight))
        {
            if (player.playerCar.car)
            {
                run = true;
                sneak = false;
                return;
            }
            sneak = !sneak;
            for (int i = 0; i < player.quests.Count; i++)
            {
                Quest quest = player.quests[i];
                if (quest.data.sneak == true)
                {
                    quest.checkSneak = true;
                }
                player.quests[i] = quest;
            }
            run = false;
        }
    }

    [Client]
    public void SmartTargeting()
    {
        if (ModularBuildingManager.singleton.inThisCollider)
        {
            if (player.target != null && !(player.target is Player && player.target is Chest))
                player.CmdSetTarget(null);

            return;
        }

        monstersSorted = nearEntity.Select(go => go.GetComponent<Entity>()).Where(m => m != null && m.health >= 0).ToList();
        sorted = monstersSorted.OrderBy(m => Vector3.Distance(player.transform.position, m.transform.position)).ToList();

        if (lastHit < NetworkTime.time - 3.0f || (lastHitEntity && Vector3.Distance(player.transform.position, lastHitEntity.transform.position) > 15))
        {
            lastHit = NetworkTime.time;
            lastHitEntity = null;

            if (sorted.Count > 0)
            {
                for (int i = 0; i < sorted.Count; i++)
                {
                    if (sorted[i] == null) continue;
                    if (Vector3.Distance(transform.position, sorted[i].transform.position) > distanceToCheckEntity) continue;
                    if (sorted[i] == player) continue;
                    if (sorted[i] == player.activePet) continue;

                    if (sorted[i].GetComponent<Mine>())
                    {
                        if (!GeneralManager.singleton.CanManageExplosiveBuilding(sorted[i].GetComponent<Building>(), player))
                        {
                            continue;
                        }
                    }

                    player.CmdSetTarget(sorted[i].GetComponent<NetworkIdentity>());
                    return;
                }
            }
        }
    }

    [Client]
    public bool SmartTargetingForniture()
    {
        if (ModularBuildingManager.singleton.inThisCollider)
        {
            fornitures = Physics2D.OverlapCircleAll(transform.position, distanceToCheckEntity, GeneralManager.singleton.modularObjectNeedBaseLayerMask);
            fornitureSorted = fornitures.Select(go => go.GetComponent<ModularObject>()).ToList();
            sortedForniture = fornitureSorted.OrderBy(m => Vector2.Distance(transform.position, m.transform.position)).ToList();

            if (sortedForniture.Count > 0)
            {
                for (int i = 0; i < sortedForniture.Count; i++)
                {
                    int index = i;
                    if (sortedForniture[index] == null) continue;
                    if (Vector2.Distance(transform.position, sortedForniture[index].transform.position) > distanceToCheckEntity) continue;

                    if (player.playerMove.nearestModularPiece != null)
                    {
                        if (Vector2.Distance(transform.position, sortedForniture[index].transform.position) > GeneralManager.singleton.GetClosestDistance(player.playerMove.nearestModularPiece.floorDoor.ToArray()))
                        {
                            player.CmdManageDoor(player.playerMove.nearestModularPiece.netIdentity, GeneralManager.singleton.GetClosestDistanceIndex(player.playerMove.nearestModularPiece.floorDoor.ToArray()));
                            return false;
                        }
                        else
                        {
                            fornitureClient = sortedForniture[index].GetComponent<ModularObject>();
                            player.CmdSetForniture(sortedForniture[index].identity);
                            return true;
                        }
                    }
                    else
                    {
                        fornitureClient = sortedForniture[index].GetComponent<ModularObject>();
                        player.CmdSetForniture(sortedForniture[index].identity);
                        return true;

                    }
                }
                return false;
            }
            else
            {
                if (player.playerMove.nearestModularPiece != null)
                {
                    player.CmdManageDoor(player.playerMove.nearestModularPiece.netIdentity, GeneralManager.singleton.GetClosestDistanceIndex(player.playerMove.nearestModularPiece.floorDoor.ToArray()));
                    return false;
                }
            }
        }
        else
        {
            if (player.playerMove.nearestModularPiece != null)
            {
                if (player.target)
                {
                    if (Vector2.Distance(transform.position, player.target.transform.position) < GeneralManager.singleton.GetClosestDistance(player.playerMove.nearestModularPiece.floorDoor.ToArray()))
                    {
                        return true;
                    }
                    else
                    {
                        player.CmdManageDoor(player.playerMove.nearestModularPiece.netIdentity, GeneralManager.singleton.GetClosestDistanceIndex(player.playerMove.nearestModularPiece.floorDoor.ToArray()));
                        return false;
                    }
                }
                else
                {
                    player.CmdManageDoor(player.playerMove.nearestModularPiece.netIdentity, GeneralManager.singleton.GetClosestDistanceIndex(player.playerMove.nearestModularPiece.floorDoor.ToArray()));
                    return false;
                }
            }
            return true;
        }
        return true;
    }

    public void Drink()
    {
        CancelInvoke(nameof(StopDrink));
        drink = true;
        Invoke(nameof(StopDrink), 3.0f);
        player.agent.ResetPath();
        TargetSetDrinkAnimation(player.connectionToClient);
    }

    public void StopDrink()
    {
        drink = false;
    }

    public void Eat()
    {
        CancelInvoke(nameof(StopEat));
        eat = true;
        player.agent.ResetPath();
        Invoke(nameof(StopEat), 3.0f);
        TargetSetEatAnimation(player.connectionToClient);
    }

    public void StopEat()
    {
        eat = false;
    }

    [TargetRpc]
    public void TargetSetDrinkAnimation(NetworkConnection connection)
    {
        Player player = connection.identity.GetComponent<Player>();

        for (int i = 0; i < player.animators.Count; i++)
        {
            player.animators[i].Play("Item-Drink");
        }
    }

    [TargetRpc]
    public void TargetSetEatAnimation(NetworkConnection connection)
    {
        Player player = connection.identity.GetComponent<Player>();

        for (int i = 0; i < player.animators.Count; i++)
        {
            player.animators[i].Play("Item-Eat");
        }
    }
}

public partial class PlayerFriend
{
    public Player player;

    public SyncList<string> playerRequest = new SyncList<string>();
    public SyncList<string> playerFriends = new SyncList<string>();


    [Command]
    public void CmdSendFriendRequest()
    {
        if (player.target is Player &&
           !((Player)player.target).playerOptions.blockFriend &&
           ((Player)player.target).playerFriend.playerRequest.Count < GeneralManager.singleton.maxFriendRequest &&
           player.playerFriend.playerFriends.Count < GeneralManager.singleton.maxFriends &&
           ((Player)player.target).playerFriend.playerFriends.Count < GeneralManager.singleton.maxFriends)
        {
            if (!((Player)player.target).playerFriend.playerRequest.Contains(name) &&
                !((Player)player.target).playerFriend.playerFriends.Contains(name))
            {
                ((Player)player.target).playerFriend.playerRequest.Add(name);
                TargetRpcFriendNotification(player.target.connectionToClient);
            }
        }
    }

    [Command]
    public void CmdLoadFriendStat(string friendName, int sexType)
    {
        GameObject friendDummy = Database.singleton.CharacterFriendLoad(friendName, sexType == 0 ? GeneralManager.singleton.male : GeneralManager.singleton.female);
        TargetRpcReturnFriend(friendDummy);
    }

    [TargetRpc]
    public void TargetRpcReturnFriend(GameObject returnFriend)
    {
        if (UIFriends.singleton)
            UIFriends.singleton.selectedFriend = returnFriend.GetComponent<Player>();
    }

    [TargetRpc]
    public void TargetRpcFriendNotification(NetworkConnection connection)
    {
        Player player = connection.identity.GetComponent<Player>();
        player.SpawnFriendObject(player.name);

    }

    [Command]
    public void CmdAcceptFriends(string friendName)
    {
        Player onlinePlayer = null;
        if (Player.onlinePlayers.TryGetValue(friendName, out onlinePlayer))
        {
            if (player.playerFriend.playerFriends.Count < GeneralManager.singleton.maxFriends &&
                onlinePlayer.playerFriend.playerFriends.Count < GeneralManager.singleton.maxFriends)
            {
                if (!player.playerFriend.playerFriends.Contains(friendName))
                {
                    onlinePlayer.playerFriend.playerFriends.Add(name);
                    player.playerFriend.playerFriends.Add(onlinePlayer.name);
                    player.playerFriend.playerRequest.Remove(friendName);
                }
            }
        }
    }

    [Command]
    public void CmdRemoveFriends(string friendName)
    {
        player.playerFriend.playerFriends.Remove(friendName);

        Player onlinePlayer;
        if (Player.onlinePlayers.TryGetValue(friendName, out onlinePlayer))
        {
            onlinePlayer.playerFriend.playerFriends.Remove(player.name);
        }
        else
        {
            Database.singleton.RemoveFriend(onlinePlayer.name, player.name);
        }
    }

    [Command]
    public void CmdRemoveRequestFriends(string friendName)
    {
        player.playerFriend.playerRequest.Remove(friendName);
    }

}
// testato
public partial class PlayerOptions
{
    public Player player;

    [SyncVar]
    public bool blockMarriage;
    [SyncVar]
    public bool blockParty;
    [SyncVar]
    public bool blockGroup;
    [SyncVar]
    public bool blockAlly;
    [SyncVar]
    public bool blockTrade;
    [SyncVar]
    public bool blockFriend;
    [SyncVar]
    public bool blockFootstep;
    [SyncVar(hook = nameof(ManageSound))]
    public bool blockSound;
    [SyncVar]
    public bool blockButtonSounds;

    public GameObject uiRestoreCredential;

    public AudioSource[] audioSource;


    public void ManageSound(bool oldBool, bool newBool)
    {
        if (isClient)
        {
            if (isLocalPlayer)
            {
                audioSource = FindObjectOfType<SoundManager>().GetComponents<AudioSource>();
                for (int i = 0; i < audioSource.Length; i++)
                {
                    int index = i;
                    audioSource[index].enabled = !newBool;
                }
            }
        }
    }

    [Command]
    public void CmdSaveIssue(string playerName, string Type, string description)
    {
        Player onlinePlayer;
        if (Player.onlinePlayers.TryGetValue(playerName, out onlinePlayer))
        {
            Database.singleton.SaveIssue(onlinePlayer, Type, description);
        }
    }

    [Command]
    public void CmdBlockAlly()
    {
        if (NetworkTime.time < player.nextRiskyActionTime) return;
        player.playerOptions.blockAlly = !player.playerOptions.blockAlly;
        player.nextRiskyActionTime = NetworkTime.time + 1;
    }

    [Command]
    public void CmdBlockFootstep()
    {
        if (NetworkTime.time < player.nextRiskyActionTime) return;
        player.playerOptions.blockFootstep = !player.playerOptions.blockFootstep;
        player.nextRiskyActionTime = NetworkTime.time + 1;
    }

    [Command]
    public void CmdBlockFriends()
    {
        if (NetworkTime.time < player.nextRiskyActionTime) return;
        player.playerOptions.blockFriend = !player.playerOptions.blockFriend;
        player.nextRiskyActionTime = NetworkTime.time + 1;
    }

    [Command]
    public void CmdBlockGroup()
    {
        if (NetworkTime.time < player.nextRiskyActionTime) return;
        player.playerOptions.blockGroup = !player.playerOptions.blockGroup;
        player.nextRiskyActionTime = NetworkTime.time + 1;
    }

    [Command]
    public void CmdBlockMarriage()
    {
        if (NetworkTime.time < player.nextRiskyActionTime) return;
        player.playerOptions.blockMarriage = !player.playerOptions.blockMarriage;
        player.nextRiskyActionTime = NetworkTime.time + 1;
    }

    [Command]
    public void CmdBlockParty()
    {
        if (NetworkTime.time < player.nextRiskyActionTime) return;
        player.playerOptions.blockParty = !player.playerOptions.blockParty;
        player.nextRiskyActionTime = NetworkTime.time + 1;
    }

    [Command]
    public void CmdBlockSound()
    {
        if (NetworkTime.time < player.nextRiskyActionTime) return;
        player.playerOptions.blockSound = !player.playerOptions.blockSound;
        player.nextRiskyActionTime = NetworkTime.time + 1;
    }

    [Command]
    public void CmdBlockButtonSound()
    {
        if (NetworkTime.time < player.nextRiskyActionTime) return;
        player.playerOptions.blockButtonSounds = !player.playerOptions.blockButtonSounds;
        player.nextRiskyActionTime = NetworkTime.time + 1;
    }

    [Command]
    public void CmdBlockTrade()
    {
        if (NetworkTime.time < player.nextRiskyActionTime) return;
        player.playerOptions.blockTrade = !player.playerOptions.blockTrade;
        player.nextRiskyActionTime = NetworkTime.time + 1;
    }


}

public partial class PlayerBlood
{
    public Player player;

    public float decreaseBloodTimer;

    [SyncVar]
    public int currentBlood;

    void Start()
    {
        if (player.isServer)
        {
            decreaseBloodTimer = GeneralManager.singleton.decreaseBloodTimer;
            InvokeRepeating(nameof(DecreaseBlood), decreaseBloodTimer, decreaseBloodTimer);
        }
    }


    public void DecreaseBlood()
    {
        if (currentBlood > 0) currentBlood--;

        if (player.playerBlood.currentBlood > GeneralManager.singleton.maxBlood)
        {
            player.playerBlood.currentBlood = GeneralManager.singleton.maxBlood;
        }
        if (player.playerBlood.currentBlood < 0)
        {
            player.playerBlood.currentBlood = 0;
        }

    }

}

// testato
public partial class PlayerCar
{
    public Player player;

    public Player pilotPlayerClient;

    [Header("Car")]
    [SyncVar] public GameObject _car;

    [SyncVar] public string passengerType = String.Empty;

    public SpriteRenderer[] childRenderer;
    public Car car
    {
        get { return _car != null ? _car.GetComponent<Car>() : null; }
        set { _car = value != null ? value.gameObject : null; }
    }

    void Start()
    {
        //InvokeRepeating("CheckSpriteRenderer", 1.0f, 1.0f);
    }


    void Update()
    {
        //if (player.nameOverlay) player.nameOverlay.gameObject.SetActive((Player.localPlayer && !car));

        //if (car)
        //{
        //    player.GetComponent<SpriteRenderer>().enabled =
        //    player.collider.enabled = false;
        //}
        //else
        //{
        //    player.GetComponent<SpriteRenderer>().enabled =
        //    player.collider.enabled = true;
        //}


        //if (car && car._pilot != string.Empty) Player.onlinePlayers.TryGetValue(car._pilot, out pilotPlayerClient);

        //if (car)
        //{
        //    if (pilotPlayerClient && car._pilot != string.Empty)
        //    {
        //        if (pilotPlayerClient)
        //            car.lookDirection = pilotPlayerClient.lookDirection;

        //        if (car._pilot != string.Empty && pilotPlayerClient.name == player.name)
        //        {
        //            car.agent.Warp(pilotPlayerClient.transform.position);
        //        }
        //    }
        //    if (car._coPilot != string.Empty && car._coPilot == player.name ||
        //        car._rearSxPassenger != string.Empty && car._rearSxPassenger == player.name ||
        //        car._rearCenterPassenger != string.Empty && car._rearCenterPassenger == player.name ||
        //        car._rearDxPassenger != string.Empty && car._rearDxPassenger == player.name)
        //    {
        //        if (pilotPlayerClient)
        //        {
        //            car.lookDirection = pilotPlayerClient.lookDirection;
        //        }
        //        player.agent.Warp(car.transform.position);
        //    }
        //}
    }
    public void PetUnsummon()
    {
        // validate
        if (player.CanUnsummonPet())
        {
            // destroy from world. item.summoned and activePet will be null.
            NetworkServer.Destroy(player.activePet.gameObject);
            player.activePet = null;
        }
    }

    [Command]
    public void CmdCarMode()
    {
        if (!(player.GetComponent<Player>().target is Car)) return;

        Car car = ((Car)player.GetComponent<Player>().target);

        if (car._pilot != string.Empty && car._pilot == name && car.currentGasoline > 0 && NetworkTime.time >= player.nextRiskyActionTime)
        {
            car.On = !car.On;
            player.nextRiskyActionTime = NetworkTime.time + 2.0f;
        }

    }

    [Command]
    public void CmdLights()
    {
        if (!(player.GetComponent<Player>().target is Car)) return;

        Car car = ((Car)player.GetComponent<Player>().target);

        if (car._pilot != string.Empty && car._pilot == name && NetworkTime.time >= player.nextRiskyActionTime)
        {
            car.lightON = !car.lightON;
            player.nextRiskyActionTime = NetworkTime.time + 2.0f;
        }

    }

    [Command]
    public void CmdExit()
    {
        if (!car) return;

        if (car._pilot == name)
        {
            car.lightON = false;
            car.On = false;
        }

        if (car._pilot == name) car._pilot = string.Empty;
        if (car._coPilot == name) car._coPilot = string.Empty;
        if (car._rearSxPassenger == name) car._rearSxPassenger = string.Empty;
        if (car._rearCenterPassenger == name) car._rearCenterPassenger = string.Empty;
        if (car._rearDxPassenger == name) car._rearDxPassenger = string.Empty;

        if (_car) _car = null;
        if (car) car = null;
        player.playerMove.run = false;
        passengerType = string.Empty;


    }

    public void Exit()
    {
        if (!car) return;

        if (car._pilot == name)
        {
            car.lightON = false;
            car.On = false;
        }

        if (car._pilot == name) car._pilot = string.Empty;
        if (car._coPilot == name) car._coPilot = string.Empty;
        if (car._rearSxPassenger == name) car._rearSxPassenger = string.Empty;
        if (car._rearCenterPassenger == name) car._rearCenterPassenger = string.Empty;
        if (car._rearDxPassenger == name) car._rearDxPassenger = string.Empty;

        if (_car) _car = null;
        if (car) car = null;
        player.playerMove.run = false;
        passengerType = string.Empty;
    }



    [Command]
    public void CmdGetPieceOfTheCar(int index)
    {
        ItemSlot slot = player.playerCar.car.inventory[index];

        if (player.InventoryCanAdd(new Item(slot.item.data), 1))
        {
            player.InventoryAdd(new Item(slot.item.data), 1);
            player.playerCar.car.inventory.Remove(slot);
        }
        for (int i = 0; i < player.playerCar.car.inventory.Count; i++)
        {
            int pIndex = i;
        }
    }


    [Command]
    public void CmdPutGasoline(int putGasoline)
    {
        if (!(player.playerCar.car)) return;

        Car car = player.playerCar.car;

        if (car.currentGasoline + putGasoline > car.maxGasoline) return;

        for (int i = 0; i < player.inventory.Count; i++)
        {
            int index = i;
            if (player.inventory[index].amount > 0)
            {
                if (player.inventory[index].item.data.generalLiquidContainer > 0)
                {
                    if (player.inventory[index].item.honeyContainer > 0)
                        continue;
                    if (player.inventory[index].item.waterContainer > 0)
                        continue;
                    if (player.inventory[index].item.gasolineContainer > 0)
                    {
                        if (player.inventory[index].item.gasolineContainer >= putGasoline)
                        {
                            ItemSlot slot = player.inventory[index];
                            car.currentGasoline += putGasoline;
                            slot.item.gasolineContainer -= putGasoline;
                            player.inventory[index] = slot;
                            continue;
                        }
                        if (player.inventory[index].item.gasolineContainer < putGasoline)
                        {
                            ItemSlot slot = player.inventory[index];
                            car.currentGasoline += slot.item.gasolineContainer;
                            putGasoline -= slot.item.gasolineContainer;
                            slot.item.gasolineContainer = 0;
                            player.inventory[index] = slot;
                            continue;
                        }
                    }
                }
            }
        }
        if (car.currentGasoline > car.maxGasoline) car.currentGasoline = car.maxGasoline;
    }

    [Command]
    public void CmdGetGasoline(int getGasoline)
    {
        if (!(player.playerCar.car)) return;

        Car car = player.playerCar.car;

        if (getGasoline > car.currentGasoline) return;

        for (int i = 0; i < player.inventory.Count; i++)
        {
            int index = i;
            if (player.inventory[index].amount > 0)
            {
                if (player.inventory[index].item.data.generalLiquidContainer > 0)
                {
                    if (player.inventory[index].item.honeyContainer > 0)
                        continue;
                    if (player.inventory[index].item.waterContainer > 0)
                        continue;
                    ItemSlot slot = player.inventory[index];
                    // controllo lo spazio libero per la benzina in questo item
                    int emptyGasoline = slot.item.data.generalLiquidContainer - slot.item.gasolineContainer;
                    // se quello che vuoi prendere è maggiore dello spazio libero
                    if (getGasoline > emptyGasoline)
                    {
                        // fulliamo item con benzina
                        slot.item.gasolineContainer += emptyGasoline;
                        //rimiuoviamo benzina da macchina
                        car.currentGasoline -= emptyGasoline;
                        // rimuoviamo la benzina già presa
                        getGasoline -= emptyGasoline;
                        player.inventory[index] = slot;
                        continue;
                    }
                    else
                    {
                        slot.item.gasolineContainer += getGasoline;
                        car.currentGasoline -= getGasoline;
                        getGasoline = 0;
                        player.inventory[index] = slot;
                        continue;
                    }
                }
            }
        }
        if (car.currentGasoline > car.maxGasoline) car.currentGasoline = car.maxGasoline;
    }


    [Command]
    public void CmdPilot(GameObject player)
    {
        if (!(player.GetComponent<Player>().target is Car)) return;
        if (player.GetComponent<Player>().playerCar.passengerType != string.Empty && player.GetComponent<Player>().playerCar.car._pilot != "Pilot") return;

        Car car = ((Car)player.GetComponent<Player>().target);
        player.GetComponent<PlayerCar>()._car = car.gameObject;

        if (car._pilot == string.Empty)
        {
            car._pilot = player.name;
            passengerType = "Pilot";
        }
        else
        {
            if (car._pilot == player.name) car._pilot = string.Empty;
            player.GetComponent<PlayerCar>()._car = null;
            passengerType = string.Empty;
        }
    }

    [Command]
    public void CmdCoPilot(GameObject player)
    {
        if (!(player.GetComponent<Player>().target is Car)) return;
        if (player.GetComponent<Player>().playerCar.passengerType != string.Empty && player.GetComponent<Player>().playerCar.car._coPilot != "CoPilot") return;


        Car car = ((Car)player.GetComponent<Player>().target);
        player.GetComponent<PlayerCar>()._car = car.gameObject;

        if (car._coPilot == string.Empty)
        {
            car._coPilot = player.name;
            passengerType = "CoPilot";
        }
        else
        {
            if (car._coPilot == player.name) car._coPilot = string.Empty;
            player.GetComponent<PlayerCar>()._car = null;
            passengerType = string.Empty;
        }
    }

    [Command]
    public void CmdPassengerSx(GameObject player)
    {
        if (!(player.GetComponent<Player>().target is Car)) return;
        if (player.GetComponent<Player>().playerCar.passengerType != string.Empty && player.GetComponent<Player>().playerCar.car._rearSxPassenger != "RearSx") return;


        Car car = ((Car)player.GetComponent<Player>().target);
        player.GetComponent<PlayerCar>()._car = car.gameObject;

        if (car._rearSxPassenger == string.Empty)
        {
            car._rearSxPassenger = player.name;
            passengerType = "RearSx";
        }
        else
        {
            if (car._rearSxPassenger == player.name) car._rearSxPassenger = string.Empty;
            player.GetComponent<PlayerCar>()._car = null;
            passengerType = string.Empty;
        }
    }

    [Command]
    public void CmdPassengerCenter(GameObject player)
    {
        if (!(player.GetComponent<Player>().target is Car)) return;
        if (player.GetComponent<Player>().playerCar.passengerType != string.Empty && player.GetComponent<Player>().playerCar.car._rearCenterPassenger != "RearCenter") return;


        Car car = ((Car)player.GetComponent<Player>().target);
        player.GetComponent<PlayerCar>()._car = car.gameObject;

        if (car._rearCenterPassenger == string.Empty)
        {
            car._rearCenterPassenger = player.name;
            passengerType = "RearCenter";
        }
        else
        {
            if (car._rearCenterPassenger == player.name) car._rearCenterPassenger = string.Empty;
            player.GetComponent<PlayerCar>()._car = null;
            passengerType = string.Empty;
        }
    }

    [Command]
    public void CmdPassengerDx(GameObject player)
    {
        if (!(player.GetComponent<Player>().target is Car)) return;
        if (player.GetComponent<Player>().playerCar.passengerType != string.Empty && player.GetComponent<Player>().playerCar.car._pilot != "RearDx") return;


        Car car = ((Car)player.GetComponent<Player>().target);
        player.GetComponent<PlayerCar>()._car = car.gameObject;

        if (car._rearDxPassenger == string.Empty)
        {
            car._rearDxPassenger = player.name;
            passengerType = "RearDx";
        }
        else
        {
            if (car._rearDxPassenger == player.name) car._rearDxPassenger = string.Empty;
            player.GetComponent<PlayerCar>()._car = null;
            passengerType = string.Empty;
        }
    }

    public int GetEmptyGasolineBootle()
    {
        int containerFree = 0;
        for (int i = 0; i < player.inventory.Count; i++)
        {
            int index = i;
            if (player.inventory[index].amount > 0)
            {
                if (player.inventory[index].item.data.generalLiquidContainer > 0)
                {
                    if (player.inventory[index].item.honeyContainer > 0) continue;
                    if (player.inventory[index].item.waterContainer > 0) continue;
                    if (player.inventory[index].item.gasolineContainer < player.inventory[index].item.data.generalLiquidContainer)
                    {
                        containerFree += player.inventory[index].item.data.generalLiquidContainer - player.inventory[index].item.gasolineContainer;
                    }
                }
            }
        }
        return containerFree;
    }

    public int GetGasolineInINventory()
    {
        int containerFree = 0;
        for (int i = 0; i < player.inventory.Count; i++)
        {
            int index = i;
            if (player.inventory[index].amount > 0)
            {
                if (player.inventory[index].item.honeyContainer > 0) continue;
                if (player.inventory[index].item.waterContainer > 0) continue;
                if (player.inventory[index].item.gasolineContainer > 0)
                {
                    containerFree += player.inventory[index].item.gasolineContainer;
                }
            }
        }
        return containerFree;
    }

    [Command]
    public void CmdSwitchInventoryCar(int[] car, int[] inventory)
    {
        //if (!CheckIfCanChangeItemBetweenCarAndInventory(car, inventory)) return;
        if (player.playerCar._car != null && player.target.GetComponent<Car>() && player.playerCar._car == player.target.gameObject)
        {
            int index = 0;
            List<int> inventoryL = inventory.ToList();
            List<int> carL = car.ToList();

            if (carL.Count > 0)
            {
                for (int i = 0; i < carL.Count; i++)
                {
                    index = i;
                    player.InventoryAdd(new Item(player.playerCar.car.inventory[carL[index]].item.data), player.playerCar.car.inventory[carL[index]].amount);
                    player.playerCar.car.inventory[carL[index]] = new ItemSlot();
                }
            }
            index = 0;
            if (inventoryL.Count > 0)
            {
                for (int i = 0; i < inventoryL.Count; i++)
                {
                    index = i;
                    player.playerCar.car.InventoryAdd(new Item(player.inventory[inventoryL[index]].item.data), player.inventory[inventoryL[index]].amount);
                    player.inventory[inventoryL[index]] = new ItemSlot();
                }
            }
        }
    }

    public bool CheckIfCanChangeItemBetweenCarAndInventory(int[] car, int[] inventory)
    {
        int index = 0;
        for (int e = 0; e < car.Length; e++)
        {
            index = e;
            if (!player.InventoryCanAdd(new Item(player.playerCar.car.inventory[car[index]].item.data), player.playerCar.car.inventory[car[index]].amount)) return false;
        }
        index = 0;
        for (int e = 0; e < inventory.Length; e++)
        {
            index = e;
            if (!player.playerCar.car.InventoryCanAdd(new Item(player.inventory[inventory[index]].item.data), player.inventory[inventory[index]].amount)) return false;
        }

        return true;
    }
}

public partial class PlayerRock
{
    public Player player;
    public GameObject damageObject;
    public ScriptableAbility ability;

    void Start()
    {
        ability = GeneralManager.singleton.rockAbility;
    }


    [TargetRpc]
    public void TargetDamage(string rewardName, string itemName)
    {
        if (player.target)
        {
            GameObject g = Instantiate(damageObject);
            g.transform.position = player.target.transform.position;
            ResourceScript resourceScript = g.GetComponent<ResourceScript>();
            resourceScript.textMesh.text = " + " + itemName;
            resourceScript.spriteRenderer.sprite = GeneralManager.singleton.GetRewardImage(rewardName);
            resourceScript.player = GetComponent<Player>();
            resourceScript.AssignAndDecrease(resourceScript.textMesh.text, resourceScript.spriteRenderer.sprite);
        }
    }
}

public partial class PlayerTree
{
    public Player player;
    public GameObject damageObject;
    public ScriptableAbility ability;

    void Start()
    {
        ability = GeneralManager.singleton.treeAbility;
    }


    [TargetRpc]
    public void TargetDamage(string rewardName, string itemName)
    {
        if (player.target)
        {
            GameObject g = Instantiate(damageObject);
            g.transform.position = player.target.transform.position;
            ResourceScript resourceScript = g.GetComponent<ResourceScript>();
            resourceScript.textMesh.text = " + " + itemName;
            resourceScript.spriteRenderer.sprite = GeneralManager.singleton.GetRewardImage(rewardName);
            resourceScript.player = GetComponent<Player>();
            resourceScript.AssignAndDecrease(resourceScript.textMesh.text, resourceScript.spriteRenderer.sprite);
        }
    }
}

public partial class PlayerPlant
{
    public Player player;
    public GameObject damageObject;
    public List<MedicalPlant> plantObject;
    public MedicalPlant selectedMedicalPlant;

    public void Update()
    {
        if (player.isClient && player.isLocalPlayer)
        {
            plantObject = plantObject.Where(item => item != null).ToList();

            if (plantObject.Count > 0)
            {
                if (plantObject != null)
                {
                    selectedMedicalPlant = plantObject[0];
                    if (!GeneralManager.singleton.spawnedPlantUIObject)
                    {
                        GeneralManager.singleton.spawnedPlantUIObject = Instantiate(GeneralManager.singleton.plantUIObject, GeneralManager.singleton.canvas);
                    }
                }
            }
            else
            {
                if (GeneralManager.singleton.spawnedPlantUIObject)
                {
                    Destroy(GeneralManager.singleton.spawnedPlantUIObject);
                }
            }
        }
    }

    [Command]
    public void CmdAddPlant(string itemName, GameObject obj)
    {
        ScriptableItem item = obj.GetComponent<MedicalPlant>().reward;
        int abilityLevel = GeneralManager.singleton.FindNetworkAbilityLevel("Farmer", player.name);
        if (abilityLevel <= 10) abilityLevel = 1;
        else
        {
            abilityLevel = Convert.ToInt32(abilityLevel / 10);
        }
        if (player.InventoryCanAdd(new Item(item), abilityLevel))
        {
            player.InventoryAdd(new Item(item), abilityLevel);
            TargetDamage(itemName, obj, abilityLevel);
        }
    }

    [TargetRpc]
    public void TargetDamage(string itemName, GameObject obj, int amount)
    {
        GameObject g = Instantiate(damageObject);
        g.transform.position = selectedMedicalPlant.transform.position;
        ResourceScript resourceScript = g.GetComponent<ResourceScript>();
        resourceScript.textMesh.text = " + " + amount + itemName;
        resourceScript.spriteRenderer.gameObject.SetActive(false);
        resourceScript.plantSpriteRenderer.gameObject.SetActive(true);

        resourceScript.plantSpriteRenderer.sprite = selectedMedicalPlant.reward.image;
        CmdDestroyPlant(obj.gameObject);
    }

    [Command]
    public void CmdDestroyPlant(GameObject obj)
    {
        NetworkServer.Destroy(obj);
    }
}

public partial class EntityObstacleCheck
{
    public Entity entity;
    public RaycastHit2D[] obstacle;
    public LayerMask obstacleLayer;

    public List<Collider2D> colliders = new List<Collider2D>();

    public void Start()
    {
        entity = GetComponent<Entity>();
    }

    public bool CheckObstacle(Vector2 monsterDestination)
    {
        if (entity is Monster)
        {
            if (entity.target)
            {
                colliders = new List<Collider2D>();
                obstacle = Physics2D.LinecastAll(entity.transform.position, entity.target.transform.position, obstacleLayer);

                for (int i = 0; i < obstacle.Length; i++)
                {
                    if (!colliders.Contains(obstacle[i].collider)) colliders.Add(obstacle[i].collider);
                }

                if (obstacle.Length > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                colliders = new List<Collider2D>();
                obstacle = Physics2D.LinecastAll(entity.transform.position, monsterDestination, obstacleLayer);

                for (int i = 0; i < obstacle.Length; i++)
                {
                    if (!colliders.Contains(obstacle[i].collider)) colliders.Add(obstacle[i].collider);
                }

                if (obstacle.Length > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        if (entity is Player)
        {
            if (!entity.target) return false;

            obstacle = Physics2D.LinecastAll(entity.transform.position, entity.target.transform.position, obstacleLayer);
            if (obstacle.Length > 0)
            {
                if (obstacle[0].collider == entity.target.collider)
                {
                    return true;
                }
                return true;
            }
            else
            {
                return true;
            }
        }

        return false;
    }
}

public partial class PlayerWet
{
    public Player player;
    [SyncVar]
    public float wetEquipment;
    [SyncVar]
    public float wetEquipmentNaked;
    [SyncVar]
    public float maxWetEquipment;
    [SyncVar]
    public float maxWetEquipmentNaked;
    [SyncVar]
    public float avgWetEquipment;
    private int index = 0;

    public void Start()
    {
        if (player.isServer)
        {
            InvokeRepeating(nameof(GetMaxWetEquipment), GeneralManager.singleton.wetInterval, GeneralManager.singleton.wetInterval);
            InvokeRepeating(nameof(GetWetEquipment), GeneralManager.singleton.wetInterval, GeneralManager.singleton.wetInterval);
            InvokeRepeating(nameof(CheckWet), GeneralManager.singleton.wetInterval, GeneralManager.singleton.wetInterval);
            InvokeRepeating(nameof(WetNotification), 60.0f, 60.0f);
        }
    }

    public void WetNotification()
    {
        if (maxWetEquipmentNaked > 0)
        {
            if (wetEquipmentNaked >= avgWetEquipment)
            {
                TargetSpawnAdvertise(player.netIdentity.connectionToClient);
            }
        }
        else if (maxWetEquipment > 0)
        {
            if (wetEquipment >= avgWetEquipment)
            {
                TargetSpawnAdvertise(player.netIdentity.connectionToClient);
            }
        }
    }


    public void CheckWet()
    {
        if (maxWetEquipmentNaked > 0)
        {
            avgWetEquipment = maxWetEquipmentNaked;
        }
        else
        {
            avgWetEquipment = ((maxWetEquipment / 100.0f) * 80.0f);
        }

        if (TemperatureManager.singleton.isRainy)
        {
            if (maxWetEquipmentNaked == 0)
            {
                wetEquipmentNaked = 0;
                for (int i = 0; i < player.equipment.Count; i++)
                {
                    int index = i;
                    if (player.equipment[index].amount > 0 && player.equipment[index].item.data is EquipmentItem && ((EquipmentItem)player.equipment[index].item.data).maxWet > 0.0f)
                    {
                        if (player.equipment[index].item.wet < ((EquipmentItem)player.equipment[index].item.data).maxWet)
                        {
                            ItemSlot slot = player.equipment[index];
                            slot.item.wet += 0.01f;
                            player.equipment[index] = slot;
                        }
                    }
                }
            }
            else
            {
                if (wetEquipmentNaked < maxWetEquipmentNaked)
                {
                    wetEquipmentNaked += 0.01f;
                }
                else if (wetEquipmentNaked >= maxWetEquipmentNaked)
                {
                    wetEquipmentNaked = maxWetEquipmentNaked;
                }
            }
        }

        if (TemperatureManager.singleton.season == "Winter" || TemperatureManager.singleton.season == "Autumn")
        {
            if (wetEquipment > 0)
            {
                if (wetEquipment >= avgWetEquipment)
                {
                    player.health -= GeneralManager.singleton.decreaseHealthIfWet;
                }
            }
            else if (wetEquipmentNaked > 0)
            {
                if (wetEquipmentNaked >= avgWetEquipment)
                {
                    player.health -= GeneralManager.singleton.decreaseHealthIfWet;
                }
            }
        }
        if (TemperatureManager.singleton.season == "Summer")
        {
            for (int i = 0; i < player.equipment.Count; i++)
            {
                int index = i;
                if (player.equipment[index].amount > 0 && player.equipment[index].item.wet > 0.0f)
                {
                    ItemSlot slot = player.equipment[index];
                    slot.item.wet -= 0.01f;
                    if (slot.item.wet < 0.0f)
                        slot.item.wet = 0.0f;
                    player.equipment[index] = slot;
                }
            }
        }
    }
    [TargetRpc]
    public void TargetSpawnAdvertise(NetworkConnection connection)
    {
        UINotificationManager.singleton.SpawnWetObject();
    }

    public void GetWetEquipment()
    {
        float equipmentBonus = 0;
        foreach (ItemSlot slot in player.equipment)
            if (slot.amount > 0)
                equipmentBonus += slot.item.wet;

        if (maxWetEquipmentNaked == 0)
            wetEquipment = equipmentBonus;
    }

    public void GetMaxWetEquipment()
    {
        float equipmentBonus = 0;
        foreach (ItemSlot slot in player.equipment)
            if (slot.amount > 0 && slot.item.data is EquipmentItem)
                equipmentBonus += ((EquipmentItem)slot.item.data).maxWet;

        maxWetEquipment = equipmentBonus;

        if (maxWetEquipment == 0)
            maxWetEquipmentNaked = 0.03f;
        else
            maxWetEquipmentNaked = 0.0f;
    }

}

public partial class PlayerCreation
{
    public Player player;

    [SyncVar]
    public int sex;
    [SyncVar(hook = nameof(ChangeWeigth))]
    public float fat;
    [SyncVar(hook = nameof(ChangeThin))]
    public float thin;
    [SyncVar(hook = nameof(ChangeMuscle))]
    public float muscle;
    [SyncVar(hook = nameof(ChangeHeight))]
    public float height;
    [SyncVar(hook = nameof(ChangeBreastSize))]
    public float breast;

    [SyncVar(hook = nameof(ChangeHairType))]
    public int hairType;
    [SyncVar(hook = nameof(ChangeBeardType))]
    public int beard;
    [SyncVar(hook = nameof(ChangeHairColor))]
    public string hairColor;
    [SyncVar(hook = nameof(ChangeUnderpantsColor))]
    public string underwearColor;
    [SyncVar(hook = nameof(ChangeEyesColor))]
    public string eyesColor;
    [SyncVar(hook = nameof(ChangeSkinColor))]
    public string skinColor;


    [SyncVar(hook = nameof(ChangeHats))]
    public int hats;
    [SyncVar(hook = nameof(ChangeAccessory))]
    public int accessory;
    [SyncVar(hook = nameof(ChangeUpper))]
    public int upper;
    [SyncVar(hook = nameof(ChangeDown))]
    public int down;
    [SyncVar(hook = nameof(ChangeShoes))]
    public int shoes;

    public GameObject dummyPresentation;

    public float percSlider;

    public ItemSlot accessorySlot;
    public ItemSlot hatsSlot;

    public CharacterCustomization characterCustomization;

    Color newCol;

    bool isThisIsLocalPlayer = false;

    void Start()
    {

        if (!GetComponent<SelectableCharacter>() && !isServer && !isClient)
        {
            player.nameOverlay.gameObject.SetActive(false);
        }
        else
        {
            player.nameOverlay.gameObject.SetActive(true);
        }

        NewSetup();
    }

    public void ChangeHairType(int oldHair, int newHair)
    {
        if (player.prefabPreview)
        {
            characterCustomization.SetElementByIndex(CharacterElementType.Hair, newHair);
        }
    }

    public void ChangeHairColor(string oldHairColor, string newHairColor)
    {
        if (player.prefabPreview)
        {
            if (ColorUtility.TryParseHtmlString(newHairColor, out newCol))
                characterCustomization.SetBodyColor(BodyColorPart.Hair, newCol);
        }
    }

    public void ChangeUnderpantsColor(string oldUnderpantsColor, string newUnderpantsColor)
    {
        if (player.prefabPreview)
        {
            if (ColorUtility.TryParseHtmlString(newUnderpantsColor, out newCol))
                characterCustomization.SetBodyColor(BodyColorPart.Underpants, newCol);
        }
    }

    public void ChangeEyesColor(string oldEyesColor, string newEyesColor)
    {
        if (player.prefabPreview)
        {
            if (ColorUtility.TryParseHtmlString(newEyesColor, out newCol))
                characterCustomization.SetBodyColor(BodyColorPart.Eye, newCol);
        }
    }

    public void ChangeSkinColor(string oldSkinColor, string newSkinColor)
    {
        if (player.prefabPreview)
        {
            if (ColorUtility.TryParseHtmlString(newSkinColor, out newCol))
                characterCustomization.SetBodyColor(BodyColorPart.Skin, newCol);
        }
    }

    public void ChangeBeardType(int oldHair, int newHair)
    {
        if (player.prefabPreview)
        {
            characterCustomization.SetElementByIndex(CharacterElementType.Beard, newHair);
        }
    }

    public void ChangeWeigth(float oldWeigth, float newWeigth)
    {
        if (player.prefabPreview)
        {
            characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Fat, newWeigth);
        }
    }

    public void ChangeThin(float oldThin, float newThin)
    {
        if (sex == 0)
        {
            if (player.prefabPreview)
            {
                characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Thin, newThin);
            }
        }
    }

    public void ChangeMuscle(float oldMuscle, float newMuscle)
    {
        if (sex == 0)
        {

            if (player.prefabPreview)
            {
                characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Muscles, newMuscle);
            }
        }
    }

    public void ChangeHeight(float oldHeight, float newHeight)
    {
        if (player.prefabPreview)
        {
            characterCustomization.SetHeight(newHeight);
        }
    }

    public void ChangeBreastSize(float oldBrestSize, float newBrestSize)
    {
        if (sex == 1)
        {
            if (player.prefabPreview)
            {
                characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.BreastSize, newBrestSize);
            }
        }
    }

    public void ChangeHats(int oldHats, int newHats)
    {
        if (player.prefabPreview)
        {
            if (newHats != -1)
            {
                characterCustomization.SetElementByIndex(CharacterElementType.Hat, newHats);
            }
            else
            {
                characterCustomization.SetElementByIndex(CharacterElementType.Hat, -1);
            }
        }
    }

    public void ChangeAccessory(int oldAccessory, int newAccessory)
    {
        if (player.prefabPreview)
        {
            if (newAccessory != -1)
            {
                characterCustomization.SetElementByIndex(CharacterElementType.Accessory, newAccessory);
            }
            else
            {
                characterCustomization.SetElementByIndex(CharacterElementType.Accessory, -1);
            }
        }
    }

    public void ChangeUpper(int oldUpper, int newUpper)
    {
        if (player.prefabPreview)
        {
            if (newUpper != -1)
            {
                characterCustomization.SetElementByIndex(CharacterElementType.Shirt, newUpper);
            }
            else
            {
                characterCustomization.SetElementByIndex(CharacterElementType.Shirt, -1);
            }
        }
    }

    public void ChangeDown(int oldDown, int newDown)
    {
        if (player.prefabPreview)
        {
            if (newDown != -1)
            {
                characterCustomization.SetElementByIndex(CharacterElementType.Pants, newDown);
            }
            else
            {
                characterCustomization.SetElementByIndex(CharacterElementType.Pants, -1);
            }
        }
    }

    public void ChangeShoes(int oldShoes, int newShoes)
    {
        if (player.prefabPreview)
        {
            if (newShoes != -1)
            {
                characterCustomization.SetElementByIndex(CharacterElementType.Shoes, newShoes);
            }
            else
            {
                characterCustomization.SetElementByIndex(CharacterElementType.Shoes, -1);
            }
        }

    }

    public void NewSetup()
    {
        ChangeHairColor(hairColor, hairColor);
        ChangeHairType(hairType, hairType);
        ChangeUnderpantsColor(underwearColor, underwearColor);
        ChangeEyesColor(eyesColor, eyesColor);
        ChangeSkinColor(skinColor, skinColor);
        ChangeBeardType(beard, beard);
        ChangeWeigth(fat, fat);
        ChangeThin(thin, thin);
        ChangeMuscle(muscle, muscle);
        ChangeHeight(height, height);
        ChangeBreastSize(breast, breast);
        ChangeHats(hats, hats);
        ChangeAccessory(accessory, accessory);
        ChangeUpper(upper, upper);
        ChangeDown(down, down);
        ChangeShoes(shoes, shoes);

    }

    //void Update()
    //{
    //    NewSetup();
    //}

    public void CheckAccessory()
    {
        if (isServer)
        {
            if (player.equipment.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Accessory")) != -1)
            {
                accessorySlot = player.equipment[player.equipment.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Accessory"))];
            }
            else
            {
                accessorySlot.amount = 0;
                accessory = -1;
            }
        }
    }

    public void CheckHat()
    {
        if (isServer)
        {
            if (player.equipment.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Head")) != -1)
            {
                hatsSlot = player.equipment[player.equipment.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Head"))];
            }
            else
            {
                hatsSlot.amount = 0;
                hats = -1;
            }
        }
    }

}

public partial class Player
{
    [Command]
    public void CmdSpawnRankBadge()
    {
        GameObject returnGameobject = GeneralManager.singleton.GetPlayerRank(playerLeaderPoints.personalPoint);
        if (returnGameobject != null && nextRiskyActionTime < NetworkTime.time)
        {
            GameObject g = Instantiate(returnGameobject);
            g.GetComponent<SpawnedBadge>().playerName = name;
            NetworkServer.Spawn(g);
            nextRiskyActionTime = NetworkTime.time + couponWaitSeconds;
        }
    }
}

public partial class PlayerTeleport : NetworkBehaviour
{
    public Player player;

    [SyncVar(hook = nameof(ItemInUse))]
    public int itemInUse = -1;
    [SyncVar(hook = nameof(Inviter))]
    public string inviterName;
    [SyncVar]
    public int countdown;

    void Start()
    {
        if (player.isServer)
            InvokeRepeating(nameof(DecreaseCountdown), 1.0f, 1.0f);
    }

    //void Update()
    //{
    //    if (player.isLocalPlayer)
    //    {
    //        if (inviterName != string.Empty)
    //        {
    //            if (GeneralManager.singleton.spawnedTeleport == null)
    //            {
    //                GeneralManager.singleton.spawnedTeleport = Instantiate(GeneralManager.singleton.teleportInviteSlot, GeneralManager.singleton.canvas);

    //            }
    //        }
    //        if (itemInUse > -1)
    //        {
    //            if (GeneralManager.singleton.spawnedteleportInviter == null)
    //            {
    //                GeneralManager.singleton.spawnedteleportInviter = Instantiate(GeneralManager.singleton.teleportInviter, GeneralManager.singleton.canvas);
    //            }
    //        }
    //        if (itemInUse == -1)
    //        {
    //            if (GeneralManager.singleton.spawnedteleportInviter != null)
    //            {
    //                Destroy(GeneralManager.singleton.spawnedteleportInviter);
    //            }
    //        }
    //    }
    //}

    public void Inviter(string oldString, string newString)
    {
        if (player.isLocalPlayer)
        {
            if (newString != string.Empty)
            {
                if (GeneralManager.singleton.spawnedTeleport == null)
                {
                    GeneralManager.singleton.spawnedTeleport = Instantiate(GeneralManager.singleton.teleportInviteSlot, GeneralManager.singleton.canvas);

                }
            }
        }
    }

    public void ItemInUse(int oldInt, int newInt)
    {
        if (player.isLocalPlayer)
        {
            if (newInt > -1)
            {
                if (GeneralManager.singleton.spawnedteleportInviter == null)
                {
                    GeneralManager.singleton.spawnedteleportInviter = Instantiate(GeneralManager.singleton.teleportInviter, GeneralManager.singleton.canvas);
                }
            }
            if (newInt == -1)
            {
                if (GeneralManager.singleton.spawnedteleportInviter != null)
                {
                    Destroy(GeneralManager.singleton.spawnedteleportInviter);
                }
            }
        }
    }

    public void DecreaseCountdown()
    {
        if (player.playerTeleport.countdown > 0)
        {
            player.playerTeleport.countdown--;
        }
        if (player.playerTeleport.countdown == 0)
        {
            inviterName = string.Empty;
        }
    }

    [Command]
    public void CmdTeleportToFriends()
    {
        bool playerTeleport = false;
        Player.onlinePlayers.TryGetValue(inviterName, out Player inviter);
        if (inviter)
        {
            player.agent.Warp(inviter.transform.position);
            inviterName = string.Empty;
            countdown = 0;
        }
    }

    [Command]
    public void CmdTeleportDecline()
    {
        inviterName = string.Empty;
        countdown = 0;
    }

    [Command]
    public void CmdSendTeleportInvite(string playerName)
    {
        Player.onlinePlayers.TryGetValue(playerName, out Player inviter);
        ItemSlot slot = player.inventory[itemInUse];
        if (inviter && inviter.playerTeleport.itemInUse == -1 && slot.item.data is TeleportItem)
        {
            inviter.playerTeleport.inviterName = name;
            inviter.playerTeleport.countdown = GeneralManager.singleton.teleportSeconds;
            slot.amount--;
            player.inventory[itemInUse] = slot;
        }
        itemInUse = -1;
    }

    [Command]
    public void CmdRemoveTeleport()
    {
        itemInUse = -1;
    }
}

public partial class PlayerBelt
{
    public Player player;
    public SyncListItemSlot belt = new SyncListItemSlot();

    public void Start()
    {
        if (player.isServer)
        {
            for (int i = 0; i < 5; i++)
            {
                belt.Add(new ItemSlot());
            }
            Database.singleton.LoadBelt(player);
        }
    }

    [Server]
    public void ClearSummonedPet()
    {
        for (int i = 0; i < player.inventory.Count; i++)
        {
            if (player.inventory[i].item.data is PetItem)
            {
                ItemSlot petItem = player.inventory[i];
                if (petItem.item.isSummoned)
                {
                    petItem.item.isSummoned = false;
                    player.inventory[i] = petItem;
                }
            }
        }

        for (int i = 0; i < player.playerBelt.belt.Count; i++)
        {
            if (player.playerBelt.belt[i].item.data is PetItem)
            {
                ItemSlot petItem = player.playerBelt.belt[i];
                if (petItem.item.isSummoned)
                {
                    petItem.item.isSummoned = false;
                    player.playerBelt.belt[i] = petItem;
                }
            }
        }
    }


    [Command]
    public void CmdUseBeltItem(int index)
    {
        // validate
        if (0 <= index && index < belt.Count && belt[index].amount > 0 &&
            belt[index].item.data is UsableItem)
        {
            if (belt[index].item.data is ScriptableBuilding)
                if (player.playerCar.car)
                    return;

            UsableItem itemData = (UsableItem)player.playerBelt.belt[index].item.data;
            if (itemData.CanUseBelt(player, index))
            {
                if (player.playerBelt.belt[index].amount > 0)
                {
                    if (player.playerBelt.belt[index].item.data is PetItem)
                    {
                        ItemSlot slot = player.playerBelt.belt[index];
                        slot.item.isSummoned = true;
                        player.playerBelt.belt[index] = slot;
                    }
                }
                if (itemData is WeaponItem)
                {
                    for (int i = 0; i < player.quests.Count; i++)
                    {
                        Quest quest = player.quests[i];
                        if (quest.data.equipWeapon == true)
                        {
                            quest.checkEquipWeapon = true;
                            player.quests[i] = quest;
                        }
                    }
                    if (((WeaponItem)itemData).ammoItems.Count > 0)
                    {
                        bool alreadySetted = false;

                        for (int amm = 0; amm < ((WeaponItem)itemData).ammoItems.Count; amm++)
                        {
                            if (player.equipment[5].amount > 0 && player.equipment[5].item.name == ((WeaponItem)itemData).ammoItems[amm].name)
                            {
                                alreadySetted = true;
                            }
                        }
                        if (alreadySetted == false)
                        {
                            alreadySetted = false;
                            for (int amm = 0; amm < ((WeaponItem)itemData).ammoItems.Count; amm++)
                            {
                                if (alreadySetted == false && player.GetAmmoIndex(((WeaponItem)itemData).ammoItems[amm].name) > -1)
                                {
                                    player.SetAmmo(((WeaponItem)itemData).ammoItems[amm].name);
                                    alreadySetted = true;
                                }
                            }
                        }
                    }
                }

                if (itemData is EquipmentItem && itemData.possibleBagWeight.baseValue > 0)
                {
                    for (int i = 0; i < player.quests.Count; i++)
                    {
                        Quest quest = player.quests[i];
                        if (quest.data.equipBag == true)
                        {
                            quest.checkEquipBag = true;
                            player.quests[i] = quest;
                        }
                    }
                }

                if (itemData is FoodItem || itemData is ScriptablePlant)
                {
                    for (int i = 0; i < player.quests.Count; i++)
                    {
                        Quest quest = player.quests[i];
                        if (quest.data.eat == true)
                        {
                            quest.checkEat = true;
                            player.quests[i] = quest;
                        }
                    }
                    player.playerMove.TargetSetEatAnimation(connectionToClient);
                }

                if (itemData is TeleportItem)
                {
                    for (int i = 0; i < player.quests.Count; i++)
                    {
                        Quest quest = player.quests[i];
                        if (quest.data.useTeleport == true)
                        {
                            quest.checkUseTeleport = true;
                            player.quests[i] = quest;
                        }
                    }
                }

                if (itemData is UsableItem && itemData.name == GeneralManager.singleton.Instantresurrect.name)
                {
                    for (int i = 0; i < player.quests.Count; i++)
                    {
                        Quest quest = player.quests[i];
                        if (quest.data.useTeleport == true)
                        {
                            quest.checkUseTeleport = true;
                            player.quests[i] = quest;
                        }
                    }
                }

                // .Use might clear the slot, so we backup the Item first for the Rpc
                Item item = player.playerBelt.belt[index].item;
                itemData.UseBelt(player, index);
                player.RpcUsedItem(item);
                return;
            }

        }
    }

    public bool InventoryOperationsAllowed()
    {
        return player.state == "IDLE" ||
               player.state == "MOVING" ||
               player.state == "CASTING" ||
               (player.state == "TRADING" && player.tradeStatus == TradeStatus.Free);
    }

    [Server]
    public void SwapBeltEquip(int inventoryIndex, int equipmentIndex)
    {
        // validate: make sure that the slots actually exist in the inventory
        // and in the equipment
        if (InventoryOperationsAllowed() &&
            0 <= inventoryIndex && inventoryIndex < belt.Count &&
            0 <= equipmentIndex && equipmentIndex < player.equipment.Count)
        {
            // item slot has to be empty (unequip) or equipable
            ItemSlot slot = belt[inventoryIndex];
            if (slot.amount == 0 ||
                slot.item.data is EquipmentItem itemData &&
                itemData.CanEquip(player, inventoryIndex, equipmentIndex))
            {
                // swap them
                //ItemSlot temp = equipment[equipmentIndex];
                //equipment[equipmentIndex] = slot;
                //inventory[inventoryIndex] = temp;
                if (player.equipment[equipmentIndex].amount == 0 || player.CanUnEquip(player.equipment[equipmentIndex].item))
                {
                    // swap them
                    var temp = player.equipment[equipmentIndex];
                    player.equipment[equipmentIndex] = belt[inventoryIndex];
                    belt[inventoryIndex] = temp;
                }
                if (((EquipmentItem)slot.item.data).indexHat > -1)
                {
                    player.playerCreation.hats = ((EquipmentItem)slot.item.data).indexHat;
                }
                if (((EquipmentItem)slot.item.data).indexAccessory > -1)
                {
                    player.playerCreation.accessory = ((EquipmentItem)slot.item.data).indexAccessory;
                }
                if (((EquipmentItem)slot.item.data).indexShirt > -1)
                {
                    player.playerCreation.upper = ((EquipmentItem)slot.item.data).indexShirt;
                }
                if (((EquipmentItem)slot.item.data).indexPants > -1)
                {
                    player.playerCreation.down = ((EquipmentItem)slot.item.data).indexPants;
                }
                if (((EquipmentItem)slot.item.data).indexShoes > -1)
                {
                    player.playerCreation.shoes = ((EquipmentItem)slot.item.data).indexShoes;
                }
            }
        }
    }


    [Server]
    public void UseInventoryItem(int index)
    {
        // validate
        if (player.InventoryOperationsAllowed() &&
            0 <= index && index < player.inventory.Count && player.inventory[index].amount > 0 &&
            player.inventory[index].item.data is UsableItem)
        {
            if (player.inventory[index].item.data is ScriptableBuilding)
                if (player.playerCar.car)
                    return;
            // use item
            // note: we don't decrease amount / destroy in all cases because
            // some items may swap to other slots in .Use()
            UsableItem itemData = (UsableItem)player.inventory[index].item.data;
            if (itemData.CanUse(player, index))
            {
                // .Use might clear the slot, so we backup the Item first for the Rpc
                Item item = player.inventory[index].item;
                itemData.Use(player, index);
                player.RpcUsedItem(item);
            }
        }
    }


    [Server]
    public void MergeInventoryBelt(int inventoryIndex, int equipmentIndex)
    {
        if (player.InventoryOperationsAllowed() &&
            0 <= inventoryIndex && inventoryIndex < player.inventory.Count &&
            0 <= equipmentIndex && equipmentIndex < player.playerBelt.belt.Count)
        {
            // both items have to be valid
            // note: no 'is EquipmentItem' check needed because we already
            //       checked when equipping 'slotTo'.
            ItemSlot slotFrom = player.inventory[inventoryIndex];
            ItemSlot slotTo = player.playerBelt.belt[equipmentIndex];
            if (slotFrom.amount > 0 && slotTo.amount > 0)
            {
                // make sure that items are the same type
                // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                if (slotFrom.item.Equals(slotTo.item))
                {
                    // merge from -> to
                    // put as many as possible into 'To' slot
                    int put = slotTo.IncreaseAmount(slotFrom.amount);
                    slotFrom.DecreaseAmount(put);

                    // put back into the list
                    player.inventory[inventoryIndex] = slotFrom;
                    player.playerBelt.belt[equipmentIndex] = slotTo;
                }
            }
        }
    }

    [Command]
    public void CmdMergeInventoryBelt(int equipmentIndex, int inventoryIndex)
    {
        MergeInventoryBelt(equipmentIndex, inventoryIndex);
    }

    [Server]
    public void SwapInventoryBelt(int inventoryIndex, int equipmentIndex)
    {
        // validate: make sure that the slots actually exist in the inventory
        // and in the equipment
        if (player.InventoryOperationsAllowed() &&
            0 <= inventoryIndex && inventoryIndex < player.inventory.Count &&
            0 <= equipmentIndex && equipmentIndex < player.playerBelt.belt.Count)
        {
            // item slot has to be empty (unequip) or equipable
            ItemSlot slot = player.inventory[inventoryIndex];
            //swap them
            ItemSlot temp = player.playerBelt.belt[equipmentIndex];
            player.playerBelt.belt[equipmentIndex] = slot;
            player.inventory[inventoryIndex] = temp;

        }
    }

    [Command]
    public void CmdSwapInventoryBelt(int inventoryIndex, int equipmentIndex)
    {
        SwapInventoryBelt(inventoryIndex, equipmentIndex);
    }


    [Command]
    public void CmdBeltSplit(int fromIndex, int toIndex)
    {
        // note: should never send a command with complex types!
        // validate: make sure that the slots actually exist in the inventory
        // and that they are not equal
        if (player.InventoryOperationsAllowed() &&
            0 <= fromIndex && fromIndex < player.playerBelt.belt.Count &&
            0 <= toIndex && toIndex < player.playerBelt.belt.Count &&
            fromIndex != toIndex)
        {
            // slotFrom needs at least two to split, slotTo has to be empty
            ItemSlot slotFrom = player.playerBelt.belt[fromIndex];
            ItemSlot slotTo = player.playerBelt.belt[toIndex];
            if (slotFrom.amount >= 2 && slotTo.amount == 0)
            {
                // split them serversided (has to work for even and odd)
                slotTo = slotFrom; // copy the value

                slotTo.amount = slotFrom.amount / 2;
                slotFrom.amount -= slotTo.amount; // works for odd too

                // put back into the list
                player.playerBelt.belt[fromIndex] = slotFrom;
                player.playerBelt.belt[toIndex] = slotTo;
            }
        }
    }

    [Command]
    public void CmdBeltMerge(int fromIndex, int toIndex)
    {
        if (player.InventoryOperationsAllowed() &&
            0 <= fromIndex && fromIndex < player.playerBelt.belt.Count &&
            0 <= toIndex && toIndex < player.playerBelt.belt.Count &&
            fromIndex != toIndex)
        {
            // both items have to be valid
            ItemSlot slotFrom = player.playerBelt.belt[fromIndex];
            ItemSlot slotTo = player.playerBelt.belt[toIndex];
            if (slotFrom.amount > 0 && slotTo.amount > 0)
            {
                // make sure that items are the same type
                // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                if (slotFrom.item.Equals(slotTo.item))
                {
                    // merge from -> to
                    // put as many as possible into 'To' slot
                    int put = slotTo.IncreaseAmount(slotFrom.amount);
                    slotFrom.DecreaseAmount(put);

                    // put back into the list
                    player.playerBelt.belt[fromIndex] = slotFrom;
                    player.playerBelt.belt[toIndex] = slotTo;
                }
            }
        }
    }

    [Command]
    public void CmdSwapBeltBelt(int fromIndex, int toIndex)
    {
        // note: should never send a command with complex types!
        // validate: make sure that the slots actually exist in the inventory
        // and that they are not equal
        if (player.InventoryOperationsAllowed() &&
            0 <= fromIndex && fromIndex < player.playerBelt.belt.Count &&
            0 <= toIndex && toIndex < player.playerBelt.belt.Count &&
            fromIndex != toIndex)
        {
            // swap them
            ItemSlot temp = player.playerBelt.belt[fromIndex];
            player.playerBelt.belt[fromIndex] = player.playerBelt.belt[toIndex];
            player.playerBelt.belt[toIndex] = temp;
        }
    }


    [Server]
    public void SwapBeltInventory(int inventoryIndex, int equipmentIndex)
    {
        // validate: make sure that the slots actually exist in the inventory
        // and in the equipment
        if (player.InventoryOperationsAllowed() &&
            0 <= inventoryIndex && inventoryIndex < player.playerBelt.belt.Count &&
            0 <= equipmentIndex && equipmentIndex < player.inventory.Count)
        {
            // item slot has to be empty (unequip) or equipable
            ItemSlot slot = player.playerBelt.belt[inventoryIndex];
            //swap them
            ItemSlot temp = player.inventory[equipmentIndex];
            player.inventory[equipmentIndex] = slot;
            player.playerBelt.belt[inventoryIndex] = temp;

        }
    }

    [Command]
    public void CmdSwapBeltInventory(int inventoryIndex, int equipmentIndex)
    {
        SwapBeltInventory(inventoryIndex, equipmentIndex);
    }



    [Command]
    public void CmdMergeBeltInventory(int equipmentIndex, int inventoryIndex)
    {
        if (player.InventoryOperationsAllowed() &&
            0 <= inventoryIndex && inventoryIndex < player.inventory.Count &&
            0 <= equipmentIndex && equipmentIndex < belt.Count)
        {
            // both items have to be valid
            ItemSlot slotFrom = belt[equipmentIndex];
            ItemSlot slotTo = player.inventory[inventoryIndex];
            if (slotFrom.amount > 0 && slotTo.amount > 0)
            {
                // make sure that items are the same type
                // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                if (slotFrom.item.Equals(slotTo.item))
                {
                    // merge from -> to
                    // put as many as possible into 'To' slot
                    int put = slotTo.IncreaseAmount(slotFrom.amount);
                    slotFrom.DecreaseAmount(put);

                    // put back into the list
                    belt[equipmentIndex] = slotFrom;
                    player.inventory[inventoryIndex] = slotTo;
                }
            }
        }
    }

}

public partial class PlayerQuest
{
    public Player player;
    public List<ScriptableQuest> playerQuest = new List<ScriptableQuest>();

    public void Start()
    {
        if (player.isServer)
            InvokeRepeating(nameof(CheckQuest), 0.0f, 3.0f);
    }

    public void CheckQuest()
    {
        if (player.quests.Count == 0)
        {
            //foreach(ScriptableQuest quest in playerQuest)
            for (int i = 0; i < playerQuest.Count; i++)
            {
                player.quests.Add(new Quest(playerQuest[i]));
            }
        }
        for (int i = 0; i < player.quests.Count; i++)
        {
            Quest quest = player.quests[i];
            if (quest.data.equipWeapon == true)
            {
                if (player.equipment[0].amount > 0) quest.checkEquipWeapon = true;
            }
            if (quest.data.equipBag == true)
            {
                if (player.equipment[7].amount > 0) quest.checkEquipBag = true;
            }
            if (quest.data.createGuild == true)
            {
                if (player.InGuild() && player.guild.master == player.name) quest.checkCreateGuild = true;
            }
            if (quest.data.createParty == true)
            {
                if (player.InParty()) quest.checkCreateParty = true;
            }
            if (quest.data.makeGroupAlly == true)
            {
                if (player.InGuild() && player.playerAlliance.guildAlly.Count > 0) quest.checkMakeGroupAlly = true;
            }
            if (quest.data.buyEmoji == true)
            {
                if (player.playerEmoji.networkEmoji.Count > 0) quest.checkBuyEmoji = true;
            }
            if (quest.data.reachAmountOfFriends > 0)
            {
                if (player.playerFriend.playerFriends.Count > 0) quest.checkFriendCount = true;
            }
            player.quests[i] = quest;
        }
    }

    [Command]
    public void CmdGetQuestReward(int questIndex)
    {
        bool canClaim = true;
        List<Quest> activeQuests = player.quests.Where(q => !q.completed).ToList();
        Quest quests = activeQuests[questIndex];
        ScriptableQuest scriptableQuest = quests.data;

        if (scriptableQuest.ability.Count > 0)
        {
            for (int a = 0; a < scriptableQuest.ability.Count; a++)
            {
                if (GeneralManager.singleton.FindNetworkAbilityLevel(scriptableQuest.ability[a].ability.name, player.name) < scriptableQuest.ability[a].level)
                {
                    canClaim = false;
                }
            }
        }

        if (scriptableQuest.Boosts.Count > 0)
        {
            for (int a = 0; a < scriptableQuest.Boosts.Count; a++)
            {
                if (GeneralManager.singleton.FindNetworkServerBoostTime(scriptableQuest.Boosts[a].boosts.name, player.name) <= 0)
                {
                    canClaim = false;
                }
            }
        }

        if (scriptableQuest.enterPremium == true)
        {
            if (quests.checkEnterPremium == false)
            {
                canClaim = false;
            }
        }

        if (scriptableQuest.createBuilding == true)
        {
            if (quests.checkCreateBuilding == false)
            {
                canClaim = false;
            }
        }

        if (scriptableQuest.killZombie.Count > 0)
        {
            for (int a = 0; a < scriptableQuest.killZombie.Count; a++)
            {
                if (scriptableQuest.killZombie[a].monster.name == "BioHazard Zombie")
                {
                    if (quests.checkPolice < scriptableQuest.killZombie[a].quantity)
                    {
                        canClaim = false;
                    }
                }

                if (scriptableQuest.killZombie[a].monster.name == "Infected Zombie")
                {
                    if (quests.checkBiohazard < scriptableQuest.killZombie[a].quantity)
                    {
                        canClaim = false;
                    }
                }
                if (scriptableQuest.killZombie[a].monster.name == "Mechanic Zombie")
                {
                    if (quests.checkInfected < scriptableQuest.killZombie[a].quantity)
                    {
                        canClaim = false;
                    }
                }
                if (scriptableQuest.killZombie[a].monster.name == "Policeman Zombie")
                {
                    if (quests.checkMechanic < scriptableQuest.killZombie[a].quantity)
                    {
                        canClaim = false;
                    }
                }
            }
        }

        if (quests.data.equipWeapon == true)
        {
            if (quests.checkEquipWeapon == false)
            {
                canClaim = false;
            }
        }

        if (quests.data.equipBag == true)
        {
            if (quests.checkEquipBag == false)
            {
                canClaim = false;
            }
        }

        if (quests.data.createGuild == true)
        {

            if (quests.checkCreateGuild == false)
            {
                canClaim = false;
            }
        }

        if (quests.data.createParty == true)
        {
            if (quests.checkCreateParty == false)
            {
                canClaim = false;
            }
        }

        if (quests.data.makeGroupAlly == true)
        {
            if (quests.checkMakeGroupAlly == false)
            {
                canClaim = false;
            }
        }

        if (quests.data.reachAmountOfFriends > 0)
        {
            if (player.playerFriend.playerFriends.Count < quests.data.reachAmountOfFriends)
            {
                canClaim = false;
            }
        }

        if (quests.data.drink == true)
        {
            if (quests.checkDrink == false)
            {
                canClaim = false;
            }
        }

        if (quests.data.eat == true)
        {
            if (quests.checkEat == false)
            {
                canClaim = false;
            }
        }

        if (quests.data.run == true)
        {
            if (quests.checkRun == false)
            {
                canClaim = false;
            }
        }

        if (quests.data.sneak == true)
        {
            if (quests.checkSneak == false)
            {
                canClaim = false;
            }
        }

        if (quests.data.makeMarriage == true)
        {
            if (quests.checkMakeMarriage == false)
            {
                canClaim = false;
            }
        }

        if (quests.data.sendAMessage == true)
        {
            if (quests.checkSendAMessage == false)
            {
                canClaim = false;
            }
        }

        if (quests.data.buyEmoji == true)
        {
            if (quests.checkBuyEmoji == false)
            {
                canClaim = false;
            }
        }

        if (quests.data.openShop == true)
        {
            if (quests.checkOpenShop == false)
            {
                canClaim = false;
            }
        }

        if (quests.data.amountPlayerToKill > 0)
        {
            if (quests.checkAmountPlayerToKill < quests.data.amountPlayerToKill)
            {
                canClaim = false;
            }
        }

        if (quests.data.makeATrade == true)
        {
            if (quests.checkMakeATrade == false)
            {
                canClaim = false;
            }
        }

        if (quests.data.amountRockToGather > 0)
        {
            if (quests.checkRockToGather < quests.data.amountRockToGather)
            {
                canClaim = false;
            }
        }

        if (quests.data.amountWoodToGather > 0)
        {
            if (quests.checkWoodToGather < quests.data.amountWoodToGather)
            {
                canClaim = false;
            }
        }

        if (quests.data.useTeleport == true)
        {
            if (quests.checkUseTeleport == false)
            {
                canClaim = false;
            }
        }

        if (quests.data.useInstantResurrect == true)
        {
            if (quests.checkUseInstantResurrect == false)
            {
                canClaim = false;
            }
        }

        if (quests.data.setSpawnpoint.numberOfSpawnpointToCreate > 0)
        {
            if (quests.checkCreateASpawnpoint < scriptableQuest.setSpawnpoint.numberOfSpawnpointToCreate)
            {
                canClaim = false;
            }
        }


        if (scriptableQuest.buildingUpgrade.Count > 0)
        {
            for (int a = 0; a < scriptableQuest.buildingUpgrade.Count; a++)
            {
                if (quests.buildingUpgrade[a].itemLevelToReach < scriptableQuest.buildingUpgrade[a].levelToReach)
                {
                    canClaim = false;
                }
            }
        }

        if (canClaim)
        {
            for (int i = 0; i < quests.data.itemRewards.Count; i++)
            {
                if (player.InventoryCanAdd(new Item(quests.data.itemRewards[i].items), quests.data.itemRewards[i].amount))
                {
                    player.InventoryAdd(new Item(quests.data.itemRewards[i].items), quests.data.itemRewards[i].amount);
                }
            }

            player.coins += quests.data.rewardCoin;
            player.gold += quests.data.rewardGold;
            player.experience += quests.data.rewardExperience;
            quests.completed = true;

            for (int q = 0; q < player.quests.Count; q++)
            {
                if (player.quests[q].name == quests.name)
                {
                    Quest pQuest = player.quests[q];
                    pQuest.completed = true;
                    player.quests[q] = pQuest;
                }

            }
        }
    }
}

public partial class PlayerItemPoint
{
    [SyncVar]
    public int point;
    public int maxPoint;
}

public partial class PlayerDance
{
    public Player player;
    [SyncVar(hook = nameof(ManageDance))]
    public int danceIndex;
    public int prevDanceIndex;

    public SyncList<string> networkDance = new SyncList<string>();

    private PlayerPlaceholderWeapon playerPlaceholderWeapon;

    public override void OnStartClient()
    {
        networkDance.Callback += OnDanceListUpdated;
    }

    void OnDanceListUpdated(SyncList<string>.Operation op, int index, string oldItem, string newItem)
    {
        switch (op)
        {
            case SyncList<string>.Operation.OP_ADD:
                // index is where it was added into the list
                // newItem is the new item
                if (player.isLocalPlayer)
                {
                    UIEmoji.singleton.ManageOpenPanelDance();
                }
                break;
            case SyncList<string>.Operation.OP_INSERT:
                // index is where it was inserted into the list
                // newItem is the new item
                if (player.isLocalPlayer)
                {
                    UIEmoji.singleton.ManageOpenPanelDance();
                }
                break;
            case SyncList<string>.Operation.OP_REMOVEAT:
                // index is where it was removed from the list
                // oldItem is the item that was removed
                break;
            case SyncList<string>.Operation.OP_SET:
                // index is of the item that was changed
                // oldItem is the previous value for the item at the index
                // newItem is the new value for the item at the index
                break;
            case SyncList<string>.Operation.OP_CLEAR:
                // list got cleared
                break;
        }
    }


    public void ManageDance(int oldInt, int newInt)
    {
        if (newInt > -1 && player.animators[0].runtimeAnimatorController != GeneralManager.singleton.listCompleteOfDance[newInt])
        {
            if (!playerPlaceholderWeapon) playerPlaceholderWeapon = Player.localPlayer.playerMove.bodyPlayer.GetComponent<PlayerPlaceholderWeapon>();

            if (player.playerItemEquipment.weapon) player.playerItemEquipment.weapon.SetActive(false);

            if (player.prevAnimator == null) player.prevAnimator = player.animators[0].runtimeAnimatorController;
            for (int i = 0; i < player.animators.Count; i++)
            {
                if (player.animators[i].runtimeAnimatorController != GeneralManager.singleton.listCompleteOfDance[newInt].animator)
                    player.animators[i].runtimeAnimatorController = GeneralManager.singleton.listCompleteOfDance[newInt].animator;
            }
        }
        else if (newInt == -1)
        {
            if (player.playerItemEquipment.weapon) player.playerItemEquipment.weapon.SetActive(true);

            if (player.playerItemEquipment.firstWeapon.amount > 0)
            {
                for (int i = 0; i < player.animators.Count; i++)
                {
                    player.animators[i].runtimeAnimatorController = ((EquipmentItem)player.playerItemEquipment.firstWeapon.item.data).animatorToSet;
                }
            }
            else
            {
                for (int i = 0; i < player.animators.Count; i++)
                {
                    player.animators[i].runtimeAnimatorController = GeneralManager.singleton.defaultAnimatorController;
                }

            }
        }
    }

    public void Update()
    {
        if (isServer)
        {
            if (player.state != "IDLE")
            {
                danceIndex = -1;
            }
        }
    }

    public void ResetAnimation()
    {
        danceIndex = -1;
    }

    [Command]
    public void CmdAddDance(string danceName, int currencyType)
    {
        ScriptableDance dance = null;
        for (int i = 0; i < GeneralManager.singleton.listCompleteOfDance.Count; i++)
        {
            if (GeneralManager.singleton.listCompleteOfDance[i].name == danceName)
            {
                dance = GeneralManager.singleton.listCompleteOfDance[i];
            }
        }
        if (currencyType == 0)
        {
            if (player.coins >= dance.coinToBuy)
            {
                player.playerDance.networkDance.Add(danceName);
                player.coins -= dance.coinToBuy;
            }
        }
        if (currencyType == 1)
        {
            if (player.gold >= dance.goldToBuy)
            {
                player.playerDance.networkDance.Add(danceName);
                player.gold -= dance.goldToBuy;
            }
        }
    }

    [Command]
    public void CmdSpawnDance(string danceName, string playerName, int index)
    {
        if (GeneralManager.singleton.FindNetworkDance(danceName, playerName) >= 0 && player.playerCar.car == null)
        {
            player.playerDance.danceIndex = index;
        }
    }
}

public partial class PlayerInjury : NetworkBehaviour
{
    public Player player;

    [SyncVar(hook = nameof(SetInjuredState))]
    public bool injured;

    public void Start()
    {
        if (player.isServer)
        {
            injured = player.HealthPercent() <= GeneralManager.singleton.activeMarriageBonusPerc;
        }
    }

    public void SetInjuredState(bool oldInjured, bool newInjured)
    {
        foreach (Animator anim in player.animators)
        {
            anim.SetBool("INJURED", newInjured);
        }

    }
}
