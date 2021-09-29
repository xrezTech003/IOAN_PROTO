using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
//using UnityOSC;
using System.IO;

/**
<summary>
	CD : headScript
	Disable all child meshes and follow headset position
</summary>
**/
public class headScript : NetworkBehaviour 
{
	#region PUBLIC_VAR
	/// <summary>
	///		VD : myID
	///		Object ID I_A ::: Player ID (0, 1, 2)
	/// </summary>
	[SyncVar(hook = "OnChangePlayerID")]
	public int myID;

	/// <summary>
	///		VD : initRef
	///		Initialized Reference
	/// </summary>
	public GameObject initRef;

	/// <summary>
	///		VD : headset
	///		Used Headset
	/// </summary>
	public GameObject headset;

	/// <summary>
	///		VD : myColor
	///		Paired with PlayerID - should match controller scripts (PlayerMove and netControlR, (R, G, B) NDH
	/// </summary>
	[SyncVar]
	public Color myColor;

	/// <summary>
	///		VD : headVisInitialized
	///		Boolean check for whether or not HMD model mesh is being rendered
	/// </summary>
	public bool headVisInitialized;
	#endregion

	#region PROTECTED_VAR
	/// <summary>
	///		VD : theSourceFile
	/// </summary>
	protected FileInfo theSourceFile = null;

	/// <summary>
	///		VD : text
	/// </summary>
	protected string text = " ";
	#endregion

	#region PRIVATE_VAR
	/// <summary>
	///		VD : startFlag
	/// </summary>
	private bool startFlag = false;

	AkAudioListener audioListener;
	#endregion

	#region UNITY_FUNC
	/**
	<summary>
		FD : Start()
		If v:myID < 0 or v:myID > 2 
			disable all meshes in children
			return
		Set v:headVisInitialized to false
		Set v:headset to mainCamera
		Set color of children
		Return if doesn't have authority //Would return anyway
	</summary>
	**/
	void Start()
	{
		if (!(0 <= myID && myID <= 2))
		{
			foreach (Transform child in transform)
                child.gameObject.GetComponentInChildren<MeshRenderer>().enabled = false;

			return;
		}

		headVisInitialized = false;
		headset = GameObject.FindGameObjectWithTag("MainCamera");
		//gameObject.GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", myColor * Mathf.LinearToGammaSpace(1.0f));
		//TODO: Temp disabled to test tag reclouring
	

		if (!hasAuthority) return;
	}

	/**
	<summary>
		FD : Update()
		Return if v:myID < 0 or v:mvID > 2
		If v:startFlag is false
			set v:initRef to object tagged with init
			If it has authority
				Call CmdSendOSC with myID
				Set v:startFlag to true
		Return if doesn't have authority
		if v:headVisInitialized is false
			set all child meshes to false
			set v:headVisInitialized to true
		Set transform data to v:headset transform data
	</summary>
	**/
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
			}
		}

		if (!hasAuthority) return;

		if (headVisInitialized == false)
		{
			foreach (Transform child in transform) child.gameObject.GetComponentInChildren<MeshRenderer>().enabled = false;

			headVisInitialized = true;
		}

		transform.position = headset.transform.position;
		transform.rotation = headset.transform.rotation;
	}
	#endregion

	#region PRIVATE_FUNC
	/**
	<summary>
		FD : OnChangePlayerID(int)
		Set v:myID to inID
		<param name="inID"/>
	</summary>
	**/
	void OnChangePlayerID(int oldID, int inID)
	{
		myID = inID;
	}

	/**
	<summary>
		FD : CmdSendOSC(int)
		Call activateHead with inID and this gameObject
		<param name="inID"/>
	</summary>
	**/
	[Command]
	void CmdSendOSC(int inID)
	{
		GameObject.FindGameObjectWithTag("OSCPacketBus").GetComponent<OSCPacketBus>().activateHead(inID, gameObject);
	}
	#endregion
}
