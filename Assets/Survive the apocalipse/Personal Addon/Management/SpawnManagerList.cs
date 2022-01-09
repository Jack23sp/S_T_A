using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManagerList : MonoBehaviour
{
    public static SpawnManagerList singleton;

    public List<SpawnManager> spawnManagers = new List<SpawnManager>();

    // Start is called before the first frame update
    void Start()
    {
        if (!singleton) singleton = this;
        int index = 0;
        foreach (SpawnManager spawnManager in spawnManagers)
        {
            spawnManager.spawnManagerIndex = index;
            index++;
        }
    }

    public void CheckPlayerInsideNormalMapZone(Player player)
    {
        foreach (SpawnManager spawnManager in spawnManagers)
        {
            if(spawnManager.playerInside.Contains(player))
            {
                spawnManager.playerInside.Remove(player);
            }
        }
    }

}
