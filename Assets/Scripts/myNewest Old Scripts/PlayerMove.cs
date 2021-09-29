using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.IO;

//using UnityOSC;
/// <summary>
/// CD: Handles most of the client side behaviour by sharing it to the server and other players as necessary
///  ::: Makes abundant use of OSCPacketBus and has  areference to just about every object in the scene :::
///   IV: It's sorta bizarre though also understandable that this is called playermove, because it's also the left hand script?
///    ::: NDH ::: IV: Every single variable
///
/// Last edited by Chris Poovey to add tapping sound functionality to server 07/10/2020
/// </summary>
public class PlayerMove : NetworkBehaviour
{

	#region PUBLIC VARIABLES
	public uiSounds uiSounds;
	public IOANVoiceManager voiceManager;
	/// <summary>
	/// VD: I'm quite sure this is being called on highlightedIDScript but never actually set ::: IV: Adding to my doubt in said scripts necessity
	/// </summary>
	public int selectedID;
	//	public string starHighlightData = "";  I'm pretty sure this variable isn't being used for anything, just set and ignored
	/// <summary>
	/// VD: Set to zero in Start and used by the wave math in LateUpdate, but never changed from 0 ::: IV: Likely some kind of garbage
	/// </summary>
	public Vector3 hit2Pos;
	/// <summary>
	/// VD: Set to zero in Start but never used ::: IV: Likely garbage
	/// </summary>
	public Vector3 hit2PosOld;
	/// <summary>
	/// VD: Set to zero in Start and used by highlightedIDScript, but never changed from 0 ::: IV: Likely some kind of garbage
	/// </summary>
	public Vector3 curStarPosOld;
	/// <summary>
	/// VD: Set to zero in Start and used by highlightedIDScript, but never changed from 0 ::: IV: Likely some kind of garbage
	/// </summary>
	public Vector3 curStarPos;
	/// <summary>
	/// VD: Set to zero but never used ::: IV: Likely garbage
	/// </summary>
	public float rayTimer = 0f;
	/// <summary>
	/// VD: Set to zero in Start but never used ::: IV: Likely garbage
	/// </summary>
	public float controllerHeldCDTimer;
	/// <summary>
	/// VD: Set to zero and used by highlightedIDScript, but never changed from 0 ::: IV: Likely some kind of garbage
	/// </summary>
	public float oldYOffset = 0f;
	/// <summary>
	/// VD: Set to zero but never used ::: IV: Likely garbage
	/// </summary>
	public bool hit_miss = false;
	/// <summary>
	/// VD: Set to zero but never used ::: IV: Likely garbage
	/// </summary>
	public bool hit_miss2 = false;
	/// <summary>
	/// VD: Set to zero but never used ::: IV: Likely garbage
	/// </summary>
	public bool hit_miss3 = false;
	/// <summary>
	/// VD: Counter for keeping track of the number of capsule colliders attached to the cloth
	/// </summary>
	public int capsuleIndex = 0;
	/// <summary>
	/// VD: Database IP off the Config read
	/// </summary>
	public string dbIP;

	/// <summary>
	/// VD: Important, keeps track of which player is which
	/// </summary>
	[SyncVar(hook = "OnChangePlayerID")]
	public int myID;

	/// <summary>
	/// VD: Should be synced up with myID, keeps track of what colour each player should be dsplayed as
	/// </summary>
	[SyncVar(hook = "OnChangeColor")]
	public Color myColor;

	/// <summary>
	/// VD: Useless sync var, set to 0, never used
	/// </summary>
	[SyncVar(hook = "OnChangeSelStarPos")]
	public Vector3 selStarPos;

	/// <summary>
	/// VD: Useless SyncVar, set to 0, never used
	/// </summary>
	/*[SyncVar(hook = "OnChangeStarHeld")]
	public bool starHeld = false;*/

	/// <summary>
	/// VD: Location of this player's left controller
	/// </summary>
	[SyncVar]
	public Vector3 leftControllerTransformPostition;

	/// <summary>
	/// VD: Location of this player's right controller
	/// </summary>
	[SyncVar]
	public Vector3 rightControllerTransformPostition;

	/// <summary>
	/// VD: EGM and NDH cannot find a use for this, another useless SyncVar
	/// </summary>
	[SyncVar]
	public int optionFlag = 0;

	/// <summary>
	/// VD: Used by a dead function, also probably dead
	/// </summary>
	[SyncVar]
	public Color curStarColor;

	/// <summary>
	/// VD: Script/GameObject Reference: TapDetector
	/// </summary>
	TapDetector tapDetector;
	/// <summary>
	/// VD: Script/GameObject Reference: GrabDetector
	/// </summary>
	GrabDetector grabDetector;
	/// <summary>
	/// VD: Script/GameObject Reference: leftController (Tag)
	/// </summary>
	GameObject leftController;
	/// <summary>
	/// VD: Script/GameObject Reference: rightController (Tag)
	/// </summary>
	GameObject rightController;
	/// <summary>
	/// VD: Script/GameObject Reference: Initializer
	/// </summary>
	public GameObject initializerObj;
	/// <summary>
	/// VD: Script/GameObject Reference: HeadPrefab (Vive Headset) - Earnest public
	/// </summary>
	public GameObject head;
	/// <summary>
	/// VD: Script/GameObject Reference: RightHandController - Earnest public
	/// </summary>
	public GameObject netControlR;
	/// <summary>
	/// VD: Full garbage
	/// </summary>
	public GameObject seqObj;
	/// <summary>
	/// VD: Full, colorful, garbage
	/// </summary>
	public GameObject mySphere;
	/// <summary>
	/// VD: Script/GameObject Reference: starTextBox - Earnest public
	/// </summary>
	public GameObject sphereBlank;
	/// <summary>
	/// VD: True garbage
	/// </summary>
	public GameObject tapInfoDisplay;
	/// <summary>
	/// VD: Script/GameObject Reference: draggableStar - Earnest public
	/// </summary>
	public GameObject draggedSphereNew;
	/// <summary>
	/// VD: Potential garbage - Earnest Public - this whole system seems obscure and outdated
	/// </summary>
	public GameObject highlightedID;
	/// <summary>
	/// VD: This object is located but then not used, waste of space
	/// </summary>
	public GameObject dObj;
	/// <summary>
	/// VD: Shader reference for the lines, discussed in newUpdateCutoff - garbage
	/// </summary>
	public Shader additiveShader;
	/// <summary>
	/// VD: Partial public: Reference this Script's Objects's child, controllerModel object
	/// </summary>
	public GameObject childContModel;
	/// <summary>
	/// VD: Reference to NetworkCanvas Prefab, called by unused function - probably garbage
	/// </summary>
	public GameObject canvas;
	/// <summary>
	/// VD: Reference to bodyModel prefab - Just a sphere collider, not sure what it's importance is - get's instantiated, then what?
	/// </summary>
	public GameObject bodyModel;
	/// <summary>
	/// VD: Finally, important, reference to the OSCPacketBus
	/// </summary>
	private GameObject oscPacketBus;
	/// <summary>
	/// VD: True garbage - discussed in newUpdateCutoff
	/// </summary>
	public LineRenderer line1;
	/// <summary>
	/// VD: True garbage- discussed in newUpdateCutoff
	/// </summary>
	public LineRenderer line2;
	#endregion

	#region PRIVATE VARIABLES
	/// <summary>
	/// VD: Causes first frame of LateUpdate to recolour the left controller of the active client
	/// </summary>
	private bool controlModelStartFlag = true;
	/// <summary>
	/// VD: Causes first frame of LateUpdate to either disable the server's controllers, or populate the cloth mesh collider list with another capsule
	/// </summary>
	private bool contOffServerFlag = true;
	/// <summary>
	/// VD: Simple boolean to force this script to know where each controller is
	/// </summary>
	private bool controllerOn;
	/// <summary>
	/// VD: Time for LateUpdate to keep track of how fast it's allowed to spawn waves
	/// </summary>
	private float wavSpawnTimer;
	/// <summary>
	/// VD: Modifier to adjust the color of the right controller on spawn
	/// </summary>
	private float rContSkinModifier;
	/// <summary>
	/// VD: Garbagio
	/// </summary>
	protected FileInfo theSourceFile = null;


	/// <summary>
	/// Used to pass the generated sphere object to the drone
	/// </summary>
	private GameObject generatedSphere;
	#endregion

	#region MONOBEHAVIOUR FUNCTIONS

	/// <summary>
	/// FD: Prepare player colors and set most necessary variables and game objects to ther proper references ::: spawn player prefabs, activate OSCPacketBus (if server)
	/// </summary>
	void Start()
	{

		voiceManager = FindObjectOfType<IOANVoiceManager>(); // Finds the IOAN voice manager
		uiSounds = GetComponent<uiSounds>(); // Finds the UI sounds
		//uiSounds.localPlayerUI = isLocalPlayer;
		//uiSounds.playerID = myID;

		GameObject controllerSensor = GameObject.FindGameObjectWithTag("ControllerSensor");

		///<remarks>Set each c:sensorCam.isClient to true if c:PLayerMove isLocalLPayer</remarks>
		if (isLocalPlayer)
		{
			foreach (SensorCam sensorCam in controllerSensor.GetComponents<SensorCam>())
			{
				sensorCam.isClient = true;
			}
			//linkListener = FindObjectOfType<audioListenerLinker>();
			//linkListener.linkListener(head);
		}

		///<remarks>Set references to o:PLayerCube's c:TapDetector, c:GrabDetector, o:DataLoad(dOBJ Tag), o:OSCPacketBus, and o:Initializer(Tag Init)</remarks>
		tapDetector = controllerSensor.GetComponent<TapDetector>();
		grabDetector = controllerSensor.GetComponent<GrabDetector>();
		dObj = GameObject.FindGameObjectWithTag("dObj");
		oscPacketBus = GameObject.FindGameObjectWithTag("OSCPacketBus");
		initializerObj = GameObject.FindGameObjectWithTag("init");

		//		oldControllerTransformPos = transform.position;
		//	leftControllerTransformPostition
		//	rightControllerTransformPostition
		//frameTimer = 0.0f;
		//oldControllerPos = 0f;

		///<remarks>Initialise v:hit2Pos, v:hit2Pos, v:curStarPosOld, v:curStarPos, v:selStarPos, v:wavSpawnTimer, and v:controllerHeldCDTimer to 0's and v:rContSkinModifier to .5</remarks>
		hit2Pos = new Vector3(0f, 0f, 0f);
		hit2PosOld = new Vector3(0f, 0f, 0f);
		curStarPosOld = new Vector3(0f, 0f, 0f);
		curStarPos = new Vector3(0f, 0f, 0f);
		selStarPos = new Vector3(0f, 0f, 0f);
		wavSpawnTimer = 0f;
		controllerHeldCDTimer = 0f;
		rContSkinModifier = 0.5f;

		///<remarks>Set the colour of the controllers of the non-local players</remarks>
		if (!isLocalPlayer)
		{
			//Debug.Log ("islocalplayer check fail");
			childContModel.GetComponent<MeshRenderer>().material.color = myColor; //non-local player spawned on this machine after spawned on theirs, so colour should be carried over?
			childContModel.GetComponent<MeshRenderer>().enabled = true; // but also disables it? - NDH
			Transform[] myChildren = gameObject.GetComponentsInChildren<Transform>();
			///<remarks>Set the emission and colour values of specific components of the controller</remarks>

			foreach (Transform child in myChildren)
			{
				if (child.name == "frontring" || child.name == "handle")
				{
					foreach (MeshRenderer gcRend in child.GetComponentsInChildren<MeshRenderer>())
					{
						gcRend.material.SetColor("_EmissionColor", myColor);
					}
				}
				else if (child.name == "rims")
				{
					foreach (MeshRenderer gcRend in child.GetComponentsInChildren<MeshRenderer>())
					{
						if (gcRend.gameObject.name == "innerRim")
						{
							gcRend.material.SetColor("_Color", myColor * 0.8f);
						}
						else
						{
							gcRend.material.SetColor("_EmissionColor", myColor * 0.8f);
						}
					}
				}
			}

			if (controllerOn == true)
			{
				MeshRenderer tRenderer = leftController.transform.Find("ControllerSkin").GetComponent<MeshRenderer>();
				tRenderer.material.color = myColor;
				tRenderer.enabled = false;
				MeshRenderer rRenderer = rightController.transform.Find("ControllerSkin").GetComponent<MeshRenderer>();
				rRenderer.material.color = myColor * rContSkinModifier;
				rRenderer.enabled = false;
			}
		}
		///<remarks>   If the local player: </remarks>
		else
		{
			//Debug.Log ("islocalplayer check success");
			///<remarks>Activate OSCPacketBus if server</remarks>
			if (isServer)
			{
				//Debug.Log ("sending serverActivate");
				oscPacketBus.GetComponent<OSCPacketBus>().serverFlagActivate();
			}

			///<remarks>Get and check the config for Client/server status - Case insensitive</remarks>
			Config config = Config.Instance;
			int myID = config.Data.myID;

			if (config.Data.serverStatus == "server" || config.Data.serverStatus == "Server")
			{
				myID = 3;
			}
			//uiSounds.playerID = myID;

			dbIP = config.Data.dataBaseIP;//This line is kinda weird NDH

			///<remarks>Tell the server we are here</remarks>
			CmdSetID(myID);
			CmdActivatePlayer(myID);

			controllerOn = false; //Locally keep track of whether or not c:PlayerMove can see the controllers - NDH

			///<remarks>Set the color variable for the local player IV: Make these nex two bits a switch</remarks>
			curStarColor = new Color(1f, 1f, 1f);
			//myColor = new Color (Random.Range (0.25f, 1.0f), Random.Range (0.25f, 1.0f), Random.Range (0.25f, 1.0f));
			//myColor = Random.ColorHSV(0f,1f,1f,1f,1f,1f,1f,1f);
			if (myID % 3 == 0)
			{
				myColor = Color.HSVToRGB(0.975f, 1f, 1f);
			}
			else if (myID % 3 == 1)
			{
				myColor = Color.HSVToRGB(0.25f, 1f, 1f);
			}
			else if (myID % 3 == 2)
			{
				myColor = Color.HSVToRGB(0.75f, 1f, 1f);
			}

			Transform[] myChildren = gameObject.GetComponentsInChildren<Transform>();
			foreach (Transform child in myChildren)
			{
				if (child.name == "frontring" || child.name == "handle")
				{
					foreach (MeshRenderer gcRend in child.GetComponentsInChildren<MeshRenderer>())
					{
						gcRend.material.SetColor("_EmissionColor", myColor);
					}
				}
				else if (child.name == "rims")
				{
					foreach (MeshRenderer gcRend in child.GetComponentsInChildren<MeshRenderer>())
					{
						if (gcRend.gameObject.name == "innerRim")
						{
							gcRend.material.SetColor("_Color", myColor * 0.8f);
						}
						else
						{
							gcRend.material.SetColor("_EmissionColor", myColor * 0.8f);
						}
					}
				}
			}

			CmdInit(myColor);
			///<remarks>		If the player ID is within client range</remarks>
			if (0 <= myID && myID <= 2)
			{
				///<remarks>Spawn the head and body</remarks>
				CmdSpawnHead(myID);
				CmdSpawnBodyModel();
				//REVERT

				//REVERT

				///<remarks>Check to see if the controllers exist in c:this, then set them if not</remarks>
				if (GameObject.FindGameObjectWithTag("controlRight") && GameObject.FindGameObjectWithTag("controlLeft"))
				{
					controllerOn = true;
					rightController = GameObject.FindGameObjectWithTag("controlRight");
					leftController = GameObject.FindGameObjectWithTag("controlLeft");

				}
				///<remarks>Spawn the NetControlR (Name?) and the IDLabel</remarks>
				CmdSpawnNetControlR(myID, isServer, myColor * rContSkinModifier);
				//CmdSpawnCanvas (myID);
				//Color tColor = new Color (0f, 0f, 0f, 0f);

				childContModel.GetComponent<MeshRenderer>().enabled = true ;

				//CmdSpawnIDLabel(myID);
			}



		}
	}

	/// <summary>
	/// FD: Late update is useful here so that GrabDetector, and maybe others (NDH has not read all scripts) can do what they need to for this to operate properly
	///  ::: handles controller actions and passes information from local scripts to server
	/// </summary>
	void LateUpdate()
	{
		///<remarks>IV: GS: Remove? Starts with likely old testing script for draggedSphereScriptNew</remarks>
		if (Input.GetKeyDown("i"))
		{
			optionFlag = 0;
		}
		else if (Input.GetKeyDown("o"))
		{
			optionFlag = 1;
		}
		else if (Input.GetKeyDown("p"))
		{
			optionFlag = 2;
		}

		///<remarks>IV: GS: Another seemingly useless portion of script that disables the o:vive_controller if out of client Id range, even though the vive_controller prefab is never called or used</remarks>
		if (contOffServerFlag)
		{
			if (0 <= myID && myID <= 2)
			{
				newUpdateCutoff();
			}
			else
			{
				//transform.position = new Vector3 (99999f, 99999f, 99999f);
				foreach (Transform val in transform.Find("vive_controller").GetComponentsInChildren<Transform>())
				{
					foreach (MeshRenderer mr in val.GetComponentsInChildren<MeshRenderer>())
					{
						mr.enabled = false;
					}
				}
				foreach (MeshRenderer mr2 in GetComponentsInChildren<MeshRenderer>())
				{
					mr2.enabled = false;
				}


			}
			contOffServerFlag = false;
		}

		///<remarks>Stop LateUpdate if not local player IV: Combine these two bits</remarks>
		if (!isLocalPlayer)
		{
			return;
		}

		///<remarks>Stop LateUpdate if ID not in Client Range IV: Combine these two bits?</remarks>
		if (!(0 <= myID && myID <= 2))
		{
			return;
		}

		///<remarks>Doublecheck that the controllers are referenced</remarks>
		if (controllerOn == false)
		{
			if (GameObject.FindGameObjectWithTag("controlRight") && GameObject.FindGameObjectWithTag("controlLeft"))
			{
				controllerOn = true;
				rightController = GameObject.FindGameObjectWithTag("controlRight");
				leftController = GameObject.FindGameObjectWithTag("controlLeft");

			}
		}
		else
		{
			///<remarks>Run on first LateUpdate only ::: GS: Update client controller colour (for left only GS: Sems like SteamVR creates it's own model on the o:Model within left and right conroller's camerarig, and this script overrides that one with the 4-face togglable one we use)</remarks>
			if (controlModelStartFlag)
			{
				controlModelStartFlag = false;
				Transform myChildModel = leftController.transform.Find("Model");
				myChildModel.gameObject.SetActive(false);

				Transform[] myChildren = gameObject.GetComponentsInChildren<Transform>();
				foreach (Transform child in myChildren)
				{
					if (child.name == "frontring" || child.name == "handle")
					{
						foreach (MeshRenderer gcRend in child.GetComponentsInChildren<MeshRenderer>())
						{
							gcRend.material.SetColor("_EmissionColor", myColor);
						}
					}
					else if (child.name == "rims")
					{
						foreach (MeshRenderer gcRend in child.GetComponentsInChildren<MeshRenderer>())
						{
							if (gcRend.gameObject.name == "innerRim")
							{
								gcRend.material.SetColor("_Color", myColor * 0.8f);
							}
							else
							{
								gcRend.material.SetColor("_EmissionColor", myColor * 0.8f);
							}
						}
					}
				}

			}

			///<remarks>Update the left controller position</remarks>
			setLeftControllerPosition();

			///<remarks>Globaly update hand locaitons, Stolen from OW: these are put into syncVars so they can be used on the server easility</remarks>
			if (leftController)
			{
				leftControllerTransformPostition = leftController.transform.position;
			}
			if (rightController)
			{
				rightControllerTransformPostition = rightController.transform.position;
			}

			///<remarks>Check if c:Grabdetector found anything at the beginning of this frame an dupdate <list type="bullet"><header>If Active or Dragged Star</header><item>reference the o:star and it's Id</item><item>Call f:CmdSetAuthority and f:RpcRemvoeAuthority</item>
			///<item>Reference the c:activatedStarScript or c:draggedSphereScriptNew for the o:star</item><item>Tell the server we grabbed it, and with what hand (f:CmdSendGrabActiveStar)</item>
			///</list><list type="bullet"><header>If mesh star or tapped</header><item>f:spawnDraggedSphere</item><item>f:spawnTapInfoDisplay</item></list></remarks>
			if (grabDetector.LeftGrabbedActiveStar != null)
			{
				//	Debug.Log("left grabbed active star");
				GameObject grabbedStar = grabDetector.LeftGrabbedActiveStar;
				NetworkIdentity starNetID = grabbedStar.GetComponent<NetworkIdentity>();

				RpcRemoveAuthority(starNetID);
				CmdSetAuthority(starNetID);

				activatedStarScript starScript = grabbedStar.GetComponent<activatedStarScript>();
				starScript.grabStar(grabDetector.LeftController);
				CmdSendGrabActiveStar(myID, starScript.starID, leftControllerTransformPostition, true);
				//									oscPacketBus.GetComponent<OSCPacketBus> ().handOff(startScript.starID, true);
				uiSounds.handoffSound(leftController, myID); //trigger UI handoff sound


			}
			else if (grabDetector.LeftGrabbedStar != 0)
			{
				spawnDraggedSphere(grabDetector.LeftGrabbedStar, true);
				//osc call made by spawnDraggedSphere
				uiSounds.grabSound(leftController, myID); //trigger UI handoff sound

			}
			else if (tapDetector.leftTapID != 0)
			{

				/*float[] tapData = spawnTapInfoDisplay(tapDetector.leftTapLocation, tapDetector.leftTapID, myID, true);
				CmdPerformTap(tapDetector.leftTapLocation, tapDetector.leftTapID, tapData);*/


			}

			if (grabDetector.RightGrabbedActiveStar != null)
			{
				GameObject grabbedStar = grabDetector.RightGrabbedActiveStar;
				NetworkIdentity starNetID = grabbedStar.GetComponent<NetworkIdentity>();
				RpcRemoveAuthority(starNetID);
				CmdSetAuthority(starNetID);
				activatedStarScript startScript = grabbedStar.GetComponent<activatedStarScript>();
				startScript.grabStar(grabDetector.RightController);
				CmdSendGrabActiveStar(myID, startScript.starID, rightControllerTransformPostition, false);
				//									oscPacketBus.GetComponent<OSCPacketBus> ().handOff(startScript.starID, false);
				uiSounds.grabSound(leftController, myID); //trigger UI handoff sound
				uiSounds.handoffSound(rightController, myID); //trigger UI handoff sound


			}
			else if (grabDetector.RightGrabbedStar != 0)
			{
				//	Debug.Log("right grabbed  star");
				spawnDraggedSphere(grabDetector.RightGrabbedStar, false);
				//osc call made by spawnDraggedSphere
				uiSounds.grabSound(rightController, myID); //trigger UI handoff sound

			}
			else if (tapDetector.rightTapID != 0)
			{
				/*float[] tapData = spawnTapInfoDisplay(tapDetector.rightTapLocation, tapDetector.rightTapID, myID, false);
				CmdPerformTap(tapDetector.rightTapLocation, tapDetector.rightTapID, tapData);*/

			}

			///<remarks>calculate and spawn the wave, decrement clock on existing waves IV: Looks like it'd be happier as a separate function</remarks>
			#region WAVE
			wavSpawnTimer -= Time.deltaTime;


			SteamVR_Controller.Device tempContL = SteamVR_Controller.Input((int)leftController.GetComponent<SteamVR_TrackedObject>().index);
			SteamVR_Controller.Device tempContR = SteamVR_Controller.Input((int)rightController.GetComponent<SteamVR_TrackedObject>().index);
			float contDist = Vector3.Distance(tempContR.transform.pos, tempContL.transform.pos);
			if ((((tempContR.velocity.y < -1.75f && wavSpawnTimer <= 0f)) && (tempContL.velocity.y < -1.75f && contDist < 0.5f))
					 && Mathf.Abs(hit2Pos.y - (leftController.transform.position.y + rightController.transform.position.y) / 2f) < 0.25f)
			{
				CmdSpawnWave(rightController.transform.position, Mathf.Pow(Mathf.Abs(tempContL.velocity.y + tempContR.velocity.y) / 2f, 1.25f), GetComponentInChildren<controllerController>().padStatus);
				wavSpawnTimer = 0.4f;
			}
			#endregion
		}
	}

	/// <summary>
	/// FD: GS: Make  alist of all the capsule colliders? ::: Also reenable the SteamVR controllerModels
	/// </summary>
	void OnDestroy()
	{
		//Debug.Log ("L control " + capsuleIndex);
		GameObject myClothPlane = GameObject.FindGameObjectWithTag("clothPlane");
		if (myClothPlane)
		{

			// I think this is trying to put all the capilse colliders into a list except for the ones that are equal to the child
			List<CapsuleCollider> colliders = new List<CapsuleCollider>();
			CapsuleCollider referenceCC = GetComponentInChildren<CapsuleCollider>();
			foreach (CapsuleCollider cc in myClothPlane.GetComponent<Cloth>().capsuleColliders)
			{
				if (cc != referenceCC)
				{
					colliders.Add(cc);
				}
			}
			myClothPlane.GetComponent<Cloth>().capsuleColliders = colliders.ToArray();
			/*
			 * this code kept crashing while stoping app.  Not sure exactly what its trying to do but I think the above code is functionally equivalent and safer
					CapsuleCollider[] myCapColList = new CapsuleCollider[myClothPlane.GetComponent<Cloth> ().capsuleColliders.Length - 1];
					int offset = 0;
					if (myCapColList.Length > 1) {
						for (int i = 0; i < myClothPlane.GetComponent<Cloth> ().capsuleColliders.Length; i++) {
							if (myClothPlane.GetComponent<Cloth> ().capsuleColliders [i] == GetComponentInChildren<CapsuleCollider> ()) {
								offset = -1;
							} else {
								myCapColList [i + offset] = myClothPlane.GetComponent<Cloth> ().capsuleColliders [i];
							}
						}

						myClothPlane.GetComponent<Cloth> ().capsuleColliders = myCapColList;
					} */
		}
		if (isLocalPlayer)
		{
			if (leftController)
			{
				Transform myChildModel = leftController.transform.Find("Model");
				if (myChildModel)
				{
					myChildModel.gameObject.SetActive(true);
				}
			}
			if (rightController)
			{
				Transform myChildModel = rightController.transform.Find("Model");
				if (myChildModel)
				{
					myChildModel.gameObject.SetActive(true);
				}
			}

		}
	}

	#endregion

	#region PRIVATE FUNCTIONS

	/// <summary>
	/// FD: Read the list of capsule colliders within the cloth and add self to it ::: ALso spawns some gradient lines and sets some colour values to them, then zeros both vertices ::: Why though? ::: IV: I smell some nonsense here, or maybe I'm missing something
	/// </summary>
	void newUpdateCutoff()
	{
		GameObject myClothPlane = GameObject.FindGameObjectWithTag("clothPlane");
		capsuleIndex = myClothPlane.GetComponent<Cloth>().capsuleColliders.Length;
		CapsuleCollider[] myCapColList = new CapsuleCollider[capsuleIndex + 1];
		for (int i = 0; i < capsuleIndex; i++)
		{
			myCapColList[i] = myClothPlane.GetComponent<Cloth>().capsuleColliders[i];
		}

		myCapColList[capsuleIndex] = GetComponentInChildren<CapsuleCollider>();
		myClothPlane.GetComponent<Cloth>().capsuleColliders = myCapColList;

		line1 = (new GameObject("line")).AddComponent<LineRenderer>();
		line2 = (new GameObject("line")).AddComponent<LineRenderer>();
		line1.widthMultiplier = 0.0015f;

		line1.material = new Material(additiveShader);

		Gradient g = new Gradient();
		GradientColorKey[] gck = new GradientColorKey[2];
		gck[0].color = Color.white;
		gck[0].time = 0.0f;
		gck[1].color = Color.white;
		gck[1].time = 1.0f;
		GradientAlphaKey[] gak = new GradientAlphaKey[2];
		gak[0].alpha = 0.0f;
		gak[0].time = 0.0f;
		gak[1].alpha = 0.85f;
		gak[1].time = 1.0f;
		g.SetKeys(gck, gak);

		line1.colorGradient = g;
		line2.material = new Material(additiveShader);
		line2.colorGradient = g;
		line2.widthMultiplier = 0.002f;
		line1.positionCount = 2;
		line2.positionCount = 2;
		line1.SetPosition(0, new Vector3(0f, 0f, 0f));
		line1.SetPosition(1, new Vector3(0f, 0f, 0f));
		line2.SetPosition(0, new Vector3(0f, 0f, 0f));
		line2.SetPosition(1, new Vector3(0f, 0f, 0f));
	}

	/// <summary>
	/// FD: Handles the spawning of stars off the grid, looks up needed info from initializer charts and the loaded submesh/interactive submesh
	/// </summary>
	/// <param name="starID">VD: star detected by sensorcam</param>
	/// <param name="isLeftController">VD: Boolean represenation of which controller detected</param>
	void spawnDraggedSphere(uint starID, bool isLeftController)
	{

		Initializer init = initializerObj.GetComponent<Initializer>();
		int selIndex = init.getIndexForStarID(starID);
		GameObject interactiveSubMesh = init.InteractiveSubMesh;
		Mesh interactiveMesh = interactiveSubMesh.GetComponent<MeshFilter>().mesh;
		Vector3 inStarOrigin = interactiveSubMesh.transform.TransformPoint(interactiveMesh.vertices[selIndex]);


		Vector4 color = interactiveMesh.colors[selIndex];

		List<Vector4> uv0List = new List<Vector4>();
		List<Vector4> uv1List = new List<Vector4>();
		interactiveMesh.GetUVs(0, uv0List);
		interactiveMesh.GetUVs(1, uv1List);

		float period = uv0List[selIndex].z;
		int myPadStat = GetComponentInChildren<controllerController>().padStatus;

        float paramVal;
        switch(myPadStat)
        {
            case 0:
                paramVal = uv1List[selIndex].x;
                break;
            case 1:
                paramVal = uv0List[selIndex].w;
                break;
            case 2:
                paramVal = uv0List[selIndex].y;
                break;
            case 3:
                paramVal = uv1List[selIndex].y;
                break;
            default:
                paramVal = 0f;
                break;
        }

		if (isLeftController)
		{
			//	Debug.Log("spawnDraggedShpere:spawning left");
			CmdSpawnDraggedSphere(
				myID, inStarOrigin, (int)starID, period, dbIP,
				color, uv0List[selIndex], uv1List[selIndex], myPadStat, paramVal,
				inStarOrigin.y, inStarOrigin, leftController.transform.TransformPoint(new Vector3(0f, 0f, 0.1f)), true);
		}
		else
		{
			//	Debug.Log("spawnDraggedShpere:spawning right");
			CmdSpawnDraggedSphere(
				myID, inStarOrigin, (int)starID, period, dbIP,
				color, uv0List[selIndex], uv1List[selIndex], myPadStat, paramVal,
				inStarOrigin.y, inStarOrigin, rightController.transform.TransformPoint(new Vector3(0f, 0f, 0.1f)), false);
		}
	}

	/// <summary>
	/// FD: Hnadles the spawning of star data on taps :: reads star info from initializer and its' interactivesubmesh
	/// Last edited 07/10/2020 by Chris Poovey to add functionality to output tapping information for sonification
	/// </summary>
	/// <param name="tapLocation">VD: Where did we tap? IV: Unused</param>
	/// <param name="inID">VD: Id of the star detected by sensorcam</param>
	/// <param name="inPlayerID">VD: Who tapped it?</param>
	/// <param name="isLeft">VD: Was it their left hand?</param>
	/// <returns name="tapData">Tap data in the format of [starMag, tapVelocity, tapAngle, starType] </returns>
	float[] spawnTapInfoDisplay(Vector3 tapLocation, uint inID, int inPlayerID, bool isLeft)
	{


		GameObject[] tapList = GameObject.FindGameObjectsWithTag("TapInfoDisplay");
		foreach (GameObject o in tapList)
		{
			// don't re-create a display thats already there
			if (o.GetComponent<sphereBlankScript>().starID == inID)
			{
				return null;
			}
		}

		Initializer init = initializerObj.GetComponent<Initializer>();
		int selIndex = init.getIndexForStarID(inID);
		GameObject interactiveSubMesh = init.InteractiveSubMesh;
		Mesh interactiveMesh = interactiveSubMesh.GetComponent<MeshFilter>().mesh;
		Vector3 starLocation = interactiveSubMesh.transform.TransformPoint(interactiveMesh.vertices[selIndex]);

		//Tap Dat

		/*
		GameObject controller;
		if (isLeft) {
			if (!leftController) {
				leftController = GameObject.FindGameObjectWithTag ("controlLeft");
			}
			controller = leftController;

		} else {
			if (!rightController) {
				rightController = GameObject.FindGameObjectWithTag ("controlRight");
			}
			controller = rightController;
		} */

		Vector3 vel = getControllerDevice(leftController).velocity;
		float angle = Vector3.Angle(vel, Vector3.up);
		angle = (angle < 90) ? 90 - angle : 180 - angle;

		//Add tap data to an array
		float[] tapData = new float[4];
		tapData[0] = interactiveMesh.colors[selIndex].r;
		tapData[1] = vel.magnitude;
		tapData[2] = angle;
		tapData[3] = interactiveMesh.uv2[selIndex].y;

		CmdSpawnDraggedSphereBlank(starLocation, 0, inID, starLocation, inPlayerID, selIndex, isLeft, vel.magnitude, angle);
		return tapData;
	}

	/// <summary>
	/// FD: Checks if we have authority of the object and tells the server to take it from us
	/// </summary>
	/// <param name="identity">VD: What thing are we checking authority on, usually a star</param>
	void RpcRemoveAuthority(NetworkIdentity identity)
	{
		if (hasAuthority)
		{
			CmdRemoveAuthority(identity);
		}
	}

	/// <summary>
	/// FD: GS: Almost certainly stolen and modified from SteamVR so the left controller works as SteamVr should plus the 4-colourPad functionality we use for sorting
	/// </summary>
	void setLeftControllerPosition()
	{
		if (SteamVR_Controller.Input((int)leftController.GetComponent<SteamVR_TrackedObject>().index)
							.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
		{
			Vector2 touchpad = (SteamVR_Controller.Input((int)leftController.GetComponent<SteamVR_TrackedObject>().index)
								.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0));
			touchpad.Normalize();
			if (touchpad.y > 0.7071f)
			{
				//GetComponentInChildren<controllerController>().OnUpPress();
			}
			else if (touchpad.y < -0.7071f)
			{
				//GetComponentInChildren<controllerController>().OnDownPress();
			}

			if (touchpad.x > 0.7071f)
			{
				//GetComponentInChildren<controllerController>().OnRightPress();

			}
			else if (touchpad.x < -0.7071f)
			{
				//GetComponentInChildren<controllerController>().OnLeftPress();
			}
		}
		GetComponentInChildren<controllerController>().SetTriggerRotation(
			SteamVR_Controller.Input((int)leftController.GetComponent<SteamVR_TrackedObject>().index)
							.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis1).x * -16f);
		///<remarks>Sets the transforms on o:this because PlayerMove is attached to the left controller</remarks>
		transform.position = leftController.transform.position;
		Quaternion myEuler = leftController.transform.rotation;

		transform.rotation = myEuler;
	}

	/// <summary>
	/// FD: Get's the controller from SteamVR tracking
	/// </summary>
	/// <param name="getleftController">If true, get's left, if false, get's the right</param>
	/// <returns>A tracking index for the left or right controller</returns>
	SteamVR_Controller.Device getControllerDevice(bool getleftController)
	{
		if (getleftController)
		{
			return SteamVR_Controller.Input((int)leftController.GetComponent<SteamVR_TrackedObject>().index);
		}
		else
		{
			return SteamVR_Controller.Input((int)rightController.GetComponent<SteamVR_TrackedObject>().index);
		}

	}
	#endregion

	#region COMMANDS and RPC

	/// <summary>
	/// FD: Tells the server to spawn a highlighted Id (sorry, NDH is not sure what that is, it's not quite the same as a tap box)
	/// </summary>
	/// <param name="inPlayerID">VD: PlayerID fo the one doing tap</param>
	[Command]
	void CmdSpawnIDLabel(int inPlayerID)
	{
		GameObject myHighlightedID = (GameObject)Instantiate(highlightedID, transform.position, Quaternion.identity);
		myHighlightedID.GetComponent<highlightedIDScript>().myID = inPlayerID;
		NetworkServer.Spawn(myHighlightedID, connectionToClient);
	}

	/// <summary>
	/// FD: Tell the server who we are
	/// </summary>
	/// <param name="inID">VD: Who are we?</param>
	[Command]
	void CmdSetID(int inID)
	{
		//Debug.Log ("setid check success");
		myID = inID;
	}

	/// <summary>
	/// FD: Stolen from Eitan? : //This is called CmdSpawnDraggedSphereBlank but I'm pretty sure it creates the pop up displays on taps
	/// </summary>
	/// <param name="inStarOrigin">VD: The location of the star (also techincally it's database ID)</param>
	/// <param name="inStarYOffset">VD: Height the star is at</param>
	/// <param name="inID">VD: Star ID passed all the eay from c:tapDetector</param>
	/// <param name="inPos">VD: IV: The star location again?</param>
	/// <param name="inPlayerID">VD: My ID(0, 1, 2)</param>
	/// <param name="inSelIndex">VD: Looked up via starId from initializer IV: More reduncancy</param>
	/// <param name="isLeftController">VD: Left or right passed as a bool</param>
	/// <param name="intensity">VD: Strength of the tap</param>
	/// <param name="angle">VD: Angle the star was tapped at</param>
	[Command]
	void CmdSpawnDraggedSphereBlank(Vector3 inStarOrigin, float inStarYOffset, uint inID, Vector3 inPos, int inPlayerID, int inSelIndex, bool isLeftController, float intensity, float angle)
	{

		oscPacketBus.GetComponent<OSCPacketBus>().newTapIDStart(inPlayerID, isLeftController, inID, inStarOrigin, intensity, angle);

		GameObject[] tapList = GameObject.FindGameObjectsWithTag("sphereBlank");
		foreach (GameObject o in tapList)
		{
			if (o.GetComponent<sphereBlankScript>().starID == inID)
			{
				return;
			}
		}

		var myDraggedSphere = Instantiate(sphereBlank, inStarOrigin, Quaternion.identity); //TODO: Refactor to new starTextBox or something even better
																						   //myDraggedSphere.GetComponent<sphereBlankScript> ().starYOffset = inStarYOffset;
		myDraggedSphere.GetComponent<sphereBlankScript>().starYOffset = inStarOrigin.y;
		myDraggedSphere.GetComponent<sphereBlankScript>().starID = inID;
		myDraggedSphere.GetComponent<sphereBlankScript>().starPos = inPos;
		myDraggedSphere.GetComponent<sphereBlankScript>().playerID = inPlayerID;
		myDraggedSphere.GetComponent<sphereBlankScript>().selIndex = inSelIndex;
		NetworkServer.Spawn(myDraggedSphere, connectionToClient);
	}

	/// <summary>
	/// FD: Tells the server to spawn a wave on the mesh after slamming
	/// </summary>
	/// <param name="initPos">VD: Where did the slam start</param>
	/// <param name="intensity">VD: How hard was it slammed?</param>
	/// <param name="inPadStat">VD: What colour was the 4-colourPad when it happened?</param>
	[Command]
	void CmdSpawnWave(Vector3 initPos, float intensity, int inPadStat)
	{
		initializerObj.GetComponent<shadeSwapper>().SpawnWave(initPos, intensity, inPadStat);

		RpcSpawnWave(initPos, intensity, inPadStat);

		oscPacketBus.GetComponent<OSCPacketBus>().waveTriggerStart(new Vector2(initPos.x, initPos.z), intensity, inPadStat);
		if (isServerOnly)
		{
			voiceManager.performWave(inPadStat, initPos, intensity * 25);
		}
	}

	/// <summary>
	/// FD: Tell the server to give us a head model
	/// </summary>
	/// <param name="spawnID">VD: PlayerId, who is us?</param>
	[Command]
	void CmdSpawnHead(int spawnID)
	{
		var myHead = (GameObject)Instantiate(head, new Vector3(0f, 0f, 0f), Quaternion.identity);
		//myHead.GetComponentInChildren<MeshRenderer> ().material.SetColor ("_EmissionColor", myColor * Mathf.LinearToGammaSpace (1.0f));
		myHead.GetComponent<headScript>().myColor = myColor;
		myHead.GetComponent<headScript>().myID = spawnID;
		NetworkServer.Spawn(myHead, connectionToClient);
	}

	/// <summary>
	/// FD: Tell the server to give us a body model ::: Exclusively for collisions IV: Potentially depreciable
	/// </summary>
	/// <param name="spawnID">PlayerId, who is us?</param>
	[Command]
	void CmdSpawnBodyModel()
	{
		var myBodyModel = (GameObject)Instantiate(bodyModel, new Vector3(0f, 0f, 0f), Quaternion.identity);
		//myBodyModel.transform.SetParent (GameObject.FindGameObjectWithTag ("tracker1").transform);
		NetworkServer.Spawn(myBodyModel, connectionToClient);
	}

	/// <summary>
	/// Create a netControlR Script on o:elf, tell it who we are and what colour we are, then give NetControlRclientAuthority ::: Script contains useless if check
	/// </summary>
	/// <param name="spawnID">VD: Who are we?</param>
	/// <param name="serverFlag">VD: IV: useles boolean</param>
	/// <param name="argColor">VD: our Colour</param>
	[Command]
	void CmdSpawnNetControlR(int spawnID, bool serverFlag, Color argColor)
	{
		//Debug.Log (argColor);
		//Debug.LogError ("Color is " + argColor.ToString());
		var myNetControlR = (GameObject)Instantiate(netControlR, transform.position, transform.rotation);
		//myHead.GetComponentInChildren<MeshRenderer> ().material.SetColor ("_EmissionColor", myColor * Mathf.LinearToGammaSpace (1.0f));
		//myNetControlR.GetComponent<netControlR> ().myColor = myColor;
		myNetControlR.GetComponent<netControlR>().myID = spawnID;
		//Debug.Log ("spawn color " + argColor);
		myNetControlR.GetComponent<netControlR>().myColor = argColor;
		if (rightController)
		{
			myNetControlR.GetComponent<netControlR>().controlRSet = rightController;
		}
		if (serverFlag)
		{
			NetworkServer.Spawn(myNetControlR, connectionToClient);
		}
		else
		{
			NetworkServer.Spawn(myNetControlR, connectionToClient);
		}
	}

	/// <summary>
	/// CD: Tell the server what colour we are, and to tell everyone else what colour we are
	/// </summary>
	/// <param name="inColor">The colour to tell everyone about</param>
	[Command]
	void CmdInit(Color inColor)
	{

		if (!isServer)
		{
			return;
		}
		myColor = inColor;
		Transform[] myChildren = gameObject.GetComponentsInChildren<Transform>();
		foreach (Transform child in myChildren)
		{
			if (child.name == "frontring" || child.name == "handle")
			{
				foreach (MeshRenderer gcRend in child.GetComponentsInChildren<MeshRenderer>())
				{
					gcRend.material.SetColor("_EmissionColor", inColor);
				}
			}
			else if (child.name == "rims")
			{
				foreach (MeshRenderer gcRend in child.GetComponentsInChildren<MeshRenderer>())
				{
					if (gcRend.gameObject.name == "innerRim")
					{
						gcRend.material.SetColor("_Color", inColor * 0.8f);
					}
					else
					{
						gcRend.material.SetColor("_EmissionColor", inColor * 0.8f);
					}
				}
			}
		}
		//Color newColor = new Color (Random.Range (0.25f, 1.0f), Random.Range (0.25f, 1.0f), Random.Range (0.25f, 1.0f));
		RpcInit(inColor);
	}

	/// <summary>
	/// FD: Tell the Server to tell OSCPacket bus we are here
	/// </summary>
	/// <param name="inID">PLayer ID, who is we?</param>
	[Command]
	void CmdActivatePlayer(int inID)
	{
		oscPacketBus.GetComponent<OSCPacketBus>().activatePlayer(inID, this.gameObject, this.gameObject);
	}

	/// <summary>
	/// FD: Tell's the server we no longer wish to have authority over a thing
	/// </summary>
	/// <param name="identity">VD: The thing we no longer wish to have authority over</param>
	[Command]
	void CmdRemoveAuthority(NetworkIdentity identity)
	{
		NetworkConnection currentOwner = identity.connectionToClient;
		if (currentOwner != null)
		{
			identity.RemoveClientAuthority();
		}
	}

	/// <summary>
	/// FD: Tell OSCPacketBus we are holding an active star
	/// </summary>
	/// <param name="playerIndex">VD: Who grabbed it?</param>
	/// <param name="starID">VD: What star?</param>
	/// <param name="location">VD: Where were you when you grabbed it?</param>
	/// <param name="isLeft">VD: Was it your left hand?</param>
	[Command]
	public void CmdSendGrabActiveStar(int playerIndex, int starID, Vector3 location, bool isLeft)
	{
		oscPacketBus.GetComponent<OSCPacketBus>().activatedStarGrabbed(playerIndex, starID, location, isLeft);
	}

	/// <summary>
	/// FD: Tell the server Handles picking up a star off the grid
	/// </summary>
	/// <param name="inID">VD: Who picked it up?</param>
	/// <param name="inStarOrigin">VD: Origin vertex of the star to be picked up</param>
	/// <param name="inStarID">VD: ID of the star to be picked up IV: Slighty redundant with Origin(we have a lookup table?)</param>
	/// <param name="inPeriod">VD: Period of the star, stored as and pulled from the UV of the interactive submesh</param>
	/// <param name="inIP">VD: Database IP</param>
	/// <param name="inColor">VD: Colour of the star, stored as and pulled from the colour of the interactive</param>
	/// <param name="inUV0">VD: UV channel 2 of the star, stored as and pulled from the UV of the interactive submesh  IV: redundant?</param>
	/// <param name="inUV1">VD: UV Channel 1 of the star, stored as and pulled from the UV of the interactive submesh IV Redundant?</param>
	/// <param name="inPadStat">VD: Colour the 4-colourPad was when picked up</param>
	/// <param name="inParamVal">VD: Enumerated represenation of the pad colour</param>
	/// <param name="inYOff">VD: Y value of the starOrigin</param>
	/// <param name="inTrueOrigin">The starOrigin, again</param>
	/// <param name="controlerAttachLocation">VD: Controller's location plus a constant</param>
	/// <param name="isLeftController">VD: Bool to hold controller handedness</param>
	[Command]
	void CmdSpawnDraggedSphere(
		int inID, Vector3 inStarOrigin, int inStarID, float inPeriod, string inIP,
		Vector4 inColor, Vector4 inUV0, Vector4 inUV1, int inPadStat, float inParamVal,
		float inYOff, Vector3 inTrueOrigin, Vector3 controlerAttachLocation, bool isLeftController)
	{
		// this is what happens when you pick up a star
		GameObject[] activeList = GameObject.FindGameObjectsWithTag("activatedStar");
		int starID = inStarID;
		//Debug.Log (starID);
		foreach (GameObject o in activeList)
		{
			if (o.GetComponent<activatedStarScript>().starID == starID)
			{
				return;
			}
		}
		GameObject myDraggedSphere = null;

        /*
		myDraggedSphere = Instantiate(draggedSphereNew, controlerAttachLocation, Quaternion.identity);

		draggedSphereScriptNew sphereScript = myDraggedSphere.GetComponent<draggedSphereScriptNew>();
		sphereScript.playerID = inID;
		sphereScript.isLeftController = isLeftController;
		sphereScript.starID = starID;
		sphereScript.starOrigin = inStarOrigin;
		sphereScript.trueOrigin = inTrueOrigin;
		sphereScript.optionFlag = optionFlag;  // ununsed (I think) EGM   ::: Agreed - NDH
		sphereScript.starPeriod = inPeriod;
		sphereScript.dbIP = inIP;
		sphereScript.dataColor = inColor;
		sphereScript.dataUV0 = inUV0;
		sphereScript.dataUV1 = inUV1;
		sphereScript.padStatus = inPadStat;
		sphereScript.paramVal = inParamVal;
		sphereScript.yOff = inYOff;
		//		Debug.Log("Spawning " + myDraggedSphere);
		NetworkServer.Spawn(myDraggedSphere, connectionToClient);
        */

        var myActivatedStar = Instantiate(draggedSphereNew, controlerAttachLocation, Quaternion.identity);

        //Chris changed this from a bunch of get components to one... this will be more efficent.
        activatedStarScript activeStarScript = myActivatedStar.GetComponent<activatedStarScript>();
        activeStarScript.starOrigin = controlerAttachLocation; // give it the current location, treat starOrigin as a goal // = inPos
        activeStarScript.origPos = inStarOrigin;
        activeStarScript.starID = inStarID;
        activeStarScript.playerID = inID;
        activeStarScript.starPeriod = inPeriod;
        activeStarScript.dbIP = dbIP;
        activeStarScript.dataColor = inColor;
        activeStarScript.dataUV0 = inUV0;
        activeStarScript.dataUV1 = inUV1;
        activeStarScript.padStatus = inPadStat;
        activeStarScript.paramVal = inParamVal;
        activeStarScript.yOff = inYOff;
        activeStarScript.newYOff = inStarOrigin.y;
        if (isLeftController) activeStarScript.grabStar(grabDetector.LeftController);
        else activeStarScript.grabStar(grabDetector.RightController);
        NetworkServer.Spawn(myActivatedStar, GetComponent<NetworkIdentity>().connectionToClient);

        // does not need to be in a command because already in a command
        oscPacketBus.GetComponent<OSCPacketBus>().starGrabbed(inID, starID, controlerAttachLocation, isLeftController);
	}



	///<summary>
	/// FD: Tell the server to give the star to us
	///</summary><remarks>RpcRemoveAuthority should be called before calling CmdSetAuthority. OW/EGM?  Looks like this has been fixed NDH</remarks>
	[Command]
	void CmdSetAuthority(NetworkIdentity identity)
	{
		NetworkConnection currentOwner = identity.connectionToClient;
		if (currentOwner == connectionToClient) return;
		if (currentOwner != null)
		{
			Debug.LogError("RpcRemoveAuthority should be called before calling CmdSetAuthority (trying anyway)");
			identity.RemoveClientAuthority();
		}
		identity.AssignClientAuthority(connectionToClient);
	}

	/// <summary>
	/// FD: Server-half of spawning the slam-wave for all players
	/// </summary>
	/// <param name="initPos">VD: where did the player slam?/Where does the wave start?</param>
	/// <param name="intensity">VD: How hard was the slam?</param>
	/// <param name="inPadStat">VD: Colour of the pad when it happpened? Affects WaveParam in shaderspeak</param>
	[ClientRpc]
	void RpcSpawnWave(Vector3 initPos, float intensity, int inPadStat)
	{
		initializerObj.GetComponent<shadeSwapper>().SpawnWave(initPos, intensity, inPadStat);
		IOANVoiceManager voiceManager = FindObjectOfType<IOANVoiceManager>();
		voiceManager.performWave(inPadStat, initPos, intensity * 25);

	}

	/// <summary>
	/// FD: "Called" from a dead function, probably dead
	/// </summary>
	/// <param name="newCol"></param>
	[ClientRpc]
	void RpcCurStarColor(Color newCol)
	{
		curStarColor = newCol;
		mySphere.GetComponent<MeshRenderer>().material.color = new Color(newCol.r, newCol.g, newCol.b);
	}

	/// <summary>
	/// FD: Called from f:CmdInit - Server tells other players what colour to make this Players controllers IV: Combine with Head?
	/// </summary>
	/// <param name="newColor"></param>
	[ClientRpc]
	void RpcInit(Color newColor)
	{
		Transform[] myChildren = gameObject.GetComponentsInChildren<Transform>();
		foreach (Transform child in myChildren)
		{
			if (child.name == "frontring" || child.name == "handle")
			{
				foreach (MeshRenderer gcRend in child.GetComponentsInChildren<MeshRenderer>())
				{
					gcRend.material.SetColor("_EmissionColor", newColor);
				}
			}
			else if (child.name == "rims")
			{
				foreach (MeshRenderer gcRend in child.GetComponentsInChildren<MeshRenderer>())
				{
					if (gcRend.gameObject.name == "innerRim")
					{
						gcRend.material.SetColor("_Color", newColor * 0.8f);
					}
					else
					{
						gcRend.material.SetColor("_EmissionColor", newColor * 0.8f);
					}
				}
			}
		}
	}
	#endregion

	#region HOOKS

	// <summary>
	// FD: Hook for SyncVar starHeld
	// </summary>
	//<param name="inStarHeld">VD: Value to be updated to </param>
	/*void OnChangeStarHeld(bool oldStarHeld, bool inStarHeld)
	{
		starHeld = inStarHeld;
	}*/
	/// <summary>
	/// FD: Hook for SyncVar PLayerID
	/// </summary>
	/// <param name="newID">VD: ID to be changed to, unless player is server</param>
	void OnChangePlayerID(int oldID, int newID)
	{
		if (!isServer)
		{
			myID = newID;
			//uiSounds.playerID = newID;
		}
		//if (isServer) {
		//	oscPacketBus.GetComponent<OSCPacketBus> ().activatePlayer (newID, gameObject);
		//}
	}
	/// <summary>
	/// FD: Hook for SyncVar myColor ::: GS: looks like it used to skip the local player, now doesn't
	/// </summary>
	/// <param name="myColor">VD: Colour to be changed to</param>
	void OnChangeColor(Color oldColor, Color newColor)
	{
		myColor = newColor;

		childContModel.GetComponent<MeshRenderer>().material.color = myColor;

		Transform[] myChildren = gameObject.GetComponentsInChildren<Transform>();
		///<remarks>Set the emission and colour values of specific components of the controller</remarks>

		foreach (Transform child in myChildren)
		{
			if (child.name == "frontring" || child.name == "handle")
			{
				foreach (MeshRenderer gcRend in child.GetComponentsInChildren<MeshRenderer>())
				{
					gcRend.material.SetColor("_EmissionColor", myColor);
				}
			}
			else if (child.name == "rims")
			{
				foreach (MeshRenderer gcRend in child.GetComponentsInChildren<MeshRenderer>())
				{
					if (gcRend.gameObject.name == "innerRim")
					{
						gcRend.material.SetColor("_Color", myColor * 0.8f);
					}
					else
					{
						gcRend.material.SetColor("_EmissionColor", myColor * 0.8f);
					}
				}
			}
		}

		if (controllerOn == true)
		{
			MeshRenderer tRenderer = leftController.transform.Find("ControllerSkin").GetComponent<MeshRenderer>();
			tRenderer.material.color = myColor;
			tRenderer.enabled = false;
			MeshRenderer rRenderer = rightController.transform.Find("ControllerSkin").GetComponent<MeshRenderer>();
			rRenderer.material.color = myColor * rContSkinModifier;
			rRenderer.enabled = false;
		}

	}
	/// <summary>
	/// FD: Hook for SyncVar selStarPos
	/// </summary>
	/// <param name="newPos">VD: position to be updated to</param>
	void OnChangeSelStarPos(Vector3 oldPos, Vector3 newPos)
	{
		selStarPos = newPos;
	}

	#endregion

	#region A Few removes prior to v3 and unused
	/*
	[Command]
	void CmdSetActiveStarAuthority (NetworkInstanceId activeStarNetID, NetworkIdentity playerID) {
		GameObject activeStarOjb = ClientScene.FindLocalObject(activeStarNetID);
		activeStarOjb.GetComponent<activatedStarScript>().grantAuthority(playerID);
//		RpcSetActiveStarAuthority(activeStarNetID, playerID);
	}

/*
	[ClientRpc]
	void RpcSetActiveStarAuthority (NetworkInstanceId activeStarNetID, NetworkIdentity playerID) {
		GameObject activeStarOjb = ClientScene.FindLocalObject(activeStarNetID);
		activeStarOjb.GetComponent<activatedStarScript>().grantAuthority(playerID);
	}
*/

	/*
		[ClientRpc]
		void RpcSpawnDraggedSphere(
			int inID, Vector3 inStarOrigin, int inStarID, float inPeriod, string inIP,
			Vector4 inColor, Vector4 inUV0, Vector4 inUV1, int inPadStat, float inParamVal,
			float inYOff, Vector3 inTrueOrigin, Vector3 controlerAttachLocation, bool isLeftController) {

			if(hasAuthority) {
				CmdSpawnDraggedSphere(
				 inID,  inStarOrigin,  inStarID,  inPeriod,  inIP,
				 inColor,  inUV0,  inUV1,  inPadStat,  inParamVal,
				 inYOff,  inTrueOrigin,  controlerAttachLocation,  isLeftController);
				}


		}
	*/

/*	[Command]
*//*	void CmdSendOSCTap(int inPlayer, bool isLeftController, uint inStar, Vector3 inPos, float inIntensity, float angle)
	{
		oscPacketBus.GetComponent<OSCPacketBus>().newTapIDStart(inPlayer, isLeftController, inStar, inPos, inIntensity, angle);

	}*//*

	[Command]*/
	void CmdSpawnCanvas(int inID)
	{
		var myCanvas = (GameObject)Instantiate(canvas, transform.position, transform.rotation);
		//myCanvas.transform.parent = transform;
		//myCanvas.transform.SetParent(gameObject.transform);
		//myCanvas.GetComponent<canvasNewNet>().playerRef = gameObject;
		myCanvas.GetComponent<canvasNewNet>().graphImgID = Random.Range(0, 16);
		myCanvas.GetComponent<canvasNewNet>().playerID = inID;
		if (rightController)
		{
			myCanvas.GetComponent<canvasNewNet>().rContObj = rightController;
		}
		if (leftController)
		{
			myCanvas.GetComponent<canvasNewNet>().lContObj = leftController;
		}
		NetworkServer.Spawn(myCanvas, connectionToClient);
	}
	/// <summary>
	/// Sends a command to the server to perform a tap.  If we want client side interaction we can add a rtc here.
	/// </summary>
	/// <param name="tapLocation">position of the tap</param>
	/// <param name="tapID">star ID</param>
	/// <param name="tapData">array of data needed to make the tap</param>
/*	[Command]

	void CmdPerformTap(Vector3 tapLocation, uint tapID, float[] tapData)
	{
		if (isServerOnly)
		{
			voiceManager.performTap(tapID, tapData[0], tapData[1], tapData[2], (int)tapData[3], tapData[4], tapLocation);
		}
		RpcPerformTap(tapLocation, tapID, tapData);
	}

	[ClientRpc]
	void RpcPerformTap(Vector3 tapLocation, uint tapID, float[] tapData)
	{
		IOANVoiceManager voiceManager = FindObjectOfType<IOANVoiceManager>();
		voiceManager.performTap(tapID, tapData[0], tapData[1], tapData[2], (int)tapData[3], tapData[4], tapLocation);
	}

	[Command]*/
	void CmdCurStarColor(Color newCol)
	{
		RpcCurStarColor(newCol);
	}

	[Command]
	public void CmdSendGrabDragStar(int playerIndex, int starID, Vector3 location, bool isLeft)
	{
		oscPacketBus.GetComponent<OSCPacketBus>().activatedStarGrabbed(playerIndex, starID, location, isLeft);
	}


	#endregion
}
