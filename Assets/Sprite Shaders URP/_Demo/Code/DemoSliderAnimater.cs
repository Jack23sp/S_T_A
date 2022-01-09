using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpriteShadersURP
{
    public class DemoSliderAnimater : MonoBehaviour
    {
        Slider slider;
        Toggle toggle;

        void Start()
        {
            slider = GetComponentInParent<Slider>();
            toggle = GetComponent<Toggle>();

            toggle.isOn = true;
        }

        void FixedUpdate()
        {
            if (toggle.isOn)
            {
                slider.SetValueWithoutNotify(Mathf.Clamp01(1.4f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * DemoManager.sliderSpeed * 1.2f)) - 0.3f));
                DemoManager.c.UpdateSlider();
            }
        }

        public void ToggleOff()
        {
            toggle.isOn = false;
        }
    }
}