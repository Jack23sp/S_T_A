using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteShadersURP
{
    public class DemoWind : MonoBehaviour
    {
        public static DemoWind c;

        [Header("Settings:")]
        public float flatterIntensity = 0.25f;
        public float flatterSpeed = 3f;
        public float windIntensity = 1f;

        void Start()
        {
            c = this;
        }

        public static float getWind(float position)
        {
            if (c == null)   //If no GlobalWind Component exists return ZERO
            {
                return 0f;
            }

            //Global Wind Exists:
            return c.getWindPrivate(position);
        }
        private float getWindPrivate(float position)
        {
            float newWind = windIntensity;
            newWind += Mathf.Sin((position * 91f + Time.time * flatterSpeed)) * flatterIntensity;
            return newWind;
        }
    }
}