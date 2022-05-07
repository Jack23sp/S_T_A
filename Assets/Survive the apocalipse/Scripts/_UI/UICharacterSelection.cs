// Simple character selection list. The charcter prefabs are known, so we could
// easily show 3D models, stats, etc. too .
using System;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public partial class UICharacterSelection : MonoBehaviour
{
    public static UICharacterSelection singleton;
    public UICharacterCreation uiCharacterCreation;
    public UIConfirmation uiConfirmation;
    public NetworkManagerMMO manager; // singleton is null until update
    public GameObject panel;
    public Button startButton;
    public Button deleteButton;
    public Button createButton;
    public Button quitButton;

    public Camera mainCamera;
    public Camera creationCamera;
    public GameObject creationPanel;
    public GameObject creationColor;

    public Transform dummySpot;

    [Header("New character selection")]
    public Transform content;
    public GameObject toSpawn;

    public RawImage bodyRawImage;
    public string selectedCharacter;
    public string prevSelectedCharacter;

    public Player[] players;

    public int playerCount = 0;
    public int prevPlayerCount = 0;

    [HideInInspector] public CharactersAvailableMsg.CharacterPreview[] characters;

    public Player selectedGameObjectPlayer;

    public UIStatisticStart statisticStart;
    public GameObject statContent;
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI guildName;

    public void Stert()
    {
        if (!singleton) singleton = this;
    }

    void Update()
    {
        if(Player.localPlayer && Player.localPlayer.isLocalPlayer)
        {
            //Destroy(creationCamera.gameObject);
            Destroy(this.gameObject);
        }

        // show while in lobby and while not creating a character
        if (manager.state == NetworkState.Lobby && !uiCharacterCreation.IsVisible())
        {
            panel.SetActive(creationCamera.depth == 0);

            // characters available message received already?
            if (manager.charactersAvailableMsg != null)
            {
                characters = manager.charactersAvailableMsg.characters;

                playerCount = characters.Length;
                if (playerCount != prevPlayerCount)
                {                 
                    players = FindObjectsOfType<Player>();
                    prevPlayerCount = playerCount;
                }

                UIUtils.BalancePrefabs(toSpawn, characters.Length, content);
                for(int i = 0; i < characters.Length; i++)
                {
                    int previndex = i;
                    CharacterSelectionSlot slot = content.GetChild(previndex).GetComponent<CharacterSelectionSlot>();
                    slot.nameText.text = characters[previndex].name;
                    slot.effect.SetActive(characters[previndex].premiumZone);
                    for(int e = 0; e < players.Length; e++)
                    {
                        int index = e;
                        if(players[index].name == characters[previndex].name)
                        {
                            if(players[index].avatarCamera) slot.faceImage.texture = players[index].avatarCamera.activeTexture;
                        }
                    }
                    slot.selectionButton.onClick.SetListener(() => {
                        selectedCharacter = characters[previndex].name;
                        selectedGameObjectPlayer = manager.selectionLocations[previndex].GetComponentInChildren<Player>();
                        statContent.SetActive(true);
                        statisticStart.SetStatistics();
                        playerName.text = selectedGameObjectPlayer.name;
                        guildName.text = selectedGameObjectPlayer.guild.name;
                        bodyRawImage.gameObject.SetActive(characters.Length > 0 && selectedCharacter != string.Empty);
                        if (prevSelectedCharacter == string.Empty) prevSelectedCharacter = "Dummy";
                        ((NetworkManagerMMO)NetworkManager.singleton).selection = previndex;
                    });
                }

                if(selectedCharacter != string.Empty)
                {
                    if (prevSelectedCharacter != selectedCharacter)
                    {
                        for (int i = 0; i < players.Length; i++)
                        {
                            if (players[i] != null && players[i].name == selectedCharacter)
                            {
                                //players[i].animator.Play("Base Layer.Wave Down", 0, 0);
                                prevSelectedCharacter = selectedCharacter;
                                bodyRawImage.texture = players[i].avatarCamera.activeTexture;
                            }
                        }
                    }
                }
                else
                {
                    if (characters.Length > 0)
                    {
                        content.GetChild(0).GetComponent<CharacterSelectionSlot>().selectionButton.onClick.Invoke();
                        selectedCharacter = content.GetChild(0).GetComponent<CharacterSelectionSlot>().nameText.text;
                    }
                }

                startButton.gameObject.SetActive(manager.selection != -1);
                startButton.onClick.SetListener(() => {

                    for (int i = 0; i < characters.Length; i++)
                    {
                        int previndex = i;
                        CharacterSelectionSlot slot = content.GetChild(previndex).GetComponent<CharacterSelectionSlot>();
                        slot.gameObject.SetActive(false);
                    }
                    bodyRawImage.gameObject.SetActive(false);

                    UICharacterCreationCustom.singleton.maleCharacterCustomization.gameObject.SetActive(false);
                    UICharacterCreationCustom.singleton.femaleCharacterCustomization.gameObject.SetActive(false);

                    // set client "ready". we will receive world messages from
                    // monsters etc. then.
                    ClientScene.Ready(NetworkClient.connection);

                    // send CharacterSelect message (need to be ready first!)
                    NetworkClient.connection.Send(new CharacterSelectMsg{ index=manager.selection });

                    // clear character selection previews
                    manager.ClearPreviews();

                    // make sure we can't select twice and call AddPlayer twice
                    panel.SetActive(false);
                });

                // delete button
                deleteButton.gameObject.SetActive(manager.selection != -1);
                deleteButton.onClick.SetListener(() => {
                    //uiConfirmation.Show(
                    //    "Do you really want to delete <b>" + characters[manager.selection].name + "</b>?",
                    //    () => { NetworkClient.Send(new CharacterDeleteMsg{index=manager.selection}); }
                    //);
                    uiConfirmation.gameObject.SetActive(true);
                    UIConfirmation.singleton.transform.GetChild(0).gameObject.SetActive(true);
                });

                // create button
                createButton.interactable = characters.Length < manager.characterLimit;
                createButton.onClick.SetListener(() => {
                    //panel.SetActive(false);
                    //uiCharacterCreation.Show();
                    creationCamera.depth = 1;
                    mainCamera.depth = 0;
                    //mainCamera.transform.GetChild(0).gameObject.SetActive(false);
                    panel.SetActive(false);
                    creationPanel.SetActive(true);
                });

                // quit button
                //quitButton.onClick.SetListener(() => { NetworkManagerMMO.Quit(); });
            }
        }
        else panel.SetActive(false);
    }
}
