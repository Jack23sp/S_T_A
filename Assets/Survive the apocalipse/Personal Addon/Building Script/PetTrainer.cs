using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using CustomType;

public class PetTrainer : NetworkBehaviourNonAlloc
{
    public SyncListPetExp petTraining = new SyncListPetExp();
    public SyncListPetExp finishedTraining = new SyncListPetExp();

    public Entity building;

    // Start is called before the first frame update
    void Start()
    {
        if (!building) building = GetComponent<Entity>();
        //if (isServer && !building.GetComponent<Building>().isPremiumZone)
        //{
        //    InvokeRepeating("DecreaseCraftTimer", 1.0f, 1.0f);
        //}
        //if (isServer && building.GetComponent<Building>().isPremiumZone)
        //{
        //    InvokeRepeating("DecreaseCraftTimerPremium", 1.0f, 1.0f);
        //}
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DecreaseCraftTimer()
    {
        if (!building) building = GetComponent<Entity>();

        for (int i = 0; i < petTraining.Count; i++)
        {
            int index = i;
            PetExp pet = petTraining[index];
            if (pet.remainingTimer > 0)
            {
                pet.remainingTimer--;
                petTraining[index] = pet;
            }
            else
            {
                pet.petItem.item.summonedExperience += pet.experienceToAdd;
                finishedTraining.Add(pet);
                petTraining.RemoveAt(index);
            }
        }
    }

    public void DecreaseCraftTimerPremium()
    {
        if (!building) building = GetComponent<Entity>();

        for (int i = 0; i < petTraining.Count; i++)
        {
            int index = i;
            PetExp pet = petTraining[index];
            if (pet.remainingTimer > 0)
            {
                pet.remainingTimer -=5;
                petTraining[index] = pet;
            }
            else
            {
                pet.petItem.item.summonedExperience += pet.experienceToAdd;
                finishedTraining.Add(pet);
                petTraining.RemoveAt(index);
            }
        }
    }


    public float TimerPercent(PetExp craftItem)
    {
        return (craftItem.remainingTimer != 0 && craftItem.timer != 0) ? (float)craftItem.remainingTimer / (float)craftItem.timer : 0;
    }

}
