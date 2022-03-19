using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantNotifier : MonoBehaviour
{
    public MedicalPlant medicalPlant;
    Player player;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            player = collision.gameObject.GetComponent<Player>();
            if (!player.playerPlant.plantObject.Contains(medicalPlant))
            {
                player.playerPlant.plantObject.Add(medicalPlant);
            }
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player = collision.gameObject.GetComponent<Player>();
            if (player.playerPlant.plantObject.Contains(medicalPlant))
            {
                player.playerPlant.plantObject.Remove(medicalPlant);
            }
        }
    }
}
