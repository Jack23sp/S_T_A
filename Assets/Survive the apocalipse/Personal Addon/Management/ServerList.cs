using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerList : MonoBehaviour
{
    public NetworkManagerMMO manager;

    public Image server1;
    public Text serverText1;
    public Button serverButton1;

    public Image server2;
    public Text serverText2;
    public Button serverButton2;

    public Image server3;
    public Text serverText3;
    public Button serverButton3;

    public Image server4;
    public Text serverText4;
    public Button serverButton4;

    public int server1Ping;
    public int server2Ping;
    public int server3Ping;
    public int server4Ping;

    public List<Outline> outlines = new List<Outline>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < manager.serverList.Count; i++)
        {

            CheckPing(manager.serverList[i].ip, i);
        }

        if (server1Ping <= GeneralManager.singleton.goodServerPick)
        {
            server1.color = Color.green;
        }
        else if (server1Ping > GeneralManager.singleton.goodServerPick && server1Ping <= GeneralManager.singleton.mediumServicePick)
        {
            server1.color = Color.yellow;
        }
        else if (server1Ping > GeneralManager.singleton.mediumServicePick)
        {
            server1.color = Color.red;
        }


        if (server2Ping <= GeneralManager.singleton.goodServerPick)
        {
            server2.color = Color.green;
        }
        else if (server2Ping > GeneralManager.singleton.goodServerPick && server2Ping <= GeneralManager.singleton.mediumServicePick)
        {
            server2.color = Color.yellow;
        }
        else if (server2Ping > GeneralManager.singleton.mediumServicePick)
        {
            server2.color = Color.red;
        }

        if (server3Ping <= GeneralManager.singleton.goodServerPick)
        {
            server3.color = Color.green;
        }
        else if (server3Ping > GeneralManager.singleton.goodServerPick && server3Ping <= GeneralManager.singleton.mediumServicePick)
        {
            server3.color = Color.yellow;
        }
        else if (server3Ping > GeneralManager.singleton.mediumServicePick)
        {
            server3.color = Color.red;
        }

        if (server4Ping <= GeneralManager.singleton.goodServerPick)
        {
            server4.color = Color.green;
        }
        else if (server4Ping > GeneralManager.singleton.goodServerPick && server4Ping <= GeneralManager.singleton.mediumServicePick)
        {
            server4.color = Color.yellow;
        }
        else if (server4Ping > GeneralManager.singleton.mediumServicePick)
        {
            server4.color = Color.red;
        }

        serverButton1.onClick.SetListener(() =>
        {
            manager.networkAddress = manager.serverList[0].ip;
            EvidenceServer(0);
        });

        serverButton2.onClick.SetListener(() =>
        {
            manager.networkAddress = manager.serverList[1].ip;
            EvidenceServer(1);
        });

        serverButton3.onClick.SetListener(() =>
        {
            manager.networkAddress = manager.serverList[2].ip;
            EvidenceServer(2);
        });

        serverButton4.onClick.SetListener(() =>
        {
            manager.networkAddress = manager.serverList[3].ip;
            EvidenceServer(3);
        });

        serverText1.text = server1Ping.ToString() + " ms";
        serverText2.text = server2Ping.ToString() + " ms";
        serverText3.text = server3Ping.ToString() + " ms";
        serverText4.text = server4Ping.ToString() + " ms";

    }

    public void EvidenceServer(int index)
    {
        for( int i = 0; i < outlines.Count; i++)
        {
            if (i == index)
                outlines[i].enabled = true;
            else
                outlines[i].enabled = false;
        }
    }

    public void CheckPing(string ip, int index)
    {
        StartCoroutine(StartPing(ip, index));
    }

    IEnumerator StartPing(string ip, int index)
    {
        WaitForSeconds f = new WaitForSeconds(2.0f);
        Ping p = new Ping(ip);
        while (p.isDone == false)
        {
            yield return f;
        }
        PingFinished(p, index);
    }


    public void PingFinished(Ping p, int index)
    {
        // stuff when the Ping p has finshed....
        if (index == 0)
            server1Ping = p.time;
        else if (index == 1)
            server2Ping = p.time;
        else if (index == 2)
            server3Ping = p.time;
        else if (index == 3)
            server4Ping = p.time;
    }
}
