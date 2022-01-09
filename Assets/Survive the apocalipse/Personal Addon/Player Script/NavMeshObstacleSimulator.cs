using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NavMeshObstacleSimulator : NetworkBehaviour
{
    public Player player;

    [SyncVar]
    public bool canMove = true;

    public GameObject objectToCheckObstacle;

    public GameObject go;

    public EntityObstacleCheck entityObstacleCheck;

    // Start is called before the first frame update
    void Start()
    {
        entityObstacleCheck = player.GetComponent<EntityObstacleCheck>();
    }


    void Update()
    {
        

        if (player.isServer)
        {

            if (!go)
            {
                go = Instantiate(objectToCheckObstacle);
                go.GetComponent<PlayerCheckObstacle>().player = player;
                go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                //go.AddComponent<SphereCollider>();
                //go.AddComponent<MeshRenderer>();
            }

            if (!entityObstacleCheck.CheckObstacle((player.transform.position + (Vector3)player.direction)))
            {
                go.transform.position = player.transform.position + (Vector3)player.direction;
            }
            //if (player._state == "IDLE")
            //{
            //    go.transform.position = player.transform.position;
            //}


            if (canMove == false)
            {
                player.agent.ResetPath();
                player.agent.ResetMovement();
                player.rubberbanding.ResetMovement();
            }

        }

    }
}

public partial class Player
{
    [Command]
    public void CmdSetDirection(float x, float y, float z)
    {
        direction = new Vector3(x, y, z);
    }
}