using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UIDeleteObject : MonoBehaviour
{
    public ScriptableBuilding scriptableBuilding;
    public NetworkIdentity identity;
    public Button closeButton;
    public Button removeButton;
    public Image itemImage;

    void Update()
    {
        closeButton.onClick.SetListener(() =>
        {
            Destroy(this.gameObject);
        });

        removeButton.onClick.SetListener(() =>
        {
            Player.localPlayer.CmdDeleteObject(identity);
            closeButton.onClick.Invoke();
        });
    }

    public void Assign(ScriptableBuilding building, NetworkIdentity netIdentity)
    {
        scriptableBuilding = building;
        itemImage.sprite = scriptableBuilding.image;
        identity = netIdentity;
    }
}
