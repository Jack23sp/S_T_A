// Saves Character Data in a SQLite database. We use SQLite for several reasons
//
// - SQLite is file based and works without having to setup a database server
//   - We can 'remove all ...' or 'modify all ...' easily via SQL queries
//   - A lot of people requested a SQL database and weren't comfortable with XML
//   - We can allow all kinds of character names, even chinese ones without
//     breaking the file system.
// - We will need MYSQL or similar when using multiple server instances later
//   and upgrading is trivial
// - XML is easier, but:
//   - we can't easily read 'just the class of a character' etc., but we need it
//     for character selection etc. often
//   - if each account is a folder that contains players, then we can't save
//     additional account info like password, banned, etc. unless we use an
//     additional account.xml file, which over-complicates everything
//   - there will always be forbidden file names like 'COM', which will cause
//     problems when people try to create accounts or characters with that name
//
// About item mall coins:
//   The payment provider's callback should add new orders to theemoji
//   character_orders table. The server will then process them while the player
//   is ingame. Don't try to modify 'coins' in the character table directly.
//
// Tools to open sqlite database files:
//   Windows/OSX program: http://sqlitebrowser.org/
//   Firefox extension: https://addons.mozilla.org/de/firefox/addon/sqlite-manager/
//   Webhost: Adminer/PhpLiteAdmin
//
// About performance:
// - It's recommended to only keep the SQLite connection open while it's used.
//   MMO Servers use it all the time, so we keep it open all the time. This also
//   allows us to use transactions easily, and it will make the transition to
//   MYSQL easier.
// - Transactions are definitely necessary:
//   saving 100 players without transactions takes 3.6s
//   saving 100 players with transactions takes    0.38s
// - Using tr = conn.BeginTransaction() + tr.Commit() and passing it through all
//   the functions is ultra complicated. We use a BEGIN + END queries instead.
//
// Some benchmarks:
//   saving 100 players unoptimized: 4s
//   saving 100 players always open connection + transactions: 3.6s
//   saving 100 players always open connection + transactions + WAL: 3.6s
//   saving 100 players in 1 'using tr = ...' transaction: 380ms
//   saving 100 players in 1 BEGIN/END style transactions: 380ms
//   saving 100 players with XML: 369ms
//   saving 1000 players with mono-sqlite @ 2019-10-03: 843ms
//   saving 1000 players with sqlite-net  @ 2019-10-03:  90ms (!)
//
// Build notes:
// - requires Player settings to be set to '.NET' instead of '.NET Subset',
//   otherwise System.Data.dll causes ArgumentException.
// - requires sqlite3.dll x86 and x64 version for standalone (windows/mac/linux)
//   => found on sqlite.org website
// - requires libsqlite3.so x86 and armeabi-v7a for android
//   => compiled from sqlite.org amalgamation source with android ndk r9b linux
using UnityEngine;
using Mirror;
using System;
using System.IO;
using System.Collections.Generic;
using SQLite; // from https://github.com/praeclarum/sqlite-net
using UnityEngine.AI;
using CustomType;

public partial class Database : MonoBehaviour
{
    // singleton for easier access
    public static Database singleton;

    // file name
    public string databaseFile = "Database.sqlite";

    // connection
    public SQLiteConnection connection;

    // database layout via .NET classes:
    // https://github.com/praeclarum/sqlite-net/wiki/GettingStarted
    class accounts
    {
        [PrimaryKey] // important for performance: O(log n) instead of O(n)
        public string name { get; set; }
        public string password { get; set; }
        // created & lastlogin for statistics like CCU/MAU/registrations/...
        public DateTime created { get; set; }
        public DateTime lastlogin { get; set; }
        public bool banned { get; set; }
    }
    class characters
    {
        [PrimaryKey] // important for performance: O(log n) instead of O(n)
        [Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string name { get; set; }
        [Indexed] // add index on account to avoid full scans when loading characters
        public string account { get; set; }
        public string classname { get; set; } // 'class' isn't available in C#
        public float x { get; set; }
        public float y { get; set; }
        public int level { get; set; }
        public int health { get; set; }
        public int mana { get; set; }
        public int strength { get; set; }
        public int intelligence { get; set; }
        public long experience { get; set; } // TODO does long work?
        public long skillExperience { get; set; } // TODO does long work?
        public long gold { get; set; } // TODO does long work?
        public long coins { get; set; } // TODO does long work?
        // online status can be checked from external programs with either just
        // just 'online', or 'online && (DateTime.UtcNow - lastsaved) <= 1min)
        // which is robust to server crashes too.
        public bool online { get; set; }
        public DateTime lastsaved { get; set; }
        public bool deleted { get; set; }
    }
    class character_inventory
    {
        public string character { get; set; }
        public int slot { get; set; }
        public string name { get; set; }
        public int amount { get; set; }
        public int summonedHealth { get; set; }
        public int summonedLevel { get; set; }
        public long summonedExperience { get; set; } // TODO does long work?
        // PRIMARY KEY (character, slot) is created manually.
    }
    class character_equipment : character_inventory // same layout
    {
        // PRIMARY KEY (character, slot) is created manually.
    }
    class character_skills
    {
        public string character { get; set; }
        public string name { get; set; }
        public int level { get; set; }
        public float castTimeEnd { get; set; }
        public float cooldownEnd { get; set; }
        // PRIMARY KEY (character, name) is created manually.
    }
    class character_buffs
    {
        public string character { get; set; }
        public string name { get; set; }
        public int level { get; set; }
        public float buffTimeEnd { get; set; }
        // PRIMARY KEY (character, name) is created manually.
    }
    class character_quests
    {
        public string character { get; set; }
        public string name { get; set; }
        public int progress { get; set; }
        public bool completed { get; set; }
        // PRIMARY KEY (character, name) is created manually.
    }
    class character_orders
    {
        // INTEGER PRIMARY KEY is auto incremented by sqlite if the insert call
        // passes NULL for it.
        [PrimaryKey] // important for performance: O(log n) instead of O(n)
        public int orderid { get; set; }
        public string character { get; set; }
        public long coins { get; set; }
        public bool processed { get; set; }
    }
    class character_guild
    {
        // guild members are saved in a separate table because instead of in a
        // characters.guild field because:
        // * guilds need to be resaved independently, not just in CharacterSave
        // * kicked members' guilds are cleared automatically because we drop
        //   and then insert all members each time. otherwise we'd have to
        //   update the kicked member's guild field manually each time
        // * it's easier to remove / modify the guild feature if it's not hard-
        //   coded into the characters table
        [PrimaryKey] // important for performance: O(log n) instead of O(n)
        public string character { get; set; }
        // add index on guild to avoid full scans when loading guild members
        [Indexed]
        public string guild { get; set; }
        public int rank { get; set; }
    }
    class guild_info
    {
        // guild master is not in guild_info in case we need more than one later
        [PrimaryKey] // important for performance: O(log n) instead of O(n)
        public string name { get; set; }
        public string notice { get; set; }
    }

    class guildAlly
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string guildName { get; set; }
        public string firstAlly { get; set; }
        public string secondAlly { get; set; }
        public string thirdAlly { get; set; }
        public string forthAlly { get; set; }
        public string fifthAlly { get; set; }
    }

    class friends
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public string friendName { get; set; }
    }

    class friendsRequest
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public string friendName { get; set; }
    }
    class issue
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public int index { get; set; }
        public string characterName { get; set; }
        public float positionX { get; set; }
        public float positionY { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public string operatorID { get; set; }
        public string closed { get; set; }
    }

    class ability
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public string abilityName { get; set; }
        public int level { get; set; }
        public int maxLevel { get; set; }
        public int baseValue { get; set; }
    }

    class boost
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public string boostName { get; set; }
        public string velocityTimer { get; set; }
        public string velocityTimerServer { get; set; }
        public float velocityPerc { get; set; }
        public string accuracyTimer { get; set; }
        public string accuracyTimerServer { get; set; }
        public float accuracyPerc { get; set; }
        public string missTimer { get; set; }
        public string missTimerServer { get; set; }
        public float missPerc { get; set; }
        public string hiddenIslandTimer { get; set; }
        public string hiddenIslandTimerServer { get; set; }
        public string doubleEXP { get; set; }
        public string doubleEXPServer { get; set; }
        public string doubleGold { get; set; }
        public string doubleGoldServer { get; set; }
        public string doubleLeaderPoints { get; set; }
        public string doubleLeaderPointsServer { get; set; }
        public string doubleDamageToMonster { get; set; }
        public string doubleDamageToMonsterServer { get; set; }
        public string doubleDamageToPlayer { get; set; }
        public string doubleDamageToPlayerServer { get; set; }
    }

    class blood
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public int currentBlood { get; set; }
    }
    class emoji
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public string emojiName { get; set; }
    }

    class hungry
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public int currentHungry { get; set; }
    }

    class leaderPoint
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public int monsterKill { get; set; }
        public int bossKill { get; set; }
        public int plantPoint { get; set; }
        public int rockPoint { get; set; }
        public int treePoint { get; set; }
        public int upgradeItemPoint { get; set; }
        public int craftItemPoint { get; set; }
        public int buildinPoint { get; set; }
        public int buyBoostPoint { get; set; }
        public int playerKill { get; set; }

    }

    class partner
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public string partnerName { get; set; }
    }

    class options
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public int blockMarriage { get; set; }
        public int blockParty { get; set; }
        public int blockGroup { get; set; }
        public int blockAlly { get; set; }
        public int blockTrade { get; set; }
        public int blockFriend { get; set; }
        public int blockFootstep { get; set; }
        public int blockSound { get; set; }
        public int blockButtonSound { get; set; }
    }

    class poisoning
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public int currentPoisoning { get; set; }
    }

    class radio
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public int isActive { get; set; }
    }
    class torch
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public int isActive { get; set; }
    }
    class spawnpoint
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public string spawnpointName { get; set; }
        public float posX { get; set; }
        public float posY { get; set; }
        public int prefered { get; set; }
    }

    class thirsty
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public int currentThirsty { get; set; }
    }

    class wood
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public int inWood { get; set; }
    }

    class additionalInventory
    {
        public int index { get; set; }
        public string characterName { get; set; }
        public int slot { get; set; }
        public string name { get; set; }
        public int currentArmor { get; set; }
        public int currentUnsanity { get; set; }
        public int alreadyShooted { get; set; }
        public int totalAlreadyShooted { get; set; }
        public int radioCurrentBattery { get; set; }
        public int torchCurrentBattery { get; set; }
        public int durability { get; set; }
        public int weight { get; set; }
        public int accuracyLevel { get; set; }
        public int missLevel { get; set; }
        public int armorLevel { get; set; }
        public int chargeLevel { get; set; }
        public int batteryLevel { get; set; }
        public int weightLevel { get; set; }
        public int durabilityLevel { get; set; }
        public int unsanityLevel { get; set; }
        public int bagLevel { get; set; }
        public int gasolineContainer { get; set; }
        public int honeyContainer { get; set; }
        public int waterContainer { get; set; }
        public int cookCountdown { get; set; }
        public float wet { get; set; }

    }

    class characterCreation
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public int sex { get; set; }
        public int hairType { get; set; }
        public int beard { get; set; }
        public string hairColor { get; set; }
        public string underwearColor { get; set; }
        public string eyesColor { get; set; }
        public string skinColor { get; set; }
        public float fat { get; set; }
        public float thin { get; set; }
        public float muscle { get; set; }
        public float height { get; set; }
        public float breast { get; set; }
        public int bag { get; set; }
    }

    class containerBee
    {
        public int myIndex { get; set; }
        public string sceneName { get; set; }
        public string actualName { get; set; }
        public string guild { get; set; }
        public string owner { get; set; }
        public float positionX { get; set; }
        public float positionY { get; set; }
        public int health { get; set; }
        public int level { get; set; }
        public int totalBeeOne { get; set; }
        public int totalHoneyOne { get; set; }
        public int totalBeeTwo { get; set; }
        public int totalHoneyTwo { get; set; }
        public int totalBeeThree { get; set; }
        public int totalHoneyThree { get; set; }
        public int totalBeeFour { get; set; }
        public int totalHoneyFour { get; set; }
        public int totalBeeFive { get; set; }
        public int totalHoneyFive { get; set; }
        public int isPremium { get; set; }
    }

    class buildingCraft
    {
        public string sceneName { get; set; }
        public string craftBuildingType { get; set; }
        public int myIndex { get; set; }
        public int health { get; set; }
        public string guild { get; set; }
        public string owner { get; set; }
        public float positionX { get; set; }
        public float positionY { get; set; }
        public int level { get; set; }
        public int isPremium { get; set; }
    }


    class buildingCraftItem
    {
        public int myIndex { get; set; }
        public string itemName { get; set; }
        public int amount { get; set; }
        public int remainingTime { get; set; }
        public int totalTime { get; set; }
        public string timeBegin { get; set; }
        public string timeEnd { get; set; }
        public string timeEndServer { get; set; }
        public string owner { get; set; }
        public string guildName { get; set; }
        public int isPremium { get; set; }
    }

    class campfire
    {
        public string sceneName { get; set; }
        public int myIndex { get; set; }
        public int health { get; set; }
        public string guild { get; set; }
        public string owner { get; set; }
        public float positionX { get; set; }
        public float positionY { get; set; }
        public int level { get; set; }
        public int currentWood { get; set; }
        public int active { get; set; }
        public int isPremium { get; set; }
    }

    class campfireItems
    {
        public int myIndex { get; set; }
        public int amount { get; set; }
        public int slot { get; set; }
        public int summonHealth { get; set; }
        public int summonedLevel { get; set; }
        public long summonedExperience { get; set; }
        public string name { get; set; }
        public int currentArmor { get; set; }
        public int currentUnsanity { get; set; }
        public int alreadyShooted { get; set; }
        public int totalAlreadyShooted { get; set; }
        public int radioCurrentBattery { get; set; }
        public int torchCurrentBattery { get; set; }
        public int durability { get; set; }
        public int weight { get; set; }
        public int accuracyLevel { get; set; }
        public int missLevel { get; set; }
        public int armorLevel { get; set; }
        public int chargeLevel { get; set; }
        public int batteryLevel { get; set; }
        public int weightLevel { get; set; }
        public int durabilityLevel { get; set; }
        public int unsanityLevel { get; set; }
        public int bagLevel { get; set; }
        public int gasolineContainer { get; set; }
        public int honeyContainer { get; set; }
        public int waterContainer { get; set; }
        public int cookCountdown { get; set; }
        public float wet { get; set; }
    }

    class dynamite
    {
        public string sceneName { get; set; }
        public int myIndex { get; set; }
        public string actualName { get; set; }
        public int health { get; set; }
        public string guild { get; set; }
        public string owner { get; set; }
        public float positionX { get; set; }
        public float positionY { get; set; }
        public int level { get; set; }
        public int isPremium { get; set; }
    }

    class warehouseGroup
    {
        public string sceneName { get; set; }
        public int myIndex { get; set; }
        public int health { get; set; }
        public string guild { get; set; }
        public string owner { get; set; }
        public float positionX { get; set; }
        public float positionY { get; set; }
        public int level { get; set; }
        public int totalSlot { get; set; }
        public int isPremium { get; set; }
    }
    class warehouseItemsGroup
    {
        public int myIndex { get; set; }
        public int containerNumber { get; set; }
        public int amount { get; set; }
        public int slot { get; set; }
        public int summonHealth { get; set; }
        public int summonedLevel { get; set; }
        public long summonedExperience { get; set; }
        public string name { get; set; }
        public int currentArmor { get; set; }
        public int currentUnsanity { get; set; }
        public int alreadyShooted { get; set; }
        public int totalAlreadyShooted { get; set; }
        public int radioCurrentBattery { get; set; }
        public int torchCurrentBattery { get; set; }
        public int durability { get; set; }
        public int weight { get; set; }
        public int accuracyLevel { get; set; }
        public int missLevel { get; set; }
        public int armorLevel { get; set; }
        public int chargeLevel { get; set; }
        public int batteryLevel { get; set; }
        public int weightLevel { get; set; }
        public int durabilityLevel { get; set; }
        public int unsanityLevel { get; set; }
        public int bagLevel { get; set; }
        public int gasolineContainer { get; set; }
        public int honeyContainer { get; set; }
        public int waterContainer { get; set; }
        public int cookCountdown { get; set; }
        public float wet { get; set; }
    }

    class warehousePersonal
    {
        public string sceneName { get; set; }
        public int myIndex { get; set; }
        public int health { get; set; }
        public string guild { get; set; }
        public string owner { get; set; }
        public float positionX { get; set; }
        public float positionY { get; set; }
        public int level { get; set; }
        public int totalSlot { get; set; }
        public int isPremium { get; set; }
    }

    class warehouseItemsPersonal
    {
        public int myIndex { get; set; }
        public int containerNumber { get; set; }
        public int amount { get; set; }
        public int slot { get; set; }
        public int summonHealth { get; set; }
        public int summonedLevel { get; set; }
        public long summonedExperience { get; set; }
        public string name { get; set; }
        public int currentArmor { get; set; }
        public int currentUnsanity { get; set; }
        public int alreadyShooted { get; set; }
        public int totalAlreadyShooted { get; set; }
        public int radioCurrentBattery { get; set; }
        public int torchCurrentBattery { get; set; }
        public int durability { get; set; }
        public int weight { get; set; }
        public int accuracyLevel { get; set; }
        public int missLevel { get; set; }
        public int armorLevel { get; set; }
        public int chargeLevel { get; set; }
        public int batteryLevel { get; set; }
        public int weightLevel { get; set; }
        public int durabilityLevel { get; set; }
        public int unsanityLevel { get; set; }
        public int bagLevel { get; set; }
        public int gasolineContainer { get; set; }
        public int honeyContainer { get; set; }
        public int waterContainer { get; set; }
        public int cookCountdown { get; set; }
        public float wet { get; set; }
    }

    class mine
    {
        public string sceneName { get; set; }
        public int myIndex { get; set; }
        public int health { get; set; }
        public string guild { get; set; }
        public string owner { get; set; }
        public float positionX { get; set; }
        public float positionY { get; set; }
        public int level { get; set; }
        public int isPremium { get; set; }
    }

    class woodWall
    {
        public string sceneName { get; set; }
        public int myIndex { get; set; }
        public int health { get; set; }
        public string guild { get; set; }
        public string owner { get; set; }
        public float positionX { get; set; }
        public float positionY { get; set; }
        public int level { get; set; }
        public int isPremium { get; set; }

        public int side { get; set; }
    }

    class barbwire
    {
        public string sceneName { get; set; }
        public int myIndex { get; set; }
        public int health { get; set; }
        public string guild { get; set; }
        public string owner { get; set; }
        public float positionX { get; set; }
        public float positionY { get; set; }
        public int level { get; set; }
        public int isPremium { get; set; }

        public int side { get; set; }
    }


    class totem
    {
        public string sceneName { get; set; }
        public int myIndex { get; set; }
        public int health { get; set; }
        public string guild { get; set; }
        public string owner { get; set; }
        public float positionX { get; set; }
        public float positionY { get; set; }
        public int level { get; set; }
        public string message { get; set; }
        public int isPremium { get; set; }
    }

    class upgradeRepair
    {
        public string sceneName { get; set; }
        public int myIndex { get; set; }
        public int health { get; set; }
        public string guild { get; set; }
        public string owner { get; set; }
        public float positionX { get; set; }
        public float positionY { get; set; }
        public int level { get; set; }
        public int isPremium { get; set; }
    }

    class upgradeItems
    {
        public int myIndex { get; set; }
        public int containerNumber { get; set; }
        public int amount { get; set; }
        public int slot { get; set; }
        public int summonHealth { get; set; }
        public int summonedLevel { get; set; }
        public long summonedExperience { get; set; }
        public string name { get; set; }
        public int currentArmor { get; set; }
        public int currentUnsanity { get; set; }
        public int alreadyShooted { get; set; }
        public int totalAlreadyShooted { get; set; }
        public int radioCurrentBattery { get; set; }
        public int torchCurrentBattery { get; set; }
        public int durability { get; set; }
        public int weight { get; set; }
        public int accuracyLevel { get; set; }
        public int missLevel { get; set; }
        public int armorLevel { get; set; }
        public int chargeLevel { get; set; }
        public int batteryLevel { get; set; }
        public int weightLevel { get; set; }
        public int durabilityLevel { get; set; }
        public int unsanityLevel { get; set; }
        public int bagLevel { get; set; }
        public int gasolineContainer { get; set; }
        public int honeyContainer { get; set; }
        public int waterContainer { get; set; }
        public int cookCountdown { get; set; }
        public float wet { get; set; }
        public string playerName { get; set; }
        public string type { get; set; }
        public int remainingTime { get; set; }
        public int totalTime { get; set; }
        public string timeBegin { get; set; }
        public string timeEnd { get; set; }
    }

    class repairItems
    {
        public int myIndex { get; set; }
        public int containerNumber { get; set; }
        public int amount { get; set; }
        public int slot { get; set; }
        public int summonHealth { get; set; }
        public int summonedLevel { get; set; }
        public long summonedExperience { get; set; }
        public string name { get; set; }
        public int currentArmor { get; set; }
        public int currentUnsanity { get; set; }
        public int alreadyShooted { get; set; }
        public int totalAlreadyShooted { get; set; }
        public int radioCurrentBattery { get; set; }
        public int torchCurrentBattery { get; set; }
        public int durability { get; set; }
        public int weight { get; set; }
        public int accuracyLevel { get; set; }
        public int missLevel { get; set; }
        public int armorLevel { get; set; }
        public int chargeLevel { get; set; }
        public int batteryLevel { get; set; }
        public int weightLevel { get; set; }
        public int durabilityLevel { get; set; }
        public int unsanityLevel { get; set; }
        public int bagLevel { get; set; }
        public int gasolineContainer { get; set; }
        public int honeyContainer { get; set; }
        public int waterContainer { get; set; }
        public int cookCountdown { get; set; }
        public float wet { get; set; }
        public string playerName { get; set; }
        public string type { get; set; }
        public int remainingTime { get; set; }
        public int totalTime { get; set; }
        public string timeBegin { get; set; }
        public string timeEnd { get; set; }
    }

    class buildingWaterWell
    {
        public string sceneName { get; set; }
        public int myIndex { get; set; }
        public int health { get; set; }
        public string guild { get; set; }
        public string owner { get; set; }
        public float positionX { get; set; }
        public float positionY { get; set; }
        public int level { get; set; }
        public int currentWater { get; set; }
        public int isPremium { get; set; }
    }

    class additionalEquipment : additionalInventory
    {

    }


    class petTrainer
    {
        public string sceneName { get; set; }
        public int myIndex { get; set; }
        public int health { get; set; }
        public string guild { get; set; }
        public string owner { get; set; }
        public float positionX { get; set; }
        public float positionY { get; set; }
        public int level { get; set; }
        public int isPremium { get; set; }
    }

    class petTrainerInProgress
    {
        public int myIndex { get; set; }
        public string petName { get; set; }
        public int level { get; set; }
        public int health { get; set; }
        public long experience { get; set; }
        public string owner { get; set; }
        public int remainingTime { get; set; }
        public string timeBegin { get; set; }
        public string timeEnd { get; set; }
    }


    class character_belt
    {
        public int myIndex { get; set; }
        public string character { get; set; }
        public int slot { get; set; }
        public string name { get; set; }
        public int amount { get; set; }
        public int summonedHealth { get; set; }
        public int summonedLevel { get; set; }
        public long summonedExperience { get; set; } // TODO does long work?
        public int currentArmor { get; set; }
        public int currentUnsanity { get; set; }
        public int alreadyShooted { get; set; }
        public int totalAlreadyShooted { get; set; }
        public int radioCurrentBattery { get; set; }
        public int torchCurrentBattery { get; set; }
        public int durability { get; set; }
        public int weight { get; set; }
        public int accuracyLevel { get; set; }
        public int missLevel { get; set; }
        public int armorLevel { get; set; }
        public int chargeLevel { get; set; }
        public int batteryLevel { get; set; }
        public int weightLevel { get; set; }
        public int durabilityLevel { get; set; }
        public int unsanityLevel { get; set; }
        public int bagLevel { get; set; }
        public int gasolineContainer { get; set; }
        public int honeyContainer { get; set; }
        public int waterContainer { get; set; }
        public int cookCountdown { get; set; }
        public float wet { get; set; }
        public string playerName { get; set; }
        public string type { get; set; }
        public int remainingTime { get; set; }
        public int totalTime { get; set; }
        // PRIMARY KEY (character, slot) is created manually.
    }

    class cultivableField
    {
        public string sceneName { get; set; }
        public int myIndex { get; set; }
        public int health { get; set; }
        public string guild { get; set; }
        public string owner { get; set; }
        public float positionX { get; set; }
        public float positionY { get; set; }
        public int level { get; set; }
        public int isPremium { get; set; }

    }

    class cuiltivableFieldItem
    {
        public int myIndex { get; set; }
        public string plantName { get; set; }
        public float dimension { get; set; }
        public string season { get; set; }
        public bool alreadyGrown { get; set; }
        public float grownQuantityX { get; set; }
        public float grownQuantityY { get; set; }
        public int seeds { get; set; }
        public int plantAmount { get; set; }
        public int timebeforeTakeMultipleSeeds { get; set; }
        public bool releaseSeeds { get; set; }
    }

    class streetLamps
    {
        public string sceneName { get; set; }
        public int myIndex { get; set; }
        public int health { get; set; }
        public string guild { get; set; }
        public string owner { get; set; }
        public float positionX { get; set; }
        public float positionY { get; set; }
        public int level { get; set; }
        public int totalSlot { get; set; }
        public int isPremium { get; set; }
    }

    class flag
    {
        public string sceneName { get; set; }
        public int myIndex { get; set; }
        public int health { get; set; }
        public string guild { get; set; }
        public string owner { get; set; }
        public float positionX { get; set; }
        public float positionY { get; set; }
        public int level { get; set; }
        public int isPremium { get; set; }
        public string NationFlag { get; set; }
    }

    class breding
    {
        public string sceneName { get; set; }
        public int myIndex { get; set; }
        public int health { get; set; }
        public string guild { get; set; }
        public string owner { get; set; }
        public float positionX { get; set; }
        public float positionY { get; set; }
        public int level { get; set; }
        public int isPremium { get; set; }

    }

    class cow
    {
        public string sceneName { get; set; }
        public int myIndex { get; set; }
        public string type { get; set; }
        public int age { get; set; }
    }

    class horse
    {
        public string sceneName { get; set; }
        public int myIndex { get; set; }
        public string type { get; set; }
        public int age { get; set; }
    }

    class goat
    {
        public string sceneName { get; set; }
        public int myIndex { get; set; }
        public string type { get; set; }
        public int age { get; set; }
    }

    class sheep
    {
        public string sceneName { get; set; }
        public int myIndex { get; set; }
        public string type { get; set; }
        public int age { get; set; }
    }

    class chicken
    {
        public string sceneName { get; set; }
        public int myIndex { get; set; }
        public string type { get; set; }
        public int age { get; set; }
    }

    class quest_additional
    {
        public string charactername { get; set; }
        public string name { get; set; }
        public bool completed { get; set; }
        public bool checkEnterPremium { get; set; }
        public bool checkCreateBuilding { get; set; }
        public bool checkEquipWeapon { get; set; }
        public bool checkEquipBag { get; set; }
        public bool checkCreateGuild { get; set; }
        public bool checkCreateParty { get; set; }
        public bool checkMakeGroupAlly { get; set; }
        public bool checkDrink { get; set; }
        public bool checkEat { get; set; }
        public bool checkRun { get; set; }
        public bool checkSneak { get; set; }
        public bool checkMakeMarriage { get; set; }
        public bool checkSendAMessage { get; set; }
        public bool checkBuyEmoji { get; set; }
        public bool checkOpenShop { get; set; }
        public int checkAmountPlayerToKill { get; set; }
        public int checkWoodToGather { get; set; }
        public int checkRockToGather { get; set; }
        public bool checkUseTeleport { get; set; }
        public bool checkUseInstantResurrect { get; set; }
        public bool checkMakeATrade { get; set; }
        public bool checkFriendCount { get; set; }
        public int checkCreateASpawnpoint { get; set; }
        public int checkHairZombie { get; set; }
        public int checkPirateZombie { get; set; }
        public int checkHatZombie { get; set; }
        public int checkCountryZombie { get; set; }
    }

    class floor
    {
        public string sceneName { get; set; }
        public int index { get; set; }
        public int isMain { get; set; }
        public string owner { get; set; }
        public string guildName { get; set; }
        public int upPart { get; set; }
        public int downPart { get; set; }
        public int leftPart { get; set; }
        public int rightPart { get; set; }
        public float posX { get; set; }
        public float posY { get; set; }
        public float posZ { get; set; }
    }

    class objects
    {
        public string sceneName { get; set; }
        public string buildingName { get; set; }
        public int index { get; set; }
        public int objectIndex { get; set; }
        public float posX { get; set; }
        public float posY { get; set; }
        public float posZ { get; set; }
    }

    class modularInventory
    {
            public int myIndex { get; set; }
            public string character { get; set; }
            public int slot { get; set; }
            public string name { get; set; }
            public int amount { get; set; }
            public int summonedHealth { get; set; }
            public int summonedLevel { get; set; }
            public long summonedExperience { get; set; } // TODO does long work?
            public int currentArmor { get; set; }
            public int currentUnsanity { get; set; }
            public int alreadyShooted { get; set; }
            public int totalAlreadyShooted { get; set; }
            public int radioCurrentBattery { get; set; }
            public int torchCurrentBattery { get; set; }
            public int durability { get; set; }
            public int weight { get; set; }
            public int accuracyLevel { get; set; }
            public int missLevel { get; set; }
            public int armorLevel { get; set; }
            public int chargeLevel { get; set; }
            public int batteryLevel { get; set; }
            public int weightLevel { get; set; }
            public int durabilityLevel { get; set; }
            public int unsanityLevel { get; set; }
            public int bagLevel { get; set; }
            public int gasolineContainer { get; set; }
            public int honeyContainer { get; set; }
            public int waterContainer { get; set; }
            public int cookCountdown { get; set; }
            public float wet { get; set; }
            public string playerName { get; set; }
            public string type { get; set; }
            public int remainingTime { get; set; }
            public int totalTime { get; set; }
            // PRIMARY KEY (character, slot) is created manually.
    }

    class dance
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public string danceName { get; set; }
    }

    public BuildingManager buildingManager;

    public string path;

    void Awake()
    {
        // initialize singleton
        if (singleton == null) singleton = this;
    }

    // connect /////////////////////////////////////////////////////////////////
    // only call this from the server, not from the client. otherwise the client
    // would create a database file / webgl would throw errors, etc.
    public void Connect()
    {
        // database path: Application.dataPath is always relative to the project,
        // but we don't want it inside the Assets folder in the Editor (git etc.),
        // instead we put it above that.
        // we also use Path.Combine for platform independent paths
        // and we need persistentDataPath on android
#if UNITY_EDITOR
        path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, databaseFile);
#elif UNITY_ANDROID
        string path = Path.Combine(Application.persistentDataPath, databaseFile);
#elif UNITY_IOS
        string path = Path.Combine(Application.persistentDataPath, databaseFile);
#else
        string path = Path.Combine(Application.dataPath, databaseFile);
#endif

        // open connection
        // note: automatically creates database file if not created yet
        connection = new SQLiteConnection(path);

        // create tables if they don't exist yet or were deleted
        connection.CreateTable<accounts>();
        connection.CreateTable<characters>();
        connection.CreateTable<character_inventory>();
        connection.CreateIndex(nameof(character_inventory), new[] { "character", "slot" });
        connection.CreateTable<character_equipment>();
        connection.CreateIndex(nameof(character_equipment), new[] { "character", "slot" });
        connection.CreateTable<character_skills>();
        connection.CreateIndex(nameof(character_skills), new[] { "character", "name" });
        connection.CreateTable<character_buffs>();
        connection.CreateIndex(nameof(character_buffs), new[] { "character", "name" });
        connection.CreateTable<character_quests>();
        connection.CreateIndex(nameof(character_quests), new[] { "character", "name" });
        connection.CreateTable<character_orders>();
        connection.CreateTable<character_guild>();
        connection.CreateTable<guild_info>();

        connection.CreateTable<guildAlly>();
        connection.CreateTable<friends>();
        connection.CreateTable<friendsRequest>();
        connection.CreateTable<issue>();
        connection.CreateTable<ability>();
        connection.CreateTable<boost>();
        connection.CreateTable<blood>();
        connection.CreateTable<emoji>();
        connection.CreateTable<hungry>();
        connection.CreateTable<leaderPoint>();
        connection.CreateTable<partner>();
        connection.CreateTable<options>();
        connection.CreateTable<poisoning>();
        connection.CreateTable<radio>();
        connection.CreateTable<torch>();
        connection.CreateTable<spawnpoint>();
        connection.CreateTable<thirsty>();
        connection.CreateTable<wood>();
        connection.CreateTable<additionalInventory>();
        connection.CreateTable<additionalEquipment>();
        connection.CreateTable<characterCreation>();
        connection.CreateTable<containerBee>();
        connection.CreateTable<containerBee>();
        connection.CreateTable<buildingCraft>();
        connection.CreateTable<buildingCraftItem>();
        connection.CreateTable<campfire>();
        connection.CreateTable<campfireItems>();
        connection.CreateTable<dynamite>();
        connection.CreateTable<warehouseGroup>();
        connection.CreateTable<warehouseItemsGroup>();
        connection.CreateTable<warehousePersonal>();
        connection.CreateTable<warehouseItemsPersonal>();
        connection.CreateTable<mine>();
        connection.CreateTable<woodWall>();
        connection.CreateTable<barbwire>();
        connection.CreateTable<totem>();
        connection.CreateTable<upgradeRepair>();
        connection.CreateTable<upgradeItems>();
        connection.CreateTable<repairItems>();
        connection.CreateTable<buildingWaterWell>();
        connection.CreateTable<petTrainer>();
        connection.CreateTable<petTrainerInProgress>();
        connection.CreateTable<cultivableField>();
        connection.CreateTable<cuiltivableFieldItem>();
        connection.CreateTable<streetLamps>();
        connection.CreateTable<floor>();
        connection.CreateTable<objects>();
        connection.CreateTable<modularInventory>();
        connection.CreateTable<flag>();
        connection.CreateTable<character_belt>();
        connection.CreateTable<breding>();
        connection.CreateTable<cow>();
        connection.CreateTable<horse>();
        connection.CreateTable<goat>();
        connection.CreateTable<sheep>();
        connection.CreateTable<chicken>();
        connection.CreateTable<quest_additional>();
        connection.CreateTable<dance>();

        // addon system hooks
        Utils.InvokeMany(typeof(Database), this, "Initialize_"); // TODO remove later. let's keep the old hook for a while to not break every single addon!
        Utils.InvokeMany(typeof(Database), this, "Connect_"); // the new hook!

        //Debug.Log("connected to database");
    }

    // close connection when Unity closes to prevent locking
    void OnApplicationQuit()
    {
        connection?.Close();
    }

    // account data ////////////////////////////////////////////////////////////
    // try to log in with an account.
    // -> not called 'CheckAccount' or 'IsValidAccount' because it both checks
    //    if the account is valid AND sets the lastlogin field
    public bool TryLogin(string account, string password)
    {
        // this function can be used to verify account credentials in a database
        // or a content management system.
        //
        // for example, we could setup a content management system with a forum,
        // news, shop etc. and then use a simple HTTP-GET to check the account
        // info, for example:
        //
        //   var request = new WWW("example.com/verify.php?id="+id+"&amp;pw="+pw);
        //   while (!request.isDone)
        //       print("loading...");
        //   return request.error == null && request.text == "ok";
        //
        // where verify.php is a script like this one:
        //   <?php
        //   // id and pw set with HTTP-GET?
        //   if (isset($_GET['id']) && isset($_GET['pw'])) {
        //       // validate id and pw by using the CMS, for example in Drupal:
        //       if (user_authenticate($_GET['id'], $_GET['pw']))
        //           echo "ok";
        //       else
        //           echo "invalid id or pw";
        //   }
        //   ?>
        //
        // or we could check in a MYSQL database:
        //   var dbConn = new MySql.Data.MySqlClient.MySqlConnection("Persist Security Info=False;server=localhost;database=notas;uid=root;password=" + dbpwd);
        //   var cmd = dbConn.CreateCommand();
        //   cmd.CommandText = "SELECT id FROM accounts WHERE id='" + account + "' AND pw='" + password + "'";
        //   dbConn.Open();
        //   var reader = cmd.ExecuteReader();
        //   if (reader.Read())
        //       return reader.ToString() == account;
        //   return false;
        //
        // as usual, we will use the simplest solution possible:
        // create account if not exists, compare password otherwise.
        // no CMS communication necessary and good enough for an Indie MMORPG.

        // not empty?
        if (!string.IsNullOrWhiteSpace(account) && !string.IsNullOrWhiteSpace(password))
        {
            // demo feature: create account if it doesn't exist yet.
            // note: sqlite-net has no InsertOrIgnore so we do it in two steps
            if (connection.FindWithQuery<accounts>("SELECT * FROM accounts WHERE name=?", account) == null)
                connection.Insert(new accounts { name = account, password = password, created = DateTime.UtcNow, lastlogin = DateTime.Now, banned = false });

            // check account name, password, banned status
            if (connection.FindWithQuery<accounts>("SELECT * FROM accounts WHERE name=? AND password=? and banned=0", account, password) != null)
            {
                // save last login time and return true
                connection.Execute("UPDATE accounts SET lastlogin=? WHERE name=?", DateTime.UtcNow, account);
                return true;
            }
        }
        return false;
    }

    // character data //////////////////////////////////////////////////////////
    public bool CharacterExists(string characterName)
    {
        // checks deleted ones too so we don't end up with duplicates if we un-
        // delete one
        return connection.FindWithQuery<characters>("SELECT * FROM characters WHERE name=?", characterName) != null;
    }

    public void CharacterDelete(string characterName)
    {
        // soft delete the character so it can always be restored later
        connection.Execute("UPDATE characters SET deleted=1 WHERE name=?", characterName);
    }

    // returns the list of character names for that account
    // => all the other values can be read with CharacterLoad!
    public List<string> CharactersForAccount(string account)
    {
        List<string> result = new List<string>();
        foreach (characters character in connection.Query<characters>("SELECT * FROM characters WHERE account=? AND deleted=0", account))
            result.Add(character.name);
        return result;
    }

    void LoadInventory(Player player)
    {
        // fill all slots first
        for (int i = 0; i < player.inventorySize; ++i)
            player.inventory.Add(new ItemSlot());

        // then load valid items and put into their slots
        // (one big query is A LOT faster than querying each slot separately)
        foreach (character_inventory row in connection.Query<character_inventory>("SELECT * FROM character_inventory WHERE character=?", player.name))
        {
            if (row.slot < player.inventorySize)
            {
                if (ScriptableItem.dict.TryGetValue(row.name.GetStableHashCode(), out ScriptableItem itemData))
                {
                    Item item = new Item(itemData);
                    item.summonedHealth = row.summonedHealth;
                    item.summonedLevel = row.summonedLevel;
                    item.summonedExperience = row.summonedExperience;
                    player.inventory[row.slot] = new ItemSlot(item, row.amount);
                }
                else Debug.LogWarning("LoadInventory: skipped item " + row.name + " for " + player.name + " because it doesn't exist anymore. If it wasn't removed intentionally then make sure it's in the Resources folder.");
            }
            else Debug.LogWarning("LoadInventory: skipped slot " + row.slot + " for " + player.name + " because it's bigger than size " + player.inventorySize);
        }
    }

    void LoadEquipment(Player player)
    {
        // fill all slots first
        for (int i = 0; i < player.equipmentInfo.Length; ++i)
            player.equipment.Add(new ItemSlot());

        // then load valid equipment and put into their slots
        // (one big query is A LOT faster than querying each slot separately)
        foreach (character_equipment row in connection.Query<character_equipment>("SELECT * FROM character_equipment WHERE character=?", player.name))
        {
            if (row.slot < player.equipmentInfo.Length)
            {
                if (ScriptableItem.dict.TryGetValue(row.name.GetStableHashCode(), out ScriptableItem itemData))
                {
                    Item item = new Item(itemData);
                    item.summonedHealth = row.summonedHealth;
                    item.summonedLevel = row.summonedLevel;
                    item.summonedExperience = row.summonedExperience;
                    player.equipment[row.slot] = new ItemSlot(item, row.amount);
                }
                else Debug.LogWarning("LoadEquipment: skipped item " + row.name + " for " + player.name + " because it doesn't exist anymore. If it wasn't removed intentionally then make sure it's in the Resources folder.");
            }
            else Debug.LogWarning("LoadEquipment: skipped slot " + row.slot + " for " + player.name + " because it's bigger than size " + player.equipmentInfo.Length);
        }
    }

    void LoadSkills(Player player)
    {
        // load skills based on skill templates (the others don't matter)
        // -> this way any skill changes in a prefab will be applied
        //    to all existing players every time (unlike item templates
        //    which are only for newly created characters)

        // fill all slots first
        foreach (ScriptableSkill skillData in player.skillTemplates)
            player.skills.Add(new Skill(skillData));

        // then load learned skills and put into their slots
        // (one big query is A LOT faster than querying each slot separately)
        foreach (character_skills row in connection.Query<character_skills>("SELECT * FROM character_skills WHERE character=?", player.name))
        {
            int index = player.skills.FindIndex(skill => skill.name == row.name);
            if (index != -1)
            {
                Skill skill = player.skills[index];
                // make sure that 1 <= level <= maxlevel (in case we removed a skill
                // level etc)
                skill.level = Mathf.Clamp(row.level, 1, skill.maxLevel);
                // make sure that 1 <= level <= maxlevel (in case we removed a skill
                // level etc)
                // castTimeEnd and cooldownEnd are based on NetworkTime.time
                // which will be different when restarting a server, hence why
                // we saved them as just the remaining times. so let's convert
                // them back again.
                skill.castTimeEnd = row.castTimeEnd + NetworkTime.time;
                skill.cooldownEnd = row.cooldownEnd + NetworkTime.time;

                player.skills[index] = skill;
            }
        }
    }

    void LoadBuffs(Player player)
    {
        // load buffs
        // note: no check if we have learned the skill for that buff
        //       since buffs may come from other people too
        foreach (character_buffs row in connection.Query<character_buffs>("SELECT * FROM character_buffs WHERE character=?", player.name))
        {
            if (ScriptableSkill.dict.TryGetValue(row.name.GetStableHashCode(), out ScriptableSkill skillData))
            {
                // make sure that 1 <= level <= maxlevel (in case we removed a skill
                // level etc)
                int level = Mathf.Clamp(row.level, 1, skillData.maxLevel);
                Buff buff = new Buff((BuffSkill)skillData, level);
                // buffTimeEnd is based on NetworkTime.time, which will be
                // different when restarting a server, hence why we saved
                // them as just the remaining times. so let's convert them
                // back again.
                buff.buffTimeEnd = row.buffTimeEnd + NetworkTime.time;
                player.buffs.Add(buff);
            }
            else Debug.LogWarning("LoadBuffs: skipped buff " + row.name + " for " + player.name + " because it doesn't exist anymore. If it wasn't removed intentionally then make sure it's in the Resources folder.");
        }
    }

    public GameObject CharacterFriendLoad(string characterName, GameObject prefab)
    {
        characters row = connection.FindWithQuery<characters>("SELECT * FROM characters WHERE name=? AND deleted=0", characterName);
        if (row != null)
        {
            // instantiate based on the class name
            if (prefab != null)
            {
                GameObject go = Instantiate(prefab.gameObject);
                Player player = go.GetComponent<Player>();

                player.name = row.name;
                player.account = row.account;
                player.className = row.classname;
                Vector2 position = new Vector2(row.x, row.y);
                player.level = Mathf.Min(row.level, player.maxLevel); // limit to max level in case we changed it
                player.strength = row.strength;
                player.intelligence = row.intelligence;
                player.experience = row.experience;
                player.skillExperience = row.skillExperience;
                player.gold = row.gold;
                player.coins = row.coins;

                LoadEquipment(player);
                LoadGuildOnDemand(player);
                LoadCustomFriendStat(player);

                player.health = row.health;
                player.mana = row.mana;


                return go;
            }
            else Debug.LogError("no prefab found for class: " + row.classname);
        }
        return null;
    }

    // only load guild when their first player logs in
    // => using NetworkManager.Awake to load all guilds.Where would work,
    //    but we would require lots of memory and it might take a long time.
    // => hooking into player loading to load guilds is a really smart solution,
    //    because we don't ever have to load guilds that aren't needed
    void LoadGuildOnDemand(Player player)
    {
        string guildName = connection.ExecuteScalar<string>("SELECT guild FROM character_guild WHERE character=?", player.name);
        if (guildName != null)
        {
            // load guild on demand when the first player of that guild logs in
            // (= if it's not in GuildSystem.guilds yet)
            if (!GuildSystem.guilds.ContainsKey(guildName))
            {
                Guild guild = LoadGuild(guildName);
                GuildSystem.guilds[guild.name] = guild;
                player.guild = guild;
            }
            // assign from already loaded guild
            else player.guild = GuildSystem.guilds[guildName];
        }
    }

    public GameObject CharacterLoad(string characterName, List<Player> prefabs, bool isPreview)
    {
        characters row = connection.FindWithQuery<characters>("SELECT * FROM characters WHERE name=? AND deleted=0", characterName);
        if (row != null)
        {
            // instantiate based on the class name
            Player prefab = prefabs.Find(p => p.name == row.classname);
            if (prefab != null)
            {
                GameObject go = Instantiate(prefab.gameObject);
                Player player = go.GetComponent<Player>();

                player.name = row.name;
                player.account = row.account;
                player.className = row.classname;
                Vector2 position = new Vector2(row.x, row.y);
                player.level = Mathf.Min(row.level, player.maxLevel); // limit to max level in case we changed it
                player.strength = row.strength;
                player.intelligence = row.intelligence;
                player.experience = row.experience;
                player.skillExperience = row.skillExperience;
                player.gold = row.gold;
                player.coins = row.coins;

                // is the position on a navmesh?
                // it might not be if we changed the terrain, or if the player
                // logged out in an instanced dungeon that doesn't exist anymore
                // NOTE: maxDist 0.1 is not enough in 2D. 1.0f works great.
                if (NavMesh2D.SamplePosition(position, out NavMeshHit2D hit, 1, NavMesh.AllAreas))
                {
                    // agent.warp is recommended over transform.position and
                    // avoids all kinds of weird bugs

                    Collider2D[] colliders;
                    colliders = Physics2D.OverlapBoxAll(position, new Vector2(1.0f, 1.0f), 0, GeneralManager.singleton.buildingCheckObstacle);

                    if (colliders.Length > 0)
                    {
                        //while (player.CheckInitialPosition() > 0)
                        //{
                        Transform start = NetworkManagerMMO.GetNearestStartPosition(position);
                        player.agent.Warp(start.position);
                        //}
                    }
                    else
                    {
                        player.agent.Warp(position);
                    }
                }
                // otherwise warp to start position
                else
                {
                    Transform start = NetworkManagerMMO.GetNearestStartPosition(position);

                    player.agent.Warp(start.position);
                    // no need to show the message all the time. it would spam
                    // the server logs too much.
                    //Debug.Log(player.name + " spawn position reset because it's not on a NavMesh anymore. This can happen if the player previously logged out in an instance or if the Terrain was changed.");
                }

                LoadEquipment(player);
                LoadInventory(player);
                LoadSkills(player);
                LoadBuffs(player);
                //LoadQuests(player);
                LoadGuildOnDemand(player);
                LoadCustom(player);

                // assign health / mana after max values were fully loaded
                // (they depend on equipment, buffs, etc.)
                player.health = row.health;
                player.mana = row.mana;



                // set 'online' directly. otherwise it would only be set during
                // the next CharacterSave() call, which might take 5-10 minutes.
                // => don't set it when loading previews though. only when
                //    really joining the world (hence setOnline flag)
                if (!isPreview)
                    connection.Execute("UPDATE characters SET online=1, lastsaved=? WHERE name=?", DateTime.UtcNow, characterName);


                // addon system hooks
                Utils.InvokeMany(typeof(Database), this, "CharacterLoad_", player);

                return go;
            }
            else Debug.LogError("no prefab found for class: " + row.classname);
        }
        return null;
    }

    void SaveInventory(Player player)
    {
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM character_inventory WHERE character=?", player.name);
        for (int i = 0; i < player.inventory.Count; ++i)
        {
            ItemSlot slot = player.inventory[i];
            if (slot.amount > 0) // only relevant items to save queries/storage/time
            {
                // note: .Insert causes a 'Constraint' exception. use Replace.
                connection.InsertOrReplace(new character_inventory
                {
                    character = player.name,
                    slot = i,
                    name = slot.item.name,
                    amount = slot.amount,
                    summonedHealth = slot.item.summonedHealth,
                    summonedLevel = slot.item.summonedLevel,
                    summonedExperience = slot.item.summonedExperience
                });
            }
        }
    }

    void SaveEquipment(Player player)
    {
        // equipment: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM character_equipment WHERE character=?", player.name);
        for (int i = 0; i < player.equipment.Count; ++i)
        {
            ItemSlot slot = player.equipment[i];
            if (slot.amount > 0) // only relevant equip to save queries/storage/time
            {
                connection.InsertOrReplace(new character_equipment
                {
                    character = player.name,
                    slot = i,
                    name = slot.item.name,
                    amount = slot.amount,
                    summonedHealth = slot.item.summonedHealth,
                    summonedLevel = slot.item.summonedLevel,
                    summonedExperience = slot.item.summonedExperience
                });
            }
        }
    }

    void SaveSkills(Player player)
    {
        // skills: remove old entries first, then add all new ones
        connection.Execute("DELETE FROM character_skills WHERE character=?", player.name);
        foreach (Skill skill in player.skills)
            if (skill.level > 0) // only learned skills to save queries/storage/time
            {
                // castTimeEnd and cooldownEnd are based on NetworkTime.time,
                // which will be different when restarting the server, so let's
                // convert them to the remaining time for easier save & load
                // note: this does NOT work when trying to save character data
                //       shortly before closing the editor or game because
                //       NetworkTime.time is 0 then.
                connection.InsertOrReplace(new character_skills
                {
                    character = player.name,
                    name = skill.name,
                    level = skill.level,
                    castTimeEnd = skill.CastTimeRemaining(),
                    cooldownEnd = skill.CooldownRemaining()
                });
            }
    }

    void SaveBuffs(Player player)
    {
        // buffs: remove old entries first, then add all new ones
        connection.Execute("DELETE FROM character_buffs WHERE character=?", player.name);
        foreach (Buff buff in player.buffs)
        {
            // buffTimeEnd is based on NetworkTime.time, which will be different
            // when restarting the server, so let's convert them to the
            // remaining time for easier save & load
            // note: this does NOT work when trying to save character data
            //       shortly before closing the editor or game because
            //       NetworkTime.time is 0 then.
            connection.InsertOrReplace(new character_buffs
            {
                character = player.name,
                name = buff.name,
                level = buff.level,
                buffTimeEnd = buff.BuffTimeRemaining()
            });
        }
    }

    //void SaveQuests(Player player)
    //{
    //    // quests: remove old entries first, then add all new ones
    //    connection.Execute("DELETE FROM character_quests WHERE character=?", player.name);
    //    foreach (Quest quest in player.quests)
    //    {
    //        connection.InsertOrReplace(new character_quests
    //        {
    //            character = player.name,
    //            name = quest.name,
    //            progress = quest.progress,
    //            completed = quest.completed
    //        });
    //    }
    //}

    // adds or overwrites character data in the database
    public void CharacterSave(Player player, bool online, bool useTransaction = true)
    {
        // only use a transaction if not called within SaveMany transaction
        if (useTransaction) connection.BeginTransaction();

        connection.InsertOrReplace(new characters
        {
            name = player.name,
            account = player.account,
            classname = player.className,
            x = player.transform.position.x,
            y = player.transform.position.y,
            level = player.level,
            health = player.health,
            mana = player.mana,
            strength = player.strength,
            intelligence = player.intelligence,
            experience = player.experience,
            skillExperience = player.skillExperience,
            gold = player.gold,
            coins = player.coins,
            online = online,
            lastsaved = DateTime.UtcNow
        });

       

        if (player.InGuild()) SaveGuild(player.guild, false); // TODO only if needs saving? but would be complicated
        if (player.InGuild() && player.guild.members[player.guild.GetMemberIndex(player.name)].rank == GuildRank.Master) SaveGuildAlly(player);


        // addon system hooks
        Utils.InvokeMany(typeof(Database), this, "CharacterSave_", player);

        if (useTransaction) connection.Commit();

        if (useTransaction) connection.BeginTransaction();
            SaveInventory(player);
            SaveEquipment(player);
            SaveSkills(player);
            SaveBuffs(player);
            //SaveQuests(player);
            SaveCustom(player);
        if (useTransaction) connection.Commit();
    }

    // save multiple characters at once (useful for ultra fast transactions)
    public void CharacterSaveMany(IEnumerable<Player> players, bool online = true)
    {
        if (!connection.IsInTransaction)
        {
            connection.BeginTransaction(); // transaction for performance
            foreach (Player player in players)
                CharacterSave(player, online, false);
            connection.Commit(); // end transaction
        }
    }

    // guilds //////////////////////////////////////////////////////////////////
    public bool GuildExists(string guild)
    {
        return connection.FindWithQuery<guild_info>("SELECT * FROM guild_info WHERE name=?", guild) != null;
    }

    public Guild LoadGuild(string guildName)
    {
        Guild guild = new Guild();

        // set name
        guild.name = guildName;

        // load guild info
        guild_info info = connection.FindWithQuery<guild_info>("SELECT * FROM guild_info WHERE name=?", guildName);
        if (info != null)
        {
            guild.notice = info.notice;
        }

        // load members list
        List<character_guild> rows = connection.Query<character_guild>("SELECT * FROM character_guild WHERE guild=?", guildName);
        GuildMember[] members = new GuildMember[rows.Count]; // avoid .ToList(). use array directly.
        for (int i = 0; i < rows.Count; ++i)
        {
            character_guild row = rows[i];

            GuildMember member = new GuildMember();
            member.name = row.character;
            member.rank = (GuildRank)row.rank;

            // is this player online right now? then use runtime data
            if (Player.onlinePlayers.TryGetValue(member.name, out Player player))
            {
                member.online = true;
                member.level = player.level;
            }
            else
            {
                member.online = false;
                // note: FindWithQuery<characters> is easier than ExecuteScalar<int> because we need the null check
                characters character = connection.FindWithQuery<characters>("SELECT * FROM characters WHERE name=?", member.name);
                member.level = character != null ? character.level : 1;
            }

            members[i] = member;
        }
        guild.members = members;
        return guild;
    }

    public void SaveGuild(Guild guild, bool useTransaction = true)
    {
        if (useTransaction) connection.BeginTransaction(); // transaction for performance

        // guild info
        connection.InsertOrReplace(new guild_info
        {
            name = guild.name,
            notice = guild.notice
        });

        // members list
        connection.Execute("DELETE FROM character_guild WHERE guild=?", guild.name);
        foreach (GuildMember member in guild.members)
        {
            connection.InsertOrReplace(new character_guild
            {
                character = member.name,
                guild = guild.name,
                rank = (int)member.rank
            });
        }

        if (useTransaction) connection.Commit(); // end transaction
    }

    public void RemoveGuild(string guild)
    {
        connection.BeginTransaction(); // transaction for performance
        connection.Execute("DELETE FROM guild_info WHERE name=?", guild);
        connection.Execute("DELETE FROM character_guild WHERE guild=?", guild);
        connection.Commit(); // end transaction
    }

    // item mall ///////////////////////////////////////////////////////////////
    public List<long> GrabCharacterOrders(string characterName)
    {
        // grab new orders from the database and delete them immediately
        //
        // note: this requires an orderid if we want someone else to write to
        // the database too. otherwise deleting would delete all the new ones or
        // updating would update all the new ones. especially in sqlite.
        //
        // note: we could just delete processed orders, but keeping them in the
        // database is easier for debugging / support.
        List<long> result = new List<long>();
        List<character_orders> rows = connection.Query<character_orders>("SELECT * FROM character_orders WHERE character=? AND processed=0", characterName);
        foreach (character_orders row in rows)
        {
            result.Add(row.coins);
            connection.Execute("UPDATE character_orders SET processed=1 WHERE orderid=?", row.orderid);
        }
        return result;
    }


    #region Ability
    public void SaveAbilities(Player player)
    {
        connection.Execute("DELETE FROM ability WHERE characterName=?", player.name);
        PlayerAbility ability = player.GetComponent<PlayerAbility>();
        if (ability.networkAbilities.Count > 0)
        {
            for (int i = 0; i < ability.networkAbilities.Count; i++)
            {
                CustomType.Ability slot = ability.networkAbilities[i];
                // note: .Insert causes a 'Constraint' exception. use Replace.
                connection.InsertOrReplace(new ability
                {
                    characterName = player.name,
                    abilityName = ability.networkAbilities[i].name,
                    level = ability.networkAbilities[i].level,
                    maxLevel = ability.networkAbilities[i].maxLevel,
                    baseValue = ability.networkAbilities[i].baseValue
                });
            }
        }
    }

    public void LoadAbilities(Player player)
    {
        PlayerAbility abilities = player.GetComponent<PlayerAbility>();

        foreach (ability row in connection.Query<ability>("SELECT * FROM ability WHERE characterName=?", player.name))
        {
            CustomType.Ability ability = new CustomType.Ability();
            ability.name = row.abilityName;
            ability.level = row.level;
            ability.maxLevel = row.maxLevel;
            ability.baseValue = row.baseValue;
            abilities.networkAbilities.Add(ability);
        }

    }

    #endregion 

    #region Boost
    public void SaveBoost(Player player)
    {
        connection.Execute("DELETE FROM boost WHERE characterName=?", player.name);

        PlayerBoost boosts = player.GetComponent<PlayerBoost>();

        if (boosts.networkBoost.Count > 0)
        {
            connection.InsertOrReplace(new boost
            {
                characterName = player.name,
                boostName = boosts.networkBoost[0].name != string.Empty ? boosts.networkBoost[0].name : string.Empty,
                velocityTimer = boosts.networkBoost[0].velocityTimer != string.Empty ? boosts.networkBoost[0].velocityTimer : string.Empty,
                velocityTimerServer = boosts.networkBoost[0].velocityTimerServer != string.Empty ? boosts.networkBoost[0].velocityTimerServer : string.Empty,
                velocityPerc = boosts.networkBoost[0].velocityPerc,
                accuracyTimer = boosts.networkBoost[0].accuracyTimer != string.Empty ? boosts.networkBoost[0].accuracyTimer : string.Empty,
                accuracyTimerServer = boosts.networkBoost[0].accuracyTimerServer != string.Empty ? boosts.networkBoost[0].accuracyTimerServer : string.Empty,
                accuracyPerc = boosts.networkBoost[0].accuracyPerc,
                missTimer = boosts.networkBoost[0].missTimer != string.Empty ? boosts.networkBoost[0].missTimer : string.Empty,
                missTimerServer = boosts.networkBoost[0].missTimerServer != string.Empty ? boosts.networkBoost[0].missTimerServer : string.Empty,
                missPerc = boosts.networkBoost[0].missPerc,
                hiddenIslandTimer = boosts.networkBoost[0].hiddenIslandTimer != string.Empty ? boosts.networkBoost[0].hiddenIslandTimer : string.Empty,
                hiddenIslandTimerServer = boosts.networkBoost[0].hiddenIslandTimerServer != string.Empty ? boosts.networkBoost[0].hiddenIslandTimerServer : string.Empty,
                doubleEXP = boosts.networkBoost[0].doubleEXP != string.Empty ? boosts.networkBoost[0].doubleEXP : string.Empty,
                doubleEXPServer = boosts.networkBoost[0].doubleEXPServer != string.Empty ? boosts.networkBoost[0].doubleEXPServer : string.Empty,
                doubleGold = boosts.networkBoost[0].doubleGold != string.Empty ? boosts.networkBoost[0].doubleGold : string.Empty,
                doubleGoldServer = boosts.networkBoost[0].doubleGoldServer != string.Empty ? boosts.networkBoost[0].doubleGoldServer : string.Empty,
                doubleLeaderPoints = boosts.networkBoost[0].doubleLeaderPoints != string.Empty ? boosts.networkBoost[0].doubleLeaderPoints : string.Empty,
                doubleLeaderPointsServer = boosts.networkBoost[0].doubleLeaderPointsServer != string.Empty ? boosts.networkBoost[0].doubleLeaderPointsServer : string.Empty,
                doubleDamageToMonster = boosts.networkBoost[0].doubleDamageToMonster != string.Empty ? boosts.networkBoost[0].doubleDamageToMonster : string.Empty,
                doubleDamageToMonsterServer = boosts.networkBoost[0].doubleDamageToMonsterServer != string.Empty ? boosts.networkBoost[0].doubleDamageToMonsterServer : string.Empty,
                doubleDamageToPlayer = boosts.networkBoost[0].doubleDamageToPlayer != string.Empty ? boosts.networkBoost[0].doubleDamageToPlayer : string.Empty,
                doubleDamageToPlayerServer = boosts.networkBoost[0].doubleDamageToPlayerServer != string.Empty ? boosts.networkBoost[0].doubleDamageToPlayerServer : string.Empty
                });
        }
    }

    public void LoadBoost(Player player)
    {
        PlayerBoost boosts = player.GetComponent<PlayerBoost>();

        foreach (boost row in connection.Query<boost>("SELECT * FROM boost WHERE characterName=?", player.name))
        {
            CustomType.Boost boost = new CustomType.Boost();
            boost.velocityTimer = row.velocityTimer;
            boost.velocityTimerServer = row.velocityTimerServer;
            boost.velocityPerc = row.velocityPerc;
            boost.accuracyTimer = row.accuracyTimer;
            boost.accuracyTimerServer = row.accuracyTimerServer;
            boost.accuracyPerc = row.accuracyPerc;
            boost.missTimer = row.missTimer;
            boost.missTimerServer = row.missTimerServer;
            boost.missPerc = row.missPerc;
            boost.hiddenIslandTimer = row.hiddenIslandTimer;
            boost.hiddenIslandTimerServer = row.hiddenIslandTimerServer;
            boost.doubleEXP = row.doubleEXP;
            boost.doubleEXPServer = row.doubleEXPServer;
            boost.doubleGold = row.doubleGold;
            boost.doubleGoldServer = row.doubleGoldServer;
            boost.doubleLeaderPoints = row.doubleLeaderPoints;
            boost.doubleLeaderPointsServer = row.doubleLeaderPointsServer;
            boost.doubleDamageToMonster = row.doubleDamageToMonster;
            boost.doubleDamageToMonsterServer = row.doubleDamageToMonsterServer;
            boost.doubleDamageToPlayer = row.doubleDamageToPlayer;
            boost.doubleDamageToPlayerServer = row.doubleDamageToPlayerServer;
            player.playerBoost.networkBoost.Clear();
            boosts.networkBoost.Add(boost);
        }

    }

    #endregion

    #region Blood
    public void SaveBlood(Player player)
    {
        PlayerBlood blood = player.GetComponent<PlayerBlood>();
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM blood WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        connection.InsertOrReplace(new blood
        {
            characterName = player.name,
            currentBlood = blood.currentBlood
        });
    }
    public void LoadBlood(Player player)
    {
        PlayerBlood bloods = player.GetComponent<PlayerBlood>();
        blood row = connection.FindWithQuery<blood>("SELECT * FROM blood WHERE characterName=?", player.name);

        if (row != null)
        {
            bloods.currentBlood = row.currentBlood;
        }
    }
    #endregion

    #region Hungry
    public void SaveHungry(Player player)
    {
        PlayerHungry hungry = player.GetComponent<PlayerHungry>();
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM hungry WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        connection.InsertOrReplace(new hungry
        {
            characterName = player.name,
            currentHungry = hungry.currentHungry
        });

    }
    public void LoadHungry(Player player)
    {
        PlayerHungry hungry = player.GetComponent<PlayerHungry>();

        foreach (hungry row in connection.Query<hungry>("SELECT * FROM hungry WHERE characterName=?", player.name))
        {
            hungry.currentHungry = row.currentHungry;
        }

    }
    #endregion

    #region Leaderpoint
    public void SaveLeaderpoint(Player player)
    {
        PlayerLeaderPoints leaderPoints = player.GetComponent<PlayerLeaderPoints>();
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM leaderPoint WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        connection.InsertOrReplace(new leaderPoint
        {
            characterName = player.name,
            monsterKill = leaderPoints.monsterKill,
            bossKill = leaderPoints.bossKill,
            plantPoint = leaderPoints.plantPoint,
            rockPoint = leaderPoints.rockPoint,
            treePoint = leaderPoints.treePoint,
            upgradeItemPoint = leaderPoints.upgradeItemPoint,
            craftItemPoint = leaderPoints.craftItemPoint,
            buildinPoint = leaderPoints.buildinPoint,
            buyBoostPoint = leaderPoints.buyBoostPoint,
            playerKill = leaderPoints.playerKill
        });

    }
    public void LoadLeaderpoint(Player player)
    {
        PlayerLeaderPoints leaderPoints = player.GetComponent<PlayerLeaderPoints>();

        foreach (leaderPoint row in connection.Query<leaderPoint>("SELECT * FROM leaderPoint WHERE characterName=?", player.name))
        {
            leaderPoints.monsterKill = row.monsterKill;
            leaderPoints.bossKill = row.bossKill;
            leaderPoints.playerKill = row.playerKill;
            leaderPoints.plantPoint = row.plantPoint;
            leaderPoints.rockPoint = row.rockPoint;
            leaderPoints.treePoint = row.treePoint;
            leaderPoints.upgradeItemPoint = row.upgradeItemPoint;
            leaderPoints.craftItemPoint = row.craftItemPoint;
            leaderPoints.buildinPoint = row.buildinPoint;
            leaderPoints.buyBoostPoint = row.buyBoostPoint;
        }

    }

    public int LoadGroupLeaderboardPoint(Player player)
    {
        int Total = 0;
        foreach (character_guild row in connection.Query<character_guild>("SELECT character FROM character_guild WHERE guild=?", player.guild.name))
        {
            foreach (leaderPoint row2 in connection.Query<leaderPoint>("SELECT * FROM leaderPoint WHERE characterName=?", row.character))
            {
                Total +=
                row2.monsterKill +
                row2.bossKill +
                row2.playerKill +
                row2.plantPoint +
                row2.rockPoint +
                row2.treePoint +
                row2.upgradeItemPoint +
                row2.craftItemPoint +
                row2.buildinPoint +
                row2.buyBoostPoint;
            }
        }
        return Total;

    }

    public int LoadAllyGroupLeaderboardPoint(Player player)
    {
        int Total = 0;
        // Group
        foreach (character_guild row in connection.Query<character_guild>("SELECT character FROM character_guild WHERE guild=?", player.guild.name))
        {
            foreach (leaderPoint row2 in connection.Query<leaderPoint>("SELECT * FROM leaderPoint WHERE characterName=?", row.character))
            {
                Total +=
                row2.monsterKill +
                row2.bossKill +
                row2.playerKill +
                row2.plantPoint +
                row2.rockPoint +
                row2.treePoint +
                row2.upgradeItemPoint +
                row2.craftItemPoint +
                row2.buildinPoint +
                row2.buyBoostPoint;

            }
        }

        // Ally group
        foreach (guildAlly row in connection.Query<guildAlly>("SELECT * FROM guildAlly WHERE guildName=?", player.guild.name))
        {
            foreach (character_guild row2 in connection.Query<character_guild>("SELECT character FROM character_guild WHERE guild=?", row.firstAlly))
            {
                foreach (leaderPoint row3 in connection.Query<leaderPoint>("SELECT * FROM leaderPoint WHERE characterName=?", row2.character))
                {
                    Total +=
                    row3.monsterKill +
                    row3.bossKill +
                    row3.playerKill +
                    row3.plantPoint +
                    row3.rockPoint +
                    row3.treePoint +
                    row3.upgradeItemPoint +
                    row3.craftItemPoint +
                    row3.buildinPoint +
                    row3.buyBoostPoint;

                }
            }
            foreach (character_guild row2 in connection.Query<character_guild>("SELECT character FROM character_guild WHERE guild=?", row.secondAlly))
            {
                foreach (leaderPoint row3 in connection.Query<leaderPoint>("SELECT * FROM leaderPoint WHERE characterName=?", row2.character))
                {
                    Total +=
                    row3.monsterKill +
                    row3.bossKill +
                    row3.playerKill +
                    row3.plantPoint +
                    row3.rockPoint +
                    row3.treePoint +
                    row3.upgradeItemPoint +
                    row3.craftItemPoint +
                    row3.buildinPoint +
                    row3.buyBoostPoint;

                }
            }

            foreach (character_guild row2 in connection.Query<character_guild>("SELECT character FROM character_guild WHERE guild=?", row.thirdAlly))
            {
                foreach (leaderPoint row3 in connection.Query<leaderPoint>("SELECT * FROM leaderPoint WHERE characterName=?", row2.character))
                {
                    Total +=
                    row3.monsterKill +
                    row3.bossKill +
                    row3.playerKill +
                    row3.plantPoint +
                    row3.rockPoint +
                    row3.treePoint +
                    row3.upgradeItemPoint +
                    row3.craftItemPoint +
                    row3.buildinPoint +
                    row3.buyBoostPoint;

                }
            }

            foreach (character_guild row2 in connection.Query<character_guild>("SELECT character FROM character_guild WHERE guild=?", row.forthAlly))
            {
                foreach (leaderPoint row3 in connection.Query<leaderPoint>("SELECT * FROM leaderPoint WHERE characterName=?", row2.character))
                {
                    Total +=
                    row3.monsterKill +
                    row3.bossKill +
                    row3.playerKill +
                    row3.plantPoint +
                    row3.rockPoint +
                    row3.treePoint +
                    row3.upgradeItemPoint +
                    row3.craftItemPoint +
                    row3.buildinPoint +
                    row3.buyBoostPoint;

                }
            }
            foreach (character_guild row2 in connection.Query<character_guild>("SELECT character FROM character_guild WHERE guild=?", row.fifthAlly))
            {
                foreach (leaderPoint row3 in connection.Query<leaderPoint>("SELECT * FROM leaderPoint WHERE characterName=?", row2.character))
                {
                    Total +=
                    row3.monsterKill +
                    row3.bossKill +
                    row3.playerKill +
                    row3.plantPoint +
                    row3.rockPoint +
                    row3.treePoint +
                    row3.upgradeItemPoint +
                    row3.craftItemPoint +
                    row3.buildinPoint +
                    row3.buyBoostPoint;

                }
            }
        }
        return Total;
    }

    #endregion

    #region Emoji
    public void SaveEmoji(Player player)
    {
        PlayerEmoji playerEmoji = player.GetComponent<PlayerEmoji>();
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM emoji WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        for (int i = 0; i < playerEmoji.networkEmoji.Count; i++)
        {
            int index = i;
            connection.InsertOrReplace(new emoji
            {
                characterName = player.name,
                emojiName = player.playerEmoji.networkEmoji[index]
            }); ;
        }
    }
    public void LoadEmoji(Player player)
    {
        PlayerEmoji emoji = player.GetComponent<PlayerEmoji>();

        foreach (emoji row in connection.Query<emoji>("SELECT * FROM emoji WHERE characterName=?", player.name))
        {
            emoji.networkEmoji.Add(row.emojiName);
        }

    }
    #endregion

    #region Dance
    public void SaveDance(Player player)
    {
        PlayerDance playerDance = player.GetComponent<PlayerDance>();
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM dance WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        for (int i = 0; i < playerDance.networkDance.Count; i++)
        {
            int index = i;
            connection.InsertOrReplace(new dance
            {
                characterName = player.name,
                danceName = player.playerDance.networkDance[index]
            }); ;
        }
    }
    public void LoadDance(Player player)
    {
        PlayerDance playerDance = player.GetComponent<PlayerDance>();

        foreach (dance row in connection.Query<dance>("SELECT * FROM dance WHERE characterName=?", player.name))
        {
            playerDance.networkDance.Add(row.danceName);
        }

    }
    #endregion

    #region Partner
    public void SavePartner(Player player)
    {
        PlayerMarriage partner = player.GetComponent<PlayerMarriage>();
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM partner WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        connection.InsertOrReplace(new partner
        {
            characterName = player.name,
            partnerName = partner.partnerName
        });

    }
    public void LoadPartner(Player player)
    {
        PlayerMarriage partner = player.GetComponent<PlayerMarriage>();

        foreach (partner row in connection.Query<partner>("SELECT * FROM partner WHERE characterName=?", player.name))
        {
            partner.partnerName = row.partnerName;
        }

    }

    public void DeletePartner(string partnerName)
    {
        connection.Execute("DELETE FROM partner WHERE characterName=?", partnerName);

    }

    #endregion

    #region Options
    public void SaveOptions(Player player)
    {
        PlayerOptions options = player.GetComponent<PlayerOptions>();
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM options WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        connection.InsertOrReplace(new options
        {
            characterName = player.name,
            blockMarriage = Convert.ToInt32(options.blockMarriage),
            blockParty = Convert.ToInt32(options.blockParty),
            blockGroup = Convert.ToInt32(options.blockGroup),
            blockAlly = Convert.ToInt32(options.blockAlly),
            blockTrade = Convert.ToInt32(options.blockTrade),
            blockFriend = Convert.ToInt32(options.blockFriend),
            blockFootstep = Convert.ToInt32(options.blockFootstep),
            blockSound = Convert.ToInt32(options.blockSound),
            blockButtonSound = Convert.ToInt32(options.blockButtonSounds),
        });

    }
    public void LoadOptions(Player player)
    {
        PlayerOptions options = player.GetComponent<PlayerOptions>();

        foreach (options row in connection.Query<options>("SELECT * FROM options WHERE characterName=?", player.name))
        {
            options.blockMarriage = Convert.ToBoolean(row.blockMarriage);
            options.blockParty = Convert.ToBoolean(row.blockParty);
            options.blockGroup = Convert.ToBoolean(row.blockGroup);
            options.blockAlly = Convert.ToBoolean(row.blockAlly);
            options.blockTrade = Convert.ToBoolean(row.blockTrade);
            options.blockFriend = Convert.ToBoolean(row.blockFriend);
            options.blockFootstep = Convert.ToBoolean(row.blockFootstep);
            options.blockSound = Convert.ToBoolean(row.blockSound);
            options.blockButtonSounds = Convert.ToBoolean(row.blockButtonSound);
        }

    }

    public void SaveIssue(Player player, string Type, string description)
    {
        // note: .Insert causes a 'Constraint' exception. use Replace.
        connection.InsertOrReplace(new issue
        {
            characterName = player.name,
            positionX = player.transform.position.x,
            positionY = player.transform.position.y,
            description = description,
            type = Type,
            operatorID = "",
            closed = ""
        });
    }

    #endregion

    #region Poisoning
    public void SavePoisoning(Player player)
    {
        PlayerPoisoning poisoning = player.GetComponent<PlayerPoisoning>();

        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM poisoning WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        connection.InsertOrReplace(new poisoning
        {
            characterName = player.name,
            currentPoisoning = poisoning.currentPoisoning
        });

    }
    public void LoadPoisoning(Player player)
    {
        PlayerPoisoning poisoning = player.GetComponent<PlayerPoisoning>();

        foreach (poisoning row in connection.Query<poisoning>("SELECT * FROM poisoning WHERE characterName=?", player.name))
        {
            poisoning.currentPoisoning = row.currentPoisoning;
        }

    }
    #endregion

    #region Radio
    public void SaveRadio(Player player)
    {
        PlayerRadio radio = player.GetComponent<PlayerRadio>();

        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM radio WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        connection.InsertOrReplace(new radio
        {
            characterName = player.name,
            isActive = Convert.ToInt32(radio.isOn)
        });

    }
    public void LoadRadio(Player player)
    {
        PlayerRadio radio = player.GetComponent<PlayerRadio>();

        foreach (radio row in connection.Query<radio>("SELECT * FROM radio WHERE characterName=?", player.name))
        {
            radio.isOn = Convert.ToBoolean(row.isActive);
        }

    }
    #endregion

    #region Torch
    public void SaveTorch(Player player)
    {
        PlayerTorch torch = player.GetComponent<PlayerTorch>();

        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM torch WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        connection.InsertOrReplace(new torch
        {
            characterName = player.name,
            isActive = Convert.ToInt32(torch.isOn)
        });

    }
    public void LoadTorch(Player player)
    {
        PlayerTorch torch = player.GetComponent<PlayerTorch>();

        foreach (torch row in connection.Query<torch>("SELECT * FROM torch WHERE characterName=?", player.name))
        {
            torch.isOn = Convert.ToBoolean(row.isActive);
        }

    }
    #endregion

    #region Spawnpoint
    public void SaveSpawnpoint(Player player)
    {
        PlayerSpawnpoint spawnpoint = player.GetComponent<PlayerSpawnpoint>();

        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM spawnpoint WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        for (int i = 0; i < spawnpoint.spawnpoint.Count; i++)
        {
            connection.InsertOrReplace(new spawnpoint
            {
                characterName = player.name,
                spawnpointName = spawnpoint.spawnpoint[i].name,
                posX = spawnpoint.spawnpoint[i].spawnPositionx,
                posY = spawnpoint.spawnpoint[i].spawnPositiony,
                prefered = Convert.ToInt32(spawnpoint.spawnpoint[i].prefered)
            });
        }

    }
    public void LoadSpawnpoint(Player player)
    {
        PlayerSpawnpoint spawnpoint = player.GetComponent<PlayerSpawnpoint>();

        foreach (spawnpoint row in connection.Query<spawnpoint>("SELECT * FROM spawnpoint WHERE characterName=?", player.name))
        {
            CustomType.Spawnpoint sp = new CustomType.Spawnpoint(row.spawnpointName, row.posX, row.posY, Convert.ToBoolean(row.prefered));
            spawnpoint.spawnpoint.Add(sp);
        }

    }
    #endregion

    #region Guild & Ally
    public void SaveGuildAlly(Player player)
    {
        PlayerAlliance alliance = player.GetComponent<PlayerAlliance>();
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        if (player.guild.name != string.Empty) connection.Execute("DELETE FROM guildAlly WHERE guildName=?", player.guild.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        connection.InsertOrReplace(new guildAlly
        {
            guildName = player.guild.name != string.Empty ? player.guild.name : string.Empty,
            firstAlly = alliance.guildAlly.Count >= 1 ? alliance.guildAlly[0] : string.Empty,
            secondAlly = alliance.guildAlly.Count >= 2 ? alliance.guildAlly[1] : string.Empty,
            thirdAlly = alliance.guildAlly.Count >= 3 ? alliance.guildAlly[2] : string.Empty,
            forthAlly = alliance.guildAlly.Count >= 4 ? alliance.guildAlly[3] : string.Empty,
            fifthAlly = alliance.guildAlly.Count >= 5 ? alliance.guildAlly[4] : string.Empty
        });
    }

    public void DeleteGuildAlly(string guildName)
    {
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM guildAlly WHERE guildName=?", guildName);
    }

    public void DeleteGuildContaindAlly(string guildName)
    {
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM guildAlly WHERE firstAlly=? OR secondAlly=? OR thirdAlly=? OR forthAlly=? OR fifthAlly=?", guildName);
    }
    public void LoadGuilAlly(Player player)
    {
        PlayerAlliance alliance = player.GetComponent<PlayerAlliance>();

        guildAlly row = connection.FindWithQuery<guildAlly>("SELECT * FROM guildAlly WHERE guildName=?", player.guild.name);

        if (row != null)
        {
            if (row.firstAlly != string.Empty)
            {
                alliance.guildAlly.Add(row.firstAlly);
            }
            if (row.secondAlly != string.Empty)
            {
                alliance.guildAlly.Add(row.secondAlly);
            }
            if (row.thirdAlly != string.Empty)
            {
                alliance.guildAlly.Add(row.thirdAlly);
            }
            if (row.forthAlly != string.Empty)
            {
                alliance.guildAlly.Add(row.forthAlly);
            }
            if (row.fifthAlly != string.Empty)
            {
                alliance.guildAlly.Add(row.fifthAlly);
            }
        }
    }

    public void LoadGuildOnDemandMenu(string guildName)
    {
        if (guildName != null)
        {
            // load guild on demand when the first player of that guild logs in
            // (= if it's not in GuildSystem.guilds yet)
            if (!GuildSystem.guilds.ContainsKey(guildName))
            {
                Guild guild = LoadGuild(guildName);
                GuildSystem.guilds[guild.name] = guild;
            }
        }
    }

    #endregion

    #region Friends
    public void SaveFriends(Player player)
    {
        PlayerFriend friends = player.GetComponent<PlayerFriend>();

        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM friends WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        foreach (string s in friends.playerFriends)
        {
            connection.InsertOrReplace(new friends
            {
                friendName = s,
                characterName = player.name
            });
        }

    }
    public void SaveSingleFriend(Player playerInviter, Player Invite)
    {
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.InsertOrReplace(new friends
        {
            friendName = playerInviter.name,
            characterName = Invite.name
        });
    }

    public void SaveFriendsRequest(Player player)
    {
        PlayerFriend friends = player.GetComponent<PlayerFriend>();

        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM friendsRequest WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        foreach (string s in friends.playerRequest)
        {
            connection.InsertOrReplace(new friendsRequest
            {
                friendName = s,
                characterName = player.name
            });
        }
    }

    public void LoadFriends(Player player)
    {
        PlayerFriend friends = player.GetComponent<PlayerFriend>();

        // then load valid items and put into their slots
        // (one big query is A LOT faster than querying each slot separately)
        foreach (friends r in connection.Query<friends>("SELECT * FROM friends WHERE characterName=?", player.name))
        {
            friends.playerFriends.Add(r.friendName);
        }
    }

    public void LoadFriendsRequest(Player player)
    {
        PlayerFriend friends = player.GetComponent<PlayerFriend>();

        // then load valid items and put into their slots
        // (one big query is A LOT faster than querying each slot separately)
        foreach (friends r in connection.Query<friends>("SELECT * FROM friendsRequest WHERE characterName=?", player.name))
        {
            friends.playerRequest.Add(r.friendName);
        }
    }

    public void RemoveFriend(string player, string friendName)
    {
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM friends WHERE friendName=? AND characterName=?", friendName, player);
    }

    public void RemoveFriendRequest(string player, string friendName)
    {
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM friendsRequest WHERE friendName=? AND characterName=?", friendName, player);
    }

    #endregion

    #region Thirsty
    public void SaveThirsty(Player player)
    {
        PlayerThirsty thirsty = player.GetComponent<PlayerThirsty>();

        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM thirsty WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        connection.InsertOrReplace(new thirsty
        {
            characterName = player.name,
            currentThirsty = thirsty.currentThirsty
        });

    }
    public void LoadThirsty(Player player)
    {
        PlayerThirsty thirsty = player.GetComponent<PlayerThirsty>();

        foreach (thirsty row in connection.Query<thirsty>("SELECT * FROM thirsty WHERE characterName=?", player.name))
        {
            thirsty.currentThirsty = row.currentThirsty;
        }

    }
    #endregion

    #region Wood
    //public void SaveWood(Player player)
    //{
    //    PlayerWood wood = player.GetComponent<PlayerWood>();

    //    // inventory: remove old entries first, then add all new ones
    //    // (we could use UPDATE where slot=... but deleting everything makes
    //    //  sure that there are never any ghosts)
    //    connection.Execute("DELETE FROM wood WHERE characterName=?", player.name);
    //    // note: .Insert causes a 'Constraint' exception. use Replace.
    //    connection.InsertOrReplace(new wood
    //    {
    //        characterName = player.name,
    //        inWood = Convert.ToInt32(wood.inWood)
    //    });

    //}
    //public void LoadWood(Player player)
    //{
    //    PlayerWood wood = player.GetComponent<PlayerWood>();

    //    foreach (wood row in connection.Query<wood>("SELECT * FROM wood WHERE characterName=?", player.name))
    //    {
    //        wood.inWood = Convert.ToBoolean(row.inWood);
    //    }

    //}
    #endregion

    #region AdditionalInventory

    public void SaveAdditionalInventory(Player player)
    {
        connection.Execute("DELETE FROM additionalInventory WHERE characterName=?", player.name);
        for (int i = 0; i < player.inventory.Count; i++)
        {
            ItemSlot slot = player.inventory[i];
            if (slot.amount > 0) // only relevant items to save queries/storage/time
            {
                // note: .Insert causes a 'Constraint' exception. use Replace.
                connection.InsertOrReplace(new additionalInventory
                {
                    index = i,
                    characterName = player.name,
                    slot = i,
                    name = slot.item.name,
                    currentArmor = slot.item.currentArmor,
                    currentUnsanity = slot.item.currentUnsanity,
                    alreadyShooted = slot.item.alreadyShooted,
                    totalAlreadyShooted = slot.item.totalAlreadyShooted,
                    radioCurrentBattery = slot.item.radioCurrentBattery,
                    torchCurrentBattery = slot.item.torchCurrentBattery,
                    durability = slot.item.durability,
                    weight = slot.item.weight,
                    accuracyLevel = slot.item.accuracyLevel,
                    missLevel = slot.item.missLevel,
                    armorLevel = slot.item.armorLevel,
                    chargeLevel = slot.item.chargeLevel,
                    batteryLevel = slot.item.batteryLevel,
                    weightLevel = slot.item.weightLevel,
                    durabilityLevel = slot.item.durabilityLevel,
                    unsanityLevel = slot.item.unsanityLevel,
                    bagLevel = slot.item.bagLevel,
                    gasolineContainer = slot.item.gasolineContainer,
                    honeyContainer = slot.item.honeyContainer,
                    waterContainer = slot.item.waterContainer,
                    cookCountdown = slot.item.cookCountdown,
                    wet = slot.item.wet
                }); ;
            }
        }

    }

    public void LoadAdditionalInventory(Player player)
    {
        foreach (additionalInventory row in connection.Query<additionalInventory>("SELECT * FROM additionalInventory WHERE characterName=?", player.name))
        {
            if (row.slot < player.inventorySize)
            {
                if (ScriptableItem.dict.TryGetValue(row.name.GetStableHashCode(), out ScriptableItem itemData))
                {
                    Item item = player.inventory[row.slot].item;
                    item.currentArmor = row.currentArmor;
                    item.currentUnsanity = row.currentUnsanity;
                    item.alreadyShooted = row.alreadyShooted;
                    item.totalAlreadyShooted = row.totalAlreadyShooted;
                    item.radioCurrentBattery = row.radioCurrentBattery;
                    item.torchCurrentBattery = row.torchCurrentBattery;
                    item.durability = row.durability;
                    item.weight = row.weight;
                    item.accuracyLevel = row.accuracyLevel;
                    item.missLevel = row.missLevel;
                    item.armorLevel = row.armorLevel;
                    item.chargeLevel = row.chargeLevel;
                    item.batteryLevel = row.batteryLevel;
                    item.weightLevel = row.weightLevel;
                    item.durabilityLevel = row.durabilityLevel;
                    item.unsanityLevel = row.unsanityLevel;
                    item.bagLevel = row.bagLevel;
                    item.gasolineContainer = row.gasolineContainer;
                    item.honeyContainer = row.honeyContainer;
                    item.waterContainer = row.waterContainer;
                    item.cookCountdown = row.cookCountdown;
                    item.wet = row.wet;
                    player.inventory[row.slot] = new ItemSlot(item, player.inventory[row.slot].amount);
                }
                else Debug.LogWarning("LoadInventory: skipped item " + row.name + " for " + player.name + " because it doesn't exist anymore. If it wasn't removed intentionally then make sure it's in the Resources folder.");
            }
            else Debug.LogWarning("LoadInventory: skipped slot " + row.slot + " for " + player.name + " because it's bigger than size " + player.inventorySize);
        }

    }

    #endregion

    #region AdditionalEquipment

    public void SaveAdditionalEquipment(Player player)
    {
        connection.Execute("DELETE FROM additionalEquipment WHERE characterName=?", player.name);
        for (int i = 0; i < player.equipment.Count; i++)
        {
            ItemSlot slot = player.equipment[i];
            if (slot.amount > 0) // only relevant items to save queries/storage/time
            {
                // note: .Insert causes a 'Constraint' exception. use Replace.
                connection.InsertOrReplace(new additionalEquipment
                {
                    index = i,
                    characterName = player.name,
                    slot = i,
                    name = slot.item.name,
                    currentArmor = slot.item.currentArmor,
                    currentUnsanity = slot.item.currentUnsanity,
                    alreadyShooted = slot.item.alreadyShooted,
                    totalAlreadyShooted = slot.item.totalAlreadyShooted,
                    radioCurrentBattery = slot.item.radioCurrentBattery,
                    torchCurrentBattery = slot.item.torchCurrentBattery,
                    durability = slot.item.durability,
                    weight = slot.item.weight,
                    accuracyLevel = slot.item.accuracyLevel,
                    missLevel = slot.item.missLevel,
                    armorLevel = slot.item.armorLevel,
                    chargeLevel = slot.item.chargeLevel,
                    batteryLevel = slot.item.batteryLevel,
                    weightLevel = slot.item.weightLevel,
                    durabilityLevel = slot.item.durabilityLevel,
                    unsanityLevel = slot.item.unsanityLevel,
                    bagLevel = slot.item.bagLevel,
                    gasolineContainer = slot.item.gasolineContainer,
                    honeyContainer = slot.item.honeyContainer,
                    waterContainer = slot.item.waterContainer,
                    cookCountdown = slot.item.cookCountdown,
                    wet = slot.item.wet
                }); ;
            }
        }

    }

    public void LoadAdditionalEquipment(Player player)
    {
        foreach (additionalInventory row in connection.Query<additionalInventory>("SELECT * FROM additionalEquipment WHERE characterName=?", player.name))
        {
            if (ScriptableItem.dict.TryGetValue(row.name.GetStableHashCode(), out ScriptableItem itemData))
            {
                Item item = player.equipment[row.slot].item;
                item.currentArmor = row.currentArmor;
                item.currentUnsanity = row.currentUnsanity;
                item.alreadyShooted = row.alreadyShooted;
                item.totalAlreadyShooted = row.totalAlreadyShooted;
                item.radioCurrentBattery = row.radioCurrentBattery;
                item.torchCurrentBattery = row.torchCurrentBattery;
                item.durability = row.durability;
                item.weight = row.weight;
                item.accuracyLevel = row.accuracyLevel;
                item.missLevel = row.missLevel;
                item.armorLevel = row.armorLevel;
                item.chargeLevel = row.chargeLevel;
                item.batteryLevel = row.batteryLevel;
                item.weightLevel = row.weightLevel;
                item.durabilityLevel = row.durabilityLevel;
                item.unsanityLevel = row.unsanityLevel;
                item.bagLevel = row.bagLevel;
                item.gasolineContainer = row.gasolineContainer;
                item.honeyContainer = row.honeyContainer;
                item.waterContainer = row.waterContainer;
                item.cookCountdown = row.cookCountdown;
                item.wet = row.wet;
                player.equipment[row.slot] = new ItemSlot(item, player.equipment[row.slot].amount);
            }
            else Debug.LogWarning("LoadInventory: skipped item " + row.name + " for " + player.name + " because it doesn't exist anymore. If it wasn't removed intentionally then make sure it's in the Resources folder.");
        }

    }

    #endregion

    #region CharacterCreation

    public void SaveCharacterCreation(Player player)
    {
        connection.Execute("DELETE FROM characterCreation WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        connection.InsertOrReplace(new characterCreation
        {
            characterName = player.name,
            sex = player.playerCreation.sex,
            hairType = player.playerCreation.hairType,
            beard = player.playerCreation.beard,
            hairColor = player.playerCreation.hairColor,
            underwearColor = player.playerCreation.underwearColor,
            eyesColor = player.playerCreation.eyesColor,
            skinColor = player.playerCreation.skinColor,
            fat = player.playerCreation.fat,
            thin = player.playerCreation.thin,
            height = player.playerCreation.height,
            breast = player.playerCreation.breast,
            bag = player.playerCreation.bag
        });
    }

    public void LoadCharacterCreation(Player player)
    {
            foreach (characterCreation row in connection.Query<characterCreation>("SELECT * FROM characterCreation WHERE characterName=?", player.name))
            {
                player.playerCreation.sex = row.sex;
                player.playerCreation.hairType = row.hairType;
                player.playerCreation.beard = row.beard;
                player.playerCreation.hairColor = row.hairColor;
                player.playerCreation.underwearColor = row.underwearColor;
                player.playerCreation.eyesColor = row.eyesColor;
                player.playerCreation.skinColor = row.skinColor;
                player.playerCreation.fat = row.fat;
                player.playerCreation.thin = row.thin;
                player.playerCreation.muscle = row.muscle;
                player.playerCreation.height = row.height;
                player.playerCreation.breast = row.breast;
                player.playerCreation.bag = row.bag;
            }        
    }

    #endregion

    #region BeeKeeper
    public void SaveBeeKeeper(string scene, List<BeeKeeper> beeKeeper)
    {
        connection.Execute("DELETE FROM containerBee");
        // note: .Insert causes a 'Constraint' exception. use Replace.
        for (int e = 0; e < beeKeeper.Count; e++)
        {
            Building building = beeKeeper[e].GetComponent<Building>();
            //Npc npc = beeKeeper[e].GetComponent<Npc>();
            connection.InsertOrReplace(new containerBee
            {
                myIndex = e,
                sceneName = scene,
                guild = building.guild,
                owner = building.owner,
                positionX = building.transform.position.x,
                positionY = building.transform.position.y,
                level = building.level,
                health = building.health,
                totalBeeOne = beeKeeper[e].beeContainers[0].totalBee,
                totalHoneyOne = beeKeeper[e].beeContainers[0].totalHoney,
                totalBeeTwo = beeKeeper[e].beeContainers[1].totalBee,
                totalHoneyTwo = beeKeeper[e].beeContainers[1].totalHoney,
                totalBeeThree = beeKeeper[e].beeContainers[2].totalBee,
                totalHoneyThree = beeKeeper[e].beeContainers[2].totalHoney,
                totalBeeFour = beeKeeper[e].beeContainers[3].totalBee,
                totalHoneyFour = beeKeeper[e].beeContainers[3].totalHoney,
                totalBeeFive = beeKeeper[e].beeContainers[4].totalBee,
                totalHoneyFive = beeKeeper[e].beeContainers[4].totalHoney,
                isPremium = Convert.ToInt32(building.isPremiumZone)
            });
        }
    }


    public void LoadBeeKeeper(GameObject prefab, string scene)
    {
        foreach (containerBee row in connection.Query<containerBee>("SELECT * FROM containerBee WHERE sceneName=?", scene))
        {
            GameObject g = Instantiate(prefab);
            Building building = g.GetComponent<Building>();
            BeeKeeper beeKeeper = g.GetComponent<BeeKeeper>();
            g.transform.position = new Vector3(row.positionX, row.positionY, 0.0f);
            building.guild = row.guild;
            building.owner = row.owner;
            building.health = row.health;
            building.level = row.level;
            building.isPremiumZone = Convert.ToBoolean(row.isPremium);
            BeeContainer beeContainer = new BeeContainer();

            if (!building.isPremiumZone) buildingManager.AddToList(g);

            if (buildingManager.beeKeeper[row.myIndex].beeContainers.Count == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    buildingManager.beeKeeper[row.myIndex].beeContainers.Add(new BeeContainer());
                }
            }

            if (building.isPremiumZone)
            {
                for (int i = 0; i < 5; i++)
                {
                    int index = i;
                    if (index == 0)
                    {
                        beeContainer = buildingManager.beeKeeper[row.myIndex].beeContainers[index];
                        beeContainer.totalBee = row.totalBeeOne;
                        beeContainer.totalHoney = row.totalHoneyOne;
                        buildingManager.beeKeeper[row.myIndex].beeContainers[0] = beeContainer;
                    }
                    if (index == 1)
                    {
                        beeContainer = buildingManager.beeKeeper[row.myIndex].beeContainers[index];
                        beeContainer.totalBee = row.totalBeeOne;
                        beeContainer.totalHoney = row.totalHoneyOne;
                        buildingManager.beeKeeper[row.myIndex].beeContainers[1] = beeContainer;
                    }
                    if (index == 2)
                    {
                        beeContainer = buildingManager.beeKeeper[row.myIndex].beeContainers[index];
                        beeContainer.totalBee = row.totalBeeOne;
                        beeContainer.totalHoney = row.totalHoneyOne;
                        buildingManager.beeKeeper[row.myIndex].beeContainers[2] = beeContainer;
                    }
                    if (index == 3)
                    {
                        beeContainer = buildingManager.beeKeeper[row.myIndex].beeContainers[index];
                        beeContainer.totalBee = row.totalBeeOne;
                        beeContainer.totalHoney = row.totalHoneyOne;
                        buildingManager.beeKeeper[row.myIndex].beeContainers[3] = beeContainer;
                    }
                    if (index == 4)
                    {
                        beeContainer = buildingManager.beeKeeper[row.myIndex].beeContainers[index];
                        beeContainer.totalBee = row.totalBeeOne;
                        beeContainer.totalHoney = row.totalHoneyOne;
                        buildingManager.beeKeeper[row.myIndex].beeContainers[4] = beeContainer;
                    }
                }
                Destroy(g.gameObject);
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    int index = i;
                    beeKeeper.beeContainers.Add(new CustomType.BeeContainer());
                    CustomType.BeeContainer container = beeKeeper.beeContainers[index];
                    if (index == 0)
                    {
                        container.totalBee = row.totalBeeOne;
                        container.totalHoney = row.totalHoneyOne;
                        beeKeeper.beeContainers[0] = container;
                    }
                    if (index == 1)
                    {
                        container.totalBee = row.totalBeeTwo;
                        container.totalHoney = row.totalHoneyTwo;
                        beeKeeper.beeContainers[1] = container;
                    }
                    if (index == 2)
                    {
                        container.totalBee = row.totalBeeThree;
                        container.totalHoney = row.totalHoneyThree;
                        beeKeeper.beeContainers[2] = container;
                    }
                    if (index == 3)
                    {
                        container.totalBee = row.totalBeeFour;
                        container.totalHoney = row.totalHoneyFour;
                        beeKeeper.beeContainers[3] = container;
                    }
                    if (index == 4)
                    {
                        container.totalBee = row.totalBeeFive;
                        container.totalHoney = row.totalHoneyFive;
                        beeKeeper.beeContainers[4] = container;
                    }
                }

                NetworkServer.Spawn(g);
            }
        }

    }
    #endregion

    #region BuildingCraft
    public void SaveBuildingCraft(string scene, List<BuildingCraft> buildingCraft)
    {
        connection.Execute("DELETE FROM buildingCraft");
        connection.Execute("DELETE FROM buildingCraftItem");
        // note: .Insert causes a 'Constraint' exception. use Replace.
        for (int i = 0; i < buildingCraft.Count; i++)
        {
            int mainIndex = i;
            Building building = buildingCraft[mainIndex].GetComponent<Building>();
            //Npc npc = buildingCraft[i].GetComponent<Npc>();
            connection.InsertOrReplace(new buildingCraft
            {
                sceneName = scene,
                craftBuildingType = building.building.name,
                myIndex = mainIndex,
                guild = building.guild,
                owner = building.owner,
                positionX = building.transform.position.x,
                positionY = building.transform.position.y,
                level = building.level,
                isPremium = Convert.ToInt32(building.isPremiumZone)
            });
            for (int e = 0; e < buildingCraft[mainIndex].craftItem.Count; e++)
            {
                int index = e;
                connection.InsertOrReplace(new buildingCraftItem
                {
                    myIndex = mainIndex,
                    itemName = buildingCraft[mainIndex].craftItem[index].itemName,
                    amount = buildingCraft[mainIndex].craftItem[index].amount,
                    remainingTime = buildingCraft[mainIndex].craftItem[index].remainingTime,
                    totalTime = buildingCraft[mainIndex].craftItem[index].totalTime,
                    timeBegin = buildingCraft[mainIndex].craftItem[index].timeBegin.ToString(),
                    timeEnd = buildingCraft[mainIndex].craftItem[index].timeEnd.ToString(),
                    timeEndServer = buildingCraft[mainIndex].craftItem[index].timeEnd.ToString(),
                    owner = buildingCraft[mainIndex].craftItem[index].owner,
                    guildName = buildingCraft[mainIndex].craftItem[index].guildName
                });
            }
        }
    }

    public void LoadBuildingCraft(List<GameObject> prefab, string scene)
    {
        foreach (buildingCraft row in connection.Query<buildingCraft>("SELECT * FROM buildingCraft WHERE sceneName=?", scene))
        {
            for (int i = 0; i < prefab.Count; i++)
            {
                int index = i;
                if (prefab[index].name == row.craftBuildingType)
                {
                    GameObject g = Instantiate(prefab[index]);
                    Building building = g.GetComponent<Building>();
                    BuildingCraft buildingCraft = g.GetComponent<BuildingCraft>();
                    g.transform.position = new Vector3(row.positionX, row.positionY, 0.0f);
                    building.health = row.health;
                    building.guild = row.guild;
                    building.owner = row.owner;
                    building.level = row.level;
                    building.isPremiumZone = Convert.ToBoolean(row.isPremium);

                    if (!building.isPremiumZone) buildingManager.AddToList(g);

                    if (building.isPremiumZone)
                    {
                        foreach (buildingCraftItem row2 in connection.Query<buildingCraftItem>("SELECT * FROM buildingCraftItem WHERE myIndex=?", row.myIndex))
                        {
                            CustomType.CraftItem item = new CustomType.CraftItem();

                            item.itemName = row2.itemName;
                            item.amount = row2.amount;
                            item.totalTime = row2.totalTime;
                            item.owner = row2.owner;
                            item.guildName = row2.guildName;
                            item.remainingTime = row2.remainingTime;
                            item.timeBegin = row2.timeBegin.ToString();
                            item.timeEnd = row2.timeEnd.ToString();
                            item.timeEndServer = row2.timeEndServer.ToString();

                            if (System.DateTime.Now >= DateTime.Parse(item.timeEndServer))
                            {
                                buildingManager.buildingCrafts[row.myIndex].allFinishedItem.Add(item);
                            }
                            else
                            {
                                buildingManager.buildingCrafts[row.myIndex].craftItem.Add(item);
                            }
                        }
                        Destroy(g);
                    }
                    else
                    {
                        foreach (buildingCraftItem row2 in connection.Query<buildingCraftItem>("SELECT * FROM buildingCraftItem WHERE myIndex=?", row.myIndex))
                        {

                            CustomType.CraftItem item = new CustomType.CraftItem();

                            item.itemName = row2.itemName;
                            item.amount = row2.amount;
                            item.totalTime = row2.totalTime;
                            item.owner = row2.owner;
                            item.guildName = row2.guildName;
                            item.remainingTime = row2.remainingTime;
                            item.timeBegin = row2.timeBegin.ToString();
                            item.timeEnd = row2.timeEnd.ToString();
                            item.timeEndServer = row2.timeEndServer.ToString();

                            buildingManager.buildingCrafts[row.myIndex].craftItem.Add(item);
                        }
                        NetworkServer.Spawn(g);
                    }
                }
            }
        }
    }
    #endregion

    #region Campfire
    public void SaveCampfire(string scene, List<Campfire> campfires)
    {
        connection.Execute("DELETE FROM campfire");
        connection.Execute("DELETE FROM campfireItems");
        // note: .Insert causes a 'Constraint' exception. use Replace.
        for (int i = 0; i < campfires.Count; i++)
        {
            Building building = campfires[i].GetComponent<Building>();
            //Npc npc = campfires[i].GetComponent<Npc>();
            Campfire campfire = campfires[i].GetComponent<Campfire>();
            connection.InsertOrReplace(new campfire
            {
                sceneName = scene,
                myIndex = i,
                guild = building.guild,
                owner = building.owner,
                positionX = building.transform.position.x,
                positionY = building.transform.position.y,
                level = building.level,
                currentWood = campfire.currentWood,
                active = Convert.ToInt32(campfire.active),
                isPremium = Convert.ToInt32(building.isPremiumZone)

            });
            for (int e = 0; e < campfire.items.Count; e++)
            {
                if (campfire.items[e].amount == 0) continue;
                // note: .Insert causes a 'Constraint' exception. use Replace.
                connection.InsertOrReplace(new campfireItems
                {
                    myIndex = i,
                    amount = campfire.items[e].amount,
                    slot = e,
                    name = campfire.items[e].item.name,
                    summonHealth = campfire.items[e].item.summonedHealth,
                    summonedLevel = campfire.items[e].item.summonedLevel,
                    summonedExperience = campfire.items[e].item.summonedExperience,
                    currentArmor = campfire.items[e].item.currentArmor,
                    currentUnsanity = campfire.items[e].item.currentUnsanity,
                    alreadyShooted = campfire.items[e].item.alreadyShooted,
                    totalAlreadyShooted = campfire.items[e].item.totalAlreadyShooted,
                    radioCurrentBattery = campfire.items[e].item.radioCurrentBattery,
                    torchCurrentBattery = campfire.items[e].item.torchCurrentBattery,
                    durability = campfire.items[e].item.durability,
                    weight = campfire.items[e].item.weight,
                    accuracyLevel = campfire.items[e].item.accuracyLevel,
                    missLevel = campfire.items[e].item.missLevel,
                    armorLevel = campfire.items[e].item.armorLevel,
                    chargeLevel = campfire.items[e].item.chargeLevel,
                    batteryLevel = campfire.items[e].item.batteryLevel,
                    weightLevel = campfire.items[e].item.weightLevel,
                    durabilityLevel = campfire.items[e].item.durabilityLevel,
                    unsanityLevel = campfire.items[e].item.unsanityLevel,
                    bagLevel = campfire.items[e].item.bagLevel,
                    gasolineContainer = campfire.items[e].item.gasolineContainer,
                    honeyContainer = campfire.items[e].item.honeyContainer,
                    waterContainer = campfire.items[e].item.waterContainer,
                    cookCountdown = campfire.items[e].item.cookCountdown,
                    wet = campfire.items[e].item.wet
                }); ;
            }

        }
    }
    public void LoadCampfire(GameObject prefab, string scene)
    {
        foreach (campfire row in connection.Query<campfire>("SELECT * FROM campfire WHERE sceneName=?", scene))
        {
            GameObject g = Instantiate(prefab);
            Building building = g.GetComponent<Building>();
            BuildingCraft buildingCraft = g.GetComponent<BuildingCraft>();
            Campfire campfire = g.GetComponent<Campfire>();
            g.transform.position = new Vector3(row.positionX, row.positionY, 0.0f);
            building.health = row.health;
            building.guild = row.guild;
            building.owner = row.owner;
            building.level = row.level;
            building.isPremiumZone = Convert.ToBoolean(row.isPremium);
            campfire.active = Convert.ToBoolean(row.active);

            if (!building.isPremiumZone) buildingManager.AddToList(g);

            if (building.isPremiumZone)
            {
                foreach (campfireItems row2 in connection.Query<campfireItems>("SELECT * FROM campfireItems WHERE myIndex=?", row.myIndex))
                {
                    if (ScriptableItem.dict.TryGetValue(row2.name.GetStableHashCode(), out ScriptableItem itemData))
                    {
                        ItemSlot slot = new ItemSlot();
                        Item item = new Item(itemData);
                        slot.amount = 1;
                        item.summonedHealth = row2.summonHealth;
                        item.summonedLevel = row2.summonedLevel;
                        item.summonedExperience = row2.summonedExperience;
                        item.accuracyLevel = row2.accuracyLevel;
                        item.missLevel = row2.missLevel;
                        item.armorLevel = row2.armorLevel;
                        item.chargeLevel = row2.chargeLevel;
                        item.batteryLevel = row2.batteryLevel;
                        item.weightLevel = row2.weightLevel;
                        item.durabilityLevel = row2.durabilityLevel;
                        item.unsanityLevel = row2.unsanityLevel;
                        item.bagLevel = row2.bagLevel;
                        item.currentArmor = row2.currentArmor;
                        item.alreadyShooted = row2.alreadyShooted;
                        item.totalAlreadyShooted = row2.totalAlreadyShooted;
                        item.durability = row2.durability;
                        item.currentUnsanity = row2.currentUnsanity;
                        item.radioCurrentBattery = row2.radioCurrentBattery;
                        item.torchCurrentBattery = row2.torchCurrentBattery;
                        item.weight = row2.weight;
                        item.gasolineContainer = row2.gasolineContainer;
                        item.honeyContainer = row2.honeyContainer;
                        item.waterContainer = row2.waterContainer;
                        item.cookCountdown = row2.cookCountdown;
                        item.wet = row2.wet;
                        slot.item = item;
                        buildingManager.campfires[row.myIndex].items.Add(slot);
                    }
                }
                Destroy(g.gameObject);
            }
            else
            {
                foreach (campfireItems row2 in connection.Query<campfireItems>("SELECT * FROM campfireItems WHERE myIndex=?", row.myIndex))
                {
                    if (ScriptableItem.dict.TryGetValue(row2.name.GetStableHashCode(), out ScriptableItem itemData))
                    {
                        ItemSlot slot = new ItemSlot();
                        Item item = new Item(itemData);
                        slot.amount = 1;
                        item.summonedHealth = row2.summonHealth;
                        item.summonedLevel = row2.summonedLevel;
                        item.summonedExperience = row2.summonedExperience;
                        item.accuracyLevel = row2.accuracyLevel;
                        item.missLevel = row2.missLevel;
                        item.armorLevel = row2.armorLevel;
                        item.chargeLevel = row2.chargeLevel;
                        item.batteryLevel = row2.batteryLevel;
                        item.weightLevel = row2.weightLevel;
                        item.durabilityLevel = row2.durabilityLevel;
                        item.unsanityLevel = row2.unsanityLevel;
                        item.bagLevel = row2.bagLevel;
                        item.currentArmor = row2.currentArmor;
                        item.alreadyShooted = row2.alreadyShooted;
                        item.totalAlreadyShooted = row2.totalAlreadyShooted;
                        item.durability = row2.durability;
                        item.currentUnsanity = row2.currentUnsanity;
                        item.radioCurrentBattery = row2.radioCurrentBattery;
                        item.torchCurrentBattery = row2.torchCurrentBattery;
                        item.weight = row2.weight;
                        item.gasolineContainer = row2.gasolineContainer;
                        item.honeyContainer = row2.honeyContainer;
                        item.waterContainer = row2.waterContainer;
                        item.cookCountdown = row2.cookCountdown;
                        item.wet = row2.wet;
                        slot.item = item;
                        campfire.items.Add(slot);
                    }
                }

                //if (campfire.active == false && building.isPremiumZone == false)
                //    g.SetActive(false);
                NetworkServer.Spawn(g);
            }
        }
    }

    #endregion

    #region Dynmaite
    public void SaveDynamite(string scene, List<Dynamite> dynamite)
    {
        connection.Execute("DELETE FROM dynamite");
        // note: .Insert causes a 'Constraint' exception. use Replace.
        for (int i = 0; i < dynamite.Count; i++)
        {
            Building building = dynamite[i].GetComponent<Building>();
            //Npc npc = dynamite[i].GetComponent<Npc>();
            connection.InsertOrReplace(new dynamite
            {
                sceneName = scene,
                health = building.health,
                guild = building.guild,
                owner = building.owner,
                positionX = building.transform.position.x,
                positionY = building.transform.position.y,
                level = building.level,
                isPremium = Convert.ToInt32(building.isPremiumZone)
            });
        }
    }

    public void LoadDynamite(GameObject prefab, string scene)
    {
        foreach (dynamite row in connection.Query<dynamite>("SELECT * FROM dynamite WHERE sceneName=?", scene))
        {
            GameObject g = Instantiate(prefab);
            Building building = g.GetComponent<Building>();
            //Npc npc = g.GetComponent<Npc>();
            Dynamite dynamite = g.GetComponent<Dynamite>();
            g.transform.position = new Vector3(row.positionX, row.positionY, 0.0f);
            building.health = row.health;
            building.guild = row.guild;
            building.owner = row.owner;
            building.level = row.level;
            building.isPremiumZone = Convert.ToBoolean(row.isPremium);

            if (!building.isPremiumZone) buildingManager.AddToList(g);

            if (!building.isPremiumZone)
            {
                buildingManager.AddToList(g);
                //g.SetActive(false);
                NetworkServer.Spawn(g);
            }
            else
            {
                buildingManager.dynamites[row.myIndex] = dynamite;
                Destroy(g);
            }
        }

    }

    #endregion

    #region Mine
    public void SaveMine(string scene, List<Mine> mine)
    {
        connection.Execute("DELETE FROM mine");
        // note: .Insert causes a 'Constraint' exception. use Replace.
        for (int i = 0; i < mine.Count; i++)
        {
            Building building = mine[i].GetComponent<Building>();
            //Npc npc = mine[i].GetComponent<Npc>();
            connection.InsertOrReplace(new mine
            {
                sceneName = scene,
                health = building.health,
                guild = building.guild,
                owner = building.owner,
                positionX = building.transform.position.x,
                positionY = building.transform.position.y,
                level = building.level,
                isPremium = Convert.ToInt32(building.isPremiumZone)
            }); ;
        }
    }

    public void LoadMine(GameObject prefab, string scene)
    {
        foreach (mine row in connection.Query<mine>("SELECT * FROM mine WHERE sceneName=?", scene))
        {
            GameObject g = Instantiate(prefab);
            Building building = g.GetComponent<Building>();
            //Npc npc = g.GetComponent<Npc>();
            Mine mine = building.GetComponent<Mine>();
            g.transform.position = new Vector3(row.positionX, row.positionY, 0.0f);
            building.health = row.health;
            building.guild = row.guild;
            building.owner = row.owner;
            building.level = row.level;
            building.isPremiumZone = Convert.ToBoolean(row.isPremium);

            if (!building.isPremiumZone) buildingManager.AddToList(g);

            if (!building.isPremiumZone)
            {
                //g.SetActive(false);
                NetworkServer.Spawn(g);
            }
            else
            {
                buildingManager.mines[row.myIndex] = mine;
                Destroy(g);
            }
        }

    }

    #endregion

    #region WoodWall
    public void SaveWoodWall(string scene, List<WoodWall> woodWall)
    {
        connection.Execute("DELETE FROM woodWall");
        // note: .Insert causes a 'Constraint' exception. use Replace.
        for (int i = 0; i < woodWall.Count; i++)
        {
            Building building = woodWall[i].GetComponent<Building>();
            //Npc npc = woodWall[i].GetComponent<Npc>();
            connection.InsertOrReplace(new woodWall
            {
                sceneName = scene,
                health = building.health,
                guild = building.guild,
                owner = building.owner,
                positionX = building.transform.position.x,
                positionY = building.transform.position.y,
                level = building.level,
                isPremium = Convert.ToInt32(building.isPremiumZone),
                side = building.GetComponent<WoodWall>().side

            });
        }
    }

    public void LoadWoodWall(GameObject prefab, string scene)
    {
        foreach (woodWall row in connection.Query<woodWall>("SELECT * FROM woodWall WHERE sceneName=?", scene))
        {
            GameObject g = Instantiate(buildingManager.woodWallsObject[row.side]);
            Building building = g.GetComponent<Building>();
            //Npc npc = g.GetComponent<Npc>();
            WoodWall woodWall = building.GetComponent<WoodWall>();
            g.transform.position = new Vector3(row.positionX, row.positionY, 0.0f);
            building.health = row.health;
            building.guild = row.guild;
            building.owner = row.owner;
            building.level = row.level;
            building.isPremiumZone = Convert.ToBoolean(row.isPremium);
            woodWall.side = row.side;

            if (!building.isPremiumZone) buildingManager.AddToList(g);

            if (!building.isPremiumZone)
            {
                //g.SetActive(false);
                NetworkServer.Spawn(g);
            }
            else
            {
                buildingManager.woodWalls[row.myIndex] = woodWall;
                Destroy(g);
            }
        }
    }

    #endregion

    #region Barbwire
    public void SaveBarbwire(string scene, List<Barbwire> woodWall)
    {
        connection.Execute("DELETE FROM Barbwire");
        // note: .Insert causes a 'Constraint' exception. use Replace.
        for (int i = 0; i < woodWall.Count; i++)
        {
            Building building = woodWall[i].GetComponent<Building>();
            //Npc npc = woodWall[i].GetComponent<Npc>();
            connection.InsertOrReplace(new barbwire
            {
                sceneName = scene,
                health = building.health,
                guild = building.guild,
                owner = building.owner,
                positionX = building.transform.position.x,
                positionY = building.transform.position.y,
                level = building.level,
                isPremium = Convert.ToInt32(building.isPremiumZone),
                side = building.GetComponent<Barbwire>().side

            });
        }
    }

    public void LoadBarbwire(GameObject prefab, string scene)
    {
        foreach (barbwire row in connection.Query<barbwire>("SELECT * FROM barbwire WHERE sceneName=?", scene))
        {
            GameObject g = Instantiate(buildingManager.barbwiresObject[row.side]);
            Building building = g.GetComponent<Building>();
            //Npc npc = g.GetComponent<Npc>();
            Barbwire woodWall = building.GetComponent<Barbwire>();
            g.transform.position = new Vector3(row.positionX, row.positionY, 0.0f);
            building.health = row.health;
            building.guild = row.guild;
            building.owner = row.owner;
            building.level = row.level;
            building.isPremiumZone = Convert.ToBoolean(row.isPremium);
            woodWall.side = row.side;

            if (!building.isPremiumZone) buildingManager.AddToList(g);

            if (!building.isPremiumZone)
            {
                //g.SetActive(false);
                NetworkServer.Spawn(g);
            }
            else
            {
                buildingManager.barbWires[row.myIndex] = woodWall;
                Destroy(g);
            }
        }
    }

    #endregion

    #region Totem
    public void SaveTotem(string scene, List<Totem> totem)
    {
        connection.Execute("DELETE FROM totem");
        // note: .Insert causes a 'Constraint' exception. use Replace.
        for (int i = 0; i < totem.Count; i++)
        {
            Building building = totem[i].GetComponent<Building>();
            //Npc npc = totem[i].GetComponent<Npc>();
            connection.InsertOrReplace(new totem
            {
                sceneName = scene,
                health = building.health,
                guild = building.guild,
                owner = building.owner,
                positionX = building.transform.position.x,
                positionY = building.transform.position.y,
                level = building.level,
                message = building.GetComponent<Totem>().message,
                isPremium = Convert.ToInt32(building.isPremiumZone)

            });
        }
    }
    public void LoadTotem(GameObject prefab, string scene)
    {
        foreach (totem row in connection.Query<totem>("SELECT * FROM totem WHERE sceneName=?", scene))
        {
            GameObject g = Instantiate(prefab);
            Building building = g.GetComponent<Building>();
            //Npc npc = g.GetComponent<Npc>();
            Totem totem = building.GetComponent<Totem>();
            g.transform.position = new Vector3(row.positionX, row.positionY, 0.0f);
            building.health = row.health;
            building.guild = row.guild;
            building.owner = row.owner;
            building.level = row.level;
            totem.message = row.message;
            building.isPremiumZone = Convert.ToBoolean(row.isPremium);

            if (!building.isPremiumZone) buildingManager.AddToList(g);

            if (building.isPremiumZone)
            {
                buildingManager.totems[row.myIndex] = totem;
                Destroy(g);
            }
            else
            {
                //g.SetActive(false);
                NetworkServer.Spawn(g);
            }


        }
    }

    #endregion

    #region BuildingUpgradeRepair
    public void SaveUpgradeRepair(string scene, List<BuildingUpgradeRepair> upgradeRepair)
    {
        connection.Execute("DELETE FROM upgradeRepair");
        connection.Execute("DELETE FROM upgradeItems");
        connection.Execute("DELETE FROM repairItems");
        connection.Execute("DELETE FROM upgradeItemsFinished");
        connection.Execute("DELETE FROM repairItemsFinished");
        // note: .Insert causes a 'Constraint' exception. use Replace.
        for (int i = 0; i < upgradeRepair.Count; i++)
        {
            Building building = upgradeRepair[i].GetComponent<Building>();
            //Npc npc = upgradeRepair[i].GetComponent<Npc>();
            connection.InsertOrReplace(new upgradeRepair
            {
                sceneName = scene,
                myIndex = i,
                guild = building.guild,
                owner = building.owner,
                positionX = building.transform.position.x,
                positionY = building.transform.position.y,
                level = building.level,
                health = building.health,
                isPremium = Convert.ToInt32(building.isPremiumZone)
            });
            for (int e = 0; e < upgradeRepair[i].upgradeItem.Count; e++)
            {
                // note: .Insert causes a 'Constraint' exception. use Replace.
                connection.InsertOrReplace(new upgradeItems
                {
                    myIndex = i,
                    amount = upgradeRepair[i].upgradeItem[e].item.amount,
                    slot = e,
                    name = upgradeRepair[i].upgradeItem[e].item.item.name,
                    summonHealth = upgradeRepair[i].upgradeItem[e].item.item.summonedHealth,
                    summonedLevel = upgradeRepair[i].upgradeItem[e].item.item.summonedLevel,
                    summonedExperience = upgradeRepair[i].upgradeItem[e].item.item.summonedExperience,
                    currentArmor = upgradeRepair[i].upgradeItem[e].item.item.currentArmor,
                    currentUnsanity = upgradeRepair[i].upgradeItem[e].item.item.currentUnsanity,
                    alreadyShooted = upgradeRepair[i].upgradeItem[e].item.item.alreadyShooted,
                    totalAlreadyShooted = upgradeRepair[i].upgradeItem[e].item.item.totalAlreadyShooted,
                    radioCurrentBattery = upgradeRepair[i].upgradeItem[e].item.item.radioCurrentBattery,
                    torchCurrentBattery = upgradeRepair[i].upgradeItem[e].item.item.torchCurrentBattery,
                    durability = upgradeRepair[i].upgradeItem[e].item.item.durability,
                    weight = upgradeRepair[i].upgradeItem[e].item.item.weight,
                    accuracyLevel = upgradeRepair[i].upgradeItem[e].item.item.accuracyLevel,
                    missLevel = upgradeRepair[i].upgradeItem[e].item.item.missLevel,
                    armorLevel = upgradeRepair[i].upgradeItem[e].item.item.armorLevel,
                    chargeLevel = upgradeRepair[i].upgradeItem[e].item.item.chargeLevel,
                    batteryLevel = upgradeRepair[i].upgradeItem[e].item.item.batteryLevel,
                    weightLevel = upgradeRepair[i].upgradeItem[e].item.item.weightLevel,
                    durabilityLevel = upgradeRepair[i].upgradeItem[e].item.item.durabilityLevel,
                    unsanityLevel = upgradeRepair[i].upgradeItem[e].item.item.unsanityLevel,
                    bagLevel = upgradeRepair[i].upgradeItem[e].item.item.bagLevel,
                    gasolineContainer = upgradeRepair[i].upgradeItem[e].item.item.gasolineContainer,
                    honeyContainer = upgradeRepair[i].upgradeItem[e].item.item.honeyContainer,
                    waterContainer = upgradeRepair[i].upgradeItem[e].item.item.waterContainer,
                    cookCountdown = upgradeRepair[i].upgradeItem[e].item.item.cookCountdown,
                    wet = upgradeRepair[i].upgradeItem[e].item.item.wet,
                    playerName = upgradeRepair[i].upgradeItem[e].playerName,
                    remainingTime = upgradeRepair[i].upgradeItem[e].remainingTime,
                    totalTime = upgradeRepair[i].upgradeItem[e].totalTime,
                    timeBegin = upgradeRepair[i].upgradeItem[e].timeBegin,
                    timeEnd = upgradeRepair[i].upgradeItem[e].timeEnd,
                    type = upgradeRepair[i].upgradeItem[e].type
                });
            }
            for (int e = 0; e < upgradeRepair[i].repairItem.Count; e++)
            {
                // note: .Insert causes a 'Constraint' exception. use Replace.
                connection.InsertOrReplace(new repairItems
                {
                    myIndex = i,
                    amount = upgradeRepair[i].repairItem[e].item.amount,
                    slot = e,
                    name = upgradeRepair[i].repairItem[e].item.item.name,
                    summonHealth = upgradeRepair[i].repairItem[e].item.item.summonedHealth,
                    summonedLevel = upgradeRepair[i].repairItem[e].item.item.summonedLevel,
                    summonedExperience = upgradeRepair[i].repairItem[e].item.item.summonedExperience,
                    currentArmor = upgradeRepair[i].repairItem[e].item.item.currentArmor,
                    currentUnsanity = upgradeRepair[i].repairItem[e].item.item.currentUnsanity,
                    alreadyShooted = upgradeRepair[i].repairItem[e].item.item.alreadyShooted,
                    totalAlreadyShooted = upgradeRepair[i].repairItem[e].item.item.totalAlreadyShooted,
                    radioCurrentBattery = upgradeRepair[i].repairItem[e].item.item.radioCurrentBattery,
                    torchCurrentBattery = upgradeRepair[i].repairItem[e].item.item.torchCurrentBattery,
                    durability = upgradeRepair[i].repairItem[e].item.item.durability,
                    weight = upgradeRepair[i].repairItem[e].item.item.weight,
                    accuracyLevel = upgradeRepair[i].repairItem[e].item.item.accuracyLevel,
                    missLevel = upgradeRepair[i].repairItem[e].item.item.missLevel,
                    armorLevel = upgradeRepair[i].repairItem[e].item.item.armorLevel,
                    chargeLevel = upgradeRepair[i].repairItem[e].item.item.chargeLevel,
                    batteryLevel = upgradeRepair[i].repairItem[e].item.item.batteryLevel,
                    weightLevel = upgradeRepair[i].repairItem[e].item.item.weightLevel,
                    durabilityLevel = upgradeRepair[i].repairItem[e].item.item.durabilityLevel,
                    unsanityLevel = upgradeRepair[i].repairItem[e].item.item.unsanityLevel,
                    bagLevel = upgradeRepair[i].repairItem[e].item.item.bagLevel,
                    gasolineContainer = upgradeRepair[i].repairItem[e].item.item.gasolineContainer,
                    honeyContainer = upgradeRepair[i].repairItem[e].item.item.honeyContainer,
                    waterContainer = upgradeRepair[i].repairItem[e].item.item.waterContainer,
                    cookCountdown = upgradeRepair[i].repairItem[e].item.item.cookCountdown,
                    wet = upgradeRepair[i].repairItem[e].item.item.wet,
                    playerName = upgradeRepair[i].repairItem[e].playerName,
                    remainingTime = upgradeRepair[i].repairItem[e].remainingTime,
                    totalTime = upgradeRepair[i].repairItem[e].totalTime,
                    timeBegin = upgradeRepair[i].repairItem[e].timeBegin,
                    timeEnd = upgradeRepair[i].repairItem[e].timeEnd,
                    type = upgradeRepair[i].repairItem[e].type
                });
            }
        }
    }

    public void LoadUpgradeRepair(GameObject prefab, string scene)
    {
        foreach (upgradeRepair row in connection.Query<upgradeRepair>("SELECT * FROM upgradeRepair WHERE sceneName=?", scene))
        {
            GameObject g = Instantiate(prefab);
            Building building = g.GetComponent<Building>();
            BuildingUpgradeRepair buildingUpgradeRepair = g.GetComponent<BuildingUpgradeRepair>();
            g.transform.position = new Vector3(row.positionX, row.positionY, 0.0f);
            building.health = row.health;
            building.guild = row.guild;
            building.owner = row.owner;
            building.level = row.level;
            building.isPremiumZone = Convert.ToBoolean(row.isPremium);

            foreach (upgradeItems row2 in connection.Query<upgradeItems>("SELECT * FROM upgradeItems WHERE myIndex=?", row.myIndex))
            {
                if (building.isPremiumZone)
                {
                    if (ScriptableItem.dict.TryGetValue(row2.name.GetStableHashCode(), out ScriptableItem itemData))
                    {
                        CustomType.UpgradeRepairItem upgradeItem = new CustomType.UpgradeRepairItem();
                        upgradeItem.item = new ItemSlot();
                        upgradeItem.item.item = new Item(itemData);

                        upgradeItem.item.item.summonedHealth = row2.summonHealth;
                        upgradeItem.item.item.summonedLevel = row2.summonedLevel;
                        upgradeItem.item.item.summonedExperience = row2.summonedExperience;
                        upgradeItem.item.item.accuracyLevel = row2.accuracyLevel;
                        upgradeItem.item.item.missLevel = row2.missLevel;
                        upgradeItem.item.item.armorLevel = row2.armorLevel;
                        upgradeItem.item.item.chargeLevel = row2.chargeLevel;
                        upgradeItem.item.item.batteryLevel = row2.batteryLevel;
                        upgradeItem.item.item.weightLevel = row2.weightLevel;
                        upgradeItem.item.item.durabilityLevel = row2.durabilityLevel;
                        upgradeItem.item.item.unsanityLevel = row2.unsanityLevel;
                        upgradeItem.item.item.bagLevel = row2.bagLevel;
                        upgradeItem.item.item.currentArmor = row2.currentArmor;
                        upgradeItem.item.item.alreadyShooted = row2.alreadyShooted;
                        upgradeItem.item.item.totalAlreadyShooted = row2.totalAlreadyShooted;
                        upgradeItem.item.item.durability = row2.durability;
                        upgradeItem.item.item.currentUnsanity = row2.currentUnsanity;
                        upgradeItem.item.item.radioCurrentBattery = row2.radioCurrentBattery;
                        upgradeItem.item.item.torchCurrentBattery = row2.torchCurrentBattery;
                        upgradeItem.item.item.weight = row2.weight;
                        upgradeItem.item.item.gasolineContainer = row2.gasolineContainer;
                        upgradeItem.item.item.honeyContainer = row2.honeyContainer;
                        upgradeItem.item.item.waterContainer = row2.waterContainer;
                        upgradeItem.item.item.cookCountdown = row2.cookCountdown;
                        upgradeItem.item.item.wet = row2.wet;
                        upgradeItem.playerName = row2.playerName;
                        upgradeItem.remainingTime = row2.remainingTime;
                        upgradeItem.totalTime = row2.totalTime;
                        upgradeItem.type = row2.type;
                        upgradeItem.timeBegin = row2.timeBegin;
                        upgradeItem.timeEnd = row2.timeEnd;

                        buildingManager.upgradeRepair[row.myIndex].upgradeItem.Add(upgradeItem);
                    }
                }
                else
                {
                    if (ScriptableItem.dict.TryGetValue(row2.name.GetStableHashCode(), out ScriptableItem itemData))
                    {
                        CustomType.UpgradeRepairItem upgradeItem = new CustomType.UpgradeRepairItem();
                        upgradeItem.item = new ItemSlot();
                        upgradeItem.item.item = new Item(itemData);

                        upgradeItem.item.item.summonedHealth = row2.summonHealth;
                        upgradeItem.item.item.summonedLevel = row2.summonedLevel;
                        upgradeItem.item.item.summonedExperience = row2.summonedExperience;
                        upgradeItem.item.item.accuracyLevel = row2.accuracyLevel;
                        upgradeItem.item.item.missLevel = row2.missLevel;
                        upgradeItem.item.item.armorLevel = row2.armorLevel;
                        upgradeItem.item.item.chargeLevel = row2.chargeLevel;
                        upgradeItem.item.item.batteryLevel = row2.batteryLevel;
                        upgradeItem.item.item.weightLevel = row2.weightLevel;
                        upgradeItem.item.item.durabilityLevel = row2.durabilityLevel;
                        upgradeItem.item.item.unsanityLevel = row2.unsanityLevel;
                        upgradeItem.item.item.bagLevel = row2.bagLevel;
                        upgradeItem.item.item.currentArmor = row2.currentArmor;
                        upgradeItem.item.item.alreadyShooted = row2.alreadyShooted;
                        upgradeItem.item.item.totalAlreadyShooted = row2.totalAlreadyShooted;
                        upgradeItem.item.item.durability = row2.durability;
                        upgradeItem.item.item.currentUnsanity = row2.currentUnsanity;
                        upgradeItem.item.item.radioCurrentBattery = row2.radioCurrentBattery;
                        upgradeItem.item.item.torchCurrentBattery = row2.torchCurrentBattery;
                        upgradeItem.item.item.weight = row2.weight;
                        upgradeItem.item.item.gasolineContainer = row2.gasolineContainer;
                        upgradeItem.item.item.honeyContainer = row2.honeyContainer;
                        upgradeItem.item.item.waterContainer = row2.waterContainer;
                        upgradeItem.item.item.cookCountdown = row2.cookCountdown;
                        upgradeItem.item.item.wet = row2.wet;
                        upgradeItem.playerName = row2.playerName;
                        upgradeItem.remainingTime = row2.remainingTime;
                        upgradeItem.totalTime = row2.totalTime;
                        upgradeItem.type = row2.type;
                        upgradeItem.timeBegin = row2.timeBegin;
                        upgradeItem.timeEnd = row2.timeEnd;
                        buildingUpgradeRepair.upgradeItem.Add(upgradeItem);
                    }
                }
            }
            foreach (repairItems row3 in connection.Query<repairItems>("SELECT * FROM repairItems WHERE myIndex=?", row.myIndex))
            {
                if (building.isPremiumZone)
                {
                    if (ScriptableItem.dict.TryGetValue(row3.name.GetStableHashCode(), out ScriptableItem itemData))
                    {
                        CustomType.UpgradeRepairItem upgradeItem = new CustomType.UpgradeRepairItem();
                        upgradeItem.item = new ItemSlot();
                        upgradeItem.item.item = new Item(itemData);

                        upgradeItem.item.item.summonedHealth = row3.summonHealth;
                        upgradeItem.item.item.summonedLevel = row3.summonedLevel;
                        upgradeItem.item.item.summonedExperience = row3.summonedExperience;
                        upgradeItem.item.item.accuracyLevel = row3.accuracyLevel;
                        upgradeItem.item.item.missLevel = row3.missLevel;
                        upgradeItem.item.item.armorLevel = row3.armorLevel;
                        upgradeItem.item.item.chargeLevel = row3.chargeLevel;
                        upgradeItem.item.item.batteryLevel = row3.batteryLevel;
                        upgradeItem.item.item.weightLevel = row3.weightLevel;
                        upgradeItem.item.item.durabilityLevel = row3.durabilityLevel;
                        upgradeItem.item.item.unsanityLevel = row3.unsanityLevel;
                        upgradeItem.item.item.bagLevel = row3.bagLevel;
                        upgradeItem.item.item.currentArmor = row3.currentArmor;
                        upgradeItem.item.item.alreadyShooted = row3.alreadyShooted;
                        upgradeItem.item.item.totalAlreadyShooted = row3.totalAlreadyShooted;
                        upgradeItem.item.item.durability = row3.durability;
                        upgradeItem.item.item.currentUnsanity = row3.currentUnsanity;
                        upgradeItem.item.item.radioCurrentBattery = row3.radioCurrentBattery;
                        upgradeItem.item.item.torchCurrentBattery = row3.torchCurrentBattery;
                        upgradeItem.item.item.weight = row3.weight;
                        upgradeItem.item.item.gasolineContainer = row3.gasolineContainer;
                        upgradeItem.item.item.honeyContainer = row3.honeyContainer;
                        upgradeItem.item.item.waterContainer = row3.waterContainer;
                        upgradeItem.item.item.cookCountdown = row3.cookCountdown;
                        upgradeItem.item.item.wet = row3.wet;
                        upgradeItem.playerName = row3.playerName;
                        upgradeItem.remainingTime = row3.remainingTime;
                        upgradeItem.totalTime = row3.totalTime;
                        upgradeItem.type = row3.type;
                        upgradeItem.timeBegin = row3.timeBegin;
                        upgradeItem.timeEnd = row3.timeEnd;
                        buildingManager.upgradeRepair[row.myIndex].repairItem.Add(upgradeItem);
                    }
                }
                else
                {
                    if (ScriptableItem.dict.TryGetValue(row3.name.GetStableHashCode(), out ScriptableItem itemData))
                    {
                        CustomType.UpgradeRepairItem upgradeItem = new CustomType.UpgradeRepairItem();
                        upgradeItem.item = new ItemSlot();
                        upgradeItem.item.item = new Item(itemData);

                        upgradeItem.item.item.summonedHealth = row3.summonHealth;
                        upgradeItem.item.item.summonedLevel = row3.summonedLevel;
                        upgradeItem.item.item.summonedExperience = row3.summonedExperience;
                        upgradeItem.item.item.accuracyLevel = row3.accuracyLevel;
                        upgradeItem.item.item.missLevel = row3.missLevel;
                        upgradeItem.item.item.armorLevel = row3.armorLevel;
                        upgradeItem.item.item.chargeLevel = row3.chargeLevel;
                        upgradeItem.item.item.batteryLevel = row3.batteryLevel;
                        upgradeItem.item.item.weightLevel = row3.weightLevel;
                        upgradeItem.item.item.durabilityLevel = row3.durabilityLevel;
                        upgradeItem.item.item.unsanityLevel = row3.unsanityLevel;
                        upgradeItem.item.item.bagLevel = row3.bagLevel;
                        upgradeItem.item.item.currentArmor = row3.currentArmor;
                        upgradeItem.item.item.alreadyShooted = row3.alreadyShooted;
                        upgradeItem.item.item.totalAlreadyShooted = row3.totalAlreadyShooted;
                        upgradeItem.item.item.durability = row3.durability;
                        upgradeItem.item.item.currentUnsanity = row3.currentUnsanity;
                        upgradeItem.item.item.radioCurrentBattery = row3.radioCurrentBattery;
                        upgradeItem.item.item.torchCurrentBattery = row3.torchCurrentBattery;
                        upgradeItem.item.item.weight = row3.weight;
                        upgradeItem.item.item.gasolineContainer = row3.gasolineContainer;
                        upgradeItem.item.item.honeyContainer = row3.honeyContainer;
                        upgradeItem.item.item.waterContainer = row3.waterContainer;
                        upgradeItem.item.item.cookCountdown = row3.cookCountdown;
                        upgradeItem.item.item.wet = row3.wet;
                        upgradeItem.playerName = row3.playerName;
                        upgradeItem.remainingTime = row3.remainingTime;
                        upgradeItem.totalTime = row3.totalTime;
                        upgradeItem.type = row3.type;
                        upgradeItem.timeBegin = row3.timeBegin;
                        upgradeItem.timeEnd = row3.timeEnd;
                        buildingUpgradeRepair.repairItem.Add(upgradeItem);
                    }
                }
            }
            if (!building.isPremiumZone) buildingManager.AddToList(g);

            if (!building.isPremiumZone)
            {
                NetworkServer.Spawn(g);
            }
            else
            {
                buildingManager.upgradeRepair[row.myIndex] = buildingUpgradeRepair;
                Destroy(g);
            }
        }
    }
    #endregion

    #region PetTrainer
    public void SavePetTrainer(string scene, List<PetTrainer> petTrainers)
    {
        connection.Execute("DELETE FROM petTrainer");
        connection.Execute("DELETE FROM petTrainerInProgress");
        // note: .Insert causes a 'Constraint' exception. use Replace.
        for (int i = 0; i < petTrainers.Count; i++)
        {
            Building building = petTrainers[i].GetComponent<Building>();
            //Npc npc = petTrainers[i].GetComponent<Npc>();
            connection.InsertOrReplace(new petTrainer
            {
                sceneName = scene,
                myIndex = i,
                guild = building.guild,
                owner = building.owner,
                positionX = building.transform.position.x,
                positionY = building.transform.position.y,
                level = building.level,
                health = building.health,
                isPremium = Convert.ToInt32(building.isPremiumZone)

            });
            for (int e = 0; e < petTrainers[i].petTraining.Count; e++)
            {
                // note: .Insert causes a 'Constraint' exception. use Replace.
                connection.InsertOrReplace(new petTrainerInProgress
                {
                    myIndex = i,
                    petName = petTrainers[i].petTraining[e].petItem.item.name,
                    level = petTrainers[i].petTraining[e].petItem.item.summonedLevel,
                    health = petTrainers[i].petTraining[e].petItem.item.summonedHealth,
                    experience = petTrainers[i].petTraining[e].petItem.item.summonedExperience,
                    owner = petTrainers[i].petTraining[e].owner,
                    remainingTime = petTrainers[i].petTraining[e].remainingTimer,
                    timeBegin = petTrainers[i].petTraining[e].timeBegin,
                    timeEnd = petTrainers[i].petTraining[e].timeEnd

                });
            }
        }
    }

    public void LoadPetTrainer(GameObject prefab, string scene)
    {
        foreach (petTrainer row in connection.Query<petTrainer>("SELECT * FROM PetTrainer WHERE sceneName=?", scene))
        {
            GameObject g = Instantiate(prefab);
            Building building = g.GetComponent<Building>();
            PetTrainer buildingUpgradeRepair = g.GetComponent<PetTrainer>();
            g.transform.position = new Vector3(row.positionX, row.positionY, 0.0f);
            building.health = row.health;
            building.guild = row.guild;
            building.owner = row.owner;
            building.level = row.level;
            building.isPremiumZone = Convert.ToBoolean(row.isPremium);

            foreach (petTrainerInProgress row2 in connection.Query<petTrainerInProgress>("SELECT * FROM petTrainerInProgress WHERE myIndex=?", row.myIndex))
            {
                if (ScriptableItem.dict.TryGetValue(row2.petName.GetStableHashCode(), out ScriptableItem itemData))
                {
                    PetExp petExp = new PetExp();
                    ItemSlot petItem = new ItemSlot(new Item(itemData), 1);

                    petExp.owner = row2.owner;
                    petItem.item.summonedHealth = row2.health;
                    petItem.item.summonedExperience = row2.experience;
                    petItem.item.summonedLevel = row2.level;
                    petExp.petItem = petItem;
                    petExp.timeBegin = row2.timeBegin;
                    petExp.timeEnd = row2.timeEnd;

                    buildingUpgradeRepair.petTraining.Add(petExp);
                }
            }
            if (!building.isPremiumZone) buildingManager.AddToList(g);

            if (!building.isPremiumZone)
            {
                //if (buildingManager.petTrainers[row.myIndex].petTraining.Count == 0)
                NetworkServer.Spawn(g);
            }
            else
            {
                buildingManager.petTrainers[row.myIndex] = buildingUpgradeRepair;
                Destroy(g);
            }

        }
    }
    #endregion

    #region BuildingWaterWell
    public void SaveWaterWell(string scene, List<BuildingWaterWell> buildingWaterWell)
    {
        connection.Execute("DELETE FROM buildingWaterWell");
        // note: .Insert causes a 'Constraint' exception. use Replace.
        for (int i = 0; i < buildingWaterWell.Count; i++)
        {
            Building building = buildingWaterWell[i].GetComponent<Building>();
            //Npc npc = buildingWaterWell[i].GetComponent<Npc>();
            connection.InsertOrReplace(new buildingWaterWell
            {
                myIndex = i,
                sceneName = scene,
                health = building.health,
                guild = building.guild,
                owner = building.owner,
                positionX = building.transform.position.x,
                positionY = building.transform.position.y,
                level = building.level,
                currentWater = building.GetComponent<BuildingWaterWell>().currentWater,
                isPremium = Convert.ToInt32(building.isPremiumZone)

            }); ;
        }
    }

    public void LoadWaterWell(GameObject prefab, string scene)
    {
        foreach (buildingWaterWell row in connection.Query<buildingWaterWell>("SELECT * FROM buildingWaterWell WHERE sceneName=?", scene))
        {
            GameObject g = Instantiate(prefab);
            Building building = g.GetComponent<Building>();
            //Npc npc = g.GetComponent<Npc>();
            BuildingWaterWell buildingWaterWell = building.GetComponent<BuildingWaterWell>();
            g.transform.position = new Vector3(row.positionX, row.positionY, 0.0f);
            building.health = row.health;
            building.guild = row.guild;
            building.owner = row.owner;
            building.level = row.level;
            buildingWaterWell.currentWater = row.currentWater;
            building.isPremiumZone = Convert.ToBoolean(row.isPremium);

            if (!building.isPremiumZone) buildingManager.AddToList(g);

            if (!building.isPremiumZone)
            {
                NetworkServer.Spawn(g);
            }
            else
            {
                buildingManager.waterWells[row.myIndex] = buildingWaterWell;
                Destroy(g);
            }
        }
    }

    #endregion

    #region CultivableField
    public void SaveCultivableField(string scene, List<CultivableField> cultivableFields)
    {
        connection.Execute("DELETE FROM cultivableField");
        connection.Execute("DELETE FROM cuiltivableFieldItem");
        // note: .Insert causes a 'Constraint' exception. use Replace.
        for (int e = 0; e < cultivableFields.Count; e++)
        {
            Building building = cultivableFields[e].GetComponent<Building>();
            CultivableField cultivableField = building.GetComponent<CultivableField>();
            //Npc npc = beeKeeper[e].GetComponent<Npc>();
            connection.InsertOrReplace(new cultivableField
            {
                myIndex = e,
                sceneName = scene,
                guild = building.guild,
                owner = building.owner,
                positionX = building.transform.position.x,
                positionY = building.transform.position.y,
                level = building.level,
                health = building.health,
                isPremium = Convert.ToInt32(building.isPremiumZone)
            });

            for (int a = 0; a < cultivableFields[e].currentPlant.Count; a++)
            {
                connection.InsertOrReplace(new cuiltivableFieldItem
                {
                    myIndex = e,
                    plantName = cultivableFields[e].currentPlant[a].plantName,
                    dimension = cultivableFields[e].currentPlant[a].dimension,
                    season = cultivableFields[e].currentPlant[a].season,
                    alreadyGrown = cultivableFields[e].currentPlant[a].alreadyGrown,
                    grownQuantityX = cultivableFields[e].currentPlant[a].grownQuantityX,
                    grownQuantityY = cultivableFields[e].currentPlant[a].grownQuantityY,
                    seeds = cultivableFields[e].currentPlant[a].seeds,
                    plantAmount = cultivableFields[e].currentPlant[a].plantAmount,
                    timebeforeTakeMultipleSeeds = cultivableFields[e].currentPlant[a].timebeforeTakeMultipleSeeds,
                    releaseSeeds = cultivableFields[e].currentPlant[a].releaseSeeds
                });
            }
        }
    }


    public void LoadCultivableField(GameObject prefab, string scene)
    {
        bool notEmptyItem = false;
        foreach (cultivableField row in connection.Query<cultivableField>("SELECT * FROM cultivableField WHERE sceneName=?", scene))
        {
            GameObject g = Instantiate(prefab);
            Building building = g.GetComponent<Building>();
            CultivableField cultivableField = g.GetComponent<CultivableField>();
            g.transform.position = new Vector3(row.positionX, row.positionY, 0.0f);
            building.guild = row.guild;
            building.owner = row.owner;
            building.health = row.health;
            building.level = row.level;
            building.isPremiumZone = Convert.ToBoolean(row.isPremium);

            foreach (cuiltivableFieldItem row2 in connection.Query<cuiltivableFieldItem>("SELECT * FROM cuiltivableFieldItem WHERE myIndex=?", row.myIndex))
            {
                CustomType.CultivableFood item = new CustomType.CultivableFood();

                if (item.plantName != string.Empty && item.plantName != "Undefined")
                {
                    notEmptyItem = true;
                }

                item.plantName = row2.plantName;
                item.dimension = row2.dimension;
                item.season = row2.season;
                item.alreadyGrown = row2.alreadyGrown;
                item.grownQuantityX = row2.grownQuantityX;
                item.grownQuantityY = row2.grownQuantityY;
                item.seeds = row2.seeds;
                item.plantAmount = row2.plantAmount;
                item.timebeforeTakeMultipleSeeds = row2.timebeforeTakeMultipleSeeds;
                item.releaseSeeds = row2.releaseSeeds;

                if (building.isPremiumZone)
                {
                    buildingManager.cultivableFields[row.myIndex].currentPlant.Add(item);
                }
                else
                {
                    cultivableField.currentPlant.Add(item);
                }
            }

            if (!building.isPremiumZone) buildingManager.AddToList(g);

            if (building.isPremiumZone)
            {
                buildingManager.cultivableFields[row.myIndex] = cultivableField;
                Destroy(g);
            }
            else
            {
                //if (notEmptyItem == true)
                //    g.SetActive(false);
                NetworkServer.Spawn(g);
            }
            notEmptyItem = false;
        }

    }

    #endregion

    #region StreetLamps
    public void SaveStreetLamps(string scene, List<StreetLamp> streetLamps)
    {
        connection.Execute("DELETE FROM streetLamps");
        // note: .Insert causes a 'Constraint' exception. use Replace.
        for (int i = 0; i < streetLamps.Count; i++)
        {
            Building building = streetLamps[i].GetComponent<Building>();
            //Npc npc = buildingWaterWell[i].GetComponent<Npc>();
            connection.InsertOrReplace(new streetLamps
            {
                myIndex = i,
                sceneName = scene,
                health = building.health,
                guild = building.guild,
                owner = building.owner,
                positionX = building.transform.position.x,
                positionY = building.transform.position.y,
                level = building.level,
                isPremium = Convert.ToInt32(building.isPremiumZone)

            }); ;
        }
    }

    public void LoadStreetLamps(GameObject prefab, string scene)
    {
        foreach (streetLamps row in connection.Query<streetLamps>("SELECT * FROM streetLamps WHERE sceneName=?", scene))
        {
            GameObject g = Instantiate(prefab);
            Building building = g.GetComponent<Building>();
            //Npc npc = g.GetComponent<Npc>();
            StreetLamp streetLamps = building.GetComponent<StreetLamp>();
            g.transform.position = new Vector3(row.positionX, row.positionY, 0.0f);
            building.health = row.health;
            building.guild = row.guild;
            building.owner = row.owner;
            building.level = row.level;
            building.isPremiumZone = Convert.ToBoolean(row.isPremium);

            if (!building.isPremiumZone) buildingManager.AddToList(g);

            if (!building.isPremiumZone)
            {
                //g.SetActive(false);
                NetworkServer.Spawn(g);
            }
            else
            {
                buildingManager.streetLamps[row.myIndex] = streetLamps;
                Destroy(g);
            }
        }
    }

    #endregion

    #region Flag
    public void SaveFlag(string scene, List<Flag> flag)
    {
        connection.Execute("DELETE FROM flag");
        // note: .Insert causes a 'Constraint' exception. use Replace.
        for (int i = 0; i < flag.Count; i++)
        {
            Building building = flag[i].GetComponent<Building>();
            //Npc npc = tesla[i].GetComponent<Npc>();
            connection.InsertOrReplace(new flag
            {
                myIndex = i,
                sceneName = scene,
                health = building.health,
                guild = building.guild,
                owner = building.owner,
                positionX = building.transform.position.x,
                positionY = building.transform.position.y,
                level = building.level,
                isPremium = Convert.ToInt32(building.isPremiumZone),
                NationFlag = building.GetComponent<Flag>().appliedNation
            });
        }
    }
    public void LoadFlag(GameObject prefab, string scene)
    {
        foreach (flag row in connection.Query<flag>("SELECT * FROM flag WHERE sceneName=?", scene))
        {
            GameObject g = Instantiate(prefab);
            Building building = g.GetComponent<Building>();
            //Npc npc = g.GetComponent<Npc>();
            Flag flag = building.GetComponent<Flag>();
            g.transform.position = new Vector3(row.positionX, row.positionY, 0.0f);
            building.health = row.health;
            building.guild = row.guild;
            building.owner = row.owner;
            building.level = row.level;
            building.isPremiumZone = Convert.ToBoolean(row.isPremium);
            flag.selectedNation = row.NationFlag;

            if (!building.isPremiumZone) buildingManager.AddToList(g);

            if (building.isPremiumZone)
            {
                buildingManager.flags[row.myIndex] = flag;
                Destroy(g);
            }
            else
            {
                //g.SetActive(false);
                NetworkServer.Spawn(g);
            }
        }
    }

    #endregion

    #region Belt
    public void SaveBelt(Player player)
    {
        if (!player.playerBelt) return;
        connection.Execute("DELETE FROM character_belt WHERE character=?", player.name);
        for (int i = 0; i < player.playerBelt.belt.Count; i++)
        {
            ItemSlot slot = player.playerBelt.belt[i];
            if (slot.amount > 0) // only relevant items to save queries/storage/time
            {
                // note: .Insert causes a 'Constraint' exception. use Replace.
                connection.InsertOrReplace(new character_belt
                {
                    myIndex = i,
                    character = player.name,
                    slot = i,
                    name = slot.item.name,
                    amount = slot.amount,
                    summonedHealth = slot.item.summonedHealth,
                    summonedLevel = slot.item.summonedLevel,
                    summonedExperience = slot.item.summonedExperience,
                    currentArmor = slot.item.currentArmor,
                    currentUnsanity = slot.item.currentUnsanity,
                    alreadyShooted = slot.item.alreadyShooted,
                    totalAlreadyShooted = slot.item.totalAlreadyShooted,
                    radioCurrentBattery = slot.item.radioCurrentBattery,
                    torchCurrentBattery = slot.item.torchCurrentBattery,
                    durability = slot.item.durability,
                    weight = slot.item.weight,
                    accuracyLevel = slot.item.accuracyLevel,
                    missLevel = slot.item.missLevel,
                    armorLevel = slot.item.armorLevel,
                    chargeLevel = slot.item.chargeLevel,
                    batteryLevel = slot.item.batteryLevel,
                    weightLevel = slot.item.weightLevel,
                    durabilityLevel = slot.item.durabilityLevel,
                    unsanityLevel = slot.item.unsanityLevel,
                    bagLevel = slot.item.bagLevel,
                    gasolineContainer = slot.item.gasolineContainer,
                    honeyContainer = slot.item.honeyContainer,
                    waterContainer = slot.item.waterContainer,
                    cookCountdown = slot.item.cookCountdown,
                    wet = slot.item.wet,
                    playerName = player.name,
                    type = string.Empty,
                    remainingTime = 0,
                    totalTime = 0

                }); ;
            }
        }

    }

    public void LoadBelt(Player player)
    {
        foreach (character_belt row in connection.Query<character_belt>("SELECT * FROM character_belt WHERE character=?", player.name))
        {
            if (ScriptableItem.dict.TryGetValue(row.name.GetStableHashCode(), out ScriptableItem itemData))
            {
                Item item = new Item(itemData);
                item.summonedHealth = row.summonedHealth;
                item.summonedLevel = row.summonedLevel;
                item.summonedExperience = row.summonedExperience;
                item.currentArmor = row.currentArmor;
                item.currentUnsanity = row.currentUnsanity;
                item.alreadyShooted = row.alreadyShooted;
                item.totalAlreadyShooted = row.totalAlreadyShooted;
                item.radioCurrentBattery = row.radioCurrentBattery;
                item.torchCurrentBattery = row.torchCurrentBattery;
                item.durability = row.durability;
                item.weight = row.weight;
                item.accuracyLevel = row.accuracyLevel;
                item.missLevel = row.missLevel;
                item.armorLevel = row.armorLevel;
                item.chargeLevel = row.chargeLevel;
                item.batteryLevel = row.batteryLevel;
                item.weightLevel = row.weightLevel;
                item.durabilityLevel = row.durabilityLevel;
                item.unsanityLevel = row.unsanityLevel;
                item.bagLevel = row.bagLevel;
                item.gasolineContainer = row.gasolineContainer;
                item.honeyContainer = row.honeyContainer;
                item.waterContainer = row.waterContainer;
                item.cookCountdown = row.cookCountdown;
                item.wet = row.wet;
                player.playerBelt.belt[row.slot] = new ItemSlot(item, row.amount);
            }
            else Debug.LogWarning("LoadInventory: skipped item " + row.name + " for " + player.name + " because it doesn't exist anymore. If it wasn't removed intentionally then make sure it's in the Resources folder.");
        }

    }


    #endregion

    #region Quest
    public void SaveQuestCustom(Player player)
    {
        // quests: remove old entries first, then add all new ones
        connection.Execute("DELETE FROM quest_additional WHERE charactername=?", player.name);
        foreach (Quest quest in player.quests)
        {
            connection.InsertOrReplace(new quest_additional
            {
                charactername = player.name,
                name = quest.name,
                completed = quest.completed,
                checkEnterPremium = quest.checkEnterPremium,
                checkCreateBuilding = quest.checkCreateBuilding,
                checkEquipWeapon = quest.checkEquipWeapon,
                checkEquipBag = quest.checkEquipBag,
                checkCreateGuild = quest.checkCreateGuild,
                checkCreateParty = quest.checkCreateParty,
                checkMakeGroupAlly = quest.checkMakeGroupAlly,
                checkDrink = quest.checkDrink,
                checkEat = quest.checkEat,
                checkRun = quest.checkRun,
                checkSneak = quest.checkSneak,
                checkMakeMarriage = quest.checkMakeMarriage,
                checkSendAMessage = quest.checkSendAMessage,
                checkBuyEmoji = quest.checkBuyEmoji,
                checkOpenShop = quest.checkOpenShop,
                checkAmountPlayerToKill = quest.checkAmountPlayerToKill,
                checkWoodToGather = quest.checkWoodToGather,
                checkRockToGather = quest.checkRockToGather,
                checkUseTeleport = quest.checkUseTeleport,
                checkUseInstantResurrect = quest.checkUseInstantResurrect,
                checkMakeATrade = quest.checkMakeATrade,
                checkFriendCount = quest.checkFriendCount,
                checkCreateASpawnpoint = quest.checkCreateASpawnpoint,
                checkHairZombie = quest.checkBiohazard,
                checkPirateZombie = quest.checkMechanic,
                checkHatZombie = quest.checkInfected,
                checkCountryZombie = quest.checkPolice

            });
        }

    }

    public void LoadQuestCustom(Player player)
    {
        foreach (quest_additional row in connection.Query<quest_additional>("SELECT * FROM quest_additional WHERE charactername=?", player.name))
        {
            ScriptableQuest questData;
            if (ScriptableQuest.dict.TryGetValue(row.name.GetStableHashCode(), out questData))
            {
                Quest quest = new Quest(questData);
                quest.completed = row.completed;
                quest.checkEnterPremium = row.checkEnterPremium;
                quest.checkCreateBuilding = row.checkCreateBuilding;
                quest.checkEquipWeapon = row.checkEquipWeapon;
                quest.checkEquipBag = row.checkEquipBag;
                quest.checkCreateGuild = row.checkCreateGuild;
                quest.checkCreateParty = row.checkCreateParty;
                quest.checkMakeGroupAlly = row.checkMakeGroupAlly;
                quest.checkDrink = row.checkDrink;
                quest.checkEat = row.checkEat;
                quest.checkRun = row.checkRun;
                quest.checkSneak = row.checkSneak;
                quest.checkMakeMarriage = row.checkMakeMarriage;
                quest.checkSendAMessage = row.checkSendAMessage;
                quest.checkBuyEmoji = row.checkBuyEmoji;
                quest.checkOpenShop = row.checkOpenShop;
                quest.checkAmountPlayerToKill = row.checkAmountPlayerToKill;
                quest.checkWoodToGather = row.checkWoodToGather;
                quest.checkRockToGather = row.checkRockToGather;
                quest.checkUseTeleport = row.checkUseTeleport;
                quest.checkUseInstantResurrect = row.checkUseInstantResurrect;
                quest.checkMakeATrade = row.checkMakeATrade;
                quest.checkFriendCount = row.checkFriendCount;
                quest.checkCreateASpawnpoint = row.checkCreateASpawnpoint;
                quest.checkBiohazard = row.checkHairZombie;
                quest.checkMechanic = row.checkPirateZombie;
                quest.checkInfected = row.checkHatZombie;
                quest.checkPolice = row.checkCountryZombie;
                player.quests.Add(quest);
            }
            else Debug.LogWarning("LoadQuests: skipped quest " + row.name + " for " + player.name + " because it doesn't exist anymore. If it wasn't removed intentionally then make sure it's in the Resources folder.");
        }

    }

    #endregion

    #region Floor
    public void SaveFloor(string scene, List<ModularPiece> modularFloor)
    {
        connection.Execute("DELETE FROM floor");

        for (int i = 0; i < modularFloor.Count; i++)
        {
            ModularPiece modularPiece = modularFloor[i].GetComponent<ModularPiece>();
            connection.InsertOrReplace(new floor
            {
                sceneName = scene,
                index = modularPiece.modularIndex,
                isMain = Convert.ToInt32(modularPiece.isMain),
                owner = modularPiece.owner,
                guildName = modularPiece.guild,
                upPart = modularPiece.upComponent,
                downPart = modularPiece.downComponent,
                leftPart = modularPiece.leftComponent,
                rightPart = modularPiece.rightComponent,
                posX = modularPiece.transform.position.x,
                posY = modularPiece.transform.position.y,
                posZ = modularPiece.transform.position.z
            });
        }
    }

    public void LoadFloor(GameObject prefab, string scene)
    {
        foreach (floor row in connection.Query<floor>("SELECT * FROM floor WHERE sceneName=?", scene))
        {
            GameObject g = Instantiate(prefab);
            ModularPiece modularPiece = g.GetComponent<ModularPiece>();
            g.transform.position = new Vector3(row.posX, row.posY, 0.0f);
            modularPiece.modularIndex = row.index;
            modularPiece.isMain = Convert.ToBoolean(row.isMain);
            modularPiece.owner = row.owner;
            modularPiece.guild = row.guildName;
            modularPiece.upComponent = row.upPart;
            modularPiece.downComponent = row.downPart;
            modularPiece.leftComponent = row.leftPart;
            modularPiece.rightComponent = row.rightPart;

            buildingManager.AddToList(g);

            NetworkServer.Spawn(g);

        }
    }

    #endregion

    #region ModularObject

    public void SaveModularObject(string scene, List<ModularObject> modularObjectList)
    {
        connection.Execute("DELETE FROM objects");
        connection.Execute("DELETE FROM buildingCraftItem");
        connection.Execute("DELETE FROM upgradeItems");
        connection.Execute("DELETE FROM repairItems");
        connection.Execute("DELETE FROM modularInventory");

        for (int i = 0; i < modularObjectList.Count; i++)
        {
            ModularObject modularObject = modularObjectList[i].GetComponent<ModularObject>();
            connection.InsertOrReplace(new objects
            {
                sceneName = scene,
                buildingName = modularObject.scriptableBuilding.name,
                index = modularObject.oldPositioning,
                objectIndex = modularObject.objectIndex,
                posX = modularObject.transform.position.x,
                posY = modularObject.transform.position.y,
                posZ = modularObject.transform.position.z
            });

            if (modularObjectList[i].GetComponent<BuildingModularCrafting>())
            {
                BuildingModularCrafting buildingModularCrafting = modularObjectList[i].GetComponent<BuildingModularCrafting>();
                for (int e = 0; e < buildingModularCrafting.craftItem.Count; e++)
                {
                    int index_e = e;
                    CraftItem slot = buildingModularCrafting.craftItem[index_e];
                    if (slot.amount > 0) // only relevant items to save queries/storage/time
                    {
                        // note: .Insert causes a 'Constraint' exception. use Replace.
                        connection.InsertOrReplace(new buildingCraftItem
                        {
                            myIndex = modularObject.objectIndex,
                            itemName = slot.itemName,
                            amount = slot.amount,
                            remainingTime = slot.remainingTime,
                            totalTime = slot.totalTime,
                            timeBegin = slot.timeBegin.ToString(),
                            timeEnd = slot.timeEnd.ToString(),
                            timeEndServer = slot.timeEnd.ToString(),
                            owner = slot.owner,
                            guildName = slot.guildName
                        });
                    }
                }
            }
            else if (modularObjectList[i].GetComponent<Fridge>())
            {
                Fridge fridge = modularObjectList[i].GetComponent<Fridge>();
                for (int a = 0; a < fridge.inventory.Count; a++)
                {
                    int index_a = a;
                    ItemSlot slot = fridge.inventory[index_a];
                    if (slot.amount > 0) // only relevant items to save queries/storage/time
                    {
                        // note: .Insert causes a 'Constraint' exception. use Replace.
                        if (slot.amount > 0) // only relevant items to save queries/storage/time
                        {
                            // note: .Insert causes a 'Constraint' exception. use Replace.
                            connection.InsertOrReplace(new modularInventory
                            {
                                myIndex = modularObject.objectIndex,
                                character = "",
                                slot = index_a,
                                name = slot.item.name,
                                amount = slot.amount,
                                summonedHealth = slot.item.summonedHealth,
                                summonedLevel = slot.item.summonedLevel,
                                summonedExperience = slot.item.summonedExperience,
                                currentArmor = slot.item.currentArmor,
                                currentUnsanity = slot.item.currentUnsanity,
                                alreadyShooted = slot.item.alreadyShooted,
                                totalAlreadyShooted = slot.item.totalAlreadyShooted,
                                radioCurrentBattery = slot.item.radioCurrentBattery,
                                torchCurrentBattery = slot.item.torchCurrentBattery,
                                durability = slot.item.durability,
                                weight = slot.item.weight,
                                accuracyLevel = slot.item.accuracyLevel,
                                missLevel = slot.item.missLevel,
                                armorLevel = slot.item.armorLevel,
                                chargeLevel = slot.item.chargeLevel,
                                batteryLevel = slot.item.batteryLevel,
                                weightLevel = slot.item.weightLevel,
                                durabilityLevel = slot.item.durabilityLevel,
                                unsanityLevel = slot.item.unsanityLevel,
                                bagLevel = slot.item.bagLevel,
                                gasolineContainer = slot.item.gasolineContainer,
                                honeyContainer = slot.item.honeyContainer,
                                waterContainer = slot.item.waterContainer,
                                cookCountdown = slot.item.cookCountdown,
                                wet = slot.item.wet,
                                playerName = "",
                                type = string.Empty,
                                remainingTime = 0,
                                totalTime = 0

                            }); ;
                        }
                    }
                }
            }
            else if (modularObjectList[i].GetComponent<Closet>())
            {
                Closet closet = modularObjectList[i].GetComponent<Closet>();
                for (int a = 0; a < closet.inventory.Count; a++)
                {
                    int index_a = a;
                    ItemSlot slot = closet.inventory[index_a];
                    if (slot.amount > 0) // only relevant items to save queries/storage/time
                    {
                        // note: .Insert causes a 'Constraint' exception. use Replace.
                        if (slot.amount > 0) // only relevant items to save queries/storage/time
                        {
                            // note: .Insert causes a 'Constraint' exception. use Replace.
                            connection.InsertOrReplace(new modularInventory
                            {
                                myIndex = modularObject.objectIndex,
                                character = "",
                                slot = index_a,
                                name = slot.item.name,
                                amount = slot.amount,
                                summonedHealth = slot.item.summonedHealth,
                                summonedLevel = slot.item.summonedLevel,
                                summonedExperience = slot.item.summonedExperience,
                                currentArmor = slot.item.currentArmor,
                                currentUnsanity = slot.item.currentUnsanity,
                                alreadyShooted = slot.item.alreadyShooted,
                                totalAlreadyShooted = slot.item.totalAlreadyShooted,
                                radioCurrentBattery = slot.item.radioCurrentBattery,
                                torchCurrentBattery = slot.item.torchCurrentBattery,
                                durability = slot.item.durability,
                                weight = slot.item.weight,
                                accuracyLevel = slot.item.accuracyLevel,
                                missLevel = slot.item.missLevel,
                                armorLevel = slot.item.armorLevel,
                                chargeLevel = slot.item.chargeLevel,
                                batteryLevel = slot.item.batteryLevel,
                                weightLevel = slot.item.weightLevel,
                                durabilityLevel = slot.item.durabilityLevel,
                                unsanityLevel = slot.item.unsanityLevel,
                                bagLevel = slot.item.bagLevel,
                                gasolineContainer = slot.item.gasolineContainer,
                                honeyContainer = slot.item.honeyContainer,
                                waterContainer = slot.item.waterContainer,
                                cookCountdown = slot.item.cookCountdown,
                                wet = slot.item.wet,
                                playerName = "",
                                type = string.Empty,
                                remainingTime = 0,
                                totalTime = 0

                            }); ;
                        }
                    }
                }
            }
            else if (modularObjectList[i].GetComponent<WeaponStorage>())
            {
                WeaponStorage weaponStorage = modularObjectList[i].GetComponent<WeaponStorage>();
                for (int a = 0; a < weaponStorage.weapon.Count; a++)
                {
                    int index_a = a;
                    ItemSlot slot = weaponStorage.weapon[index_a];
                    if (slot.amount > 0) // only relevant items to save queries/storage/time
                    {
                        // note: .Insert causes a 'Constraint' exception. use Replace.
                        if (slot.amount > 0) // only relevant items to save queries/storage/time
                        {
                            // note: .Insert causes a 'Constraint' exception. use Replace.
                            connection.InsertOrReplace(new modularInventory
                            {
                                myIndex = modularObject.objectIndex,
                                character = "",
                                slot = index_a,
                                name = slot.item.name,
                                amount = slot.amount,
                                summonedHealth = slot.item.summonedHealth,
                                summonedLevel = slot.item.summonedLevel,
                                summonedExperience = slot.item.summonedExperience,
                                currentArmor = slot.item.currentArmor,
                                currentUnsanity = slot.item.currentUnsanity,
                                alreadyShooted = slot.item.alreadyShooted,
                                totalAlreadyShooted = slot.item.totalAlreadyShooted,
                                radioCurrentBattery = slot.item.radioCurrentBattery,
                                torchCurrentBattery = slot.item.torchCurrentBattery,
                                durability = slot.item.durability,
                                weight = slot.item.weight,
                                accuracyLevel = slot.item.accuracyLevel,
                                missLevel = slot.item.missLevel,
                                armorLevel = slot.item.armorLevel,
                                chargeLevel = slot.item.chargeLevel,
                                batteryLevel = slot.item.batteryLevel,
                                weightLevel = slot.item.weightLevel,
                                durabilityLevel = slot.item.durabilityLevel,
                                unsanityLevel = slot.item.unsanityLevel,
                                bagLevel = slot.item.bagLevel,
                                gasolineContainer = slot.item.gasolineContainer,
                                honeyContainer = slot.item.honeyContainer,
                                waterContainer = slot.item.waterContainer,
                                cookCountdown = slot.item.cookCountdown,
                                wet = slot.item.wet,
                                playerName = "",
                                type = string.Empty,
                                remainingTime = 0,
                                totalTime = 0

                            }); ;
                        }
                    }
                }
            }

            if (modularObjectList[i].GetComponent<ModularPersonalWareHouse>())
            {
                ModularPersonalWareHouse warehouse = modularObjectList[i].GetComponent<ModularPersonalWareHouse>();
                for (int a = 0; a < warehouse.inventory.Count; a++)
                {
                    int index_a = a;
                    ItemSlot slot = warehouse.inventory[index_a];
                    if (slot.amount > 0) // only relevant items to save queries/storage/time
                    {
                        // note: .Insert causes a 'Constraint' exception. use Replace.
                        if (slot.amount > 0) // only relevant items to save queries/storage/time
                        {
                            // note: .Insert causes a 'Constraint' exception. use Replace.
                            connection.InsertOrReplace(new modularInventory
                            {
                                myIndex = modularObject.objectIndex,
                                character = "",
                                slot = index_a,
                                name = slot.item.name,
                                amount = slot.amount,
                                summonedHealth = slot.item.summonedHealth,
                                summonedLevel = slot.item.summonedLevel,
                                summonedExperience = slot.item.summonedExperience,
                                currentArmor = slot.item.currentArmor,
                                currentUnsanity = slot.item.currentUnsanity,
                                alreadyShooted = slot.item.alreadyShooted,
                                totalAlreadyShooted = slot.item.totalAlreadyShooted,
                                radioCurrentBattery = slot.item.radioCurrentBattery,
                                torchCurrentBattery = slot.item.torchCurrentBattery,
                                durability = slot.item.durability,
                                weight = slot.item.weight,
                                accuracyLevel = slot.item.accuracyLevel,
                                missLevel = slot.item.missLevel,
                                armorLevel = slot.item.armorLevel,
                                chargeLevel = slot.item.chargeLevel,
                                batteryLevel = slot.item.batteryLevel,
                                weightLevel = slot.item.weightLevel,
                                durabilityLevel = slot.item.durabilityLevel,
                                unsanityLevel = slot.item.unsanityLevel,
                                bagLevel = slot.item.bagLevel,
                                gasolineContainer = slot.item.gasolineContainer,
                                honeyContainer = slot.item.honeyContainer,
                                waterContainer = slot.item.waterContainer,
                                cookCountdown = slot.item.cookCountdown,
                                wet = slot.item.wet,
                                playerName = "",
                                type = string.Empty,
                                remainingTime = 0,
                                totalTime = 0

                            }); ;
                        }
                    }
                }
            }

            if (modularObjectList[i].GetComponent<BuildingUpgradeRepair>())
            {
                BuildingUpgradeRepair buildingUpgradeRepair = modularObjectList[i].GetComponent<BuildingUpgradeRepair>();
                for (int y = 0; y < buildingUpgradeRepair.upgradeItem.Count; y++)
                {
                    int index_y = y;
                    // note: .Insert causes a 'Constraint' exception. use Replace.
                    connection.InsertOrReplace(new upgradeItems
                    {
                        myIndex = modularObject.objectIndex,
                        amount = buildingUpgradeRepair.upgradeItem[index_y].item.amount,
                        slot = index_y,
                        name = buildingUpgradeRepair.upgradeItem[index_y].item.item.name,
                        summonHealth = buildingUpgradeRepair.upgradeItem[index_y].item.item.summonedHealth,
                        summonedLevel = buildingUpgradeRepair.upgradeItem[index_y].item.item.summonedLevel,
                        summonedExperience = buildingUpgradeRepair.upgradeItem[index_y].item.item.summonedExperience,
                        currentArmor = buildingUpgradeRepair.upgradeItem[index_y].item.item.currentArmor,
                        currentUnsanity = buildingUpgradeRepair.upgradeItem[index_y].item.item.currentUnsanity,
                        alreadyShooted = buildingUpgradeRepair.upgradeItem[index_y].item.item.alreadyShooted,
                        totalAlreadyShooted = buildingUpgradeRepair.upgradeItem[index_y].item.item.totalAlreadyShooted,
                        radioCurrentBattery = buildingUpgradeRepair.upgradeItem[index_y].item.item.radioCurrentBattery,
                        torchCurrentBattery = buildingUpgradeRepair.upgradeItem[index_y].item.item.torchCurrentBattery,
                        durability = buildingUpgradeRepair.upgradeItem[index_y].item.item.durability,
                        weight = buildingUpgradeRepair.upgradeItem[index_y].item.item.weight,
                        accuracyLevel = buildingUpgradeRepair.upgradeItem[index_y].item.item.accuracyLevel,
                        missLevel = buildingUpgradeRepair.upgradeItem[index_y].item.item.missLevel,
                        armorLevel = buildingUpgradeRepair.upgradeItem[index_y].item.item.armorLevel,
                        chargeLevel = buildingUpgradeRepair.upgradeItem[index_y].item.item.chargeLevel,
                        batteryLevel = buildingUpgradeRepair.upgradeItem[index_y].item.item.batteryLevel,
                        weightLevel = buildingUpgradeRepair.upgradeItem[index_y].item.item.weightLevel,
                        durabilityLevel = buildingUpgradeRepair.upgradeItem[index_y].item.item.durabilityLevel,
                        unsanityLevel = buildingUpgradeRepair.upgradeItem[index_y].item.item.unsanityLevel,
                        bagLevel = buildingUpgradeRepair.upgradeItem[index_y].item.item.bagLevel,
                        gasolineContainer = buildingUpgradeRepair.upgradeItem[index_y].item.item.gasolineContainer,
                        honeyContainer = buildingUpgradeRepair.upgradeItem[index_y].item.item.honeyContainer,
                        waterContainer = buildingUpgradeRepair.upgradeItem[index_y].item.item.waterContainer,
                        cookCountdown = buildingUpgradeRepair.upgradeItem[index_y].item.item.cookCountdown,
                        wet = buildingUpgradeRepair.upgradeItem[index_y].item.item.wet,
                        playerName = buildingUpgradeRepair.upgradeItem[index_y].playerName,
                        remainingTime = buildingUpgradeRepair.upgradeItem[index_y].remainingTime,
                        totalTime = buildingUpgradeRepair.upgradeItem[index_y].totalTime,
                        timeBegin = buildingUpgradeRepair.upgradeItem[index_y].timeBegin,
                        timeEnd = buildingUpgradeRepair.upgradeItem[index_y].timeEnd,
                        type = buildingUpgradeRepair.upgradeItem[index_y].type
                    });
                }
                for (int x = 0; x < buildingUpgradeRepair.repairItem.Count; x++)
                {
                    int index_x = x;
                    // note: .Insert causes a 'Constraint' exception. use Replace.
                    connection.InsertOrReplace(new repairItems
                    {
                        myIndex = modularObject.objectIndex,
                        amount = buildingUpgradeRepair.repairItem[index_x].item.amount,
                        slot = index_x,
                        name = buildingUpgradeRepair.repairItem[index_x].item.item.name,
                        summonHealth = buildingUpgradeRepair.repairItem[index_x].item.item.summonedHealth,
                        summonedLevel = buildingUpgradeRepair.repairItem[index_x].item.item.summonedLevel,
                        summonedExperience = buildingUpgradeRepair.repairItem[index_x].item.item.summonedExperience,
                        currentArmor = buildingUpgradeRepair.repairItem[index_x].item.item.currentArmor,
                        currentUnsanity = buildingUpgradeRepair.repairItem[index_x].item.item.currentUnsanity,
                        alreadyShooted = buildingUpgradeRepair.repairItem[index_x].item.item.alreadyShooted,
                        totalAlreadyShooted = buildingUpgradeRepair.repairItem[index_x].item.item.totalAlreadyShooted,
                        radioCurrentBattery = buildingUpgradeRepair.repairItem[index_x].item.item.radioCurrentBattery,
                        torchCurrentBattery = buildingUpgradeRepair.repairItem[index_x].item.item.torchCurrentBattery,
                        durability = buildingUpgradeRepair.repairItem[index_x].item.item.durability,
                        weight = buildingUpgradeRepair.repairItem[index_x].item.item.weight,
                        accuracyLevel = buildingUpgradeRepair.repairItem[index_x].item.item.accuracyLevel,
                        missLevel = buildingUpgradeRepair.repairItem[index_x].item.item.missLevel,
                        armorLevel = buildingUpgradeRepair.repairItem[index_x].item.item.armorLevel,
                        chargeLevel = buildingUpgradeRepair.repairItem[index_x].item.item.chargeLevel,
                        batteryLevel = buildingUpgradeRepair.repairItem[index_x].item.item.batteryLevel,
                        weightLevel = buildingUpgradeRepair.repairItem[index_x].item.item.weightLevel,
                        durabilityLevel = buildingUpgradeRepair.repairItem[index_x].item.item.durabilityLevel,
                        unsanityLevel = buildingUpgradeRepair.repairItem[index_x].item.item.unsanityLevel,
                        bagLevel = buildingUpgradeRepair.repairItem[index_x].item.item.bagLevel,
                        gasolineContainer = buildingUpgradeRepair.repairItem[index_x].item.item.gasolineContainer,
                        honeyContainer = buildingUpgradeRepair.repairItem[index_x].item.item.honeyContainer,
                        waterContainer = buildingUpgradeRepair.repairItem[index_x].item.item.waterContainer,
                        cookCountdown = buildingUpgradeRepair.repairItem[index_x].item.item.cookCountdown,
                        wet = buildingUpgradeRepair.repairItem[index_x].item.item.wet,
                        playerName = buildingUpgradeRepair.repairItem[index_x].playerName,
                        remainingTime = buildingUpgradeRepair.repairItem[index_x].remainingTime,
                        totalTime = buildingUpgradeRepair.repairItem[index_x].totalTime,
                        timeBegin = buildingUpgradeRepair.repairItem[index_x].timeBegin,
                        timeEnd = buildingUpgradeRepair.repairItem[index_x].timeEnd,
                        type = buildingUpgradeRepair.repairItem[index_x].type
                    });
                }
            }
        }
    }

    public void LoadModularObject( string scene)
    {
        foreach (objects row in connection.Query<objects>("SELECT * FROM objects WHERE sceneName=?", scene))
        {
            if (ScriptableBuilding.dict.TryGetValue(row.buildingName.GetStableHashCode(), out ScriptableBuilding itemData))
            {
                GameObject g = Instantiate(itemData.buildingList[row.index].buildingObject);
                g.transform.position = new Vector3(row.posX, row.posY, 0.0f);

                BuildingModularCrafting modularCrafting = g.GetComponent<BuildingModularCrafting>();
                Fridge fridge = g.GetComponent<Fridge>();
                Closet closet = g.GetComponent<Closet>();
                WeaponStorage weaponStorage = g.GetComponent<WeaponStorage>();
                ModularPersonalWareHouse warehouse = g.GetComponent<ModularPersonalWareHouse>();
                BuildingUpgradeRepair upgradeRepair = g.GetComponent<BuildingUpgradeRepair>();

                g.GetComponent<Forniture>().objectIndex = row.objectIndex;

                if(modularCrafting)
                {
                    g = LoadCrafting(row.objectIndex, modularCrafting);
                }
                if(fridge)
                {
                    g = LoadFridge(row.objectIndex, fridge);
                }
                if(closet)
                {
                    g = LoadCloset(row.objectIndex, closet);
                }
                if(weaponStorage)
                {
                    g = LoadWeaponStorage(row.objectIndex, weaponStorage);
                }
                if(warehouse)
                {
                    g = LoadWarehouse(row.objectIndex, warehouse);
                }
                if(upgradeRepair)
                {
                    g = LoadUpgradeReapir(row.objectIndex, upgradeRepair);
                }

                buildingManager.AddToList(g);
                NetworkServer.Spawn(g);
            }
        }
    }

    public GameObject LoadCrafting(int index, BuildingModularCrafting modularObject)
    {
        foreach (buildingCraftItem row2 in connection.Query<buildingCraftItem>("SELECT * FROM buildingCraftItem WHERE myIndex=?", index))
        {
            if (ScriptableItem.dict.TryGetValue(row2.itemName.GetStableHashCode(), out ScriptableItem itemData))
            {
                CustomType.CraftItem item = new CustomType.CraftItem();

                item.itemName = row2.itemName;
                item.amount = row2.amount;
                item.totalTime = row2.totalTime;
                item.owner = row2.owner;
                item.guildName = row2.guildName;
                item.remainingTime = row2.remainingTime;
                item.timeBegin = row2.timeBegin.ToString();
                item.timeEnd = row2.timeEnd.ToString();
                item.timeEndServer = row2.timeEndServer.ToString();

                modularObject.craftItem.Add(item);
            }
        }
        return modularObject.gameObject;
    }

    public GameObject LoadFridge(int index, Fridge modularObject)
    {
        foreach (modularInventory row2 in connection.Query<modularInventory>("SELECT * FROM modularInventory WHERE myIndex=?", index))
        {
            if (ScriptableItem.dict.TryGetValue(row2.name.GetStableHashCode(), out ScriptableItem itemData))
            {
                Item item = new Item(itemData);

                item.summonedHealth = row2.summonedHealth;
                item.summonedLevel = row2.summonedLevel;
                item.summonedExperience = row2.summonedExperience;
                item.accuracyLevel = row2.accuracyLevel;
                item.missLevel = row2.missLevel;
                item.armorLevel = row2.armorLevel;
                item.chargeLevel = row2.chargeLevel;
                item.batteryLevel = row2.batteryLevel;
                item.weightLevel = row2.weightLevel;
                item.durabilityLevel = row2.durabilityLevel;
                item.unsanityLevel = row2.unsanityLevel;
                item.bagLevel = row2.bagLevel;
                item.currentArmor = row2.currentArmor;
                item.alreadyShooted = row2.alreadyShooted;
                item.totalAlreadyShooted = row2.totalAlreadyShooted;
                item.durability = row2.durability;
                item.currentUnsanity = row2.currentUnsanity;
                item.radioCurrentBattery = row2.radioCurrentBattery;
                item.torchCurrentBattery = row2.torchCurrentBattery;
                item.weight = row2.weight;
                item.gasolineContainer = row2.gasolineContainer;
                item.honeyContainer = row2.honeyContainer;
                item.waterContainer = row2.waterContainer;
                item.cookCountdown = row2.cookCountdown;
                item.wet = row2.wet;

                modularObject.inventory[row2.slot] = new ItemSlot(item, row2.amount);

            }
        }
        return modularObject.gameObject;
    }

    public GameObject LoadCloset(int index, Closet modularObject)
    {
        foreach (modularInventory row2 in connection.Query<modularInventory>("SELECT * FROM modularInventory WHERE myIndex=?", index))
        {
            if (ScriptableItem.dict.TryGetValue(row2.name.GetStableHashCode(), out ScriptableItem itemData))
            {
                Item item = new Item(itemData);

                item.summonedHealth = row2.summonedHealth;
                item.summonedLevel = row2.summonedLevel;
                item.summonedExperience = row2.summonedExperience;
                item.accuracyLevel = row2.accuracyLevel;
                item.missLevel = row2.missLevel;
                item.armorLevel = row2.armorLevel;
                item.chargeLevel = row2.chargeLevel;
                item.batteryLevel = row2.batteryLevel;
                item.weightLevel = row2.weightLevel;
                item.durabilityLevel = row2.durabilityLevel;
                item.unsanityLevel = row2.unsanityLevel;
                item.bagLevel = row2.bagLevel;
                item.currentArmor = row2.currentArmor;
                item.alreadyShooted = row2.alreadyShooted;
                item.totalAlreadyShooted = row2.totalAlreadyShooted;
                item.durability = row2.durability;
                item.currentUnsanity = row2.currentUnsanity;
                item.radioCurrentBattery = row2.radioCurrentBattery;
                item.torchCurrentBattery = row2.torchCurrentBattery;
                item.weight = row2.weight;
                item.gasolineContainer = row2.gasolineContainer;
                item.honeyContainer = row2.honeyContainer;
                item.waterContainer = row2.waterContainer;
                item.cookCountdown = row2.cookCountdown;
                item.wet = row2.wet;

                modularObject.inventory[row2.slot] = new ItemSlot(item, row2.amount);


            }
        }
        return modularObject.gameObject;
    }

    public GameObject LoadWeaponStorage(int index, WeaponStorage modularObject)
    {
        foreach (modularInventory row2 in connection.Query<modularInventory>("SELECT * FROM modularInventory WHERE myIndex=?", index))
        {
            if (ScriptableItem.dict.TryGetValue(row2.name.GetStableHashCode(), out ScriptableItem itemData))
            {
                Item item = new Item(itemData);

                item.summonedHealth = row2.summonedHealth;
                item.summonedLevel = row2.summonedLevel;
                item.summonedExperience = row2.summonedExperience;
                item.accuracyLevel = row2.accuracyLevel;
                item.missLevel = row2.missLevel;
                item.armorLevel = row2.armorLevel;
                item.chargeLevel = row2.chargeLevel;
                item.batteryLevel = row2.batteryLevel;
                item.weightLevel = row2.weightLevel;
                item.durabilityLevel = row2.durabilityLevel;
                item.unsanityLevel = row2.unsanityLevel;
                item.bagLevel = row2.bagLevel;
                item.currentArmor = row2.currentArmor;
                item.alreadyShooted = row2.alreadyShooted;
                item.totalAlreadyShooted = row2.totalAlreadyShooted;
                item.durability = row2.durability;
                item.currentUnsanity = row2.currentUnsanity;
                item.radioCurrentBattery = row2.radioCurrentBattery;
                item.torchCurrentBattery = row2.torchCurrentBattery;
                item.weight = row2.weight;
                item.gasolineContainer = row2.gasolineContainer;
                item.honeyContainer = row2.honeyContainer;
                item.waterContainer = row2.waterContainer;
                item.cookCountdown = row2.cookCountdown;
                item.wet = row2.wet;

                modularObject.weapon[row2.slot] = new ItemSlot(item, row2.amount);


            }
        }
        return modularObject.gameObject;
    }

    public GameObject LoadWarehouse(int index, ModularPersonalWareHouse modularObject)
    {
        foreach (modularInventory row2 in connection.Query<modularInventory>("SELECT * FROM modularInventory WHERE myIndex=?", index))
        {
            if (ScriptableItem.dict.TryGetValue(row2.name.GetStableHashCode(), out ScriptableItem itemData))
            {
                Item item = new Item(itemData);

                item.summonedHealth = row2.summonedHealth;
                item.summonedLevel = row2.summonedLevel;
                item.summonedExperience = row2.summonedExperience;
                item.accuracyLevel = row2.accuracyLevel;
                item.missLevel = row2.missLevel;
                item.armorLevel = row2.armorLevel;
                item.chargeLevel = row2.chargeLevel;
                item.batteryLevel = row2.batteryLevel;
                item.weightLevel = row2.weightLevel;
                item.durabilityLevel = row2.durabilityLevel;
                item.unsanityLevel = row2.unsanityLevel;
                item.bagLevel = row2.bagLevel;
                item.currentArmor = row2.currentArmor;
                item.alreadyShooted = row2.alreadyShooted;
                item.totalAlreadyShooted = row2.totalAlreadyShooted;
                item.durability = row2.durability;
                item.currentUnsanity = row2.currentUnsanity;
                item.radioCurrentBattery = row2.radioCurrentBattery;
                item.torchCurrentBattery = row2.torchCurrentBattery;
                item.weight = row2.weight;
                item.gasolineContainer = row2.gasolineContainer;
                item.honeyContainer = row2.honeyContainer;
                item.waterContainer = row2.waterContainer;
                item.cookCountdown = row2.cookCountdown;
                item.wet = row2.wet;

                modularObject.inventory[row2.slot] = new ItemSlot(item, row2.amount);


            }
        }
        return modularObject.gameObject;
    }

    public GameObject LoadUpgradeReapir(int index, BuildingUpgradeRepair modularObject)
    {
        foreach (upgradeItems row2 in connection.Query<upgradeItems>("SELECT * FROM upgradeItems WHERE myIndex=?", index))
        {
            if (ScriptableItem.dict.TryGetValue(row2.name.GetStableHashCode(), out ScriptableItem itemData))
            {
                CustomType.UpgradeRepairItem upgradeItem = new CustomType.UpgradeRepairItem();
                upgradeItem.item = new ItemSlot();
                upgradeItem.item.item = new Item(itemData);

                upgradeItem.item.item.summonedHealth = row2.summonHealth;
                upgradeItem.item.item.summonedLevel = row2.summonedLevel;
                upgradeItem.item.item.summonedExperience = row2.summonedExperience;
                upgradeItem.item.item.accuracyLevel = row2.accuracyLevel;
                upgradeItem.item.item.missLevel = row2.missLevel;
                upgradeItem.item.item.armorLevel = row2.armorLevel;
                upgradeItem.item.item.chargeLevel = row2.chargeLevel;
                upgradeItem.item.item.batteryLevel = row2.batteryLevel;
                upgradeItem.item.item.weightLevel = row2.weightLevel;
                upgradeItem.item.item.durabilityLevel = row2.durabilityLevel;
                upgradeItem.item.item.unsanityLevel = row2.unsanityLevel;
                upgradeItem.item.item.bagLevel = row2.bagLevel;
                upgradeItem.item.item.currentArmor = row2.currentArmor;
                upgradeItem.item.item.alreadyShooted = row2.alreadyShooted;
                upgradeItem.item.item.totalAlreadyShooted = row2.totalAlreadyShooted;
                upgradeItem.item.item.durability = row2.durability;
                upgradeItem.item.item.currentUnsanity = row2.currentUnsanity;
                upgradeItem.item.item.radioCurrentBattery = row2.radioCurrentBattery;
                upgradeItem.item.item.torchCurrentBattery = row2.torchCurrentBattery;
                upgradeItem.item.item.weight = row2.weight;
                upgradeItem.item.item.gasolineContainer = row2.gasolineContainer;
                upgradeItem.item.item.honeyContainer = row2.honeyContainer;
                upgradeItem.item.item.waterContainer = row2.waterContainer;
                upgradeItem.item.item.cookCountdown = row2.cookCountdown;
                upgradeItem.item.item.wet = row2.wet;
                upgradeItem.playerName = row2.playerName;
                upgradeItem.remainingTime = row2.remainingTime;
                upgradeItem.totalTime = row2.totalTime;
                upgradeItem.type = row2.type;
                upgradeItem.timeBegin = row2.timeBegin;
                upgradeItem.timeEnd = row2.timeEnd;

                modularObject.upgradeItem.Add(upgradeItem);

            }
        }
        foreach (repairItems row2 in connection.Query<repairItems>("SELECT * FROM repairItems WHERE myIndex=?", index))
        {
            if (ScriptableItem.dict.TryGetValue(row2.name.GetStableHashCode(), out ScriptableItem itemData))
            {
                CustomType.UpgradeRepairItem upgradeItem = new CustomType.UpgradeRepairItem();
                upgradeItem.item = new ItemSlot();
                upgradeItem.item.item = new Item(itemData);

                upgradeItem.item.item.summonedHealth = row2.summonHealth;
                upgradeItem.item.item.summonedLevel = row2.summonedLevel;
                upgradeItem.item.item.summonedExperience = row2.summonedExperience;
                upgradeItem.item.item.accuracyLevel = row2.accuracyLevel;
                upgradeItem.item.item.missLevel = row2.missLevel;
                upgradeItem.item.item.armorLevel = row2.armorLevel;
                upgradeItem.item.item.chargeLevel = row2.chargeLevel;
                upgradeItem.item.item.batteryLevel = row2.batteryLevel;
                upgradeItem.item.item.weightLevel = row2.weightLevel;
                upgradeItem.item.item.durabilityLevel = row2.durabilityLevel;
                upgradeItem.item.item.unsanityLevel = row2.unsanityLevel;
                upgradeItem.item.item.bagLevel = row2.bagLevel;
                upgradeItem.item.item.currentArmor = row2.currentArmor;
                upgradeItem.item.item.alreadyShooted = row2.alreadyShooted;
                upgradeItem.item.item.totalAlreadyShooted = row2.totalAlreadyShooted;
                upgradeItem.item.item.durability = row2.durability;
                upgradeItem.item.item.currentUnsanity = row2.currentUnsanity;
                upgradeItem.item.item.radioCurrentBattery = row2.radioCurrentBattery;
                upgradeItem.item.item.torchCurrentBattery = row2.torchCurrentBattery;
                upgradeItem.item.item.weight = row2.weight;
                upgradeItem.item.item.gasolineContainer = row2.gasolineContainer;
                upgradeItem.item.item.honeyContainer = row2.honeyContainer;
                upgradeItem.item.item.waterContainer = row2.waterContainer;
                upgradeItem.item.item.cookCountdown = row2.cookCountdown;
                upgradeItem.item.item.wet = row2.wet;
                upgradeItem.playerName = row2.playerName;
                upgradeItem.remainingTime = row2.remainingTime;
                upgradeItem.totalTime = row2.totalTime;
                upgradeItem.type = row2.type;
                upgradeItem.timeBegin = row2.timeBegin;
                upgradeItem.timeEnd = row2.timeEnd;

                modularObject.repairItem.Add(upgradeItem);

            }
        }
        return modularObject.gameObject;
    }

    #endregion


    public void SaveBuilding(string sceneName)
    {
        BuildingManager buildingManager = FindObjectOfType<BuildingManager>();

        if (!connection.IsInTransaction)
        {
            buildingManager.RemoveToList();
            connection.BeginTransaction();
            SaveBeeKeeper(sceneName, buildingManager.beeKeeper); 
            SaveCampfire(sceneName, buildingManager.campfires);
            SaveDynamite(sceneName, buildingManager.dynamites);
            SaveMine(sceneName, buildingManager.mines);
            SaveWoodWall(sceneName, buildingManager.woodWalls);
            SaveBarbwire(sceneName, buildingManager.barbWires);
            SaveTotem(sceneName, buildingManager.totems);
            SavePetTrainer(sceneName, buildingManager.petTrainers);
            SaveCultivableField(sceneName, buildingManager.cultivableFields);
            SaveStreetLamps(sceneName, buildingManager.streetLamps);
            SaveFlag(sceneName, buildingManager.flags);
            SaveFloor(sceneName, buildingManager.modularPieces);
            SaveModularObject(sceneName, buildingManager.modularObjects);
            connection.Commit();
        }
    }

    public void LoadBuilding(string sceneName)
    {
        LoadBeeKeeper(buildingManager.beeKeeperObject, sceneName);
        LoadCampfire(buildingManager.campfiresObject, sceneName);
        LoadDynamite(buildingManager.dynamitesObject, sceneName);
        LoadMine(buildingManager.minesObject, sceneName);
        LoadWoodWall(buildingManager.woodWallsObject[0], sceneName);
        LoadBarbwire(buildingManager.barbwiresObject[0], sceneName);
        LoadTotem(buildingManager.totemsObject, sceneName);
        LoadPetTrainer(buildingManager.petTrainersObject, sceneName);
        LoadCultivableField(buildingManager.cultivableFieldsObject, sceneName);
        LoadStreetLamps(buildingManager.streetLampsObject, sceneName);
        LoadFlag(buildingManager.flagObject, sceneName);
        LoadFloor(buildingManager.modularPiece, sceneName);
        LoadModularObject(sceneName);

        buildingManager.isLoaded = true;
    }

    public void SaveCustom(Player player)
    {
        SaveAbilities(player);
        SaveBoost(player);
        SaveBlood(player);
        SaveFriends(player);
        SaveFriendsRequest(player);
        SaveHungry(player);
        SaveLeaderpoint(player);
        SaveEmoji(player);
        SaveDance(player);
        SavePartner(player);
        SaveOptions(player);
        SavePoisoning(player);
        SaveRadio(player);
        SaveTorch(player);
        SaveSpawnpoint(player);
        SaveGuildAlly(player);
        SaveFriends(player);
        SaveOptions(player);
        SaveThirsty(player);
        SaveAdditionalEquipment(player);
        SaveAdditionalInventory(player);
        SaveBelt(player);
        SaveCharacterCreation(player);
        SaveQuestCustom(player);
    }

    public void LoadCustom(Player player)
    {
        LoadAbilities(player);
        LoadBoost(player);
        LoadBlood(player);
        LoadFriends(player);
        LoadFriendsRequest(player);
        LoadHungry(player);
        LoadLeaderpoint(player);
        LoadEmoji(player);
        LoadDance(player);
        LoadPartner(player);
        LoadOptions(player);
        LoadPoisoning(player);
        LoadRadio(player);
        LoadTorch(player);
        LoadSpawnpoint(player);
        LoadGuilAlly(player);
        LoadFriends(player);
        LoadOptions(player);
        LoadThirsty(player);
        LoadAdditionalEquipment(player);
        LoadAdditionalInventory(player);
        LoadCharacterCreation(player);
        LoadQuestCustom(player);
    }

    public void LoadCustomFriendStat(Player player)
    {
        LoadAbilities(player);
        LoadBoost(player);
        LoadPartner(player);
        LoadGuilAlly(player);
        LoadCharacterCreation(player);
    }

    // character data //////////////////////////////////////////////////////////
    public bool CharacterExistsForCreation(string characterName)
    {
        // checks deleted ones too so we don't end up with duplicates if we un-
        // delete one
        return connection.FindWithQuery<characters>("SELECT * FROM characters WHERE name=? and banned = 0", characterName) != null;
    }
}