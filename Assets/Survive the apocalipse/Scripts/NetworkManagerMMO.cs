// We use a custom NetworkManager that also takes care of login, character
// selection, character creation and more.
//
// We don't use the playerPrefab, instead all available player classes should be
// dragged into the spawnable objects property.
//
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using Mirror;
using AdvancedCustomizableSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

// we need a clearly defined state to know if we are offline/in world/in lobby
// otherwise UICharacterSelection etc. never know 100% if they should be visible
// or not.
public enum NetworkState { Offline, Handshake, Lobby, World }

[RequireComponent(typeof(Database))]
public partial class NetworkManagerMMO : NetworkManager
{
    // current network manager state on client
    public NetworkState state = NetworkState.Offline;

    // <conn, account> dict for the lobby
    // (people that are still creating or selecting characters)
    public Dictionary<NetworkConnection, string> lobby = new Dictionary<NetworkConnection, string>();

    // UI components to avoid FindObjectOfType
    [Header("UI")]
    public UIPopup uiPopup;

    // we may want to add another game server if the first one gets too crowded.
    // the server list allows people to choose a server.
    //
    // note: we use one port for all servers, so that a headless server knows
    // which port to bind to. otherwise it would have to know which one to
    // choose from the list, which is far too complicated. one port for all
    // servers will do just fine for an Indie MMORPG.
    [Serializable]
    public class ServerInfo
    {
        public string name;
        public string ip;
    }
    public List<ServerInfo> serverList = new List<ServerInfo>() {
        new ServerInfo{name="Local", ip="localhost"}
    };

    [Header("Logout")]
    [Tooltip("Players shouldn't be able to log out instantly to flee combat. There should be a delay.")]
    public float combatLogoutDelay = 5;

    [Header("Character Selection")]
    public int selection = -1;
    public Transform[] selectionLocations;
    public Transform selectionCameraLocation;
    public List<Player> playerClasses = new List<Player>(); // cached in Awake

    [Header("Database")]
    public int characterLimit = 4;
    public int characterNameMaxLength = 16;
    public float saveInterval = 60f; // in seconds

    private CharacterCustomization characterCustomization;

    // store characters available message on client so that UI can access it
    [HideInInspector] public CharactersAvailableMsg charactersAvailableMsg;

    // name checks /////////////////////////////////////////////////////////////
    public bool IsAllowedCharacterName(string characterName)
    {
        // not too long?
        // only contains letters, number and underscore and not empty (+)?
        // (important for database safety etc.)
        return characterName.Length <= characterNameMaxLength &&
               Regex.IsMatch(characterName, @"^[a-zA-Z0-9_]+$");
    }

    // nearest startposition ///////////////////////////////////////////////////
    public static Transform GetNearestStartPosition(Vector2 from) =>
        Utils.GetNearestTransform(startPositions, from);

    // player classes //////////////////////////////////////////////////////////]
    public List<Player> FindPlayerClasses()
    {
        // filter out all Player prefabs from spawnPrefabs
        // (avoid Linq for performance/gc. players are spawned a lot. it matters.)
        List<Player> classes = new List<Player>();
        foreach (GameObject prefab in spawnPrefabs)
        {
            Player player = prefab.GetComponent<Player>();
            if (player != null)
                classes.Add(player);
        }
        return classes;
    }

    // events //////////////////////////////////////////////////////////////////
    public override void Awake()
    {
        base.Awake();

        // cache list of player classes from spawn prefabs.
        // => we assume that this won't be changed at runtime (why would it?)
        // => this is way better than looping all prefabs in character
        //    select/create/delete each time!
        playerClasses = FindPlayerClasses();
    }

    public override void Start()
    {
        // call base function
        base.Start();

        // addon system hooks
        Utils.InvokeMany(typeof(NetworkManagerMMO), this, "Start_");
    }

    void Update()
    {
        // any valid local player? then set state to world
        if (ClientScene.localPlayer != null)
            state = NetworkState.World;
    }

    // client popup messages ///////////////////////////////////////////////////
    public void ServerSendError(NetworkConnection conn, string error, bool disconnect)
    {
        conn.Send(new ErrorMsg { text = error, causesDisconnect = disconnect });
    }

    void OnClientError(NetworkConnection conn, ErrorMsg message)
    {
        print("OnClientError: " + message.text);

        // show a popup
        GameObject popUp = Instantiate(GeneralManager.singleton.popUpAutomicaticName, GeneralManager.singleton.canvas);
        UIAutomaticName popUpNotification = GetComponent<UIAutomaticName>();
        popUpNotification.pickName.gameObject.SetActive(false);
        popUpNotification.message.text = message.text;


        // disconnect if it was an important network error
        // (this is needed because the login failure message doesn't disconnect
        //  the client immediately (only after timeout))
        if (message.causesDisconnect)
        {
            conn.Disconnect();

            // also stop the host if running as host
            // (host shouldn't start server but disconnect client for invalid
            //  login, which would be pointless)
            if (NetworkServer.active) StopHost();
        }
    }

    // start & stop ////////////////////////////////////////////////////////////
    public override void OnStartClient()
    {
        // setup handlers
        NetworkClient.RegisterHandler<ErrorMsg>(OnClientError, false); // allowed before auth!
        NetworkClient.RegisterHandler<CharactersAvailableMsg>(OnClientCharactersAvailable);

        // addon system hooks
        Utils.InvokeMany(typeof(NetworkManagerMMO), this, "OnStartClient_");
    }

    public override void OnStartServer()
    {
        // connect to database
        Database.singleton.Connect();

        // handshake packet handlers
        NetworkServer.RegisterHandler<CharacterCreateMsg>(OnServerCharacterCreate);
        NetworkServer.RegisterHandler<CharacterSelectMsg>(OnServerCharacterSelect);
        NetworkServer.RegisterHandler<CharacterDeleteMsg>(OnServerCharacterDelete);

        // invoke saving
        InvokeRepeating(nameof(SavePlayers), saveInterval, saveInterval);

        // addon system hooks
        Utils.InvokeMany(typeof(NetworkManagerMMO), this, "OnStartServer_");
    }

    public override void OnStopServer()
    {
        print("OnStopServer");
        CancelInvoke(nameof(SavePlayers));

        // addon system hooks
        Utils.InvokeMany(typeof(NetworkManagerMMO), this, "OnStopServer_");
    }

    // handshake: login ////////////////////////////////////////////////////////
    public bool IsConnecting() => NetworkClient.active && !ClientScene.ready;

    // called on the client if a client connects after successful auth
    public override void OnClientConnect(NetworkConnection conn)
    {
        // addon system hooks
        Utils.InvokeMany(typeof(NetworkManagerMMO), this, "OnClientConnect_", conn);

        // call base function to make sure that client becomes "ready"
        //base.OnClientConnect(conn);
    }

    // called on the server if a client connects after successful auth
    public override void OnServerConnect(NetworkConnection conn)
    {
        // grab the account from the lobby
        string account = lobby[conn];

        // send necessary data to client
        conn.Send(MakeCharactersAvailableMessage(account));

        // addon system hooks
        Utils.InvokeMany(typeof(NetworkManagerMMO), this, "OnServerConnect_", conn);
    }

    // the default OnClientSceneChanged sets the client as ready automatically,
    // which makes no sense for MMORPG situations. this was more for situations
    // where the server tells all clients to load a new scene.
    // -> setting client as ready will cause 'already set as ready' errors if
    //    we call StartClient before loading a new scene (e.g. for zones)
    // -> it's best to just overwrite this with an empty function
    public override void OnClientSceneChanged(NetworkConnection conn) { }

    // helper function to make a CharactersAvailableMsg from all characters in
    // an account
    CharactersAvailableMsg MakeCharactersAvailableMessage(string account)
    {
        // load from database
        // (avoid Linq for performance/gc. characters are loaded frequently!)
        List<Player> characters = new List<Player>();
        foreach (string characterName in Database.singleton.CharactersForAccount(account))
        {
            GameObject player = Database.singleton.CharacterLoad(characterName, playerClasses, true);
            characters.Add(player.GetComponent<Player>());
        }

        // construct the message
        CharactersAvailableMsg message = new CharactersAvailableMsg();
        message.Load(characters);

        // destroy the temporary players again and return the result
        characters.ForEach(player => Destroy(player.gameObject));
        return message;
    }

    // handshake: character selection //////////////////////////////////////////
    void LoadPreview(GameObject prefab, Transform location, int selectionIndex, CharactersAvailableMsg.CharacterPreview character)
    {
        GeneralManager generalManager = FindObjectOfType<GeneralManager>();
        // instantiate the prefab
        GameObject preview = Instantiate(prefab.gameObject, location.position, location.rotation);
        preview.transform.parent = location;
        Player player = preview.GetComponent<Player>();

        // assign basic preview values like name and equipment
        player.name = character.name;
        for (int i = 0; i < character.equipment.Length; ++i)
        {
            ItemSlot slot = character.equipment[i];
            player.equipment.Add(slot);
            if (slot.amount > 0)
            {
                // OnEquipmentChanged won't be called unless spawned, we
                // need to refresh manually
                player.RefreshLocation(i);
                if (i == 2)
                {
                    if (slot.amount > 0)
                    {
                        player.playerCreation.upper = ((WeaponItem)slot.item.data).indexShirt;
                    }
                    else
                    {
                        player.playerCreation.upper = -1;
                    }
                }
                if (i == 3)
                {
                    if (character.equipment[2].amount > 0 && ((EquipmentItem)character.equipment[2].item.data).ignorePants == true)
                    {
                        player.playerCreation.down = -1;
                    }
                    else
                    {
                        if (slot.amount > 0)
                        {
                            player.playerCreation.down = ((WeaponItem)slot.item.data).indexPants;
                        }
                        else
                        {
                            player.playerCreation.down = -1;
                        }
                    }
                }
                if (i == 6)
                {
                    if (slot.amount > 0)
                    {
                        player.playerCreation.hats = ((WeaponItem)slot.item.data).indexHat;
                    }
                    else
                    {
                        player.playerCreation.hats = -1;
                    }
                }
                if (i == 8)
                {
                    if (slot.amount > 0)
                    {
                        player.playerCreation.shoes = ((WeaponItem)slot.item.data).indexShoes;
                    }
                    else
                    {
                        player.playerCreation.shoes = -1;
                    }
                }
                if (i == 10)
                {
                    if (slot.amount > 0)
                    {
                        player.playerCreation.accessory = ((WeaponItem)slot.item.data).indexAccessory;
                    }
                    else
                    {
                        player.playerCreation.accessory = -1;
                    }
                }    
            }
        }
            player.playerCreation.sex = character.sex;
            player.playerCreation.hairType = character.hairType;
            player.playerCreation.beard = character.beard;
            player.playerCreation.hairColor = character.hairColor;
            player.playerCreation.underwearColor = character.underwearColor;
            player.playerCreation.eyesColor = character.eyesColor;
            player.playerCreation.skinColor = character.skinColor;
            player.playerCreation.fat = character.fat;
            player.playerCreation.thin = character.thin;
            player.playerCreation.muscle = character.muscle;
            player.playerCreation.height = character.height;
            player.playerCreation.breast = character.breast;

        if(player.playerCreation.sex == 0)
        {
            GameObject prefabPreview = Instantiate(GeneralManager.singleton.malePrefab.gameObject, player.playerCreation.dummyPresentation.transform.position, player.playerCreation.dummyPresentation.transform.rotation);
            characterCustomization = prefabPreview.GetComponent<CharacterCustomization>();
            PlayerSelectionCameraPositioning playerSelectionCameraPositioning = characterCustomization.GetComponent<PlayerSelectionCameraPositioning>();
            characterCustomization.SetHairByIndex(player.playerCreation.hairType);
            characterCustomization.SetBeardByIndex(player.playerCreation.beard);
            Color newCol;
            if (ColorUtility.TryParseHtmlString(player.playerCreation.hairColor, out newCol))
                characterCustomization.SetBodyColor(BodyColorPart.Hair, newCol);
            if (ColorUtility.TryParseHtmlString(player.playerCreation.underwearColor, out newCol))
                characterCustomization.SetBodyColor(BodyColorPart.Underpants, newCol);
            if (ColorUtility.TryParseHtmlString(player.playerCreation.eyesColor, out newCol))
                characterCustomization.SetBodyColor(BodyColorPart.Eye, newCol);
            if (ColorUtility.TryParseHtmlString(player.playerCreation.skinColor, out newCol))
                characterCustomization.SetBodyColor(BodyColorPart.Skin, newCol);
            characterCustomization.SetBodyShape(BodyShapeType.Fat, player.playerCreation.fat);
            characterCustomization.SetBodyShape(BodyShapeType.Thin, player.playerCreation.thin);
            characterCustomization.SetBodyShape(BodyShapeType.Muscles, player.playerCreation.muscle);

            DressSelectablePlayer(player, characterCustomization);

            characterCustomization.SetBodyShape(BodyShapeType.BreastSize, player.playerCreation.breast);

            prefabPreview.transform.SetParent(player.transform);
            prefabPreview.GetComponentInParent<PlayerCreation>().percSlider = Mathf.InverseLerp(generalManager.minHeight, generalManager.maxHeight, player.playerCreation.height);

            float positioning = Convert.ToSingle((-0.3100281 / 100) * (prefabPreview.GetComponentInParent<PlayerCreation>().percSlider * 100));

            characterCustomization.SetHeight(player.playerCreation.height);
            playerSelectionCameraPositioning.player = player;

            prefabPreview.transform.localPosition = new Vector3(0.0f, positioning, 0.0f);
        }
        else
        {
            GameObject prefabPreview = Instantiate(GeneralManager.singleton.femalePrefab.gameObject, player.playerCreation.dummyPresentation.transform.position, player.playerCreation.dummyPresentation.transform.rotation);
            characterCustomization = prefabPreview.GetComponent<CharacterCustomization>();
            PlayerSelectionCameraPositioning playerSelectionCameraPositioning = characterCustomization.GetComponent<PlayerSelectionCameraPositioning>();
            characterCustomization.SetHairByIndex(player.playerCreation.hairType);
            characterCustomization.SetBeardByIndex(player.playerCreation.beard);
            Color newCol;
            if (ColorUtility.TryParseHtmlString(player.playerCreation.hairColor, out newCol))
                characterCustomization.SetBodyColor(BodyColorPart.Hair, newCol);
            if (ColorUtility.TryParseHtmlString(player.playerCreation.underwearColor, out newCol))
                characterCustomization.SetBodyColor(BodyColorPart.Underpants, newCol);
            if (ColorUtility.TryParseHtmlString(player.playerCreation.eyesColor, out newCol))
                characterCustomization.SetBodyColor(BodyColorPart.Eye, newCol);
            if (ColorUtility.TryParseHtmlString(player.playerCreation.skinColor, out newCol))
                characterCustomization.SetBodyColor(BodyColorPart.Skin, newCol);
            characterCustomization.SetBodyShape(BodyShapeType.Fat, player.playerCreation.fat);
            characterCustomization.SetBodyShape(BodyShapeType.Thin, player.playerCreation.thin);
            characterCustomization.SetBodyShape(BodyShapeType.Muscles, player.playerCreation.muscle);

            DressSelectablePlayer(player, characterCustomization);

            characterCustomization.SetBodyShape(BodyShapeType.BreastSize, player.playerCreation.breast);

            prefabPreview.transform.SetParent(player.transform);
            prefabPreview.GetComponentInParent<PlayerCreation>().percSlider = Mathf.InverseLerp(generalManager.minHeight, generalManager.maxHeight, player.playerCreation.height);

            float positioning = Convert.ToSingle((-0.3100281 / 100) * (prefabPreview.GetComponentInParent<PlayerCreation>().percSlider * 100));

            characterCustomization.SetHeight(player.playerCreation.height);
            playerSelectionCameraPositioning.player = player;

            prefabPreview.transform.localPosition = new Vector3(0.0f, positioning, 0.0f);
        }

       

        if (player.GetComponent<NotOnlinePlayerPremiumManager>())player.GetComponent<NotOnlinePlayerPremiumManager>().inPremiumZone = character.premiumZone;

        // add selection script
        preview.AddComponent<SelectableCharacter>();
        preview.GetComponent<SelectableCharacter>().index = selectionIndex;
    }

    public void DressSelectablePlayer(Player player, CharacterCustomization characterCustomization)
    {
        if (player.playerCreation.hats != -1)
        {
            characterCustomization.SetElementByIndex(ClothesPartType.Hat, player.playerCreation.hats);
        }
        else
        {
            characterCustomization.SetElementByIndex(ClothesPartType.Hat, -1);
        }

        if (player.playerCreation.accessory != -1)
        {
            characterCustomization.SetElementByIndex(ClothesPartType.Accessory, player.playerCreation.accessory);
        }
        else
        {
            characterCustomization.SetElementByIndex(ClothesPartType.Accessory, -1);
        }

        if (player.playerCreation.upper != -1)
        {
            characterCustomization.SetElementByIndex(ClothesPartType.Shirt, player.playerCreation.upper);
        }
        else
        {
            characterCustomization.SetElementByIndex(ClothesPartType.Shirt, -1);
        }

        if (player.playerCreation.down != -1)
        {
            characterCustomization.SetElementByIndex(ClothesPartType.Pants, player.playerCreation.down);
        }
        else
        {
            characterCustomization.SetElementByIndex(ClothesPartType.Pants, -1);
        }

        if (player.playerCreation.shoes != -1)
        {
            characterCustomization.SetElementByIndex(ClothesPartType.Shoes, player.playerCreation.shoes);
        }
        else
        {
            characterCustomization.SetElementByIndex(ClothesPartType.Shoes, -1);
        }
    }

    public void ClearPreviews()
    {
        selection = -1;
        foreach (Transform location in selectionLocations)
            if (location.childCount > 0)
                Destroy(location.GetChild(0).gameObject);
    }

    void OnClientCharactersAvailable(NetworkConnection conn, CharactersAvailableMsg message)
    {
        charactersAvailableMsg = message;
        print("characters available:" + charactersAvailableMsg.characters.Length);

        // set state
        state = NetworkState.Lobby;

        // clear previous previews in any case
        ClearPreviews();

        // load previews for 3D character selection
        for (int i = 0; i < charactersAvailableMsg.characters.Length; ++i)
        {
            CharactersAvailableMsg.CharacterPreview character = charactersAvailableMsg.characters[i];

            // find the prefab for that class
            Player prefab = playerClasses.Find(p => p.name == character.className);
            if (prefab != null)
                LoadPreview(prefab.gameObject, selectionLocations[i], i, character);
            else
                Debug.LogWarning("Character Selection: no prefab found for class " + character.className);
        }

        // setup camera
        Camera.main.depth = 2;
        //UICharacterSelection.singleton.creationCamera.depth = 0;
        Camera.main.transform.position = selectionCameraLocation.position;
        Camera.main.transform.rotation = selectionCameraLocation.rotation;

        // addon system hooks
        Utils.InvokeMany(typeof(NetworkManagerMMO), this, "OnClientCharactersAvailable_", charactersAvailableMsg);
    }

    // overwrite the original OnServerAddPlayer function so nothing happens if
    // someone sends that message.
    public override void OnServerAddPlayer(NetworkConnection conn) { Debug.LogWarning("Use the CharacterSelectMsg instead"); }

    void OnServerCharacterSelect(NetworkConnection conn, CharacterSelectMsg message)
    {
        //print("OnServerCharacterSelect");
        // only while in lobby (aka after handshake and not ingame)
        if (lobby.ContainsKey(conn))
        {
            // read the index and find the n-th character
            // (only if we know that he is not ingame, otherwise lobby has
            //  no netMsg.conn key)
            string account = lobby[conn];
            List<string> characters = Database.singleton.CharactersForAccount(account);

            // validate index
            if (0 <= message.index && message.index < characters.Count)
            {
                //print(account + " selected player " + characters[index]);

                // load character data
                GameObject go = Database.singleton.CharacterLoad(characters[message.index], playerClasses, false);
                // add to client
                NetworkServer.AddPlayerForConnection(conn, go);

                // addon system hooks
                Utils.InvokeMany(typeof(NetworkManagerMMO), this, "OnServerCharacterSelect_", account, go, conn, message);
#pragma warning disable CS0618 // AddPlayerMessage.value is obsolete
                //Utils.InvokeMany(typeof(NetworkManagerMMO), this, "OnServerAddPlayer_", account, go, conn, new AddPlayerMessage { value = BitConverter.GetBytes(message.index) }); // old hook
#pragma warning restore CS0618 // AddPlayerMessage.value is obsolete
                // remove from lobby
                lobby.Remove(conn);
            }
            else
            {
                print("invalid character index: " + account + " " + message.index);
                ServerSendError(conn, "invalid character index", false);
            }
        }
        else
        {
            print("CharacterSelect: not in lobby" + conn);
            ServerSendError(conn, "CharacterSelect: not in lobby", true);
        }
    }

    // handshake: character creation ///////////////////////////////////////////
    // find a NetworkStartPosition for this class, or a normal one otherwise
    // (ignore the ones with playerPrefab == null)
    public Transform GetStartPositionFor(string className)
    {
        // avoid Linq for performance/GC. players spawn frequently!
        foreach (Transform startPosition in startPositions)
        {
            NetworkStartPositionForClass spawn = startPosition.GetComponent<NetworkStartPositionForClass>();
            if (spawn != null &&
                spawn.playerPrefab != null &&
                spawn.playerPrefab.name == className)
                return spawn.transform;
        }
        // return any start position otherwise
        return GetStartPosition();
    }

    Player CreateCharacter(GameObject classPrefab, string characterName, string account, CharacterCreateMsg message)
    {
        // create new character based on the prefab.
        // -> we also assign default items and equipment for new characters
        // -> skills are handled in Database.CharacterLoad every time. if we
        //    add new ones to a prefab, all existing players should get them
        // (instantiate temporary player)
        //print("creating character: " + message.name + " " + message.classIndex);
        Player player = Instantiate(classPrefab).GetComponent<Player>();
        player.name = characterName;
        player.account = account;
        player.className = classPrefab.name;
        player.transform.position = GetStartPositionFor(player.className).position;
        for (int i = 0; i < player.inventorySize; ++i)
        {
            // add empty slot or default item if any
            player.inventory.Add(i < player.defaultItems.Length ? new ItemSlot(new Item(player.defaultItems[i].item), player.defaultItems[i].amount) : new ItemSlot());
        }
        for (int i = 0; i < player.equipmentInfo.Length; ++i)
        {
            // add empty slot or default item if any
            EquipmentInfo info = player.equipmentInfo[i];
            player.equipment.Add(info.defaultItem.item != null ? new ItemSlot(new Item(info.defaultItem.item), info.defaultItem.amount) : new ItemSlot());
        }
        player.health = player.healthMax; // after equipment in case of boni
        player.mana = player.manaMax; // after equipment in case of boni
        player.playerCreation.sex = message.sex;
        player.playerCreation.hairType = message.hairType;
        player.playerCreation.beard = message.beard;
        player.playerCreation.hairColor = message.hairColor;
        player.playerCreation.underwearColor = message.underwearColor;
        player.playerCreation.eyesColor = message.eyesColor;
        player.playerCreation.skinColor = message.skinColor;
        player.playerCreation.fat = message.fat;
        player.playerCreation.thin = message.thin;
        player.playerCreation.muscle = message.muscle;
        player.playerCreation.height = message.height;
        player.playerCreation.breast = message.breast;

        return player;
    }

    void OnServerCharacterCreate(NetworkConnection conn, CharacterCreateMsg message)
    {
        //print("OnServerCharacterCreate " + conn);

        // only while in lobby (aka after handshake and not ingame)
        if (lobby.ContainsKey(conn))
        {

            // allowed character name?
            if (IsAllowedCharacterName(message.name))
            {
                // not existant yet?
                string account = lobby[conn];
                if (!Database.singleton.CharacterExists(message.name))
                {
                    // not too may characters created yet?
                    if (Database.singleton.CharactersForAccount(account).Count < characterLimit)
                    {
                        // valid class index?
                        if (0 <= message.classIndex && message.classIndex < playerClasses.Count)
                        {
                            // create new character based on the prefab.
                            Player player = CreateCharacter(playerClasses[message.classIndex].gameObject, message.name, account, message);

                            // addon system hooks
                            Utils.InvokeMany(typeof(NetworkManagerMMO), this, "OnServerCharacterCreate_", message, player);

                            // save the player
                            Database.singleton.CharacterSave(player, false);
                            Destroy(player.gameObject);

                            // send available characters list again, causing
                            // the client to switch to the character
                            // selection scene again
                            conn.Send(MakeCharactersAvailableMessage(account));
                        }
                        else
                        {
                            //print("character invalid class: " + message.classIndex); <- don't show on live server
                            ServerSendError(conn, "character invalid class", false);
                        }
                    }
                    else
                    {
                        //print("character limit reached: " + message.name); <- don't show on live server
                        ServerSendError(conn, "character limit reached", false);
                    }
                }
                else
                {
                    //print("character name already exists: " + message.name); <- don't show on live server
                    ServerSendError(conn, "name already exists", false);
                }
            }
            else
            {
                //print("character name not allowed: " + message.name); <- don't show on live server
                ServerSendError(conn, "character name not allowed", false);
            }
        }
        else
        {
            //print("CharacterCreate: not in lobby"); <- don't show on live server
            ServerSendError(conn, "CharacterCreate: not in lobby", true);
        }
    }

    void OnServerCharacterDelete(NetworkConnection conn, CharacterDeleteMsg message)
    {
        //print("OnServerCharacterDelete " + conn);

        // only while in lobby (aka after handshake and not ingame)
        if (lobby.ContainsKey(conn))
        {
            string account = lobby[conn];
            List<string> characters = Database.singleton.CharactersForAccount(account);

            // validate index
            if (0 <= message.index && message.index < characters.Count)
            {
                // delete the character
                print("delete character: " + characters[message.index]);
                Database.singleton.CharacterDelete(characters[message.index]);

                // addon system hooks
                Utils.InvokeMany(typeof(NetworkManagerMMO), this, "OnServerCharacterDelete_", message);

                // send the new character list to client
                conn.Send(MakeCharactersAvailableMessage(account));
            }
            else
            {
                print("invalid character index: " + account + " " + message.index);
                ServerSendError(conn, "invalid character index", false);
            }
        }
        else
        {
            print("CharacterDelete: not in lobby: " + conn);
            ServerSendError(conn, "CharacterDelete: not in lobby", true);
        }
    }

    // player saving ///////////////////////////////////////////////////////////
    // we have to save all players at once to make sure that item trading is
    // perfectly save. if we would invoke a save function every few minutes on
    // each player seperately then it could happen that two players trade items
    // and only one of them is saved before a server crash - hence causing item
    // duplicates.
    void SavePlayers()
    {
        Database.singleton.CharacterSaveMany(Player.onlinePlayers.Values);
        if (Player.onlinePlayers.Count > 0) Debug.Log("saved " + Player.onlinePlayers.Count + " player(s)");
    }

    // stop/disconnect /////////////////////////////////////////////////////////
    // called on the server when a client disconnects
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        //print("OnServerDisconnect " + conn);

        // players shouldn't be able to log out instantly to flee combat.
        // there should be a delay.
        float delay = 0;
        if (conn.identity != null)
        {
            Player player = conn.identity.GetComponent<Player>();
            delay = (float)player.remainingLogoutTime;
        }

        StartCoroutine(DoServerDisconnect(conn, delay));
    }

    IEnumerator<WaitForSeconds> DoServerDisconnect(NetworkConnection conn, float delay)
    {
        yield return new WaitForSeconds(delay);

        //print("DoServerDisconnect " + conn);

        // save player (if any. nothing to save if disconnecting while in lobby.)
        if (conn.identity != null)
        {
            Database.singleton.CharacterSave(conn.identity.GetComponent<Player>(), false);
            print("saved:" + conn.identity.name);
        }

        // addon system hooks
        Utils.InvokeMany(typeof(NetworkManagerMMO), this, "OnServerDisconnect_", conn);

        // remove logged in account after everything else was done
        lobby.Remove(conn); // just returns false if not found

        // do base function logic (removes the player for the connection)
        base.OnServerDisconnect(conn);
    }

    // called on the client if he disconnects
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        print("OnClientDisconnect");

        // show a popup so that users know what happened
        uiPopup.Show("Disconnected.");

        // call base function to guarantee proper functionality
        base.OnClientDisconnect(conn);

        // set state
        state = NetworkState.Offline;

        // addon system hooks
        Utils.InvokeMany(typeof(NetworkManagerMMO), this, "OnClientDisconnect_", conn);
    }

    // universal quit function for editor & build
    public static void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // called when quitting the application by closing the window / pressing
    // stop in the editor
    // -> we want to send the quit packet to the server instead of waiting for a
    //    timeout
    new void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        if (NetworkClient.isConnected)
        {
            StopClient();
            print("OnApplicationQuit: stopped client");
        }
    }

    new void OnValidate()
    {
        base.OnValidate();

        // ip has to be changed in the server list. make it obvious to users.
        if (!Application.isPlaying && networkAddress != "")
            networkAddress = "Use the Server List below!";

        // need enough character selection locations for character limit
        if (selectionLocations.Length != characterLimit)
        {
            // create new array with proper size
            Transform[] newArray = new Transform[characterLimit];

            // copy old values
            for (int i = 0; i < Mathf.Min(characterLimit, selectionLocations.Length); ++i)
                newArray[i] = selectionLocations[i];

            // use new array
            selectionLocations = newArray;
        }
    }
}