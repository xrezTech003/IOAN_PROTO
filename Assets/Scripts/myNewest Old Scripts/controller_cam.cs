using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityOSC;

/**
<summary>
	CD : controller_cam
	Reassigns Meshes in front of rcDummy object :
	:: IV: Mostly garbage, bits of fragmented usefulness in a useless package - NDH
</summary>
**/
public class controller_cam : MonoBehaviour
{
	#region PUBLIC_VAR
	/// <summary>
	///		VD : trackedID
	/// </summary>
	public GameObject trackedID;

	/// <summary>
	///		VD : hit_miss
	/// </summary>
	public bool hit_miss = false; //Var unneeded

	/// <summary>
	///		VD : trigger
	/// </summary>
	public bool trigger = false;

	/// <summary>
	///		VD : selectedID
	/// </summary>
	public int selectedID = 0;

	/// <summary>
	///		VD : fwd
	/// </summary>
	public Vector3 fwd;

	/// <summary>
	///		VD : direction
	/// </summary>
	public Ray direction;

	/// <summary>
	///		VD : hit
	/// </summary>
	public RaycastHit hit;

	/// <summary>
	///		VD : rcDummy
	/// </summary>
	public GameObject rcDummy;

	/// <summary>
	///		VD : triggerState
	/// </summary>
	public bool triggerState;

	/// <summary>
	///		VD : highlightActive
	/// </summary>
	public bool highlightActive;

	/**
	<summary>
		Group : p Vectors
		Members : pA, pB, pC, pD
	</summary>
	**/
	public Vector3 pA;
	public Vector3 pB;
	public Vector3 pC;
	public Vector3 pD;

	/// <param name="modelID"></param>
	public int modelID;

	/**
	<summary>
		Group : Meshes
		Members : meshList, m1, m2, m3, m4, m5, m6, m7
	</summary>
	**/
	public Mesh[] meshList;
	public Mesh m1;
	public Mesh m2;
	public Mesh m3;
	public Mesh m4;
	public Mesh m5;
	public Mesh m6;
	public Mesh m7;
	#endregion

	#region PRIVATE_VAR
	/// <summary>
	///		VD : trackedObj
	/// </summary>
	private SteamVR_TrackedObject trackedObj;

	/// <summary>
	///		VD : GO
	/// </summary>
	private GameObject GO;
	#endregion

	#region UNITY_FUNC
	/**
	<summary>
		FD : Start()
		Init all values
		<remarks>
			IV
			Can init all meshList meshes in component menu instead of creating 7 different meshes
		</remarks>
	</summary>
	**/
	void Start()
	{
		modelID = 0;

		meshList = new Mesh[7];
		meshList[0] = m1;
		meshList[1] = m2;
		meshList[2] = m3;
		meshList[3] = m4;
		meshList[4] = m5;
		meshList[5] = m6;
		meshList[6] = m7;

		OSCHandler.Instance.Init();

		triggerState = false;
		highlightActive = false;

        pA = pB = pC = pD = Vector3.zero;
	}
	#endregion

	#region PUBLIC_FUNC
	/**
	<summary>
		FD : controllerhandler_trigger()
		Set Trigger state to true
		Set model ID to a random int 0 - 7
		If there is anything in front of gameObject
			And if the hit object tag is "submesh"
				Get all vertices and triangles from the submesh
				Set all p0-2 to triangle index of hit
				Reasign p0-2 to hitTransform Transform point
				Assign pA-C to p0-2 and pD to hit.point
				set highlightActive to true
				Get nedbtester from gameobject.
				If the star is selected
					Set color of newdbtester frontStar to random color
		Else
			set highlightActive to false
		If newdbtester coroutine a-c isn't active
			If Call Timer is less than 6
				If nedbtester coroutine a, b, or c is active
					Call controllerhandle_release()
		Else set highlight active to false
	</summary>
	**/
	public void ControllerHandleTrigger()
	{
		triggerState = true;
		Debug.Log("CCAM: " + gameObject.name + "Trigger Pressed");

		modelID = Random.Range(0, 7);
		Debug.Log("CCAM: Random ID -> " + modelID);

		gameObject.GetComponent<newdbtester>().frontStar.GetComponent<MeshFilter>().mesh = meshList[modelID];

		RaycastHit hit;
		if (Physics.Raycast(rcDummy.transform.position, rcDummy.transform.forward, out hit))
		{
			if (hit.collider.gameObject.tag == "submesh")
			{
				MeshCollider meshCollider = hit.collider as MeshCollider;
				if (meshCollider == null || meshCollider.sharedMesh == null) return;

				Mesh mesh = meshCollider.sharedMesh;
				Vector3[] vertices = mesh.vertices;
				int[] triangles = mesh.triangles;

				selectedID = hit.collider.gameObject.GetComponent<StarSerializer>().myIDs[triangles[hit.triangleIndex * 3]];
				gameObject.GetComponent<newdbtester>().starOrigin = hit.point;

				Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
				Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
				Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];

				Transform hitTransform = hit.collider.transform;

				p0 = hitTransform.TransformPoint(p0);
				p1 = hitTransform.TransformPoint(p1);
				p2 = hitTransform.TransformPoint(p2);

                pA = p0;
                pB = p1;
                pC = p2;
				pD = hit.point;

				highlightActive = true;
				newdbtester ndta = gameObject.GetComponent<newdbtester>();

				if (!ndta.starSelected)
				{
                    Color myCol = Color.clear;
					Random.InitState(selectedID);

					myCol.r = Random.Range(0.0f, 1.0f);
					myCol.g = Random.Range(0.0f, 1.0f);
					myCol.b = Random.Range(0.0f, 1.0f);

					ndta.frontStar.GetComponent<MeshRenderer>().material.color = myCol;
				}
			}
		}
		else
            highlightActive = false;

		newdbtester ndt = gameObject.GetComponent<newdbtester>();
		if (!ndt.coaRoutActive && (!ndt.cobRoutActive && !ndt.cocRoutActive))
		{
			if (ndt.dbCallTimer < 6.0f)
			{
				if (ndt.coaRoutActive || (ndt.cobRoutActive || ndt.cocRoutActive)) controllerhandle_release(); //This should never run?

				ndt.IDLoader(selectedID);
			}
		}
	}

	/**
	<summary>
		FD : controllerhandle_release()
		Reset State Trigger
		Set colors to opaque
	</summary>
	**/
	public void controllerhandle_release()
	{
		triggerState = false;

		newdbtester ndt = gameObject.GetComponent<newdbtester>();
		ndt.IDStopper();
		ndt.frontStar.transform.localPosition = new Vector3(-0.001f, 0.0f, 800.005f);
		ndt.frontStar.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
		ndt.frontRay.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, (80.0f / 255.0f));
		ndt.starSelected = false;
	}
	#endregion
}
