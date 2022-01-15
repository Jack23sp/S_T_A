using System.Collections;
using CustomType;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ModularPiece : NetworkBehaviour
{
    [SyncVar]
    public int modularIndex;

    [SyncVar]
    public bool occupiedLEFT;
    [SyncVar]
    public bool occupiedRIGHT;
    [SyncVar]
    public bool occupiedUP;
    [SyncVar]
    public bool occupiedDOWN;


    // index of the nearest part
    [SyncVar]
    public int leftPart;
    [SyncVar]
    public int rightPart;
    [SyncVar]
    public int upPart;
    [SyncVar]
    public int downPart;

    public int clientLeftPart = -5;
    public int clientRightPart = -5;
    public int clientUpPart = -5;
    public int clientDownPart = -5;

    // type of the other component
    [SyncVar]
    public int leftComponent;
    [SyncVar]
    public int rightComponent;
    [SyncVar]
    public int upComponent;
    [SyncVar]
    public int downComponent;

    public int clientleftComponent = -5;
    public int clientrightComponent = -5;
    public int clientupComponent = -5;
    public int clientdownComponent = -5;

    [SyncVar]
    public string owner;
    [SyncVar]
    public string guild;

    public List<FlooreBasementOccupied> modularFloorPoint = new List<FlooreBasementOccupied>();

    public FloorPlacement floorPlacement;
    public SpriteRenderer floorPlacementSprite;

    public Color pColor;

    private Transform buildingTransform;

    public SpriteRenderer leftWallPointer;
    public SpriteRenderer rightWallPointer;
    public SpriteRenderer upWallPointer;
    public SpriteRenderer downWallPointer;

    public GameObject upWall;
    public GameObject upDoor;
    public GameObject leftWall;
    public GameObject leftDoor;
    public GameObject rightWall;
    public GameObject rightDoor;
    public GameObject downWall;
    public GameObject downDoor;

    private NavMeshObstacle2D[] navMeshObstacle2Ds;

    public List<Transform> floorAccessories = new List<Transform>();

    [SyncVar]
    public int playerInsideLeftDoor;
    [SyncVar]
    public int playerInsideRightDoor;
    [SyncVar]
    public int playerInsideUpDoor;
    [SyncVar]
    public int playerInsideDownDoor;

    public void OnDestroy()
    {
        clientdownComponent = -5;
        clientupComponent = -5;
        clientleftComponent = -5;
        clientrightComponent = -5;
        CheckWall();
    }

    void Start()
    {
        buildingTransform = GetComponent<Transform>();
        name.Replace("(Clone)", "");
        name = name + UnityEngine.Random.Range(0, 100000);

        if (isClient)
        {
            InvokeRepeating(nameof(SetColor), 0.2f, 0.2f);
        }
        Invoke(nameof(SyncStartValue), 0.7f);
        CheckWall();
    }

    public void SyncStartValue()
    {
        if (isServer)
        {
            clientdownComponent = downComponent;
            clientupComponent = upComponent;
            clientleftComponent = leftComponent;
            clientrightComponent = rightComponent;
        }
    }

    public void CheckWall()
    {
        if(clientleftComponent != -5)
        {
            if (clientleftComponent == 0 || leftComponent == 0)
            {
                leftWall.SetActive(true);
                navMeshObstacle2Ds = leftWall.GetComponents<NavMeshObstacle2D>();
                foreach (NavMeshObstacle2D obstacle2D in navMeshObstacle2Ds)
                {
                    obstacle2D.Spawn();
                }
            }
            else if (clientleftComponent == 1 || leftComponent == 1)
            {
                leftDoor.SetActive(true);
                navMeshObstacle2Ds = leftDoor.GetComponents<NavMeshObstacle2D>();
                foreach (NavMeshObstacle2D obstacle2D in navMeshObstacle2Ds)
                {
                    obstacle2D.Spawn();
                }
            }
        }
        else
        {
            leftWall.SetActive(false);
            leftDoor.SetActive(false);
            navMeshObstacle2Ds = leftWall.GetComponents<NavMeshObstacle2D>();
            foreach (NavMeshObstacle2D obstacle2D in navMeshObstacle2Ds)
            {
                Destroy(obstacle2D.go);
                obstacle2D.go = null;
            }
            navMeshObstacle2Ds = leftDoor.GetComponents<NavMeshObstacle2D>();
            foreach (NavMeshObstacle2D obstacle2D in navMeshObstacle2Ds)
            {
                Destroy(obstacle2D.go);
                obstacle2D.go = null;
            }
        }

        if (clientrightComponent != -5)
        {
            if (clientrightComponent == 0 || rightComponent == 0)
            {
                rightWall.SetActive(true);
                navMeshObstacle2Ds = rightWall.GetComponents<NavMeshObstacle2D>();
                foreach (NavMeshObstacle2D obstacle2D in navMeshObstacle2Ds)
                {
                    obstacle2D.Spawn();
                }
            }
            else if (clientrightComponent == 1 || rightComponent == 1)
            {
                rightDoor.SetActive(true);
                navMeshObstacle2Ds = rightDoor.GetComponents<NavMeshObstacle2D>();
                foreach (NavMeshObstacle2D obstacle2D in navMeshObstacle2Ds)
                {
                    obstacle2D.Spawn();
                }
            }
        }
        else
        {
            rightWall.SetActive(false);
            navMeshObstacle2Ds = rightWall.GetComponents<NavMeshObstacle2D>();
            foreach (NavMeshObstacle2D obstacle2D in navMeshObstacle2Ds)
            {
                Destroy(obstacle2D.go);
                obstacle2D.go = null;
            }
            rightDoor.SetActive(false);
            navMeshObstacle2Ds = rightDoor.GetComponents<NavMeshObstacle2D>();
            foreach (NavMeshObstacle2D obstacle2D in navMeshObstacle2Ds)
            {
                Destroy(obstacle2D.go);
                obstacle2D.go = null;
            }
        }
        if (clientupComponent != -5)
        {
            if (clientupComponent == 0 || upComponent == 0)
            {
                upWall.SetActive(true);
                navMeshObstacle2Ds = upWall.GetComponents<NavMeshObstacle2D>();
                foreach (NavMeshObstacle2D obstacle2D in navMeshObstacle2Ds)
                {
                    obstacle2D.Spawn();
                }
            }
            else if (clientupComponent == 1 || upComponent == 1)
            {
                upDoor.SetActive(true);
                navMeshObstacle2Ds = upDoor.GetComponents<NavMeshObstacle2D>();
                foreach (NavMeshObstacle2D obstacle2D in navMeshObstacle2Ds)
                {
                    obstacle2D.Spawn();
                }
            }
        }
        else
        {
            upWall.SetActive(false);
            navMeshObstacle2Ds = upWall.GetComponents<NavMeshObstacle2D>();
            foreach (NavMeshObstacle2D obstacle2D in navMeshObstacle2Ds)
            {
                Destroy(obstacle2D.go);
                obstacle2D.go = null;
            }
            upDoor.SetActive(false);
            navMeshObstacle2Ds = upDoor.GetComponents<NavMeshObstacle2D>();
            foreach (NavMeshObstacle2D obstacle2D in navMeshObstacle2Ds)
            {
                Destroy(obstacle2D.go);
                obstacle2D.go = null;
            }
        }
        if (clientdownComponent != -5)
        {
            if (clientdownComponent == 0 || downComponent == 0)
            {
                downWall.SetActive(true);
                navMeshObstacle2Ds = downWall.GetComponents<NavMeshObstacle2D>();
                foreach (NavMeshObstacle2D obstacle2D in navMeshObstacle2Ds)
                {
                    obstacle2D.Spawn();
                }
            }
            else if (clientdownComponent == 1 || downComponent == 1)
            {
                downDoor.SetActive(true);
                navMeshObstacle2Ds = downDoor.GetComponents<NavMeshObstacle2D>();
                foreach (NavMeshObstacle2D obstacle2D in navMeshObstacle2Ds)
                {
                    obstacle2D.Spawn();
                }
            }
        }
        else
        {
            downWall.SetActive(false);
            navMeshObstacle2Ds = downWall.GetComponents<NavMeshObstacle2D>();
            foreach (NavMeshObstacle2D obstacle2D in navMeshObstacle2Ds)
            {
                Destroy(obstacle2D.go);
                obstacle2D.go = null;
            }
            downDoor.SetActive(false);
            navMeshObstacle2Ds = downDoor.GetComponents<NavMeshObstacle2D>();
            foreach (NavMeshObstacle2D obstacle2D in navMeshObstacle2Ds)
            {
                Destroy(obstacle2D.go);
                obstacle2D.go = null;
            }

        }
    }

    public bool CanSpawn()
    {
        return floorPlacement.colliders.Count <= 0;
    }

    public void SetColor()
    {
        if (floorPlacement)
        {
            if (netId == 0)
            {
                pColor = floorPlacementSprite.color;

                if (floorPlacement.colliders.Count <= 0)
                {
                    pColor = GeneralManager.singleton.canSpawn;
                    floorPlacementSprite.color = pColor;
                }
                else
                {
                    pColor = GeneralManager.singleton.notSpawn;
                    floorPlacementSprite.color = pColor;
                }
            }
            else
            {
                CancelInvoke(nameof(SetColor));
                Destroy(floorPlacement.gameObject);
            }
        }

    }

    public void Up()
    {
        Vector3 pos = buildingTransform.position;
        pos.y += GeneralManager.singleton.buildingSensibility;

        buildingTransform.position = pos;
    }
    public void Left()
    {
        Vector3 pos = buildingTransform.position;
        pos.x -= GeneralManager.singleton.buildingSensibility;

        buildingTransform.position = pos;
    }
    public void Down()
    {
        Vector3 pos = buildingTransform.position;
        pos.y -= GeneralManager.singleton.buildingSensibility;

        buildingTransform.position = pos;
    }
    public void Right()
    {
        Vector3 pos = buildingTransform.position;
        pos.x += GeneralManager.singleton.buildingSensibility;

        buildingTransform.position = pos;
    }
    public void chengePerspective(Positioning positioning)
    {
    }

    public void DestroyBuilding()
    {
        Player player = Player.localPlayer;

        if(player.playerBuilding.actualBuilding) Destroy(player.playerBuilding.actualBuilding);
        player.playerBuilding.actualBuilding = null;
        player.playerBuilding.building = null;
        player.playerBuilding.inventoryIndex = -1;
        if (GeneralManager.singleton.spawnedBuildingObject) Destroy(GeneralManager.singleton.spawnedBuildingObject);
        ModularBuildingManager.singleton.ableModificationMode = false;
        ModularBuildingManager.singleton.ableModificationWallMode = false;
    }


    public void SpawnBuilding()
    {
        Player player = Player.localPlayer;

        if (player.playerBuilding.actualBuilding != null)
        {
            ModularPiece modularPiece = player.playerBuilding.actualBuilding.GetComponent<ModularPiece>();

            if (modularPiece)
            {
                if (player.playerBuilding.building.isBasement)
                {
                    if (modularPiece.CanSpawn())
                    {
                        player.playerBuilding.CmdSpawnBasement(player.playerBuilding.inventoryIndex, player.playerBuilding.building.name, new Vector2(player.playerBuilding.actualBuilding.transform.position.x, player.playerBuilding.actualBuilding.transform.position.y), player.playerBuilding.invBelt);
                        DestroyBuilding();
                    }
                }
                else if (player.playerBuilding.building.isWall || player.playerBuilding.building.isDoor)
                {
                    player.playerBuilding.CmdSyncWallDoor(player.playerBuilding.actualBuilding.GetComponent<NetworkIdentity>(), modularPiece.clientupComponent, modularPiece.clientdownComponent, modularPiece.clientleftComponent, modularPiece.clientrightComponent);
                    DestroyBuilding();
                }
            }
        }
    }

}
