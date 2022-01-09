using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIConfirmDeleteBuilding : MonoBehaviour
{
    [HideInInspector] public Player player;
    public Button AcceptButton;
    public Button DeclineButton;
    public Button closeButton;

    public void Update()
    {
        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

        if (!player) player = Player.localPlayer;
        if (!player) return;
        transform.GetChild(0).gameObject.SetActive(player.target != null && player.target.GetComponent<Building>() && player.CanInteractBuildingTarget(player.target.GetComponent<Building>(), player));

        AcceptButton.onClick.SetListener(() =>
        {
            player.CmdDestroyBuilding();
            Destroy(this.gameObject);
        });
        DeclineButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

        if (player.health == 0)
            closeButton.onClick.Invoke();
        
    }
}
