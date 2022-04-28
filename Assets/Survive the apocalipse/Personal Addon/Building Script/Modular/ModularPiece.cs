using System.Collections;
using CustomType;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ModularPiece : NetworkBehaviour
{
    public BoxCollider2D modularCollider;

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

    [SyncVar]
    public bool isMain;

    public GameObject electricBox;

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
    [SyncVar]
    public int level;

    public List<FlooreBasementOccupied> modularFloorPoint = new List<FlooreBasementOccupied>();

    public FloorPlacement floorPlacement;
    public SpriteRenderer floorPlacementSprite;

    public Color pColor;

    private Transform buildingTransform;

    public SpriteRenderer leftWallPointer;
    public SpriteRenderer rightWallPointer;
    public SpriteRenderer upWallPointer;
    public SpriteRenderer downWallPointer;

    public SpriteRenderer leftFloorPointer;
    public SpriteRenderer rightFloorPointer;
    public SpriteRenderer upFloorPointer;
    public SpriteRenderer downFloorPointer;

    public GameObject upWall;
    public GameObject upDoor;
    public GameObject leftWall;
    public GameObject leftDoor;
    public GameObject rightWall;
    public GameObject rightDoor;
    public GameObject downWall;
    public GameObject downDoor;

    private NavMeshObstacle2D[] navMeshObstacle2Ds;

    public List<Transform> floorDoor = new List<Transform>();
    public Transform upTransfomrDoor;
    public Transform downTransfomrDoor;
    public Transform leftTransfomrDoor;
    public Transform rightTransfomrDoor;

    //[SyncVar]
    //public int playerInsideLeftDoor;
    //[SyncVar]
    //public int playerInsideRightDoor;
    //[SyncVar]
    //public int playerInsideUpDoor;
    //[SyncVar]
    //public int playerInsideDownDoor;

    public List<Collider2D> buildingColliders = new List<Collider2D>();
    public List<Collider2D> fornitureColliders = new List<Collider2D>();
    public List<Collider2D> playerInside = new List<Collider2D>();

    public GameObject roof;

    public List<ModularObject> insideModularObject = new List<ModularObject>();

    private NavMeshObstacle2D[] leftDoors;
    private NavMeshObstacle2D[] leftWalls;
    private NavMeshObstacle2D[] rightDoors;
    private NavMeshObstacle2D[] rightWalls;
    private NavMeshObstacle2D[] upDoors;
    private NavMeshObstacle2D[] upWalls;
    private NavMeshObstacle2D[] downDoors;
    private NavMeshObstacle2D[] downWalls;

    public SpriteRenderer upDoorKey;
    public SpriteRenderer downDoorKey;
    public SpriteRenderer leftDoorKey;
    public SpriteRenderer rightDoorKey;

    [SyncVar (hook = "PlayDoorSoundUp")]
    public bool doorUpOpen;
    [SyncVar(hook = "PlayDoorSoundDown")]
    public bool doorDownOpen;
    [SyncVar(hook = "PlayDoorSoundLeft")]
    public bool doorLeftOpen;
    [SyncVar(hook = "PlayDoorSoundRight")]
    public bool doorRightOpen;

    public void OnDestroy()
    {
        ModularBuildingManager.singleton.allModularPiece.Remove(this);
        clientdownComponent = -5;
        clientupComponent = -5;
        clientleftComponent = -5;
        clientrightComponent = -5;
        CancelInvoke();
        CheckWall();
    }

    void Start()
    {
        buildingTransform = GetComponent<Transform>();
        name.Replace("(Clone)", "");
        name = name + UnityEngine.Random.Range(0, 100000);

        leftDoors = leftDoor.GetComponents<NavMeshObstacle2D>();
        leftWalls = leftWall.GetComponents<NavMeshObstacle2D>();
        rightDoors = rightDoor.GetComponents<NavMeshObstacle2D>();
        rightWalls = rightWall.GetComponents<NavMeshObstacle2D>();
        upDoors = upDoor.GetComponents<NavMeshObstacle2D>();
        upWalls = upWall.GetComponents<NavMeshObstacle2D>();
        downDoors = downDoor.GetComponents<NavMeshObstacle2D>();
        downWalls = downWall.GetComponents<NavMeshObstacle2D>();

        if (isClient)
        {
            Invoke(nameof(SetColor), 0.2f);
            InvokeRepeating(nameof(CheckWall), 0.5f,0.5f);
            electricBox.SetActive(isMain);
            if ((leftComponent != -5 && rightComponent != -5 && upComponent != -5 && downComponent != -5) ||
                (leftComponent != -5 && rightComponent != -5) || (upComponent != -5 && downComponent != -5))
            {
                roof.SetActive(true);
            }
        }
        else if (isServer)
        {
            Invoke(nameof(SyncStartValue), 0.7f);
        }

        if(ModularBuildingManager.singleton) ModularBuildingManager.singleton.allModularPiece.Add(this);
        else Invoke(nameof(RegisterModularPiece), 2.0f);
        Invoke(nameof(CallCheckWall), 0.5f);
    }

    public void RegisterModularPiece()
    {
        ModularBuildingManager.singleton.allModularPiece.Add(this);
    }

    [ClientRpc]
    public void RpcRebuildMain(NetworkIdentity id, bool condition)
    {
        id.GetComponent<ModularPiece>().electricBox.SetActive(condition);
    }

    public void CallCheckWall()
    {
        if (netIdentity.netId == 0)
        {
            Invoke(nameof(CallCheckWall), 0.5f);

        }
        else
        {
            CheckWall();
            if (ModularBuildingManager.singleton.inThisCollider)
                ModularBuildingManager.singleton.DisableRoof();
            else
            {
                Invoke(nameof(CheckRoofAtStart), 0.5f);
            }
        }
    }

    public void CheckRoofAtStart()
    {
        if (ModularBuildingManager.singleton.inThisCollider)
            ModularBuildingManager.singleton.DisableRoof();
        else
            Invoke(nameof(CheckRoofAtStart), 0.5f);
    }

    public void SyncStartValue()
    {
        if (netIdentity.netId != 0)
        {
            if (isServer)
            {
                clientdownComponent = downComponent;
                clientupComponent = upComponent;
                clientleftComponent = leftComponent;
                clientrightComponent = rightComponent;
            }
        }
        else
        {
            Invoke(nameof(SyncStartValue), 0.7f);
        }
    }

    public void CheckWall()
    {
        floorDoor.Clear();
        if (leftComponent != -5)
        {
            if (leftComponent == 0)
            {
                leftWall.SetActive(true);
                if(floorDoor.Contains(leftTransfomrDoor))floorDoor.Remove(leftTransfomrDoor);
                foreach (NavMeshObstacle2D obstacle2D in leftWalls)
                {
                    obstacle2D.Spawn();
                }
            }
            else if (leftComponent == 1)
            {
                leftDoor.SetActive(true);
                if (!floorDoor.Contains(leftTransfomrDoor)) floorDoor.Add(leftTransfomrDoor);
                foreach (NavMeshObstacle2D obstacle2D in leftDoors)
                {
                    obstacle2D.Spawn();
                }
            }
        }
        else
        {
            if (clientleftComponent != -5)
            {
                if (clientleftComponent == 0)
                {
                    leftWall.SetActive(true);
                }
                else if (clientleftComponent == 1)
                {
                    leftDoor.SetActive(true);
                }
            }
            else
            {
                leftWall.SetActive(false);
                leftDoor.SetActive(false);
            }
            foreach (NavMeshObstacle2D obstacle2D in leftWalls)
            {
                Destroy(obstacle2D.go);
                obstacle2D.go = null;
            }
            foreach (NavMeshObstacle2D obstacle2D in leftDoors)
            {
                Destroy(obstacle2D.go);
                obstacle2D.go = null;
            }
        }



        if (rightComponent != -5)
        {
            if (rightComponent == 0)
            {
                rightWall.SetActive(true);
                if (floorDoor.Contains(rightTransfomrDoor)) floorDoor.Remove(rightTransfomrDoor);
                foreach (NavMeshObstacle2D obstacle2D in rightWalls)
                {
                    obstacle2D.Spawn();
                }
            }
            else if (rightComponent == 1)
            {
                rightDoor.SetActive(true);
                if (!floorDoor.Contains(rightTransfomrDoor)) floorDoor.Add(rightTransfomrDoor);
                foreach (NavMeshObstacle2D obstacle2D in rightDoors)
                {
                    obstacle2D.Spawn();
                }
            }
        }
        else
        {
            if (clientrightComponent != -5)
            {
                if (clientrightComponent == 0)
                {
                    rightWall.SetActive(true);
                }
                else if (clientrightComponent == 1)
                {
                    rightDoor.SetActive(true);
                }
            }
            else
            {
                rightWall.SetActive(false);
                rightDoor.SetActive(false);
            }
            foreach (NavMeshObstacle2D obstacle2D in rightWalls)
            {
                Destroy(obstacle2D.go);
                obstacle2D.go = null;
            }
            foreach (NavMeshObstacle2D obstacle2D in rightDoors)
            {
                Destroy(obstacle2D.go);
                obstacle2D.go = null;
            }
        }


        if (upComponent != -5)
        {
            if (upComponent == 0)
            {
                upWall.SetActive(true);
                if (floorDoor.Contains(upTransfomrDoor)) floorDoor.Remove(upTransfomrDoor);
                foreach (NavMeshObstacle2D obstacle2D in upWalls)
                {
                    obstacle2D.Spawn();
                }
            }
            else if (upComponent == 1)
            {
                upDoor.SetActive(true);
                if (!floorDoor.Contains(upTransfomrDoor)) floorDoor.Add(upTransfomrDoor);
                foreach (NavMeshObstacle2D obstacle2D in upDoors)
                {
                    obstacle2D.Spawn();
                }
            }
        }
        else
        {
            if (clientupComponent != -5)
            {
                if (clientupComponent == 0)
                {
                    upWall.SetActive(true);
                }
                else if (clientupComponent == 1)
                {
                    upDoor.SetActive(true);
                }
            }
            else
            {
                upWall.SetActive(false);
                upDoor.SetActive(false);
            }
            foreach (NavMeshObstacle2D obstacle2D in upWalls)
            {
                Destroy(obstacle2D.go);
                obstacle2D.go = null;
            }
            foreach (NavMeshObstacle2D obstacle2D in upDoors)
            {
                Destroy(obstacle2D.go);
                obstacle2D.go = null;
            }
        }


        if (downComponent != -5)
        {
            if (downComponent == 0)
            {
                downWall.SetActive(true);
                if (floorDoor.Contains(downTransfomrDoor)) floorDoor.Remove(downTransfomrDoor);
                foreach (NavMeshObstacle2D obstacle2D in downWalls)
                {
                    obstacle2D.Spawn();
                }
            }
            else if (downComponent == 1)
            {
                downDoor.SetActive(true);
                if (!floorDoor.Contains(downTransfomrDoor)) floorDoor.Add(downTransfomrDoor);
                foreach (NavMeshObstacle2D obstacle2D in downDoors)
                {
                    obstacle2D.Spawn();
                }
            }
        }
        else
        {
            if (clientdownComponent != -5)
            {
                if (clientdownComponent == 0)
                {
                    downWall.SetActive(true);
                }
                else if (clientdownComponent == 1)
                {
                    downDoor.SetActive(true);
                }
            }
            else
            {
                downWall.SetActive(false);
                downDoor.SetActive(false);
            }
            foreach (NavMeshObstacle2D obstacle2D in downWalls)
            {
                Destroy(obstacle2D.go);
                obstacle2D.go = null;
            }
            foreach (NavMeshObstacle2D obstacle2D in downDoors)
            {
                Destroy(obstacle2D.go);
                obstacle2D.go = null;
            }
        }

    }

    public void CheckFloorDoor()
    {
        floorDoor.Clear();
        if (leftComponent != -5)
        {
            if (leftComponent == 0)
            {
                if (floorDoor.Contains(leftTransfomrDoor)) floorDoor.Remove(leftTransfomrDoor);
            }
            else if (leftComponent == 1)
            {
                if (!floorDoor.Contains(leftTransfomrDoor)) floorDoor.Add(leftTransfomrDoor);
            }
        }

        if (rightComponent != -5)
        {
            if (rightComponent == 0)
            {
                if (floorDoor.Contains(rightTransfomrDoor)) floorDoor.Remove(rightTransfomrDoor);
            }
            else if (rightComponent == 1)
            {
                if (!floorDoor.Contains(rightTransfomrDoor)) floorDoor.Add(rightTransfomrDoor);
            }
        }

        if (upComponent != -5)
        {
            if (upComponent == 0)
            {
                if (floorDoor.Contains(upTransfomrDoor)) floorDoor.Remove(upTransfomrDoor);
            }
            else if (upComponent == 1)
            {
                if (!floorDoor.Contains(upTransfomrDoor)) floorDoor.Add(upTransfomrDoor);
            }
        }

        if (downComponent != -5)
        {
            if (downComponent == 0)
            {
                if (floorDoor.Contains(downTransfomrDoor)) floorDoor.Remove(downTransfomrDoor);
            }
            else if (downComponent == 1)
            {
                if (!floorDoor.Contains(downTransfomrDoor)) floorDoor.Add(downTransfomrDoor);
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
                Invoke(nameof(SetColor), 0.2f);
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

        if (player.playerBuilding.actualBuilding) Destroy(player.playerBuilding.actualBuilding);
        player.playerBuilding.actualBuilding = null;
        player.playerBuilding.building = null;
        player.playerBuilding.inventoryIndex = -1;
        if (GeneralManager.singleton.spawnedBuildingObject) Destroy(GeneralManager.singleton.spawnedBuildingObject);
        ModularBuildingManager.singleton.ableModificationMode = false;
        ModularBuildingManager.singleton.ableModificationWallMode = false;
        ModularBuildingManager.singleton.CheckModularWallAfterDeath();
        ModularBuildingManager.singleton.isInitialBasement = true;

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
                        player.playerBuilding.CmdSpawnBasement(player.playerBuilding.inventoryIndex, player.playerBuilding.building.name, new Vector2(player.playerBuilding.actualBuilding.transform.position.x, player.playerBuilding.actualBuilding.transform.position.y), player.playerBuilding.invBelt, 0, ModularBuildingManager.singleton.isInitialBasement, ModularBuildingManager.singleton.selectedPiece == null ? -5 : ModularBuildingManager.singleton.selectedPiece.modularIndex, !ModularBuildingManager.singleton.ableModificationMode);
                        DestroyBuilding();
                    }
                }
                else if (player.playerBuilding.building.isWall || player.playerBuilding.building.isDoor)
                {
                    player.playerBuilding.CmdSyncWallDoor(player.playerBuilding.actualBuilding.GetComponent<NetworkIdentity>(), modularPiece.clientupComponent, modularPiece.clientdownComponent, modularPiece.clientleftComponent, modularPiece.clientrightComponent, player.playerBuilding.invBelt, player.playerBuilding.inventoryIndex);
                    DestroyBuilding();
                }
            }
        }
    }

    public void PlayDoorSoundUp(bool oldState, bool newState)
    {
        if (!Player.localPlayer.playerOptions.blockSound)
        {
            if (newState == true)
            {
                if (Vector2.Distance(Player.localPlayer.transform.position, upTransfomrDoor.position) < 5)
                {
                    upTransfomrDoor.GetComponent<DoorTrigger>().audioSource.PlayOneShot(ModularBuildingManager.singleton.openDoorSound);
                }
            }
            else
            {
                if (Vector2.Distance(Player.localPlayer.transform.position, upTransfomrDoor.position) < 5)
                {
                    upTransfomrDoor.GetComponent<DoorTrigger>().audioSource.PlayOneShot(ModularBuildingManager.singleton.closeDoorSound);
                }
            }
        }
    }

    public void PlayDoorSoundDown(bool oldState, bool newState)
    {
        if (!Player.localPlayer.playerOptions.blockSound)
        {
            if (newState == true)
            {
                if (Vector2.Distance(Player.localPlayer.transform.position, downTransfomrDoor.position) < 5)
                {
                    downTransfomrDoor.GetComponent<DoorTrigger>().audioSource.PlayOneShot(ModularBuildingManager.singleton.openDoorSound);
                }
            }
            else
            {
                if (Vector2.Distance(Player.localPlayer.transform.position, downTransfomrDoor.position) < 5)
                {
                    downTransfomrDoor.GetComponent<DoorTrigger>().audioSource.PlayOneShot(ModularBuildingManager.singleton.closeDoorSound);
                }
            }
        }
    }

    public void PlayDoorSoundLeft(bool oldState, bool newState)
    {
        if (!Player.localPlayer.playerOptions.blockSound)
        {
            if (newState == true)
            {
                if (Vector2.Distance(Player.localPlayer.transform.position, leftTransfomrDoor.position) < 5)
                {
                    leftTransfomrDoor.GetComponent<DoorTrigger>().audioSource.PlayOneShot(ModularBuildingManager.singleton.openDoorSound);
                }
            }
            else
            {
                if (Vector2.Distance(Player.localPlayer.transform.position, leftTransfomrDoor.position) < 5)
                {
                    leftTransfomrDoor.GetComponent<DoorTrigger>().audioSource.PlayOneShot(ModularBuildingManager.singleton.closeDoorSound);
                }
            }
        }
    }

    public void PlayDoorSoundRight(bool oldState, bool newState)
    {
        if (!Player.localPlayer.playerOptions.blockSound)
        {
            if (newState == true)
            {
                if (Vector2.Distance(Player.localPlayer.transform.position, rightTransfomrDoor.position) < 5)
                {
                    rightTransfomrDoor.GetComponent<DoorTrigger>().audioSource.PlayOneShot(ModularBuildingManager.singleton.openDoorSound);
                }
            }
            else
            {
                if (Vector2.Distance(Player.localPlayer.transform.position, rightTransfomrDoor.position) < 5)
                {
                    rightTransfomrDoor.GetComponent<DoorTrigger>().audioSource.PlayOneShot(ModularBuildingManager.singleton.closeDoorSound);
                }
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("FloorChecker"))
        {
            if (!playerInside.Contains(collider))
            {
                playerInside.Add(collider);
                ModularBuildingManager.singleton.inThisCollider = modularCollider;
            }
        }
    }

    public void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.CompareTag("Building"))
        {
            if (!buildingColliders.Contains(collider))
            {
                buildingColliders.Add(collider);
            }
        }
        else if (collider.CompareTag("Forniture"))
        {
            if (!fornitureColliders.Contains(collider))
            {
                fornitureColliders.Add(collider);
            }
        }
        else if (collider.CompareTag("FloorChecker"))
        {
            if (!playerInside.Contains(collider))
            {
                playerInside.Add(collider);
                ModularBuildingManager.singleton.inThisCollider = modularCollider;
            }
        }
    }


    public void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Building"))
        {
            if (buildingColliders.Contains(collider))
            {
                buildingColliders.Remove(collider);
            }
        }
        else if (collider.CompareTag("Forniture"))
        {
            if (fornitureColliders.Contains(collider))
            {
                fornitureColliders.Remove(collider);
            }
        }
        else if (collider.CompareTag("FloorChecker"))
        {
            if (playerInside.Contains(collider))
            {
                playerInside.Remove(collider);
                if (ModularBuildingManager.singleton.inThisCollider == modularCollider) ModularBuildingManager.singleton.inThisCollider = null;
                Invoke(nameof(CheckPlayerInside), 1.0f);
            }
        }
    }

    public void CheckPlayerInside()
    {
        ModularBuildingManager.singleton.DisableRoof();
    }


}
