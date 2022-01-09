using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class CarLightManager : MonoBehaviour
{
    public Car car;

    public GameObject upObject;
    public GameObject downObject;
    public GameObject leftObject;
    public GameObject rightObject;

    public Light2D light2D;

    public float lightDistance = 1.14f;

    // Start is called before the first frame update
    void Start()
    {
        car = GetComponentInParent<Car>();
    }

    // Update is called once per frame
    void Update()
    {

        light2D.gameObject.SetActive(car.lightON);

        if (light2D.gameObject.activeInHierarchy)
        {
            upObject.transform.localPosition = new Vector3(0.0f, lightDistance, 0.0f);
            downObject.transform.localPosition = new Vector3(0.0f, -(lightDistance), 0.0f);
            leftObject.transform.localPosition = new Vector3(-(lightDistance), 0.0f, 0.0f);
            rightObject.transform.localPosition = new Vector3(lightDistance, 0.0f, 0.0f);

            if (car.lookDirection.y == 1)
            {
                light2D.transform.SetParent(upObject.transform);
                light2D.transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
                light2D.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            }
            else if (car.lookDirection.y == -1)
            {
                light2D.transform.SetParent(downObject.transform);
                light2D.transform.eulerAngles = new Vector3(0.0f, 0.0f, 180.0f);
                light2D.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            }

            if (car.lookDirection.x == -1)
            {
                light2D.transform.SetParent(leftObject.transform);
                light2D.transform.eulerAngles = new Vector3(0.0f, 0.0f, 90.0f);
                light2D.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            }
            else if (car.lookDirection.x == 1)
            {
                light2D.transform.SetParent(rightObject.transform);
                light2D.transform.eulerAngles = new Vector3(0.0f, 0.0f, -90.0f);
                light2D.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            }
        }
    }
}
