// Pixel Perfect Rendering in Unity
// Source: Unity 2014 Unity 2D best practices
// (modified by noobtuts.com | vis2k (zoom factor))
using UnityEngine;

[ExecuteInEditMode]
public class PixelDensityCamera: MonoBehaviour
{
    // The value that all the Sprites use
    public float pixelsToUnits = 16;

    // Zoom Factor
    public float zoom = 2;

    void Update()
    {
        if (Application.platform == RuntimePlatform.Android ||
            Application.platform == RuntimePlatform.IPhonePlayer)
        {
            zoom = 4;
        }
        GetComponent<Camera>().orthographicSize = Screen.height / pixelsToUnits / zoom / 2;
    }
}