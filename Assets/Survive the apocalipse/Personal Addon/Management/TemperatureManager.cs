using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Tilemaps;

public class TemperatureManager : NetworkBehaviour
{

    public static TemperatureManager singleton;

    [SyncVar]
    public string season = "Winter";
    public string nextSeason;
    public bool isComing;
    [SyncVar]
    public float actualSafeCover;

    [SyncVar]
    public string time;
    [SyncVar]
    public string colorSync;
    [SyncVar]
    public string ambientColorSync;


    [SyncVar]
    public bool isRainy;
    [SyncVar]
    public bool isWindy;
    [SyncVar]
    public bool isSunny;
    [SyncVar]
    public bool isSnowy;

    [Space(5)]

    public int seconds = 00;
    public int minutes = 00;
    public int hours = 00;
    public int days = 0;

    [Space(5)]

    public int WinterDayDuration;
    public int SpringDayDuration;
    public int SummerDuration;
    public int AutumnDayDuration;

    [Space(5)]

    public int winterSunset;
    public int winterLightestPoint;
    public int winterSunrise;

    [Space(5)]

    public int springSunset;
    public int springLightestPoint;
    public int springSunrise;

    [Space(5)]

    public int summerSunset;
    public int summerLightestPoint;
    public int summerSunrise;

    [Space(5)]

    public int autumunSunset;
    public int autumunLightestPoint;
    public int autumunSunrise;

    [Space(5)]

    public float requiredCoverWinter = 1.0f;
    public float requiredCoverSpring = 0.5f;
    public float requiredCoverSummer = 0.2f;
    public float requiredCoverAutumn = 0.6f;

    [Space(5)]
    public string winter = "Winter";
    public string spring = "Spring";
    public string summer = "Summer";
    public string autumn = "Autumn";

    [Space(5)]

    public float winterProbabilityOfRainStart = 1.0f;
    public float winterProbabilityOfRainEnd = 1.0f;
    public float winterProbabilityOfSunStart = 0.5f;
    public float winterProbabilityOfSunEnd = 0.5f;
    public float winterProbabilityOfSnowStart = 0.2f;
    public float winterProbabilityOfSnowEnd = 0.5f;
    public float winterProbabilityWindy = 0.1f;

    [Space(5)]

    public float springProbabilityOfRainStart = 1.0f;
    public float springProbabilityOfRainEnd = 1.0f;
    public float springProbabilityOfSunStart = 0.5f;
    public float springProbabilityOfSunEnd = 0.5f;
    public float springProbabilityOfSnowStart = 0.2f;
    public float springProbabilityOfSnowEnd = 0.5f;
    public float springProbabilityWindy = 0.1f;

    [Space(5)]

    public float summerProbabilityOfRainStart = 1.0f;
    public float summerProbabilityOfRainEnd = 1.0f;
    public float summerProbabilityOfSunStart = 0.5f;
    public float summerProbabilityOfSunEnd = 0.5f;
    public float summerProbabilityOfSnowStart = 0.2f;
    public float summerProbabilityOfSnowEnd = 0.5f;
    public float summerProbabilityWindy = 0.1f;

    [Space(5)]

    public float autumnProbabilityOfRainStart = 1.0f;
    public float autumnProbabilityOfRainEnd = 1.0f;
    public float autumnProbabilityOfSunStart = 0.5f;
    public float autumnProbabilityOfSunEnd = 0.5f;
    public float autumnProbabilityOfSnowStart = 0.2f;
    public float autumnProbabilityOfSnowEnd = 0.5f;
    public float autumnProbabilityWindy = 0.1f;

    private string secondTime;
    private string minuteTime;
    private string hourTime;

    public Color sunsetSunriseColor;

    public Color halfDayColor;

    public Color MidnightColor;

    public Color desiredColor;

    public float smooth = 100;

    private bool prevRainy;
    private bool prevWindy;
    private bool prevSunny;
    private bool prevSnowy;

    public List<double> changeTime = new List<double>();

    public GameObject snowObject;
    public GameObject rainObject;

    public Color springColor;
    public Color winterColor;
    public Color autumnColor;

    public Color desiredDecorationColor;

    private Color color;

    public SpriteRenderer groundSprite;
    //public Tilemap grassTilemap;
    //public Tilemap bushTilemap;

    public string weather;

    public float timeBetweenWeatherChange = 360.0f;
    [SyncVar]
    public bool nightMusic;

    public List<Aquifer> actualAcquifer = new List<Aquifer>();

    void Start()
    {
        if (!singleton) singleton = this;
        if (isServer || isServerOnly)
        {
            InvokeRepeating(nameof(TimeManager), 0.01f, 0.01f);
            InvokeRepeating(nameof(ChangeWeatherConditions), timeBetweenWeatherChange, timeBetweenWeatherChange);
            InvokeRepeating(nameof(ChangeWindConditions), 0.0f, 30.0f);
            InvokeRepeating(nameof(ChargeAquifer), 0.0f, 30.0f);
            prevRainy = isRainy;
            prevWindy = isWindy;
            prevSunny = isSunny;
            prevSnowy = isSnowy;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (isClient)
        {
            rainObject.SetActive(isRainy);
            snowObject.SetActive(isSnowy);
        }

        if (isRainy) weather = "Rainy";
        if (isSnowy) weather = "Snowy";
        if (isSnowy) weather = "Sunny";

        if (minutes < 10)
            minuteTime = "0" + minutes.ToString();
        else
            minuteTime = minutes.ToString();

        if (hours < 10)
            hourTime = "0" + hours.ToString();
        else
            hourTime = hours.ToString();

        if (isServer || isServerOnly)
        {
            if (isComing)
            {
                time = hourTime + " : " + minuteTime + "  " + nextSeason + " is coming...";
            }
            else
            {
                time = hourTime + " : " + minuteTime + "  " + season;
            }
        }

        if (isServer || isServerOnly)
        {
            if (season == winter)
            {
                // Check if is after winter sunrise and before midnight
                if (hours >= winterSunrise && hours <= 23 && minutes <= 59 && seconds <= 59)
                {
                    if (MidnightColor.r > desiredColor.r)
                    {
                        desiredColor.r += Time.deltaTime / smooth;
                    }
                    else if (MidnightColor.r < desiredColor.r)
                    {
                        desiredColor.r -= Time.deltaTime / smooth;
                    }

                    if (MidnightColor.g > desiredColor.g)
                    {
                        desiredColor.g += Time.deltaTime / smooth;
                    }
                    else if (MidnightColor.g < desiredColor.g)
                    {
                        desiredColor.g -= Time.deltaTime / smooth;
                    }

                    if (MidnightColor.b > desiredColor.b)
                    {
                        desiredColor.b += Time.deltaTime / smooth;
                    }
                    else if (MidnightColor.b < desiredColor.b)
                    {
                        desiredColor.b -= Time.deltaTime / smooth;
                    }
                    nightMusic = true;
                }
                // Check if is after midnight and before sunset
                if (hours >= 0 && hours <= winterSunset)
                {
                    if (sunsetSunriseColor.r > desiredColor.r)
                    {
                        desiredColor.r += Time.deltaTime / smooth;
                    }
                    else if (sunsetSunriseColor.r < desiredColor.r)
                    {
                        desiredColor.r -= Time.deltaTime / smooth;
                    }

                    if (sunsetSunriseColor.g > desiredColor.g)
                    {
                        desiredColor.g += Time.deltaTime / smooth;
                    }
                    else if (sunsetSunriseColor.g < desiredColor.g)
                    {
                        desiredColor.g -= Time.deltaTime / smooth;
                    }

                    if (sunsetSunriseColor.b > desiredColor.b)
                    {
                        desiredColor.b += Time.deltaTime / smooth;
                    }
                    else if (sunsetSunriseColor.b < desiredColor.b)
                    {
                        desiredColor.b -= Time.deltaTime / smooth;
                    }
                    nightMusic = true;
                }
                // Check if is after sunset and midday
                if (hours > winterSunset && hours <= 12)
                {
                    if (halfDayColor.r > desiredColor.r)
                    {
                        desiredColor.r += Time.deltaTime / smooth;
                    }
                    else if (halfDayColor.r < desiredColor.r)
                    {
                        desiredColor.r -= Time.deltaTime / smooth;
                    }

                    if (halfDayColor.g > desiredColor.g)
                    {
                        desiredColor.g += Time.deltaTime / smooth;
                    }
                    else if (halfDayColor.g < desiredColor.g)
                    {
                        desiredColor.g -= Time.deltaTime / smooth;
                    }

                    if (halfDayColor.b > desiredColor.b)
                    {
                        desiredColor.b += Time.deltaTime / smooth;
                    }
                    else if (halfDayColor.b < desiredColor.b)
                    {
                        desiredColor.b -= Time.deltaTime / smooth;
                    }
                    nightMusic = false;
                }
                // Check if is after 12 and before sunrise
                if (hours >= 12 && hours < winterSunrise)
                {
                    if (sunsetSunriseColor.r > desiredColor.r)
                    {
                        desiredColor.r += Time.deltaTime / smooth;
                    }
                    else if (sunsetSunriseColor.r < desiredColor.r)
                    {
                        desiredColor.r -= Time.deltaTime / smooth;
                    }

                    if (sunsetSunriseColor.g > desiredColor.g)
                    {
                        desiredColor.g += Time.deltaTime / smooth;
                    }
                    else if (sunsetSunriseColor.g < desiredColor.g)
                    {
                        desiredColor.g -= Time.deltaTime / smooth;
                    }

                    if (sunsetSunriseColor.b > desiredColor.b)
                    {
                        desiredColor.b += Time.deltaTime / smooth;
                    }
                    else if (sunsetSunriseColor.b < desiredColor.b)
                    {
                        desiredColor.b -= Time.deltaTime / smooth;
                    }
                    nightMusic = false;
                }
            }

            if (season == spring)
            {
                // Check if is after winter sunrise and before midnight
                if (hours >= springSunrise && hours <= 23 && minutes <= 59 && seconds <= 59)
                {
                    //darkHourToMidnight = 24 - hours;
                    //darkHourToSunrise = 0;

                    if (MidnightColor.r > desiredColor.r)
                    {
                        desiredColor.r += Time.deltaTime / smooth;
                    }
                    else if (MidnightColor.r < desiredColor.r)
                    {
                        desiredColor.r -= Time.deltaTime / smooth;
                    }

                    if (MidnightColor.g > desiredColor.g)
                    {
                        desiredColor.g += Time.deltaTime / smooth;
                    }
                    else if (MidnightColor.g < desiredColor.g)
                    {
                        desiredColor.g -= Time.deltaTime / smooth;
                    }

                    if (MidnightColor.b > desiredColor.b)
                    {
                        desiredColor.b += Time.deltaTime / smooth;
                    }
                    else if (MidnightColor.b < desiredColor.b)
                    {
                        desiredColor.b -= Time.deltaTime / smooth;
                    }
                    nightMusic = true;
                }
                // Check if is after midnight and before sunset
                if (hours >= 0 && hours <= springSunset)
                {
                    //darkHourToSunrise = winterSunset - hours;
                    //darkHourToMidnight = 0;

                    if (sunsetSunriseColor.r > desiredColor.r)
                    {
                        desiredColor.r += Time.deltaTime / smooth;
                    }
                    else if (sunsetSunriseColor.r < desiredColor.r)
                    {
                        desiredColor.r -= Time.deltaTime / smooth;
                    }

                    if (sunsetSunriseColor.g > desiredColor.g)
                    {
                        desiredColor.g += Time.deltaTime / smooth;
                    }
                    else if (sunsetSunriseColor.g < desiredColor.g)
                    {
                        desiredColor.g -= Time.deltaTime / smooth;
                    }

                    if (sunsetSunriseColor.b > desiredColor.b)
                    {
                        desiredColor.b += Time.deltaTime / smooth;
                    }
                    else if (sunsetSunriseColor.b < desiredColor.b)
                    {
                        desiredColor.b -= Time.deltaTime / smooth;
                    }
                    nightMusic = true;
                }
                // Check if is after sunset and midday
                if (hours > springSunset && hours <= 12)
                {
                    if (halfDayColor.r > desiredColor.r)
                    {
                        desiredColor.r += Time.deltaTime / smooth;
                    }
                    else if (halfDayColor.r < desiredColor.r)
                    {
                        desiredColor.r -= Time.deltaTime / smooth;
                    }

                    if (halfDayColor.g > desiredColor.g)
                    {
                        desiredColor.g += Time.deltaTime / smooth;
                    }
                    else if (halfDayColor.g < desiredColor.g)
                    {
                        desiredColor.g -= Time.deltaTime / smooth;
                    }

                    if (halfDayColor.b > desiredColor.b)
                    {
                        desiredColor.b += Time.deltaTime / smooth;
                    }
                    else if (halfDayColor.b < desiredColor.b)
                    {
                        desiredColor.b -= Time.deltaTime / smooth;
                    }
                    nightMusic = false;
                }
                // Check if is after 12 and before sunrise
                if (hours >= 12 && hours < springSunrise)
                {
                    if (sunsetSunriseColor.r > desiredColor.r)
                    {
                        desiredColor.r += Time.deltaTime / smooth;
                    }
                    else if (sunsetSunriseColor.r < desiredColor.r)
                    {
                        desiredColor.r -= Time.deltaTime / smooth;
                    }

                    if (sunsetSunriseColor.g > desiredColor.g)
                    {
                        desiredColor.g += Time.deltaTime / smooth;
                    }
                    else if (sunsetSunriseColor.g < desiredColor.g)
                    {
                        desiredColor.g -= Time.deltaTime / smooth;
                    }

                    if (sunsetSunriseColor.b > desiredColor.b)
                    {
                        desiredColor.b += Time.deltaTime / smooth;
                    }
                    else if (sunsetSunriseColor.b < desiredColor.b)
                    {
                        desiredColor.b -= Time.deltaTime / smooth;
                    }
                    nightMusic = false;
                }
            }

            if (season == summer)
            {
                // Check if is after winter sunrise and before midnight
                if (hours >= summerSunrise && hours <= 23 && minutes <= 59 && seconds <= 59)
                {
                    //darkHourToMidnight = 24 - hours;
                    //darkHourToSunrise = 0;

                    if (MidnightColor.r > desiredColor.r)
                    {
                        desiredColor.r += Time.deltaTime / smooth;
                    }
                    else if (MidnightColor.r < desiredColor.r)
                    {
                        desiredColor.r -= Time.deltaTime / smooth;
                    }

                    if (MidnightColor.g > desiredColor.g)
                    {
                        desiredColor.g += Time.deltaTime / smooth;
                    }
                    else if (MidnightColor.g < desiredColor.g)
                    {
                        desiredColor.g -= Time.deltaTime / smooth;
                    }

                    if (MidnightColor.b > desiredColor.b)
                    {
                        desiredColor.b += Time.deltaTime / smooth;
                    }
                    else if (MidnightColor.b < desiredColor.b)
                    {
                        desiredColor.b -= Time.deltaTime / smooth;
                    }
                    nightMusic = true;
                }
                // Check if is after midnight and before sunset
                if (hours >= 0 && hours <= summerSunset)
                {
                    //darkHourToSunrise = winterSunset - hours;
                    //darkHourToMidnight = 0;

                    if (sunsetSunriseColor.r > desiredColor.r)
                    {
                        desiredColor.r += Time.deltaTime / smooth;
                    }
                    else if (sunsetSunriseColor.r < desiredColor.r)
                    {
                        desiredColor.r -= Time.deltaTime / smooth;
                    }

                    if (sunsetSunriseColor.g > desiredColor.g)
                    {
                        desiredColor.g += Time.deltaTime / smooth;
                    }
                    else if (sunsetSunriseColor.g < desiredColor.g)
                    {
                        desiredColor.g -= Time.deltaTime / smooth;
                    }

                    if (sunsetSunriseColor.b > desiredColor.b)
                    {
                        desiredColor.b += Time.deltaTime / smooth;
                    }
                    else if (sunsetSunriseColor.b < desiredColor.b)
                    {
                        desiredColor.b -= Time.deltaTime / smooth;
                    }
                    nightMusic = true;
                }
                // Check if is after sunset and midday
                if (hours > summerSunset && hours <= 12)
                {
                    if (halfDayColor.r > desiredColor.r)
                    {
                        desiredColor.r += Time.deltaTime / smooth;
                    }
                    else if (halfDayColor.r < desiredColor.r)
                    {
                        desiredColor.r -= Time.deltaTime / smooth;
                    }

                    if (halfDayColor.g > desiredColor.g)
                    {
                        desiredColor.g += Time.deltaTime / smooth;
                    }
                    else if (halfDayColor.g < desiredColor.g)
                    {
                        desiredColor.g -= Time.deltaTime / smooth;
                    }

                    if (halfDayColor.b > desiredColor.b)
                    {
                        desiredColor.b += Time.deltaTime / smooth;
                    }
                    else if (halfDayColor.b < desiredColor.b)
                    {
                        desiredColor.b -= Time.deltaTime / smooth;
                    }
                    nightMusic = false;
                }
                // Check if is after 12 and before sunrise
                if (hours >= 12 && hours < summerSunrise)
                {
                    if (sunsetSunriseColor.r > desiredColor.r)
                    {
                        desiredColor.r += Time.deltaTime / smooth;
                    }
                    else if (sunsetSunriseColor.r < desiredColor.r)
                    {
                        desiredColor.r -= Time.deltaTime / smooth;
                    }

                    if (sunsetSunriseColor.g > desiredColor.g)
                    {
                        desiredColor.g += Time.deltaTime / smooth;
                    }
                    else if (sunsetSunriseColor.g < desiredColor.g)
                    {
                        desiredColor.g -= Time.deltaTime / smooth;
                    }

                    if (sunsetSunriseColor.b > desiredColor.b)
                    {
                        desiredColor.b += Time.deltaTime / smooth;
                    }
                    else if (sunsetSunriseColor.b < desiredColor.b)
                    {
                        desiredColor.b -= Time.deltaTime / smooth;
                    }
                    nightMusic = false;
                }
            }

            if (season == autumn)
            {
                // Check if is after winter sunrise and before midnight
                if (hours >= autumunSunrise && hours <= 23 && minutes <= 59 && seconds <= 59)
                {
                    //darkHourToMidnight = 24 - hours;
                    //darkHourToSunrise = 0;

                    if (MidnightColor.r > desiredColor.r)
                    {
                        desiredColor.r += Time.deltaTime / smooth;
                    }
                    else if (MidnightColor.r < desiredColor.r)
                    {
                        desiredColor.r -= Time.deltaTime / smooth;
                    }

                    if (MidnightColor.g > desiredColor.g)
                    {
                        desiredColor.g += Time.deltaTime / smooth;
                    }
                    else if (MidnightColor.g < desiredColor.g)
                    {
                        desiredColor.g -= Time.deltaTime / smooth;
                    }

                    if (MidnightColor.b > desiredColor.b)
                    {
                        desiredColor.b += Time.deltaTime / smooth;
                    }
                    else if (MidnightColor.b < desiredColor.b)
                    {
                        desiredColor.b -= Time.deltaTime / smooth;
                    }
                    nightMusic = true;
                }
                // Check if is after midnight and before sunset
                if (hours >= 0 && hours <= autumunSunset)
                {
                    //darkHourToSunrise = winterSunset - hours;
                    //darkHourToMidnight = 0;

                    if (sunsetSunriseColor.r > desiredColor.r)
                    {
                        desiredColor.r += Time.deltaTime / smooth;
                    }
                    else if (sunsetSunriseColor.r < desiredColor.r)
                    {
                        desiredColor.r -= Time.deltaTime / smooth;
                    }

                    if (sunsetSunriseColor.g > desiredColor.g)
                    {
                        desiredColor.g += Time.deltaTime / smooth;
                    }
                    else if (sunsetSunriseColor.g < desiredColor.g)
                    {
                        desiredColor.g -= Time.deltaTime / smooth;
                    }

                    if (sunsetSunriseColor.b > desiredColor.b)
                    {
                        desiredColor.b += Time.deltaTime / smooth;
                    }
                    else if (sunsetSunriseColor.b < desiredColor.b)
                    {
                        desiredColor.b -= Time.deltaTime / smooth;
                    }
                    nightMusic = true;
                }
                // Check if is after sunset and midday
                if (hours > autumunSunset && hours <= 12)
                {
                    if (halfDayColor.r > desiredColor.r)
                    {
                        desiredColor.r += Time.deltaTime / smooth;
                    }
                    else if (halfDayColor.r < desiredColor.r)
                    {
                        desiredColor.r -= Time.deltaTime / smooth;
                    }

                    if (halfDayColor.g > desiredColor.g)
                    {
                        desiredColor.g += Time.deltaTime / smooth;
                    }
                    else if (halfDayColor.g < desiredColor.g)
                    {
                        desiredColor.g -= Time.deltaTime / smooth;
                    }

                    if (halfDayColor.b > desiredColor.b)
                    {
                        desiredColor.b += Time.deltaTime / smooth;
                    }
                    else if (halfDayColor.b < desiredColor.b)
                    {
                        desiredColor.b -= Time.deltaTime / smooth;
                    }
                    nightMusic = false;
                }
                // Check if is after 12 and before sunrise
                if (hours >= 12 && hours < autumunSunrise)
                {
                    if (sunsetSunriseColor.r > desiredColor.r)
                    {
                        desiredColor.r += Time.deltaTime / smooth;
                    }
                    else if (sunsetSunriseColor.r < desiredColor.r)
                    {
                        desiredColor.r -= Time.deltaTime / smooth;
                    }

                    if (sunsetSunriseColor.g > desiredColor.g)
                    {
                        desiredColor.g += Time.deltaTime / smooth;
                    }
                    else if (sunsetSunriseColor.g < desiredColor.g)
                    {
                        desiredColor.g -= Time.deltaTime / smooth;
                    }

                    if (sunsetSunriseColor.b > desiredColor.b)
                    {
                        desiredColor.b += Time.deltaTime / smooth;
                    }
                    else if (sunsetSunriseColor.b < desiredColor.b)
                    {
                        desiredColor.b -= Time.deltaTime / smooth;
                    }
                    nightMusic = false;
                }
            }

            SeasonColor();
            colorSync = ColorUtility.ToHtmlStringRGBA(desiredColor);
            ambientColorSync = ColorUtility.ToHtmlStringRGBA(desiredDecorationColor);
        }

        if (ColorUtility.TryParseHtmlString("#" + TemperatureManager.singleton.ambientColorSync, out color))
        {
            if (season != winter)
            {
                groundSprite.color = color;
            }
            //bushTilemap.color = color;
            //grassTilemap.color = color;
        }


    }

    public void SeasonColor()
    {
        if (season == winter)
        {
            if (isSnowy)
            {
                if (desiredDecorationColor.r < winterColor.r)
                {
                    desiredDecorationColor.r += Time.deltaTime / smooth;
                }
                else if (desiredDecorationColor.r > winterColor.r)
                {
                    desiredDecorationColor.r -= Time.deltaTime / smooth;
                }

                if (desiredDecorationColor.g < winterColor.g)
                {
                    desiredDecorationColor.g += Time.deltaTime / smooth;
                }
                else if (desiredDecorationColor.g > winterColor.g)
                {
                    desiredDecorationColor.g -= Time.deltaTime / smooth;
                }

                if (desiredDecorationColor.b < winterColor.b)
                {
                    desiredDecorationColor.b += Time.deltaTime / smooth;
                }
                else if (desiredDecorationColor.b > winterColor.b)
                {
                    desiredDecorationColor.b -= Time.deltaTime / smooth;
                }
            }
            else
            {
                if (desiredDecorationColor.r < springColor.r)
                {
                    desiredDecorationColor.r += Time.deltaTime / smooth;
                }
                else if (desiredDecorationColor.r >= springColor.r)
                {
                    desiredDecorationColor.r -= Time.deltaTime / smooth;
                }

                if (desiredDecorationColor.g < springColor.g)
                {
                    desiredDecorationColor.g += Time.deltaTime / smooth;
                }
                else if (desiredDecorationColor.g >= springColor.g)
                {
                    desiredDecorationColor.g -= Time.deltaTime / smooth;
                }

                if (desiredDecorationColor.b < springColor.b)
                {
                    desiredDecorationColor.b += Time.deltaTime / smooth;
                }
                else if (desiredDecorationColor.b >= springColor.b)
                {
                    desiredDecorationColor.b -= Time.deltaTime / smooth;
                }
            }
        }
        else if (season == autumn)
        {
            if (desiredDecorationColor.r < autumnColor.r)
            {
                desiredDecorationColor.r += Time.deltaTime / smooth;
            }
            else if (desiredDecorationColor.r > autumnColor.r)
            {
                desiredDecorationColor.r -= Time.deltaTime / smooth;
            }

            if (desiredDecorationColor.g < autumnColor.g)
            {
                desiredDecorationColor.g += Time.deltaTime / smooth;
            }
            else if (desiredDecorationColor.g > autumnColor.g)
            {
                desiredDecorationColor.g -= Time.deltaTime / smooth;
            }

            if (desiredDecorationColor.b < autumnColor.b)
            {
                desiredDecorationColor.b += Time.deltaTime / smooth;
            }
            else if (desiredDecorationColor.b > autumnColor.b)
            {
                desiredDecorationColor.b -= Time.deltaTime / smooth;
            }
        }
        else
        {
            if (desiredDecorationColor.r < springColor.r)
            {
                desiredDecorationColor.r += Time.deltaTime / smooth;
            }
            else if (desiredDecorationColor.r > springColor.r)
            {
                desiredDecorationColor.r -= Time.deltaTime / smooth;
            }

            if (desiredDecorationColor.g < springColor.g)
            {
                desiredDecorationColor.g += Time.deltaTime / smooth;
            }
            else if (desiredDecorationColor.g > springColor.g)
            {
                desiredDecorationColor.g -= Time.deltaTime / smooth;
            }

            if (desiredDecorationColor.b < springColor.b)
            {
                desiredDecorationColor.b += Time.deltaTime / smooth;
            }
            else if (desiredDecorationColor.b > springColor.b)
            {
                desiredDecorationColor.b -= Time.deltaTime / smooth;
            }
        }
    }

    public void TimeManager()
    {
        if (!isServer && !isServerOnly)
        {
            return;
        }
        if (seconds < 60)
        {
            seconds++;
        }
        else
        {
            seconds = 00;
            if (minutes < 59)
            {
                minutes++;
            }
            else
            {
                if (hours < 23)
                {
                    hours++;
                }
                else
                {
                    if (days >= (WinterDayDuration + SpringDayDuration + SummerDuration + AutumnDayDuration))
                    {
                        days = 0;
                    }
                    else
                    {
                        days++;
                    }
                    seconds = 0;
                    minutes = 0;
                    hours = 0;
                }
                minutes = 00;
            }
        }

        if (days <= WinterDayDuration)
        {
            season = winter;
            actualSafeCover = requiredCoverWinter;
            nextSeason = spring;

            if (days == WinterDayDuration)
            {
                isComing = true;
            }
            else
            {
                isComing = false;
            }

            return;
        }
        if (days <= (WinterDayDuration + SpringDayDuration))
        {
            season = spring;
            actualSafeCover = requiredCoverSpring;
            nextSeason = summer;

            if (days == (WinterDayDuration + SpringDayDuration))
            {
                isComing = true;
            }
            else
            {
                isComing = false;
            }

            return;
        }
        if (days <= (WinterDayDuration + SpringDayDuration + SummerDuration))
        {
            season = summer;
            actualSafeCover = requiredCoverSummer;
            nextSeason = autumn;

            if (days == (WinterDayDuration + SpringDayDuration + SummerDuration))
            {
                isComing = true;
            }
            else
            {
                isComing = false;
            }

            return;
        }
        if (days <= (WinterDayDuration + SpringDayDuration + SummerDuration + AutumnDayDuration))
        {
            season = autumn;
            actualSafeCover = requiredCoverAutumn;
            nextSeason = winter;

            if (days == (WinterDayDuration + SpringDayDuration + SummerDuration + AutumnDayDuration))
            {
                isComing = true;
            }
            else
            {
                isComing = false;
            }

            return;
        }


    }

    public void ChangeWeatherConditions()
    {
        if (!isServer || !isServerOnly)
        {
            return;
        }
        float nextweather = Random.Range(0.0f, 1.0f);

        if (season == winter)
        {
            if (nextweather >= winterProbabilityOfRainStart && nextweather <= winterProbabilityOfRainEnd)
            {
                isRainy = true;
                isSunny = false;
                isSnowy = false;
            }
            if (nextweather >= winterProbabilityOfSunStart && nextweather <= winterProbabilityOfSunEnd)
            {
                isRainy = false;
                isSunny = true;
                isSnowy = false;
            }
            if (nextweather >= winterProbabilityOfSnowStart && nextweather <= winterProbabilityOfSnowEnd)
            {
                isRainy = false;
                isSunny = false;
                isSnowy = true;
            }
            changeTime.Add(NetworkTime.time);
        }

        if (season == spring)
        {
            if (nextweather >= springProbabilityOfRainStart && nextweather <= springProbabilityOfRainEnd)
            {
                isRainy = true;
                isSunny = false;
                isSnowy = false;
            }
            if (nextweather >= springProbabilityOfSunStart && nextweather <= springProbabilityOfSunEnd)
            {
                isRainy = false;
                isSunny = true;
                isSnowy = false;
            }
            if (nextweather >= springProbabilityOfSnowStart && nextweather <= springProbabilityOfSnowEnd)
            {
                isRainy = false;
                isSunny = false;
                isSnowy = true;
            }
            changeTime.Add(NetworkTime.time);
        }

        if (season == summer)
        {
            if (nextweather >= summerProbabilityOfRainStart && nextweather <= summerProbabilityOfRainEnd)
            {
                isRainy = true;
                isSunny = false;
                isSnowy = false;
            }
            if (nextweather >= summerProbabilityOfSunStart && nextweather <= summerProbabilityOfSunEnd)
            {
                isRainy = false;
                isSunny = true;
                isSnowy = false;
            }
            if (nextweather >= summerProbabilityOfSnowStart && nextweather <= summerProbabilityOfSnowEnd)
            {
                isRainy = false;
                isSunny = false;
                isSnowy = true;
            }
            changeTime.Add(NetworkTime.time);
        }

        if (season == autumn)
        {
            if (nextweather >= autumnProbabilityOfRainStart && nextweather <= autumnProbabilityOfRainEnd)
            {
                isRainy = true;
                isSunny = false;
                isSnowy = false;
            }
            if (nextweather >= autumnProbabilityOfSunStart && nextweather <= autumnProbabilityOfSunEnd)
            {
                isRainy = false;
                isSunny = true;
                isSnowy = false;
            }
            if (nextweather >= autumnProbabilityOfSnowStart && nextweather <= autumnProbabilityOfSnowEnd)
            {
                isRainy = false;
                isSunny = false;
                isSnowy = true;
            }
            changeTime.Add(NetworkTime.time);
        }
    }

    public void ChangeWindConditions()
    {
        if (!isServer || !isServerOnly)
        {
            return;
        }
        float nextweather = Random.Range(0.0f, 1.0f);

        if (season == winter)
        {
            if (nextweather <= winterProbabilityWindy)
            {
                isWindy = true;
            }
            else
                isWindy = false;
        }

        if (season == spring)
        {
            if (nextweather <= springProbabilityWindy)
            {
                isWindy = true;
            }
            else
                isWindy = false;
        }

        if (season == summer)
        {
            if (nextweather <= summerProbabilityWindy)
            {
                isWindy = true;
            }
            else
                isWindy = false;
        }

        if (season == autumn)
        {
            if (nextweather <= autumnProbabilityWindy)
            {
                isWindy = true;
            }
            else
                isWindy = false;
        }
    }

    public void ChargeAquifer()
    {
        if(isRainy)
        {
            for(int i = 0; i < actualAcquifer.Count; i++)
            {
                actualAcquifer[i].actualWater += 10;
                if (actualAcquifer[i].actualWater > actualAcquifer[i].maxWater) actualAcquifer[i].actualWater = actualAcquifer[i].maxWater;
            }
        }
    }
}
