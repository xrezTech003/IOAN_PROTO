using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CameraGrabber : MonoBehaviour
{
    /// <summary>
    /// VD: Hard reference to the player node, which handles networking
    /// </summary>
    public iOANHeteroPlayer playerNode;
    /// <summary>
    /// VD: Hard reference to the modified ovr script which enables the right ovr model based on Oculus SDK
    /// </summary>
    public iOANOVRControllerHelper OVRHelper;
    /// <summary>
    /// VD: Late start flag for things we need done early but not quite frame 0
    /// </summary>
    private bool lateStart = false;
    /// <summary>
    /// VD: representative game object for controller tracking simplification ::: essentially, points to the controller that Oculus or OpenVR are actually tracking for this hand for tracking and inputs
    /// </summary>
    public GameObject activeController;
    /// <summary>
    /// VD: Inspector set boolean to tell the left hand from the right for select functions (tap detector and calls to player node)
    /// </summary>
    public bool isLeft;
    /// <summary>
    /// VD: Redundant but necessary secondary link to the steam tracked game object
    /// </summary>
    public GameObject steamTracked;
    /// <summary>
    /// VD: Redundant but necessary tertiary link to the steam tracked game object's steamVR transformational representation 
    /// </summary>
    public SteamVR_TrackedObject sVR_TO;
    /// <summary>
    /// VD: Redundant but necessary quadrary link to the steam tracked game object's steamVR input index, which varies as the controller's switch hands
    /// </summary>
    public int steam_index;
    /// <summary>
    /// VD: Redundant but necessary quintary link to the steam tracked game object's steamVR input representation 
    /// </summary>
    public SteamVR_Controller.Device steamInput;
    /// <summary>
    /// VD: Hard reference to the grab detector, which is a manually updatable flag setter for handling the pickup of active and inactive stars using starID's and distance, respectively  
    /// </summary>
    public ControllerGrabDetector grabDetector;
    /// <summary>
    /// VD: Hard reference to the intializer obj whcih enables the translation from starId to sequel index on inactive grab
    /// </summary>
    public GameObject initializerObj;
    /// <summary>
    /// VD: Start set reference to the ovr manager, a necessary parent script to make full use of oculus functionality ::: notice that another script on the same object will find this player if it is running oculus and make it a descendant
    /// </summary>
    public OVRManager ovrm;
    /// <summary>
    /// VD: Boolean Web for server-client redundancy with grabs ::: active and inactive updated by grab detector if a star is available to be grabbed
    /// </summary>
    public bool active, inactive;
    /// <summary>
    /// VD: Boolean Web for server-client redundancy with grabs ::: trigger remembers the controller state from update to late update
    /// </summary>
    public bool trigger;
    /// <summary>
    /// VD: Boolean Web for server-client redundancy with grabs ::: holding matches the trigger state but for three line's wherein it's difference determines release 
    /// </summary>
    private bool holding = false;
    /// <summary>
    /// VD: Boolean Web for server-client redundancy with grabs ::: grabbing acts as a stop to prevent new frames from causing repeat events
    /// </summary>
    public bool grabbing = false;
    /// <summary>
    /// VD: Boolean Web for server-client redundancy with grabs ::: just spawned is a unique active state which helps an active star we just created attach to the controller properly
    /// </summary>
    public bool justSpawned = false;
    /// <summary>
    /// VD: Redundancy boolean to enable controller refinds
    /// </summary>
    private bool controller_off = false;
    /// <summary>
    /// VD: Used to initialize grab detector with desired active star pickup range
    /// </summary>
    public float grab_d = 0.2f;

    public float increment = 15.0f;


    void Start()
    {
        lateStart = true;
        if (!playerNode.isOVR)
        {
            initViveController();
        }

        //playerNode.CmdActivatePlayer((int)playerNode.playerID);
        grabDetector = new ControllerGrabDetector(grab_d);

        if (playerNode.isOVR) ovrm = GameObject.FindGameObjectWithTag("OVRRig").GetComponent<OVRManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerNode.isLocalPlayer) return;
        if (!playerNode.isOVR && (steamTracked == null || controller_off)) initViveController();

        if (lateStart && Time.realtimeSinceStartup > 15)
        {
            if (!playerNode.isOVR)
            {
                //"Vive Controller" Should Essentially just be the model with a script to link it's location to the steamVR tracked controllers
                activeController = steamTracked;/*gameObject.transform.Find("SteamController").gameObject;*/
                OVRHelper.gameObject.SetActive(false);
            }
            else
            {
                activeController = OVRHelper.gameObject;
                gameObject.transform.Find("SteamController").gameObject.SetActive(false);
            }

            grabDetector = new ControllerGrabDetector(grab_d);
            lateStart = false;
        }
        if ((((playerNode.isOVR) && (ovrm != null)) || (!playerNode.isOVR)) && !controller_off)
        {
            if (!playerNode.isOVR && steam_index != (int)sVR_TO.index)
            {
                steam_index = (int)sVR_TO.index;
                steamInput = SteamVR_Controller.Input(steam_index);

                if (isLeft) controllerController.deviceA = steamInput;
                else controllerController.deviceB = steamInput;
            }

            if (activeController != null)
            {
                if (grabDetector == null) grabDetector = new ControllerGrabDetector(grab_d);

                initializerObj = GameObject.FindGameObjectWithTag("init");
                gameObject.transform.position = activeController.transform.position;
                gameObject.transform.rotation = activeController.transform.rotation;

                if (playerNode.isOVR)
                {
                    OVRInput.Update();
                    holding = trigger;
                    trigger = isLeft ? OVRInput.Get(OVRInput.RawButton.LIndexTrigger) : OVRInput.Get(OVRInput.RawButton.RIndexTrigger);
                    if (trigger && !holding) grabbing = true;
                }
                else
                {
                    holding = trigger;
                    trigger = steamInput.GetHairTrigger();
                    if (trigger && !holding) grabbing = true;
                }
            }
            else
            {
                if (!playerNode.isOVR)
                {
                    //"Vive Controller" Should Essentially just be the model with a script to link it's location to the steamVR tracked controllers
                    activeController = steamTracked;
                    //gameObject.transform.Find("SteamController").gameObject;
                    OVRHelper.gameObject.SetActive(false);
                }
                else
                {
                    activeController = OVRHelper.gameObject;
                    gameObject.transform.Find("SteamController").gameObject.SetActive(false);
                }
            }
        } //catches ovrm fails
        else if (!controller_off) ovrm = GameObject.FindGameObjectWithTag("OVRRig").GetComponent<OVRManager>();
    }

    private void LateUpdate()
    {
        if (!playerNode.isLocalPlayer) return;
        if (!controller_off)
        {
            grabDetector.Update(trigger, gameObject.transform.TransformPoint(Vector3.forward * 0.1f), holding);

            if ((grabbing || holding) && grabDetector.cameraBody != null)
            {

                 playerNode.CmdSetAuthority(grabDetector.cameraBody.GetComponent<NetworkIdentity>());


                //Debug.Log("SHOULD BE HOLDING CAM OBJ");

                grabDetector.cameraBody.transform.position = gameObject.transform.TransformPoint(new Vector3(0f, 0f, 0.1f));
                if(grabDetector.cameraBody.name != "Wwise Audio Listener")
                {
                    Vector3 snapped_rot = new Vector3(Mathf.Round(gameObject.transform.eulerAngles.x / increment) * increment, Mathf.Round(gameObject.transform.eulerAngles.y / increment) * increment, Mathf.Round(gameObject.transform.eulerAngles.z / increment) * increment);
                    //Debug.Log("X: " + snapped_rot.x + " Y: " + snapped_rot.y + " Z: " + snapped_rot.z);
                    Quaternion final_rot = Quaternion.Euler(snapped_rot) * Quaternion.Euler(0, 180f, 0);
                    grabDetector.cameraBody.transform.rotation = final_rot;
                }
                holding = trigger;
            }
        }
    }

    void initViveController()
    {
        steamTracked = isLeft ? GameObject.FindGameObjectWithTag("controlLeft") : GameObject.FindGameObjectWithTag("controlRight");

        if (steamTracked != null)
        {
            controller_off = false;
            sVR_TO = steamTracked.GetComponent<SteamVR_TrackedObject>();
            steam_index = (int)sVR_TO.index;
            steamInput = SteamVR_Controller.Input(steam_index);
            if (isLeft)
            {
                //GameObject defaultControllerM = GameObject.Find("/VRControl/[CameraRig]/Controller (left)/Model");
                //defaultControllerM.SetActive(false);
                controllerController.deviceA = steamInput;
            }
            else
            {
                //GameObject defaultControllerM = GameObject.Find("/VRControl/[CameraRig]/Controller (right)/Model");
                //defaultControllerM.SetActive(false);
                controllerController.deviceB = steamInput;
            }

        }
        else controller_off = true;
    }


    public class ControllerGrabDetector
    {
        /// <summary>
        /// VD: Distance from the camera an object can be to become a candidate for pickup, constructed
        /// </summary>
        public float grabDist;

        /// <summary>
        /// VD: Placeholder for Dragged Star - checked in f:Update
        /// </summary>
        //public GameObject grabbedDragStar;
        /// <summary>
        /// VD: Placeholder for Active Star - checked in f:Update
        /// </summary>
        public GameObject cameraBody;
        //public uint starID;
        /// <summary>
        /// FD: Set's self.sensorCam and self.grabDist
        /// </summary>
        /// <param name="sensorCam">Which sensor cam did the detecting? Should match the side of the o:ControllerGrabDetector</param>
        /// <param name="grabDist">Max distance from the sensor?</param>
        public ControllerGrabDetector(float grabDist)
        {
            this.grabDist = grabDist;
        }

        /// <summary>
        /// FD: Called from c:GrabDetector every frame as long as the controller and sensor are !NULL
        /// </summary>
        /// <param name="controller">Since there will always be two of these,it helps to be able to talk about c:GrabDetector's controller with 1 word instead of 5 ::: IV: Seriously, one class would be better</param>
        public void Update(bool trigger, Vector3 attachPoint, bool holding)
        {
            ///<remarks>Ensure working with a cleared variable cache</remarks>
            ///<remarks>Ensure working with a cleared variable cache</remarks>

            if (!holding)
            {
                cameraBody = null;
            }

            //grabbedDragStar = null;

            if (trigger)
            {
                //trigger = false;
                // Debug.Log("Grab Detector: Star ID?!?: " + starID);

                ///<remarks>On success, NULL the other variables</remarks>
                //if (grabbedInactiveStar != 0 && !grabbing) grabbedActiveStar = null;
                //else
                //{
                /** check for grabbed active star or drag star --==--
                    check to see if the user is grabbing an activated star */
                ///<remarks>The controllers loction, the passed in distance allowance, and a placeholder for the star that is found </remarks>

                float closestDistance = grabDist;
                GameObject closestServerSetupObject = null;

                ///<remarks>Find interactable server objects and grab the closest one</remarks>
                GameObject[] serverSetupObjects = GameObject.FindGameObjectsWithTag("sCam");


                foreach (GameObject serverSetupObject in serverSetupObjects)
                {
                    //Debug.Log("Held Star ID: " + heldStar);
                    //Debug.Log("This ActivatedStarID: " + star.GetComponent<activatedStarScript>().starID);

                    float camDistance = Vector3.Distance(attachPoint, serverSetupObject.transform.position);

                    if (camDistance <= closestDistance)
                    {
                        Debug.Log("FOUND A SET UP OBJ");

                        closestDistance = camDistance;
                        closestServerSetupObject = serverSetupObject;

                    }
                }

                cameraBody = closestServerSetupObject;

                //}
            }
        }
    }

}
