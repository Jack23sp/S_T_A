using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModularWall : MonoBehaviour
{
    public List<NavMeshObstacle2D> obstacle2D = new List<NavMeshObstacle2D>();

    public void OnEnable()
    {
        for(int i = 0; i < obstacle2D.Count; i++)
        {
            int index = i;
            obstacle2D[index].enabled = true;
            //obstacle2D[index].Awake();
        }
    }

    public void OnDisable()
    {
        for (int i = 0; i < obstacle2D.Count; i++)
        {
            int index = i;
            if (obstacle2D[index].go)
            {
                obstacle2D[index].OnDestroy();
            }
            obstacle2D[index].enabled = false;
        }
    }
}
