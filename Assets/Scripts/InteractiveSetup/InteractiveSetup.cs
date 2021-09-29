using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
<summary>
	CD : InteractiveSetup
	Switch the positions of all cameras while in game
</summary>
**/
public class InteractiveSetup : MonoBehaviour
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

	public bool is_Serving = false;
	#endregion

	#region PRIVATE_VAR
	/// <summary>
	///		VD : cams
	///		Holds Camera components for all child objects
	/// </summary>
	Camera[] cams;

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
		Config config = Config.Instance;

		if(config.Data.serverStatus == "server")
        {
			is_Serving = true;

			cams = new Camera[3]; //This should be init with transform.childCount

			for (int i = 0; i < transform.childCount; i++) cams[i] = transform.GetChild(i).gameObject.GetComponent<Camera>(); //Or this should only loop through 3 times

			setCamsFromConfig();
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
		if (is_Serving)
		{
			if (Input.GetKeyDown(KeyCode.Alpha0)) cams[0].transform.position = getControllerPosition();

			if (Input.GetKeyDown(KeyCode.Alpha1)) cams[1].transform.position = getControllerPosition();

			if (Input.GetKeyDown(KeyCode.Alpha2)) cams[2].transform.position = getControllerPosition();

			if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				centerPoint = getControllerPosition();
				turnToCenter();
			}

			if (Input.GetKeyDown(KeyCode.Alpha5))
			{
				switch (permute)
				{
					case 0:
						cams[0].targetDisplay = 1;
						cams[1].targetDisplay = 2;
						cams[2].targetDisplay = 3;
						break;
					case 1:
						cams[0].targetDisplay = 1;
						cams[1].targetDisplay = 3;
						cams[2].targetDisplay = 2;
						break;
					case 2:
						cams[0].targetDisplay = 2;
						cams[1].targetDisplay = 1;
						cams[2].targetDisplay = 3;
						break;
					case 3:
						cams[0].targetDisplay = 2;
						cams[1].targetDisplay = 3;
						cams[2].targetDisplay = 1;
						break;
					case 4:
						cams[0].targetDisplay = 3;
						cams[1].targetDisplay = 1;
						cams[2].targetDisplay = 2;
						break;
					default:
						cams[0].targetDisplay = 3;
						cams[1].targetDisplay = 2;
						cams[2].targetDisplay = 1;
						break;
				}

				permute += 1;
				permute %= 6;
			}

			if (Input.GetKeyDown(KeyCode.Alpha6)) updateConfigFile();
		}
	}
	#endregion

	#region PRIVATE_FUNC
	/**
	<summary>
		FD : setCamsFromConfig()
		Set all v:cams[] position to config file data with specific default values if no config data is present
		Set all v:cams[] targetDisplay to config file data with specific default values if no config data is present
		Set v:centerPoint to config file data with specific default values if no config data is present
		Call f:turnToCenter()
	</summary>
	**/
	void setCamsFromConfig()
	{
		Config config = Config.Instance;

        cams[0].transform.position = config.Data.cam0Position;
		//cams[0].transform.rotation = config.Data.cam0Rotation;

        cams[1].transform.position = config.Data.cam1Position;
		//cams[1].transform.rotation = config.Data.cam1Rotation;

		cams[2].transform.position = config.Data.cam2Position;
		//cams[2].transform.rotation = config.Data.cam2Rotation;

		cams[0].targetDisplay = config.Data.cam0TargetDisplay;
        cams[1].targetDisplay = config.Data.cam1TargetDisplay;
        cams[2].targetDisplay = config.Data.cam2TargetDisplay;

        centerPoint = config.Data.camCenterPoint;

		turnToCenter();
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

		config.Data.cam0Position = cams[0].transform.position;
		config.Data.cam1Position = cams[1].transform.position;
		config.Data.cam2Position = cams[2].transform.position;

		config.Data.cam0TargetDisplay = cams[0].targetDisplay;
		config.Data.cam1TargetDisplay = cams[1].targetDisplay;
		config.Data.cam2TargetDisplay = cams[2].targetDisplay;

		config.Data.camCenterPoint = centerPoint;
	}

	/**
	<summary>
		FD : getControllerPosition()
		Find the OSCPacketBus, find the first playerdata, return its position
		<reamrks>
			IV
			<code>
				Vector3 getControllerPosition()
				{
					return GameObject.FindGameObjectWithTag("OSCPacketBus").GetComponent<OSCPacketBus>().playerDataList[0].contL.transform.position;
				}
			</code>
		</reamrks>
	</summary>
	**/
	Vector3 getControllerPosition()
	{
		OSCPacketBus osc = GameObject.FindGameObjectWithTag("OSCPacketBus").GetComponent<OSCPacketBus>();
		PlayerData data = osc.playerDataList[0];
		return data.contL.transform.position; //return osc.playerDataList[0].contL.transform.position
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
