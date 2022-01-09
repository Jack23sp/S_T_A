using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampfireDryClothes : MonoBehaviour
{
    public Campfire campfire;
    public Collider2D[] colliders;
    public AudioSource audioSource;

    public void Start()
    {
        if(campfire.isServer)
        {
            InvokeRepeating(nameof(DryClothes), GeneralManager.singleton.intervalDryCampfire, GeneralManager.singleton.intervalDryCampfire);
            InvokeRepeating(nameof(CheckNearPlayer), 3.0f, 3.0f);
        }
    }

    public void CheckNearPlayer()
    {
        colliders = Physics2D.OverlapCircleAll(transform.position, 3.7f, GeneralManager.singleton.campfireLayerMask);
        if(colliders.Length > 0 && campfire.active == true)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            if(audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    public void DryClothes()
    {
        if (!campfire.active) return;
        for (int i = 0; i < colliders.Length; i++)
        {
            int index = i;
            Player player = colliders[index].GetComponent<Player>();
            player.playerWet.wetEquipmentNaked = 0;
            if (player)
            {
                for (int e = 0; e < player.equipment.Count; e++)
                {
                    int indexe = e;
                    if (player.equipment[e].amount > 0 && player.equipment[e].item.data is EquipmentItem && player.equipment[e].item.wet > 0.0f)
                    {
                        ItemSlot slot = player.equipment[e];
                        slot.item.wet = 0.0f;
                        player.equipment[e] = slot;
                        break;
                    }
                }
            }
        }
    }

}
