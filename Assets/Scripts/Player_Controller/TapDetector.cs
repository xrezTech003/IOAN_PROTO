using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/**
<summary>
	CD : TapDetector
	Class for detecting if the star was tapped or not
</summary>
**/
public class TapDetector : MonoBehaviour
{
	#region PUBLIC_VAR
	/// <summary>
	///		VD : retapDistance
	///		Will set m_retapDist and retapDistSqr
	/// </summary>
	[SerializeField]
	public float retapDistance
	{
		get { return m_retapDist; }
		set
		{
			m_retapDist = value;
			retapDistSqr = value * value;
		}
	}

	/// <summary>
	///		VD : leftTapID
	///		ID of left tap controller
	/// </summary>
	public uint leftTapID = 0;

	/// <summary>
	///		VD : leftTapLocation
	///		Location of left tap controller
	/// </summary>
	public Vector3 leftTapLocation;

	/// <summary>
	///		VD : rightTapID
	///		ID of right tap controller
	/// </summary>
	public uint rightTapID = 0;

	/// <summary>
	///		VD : rightTapLocation
	///		Location of right tap controller
	/// </summary>
	public Vector3 rightTapLocation;
	public OVRManager ovrm;
	//public GameObject rightOculus, leftOculus;
	#endregion

	#region PRIVATE_VAR
	/// <summary>
	///		VD : m_retapDist
	/// </summary>
	[Tooltip("The min distance a controller has to move away from a tap location before it can retap the same object")]
	private float m_retapDist = .03f;

	/// <summary>
	///		VD : retapDistSqr
	///		Never used
	/// </summary>
	private float retapDistSqr;

	/// <summary>
	///		VD : playerID
	/// </summary>
	private int playerID;

	/// <summary>
	///		VD : shadeSwapperScript
	/// </summary>
	private shadeSwapper shadeSwapperScript;

	/// <summary>
	///		VD : controllerSensor
	/// </summary>
	private GameObject controllerSensor;

	/// <summary>
	///		VD : leftController
	/// </summary>
	private GameObject leftController;

	/// <summary>
	///		VD : rightController
	/// </summary>
	private GameObject rightController;
	public iOANHeteroPlayer playerNode;
	private iOANHeteroController controllerNode;

	/// <summary>
	///		VD : leftSensorCam
	/// </summary>
	public SensorCam leftSensorCam;
	
	/// <summary>
	///		VD : rightSensorCam
	/// </summary>
	public SensorCam rightSensorCam;

	/// <summary>
	///		VD : leftRetapChecker
	/// </summary>
	private RetapChecker leftRetapChecker;

	/// <summary>
	///		VD : rightRetapChecker
	/// </summary>
	private RetapChecker rightRetapChecker;
	#endregion

	#region PRIVATE_CLASS
	class RetapChecker
	{
		#region PRIVATE_VAR
		/// <summary>
		///		VD : curID
		/// </summary>
		uint curID = 0;

		/// <summary>
		///		VD : tapLocation
		/// </summary>
		Vector3 tapLocation;

		/// <summary>
		///		VD : retapDistSqr
		/// </summary>
		float retapDistSqr;
		#endregion

		#region CONSTRUCTOR
		/**
		<summary>
			FD : RetapChecker(float)
			// makes sure we don't get things buzzing on the edge of a star
			// need to move away at least retapDist
			<param name="retapDist"></param>
		</summary>
		**/
		public RetapChecker(float retapDist)
		{
			this.retapDistSqr = retapDist * retapDist;
		}
		#endregion

		#region PUBLIC_FUNC
		/**
		<summary>
			FD : isReadyFornewTap()
			Returns true if v:curID is equal to 0
		</summary>
		**/
		public bool isReadyFornewTap()
		{
			return curID == 0;
		}

		/**
		<summary>
			FD : tapDetected(uint, Vector3)
			Will return true if the star was tapped
			<param name="starID"></param>
			<param name="tapLocation"></param>
		</summary>
		**/
		public bool tapDetected(uint starID, Vector3 tapLocation)
		{
			//returns true if there should be a tap.
			if (curID == 0)
			{
				if (starID > 0)
				{
					curID = starID;
					this.tapLocation = tapLocation;
					return true;
				}
				else return false; // nothing to do, no star is tapped and no star was tapped recently
			}
			else
			{
				if (starID == 0)
				{ // if nothing is tapped right now check for reset
					if ((this.tapLocation - tapLocation).sqrMagnitude >= retapDistSqr) curID = 0; //reset
					return false;
				}
				else if (starID != curID)
				{ // a differnt star was tapped no need to check disst
					this.curID = starID;
					this.tapLocation = tapLocation;
					return true;
				}
				else return false; // the same star was tapped and never untapped, no need to check anything
			}
		}
		#endregion
	}
	#endregion

	#region UNITY_FUNC
	/**
	<summary>
		FD : Start()
		Set v:playerID to ID from config
		Set v:shadeSwapperScript to object with tag "init" shadeSwapper
		Set v:retapDistSqr, v:leftRetapChecker, and v:rightRetapChecker
		Find the object tagged with "ControllerSensor"
		Find the left and right SensorCam from that object

	</summary>
	**/
	void Start()
	{

		playerID = Config.Instance.Data.myID;
		playerNode = GetComponent<iOANHeteroPlayer>();
		//playerNode = (GameObject.FindGameObjectsWithTag("heteroPlayer").Where(o => o.GetComponent<iOANHeteroPlayer>().playerID == (iOANPlayerUtil.playerID)playerID)).ToList()[0].GetComponent<iOANHeteroPlayer>();
		shadeSwapperScript = GameObject.FindGameObjectWithTag("init").GetComponent<shadeSwapper>();

		if (!playerNode.isLocalPlayer)
		{
			return;
		}

		//clientOSCBus = GetComponent<ClientOSCPacketBus> ();
		retapDistSqr = m_retapDist * m_retapDist;
		leftRetapChecker = new RetapChecker(retapDistance);
		rightRetapChecker = new RetapChecker(retapDistance);

		controllerSensor = GameObject.FindGameObjectWithTag("ControllerSensor");

		//foreach (SensorCam sc in controllerSensor.GetComponents<SensorCam>())
		//{
		//	if (sc.isLeft) leftSensorCam = sc;
		//	else rightSensorCam = sc;
		//}

		if (!leftSensorCam) Debug.LogWarning("Left sensor cam does not exits (or its isLeft box is not checked)");
		if (!rightSensorCam) Debug.LogWarning("Right sensor cam does not exits (or its isLeft box is checked)");
	}

	/**
	<summary>
		VD : Update()
		If left or right controllers are null, find objects tagged with "controlLeft" and "controlRight" respectively
		If v:leftController isn't null
			If a tap is detected
				Set v:leftTapID
				Set v:leftTapLocation
			Else
				Set v:leftTapID to 0
				Left is ready for new tap
		Else 
			Set v:leftTapID to 0
		If v:rightController isn't null
			If a tap is detected
				Set v:rightTapID
				Set v:rightTapLocation
			Else
				Set v:rightTapID to 0
				Right is ready for new tap
		Else 
			Set v:rightTapID to 0
	</summary>
	**/
	/*void Update()
	{

		//if (!leftController) leftController = (OVRPlugin.GetSystemHeadsetType() != 0) ? GameObject.FindGameObjectWithTag("oculusLeft") : GameObject.FindGameObjectWithTag("controlLeft");
		//if (!rightController) rightController = (OVRPlugin.GetSystemHeadsetType() != 0) ? GameObject.FindGameObjectWithTag("oculusRight") : GameObject.FindGameObjectWithTag("controlRight");
		if (!leftController) leftController = (gameObject.GetComponent<iOANHeteroPlayer>().isOVR) ? GameObject.FindGameObjectWithTag("oculusLeft") : GameObject.FindGameObjectWithTag("controlLeft");
		if (!rightController) rightController = (gameObject.GetComponent<iOANHeteroPlayer>().isOVR) ? GameObject.FindGameObjectWithTag("oculusRight") : GameObject.FindGameObjectWithTag("controlRight");
		

		if (leftController)
		{
			controllerNode = playerNode.LeftController.GetComponent<iOANHeteroController>();
			float velocity = controllerNode.fullVelocity.magnitude;

			if (leftRetapChecker.tapDetected(leftSensorCam.starID, leftController.transform.position))
			{
				
				leftTapID = leftSensorCam.starID;
				leftTapLocation = leftController.transform.position;
				//	clientOSCBus.tapLeft (leftTapID);
				//	Debug.Log("left " + leftTapID);
				//TODO: hilightpos should be a uint
				shadeSwapperScript.UpdateHighlightPos((int)leftTapID, playerID, shadeSwapper.HighLightType.LEFT, velocity);
                GetComponent<iOANHeteroPlayer>().CmdSpawnPulse(leftTapID);


				//				initRef.GetComponent<shadeSwapper> ().updateHighlightPos (starID, myID);
			}
			else
			{
				if (leftRetapChecker.isReadyFornewTap())

						shadeSwapperScript.UpdateHighlightPos(0, playerID, shadeSwapper.HighLightType.LEFT, 0);

					leftTapID = 0;
			}
		}
		else leftTapID = 0;


		if (rightController)
		{
			controllerNode = playerNode.RightController.GetComponent<iOANHeteroController>();
			float velocity = controllerNode.fullVelocity.magnitude;
			if (rightRetapChecker.tapDetected(rightSensorCam.starID, rightController.transform.position))
			{
				rightTapID = rightSensorCam.starID;
				rightTapLocation = rightController.transform.position;
				//		clientOSCBus.tapRight (rightTapID);
				//		Debug.Log("right " + rightTapID);
				shadeSwapperScript.UpdateHighlightPos((int)rightTapID, playerID, shadeSwapper.HighLightType.RIGHT, velocity);
                GetComponent<iOANHeteroPlayer>().CmdSpawnPulse(leftTapID);
            }
			else
			{
				if (rightRetapChecker.isReadyFornewTap())
				{

					shadeSwapperScript.UpdateHighlightPos(0, playerID, shadeSwapper.HighLightType.RIGHT, 0);

				rightTapID = 0;
			}
		}
		else rightTapID = 0;

	}*/
	void Update()
	{
		if(!playerNode.isLocalPlayer)
        {
			return;
        }

		//if (!leftController) leftController = (OVRPlugin.GetSystemHeadsetType() != 0) ? GameObject.FindGameObjectWithTag("oculusLeft") : GameObject.FindGameObjectWithTag("controlLeft");
		//if (!rightController) rightController = (OVRPlugin.GetSystemHeadsetType() != 0) ? GameObject.FindGameObjectWithTag("oculusRight") : GameObject.FindGameObjectWithTag("controlRight");
		if (!leftController) leftController = (gameObject.GetComponent<iOANHeteroPlayer>().isOVR) ? GameObject.FindGameObjectWithTag("oculusLeft") : GameObject.FindGameObjectWithTag("controlLeft");
		if (!rightController) rightController = (gameObject.GetComponent<iOANHeteroPlayer>().isOVR) ? GameObject.FindGameObjectWithTag("oculusRight") : GameObject.FindGameObjectWithTag("controlRight");


		if (leftController)
		{
			controllerNode = playerNode.LeftController.GetComponent<iOANHeteroController>();
			//leftRightVel.x = controllerNode.fullVelocity.magnitude;

			if (leftRetapChecker.tapDetected(leftSensorCam.starID, leftController.transform.position))
			{

				leftTapID = leftSensorCam.starID;
				leftTapLocation = leftController.transform.position;
				//	clientOSCBus.tapLeft (leftTapID);
				//	Debug.Log("left " + leftTapID);
				//TODO: hilightpos should be a uint
				//int highlightListIndex = !(shadeSwapperScript.getTimer(positionArray[playerID, 0]) > 0) ? 0 : !(shadeSwapperScript.getTimer(positionArray[playerID, 1]) > 0) ? 1 : !(shadeSwapperScript.getTimer(positionArray[playerID, 2]) > 0) ? 2 : -1;
				//int highlightListIndex = (tapIDs[playerID, 0] == 0  && tapIDs[playerID, 0] != leftTapID) ? 0 : (tapIDs[playerID, 1] == 0  && tapIDs[playerID, 1] != leftTapID) ? 1 : (tapIDs[playerID, 2] == 0  && tapIDs[playerID, 2] != leftTapID) ? 2 : -1;
				//if (highlightListIndex >= 0)
				//{
					//tapIDs[playerID, highlightListIndex] = (int)leftTapID;
					//Debug.Log("Set Tap ID" + playerID + ", " + highlightListIndex + " to " + leftTapID);
				//}
				
				GetComponent<iOANHeteroPlayer>().CmdSpawnPulse(leftTapID);
				

				//				initRef.GetComponent<shadeSwapper> ().updateHighlightPos (starID, myID);
			}
			else leftTapID = 0;
		}
		if (rightController)
		{
			controllerNode = playerNode.RightController.GetComponent<iOANHeteroController>();
			//leftRightVel.y = controllerNode.fullVelocity.magnitude;
			if (rightRetapChecker.tapDetected(rightSensorCam.starID, rightController.transform.position))
			{
				rightTapID = rightSensorCam.starID;
				rightTapLocation = rightController.transform.position;
				//		clientOSCBus.tapRight (rightTapID);
				//		Debug.Log("right " + rightTapID);
				//int highlightListIndex = !(shadeSwapperScript.getTimer(positionArray[playerID, 3]) > 0) ? 3 : !(shadeSwapperScript.getTimer(positionArray[playerID, 4]) > 0) ? 4 : !(shadeSwapperScript.getTimer(positionArray[playerID, 5]) > 0) ? 5 : -1;
				//int highlightListIndex = (tapIDs[playerID, 3] == 0  && tapIDs[playerID, 3] != rightTapID) ? 3 : (tapIDs[playerID, 4] == 0  && tapIDs[playerID, 4] != rightTapID) ? 4 : (tapIDs[playerID, 5] == 0  && tapIDs[playerID, 5] != rightTapID) ? 5 : -1;
				//if (highlightListIndex >= 0)
				//
					//tapIDs[playerID, highlightListIndex] = (int)rightTapID;
					//Debug.Log("Set Tap ID " + playerID + ", " + highlightListIndex + " to " + rightTapID);
				//}
				GetComponent<iOANHeteroPlayer>().CmdSpawnPulse(rightTapID);
			}
			else rightTapID = 0;
		}

        if (leftTapID != 0) GetComponent<iOANHeteroPlayer>().CmdUpdateHighlightPos((int)leftTapID);
        if (rightTapID != 0) GetComponent<iOANHeteroPlayer>().CmdUpdateHighlightPos((int)rightTapID);
    }
	#endregion
}
