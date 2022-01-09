using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class UIJoystick : MonoBehaviour {

    public GameObject panel;
    public Image imgPicture;
    public Image imgPicture2;
    public GameObject chat;
    private bool setted;

    void Update() {
        var player = Player.localPlayer;
        panel.SetActive(player != null); // hide while not in the game world
        if (!player) return;
		
        if(!setted && transform.GetSiblingIndex() >= chat.transform.GetSiblingIndex())
        {
            transform.SetSiblingIndex(chat.transform.GetSiblingIndex() - 1);
            setted = true;
        }
		
        // addon system hooks
        Utils.InvokeMany(typeof(UIJoystick), this, "Update_");
		
	}
}
