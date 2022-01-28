using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using CustomType;

public class UISelectedItem : MonoBehaviour
{
    public static UISelectedItem singleton;
    public TextMeshProUGUI description;
    public Image itemImage;
    public Button useButton;
    public Button deleteButton;
    public Button closeItemButton;
    public Slider liquidSlider;
    public Button useLiquid;
    public TextMeshProUGUI minimumLiquid;
    public TextMeshProUGUI maximumLiquid;

    public List<Entity> woodWall = new List<Entity>();
    public List<Gate> gates = new List<Gate>();
    public List<GameObject> wallOrdered = new List<GameObject>();

    private bool instantiate;

    public void Start()
    {
        if (!singleton) singleton = this;
    }

    public void Update()
    {
        if (Player.localPlayer.health == 0) Destroy(this.gameObject);

        if (!Player.localPlayer.playerBuilding.invBelt)
        {
            if (UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.waterContainer > 0 ||
                UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.honeyContainer > 0)
            {
                liquidSlider.gameObject.SetActive(true);
                useLiquid.gameObject.SetActive(true);
                minimumLiquid.gameObject.SetActive(true);
                maximumLiquid.gameObject.SetActive(true);
                useButton.gameObject.SetActive(false);
                if (UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.waterContainer > 0)
                {
                    liquidSlider.maxValue = UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.waterContainer;
                    liquidSlider.minValue = 0;
                    minimumLiquid.text = "0";
                    maximumLiquid.text = UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.waterContainer.ToString();
                }
                if (UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.honeyContainer > 0)
                {
                    liquidSlider.maxValue = UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.honeyContainer;
                    liquidSlider.minValue = 0;
                    minimumLiquid.text = "0";
                    maximumLiquid.text = UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.honeyContainer.ToString();
                }
            }
            else
            {
                liquidSlider.gameObject.SetActive(false);
                useLiquid.gameObject.SetActive(false);
                minimumLiquid.gameObject.SetActive(false);
                maximumLiquid.gameObject.SetActive(false);
                if (UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.data.generalLiquidContainer > 0 &&
                    UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.waterContainer == 0 &&
                    UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.honeyContainer == 0)
                {
                    useButton.gameObject.SetActive(false);
                }
                else
                {
                    useButton.gameObject.SetActive(true);
                }
            }

            useLiquid.onClick.SetListener(() =>
            {
                if (UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.waterContainer > 0)
                {
                    Player.localPlayer.CmdUseWaterItem(Convert.ToInt32(liquidSlider.value), UIInventory.singleton.selectedItem, false);
                }
                if (UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.honeyContainer > 0)
                {
                    Player.localPlayer.CmdUseHoneyItem(Convert.ToInt32(liquidSlider.value), UIInventory.singleton.selectedItem, false);
                }
            });

            closeItemButton.onClick.SetListener(() =>
            {
                UIInventory.singleton.selectedItem = -1;
                UIOrderManager.singleton.btnCloseLastPanel.onClick.Invoke();
            });

            useButton.onClick.SetListener(() =>
            {
                if (UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.data is ScriptableBuilding)
                {
                    UIInventory.singleton.player.playerBuilding.building = ((ScriptableBuilding)UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.data);
                    UIInventory.singleton.player.playerBuilding.inventoryIndex = UIInventory.singleton.selectedItem;
                    if (UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.data.name == "Wood Wall")
                    {
                        instantiate = false;
                        FindNearestWoodwall();

                        if (woodWall.Count > 0)
                        {
                            for (int y = 0; y < woodWall.Count; y++)
                            {
                                if (instantiate == true) return;
                                Transform[] children = woodWall[y].transform.GetComponentsInChildren<Transform>();
                                foreach (Transform child in children)
                                {
                                    if (child.gameObject.layer == 15)
                                    {
                                        GameObject g = Instantiate(((ScriptableBuilding)UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.data).buildingList[0].buildingObject, new Vector3(child.transform.position.x, child.transform.position.y, 0.0f), Quaternion.identity);
                                        UIInventory.singleton.player.playerBuilding.actualBuilding = g;

                                        instantiate = true;

                                        if (GeneralManager.singleton.spawnedAttackObject)
                                        {
                                            Destroy(GeneralManager.singleton.spawnedAttackObject);
                                            GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                        }
                                        else
                                        {
                                            GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                        }

                                        foreach (Transform woodWallChildren in woodWall[y].transform)
                                        {
                                            if (woodWallChildren.gameObject.layer == 15)
                                                GeneralManager.singleton.spawnedBuildingObject.GetComponent<UIBuilding>().childpositioning.Add(new Positioning(woodWallChildren, 0));
                                            else if (woodWallChildren.gameObject.layer == 16)
                                                GeneralManager.singleton.spawnedBuildingObject.GetComponent<UIBuilding>().childpositioning.Add(new Positioning(woodWallChildren, 1));
                                        }

                                        Destroy(this.gameObject);
                                        UIOrderManager.singleton.btnCloseLastPanel.onClick.Invoke();
                                        return;
                                    }
                                }
                            }
                            if (instantiate == false)
                            {
                                GameObject g = Instantiate(((ScriptableBuilding)UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.data).buildingList[0].buildingObject, new Vector3(Player.localPlayer.transform.position.x, Player.localPlayer.transform.position.y, 0.0f), Quaternion.identity);
                                UIInventory.singleton.player.playerBuilding.actualBuilding = g;
                                if (GeneralManager.singleton.spawnedAttackObject)
                                {
                                    Destroy(GeneralManager.singleton.spawnedAttackObject);
                                    GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                }
                                else
                                {
                                    GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                }
                                return;
                            }
                            instantiate = false;
                        }
                        else
                        {
                            GameObject g = Instantiate(((ScriptableBuilding)UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.data).buildingList[0].buildingObject, new Vector3(Player.localPlayer.transform.position.x, Player.localPlayer.transform.position.y, 0.0f), Quaternion.identity);
                            UIInventory.singleton.player.playerBuilding.actualBuilding = g;
                            if (GeneralManager.singleton.spawnedAttackObject)
                            {
                                Destroy(GeneralManager.singleton.spawnedAttackObject);
                                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                            }
                            else
                            {
                                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                            }
                        }
                    }
                    else if (UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.data.name == "Gate")
                    {
                        instantiate = false;
                        FindNearestWoodwall();

                        if (woodWall.Count > 0)
                        {
                            for (int y = 0; y < woodWall.Count; y++)
                            {
                                if (instantiate == true) return;
                                Transform[] children = woodWall[y].transform.GetComponentsInChildren<Transform>();
                                foreach (Transform child in children)
                                {
                                    if (child.gameObject.layer == 18)
                                    {
                                        GameObject g = Instantiate(((ScriptableBuilding)UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.data).buildingList[0].buildingObject, new Vector3(child.transform.position.x, child.transform.position.y, 0.0f), Quaternion.identity);
                                        UIInventory.singleton.player.playerBuilding.actualBuilding = g;

                                        instantiate = true;

                                        if (GeneralManager.singleton.spawnedAttackObject)
                                        {
                                            Destroy(GeneralManager.singleton.spawnedAttackObject);
                                            GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                        }
                                        else
                                        {
                                            GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                        }

                                        foreach (Transform woodWallChildren in woodWall[y].transform)
                                        {
                                            if (woodWallChildren.gameObject.layer == 18)
                                                GeneralManager.singleton.spawnedBuildingObject.GetComponent<UIBuilding>().childpositioning.Add(new Positioning(woodWallChildren, 0));
                                            //else if (woodWallChildren.gameObject.layer == 16)
                                            //    GeneralManager.singleton.spawnedBuildingObject.GetComponent<UIBuilding>().childpositioning.Add(new Positioning(woodWallChildren, 1));
                                        }

                                        Destroy(this.gameObject);
                                        UIOrderManager.singleton.btnCloseLastPanel.onClick.Invoke();
                                        return;
                                    }
                                }
                            }
                            if (instantiate == false)
                            {
                                GameObject g = Instantiate(((ScriptableBuilding)UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.data).buildingList[0].buildingObject, new Vector3(Player.localPlayer.transform.position.x, Player.localPlayer.transform.position.y, 0.0f), Quaternion.identity);
                                UIInventory.singleton.player.playerBuilding.actualBuilding = g;
                                if (GeneralManager.singleton.spawnedAttackObject)
                                {
                                    Destroy(GeneralManager.singleton.spawnedAttackObject);
                                    GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                }
                                else
                                {
                                    GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                }
                                return;
                            }
                            instantiate = false;
                        }
                        else
                        {
                            GameObject g = Instantiate(((ScriptableBuilding)UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.data).buildingList[0].buildingObject, new Vector3(Player.localPlayer.transform.position.x, Player.localPlayer.transform.position.y, 0.0f), Quaternion.identity);
                            UIInventory.singleton.player.playerBuilding.actualBuilding = g;
                            if (GeneralManager.singleton.spawnedAttackObject)
                            {
                                Destroy(GeneralManager.singleton.spawnedAttackObject);
                                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                            }
                            else
                            {
                                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                            }
                        }
                    }
                    else if (UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.data.name == "Barbwire")
                    {
                        instantiate = false;
                        FindNearesBarbwire();

                        if (woodWall.Count > 0)
                        {
                            for (int y = 0; y < woodWall.Count; y++)
                            {
                                if (instantiate == true) return;
                                Transform[] children = woodWall[y].transform.GetComponentsInChildren<Transform>();
                                foreach (Transform child in children)
                                {
                                    if (child.gameObject.layer == 28)
                                    {
                                        GameObject g = Instantiate(((ScriptableBuilding)UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.data).buildingList[0].buildingObject, new Vector3(child.transform.position.x, child.transform.position.y, 0.0f), Quaternion.identity);
                                        UIInventory.singleton.player.playerBuilding.actualBuilding = g;

                                        instantiate = true;

                                        if (GeneralManager.singleton.spawnedAttackObject)
                                        {
                                            Destroy(GeneralManager.singleton.spawnedAttackObject);
                                            GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                        }
                                        else
                                        {
                                            GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                        }

                                        foreach (Transform woodWallChildren in woodWall[y].transform)
                                        {
                                            if (woodWallChildren.gameObject.layer == 28)
                                                GeneralManager.singleton.spawnedBuildingObject.GetComponent<UIBuilding>().childpositioning.Add(new Positioning(woodWallChildren, 0));
                                            else if (woodWallChildren.gameObject.layer == 29)
                                                GeneralManager.singleton.spawnedBuildingObject.GetComponent<UIBuilding>().childpositioning.Add(new Positioning(woodWallChildren, 1));
                                        }

                                        Destroy(this.gameObject);
                                        UIOrderManager.singleton.btnCloseLastPanel.onClick.Invoke();
                                        return;
                                    }
                                }
                            }
                            if (instantiate == false)
                            {
                                GameObject g = Instantiate(((ScriptableBuilding)UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.data).buildingList[0].buildingObject, new Vector3(Player.localPlayer.transform.position.x, Player.localPlayer.transform.position.y, 0.0f), Quaternion.identity);
                                UIInventory.singleton.player.playerBuilding.actualBuilding = g;
                                if (GeneralManager.singleton.spawnedAttackObject)
                                {
                                    Destroy(GeneralManager.singleton.spawnedAttackObject);
                                    GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                }
                                else
                                {
                                    GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                }
                                return;
                            }
                            instantiate = false;
                        }
                        else
                        {
                            GameObject g = Instantiate(((ScriptableBuilding)UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.data).buildingList[0].buildingObject, new Vector3(Player.localPlayer.transform.position.x, Player.localPlayer.transform.position.y, 0.0f), Quaternion.identity);
                            UIInventory.singleton.player.playerBuilding.actualBuilding = g;
                            if (GeneralManager.singleton.spawnedAttackObject)
                            {
                                Destroy(GeneralManager.singleton.spawnedAttackObject);
                                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                            }
                            else
                            {
                                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                            }
                        }
                    }
                    else if (UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.data.name == "Flag")
                    {
                        Instantiate(GeneralManager.singleton.flagManager, GeneralManager.singleton.canvas);
                    }
                    else if (((ScriptableBuilding)UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.data).isBasement)
                    {
                        if (FindNearestFloor())
                        {
                            GameObject g = Instantiate(((ScriptableBuilding)UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.data).buildingList[0].buildingObject, new Vector3(Player.localPlayer.transform.position.x, Player.localPlayer.transform.position.y, 0.0f), Quaternion.identity);
                            UIInventory.singleton.player.playerBuilding.actualBuilding = g;
                            if (GeneralManager.singleton.spawnedAttackObject)
                            {
                                Destroy(GeneralManager.singleton.spawnedAttackObject);
                                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                            }
                            else
                            {
                                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                            }
                            ModularBuildingManager.singleton.ableModificationMode = false;
                        }
                        else
                        {
                            ModularBuildingManager.singleton.building = ((ScriptableBuilding)UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.data);
                            UIInventory.singleton.player.playerBuilding.actualBuilding = Instantiate(((ScriptableBuilding)UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.data).buildingList[0].buildingObject, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);

                            ModularBuildingManager.singleton.ableModificationMode = true;

                            if (GeneralManager.singleton.spawnedAttackObject)
                            {
                                Destroy(GeneralManager.singleton.spawnedAttackObject);
                                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                            }
                            else
                            {
                                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                            }

                            GeneralManager.singleton.spawnedBuildingObject.GetComponent<UIBuilding>().DisableButton();
                        }
                    }
                    else if (((ScriptableBuilding)UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.data).isWall ||
                            ((ScriptableBuilding)UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.data).isDoor)
                    {

                        if (FindNearestFloorForWallDoor())
                        {
                            ModularBuildingManager.singleton.building = ((ScriptableBuilding)UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.data);
                            ModularBuildingManager.singleton.ableModificationWallMode = true;
                            if (GeneralManager.singleton.spawnedAttackObject)
                            {
                                Destroy(GeneralManager.singleton.spawnedAttackObject);
                                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                            }
                            else
                            {
                                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                            }

                            GeneralManager.singleton.spawnedBuildingObject.GetComponent<UIBuilding>().DisableButton();
                        }
                        else
                        {
                            UIInventory.singleton.player.playerBuilding.building = null;
                            UIInventory.singleton.player.playerBuilding.inventoryIndex = -1;
                            UIInventory.singleton.selectedItem = -1;
                            ModularBuildingManager.singleton.ableModificationWallMode = false;
                            ModularBuildingManager.singleton.ableModificationMode = false;
                        }
                    }
                    else if (((ScriptableBuilding)UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.data).modularAccessory)
                    {
                        GameObject g = Instantiate(((ScriptableBuilding)Player.localPlayer.inventory[UIInventory.singleton.selectedItem].item.data).buildingList[0].buildingObject, new Vector3(Player.localPlayer.transform.position.x, Player.localPlayer.transform.position.y, 0.0f), Quaternion.identity);
                        UIInventory.singleton.player.playerBuilding.actualBuilding = g;

                        if (GeneralManager.singleton.spawnedAttackObject)
                        {
                            Destroy(GeneralManager.singleton.spawnedAttackObject);
                            GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                        }
                        else
                        {
                            GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                        }
                    }
                    else
                    {
                        GameObject g = Instantiate(((ScriptableBuilding)UIInventory.singleton.player.inventory[Player.localPlayer.playerBuilding.inventoryIndex].item.data).buildingList[0].buildingObject, new Vector3(Player.localPlayer.transform.position.x, Player.localPlayer.transform.position.y, 0.0f), Quaternion.identity);
                        Player.localPlayer.playerBuilding.actualBuilding = g;
                        if (GeneralManager.singleton.spawnedAttackObject)
                        {
                            Destroy(GeneralManager.singleton.spawnedAttackObject);
                            GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                        }
                        else
                        {
                            GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                        }
                        Destroy(this.gameObject);
                    }
                }               
                else
                {
                    if (UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.data is ScriptableItem &&
                        UIInventory.singleton.player.inventory[UIInventory.singleton.selectedItem].item.name == "Building Crafter")
                        Instantiate(GeneralManager.singleton.buildingCrafterPanel, GeneralManager.singleton.canvas);

                    if (Player.localPlayer.inventory[UIInventory.singleton.selectedItem].item.data is PetItem usablePet &&
                        usablePet.CanUse(Player.localPlayer, UIInventory.singleton.selectedItem))
                    {
                        Player.localPlayer.CmdUseInventoryItem(UIInventory.singleton.selectedItem);
                    }
                    else if (Player.localPlayer.inventory[UIInventory.singleton.selectedItem].item.data is UsableItem usable &&
                        usable.CanUse(Player.localPlayer, UIInventory.singleton.selectedItem))
                    {
                        Player.localPlayer.CmdUseInventoryItem(UIInventory.singleton.selectedItem);
                    }
                }
                closeItemButton.onClick.Invoke();
            });

            deleteButton.gameObject.SetActive(true);
            deleteButton.onClick.SetListener(() =>
            {
                UIInventory.singleton.player.CmdDeleteItem(UIInventory.singleton.selectedItem);
                closeItemButton.onClick.Invoke();
            });
        }
        else
        {
            if (Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.waterContainer > 0 ||
                Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.honeyContainer > 0)
            {
                liquidSlider.gameObject.SetActive(true);
                useLiquid.gameObject.SetActive(true);
                minimumLiquid.gameObject.SetActive(true);
                maximumLiquid.gameObject.SetActive(true);
                useButton.gameObject.SetActive(false);
                if (Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.waterContainer > 0)
                {
                    liquidSlider.maxValue = Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.waterContainer;
                    liquidSlider.minValue = 0;
                    minimumLiquid.text = "0";
                    maximumLiquid.text = Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.waterContainer.ToString();
                }
                if (Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.honeyContainer > 0)
                {
                    liquidSlider.maxValue = Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.honeyContainer;
                    liquidSlider.minValue = 0;
                    minimumLiquid.text = "0";
                    maximumLiquid.text = Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.honeyContainer.ToString();
                }
            }
            else
            {
                liquidSlider.gameObject.SetActive(false);
                useLiquid.gameObject.SetActive(false);
                minimumLiquid.gameObject.SetActive(false);
                maximumLiquid.gameObject.SetActive(false);
                if (Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data.generalLiquidContainer > 0 &&
                    Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.waterContainer == 0 &&
                    Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.honeyContainer == 0)
                {
                    useButton.gameObject.SetActive(false);
                }
                else
                {
                    useButton.gameObject.SetActive(true);
                }
            }

            useLiquid.onClick.SetListener(() =>
            {
                if (Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.waterContainer > 0)
                {
                    Player.localPlayer.CmdUseWaterItem(Convert.ToInt32(liquidSlider.value), Player.localPlayer.playerBuilding.inventoryIndex, true);
                }
                if (Player.localPlayer.playerBelt.belt[UIInventory.singleton.selectedItem].item.honeyContainer > 0)
                {
                    Player.localPlayer.CmdUseHoneyItem(Convert.ToInt32(liquidSlider.value), Player.localPlayer.playerBuilding.inventoryIndex, true);
                }
            });

            closeItemButton.onClick.SetListener(() =>
            {
                Player.localPlayer.playerBuilding.invBelt = false;
                Player.localPlayer.playerBuilding.inventoryIndex = -1;
                Destroy(this.gameObject);
            });

            useButton.onClick.SetListener(() =>
            {
                if (Player.localPlayer.playerBuilding.invBelt)
                {
                    if (Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data is ScriptableBuilding)
                    {
                        Player.localPlayer.playerBuilding.building = ((ScriptableBuilding)Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data);

                        if (Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data.name == "Wood Wall")
                        {
                            instantiate = false;
                            FindNearestWoodwall();

                            if (woodWall.Count > 0)
                            {
                                for (int y = 0; y < woodWall.Count; y++)
                                {
                                    if (instantiate == true) return;
                                    Transform[] children = woodWall[y].transform.GetComponentsInChildren<Transform>();
                                    foreach (Transform child in children)
                                    {
                                        if (child.gameObject.layer == 15)
                                        {
                                            GameObject g = Instantiate(((ScriptableBuilding)Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data).buildingList[0].buildingObject, new Vector3(child.transform.position.x, child.transform.position.y, 0.0f), Quaternion.identity);
                                            Player.localPlayer.playerBuilding.actualBuilding = g;

                                            instantiate = true;
                                            if (GeneralManager.singleton.spawnedAttackObject)
                                            {
                                                Destroy(GeneralManager.singleton.spawnedAttackObject);
                                                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                            }
                                            else
                                            {
                                                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                            }

                                            foreach (Transform woodWallChildren in woodWall[y].transform)
                                            {
                                                if (woodWallChildren.gameObject.layer == 15)
                                                    GeneralManager.singleton.spawnedBuildingObject.GetComponent<UIBuilding>().childpositioning.Add(new Positioning(woodWallChildren, 0));
                                                else if (woodWallChildren.gameObject.layer == 16)
                                                    GeneralManager.singleton.spawnedBuildingObject.GetComponent<UIBuilding>().childpositioning.Add(new Positioning(woodWallChildren, 1));
                                            }

                                            Destroy(this.gameObject);
                                            UIOrderManager.singleton.btnCloseLastPanel.onClick.Invoke();
                                            return;
                                        }
                                    }
                                }
                                if (instantiate == false)
                                {
                                    GameObject g = Instantiate(((ScriptableBuilding)Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data).buildingList[0].buildingObject, new Vector3(Player.localPlayer.transform.position.x, Player.localPlayer.transform.position.y, 0.0f), Quaternion.identity);
                                    Player.localPlayer.playerBuilding.actualBuilding = g;
                                    if (GeneralManager.singleton.spawnedAttackObject)
                                    {
                                        Destroy(GeneralManager.singleton.spawnedAttackObject);
                                        GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                    }
                                    else
                                    {
                                        GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                    }
                                    return;
                                }
                                instantiate = false;
                            }
                            else
                            {
                                GameObject g = Instantiate(((ScriptableBuilding)Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data).buildingList[0].buildingObject, new Vector3(Player.localPlayer.transform.position.x, Player.localPlayer.transform.position.y, 0.0f), Quaternion.identity);
                                Player.localPlayer.playerBuilding.actualBuilding = g;
                                if (GeneralManager.singleton.spawnedAttackObject)
                                {
                                    Destroy(GeneralManager.singleton.spawnedAttackObject);
                                    GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                }
                                else
                                {
                                    GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                }
                            }
                        }
                        else if (Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data.name == "Gate")
                        {
                            instantiate = false;
                            FindNearestWoodwall();

                            if (woodWall.Count > 0)
                            {
                                for (int y = 0; y < woodWall.Count; y++)
                                {
                                    Transform[] children = woodWall[y].transform.GetComponentsInChildren<Transform>();
                                    foreach (Transform child in children)
                                    {
                                        if (instantiate == true) return;
                                        if (child.gameObject.layer == 18)
                                        {
                                            GameObject g = Instantiate(((ScriptableBuilding)Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data).buildingList[0].buildingObject, new Vector3(child.transform.position.x, child.transform.position.y, 0.0f), Quaternion.identity);
                                            Player.localPlayer.playerBuilding.actualBuilding = g;

                                            instantiate = true;

                                            if (GeneralManager.singleton.spawnedAttackObject)
                                            {
                                                Destroy(GeneralManager.singleton.spawnedAttackObject);
                                                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                            }
                                            else
                                            {
                                                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                            }

                                            foreach (Transform woodWallChildren in woodWall[y].transform)
                                            {
                                                if (woodWallChildren.gameObject.layer == 18)
                                                    GeneralManager.singleton.spawnedBuildingObject.GetComponent<UIBuilding>().childpositioning.Add(new Positioning(woodWallChildren, 0));
                                                //else if (woodWallChildren.gameObject.layer == 16)
                                                //    GeneralManager.singleton.spawnedBuildingObject.GetComponent<UIBuilding>().childpositioning.Add(new Positioning(woodWallChildren, 1));
                                            }

                                            Destroy(this.gameObject);
                                            UIOrderManager.singleton.btnCloseLastPanel.onClick.Invoke();
                                            return;
                                        }
                                    }
                                }
                                if (instantiate == false)
                                {
                                    GameObject g = Instantiate(((ScriptableBuilding)Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data).buildingList[0].buildingObject, new Vector3(Player.localPlayer.transform.position.x, Player.localPlayer.transform.position.y, 0.0f), Quaternion.identity);
                                    Player.localPlayer.playerBuilding.actualBuilding = g;
                                    if (GeneralManager.singleton.spawnedAttackObject)
                                    {
                                        Destroy(GeneralManager.singleton.spawnedAttackObject);
                                        GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                    }
                                    else
                                    {
                                        GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                    }
                                    return;
                                }
                                instantiate = false;
                            }
                            else
                            {
                                GameObject g = Instantiate(((ScriptableBuilding)Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data).buildingList[0].buildingObject, new Vector3(Player.localPlayer.transform.position.x, Player.localPlayer.transform.position.y, 0.0f), Quaternion.identity);
                                Player.localPlayer.playerBuilding.actualBuilding = g;
                                if (GeneralManager.singleton.spawnedAttackObject)
                                {
                                    Destroy(GeneralManager.singleton.spawnedAttackObject);
                                    GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                }
                                else
                                {
                                    GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                }
                            }
                        }
                        else if (Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data.name == "Barbwire")
                        {
                            instantiate = false;
                            FindNearesBarbwire();

                            if (woodWall.Count > 0)
                            {
                                for (int y = 0; y < woodWall.Count; y++)
                                {
                                    if (instantiate == true) return;
                                    Transform[] children = woodWall[y].transform.GetComponentsInChildren<Transform>();
                                    foreach (Transform child in children)
                                    {
                                        if (child.gameObject.layer == 28)
                                        {
                                            GameObject g = Instantiate(((ScriptableBuilding)Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data).buildingList[0].buildingObject, new Vector3(child.transform.position.x, child.transform.position.y, 0.0f), Quaternion.identity);
                                            Player.localPlayer.playerBuilding.actualBuilding = g;

                                            instantiate = true;
                                            if (GeneralManager.singleton.spawnedAttackObject)
                                            {
                                                Destroy(GeneralManager.singleton.spawnedAttackObject);
                                                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                            }
                                            else
                                            {
                                                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                            }

                                            foreach (Transform woodWallChildren in woodWall[y].transform)
                                            {
                                                if (woodWallChildren.gameObject.layer == 28)
                                                    GeneralManager.singleton.spawnedBuildingObject.GetComponent<UIBuilding>().childpositioning.Add(new Positioning(woodWallChildren, 0));
                                                else if (woodWallChildren.gameObject.layer == 29)
                                                    GeneralManager.singleton.spawnedBuildingObject.GetComponent<UIBuilding>().childpositioning.Add(new Positioning(woodWallChildren, 1));
                                            }

                                            Destroy(this.gameObject);
                                            UIOrderManager.singleton.btnCloseLastPanel.onClick.Invoke();
                                            return;
                                        }
                                    }
                                }
                                if (instantiate == false)
                                {
                                    GameObject g = Instantiate(((ScriptableBuilding)Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data).buildingList[0].buildingObject, new Vector3(Player.localPlayer.transform.position.x, Player.localPlayer.transform.position.y, 0.0f), Quaternion.identity);
                                    Player.localPlayer.playerBuilding.actualBuilding = g;
                                    if (GeneralManager.singleton.spawnedAttackObject)
                                    {
                                        Destroy(GeneralManager.singleton.spawnedAttackObject);
                                        GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                    }
                                    else
                                    {
                                        GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                    }
                                    return;
                                }
                                instantiate = false;
                            }
                            else
                            {
                                GameObject g = Instantiate(((ScriptableBuilding)Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data).buildingList[0].buildingObject, new Vector3(Player.localPlayer.transform.position.x, Player.localPlayer.transform.position.y, 0.0f), Quaternion.identity);
                                Player.localPlayer.playerBuilding.actualBuilding = g;
                                if (GeneralManager.singleton.spawnedAttackObject)
                                {
                                    Destroy(GeneralManager.singleton.spawnedAttackObject);
                                    GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                }
                                else
                                {
                                    GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                }
                            }
                        }
                        else if (Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data.name == "Flag")
                        {
                            Instantiate(GeneralManager.singleton.flagManager, GeneralManager.singleton.canvas);
                        }
                        else if (((ScriptableBuilding)Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data).isBasement)
                        {
                            if (FindNearestFloor())
                            {
                                GameObject g = Instantiate(((ScriptableBuilding)Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data).buildingList[0].buildingObject, new Vector3(Player.localPlayer.transform.position.x, Player.localPlayer.transform.position.y, 0.0f), Quaternion.identity);
                                Debug.Log("Instantiate object : " + g);
                                Player.localPlayer.playerBuilding.actualBuilding = g;
                                if (GeneralManager.singleton.spawnedAttackObject)
                                {
                                    Destroy(GeneralManager.singleton.spawnedAttackObject);
                                    GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                }
                                else
                                {
                                    GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                }
                            }
                            else
                            {
                                ModularBuildingManager.singleton.building = ((ScriptableBuilding)Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data);
                                Player.localPlayer.playerBuilding.actualBuilding = Instantiate(((ScriptableBuilding)Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data).buildingList[0].buildingObject, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);

                                ModularBuildingManager.singleton.ableModificationMode = true;

                                if (GeneralManager.singleton.spawnedAttackObject)
                                {
                                    Destroy(GeneralManager.singleton.spawnedAttackObject);
                                    GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                }
                                else
                                {
                                    GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                }

                                GeneralManager.singleton.spawnedBuildingObject.GetComponent<UIBuilding>().DisableButton();
                            }
                        }
                        else if (((ScriptableBuilding)Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data).modularForniture)
                        {
                            GameObject g = Instantiate(((ScriptableBuilding)Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data).buildingList[0].buildingObject, new Vector3(Player.localPlayer.transform.position.x, Player.localPlayer.transform.position.y, 0.0f), Quaternion.identity);
                            Player.localPlayer.playerBuilding.actualBuilding = g;
                            if (GeneralManager.singleton.spawnedAttackObject)
                            {
                                Destroy(GeneralManager.singleton.spawnedAttackObject);
                                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                            }
                            else
                            {
                                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                            }

                        }
                        else if (((ScriptableBuilding)Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data).isWall ||
                                ((ScriptableBuilding)Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data).isDoor)
                        {

                            if (FindNearestFloorForWallDoor())
                            {
                                ModularBuildingManager.singleton.building = ((ScriptableBuilding)Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data);
                                ModularBuildingManager.singleton.ableModificationWallMode = true;
                                if (GeneralManager.singleton.spawnedAttackObject)
                                {
                                    Destroy(GeneralManager.singleton.spawnedAttackObject);
                                    GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                }
                                else
                                {
                                    GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                                }

                                GeneralManager.singleton.spawnedBuildingObject.GetComponent<UIBuilding>().DisableButton();
                            }
                            else
                            {
                                Player.localPlayer.playerBuilding.building = null;
                                Player.localPlayer.playerBuilding.inventoryIndex = -1;
                                //UIInventory.singleton.selectedItem = -1;
                                ModularBuildingManager.singleton.ableModificationWallMode = false;
                                ModularBuildingManager.singleton.ableModificationMode = false;
                            }
                        }
                        else if (((ScriptableBuilding)Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data).modularAccessory)
                        {
                            GameObject g = Instantiate(((ScriptableBuilding)Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data).buildingList[0].buildingObject, new Vector3(Player.localPlayer.transform.position.x, Player.localPlayer.transform.position.y, 0.0f), Quaternion.identity);
                            Player.localPlayer.playerBuilding.actualBuilding = g;

                            if (GeneralManager.singleton.spawnedAttackObject)
                            {
                                Destroy(GeneralManager.singleton.spawnedAttackObject);
                                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                            }
                            else
                            {
                                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                            }
                        }
                        else
                        {
                            GameObject g = Instantiate(((ScriptableBuilding)Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data).buildingList[0].buildingObject, new Vector3(Player.localPlayer.transform.position.x, Player.localPlayer.transform.position.y, 0.0f), Quaternion.identity);
                            Player.localPlayer.playerBuilding.actualBuilding = g;
                            if (GeneralManager.singleton.spawnedAttackObject)
                            {
                                Destroy(GeneralManager.singleton.spawnedAttackObject);
                                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                            }
                            else
                            {
                                GeneralManager.singleton.spawnedBuildingObject = Instantiate(GeneralManager.singleton.buildingManager, GeneralManager.singleton.canvas);
                            }
                            Destroy(this.gameObject);
                        }
                    }
                    else
                    {
                        if (Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data is PetItem usablePet &&
                            usablePet.CanUseBelt(Player.localPlayer, Player.localPlayer.playerBuilding.inventoryIndex))
                        {
                            Player.localPlayer.playerBelt.CmdUseBeltItem(Player.localPlayer.playerBuilding.inventoryIndex);
                        }
                        else if (Player.localPlayer.playerBelt.belt[Player.localPlayer.playerBuilding.inventoryIndex].item.data is UsableItem usable &&
                            usable.CanUse(Player.localPlayer, Player.localPlayer.playerBuilding.inventoryIndex))
                        {
                            Player.localPlayer.playerBelt.CmdUseBeltItem(Player.localPlayer.playerBuilding.inventoryIndex);
                        }
                    }
                    Destroy(this.gameObject);
                }
            });

            deleteButton.gameObject.SetActive(false);
        }


    }

    public bool FindNearestFloor()
    {
        GameObject[] floor = GameObject.FindGameObjectsWithTag("FloorBasement");
        wallOrdered = floor.Where(go => go.GetComponent<ModularPiece>() && GeneralManager.singleton.CanDoOtherActionFloor(go.GetComponent<ModularPiece>(), Player.localPlayer)).ToList();

        wallOrdered = wallOrdered.OrderBy(m => Vector2.Distance(Player.localPlayer.transform.position, m.transform.position)).ToList();

        if (wallOrdered.Count > 0)
        {
            if (Vector2.Distance(Player.localPlayer.transform.position, wallOrdered[0].transform.position) > 10)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    public bool FindNearestFloorForWallDoor()
    {
        GameObject[] floor = GameObject.FindGameObjectsWithTag("FloorBasement");
        wallOrdered = floor.Where(go => go.GetComponent<ModularPiece>() && GeneralManager.singleton.CanDoOtherActionFloor(go.GetComponent<ModularPiece>(), Player.localPlayer)).ToList();

        wallOrdered = wallOrdered.OrderBy(m => Vector2.Distance(Player.localPlayer.transform.position, m.transform.position)).ToList();

        if (wallOrdered.Count > 0)
        {
            if (Vector2.Distance(Player.localPlayer.transform.position, wallOrdered[0].transform.position) > 10)
            {
                Debug.Log("Not find object by distance");
                return false;
            }
            else
            {
                Debug.Log("Find  Object");
                return true;
            }
        }
        else
        {
            Debug.Log("Not find  Object");
            return false;
        }
    }

    public GameObject FindNearestFloorObject()
    {
        GameObject[] floor = GameObject.FindGameObjectsWithTag("FloorBasement");
        wallOrdered = floor.Where(go => go.GetComponent<ModularPiece>() && GeneralManager.singleton.CanDoOtherActionFloor(go.GetComponent<ModularPiece>(), Player.localPlayer)).ToList();

        wallOrdered = wallOrdered.OrderBy(m => Vector2.Distance(Player.localPlayer.transform.position, m.transform.position)).ToList();

        return wallOrdered[0];
    }

    public int FindNearestFloorPointAvailable(ModularPiece floorObject)
    {
        float distance = 100000.0f;
        int flooriIndex = -1;
        for (int i = 0; i < floorObject.modularFloorPoint.Count; i++)
        {
            int index = i;
            if (Vector2.Distance(Player.localPlayer.transform.position, floorObject.modularFloorPoint[index].transform.position) < distance)
            {
                if (index == 0 && floorObject.occupiedLEFT) continue;
                if (index == 1 && floorObject.occupiedRIGHT) continue;
                if (index == 2 && floorObject.occupiedUP) continue;
                if (index == 3 && floorObject.occupiedDOWN) continue;
                distance = Vector2.Distance(Player.localPlayer.transform.position, floorObject.modularFloorPoint[index].transform.position);
                flooriIndex = index;
            }
        }
        return flooriIndex;
    }

    public void FindNearestWoodwall()
    {
        woodWall.Clear();

        GameObject[] wall = GameObject.FindGameObjectsWithTag("WoodWall");
        GameObject[] gate = GameObject.FindGameObjectsWithTag("Gate");
        GameObject[] allObject = wall.Concat(gate).ToArray();
        List<Entity> monster = new List<Entity>();

        List<Entity> monsters = allObject.Select(go => go.GetComponent<Entity>()).ToList();
        if (monsters.Count > 0)
            woodWall = monsters.OrderBy(m => Vector2.Distance(Player.localPlayer.transform.position, m.transform.position)).ToList();

        List<Entity> woodwallToDelete = new List<Entity>();

        foreach (Entity wood in woodWall)
        {
            if (Vector2.Distance(Player.localPlayer.transform.position, wood.transform.position) > 10.0f)
            {
                woodwallToDelete.Add(wood);
            }
        }

        foreach (Entity wood in woodwallToDelete)
        {
            if (woodWall.Contains(wood))
            {
                woodWall.Remove(wood);
            }
        }

    }

    public void FindNearesBarbwire()
    {
        woodWall.Clear();

        GameObject[] wall = GameObject.FindGameObjectsWithTag("Barbwire");
        GameObject[] gate = GameObject.FindGameObjectsWithTag("Gate");
        GameObject[] allObject = wall.Concat(gate).ToArray();
        List<Entity> monster = new List<Entity>();

        List<Entity> monsters = allObject.Select(go => go.GetComponent<Entity>()).ToList();
        if (monsters.Count > 0)
            woodWall = monsters.OrderBy(m => Vector2.Distance(Player.localPlayer.transform.position, m.transform.position)).ToList();

        List<Entity> woodwallToDelete = new List<Entity>();

        foreach (Entity wood in woodWall)
        {
            if (Vector2.Distance(Player.localPlayer.transform.position, wood.transform.position) > 5.0f)
            {
                woodwallToDelete.Add(wood);
            }
        }

        foreach (Entity wood in woodwallToDelete)
        {
            if (woodWall.Contains(wood))
            {
                woodWall.Remove(wood);
            }
        }

    }
}
