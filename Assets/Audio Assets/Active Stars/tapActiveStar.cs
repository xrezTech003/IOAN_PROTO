using UnityEngine;
using Mirror;
using JetBrains.Annotations;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ZT: Script to tap active stars. Uses collisions to trigger events on the star object
/// </summary>

public class tapActiveStar : MonoBehaviour
{
    #region PUBLICVARS
    /// <summary>
    /// The other controller 
    /// </summary>
    public tapActiveStar otherHand;
	/// <summary>
	/// The amount of time between taps before a star can be tapped again 
	/// </summary>
	public float tapDelay = 0.1f;
	#endregion
	#region PRIAVATEVARS
	/// <summary>
	/// Player node controller is attached to
	/// </summary>
	iOANHeteroPlayer playerNode;
	/// <summary>
	/// The controller this script is attached to
	/// </summary>
	iOANHeteroController controller;
	/// <summary>
	/// Star that is being collided with 
	/// </summary>
	activatedStarScript activatedStar;
	bool isLeft;
	SteamVR_Controller.Device steamInput;
	/// <summary>
	/// A list of busy stars that is checked every time a tap ininitiated 
	/// </summary>
	LinkedList<activatedStarScript> busyStars = new LinkedList<activatedStarScript>();
    #endregion

    #region UNITYFUNCTIONS
    private void Start()
    {
		
		controller = gameObject.GetComponentInParent<iOANHeteroController>();
		playerNode = controller.gameObject.GetComponentInParent<iOANHeteroPlayer>();
		isLeft = controller.isLeft;
		steamInput = controller.steamInput;
		if (otherHand != null)
        {
			otherHand.tapDelay = tapDelay;
        }
	}
	/// <summary>
	/// On the exit of the collision, the tap timer is activate.  The star will not be tappable until the timer finishes. 
	/// </summary>
	/// <param name="other"></param>
	private void OnCollisionExit(Collision other)
	{

		activatedStar = other.collider.gameObject.GetComponent<activatedStarScript>();

		if (!busyStars.Contains(activatedStar))
		{
			StartCoroutine(tapTimer(tapDelay, activatedStar));
		}

	}
	/// <summary>
	/// On enter, a star should change authority to the tapping player then issue a tap.
	/// </summary>
	/// <param name="other"></param>
	void OnCollisionEnter(Collision other)
	{


		activatedStar = other.collider.gameObject.GetComponent<activatedStarScript>();


		if (!busyStars.Contains(activatedStar))
		{
			if (activatedStar != null && activatedStar.dragState != activatedStarScript.DragState.ATTACHED && activatedStar.activated)
			{

				print("collision with active star and " + other);

				if (controller == null)
				{
					controller = gameObject.GetComponentInParent<iOANHeteroController>();
					playerNode = controller.gameObject.GetComponentInParent<iOANHeteroPlayer>();
				}

				isLeft = controller.isLeft;
				steamInput = controller.steamInput;
				ActiveStarPhysics physics = activatedStar.gameObject.GetComponentInChildren<ActiveStarPhysics>();


				Vector3 velocity3d;

				if (playerNode.isOVR) velocity3d = isLeft ? ((OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch)))
														  : ((OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch)));
				else velocity3d = controller.fullVelocity; //This is breaking for some reason				


				if (activatedStar.isClient && activatedStar.activated && playerNode.isLocalPlayer)
				{
					if (!activatedStar.hasAuthority)
					{
						//Change authority 
						NetworkIdentity starNetID = activatedStar.GetComponent<NetworkIdentity>();
						playerNode.CmdSetAuthority(starNetID);
					}

					StartCoroutine(Cr_initiate(activatedStar, physics, velocity3d));
				}


			}
		}


	}
    #endregion

    #region PRIAVATEFUNCTIONS
    /// <summary>
    /// Stored and removes a refrence to an active star that cannot be tapped for the duration of the timer 
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="activeStar"></param>
    /// <returns></returns>
    IEnumerator tapTimer(float delay, activatedStarScript activeStar)
    {
		busyStars.AddLast(activeStar);
		yield return new WaitForSeconds(delay);
		busyStars.Remove(activeStar);
		
    }

	/// <summary>
	/// Waits to initiate any events until the player has aithoriry over the star
	/// </summary>
	/// <param name="activeStar">Target Star</param>
	/// <param name="physics">Star Physics</param>
	/// <param name="velocity">Tap Velocity</param>
	/// <returns></returns>
	IEnumerator Cr_initiate(activatedStarScript activeStar, ActiveStarPhysics physics, Vector3 velocity)
    {
		System.Func<bool> authority = new System.Func<bool>(() => activeStar.hasAuthority);
		yield return new WaitUntil(authority);
		print("Activated Star Tapped!");
		print("tap active velocity: " + velocity);
		//Do Physics 
		physics.Initiate(velocity);
		//Make Sound
		float angle = Vector3.Angle(velocity, Vector3.up);
		angle = (angle < 90) ? 90 - angle : 180 - angle;
		activatedStar.doTap(velocity.magnitude * 0.5f, angle);
		//Haptics 
		StartCoroutine(controller.sendBuzz(controller.tapFreq, controller.tapAmp, controller.tapDuration));
	}
    #endregion

}

