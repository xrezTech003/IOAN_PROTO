using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
<summary>
	CD : controllerMover
	Updates the position of the controller I_A ::: IV: Never referecned, whacky values, probably garbage
</summary>
**/
public class controllerMover : MonoBehaviour 
{
	#region PUBLIC_VAR
	/// <summary>
	///		VD offsetX
	/// </summary>
	public float offsetX;

	/// <summary>
	///		VD : offsetY
	/// </summary>
	public float offsetY;

	/// <summary>
	///		VD : controller
	/// </summary>
	public GameObject controller;
    #endregion

    #region UNITY_FUNC
	/**
	<summary>
		FD : Update()
		Sets gameObject position and local position
	</summary>
	**/
    void Update() 
	{
        Vector3 tPos = controller.transform.position + new Vector3(547895f, -441492f, 0);
		tPos.z = 10.0f;

	    transform.position = tPos;
		transform.localPosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y, tPos.z);
	}
	#endregion
}
