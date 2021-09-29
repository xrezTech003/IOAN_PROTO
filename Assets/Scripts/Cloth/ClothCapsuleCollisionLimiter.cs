using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CD: ClothCapsuleCollisionLimiter this class contains the a way to prevent the cloth from being infinitely pushed down by the controllers collider.
/// </summary>
public class ClothCapsuleCollisionLimiter : MonoBehaviour
{
   CapsuleCollider myCollider;
   Collider otherCollider;
    /// <summary>
    /// IV: Remove this maybe on line 15? 
    /// </summary>
	//float collidionDist = -1;
    // Start is called before the first frame update
    /// <summary>
    /// FD: Start(): gets capsule collider on init of script
    /// </summary>
    void Start()
    {
		myCollider = GetComponent<CapsuleCollider>();

    }

    /// <summary>
    /// IV: Remove this maybe on line 26? 
    /// </summary>
    /*
    void OnCollisionEnter(Collision collision)
    {
	//	otherCollider = collision.collider;
	//	myCollider.transform.position = new Vector3 (myCollider.transform.position.x, .10f, myCollider.transform.position.z);
	//	collidionDist = Vector3.Distance(otherCollider.transform.position, transform.parent.position);
		Debug.Log ("WE HIT SOMETHING");
    }
    */

    // Update is called once per frame
    /// <summary>
    /// FD: Update(): If the collider is below the allowed threshold, put it there. 
    /// </summary>
    void Update()
    {
        //)))check to re-enable capsule collider when far enough away 
        //###Probably should remove this
        /*if (collidionDist >= 0) { 
			float curDist = Vector3.Distance (otherCollider.transform.position, transform.parent.position);
			if (curDist >= 1.5 * collidionDist) {
				otherCollider = null;
				collidionDist = -1;
			} else {
				myCollider.transform.position = new Vector3 (myCollider.transform.position.x, .10f, myCollider.transform.position.z);
			}
		}*/

        /// <summary>
        /// CM: This is to basically hold the capsule collider at a set y distance into the cloth so it can't continually press the cloth further.
        /// </summary>
        if (transform.parent.position.y <= -0.006f) {
			myCollider.transform.position = new Vector3 (myCollider.transform.position.x, -0.006f, myCollider.transform.position.z);
		} else {
			myCollider.transform.position = new Vector3 (myCollider.transform.position.x, transform.parent.position.y, myCollider.transform.position.z);
		}



    }
}
