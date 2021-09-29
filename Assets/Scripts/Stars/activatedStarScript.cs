using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;
using Unity.Collections;
using Unity.Jobs;

/**
<summary>
    CD : activatedStarScript
    This is the only star script. Determines when it is dropped to activate dropdown menus, sounds, and tendrils
</summary>
**/
public class activatedStarScript : NetworkBehaviour 
{
    private delegate void UpdateEvent();
    private UpdateEvent update;

    #region PUBLIC_ENUM
    /// <summary>
    ///     ENUM : DragState
    ///     Values : FREE, LERP, ATTACHED, FREEING, FALLING
    ///     //not being dragged, grabbed and lerping to attach point, grabbed attached to controller, dropped but rotateing back to correct/standard orientation
    /// </summary>
    public enum DragState { FREE, LERP, ATTACHED, FREEING, FALLING };

    /// <summary>
    ///     ENUM : ActivationState
    ///     Values : UNSET, ACTIVATED, DEACTIVATED
    /// </summary>
    public enum ActivationState { UNSET, ACTIVATED, DEACTIVATED };
    #endregion

    #region PUBLIC_VAR
    //public bool activated = false;
    /// <summary>
    /// ZT: script for triggering UI Sounds
    /// </summary>
    public uiSounds uiSounds;

    /// <summary>
    ///     VD : starOrigin
    ///     Original star position
    /// </summary>
    [SyncVar]
	public Vector3 starOrigin;

    /// <summary>
    ///     VD : origPos
    ///     original position
    ///     // position with y offset I think (maybe becasue of cloth)
    /// </summary>
	[SyncVar]
	public Vector3 origPos;

    /// <summary>
    ///     VD : starID
    ///     Star ID
    /// </summary>
	[SyncVar]
	public int starID;

    /// <summary>
    ///     VD : playerID
    ///     Player ID
    /// </summary>
	[SyncVar]
	public int playerID;

    /// <summary>
    ///     VD : myText1
    ///     No References in Script
    /// </summary>
	[SyncVar]
	public string myText1 = "";

    /// <summary>
    ///     VD : myText2
    ///     No References in Script
    /// </summary>
	[SyncVar]
	public string myText2 = "";

    /// <summary>
    ///     VD : dbIP
    ///     IP address of database
    /// </summary>
	[SyncVar(hook = "OnChangedbIP")]
	public string dbIP;

    /// <summary>
    ///     VD : starPeriod
    ///     Only defined, never used
    /// </summary>
    [SyncVar(hook = "OnChangeStarPeriod")]
    public float starPeriod;

    /// <summary>
    ///     VD : dataColor
    ///     Data color for the shader
    /// </summary>
    [SyncVar]
    public Vector4 dataColor;

    /// <summary>
    ///     VD : dataUV0
    ///     More data for shader
    /// </summary>
    [SyncVar]
    public Vector4 dataUV0;

    /// <summary>
    ///     VD : dataUV1
    ///     More data for shader
    /// </summary>
    [SyncVar]
    public Vector4 dataUV1;

    /// <summary>
    ///     VD : padStatus
    ///     Defined only
    /// </summary>
    [SyncVar]
    public int padStatus;

    /// <summary>
    ///     VD : paramVal
    ///     Parameter for data values
    /// </summary>
    [SyncVar]
    public float paramVal;

    /// <summary>
    ///     VD : yOff
    ///     Used for drip of Shader
    /// </summary>
    [SyncVar]
    public float yOff;

    /// <summary>
    ///     VD : newYOff
    ///     Only defined
    /// </summary>
    [SyncVar]
    public float newYOff;

    /// <summary>
    ///     VD : timeToDie
    ///     OSC Message timeout
    /// </summary>
    [SyncVar]
    public double timeToDie;

    /// <summary>
    ///     VD : activationState
    ///     Current Activation State
    /// </summary>
    [SyncVar]
    public ActivationState activationState = ActivationState.UNSET;

    /// <summary>
    ///     VD : oldActivationState
    ///     Previous Activation State
    /// </summary>
    public ActivationState oldActivationState = ActivationState.UNSET;

    /// <summary>
    ///     VD : dragState
    ///     Current Drag State
    /// </summary>
    public DragState dragState = DragState.FREE;

    /// <summary>
    ///     VD : startFlag
    ///     On start toggle for update
    /// </summary>
    public bool startFlag = false;

    /// <summary>
    ///     VD : tendrilPrefab
    /// </summary>
	public GameObject tendrilPrefab;

    /// <summary>
    ///     VD : goalSpherePrefab
    /// </summary>
	public GameObject goalSpherePrefab;

    /// <summary>
    ///     VD : nonvariableTex
    /// </summary>
	public Texture nonvariableTex;

    /// <summary>
    ///     VD : variableTex
    /// </summary>
	public Texture variableTex;

    /// <summary>
    ///     VD : loadIDFlag
    /// </summary>
	private bool loadIDFlag = false;

    /// <summary>
    ///     VD : lerpDuration
    ///     Duration for new Lerper
    /// </summary>
	public float lerpDuration = 0.2f;

    /// <summary>
    ///     VD : heightToActivateStar
    ///     Height threshold for activating star
    /// </summary>
	public const float heightToActivateStar = 0.15f;

    /// <summary>
    ///     VD : holdingController
    ///     // this is the controller that is holding the star, if the star is not behing held then the holdingController is null
    /// </summary>
    public GameObject holdingController;
    /// <summary>
    ///     ZT : duration
    ///     // duration of star at initialization
    /// </summary>
    public float duration = 60; //Default to a minute
    /// <summary>
    ///     ZT : dataMapper
    ///     // grab the ioanDataMapper to do conversion for tapping sequence instruments
    /// </summary>
    public ioanDataMapper dataMapper;

    /// <summary>
    /// Toggle for star falling or blinking out on death
    /// </summary>
    public bool enableFallAnimation;
    #endregion

    #region PRIVATE_VAR
    /// <summary>
    ///     VD : lerper
    /// </summary>
    private Lerper lerper;

    /// <summary>
    ///     VD : oscPacketBus
    /// </summary>
	private OSCPacketBus oscPacketBus = null;

    /// <summary>
    ///     VD : Max distance to grab active star
    /// </summary>
    public float grab_d = 0.2f;

    /// <summary>
    ///     VD : celestialObject
    ///     // ID of the closest star to the controller when the trigger is pulled
    ///     //static int closestStarIDLeft = -1;
    ///     //static float closestStarDist = float.MaxValue;
    /// </summary>
    private GameObject celestialObject;
    /// <summary>
    /// VD: Stopcheck to ensure star cannot be activated by multiple players on accident
    /// </summary>
    public bool activated = false;
    /// <summary>
    /// VD: Textbox under the star object that displays the ID
    /// </summary>
    private TextMesh starIDText;

    /// <summary>
    ///     VD : display starIDText in opposite direction as well
    /// </summary>
    private TextMesh revStarIDText;

    /// <summary>
    /// VD: iOAN Specific script to format and display star data under the star
    /// </summary>
    private DataTextLoader textLoader;
    /// <summary>
    /// VD: Reference to the iOAN wwise voice manager
    /// </summary>
    IOANVoiceManager voiceManager;
    /// <summary>
    /// VD: Reference to the iOAN wwise voice sonifier
    /// </summary>
    IOANSonify sonifier;
    /// <summary>
    /// VD: Controller handedness boolean
    /// </summary>
    public bool isLeft;
    /// <summary>
    /// Refrance to activatedStarPhysics for active taps 
    /// </summary>
    public ActiveStarPhysics starPhysics;

    /// <summary>
    ///     VD : Extra late start flag
    /// </summary>
    public bool extra_start = true;

    /// <summary>
    ///     VD : Time the star spawns
    /// </summary>
    public double start_time;

    /// <summary>
    ///     VD : duration of decaying star
    /// </summary>
    float decayed_duration = 60;

    /// <summary>
    /// Variable for descending the star
    /// </summary>
    private float fallDist = 0.001f;

    /// <summary>
    /// Where Star Starts falling
    /// </summary>
    private float fallingHeight;

    /// <summary>
    ///     VD : Where star starts fading out
    /// </summary>
    private float fallingBottom;

    /// <summary>
    ///     VD : Done looking for stars for tendrils
    /// </summary>
    private bool ScanFinished = false;
    #endregion 

    #region UNITY_FUNC
    /**
    <summary>
        FD : Start()
        Gets ShaderSetter component from children
        Sets all ShadeSetter values to specific vals
        Sets init tagged object shadeSwapper comp addObscurePos
        Find celetialObject child object
        Call CmdSetID with starID
        Call CmdSetTag
    </summary>
    **/
    void Start()
    {
       //if (holdingController == null) UnityEngine.Debug.Log("Activated Star Script: Holding Controller Null at Start");
        ShaderSetter ss = GetComponentInChildren<ShaderSetter>();
        
        ss.blend =         dataColor.x * 12f - 3f;
        ss.poke =          dataColor.z * 1.2f - .8f;
        ss.edgeWidth =     dataUV1.z * -4.5f + .6f;
        ss.noiseScale =    dataUV1.x * 4f;
        ss.interiorScale = dataUV0.x + 0.25f;
        ss.exteriorScale = dataUV1.y * 3f + 2.75f;
        ss.noiseWeight =   dataUV0.w * 4f + 1f;
        ss.speed =         dataUV0.z * 2f - 1f;
        ss.rimExpo =       dataColor.x * -9f + 10f;
        ss.drip =          yOff * (8f / 0.28f);
        ss.rimColor =      Color.HSVToRGB(dataUV0.y, 1f, 1f);
        ss.gradColorA =    Color.HSVToRGB(dataColor.y, 1f, 1f);
        ss.gradColorB =    Color.HSVToRGB(1f, 0f, dataColor.w == 0f ? 0f : 1f);
        ss.animDuration =  0.5f;

        ss.gameObject.GetComponentInChildren<MeshRenderer>().material.mainTexture = dataColor.w == 1f ? nonvariableTex : variableTex;
        ss.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().material.mainTexture = dataColor.w == 1f ? nonvariableTex : variableTex;

        ss.starID = starID;

        if (isClient) CmdAddObscure(starID);

        voiceManager = FindObjectOfType<IOANVoiceManager>();
        dataMapper = FindObjectOfType<ioanDataMapper>();

        //Set Physics values
        starPhysics = GetComponent<ActiveStarPhysics>();
        starPhysics.mass = Mathf.Lerp(0.0f, 2.0f, (ss.drip + 4.0f) / 8.0f);
        starPhysics.length = Mathf.Lerp(0.0f, 5.0f, ss.noiseWeight / 10.0f);

        for (int i = 0; i < transform.childCount && celestialObject == null; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.tag == "CelestialObject") celestialObject = child;
        }

        starIDText = transform.Find("StarIDText").GetComponent<TextMesh>();
        revStarIDText = transform.Find("RevStarIDText").GetComponent<TextMesh>();
        textLoader = transform.Find("DataText").GetComponent<DataTextLoader>();

        ///Gets all ui sound objects and selects the one assigned to the player who activated the star. 
        uiSounds[] uiSoundArray = FindObjectsOfType<uiSounds>();
        foreach (uiSounds uiSoundSelect in uiSoundArray)
        {
            if (uiSoundSelect.playerID == playerID)
            {
                uiSounds = uiSoundSelect;
               // Debug.Log("UI FOUND");
                break;
            }
        }

        //get period from mesh
        float period = dataUV0.z;
       // Debug.Log("Activated Star period: " + period);
        //map to duration (this should grab from datamapper values?)
        //duration = 45.0f + ((120.0f + 45.0f) / (1.0f - 0.0f) * (period - 0.0f)); //def cleaner way to do this but leaving it in for changing later

        //Debug.Log("Activated Star duration: " + duration +'s');
        
        if(hasAuthority)
        {
            CmdSetID(starID);
            CmdSetTag();
        }

        start_time = NetworkTime.time;
        timeToDie = NetworkTime.time + 10;
    }

    /**
    <summary>
        FD : Update()
        If loadIDFlag isn't true
            Start the Coroutine LoadIDStuff with starID
            set loadIDFlag to true
        If gameObject has Authority
            And if startFlag isn't true
                set dragState to FREE
                Start the Coroutine sendTendril()
                set startFlag to true
                set lerper to Lerper equal to lerpDuration
                set timeToDie to time + 60 or 60 seconds ahead
            And if holdingController isn't null
                And if Steam controller HairTriggerUp
                    set holdingController to null
                    And if isAboveActivationLine()
                        And if dragState is equal to LERP or ATTACHED
                            Set dragState to FREEING
                            Reset lerper start time and InterpRotations to celestialObject rotation
                    Else
                        deactivate()
                Else
                    Reset timeToDie to 60 seconds later
                    Set attachPoint to 
        <remarks>
            IV
            NEEDS OPTIMIZATION
        </remarks>
    </summary>
    **/
    void Update()
    {
        if (sonifier != null) sonifier.networkTime = NetworkTime.time;

        decayed_duration = Mathf.Clamp(decayed_duration - Time.deltaTime,duration*0.25f, duration);
        if (extra_start && hasAuthority && NetworkTime.time - start_time > 0.05f)
        {
            CmdSetID(starID);
            CmdSetTag();
            extra_start = false;
        }

        //if(holdingController == null) UnityEngine.Debug.Log("Activated Star Script: Holding Controller Null at Update");
        if (uiSounds = null)
        {
            uiSounds[] uiSoundArray = FindObjectsOfType<uiSounds>();
            foreach (uiSounds uiSoundSelect in uiSoundArray)
            {
                if (uiSoundSelect.playerID == playerID)
                {
                    uiSounds = uiSoundSelect;
                    break;
                }
            }
        }
        if (voiceManager == null)
        {
            voiceManager = FindObjectOfType<IOANVoiceManager>();
        }

        if (hasAuthority)
        {
            if (!startFlag) // this all seems a bit more complicated than it needs to be EGM
            {
                if (holdingController == null) // somehow made but cant find controller
                {
                    //dragState = DragState.FREE; // starts out free because it is created when the dragged star is dropped
                }
                                            //				CmdSendOSCRel (playerID, starID,  transform.position); // send sound from released position

                startFlag = true;
                lerper = new Lerper(lerpDuration);
                //timeToDie = Time.time + 60; //old method
                timeToDie = NetworkTime.time + decayed_duration;
                if (sonifier != null)
                {
                    sonifier.timeToDie = (float)timeToDie;
                }
            }

            Vector3 attachPoint = transform.position;
            Quaternion attachRotation = celestialObject.transform.rotation;

            if (holdingController != null)
            {
                bool steamClick = false;
                if (OVRPlugin.GetSystemHeadsetType() == 0)
                {
                    steamClick = !holdingController.GetComponent<iOANHeteroController>().steamInput.GetHairTrigger();
                    //if (steamClick) Debug.Log("Trigger Release!");
                }

                bool OVRUpLeft = false;
                bool OVRUpRight = false;

                if (holdingController.GetComponent<iOANHeteroController>().playerNode.isOVR && !OVRInput.Get(OVRInput.RawButton.LIndexTrigger))
                {
                    OVRUpLeft = true;
                    //Debug.Log("Left Trigger Release!");
                }

                if (holdingController.GetComponent<iOANHeteroController>().playerNode.isOVR && !OVRInput.Get(OVRInput.RawButton.RIndexTrigger))
                {
                    OVRUpRight = true;
                    //Debug.Log("Right Trigger Released!");
                }

                if ((OVRUpRight && OVRUpLeft) || steamClick)
                {
                   // UnityEngine.Debug.Log("What are we doing here tho?");
                    holdingController = null;

                    if (isAboveActivationLine())
                    {
                        //dummy comment
                        if ((dragState == DragState.LERP) || (dragState == DragState.ATTACHED))
                        {
                            dragState = DragState.FREEING;                            
                            
                            if (sonifier != null)
                            {
                                timeToDie = NetworkTime.time + decayed_duration;
                                sonifier.timeToDie = (float)timeToDie;
                            }
                            

                            lerper.setStartTime((float)NetworkTime.time);
                            lerper.setInterpRotations(celestialObject.transform.rotation, Quaternion.identity);
                        }

                        //This activates the star after being picked up
                        if (!activated) ActivateStar();
                    }
                    else
                    {
                        if (uiSounds != null) uiSounds.returnSound(gameObject, playerID); //triggers ui sound for returning object to the mesh
                        Debug.Log("Activation Death");
                        destroyStar(); //discarded
                    }
                }
                else //if its being held re-up the time to die
                {

                    //Debug.Log("renewed timer");
                    attachPoint = holdingController.transform.TransformPoint(new Vector3(0f, 0f, 0.1f));
                    attachRotation = holdingController.transform.rotation;

                }
            }
            else
            {
                /*
                //SERVER STAR DROP TEST
                if (!activated)
                {
                    GameObject[] playerList = (GameObject.FindGameObjectsWithTag("heteroPlayer"));
                    GameObject instancePlayer = Array.Find(playerList, obj => obj.GetComponent<iOANHeteroPlayer>().playerID == (iOANPlayerUtil.playerID)playerID);
                    holdingController = isLeft ? instancePlayer.GetComponent<iOANHeteroPlayer>().LeftController : instancePlayer.GetComponent<iOANHeteroPlayer>().RightController;
                }
                //SERVER STAR DROP TEST
                */

                if (NetworkTime.time > timeToDie) //no osc message received time out
                {
                    //Debug.Log("Time Death");
                    destroyStar();
                }
            }

            switch (dragState)
            {
                case DragState.FREE:
                    {
                        if (activated) {
                        }
                        if (holdingController != null) // just picked up
                        {
                            dragState = DragState.LERP;
                            lerper.setStartTime((float)NetworkTime.time);
                            lerper.setInterpPoints(transform.position, attachPoint);
                            lerper.setInterpRotations(celestialObject.transform.rotation, attachRotation);
                        }
                        //else  UnityEngine.Debug.Log("Activated Star Script: Holding Controller Null in FREE state");
                    }
                    break;
                case DragState.LERP: // it is impossible to be here an not held
                    {
                        float activeStarDistance = Vector3.Distance(attachPoint, celestialObject.transform.position);
                        if (lerper.update((float)NetworkTime.time) >= 1 && activeStarDistance <= grab_d)
                        {
                            dragState = DragState.ATTACHED;
                           
                        }
                        lerper.updateGoal(attachPoint, attachRotation);
                        transform.position = lerper.getPoint();
                        celestialObject.transform.rotation = lerper.getRotation();
                    }
                    break;
                case DragState.ATTACHED: // it is impossible to be here an not held
                    {
                        starPhysics.SetAnchor(true);
                        
                        celestialObject.transform.rotation = attachRotation;
                        transform.position = attachPoint;
                    }
                    break;
                case DragState.FREEING:
                    {
                        starPhysics.SetAnchor(false);
                        if (lerper.update((float)NetworkTime.time) >= 1) dragState = DragState.FREE;

                        celestialObject.transform.rotation = lerper.getRotation();
                    }
                    break;
            }
        }

        if (isAboveActivationLine())
        {
            if (hasAuthority) activationState = ActivationState.ACTIVATED;

            ShaderSetter cs = transform.Find("CelestialObject").GetComponent<ShaderSetter>();
            cs.gradColorA = cs.gradColorAOrig * 1.0f;
            cs.gradColorB = cs.gradColorBOrig * 1.0f;
            cs.edgeColor = cs.edgeColorOrig * 1.0f;
            cs.rimColor = cs.rimColorOrig * 1.0f;
            cs.exteriorScale = cs.exteriorScaleOrig * 1.0f;
            cs.interiorScale = cs.interiorScaleOrig * 1.0f;

            if (dragState != DragState.FALLING)
            {
                cs.alpha = 1.0f;
                textLoader.SetTotalAlpha(1.0f);
                starIDText.gameObject.GetComponent<MeshRenderer>().material.SetFloat("Alpha", 1.0f);
                revStarIDText.gameObject.GetComponent<MeshRenderer>().material.SetFloat("Alpha", 1.0f);
            }
            else
            {
                float y = transform.position.y;
                float p = (y - fallingBottom) / (fallingHeight - fallingBottom);

                float newAlpha = Mathf.Lerp(0.01f, 1.0f, p);
                cs.alpha = newAlpha;
                textLoader.SetTotalAlpha(newAlpha);
                starIDText.gameObject.GetComponent<MeshRenderer>().material.SetFloat("Alpha", newAlpha);
                revStarIDText.gameObject.GetComponent<MeshRenderer>().material.SetFloat("Alpha", newAlpha);
            }
        }
        else
        {
            if (hasAuthority) activationState = ActivationState.DEACTIVATED;

            ShaderSetter cs = transform.Find("CelestialObject").GetComponent<ShaderSetter>();
            float cMod = 0.3f;
            float sMod = 0.6f;
            cs.gradColorA = cs.gradColorAOrig * cMod;
            cs.gradColorB = cs.gradColorBOrig * cMod;
            cs.edgeColor = cs.edgeColorOrig * cMod;
            cs.rimColor = cs.rimColorOrig * 1.0f;
            cs.exteriorScale = cs.exteriorScaleOrig * (sMod * 0.8f);
            cs.interiorScale = cs.interiorScaleOrig * (sMod * 1.2f);

            float y = transform.position.y;
            float p = (y - fallingBottom) / (fallingHeight - fallingBottom);

            float newAlpha = Mathf.Lerp(0.01f, 1.0f, p);
            cs.alpha = newAlpha;
            textLoader.SetTotalAlpha(newAlpha);
            starIDText.gameObject.GetComponent<MeshRenderer>().material.SetFloat("Alpha", newAlpha);
            revStarIDText.gameObject.GetComponent<MeshRenderer>().material.SetFloat("Alpha", newAlpha);
        }

        //if ((oldActivationState != activationState) && (oldActivationState != ActivationState.UNSET)) CmdSendCrossThreshMessage(activationState == ActivationState.ACTIVATED);

        oldActivationState = activationState;

        update?.Invoke();
    }
    /// <summary>
    /// FD: Terminates sounds in wwwise when the star dies, if need be, also replaces the star on the stargrid
    /// </summary>
    private void OnDestroy()
    {
        //GameObject.FindGameObjectWithTag("init").GetComponent<shadeSwapper>().RemoveObscurePos(starID);

        if (sonifier != null) //If there was a sonifier assigned...
        {
            if (sonifier.isDroning)
            {
                voiceManager.releaseDrone(sonifier);
                sonifier = null;
            }
            else if (sonifier.isSequencing)
            {
                voiceManager.releaseSequence(sonifier);

                sonifier = null;
            }

        }
    }
    #endregion

    #region PUBLIC_FUNC
    /** 
    <summary> 
        FD : getOSCPacketBus()
        Returns v:oscPacketBus
        If null, finds Object with tag "OSCPacketBus"
    </summary> 
    **/
    public OSCPacketBus getOSCPacketBus()
    {
        if (oscPacketBus == null)
        {
            GameObject oscBus = GameObject.FindGameObjectWithTag("OSCPacketBus");

            if (oscBus != null) oscPacketBus = oscBus.GetComponent<OSCPacketBus>();
        }

        return oscPacketBus;
    }

    /**
    <summary> 
        FD : grabStar(GameObject)
        Sets v:holdingController to the holding Controller and v:dragState to FREE
        <param name="controller">Using Controller</param>
    </summary>
    **/
    public IEnumerator grabStar(GameObject controller)
    {
        
        // the controller that is grabbing the object
        // and the player netidentiry that is holding the controller
        // should only be used by client taking controll
        //if (holdingController == null) UnityEngine.Debug.Log("Activated Star Script: Holding Controller Null before Setting");
        holdingController = controller;

        if (dragState == DragState.FALLING)
        {
            update = null;
            if (sonifier) sonifier.linkPosition(gameObject);
        }

        dragState = DragState.FREE; // setting it to free will have it start lerping again.
        //UnityEngine.Debug.Log("Grab star on activated star script");
        yield return new WaitForSeconds(0f);
    }

    /**
    <summary> 
       FD : destroyStar(float)
       Calls f:CmdDestroy with dur
       Can descend star from grid before destroying it
       <param name="dur">Time to Destroy</param>
    </summary>
    **/
    public void destroyStar()
    {
        if (hasAuthority)
        {
            if (enableFallAnimation)
            {
                if (dragState != DragState.FALLING)
                {
                    dragState = DragState.FALLING;
                    if (sonifier) sonifier.unlinkPosition();
                    fallingHeight = transform.position.y;
                    fallingBottom = heightToActivateStar - 0.5f;

                    if (isAboveActivationLine()) fallDist = 0.001f;
                    else fallDist = 0.01f;

                    update += () =>
                    {
                        Vector3 pos = transform.position;
                        pos.y -= fallDist;

                        if (pos.y > -1f) fallDist *= 1.1f;
                        else fallDist *= 1.6f;

                        transform.position = pos;
                        if (isServer) RpcMoveStar(pos);

                        if (pos.y < -750f)
                        {
                            CmdDestroy();
                            if (isClient) CmdRemoveObscure(starID);
                        }
                    };
                }
            }
            else
            {
                CmdDestroy();
                if (isClient) CmdRemoveObscure(starID);
            }
        }
    }

    [ClientRpc]
    private void RpcMoveStar(Vector3 pos)
    {
        transform.position = pos;
    }

    /**
    <summary>
        FD : CmdSendCrossThreshMessage(bool)
        Based on v:isAbove, calls above or below activation threshold
        <param name="isAbove"></param>
    </summary>
    **/ /*
    [Command]
    public void CmdSendCrossThreshMessage(bool isAbove)
    {
        if (isAbove) getOSCPacketBus().dragAboveActivationThreshold(playerID, starID);
        else getOSCPacketBus().dragBelowActivationThreshold(playerID, starID);
    }*/

     /**
     <summary>
        FD : CmdSendDeactivateMsg()
        Gets the OSCPacketBus and calls activatedStartDeslected() with v:playerID and v:starID
     </summary>
     **/
    [Command]
    public void CmdSendDeactivateMsg()
    {
        getOSCPacketBus().activatedStartDeselected(playerID, starID);
    }
    #endregion

    #region PRIVATE_FUNC
    /// <summary>
    ///     FD : Add obscure star id onto server obscure list
    /// </summary>
    /// <param name="inID"></param>
    [Command]
    private void CmdAddObscure(int inID)
    {
        GameObject.FindGameObjectWithTag("init").GetComponent<shadeSwapper>().AddObscurePos(starID);
        RpcAddObscure(inID);
    }

    /// <summary>
    ///     FD : Add obscure star id onto client obscure list
    /// </summary>
    /// <param name="inID"></param>
    [ClientRpc]
    private void RpcAddObscure(int inID)
    {
        GameObject.FindGameObjectWithTag("init").GetComponent<shadeSwapper>().AddObscurePos(starID);
    }

    /// <summary>
    ///     FD : Remove obscure star id onto server obscure list
    /// </summary>
    /// <param name="inID"></param>
    [Command]
    private void CmdRemoveObscure(int inID)
    {
        GameObject.FindGameObjectWithTag("init").GetComponent<shadeSwapper>().RemoveObscurePos(starID);
        RpcRemoveObscure(inID);
    }

    /// <summary>
    ///     FD : Remove obscure star id onto client obscure list
    /// </summary>
    /// <param name="inID"></param>
    [ClientRpc]
    private void RpcRemoveObscure(int inID)
    {
        GameObject.FindGameObjectWithTag("init").GetComponent<shadeSwapper>().RemoveObscurePos(starID);
    }


    /// <summary>
    /// FD: Funny syncvar for preventing multi-spawning of tendrils
    /// </summary>
    [Command]
    private void CmdActivateStar()
    {
        activated = true;

        RpcActivateStar();
    }
    /// <summary>
    /// FD: Funny syncvar for preventing multi-spawning of tendrils
    /// </summary>
    [ClientRpc]
    private void RpcActivateStar()
    {
        activated = true;
    }
    /// <summary>
    /// FD: spawns pulses and tendrils and loads star data 
    /// </summary>
    private void ActivateStar()
    {
        LinesManager lineHolder = GameObject.Find("lineHolder").GetComponent<LinesManager>();
        Vector3 pulseHSV;
        Color.RGBToHSV(GetComponentInChildren<ShaderSetter>().gradColorA, out pulseHSV.x, out pulseHSV.y, out pulseHSV.z);
        pulseHSV.y *= 2f;
        if (pulseHSV.y > 1f) pulseHSV.y = 1f;
        lineHolder.SpawnPulses(Color.HSVToRGB(pulseHSV.x, pulseHSV.y, pulseHSV.z), true);
        
        StartCoroutine(LoadIDStuff(starID));
        StartCoroutine(SendTendrilBeta());
        CmdActivateStar(); // testing to see if this can go BEFORE sendTendril
        Cmd_wwiseLinkedEvent();
    }
    /**
    <summary>
        FD : OnChangedbIP(string)
        Set v:dbIP to inIP
        <param name="inIP">Input IP</param>
    </summary>
    **/
    void OnChangedbIP(string oldIP, string inIP)
    {
		dbIP = inIP;
	}

    /**
    <summary>
        FD : OnChangeStarPeriod(float)
        Set v:starPeriod to inPeriod
        <param name="inPeriod">Input starPeriod</param>
    </summary>
    **/
	void OnChangeStarPeriod(float oldPeriod, float inPeriod)
    {
		starPeriod = inPeriod;
	}

    /**
    <summary>
        FD : CmdDestroy(float)
        Starts coroutine f:tDestroy() with dur
        <param name="dur"></param>
    </summary>
    **/
	[Command]
	void CmdDestroy()
    {
		StartCoroutine(tDestroy());
	}

    /**
    <summary>
        FD : tDestroy(float)
        Wait for dur seconds
        Then destroy gameObject in Server
        <param name="dur">Time to destroy gameObject</param>
    </summary>
    **/
	IEnumerator tDestroy()
    {
        yield return new WaitForEndOfFrame();
		NetworkServer.Destroy(gameObject);

		//RpcDestroy ();
	}

    /**
    <summary>
        FD : CmdSetTendrilParams(Vector3, Vector3)
         sets new tendril with Vector3 values if isServer
        <param name="start">Start position of Tendril</param>
        <param name="end">End Position of Tendril</param>
    </summary>
    **/
    [Command]
	void CmdSetTendrilParams(Vector3 start, Vector3 end)
    {
		if (isServer)
        {
			Tendril myTendril = GetComponentInChildren<Tendril> ();

			myTendril.startPosition = start;
			myTendril.endPosition = end;

			//myTendril.transform.localScale = new Vector3 (.5f, .5f, .5f);

			myTendril.enabled = true;
		}

		RpcSetTendrilParams(start, end);
	}

    /**
   <summary>
        FD : RpcSetTendrilParams(Vector3, Vector3)
        sets new tendril with Vector3 values if isServer
        <param name="start">Start position of Tendril</param>
        <param name="end">End Position of Tendril</param>
   </summary>
   **/
    [ClientRpc]
	void RpcSetTendrilParams(Vector3 start, Vector3 end)
    {
		Tendril myTendril = GetComponentInChildren<Tendril>();

		myTendril.startPosition = start;
		myTendril.endPosition = end;

		//myTendril.transform.localScale = new Vector3 (.5f, .5f, .5f);

		myTendril.enabled = true;
	}

    /**
    <summary>
        FD : CmdSetTag()
        Call f:RpcSetTag()
    </summary>
    **/
	[Command]
	void CmdSetTag()
    {
		RpcSetTag ();
	}

    /**
    <summary>
        FD : RpcSetTag()
        Set tag to "activatedStar"
    </summary>
    **/
	[ClientRpc]
	void RpcSetTag()
    {
		gameObject.tag = "activatedStar";
	}
    /// <summary>
    /// FD: Set's the star's textbox ID
    /// </summary>
    /// <param name="inID"></param>
    void SetID(int inID)
    {
        starIDText.text = inID.ToString();
        revStarIDText.text = inID.ToString();
    }

    /**
    <summary>
        FD : CmdSetID(int)
        Call f:RpcSetID(inID)
        <param name="inID">Input ID</param>
    </summary>
    **/
	[Command]
	void CmdSetID(int inID)
    {
        SetID(inID);
		RpcSetID(inID);
	}

    /**
    <summary>
        FD : RpcSetID(int)
        Set all textMesh texts in children to inID if 
        <param name="inID">Input IP</param>
    </summary>
    **/
	[ClientRpc]
    void RpcSetID(int inID)
    {
        SetID(inID);
    }

    /**
    <summary>
        FD : isAboveActivationLine()
        Checks if the difference between current height and previous height is greater than v:heightToActivateStar
    </summary>
    **/
    bool isAboveActivationLine()
    {
		return transform.position.y > heightToActivateStar;
	}

    /**
    <summary>
       FD : LoadIDStuff(int)
       Connects to url and parse through data for star at int
       <param name="id">Star ID</param>
    </summary>
    **/
    IEnumerator LoadIDStuff(int id)
    {
        //string loadURL = "http://192.168.1.129/ioan_newidtester.php?id=" + id;
        //Connects to url given with default of localhost

        yield return new WaitUntil(() => ScanFinished);

        dbIP = Config.Instance.Data.dataBaseIP;

        if (dbIP == null)
        {
            Debug.Log("databaseIP not set properly. defaulting to localhost");
            dbIP = "localhost";
        }

        string loadURL = "http://" + dbIP + "/ioan_newidtester_fig_2.php?id=" + id;

        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(loadURL))
        {
            yield return request.SendWebRequest();

            string graphURL = "";
            List<string> oldSplitVals = request.downloadHandler.text.Split(new string[] { "<br/>" }, System.StringSplitOptions.None).ToList();
            List<string> splitVals = new List<string>();

            foreach (string val in oldSplitVals)
            {
                if (val.Length < 2) splitVals.Add(val);
                else if (val.Substring(0, 6) == "http:/" || val.Substring(0, 6) == "https:") graphURL = val;
                //else if (val.Substring(0, 6) == "fields") { /* splitVals.Remove (val); */ }
                else if (val.Substring(0, 6) == "Figure") { /* splitVals.Remove (val); */ }
                else splitVals.Add(val);
            }

            if (isClient) CmdSetQueryText(splitVals.ToArray(), graphURL, starID);
        }
    }

    /**
    <summary>
        FD : CmdSetQueryText(string, string, int)
        Set v:myText0 to intext0
        Find object with tag "textSphere" and calls DataDisplay addLineUDP with inID and intext0
        Calls f:RpcUpdateCanvas with params
        <param name="intext0">Input Text0</param>
        <param name="inGraphURL">Graph URL</param>
        <param name="inID">Input StarID</param>
    </summary>
    **/
    [Command]
	void CmdSetQueryText (string[] splitVals, string inGraphURL, int inID)
    {
        string outVal = string.Join("\n", splitVals);
        if (isServerOnly)
        {
            textLoader.OutputData(splitVals, inGraphURL, true);

            GameObject.FindGameObjectWithTag("textSphere").GetComponent<DataDisplay>().addLineUDP(inID.ToString() + "\n" + outVal);
            GameObject.FindGameObjectWithTag("textSphere").GetComponent<DataDisplay>().addLine(inID.ToString() + "\n" + outVal);
        }

        RpcUpdateCanvas(inGraphURL, splitVals, inID);
	}

    /**
    <summary>
       FD : RpcUpdateCanvas(string, string, int)
        Start Coroutine f:LoadGraphImgwith inGraphURL
        Set all TextMesh texts in children to intext0 if charactersize isn't .003
       <param name="intext0">Text for TextMesh</param>
       <param name="inGraphURL">Graph URL</param>
       <param name="inID">Star ID</param>
    </summary>
    **/
    [ClientRpc]
    void RpcUpdateCanvas (string inGraphURL, string[] splitVals, int inID)
    {
        string outVal = string.Join("\n", splitVals.ToArray());
        //StartCoroutine(LoadGraphImg(inGraphURL));

        textLoader.OutputData(splitVals, inGraphURL);

        GameObject.FindGameObjectWithTag("textSphere").GetComponent<DataDisplay>().addLine(inID.ToString() + "\n" + outVal);
	}

    /**
    <summary>
        FD : sendTendril()
        Wait 5 seconds
        Find all Objects with tag "player"
        For all ids in players, if not v:playerID and between 0 and 2, add this id to an id list
        If id list is empty, add -1
        If -1 is the first element in list
            Set myNewRaycastPos to position + random Vector2
        Else
            Set myNewRaycastPos to random player position
        If there is something below object
            Wait .005 seconds
            If the distance between what was hit and myNewRaycastPos < 999
            Create new pos Vector3 based on mesh calculations
            If there is something below this pos
                Wait .005 seconds
                Calculate new list of ints based on hitmesh triangles
                Collect mesh colors based on values in list
                Find the final Indices between tendrils
                For all tendrils to spawn
                    Spawn new tendrils based on input data
    </summary>
    **/
    private IEnumerator SendTendrilAlpha()
    {
        yield return new WaitForSeconds(0.005f);

        //Gather all Players
        GameObject[] playerList = GameObject.FindGameObjectsWithTag("heteroPlayer");
        List<int> ids = new List<int>();
        for (int i = 0; i < playerList.Length; i++)
        {
            if (playerList[i].GetComponent<iOANHeteroPlayer>().playerID != (iOANPlayerUtil.playerID)playerID &&
                playerList[i].GetComponent<iOANHeteroPlayer>().playerID >= (iOANPlayerUtil.playerID)0 &&
                playerList[i].GetComponent<iOANHeteroPlayer>().playerID <= (iOANPlayerUtil.playerID)2) ids.Add(i);
        }

        //Find Tendril "Source" position
        Vector3 myNewRaycastPos;

        if (ids.Count == 0)
        {
            Vector2 randVec = (new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f))).normalized;
            myNewRaycastPos = transform.position + new Vector3(randVec.x, 0f, randVec.y);
        }
        else
        {
            int selectedPlayer = ids[UnityEngine.Random.Range(0, ids.Count)];
            myNewRaycastPos = playerList[selectedPlayer].transform.Find("Head").position;
            myNewRaycastPos.y = 0;
        }

        //Get StarGrid Collider
        int layerMask2 = LayerMask.GetMask("cloth");
        bool hitMissTwo = Physics.Raycast(myNewRaycastPos + Vector3.up * 20f, Vector3.down, out RaycastHit hit, Mathf.Infinity, layerMask2);
        if (!hitMissTwo) yield break;
		yield return new WaitForSeconds (0.005f);

        //Get StarGrid Mesh
		Mesh hitMesh = (hit.collider as MeshCollider).sharedMesh;

        //Get Vertices for the Triangle Hit
        int[] hitTris = { hitMesh.triangles[hit.triangleIndex * 3 + 0],
						  hitMesh.triangles[hit.triangleIndex * 3 + 1],
						  hitMesh.triangles[hit.triangleIndex * 3 + 2] };

        //Get the Triangle indices of all connected triangles
		List<int> newIndexL = new List<int>();
		foreach (int id in hitTris)
            newIndexL.AddRange(hitMesh.triangles.Select((b, i) => b == id ? i : -1).Where((i) => i != -1).ToList());
		yield return new WaitForSeconds (0.005f);

        //For every index in newIndexL, get all the indices for that triangle
		List<int> vVals = new List<int>();
		foreach (int val in newIndexL)
        {
			vVals.AddRange (new int[] { hitMesh.triangles[val - (val % 3) + 0],
								        hitMesh.triangles[val - (val % 3) + 1],
								        hitMesh.triangles[val - (val % 3) + 2] });
		}

        ScanFinished = true;

        // THIS SELECTS THE PARAM (for every vertice index, get the rgba associated with the padStatus)
        vVals = new HashSet<int>(vVals).ToList();
        List<Vector2> dataVals = new List<Vector2>();
        Initializer init = GameObject.FindGameObjectWithTag("init").GetComponent<Initializer>();
        List<Vector4> uv0 = new List<Vector4>();
        List<Vector4> uv1 = new List<Vector4>();
        hitMesh.GetUVs(0, uv0);
        hitMesh.GetUVs(1, uv1);

        int selIndex = init.getIndexForStarID((uint)starID);
        float paramVal;
        switch (padStatus)
        {
            case 0:
                paramVal = uv1[selIndex].x;
                break;
            case 1:
                paramVal = uv0[selIndex].w;
                break;
            case 2:
                paramVal = uv0[selIndex].y;
                break;
            case 3:
                paramVal = uv1[selIndex].y;
                break;
            default:
                paramVal = 0f;
                break;
        }

        for (int i = 0; i < vVals.Count(); i++)
        {
            float vertexVal;
            switch(padStatus)
            {
                case 0:
                    vertexVal = uv1[vVals[i]].x;
                    break;
                case 1:
                    vertexVal = uv0[vVals[i]].w;
                    break;
                case 2:
                    vertexVal = uv0[vVals[i]].y;
                    break;
                case 3:
                    vertexVal = uv1[vVals[i]].y;
                    break;
                default:
                    vertexVal = 0f;
                    break;
            }

            dataVals.Add(new Vector2(vertexVal, i));
        }

        dataVals = dataVals.OrderBy(x => Mathf.Abs(paramVal - x.x)).ToList();

        // CHANGE THIS TO SPAWN MORE DUDES
        const int numTendrilsToSpawn = 2;

        //Get the vertex indices of the star closest in value to this stars param value, spawn tendril directed towards that vertex
        for (int i = 0; i < numTendrilsToSpawn; i++)
        {
            int index = vVals[(int)dataVals[i].y];
			int selectedID = (int)hitMesh.normals[index].x * 10000 + (int)hitMesh.normals[index].y;

			CmdSpawnTendril(transform.position, hitMesh.vertices[index], selectedID, padStatus, dataVals[i].x, hitMesh.vertices[index].y, padStatus, playerID);
		}
	}

    /// <summary>
    ///     FD : Current Method for finding tendril stars
    /// </summary>
    /// <returns></returns>
    private IEnumerator SendTendrilBeta()
    {
        yield return new WaitForSeconds(0.005f);

        //Gather all Players
        GameObject[] playerList = GameObject.FindGameObjectsWithTag("heteroPlayer");
        List<int> ids = new List<int>();
        for (int i = 0; i < playerList.Length; i++)
        {
            if (playerList[i].GetComponent<iOANHeteroPlayer>().playerID != (iOANPlayerUtil.playerID)playerID &&
                playerList[i].GetComponent<iOANHeteroPlayer>().playerID >= (iOANPlayerUtil.playerID)0 &&
                playerList[i].GetComponent<iOANHeteroPlayer>().playerID <= (iOANPlayerUtil.playerID)2) ids.Add(i);
        }

        //Find Tendril "Source" position
        Vector3 myNewRaycastPos;

        if (ids.Count == 0)
        {
            Vector2 randVec = (new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f))).normalized;
            myNewRaycastPos = transform.position + new Vector3(randVec.x, 0f, randVec.y);
        }
        else
        {
            int selectedPlayer = ids[UnityEngine.Random.Range(0, ids.Count)];
            myNewRaycastPos = playerList[selectedPlayer].transform.Find("Head").position;
        }
        myNewRaycastPos.y = 0;

        float maxStarDist = 0.25f;
        //int querySize = 1;
        Initializer init = GameObject.FindGameObjectWithTag("init").GetComponent<Initializer>();
        Mesh submesh = init.InteractiveSubMesh.GetComponent<MeshFilter>().mesh;
        /*Mesh[] meshes = init.Meshes.OrderBy((mf) =>
        {
            Vector2 pos = mf.gameObject.GetComponent<SubMeshLoader>().pos;
            return (new Vector3(pos.x, 0.0f, pos.y) - myNewRaycastPos).magnitude;
        }).Select((mf) => mf.mesh).Take(querySize).ToArray();*/

        // CHANGE THIS TO SPAWN MORE DUDES
        const int numTendrilsToSpawn = 2;
        NativeArray<int> indices = new NativeArray<int>(numTendrilsToSpawn, Allocator.TempJob);
        NativeArray<float> value = new NativeArray<float>(numTendrilsToSpawn, Allocator.TempJob);

        List<Vector4> uv0s = new List<Vector4>();
        List<Vector4> uv1s = new List<Vector4>();

        submesh.GetUVs(0, uv0s);
        submesh.GetUVs(1, uv1s);

        FindTendrilDestJob tendrilJob = new FindTendrilDestJob()
        {
            verts = new NativeArray<Vector3>(submesh.vertices, Allocator.TempJob),
            uv0 = new NativeArray<Vector4>(uv0s.ToArray(), Allocator.TempJob),
            uv1 = new NativeArray<Vector4>(uv1s.ToArray(), Allocator.TempJob),
            myNewRaycastPos = myNewRaycastPos,
            maxStarDist = maxStarDist,
            selIndex = init.getIndexForStarID((uint)starID),
            padStatus = padStatus,
            selectIndices = indices,
            indexValue = value
        };

        ScanFinished = true;

        JobHandle handle = tendrilJob.Schedule();
        yield return new WaitUntil(() => handle.IsCompleted);
        handle.Complete();

        //Get the vertex indices of the star closest in value to this stars param value, spawn tendril directed towards that vertex
        for (int i = 0; i < numTendrilsToSpawn; i++)
        {
            int index = indices[i];
            int selectedID = (int)submesh.normals[index].x * 10000 + (int)submesh.normals[index].y;

            CmdSpawnTendril(transform.position, submesh.vertices[index], selectedID, padStatus, value[i], submesh.vertices[index].y, padStatus, playerID);
        }

        indices.Dispose();
        value.Dispose();

        tendrilJob.verts.Dispose();
        tendrilJob.uv0.Dispose();
        tendrilJob.uv1.Dispose();
    }

    /// <summary>
    ///     Fd : Basic denormalization function
    /// </summary>
    /// <param name="val"></param>
    /// <param name="max"></param>
    /// <param name="min"></param>
    /// <returns></returns>
    private float denormalizeToRange(float val, float max, float min)
    {
        return val * (max - min) + min;
    }

    private IEnumerator SendTendrilGamma()
    {
        yield return new WaitForSeconds(0.005f);

        Initializer init = GameObject.FindGameObjectWithTag("init").GetComponent<Initializer>();
        Mesh submesh = init.InteractiveSubMesh.GetComponent<MeshFilter>().mesh;
        SFLimitsStruct limits = Config.Instance.GetLimits();
        SFScales scales = Config.Instance.GetScales();

        Vector3 newPos = Vector3.zero; // GetCentroid();
        newPos.x = newPos.x / scales.xScale + scales.xOffset;
        newPos.x = denormalizeToRange(newPos.x, limits.ra.x, limits.ra.y);
        newPos.z = newPos.z / scales.zScale + scales.zOffset;
        newPos.z = denormalizeToRange(newPos.z, limits.dec.x, limits.dec.y);

        dbIP = Config.Instance.Data.dataBaseIP;
        if (dbIP == null) dbIP = "localhost";
        string loadURL = "http://" + dbIP + "/ioan_proximity_query.php?xPos=" + newPos.x + "&zPos=" + newPos.z + "&radius=0.005&paramEnum=0&paramVal=10";

        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(loadURL))
        {
            yield return request.SendWebRequest();

            uint id = uint.Parse(request.downloadHandler.text);
            int index = init.getIndexForStarID(id);

            CmdSpawnTendril(transform.position, submesh.vertices[index], (int)id, padStatus, 0.0f, submesh.vertices[index].y, padStatus, playerID);
        }
    }

    /**
    <summary>
        FD : CmdSpawnTendrilGoalSphere(Vector3, int, int, float, float)
        Instantiate new v:goalSpherePrefab
        Create string based on inStarID, param, and paramVal
        Set new SpherePrefab statusText to new Text and starYOffset to inStarYOffset
        Spawn new sphere on server
        <param name="inPos">Start Position</param>
        <param name="inStarID">Star ID</param>
        <param name="param">Part of Statistics</param>
        <param name="paramVal">Part of Statistics</param>
        <param name="inStarYOffset"></param>
    </summary>
    **/
    [Command]
	void CmdSpawnTendrilGoalSphere(Vector3 inPos, int inStarID, int param, float paramVal, float inStarYOffset)
    {
		var myGoalSphere = Instantiate(goalSpherePrefab, inPos, Quaternion.identity);
		string statText = inStarID.ToString() + "\n" + param.ToString() + "\n" + paramVal.ToString() + "\n";

		myGoalSphere.GetComponent<GoalStarActions>().statusText = statText;
		myGoalSphere.GetComponent<GoalStarActions>().starYOffset = inStarYOffset;

		NetworkServer.Spawn(myGoalSphere);
	}

    /**
    <summary>
        FD : CmdSpawnTendril(Vector3, Vector3, int, int, float, float, int, int)
        Instantiate new v:tendrilPrefab Object
        Set start and end position to params
        Create new StatText, first with inStarID, then with a color based on the value of param
        Then add paramVal to the string
        Set tendril statusText to statText, starYOffset to inStarYOffset, padStatus to inPadStat, and playerID to inPlayerID
        Spawn this object on server with client authority
        <param name="start">Start of Tendril Position</param>
        <param name="end">End of Tendril Position</param>
        <param name="inStarID">Star ID</param>
        <param name="param">Part of Statistics</param>
        <param name="paramVal">Part of Statistics</param>
        <param name="inStarYOffset"></param>
        <param name="inPadStat"></param>
        <param name="inPlayerID"></param>
    </summary>
    **/
    [Command]
    private void CmdSpawnTendril(Vector3 start, Vector3 end, int inStarID, int param, float paramVal, float inStarYOffset, int inPadStat, int inPlayerID)
    {
        //Set the Tendril Data
        float satMod = 0.65f;
        string statText = inStarID.ToString() + "\n";
        float min, max;

        if (param == 0)
        {
            statText += "Parameter: <color=#" + ColorUtility.ToHtmlStringRGBA(Color.HSVToRGB(0f, 0f, 1f)) + ">Lightcurve RMS</color>\n";
            min = 0f;
            max = 0.6715103569670999f;
        }
        else if (param == 1)
        {
            statText += "Parameter: <color=#" + ColorUtility.ToHtmlStringRGBA(Color.HSVToRGB(0.486f, satMod, 1f)) + ">Periodogram SNR</color>\n";
            min = 0f;
            max = 26.61842977741538f;
        }
        else if (param == 2)
        {
            statText += "Parameter: <color=#" + ColorUtility.ToHtmlStringRGBA(Color.HSVToRGB(0.827f, satMod, 1f)) + ">Astrometric Psuedocolor</color>\n";
            min = 1.231199705120561f;
            max = 1.8991459147993115f;
        }
        else
        {
            statText += "Parameter: <color=#" + ColorUtility.ToHtmlStringRGBA(Color.HSVToRGB(0.166f, satMod, 1f)) + ">Variability Class</color>\n";
            min = 0f;
            max = 1f;
        }

        statText += "Value: " + Denormalize(paramVal, min, max).ToString() + "\n";

        //Load Data
        GameObject myNewTendril = Instantiate(tendrilPrefab, transform.position, Quaternion.identity);
        
        myNewTendril.GetComponent<Tendril>().inStartPosition = start;
        myNewTendril.GetComponent<Tendril>().inEndPosition = end;
        myNewTendril.GetComponent<Tendril>().statusText = statText;
        myNewTendril.GetComponent<Tendril>().starYOffset = inStarYOffset;
        myNewTendril.GetComponent<Tendril>().padStatus = inPadStat;
        myNewTendril.GetComponent<Tendril>().playerID = inPlayerID;
        myNewTendril.GetComponent<Tendril>().starID = inStarID.ToString();
        /*
        myNewTendril.GetComponent<NewTendrilController>().statusText = statText;
        myNewTendril.GetComponent<NewTendrilController>().starYOffset = inStarYOffset;
        myNewTendril.GetComponent<NewTendrilController>().padStatus = inPadStat;
        myNewTendril.GetComponent<NewTendrilController>().playerID = inPlayerID;
        myNewTendril.GetComponent<NewTendrilController>().Init(start, end);
        */

        //Spawn on Server
        NetworkServer.Spawn(myNewTendril, GetComponent<NetworkIdentity>().connectionToClient);
    }
    /// <summary>
    /// FD: Takes in a normalized value, a min, and a max, and returns the value which produced it within the range
    /// </summary>
    /// <param name="val">value to denormalize</param>
    /// <param name="min">minimum of the range of normalization</param>
    /// <param name="max">maximum of the range of normalizatio</param>
    /// <returns></returns>
    private float Denormalize(float val, float min, float max)
    {
        return (val * (max - min)) + min;
    }

    /// <summary>
    /// Generates a wwise linked event at the end of the frame.  Linked events include drones and sequences 
    /// </summary>
    /// <returns></returns>

  

    /// <summary>
    /// ZT: finds the voice manager and initiates a sequence or drone while passing star data for sonification
    /// </summary>
    private IEnumerator wwiseLinkedEvent(int seed)
    {

        yield return new WaitForEndOfFrame();

        GameObject linkedObject = gameObject;
        uint ustarID = (uint)starID;
        float normalizedMagnitude = dataColor[0];
        float normalizedPeriod = dataUV0[2];
        float normalizedSNR = dataUV0[3];
        float normalizedRMS = dataUV1[0];
        float varType = dataUV1[1];
        float normalizedColor = dataUV0[1];



        if (voiceManager == null)
        {
            voiceManager = FindObjectOfType<IOANVoiceManager>();
        }

        if (varType == 1)
        {
            if (sonifier == null)
            {
                sonifier = voiceManager.performSequence(ustarID, NetworkTime.time, normalizedRMS, normalizedPeriod, normalizedSNR, normalizedMagnitude, normalizedColor, linkedObject, seed);
            }
            if (sonifier == null)
            {
                Debug.LogError("wwiseLinkedEvent activated star could not find a sonifier");
            }
            duration = sonifier.duration;

        }
        else
        {
            if (sonifier == null)
            {
                sonifier = voiceManager.performDrone(ustarID, NetworkTime.time, normalizedMagnitude, normalizedRMS, normalizedPeriod, normalizedSNR, normalizedColor, linkedObject);
            }
            if (sonifier == null)
            {
                Debug.LogError("wwiseLinkedEvent activated star could not find a sonifier");
            }
            duration = sonifier.duration;
        }

        decayed_duration = duration;

        if (duration <= 0)
        {
            Debug.LogError("durration error! : duration is " + duration);
        }

        timeToDie = NetworkTime.time + decayed_duration;
        if (sonifier != null)
        {
            sonifier.timeToDie = (float)timeToDie;
        }


        Debug.Log("wwiseLinkedEvent New Link: \n starID: " + starID + "\nnormalizedMagnitude: " + normalizedMagnitude + "\n  normalizedPeriod: " + normalizedPeriod + "\nnormalizedSNR: " + normalizedSNR + "\nvarType: " + varType + "\n duration: " + duration);


        if (isClient)
        {
            if (uiSounds == null)
            {
                uiSounds = FindObjectOfType<uiSounds>();
            }

            uiSounds.spawnSound(linkedObject, playerID);
        }
        


    }
    /// <summary>
    /// Command that creates a linked event on the server
    /// </summary>
    [Command]
    void Cmd_wwiseLinkedEvent()
    {
        int seed = UnityEngine.Random.Range(5000, 50000);
        StartCoroutine(wwiseLinkedEvent(seed));
        Rpc_wwiseLinkedEvent(seed);
    }
    /// <summary>
    /// Creates a linked event on all of the clients 
    /// </summary>
    [ClientRpc]
    void Rpc_wwiseLinkedEvent(int seed)
    {
        StartCoroutine(wwiseLinkedEvent(seed));
    }
    /// <summary>
    /// Activated star tap to the server
    /// </summary>
    /// <param name="velocity"></param>
    /// <param name="angle"></param>
    [Command]
    void Cmd_activatedStarTap(float velocity, float angle)
    {
        Rpc_activatedStarTap(velocity, angle);
        StartCoroutine(activatedStarTap(velocity, angle));
    }
    /// <summary>
    /// Activated star tap on the clients 
    /// </summary>
    /// <param name="velocity"></param>
    /// <param name="angle"></param>
    [ClientRpc]
    void Rpc_activatedStarTap(float velocity, float angle)
    {
        StartCoroutine( activatedStarTap(velocity, angle) );
    }
    /// <summary>
    /// Runs an activated star tap at the end of the frame
    /// </summary>
    /// <param name="velocity"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    IEnumerator activatedStarTap(float velocity, float angle)
    {
        yield return new WaitForEndOfFrame();

        timeToDie += 2; //Add a second per tap

        timeToDie = Mathf.Clamp((float)timeToDie, 0, (float)NetworkTime.time + decayed_duration);
        if (sonifier != null)
        {
            sonifier.timeToDie = (float)timeToDie;
        }

        if (voiceManager == null)
        {
            voiceManager = FindObjectOfType<IOANVoiceManager>();
        }
        float normalizedMagnitude = dataColor[0];
        float normalizedSnr = dataUV0[3];
        float varType = dataUV1[1];
        float normalizedColor = dataUV0[1];
        if (varType == 1)
        {

            //voiceManager.performTap((uint)starID, normalizedMagnitude, velocity, (int)varType, angle, gameObject.transform.position, sonifier.sequenceInstrument);
            //This picks a random pitch from the sequence 
            
            if (sonifier != null)
            {
                IOANSonify tapVoice = voiceManager.findVoice((uint)starID);
                tapVoice.doTap(sonifier.pitchSequence[UnityEngine.Random.Range(0, sonifier.pitchSequence.Length - 1)], (int)voiceManager.dataMapper.tapIntensityToVelocity.map(velocity), 1, voiceManager.dataMapper.tapAngleToTimbre.map(angle), sonifier.sequenceInstrument);
            }
            }
        else if (varType == 2.0)
        {
            varType = 3 + dataMapper.magSnrToInstrument.mapNormalizedInt((normalizedMagnitude / normalizedSnr));
            voiceManager.performTap((uint)starID, normalizedMagnitude, normalizedSnr ,velocity, (int)varType, normalizedColor, angle, gameObject.transform.position);
        }
        else
        {
            voiceManager.performTap((uint)starID, normalizedMagnitude,normalizedSnr , velocity, (int)varType, normalizedColor, angle, gameObject.transform.position);
        }



    }
    /// <summary>
    /// Public trigger for an activated tap
    /// </summary>
    /// <param name="velocity"></param>
    /// <param name="angle"></param>
    public void doTap(float velocity, float angle)
    {
        if (isClient)
        {
            Cmd_activatedStarTap(velocity, angle);
        }
       

    }
    #endregion
}

/// <summary>
///     SD : Job for selecting tendril destinations in submesh vertices
/// </summary>
public struct FindTendrilDestJob : IJob
{
    public Vector3 myNewRaycastPos;
    public float maxStarDist;
    public int selIndex;
    public int padStatus;

    public NativeArray<Vector3> verts;
    public NativeArray<Vector4> uv0;
    public NativeArray<Vector4> uv1;
    public NativeArray<int> selectIndices;
    public NativeArray<float> indexValue;

    public void Execute()
    {
        Vector3 newCastPos = myNewRaycastPos;
        float maxDist = maxStarDist;

        List<int> vs = new List<int>();
        vs.AddRange(verts.Select((v, i) => (v, i)).Where((t) => {
            float dist = Mathf.Sqrt(Mathf.Pow(t.v.x - newCastPos.x, 2) + Mathf.Pow(t.v.z - newCastPos.z, 2));
            return dist < maxDist;
        }).Select((t) => t.i).ToList());

        // THIS SELECTS THE PARAM (for every vertice index, get the rgba associated with the padStatus)
        List<Vector2> dataVals = new List<Vector2>();

        float paramVal;
        switch (padStatus)
        {
            case 0:
                paramVal = uv1[selIndex].x;
                break;
            case 1:
                paramVal = uv0[selIndex].w;
                break;
            case 2:
                paramVal = uv0[selIndex].y;
                break;
            case 3:
                paramVal = uv1[selIndex].y;
                break;
            default:
                paramVal = 0f;
                break;
        }

        for (int i = 0; i < vs.Count(); i++)
        {
            float vertexVal;
            switch (padStatus)
            {
                case 0:
                    vertexVal = uv1[vs[i]].x;
                    break;
                case 1:
                    vertexVal = uv0[vs[i]].w;
                    break;
                case 2:
                    vertexVal = uv0[vs[i]].y;
                    break;
                case 3:
                    vertexVal = uv1[vs[i]].y;
                    break;
                default:
                    vertexVal = 0f;
                    break;
            }

            dataVals.Add(new Vector2(vertexVal, vs[i]));
        }

        dataVals = dataVals.OrderBy(x => Mathf.Abs(paramVal - x.x)).ToList();

        for (int i = 0; i < selectIndices.Length; i++)
        {
            selectIndices[i] = (int)dataVals[i].y;
            indexValue[i] = dataVals[i].x;
        }
    }
}