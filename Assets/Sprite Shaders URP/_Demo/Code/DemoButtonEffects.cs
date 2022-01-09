using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SpriteShadersURP
{
    public class DemoButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        bool hovered;
        Image img;

        void Start()
        {
            hovered = false;
            img = GetComponent<Image>();
        }

        void Update()
        {
            if (hovered)
            {
                transform.localScale += (Vector3.one * 1.2f - transform.localScale) * Time.unscaledDeltaTime * 4f;
                img.color = Color.Lerp(img.color, new Color(0.8f, 0.8f, 0.8f, 1f), Time.unscaledDeltaTime * 4f);
            }
            else
            {
                transform.localScale += (Vector3.one * 0.95f - transform.localScale) * Time.unscaledDeltaTime * 4f;
                img.color = Color.Lerp(img.color, new Color(0.55f, 0.55f, 0.55f, 0.75f), Time.unscaledDeltaTime * 4f);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            transform.localScale = Vector3.one * 1.4f;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
        }
    }
}
