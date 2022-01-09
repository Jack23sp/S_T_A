using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using CustomType;
using System;

public class BuildingCraft : NetworkBehaviourNonAlloc
{
    public SyncListCraft craftItem = new SyncListCraft();
    public SyncListCraft allFinishedItem = new SyncListCraft();

    public Entity building;

    // Start is called before the first frame update
    void Start()
    {
        if (!building) GetComponent<Entity>();
        //if (isServer)
        //{
        //    InvokeRepeating("ManageCraftTimer", 5.0f, 5.0f);
        //}
    }

    // Update is called once per frame
    void Update()
    {

    }

    //public void ManageCraftTimer()
    //{
    //    if (!building) building = GetComponent<Entity>();

    //    for (int i = 0; i < craftItem.Count; i++)
    //    {
    //        int index = i;
    //        //if (index < building.level)
    //        //{
    //            CraftItem craft = craftItem[index];
    //            TimeSpan difference = DateTime.Parse(craftItem[index].timeEnd) - TimeZoneInfo.ConvertTime(System.DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById(GeneralManager.singleton.timeZone));

    //            if (difference.TotalSeconds <= 0)
    //            {
    //                allFinishedItem.Add(craft);
    //                craftItem.Remove(craft);
    //            }
    //        //}
    //    }
    //}


    public float TimerPercent(CraftItem craftItem)
    {
        return (craftItem.remainingTime != 0 && craftItem.totalTime != 0) ? (float)craftItem.remainingTime / (float)craftItem.totalTime : 0;
    }

}
