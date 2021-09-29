using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/**
<summary>
	CD : serverClientSwitch
	Will Load client, server, or camera objects based on config
</summary>
**/
public class serverClientSwitch : MonoBehaviour
{
    #region PUBLIC_VAR
	/// <summary>
	///		VD : clientControl
	/// </summary>
    public GameObject viveControl;
	/// <summary>
	///		VD : clientControl
	/// </summary>
	public GameObject ovrControl;

	/// <summary>
	///		VD : serverControl
	/// </summary>
	public GameObject serverControl;

	/// <summary>
	///		VD : stationaryCams
	/// </summary>
	public GameObject stationaryCams;
	#endregion
	#region UNITY_FUNC
	/**
	<summary>
		FD : Start()
		Based on the data taken from the config file in the config.cs script, loads the three pubilc variables accordingly
	</summary>
	**/
	void Start ()
    {
		Config config = Config.Instance;
        string serverStatus = config.Data.serverStatus;
		serverStatus = serverStatus.ToLower();

		if (serverStatus == "server")
		{
			Debug.Log("Server system detected");

			if (!config.Data.serverEnableVR)
			{
				ovrControl.SetActive(false);
				viveControl.SetActive(false);
			}
			else
			{
				Debug.Log("HMD and controls are on due to everide in cfg.txt");
				viveControl.SetActive(true);
				if (OVRPlugin.GetSystemHeadsetType() != 0) ovrControl.SetActive(true);
				
			}

			serverControl.SetActive(true);
			stationaryCams.SetActive(true);
		}
		else
		{
			if (serverStatus == "client") Debug.Log("Client system detected");
			else
			{
				Debug.LogError("Error reader server/client switch. Is something wrong with config/cgf.txt?\n" +
				"Falling back to client.");
			}
			viveControl.SetActive(true);
			if (OVRPlugin.GetSystemHeadsetType() != 0) ovrControl.SetActive(true);
			serverControl.SetActive(false);
			//stationaryCams.SetActive(true);
			GameObject[] sCams = GameObject.FindGameObjectsWithTag("sCam");
			foreach (GameObject sCam in sCams)
			{
				sCam.SetActive(false);
			}
		}
	}
    #endregion
}
