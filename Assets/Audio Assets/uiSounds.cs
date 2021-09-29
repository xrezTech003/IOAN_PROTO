using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AK.Wwise;
using Mirror;
/// <summary>
/// Used to generate sounds for UI of each player 
/// </summary>
public class uiSounds : NetworkBehaviour
{
    #region PUBLIC VARIABLES
    /// <summary>
    /// For testing 
    /// </summary>
    public bool test = false;
    /// <summary>
    /// Stores if this instance is local
    /// </summary>
    public bool localPlayerUI = false;
    /// <summary>
    /// Stores the player ID found in the playerMover script on creation 
    /// </summary>
    public int playerID = -1;
    #endregion

    #region OBJECT REFERENCES
    [Tooltip("only for testing")]
    public GameObject leftController = null;
    [Tooltip("only for testing")]
    public GameObject rightController = null;
    #endregion

    #region PRIVATE VARIABLES
    /// <summary>
    /// Object to link the sonifier to
    /// </summary>
    private GameObject linkedObject;
    #endregion

    #region PUBLIC FUNCTIONS
    /// <summary>
    /// Creates a grab sound for the player who grabbed a star.  The function will check and make sure the player is local and if the activated star came from that player 
    /// </summary>
    /// <param name="gameObject">Game object the sound will come from</param>
    /// <param name="player">the player who activated the star</param>
    public void grabSound(GameObject gameObject, int player) //UI Sound for grabbing an object
    {
        if (isLocalPlayer && (player == playerID))
        {
            AkSoundEngine.PostEvent("grabObject", gameObject);
        }

    }
    /// <summary>
    /// Creates a grab sound for the player who spawned a star.  The function will check and make sure the player is local and if the activated star came from that player 
    /// </summary>
    /// <param name="gameObject">Game object the sound will come from</param>
    /// <param name="player">the player who activated the star</param>
    public void spawnSound(GameObject linkedObject, int player) //UI Sound for releasing an object
    {
        if (isLocalPlayer && (player == playerID))
        {
            AkSoundEngine.PostEvent("releaseObject", linkedObject);
        }
    }
    /// <summary>
    /// Creates a grab sound for the player who returned a star.  The function will check and make sure the player is local and if the activated star came from that player 
    /// </summary>
    /// <param name="gameObject">Game object the sound will come from</param>
    /// <param name="player">the player who activated the star</param>
    public void returnSound(GameObject gameObject, int player) //UI Sound for returning an object to mesh
    {
        if (isLocalPlayer && (player == playerID))
        {
            AkSoundEngine.PostEvent("releaseObject", gameObject);
        }
    }
    /// <summary>
    /// Creates a grab sound for the player who handed off a star.  The function will check and make sure the player is local and if the activated star came from that player 
    /// </summary>
    /// <param name="gameObject">Game object the sound will come from</param>
    /// <param name="player">the player who activated the star</param>
    public void handoffSound(GameObject gameObject, int player) //UI Sound for handoff
    {
        if (isLocalPlayer && (player == playerID))
        {
            AkSoundEngine.PostEvent("passObject", gameObject);
        }
    }

    #endregion

    private void Start()
    {
        localPlayerUI = isLocalPlayer;

        if(isLocalPlayer)
        {
            ///<remarks>Get and check the config for Client/server status - Case insensitive</remarks>
			Config config = Config.Instance;
            playerID = config.Data.myID;

            if (config.Data.serverStatus == "server" || config.Data.serverStatus == "Server")
            {
                playerID = 0;
            }
        }
        //leftController = GameObject.Find("Controller (left)");
    }
    private void Update()
    {
        if (test == true)
        {
            spawnSound(leftController, playerID);
            test = false;
        }
    }

}






