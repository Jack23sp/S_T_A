using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpriteShadersURP
{
    public class DemoShaderPreview : MonoBehaviour
    {
        [Header("Information:")]
        [TextArea()] public string description;

        [Header("Slider:")]
        public bool hasSlider = true;
        public string valueName = "_Intensity";
        public float minValue = 0;
        public float maxValue = 1f;
        [Range(0,1)] public float currentValue = 1f;
        public float sliderSpeed = 1f;

        public void SelectThis()
        {
            int previousIndex = transform.GetSiblingIndex() - 1;
            if (previousIndex < 0) previousIndex = transform.parent.childCount - 1;
            int nextIndex = transform.GetSiblingIndex() + 1;
            if (nextIndex > transform.parent.childCount - 1) nextIndex = 0;
            transform.parent.GetChild(nextIndex).gameObject.SetActive(false);
            transform.parent.GetChild(previousIndex).gameObject.SetActive(false);
            gameObject.SetActive(true);
            DemoManager.sliderSpeed = sliderSpeed;

            Transform shader = transform;

            if(shader != null && hasSlider)
            {
                DemoManager.c.go = shader.gameObject;
                DemoManager.c.minValue = minValue;
                DemoManager.c.maxValue = maxValue;
                DemoManager.c.valueName = valueName;

                DemoManager.c.SetValue(currentValue);
                DemoManager.c.UpdateSlider();
            }
            else
            {
                DemoManager.c.go = null;
            }

            Transform lit = transform.Find("Lit");
            if(lit != null)
            {
                GameObject unlit = transform.Find("Unlit").gameObject;

                if (DemoManager.c.isLit)
                {
                    lit.gameObject.SetActive(true);
                    unlit.gameObject.SetActive(false);
                }
                else
                {
                    lit.gameObject.SetActive(false);
                    unlit.gameObject.SetActive(true);
                }

                DemoManager.c.transform.Find("Background/LitToggle").gameObject.SetActive(true);
            }
            else
            {
                DemoManager.c.NotLit();
            }

            DemoManager.c.SetInformation(gameObject.name, description);
        }

        [ExecuteInEditMode]
        private void OnValidate()
        {
            Transform parent = GameObject.Find("Canvas/Background").transform;
            parent.Find("Title").GetComponent<Text>().text = gameObject.name;
            parent.Find("Description").GetComponent<Text>().text = description;
        }
    }
}