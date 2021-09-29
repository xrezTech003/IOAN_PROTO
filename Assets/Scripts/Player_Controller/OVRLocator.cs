using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
///  CD: Network behavior to check when the client starts, parents the ovr rig to oculus players
/// </summary>
public class OVRLocator : NetworkBehaviour
{
    private GameObject[] search;

    /// <summary>
    /// FD: On client start, find's hetero players and puts them under the ovr rig for input operations to work appropriately
    /// </summary>
    public override void OnStartClient()
    {
            search = GameObject.FindGameObjectsWithTag("heteroPlayer");

            foreach(GameObject found in search)
                if (found.GetComponent<iOANHeteroPlayer>().isOVR)
                    found.transform.parent = gameObject.transform;
  
    }
}
