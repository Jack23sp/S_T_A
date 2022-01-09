using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public struct TextToTranslate
{
    public string defaultLanguage;
    public string returnString;
}

public class LanguageTranslator : MonoBehaviour
{
    public TextMeshProUGUI textTarget;
    public string defaultLanguage;

    public List<TextToTranslate> textToTranslate = new List<TextToTranslate>();
    private int index = 0;

    public void Awake()
    {
        textTarget = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
            defaultLanguage = GeneralManager.singleton.languagesManager.defaultLanguages;
            Translate(defaultLanguage);
            //if (defaultLanguage != string.Empty) Destroy(this);
    }

    public void Translate(string language)
    {
        index = 0;
        for(int i = 0; i < textToTranslate.Count; i++)
        {
            index = i;
            if(textToTranslate[index].defaultLanguage == language)
            {
                textTarget.text = textToTranslate[index].returnString;
            }
        }
        //return string.Empty;
    }
}
