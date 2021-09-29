using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
<summary>
    CD : canvasMover
    Sets canvas position to Camera screen point position I_A ::: Never referenced, never used NDH
</summary>
**/
public class canvasMover : MonoBehaviour 
{
    #region PUBLIC_VAR
    /// <summary>
    ///     VD : canvasContainer
    ///     GameObject to have position modifiedn
    /// </summary>
    public GameObject canvasContainer;

    /// <summary>
    ///     VD : myCam Earnest Public - Nonexistent in scene, therefore empty, dead
    /// </summary>
	public Camera myCam;
    #endregion

    /**
    <summary>
        FD : Update()
        set v:canvasContainer position to the camera objects World to screen point value
    </summary>
    **/
    void Update ()
    {
		Vector3 screenPos = myCam.WorldToScreenPoint(transform.position);
		canvasContainer.transform.position = screenPos;
	}
}
