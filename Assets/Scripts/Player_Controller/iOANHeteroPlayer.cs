using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.IO;
using UnityEngine.XR;

/// <summary>
/// CD: Attached to the top game object for the spawned in player, handles all command and rpc messages for each player, as well as color setting for the player as a whole
/// </summary>
public class iOANHeteroPlayer : NetworkBehaviour
{
	/// <summary>
	/// VD: Sync variable to tell the server if we are ovr or not ::: IV: Will gain more usage on full-hetero update
	/// </summary>
	[SyncVar]
	public bool isOVR;
	/// <summary>
	/// VD: Syncvar read from local player and updated through cmd,then transmitted out to other players
	/// </summary>
	[SyncVar]
	public iOANPlayerUtil.playerID playerID;
	/// <summary>
	/// VD: Syncvar read from controllers, passed up through commands
	/// </summary>
	[SyncVar]
	public iOANPlayerUtil.dialID dialID;
	/// <summary>
	/// VD: Inspector reference to left controller child
	/// </summary>
    public GameObject LeftController;
	/// <summary>
	/// VD: Inspector reference to right controller child
	/// </summary>
    public GameObject RightController;
	/// <summary>
	/// VD: Inspector reference to HMD child
	/// </summary>
	public GameObject HMD;
	/// <summary>
	/// VD: Inspector reference to UI sounds object
	/// </summary>
	public uiSounds uiSounds;
	/// <summary>
	/// VD: myColor is set in the same command wherein the local player updates it's ID, server then sets the colour variable via sync, and runs the hook to change the actual colours of the player on each client and then self
	/// </summary>
	[SyncVar(hook = nameof(OnChangeColor))]
	public Color myColor;
	/// <summary>
	/// VD: Timer for spawning waves, used to prevent spawning waves too close together
	/// </summary>
    private float wavSpawnTimer = 0f;
	/// <summary>
	/// VD: Grabbed from config, used to lookup star data for wave colour manipulation
	/// </summary>
	public string dbIP;
	/// <summary>
	/// VD: Start set reference to the initializer, used for star data lookup
	/// </summary>
	public GameObject initializerObj;
	/// <summary>
	/// VD: Start set reference to the wwise voice manage
	/// </summary>
	private IOANVoiceManager voiceManager;
	/// <summary>
	/// VD: Inspector reference to spawnable star and text box objects
	/// </summary>
	public GameObject starTextBox, starObject, highlightIDText;
	/// <summary>
	/// ZT: animation curve for the wave haptic strength over time
	/// </summary>
	public AnimationCurve waveStrength = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1), });
	/// <summary>
	/// ZT: checks if wave haptic is happening
	/// </summary>
	public bool hapticWaving = false;

	//private bool nonlocal_color_flag = true; //DEBUG remove if 10/9/2020 Color changes work
	/// <summary>
	/// FD: Set's local non-inspector-friendly references and enables sensorcams if local
	/// </summary>
	void Start()
    {
		/*if (!isLocalPlayer)
        {
			initializerObj = GameObject.FindGameObjectWithTag("init");
			voiceManager = FindObjectOfType<IOANVoiceManager>();
			myColor = iOANPlayerUtil.playerColor[playerID];
            clientSetColor();
            uiSounds.playerID = (int)playerID;
			//Debug.Log("I AM STARTING AS NONLOCAL PLAYER");
            return;
        }*/
		Config config = Config.Instance;
		uiSounds.playerID = (int)playerID;
		dbIP = config.Data.dataBaseIP;
		if (isLocalPlayer)
        {
            HMD.transform.Find("vive_headsetnew").gameObject.GetComponent<MeshRenderer>().enabled = false;
            LeftController.GetComponent<iOANHeteroController>().sensorCam.isClient = true;
            RightController.GetComponent<iOANHeteroController>().sensorCam.isClient = true;
			playerID = (iOANPlayerUtil.playerID) config.Data.myID;
			CmdSetID((int)playerID); //This also sets the color top-down
        }
        if (OVRPlugin.GetSystemHeadsetType() != 0) isOVR = true;
        
        initializerObj = GameObject.FindGameObjectWithTag("init");
        voiceManager = FindObjectOfType<IOANVoiceManager>();

        //gameObject.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
        //clientSetColor();
    }
	/// <summary>
	/// FD: Returns if not local, otherwise, reads velocity from Left and Right Controller and spawns waves if cased correctly
	/// </summary>
    void LateUpdate()
    {
        if (!isLocalPlayer)
        {
			/*if(nonlocal_color_flag)
            {	
				myColor = iOANPlayerUtil.playerColor[playerID];
				//if (colorTestRef.material.GetColor("_Color") != myColor)  StartCoroutine(nonlocalSetColor());
				
				//clientSetColor();
				//color_remote_set = false;
			}*/ //DEBUG remove if 10/9/2020 Color changes work
			return;
		}
			

		wavSpawnTimer -= Time.deltaTime;
        if (LeftController.GetComponent<iOANHeteroController>().velocity < -1.75f && RightController.GetComponent<iOANHeteroController>().velocity < -1.75f && wavSpawnTimer <= 0 && Mathf.Abs((LeftController.transform.position.y + RightController.transform.position.y) / 2f) < 0.25f)
        {
            CmdSpawnWave(RightController.transform.position, Mathf.Pow(Mathf.Abs(RightController.GetComponent<iOANHeteroController>().velocity + LeftController.GetComponent<iOANHeteroController>().velocity) / 2f, 1.25f), (int)dialID);

			//if haptics are happening, reset haptic, otherwise start new haptic
			if (hapticWaving)
            {
				//RightController.GetComponent<iOANHeteroController>().stopWaveHaptic(25, 0.05f, 0.025f, 1.0f, 0.001f);
				//LeftController.GetComponent<iOANHeteroController>().stopWaveHaptic(25, 0.05f, 0.025f, 1.0f, 0.001f);
			}
			hapticWaving = true;
			RightController.GetComponent<iOANHeteroController>().performWaveHaptic(10, 0.025f, 0.005f, 1.0f, 0.001f);
			LeftController.GetComponent<iOANHeteroController>().performWaveHaptic(10, 0.025f, 0.005f, 1.0f, 0.001f);
			
			wavSpawnTimer = 1f; //set wave timer
        }
    }

	/// <summary>
	/// FD: Hook for SyncVar myColor ::: Runs on clients only by default
	/// </summary>
	/// <param name="myColor">VD: Colour to be changed to</param>
	void OnChangeColor(Color oldColor, Color newColor)
	{
		myColor = iOANPlayerUtil.playerColor[playerID];
		GameObject[] glows = GameObject.FindGameObjectsWithTag("Glow");
			foreach (GameObject glow in glows)
			{
				if (glow.transform.IsChildOf(this.transform))
				{
					foreach (MeshRenderer gcRend in glow.GetComponentsInChildren<MeshRenderer>())
					{
						gcRend.material.SetColor("_Color", newColor * .8f);
						gcRend.material.SetColor("_EmissionColor", newColor * .8f);
					}
				}
			}

			GameObject[] emits = GameObject.FindGameObjectsWithTag("GlowEmit");
			foreach (GameObject emit in emits)
			{
				if (emit.transform.IsChildOf(this.transform))
				{
					foreach (MeshRenderer gcRend in emit.GetComponentsInChildren<MeshRenderer>())
					{
						gcRend.material.SetColor("_EmissionColor", (emit.name == "vive_headsetnew") ? newColor * Mathf.LinearToGammaSpace(1.0f) : newColor);
					}
				}
			}
	}
	/// <summary>
	/// FD: Server partner function for OnChangeColor ::: Runs on server only by layout
	/// </summary>
	/// <param name="myColor">VD: Colour to be changed to</param>
	public IEnumerator serverSetColor()
	{
		yield return new WaitForEndOfFrame();

		GameObject[] glows = GameObject.FindGameObjectsWithTag("Glow");
		foreach (GameObject glow in glows)
		{
			if (glow.transform.IsChildOf(transform))
			{
				foreach (MeshRenderer gcRend in glow.GetComponentsInChildren<MeshRenderer>())
				{
					gcRend.material.SetColor("_Color", myColor * .8f);
					gcRend.material.SetColor("_EmissionColor", myColor * .8f);
				}
			}
		}

		GameObject[] emits = GameObject.FindGameObjectsWithTag("GlowEmit");
		foreach (GameObject emit in emits)
			if (emit.transform.IsChildOf(transform))
				foreach (MeshRenderer gcRend in emit.GetComponentsInChildren<MeshRenderer>())
					gcRend.material.SetColor("_EmissionColor", (emit.name == "vive_headsetnew") ? myColor * Mathf.LinearToGammaSpace(1.0f) : myColor);
	}
	/// <summary>
	/// FD: Checks if we have authority of the object and tells the server to take it from us
	/// </summary>
	/// <param name="identity">VD: What thing are we checking authority on, usually a star</param>
	public void CliRemoveAuthority(NetworkIdentity identity)
	{
		if (hasAuthority) CmdRemoveAuthority(identity);
	}
	/// <summary>
	/// FD: Client to server command to lineup line pulses across users
	/// </summary>
	/// <param name="starID">uUsed by pulse to display proper text data</param>
	[Command]
    public void CmdSpawnPulse(uint starID)
    {
        RpcSpawnPulse(starID);

        LinesManager lines = GameObject.Find("lineHolder").GetComponent<LinesManager>();
        lines.SpawnPulses(Color.white, false);
    }

    [Command]
    public void CmdUpdateHighlightPos(int inID)
    {
        RpcUpdateHighlightPos(inID);
        GameObject.FindGameObjectWithTag("init").GetComponent<shadeSwapper>().UpdateHighlightPos(inID);
    }

	/// <summary>
	/// FD: Tells the server to spawn a wave on the mesh after slamming
	/// </summary>
	/// <param name="initPos">VD: Where did the slam start</param>
	/// <param name="intensity">VD: How hard was it slammed?</param>
	/// <param name="inPadStat">VD: What colour was the 4-colourPad when it happened?</param>
	[Command]
	public void CmdSpawnWave(Vector3 initPos, float intensity, int inPadStat)
	{
		initializerObj.GetComponent<shadeSwapper>().SpawnWave(initPos, intensity, inPadStat);

		RpcSpawnWave(initPos, intensity, inPadStat);

		//oscPacketBus.GetComponent<OSCPacketBus>().waveTriggerStart(new Vector2(initPos.x, initPos.z), intensity, inPadStat);
		if (isServerOnly) voiceManager.performWave(inPadStat, initPos, intensity * 25);
	}
	/// <summary>
	/// FD: called by clients to tell the server what ID their configs tell them to have ::: Updates color per player which calls hook, then sets the color on the server
	/// </summary>
	/// <param name="inID">passed by the client to the server, int cast of playerID enumerator as saved in local config</param>
	[Command]
	public void CmdSetID(int inID)
	{
		//Debug.Log ("setid check success");
		playerID = (iOANPlayerUtil.playerID)inID;
		myColor = iOANPlayerUtil.playerColor[playerID];
		StartCoroutine(serverSetColor());
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
	public void CmdSpawnDraggedSphereBlank(Vector3 inStarOrigin, float inStarYOffset, uint inID, Vector3 inPos, int inPlayerID, int inSelIndex, bool isLeftController, float intensity, float angle)
	{

		//oscPacketBus.GetComponent<OSCPacketBus>().newTapIDStart(inPlayerID, isLeftController, inID, inStarOrigin, intensity, angle);

		GameObject[] tapList = GameObject.FindGameObjectsWithTag("sphereBlank");
		foreach (GameObject o in tapList)
			if (o.GetComponent<sphereBlankScript>().starID == inID)
				return;

		//var myDraggedSphere = Instantiate(starTextBox, inStarOrigin, Quaternion.identity); //TODO: Refactor to new starTextBox or something even better
		GameObject myDraggedSphere = Instantiate(starTextBox, inStarOrigin, Quaternion.identity);                                                             //myDraggedSphere.GetComponent<sphereBlankScript> ().starYOffset = inStarYOffset;
        myDraggedSphere.GetComponent<sphereBlankScript>().starYOffset = inStarOrigin.y;
		myDraggedSphere.GetComponent<sphereBlankScript>().starID = inID;
		myDraggedSphere.GetComponent<sphereBlankScript>().starPos = inPos;
		myDraggedSphere.GetComponent<sphereBlankScript>().playerID = inPlayerID;
		myDraggedSphere.GetComponent<sphereBlankScript>().selIndex = inSelIndex;
		NetworkServer.Spawn(myDraggedSphere, connectionToClient);
	}
	/// <summary>
	/// FD: Tell the Server to tell OSCPacket bus we are here
	/// </summary>
	/// <param name="inID">PLayer ID, who is we?</param>
	[Command]
	public void CmdActivatePlayer(int inID)
	{
		//oscPacketBus.GetComponent<OSCPacketBus>().activatePlayer(inID, this.gameObject, this.gameObject);
	}
	/// <summary>
	/// FD: Tell's the server we no longer wish to have authority over a thing
	/// </summary>
	/// <param name="identity">VD: The thing we no longer wish to have authority over</param>
	[Command]
	public void CmdRemoveAuthority(NetworkIdentity identity)
	{
		NetworkConnection currentOwner = identity.connectionToClient;
		if (currentOwner != null) identity.RemoveClientAuthority();
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
		//oscPacketBus.GetComponent<OSCPacketBus>().activatedStarGrabbed(playerIndex, starID, location, isLeft);
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
	public void CmdSpawnDraggedSphere(int inID, Vector3 inStarOrigin, int inStarID, float inPeriod, string inIP,
									  Vector4 inColor, Vector4 inUV0, Vector4 inUV1, int inPadStat, float inParamVal,
									  float inYOff, Vector3 inTrueOrigin, Vector3 controlerAttachLocation, bool isLeftController)
	{
		// this is what happens when you pick up a star
		GameObject[] activeList = GameObject.FindGameObjectsWithTag("activatedStar");
		int starID = inStarID;
		//Debug.Log("Server Command sent, Star to be generated: " + starID);

		foreach (GameObject o in activeList)
			if (o.GetComponent<activatedStarScript>().starID == starID)
				return;

		var myActivatedStar = Instantiate(starObject, controlerAttachLocation, Quaternion.identity);

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

		NetworkServer.Spawn(myActivatedStar, GetComponent<NetworkIdentity>().connectionToClient);

		if (isLeftController) StartCoroutine(activeStarScript.grabStar(LeftController));
		else StartCoroutine(activeStarScript.grabStar(RightController));

		//SERVER STAR DROP TEST
		activeStarScript.isLeft = isLeftController;
        //SERVER STAR DROP TEST

        foreach (var c in myActivatedStar.GetComponentsInChildren<cameraFacer>())
            c.LookAtLevelEntity(transform.Find("Head"));

		// does not need to be in a command because already in a command
		//oscPacketBus.GetComponent<OSCPacketBus>().starGrabbed(inID, starID, controlerAttachLocation, isLeftController);
	}
	///<summary>
	/// FD: Tell the server to give the star to us
	///</summary><remarks>RpcRemoveAuthority should be called before calling CmdSetAuthority. OW/EGM?  Looks like this has been fixed NDH</remarks>
	[Command]
	public void CmdSetAuthority(NetworkIdentity identity)
	{
		NetworkConnection currentOwner = identity.connectionToClient;

		if (currentOwner == connectionToClient) return;

		if (currentOwner != null)
		{
			//Debug.LogError("RpcRemoveAuthority should be called before calling CmdSetAuthority (trying anyway)");
			identity.RemoveClientAuthority();
		}

		identity.AssignClientAuthority(connectionToClient);
	}
	/// <summary>
	/// FD: Client to server command to lineup tap sounds and text boxes
	/// </summary>
	/// <param name="starID">uUsed by tap to display proper text data</param>
	[Command]
	public void CmdPerformTap(Vector3 tapLocation, uint tapID, float[] tapData)
	{
		if (isServerOnly) voiceManager.performTap(tapID, tapData[0], tapData[1], tapData[2], (int)tapData[3], tapData[5], tapData[4], tapLocation);
		RpcPerformTap(tapLocation, tapID, tapData);
	}
	/// <summary>
	/// FD: Server to client command to lineup line pulses across users
	/// </summary>
	/// <param name="starID">uUsed by pulse to display proper text data</param>
	[ClientRpc]
    public void RpcSpawnPulse(uint starID)
    {
        LinesManager lines = GameObject.Find("lineHolder").GetComponent<LinesManager>();
        lines.SpawnPulses(Color.white, false);
    }

    [ClientRpc]
    public void RpcUpdateHighlightPos(int inID)
    {
        GameObject.FindGameObjectWithTag("init").GetComponent<shadeSwapper>().UpdateHighlightPos(inID);
    }

    /// <summary>
    /// FD: Server-half of spawning the slam-wave for all players
    /// </summary>
    /// <param name="initPos">VD: where did the player slam?/Where does the wave start?</param>
    /// <param name="intensity">VD: How hard was the slam?</param>
    /// <param name="inPadStat">VD: Colour of the pad when it happpened? Affects WaveParam in shaderspeak</param>
    [ClientRpc]
    public void RpcSpawnWave(Vector3 initPos, float intensity, int inPadStat)
    {
        initializerObj.GetComponent<shadeSwapper>().SpawnWave(initPos, intensity, inPadStat);
        IOANVoiceManager voiceManager = FindObjectOfType<IOANVoiceManager>();
        voiceManager.performWave(inPadStat, initPos, intensity * 25);
		

    }
	/// <summary>
	/// FD: Server to client command to lineup tap sounds and text boxes
	/// </summary>
	/// <param name="starID">uUsed by tap to display proper text data</param>
	[ClientRpc]
	public void RpcPerformTap(Vector3 tapLocation, uint tapID, float[] tapData)
	{
		IOANVoiceManager voiceManager = FindObjectOfType<IOANVoiceManager>();
		voiceManager.performTap(tapID, tapData[0], tapData[1], tapData[2], (int)tapData[3], tapData[5], tapData[4], tapLocation);
	}

    public void TouchPadInteraction(int i)
    {
        CmdTouchPadInteraction(i);
    }
    
    [Command]
    public void CmdTouchPadInteraction(int i)
    {
        controllerController leftCont = LeftController.GetComponent<iOANHeteroController>().contCont;
        controllerController rightCont = RightController.GetComponent<iOANHeteroController>().contCont;

        if (rightCont && leftCont)
        {
            switch(i)
            {
                case 0:
                    rightCont.OnCenterPress();
                    leftCont.OnCenterPress();
                    break;
                case 1:
                    rightCont.OnUpPress?.Invoke(0);
                    leftCont.OnUpPress?.Invoke(0);
                    break;
                case 2:
                    rightCont.OnDownPress?.Invoke(2);
                    leftCont.OnDownPress?.Invoke(2);
                    break;
                case 3:
                    rightCont.OnRightPress?.Invoke(1);
                    leftCont.OnRightPress?.Invoke(1);
                    break;
                case 4:
                    rightCont.OnLeftPress?.Invoke(3);
                    leftCont.OnLeftPress?.Invoke(3);
                    break;
                case 5:
                    rightCont.OnRelease?.Invoke(0);
                    leftCont.OnRelease?.Invoke(0);
                    break;
            }
        }

        RpcTouchpadInteraction(i);
    }

    [ClientRpc]
    public void RpcTouchpadInteraction(int i)
    {
        controllerController leftCont = LeftController.GetComponent<iOANHeteroController>().contCont;
        controllerController rightCont = RightController.GetComponent<iOANHeteroController>().contCont;

        if (rightCont && leftCont && isClientOnly)
        {
            switch (i)
            {
                case 0:
                    rightCont.OnCenterPress();
                    leftCont.OnCenterPress();
                    break;
                case 1:
                    rightCont.OnUpPress?.Invoke(0);
                    leftCont.OnUpPress?.Invoke(0);
                    break;
                case 2:
                    rightCont.OnDownPress?.Invoke(2);
                    leftCont.OnDownPress?.Invoke(2);
                    break;
                case 3:
                    rightCont.OnRightPress?.Invoke(1);
                    leftCont.OnRightPress?.Invoke(1);
                    break;
                case 4:
                    rightCont.OnLeftPress?.Invoke(3);
                    leftCont.OnLeftPress?.Invoke(3);
                    break;
                case 5:
                    rightCont.OnRelease?.Invoke(0);
                    leftCont.OnRelease?.Invoke(0);
                    break;
            }
        }
    }

    /*
    public void SignalSetCenterSphere(int i)
    {
        //Debug.Log("SIGNAL TO SERVER: " + isClient + " " + isServer);
        CmdSetCenterSphere(i);
    }

    [Command]
    public void CmdSetCenterSphere(int i)
    {
        //Debug.Log("COMMAND ON SERVER: " + isClient + " " + isServer);
        controllerController leftCont = LeftController.GetComponent<iOANHeteroController>().contCont;
        if (leftCont) leftCont.SetCenterSphere(i);
        controllerController rightCont = RightController.GetComponent<iOANHeteroController>().contCont;
        if (rightCont) rightCont.SetCenterSphere(i);
        RpcSetCenterSphere(i);
    }

    [ClientRpc]
    public void RpcSetCenterSphere(int i)
    {
        controllerController leftCont = LeftController.GetComponent<iOANHeteroController>().contCont;
        if (leftCont) leftCont.SetCenterSphere(i);
        controllerController rightCont = RightController.GetComponent<iOANHeteroController>().contCont;
        if (rightCont) rightCont.SetCenterSphere(i);
    }
    */
}


