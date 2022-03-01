using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WeaponStorage : ModularObject
{
    public SyncListItemSlot weapon = new SyncListItemSlot();
    public void Start()
    {
        if (weapon.Count == 0)
        {
            for (int i = 0; i < 16; i++)
            {
                weapon.Add(new ItemSlot());
            }
        }
    }

    // For weapon make one lost for each one
    public bool InventoryCanAdd(Item item, int amount, int final, int initial = 0, bool unique = true)
    {
        for (int i = initial; i < final; i++)
        {
            if (amount > 0)
            {
                if (unique)
                {
                    if (weapon[i].amount == 0)
                        amount -= item.maxStack;
                }
                else
                {
                    if (weapon[i].amount == 0)
                        amount -= item.maxStack;
                    else if (weapon[i].item.data.name == item.data.name)
                        amount -= (weapon[i].item.maxStack - weapon[i].amount);
                }
                if (amount <= 0) return true;
            }
        }

        return false;
    }

    public bool InventoryAdd(Item item, int amount, int final, int inventoryIndex, int initial = 0, bool unique = true)
    {
        if (InventoryCanAdd(item, amount, final, initial))
        {
            if (unique == false)
            {
                for (int i = initial; i < final; i++)
                {
                    int index = i;
                    if (weapon[index].amount > 0 && weapon[index].item.data.name == item.name)
                    {
                        ItemSlot temp = weapon[index];
                        amount -= temp.IncreaseAmount(amount);
                        temp.amount += temp.IncreaseAmount(amount);
                        weapon[index] = temp;
                    }

                    if (amount <= 0)
                    {
                        ItemSlot temp = Player.localPlayer.inventory[inventoryIndex];
                        temp = new ItemSlot();
                        Player.localPlayer.inventory[inventoryIndex] = temp;
                        return true;
                    }
                }

                for (int i = initial; i < final; i++)
                {
                    int index = i;
                    if (weapon[index].amount == 0)
                    {
                        int add = Mathf.Min(amount, item.maxStack);
                        weapon[index] = new ItemSlot(item, add);
                        amount -= add;
                    }

                    if (amount <= 0)
                    {
                        ItemSlot temp = Player.localPlayer.inventory[inventoryIndex];
                        temp = new ItemSlot();
                        Player.localPlayer.inventory[inventoryIndex] = temp;
                        return true;
                    }
                }
            }
            else
            {
                for (int i = initial; i < final; i++)
                {
                    int index = i;
                    if (weapon[index].amount == 0)
                    {
                        int add = Mathf.Min(amount, item.maxStack);
                        weapon[index] = new ItemSlot(item, add);
                        amount -= add;
                    }

                    if (amount <= 0)
                    {
                        ItemSlot temp = Player.localPlayer.inventory[inventoryIndex];
                        temp = new ItemSlot();
                        Player.localPlayer.inventory[inventoryIndex] = temp;
                        return true;
                    }
                }
            }
            if (amount != 0) Debug.LogError("inventory add failed: " + item.name + " " + amount);
        }
        return false;
    }
}
