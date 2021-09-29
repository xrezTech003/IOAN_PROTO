using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
///     Class for controlling all GUI sections
/// </summary>
public class ConfigGui : MonoBehaviour
{
    #region PUBLIC_VAR
    /// <summary>
    ///     Enumeration for which gui object is enabled
    /// </summary>
    public enum GUISection { IPVAL = 0x1, MESH = 0x2, FPS = 0x4, MESHLOADING = 0x8, AUDIORANGES = 0x10, VOLUMESLIDERES = 0x20 }

    /// <summary>
    ///     Font used in the loading bar
    /// </summary>
    public Font loadFont;
    #endregion

    #region PRIVATE_VAR
    /// <summary>
    ///     bits for which fields are modyfied
    /// </summary>
    private int modBits = 0x0;

    /// <summary>
    ///     new field values for their respective fields
    /// </summary>
    private string newID, newAudio, newServer, newText, newDatabase, newMeshRangeX, newMeshRangeZ, newMQMeshRangeX, newMQMeshRangeZ, newHQMeshRangeX, newHQMeshRangeZ;

    /// <summary>
    ///     timer for when to recalculate the fps
    /// </summary>
    private float fpsTimer = 0f;

    /// <summary>
    ///     current fps
    /// </summary>
    private float fps = 0f;

    /// <summary>
    ///     pointer to the Config instance
    /// </summary>
    private Config config = null;

    /// <summary>
    ///     pointer to the Initializer instance
    /// </summary>
    private Initializer init = null;

    /// <summary>
    ///     GUI flags for each section of the gui
    /// </summary>
    private int guiFlags = 0x8;

    /// <summary>
    ///     start x pos for gui drawing
    /// </summary>
    private readonly float x = 10;

    /// <summary>
    ///     start y pos for gui drawing
    /// </summary>
    private readonly float y = 130;

    /// <summary>
    ///     string output for the loading bar
    /// </summary>
    private string progressStr;

    /// <summary>
    ///     loading bar percentage
    /// </summary>
    private float progressAmt;

    private int audioPageIndex = 0;

    private float audioX = 0f;
    private float audioY = 0f;
    private int audioModBits = 0x0;

    private string[] newTapRanges = new string[4];
    private string[] newDroneRanges = new string[11];
    private string[] newSequenceRanges = new string[7];
    private string[] newWaveRanges = new string[4];
    #endregion

    #region UNITY_FUNC
    /// <summary>
    ///     Find the initializer object
    /// </summary>
    private void Start()
    {
        newID = newAudio = newServer = newText = newDatabase = 
            newMeshRangeX = newMeshRangeZ = newMQMeshRangeX = newMQMeshRangeZ = newHQMeshRangeX = newHQMeshRangeZ = "";

        for (int i = 0; i < 4; i++) newTapRanges[i] = "";
        for (int i = 0; i < 11; i++) newDroneRanges[i] = "";
        for (int i = 0; i < 7; i++) newSequenceRanges[i] = "";
        for (int i = 0; i < 4; i++) newWaveRanges[i] = "";

        init = GameObject.FindGameObjectWithTag("init").GetComponent<Initializer>();
    }

    /// <summary>
    ///     Late start find Config instance, calculate fps
    /// </summary>
    private void Update()
    {
        if (config == null)
            config = Config.Instance;

        if (Time.time - fpsTimer > 1f)
        {
            fpsTimer = Time.time;
            fps = 1f / Time.deltaTime;
        }

        audioX = Screen.width - 10;
        //audioY = Screen.height - 10;
    }

    /// <summary>
    ///     Control GUI interactions
    /// </summary>
    private void OnGUI()
    {
        //Draw loading bar
        if ((guiFlags & (int)GUISection.MESHLOADING) != 0x0)
        {
            Vector2 center = new Vector2(Screen.width / 2.0f, Screen.height / 2.0f);
            Vector2 dim = new Vector2(100, 25);
            GUIStyle style = new GUIStyle()
            {
                normal = new GUIStyleState()
                {
                    textColor = Color.white,
                },
                font = loadFont,
                fontStyle = FontStyle.Normal,
                fontSize = 50,
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };

            GUI.Label(new Rect(center.x - dim.x / 2f, center.y - dim.y / 2f, dim.x, dim.y), progressStr, style);
        }

        if ((guiFlags & (int)GUISection.IPVAL) != 0x0)
        {
            //Player ID
            GUI.Label(new Rect(x, y, 75, 25), "PlayerID");
            newID = GUI.TextField(new Rect(x + 80, y, 150, 25), ((modBits & 0x1) != 0x0) ? newID : config.Data.myID.ToString());
            if (newID != "") modBits |= 0x1;

            //IP Addresses
            GUI.Label(new Rect(x, y + 30, 75, 25), "Audio IP");
            newAudio = GUI.TextField(new Rect(x + 80, y + 30, 150, 25), ((modBits & 0x2) != 0x0) ? newAudio : config.Data.audioIP);
            if (newAudio != "") modBits |= 0x2;

            GUI.Label(new Rect(x, y + 60, 75, 25), "Server IP");
            newServer = GUI.TextField(new Rect(x + 80, y + 60, 150, 25), ((modBits & 0x4) != 0x0) ? newServer : config.Data.serverIP);
            if (newServer != "") modBits |= 0x4;

            GUI.Label(new Rect(x, y + 90, 75, 25), "Text IP");
            newText = GUI.TextField(new Rect(x + 80, y + 90, 150, 25), ((modBits & 0x8) != 0x0) ? newText : config.Data.textIP);
            if (newText != "") modBits |= 0x8;

            GUI.Label(new Rect(x, y + 120, 75, 25), "Database IP");
            newDatabase = GUI.TextField(new Rect(x + 80, y + 120, 150, 25), ((modBits & 0x10) != 0x0) ? newDatabase : config.Data.dataBaseIP);
            if (newDatabase != "") modBits |= 0x10;

            //XAMPP Revert
            if (GUI.Button(new Rect(x, y + 150, 230, 25), "Reset XAMPP"))
            {
                string pwd = Directory.GetCurrentDirectory();
                File.Copy(@pwd + @"\config\Reset_Files\httpd.conf", @pwd + @"\xampp\apache\conf\httpd.conf", true);
                File.Copy(@pwd + @"\config\Reset_Files\httpd-ssl.conf", @pwd + @"\xampp\apache\conf\extra\httpd-ssl.conf", true);
                File.Copy(@pwd + @"\config\Reset_Files\httpd-xampp.conf", @pwd + @"\xampp\apache\conf\extra\httpd-xampp.conf", true);
                File.Copy(@pwd + @"\config\Reset_Files\my.ini", @pwd + @"\xampp\mysql\bin\my.ini", true);
                File.Copy(@pwd + @"\config\Reset_Files\php.ini", @pwd + @"\xampp\php\php.ini", true);

                config.Data.xampp_dir = "dummy_dir";
                config.Data.initial_start = true;
                config.WriteToJson();
            }
        }

        //Mesh Ranges
        if ((guiFlags & (int)GUISection.MESH) != 0x0)
        {
            Func<Vector2, string> RangeToString = new Func<Vector2, string>((v) => "<" + v.x + ", " + v.y + ">");

            GUI.Label(new Rect(x, y + 210, 120, 25), "Mesh Range X");
            newMeshRangeX = GUI.TextField(new Rect(x + 125, y + 210, 105, 25), ((modBits & 0x20) != 0x0) ? newMeshRangeX : RangeToString(config.Data.meshRangeX));
            if (newMeshRangeX != "") modBits |= 0x20;

            GUI.Label(new Rect(x, y + 240, 120, 25), "Mesh Range Z");
            newMeshRangeZ = GUI.TextField(new Rect(x + 125, y + 240, 105, 25), ((modBits & 0x40) != 0x0) ? newMeshRangeZ : RangeToString(config.Data.meshRangeZ));
            if (newMeshRangeZ != "") modBits |= 0x40;

            GUI.Label(new Rect(x, y + 270, 120, 25), "MQ Mesh Range X");
            newMQMeshRangeX = GUI.TextField(new Rect(x + 125, y + 270, 105, 25), ((modBits & 0x80) != 0x0) ? newMQMeshRangeX : RangeToString(config.Data.meshMQRangeX));
            if (newMQMeshRangeX != "") modBits |= 0x80;

            GUI.Label(new Rect(x, y + 300, 120, 25), "MQ Mesh Range Z");
            newMQMeshRangeZ = GUI.TextField(new Rect(x + 125, y + 300, 105, 25), ((modBits & 0x100) != 0x0) ? newMQMeshRangeZ : RangeToString(config.Data.meshMQRangeZ));
            if (newMQMeshRangeZ != "") modBits |= 0x100;

            GUI.Label(new Rect(x, y + 330, 120, 25), "HQ Mesh Range X");
            newHQMeshRangeX = GUI.TextField(new Rect(x + 125, y + 330, 105, 25), ((modBits & 0x200) != 0x0) ? newHQMeshRangeX : RangeToString(config.Data.meshHQRangeX));
            if (newHQMeshRangeX != "") modBits |= 0x80;

            GUI.Label(new Rect(x, y + 360, 120, 25), "HQ Mesh Range Z");
            newHQMeshRangeZ = GUI.TextField(new Rect(x + 125, y + 360, 105, 25), ((modBits & 0x400) != 0x0) ? newHQMeshRangeZ : RangeToString(config.Data.meshHQRangeZ));
            if (newHQMeshRangeZ != "") modBits |= 0x100;

            if (init.doneLoading)
            {
                if (GUI.Button(new Rect(x, y + 390, 230, 25), "Reload Star Field"))
                {
                    init.ReloadMeshes();
                }
            }
        }

        //Volume Sliders
        if ((guiFlags & (int)GUISection.VOLUMESLIDERES) != 0x0)
        {
            GUIStyle sliderStyle = GUI.skin.horizontalSlider;
            GUIStyle thumbStyle = GUI.skin.horizontalSliderThumb;

            GUI.Label(new Rect(x + 250, y - 100, 120, 25), "Tap Volume");
            config.Data.masterTapVolume = (int)GUI.HorizontalSlider(new Rect(x + 370, y - 100, 200, 25), config.Data.masterTapVolume, 0, 100, sliderStyle, thumbStyle);
            GUI.Label(new Rect(x + 580, y - 100, 100, 25), config.Data.masterTapVolume.ToString());

            GUI.Label(new Rect(x + 250, y - 70, 120, 25), "Sequence Volume");
            config.Data.masterSequenceVolume = (int)GUI.HorizontalSlider(new Rect(x + 370, y - 70, 200, 25), config.Data.masterSequenceVolume, 0, 100, sliderStyle, thumbStyle);
            GUI.Label(new Rect(x + 580, y - 70, 100, 25), config.Data.masterSequenceVolume.ToString());

            GUI.Label(new Rect(x + 250, y - 40, 120, 25), "Drone Volume");
            config.Data.masterDroneVolume = (int)GUI.HorizontalSlider(new Rect(x + 370, y - 40, 200, 25), config.Data.masterDroneVolume, 0, 100, sliderStyle, thumbStyle);
            GUI.Label(new Rect(x + 580, y - 40, 100, 25), config.Data.masterDroneVolume.ToString());

            GUI.Label(new Rect(x + 250, y - 10, 120, 25), "Wave Volume");
            config.Data.masterWaveVolume = (int)GUI.HorizontalSlider(new Rect(x + 370, y - 10, 200, 25), config.Data.masterWaveVolume, 0, 100, sliderStyle, thumbStyle);
            GUI.Label(new Rect(x + 580, y - 10, 100, 25), config.Data.masterWaveVolume.ToString());

            GUI.Label(new Rect(x + 250, y + 20, 120, 25), "Interface Volume");
            config.Data.masterInterfaceVolume = (int)GUI.HorizontalSlider(new Rect(x + 370, y + 20, 200, 25), config.Data.masterInterfaceVolume, 0, 100, sliderStyle, thumbStyle);
            GUI.Label(new Rect(x + 580, y + 20, 100, 25), config.Data.masterInterfaceVolume.ToString());

            GUI.Label(new Rect(x + 250, y + 50, 120, 25), "Master Volume");
            config.Data.masterVolume = (int)GUI.HorizontalSlider(new Rect(x + 370, y + 50, 200, 25), config.Data.masterVolume, 0, 100, sliderStyle, thumbStyle);
            GUI.Label(new Rect(x + 580, y + 50, 100, 25), config.Data.masterVolume.ToString());
        }

        //Audio Ranges
        if ((guiFlags & (int)GUISection.AUDIORANGES) != 0x0)
        {
            Func<Vector3, string> RangeToString = new Func<Vector3, string>((v) => "<" + v.x + ", " + v.y + "> (" + v.z + ")");

            switch (audioPageIndex)
            {
                case 0: //Tap Ranges
                    GUI.Label(new Rect(audioX - 420, audioY + 15, 180, 25), "Tap Ranges");

                    GUI.Label(new Rect(audioX - 420, audioY + 45, 270, 25), "Magnitude to Pitch Range");
                    newTapRanges[0] = GUI.TextField(new Rect(audioX - 150, audioY +  45, 150, 25), ((audioModBits & 0x1) != 0x0) ? newTapRanges[0] : RangeToString(config.Data.tapMagToPitchRange));
                    if (newTapRanges[0] != "") audioModBits |= 0x1;

                    GUI.Label(new Rect(audioX - 420, audioY + 75, 270, 25), "Intesity to Velocity Range");
                    newTapRanges[1] = GUI.TextField(new Rect(audioX - 150, audioY + 75, 150, 25), ((audioModBits & 0x2) != 0x0) ? newTapRanges[1] : RangeToString(config.Data.tapIntToVelocityRange));
                    if (newTapRanges[1] != "") audioModBits |= 0x2;

                    GUI.Label(new Rect(audioX - 420, audioY + 105, 270, 25), "Angle to Timbre Range");
                    newTapRanges[2] = GUI.TextField(new Rect(audioX - 150, audioY + 105, 150, 25), ((audioModBits & 0x4) != 0x0) ? newTapRanges[2] : RangeToString(config.Data.tapAngleToTimbreRange));
                    if (newTapRanges[2] != "") audioModBits |= 0x4;

                    GUI.Label(new Rect(audioX - 420, audioY + 135, 270, 25), "Color to Instrument Range");
                    newTapRanges[3] = GUI.TextField(new Rect(audioX - 150, audioY + 135, 150, 25), ((audioModBits & 0x8) != 0x0) ? newTapRanges[3] : RangeToString(config.Data.tapColorToInstrumentRange));
                    if (newTapRanges[3] != "") audioModBits |= 0x8;

                    break;
                case 1: //Drone Ranges
                    GUI.Label(new Rect(audioX - 420, audioY + 15, 270, 25), "Drone Ranges");

                    GUI.Label(new Rect(audioX - 420, audioY + 45, 270, 25), "Magnitude to Pitch Range");
                    newDroneRanges[0] = GUI.TextField(new Rect(audioX - 150, audioY + 45, 150, 25), ((audioModBits & 0x10) != 0x0) ? newDroneRanges[0] : RangeToString(config.Data.droneMagToPitchRange));
                    if (newDroneRanges[0] != "") audioModBits |= 0x10;

                    GUI.Label(new Rect(audioX - 420, audioY + 75, 270, 25), "RMS to Velocity Range");
                    newDroneRanges[1] = GUI.TextField(new Rect(audioX - 150, audioY + 75, 150, 25), ((audioModBits & 0x20) != 0x0) ? newDroneRanges[1] : RangeToString(config.Data.droneRMSToVelocityRange));
                    if (newDroneRanges[1] != "") audioModBits |= 0x20;

                    GUI.Label(new Rect(audioX - 420, audioY + 105, 270, 25), "Period to Max Duration Range");
                    newDroneRanges[2] = GUI.TextField(new Rect(audioX - 150, audioY + 105, 150, 25), ((audioModBits & 0x40) != 0x0) ? newDroneRanges[2] : RangeToString(config.Data.dronePeriodToMaxDurRange));
                    if (newDroneRanges[2] != "") audioModBits |= 0x40;

                    GUI.Label(new Rect(audioX - 420, audioY + 135, 270, 25), "SNR to Modular Mix Range");
                    newDroneRanges[3] = GUI.TextField(new Rect(audioX - 150, audioY + 135, 150, 25), ((audioModBits & 0x80) != 0x0) ? newDroneRanges[3] : RangeToString(config.Data.droneSNRToModMixRange));
                    if (newDroneRanges[3] != "") audioModBits |= 0x80;

                    GUI.Label(new Rect(audioX - 420, audioY + 165, 270, 25), "Magnitude RMS to Drone Vibration Range");
                    newDroneRanges[4] = GUI.TextField(new Rect(audioX - 150, audioY + 165, 150, 25), ((audioModBits & 0x100) != 0x0) ? newDroneRanges[4] : RangeToString(config.Data.droneMagRMSToDroneVibRange));
                    if (newDroneRanges[4] != "") audioModBits |= 0x100;

                    GUI.Label(new Rect(audioX - 420, audioY + 195, 270, 25), "Magnitude Period to Drone Speed Range");
                    newDroneRanges[5] = GUI.TextField(new Rect(audioX - 150, audioY + 195, 150, 25), ((audioModBits & 0x200) != 0x0) ? newDroneRanges[5] : RangeToString(config.Data.droneMagPeriodToDroneSpeedRange));
                    if (newDroneRanges[5] != "") audioModBits |= 0x200;

                    GUI.Label(new Rect(audioX - 420, audioY + 225, 270, 25), "Magnitude SNR to Drone Harm Range");
                    newDroneRanges[6] = GUI.TextField(new Rect(audioX - 150, audioY + 225, 150, 25), ((audioModBits & 0x400) != 0x0) ? newDroneRanges[6] : RangeToString(config.Data.droneMagSNRToDroneHarmRange));
                    if (newDroneRanges[6] != "") audioModBits |= 0x400;

                    GUI.Label(new Rect(audioX - 420, audioY + 255, 270, 25), "SNR to Drone FB Range");
                    newDroneRanges[7] = GUI.TextField(new Rect(audioX - 150, audioY + 255, 150, 25), ((audioModBits & 0x800) != 0x0) ? newDroneRanges[7] : RangeToString(config.Data.droneSNRToDroneFBRange));
                    if (newDroneRanges[7] != "") audioModBits |= 0x800;

                    GUI.Label(new Rect(audioX - 420, audioY + 285, 270, 25), "Magnitude SNR to Instrument Range");
                    newDroneRanges[8] = GUI.TextField(new Rect(audioX - 150, audioY + 285, 150, 25), ((audioModBits & 0x1000) != 0x0) ? newDroneRanges[8] : RangeToString(config.Data.droneMagSNRToInstrumentRange));
                    if (newDroneRanges[8] != "") audioModBits |= 0x1000;

                    GUI.Label(new Rect(audioX - 420, audioY + 315, 270, 25), "Color to Instrument Range");
                    newDroneRanges[9] = GUI.TextField(new Rect(audioX - 150, audioY + 315, 150, 25), ((audioModBits & 0x2000) != 0x0) ? newDroneRanges[9] : RangeToString(config.Data.droneColorToInstrumentRange));
                    if (newDroneRanges[9] != "") audioModBits |= 0x2000;

                    GUI.Label(new Rect(audioX - 420, audioY + 345, 270, 25), "RMS to Instrument Range");
                    newDroneRanges[10] = GUI.TextField(new Rect(audioX - 150, audioY + 345, 150, 25), ((audioModBits & 0x4000) != 0x0) ? newDroneRanges[10] : RangeToString(config.Data.droneRMSToInstrumentRange));
                    if (newDroneRanges[10] != "") audioModBits |= 0x4000;

                    break;
                case 2: //Sequence Ranges
                    GUI.Label(new Rect(audioX - 420, audioY + 15, 270, 25), "Sequence Ranges");

                    GUI.Label(new Rect(audioX - 420, audioY + 45, 270, 25), "Lightcurve Magnitude to Pitch Range");
                    newSequenceRanges[0] = GUI.TextField(new Rect(audioX - 150, audioY + 45, 150, 25), ((audioModBits & 0x8000) != 0x0) ? newSequenceRanges[0] : RangeToString(config.Data.seqLightcurveMagToPitchRange));
                    if (newSequenceRanges[0] != "") audioModBits |= 0x8000;

                    GUI.Label(new Rect(audioX - 420, audioY + 75, 270, 25), "RMS to Tempo Range");
                    newSequenceRanges[1] = GUI.TextField(new Rect(audioX - 150, audioY + 75, 150, 25), ((audioModBits & 0x10000) != 0x0) ? newSequenceRanges[1] : RangeToString(config.Data.seqRMSToTempoRange));
                    if (newSequenceRanges[1] != "") audioModBits |= 0x10000;

                    GUI.Label(new Rect(audioX - 420, audioY + 105, 270, 25), "Period to Duration Range");
                    newSequenceRanges[2] = GUI.TextField(new Rect(audioX - 150, audioY + 105, 150, 25), ((audioModBits & 0x20000) != 0x0) ? newSequenceRanges[2] : RangeToString(config.Data.seqPeriodToDurationRange));
                    if (newSequenceRanges[2] != "") audioModBits |= 0x20000;

                    GUI.Label(new Rect(audioX - 420, audioY + 135, 270, 25), "SNR to Rhythmic Variable Range");
                    newSequenceRanges[3] = GUI.TextField(new Rect(audioX - 150, audioY + 135, 150, 25), ((audioModBits & 0x40000) != 0x0) ? newSequenceRanges[3] : RangeToString(config.Data.seqSNRToRhythmicVarRange));
                    if (newSequenceRanges[3] != "") audioModBits |= 0x40000;

                    GUI.Label(new Rect(audioX - 420, audioY + 165, 270, 25), "Magnitude SNR to Instrument Range");
                    newSequenceRanges[4] = GUI.TextField(new Rect(audioX - 150, audioY + 165, 150, 25), ((audioModBits & 0x80000) != 0x0) ? newSequenceRanges[4] : RangeToString(config.Data.seqMagSNRToInstrumentRange));
                    if (newSequenceRanges[4] != "") audioModBits |= 0x80000;

                    GUI.Label(new Rect(audioX - 420, audioY + 195, 270, 25), "Sequence Mapping Ranges");

                    GUI.Label(new Rect(audioX - 420, audioY + 225, 270, 25), "Magnitude Range to Pitch Range");
                    newSequenceRanges[5] = GUI.TextField(new Rect(audioX - 150, audioY + 225, 150, 25), ((audioModBits & 0x100000) != 0x0) ? newSequenceRanges[5] : RangeToString(config.Data.seqmapMagRangetoPitchRange));
                    if (newSequenceRanges[5] != "") audioModBits |= 0x100000;

                    GUI.Label(new Rect(audioX - 420, audioY + 255, 270, 25), "Magnitude Mean to Pitch Center Range");
                    newSequenceRanges[6] = GUI.TextField(new Rect(audioX - 150, audioY + 255, 150, 25), ((audioModBits & 0x200000) != 0x0) ? newSequenceRanges[6] : RangeToString(config.Data.seqmapMagMeantoPitchCenterRange));
                    if (newSequenceRanges[6] != "") audioModBits |= 0x200000;

                    break;
                case 3: //Wave Ranges
                    GUI.Label(new Rect(audioX - 420, audioY + 15, 270, 25), "Wave Ranges");

                    GUI.Label(new Rect(audioX - 420, audioY + 45, 270, 25), "SNR Map to Pitch Range");
                    newWaveRanges[0] = GUI.TextField(new Rect(audioX - 150, audioY + 45, 150, 25), ((audioModBits & 0x400000) != 0x0) ? newWaveRanges[0] : RangeToString(config.Data.waveSNRMapToPitchRange));
                    if (newWaveRanges[0] != "") audioModBits |= 0x400000;

                    GUI.Label(new Rect(audioX - 420, audioY + 75, 270, 25), "RMS Map to Pitch Range");
                    newWaveRanges[1] = GUI.TextField(new Rect(audioX - 150, audioY + 75, 150, 25), ((audioModBits & 0x800000) != 0x0) ? newWaveRanges[1] : RangeToString(config.Data.waveRMSMapToPitchRange));
                    if (newWaveRanges[1] != "") audioModBits |= 0x800000;

                    GUI.Label(new Rect(audioX - 420, audioY + 105, 270, 25), "Teff Map to Pitch Range");
                    newWaveRanges[2] = GUI.TextField(new Rect(audioX - 150, audioY + 105, 150, 25), ((audioModBits & 0x1000000) != 0x0) ? newWaveRanges[2] : RangeToString(config.Data.waveTeffMapToPitchRange));
                    if (newWaveRanges[2] != "") audioModBits |= 0x1000000;

                    GUI.Label(new Rect(audioX - 420, audioY + 135, 270, 25), "Var Map to Pitch Range");
                    newWaveRanges[3] = GUI.TextField(new Rect(audioX - 150, audioY + 135, 150, 25), ((audioModBits & 0x2000000) != 0x0) ? newWaveRanges[3] : RangeToString(config.Data.waveVarMapToPitchRange));
                    if (newWaveRanges[3] != "") audioModBits |= 0x2000000;

                    break;
                default:
                    break;
            }
        }

        //Update Config
        if ((guiFlags & ((int)GUISection.IPVAL + (int)GUISection.MESH + (int)GUISection.AUDIORANGES + (int)GUISection.VOLUMESLIDERES)) != 0x0)
        {
            if (GUI.Button(new Rect(x, Screen.height - 100, 230, 75), "Update Config"))
            {
                modBits = 0x0;
                audioModBits = 0x0;

                if (newID != "") config.Data.myID = int.Parse(newID);
                if (config.Data.myID < 0 || 3 < config.Data.myID) config.Data.myID = -1;
                else if (config.Data.myID == 3) config.Data.serverStatus = "server";
                else config.Data.serverStatus = "client";

                Func<string, string, string> ipCheck = new Func<string, string, string>((n, d) =>
                {
                    Regex lh = new Regex(@" *[Ll]ocalhost *");
                    Match lResult = lh.Match(n);
                    if (lResult.Success) return "127.0.0.1";

                    Regex ip = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
                    Match result = ip.Match(n);

                    if (result.Success) return n;
                    return d;
                });

                config.Data.audioIP = ipCheck(newAudio, config.Data.audioIP);
                config.Data.serverIP = ipCheck(newServer, config.Data.audioIP);
                config.Data.textIP = ipCheck(newText, config.Data.audioIP);
                config.Data.dataBaseIP = ipCheck(newDatabase, config.Data.audioIP);

                Func<string, string, Vector2, Vector2> meshRangeCheck = new Func<string, string, Vector2, Vector2>((n, r, d) =>
                {
                    Regex vector = new Regex(@r);
                    Match result = vector.Match(n);

                    if (!result.Success) return d;

                    n = n.Trim(new char[] { '<', '>', ' ' });
                    string[] nums = n.Split(new char[] { ',' });

                    return new Vector2(int.Parse(nums[0]), int.Parse(nums[1]));
                });

                string xRegex = "^ *<([1-9]|10|11), *([1-9]|10|11)> *$";
                string zRegex = "^ *<[1-5], *[1-5]> *$";

                config.Data.meshRangeX = meshRangeCheck(newMeshRangeX, xRegex, config.Data.meshRangeX);
                config.Data.meshRangeZ = meshRangeCheck(newMeshRangeZ, zRegex, config.Data.meshRangeZ);
                config.Data.meshMQRangeX = meshRangeCheck(newMQMeshRangeX, xRegex, config.Data.meshMQRangeX);
                config.Data.meshMQRangeZ = meshRangeCheck(newMQMeshRangeZ, zRegex, config.Data.meshMQRangeZ);
                config.Data.meshHQRangeX = meshRangeCheck(newHQMeshRangeX, xRegex, config.Data.meshHQRangeX);
                config.Data.meshHQRangeZ = meshRangeCheck(newHQMeshRangeZ, zRegex, config.Data.meshHQRangeZ);

                Func<string, string, Vector3, Vector3> normRangeCheck = new Func<string, string, Vector3, Vector3>((n, r, d) =>
                {
                    Regex vector = new Regex(@r);
                    Match result = vector.Match(n);

                    if (!result.Success) return d;

                    n = n.Trim(new char[] { '<', ')', ' ' });
                    string[] nums = n.Split(new char[] { ',', '>', '(' });

                    return new Vector3(float.Parse(nums[0]), float.Parse(nums[1]), int.Parse(nums[3]));
                });

                string numRegex = "^ *<[0-9]+|[0-9]+\\.[0-9]+, *[0-9]+|[0-9]+\\.[0-9]+> *([0-9]+) *$";

                config.Data.tapMagToPitchRange = normRangeCheck(newTapRanges[0], numRegex, config.Data.tapMagToPitchRange);
                config.Data.tapIntToVelocityRange = normRangeCheck(newTapRanges[1], numRegex, config.Data.tapIntToVelocityRange);
                config.Data.tapAngleToTimbreRange = normRangeCheck(newTapRanges[2], numRegex, config.Data.tapAngleToTimbreRange);
                config.Data.tapColorToInstrumentRange = normRangeCheck(newTapRanges[3], numRegex, config.Data.tapColorToInstrumentRange);

                config.Data.droneMagToPitchRange = normRangeCheck(newDroneRanges[0], numRegex, config.Data.droneMagToPitchRange);
                config.Data.droneRMSToVelocityRange = normRangeCheck(newDroneRanges[1], numRegex, config.Data.droneRMSToVelocityRange);
                config.Data.dronePeriodToMaxDurRange = normRangeCheck(newDroneRanges[2], numRegex, config.Data.dronePeriodToMaxDurRange);
                config.Data.droneSNRToModMixRange = normRangeCheck(newDroneRanges[3], numRegex, config.Data.droneSNRToModMixRange);
                config.Data.droneMagRMSToDroneVibRange = normRangeCheck(newDroneRanges[4], numRegex, config.Data.droneMagRMSToDroneVibRange);
                config.Data.droneMagPeriodToDroneSpeedRange = normRangeCheck(newDroneRanges[5], numRegex, config.Data.droneMagPeriodToDroneSpeedRange);
                config.Data.droneMagSNRToDroneHarmRange = normRangeCheck(newDroneRanges[6], numRegex, config.Data.droneMagSNRToDroneHarmRange);
                config.Data.droneSNRToDroneFBRange = normRangeCheck(newDroneRanges[7], numRegex, config.Data.droneSNRToDroneFBRange);
                config.Data.droneMagSNRToInstrumentRange = normRangeCheck(newDroneRanges[8], numRegex, config.Data.droneMagSNRToInstrumentRange);
                config.Data.droneColorToInstrumentRange = normRangeCheck(newDroneRanges[9], numRegex, config.Data.droneColorToInstrumentRange);
                config.Data.droneRMSToInstrumentRange = normRangeCheck(newDroneRanges[10], numRegex, config.Data.droneRMSToInstrumentRange);

                config.Data.seqLightcurveMagToPitchRange = normRangeCheck(newSequenceRanges[0], numRegex, config.Data.seqLightcurveMagToPitchRange);
                config.Data.seqRMSToTempoRange = normRangeCheck(newSequenceRanges[1], numRegex, config.Data.seqRMSToTempoRange);
                config.Data.seqPeriodToDurationRange = normRangeCheck(newSequenceRanges[2], numRegex, config.Data.seqPeriodToDurationRange);
                config.Data.seqSNRToRhythmicVarRange = normRangeCheck(newSequenceRanges[3], numRegex, config.Data.seqSNRToRhythmicVarRange);
                config.Data.seqMagSNRToInstrumentRange = normRangeCheck(newSequenceRanges[4], numRegex, config.Data.seqMagSNRToInstrumentRange);
                config.Data.seqmapMagRangetoPitchRange = normRangeCheck(newSequenceRanges[5], numRegex, config.Data.seqmapMagRangetoPitchRange);
                config.Data.seqmapMagMeantoPitchCenterRange = normRangeCheck(newSequenceRanges[6], numRegex, config.Data.seqmapMagMeantoPitchCenterRange);

                config.Data.waveSNRMapToPitchRange = normRangeCheck(newWaveRanges[0], numRegex, config.Data.waveSNRMapToPitchRange);
                config.Data.waveRMSMapToPitchRange = normRangeCheck(newWaveRanges[1], numRegex, config.Data.waveRMSMapToPitchRange);
                config.Data.waveTeffMapToPitchRange = normRangeCheck(newWaveRanges[2], numRegex, config.Data.waveTeffMapToPitchRange);
                config.Data.waveVarMapToPitchRange = normRangeCheck(newWaveRanges[3], numRegex, config.Data.waveVarMapToPitchRange);

                FindObjectOfType<ioanDataMapper>().UpdateAudioRangeValues(config.Data);
                config.WriteToJson();
            }
        }

        //FPS Counter
        if ((guiFlags & (int)GUISection.FPS) != 0x0)
        {
            GUI.Label(new Rect(Screen.width - 60f, 10f, 200f, 25f), string.Format("{0} FPS", fps.ToString("N0")));
        }
    }
    #endregion

    #region PUBLIC_FUNC
    /// <summary>
    ///     Progress the loading bar
    /// </summary>
    /// <param name="step"></param>
    public void TickLoading(float step)
    {
        if (step < 0.0f) progressAmt = 0.0f;
        else progressAmt += step;
        string colString = ColorUtility.ToHtmlStringRGBA(Color.HSVToRGB(progressAmt, 0.75f, 1.0f));
        progressStr = "Loading Meshes\n- <color=#" + colString + ">" + ((int)(progressAmt * 100f)).ToString() + "%</color> -";
    }

    /// <summary>
    ///     Toggle the GUISection flag on/off
    /// </summary>
    /// <param name="val"></param>
    public void ToggleGUI(GUISection val)
    {
        guiFlags ^= (int)val;
    }

    /// <summary>
    ///     Set GUISection flag on
    /// </summary>
    /// <param name="val"></param>
    public void SetGUI(GUISection val)
    {
        guiFlags |= (int)val;
    }

    /// <summary>
    ///     Set GUISection flag off
    /// </summary>
    /// <param name="val"></param>
    public void UnsetGUI(GUISection val)
    {
        guiFlags &= (int.MaxValue ^ (int)val);
    }

    public void CycleAudioRangePage()
    {
        audioPageIndex++;
        audioPageIndex %= 4;
    }
    #endregion
}