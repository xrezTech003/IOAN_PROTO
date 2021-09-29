using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
<summary>
	CD : fadeIner
	Fade text color to transparency
</summary>
**/
public class fadeIner : MonoBehaviour 
{
	#region PRIVATE_VAR
	/// <summary>
	///		VD : elapsed
	///		Time elapsed
	/// </summary>
	private float elapsed = -6f;
    #endregion

    #region UNITY_FUNC
	/**
	<summary>
		FD : Update()
		If v:elapsed is less than 20
			Add time to v:elapsed
			Set alpha value of the text component color accordignly
	</summary>
	**/
	void Update () 
	{
		if (elapsed < 20.0f)
		{
			elapsed += Time.deltaTime;

			float aScal = elapsed <= 20.0f ? elapsed / 20.0f : 1.0f;

			Color color = gameObject.GetComponent<Text>().color;
			color.a = aScal;

			gameObject.GetComponent<Text>().color = color;
		}
	}
	#endregion
}
