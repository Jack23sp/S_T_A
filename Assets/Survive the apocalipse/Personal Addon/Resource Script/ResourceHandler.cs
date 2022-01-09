using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceHandler : MonoBehaviour
{
    public int index;
    public string type;
    public GameObject referenceType;
    public Entity entity;
    public bool isDecoration;

    public void Follow()
    {
        if(referenceType.activeInHierarchy)
        {
            referenceType.transform.position = transform.position;
        }
    }

    public void OnDisable()
    {
        CancelInvoke("Follow");
    }

    public void OnEnable()
    {
        if (!entity) entity = GetComponent<Entity>();

        if (entity && entity.isServer)
            InvokeRepeating("Follow", 2.0f, 2.0f);
    }
}
