using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteShadersURP
{
    public class DemoFoilage : MonoBehaviour
    {
        [Header("Bending Settings:")]
        [Range(0, 10)] public float maximumRotation = 3f;
        [Range(0, 1)] public float maximumShrink = 0.6f;
        public float bendSpeed = 3f;

        [Header("Wind Intensity:")]
        public float horizontalIntensity = 0.5f;
        public float verticalIntensity = 0.5f;

        [Header("Bending by Entities:")]
        public bool canBeBent = true;
        public float entityRotation = 2f;
        public float entityPressDown = 0.5f;
        public float distanceFactor = 1f;

        Material mat;

        float lastOffset;

        //Rotation
        float wind;
        float currentAngle;
        float currentPushDown;

        //Entities Inside:
        int insideCount;
        List<Transform> entities;

        void Start()
        {
            mat = GetComponent<SpriteRenderer>().material;
            mat.SetFloat("_Seed", Random.value * 1000f);

            currentAngle = 0;

            entities = new List<Transform>();
            insideCount = 0;
        }

        void FixedUpdate()
        {
            wind = DemoWind.getWind(transform.position.x);
            float zRotation = Mathf.Clamp(wind * horizontalIntensity, -maximumRotation, maximumRotation);
            float currentSpeed = bendSpeed;

            while (entities.Count > 0 && entities[0] == null)
            {
                entities.RemoveAt(0);
                insideCount--;
            }

            if (canBeBent && insideCount > 0) //Adding Entity Bending:
            {
                float xOffset = transform.position.x - entities[0].position.x;
                currentSpeed *= 2f;

                if (lastOffset == 0)
                {
                    lastOffset = xOffset;
                }
                else if (lastOffset > 0 == xOffset > 0)
                {
                    lastOffset = xOffset;
                }

                float factor = Mathf.Clamp01(1 - Mathf.Abs(lastOffset) * distanceFactor);

                if (lastOffset > 0)
                {
                    zRotation += -entityRotation * factor;
                }
                else
                {
                    zRotation += entityRotation * factor;
                }

                currentPushDown += (factor * entityPressDown - currentPushDown) * Time.fixedDeltaTime * currentSpeed;

                zRotation = Mathf.Clamp(zRotation, -maximumRotation, maximumRotation);
            }
            else
            {
                lastOffset = 0;
                currentPushDown -= currentPushDown * Time.fixedDeltaTime * currentSpeed * 1.5f;
            }

            currentAngle += (zRotation - currentAngle) * Time.fixedDeltaTime * currentSpeed;

            mat.SetFloat("_Rotation", currentAngle);
            mat.SetFloat("_Press_Down", currentPushDown);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (canBeBent)
            {
                insideCount++;
                entities.Add(collision.transform);
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (canBeBent)
            {
                insideCount--;
                entities.Remove(collision.transform);
            }
        }
    }
}