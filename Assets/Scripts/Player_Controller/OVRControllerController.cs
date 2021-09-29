using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// CD: Modified to act similarly to Controller controller as used on vive models ::: Handles dial inputs and modifies the controller colors based upon such
/// </summary>
public class OVRControllerController : MonoBehaviour
{
	#region PUBLIC_VAR
	/// <summary>
	///		VD : padStatus
	///		Status of touchpad, could use an enum instead of int
	/// </summary>
	public int padStatus = 0;
	#endregion

	#region PRIVATE_VAR
	/**
	<summary>
		Group : GameObjects
		Members : touchpadGroup, q1, q2, q3, q4
	</summary>
	**/
	//private GameObject touchpadGroup, q1, q2, q3, q4;

	//private TextMesh textBox;

	private bool buttonDown = false;

	/// <summary>
	///		VD : Inspector references to colors used for quadrants and top plate glow ::: Unifies oculus controlles so hands align
	/// </summary>
	public Material dial_y, dial_m, dial_k, dial_c, pad_glow;
	#endregion

	#region UNITY_FUNC
	/**
	<summary>
		FD : Enables the emission on the top of the oculus pad
	</summary>
	**/
	void Start()
	{
		pad_glow.EnableKeyword("_EMISSION");

	}

	/**
	<summary>
		FD : Update()
		Act depending on what button is down
	</summary>
	**/
	void Update()
	{
		// determine if touch is in a quadrant
		if (OVRInput.Get(OVRInput.Button.PrimaryThumbstickUp) || OVRInput.Get(OVRInput.Button.SecondaryThumbstickUp)) OnUpPress();
		//else if (Input.GetKeyUp(KeyCode.UpArrow)) Debug.Log("HERE");
		else if (OVRInput.Get(OVRInput.Button.PrimaryThumbstickRight) || OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight)) OnRightPress();
		else if (OVRInput.Get(OVRInput.Button.PrimaryThumbstickDown) || OVRInput.Get(OVRInput.Button.SecondaryThumbstickDown)) OnDownPress();
		else if (OVRInput.Get(OVRInput.Button.PrimaryThumbstickLeft) || OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft)) OnLeftPress();
	}
	#endregion

	#region PUBLIC_FUNC
	/// <summary>
	/// FD: On Up, Set glow to white and make only the white sections glow
	/// </summary>
	public void OnUpPress()
	{
		padStatus = 0;
		dial_k.EnableKeyword("_EMISSION");
		dial_c.DisableKeyword("_EMISSION");
		dial_m.DisableKeyword("_EMISSION");
		dial_y.DisableKeyword("_EMISSION");
		pad_glow.SetColor("_EmissionColor", Color.white * .3f);
	}

	/// <summary>
	/// FD: On Right, Set glow to cyan and make only the cyan sections glow
	/// </summary>
	public void OnRightPress()
	{
		padStatus = 1;
		dial_k.DisableKeyword("_EMISSION");
		dial_c.EnableKeyword("_EMISSION");
		dial_m.DisableKeyword("_EMISSION");
		dial_y.DisableKeyword("_EMISSION");
		pad_glow.SetColor("_EmissionColor", Color.cyan * .3f);
	}

	/// <summary>
	/// FD: On Left, Set glow to magenta and make only the magenta sections glow
	/// </summary>
	public void OnDownPress()
	{
		padStatus = 2;
		dial_k.DisableKeyword("_EMISSION");
		dial_c.DisableKeyword("_EMISSION");
		dial_m.EnableKeyword("_EMISSION");
		dial_y.DisableKeyword("_EMISSION");
		pad_glow.SetColor("_EmissionColor", Color.magenta * .3f);
	}

	/// <summary>
	/// FD:on Right, Set glow to yellow and make only the yellow sections glow
	/// </summary>
	public void OnLeftPress()
	{
		padStatus = 3;
		dial_k.DisableKeyword("_EMISSION");
		dial_c.DisableKeyword("_EMISSION");
		dial_m.DisableKeyword("_EMISSION");
		dial_y.EnableKeyword("_EMISSION");
		pad_glow.SetColor("_EmissionColor", Color.yellow * .3f);
	}
	#endregion
}
