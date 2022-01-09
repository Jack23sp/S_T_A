using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public partial class UICharacterCreation : MonoBehaviour
{
    public static UICharacterCreation singleton;
    public NetworkManagerMMO manager; // singleton is null until update
    public GameObject panel;
    public InputField nameInput;
    public Dropdown classDropdown;
    public Button createButton;
    public Button cancelButton;

    public void Start()
    {
        if (!singleton) singleton = this;
    }

    void Update()
    {
        if(Player.localPlayer && Player.localPlayer.isLocalPlayer)
        {
            Destroy(this.gameObject);
        }

        // only update while visible (after character selection made it visible)
        if (panel.activeSelf)
        {
            // still in lobby?
            if (manager.state == NetworkState.Lobby)
            {
                Show();

                // copy player classes to class selection
                classDropdown.options = manager.playerClasses.Select(
                    p => new Dropdown.OptionData(p.name)
                ).ToList();

                // create
                createButton.interactable = manager.IsAllowedCharacterName(nameInput.text);
                //createButton.onClick.SetListener(() =>
                //{
                //    CharacterCreateMsg message = new CharacterCreateMsg
                //    {
                //        name = nameInput.text,
                //        classIndex = classDropdown.value,
                //        sex = UICharacterCreationCustom.singleton.selectedSex,
                //        skinColor = UICharacterCreationCustom.singleton.selectedSkinTemplate,
                //        hairColor = UICharacterCreationCustom.singleton.selectedHairColor,
                //        hairType = UICharacterCreationCustom.singleton.selectedHairType,
                //        eyesColor = UICharacterCreationCustom.singleton.selectedEyesColor
                //    };
                //    NetworkClient.Send(message);
                //    Hide();
                //});

                // cancel
                cancelButton.onClick.SetListener(() => {
                    nameInput.text = "";
                    Hide();
                });
            }
            else Hide();
        }
        else Hide();
    }

    public void Hide() { panel.SetActive(false); }
    public void Show() { panel.SetActive(true); }
    public bool IsVisible() { return panel.activeSelf; }
}
