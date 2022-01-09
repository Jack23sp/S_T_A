using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSpawnCheck : MonoBehaviour
{
    public List<Collider2D> checkCollider = new List<Collider2D>();

    // Start is called before the first frame update
    void Start()
    {
        checkCollider.Add(GetComponent<Collider2D>());    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D co)
    {
        if(!checkCollider.Contains(co))
        {
            checkCollider.Add(co);
        }
    }

    void OnTriggerStay2D(Collider2D co)
    {
        if (!checkCollider.Contains(co))
        {
            checkCollider.Add(co);
        }
    }

    void OnTriggerExit2D(Collider2D co)
    {
        if (checkCollider.Contains(co))
        {
            checkCollider.Remove(co);
        }
    }
}
