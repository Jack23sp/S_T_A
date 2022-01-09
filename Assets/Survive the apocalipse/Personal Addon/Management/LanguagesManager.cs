using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguagesManager : MonoBehaviour
{
    public string defaultLanguages;
    public static LanguagesManager singleton;


    public void GetDefaultLanguage()
    {
        if (!singleton) singleton = this;
        defaultLanguages = Application.systemLanguage.ToString();
        if (defaultLanguages != "Italian") defaultLanguages = "English";
    }

}
