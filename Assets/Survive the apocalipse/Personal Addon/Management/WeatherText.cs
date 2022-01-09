using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeatherText : MonoBehaviour
{
    public TextMeshProUGUI weatherText;
    public GameObject objectToManage;

    public Player player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!player) player = Player.localPlayer;
        if (!player) return;

        objectToManage.SetActive(player);

        weatherText.text = "   " + TemperatureManager.singleton.time.ToString();
        
    }
}
