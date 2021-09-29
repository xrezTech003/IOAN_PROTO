using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/**
<summary>
	CD : NetworkConfigurator :
	:: Set the server IP :
	:: Toggle showing the network configuration
</summary>
**/
public class NetworkConfigurator : MonoBehaviour
{
    #region PRIVATE_VAR
	/// <summary>
	///		VD : hud
	/// </summary>
    NetworkManagerHUD hud;
    #endregion

    #region UNITY_FUNC

	/**
	<summary>
		FD : Start()
		Get server IP for network manager
	</summary>
	**/
    void Start()
	{
		// TODO: use NewtorkMangaer allbacks to make startup easier?
		NetworkManager manager = GetComponent<NetworkManager>(); //GetComponent<NetworkManager>().networkAddress = Config.getConfig().get("serverIP", "169.254.169.47")
        manager.networkAddress = Config.Instance.Data.serverIP;

		hud = GetComponent<NetworkManagerHUD>();
	}

	/**
	<summary>
		FD : Update()
		Toggle showGUI if N key is pressed
	</summary>
	**/
	void Update() 
	{
		if (Input.GetKeyDown(KeyCode.N)) 
		{
			Debug.Log ("Toggling Network GUI");
			hud.showGUI = !hud.showGUI;
		}
	}
	#endregion
}
