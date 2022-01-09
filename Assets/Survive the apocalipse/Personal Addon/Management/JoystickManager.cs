using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoystickManager : MonoBehaviour
{
    public static JoystickManager singleton;

    public Image backgroundJoystick;

    //public Image leaderboardImage;
    //public Image spawnpointImage;
    //public Image itemmallImage;

    //public Image menuImage;
    //public Image tabImage;

    //public Image sneakImage;
    //public Image runImage;

    //public Image ammoselectorImage;
    //public Image chargeImage;

    //public List<Image> skillbarImage = new List<Image>();
    //public List<Image> equipmentImage = new List<Image>();

    private Player player;

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

        backgroundJoystick.color = Color.black;

    }
}
