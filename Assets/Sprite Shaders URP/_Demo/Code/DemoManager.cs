using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpriteShadersURP
{
    public class DemoManager : MonoBehaviour
    {
        public static DemoManager c; //static reference
        public static float sliderSpeed;

        int currentIndex;

        Text title;
        Text description;
        DemoCounter dc;
        Slider slider;
        CanvasGroup sliderCG;

        [Header("Slider:")]
        public float minValue;
        public float maxValue;
        public string valueName;
        public GameObject go;

        [HideInInspector] public bool isLit = false;

        Transform shadersParent;
        DemoShaderPreview[] previews;

        void Awake()
        {
            c = this;

            isLit = false;
            title = transform.Find("Background/Title").GetComponent<Text>();
            description = transform.Find("Background/Description").GetComponent<Text>();
            dc = transform.Find("Background/Counter").GetComponent<DemoCounter>();
            slider = transform.Find("Background/Slider").GetComponent<Slider>();
            sliderCG = transform.Find("Background/Slider").GetComponent<CanvasGroup>();

            shadersParent = GameObject.Find("Shaders").transform;
            previews = new DemoShaderPreview[shadersParent.childCount];
            for (int n = 0; n < previews.Length; n++)
            {
                previews[n] = shadersParent.GetChild(n).GetComponent<DemoShaderPreview>();
                previews[n].gameObject.SetActive(false);
            }

            currentIndex = 0;
            sliderSpeed = 1f;

        }
        void Start()
        {
            UpdateCycle();
        }

        public void ToggleLit()
        {
            isLit = transform.Find("Background/LitToggle").GetComponent<Toggle>().isOn;
            UpdateCycle();
        }

        public void NotLit()
        {
            transform.Find("Background/LitToggle").gameObject.SetActive(false);
        }

        public void CycleRight()
        {
            currentIndex++;
            if(currentIndex > previews.Length - 1)
            {
                currentIndex = 0;
            }

            UpdateCycle();
        }
        public void CycleLeft()
        {
            currentIndex--;
            if (currentIndex < 0)
            {
                currentIndex = previews.Length - 1;
            }

            UpdateCycle();
        }
        public void UpdateCycle()
        {
            dc.ChangeText((1 + currentIndex) + "/" + previews.Length);
            previews[currentIndex].SelectThis();
        }

        public void SetValue(float value)
        {
            slider.SetValueWithoutNotify(value);
        }

        public void SetInformation(string newTitle, string newDescription)
        {
            title.text = newTitle;
            description.text = newDescription;
        }

        public void UpdateSlider()
        {
            if(go != null)
            {
                sliderCG.alpha = 1;
                slider.interactable = true;

                foreach(SpriteRenderer spr in go.GetComponentsInChildren<SpriteRenderer>())
                {
                    if(spr.name.StartsWith("LightSource") == false)
                    {
                        spr.material.SetFloat(valueName, minValue + (maxValue - minValue) * slider.value);
                    }
                }
            }
            else
            {
                sliderCG.alpha = 0;
                slider.interactable = false;
            }
        }
    }
}