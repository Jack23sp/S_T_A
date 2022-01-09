using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpriteShadersURP
{
    public class DemoCounter : MonoBehaviour
    {
        Text text;

        float lastEffectTime;

        void Awake()
        {
            text = GetComponent<Text>();
        }

        void FixedUpdate()
        {
            text.material.SetVector("_Vertex_Displacement", new Vector4((1f - Mathf.Min((Time.unscaledTime - lastEffectTime) * 3f, 1)) * 15f, 0, 0, 0));
        }

        public void ChangeText(string newText)
        {
            text.text = newText;
            lastEffectTime = Time.unscaledTime;
        }
    }
}
