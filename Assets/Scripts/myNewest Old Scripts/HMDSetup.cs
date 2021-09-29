using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

/**
<summary>
	CD : HMDSetup
	Disables all child meshes and sets transform data to headset
	Theres another script that does something very similar I_A ::: Also never referenced anywhere and confirmed compiler says nothing bad when the whole code is gone NDH
</summary>
**/
public class HMDSetup : NetworkBehaviour
{
	#region PUBLIC_VAR
	/// <summary>
	///		VD : headset
	///		MainCamera
	/// </summary>
	public GameObject headset;

	/// <summary>
	///		VD : myColor GS: NDH - If syncvars work how I think they do, this is set by playerMove and is either R G or B
	/// </summary>
	[SyncVar]
	public Color myColor;

	/// <summary>
	///		VD : headVisInitialized
	/// </summary>
	public bool headVisInitialized;

	/// <summary>
	///		VD : myID
	/// </summary>
	[SyncVar(hook = "OnChangePlayerID")]
	public int myID;

	/// <summary>
	///		VD : initRef
	///		Reference for Initialization
	/// </summary>
	public GameObject initRef;
	#endregion

	#region PRIVATE_VAR
	/// <summary>
	///		VD : startFlag
	/// </summary>
	private bool startFlag = false;
	#endregion

	#region UNITY_FUNC
	/**
	<summary>
		FD : Start()
		If v:myID < 0 or > 2
			Set all child meshRenders to false
		Else
			Set v:headset to GameObject with tag "MainCamera"
			Set the color of the gameObjects mesh
	</summary>
	**/
	void Start()
	{
		if (myID < 0 || myID > 2) foreach (Transform child in transform) child.gameObject.GetComponentInChildren<MeshRenderer>().enabled = false;
		else
		{
			headVisInitialized = false;
			headset = GameObject.FindGameObjectWithTag("MainCamera");
			gameObject.GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", myColor * Mathf.LinearToGammaSpace(1.0f));
			//CmdStartOSC ();

			//ITS DOING EVERYTHING TWICE
			//oscPacketBus = GameObject.FindGameObjectWithTag ("OSCPacketBus");
			headVisInitialized = false;
			headset = GameObject.FindGameObjectWithTag("MainCamera");
			gameObject.GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", myColor * Mathf.LinearToGammaSpace(1.0f));
			//CmdStartOSC ();

			//REVERT
			//if (isServer) {
			//	oscPacketBus.GetComponent<OSCPacketBus> ().activateHead (myID, gameObject);
			//}
			//END
		}
	}

	/**
	<summary>
		FD : Update()
		Return if 0 < v:myID < 2 isn't true
		If v:startFlag is False
			Find init object
			If the object has authority
				Call CmdSendOSC with v:myID
				Set v:startFlag to true
				If head isn't init
					Disable all mesh in children
					set head init to true
				Set transform data to headset transform data
		<remarks>
			IV
			<code>
				void Update()
				{
					if (!(0 <= myID && myID <= 2)) return;

					if (!startFlag)
					{
						initRef = GameObject.FindGameObjectWithTag("init");

						if (hasAuthority)
						{
							CmdSendOSC(myID);
							startFlag = true;

							if (!headVisInitialized)
							{
								foreach (Transform child in transform) child.gameObject.GetComponentInChildren<MeshRenderer>().enabled = false;

								headVisInitialized = true;
							}

							transform.position = headset.transform.position;
							transform.rotation = headset.transform.rotation;
						}
					}
				}
			</code>
		</remarks>
	</summary>
	**/
	void Update()
	{
		if (!(0 <= myID && myID <= 2)) return;

		if (!startFlag)
		{
			initRef = GameObject.FindGameObjectWithTag("init");
			//startFlag = true;
			if (hasAuthority)
			{
				//oscPacketBus = GameObject.FindGameObjectWithTag ("OSCPacketBus");
				CmdSendOSC(myID);
				startFlag = true;
				//oscPacketBus.GetComponent<OSCPacketBus> ().activateHead (myID, gameObject);
			}

			if (initRef) //DOES NOTHING
			{
				//initRef.GetComponent<shadeSwapper> ().updateBodyPosArray (new Vector3(transform.position.x,
				//	transform.position.y - 2f, transform.position.z));
			}

			if (isServer) //DOES NOTHING
			{
				//oscPacketBus.GetComponent<OSCPacketBus> ().UpdateHead (myID, gameObject.transform.position, gameObject.transform.rotation);
			}

			if (hasAuthority)
			{
				//CmdOSCSendMessage (myID);
				if (headVisInitialized == false)
				{
					foreach (Transform child in transform) child.gameObject.GetComponentInChildren<MeshRenderer>().enabled = false;

					headVisInitialized = true;
				}

				transform.position = headset.transform.position;
				transform.rotation = headset.transform.rotation;
			}
		}
	}
	#endregion

	#region PRIVATE_FUNC
	/**
	<summary>
		FD : OnChargePlayerID(int)
		Set v:myID to inID
		<param name="inID"/>
	</summary>
	**/
	void OnChangePlayerID(int inID)
	{
		myID = inID;
	}

	/**
	<summary>
		FD : CmdSendOSC(int)
		Activate OSCPacketBust tagged object
		<param name="inID"/>
	</summary>
	**/
	[Command]
	void CmdSendOSC(int inID)
	{
		GameObject.FindGameObjectWithTag("OSCPacketBus").GetComponent<OSCPacketBus>().activateHead(inID, this.gameObject);
	}
	#endregion

	#region COMMENTED_CODE
	/*
	[Command]
	void CmdStartOSC() {
		if (!OSCHandler.Instance) {
			OSCHandler.Instance.Init ();
		}
	}*/

	/*
	[Command]
	void CmdOSCSendMessage(int playerID) {
		oscPacketBus.GetComponent<OSCPacketBus> ().UpdateHead (playerID, gameObject.transform.position, gameObject.transform.rotation);
//		if (OSCHandler.Instance.Clients.ContainsKey ("SuperCollider")) {
//
//			//OSCHandler.Instance.SendMessageToClient ("SuperCollider", "/headPhoneID", base.connectionToClient.connectionId);
//			OSCHandler.Instance.SendMessageToClient ("SuperCollider", "/hpx" + clientID, gameObject.transform.position.x);
//			OSCHandler.Instance.SendMessageToClient ("SuperCollider", "/hpy" + clientID, gameObject.transform.position.y);
//			OSCHandler.Instance.SendMessageToClient ("SuperCollider", "/hpz" + clientID, gameObject.transform.position.z);
//			OSCHandler.Instance.SendMessageToClient ("SuperCollider", "/hrx" + clientID, gameObject.transform.rotation.x);
//			OSCHandler.Instance.SendMessageToClient ("SuperCollider", "/hry" + clientID, gameObject.transform.rotation.y);
//			OSCHandler.Instance.SendMessageToClient ("SuperCollider", "/hrz" + clientID, gameObject.transform.rotation.z);
//			OSCHandler.Instance.SendMessageToClient ("SuperCollider", "/hrw" + clientID, gameObject.transform.rotation.w);
//			//Debug.Log ();
//			//Debug.Log (gameObject.transform.position.x.ToString ());
//		}
	}
	*/
	#endregion
}
