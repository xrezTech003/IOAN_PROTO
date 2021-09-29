using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;
using System.Net;
using System.IO;

/**
<summary>
	CD : OSCPacketBus
	Class for controlling data of the three entities assigned to each player: head, left controller, and right controller
</summary>
**/
public class OSCPacketBus : MonoBehaviour 
{
	#region PUBLIC_VARS
	/// <summary>
	///		VD : IP
	///		Store the network address the packet is being directed to.
	/// </summary>
	private string IP;
	//private int port;

	/// <summary>
	///		VD : playerDataList
	///		Store the list of player data.
	/// </summary>
	public List<PlayerData> playerDataList;

	/// <summary>
	///		VD : busFrameTime
	///		Store the time in which the bus is in frame.
	/// </summary>
	public float busFrameTime = 0.25f;

	/// <summary>
	///		VD : serverFlag
	///		Store whether or not the packet can handshake via the bus.
	/// </summary>
	public bool serverFlag;

	/// <summary>
	///		VD : oscReciever
	///		Object for recieving packets.
	/// </summary>
	public OSCReciever oscReciever;

	/// <summary>
	///		VD : ControllerName
	///		Struct defining a set of consts for LEFT (contL) and RIGHT (contR) variables.
	/// </summary>
	public static class ControllerName {
		public const string LEFT = "contL";
		public const string RIGHT = "contR";
	}
    #endregion

    #region PRIVATE_VARS
	/// <summary>
	///		VD : busTrigger
	///		Wait value for all coroutines
	/// </summary>
    private bool busTrigger;

	/// <summary>
	///		VD : busTimer
	///		Timer value for f:Update
	/// </summary>
	private float busTimer;
    #endregion

    #region UNITY_FUNCS
	/**
	<summary>
		FD : Start()
		Init v:playerDataList, v:serverFlag, v:IP, v:busTrigger, v:busTimer, and v:oscReciever
	</summary>
	**/
    void Start () 
	{
		playerDataList = new List<PlayerData>();
		serverFlag = false;
		//busFrameTime = 0.25f;

        Config config = Config.Instance;
        string audioIP = config.Data.audioIP;

		//IP = "192.168.1.113";
		IP = audioIP;
		//port = 8059;

		OSCHandler.Instance.CreateClient("audioSys", IPAddress.Parse (IP), 8059);
		busTrigger = false;
		busTimer = Time.time;

		// There are slots for 6 players currently, edit this to change that
		for (int i = 0; i < 6; i++) playerDataList.Add(new PlayerData());

        string serverStatus = config.Data.serverStatus;

		if(serverStatus == "server") 
		{
			Debug.Log("Creating OSC receiver on the server");
			if(oscReciever == null) // not sure if start could get called twice with out a quit so I'm being careful
			{ 
				oscReciever = new OSCReciever();
				oscReciever.Open(9000);
			}
		}
	}

	/**
	<summary>
		FD : OnApplicationQuit()
		Close the oscReciever is not null
	</summary>
	**/
	void OnApplicationQuit() 
	{
		Debug.Log("Closing oscReciever");

		if (oscReciever != null) 
		{
			oscReciever.Close ();
			oscReciever = null;
		}
	}

	/**
	<summary>
		FD : Update()
		Set v:busTrigger to false
		If it was already false
			Reset v:busTimer and v:busTrigger
			Call f:playerDataTrigger() if v:serverFlag is true
		Handler osc messages if v:oscReciever isn't null
	</summary>
	**/
	void Update () 
	{
		if (busTrigger == true) busTrigger = false;
		else if (Time.time - busTimer > busFrameTime) 
		{
			busTimer = Time.time + Random.Range(-0.025f,0.025f);
			busTrigger = true;

			if (serverFlag) playerDataTrigger();
		}

		if(oscReciever != null) 
		{
			while(oscReciever.hasWaitingMessages())
				handleMessage(oscReciever.getNextMessage());
		}
	}
    #endregion

    #region PUBLIC_FUNCS
	/**
	<summary>
		FD : handleMessage(OSCMessage)
		If msg has the right Address, call f:endStar() with the starID
		<param name="msg"></param>
	</summary>
	**/
    public void handleMessage(OSCMessage msg) 
	{
		if(msg.Address == "/endStar") 
		{
			int starID = (int)msg.Data[0];
			endStar(starID);
		} 
		else 
		{
			Debug.Log("OSCReciever unrecognized address "  + msg.Address);
		}
	}

	/**
	<summary>
		FD : endStar(int)
		Get all objects tagged with "activatedStar"
		Destroy all stars
		<param name="starID"></param>
	</summary>
	**/
	public void endStar(int starID) 
	{
		GameObject[] activeList = GameObject.FindGameObjectsWithTag ("activatedStar");

		foreach (GameObject star in activeList) 
		{
			activatedStarScript starScript = star.GetComponent<activatedStarScript>();

			if (starScript.starID == starID)
			{
				// does the sever need to take authority?
				starScript.destroyStar();
				return;
			}
		}

		Debug.Log("OSCPacketBus unable to destroy active star: " + starID + " (id not found)" );
	}

	/**
	<summary>
		FD : serverFlagActivate()
		Set v:serverFlag to true
	</summary>
	**/
	public void serverFlagActivate() 
	{
		//Debug.Log ("Received Server Activate");
		serverFlag = true;
	}

	/**
	<summary>
		FD : activatePlayer(int, GameObject, GameObject)
		If v:serverFlag is true: set values of v:playerDataList
		<param name="playerID"></param>
		<param name="controllerL"></param>
		<param name="controllerR"></param>
	</summary>
	**/
	public void activatePlayer(int playerID, GameObject controllerL, GameObject controllerR) 
	{
		//Debug.Log ("Received Player" + playerID + "Activate");
		if (serverFlag) 
		{
			playerDataList[playerID].active = true;
			playerDataList[playerID].contL = controllerL;
			playerDataList[playerID].contR = controllerR;
		}
	}

	/**
	<summary>
		FD : actiavteHead(int, GameObject)
		If v:serverFlag is true: set v:playerDataList head to head
		<param name="playerID"></param>
		<param name="head"></param>
	</summary>
	**/
	public void activateHead(int playerID, GameObject head)
	{
		////Debug.Log ("Received Player Head" + playerID + "Activate");
		if (serverFlag) playerDataList[playerID].head = head;
	}

	/**
	<summary>
		FD : deactivatePlayer(int)
		If v:serverFlag is true, set v:playerDataList active to false
		<param name="playerID"></param>
	</summary>
	**/
	public void deactivatePlayer(int playerID)
	{
		//Debug.Log ("Received Player Deactivate");
		if (serverFlag) playerDataList[playerID].active = false;
	}

	/**
	<summary>
		FD : SelRelStar(int, GameObject)
		If v:serverFlag is true, set v:playerDataList relStarObj to relStar
		<param name="playerID"></param>
		<param name="relStar"></param>
	</summary>
	**/
	public void SetRelStar(int playerID, GameObject relStar)
	{
		if (serverFlag) playerDataList[playerID].relStarObj = relStar;
	}

	/**
	<summary>
		FD : SetStarHoldObj(int, GameObject)
		If v:serverFlag is true, Set v:playerDataList starHoldObj to starHoldObj
		<param name="playerID"></param>
		<param name="starHoldObj"></param>
	</summary>
	**/
	public void SetStarHoldObj(int playerID, GameObject starHoldObj)
	{
		if (serverFlag) playerDataList[playerID].starHoldObj = starHoldObj;
	}

	/**
	<summary>
		FD : UpdateHead(int, Vector3, Quaternion)
		If v:serverFlag is true, set v:playerDataList head data to parameters
		<param name="playerID"></param>
		<param name="headPos"></param>
		<param name="headRot"></param>
	</summary>
	**/
	public void UpdateHead(int playerID, Vector3 headPos, Quaternion headRot)
	{
		//Debug.Log ("UpdateHead Call");
		if (serverFlag) 
		{
			playerDataList [playerID].headPos = headPos;
			playerDataList [playerID].headRot = headRot;
		}
	}

	/**
	<summary>
		FD : UpdateSelStarPos(int, Vector3)
		If v:serverFlag is true, v:playerDataList selStarPos to selStarPos
		<param name="playerID"></param>
		<param name="selStarPos"></param>
	</summary>
	**/
	public void UpdateSelStarPos(int playerID, Vector3 selStarPos)
	{
		//Debug.Log ("selStarPos call");
		if (serverFlag) playerDataList[playerID].selStarPos = selStarPos;
	}

	/**
	<summary>
		FD : starGrabbed(int, int, Vector3, bool)
		If v:serverFlag is true, Start coroutine f:newStarGrabbedTrigger with parameters
		// inactive star is grabbed and turned into drag star
		<param name="playerIndex"></param>
		<param name="starID"></param>
		<param name="starLocation"></param>
		<param name="isLeft"></param>
	</summary>
	**/
	public void starGrabbed(int playerIndex, int starID, Vector3 starLocation, bool isLeft)
	{
		if (serverFlag)
		{
			// I'm not sure why everything is wrapped in an extra function.  I'm not going to do it here
			StartCoroutine(newStarGrabbedTrigger(playerIndex, starID, starLocation, isLeft));
		}
		else Debug.LogWarning("Unable to send starGrabbed message from a client");
	}

	/**
	<summary>
		FD : dragDisgarded(int, int, Vector3, bool)
		If v:serverFlag is true, Start coroutine f:dragDisgardedTrigger with params
		// drag star disgarded and not activated
		<param name="playerIndex"></param>
		<param name="starID"></param>
		<param name="starLocation"></param>
		<param name="isLeft"></param>
	</summary>
	**/
	public void dragDisgarded(int playerIndex, int starID, Vector3 starLocation, bool isLeft)
	{
		if (serverFlag)
		{
			// I'm not sure why everything is wrapped in an extra function.  I'm not going to do it here
			StartCoroutine(dragDisgardedTrigger(playerIndex, starID, starLocation, isLeft));
		}
		else Debug.LogWarning("Unable to send dragDisgarded message from a client");
	}

	/**
	<summary>
		FD : dragAboveActivationThreshold(int, int)
		If v:serverFlag is true, Start coroutine f:activationCrossTrigger with params
		<param name="playerIndex"></param>
		<param name="starID"></param>
	</summary>
	**/
	public void dragAboveActivationThreshold(int playerIndex, int starID)
	{
		if (serverFlag) StartCoroutine(activationCrossTrigger(playerIndex, starID, 1));
		else Debug.LogWarning("Unable to send dragAboveActivationThreshold message from a client");
	}

	/**
	<summary>
		FD : dragBelowActivationThreshold(int, int)
		If v:serverFlag is true, Start coroutine f:activationCrossTrigger with params
		<param name="playerIndex"></param>
		<param name="starID"></param>
	</summary>
	**/
	public void dragBelowActivationThreshold(int playerIndex, int starID)
	{
		if (serverFlag) StartCoroutine(activationCrossTrigger(playerIndex, starID, 0));
		else Debug.LogWarning("Unable to send dragBelowActivationThreshold message from a client");
	}

	/**
	<summary>
		FD : starActivated(int, int, Vector3, bool)
		If v:serverFlag is true, Start coroutine f:starActivatedTrigger with params
		<param name="playerIndex"></param>
		<param name="starID"></param>
		<param name="starLocation"></param>
		<param name="isLeft"></param>
	</summary>
	**/
	public void starActivated(int playerIndex, int starID, Vector3 starLocation, bool isLeft)
	{
		if (serverFlag) StartCoroutine(starActivatedTrigger(playerIndex, starID, starLocation, isLeft));
		else Debug.LogWarning("Unable to send starActivated message from a client");
	}

	/**
	<summary>
		FD : activatedStarGrabbed(int, int, Vector3, bool)
		If v:serverFlag is true, Start coroutine f:activatedStarGrabbedTrigger with params
		// used for both grabbing activated stars and for handoffs
		<param name="playerIndex"></param>
		<param name="starID"></param>
		<param name="starLocation"></param>
		<param name="isLeft"></param>
	</summary>
	**/
	public void activatedStarGrabbed(int playerIndex, int starID, Vector3 starLocation, bool isLeft)
	{
		if (serverFlag) StartCoroutine(activatedStarGrabbedTrigger(playerIndex, starID, starLocation, isLeft));
		else Debug.LogWarning("Unable to send starActivated message from a client");
	}

	/**
	<summary>
		FD : activatedStartDeselected(int, int)
		If v:serverFlag is true, Start coroutine f:activatedStartDeselectedTrigger with params
		<param name="playerIndex"></param>
		<param name="starID"></param>
	</summary>
	**/
	public void activatedStartDeselected(int playerIndex, int starID)
	{
		if (serverFlag) StartCoroutine(activatedStartDeselectedTrigger(playerIndex, starID));
		else Debug.LogWarning("Unable to send starActivated message from a client");
	}

	/**
	<summary>
		FD : newTapIDStart(int, bool, uint, Vector3, float, float)
		If v:serverFlag is true, Start coroutine f:newTapIDTrigger with params
		<param name="playerIndex"></param>
		<param name="isLeftController"></param>
		<param name="starID"></param>
		<param name="starPos"></param>
		<param name="intensity"></param>
		<param name="angle"></param>
	</summary>
	**/
	public void newTapIDStart(int playerIndex, bool isLeftController, uint starID, Vector3 starPos, float intensity, float angle)
	{
		if (serverFlag) StartCoroutine(newTapIDTrigger(playerIndex, isLeftController, starID, starPos, intensity, angle));

		//Debug.Log (playerIndex.ToString() + " " + starID.ToString() + " " + starPos.ToString() + " " + intensity.ToString());
	}

	/**
	<summary>
		FD : waveTriggerStart(Vector2, float, int)
		If v:serverFlag is true, Start coroutine f:waveTrigger with params
		<param name="waveOrigin"></param>
		<param name="waveIntensity"></param>
		<param name="inPadStat"></param>
	</summary>
	**/
	public void waveTriggerStart(Vector2 waveOrigin, float waveIntensity, int inPadStat)
	{
		//Debug.Log ("waveTriggerStart call");
		if (serverFlag) StartCoroutine(waveTrigger(waveOrigin, waveIntensity, inPadStat));
	}
    #endregion

    #region PRIVATE_FUNC
	/**
	<summary>
		FD : newTapIDTrigger(int, bool, uint, Vector3, float, float)
		Wait for v:busTrigger to be true
		Send message to client with playerIndex, apprpriate controller, and star data(starID, position, intensity, and angle)
		<param name="playerIndex"></param>
		<param name="isLeftController"></param>
		<param name="starID"></param>
		<param name="starPos"></param>
		<param name="intensity"></param>
		<param name="angle"></param>
	</summary>
	**/
    private IEnumerator newTapIDTrigger(int playerIndex,  bool isLeftController, uint starID, Vector3 starPos, float intensity, float angle)
	{
		yield return new WaitUntil(() => busTrigger == true);
		string controller = isLeftController ? ControllerName.LEFT : ControllerName.RIGHT;

		OSCHandler.Instance.SendMessageToClient ("audioSys", "/" + playerIndex + "/" + controller + "/TapID", new List<System.Object>{ starID.ToString (),  starPos.x, starPos.y, starPos.z, intensity, angle });
	}

	/**
	<summary>
		FD : starActivatedTrigger(int, int, Vector3, bool)
		Wait for v:busTrigger to be true
		Send message to client with playerIndex, apprpriate controller, and star data(starID and starLocation)
		<param name="playerIndex"></param>
		<param name="starID"></param>
		<param name="starLocation"></param>
		<param name="isLeft"></param>
	</summary>
	**/
	private IEnumerator starActivatedTrigger(int playerIndex, int starID, Vector3 starLocation, bool isLeft)
	{
		yield return new WaitUntil(() => busTrigger == true);
		string controller = isLeft ? ControllerName.LEFT : ControllerName.RIGHT;

		OSCHandler.Instance.SendMessageToClient("audioSys", "/" + playerIndex + "/" + controller + "/releaseID", new List<System.Object> { starID.ToString(), starLocation.x, starLocation.y, starLocation.z });
	}

	/**
	<summary>
		FD : actiavtionCrossTrigger(int, int, int)
		Wait for v:busTrigger to be true
		Send message to client with playerIndex and star data(starID and aboveThresh)
		<param name="playerIndex"></param>
		<param name="starID"></param>
		<param name="aboveThresh"></param>
	</summary>
	**/
	private IEnumerator activationCrossTrigger(int playerIndex, int starID, int aboveThresh)
	{
		yield return new WaitUntil(() => busTrigger == true);

		OSCHandler.Instance.SendMessageToClient("audioSys", "/" + playerIndex + "/activationCross", new List<System.Object> { starID.ToString(), aboveThresh });
	}

	/**
	<summary>
		FD : activatedStarGrabbedTrigger(int, int, Vector3, bool)
		Wait for v:busTrigger to be true
		Send message to client with playerIndex, apprpriate controller, and star data(starID and starLocation)
		<param name="playerIndex"></param>
		<param name="starID"></param>
		<param name="starLocation"></param>
		<param name="isLeft"></param>
	</summary>
	**/
	private IEnumerator activatedStarGrabbedTrigger(int playerIndex, int starID, Vector3 starLocation, bool isLeft)
	{
		yield return new WaitUntil(() => busTrigger == true);
		string controller = isLeft ? ControllerName.LEFT : ControllerName.RIGHT;

		OSCHandler.Instance.SendMessageToClient("audioSys", "/" + playerIndex + "/" + controller + "/grabActive", new List<System.Object> { starID.ToString(), starLocation.x, starLocation.y, starLocation.z });
	}

	/**
	<summary>
		FD : waveTrigger(Vector2, float, int)
		Wait for v:busTrigger to be true
		Send message to client with inPadStat and waveOrigin
		<param name="waveOrigin"></param>
		<param name="waveIntensity"></param>
		<param name="inPadStat"></param>
	</summary>
	**/
	private IEnumerator waveTrigger(Vector2 waveOrigin, float waveIntensity, int inPadStat)
	{
		yield return new WaitUntil (() => busTrigger == true);
		//Debug.Log ("waveTrigger IEnumerator call");

		OSCHandler.Instance.SendMessageToClient ("audioSys", "/waveOrigin", new List<float>(){(float)inPadStat, waveOrigin.x, waveOrigin.y, waveIntensity});

		//print ("Sent wave with intensity " + waveIntensity.ToString());
		//OSCHandler.Instance.SendMessageToClient ("audioSys", "/waveIntensity", waveIntensity);
	}

	/**
	<summary>
		FD : activatedStartDeselectedTrigger(int, int)
		Wait for v:busTrigger to be true
		Send message to client with playerIndex and starID
		<param name="playerIndex"></param>
		<param name="starID"></param>
	</summary>
	**/
	private IEnumerator activatedStartDeselectedTrigger(int playerIndex, int starID)
	{
		yield return new WaitUntil(() => busTrigger == true);

		OSCHandler.Instance.SendMessageToClient("audioSys", "/" + playerIndex + "/deselectID", new List<System.Object> { starID.ToString() });
	}

	/**
	<summary>
		FD : dragDisgardedTrigger(int, int, Vector3, bool)
		Wait for v:busTrigger to be true
		Send message to client with playerIndex, apprpriate controller, and star data(starID and starLocation)
		<param name="playerIndex"></param>
		<param name="starID"></param>
		<param name="starLocation"></param>
		<param name="isLeft"></param>
	</summary>
	**/
	private IEnumerator dragDisgardedTrigger(int playerIndex, int starID, Vector3 starLocation, bool isLeft)
	{
		yield return new WaitUntil(() => busTrigger == true);
		string controller = isLeft ? ControllerName.LEFT : ControllerName.RIGHT;

		OSCHandler.Instance.SendMessageToClient("audioSys", "/" + playerIndex + "/" + controller + "/selectOff", new List<System.Object> { starID.ToString(), starLocation.x, starLocation.y, starLocation.z });

	}

	/**
	<summary>
		FD : newStarGrabbedTrigger(int, int, Vector3, bool)
		Wait for v:busTrigger to be true
		Send message to client with playerIndex, apprpriate controller, and star data(starID and starLocation)
		<param name="playerIndex"></param>
		<param name="starID"></param>
		<param name="starLocation"></param>
		<param name="isLeft"></param>
	</summary>
	**/
	private IEnumerator newStarGrabbedTrigger(int playerIndex, int starID, Vector3 starLocation, bool isLeft)
	{
		yield return new WaitUntil(() => busTrigger == true);
		string controller = isLeft ? ControllerName.LEFT : ControllerName.RIGHT;

		OSCHandler.Instance.SendMessageToClient("audioSys", "/" + playerIndex + "/" + controller + "/selectOn", new List<System.Object> { starID.ToString(), starLocation.x, starLocation.y, starLocation.z });
	}

	/**
	<summary>
		FD : playerDataTrigger()
		For all playerData in v:playerDataList
			If the player is active
				Send data to client based on if head, contL, or contR is available for that player 
	</summary>
	**/
	private void playerDataTrigger()
	{
		//yield return new WaitForEndOfFrame ();
		//Debug.Log ("player Data Send Method Called");

		for (int i = 0; i < playerDataList.Count; i++)
		{
			if (playerDataList[i].active == true)
			{
				//Debug.Log ("sending player data " + i);

				if (playerDataList[i].head != null)
				{
					Vector3 theadPos = playerDataList[i].head.transform.position;
					Quaternion theadRot = playerDataList[i].head.transform.rotation;

					OSCHandler.Instance.SendMessageToClient("audioSys", "/" + i + "/headPos", new List<float>() { theadPos.x, theadPos.y, theadPos.z, theadRot.x, theadRot.y, theadRot.z, theadRot.w });

					//OSCHandler.Instance.SendMessageToClient ("audioSys","/" + i + "/headRot",
					//	new List<float> (){ theadRot.x, theadRot.y, theadRot.z, theadRot.w });
				}

				if (playerDataList[i].contL != null)
				{
					Vector3 tcontPos = playerDataList[i].contL.transform.position;

					OSCHandler.Instance.SendMessageToClient("audioSys", "/" + i + "/contL/contPos", new List<float>() { tcontPos.x, tcontPos.y, tcontPos.z });
				}

				if (playerDataList[i].contR != null)
				{
					Vector3 tcontPos = playerDataList[i].contR.transform.position;

					OSCHandler.Instance.SendMessageToClient("audioSys", "/" + i + "/contR/contPos", new List<float>() { tcontPos.x, tcontPos.y, tcontPos.z });
				}

				/*TODO keep this?
				if (playerDataList [i].selStarID != -1 && playerDataList [i].contR != null) {
					Vector3 tselStarPos = playerDataList [i].contR.GetComponent<PlayerMove> ().selStarPos;
					OSCHandler.Instance.SendMessageToClient ("audioSys","/" + i + "/selStarPos",
						new List<float>(){tselStarPos.x,tselStarPos.y,tselStarPos.z});
				} else {
					//OSCHandler.Instance.SendMessageToClient ("audioSys", "/rayPos" + i,
					//	new List<float>(){playerDataList [i].rayPos.x,playerDataList [i].rayPos.y,playerDataList [i].rayPos.z});
				}
				*/

				/*
				if (playerDataList [i].relStarID != 0 && playerDataList [i].relStarObj != null) {
					Vector3 trelStarPos = playerDataList [i].relStarObj.transform.position;
					OSCHandler.Instance.SendMessageToClient ("audioSys","/" + i + "/relStarPos",
						new List<float>(){trelStarPos.x,trelStarPos.y,trelStarPos.z});
				}

				if (playerDataList [i].starHolding && playerDataList [i].starHoldObj != null) {
					Vector3 tstarHoldPos = playerDataList [i].starHoldObj.transform.position;
					OSCHandler.Instance.SendMessageToClient ("audioSys","/" + i + "/starHoldPos",
						new List<float>(){tstarHoldPos.x,tstarHoldPos.y,tstarHoldPos.z});
				}
				*/
			}
		}
	}
    #endregion

    #region Commented Code
    /*
		private IEnumerator seqTrigger(int seqID, float seqPhase, Vector3 seqOrigin){
			yield return new WaitUntil (() => busTrigger == true);
			//Debug.Log ("seqTrigger IEnumerator call");
			OSCHandler.Instance.SendMessageToClient ("audioSys", "/seqID", new List<float>(){seqID, seqPhase,
				seqOrigin.x,seqOrigin.y,seqOrigin.z});
			//OSCHandler.Instance.SendMessageToClient ("audioSys", "/seqPhase", seqPhase);
			//OSCHandler.Instance.SendMessageToClient ("audioSys", "/seqOrigin",
			//	new List<float>(){seqOrigin.x,seqOrigin.y,seqOrigin.z});
		}

		private IEnumerator selStarTrigger(int playerIndex, int starID, bool discard, Vector3 starOrigin){
			playerDataList [playerIndex].selStarID = starID;
			if (discard == false) {
				playerDataList [playerIndex].selStarOrigin = starOrigin;
			} else {
				playerDataList [playerIndex].selStarID = -1;
			}
			yield return new WaitUntil (() => busTrigger == true);
			//Debug.Log ("selStarTrigger IEnumerator call");
			if (discard == false) {
				//OSCHandler.Instance.SendMessageToClient ("audioSys","/" + playerIndex + "/selStarOrigin",
				//	new List<float>(){starOrigin.x,starOrigin.y,starOrigin.z});
				OSCHandler.Instance.SendMessageToClient ("audioSys","/" + playerIndex + "/selStarID",
					new List<float>(){starID, starOrigin.x,starOrigin.y,starOrigin.z});
			} else {
				//OSCHandler.Instance.SendMessageToClient ("audioSys","/" + playerIndex + "/selStarDiscard", 1);
				OSCHandler.Instance.SendMessageToClient ("audioSys","/" + playerIndex + "/selStarID", -1);
			}
		}

		private IEnumerator relStarTrigger(int playerIndex, int starID, bool terminate){
			int oldStarID = playerDataList [playerIndex].relStarID;
			if (terminate == false) {
				playerDataList [playerIndex].relStarID = starID;
			} else {
				if (starID == oldStarID) {
					////Debug.Log ("relStarterminate");
					playerDataList [playerIndex].relStarID = -1;
					playerDataList [playerIndex].selStarID = -1;
				}
			}
			yield return new WaitUntil (() => busTrigger == true);
			//Debug.Log ("relStarTrigger IEnumerator call");
			if (terminate == false) {
				OSCHandler.Instance.SendMessageToClient ("audioSys","/" + playerIndex + "/relStarID", starID);
			} else {
				if (starID == oldStarID) {
					OSCHandler.Instance.SendMessageToClient ("audioSys","/" + playerIndex + "/relStarID", -1);
				}
			}
		}

		private IEnumerator starHoldingTrigger(int playerIndex, bool holding){
			playerDataList [playerIndex].starHolding = holding;
			yield return new WaitUntil (() => busTrigger == true);
			//Debug.Log ("starHoldingTrigger IEnumerator call");
			OSCHandler.Instance.SendMessageToClient ("audioSys","/" + playerIndex + "/starHolding", holding ? 1 : 0);
		}
	*/
    /*
		private IEnumerator ifaceDoneTrigger(int playerIndex){
			playerDataList [playerIndex].ifaceDone = true;
			yield return new WaitUntil (() => busTrigger == true);
			//Debug.Log ("ifaceDoneTrigger IEnumerator call");
			OSCHandler.Instance.SendMessageToClient ("audioSys","/" + playerIndex + "/ifaceDone", 1);
			playerDataList [playerIndex].ifaceDone = false;
		}
		*/

    /*
		public void UpdateRelStarPos(int playerID, Vector3 relStarPos){
			//Debug.Log ("relStarPos call");
			if (serverFlag) {
				playerDataList [playerID].relStarPos = relStarPos;
			}
		}

		public void UpdateStarHoldPos(int playerID, Vector3 starHoldPos){
			//Debug.Log ("starHoldPos call");
			if (serverFlag) {
				playerDataList [playerID].starHoldPos = starHoldPos;
			}
		}
	*/

    /* TODO: replace
	public void seqTriggerStart(int seqID, float seqPhase, Vector3 seqOrigin){
		//Debug.Log ("seqTriggerStart call");
		if (serverFlag) {
			StartCoroutine (seqTrigger (seqID, seqPhase, seqOrigin));
		}
	}

	public void selStarTriggerStart(int playerIndex, int starID, bool discard, Vector3 starOrigin) {
		//Debug.Log ("selStarTriggerStart call");
		if (serverFlag) {
			StartCoroutine (selStarTrigger (playerIndex, starID, discard, starOrigin));
		}
	}

	public void relStarTriggerStart(int playerIndex, int starID, bool terminate) {
		//Debug.Log ("relStarTriggerStart call");
		if (serverFlag) {
			StartCoroutine (relStarTrigger (playerIndex, starID, terminate));
		}
	}
*/
    /* TODO: replace
	// ignore for now, may use later
	public void starHoldingTriggerStart(int playerIndex, bool holding) {
		//Debug.Log ("starHoldingTriggerStart call");
		if (serverFlag) {
			StartCoroutine (starHoldingTrigger (playerIndex, holding));
		}
	}

	// ignore for now, may use later
	public void ifaceDoneTriggerStart(int playerIndex){
		//Debug.Log ("ifaceDoneTriggerStart call");
		if (serverFlag) {
			StartCoroutine (ifaceDoneTrigger (playerIndex));
		}
	}
	*/


    /*
	public void dragReleased(int playerIndex, int starID, Vector3 starLocation) {
		if(serverFlag) {
			StartCoroutine (dragReleasedTrigger (playerIndex, starID, starLocation));
		} else {
			Debug.LogWarning("Unable to send dragReleased message from a client");
		}
	}

	private IEnumerator  dragReleasedTrigger(int playerIndex, int starID, Vector3 starLocation, bool isLeft) {
		yield return new WaitUntil (() => busTrigger == true);
		string controller = isLeft ? ControllerName.LEFT : ControllerName.RIGHT;
		OSCHandler.Instance.SendMessageToClient ("audioSys", "/" + playerIndex  + "/" + controller +  "/releaseID",
			new List<System.Object>{ starID.ToString (),  starLocation.x, starLocation.y, starLocation.z });
	}
	*/


    /* old message format
		public void newSelIDStart(int playerIndex, int starID){
			if (serverFlag) {
				StartCoroutine (newSelIDTrigger (playerIndex, starID));
			}
		}
	*/
    /*
	public void newRelIDStart(int playerIndex, int starID, Vector3 starDestination){
		if (serverFlag) {
			StartCoroutine (newRelIDTrigger (playerIndex, starID, starDestination));
		}
	}

	public void newDeadIDStart(int starID){
		if (serverFlag) {
			StartCoroutine (newDeadIDTrigger (starID));
		}
	}

	public void newDiscardIDStart(int playerIndex, int starID){
		if (serverFlag) {
			StartCoroutine (newDiscardIDTrigger (playerIndex, starID));
		}
	}
	*/




    // old version
    /*
		private IEnumerator newSelIDTrigger(int playerIndex, int starID){
			yield return new WaitUntil (() => busTrigger == true);
			OSCHandler.Instance.SendMessageToClient ("audioSys", "/" + playerIndex + "/newSelID", starID);
			//print ("sel id");
		}
	*/

    /* oldversion
		private IEnumerator newRelIDTrigger(int playerIndex, int starID, Vector3 starDestination){
			yield return new WaitUntil (() => busTrigger == true);
			OSCHandler.Instance.SendMessageToClient ("audioSys","/" + playerIndex + "/newRelID",
				new List<string>(){starID.ToString(), starDestination.x.ToString(), starDestination.y.ToString(), starDestination.z.ToString()});
			//print ("rel id");
		}
	*/
    /*
		private IEnumerator newDiscardIDTrigger(int playerIndex, int starID){
			yield return new WaitUntil (() => busTrigger == true);
			OSCHandler.Instance.SendMessageToClient ("audioSys","/" + playerIndex + "/newDiscardID",
				starID.ToString());
			//print ("rel id");
		}

		private IEnumerator newDeadIDTrigger(int starID){
			//Debug.Log (starID);
			yield return new WaitUntil (() => busTrigger == true);
			OSCHandler.Instance.SendMessageToClient ("audioSys", "/newDeadID", starID);
			//print ("dead id");
		}
	*/
    #endregion
}
