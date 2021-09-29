using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
///	CD: Network controller for, colour setter for, and (dead)physics handler for the right controllers of players
/// </summary>
public class netControlR : NetworkBehaviour 
{
	#region PUBLIC_VAR
	public OVRManager ovrm;
	/// <summary>
	/// VD: ID (0-2) of the player this right controller is attached to
	/// </summary>
	[SyncVar(hook = "OnChangePlayerID")]
	public int myID;

	/// <summary>
	/// VD: Set in PlayerMove to the rightcontroller tagged object in VRControl - reference ot the right controller (no pad)
	/// </summary>
	public GameObject controlRSet;

	/// <summary>
	/// VD: Should line up with myID, colour to set the conroller to (red, green, blue)
	/// </summary>
	[SyncVar]
	public Color myColor;

	/// <summary>
	/// VD: Reference to the o:leftcontroller (TAG)
	/// </summary>
	public GameObject lContObj;

	/// <summary>
	/// VD: Counter to keep track of number of capsule colliders on the cloth when adding this one IV: Not sure why it's public
	/// </summary>
	public int capsuleIndex = 0;

	/// <summary>
	/// VD: GS: Part of a random raycast that serves no purpose? 
	/// </summary>
	public GameObject midMesh;

	/// <summary>
	/// VD: GS: Part of a random raycast that serves no purpose? 
	/// </summary>
	public int midMeshNumVerts;

	/// <summary>
	/// VD: Reference to o:Initializer c:Initializer which holds the interactiveSubMesh and Star Lookup table
	/// </summary>
	public GameObject initRef;

	/// <summary>
	/// This thing is set to zero in ShadeSwapper, where it is get'ed from in this script, and a couple scripts do math with it anyway? Possibly dead
	/// </summary>
	public Vector4 vertBounds;
	public GameObject OVRTrackingSpace;
	#endregion

	#region PRIVATE_VAR
	/// <summary>
	/// VD: Reference to o:Initialiser c:ShadeSwapper
	/// </summary>
	public shadeSwapper sSwapper;

	/// <summary>
	/// VD :Reference to OSCPacketBus
	/// </summary>
	private GameObject oscPacketBus;

	/// <summary>
	/// VD: Boolean to update the cloth capsule collider list in the first frame of Update
	/// </summary>
	private bool startFlag;

	/// <summary>
	/// VD: Float to keep track of/make sure you can't tap twice in less than half a second
	/// </summary>
	private float starTapTimer = 0f;
	#endregion

	#region UNITY_FUNC
	/**
	<summary><sig>			FD : Start()
		If v:myID < 0 and v:myID > 2: disable all MeshRenders in all children of the "vive_controller" child, then return
		Find Object with tag "clothPlane"
		Set v:vertBounds to Object with tag "init" shadSwapper var vertBounds
		Set v:capsuleIndex to myClothPlane comp Cloth capsuleColliders Length
		Make new CapsuleCollider array equal to myClothPlane capsuleColliders with one extra open spot
		Set v:startFlag to true, v:oscPacketBus to Object with tag "OSCPacketBus", and v:sSwapper to Object with tag "init"
		For every child
			If the name is either "frontring" or "handle", Set all renderer colors "_EmissionColor" to v:myColor
			Else if the name is "rims"
				For all MeshRenderers in Children
					Set "_color" to v:myColor * .8 if name is also "innerRim" or set "_EmissionColor" to myColor if not
			Else if the name is "touchpad"
				For all MeshRenderers in Children
					Set "_Color" and "_EmissionColor" to clear
		If v:isServer is true: SetStarHoldObj to myID and this gameObject
	</sig></summary>
	**/
	void Start()
	{
		OVRTrackingSpace = GameObject.Find("TrackingSpace");
		sSwapper = GameObject.FindGameObjectWithTag("init").GetComponent<shadeSwapper>();
		ovrm = GameObject.FindGameObjectWithTag("OVRRig").GetComponent<OVRManager>();
		//OVRTrackingSpace = GameObject.Find("TrackingSpace");
		if (!(0 <= myID && myID <= 2))
		{
			foreach(Transform val in transform.Find("vive_controller").GetComponentsInChildren<Transform>()) foreach(MeshRenderer mr in val.GetComponentsInChildren<MeshRenderer>()) mr.enabled = false;

			return;
		}

		GameObject myClothPlane = GameObject.FindGameObjectWithTag("clothPlane");
		initRef = GameObject.FindGameObjectWithTag("init");
		vertBounds = sSwapper.vertBounds; //vertBounds = GameObject.FindGameObjectWithTag("init").GetComponent<shadeSwapper>().vertBounds;

		capsuleIndex = myClothPlane.GetComponent<Cloth>().capsuleColliders.Length;
		CapsuleCollider[] myCapColList = new CapsuleCollider[capsuleIndex + 1];

		for (int i = 0; i < capsuleIndex; i++)
            myCapColList[i] = myClothPlane.GetComponent<Cloth>().capsuleColliders[i];

		myCapColList[capsuleIndex] = GetComponentInChildren<CapsuleCollider>();
		myClothPlane.GetComponent<Cloth>().capsuleColliders = myCapColList;

		startFlag = true;

		oscPacketBus = GameObject.FindGameObjectWithTag("OSCPacketBus");

		//wavSpawnTimer = 0f;
		

		Transform[] myChildren = gameObject.GetComponentsInChildren<Transform>();

		/* //Cntrolller parts colour set here NDH
		foreach (Transform child in myChildren) //Transform child in gameObject.GetComponentsInChildren<Transform>()
		{
			if (child.name == "frontring" || child.name == "handle") foreach (MeshRenderer gcRend in child.GetComponentsInChildren<MeshRenderer>()) gcRend.material.SetColor("_EmissionColor", myColor);
			else if (child.name == "rims")
			{
				foreach (MeshRenderer gcRend in child.GetComponentsInChildren<MeshRenderer>())
				{
					if (gcRend.gameObject.name == "innerRim") gcRend.material.SetColor("_Color", myColor * 0.8f);
					else gcRend.material.SetColor("_EmissionColor", myColor * 0.8f);
				}
			}*/
		//TODO: Temp disabled to test tag reclouring
		GameObject[] glows = GameObject.FindGameObjectsWithTag("Glow");
		foreach (GameObject glow in glows)
		{
			foreach (MeshRenderer gcRend in glow.GetComponentsInChildren<MeshRenderer>())
			{
				gcRend.material.SetColor("_Color", myColor * .8f);
				gcRend.material.SetColor("_EmissionColor", myColor * .8f);
			}
		}
		GameObject[] emits = GameObject.FindGameObjectsWithTag("GlowEmit");
		foreach (GameObject emit in emits)
		{
			foreach (MeshRenderer gcRend in emit.GetComponentsInChildren<MeshRenderer>())
			{
				gcRend.material.SetColor("_EmissionColor", (emit.name == "vive_headsetnew") ? myColor * Mathf.LinearToGammaSpace(1.0f) : myColor);
			}
		}
		GameObject vive = gameObject.transform.Find("vive_controller").gameObject;
		GameObject touchPad = vive.transform.Find("touchpad").gameObject;
			{
				foreach (MeshRenderer gcRend in touchPad.GetComponentsInChildren<MeshRenderer>())
				{
					gcRend.material.SetColor("_Color", new Color(0f, 0f, 0f, 0f)); //Color.clear
					gcRend.material.SetColor("_EmissionColor", new Color(0f, 0f, 0f, 0f)); //Color.clear
				}
			}
		

		if (isServer) oscPacketBus.GetComponent<OSCPacketBus>().SetStarHoldObj(myID, gameObject);
	}

	/**
	<summary><sig>		FD : Update()
		If the object has authority
			If v:myID < 0 and > 2: return
			If v:lContObj is null: set lContObj to Object with tag "controlLeft"
			If v:lcontrolRSet is null: set controlRSet to Object with tag "controlRight"
			Else
				If startFlag is true
					Set startFlag to false
					Set child of controlRSet named "Model" to false
					If There is anything below this object
						Set v:midMesh to what is below
						Set v:midMeshNumVerts to sharedMesh of v:midMesh
				Set transform data to controlRSet data
				If v:startTapTimer > 0: subtract deltaTime from it
				If v:startTapTimer is < 0 and the y velocity of v:controlRSet controller is < -2
					If -.1 < position.y - hit2 y < .35
						Set Vectors ap0-2 to Mesh.uv at 0-2
						Set interpUV to Sum api * barycentricCoordinate.xyz
						Create new position based on interpUV
						If anything is below that new position
							Set p00-20 to Mesh vertices 0-2
							Set p0-2 to colliders based on p00-20
							Find which p0-2 is closer to the hit point
							Set tabID the closest triangle ID
						Call CmdSendOSCTap with myID, tapID, controller velocity and some constants
				Set trigger rotations
		Set sSwapper lControllerPos to transform position
		Increment sSwapper PlayerIncremenetor
	</sig></summary>
	**/
	void Update()
	{
		OVRInput.Update();
		if(ovrm == null) ovrm = GameObject.FindGameObjectWithTag("OVRRig").GetComponent<OVRManager>();
		if (hasAuthority)
		{
			if (!(0 <= myID && myID <= 2)) return;

			if (!lContObj && OVRPlugin.GetSystemHeadsetType() == 0) lContObj = GameObject.FindGameObjectWithTag("controlLeft");
			else lContObj = GameObject.FindGameObjectWithTag("oculusLeft");

			if (!controlRSet && OVRPlugin.GetSystemHeadsetType() == 0) controlRSet = GameObject.FindGameObjectWithTag("controlRight");
			else if(!controlRSet) controlRSet = GameObject.FindGameObjectWithTag("oculusRight");
			else
			{
				if (startFlag)
				{
					startFlag = false;

					Transform myChildModel = controlRSet.transform.Find("Model");
					//myChildModel.gameObject.SetActive(false); //controlRSet.transform.Find("Model").gameObject.SetActive(false);

					int layerMask = LayerMask.GetMask("cloth");
					RaycastHit hit;
					bool hit_miss = Physics.Raycast(transform.position + new Vector3(0f, 20f, 0f), Vector3.down, out hit, Mathf.Infinity, layerMask);
					if (hit_miss)
					{
						midMesh = hit.collider.gameObject;
						midMeshNumVerts = midMesh.GetComponent<MeshFilter>().sharedMesh.vertexCount;
					}
				}

				transform.position = controlRSet.transform.position;
				transform.rotation = controlRSet.transform.rotation;
				//oldControllerPos = controlRSet.transform.position.y;

				if (starTapTimer >= 0.0f) starTapTimer -= Time.deltaTime;
				float vel = 0;
				if (OVRPlugin.GetSystemHeadsetType() == 0) vel = SteamVR_Controller.Input((int)controlRSet.GetComponent<SteamVR_TrackedObject>().index).velocity.y;
				else vel =((OVRTrackingSpace.transform.TransformVector(OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch)))).y;


				if (starTapTimer <= 0.0f && vel < -2.0f)
				{
					RaycastHit hit2;
					bool hit_miss2 = Physics.Raycast(transform.position + new Vector3(0f, 20f, 0f), Vector3.down, out hit2, Mathf.Infinity);
					if (-0.1f < transform.TransformPoint(new Vector3(0f, 0f, 0.065f)).y - hit2.point.y && transform.TransformPoint(new Vector3(0f, 0f, 0.065f)).y - hit2.point.y < 0.35f)
					{
						//Debug.Log ("tapSent2!");
						int tapID = 40000001;

						Mesh hitMesh2 = (hit2.collider as MeshCollider).sharedMesh;

						Vector3 ap0 = hitMesh2.uv[hitMesh2.triangles[hit2.triangleIndex * 3 + 0]];
						Vector3 ap1 = hitMesh2.uv[hitMesh2.triangles[hit2.triangleIndex * 3 + 1]];
						Vector3 ap2 = hitMesh2.uv[hitMesh2.triangles[hit2.triangleIndex * 3 + 2]];

						Vector3 baryCenter = hit2.barycentricCoordinate;
						Vector3 interpUV = ap0 * baryCenter.x + ap1 * baryCenter.y + ap2 * baryCenter.z;

						float newX = interpUV.x * (vertBounds.z * 2) - vertBounds.z; //This will equal zero - NDH
						float newZ = interpUV.y * (vertBounds.w * 2) - vertBounds.w; //This will equal zero - NDH

						Vector3 myNewPos = new Vector3(-newX, 0f, -newZ);

						int layerMask = LayerMask.GetMask("cloth");
						RaycastHit hit;
						bool hit_miss = Physics.Raycast(myNewPos + new Vector3(0f, 20f, 0f), Vector3.down, out hit, Mathf.Infinity, layerMask);
						if (hit_miss)
						{
							Mesh hitMesh = (hit.collider as MeshCollider).sharedMesh;

							Vector3 p00 = hitMesh.vertices[hitMesh.triangles[hit.triangleIndex * 3 + 0]];
							Vector3 p10 = hitMesh.vertices[hitMesh.triangles[hit.triangleIndex * 3 + 1]];
							Vector3 p20 = hitMesh.vertices[hitMesh.triangles[hit.triangleIndex * 3 + 2]];

							Vector3 p0 = hit.collider.transform.TransformPoint(p00);
							Vector3 p1 = hit.collider.transform.TransformPoint(p10);
							Vector3 p2 = hit.collider.transform.TransformPoint(p20);

							float minDist = Vector3.Distance(hit.point, p0);
							//int minIndex = 0;
							//Vector3 selectedPoint = p0;
							//Vector3 selectedPointOrig = p00;
							int selIndex = 0;

							if (Vector3.Distance(hit.point, p1) < minDist)
							{
								minDist = Vector3.Distance(hit.point, p1);
								//minIndex = 1;
								//selectedPoint = p1;
								//selectedPointOrig = p10;
								selIndex = 1;
							}

							if (Vector3.Distance(hit.point, p2) < minDist)
							{
								minDist = Vector3.Distance(hit.point, p2);
								//minIndex = 2;
								//selectedPoint = p2;
								//selectedPointOrig = p20;
								selIndex = 2;
							}

							selIndex = hitMesh.triangles[hit.triangleIndex * 3 + selIndex];
							int selectedID = (int)hitMesh.normals[selIndex].x * 10000 + (int)hitMesh.normals[selIndex].y;
							tapID = selectedID;
						}

						CmdSendOSCTap(myID, tapID, transform.position, Mathf.Clamp(vel, 0.0f, 9999f));
						starTapTimer = 0.5f;
					}
				}

				if (OVRPlugin.GetSystemHeadsetType() == 0) GetComponentInChildren<controllerController>().SetTriggerRotation(SteamVR_Controller.Input((int)controlRSet.GetComponent<SteamVR_TrackedObject>().index).GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis1).x * -16f);
			}
		}

        sSwapper.UpdatePlayerPosition(transform.position);

    }

	/**
	<summary><sig>			FD : OnDestroy()
		Set myClothPlane to Object with tag "clothPlane"
		If it exists
			Create new CapsuleCollider array with length one less than myClothPlane colliders
			Make offset to 0
			For all capsuleColliders in myClothPlane
				If the collider is a collider in a child, Set offset to -1
				Else, set new collider list at i + offset to myClothPlane collider at i
			Set myClothPlane colliders to new collider list
		If v:isLocalPlayer is true
			if v:controlRSet is true
				If a child names "Model" exists, set it to true
			if v:lContObj is true
				If a child names "Model" exists, set it to true
	</sig></summary>
	**/
	void OnDestroy()
	{
		GameObject myClothPlane = GameObject.FindGameObjectWithTag("clothPlane");

		if (myClothPlane)
		{
			CapsuleCollider[] myCapColList = new CapsuleCollider[myClothPlane.GetComponent<Cloth>().capsuleColliders.Length - 1];
			int offset = 0;

			for (int i = 0; i < myClothPlane.GetComponent<Cloth>().capsuleColliders.Length; i++)
			{
				if (myClothPlane.GetComponent<Cloth>().capsuleColliders[i] == GetComponentInChildren<CapsuleCollider>()) offset = -1;
				else myCapColList[i + offset] = myClothPlane.GetComponent<Cloth>().capsuleColliders[i];
			}

			myClothPlane.GetComponent<Cloth>().capsuleColliders = myCapColList;
		}

		if (isLocalPlayer)
		{
			if (controlRSet)
			{
				Transform myChildModel = controlRSet.transform.Find("Model");
				if (myChildModel) myChildModel.gameObject.SetActive(true);
			}

			if (lContObj)
			{
				Transform myChildModel = lContObj.transform.Find("Model");
				if (myChildModel) myChildModel.gameObject.SetActive(true);
			}
		}
	}
	#endregion

	#region PRIVATE_FUNC


	/**
	<summary><sig>
		FD : OnChangePlayerID(int)
		Set v:myID to inID</sig>
	</summary><param name="inID">VD: Changed ID</param>
	**/
	void OnChangePlayerID(int oldID, int inID)
	{
		myID = inID;
	}

	/**
	<summary><sig>
		FD : CmdSendOSCTap(int, int, Vector3, float)
		Calls the OSCPacketBus component function newTapIDStart with all params and 999 as the angle
		<param name="inPlayer"/>
		<param name="inStar"/>
		<param name="inPos"/>
		<param name="inIntensity"/>
	</sig></summary>
	**/
	[Command]
	void CmdSendOSCTap(int inPlayer, int inStar, Vector3 inPos, float inIntensity)
	{
		oscPacketBus.GetComponent<OSCPacketBus>().newTapIDStart(inPlayer, false, (uint)inStar, inPos, inIntensity, 999);
	}
	#endregion
}
