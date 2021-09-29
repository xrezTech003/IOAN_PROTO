using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/**
<summary>
	CD : draggedSphereScriptNew
	Have object follow holdingController and set shader properties based on activation
</summary>
**/
public class draggedSphereScriptNew : NetworkBehaviour
{
	#region PUBLIC_ENUM
	/**
	<summary>
		ENUM : ActivationState
		Values : UNSET, ACTIVATED, DEACTIVATED
	</summary>
	**/
	public enum ActivationState { UNSET, ACTIVATED, DEACTIVATED };
	#endregion

	#region PUBLIC_VAR
	private bool doing = false;
	public OVRManager ovrm;
	/// <summary>
	///		VD : playerID
	///		ID of holding player
	/// </summary>
	[SyncVar(hook = "OnChangePlayerID")]
	public int playerID;

	/// <summary>
	///		VD : starID
	///		Star ID
	/// </summary>
	[SyncVar(hook = "OnChangeStarID")]
	public int starID;

	/// <summary>
	///		VD : starOrigin
	///		Original Star Position
	///		Only Defined
	/// </summary>
	[SyncVar(hook = "OnChangeStarOrigin")]
	public Vector3 starOrigin;

	/// <summary>
	///		VD : starPeriod
	/// </summary>
	[SyncVar(hook = "OnChangeStarPeriod")]
	public float starPeriod;

	/// <summary>
	///		VD : trueOrigin
	///		True Star Origin
	/// </summary>
	[SyncVar(hook = "OnChangeTrueOrigin")]
	public Vector3 trueOrigin;

	/// <summary>
	///		VD : activatedStarObj
	///		Activated Star prefab
	/// </summary>
	public GameObject activatedStarObj;

	/// <summary>
	///		VD : optionFlag
	///		Useless Variable
	/// </summary>
	[SyncVar]
	public int optionFlag = 0;

	/// <summary>
	///		VD : dbIP
	///		Database IP
	/// </summary>
	[SyncVar(hook = "OnChangedbIP")]
	public string dbIP;

	/// <summary>
	///		VD : dataColor
	///		Shader data
	/// </summary>
	[SyncVar]
	public Vector4 dataColor;

	/// <summary>
	///		VD : dataUV0
	///		Shader Data
	/// </summary>
	[SyncVar]
	public Vector4 dataUV0;

	/// <summary>
	///		VD : dataUV1
	///		Shader Data
	/// </summary>
	[SyncVar]
	public Vector4 dataUV1;

	/// <summary>
	///		VD : nonvariableTex
	/// </summary>
	public Texture nonvariableTex;

	/// <summary>
	///		VD : variableTex
	/// </summary>
	public Texture variableTex;

	/// <summary>
	///		VD : padStatus
	/// </summary>
	[SyncVar]
	public int padStatus;

	/// <summary>
	///		VD : paramVal
	/// </summary>
	[SyncVar]
	public float paramVal;

	/// <summary>
	///		VD : yOff
	/// </summary>
	[SyncVar]
	public float yOff;

	/// <summary>
	///		VD : isLeftController
	///		Boolean Toggle for handedness of holding controller
	/// </summary>
	[SyncVar]
	public bool isLeftController;

	/// <summary>
	///		VD : holdingController
	///		Holding Controller Object
	///		<remarks>
	///			I can't seem to figure out hos to get the local instance of the spawned object
	///			so instaead set the controller to null
	///			set isLeftController ot true or false
	///			then on update when you have authority and holdingController is null get the correct controller
	///		</remarks>
	/// </summary>
	public GameObject holdingController;

	/// <summary>
	///		VD : lerpDuration
	///		Duration for new Lerpers
	/// </summary>
	public float lerpDuration = .25f;

	/// <summary>
	///		VD : activationState
	/// </summary>
	[SyncVar]
	public ActivationState activationState = ActivationState.UNSET;

	/// <summary>
	///		VD : oldActivationState
	/// </summary>
	public ActivationState oldActivationState = ActivationState.UNSET;
	#endregion

	#region PRIVATE_VAR
	/// <summary>
	///		VD : startFlag
	///		Somewhat useless var
	/// </summary>
	private bool startFlag = false;

	/// <summary>
	///		VD : lerper
	///		Lerper for this script
	/// </summary>
	public Lerper lerper = new Lerper(.25f);

	/// <summary>
	///		VD : oscPacketBus
	///		OSCPacketBus for this script
	/// </summary>
	OSCPacketBus oscPacketBus;
	#endregion

	#region UNITY_FUNC
	/**
	<summary>
		FD : Start()
		Sets text to the star ID
		Sets all ShaderSetter values of child
		Creates new Lerper
	</summary>
	**/
	void Start()
	{
		ovrm = GameObject.FindGameObjectWithTag("OVRRig").GetComponent<OVRManager>();
		Transform childText = this.gameObject.transform.Find("IDText"); //Transform childText = transform.Find("IDText");
		childText.GetComponent<TextMesh>().text = starID.ToString();

		ShaderSetter ss = GetComponentInChildren<ShaderSetter>();

		dataColor = Clamper(dataColor);
		dataUV0 = Clamper(dataUV0);
		dataUV1 = Clamper(dataUV1);

		ss.blend = dataColor.x * (9f - -3f) + -3f; //* (12f) - 3f
		ss.poke = dataColor.z * (.4f - -.8f) + -.8f; //* (12f) - .8f
		ss.edgeWidth = dataUV1.z * (1.5f - .6f) + .6f; //* (7.5f) + .6f
		ss.noiseScale = dataUV1.x * (4f - 0f) + 0f; //* (4f)
		ss.interiorScale = dataUV0.x * (1.25f - 0.25f) + 0.25f; //* (1f) + .25f
		ss.exteriorScale = dataUV1.y * (5.75f - 2.75f) + 2.75f; //* (3f) + 2.75f
		ss.noiseWeight = dataUV0.w * (5f - 1f) + 1f; //* (4f) + 1f
													 //ss.drip = ((starOrigin.y / 0.28f) + 0.5f) * (4f - -4f) + -4f;
													 //ss.drip = ((origY / 0.28f) + 0.5f) * (4f - -4f) + -4f;
		ss.drip = ((yOff / 0.28f) + 0.5f) * (4f - -4f) + -4f; //* (8f) - 4f
		ss.speed = dataUV0.z * (1f - -1f) + -1f; //* (2f) - 1f
		ss.rimColor = Color.HSVToRGB(dataUV0.y, 1f, 1f);
		ss.rimExpo = dataColor.x * (1f - 10f) + 10f; //* (-9f) + 10f
		ss.gradColorA = Color.HSVToRGB(dataColor.y, 1f, 1f);
		ss.gradColorB = Color.HSVToRGB(1f, 0f, dataColor.w == 0f ? 0f : 1f);
		ss.animDuration = 2f;
		ss.gameObject.GetComponentInChildren<MeshRenderer>().material.mainTexture =
		dataColor.w == 1f ? nonvariableTex : variableTex;
		ss.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().material.mainTexture =
		dataColor.w == 1f ? nonvariableTex : variableTex;


		lerper = new Lerper(lerpDuration);
	}

	/**
	<summary>
		FD : Update()
		Toggles v:startFlag if false / Unneeded
		If null v:holdingController, Find GameObject with either "controlLeft" or "controlRight" tag and set lerper
		If v:holdingController isn't null
			If lerper time is greater than a second
				Set transform data
			Else
				Call updateGoal with v:holdingController
				Set transform data
			If Steam controller GetHairTriggerUp is true
				Spawn Star or destroy this GameObject
		Set ShadeSetter based on if above activation height
	</summary>
	**/
	void Update()
	{
		OVRInput.Update();
		if (hasAuthority)
		{
			if (!startFlag) startFlag = true; /// <remarks>CmdSendOSCSel (playerID, starID); TODO replace</remarks>

			if (holdingController == null)
			{
				if (isLeftController)
				{
					Debug.Log("In left hand");
					if (OVRPlugin.GetSystemHeadsetType() != 0) holdingController = GameObject.FindGameObjectWithTag("oculusLeft");
					else holdingController = GameObject.FindGameObjectWithTag("controlLeft");
				}
				else
				{
					Debug.Log("In right hand");
					if (OVRPlugin.GetSystemHeadsetType() != 0) holdingController = GameObject.FindGameObjectWithTag("oculusRight");
					else holdingController = GameObject.FindGameObjectWithTag("controlRight");
				}


				lerper.setStartTime(Time.time);
				lerper.setInterpPoints(transform.position, holdingController.transform.TransformPoint(new Vector3(0f, 0f, 0.1f)));
				lerper.setInterpRotations(transform.rotation, holdingController.transform.rotation);
			}

			if (holdingController)
			{
				if (lerper.update(Time.time) >= 1)
				{
					transform.position = holdingController.transform.TransformPoint(new Vector3(0f, 0f, 0.1f));
					transform.rotation = holdingController.transform.rotation;
				}
				else
				{
					lerper.updateGoal(holdingController.transform.TransformPoint(new Vector3(0f, 0f, 0.1f)), holdingController.transform.rotation);
					transform.position = lerper.getPoint();
					transform.rotation = lerper.getRotation();
				}
				bool steamClick = false;
				if (OVRPlugin.GetSystemHeadsetType() == 0) steamClick = SteamVR_Controller.Input((int)holdingController.GetComponent<SteamVR_TrackedObject>().index).GetHairTriggerUp();
				bool OVRReleaseLeft = false;
				bool OVRReleaseRight = false;
				OVRInput.Update();
				if (!OVRInput.Get(OVRInput.RawButton.LIndexTrigger)) { OVRReleaseLeft = true; Debug.Log("Left Trigger Release!"); }
				if (!OVRInput.Get(OVRInput.RawButton.RIndexTrigger)) { OVRReleaseRight = true; Debug.Log("Right Trigger Released!"); }
				//if (OVRInput.GetUp(OVRInput.RawButton.PrimaryIndexTrigger) || OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger) || steamClick)
				if (((OVRReleaseRight && !isLeftController) || (OVRReleaseLeft && isLeftController) || steamClick) && !doing)
				{
					doing = true;
					if (transform.position.y - trueOrigin.y > activatedStarScript.heightToActivateStar)
					{
						CmdSpawnActivatedStar(transform.position, trueOrigin, starID, playerID, dataColor, dataUV0, dataUV1, padStatus, paramVal, yOff, isLeftController);


						CmdDestroyMe(true, isLeftController);
					}
					else
					{
						CmdDestroyMe(false, isLeftController); // CmdSendOSCDiscard (playerID, starID); // TODO: replace
					}
				}
			}
		}

		bool isAboveActivationLine = transform.position.y - trueOrigin.y > activatedStarScript.heightToActivateStar;
		if (isAboveActivationLine)
		{
			if (hasAuthority) activationState = ActivationState.ACTIVATED;

			ShaderSetter cs = transform.Find("CelestialObject").GetComponent<ShaderSetter>();
			cs.gradColorA = cs.gradColorAOrig * 1.0f;
			cs.gradColorB = cs.gradColorBOrig * 1.0f;
			cs.edgeColor = cs.edgeColorOrig * 1.0f;
			cs.rimColor = cs.rimColorOrig * 1.0f;
			cs.exteriorScale = cs.exteriorScaleOrig * 1.0f;
			cs.interiorScale = cs.interiorScaleOrig * 1.0f;
		}
		else
		{
			if (hasAuthority) activationState = ActivationState.DEACTIVATED;

			ShaderSetter cs = transform.Find("CelestialObject").GetComponent<ShaderSetter>();
			float cMod = 0.3f;
			float sMod = 0.6f;
			cs.gradColorA = cs.gradColorAOrig * cMod;
			cs.gradColorB = cs.gradColorBOrig * cMod;
			cs.edgeColor = cs.edgeColorOrig * cMod;
			cs.rimColor = cs.rimColorOrig * 1.0f;
			cs.exteriorScale = cs.exteriorScaleOrig * (sMod * 0.8f);
			cs.interiorScale = cs.interiorScaleOrig * (sMod * 1.2f);
		}

		//if ((oldActivationState != activationState) && (oldActivationState != ActivationState.UNSET)) CmdSendCrossThreshMessage(activationState == ActivationState.ACTIVATED);

		oldActivationState = activationState;
	}
	#endregion

	#region PUBLIC_FUNC
	/**
	<summary>
		FD : getOSCPacketBus()
		Return oscPacketBus if already initialized
		Find GameObject with "OSCPacketBus" tag if not initialized
	</summary>
	**/
	public OSCPacketBus getOSCPacketBus()
	{
		if (oscPacketBus == null)
		{
			GameObject oscBus = GameObject.FindGameObjectWithTag("OSCPacketBus");

			if (oscBus != null) oscPacketBus = oscBus.GetComponent<OSCPacketBus>();
		}
		return oscPacketBus;
	}


	/**
	<summary>
		FD : grabStar(bool)
		<remarks>
			Dev Comments:
			the controller that is grabbing the object
			and the player netidentiry that is holding the controller
			should only be used by client taking controll
			I can't seem to figure out hos to get the local instance of the spawned object
			so instaead set the controller to null
			set isLeftController ot true or false
			then on update when NetworkIdentity have authority and holdingController is null get the correct controller
		</remarks>
		<param name="isLController">Bool if controller is left</param>
	</summary>
	**/
	public void grabStar(bool isLController, GameObject hController)
	{
		// moved setAuthority to player move
		isLeftController = isLController; //Remove this.
		holdingController = hController;
	}

	/**
	<summary>
		FD : CmdSendCrossThreshMessage(bool)
		Call DragAbove if isAbove is true
		Call DragBelow if not
		<param name="isAbove">Bool if above</param>
	</summary>
	**/
	[Command]
	public void CmdSendCrossThreshMessage(bool isAbove)
	{
		if (isAbove) getOSCPacketBus().dragAboveActivationThreshold(playerID, starID);
		else getOSCPacketBus().dragBelowActivationThreshold(playerID, starID);
	}
	#endregion

	#region PRIVATE_FUNC
	/**
	<summary>
		FD : OnChangedTrueOrigin(Vector3)
		Sets v:trueOrigin to param
		<param name="inTrueOrigin">Input TrueOrigin</param>
	</summary>
	**/
	void OnChangeTrueOrigin(Vector3 oldTrueOrigin, Vector3 inTrueOrigin)
	{
		trueOrigin = inTrueOrigin;
	}

	/**
	<summary>
		FD : OnChangeddbIP(string)
		Sets v:dbIP to param
		<param name="inIP">Input IP</param>
	</summary>
	**/
	void OnChangedbIP(string oldIP, string inIP)
	{
		dbIP = inIP;
	}

	/**
	<summary>
		FD : OnChangedPlayerID(int)
		Sets v:playerID to param
		<param name="playerID">Input playerID</param>
	</summary>
	**/
	void OnChangePlayerID(int oldID, int inID)
	{
		playerID = inID;
	}

	/**
	<summary>
		FD : OnChangedStarPeriod(float)
		Sets v:starPeriod to param
		<param name="inPeriod">Input Period</param>
	</summary>
	**/
	void OnChangeStarPeriod(float oldPeriod, float inPeriod)
	{
		starPeriod = inPeriod;
	}

	/**
	<summary>
		FD : OnChangeStarID(int)
		Sets v:starID to param
		<param name="newID">Input ID</param>
	</summary>
	**/
	void OnChangeStarID(int oldID, int newID)
	{
		starID = newID;
	}

	/**
	<summary>
		FD : OnChangedTrueOrigin(Vector3)
		Sets v:starOrigin to param
		<param name="newOrigin">Input StarOrigin</param>
	</summary>
	**/
	void OnChangeStarOrigin(Vector3 oldOrigin, Vector3 newOrigin)
	{
		starOrigin = newOrigin;
	}

	/**
	<summary>
		FD : rand(Vector2)
		Returns a random float based on param
		Take the dot product of param and const vector2
		Take sin of result
		Multiply it to const float
		Take result mod one
		<param name="co">Input Vector</param>
	</summary>
	**/
	float rand(Vector2 co)
	{
		return (Mathf.Sin(Vector2.Dot(co, new Vector2(12.9898f, 78.233f))) * 43758.5453f) % 1.0f;
	}

	/**
	<summary>
		FD : Clamper(Vector4)
		Clamp all vector components
		<param name="inData">Input Data</param>
	</summary>
	**/
	Vector4 Clamper(Vector4 inData)
	{
		for (int i = 0; i < 4; i++) inData[i] = Mathf.Clamp01(inData[i]);

		return inData;
	}

	/**
	<summary>
		FD : CmdSpawnActivatedStar(Vector3, Vector3, int, int, Vector4, Vector4, Vector4, int, float, float, bool)
		Creates new activated Star Obj and sets all values to params
		Give it Client authority
		Call CmdDestroyMe
	Last edited by Chris Poovey 07/10/20
		<param name="inPos"/>
		<param name="inOrigin"/>
		<param name="inID"/>
		<param name="inPlayer"/>
		<param name="inColor"/>
		<param name="inUV0"/>
		<param name="inUV1"/>
		<param name="inPadStat"/>
		<param name="inParamVal"/>
		<param name="inYOff"/>
		<param name="isLeft"/>
	</summary>
	**/
	[Command]
	void CmdSpawnActivatedStar(Vector3 inPos, Vector3 inOrigin, int inID, int inPlayer, Vector4 inColor, Vector4 inUV0, Vector4 inUV1, int inPadStat, float inParamVal, float inYOff, bool isLeft)
	{
		var myActivatedStar = Instantiate(activatedStarObj, inPos, Quaternion.identity);

		//Chris changed this from a bunch of get components to one... this will be more efficent.
		activatedStarScript activeStarScript = myActivatedStar.GetComponent<activatedStarScript>();
		activeStarScript.starOrigin = new Vector3(inPos.x, inPos.y, inPos.z); // give it the current location, treat starOrigin as a goal // = inPos
		activeStarScript.origPos = inOrigin;
		activeStarScript.starID = inID;
		activeStarScript.playerID = inPlayer;
		activeStarScript.starPeriod = starPeriod;
		activeStarScript.dbIP = dbIP;
		activeStarScript.dataColor = inColor;
		activeStarScript.dataUV0 = inUV0;
		activeStarScript.dataUV1 = inUV1;
		activeStarScript.padStatus = inPadStat;
		activeStarScript.paramVal = inParamVal;
		activeStarScript.yOff = inYOff;
		activeStarScript.newYOff = inOrigin.y;
		NetworkServer.Spawn(myActivatedStar, GetComponent<NetworkIdentity>().connectionToClient);
		// getOSCPacketBus().starActivated(playerID, starID, inPos, isLeft);
		//CmdDestroyMe(true, isLeft);

	}
	/// <summary>
	/// creates an audio event linked to the current dragged sphere. 
	/// Is on the dragged sphere script so that it activates on the server when the sphere is activated. If we want to sync clients we need to set up an rtc
	/// Author Chris Poovey
	/// Last edited by Chris Poovey 07/10/2020
	/// </summary>
	/// <param name="linkedObject">Object to link the position of the sonified voice to (current gameObject)</param>
	/// <param name="sphereScript">Sphere script with star data (this)</param>





	/**
	<summary>
		FD : CmdDestroyMe(bool, bool)
		If wasActivated is true
			Calls starActivated
		Else
			Calls dragDisregarded
		Destroy this gameObject on Server
		<param name="wasActivated">Bool of activation</param>
		<param name="isLeft">Bool of Left</param>
	</summary>
	**/
	[Command]
	void CmdDestroyMe(bool wasActivated, bool isLeft)
	{
		//if (wasActivated) getOSCPacketBus().starActivated(playerID, starID, transform.position, isLeft);
		//else getOSCPacketBus().dragDisgarded(playerID, starID, transform.position, isLeft);

		NetworkServer.Destroy(gameObject);
	}
	#endregion
}

