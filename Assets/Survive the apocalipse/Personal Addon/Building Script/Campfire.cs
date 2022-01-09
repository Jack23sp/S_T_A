using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Campfire : NetworkBehaviour
{
    [SyncVar]
    public int currentWood;
    [SyncVar]
    public bool active;

    public GameObject lightObject;

    public SyncListItemSlot items = new SyncListItemSlot();

    public Collider2D[] colliders;

    public bool prevActive;

    public CampfireDryClothes campfireDryClothes;

    public Entity entity;

    // Start is called before the first frame update
    void Start()
    {
        if(isServer)
        {
            InvokeRepeating(nameof(DecreaseCookedCountdown), 1.0f, 1.0f);
            InvokeRepeating(nameof(DecreaseWood), GeneralManager.singleton.intervalConsumeWoodCampfire, GeneralManager.singleton.intervalConsumeWoodCampfire);
            campfireDryClothes.gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(prevActive != active)
        {
            lightObject.SetActive(active);
            entity.animator.SetBool("Active", active);
            prevActive = active;
        }
    }


    public void DecreaseWood()
    {
        if(currentWood > 0 && active)
        {
            currentWood--;
        }
        if(currentWood == 0)
        {
            active = false;
        }
    }

    public void DecreaseCookedCountdown()
    {
        if (currentWood == 0 || !active) return;
        for(int i = 0; i < items.Count; i++)
        {
            int index = i;
            ItemSlot slot = items[index];
            if (slot.item.cookCountdown > 0) slot.item.cookCountdown--;
            items[index] = slot;
        }
        if (currentWood == 0)
        {
            active = false;
        }
    }

    public float CookPercent(ItemSlot slot)
    {
        return 1.0f - ((float)slot.item.cookCountdown / (float)((FoodItem)slot.item.data).maxAmountCook);
    }
}
