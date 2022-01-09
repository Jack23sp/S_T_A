// small helper script that is added to character selection previews at runtime
using UnityEngine;
using Mirror;
using TMPro;

public class SelectableCharacter : MonoBehaviour
{
    // index will be set by networkmanager when creating this script
    public int index = -1;
    Player player;
    void OnMouseDown()
    {
        // set selection index
        ((NetworkManagerMMO)NetworkManager.singleton).selection = index;
        player = GetComponent<Player>();
        //player.animator.Play("Base Layer.Wave Down", 0, 0);
    }

    void Update()
    {
        // selected?
        bool selected = ((NetworkManagerMMO)NetworkManager.singleton).selection != index;

        // set name overlay font style as indicator
        if(!player) player = GetComponent<Player>();
        //player.nameOverlay.fontStyle = selected ? fontStyle.Normal : fontStyle.Bold;
        if (!selected)
        {
            player.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
        }
        else
        {
            player.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            //player.animator.Play("Base Layer.Stand Down", 0, 0);
        }
    }
}
