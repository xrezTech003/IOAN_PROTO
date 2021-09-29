using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;
using System.Net;
using System.IO;

//this is a packet bus that is only used by client machines
// it is used by each local client to send client data
// taps
// controller position
// headPosition

/**
	<summary>
		CD : ClientOSCPacketBus
		This is used to send messages to the max engine on the audio machine.
		CM: this is a packet bus that is only used by client machines
			it is used by each local client to send client data
			taps
			controller position
			headPosition
	</summary>
**/
public class ClientOSCPacketBus : MonoBehaviour
{
	#region PUBLIC_VAR
	/// <summary>
	///		VD : frameDelay
	///		Fraction of a sec between oscSends for client head and contorller position data.  i.e. 1/(osc sends per sec)
	/// </summary>
	[Tooltip("Fraction of a sec between oscSends for client head and contorller position data.  i.e. 1/(osc sends per sec)")]
	public float frameDealy = 1.0f / 20.0f;

	/// <summary>
	///		VD : audioIP
	///		IP to audio server
	/// </summary>
	public string audioIP;

	/// <summary>
	///		VD : port
	///		Port num to communicate to audio server
	/// </summary>
	public int port;

	/// <summary>
	///		VD : playerIDNum
	/// </summary>
	public int playerIDNum;

	/// <summary>
	///		VD : leftController
	///		GameObject for left controller
	/// </summary>
	public GameObject leftController = null;

	/// <summary>
	///		VD : rightController
	///		GameObject for right controller
	/// </summary>
	public GameObject rightController = null;

	/// <summary>
	///		VD : head
	///		GameObject for head/headset
	/// </summary>
	public GameObject head = null;

	/// <summary>
	///		VD : tapDetector
	///		TapDetector object
	/// </summary>
	public TapDetector tapDetector;

	/// <summary>
	///		VD : timeToSend
	/// </summary>
	public float timeToSend = 0;

	/// <summary>
	///		VD : oscHandler
	/// </summary>
	public OSCHandler oscHandler;

	/// <summary>
	///		VD : OSC_CLIENT
	/// </summary>
	public const string OSC_CLIENT = "audioSys";
	#endregion

	#region UNITY_FUNC
	/**
	<summary>
		FD : Start()
		This sets up all the needed ports, ID numbers for player for the audio machine, and IP addresses.
	</summary>
	**/
	void Start()
	{
		Config config = Config.Instance;

        playerIDNum = config.Data.myID;

        audioIP = config.Data.audioIP;

		port = 8059;

		oscHandler = OSCHandler.Instance;

		oscHandler.CreateClient(OSC_CLIENT, IPAddress.Parse(audioIP), port);

		tapDetector = GetComponent<TapDetector>();
	}
	#endregion

	#region PUBLIC_FUNC
	/**
	<summary>
		FD : tapLeft()
		sets up the correct arguments to be sent for f:sendTap()
		<param name="starID">Passed for pulling up the correct stars sound.</param>
	</summary>
	**/
	public void tapLeft(uint starID)
	{
		sendTap(true, tapDetector.leftTapID, leftController.transform.position, getControllerDevice(leftController).velocity);
	}

	/**
	<summary>
		FD : tapRight()
		Sets up the correct arguments to be sent for f:sendTap()
		<param name="starID">Passed for pulling up the correct stars sound.</param>
	</summary>
	**/
	public void tapRight(uint starID)
	{
		sendTap(false, tapDetector.rightTapID, rightController.transform.position, getControllerDevice(rightController).velocity);
	}

	/**
	<summary>
		FD : SendPlayerData()
		This sends the needed player data such as player ID, controller positions, and head position to the MAX audio engine.
		<param name="value">Holds value for which value to be sent, either controller, or head</param>
		<param name="values">Position of said value being sent.</param>
	</summary>
	**/
	public void sendPlayerData(string value, List<float> values)
	{
		oscHandler.SendMessageToClient(OSC_CLIENT, "/" + playerIDNum + "/" + value, values);
	}

	/**
	<summary>
		FD : sendTapMessage()
		This function sends the needed tap data to the max audio engine. This function takes the controller that caused it, player ID, position of hit, magnitude of impact and angle.
		<param name="isLeft">VD: The bool for if the conotroller left or right</param>
		<param name="starId">VD: Star Id to correlate sound with, is converted to string</param>
		<param name="pos">VD: GS: Position of the controller that hit it</param>
		<param name="mag">VD: maginitude the controller hit the star with</param>
		<param name="ang">VD: Angle at which the star was hit.</param>
	</summary>
	**/
	public void sendTapMessage(bool isLeft, uint starId, Vector3 pos, float mag, float ang)
	{
		string value = "contR/TapID2";
		if (isLeft)
			value = "contL/TapID2";

		oscHandler.SendMessageToClient(OSC_CLIENT, "/" + playerIDNum + "/" + value, new List<System.Object> { starId.ToString(), pos.x, pos.y, pos.z, mag, ang });
		//startID to string because it doens't fit in a 32 bit int and longs don't seem to work over osc to max
	}
	#endregion

	#region PRIVATE_FUNC
	/**
	<summary>
		FD : vecToList(Vector3)
		seperates a vector into its parts and makes that a list in the order of x y z
		<param name="vec">vector to be parsed</param>
		<returns>A list of the vector in x y z</returns>
	</summary>
	**/
	List<float> vecToList(Vector3 vec)
	{
		return new List<float>() { vec.x, vec.y, vec.z };
	}

	/**
	<summary>
		FD : vecToList(Vector3, Vector3)
		Seperates a vector into its parts and makes that a list in the order of x y z and does this again for a second vector
		<param name="vec">first vector to be parsed</param>
		<param name="vec2">second vector to be parsed</param>
		<returns>A list of the vector in x y z, and then x2, y2, z2</returns>
	</summary>
	**/
	List<float> vecToList(Vector3 vec, Vector3 vec2)
	{
		return new List<float>() { vec.x, vec.y, vec.z, vec2.x, vec2.y, vec2.z };
	}

	/**
	<summary>
		FD : vecToList(Vector3, Quaternion)
		Seperates a vector into its parts and makes that a list in the order of x y z and a quaternion in the order of y z w
		<param name="vec">vector to be parsed into list</param>
		<param name="quat">quaternion to be parsed into list</param>
	</summary>
	**/
	List<float> vecToList(Vector3 vec, Quaternion quat)
	{
		return new List<float>() { vec.x, vec.y, vec.z, quat.x, quat.y, quat.z, quat.w };
	}

	/**
	<summary>
		FD : getControllerDevice(GameObject)
		Return Steam Controller from controller object
		<param name="controller"></param>
	</summary>
	**/
	SteamVR_Controller.Device getControllerDevice(GameObject controller)
	{
		return SteamVR_Controller.Input((int)controller.GetComponent<SteamVR_TrackedObject>().index);
	}

	/**
	<summary>
		FD : sendTap(bool, uint, Vector3, Vector3)
		Get angle between vel and Vector3.up
		Keep angle between 0-90
		Call f:sendTapMessage with isLeft, starID, location, vel.mag, and angle
		<param name="isLeft">/param>
		<param name="starID"></param>
		<param name="location"></param>
		<param name="vel"></param>
	</summary>
	**/
	void sendTap(bool isLeft, uint starID, Vector3 location, Vector3 vel)
	{
		float angle = Vector3.Angle(vel, Vector3.up);
		angle = (angle < 90) ? 90 - angle : 180 - angle;

		string value = "contR/TapID2"; ///Never used
		if (isLeft) value = "contL/TapID2";

		//	Debug.Log ("Tap: " + starID);
		sendTapMessage(isLeft, starID, location, vel.magnitude, angle);
	}

	/**
	<summary>
		FD : sendClientTrackingData()
		Call f:sendPlayerData based on whether v:leftController, v:rightController, and v:head is not null
	</summary>
	**/
	void sendClientTrackingData()
	{
		//yield return new WaitForEndOfFrame ();
		//Debug.Log ("player Data Send Method Called");

		if (leftController) sendPlayerData("contL/contPos2", vecToList(leftController.transform.position));

		if (rightController) sendPlayerData("contR/contPos2", vecToList(rightController.transform.position));

		if (head) sendPlayerData("headPos", vecToList(head.transform.position, head.transform.rotation));
	}
	#endregion

	#region USELESS_CODE
	/// <summary>
	/// FD: IV: Update(): Empty should delete/remove
	/// </summary>
	void Update()
	{
		/*
		if (! leftController) {
			leftController = GameObject.FindGameObjectWithTag ("controlLeft");
		}
		if (! rightController) {
			rightController = GameObject.FindGameObjectWithTag ("controlRight");
		}

		if (!head) {
			head = GameObject.FindGameObjectWithTag("MainCamera");
		}




		if (Time.time > timeToSend) {
			sendClientTrackingData();
		}
		*/
	}

	// taps are called by the tap detector
	#endregion
}
