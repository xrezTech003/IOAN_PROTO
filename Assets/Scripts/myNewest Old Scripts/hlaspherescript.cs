using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

/**
<summary>
	CD : hlaspherescript
	Set Position and Light based on controller_cam :
	:: Funky garbage - NDH
</summary>
**/
public class hlaspherescript : MonoBehaviour
{
	#region PUBLIC_VAR
	/// <summary>
	///		VD : conScript
	///		GameObject containing controller_cam component
	/// </summary>
	public GameObject conScript;
    #endregion

    #region UNITY_FUNC
	/**
	<summary>
		FD : Update
		Get controller_cam from conScript
		Set position to controller_cam pD var
		If controller_cam is highlighted
			Enable Light
			Set range to calculation based on position
		Else
			Disable Light
	</summary>
	**/
    void Update () 
	{
		controller_cam cCam = conScript.GetComponent<controller_cam>();
	
		gameObject.transform.position = cCam.pD;

		if (cCam.highlightActive) 
		{
			gameObject.GetComponent<Light>().enabled = true;
			float mySize = (Camera.main.transform.position - gameObject.transform.position).magnitude * 0.015f;
			gameObject.GetComponent<Light>().range = mySize;
		} 
		else
            gameObject.GetComponent<Light>().enabled = false;
	}
	#endregion
}
