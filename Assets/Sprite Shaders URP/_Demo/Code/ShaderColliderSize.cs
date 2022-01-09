using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteShadersURP
{
    public class ShaderColliderSize : MonoBehaviour
    {
        SpriteRenderer sr;
        Material mat;
        Collider2D col;

        void Start()
        {
            sr = GetComponent<SpriteRenderer>();
            mat = sr.material;

            col = GetComponentInParent<Collider2D>();
        }

        void FixedUpdate()
        {
            mat.SetVector("_Size", new Vector4(col.bounds.extents.x * 2f, col.bounds.extents.y * 2f, 0, 0));
        }
    }
}