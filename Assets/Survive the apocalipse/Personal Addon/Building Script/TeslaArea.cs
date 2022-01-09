using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TeslaArea : MonoBehaviour
{
    public NetworkIdentity mainID;
    public Tesla tesla;

    public List<Collider2D> colliders = new List<Collider2D>();
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnTriggerStay2D(Collider2D col)
    {
        if (!colliders.Contains(col)) colliders.Add(col);
        if (mainID.isServer)
        {
            if (col.GetComponent<Entity>() is Player)
            {
                // if is not ally
                if (GeneralManager.singleton.CanInteractBuilding(tesla.building, col.GetComponent<Player>()))
                {
                    if (!GeneralManager.singleton.TeslaEquipment(col.GetComponent<Player>()))
                    {
                        if (!tesla._target) tesla._target = col.gameObject;
                        else if (tesla._target && tesla.target && tesla.target.health == 0)
                        {
                            if (colliders.Contains(tesla.target.collider)) colliders.Remove(tesla.target.collider);
                            tesla._target = col.gameObject;
                        }
                    }
                    tesla.building.isHide = false;
                }
            }
        }
    }

    public void OnTriggerExit2D(Collider2D col)
    {
        if (colliders.Contains(col)) colliders.Remove(col);

        if (mainID.isServer)
        {
            if (tesla.target == col.GetComponent<Entity>())
            {
                tesla._target = null;
            }
        }
    }

}
