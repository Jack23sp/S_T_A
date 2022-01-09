using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazzleFlashSelector : MonoBehaviour
{
    public MazzleFlashSpawner mazzleFlashSpawner;

    private void Awake()
    {
        mazzleFlashSpawner = GetComponentInParent<MazzleFlashSpawner>();
        mazzleFlashSpawner.objectToSpawn = this.gameObject;
    }

}
