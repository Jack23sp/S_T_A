using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCameraShake : MonoBehaviour
{
    public static CustomCameraShake singleton;
    public Animator animator;
    public Camera cameraMain;
    // Start is called before the first frame update
    void Start()
    {
        if (!singleton) singleton = this;
        if (!cameraMain) cameraMain = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShakeCameraLeftHigh()
    {
        Vector3 currentRotation = new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.y - 5.0f);
        this.transform.eulerAngles = currentRotation;
    }
    public void ShakeCameraRightHigh()
    {
        Vector3 currentRotation = new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.y + 5.0f);
        this.transform.eulerAngles = currentRotation;
    }
    public void ShakeCameraLeftMedium()
    {
        Vector3 currentRotation = new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.y -3.0f);
        this.transform.eulerAngles = currentRotation;
    }
    public void ShakeCameraRightMedium()
    {
        Vector3 currentRotation = new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.y + 3.0f);
        this.transform.eulerAngles = currentRotation;
    }
    public void ShakeCameraLeftLow()
    {
        Vector3 currentRotation = new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.y -1.0f);
        this.transform.eulerAngles = currentRotation;
    }
    public void ShakeCameraRightLow()
    {
        Vector3 currentRotation = new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.y + 1.0f);
        this.transform.eulerAngles = currentRotation;
    }
    public void ShakeCameraNeutral()
    {
        Vector3 currentRotation = new Vector3(0.0f,0.0f,0.0f);
        this.transform.eulerAngles = currentRotation;
    }

    public void ResetShakeBool()
    {
        animator.SetBool("SHAKE", false);
    }
}
