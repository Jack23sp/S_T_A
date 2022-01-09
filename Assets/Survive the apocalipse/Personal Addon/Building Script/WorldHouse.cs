using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WorldHouse : NetworkBehaviour
{
    public SyncListItemSlot items = new SyncListItemSlot();

    // Start is called before the first frame update
    void Start()
    {
        if(isServer)
        {
            InvokeRepeating("ManageItems", 0.0f, GeneralManager.singleton.intervalCheckWorldHouse);
        }
    }

    public void ManageItems()
    {
        if(items.Count < GeneralManager.singleton.maxItemInWorldHouse)
        {
            int diffItem = GeneralManager.singleton.maxItemInWorldHouse - items.Count;
            for(int i = 0; i < diffItem; i++)
            {
                int selectedItem = UnityEngine.Random.Range(0, GeneralManager.singleton.itemSpawner.Count);
                ItemSlot slot = new ItemSlot(new Item(GeneralManager.singleton.itemSpawner[selectedItem].items), UnityEngine.Random.Range(1, GeneralManager.singleton.itemSpawner[selectedItem].amount + 1));
                items.Add(slot);
            }
        }
    }
}
