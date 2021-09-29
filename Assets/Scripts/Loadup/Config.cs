using System;
using System.Collections;
using System.IO;
using System.IO.MemoryMappedFiles;
using Mirror;
using UnityEngine;
using UnityEngine.PostProcessing;

/// <summary>
///     Load and store data from config file, acts as database
/// </summary>
public class Config : MonoBehaviour
{
    #region PUBLIC_VAR
    [HelpBox("Config supports one key value pair per line.\n" +
             "Specified as:\n " +
             "    key=value\n" +
             "    // (slash-slash) begins comments, \n" +
             "Everything on a line after // is ignored\n", HelpBoxMessageType.Info)]
    [Tooltip("Location of config file.")]
    public string path = "config/cfg.json";

    /// <summary>
    ///     Config data instance
    /// </summary>
    public ConfigData Data;

    /// <summary>
    ///     Public Scales values
    /// </summary>
    public Vector3 meshScales = new Vector3(400.0f, 1.5f, 200.0f);

    public PostProcessingProfile ppProfile;
    #endregion

    #region PRIVATE_VAR
    /// <summary>
    ///     Instance of the limits class
    /// </summary>
    private SFLimits limits;

    /// <summary>
    ///     Pointer to the network hud
    /// </summary>
    private NetworkManagerHUD networkHUD;

    /// <summary>
    ///     GUI class on the same game object
    /// </summary>
    private ConfigGui gui;

    /// <summary>
    ///     Instance of the Initializer class in the world
    /// </summary>
    private Initializer init;
    #endregion

    #region PUBLIC_SINGLETON
    /// <summary>
    ///     Singleton for the config instance
    /// </summary>
    private static Config _config = null;
    public static Config Instance
    {
        get { return _config; }
        private set { if (_config == null) _config = value; }
    }
    #endregion

    #region UNITY_FUNC
    /// <summary>
    ///     Load in config data from Json, init other components as well
    /// </summary>
    private void Start()
    {
        Debug.Log("CONFIG: Initializing...");
        Instance = this;

        //Debug.Log("CONFIG: Parsing Data...");
        StreamReader reader = new StreamReader(path);
        string json = reader.ReadToEnd();
        reader.Close();

        //Debug.Log("CONFIG: Loading Data...");
        Data = JsonUtility.FromJson<ConfigData>(json);
        limits = new SFLimits();
        limits.SetScaleValues(meshScales);

        //Set serverStatus to all lowercase
        Data.serverStatus = Data.serverStatus.ToLower();

        Debug.Log("CONFIG: Data loaded!");

        networkHUD = GameObject.Find("NetworkManager").GetComponent<NetworkManagerHUD>();
        networkHUD.enabled = false;
        gui = GetComponent<ConfigGui>();
        init = GameObject.Find("Initializer").GetComponent<Initializer>();
        StartCoroutine(CheckMeshLoading());

        if (Data.serverStatus == "server" || SystemInfo.graphicsMemorySize < 6000)
        {
            ppProfile.antialiasing.enabled = false;
            ppProfile.bloom.enabled = false;
        }
        else
        {
            ppProfile.antialiasing.enabled = true;
            ppProfile.bloom.enabled = true;
        }

        FindObjectOfType<ioanDataMapper>().UpdateAudioRangeValues(Data);
    }

    /// <summary>
    ///     Get user input to toggle GUI
    /// </summary>
    private void Update()
    {
        if (!Data.devFlag) return;

        if (Input.GetKeyDown(KeyCode.F1)) gui.ToggleGUI(ConfigGui.GUISection.IPVAL); //toggle IP value gui page
        if (Input.GetKeyDown(KeyCode.F2)) gui.ToggleGUI(ConfigGui.GUISection.MESH); //toggle mesh ranges gui page
        if (Input.GetKeyDown(KeyCode.F3)) gui.ToggleGUI(ConfigGui.GUISection.FPS); //toggle custom fps counter
        if (Input.GetKeyDown(KeyCode.F4) && init.doneLoading) networkHUD.enabled = !networkHUD.enabled; //disable-enable network hud gui
        if (Input.GetKeyDown(KeyCode.F5)) Screen.fullScreen = !Screen.fullScreen; //toggle fullscreen
        if (Input.GetKeyDown(KeyCode.F6)) gui.ToggleGUI(ConfigGui.GUISection.VOLUMESLIDERES); //toggle volume sliders
        if (Input.GetKeyDown(KeyCode.F7)) gui.ToggleGUI(ConfigGui.GUISection.AUDIORANGES); //toggle audio ranges gui page
        if (Input.GetKeyDown(KeyCode.F8)) gui.CycleAudioRangePage();
    }
    #endregion

    #region PUBLIC_FUNC
    /// <summary>
    ///     Write data to json
    /// </summary>
    public void WriteToJson()
    {
        //Debug.Log("CONFIG: Converting Data to JSON...");
        string json = JsonUtility.ToJson(Data, true);

        //Debug.Log("CONFIG: Outputting Data...");
        StreamWriter writer = new StreamWriter(path);
        writer.WriteLine(json);
        writer.Close();

        Debug.Log("CONFIG: Data Saved!");
    }

    /// <summary>
    ///     progress the loading bar or disable it
    /// </summary>
    /// <param name="enabled"></param>
    public void TickLoadingProgress(bool enabled = true)
    {
        if (enabled)
        {
            gui.TickLoading(1f / ((Data.meshRangeX.y - Data.meshRangeX.x + 1.0f) * (Data.meshRangeZ.y - Data.meshRangeZ.x + 1.0f)));
            gui.SetGUI(ConfigGui.GUISection.MESHLOADING);
        }
        else
        {
            gui.TickLoading(-1.0f);
            gui.UnsetGUI(ConfigGui.GUISection.MESHLOADING);
        }
    }

    /// <summary>
    ///     Return Limits
    /// </summary>
    /// <returns></returns>
    public SFLimitsStruct GetLimits()
    {
        return limits.Limits;
    }

    /// <summary>
    ///     Return Scales
    /// </summary>
    /// <returns></returns>
    public SFScales GetScales()
    {
        return limits.Scales;
    }

    /// <summary>
    ///     Load data into limits from file
    /// </summary>
    /// <param name="filename">location of "limits.b"</param>
    public void ReadHeaderData(string filename)
    {
        limits.ReadIndexData(filename);
    }
    #endregion

    #region PRIVATE_FUNC
    /// <summary>
    ///     Coroutine for checking if the meshes are loaded in
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckMeshLoading()
    {
        yield return new WaitUntil(() => init.doneLoading);
        networkHUD.enabled = true;
    }
    #endregion
}

#region CONFIG_DATA
/// <summary>
///     Serialized class for storing config data fields
/// </summary>
[Serializable]
public class ConfigData
{
    [Header("Camera Info")]
    public Vector3 cam0Position = new Vector3(0f, .5f, 1f);
    public Quaternion cam0Rotation = new Quaternion();
    [Space]
    public Vector3 cam1Position = new Vector3(1f, .5f, 0f);
    public Quaternion cam1Rotation = new Quaternion();
    [Space]
    public Vector3 cam2Position = new Vector3(-1f, .5f, 0f);
    public Quaternion cam2Rotation = new Quaternion();
    [Space]
    public Vector3 wwPosition = new Vector3(0f, .75f, 0f);
    public Vector3 wwRotation = new Vector3(0f, 0f, 0f);
    [Space]
    public Vector3 camCenterPoint = new Vector3(0f, 0f, 0f);

    [Header("Display Info")]
    public int cam0TargetDisplay = 2;
    public int cam1TargetDisplay = 3;
    public int cam2TargetDisplay = 4;

    [Header("Audio Info")]
    public bool remixReplay = false;
    public Vector3 serverAudioPosition = new Vector3(0, 0, 0);
    public Vector3 serverAudioEulerRotation = new Vector3(0, 0, 0);
    public string scale = "default";
    public float[] userScale = { 0, 2, 4, 5,7, 9, 11 };
    public float pitchFieldSize = -1;
    public int audioChannels = 5;
    public float volume = 75.0f;

    [Header("Audio Volume Levels")]
    public float masterTapVolume = 80.0f;
    public float masterSequenceVolume = 80.0f;
    public float masterDroneVolume = 80.0f;
    public float masterWaveVolume = 80.0f;
    public float masterInterfaceVolume = 80.0f;
    public float masterVolume = 80.0f;

    [Header("Audio Data Ranges")]
    public Vector3 tapMagToPitchRange = new Vector3(40, 90, 1);
    public Vector3 tapIntToVelocityRange = new Vector3(20, 120, 1);
    public Vector3 tapAngleToTimbreRange = new Vector3(0, 1, 1);
    public Vector3 tapColorToInstrumentRange = new Vector3(0, 7, 1);

    public Vector3 droneMagToPitchRange = new Vector3(40, 90, 1);
    public Vector3 droneRMSToVelocityRange = new Vector3(60, 110, 1);
    public Vector3 dronePeriodToMaxDurRange = new Vector3(10, 40, 1);
    public Vector3 droneSNRToModMixRange = new Vector3(0.1f, 1, 1);
    public Vector3 droneMagRMSToDroneVibRange = new Vector3(0, 100, 1);
    public Vector3 droneMagPeriodToDroneSpeedRange = new Vector3(0, 100, 1);
    public Vector3 droneMagSNRToDroneHarmRange = new Vector3(0, 100, 1);
    public Vector3 droneSNRToDroneFBRange = new Vector3(0, 100, 1);
    public Vector3 droneMagSNRToInstrumentRange = new Vector3(0, 2, 1);
    public Vector3 droneColorToInstrumentRange = new Vector3(0, 7, 1);
    public Vector3 droneRMSToInstrumentRange = new Vector3(0, 1, 1);

    public Vector3 seqLightcurveMagToPitchRange = new Vector3(50, 90, 1);
    public Vector3 seqRMSToTempoRange = new Vector3(70, 190, 1);
    public Vector3 seqPeriodToDurationRange = new Vector3(30, 60, 1);
    public Vector3 seqSNRToRhythmicVarRange = new Vector3(0, 0.75f, 1);
    public Vector3 seqMagSNRToInstrumentRange = new Vector3(0, 7, 1);

    public Vector3 seqmapMagRangetoPitchRange = new Vector3(5, 12, 1);
    public Vector3 seqmapMagMeantoPitchCenterRange = new Vector3(60, 94, 1);

    public Vector3 waveSNRMapToPitchRange = new Vector3(40, 80, 1);
    public Vector3 waveRMSMapToPitchRange = new Vector3(42, 90, 1);
    public Vector3 waveTeffMapToPitchRange = new Vector3(60, 80, 1);
    public Vector3 waveVarMapToPitchRange = new Vector3(40, 80, 1);

    [Header("Player Info")]
    public int myID = -1;
    public int testPort = 8787;
    public bool serverEnableVR = false;

    [Header ("Network Info")]
    public string serverStatus = "client";
    public string audioIP = "localhost";
    public string serverIP = "169.254.169.47";
    public string textIP = "169.254.22.151";
    public string dataBaseIP = "localHost";
    public string xampp_dir = "bad_dir";
    public bool initial_start = true;

    [Header ("Mesh Info")]
    public Vector2 meshRangeX = new Vector2(1, 11);
    public Vector2 meshRangeZ = new Vector2(1, 5);
    public Vector2 meshHQRangeX = new Vector2(6, 6);
    public Vector2 meshHQRangeZ = new Vector2(3, 3);
    public Vector2 meshMQRangeX = new Vector2(5, 7);
    public Vector2 meshMQRangeZ = new Vector2(2, 4);

    [Header ("Misc")]
    public bool devFlag = false;
    public bool controllerTextFlag = false;
}
#endregion

#region MESH_DATA
/// <summary>
///     Class for controlling limits data
/// </summary>
public class SFLimits
{
    /// <summary>
    ///     Instance of scales used
    /// </summary>
    public SFScales Scales { get; private set; }

    /// <summary>
    ///     Instance of limits used
    /// </summary>
    public SFLimitsStruct Limits { get; private set; }


    /// <summary>
    ///     Constructor, Create new instances
    /// </summary>
    public SFLimits()
    {
        Scales = new SFScales()
        {
            xScale = 400.0f,
            xOffset = 0.5f,
            yScale = 0.3f,
            yOffset = 0.5f,
            zScale = 200.0f,
            zOffset = 0.5f
        };

        Limits = new SFLimitsStruct();
    }

    /// <summary>
    ///     Read in limits data from filename
    /// </summary>
    /// <param name="filename"></param>
    public void ReadIndexData(string filename)
    {
        using (var mmf = MemoryMappedFile.CreateFromFile(filename, FileMode.Open))
        {
            using (var file = mmf.CreateViewAccessor(0x0, 0x0))
            {
                Limits = new SFLimitsStruct()
                {
                    ra = new Vector2((float)file.ReadDouble(0x0), (float)file.ReadDouble(0x8)),
                    dec = new Vector2((float)file.ReadDouble(0x10), (float)file.ReadDouble(0x18)),
                    mag = new Vector2((float)file.ReadDouble(0x20), (float)file.ReadDouble(0x28)),
                    std = new Vector2((float)file.ReadDouble(0x30), (float)file.ReadDouble(0x38)),
                    obs = new Vector3((float)file.ReadDouble(0x40), (float)file.ReadDouble(0x48), (float)file.ReadDouble(0x50)),
                    var_flag = new Vector2((float)file.ReadDouble(0x58), (float)file.ReadDouble(0x60)),
                    catalog = new Vector2((float)file.ReadDouble(0x68), (float)file.ReadDouble(0x70)),
                    color = new Vector2((float)file.ReadDouble(0x78), (float)file.ReadDouble(0x80)),
                    period = new Vector2((float)file.ReadDouble(0x88), (float)file.ReadDouble(0x90)),
                    snr = new Vector2((float)file.ReadDouble(0x98), (float)file.ReadDouble(0xA0)),
                    rms = new Vector2((float)file.ReadDouble(0xA8), (float)file.ReadDouble(0xB0)),
                    pmra = new Vector3((float)file.ReadDouble(0xB8), (float)file.ReadDouble(0xC0), (float)file.ReadDouble(0xC8)),
                    parallax = new Vector2((float)file.ReadDouble(0xD0), (float)file.ReadDouble(0xD8))
                };
            }
        }
    }

    /// <summary>
    ///     Set scale values from input
    /// </summary>
    /// <param name="scale"></param>
    public void SetScaleValues(Vector3 scale)
    {
        Scales = new SFScales()
        {
            xScale = scale.x,
            xOffset = 0.5f,
            yScale = scale.y,
            yOffset = 0.5f,
            zScale = scale.z,
            zOffset = 0.5f
        };
    }

    /// <summary>
    ///     Print limits (FOR DEBUGGING)
    /// </summary>
    public void PrintLimits()
    {
        Debug.Log("LIMITS");
        Debug.Log(Limits.ra);
        Debug.Log(Limits.dec);
        Debug.Log(Limits.mag);
        Debug.Log(Limits.std);
        Debug.Log(Limits.obs);
        Debug.Log(Limits.var_flag);
        Debug.Log(Limits.catalog);
        Debug.Log(Limits.color);
        Debug.Log(Limits.period);
        Debug.Log(Limits.snr);
        Debug.Log(Limits.rms);
        Debug.Log(Limits.pmra);
        Debug.Log(Limits.parallax);
        Debug.Log("END LIMITS");
    }
}

/// <summary>
///     Struct for moving limit data
/// </summary>
public struct SFLimitsStruct
{
    public Vector2 ra;
    public Vector2 dec;
    public Vector2 mag;
    public Vector2 std;
    public Vector3 obs;
    public Vector2 var_flag;
    public Vector2 catalog;
    public Vector2 color;
    public Vector2 period;
    public Vector2 snr;
    public Vector2 rms;
    public Vector2 type;
    public Vector3 pmra;
    public Vector2 parallax;
}

/// <summary>
///     Struct for moving scale data
/// </summary>
public struct SFScales
{
    public float xScale;
    public float xOffset;
    public float yScale;
    public float yOffset;
    public float zScale;
    public float zOffset;

    // Vector2 : x - min, y - max
    // Vector3 : x - min, y - max, z - mean
}
#endregion
