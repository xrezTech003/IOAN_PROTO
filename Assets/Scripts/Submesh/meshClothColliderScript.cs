using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
<summary>
	CD : meshClothColliderScript
	Update meshes randomly
</summary>
**/
public class meshClothColliderScript : MonoBehaviour 
{
	#region PRIVATE_VAR
	/// <summary>
	///		VD : updateTimer
	///		Timer for f:Update()
	/// </summary>
	float updateTimer = 0f;

	/// <summary>
	///		VD : myMesh
	/// </summary>
	Mesh myMesh;

	/// <summary>
	///		VD : cloth
	/// </summary>
	Cloth cloth;
	#endregion

	#region UNITY_FUNC
	/**
	<summary>
		FD : Start()
		Set v:cloth to parent component Cloth && 
		Set v:myMesh to MeshFilter mesh
	</summary>
	**/
	void Start()
	{
		cloth = GetComponentInParent<Cloth>();
		myMesh = GetComponent<MeshFilter>().mesh;
	}

	/**
	<summary>
		FD : Update()
		If updateTimer - deltaTime is less than 0
			Set updateTime to a random float between .075 and .125
			Set the v:myMesh vertices to v:cloth vertices
			Set MeshCollider sharedMesh to v:myMesh
	</summary>
	**/
	void Update()
	{
		if ((updateTimer -= Time.deltaTime) < 0f)
		{
			updateTimer = Random.Range(0.075f, 0.125f);
			myMesh.vertices = cloth.vertices;
			GetComponent<MeshCollider>().sharedMesh = myMesh;	
		}
	}
	#endregion

	#region COMMENTED_CODE
	/*
	 * Was in Start()
	 * 
	//GetComponent < MeshCollider> ().sharedMesh = null;
	//myMesh = GetComponentInParent<Cloth> ().GetComponent<MeshFilter> ().mesh;

	//foreach (var col in cloth.GetComponents<MeshCollider>()) {
	//	col.sharedMesh = null;
	//}
	*/

	/*
	 * Was in Update()
	 * 
	//myMesh = new Mesh();
	//GetComponentInParent<SkinnedMeshRenderer>().BakeMesh(myMesh);
	//GetComponent < MeshCollider> ().sharedMesh = myMesh;
	*/

	/*
	public Transform clothObject;
	Cloth cloth;
	Mesh mesh;
	MeshCollider ourcollider;
	// Use this for initialization
	void Start () {           
		cloth = clothObject.GetComponent<Cloth>();
		mesh = GetComponent<MeshFilter>().mesh;
		ourcollider = GetComponent<MeshCollider>();

		//remove the initial flat collider... it took a while to figure this out. I think it's right
		//foreach (var col in GetComponents<MeshCollider>())
		//{
		//	col.sharedMesh = null;
		//}

	}

	// Update is called once per frame
	void Update () {

		//http://answers.unity.com/answers/361077/view.html
		mesh.vertices = cloth.vertices;      
		ourcollider.sharedMesh = mesh;
		//mesh.RecalculateNormals();
	}
	*/
	#endregion
}
