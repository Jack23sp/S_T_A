using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Tesla : NetworkBehaviourNonAlloc
{
    public Entity entity;
    public Building building;
    [Header("Target")]
    [SyncVar] public GameObject _target;
    public Entity target
    {
        get { return _target != null ? _target.GetComponent<Entity>() : null; }
        set { _target = value != null ? value.gameObject : null; }
    }

    public GameObject destionationObject;
    public GameObject teslaTrigger;
    public LineRenderer lineRenderer;


    // Start is called before the first frame update
    void Start()
    {
        if (!building) building = GetComponent<Building>();
        if (!entity) entity = GetComponent<Entity>();
        if(isServer)
        {
            InvokeRepeating("DealDamageToEntity", 1.0f, 1.0f);
        }
        lineRenderer.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(target && target.health > 0)
        {
            destionationObject.transform.position = target.transform.position;
        }
        else
        {
            destionationObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        }
           
        teslaTrigger.SetActive(entity.isServer);
    }

    public void DealDamageToEntity()
    {
        if(target)
        {
            target.health -= GeneralManager.singleton.teslaDamage.Get(entity.level);
            if(target.health <= 0) _target = null; 
        }
    }


}
