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

    public List<ModularPiece> allRoof = new List<ModularPiece>();

    public GameObject instantiatedUI;

    void Start()
    {
        if (!singleton) singleton = this;
    }

    public void AbleRoof()
    {
        foreach (Collider2D modularPiece in allColliders)
        {
            modularPiece.GetComponent<ModularPiece>().roof.SetActive(true);
        }
    }

    public void DisableRoof()
    {
        foreach (Collider2D modularPiece in allColliders)
        {
            modularPiece.GetComponent<ModularPiece>().roof.SetActive(false);
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

        if(player.playerMove.forniture != null)
        {
            if(instantiatedUI == null)
            {
                instantiatedUI = Instantiate(Player.localPlayer.SearchUiToSpawnInManager(player.playerMove.forniture.GetComponent<Forniture>().scriptableBuilding), GeneralManager.singleton.canvas);
            }
        }
        else
        {
            if (instantiatedUI) Destroy(instantiatedUI.gameObject);
        }

        if (Input.GetMouseButtonDown(0))
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
            }

            if (!roofClosed)
            {
                for (int i = 0; i < hit.Length; i++)
                {
                    int index = i;
                    if (hit[index].collider.gameObject.layer == LayerMask.NameToLayer("Forniture"))
                    {
                        player.CmdSetForniture(hit[index].collider.GetComponent<NetworkIdentity>());
                    }
                }
            }

            for (int i = 0; i < hit.Length; i++)
            {
                int index = i;
                if (hit[index].collider.CompareTag("FloorBasement"))
                {
                    selectedPiece = hit[index].collider.transform.GetComponentInParent<ModularPiece>();
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

        for (int i = 0; i < wallOrdered.Count; i++)
        {
            int index = i;
            ModularPiece piece = wallOrdered[index].GetComponent<ModularPiece>();

            piece.leftWallPointer.enabled = piece.clientleftComponent == -5;
            piece.rightWallPointer.enabled = piece.clientrightComponent == -5;
            piece.upWallPointer.enabled = piece.clientupComponent == -5;
            piece.downWallPointer.enabled = piece.clientdownComponent == -5;
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
        }
    }

}
