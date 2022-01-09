using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIOrderManager : MonoBehaviour
{
    public List<GameObject> selectedUI = new List<GameObject>();
    public List<GameObject> singleTimePanel = new List<GameObject>();
    public GameObject startPanel;
    public Button btnCloseLastPanel;

    public static UIOrderManager singleton;
    private UIGroup uiGroup;

    private Player player;

    void Start()
    {
        if (!singleton) singleton = this;
    }

    void Update()
    {
        player = Player.localPlayer;

        if (player && player.health == 0) btnCloseLastPanel.onClick.Invoke();

        btnCloseLastPanel.onClick.SetListener(() =>
        {
            uiGroup = UIGroup.singleton;
            if (uiGroup) uiGroup.selectedGroup = string.Empty;

            if (selectedUI.Count > 0)
            {
                if (singleTimePanel.Count > 0)
                {
                    GameObject g = singleTimePanel[singleTimePanel.Count - 1];
                    singleTimePanel.Remove(g);
                    Destroy(g.gameObject);
                    return;
                }
                else
                {
                    GameObject g = selectedUI[selectedUI.Count - 1];
                    for (int i = 0; i < selectedUI.Count; i++)
                    {
                        if (selectedUI[i].name == g.name)
                        {
                            selectedUI.Remove(selectedUI[i]);
                            break;
                        }
                    }
                    Destroy(g.gameObject);
                    return;
                }
            }
            else
            {
                if (singleTimePanel.Count > 0)
                {
                    GameObject g = singleTimePanel[singleTimePanel.Count - 1];
                    singleTimePanel.Remove(g);
                    Destroy(g.gameObject);
                    return;
                }
                else
                {
                    if (selectedUI.Count > 0)
                    {
                        GameObject g = selectedUI[selectedUI.Count - 1];
                        for (int i = 0; i < selectedUI.Count; i++)
                        {
                            if (selectedUI[i].name == g.name)
                            {
                                selectedUI.Remove(selectedUI[i]);
                                break;
                            }
                        }
                        Destroy(g.gameObject);
                    }
                    InstantiateUIStartPanel();
                    this.gameObject.SetActive(false);
                    return;
                }
            }

        });

    }

    public void InstantiateUIStartPanel()
    {
        if(transform.childCount > 0)
            Destroy(transform.GetChild(0).gameObject);
        GameObject g = Instantiate(startPanel, this.transform);
        this.transform.SetParent(this.transform);
    }

    public void InstantiatePanel(GameObject toInstantiate)
    {
        GameObject select = null;


        for (int i = 0; i < selectedUI.Count; i++)
        {
            select = selectedUI[i];
            selectedUI.Remove(selectedUI[i]);
            Destroy(select.gameObject);
        }
        GameObject g = Instantiate(toInstantiate, this.transform);
        g.transform.SetParent(UIOrderManager.singleton.transform);
        selectedUI.Add(g);
    }


    public GameObject SingleInstantePanel(GameObject toInstantiate)
    {
            GameObject g = Instantiate(toInstantiate, this.transform);
            g.transform.SetParent(UIOrderManager.singleton.transform);
            singleTimePanel.Add(g);
        return g;
    }
}
