using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenNotificationManager : MonoBehaviour
{
    public static ScreenNotificationManager singleton;

    public GameObject objectToSpawn;
    public string purchaseType;

    // Start is called before the first frame update
    void Start()
    {
        if (!singleton) singleton = this;    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnNotification (string description , string purchaseType)
    {
        GameObject go = Instantiate(objectToSpawn, GeneralManager.singleton.canvas);
        go.GetComponent<NotificationSlot>().description.text = description;
        go.GetComponent<NotificationSlot>().purchaseType = purchaseType;
    }
}
