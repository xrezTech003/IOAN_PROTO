using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
/// <summary>
/// This class enables and dissables audio listeners to ensure only one audio listner is avalible for wwise.  If this does not occure, the spatialization willl be messed up! There are listeners on the local player and in the server camera rig
/// </summary>
public class WwiseAudioListenerControl : NetworkBehaviour
{
    /// <summary>
    /// Marks this listener as the server listener 
    /// </summary>
    public bool serverListener;
    /// <summary>
    /// Ths listener attached to this game object
    /// </summary>
    AkAudioListener listener;
    // Start is called before the first frame update
    private void Start()
    {
        Config config = Config.Instance; //Get an instance of the config
        listener = GetComponent<AkAudioListener>();

        if (config.Data.serverStatus == "server")
        {
            if (!serverListener)
            {
                //Turn off this audio listener if it is not the server listener and the config is set to server
                listener.enabled = false;
                gameObject.SetActive(false);
            }
            else
            {

            }
        }
        else
        {
            if (serverListener)
            {
                //Turn off this audio listener if it is the server listener and the config is set to not server
                listener.enabled = false;
                gameObject.SetActive(false);
            }
            else
            {

            }
        }
    
}
}
