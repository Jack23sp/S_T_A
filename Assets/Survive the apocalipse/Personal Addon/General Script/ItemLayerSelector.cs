using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemLayerSelector : MonoBehaviour
{
    public List<GameObject> objectList = new List<GameObject>();

    private int index = -1;

    public Player player;

    public void SetLayer()
    {
        for(int i = 0; i < objectList.Count; i++)
        {
            if (!player.isLocalPlayer)
            {
                index = i;
                objectList[index].layer = LayerMask.NameToLayer("MyRenderPlayer");
            }
        }
        Destroy(this);
    }
}
