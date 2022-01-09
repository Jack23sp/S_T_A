using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VisibilityManager : MonoBehaviour
{
    [HideInInspector] public ComponentVisualizer visualizer;
    [HideInInspector] public NetworkIdentity identity;
    [HideInInspector] public Entity entity;
    [HideInInspector] public NetworkProximityChecker networkProximity;

    public bool isClean = false;

    public bool isServer;
    public uint netId;

    public void Start()
    {
        entity = GetComponent<Entity>();
        visualizer = GetComponent<ComponentVisualizer>();
        identity = GetComponent<NetworkIdentity>();
        networkProximity = GetComponent<NetworkProximityChecker>();
        isServer = identity.isServer;
        netId = identity.netId;
        if (isServer)
            InvokeRepeating("ManageVisibility", 0.0f, 0.5f + UnityEngine.Random.Range(0.1f, 0.5f));
    }

    void ManageVisibility()
    {
        if (!NetworkServer.active)
            return;

        if (netId == 0) return;

        if (identity.observers.Count == 0)
        {
            if (!isClean)
            {
                if (visualizer is TreeComponentVisualizer)
                {
                    ((TreeComponentVisualizer)visualizer).VisibilityZero();
                }
                if (visualizer is RockComponentVisualizer)
                {
                    ((RockComponentVisualizer)visualizer).VisibilityZero();
                }
                if (visualizer is MonsterComponentVisualizer)
                {
                    ((MonsterComponentVisualizer)visualizer).VisibilityZero();
                }
                if (visualizer is BuildingComponentVisualizer)
                {
                    ((BuildingComponentVisualizer)visualizer).VisibilityZero();
                }
                if (visualizer is NpcComponentVisualizer)
                {
                    ((NpcComponentVisualizer)visualizer).VisibilityZero();
                }
                isClean = true;
            }


        }
        else
        {
            if (isClean)
            {
                if (visualizer is TreeComponentVisualizer)
                {
                    ((TreeComponentVisualizer)visualizer).VisibilityDifferntZero();
                }
                if (visualizer is RockComponentVisualizer)
                {
                    ((RockComponentVisualizer)visualizer).VisibilityDifferntZero();
                }
                if (visualizer is MonsterComponentVisualizer)
                {
                    ((MonsterComponentVisualizer)visualizer).VisibilityDifferntZero();
                }
                if (visualizer is BuildingComponentVisualizer)
                {
                    ((BuildingComponentVisualizer)visualizer).VisibilityDifferntZero();
                }
                if (visualizer is NpcComponentVisualizer)
                {
                    ((NpcComponentVisualizer)visualizer).VisibilityDifferntZero();
                }
                isClean = false;
            }
        }
    }
}
