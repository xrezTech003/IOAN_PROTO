using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CD: Stolen from OW: when the trigger is pulled on either controller this class checks to see if that controller is touching star (either active or inactive)
///  ::: IV: This whole script is gymnastics. One clas uses the other class to accomplish similar goals. It's a knot
/// </summary>
public class GrabDetector : MonoBehaviour
{   // when the trigger is pulled on either controller this class checks to see if that controller is touching star (either active or inactive)

    #region Varables and Properties
    public OVRManager ovrm;
    /// <summary>
    /// VD: Max distance for grabbing an activated star
    /// </summary>
    [Tooltip("Max distance for grabbing an activated star")]
    public float grabDistance = .07f;

    /// <summary>
    /// VD: Pointer to left controller sensor cam
    ///  ::: IV: Does some gymnastics in the scene, worth critically looking at
    /// </summary>
    public SensorCam leftSensorCam;
    /// <summary>
    /// VD: Pointer to right controller sensor cam
    ///  ::: IV: Does some gymnastics in the scene, worth critically looking at
    /// </summary>
    public SensorCam rightSensorCam;

    /// <summary>
    /// OD: Insantiation of helper class designed to update grabbed stars
    /// </summary>
    public ControllerGrabDetector left;
    /// <summary>
    /// FD: Get function for left-handed inactive star
    /// </summary>
    /// <returns>left.grabbedInactiveStar</returns>
    public uint LeftGrabbedStar
    {
        get
        {
            if (left == null) return 0;
            return left.grabbedInactiveStar;
        }
    }
    /// <summary>
    /// FD: Get function for left-handed active star
    /// </summary>
    /// <returns>left.grabbedActiveStar</returns>
    public GameObject LeftGrabbedActiveStar
    {
        get
        {
            if (left == null) return null;
            return left.grabbedActiveStar;
        }
    }

    /// <summary>
    ///  OD: Insantiation of helper class designed to update grabbed stars
    /// </summary>
    public ControllerGrabDetector right;
    /// <summary>
    /// FD: Get function for right-handed inactive star
    /// </summary>
    /// <returns>right.grabbedInactiveStar</returns>
    public uint RightGrabbedStar
    {
        get
        {
            if (right == null) return 0;
            return right.grabbedInactiveStar;
        }
    }
    /// <summary>
    /// FD: Get function for right-handed active star
    /// </summary>
    /// <returns>right.grabbedActiveStar</returns>
    public GameObject RightGrabbedActiveStar
    {
        get
        {
            if (right == null) return null;
            return right.grabbedActiveStar;
        }
    }

    /// <summary>
    /// OD: Pointer to the Player's left controller
    /// </summary>
    public GameObject LeftController { get; private set; }
    /// <summary>
    /// OD: Pointer to the Player's right controller
    /// </summary>
    public GameObject RightController { get; private set; }

    //NDH OVR Protoypes
    private bool OVR;
    private bool isLeft;
    
    #endregion

    /// <summary>
    /// FD: Populate the sensorCm reference variables for use in f:Update
    /// </summary>
    public void UpdateSensorCams()
    {
        /*GameObject controllerSensor = GameObject.FindGameObjectWithTag ("ControllerSensor");
        foreach (SensorCam sc in controllerSensor.GetComponents<SensorCam>())
        {
            if (sc.isLeft) leftSensorCam = sc;
            else rightSensorCam = sc;
        }

        if (!leftSensorCam) Debug.LogWarning ("Left sensor cam does not exits (or its isLeft box is not checked)");
        if (!rightSensorCam) Debug.LogWarning ("Right sensor cam does not exits (or its isLeft box is checked)");
        */if (OVRPlugin.GetSystemHeadsetType() != 0) OVR = true;
    }

    /// <summary>
    /// Lengthy one: VD: WLoG Left and Right: 
    /// series of checks for c:GrabDetector v:leftcontroller, which is found by tag in this function,
    /// a c:ControllerGrabDetector o:left, and c:GrabDetector v:leftSensorCam. 
    /// Ensures all are !NULL and runs c:left.f:Update when not
    /// </summary>
    void Update()
    {
        if (OVRPlugin.GetSystemHeadsetType() != 0) OVR = true;

        if ((leftSensorCam == null) || (rightSensorCam == null))
            UpdateSensorCams();

        if (OVRPlugin.GetSystemHeadsetType() == 0) LeftController = GameObject.FindGameObjectWithTag("controlLeft");
        else LeftController = GameObject.FindGameObjectWithTag("oculusLeft");
        if ((LeftController != null) && (leftSensorCam != null))
        {
            if (left == null)
                left = new ControllerGrabDetector(leftSensorCam, grabDistance);
            else
            {
                //left.setStarID();
                left.Update(LeftController, OVR, true);
            }
        }
        if (OVRPlugin.GetSystemHeadsetType() == 0) RightController = GameObject.FindGameObjectWithTag("controlRight");
        else RightController = GameObject.FindGameObjectWithTag("oculusRight");
        if ((RightController != null) && (rightSensorCam != null))
        {
            if (right == null)
                right = new ControllerGrabDetector(rightSensorCam, grabDistance);
            else
            {
               // right.setStarID();
                right.Update(RightController, OVR, false);
            }
        }
    }
    /*
    //this does the work but its instantiated twice
    //once for each controller.
    //better than the appoach I used with sensor cam (two copies of the script with an isLeft flag) Oringal Writer? Eitan?
    */

    /// <summary>
    /// Helper Class for GrabDetector ::: IV: Seriously thinking they could just be combined somehow, <b>BIG</b> refactor though
    /// </summary>
    public class ControllerGrabDetector
    {
        /// <summary>
        /// VD: Each ControllerGrabDetector is named a side, this refrences that controller's sensorCam, constructed
        /// </summary>
        public SensorCam sensorCam;
        /// <summary>
        /// VD: Distance from the camera an object can be to become a candidate for pickup, constructed
        /// </summary>
        float grabDist;

        /// <summary>
        /// VD: Placeholder for an inactive star, clears the other variables until updated - checked in f:Update
        /// </summary>
        public uint grabbedInactiveStar;
        /// <summary>
        /// VD: Placeholder for Dragged Star - checked in f:Update
        /// </summary>
        public GameObject grabbedDragStar;
        /// <summary>
        /// VD: Placeholder for Active Star - checked in f:Update
        /// </summary>
        public GameObject grabbedActiveStar;
        public uint starID; 
        /// <summary>
        /// FD: Set's self.sensorCam and self.grabDist
        /// </summary>
        /// <param name="sensorCam">Which sensor cam did the detecting? Should match the side of the o:ControllerGrabDetector</param>
        /// <param name="grabDist">Max distance from the sensor?</param>
        public ControllerGrabDetector(SensorCam sensorCam, float grabDist)
        {
            this.sensorCam = sensorCam;
            this.grabDist = grabDist;
        }
        private bool doing = false;
        private bool checking = false;

        /// <summary>
        /// FD: Called from c:GrabDetector every frame as long as the controller and sensor are !NULL
        /// </summary>
        /// <param name="controller">Since there will always be two of these,it helps to be able to talk about c:GrabDetector's controller with 1 word instead of 5 ::: IV: Seriously, one class would be better</param>
        public void Update (GameObject controller, bool OVR = false, bool isLeft = false)
        {
            ///<remarks>Point to the steam device</remarks>
            bool steamClick = false;
            SteamVR_Controller.Device device = null;
            if (OVRPlugin.GetSystemHeadsetType() == 0)
            {
               device = SteamVR_Controller.Input((int)controller.GetComponent<SteamVR_TrackedObject>().index);
                steamClick = device.GetHairTriggerDown();
            }
            ///<remarks>Ensure working with a cleared variable cache</remarks>
            grabbedActiveStar = null;
            grabbedInactiveStar = 0;
            grabbedDragStar = null;

            ///<remarks>Check if the trigger is pulled</remarks>
            ///
            OVRInput.Update();
            bool OVRLIndex = false;
            bool OVRRIndex = false;
            if (OVRInput.Get(OVRInput.RawButton.LIndexTrigger) && OVRLIndex == false) { OVRLIndex = true; Debug.Log("Left Trigger"); }
            else if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger) && OVRRIndex == false) { OVRRIndex = true; Debug.Log("Right Trigger"); }
            else doing = false;
            //if (OVR && OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, (isLeft)? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch)) || device.GetHairTriggerDown()) 
            //if ((OVR && OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger, (isLeft) ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch)) || steamClick)
            if (!doing)
            {
                if ((OVR && isLeft && OVRLIndex) || (OVR && !isLeft && OVRRIndex) || steamClick)
                {
                    doing = true;
                    OVRLIndex = false; OVRRIndex = false;
                    Debug.Log("Grab Detector: Star ID?!?: " + starID);
                    grabbedInactiveStar = starID;

                    ///<remarks>On success, NULL the other variables</remarks>
                    if (grabbedInactiveStar != 0)
                    {
                        grabbedActiveStar = null;
                        grabbedDragStar = null;
                    }
                    else 
                    {
                        /** check for grabbed active star or drag star --==--
                            check to see if the user is grabbing an activated star */
                        ///<remarks>The controllers loction, the passed in distance allowance, and a placeholder for the star that is found </remarks>
                        Vector3 attachPoint = controller.transform.TransformPoint(Vector3.forward * 0.1f);
                        float closestDistance = grabDist;
                        GameObject closestStarObject = null;

                        ///<remarks>Search through all the activated stars to find one wihtin the passed-in distance</remarks>
                        foreach (GameObject star in GameObject.FindGameObjectsWithTag("activatedStar"))
                        {
                            float activeStarDistance = Vector3.Distance(attachPoint, star.transform.position);
                            if (activeStarDistance < closestDistance)
                            {
                                closestDistance = activeStarDistance;
                                closestStarObject = star;
                            }

                        }

                        grabbedActiveStar = closestStarObject;
                        closestStarObject = null;

                        ///<remarks>Search through all the dragged stars to find one within the passed-in distance</remarks>
                        foreach (GameObject star in GameObject.FindGameObjectsWithTag("draggedSphere"))
                        {
                            float dragStarDist = Vector3.Distance(attachPoint, star.transform.position);
                            if (dragStarDist < closestDistance)
                            {
                                closestDistance = dragStarDist;
                                closestStarObject = star;
                            }

                        }
                        if (closestStarObject != null)
                        {
                            grabbedDragStar = closestStarObject;
                            grabbedActiveStar = null;

                            ///<remarks>By this time, either activatedStar or grabStar is populated and c:playerMove's LateUpdate will catch it and respond</remarks>
                        }
                    }
                }
                
            }
        }
        public void setStarID()
        {

                starID = sensorCam.starID;

        }
    }
}