using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SetupManager : NetworkBehaviour
{
	#region PUBLIC_VAR
	/// <summary>
	///		VD : playerDataList
	///		Not used in Script
	/// </summary>
	public List<PlayerData> playerDataList;

	/// <summary>
	///		VD : centerPoint
	///		CenterPoint between all cameras
	/// </summary>
	public Vector3 centerPoint;

	public bool setup_on = false;

	public GameObject wwise_obj;
	#endregion

	#region PRIVATE_VAR
	/// <summary>
	///		VD : cams
	///		Holds Camera components for all child objects
	/// </summary>
	Camera[] cams;
	AkAudioListener serverAudio;

	/// <summary>
	///		VD : permute
	///		Will be updated to iterate through 0-5 on loop
	/// </summary>
	int permute = 0;
	#endregion

	
	#region UNITY_FUNC
	/**
	<summary>
		FD : Start()
		Set v:cams to all Cameras in child objects
		Call f:setCamsFromConfig()
	</summary>
	**/
	void Start()
	{
		if(!isClient)
        {
			cams = new Camera[3]; //This should be init with transform.childCount
			serverAudio = GetComponentInChildren<AkAudioListener>();
			for (int i = 0; i < transform.childCount; i++) cams[i] = transform.GetChild(i).gameObject.GetComponent<Camera>(); //Or this should only loop through 3 times

			setAudioAndCamFromConfig();
		}
	}

	/**
	<summary>
		FD : Update()
		Set v:cam[0] position to controller position if '0' button is pressed
		Set v:cam[1] position to controller position if '1' button is pressed
		Set v:cam[2] position to controller position if '2' button is pressed
		If '4' button is pressed
			Set v:centerPoint to controller position
			Call f:turnToCenter()
		If '5' button is pressed
			Switch based on v:permute to set the targetDisplay values for each v:cam[] to a different permutation of 1-3
			add one to permute then mod it by 6 to wrap around
		Call f:updateConfigFile() if '6' is pressed
	</summary>
	**/
	void Update()
	{
		//Debug.Log(Input.GetKeyDown(KeyCode.Tilde));
		if (Input.GetKeyDown(KeyCode.Equals) && isServer)
		{

			if (!setup_on)
			{
				Debug.Log("ENTERING CAM SETUP");
				setup_on = true;
				enableSetupMode(false);
				Rpc_enableSetup(false);
			}
			
		}
		if(Input.GetKeyDown(KeyCode.Minus) && isServer)
        {
			if (setup_on)
			{
				Debug.Log("EXITING CAM SETUP");
				setup_on = false;
				disableSetupMode();
				Rpc_disableSetup();

			}
		}
		if (Input.GetKeyDown(KeyCode.BackQuote) && setup_on && isServer)
		{
			Debug.Log("Saving to Config for setup"); 
			updateConfigFile();
		}
		if (Input.GetKeyDown(KeyCode.Alpha0) && setup_on && isServer)
        {
			Debug.Log("Resetting Positions");
			resetPositions();
			Rpc_resetPositions();
		}
	}
	#endregion

	#region PRIVATE_FUNC

	[Command]
	public void Cmd_enableSetup(bool reset_pos)
    {
		enableSetupMode(reset_pos);
	}

	[ClientRpc]
	public void Rpc_enableSetup(bool reset_pos)
	{
		enableSetupMode(reset_pos);
	}

	[Command]
	public void Cmd_disableSetup()
	{
		disableSetupMode();
	}

	[ClientRpc]
	public void Rpc_disableSetup()
	{
		disableSetupMode();
	}

	[ClientRpc]
	public void Rpc_resetPositions()
	{
		resetPositions();	
	}
	
	public void resetPositions()
    {
		GameObject[] sCams = GameObject.FindGameObjectsWithTag("sCam");
		foreach (GameObject sCam in sCams)
		{
			if (isClient)
			{
				sCam.GetComponent<Camera>().enabled = false;
			}
			if (sCam.name == "stationaryCamera1")
			{
				sCam.transform.position = new Vector3(0.3f, 0.5f, -0.3f);
			}
			else if (sCam.name == "stationaryCamera2")
			{
				sCam.transform.position = new Vector3(-0.3f, 0.5f, -0.3f);
			}
			else
			{
				sCam.transform.position = new Vector3(0.0f, 0.5f, -0.3f);
			}
		}
	}

	void disableSetupMode()
	{
		if (isClient)
		{
			wwise_obj.SetActive(false);
		}

		serverClientSwitch sCS = GameObject.FindGameObjectWithTag("configObj").GetComponent<serverClientSwitch>();

		GameObject[] cBodies = GameObject.FindGameObjectsWithTag("Camera Body");

		foreach (GameObject cBody in cBodies)
		{
			cBody.GetComponent<MeshRenderer>().enabled = false;
		}

		GameObject[] sSqs = GameObject.FindGameObjectsWithTag("setupsq");
		foreach (GameObject sq in sSqs)
		{
			sq.GetComponent<Canvas>().enabled = false;
		}

		if (isClient)
		{

			GameObject[] playerObjs = GameObject.FindGameObjectsWithTag("heteroPlayer");
			foreach (GameObject player in playerObjs)
			{
				CameraGrabber[] controlscrps2 = player.GetComponentsInChildren<CameraGrabber>(true);
				foreach (CameraGrabber cs in controlscrps2)
				{
					cs.enabled = false;
				}

				iOANHeteroController[] controlscrps = player.GetComponentsInChildren<iOANHeteroController>(true);
				foreach (iOANHeteroController cs in controlscrps)
				{
					cs.enabled = true;
				}

			}
		}

	}

	void enableSetupMode(bool reset_pos)
	{
		if (isClient)
		{
			wwise_obj.SetActive(true);
		}


		serverClientSwitch sCS = GameObject.FindGameObjectWithTag("configObj").GetComponent<serverClientSwitch>();

		sCS.stationaryCams.SetActive(true);

		GameObject[] sSqs = GameObject.FindGameObjectsWithTag("setupsq");
		foreach (GameObject sq in sSqs)
		{
			sq.GetComponent<Canvas>().enabled = true;
		}

		

		GameObject[] cBodies = GameObject.FindGameObjectsWithTag("Camera Body");

		foreach (GameObject cBody in cBodies)
		{
			cBody.GetComponent<MeshRenderer>().enabled = true;
		}

		if(isClient)
        {

			GameObject[] playerObjs = GameObject.FindGameObjectsWithTag("heteroPlayer");
			foreach(GameObject player in playerObjs)
            {
				iOANHeteroController[] controlscrps = player.GetComponentsInChildren<iOANHeteroController>(true);
				foreach(iOANHeteroController cs in controlscrps)
                {
					cs.enabled = false;
                }

				CameraGrabber[] controlscrps2 = player.GetComponentsInChildren<CameraGrabber>(true);
				foreach (CameraGrabber cs in controlscrps2)
				{
					cs.enabled = true;
				}
			}
		}
	}
    /**
	<summary>
		FD : setCamsFromConfig()
		Set all v:cams[] position to config file data with specific default values if no config data is present
		Set all v:cams[] targetDisplay to config file data with specific default values if no config data is present
		Set v:centerPoint to config file data with specific default values if no config data is present
		Call f:turnToCenter()
	</summary>
	**/
    public void setAudioAndCamFromConfig()
	{
		Config config = Config.Instance;

		cams[0].transform.position = config.Data.cam0Position;
		cams[0].transform.rotation = config.Data.cam0Rotation;

		cams[1].transform.position = config.Data.cam1Position;
		cams[1].transform.rotation = config.Data.cam1Rotation;

		cams[2].transform.position = config.Data.cam2Position;
		cams[2].transform.rotation = config.Data.cam2Rotation;

		cams[0].targetDisplay = config.Data.cam0TargetDisplay;
		cams[1].targetDisplay = config.Data.cam1TargetDisplay;
		cams[2].targetDisplay = config.Data.cam2TargetDisplay;

		//centerPoint = config.Data.camCenterPoint;
		//turnToCenter();

		//Also set the audio listener 
		wwise_obj.transform.position = config.Data.wwPosition;
        wwise_obj.transform.rotation = Quaternion.Euler(config.Data.wwRotation);
	}

	/**
	<summary>
		FD : updateConfigFile()
		Set data in the config file to the current values of every v:cams[i]
		Write all config data to file
	</summary>
	**/
	void updateConfigFile()
	{
		Config config = Config.Instance;

		print("Cam 0 stuff");
		print(cams[0].transform.position);

		print("Cam 1 stuff");
		print(cams[1].transform.position);

		print("Cam 2 stuff");
		print(cams[2].transform.position);

		print("ww stuff");
		print(wwise_obj.transform.position);

		config.Data.cam0Position = cams[0].transform.position;
		config.Data.cam0Rotation = cams[0].transform.rotation;

		config.Data.cam1Position = cams[1].transform.position;
		config.Data.cam1Rotation = cams[1].transform.rotation;

		config.Data.cam2Position = cams[2].transform.position;
		config.Data.cam2Rotation = cams[2].transform.rotation;

		config.Data.cam0TargetDisplay = cams[0].targetDisplay;
		config.Data.cam1TargetDisplay = cams[1].targetDisplay;
		config.Data.cam2TargetDisplay = cams[2].targetDisplay;

		//Set audio listener for server
		config.Data.wwPosition = wwise_obj.transform.position;
        config.Data.wwRotation = wwise_obj.transform.rotation.eulerAngles;

		//config.Data.camCenterPoint = centerPoint;

		config.WriteToJson();
	}

	/**
	<summary>
		FD : turnToCenter()
		For every Camera in v:cams
		Reset the v:centerPoint y value to the Camera y position value
		Set the Camera rotation to the normalized vector between the centerpoint and the Camera position
	</summary>
	**/
	void turnToCenter()
	{
		for (int i = 0; i < cams.Length; i++)
		{
			Camera cam = cams[i];
			centerPoint.y = cam.transform.position.y;
			Vector3 diff = (centerPoint - cam.transform.position).normalized;
			cam.transform.rotation = Quaternion.LookRotation(diff); //Quaternion.LookRotation((centerPoint - cam.transform.position).normalized);
		}
	}
	#endregion
}
