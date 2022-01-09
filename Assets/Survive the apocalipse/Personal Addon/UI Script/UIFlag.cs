using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class UIFlag : MonoBehaviour
{
    public TMP_InputField nameInput;
    public Transform content;
    public GameObject flagObject;
    public Button spawnFlag;
    public Button closeButton;

    public GameObject flagBuildingObject;

    private string searchedFlag;

    public List<Sprite> flagImage = new List<Sprite>();

    public List<Sprite> searchedFlagImage = new List<Sprite>();

    private Player player;

    public string selectedFlag;

    public TextMeshProUGUI selectedNationText;

    public void Start()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        searchedFlag = nameInput.text;
        flagImage = GeneralManager.singleton.flagSprite;
        flagImage.RemoveAt(0);

        UIUtils.BalancePrefabs(flagObject, (flagImage.Count), content);
        for (int i = 0; i < flagImage.Count; i++)
        {
            FlagSlot flagSlot = content.GetChild(i).GetComponent<FlagSlot>();
            flagSlot.flagImage.sprite = flagImage[i];
            flagSlot.flagName.text = flagImage[i].name;            
        }

    }

    public void Update()
    {
        if (player.health == 0) closeButton.onClick.Invoke();

        closeButton.onClick.SetListener(() =>
        {
            Player.localPlayer.playerBuilding.invBelt = false;
            Player.localPlayer.playerBuilding.inventoryIndex = -1;
            Player.localPlayer.playerBuilding.building = null;
            Player.localPlayer.playerBuilding.flagSelectedNation = string.Empty;
            Destroy(this.gameObject);
        });

        spawnFlag.onClick.SetListener(() =>
        {
            GameObject g = Instantiate(flagBuildingObject, new Vector3(player.transform.position.x, player.transform.position.y, 0.0f), Quaternion.identity);
            g.GetComponent<Flag>().selectedNation = selectedFlag;
            player.playerBuilding.flagSelectedNation = selectedFlag;
            Player.localPlayer.playerBuilding.actualBuilding = g;

            if (GeneralManager.singleton.spawnedAttackObject)
            {
                Destroy(GeneralManager.singleton.spawnedAttackObject);
                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
            }
            else
            {
                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
            }
            Destroy(this.gameObject);
        });

        if (nameInput.text != string.Empty)
        {
            searchedFlagImage.Clear();

            for(int e = 0; e < flagImage.Count; e++)
            {
                if(flagImage[e].name.ToUpper().Contains(nameInput.text.ToUpper()))
                {
                    searchedFlagImage.Add(flagImage[e]);
                }
            }

            UIUtils.BalancePrefabs(flagObject, (searchedFlagImage.Count), content);
            for (int i = 0; i < searchedFlagImage.Count; i++)
            {
                int index = i;
                FlagSlot flagSlot = content.GetChild(index).GetComponent<FlagSlot>();
                flagSlot.flagImage.sprite = searchedFlagImage[index];
                flagSlot.flagName.text = searchedFlagImage[index].name;
                flagSlot.button.onClick.SetListener(() =>
                {
                    selectedNationText.text = searchedFlagImage[index].name;
                    selectedFlag = selectedNationText.text;
                });
            }
        }
        else if(nameInput.text == string.Empty)
        {
            UIUtils.BalancePrefabs(flagObject, (flagImage.Count), content);
            for (int i = 0; i < flagImage.Count; i++)
            {
                int index = i;
                FlagSlot flagSlot = content.GetChild(index).GetComponent<FlagSlot>();
                flagSlot.flagImage.sprite = flagImage[index];
                flagSlot.flagName.text = flagImage[index].name;
                flagSlot.button.onClick.SetListener(() =>
                {
                    selectedNationText.text = flagImage[index].name;
                    selectedFlag = selectedNationText.text;
                });
            }
        }

        spawnFlag.interactable = selectedFlag != string.Empty;
    }

}
