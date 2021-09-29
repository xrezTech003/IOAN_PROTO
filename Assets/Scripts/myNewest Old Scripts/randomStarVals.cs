using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
<summary>
	CD : randomStarVals
	Sets gameObject shadesetter values to random values
</summary>
**/
public class randomStarVals : MonoBehaviour 
{
	/**
	<summary>
		FD : Start()
		Set ShaderSetter to random values
	</summary>
	**/
	void Start () 
	{
		ShaderSetter ss = GetComponent<ShaderSetter> ();
		ss.blend *= Random.Range (0.5f, 1.5f);
		ss.drip *= Random.Range (0.5f, 1.5f);
		ss.edgeWidth *= Random.Range (0.5f, 1.5f);
		ss.noiseScale *= Random.Range (0.5f, 1.5f);
		ss.noiseWeight *= Random.Range (0.5f, 1.5f);
		ss.poke *= Random.Range (0.5f, 1.5f);
		ss.speed *= Random.Range (0.5f, 1.5f);
		ss.rimExpo *= Random.Range (0.5f, 1.5f);
		ss.exteriorScale *= Random.Range (0.5f, 1.5f);
		ss.interiorScale *= Random.Range (0.5f, 1.5f);
		ss.gradColorA = Random.ColorHSV (0f, 1f, .5f, 1f, .4f, 1f, 1f, 1f);
		//ss.gradColorB = Random.ColorHSV (0f, 1f, .5f, 1f, .4f, 1f, 1f, 1f);
		ss.gradColorB = new Color(1f,1f,1f,1f);
		ss.rimColor = Random.ColorHSV (0f, 1f, .4f, 1f, .4f, 1f, 1f, 1f);
		ss.edgeColor = Random.ColorHSV (0f, 1f, .4f, 1f, .4f, 1f, 1f, 1f);
	}

    #region USELESS_CODE
	// Update is called once per frame
	void Update()
	{

	}
    #endregion
}
