using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AuthoritySphere : MonoBehaviour
{
    activatedStarScript activatedStar;
    iOANHeteroController controller;
    
    iOANHeteroPlayer playerNode;
    bool authority;
    private void Start()
    {
        controller = gameObject.GetComponentInParent<iOANHeteroController>();
        playerNode = playerNode = controller.gameObject.GetComponentInParent<iOANHeteroPlayer>();
    }
    void OnCollisionEnter(Collision other)
    {
        if (other.collider.gameObject.GetComponent<activatedStarScript>() != null)
        {
            activatedStar = other.collider.gameObject.GetComponent<activatedStarScript>();
            NetworkIdentity starNetID = activatedStar.GetComponent<NetworkIdentity>();
            playerNode.CmdSetAuthority(starNetID);
        }
        
    }
    void OnCollisionExit(Collision other)
    {
        if (other.collider.gameObject.GetComponent<activatedStarScript>() != null)
        {
            activatedStar = other.collider.gameObject.GetComponent<activatedStarScript>();
            NetworkIdentity starNetID = activatedStar.GetComponent<NetworkIdentity>();
            starNetID.RemoveClientAuthority();
        }

    }
}
