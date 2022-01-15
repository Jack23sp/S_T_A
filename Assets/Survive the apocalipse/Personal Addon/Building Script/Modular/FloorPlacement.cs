﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorPlacement : MonoBehaviour
{
    public List<Collider2D> colliders = new List<Collider2D>();

    public void OnTriggerEnter2D(Collider2D collider)
    {
        //if (collider.CompareTag("FloorPlacement"))
        //{
            if (!colliders.Contains(collider))
            {
                if ((GeneralManager.singleton.buildingCheckSpawn.value & (1 << collider.gameObject.layer)) > 0)
                    colliders.Add(collider);
            }
        //}
    }

    //public void OnTriggerStay2D(Collider2D collider)
    //{
    //    if (collider.CompareTag("FloorPlacement"))
    //    {
    //        if (!colliders.Contains(collider))
    //        {
    //            if ((GeneralManager.singleton.buildingCheckSpawn.value & (1 << collider.gameObject.layer)) > 0)
    //                colliders.Add(collider);
    //        }
    //    }
    //}

    public void OnTriggerExit2D(Collider2D collider)
    {
        if (colliders.Contains(collider))
        {
            colliders.Remove(collider);
        }
    }
}