using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.IO;
using UnityEngine.XR;

/// <summary>
/// CD: Primary script for the player's hmd ::: Set's color, disables model if local to remove visual artefacts, and set's self tracking to active headset (oculus/vive)
/// </summary>
public class iOANHeteroHead : MonoBehaviour
{   
    /// <summary>
    /// VD: Hard reference to the player node, which handles networking
    /// </summary>
    public iOANHeteroPlayer playerNode;
    /// <summary>
    /// VD: assigned at start via boolean check for oculus hmd, camera unity is using for the player to be presented with this hmd
    /// </summary>
    private GameObject trackedObject;
    private bool lateCheck = false;
    /// <summary>
    /// FD: Assign tracked object to correct hmd ::: Notice color is not handles here, tags are used and all color is top down from the playernode on the server
    /// </summary>
    void Start()
    {
        if (!playerNode.isLocalPlayer) return;

        if (playerNode.isOVR) trackedObject = GameObject.FindGameObjectWithTag("oculusCam");
        else trackedObject = GameObject.FindGameObjectWithTag("MainCamera");

        lateCheck = true;
    }
    /// <summary>
    /// Don't update anything if this script is not running on the local machine it represents, otherwise, follow the tracked object
    /// </summary>
    void Update()
    {
        if (!playerNode.isLocalPlayer) return; 

        if (!playerNode.isOVR && trackedObject == null) trackedObject = GameObject.FindGameObjectWithTag("MainCamera");

        if (lateCheck)
        {
            gameObject.transform.parent.transform.position = trackedObject.transform.position;
            gameObject.transform.parent.transform.rotation = trackedObject.transform.rotation;
            lateCheck = false;
        }

        gameObject.transform.position = trackedObject.transform.position;
        gameObject.transform.rotation = trackedObject.transform.rotation;
    }
}
