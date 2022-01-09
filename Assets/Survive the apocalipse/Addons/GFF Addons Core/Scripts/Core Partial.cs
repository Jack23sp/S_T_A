using Mirror;
using UnityEngine;

public partial class ScriptableItem
{
    //for Upgrade, Auction, GFF ToolTips, Gathering
    [Header("Item Category")]
    public string Group;
    public string subGroup;
}

public class GFFUtils
{
    public static void BalancePrefabss(GameObject prefab, int amount, Transform parent)
    {
        // instantiate until amount
        for (int i = 0; i < amount; ++i)
        {
            var go = GameObject.Instantiate(prefab);
            go.transform.SetParent(parent.GetChild(i).transform, false);
        }

        // delete everything that's too much
        // (backwards loop because Destroy changes childCount)
        for (int i = 0; i < amount; ++i)
            if (parent.GetChild(i).transform.childCount > 1)
                GameObject.Destroy(parent.GetChild(i).transform.GetChild(1).gameObject);
    }

    public static Player FindPlayerByName(string name)
    {
        if (Player.onlinePlayers.ContainsKey(name))
        {
            return Player.onlinePlayers[name].GetComponent<Player>();
        }
        else return null;
    }
}

public partial class Player
{
    [Command]
    public void CmdGoldOnCharacter(long value)
    {
        gold = value;
    }
    [Command]
    public void CmdCoinsOnCharacter(long value)
    {
        coins = value;
    }

    [Command]
    public void CmdUpdateInventoryItemSlot(ItemSlot item, int index)
    {
        inventory[index] = item;
    }
    [Command]
    public void CmdUpdateEquipmentItemSlot(ItemSlot item, int index)
    {
        equipment[index] = item;
    }

    [Command]
    public void CmdAddSlotToInventory(ItemSlot itemSlot)
    {
        bool isDone = false;

        // add to same item stacks first (if any)
        // (otherwise we add to first empty even if there is an existing stack afterwards)
        for (int i = 0; i < inventory.Count; ++i)
        {
            // not empty and same type? then add free amount (max-amount)
            // note: .Equals because name AND dynamic variables matter (petLevel etc.)
            if (inventory[i].amount > 0 && inventory[i].item.Equals(itemSlot.item))
            {
                ItemSlot temp = inventory[i];
                itemSlot.amount -= temp.IncreaseAmount(itemSlot.amount);
                inventory[i] = temp;
            }

            // were we able to fit the whole amount already? then stop loop
            if (itemSlot.amount <= 0) isDone = true;
        }

        if (!isDone)
        {
            // add to empty slots (if any)
            for (int i = 0; i < inventory.Count; ++i)
            {
                // empty? then fill slot with as many as possible
                if (inventory[i].amount == 0)
                {
                    int add = Mathf.Min(itemSlot.amount, itemSlot.item.maxStack);
                    inventory[i] = itemSlot;
                    itemSlot.amount -= add;
                }

                // were we able to fit the whole amount already? then stop loop
                if (itemSlot.amount <= 0) isDone = true;
            }
        }

        // we should have been able to add all of them
        if (!isDone && itemSlot.amount != 0) Debug.LogError("inventory add failed: " + itemSlot.item.name + " " + itemSlot.amount);
    }

    //Auction, AutoAction
    public int FindFreeInventorySlot()
    {
        for (int i = 0; i < inventorySize; ++i)
        {
            if (inventory[i].amount == 0) return i;
        }
        return -1;
    }

    //AutoAction, TargetPanel
    [Client]
    public void GffAutoMoveTo(Vector3 destination)
    {
        agent.stoppingDistance = 1.3f;
        agent.destination = destination;
    }
}

public partial class Monster
{
    [Header("GFF Loot addon & Drop Bonuses")]
    [SyncVar] public GameObject _killed;
    public Entity killed
    {
        get { return _killed != null ? _killed.GetComponent<Entity>() : null; }
        set { _killed = value != null ? value.gameObject : null; }
    }

    [Command]
    public void CmdKilledNull()
    {
        killed = null;
    }
}

public partial class NetworkManagerMMO
{
    public int accountMinLength = 0;
    public int accountMaxLength = 0;
    public int passwordMinLength = 0;
}

public static class GFFExtensions
{
    public static int ToInt(this bool Value)
    {
        return Value ? 1 : 0;
    }

    public static bool ToBool(this int Value)
    {
        return Value == 1 ? true : false;
    }
}