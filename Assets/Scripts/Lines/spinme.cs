using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is a likely stolen script which rotates an object consisently about the y axis
///  ::: IV: Seems either useless or used in some abstract shader based way
/// </summary><param name="Start"><summary>Creates a neutral Qaurternion to set initial rotation</summary></param>
/// <param name="Update"><summary>Set's the objects ongoing rotation</summary></param>
public class spinme : MonoBehaviour {

	// Use this for initialization
	/// <summary>
	/// Set's the objects initial rotation
	/// </summary>
	void Start () {

		var q = new Quaternion();

        q.eulerAngles = new Vector3(2.0f,2.0f,0.0f);
  
        //Debug.Log(q);
	}

	// Update is called once per frame
	/// <summary>
	/// Set's the objects ongoing rotation
	/// </summary>
	void Update ()
	{

		var rot = new Quaternion();

		rot.SetAxisAngle(new Vector3(0, 1, 0), Time.realtimeSinceStartup);

		//transform.rotation = rot;
	}
}
