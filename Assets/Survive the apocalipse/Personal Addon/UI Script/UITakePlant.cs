using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UITakePlant : MonoBehaviour
{
    public TextMeshProUGUI plantName;
    public Image itemImage;
    public Button itemButton;
    public Player player;

    public void Start()
    {
        player = Player.localPlayer;
        itemImage.sprite = player.playerPlant.selectedMedicalPlant.GetComponent<MedicalPlant>().reward.image;
        plantName.text = player.playerPlant.selectedMedicalPlant.GetComponent<MedicalPlant>().reward.name;
    }

    public void Update()
    {
        itemButton.onClick.SetListener(() =>
        {
            if(player.playerPlant.selectedMedicalPlant != null)
            {
                player.playerPlant.CmdAddPlant(player.playerPlant.selectedMedicalPlant.reward.name, player.playerPlant.selectedMedicalPlant.gameObject);
            }
            player.playerPlant.selectedMedicalPlant = null;
            Destroy(this.gameObject);
        });
    }

}
