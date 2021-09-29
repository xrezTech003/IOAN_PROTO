using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CD: GS: Only attched to one prefab, the same one as starSerializer, and it's also disabled by default and never reenabled, so I thknk it's expired
///  ::: Otherwise it would have just been used to give something a halo when collided with ::: was this a beta feature?
/// </summary>
public class ColliderScript : MonoBehaviour {

	float timeLeft;

	/// <summary>
	/// FD: Moot, see CD
	/// </summary>
	public void onHit(){
		//Vector3 myCoord = gameObject.GetComponent<StarSerializer> ().meshCoord;
		Behaviour halo = (Behaviour)GetComponent ("Halo");
		//halo.transform.position = myCoord;
		halo.enabled = true;
		timeLeft = 0.125f;
	}

	/// <summary>
	/// FD: Moot, see CD
	/// </summary>
	// Use this for initialization
	void Start () {
		timeLeft = 0.0f;
	}

	/// <summary>
	/// FD: Moot, see CD
	/// </summary>
	// Update is called once per frame
	void Update () {
		timeLeft -= Time.deltaTime;
		if (timeLeft <= 0.0f) {
			Behaviour halo = (Behaviour)GetComponent ("Halo");
			//halo.transform.position = myCoord;
			halo.enabled = false;
		}
	}

}
