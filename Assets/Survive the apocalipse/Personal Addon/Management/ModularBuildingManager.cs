using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class ModularBuildingManager : NetworkBehaviour
{
    public static ModularBuildingManager singleton;

    public Player player;

    [SyncVar]
    public int lastModularPieceIndexSpawn;

    public bool ableModificationMode;
    public bool ableModificationWallMode;

    public List<GameObject> wallOrdered = new List<GameObject>();

    public ScriptableBuilding building;

    public SpriteRenderer selectedPoint;

    public ModularPiece modularPiece;

    public ModularPiece selectedPiece;

    public Collider2D[] allColliders;

    public GameObject instantiatedUI;

    public BoxCollider2D inThisCollider;

    public bool isInitialBasement = true;

    public HashSet<ModularPiece> allModularPiece = new HashSet<ModularPiece>();

    public int insideIndex = -1;

    void Start()
    {
        if (!singleton) singleton = this;
    }

    public void AbleRoof()
    {
        insideIndex = inThisCollider.GetComponent<ModularPiece>().modularIndex;
        foreach (ModularPiece modularPiece in allModularPiece)
        {
            if(modularPiece.modularIndex == insideIndex)
                modularPiece.roof.SetActive(true);
        }
        insideIndex = -1;
    }

    public void DisableRoof()
    {
        if (inThisCollider)
        {
            insideIndex = inThisCollider.GetComponent<ModularPiece>().modularIndex;
            foreach (ModularPiece modularPiece in allModularPiece)
            {
                if (modularPiece.modularIndex == insideIndex) modularPiece.roof.SetActive(false);

                foreach (SortByDepth sortByDepth in modularPiece.sortPlus)
                {
                    sortByDepth.relatedToPlayer = true;
                    sortByDepth.amountRelatedToPlayer = 1;
                    sortByDepth.SetOrder();
                }

                foreach (SortByDepth sortByDepth in modularPiece.sortMinus)
                {
                    sortByDepth.relatedToPlayer = true;
                    sortByDepth.amountRelatedToPlayer = -1;
                    sortByDepth.SetOrder();
                }
            }
            insideIndex = -1;
        }
    }

    public void CheckModularWallAfterDeath()
    {
        if(modularPiece)
        {
            if (modularPiece.leftComponent != modularPiece.clientleftComponent) modularPiece.clientleftComponent = -5;
            if (modularPiece.rightComponent != modularPiece.clientrightComponent) modularPiece.clientrightComponent = -5;
            if (modularPiece.upComponent != modularPiece.clientupComponent) modularPiece.clientupComponent = -5;
            if (modularPiece.downComponent != modularPiece.clientdownComponent) modularPiece.clientdownComponent = -5;
            modularPiece.CheckWall();
            selectedPiece = null;
            modularPiece = null;
            isInitialBasement = true;
        }
    }

    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        if(ableModificationMode)
            FindNearestFloor(ableModificationMode);

        if (ableModificationWallMode)
            FindNearestWall();

        if (Input.GetMouseButtonDown(0) && !Utils.IsCursorOverUserInterface())
        {
            Vector3 screenPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hit = Physics2D.RaycastAll(screenPos, Vector2.zero);

            bool roofClosed = false;        

            for (int i = 0; i < hit.Length; i++)
            {
                int index = i;

                if (hit[index].collider.CompareTag("Roof"))
                {
                    roofClosed = true;
                }

                if (hit[index].collider.CompareTag("Electric box"))
                {
                    selectedPiece = hit[index].collider.transform.GetComponentInParent<ModularPiece>();
                    if (!GeneralManager.singleton.instantiatedElecteicBox) GeneralManager.singleton.instantiatedElecteicBox = Instantiate(GeneralManager.singleton.electricBoxObject, GeneralManager.singleton.canvas);
                    GeneralManager.singleton.instantiatedElecteicBox.GetComponent<UIElectricBox>().modularPiece = selectedPiece;
                }
                if (hit[index].collider.gameObject.layer == LayerMask.NameToLayer("Forniture") || hit[index].collider.gameObject.layer == LayerMask.NameToLayer("WallForniture"))
                {
                    selectedPiece = hit[index].collider.transform.GetComponentInParent<ModularPiece>();
                    if (!GeneralManager.singleton.instantiatedDeleteObject) GeneralManager.singleton.instantiatedDeleteObject = Instantiate(GeneralManager.singleton.deleteObject, GeneralManager.singleton.canvas);
                    GeneralManager.singleton.instantiatedDeleteObject.GetComponent<UIDeleteObject>().Assign(hit[index].collider.transform.GetComponentInParent<Forniture>().scriptableBuilding, hit[index].collider.transform.GetComponentInParent<ModularObject>().identity);
                }
                if (hit[index].collider.CompareTag("FloorBasement"))
                {
                    if(GeneralManager.singleton.CanDoOtherActionFloor(hit[index].collider.transform.GetComponentInParent<ModularPiece>(), Player.localPlayer))
                    selectedPiece = hit[index].collider.transform.GetComponentInParent<ModularPiece>();
                }
            }

            if (!roofClosed)
            {
                for (int i = 0; i < hit.Length; i++)
                {
                    int index = i;
                    if (hit[index].collider.gameObject.layer == LayerMask.NameToLayer("Forniture"))
                    {
                        player.playerMove.fornitureClient = hit[index].collider.gameObject.GetComponent<ModularObject>();
                        player.CmdSetForniture(hit[index].collider.GetComponent<NetworkIdentity>());
                        //if (instantiatedUI == null)
                        //{
                        //    instantiatedUI = Instantiate(Player.localPlayer.SearchUiToSpawnInManager(player.playerMove.forniture.GetComponent<ModularObject>().scriptableBuilding), GeneralManager.singleton.canvas);
                        //    return;
                        //}
                    }
                }
            }

            if (ableModificationMode)
            {
                for (int i = 0; i < hit.Length; i++)
                {
                    int index = i;
                    if (hit[index].collider.CompareTag("FloorPositioning"))
                    {
                        if (selectedPoint) selectedPoint.GetComponent<SpriteRenderer>().enabled = true;
                        selectedPoint = hit[index].collider.GetComponent<SpriteRenderer>();
                        if (Player.localPlayer.playerBuilding.actualBuilding)
                            Player.localPlayer.playerBuilding.actualBuilding.transform.position = selectedPoint.transform.position;
                        selectedPiece = selectedPoint.transform.GetComponentInParent<ModularPiece>();
                        selectedPoint = null;
                        isInitialBasement = false;
                    }
                }
            }
            if (ableModificationWallMode)
            {
                for (int i = 0; i < hit.Length; i++)
                {
                    int index = i;
                    if (hit[index].collider.CompareTag("WallPositioning"))
                    {
                        if(modularPiece)
                        {
                            ResetComponent(modularPiece);
                        }
                        selectedPoint = hit[index].collider.GetComponent<SpriteRenderer>();

                        modularPiece = selectedPoint.transform.GetComponentInParent<ModularPiece>();

                        ResetComponent(modularPiece);

                        if (selectedPoint.transform.GetComponent<FlooreBasementOccupied>().up)
                        {
                            if (Player.localPlayer.playerBuilding.building.isWall)
                            {
                                modularPiece.clientupComponent = 0;
                            }
                            if (Player.localPlayer.playerBuilding.building.isDoor)
                            {
                                modularPiece.clientupComponent = 1;
                            }
                        }
                        if (selectedPoint.transform.GetComponent<FlooreBasementOccupied>().left)
                        {
                            if (Player.localPlayer.playerBuilding.building.isWall)
                            {
                                modularPiece.clientleftComponent = 0;
                            }
                            if (Player.localPlayer.playerBuilding.building.isDoor)
                            {
                                modularPiece.clientleftComponent = 1;
                            }
                        }
                        if (selectedPoint.transform.GetComponent<FlooreBasementOccupied>().right)
                        {
                            if (Player.localPlayer.playerBuilding.building.isWall)
                            {
                                modularPiece.clientrightComponent = 0;
                            }
                            if (Player.localPlayer.playerBuilding.building.isDoor)
                            {
                                modularPiece.clientrightComponent = 1;
                            }
                        }
                        if (selectedPoint.transform.GetComponent<FlooreBasementOccupied>().down)
                        {
                            if (Player.localPlayer.playerBuilding.building.isWall)
                            {
                                modularPiece.clientdownComponent = 0;
                            }
                            if (Player.localPlayer.playerBuilding.building.isDoor)
                            {
                                modularPiece.clientdownComponent = 1;
                            }
                        }

                        modularPiece.CheckWall();
                    }
                }
            }
        }

        if (Player.localPlayer && Player.localPlayer.playerBuilding && Player.localPlayer.playerBuilding.actualBuilding)
        {
            if (UIBuilding.singleton) UIBuilding.singleton.spawn.interactable = true;
        }
    }

    public void ResetComponent(ModularPiece modular)
    {
        if (modular)
        {
            if (modular.downComponent == -5) modular.clientdownComponent = -5;
            if (modular.leftComponent == -5) modular.clientleftComponent = -5;
            if (modular.rightComponent == -5) modular.clientrightComponent = -5;
            if (modular.upComponent == -5) modular.clientupComponent = -5;
            modular.CheckWall();
        }
    }

    public int GetNewIndex()
    {
        lastModularPieceIndexSpawn++;
        return lastModularPieceIndexSpawn;
    }


    public void FindNearestFloor(bool condition)
    {
        GameObject[] floor = GameObject.FindGameObjectsWithTag("FloorBasement");
        wallOrdered = floor.Where(go => go.GetComponent<ModularPiece>() && GeneralManager.singleton.CanDoOtherActionFloor(go.GetComponent<ModularPiece>(), Player.localPlayer)).ToList();

        wallOrdered = wallOrdered.OrderBy(m => Vector2.Distance(Player.localPlayer.transform.position, m.transform.position)).ToList();

        for (int i = 0; i < wallOrdered.Count; i++)
        {
            int index = i;
            for (int e = 0; e < wallOrdered[index].GetComponent<ModularPiece>().modularFloorPoint.Count; e++)
            {
                int indexe = e;
                wallOrdered[index].GetComponent<ModularPiece>().modularFloorPoint[indexe].GetComponent<FlooreBasementOccupied>().spriteRenderer.enabled = condition;
            }
        }
    }

    public void FindNearestWall()
    {
        GameObject[] floor = GameObject.FindGameObjectsWithTag("FloorBasement");
        wallOrdered = floor.Where(go => go.GetComponent<ModularPiece>() && GeneralManager.singleton.CanDoOtherActionFloor(go.GetComponent<ModularPiece>(), Player.localPlayer)).ToList();

        wallOrdered = wallOrdered.OrderBy(m => Vector2.Distance(Player.localPlayer.transform.position, m.transform.position)).ToList();

        ModularPiece nearModularPiece = null;

        for (int i = 0; i < wallOrdered.Count; i++)
        {
            int index = i;
            ModularPiece piece = wallOrdered[index].GetComponent<ModularPiece>();

            nearModularPiece = (CheckFloorPresence(piece.leftFloorPointer.transform));
            piece.leftWallPointer.enabled = ((nearModularPiece == null && piece.leftComponent == -5) || (nearModularPiece != null && (nearModularPiece.rightComponent == -5 && piece.leftComponent == -5)));

            nearModularPiece = (CheckFloorPresence(piece.rightFloorPointer.transform));
            piece.rightWallPointer.enabled = ((nearModularPiece == null && piece.rightComponent == -5) || (nearModularPiece != null && (nearModularPiece.leftComponent == -5 && piece.rightComponent == -5)));

            nearModularPiece = (CheckFloorPresence(piece.upFloorPointer.transform));
            piece.upWallPointer.enabled = ((nearModularPiece == null && piece.upComponent == -5) || (nearModularPiece != null && (nearModularPiece.downComponent == -5 && piece.upComponent == -5)));

            nearModularPiece = (CheckFloorPresence(piece.downFloorPointer.transform));
            piece.downWallPointer.enabled = ((nearModularPiece == null && piece.downComponent == -5) || (nearModularPiece != null && (nearModularPiece.upComponent == -5 && piece.downComponent == -5)));
        }
    }

    public ModularPiece CheckFloorPresence(Transform pieceTransform)
    {
        Collider2D[] floorCollider = Physics2D.OverlapBoxAll(pieceTransform.position, new Vector2(pieceTransform.localScale.x, pieceTransform.localScale.y), GeneralManager.singleton.modularObjectLayerMask);

        for(int i = 0; i < floorCollider.Length; i++)
        {
            int index = i;
            if(floorCollider[index].gameObject.layer == LayerMask.NameToLayer("FloorBasement"))
            {
                return floorCollider[index].GetComponent<ModularPiece>();
            }
        }
        return null;
    }

    public void CheckFloorPresenceDebug(Transform pieceTransform, ModularPiece modular)
    {
        Collider2D[] floorCollider = Physics2D.OverlapBoxAll(pieceTransform.position, new Vector2(pieceTransform.localScale.x, pieceTransform.localScale.y), GeneralManager.singleton.modularObjectLayerMask);
        for(int i = 0; i < floorCollider.Length; i++)
        {
            int index = i;
        }
    }

    public void DisableNearestWall()
    {
        GameObject[] floor = GameObject.FindGameObjectsWithTag("FloorBasement");
        wallOrdered = floor.Where(go => go.GetComponent<ModularPiece>() && GeneralManager.singleton.CanDoOtherActionFloor(go.GetComponent<ModularPiece>(), Player.localPlayer)).ToList();

        wallOrdered = wallOrdered.OrderBy(m => Vector2.Distance(Player.localPlayer.transform.position, m.transform.position)).ToList();

        for (int i = 0; i < wallOrdered.Count; i++)
        {
            int index = i;
            ModularPiece piece = wallOrdered[index].GetComponent<ModularPiece>();

            piece.leftWallPointer.enabled = false;
            piece.rightWallPointer.enabled = false;
            piece.upWallPointer.enabled = false;
            piece.downWallPointer.enabled = false;

            piece.leftFloorPointer.enabled = false;
            piece.rightFloorPointer.enabled = false;
            piece.upFloorPointer.enabled = false;
            piece.downFloorPointer.enabled = false;
        }
    }

}
