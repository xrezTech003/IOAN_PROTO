using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.IO;
using UnityEngine.XR;
using Unity.Mathematics;
/// <summary>
/// CD: Heavy lifter for the iOAN player. contains grab detector and references to c:TapDetector and c:sensorcam ::: Each hand handles it's own grabbing, tapping, and tracking, and communicates it's action's to the server by calling functions on the player node
/// </summary>
public class iOANHeteroController : MonoBehaviour
{
    #region Variables
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
    /// VD: CMKY color dial input representation
    /// </summary>
    public bool[] dial = new bool[4];
    /// <summary>
    /// VD: Y velocity of the controller aolf
    /// </summary>
    public float velocity;
    /// <summary>
    /// VD: Speed of the controller aolf
    /// </summary>
    public Vector3 fullVelocity;
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

    public controllerController contCont;
    /// <summary>
    /// VD: Hard reference to the sensor cam, which takes gpu asynchronous snapshots of the star field and give's this script a starID to use with grab detector
    /// </summary>
    public SensorCam sensorCam;
    /// <summary>
    /// VD: Hard reference to the grab detector, which is a manually updatable flag setter for handling the pickup of active and inactive stars using starID's and distance, respectively  
    /// </summary>
    public ControllerGrabDetector grabDetector;
    //public uiSounds uiSounds;
    /// <summary>
    /// VD: Hard reference to the tap detector, which is attached to the player node game object, this script acts as a middle man between the action of tapping which is picked up by tap detector, and the the communication of such to the server through the player node
    /// </summary>
    public TapDetector tapDetector;
    /// <summary>
    /// VD: Hard reference to the intializer obj whcih enables the translation from starId to sequel index on inactive grab
    /// </summary>
    public GameObject initializerObj;
    /// <summary>
    /// VD: Inspector references to spawnable game objects
    /// </summary>
    public GameObject starTextBox, highlightIDText;
    /// <summary>
    /// VD: Start set reference to wwise sound manager
    /// </summary>
    public IOANVoiceManager voiceManager;
    /// <summary>
    /// VD: Start set reference to ip address to pull spawned star data from
    /// </summary>
    public string dbIP;
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
    /// VD: Set by sensorcam, used by grab detector, cross referenced in init 
    /// </summary>
    public uint starID;
    /// <summary>
    /// VD: Used to initialize grab detector with desired active star pickup range
    /// </summary>
    public float grab_d = 0.2f;
    /// <summary>
    /// VD: publicly adjustable frequency of oculus haptic
    /// </summary>
    public float tapFreq = 0.1f;
    /// <summary>
    /// VD: publicly adjustable amplitude of oculus haptic
    /// </summary>
    public float tapAmp = 0.5f;
    /// <summary>
    /// VD: publicly adjustable duration of oculus haptic, converted to full value for vive
    /// </summary>
    public float tapDuration = 0.1f;
    /// <summary>
    /// VD: publicly adjustable bool to test haptic
    /// </summary>
    public bool testWaveHaptic = false;
    /// <summary>
    /// VD: stop flag in case we are still holding the trigger
    /// </summary>
    private bool grabTimer = true;

    #endregion

    #region Unity Functions
    /// <summary>
    /// FD: Determine oculus status, assign local references not available to inspector, reset sensorcam is vive, initialize grabdetector and set latestart flag to true
    /// </summary>
    void Start()
    {
        //gameObject.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
        //playerNode.CmdSetAuthority(gameObject.GetComponent<NetworkIdentity>());

        /*if (!playerNode.isLocalPlayer)
        {
            if(playerNode.isServer)
            {
                playerNode.RpcInit(iOANPlayerUtil.playerColor[playerNode.playerID]);
            }
            playerNode.clientSetColor();
            //Debug.Log("I AM STARTING AS NONLOCAL PLAYER IN CONTROLLER");
            return;
        }*/ //DEBUG Relocation - Update

        lateStart = true;
        if (!playerNode.isOVR)
        {
            initViveController();

            //Reset SensorCam
            RectTransform rect = sensorCam.cam.gameObject.GetComponent<RectTransform>();
            rect.position = new Vector3(0, 0, -0.15f);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.rotation = Quaternion.Euler(10f, 0f, 0f);
            rect.localScale = new Vector3(0.1f, 0.1f, 0.2f);
        }

        tapDetector = gameObject.transform.parent.GetComponent<TapDetector>();
        dbIP = playerNode.dbIP;

        //playerNode.CmdActivatePlayer((int)playerNode.playerID);
        grabDetector = new ControllerGrabDetector(sensorCam, grab_d);
        //playerNode.CmdInit(iOANPlayerUtil.playerColor[playerNode.playerID], gameObject.GetComponent<NetworkIdentity>()); //DEBUG
        voiceManager = FindObjectOfType<IOANVoiceManager>();

        //uiSounds = GetComponent<uiSounds>();
        //dObj = GameObject.FindGameObjectWithTag("dObj");

        if (playerNode.isOVR) ovrm = GameObject.FindGameObjectWithTag("OVRRig").GetComponent<OVRManager>();
        else
        {
            contCont = GetComponentInChildren<controllerController>();
            contCont.player = playerNode;
        }
    }
    /// <summary>
    /// FD: Update reads the input modeules from the proper SDK, assigns transforms and dial, veleocity, and trigger states for use in LateUpdate ::: Several safety if statements wrap different sections to prevent unity from calling things it lost reference to
    /// </summary>
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

            grabDetector = new ControllerGrabDetector(sensorCam, grab_d);
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
                if (grabDetector == null) grabDetector = new ControllerGrabDetector(sensorCam, grab_d);

                initializerObj = GameObject.FindGameObjectWithTag("init");
                gameObject.transform.position = activeController.transform.position;
                gameObject.transform.rotation = activeController.transform.rotation;

                if (playerNode.isOVR)
                {
                    OVRInput.Update();
                    holding = trigger;
                    trigger = isLeft ? OVRInput.Get(OVRInput.RawButton.LIndexTrigger) : OVRInput.Get(OVRInput.RawButton.RIndexTrigger);
                    if (trigger && !holding) grabbing = true;
                    fullVelocity = isLeft ? ((gameObject.transform.parent.transform.TransformVector(OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch)))) : ((gameObject.transform.parent.transform.TransformVector(OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch))));

                    velocity = isLeft ? ((gameObject.transform.parent.transform.TransformVector(OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch)))).y : ((gameObject.transform.parent.transform.TransformVector(OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch)))).y;
                    playerNode.dialID = (iOANPlayerUtil.dialID)ovrm.GetComponent<OVRControllerController>().padStatus;
                }
                else
                {
                    holding = trigger;
                    trigger = steamInput.GetHairTrigger();
                    if (trigger && !holding) grabbing = true;
                    fullVelocity = steamInput.velocity;
                    velocity = steamInput.velocity.y;
                    playerNode.dialID = (iOANPlayerUtil.dialID) contCont.padStatus;
                    contCont.SetTriggerRotation(SteamVR_Controller.Input((int)sVR_TO.index).GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis1).x * -16f);
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

        //SteamController TouchPad
        if (!playerNode.isOVR && steamInput != null)
        {
            bool press = steamInput.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad);
            bool release = steamInput.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad);

            if (press)
            {
                Vector2 touchpad = steamInput.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);

                if (Mathf.Abs(touchpad.x) < 0.25f && Mathf.Abs(touchpad.y) < 0.25f) playerNode.TouchPadInteraction(0);
                else if (touchpad.y > touchpad.x && touchpad.y > -touchpad.x) playerNode.TouchPadInteraction(1);
                else if (touchpad.y < touchpad.x && touchpad.y < -touchpad.x) playerNode.TouchPadInteraction(2);
                else if (touchpad.y < touchpad.x && touchpad.y > -touchpad.x) playerNode.TouchPadInteraction(3);
                else if (touchpad.y > touchpad.x && touchpad.y < -touchpad.x) playerNode.TouchPadInteraction(4);
            }
            else if (release) playerNode.TouchPadInteraction(5);
        }
    }

    /// <summary>
    /// FD: Updates the grab detector with states from Update, uses return values to grab a star, if not, checks tap detector, if netiehr does nothing. ::: On grabbing or tapping calls the respective command from the playernode
    /// </summary>
    void LateUpdate()
    {
        if (!playerNode.isLocalPlayer) return;
        if (!controller_off)
        {
            grabDetector.Update(trigger, gameObject.transform.TransformPoint(Vector3.forward * 0.1f), starID, holding);
            //playerNode.dialID = (iOANPlayerUtil.dialID)(dial[1] ? 1 : dial[2] ? 2 : dial[3] ? 3 : 0);
            active = grabDetector.grabbedActiveStar != null;
            //drag = grabDetector.grabbedDragStar != null;
            inactive = (grabDetector.grabbedInactiveStar != 0);

            if (justSpawned)
            {
                if (active)
                {
                    GameObject grabbedStar = grabDetector.grabbedActiveStar;
                    //grabDetector.grabDist = grab_d;
                    //NetworkIdentity starNetID = grabbedStar.GetComponent<NetworkIdentity>();

                    //playerNode.CliRemoveAuthority(starNetID);
                    //playerNode.CmdSetAuthority(starNetID);

                    activatedStarScript starScript = grabbedStar.GetComponent<activatedStarScript>();
                    StartCoroutine(starScript.grabStar(gameObject));
                    //starScript.dragState = activatedStarScript.DragState.ATTACHED;
                    ///playerNode.CmdSendGrabActiveStar((int)playerNode.playerID, starScript.starID, gameObject.transform.position, true);
                    justSpawned = false;

                    
                }
            }
            else if (inactive && grabbing)
            {
                uint holdID = grabDetector.grabbedInactiveStar;
                ClearRelatedObjects(holdID);
                //Debug.Log("grabbed new/inactive star");
                if (grabTimer)
                {
                    spawnDraggedSphere(grabDetector.grabbedInactiveStar);
                    grabDetector.heldStar = grabDetector.grabbedInactiveStar;
                    StartCoroutine(StartGrabTimer());
                }

                //osc call made by spawnDraggedSphere
                playerNode.uiSounds.grabSound(gameObject, (int)playerNode.playerID); //trigger UI handoff sound
                StartCoroutine(sendBuzz(.2f, .15f, .3f));

                holding = trigger;
                grabbing = false;
                justSpawned = true;



            }
            else if (active && grabbing)
            {
                // Debug.Log("grabbed active star");
                GameObject grabbedStar = grabDetector.grabbedActiveStar;
                NetworkIdentity starNetID = grabbedStar.GetComponent<NetworkIdentity>();

                
                //playerNode.CliRemoveAuthority(starNetID);
                playerNode.CmdSetAuthority(starNetID);

                activatedStarScript starScript = grabbedStar.GetComponent<activatedStarScript>();
                if (starScript.dragState == activatedStarScript.DragState.FREEING) return;
                playerNode.CmdSendGrabActiveStar((int)playerNode.playerID, starScript.starID, gameObject.transform.position, true);
                //									oscPacketBus.GetComponent<OSCPacketBus> ().handOff(startScript.starID, true);
                StartCoroutine(starScript.grabStar(gameObject));
                playerNode.uiSounds.handoffSound(this.transform.gameObject, (int)playerNode.playerID); //trigger UI handoff sound
                StartCoroutine(sendBuzz(.1f, .25f, .3f));

                holding = trigger;

                //REMOVED FOR NOW - Tested this interaction, didnt feel right.
                //check to the varType to see if star is a droning star, otherwise haptics is triggered by sequence
                //float varType = starScript.dataUV1[1];
                //if (varType != 1)
                //{
                //    StartCoroutine(droneHaptic(0.05f, 0.025f, 0.5f)); //drone haptics while holding is true
                //}
                
                grabbing = false;
            }
            else if (isLeft && tapDetector.leftTapID != 0)
            {
                float[] tapData = spawnTapInfoDisplay(tapDetector.leftTapLocation, tapDetector.leftTapID, (int)playerNode.playerID, true);
                
                playerNode.CmdPerformTap(tapDetector.leftTapLocation, tapDetector.leftTapID, tapData);
                StartCoroutine(sendBuzz(tapFreq, tapAmp, tapDuration));
            }
            else if (!isLeft && tapDetector.rightTapID != 0)
            {
                float[] tapData = spawnTapInfoDisplay(tapDetector.rightTapLocation, tapDetector.rightTapID, (int)playerNode.playerID, false);
                playerNode.CmdPerformTap(tapDetector.rightTapLocation, tapDetector.rightTapID, tapData);
                StartCoroutine(sendBuzz(tapFreq, tapAmp, tapDuration));
            }

            if (justSpawned)
            {
                if (active)
                {
                    GameObject grabbedStar = grabDetector.grabbedActiveStar;
                    //grabDetector.grabDist = grab_d;
                    //NetworkIdentity starNetID = grabbedStar.GetComponent<NetworkIdentity>();

                    //playerNode.CliRemoveAuthority(starNetID);
                    //playerNode.CmdSetAuthority(starNetID);

                    activatedStarScript starScript = grabbedStar.GetComponent<activatedStarScript>();
                    StartCoroutine(starScript.grabStar(gameObject));
                    //starScript.dragState = activatedStarScript.DragState.ATTACHED;
                    ///playerNode.CmdSendGrabActiveStar((int)playerNode.playerID, starScript.starID, gameObject.transform.position, true);
                    justSpawned = false;


                }
            }
        }
    }
    #endregion
    
    /// <summary>
    /// VD: keep grabtimer false as long as the trigger is being held
    /// </summary>
    /// <returns>when we let the trigger go</returns>
    private IEnumerator StartGrabTimer()
    {
        grabTimer = false;
        yield return new WaitWhile(() => trigger);
        grabTimer = true;
    }
    /// <summary>
    /// Instantiate a star on this machine based on the starId and thus data from the initializer, tell playernode to spawn this star on the server and thus everywhere, then set flags to pick it up asap
    /// </summary>
    /// <param name="starID">starID as passed from sensorcam up</param>
	void spawnDraggedSphere(uint starID)
    {
        Initializer init = initializerObj.GetComponent<Initializer>();
        int selIndex = init.getIndexForStarID(starID);
        GameObject interactiveSubMesh = init.InteractiveSubMesh;
        Mesh interactiveMesh = interactiveSubMesh.GetComponent<MeshFilter>().mesh;
        Vector3 inStarOrigin = interactiveSubMesh.transform.TransformPoint(interactiveMesh.vertices[selIndex]);

        Vector4 color = interactiveMesh.colors[selIndex];

        List<Vector4> uv0List = new List<Vector4>();
        List<Vector4> uv1List = new List<Vector4>();
        interactiveMesh.GetUVs(0, uv0List);
        interactiveMesh.GetUVs(1, uv1List);

        float period = uv0List[selIndex].z;
        float paramVal;
        switch ((int)playerNode.dialID)
        {
            case 0:
                paramVal = uv1List[selIndex].x;
                break;
            case 1:
                paramVal = uv0List[selIndex].w;
                break;
            case 2:
                paramVal = uv0List[selIndex].y;
                break;
            case 3:
                paramVal = uv1List[selIndex].y;
                break;
            default:
                paramVal = 0f;
                break;
        }

        playerNode.CmdSpawnDraggedSphere((int)playerNode.playerID, inStarOrigin, (int)starID, period, dbIP,
                                         color, uv0List[selIndex], uv1List[selIndex], (int)playerNode.dialID, paramVal,
                                         inStarOrigin.y, inStarOrigin, gameObject.transform.TransformPoint(new Vector3(0f, 0f, 0.1f)), isLeft);
    }

    /// <summary>
    /// FD: Handles the spawning of star data on taps :: reads star info from initializer and its' interactivesubmesh
    /// Last edited 07/10/2020 by Chris Poovey to add functionality to output tapping information for sonification
    /// </summary>
    /// <param name="tapLocation">VD: Where did we tap? IV: Unused</param>
    /// <param name="inID">VD: Id of the star detected by sensorcam</param>
    /// <param name="inPlayerID">VD: Who tapped it?</param>
    /// <param name="isLeft">VD: Was it their left hand?</param>
    /// <returns name="tapData">Tap data in the format of [starMag, tapVelocity, tapAngle, starType] </returns>
    float[] spawnTapInfoDisplay(Vector3 tapLocation, uint inID, int inPlayerID, bool isLeft)
    {
        GameObject[] tapList = GameObject.FindGameObjectsWithTag("TapInfoDisplay");
        foreach (GameObject o in tapList)
            if (o.GetComponent<sphereBlankScript>().starID == inID)
                return null;

        Initializer init = initializerObj.GetComponent<Initializer>();
        int selIndex = init.getIndexForStarID(inID);
        GameObject interactiveSubMesh = init.InteractiveSubMesh;
        Mesh interactiveMesh = interactiveSubMesh.GetComponent<MeshFilter>().mesh;
        Vector3 starLocation = interactiveSubMesh.transform.TransformPoint(interactiveMesh.vertices[selIndex]);

        float angle = 0;
        Vector3 velocity3d;

        if (playerNode.isOVR) velocity3d = isLeft ? ((gameObject.transform.parent.transform.TransformVector(OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch))))
                                                  : ((gameObject.transform.parent.transform.TransformVector(OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch))));
        else velocity3d = steamInput.velocity;

        angle = Vector3.Angle(velocity3d, Vector3.up);
        angle = (angle < 90) ? 90 - angle : 180 - angle;

        List<Vector4> uv2 = new List<Vector4>();
        List<Vector4> uv = new List<Vector4>();
        interactiveMesh.GetUVs(0, uv);
        interactiveMesh.GetUVs(1, uv2);
        float[] tapData = new float[6];
        tapData[0] = (interactiveMesh.colors[selIndex].r); //Mean Mag
        //tapData.Add(interactiveMesh.colors[selIndex].g); //MagSTD
        //tapData.Add(interactiveMesh.colors[selIndex].b); //numObs
        //tapData.Add(interactiveMesh.colors[selIndex].a); // variability

        //tapData.Add(uv[selIndex].x); //catalog
        //tapData.Add(uv[selIndex].y); //astroColor
        //tapData.Add(uv[selIndex].z); //period
        tapData[1]=(uv[selIndex].w); //periodSNR

        //tapData.Add(uv2[selIndex].x); //lightcurveRMS
       
        //tapData.Add(uv2[selIndex].z); //properMotionRA
        //tapData.Add(uv2[selIndex].w); //parsecs

        tapData[2]=(velocity3d.magnitude); //velocity

        tapData[3]=(uv2[selIndex].y); //varClass

        tapData[4]=(angle); //angle
        tapData[5]=(uv[selIndex].y); //astroColor

        playerNode.CmdSpawnDraggedSphereBlank(starLocation, 0, inID, starLocation, inPlayerID, selIndex, isLeft, velocity3d.magnitude, angle);
        return tapData;
    }
    /// <summary>
    /// FD: Functionalized setting of the proper vive index based on changes in Vive tracking 
    /// </summary>
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

    /// <summary>
    /// CD: The grab detector is a convenient way to control the timing of sensorcam and controller input information ::: takes input from both and creates flags to let the controller execute a grab
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
        public float grabDist;

        /// <summary>
        /// VD: Placeholder for an inactive star, clears the other variables until updated - checked in f:Update
        /// </summary>
        public uint grabbedInactiveStar;

        public uint heldStar = 0;
        /// <summary>
        /// VD: Placeholder for Dragged Star - checked in f:Update
        /// </summary>
        //public GameObject grabbedDragStar;
        /// <summary>
        /// VD: Placeholder for Active Star - checked in f:Update
        /// </summary>
        public GameObject grabbedActiveStar;
        //public uint starID;
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
        private bool checking = false;

        /// <summary>
        /// FD: Called from c:GrabDetector every frame as long as the controller and sensor are !NULL
        /// </summary>
        /// <param name="controller">Since there will always be two of these,it helps to be able to talk about c:GrabDetector's controller with 1 word instead of 5 ::: IV: Seriously, one class would be better</param>
        public void Update(bool trigger, Vector3 attachPoint, uint starID, bool holding)
        {
            ///<remarks>Ensure working with a cleared variable cache</remarks>
            ///<remarks>Ensure working with a cleared variable cache</remarks>

            if (!holding)
            {
                grabbedActiveStar = null;
                heldStar = 0;
            }


            grabbedInactiveStar = 0;

            //grabbedDragStar = null;

            if (trigger)
            {
                //trigger = false;
                // Debug.Log("Grab Detector: Star ID?!?: " + starID);
                grabbedInactiveStar = starID;

                ///<remarks>On success, NULL the other variables</remarks>
                //if (grabbedInactiveStar != 0 && !grabbing) grabbedActiveStar = null;
                //else
                //{
                /** check for grabbed active star or drag star --==--
                    check to see if the user is grabbing an activated star */
                ///<remarks>The controllers loction, the passed in distance allowance, and a placeholder for the star that is found </remarks>

                float closestDistance = grabDist;
                GameObject closestStarObject = null;

                ///<remarks>Search through all the activated stars to find one wihtin the passed-in distance</remarks>
                foreach (GameObject star in GameObject.FindGameObjectsWithTag("activatedStar"))
                {
                    //Debug.Log("Held Star ID: " + heldStar);
                    //Debug.Log("This ActivatedStarID: " + star.GetComponent<activatedStarScript>().starID);

                    float activeStarDistance = Vector3.Distance(attachPoint, star.transform.position);

                    if (activeStarDistance <= closestDistance)
                    {
                        closestDistance = activeStarDistance;
                        closestStarObject = star;
                        heldStar = (uint)star.GetComponent<activatedStarScript>().starID;
                    }
                    else if ((uint)star.GetComponent<activatedStarScript>().starID == heldStar)
                    {
                        //Debug.Log("HELD STAR FOUND");
                        closestStarObject = star;
                        heldStar = (uint)star.GetComponent<activatedStarScript>().starID;
                    }
                }
                grabbedActiveStar = closestStarObject;

                //}
            }
        }
    }

    /// <summary>
    /// FD: Coroutine to Send haptic command to steamVR or OVR dependent ::: Automatically converts PV:hapticDuration for steamVR ::: OVR is an on/off switch so coroutine uses duration to countdown
    /// </summary>
    /// <param name="freq">VD: frequency of haptic, OVR only</param>
    /// <param name="amp">VD: amplitude of haptic, OVR only</param>
    /// <param name="dur">VD: duration for haptic to last, in seconds, converted to quarter-microseconds for steamVR</param>
    /// <returns></returns>
    public IEnumerator sendBuzz(float freq, float amp, float dur)
    {
        if (playerNode.isOVR) OVRInput.SetControllerVibration(freq, amp, isLeft ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch);
        else steamInput.TriggerHapticPulse((ushort)Mathf.Abs(3999 * dur / .3f));
        yield return new WaitForSeconds(dur);
        if (playerNode.isOVR) OVRInput.SetControllerVibration(0, 0, isLeft ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch);

    }
    ///<summary>
    ///length is how long the vibration should go for
    ///strength is vibration strength from 0-1
    ///</summary>
    IEnumerator longHaptic(float length, float strength)
    {
        for (float i = 0; i < length; i += Time.deltaTime)
        {
            steamInput.TriggerHapticPulse((ushort)Mathf.Lerp(0, 3999, strength));
            yield return null;
        }
    }
    /// <summary>
    ///vibrationCount is how many vibrations ::: 
    ///vibrationLength is how long each vibration should go for ::: 
    ///gapLength is how long to wait between vibrations ::: 
    ///strength is vibration strength from 0-1 ::: 
    /// </summary>
    /// <param name="vibrationCount">VD: number of pulses to send to vive controller</param>
    /// <param name="vibrationLength">VD: length of pulses to send to vive controlle</param>
    /// <param name="gapLength">VD: gap length between pulses to send to vive controlle</param>
    /// <param name="strength">VD: strength of pulses to send to vive controller</param>
    /// <returns></returns>

    IEnumerator pulseHaptic(int vibrationCount, float vibrationLength, float gapLength, float strength)
    {
        strength = Mathf.Clamp01(strength);
        for (int i = 0; i < vibrationCount; i++)
        {
            if (i != 0) yield return new WaitForSeconds(gapLength);
            yield return StartCoroutine(longHaptic(vibrationLength,strength));
        }
    }

    IEnumerator droneHaptic(float vibrationLength, float gapLength, float strength)
    {
        strength = Mathf.Clamp01(strength);
        while (holding)
        {
            yield return new WaitForSeconds(gapLength);
            yield return StartCoroutine(longHaptic(vibrationLength, UnityEngine.Random.Range(0.25f, 0.75f)));
        }
    }

    public IEnumerator waveHaptic(int vibrationCount, float vibrationLength, float gapLength, float strengthStart, float strengthEnd)
    {
        float vibLengthStart = vibrationLength;
        float vibLengthEnd = 0.05f;
        strengthStart = Mathf.Clamp01(strengthStart);
        strengthEnd = Mathf.Clamp01(strengthEnd);
        print("strength start and end: " + strengthStart + " " + strengthEnd);

        for (int i = 0; i < vibrationCount; i++)
        {

                float transfer = playerNode.waveStrength.Evaluate((float)i / vibrationCount);
                float gap = Mathf.Lerp(gapLength, 0.001f, transfer);
                if (i != 0) yield return new WaitForSeconds(gap);

                //print("i / vibcount: " + (float)i / vibrationCount);
                float strength = Mathf.Lerp(strengthEnd, strengthStart, transfer);
                float vLength = Mathf.Lerp(vibLengthStart, vibLengthEnd, transfer);

                //print("wave strength: " + strength); //debug
                yield return StartCoroutine(longHaptic(vLength, strength));
        }

        playerNode.hapticWaving = false; //set waving bool to false when finished
    }

    public void performWaveHaptic(int vibrationCount, float vibrationLength, float gapLength, float strengthStart, float strengthEnd)
    {
        StartCoroutine(waveHaptic(vibrationCount, vibrationLength, gapLength, strengthStart, strengthEnd));
    }

    public void ClearRelatedObjects(uint inID)
    {
        GameObject tapDisplay = GameObject.Find("TapDisplay-" + inID);
        GameObject tendrilHalo = GameObject.Find("TendrilHalo-" + inID);

        if (tapDisplay) tapDisplay.GetComponent<sphereBlankScript>().CmdTDestroy();
        if (tendrilHalo) tendrilHalo.GetComponent<GoalStarActions>().DestroyObject();
    }
}