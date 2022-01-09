using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazzleFlashSpawner : MonoBehaviour
{
    public GameObject objectToSpawn;
    public float timerToDisable = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        if(Player.localPlayer) Player.localPlayer.mazzleFlashSpawner = this;
    }

    // Update is called once per frame
    void Update()
    {
      
    }

    public void Disable()
    {
        if(objectToSpawn) objectToSpawn.SetActive(false);
    }

    public void Able()
    {
        if (objectToSpawn)
        {
            objectToSpawn.SetActive(true);
            Invoke(nameof(Disable), timerToDisable);
        }
    }
}
