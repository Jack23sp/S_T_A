using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GatePlayerChecker : MonoBehaviour
{
    public Entity Entity;
    public Building building;
    public Gate gate;
    public List<Player> playerInside = new List<Player>();

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (Entity.isServer)
        {
            if (collision.GetComponent<Player>())
            {
                Player player = collision.GetComponent<Player>();
                if (GeneralManager.singleton.FindNetworkAbilityLevel("Burglar", player.name) >= building.level)
                {
                    if (!playerInside.Contains(player))
                    {
                        playerInside.Add(player);
                        gate.playerInside = playerInside.Count;
                    }
                }
                else
                {
                    if (GeneralManager.singleton.CanUseTheGate(building, player))
                    {
                        if (!playerInside.Contains(player))
                        {
                            playerInside.Add(player);
                            gate.playerInside = playerInside.Count;
                        }
                    }
                }
            }
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (Entity.isServer)
        {
            if (collision.GetComponent<Player>())
            {
                Player player = collision.GetComponent<Player>();
                if (playerInside.Contains(player))
                {
                    playerInside.Remove(player);
                    gate.playerInside = playerInside.Count;
                }
            }
        }
    }

}
