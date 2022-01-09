using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using System;

public class UINotificationManager : MonoBehaviour
{
    public static UINotificationManager singleton;
    public GameObject notificationObject;

    public string trapEnemy;

    public Player player;

    // Start is called before the first frame update
    void Start()
    {
        if (!singleton) singleton = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

    }

    public void SpawnThirstObject()
    {
        GameObject g = Instantiate(notificationObject, GeneralManager.singleton.canvas);
        NotificationManager notificationManager = g.GetComponentInChildren<NotificationManager>();
        notificationManager.icon = GeneralManager.singleton.thirsty;
        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
        {
            notificationManager.title = "Attento sei assetato";
            notificationManager.description = "";
        }
        else
        {
            notificationManager.title = "Be carful your thirsty";
            notificationManager.description = "";
        }
        notificationManager.GetComponent<Animator>().SetBool("OUT", true);
    }

    public void SpawnFriendObject(string friendRequest)
    {
        GameObject g = Instantiate(notificationObject, GeneralManager.singleton.canvas);
        NotificationManager notificationManager = g.GetComponentInChildren<NotificationManager>();
        notificationManager.icon = GeneralManager.singleton.thirsty;
        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
        {
            notificationManager.title = friendRequest;
            notificationManager.description = "Vuole essere tuo amico";
        }
        else
        {
            notificationManager.title = friendRequest;
            notificationManager.description = "want be your friend";
        }
        notificationManager.GetComponent<Animator>().SetBool("OUT", true);
    }

    public void SpawnHungryObject()
    {
        GameObject g = Instantiate(notificationObject, GeneralManager.singleton.canvas);
        NotificationManager notificationManager = g.GetComponentInChildren<NotificationManager>();
        notificationManager.icon = GeneralManager.singleton.hungry;
        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
        {
            notificationManager.title = "Attento sei affamato";
            notificationManager.description = "";
        }
        else
        {
            notificationManager.title = "Be careful you are hungry";
            notificationManager.description = "";
        }
        notificationManager.GetComponent<Animator>().SetBool("OUT", true);
    }

    public void SpawnCoverObject()
    {
        GameObject g = Instantiate(notificationObject, GeneralManager.singleton.canvas);
        NotificationManager notificationManager = g.GetComponentInChildren<NotificationManager>();
        notificationManager.icon = GeneralManager.singleton.cover;
        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
        {
            notificationManager.title = "Controlla la copertura dei tuoi abiti";
            notificationManager.description = "";
        }
        else
        {
            notificationManager.title = "Check you clothes coverage";
            notificationManager.description = "";
        }
        notificationManager.GetComponent<Animator>().SetBool("OUT", true);
    }

    public void SpawnTrapObject()
    {
        GameObject g = Instantiate(notificationObject, GeneralManager.singleton.canvas);
        NotificationManager notificationManager = g.GetComponentInChildren<NotificationManager>();
        notificationManager.icon = GeneralManager.singleton.trap;
        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
        {
            notificationManager.title = "Nemico individuato";
            notificationManager.description = "";
        }
        else
        {
            notificationManager.title = "Enemy detected";
            notificationManager.description = "";
        }
        notificationManager.GetComponent<Animator>().SetBool("OUT", true);
    }

    public void SpawnWetObject()
    {
        GameObject g = Instantiate(notificationObject, GeneralManager.singleton.canvas);
        NotificationManager notificationManager = g.GetComponentInChildren<NotificationManager>();
        notificationManager.icon = GeneralManager.singleton.radioImg;
        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
        {
            notificationManager.title = "Asciuga i tuoi vestiti";
            notificationManager.description = "";
        }
        else
        {
            notificationManager.title = "Dry your clothes";
            notificationManager.description = "";
        }
        notificationManager.GetComponent<Animator>().SetBool("OUT", true);
    }

    public void SpawnLostableObject()
    {
        GameObject g = Instantiate(notificationObject, GeneralManager.singleton.canvas);
        NotificationManager notificationManager = g.GetComponentInChildren<NotificationManager>();
        notificationManager.icon = GeneralManager.singleton.lostable;
        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
        {
            notificationManager.title = "Hai perso quale oggetto quando sei morto";
            notificationManager.description = "";
        }
        else
        {
            notificationManager.title = "Dropped some items at death";
            notificationManager.description = "";
        }
        notificationManager.GetComponent<Animator>().SetBool("OUT", true);
    }

    public void SpawnTicketObject()
    {
        GameObject g = Instantiate(notificationObject, GeneralManager.singleton.canvas);
        NotificationManager notificationManager = g.GetComponentInChildren<NotificationManager>();
        notificationManager.icon = GeneralManager.singleton.ticketImage;
        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
        {
            notificationManager.title = "Il tuo boost scadrà tra un ora";
            notificationManager.description = "";
        }
        else
        {
            notificationManager.title = "Boost will expire in 1h";
            notificationManager.description = "";
        }
        notificationManager.GetComponent<Animator>().SetBool("OUT", true);
    }

    public void SpawnRadioObject()
    {
        GameObject g = Instantiate(notificationObject, GeneralManager.singleton.canvas);
        NotificationManager notificationManager = g.GetComponentInChildren<NotificationManager>();
        notificationManager.icon = GeneralManager.singleton.radioImg;
        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
        {
            notificationManager.title = "Livello batteria radio basso";
            notificationManager.description = "";
        }
        else
        {
            notificationManager.title = "Your radio battery level is low";
            notificationManager.description = "";
        }
        notificationManager.GetComponent<Animator>().SetBool("OUT", true);
    }

    public void SpawnTorchObject()
    {
        GameObject g = Instantiate(notificationObject, GeneralManager.singleton.canvas);
        NotificationManager notificationManager = g.GetComponentInChildren<NotificationManager>();
        notificationManager.icon = GeneralManager.singleton.torchImg;
        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
        {
            notificationManager.title = "Livello batteria torcia basso";
            notificationManager.description = "";
        }
        else
        {
            notificationManager.title = "Your torch battery level is low";
            notificationManager.description = "";
        }
        notificationManager.GetComponent<Animator>().SetBool("OUT", true);
    }

    public void SpawnPremiumObject()
    {
        TimeSpan difference;
        GameObject g = Instantiate(notificationObject, GeneralManager.singleton.canvas);
        NotificationManager notificationManager = g.GetComponentInChildren<NotificationManager>();
        notificationManager.iconObj.gameObject.SetActive(false);
        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
        {
            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer))
                difference = DateTime.Parse(player.playerBoost.networkBoost[0].hiddenIslandTimer) - DateTime.Now;
            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer) && difference.TotalSeconds > 0)
                notificationManager.title = "Tempo rimasto del premium boost :";
            notificationManager.description = "    " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds));
        }
        else
        {
            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer))
                difference = DateTime.Parse(player.playerBoost.networkBoost[0].hiddenIslandTimer) - DateTime.Now;
            if (!string.IsNullOrEmpty(player.playerBoost.networkBoost[0].hiddenIslandTimer) && difference.TotalSeconds > 0)
                notificationManager.title = "Remaining premium timer :";
            notificationManager.description = "    " + GeneralManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds));
        }
        notificationManager.GetComponent<Animator>().SetBool("OUT", true);
    }

    public void SpawnWetObjectIndication()
    {
        GameObject g = Instantiate(notificationObject, GeneralManager.singleton.canvas);
        NotificationManager notificationManager = g.GetComponentInChildren<NotificationManager>();
        notificationManager.iconObj.gameObject.SetActive(false);
        if (GeneralManager.singleton.languagesManager.defaultLanguages == "Italian")
        {
            notificationManager.title = "I tuoi vestiti sono bagnati, asciugali vicino ad un fuoco";
            notificationManager.description = "";
        }
        else
        {
            notificationManager.title = "Your clothes are wet, dry it near a fire";
            notificationManager.description = "";
        }
        notificationManager.GetComponent<Animator>().SetBool("OUT", true);
    }
}
