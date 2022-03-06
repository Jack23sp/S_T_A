using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using CustomType;

public class BuildingUpgradeRepair : NetworkBehaviour
{
    public SyncListUpgradeRepair upgradeItem = new SyncListUpgradeRepair();
    public SyncListUpgradeRepair repairItem = new SyncListUpgradeRepair();
    //public SyncListUpgradeRepair finishUpgradeItem = new SyncListUpgradeRepair();
    //public SyncListUpgradeRepair finishRepairItem = new SyncListUpgradeRepair();

    //int buildinglevel = 0;
    //public Entity building;

    // Start is called before the first frame update
    void Start()
    {
        //if (!building) building = GetComponent<Entity>();
        //if (isServer && !building.GetComponent<Building>().isPremiumZone)
        //{
        //    InvokeRepeating("ManageItem", 1.0f, 1.0f);
        //}
        //if (isServer && building.GetComponent<Building>().isPremiumZone)
        //{
        //    InvokeRepeating("ManageItemPremium", 1.0f, 1.0f);
        //}
    }
    // Update is called once per frame
    //void ManageItem()
    //{
    //    if (building)
    //    {
    //        buildinglevel = (int)(building.level / 10);
    //        buildinglevel += 1;
    //        for (int i = 0; i < upgradeItem.Count; i++)
    //        {
    //            int index = i;
    //            if (index < buildinglevel)
    //            {
    //                UpgradeRepairItem item = upgradeItem[index];
    //                if (item.remainingTime > 0)
    //                {
    //                    item.remainingTime--;
    //                    upgradeItem[index] = item;
    //                }
    //                if (item.remainingTime == 0)
    //                {
    //                    if (!finishUpgradeItem.Contains(item))
    //                    {
    //                        finishUpgradeItem.Add(item);
    //                        upgradeItem.Remove(item);
    //                    }
    //                }
    //            }
    //        }

    //        for (int e = 0; e < repairItem.Count; e++)
    //        {
    //            int index = e;
    //            if (index < buildinglevel)
    //            {
    //                UpgradeRepairItem item = repairItem[index];
    //                if (item.remainingTime > 0)
    //                {
    //                    item.remainingTime--;
    //                    repairItem[index] = item;
    //                }
    //                if (item.remainingTime == 0)
    //                {
    //                    if (!finishRepairItem.Contains(item))
    //                    {
    //                        item.item.item.durability = item.item.item.data.maxDurability.Get(item.item.item.durabilityLevel);
    //                        repairItem[index] = item;
    //                        finishRepairItem.Add(item);
    //                        repairItem.Remove(item);
    //                    }
    //                }
    //            }
    //        }
    //        for (int e = 0; e < finishRepairItem.Count; e++)
    //        {
    //            int index = e;
    //            UpgradeRepairItem item = finishRepairItem[index];

    //            //Debug.Log("Durability level : " + item.item.item.durabilityLevel);
    //            item.item.item.durability = item.item.item.data.maxDurability.Get(item.item.item.durabilityLevel);
    //            finishRepairItem[index] = item;
    //        }
    //    }
    //}

    //void ManageItemPremium()
    //{
    //    if (building)
    //    {
    //        buildinglevel = (int)(building.level / 10);
    //        buildinglevel += 1;
    //        for (int i = 0; i < upgradeItem.Count; i++)
    //        {
    //            int index = i;
    //            if (index < buildinglevel)
    //            {
    //                UpgradeRepairItem item = upgradeItem[index];
    //                if (item.remainingTime > 0)
    //                {
    //                    item.remainingTime-=5;
    //                    upgradeItem[index] = item;
    //                }
    //                if (item.remainingTime == 0)
    //                {
    //                    if (!finishUpgradeItem.Contains(item))
    //                    {
    //                        finishUpgradeItem.Add(item);
    //                        upgradeItem.Remove(item);
    //                    }
    //                }
    //            }
    //        }

    //        for (int e = 0; e < repairItem.Count; e++)
    //        {
    //            int index = e;
    //            if (index < buildinglevel)
    //            {
    //                UpgradeRepairItem item = repairItem[index];
    //                if (item.remainingTime > 0)
    //                {
    //                    item.remainingTime-=5;
    //                    repairItem[index] = item;
    //                }
    //                if (item.remainingTime == 0)
    //                {
    //                    if (!finishRepairItem.Contains(item))
    //                    {
    //                        item.item.item.durability = item.item.item.data.maxDurability.Get(item.item.item.durabilityLevel);
    //                        repairItem[index] = item;
    //                        finishRepairItem.Add(item);
    //                        repairItem.Remove(item);
    //                    }
    //                }
    //            }
    //        }
    //        for (int e = 0; e < finishRepairItem.Count; e++)
    //        {
    //            int index = e;
    //            UpgradeRepairItem item = finishRepairItem[index];

    //            //Debug.Log("Durability level : " + item.item.item.durabilityLevel);
    //            item.item.item.durability = item.item.item.data.maxDurability.Get(item.item.item.durabilityLevel);
    //            finishRepairItem[index] = item;
    //        }
    //    }
    //}
}
