using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
///		CD : highlightedIDScript
///		Modify transform based on player
///		<remarks> ::: IV: The variables that are used in this script are barely adjusted, and the playerRef doesn;t seem to have  away to be set to the player. 
///		It starts set to the init Object, but that object doesn't have a PlayerMove, so the Update is messed up, essentially just .
///		Unless there's a bit of network stuff I don't understand, half of this script either shouldn't work or just won't do anything. With that,
///		when do starIDs popup at all in the experience? Is this an outdated step-down from the infoboxes on tap? Could we fix it by setting the PlayerRef in a way that makes more sense?
///		NDH</remarks>
/// </summary>
public class highlightedIDScript : NetworkBehaviour 
{
	#region PUBLIC_VAR
	/// <summary>
	///		VD : playerRef by default set to init, makes for some funny looking gimicks -- NDH
	/// </summary>
	public GameObject playerRef;

	/// <summary>
	///		VD : initRef -- self set in f:Start literally a reference to the Initializer object
	/// </summary>
	public GameObject initRef;

	/// <summary>
	///		VD : myID Value between 0 and 2 for player tracking - NDH
	/// </summary>
	[SyncVar]
	public int myID;

	/// <summary>
	///		VD : yOff -- Always 0 - NDH
	/// </summary>
	[SyncVar(hook = "OnChangeYOff")]
	public float yOff;

	/// <summary>
	///		VD : xzPos - NDH - Fed a value from self.Update if the playerRef is not empty, but it brings up a weird issue, see lass Description
	/// </summary>
	[SyncVar(hook = "OnChangeXZPos")]
	public Vector2 xzPos;
	#endregion

	#region PRIVATE_VAR
	/// <summary>
	///		VD : startFlag
	/// </summary>
	private bool startFlag = false;

	/// <summary>
	///		VD : starIReference ID for the star being raycasted
	/// </summary>
	[SyncVar]
	int starID;
	#endregion

	#region UNITY_FUNC
	/**
	<summary>
		FD : Start()
		Set v:initRef to Object with tag "init"
		Set v:yOff to 0
	</summary>
	**/
	void Start()
	{
		initRef = GameObject.FindGameObjectWithTag("init");
		//playerRef = GameObject.FindGameObjectWithTag("heteroPlayer")
		yOff = 0f;
	}

	/**
	<summary>
		FD : Update()
		If the object has authority
			If v:startFlag is false
				Find all Objects with tag as "player" that are local players and set v:playerRef to playerObj
				Set v:startFlag to true
			If v:playerRef isn't null
				Call CmdSetText with v:playerRef selectedID
				Set position to curStarPos x, z and curStarPosOld y
				Call CmdSetXZPos with position x, z
				Call CmdSetYOff with playerRef oldYOffset
		Else
			Set position to v:xzPos if something is below v:xzPos
		Look at camera
		Rotate 180 degrees
		Set text tp v:starID
		If v:initRef isn't null
			Find object with "init" tag
		Else
			Update highlight pos of shadeswapper
	</summary>
	**/
	void Update()
	{
		/*if (hasAuthority)
		{
			if (!startFlag)
			{
				foreach (GameObject playerObj in GameObject.FindGameObjectsWithTag("player")) if (playerObj.GetComponent<NetworkBehaviour>().isLocalPlayer) playerRef = playerObj;

				startFlag = true;
			}

			if (playerRef)
			{
				CmdSetText(playerRef.GetComponent<PlayerMove>().selectedID);
				transform.position = new Vector3(playerRef.GetComponent<PlayerMove>().curStarPos.x,
												 playerRef.GetComponent<PlayerMove>().curStarPosOld.y,
												 playerRef.GetComponent<PlayerMove>().curStarPos.z);
				CmdSetXZPos(new Vector2(transform.position.x, transform.position.z));
				CmdSetYOff(playerRef.GetComponent<PlayerMove>().oldYOffset);
			}
		}
		else
		{*/
			int layerMask2 = LayerMask.GetMask("newnewCloth");
			RaycastHit hit2;
			if (Physics.Raycast(new Vector3(xzPos.x, 0.0f, xzPos.y) + new Vector3(0f, 20f, 0f), Vector3.down, out hit2, Mathf.Infinity, layerMask2))
                transform.position = new Vector3(xzPos.x, hit2.point.y + yOff, xzPos.y);
		//}

		transform.LookAt(Camera.main.transform.position);
		transform.Rotate(0.0f, 180.0f, 0.0f);

		GetComponent<TextMesh>().text = starID.ToString();

        /*
		if (!initRef) initRef = GameObject.FindGameObjectWithTag("init");
		else initRef.GetComponent<shadeSwapper>().UpdateHighlightPos(starID, myID, shadeSwapper.HighLightType.RAY_CAST, -1);
        */
	}

	/**
	<summary>
		FD : OnDestroy()
		If v:initRef isn't null
			Set Shadeswapper values
	</summary>
	**/
	void OnDestroy()
	{
		if (initRef)
		{
            /*
			initRef.GetComponent<shadeSwapper>().UpdateHighlightPos(0, myID, shadeSwapper.HighLightType.LEFT, -1);
			initRef.GetComponent<shadeSwapper>().UpdateHighlightPos(0, myID, shadeSwapper.HighLightType.RIGHT, -1);
			initRef.GetComponent<shadeSwapper>().UpdateHighlightPos(0, myID, shadeSwapper.HighLightType.RAY_CAST, -1);
            */
		}
	}
	#endregion

	#region PRIVATE_FUNC
	/**
	<summary>
		FD : OnChangedXZPos(Vector2)
		Set v:xzPos to inXZPos
		<param name="inXZPos"/>
	</summary>
	**/
	void OnChangeXZPos(Vector2 oldXZPos, Vector2 inXZPos)
	{
		xzPos = inXZPos;
	}

	/**
	<summary>
		FD : OnChangedYOff(float)
		Set v:yOff to inYOff
		<param name="inYOff"/>
	</summary>
	**/
	void OnChangeYOff(float oldYOff, float inYOff)
	{
		yOff = inYOff;
	}

	/**
	<summary>
		FD : CmdSetXZPos(Vector2)
		Set v:xzPos to inPos
		<param name="inPos"/>
	</summary>
	**/
	[Command]
	void CmdSetXZPos(Vector2 inPos)
	{
		xzPos = inPos;
	}

	/**
	<summary>
		FD : CmdSetYOff(float)
		Set v:yOff to inYOff
		<param name=inYOff"/>
	</summary>
	**/
	[Command]
	void CmdSetYOff(float inYOff)
	{
		yOff = inYOff;
	}

	/**
	<summary>
		FD : CmdSetText(int)
		Set v:starID to inID
		<param name="inID"/>
	</summary>
	**/
	[Command]
	void CmdSetText(int inID)
	{
		starID = inID;
	}
	#endregion
}