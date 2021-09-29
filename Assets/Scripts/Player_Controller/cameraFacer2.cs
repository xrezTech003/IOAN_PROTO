using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
<summary>
    CD : cameraFacer2
    Makes gameObject face Camera.main
</summary>
**/
public class cameraFacer2 : MonoBehaviour
{
    /**
    <summary> 
        FD : LateUpdate()
        gameObject will look at main camera
        gameObject will rotate Y value 180 degrees
    </summary>
    **/
    void LateUpdate ()
    {
		transform.LookAt(Camera.main.transform.position);
		transform.Rotate(Vector3.up * 180f);
	}
}