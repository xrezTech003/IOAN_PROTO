using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
<summary>
	CD : BoundsFinder
	Sets vertex bounds based on gameObject MeshFilter
</summary>
**/
public class BoundsFinder : MonoBehaviour 
{
	#region PUBLIC_VAR
	/// <summary>
	///		VD : initObject
	///		c:shadeSwapper.vertBounds are reset
	/// </summary>
	public GameObject initObject;
	#endregion

	#region PRIVATE_VAR
	/// <summary>
	///		VD : runOnceFlag
	///		Boolean switch for f:Update()
	/// </summary>
	bool runOnceFlag;
	#endregion

	#region UNITY_FUNCTIONS
	/**
	<summary>
		FD : Start()
		Initializes v:runOnceFlag to false
	</summary>
	**/
	void Start ()
    {
		runOnceFlag = false;
	}
	
	 /**
	 <summary>
		 FD : Update()
		 Creates new Vector4 and an Array of Vector3 from gameObject mesh vertices
		 Sets vertBounds compared to every Vector3 in Array
		 Sets v:initObject c:shadeSwapper vertBounds to vertBounds
		 Toggle v:runOnceFlag
	 </summary>
	 **/
	void Update ()
    {
		if (!runOnceFlag)
        {
			Vector4 vertBounds = Vector4.zero;
			Mesh myMesh = GetComponent<MeshFilter>().sharedMesh;

			Vector3[] verts = myMesh.vertices;

			for (int i = 0; i < verts.Length; i++)
            {
				Vector3 vert = transform.TransformPoint(verts [i]);

				if (vert.x < vertBounds.x) vertBounds.x = vert.x;

				if (vert.z < vertBounds.y) vertBounds.y = vert.z;

				if (vert.x > vertBounds.z) vertBounds.z = vert.x;

				if (vert.z > vertBounds.w) vertBounds.w = vert.z;
			}

			initObject.GetComponent<shadeSwapper>().vertBounds = vertBounds;

			runOnceFlag = true;
		}
	}
    #endregion
}
