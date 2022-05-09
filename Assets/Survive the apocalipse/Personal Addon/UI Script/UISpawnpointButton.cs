using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISpawnpointButton : MonoBehaviour
{
    private Player player;
    public static UISpawnpointButton singleton;
    public Button spawnpointButton;
    public GameObject spawnedObject;

    // Start is called before the first frame update
    void Start()
    {
        if (!singleton) singleton = this;
        InvokeRepeating(nameof(CheckSpawnPanel), 1.0f, 2.0f);
        spawnpointButton.onClick.AddListener(() =>
        {
            if (player.health > 0)
            {
                if (spawnedObject)
                {
                    Destroy(spawnedObject);
                }
                else
                {
                    spawnedObject = Instantiate(GeneralManager.singleton.spawnpointPanelToCreate, GeneralManager.singleton.canvas);
                }
            }
        });
    }

    public void CheckSpawnPanel()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        if (player.health == 0 && !spawnedObject)
        {
            spawnedObject = Instantiate(GeneralManager.singleton.spawnpointPanelToCreate, GeneralManager.singleton.canvas);
        }
    }
}
