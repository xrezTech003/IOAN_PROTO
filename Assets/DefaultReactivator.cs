using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultReactivator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.FindGameObjectWithTag("heteroPlayer") == null && !GameObject.Find("/VRControl/[CameraRig]/Controller (right)/Model").activeInHierarchy)
        {
            GameObject defaultControllerM = GameObject.Find("/VRControl/[CameraRig]/Controller (right)/Model");
            defaultControllerM.SetActive(true);
        }
        else if (GameObject.FindGameObjectWithTag("heteroPlayer") == null && !GameObject.Find("/VRControl/[CameraRig]/Controller (left)/Model").activeInHierarchy)
        {
            GameObject defaultControllerM = GameObject.Find("/VRControl/[CameraRig]/Controller (left)/Model");
            defaultControllerM.SetActive(true);
        }
    }
}
