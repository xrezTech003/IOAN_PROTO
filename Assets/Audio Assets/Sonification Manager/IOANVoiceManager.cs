using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.Windows;
using System.Globalization;
using UnityEngine.Events;
using AK.Wwise;
using Unity.Entities.UniversalDelegates;
/// <summary>
/// This class broadcasts for audio reative elements 
/// </summary>
public class SonificationEvent : UnityEvent<string, Dictionary<string, object>>
{

}


/// <summary>
/// IOANVoiceManager generates a number of voices from a prefab and then allocates new events to avalible voices
/// 
/// 10/09/2020: pretty large refactor to allow for more flexibility
/// **Moved sound banks to this script from the sonifiers 
/// **Added overloads for "do" functions to allow for things to be set dirrectly as well as programatically 
/// 
/// REQUIRES: ioanDataMapper cs on the same object
/// 
/// Author: Chris Poovey
/// Last edited 11/25/2020 by Chris Poovey
/// </summary>
public class IOANVoiceManager : MonoBehaviour
{
    #region PUBLIC_VARS
    /// <summary>
    /// refrences the IOAN sonfication prefab-- this prefab will be instantiated many times on start
    /// </summary>
    [Header("Sonifier instantiation")]
    [Tooltip("Place the IOAN Sonification Prefab Here")] [SerializeField] public GameObject ioanSonificationPrefab;
    /// <summary>
    /// The initial number of prefabs that will be created on load
    /// </summary>
    [SerializeField] public int initalNumberOfVoices = 32;
    //[SerializeField] public int maxVoices = 64;
    /// <summary>
    /// Holds the different sounds for taps
    /// </summary>
    [Header("Sound Banks")]
    [HelpBox("These are events from Wwise")]
    public AK.Wwise.Event[] tapInstruments;
    /// <summary>
    /// An array of wwise event objects.  Each event is a different instrument. These are for sequences. 
    /// </summary>
    public AK.Wwise.Event[] sequenceInstruments; //Holds sounds for sequences 
    /// <summary>
    /// An array of wwise event objects.  Each event is a different instrument. These are for drones.
    /// </summary>
    public AK.Wwise.Event[] droneInstruments; //Holds sounds for drones 
    /// <summary>
    /// An array of wwise event objects.  Each event is a different instrument. These are for waves. 
    /// </summary>
    public AK.Wwise.Event[] waveSounds; //Events for waves 
    /// <summary>
    /// An array of wwise event objects.  Each event is a different instrument. These are for wave impulses.
    /// </summary>
    public AK.Wwise.Event[] waveImpulse; //Impulses for the waves


    /// <summary>
    /// Link to the rms CSV for the wave
    /// </summary>
    [Space]
    [Header("Wave Assigments")]
    [HelpBox("Set both CSVs for the waves and their read indexes.  Indices are 0 based")]
    public TextAsset rmsCsv;
    /// <summary>
    /// Column to read from the rms csv
    /// </summary>
    public int rmsCsvReadIndex = 2;
    /// <summary>
    /// Link to the snr csv for the wave
    /// </summary>
    public TextAsset snrCsv;
    /// <summary>
    /// Column to read from the snr csv
    /// </summary>
    public int snrCsvReadIndex = 2;
    /// <summary>
    /// Link to the teff csv for the wave
    /// </summary>
    public TextAsset teffCsv;
    /// <summary>
    /// Column to read from the teff csv
    /// </summary>
    public int teffCsvReadIndex = 2;
    /// <summary>
    /// Link to the var type csv for the wave
    /// </summary>
    public TextAsset varTypeCsv;
    /// <summary>
    /// Column to read from the var type csv
    /// </summary>
    public int varTypeCsvReadIndex = 2;
    /// <summary>
    /// Path to a folder containing the sequence periodigrams 
    /// </summary>
    [Space]
    [Header("Pitch sequence information")]
    public string pathToSequencePeriodigrams = "pitchSequences";
    /// <summary>
    /// Path to a file to look up which file to find a speciific periodigram
    /// </summary>
    public TextAsset figLocFile;
    /// <summary>
    /// Link to the remixReplayEventRecord script in the scene
    /// </summary>
    [Space]
    [Header("Remix Replay and Events")]
    public remixReplayEventRecord remixReplayEventRecord;

    /// <summary>
    /// Dictionary used for audio reactive events 
    /// </summary>
    public Dictionary<string,Dictionary<string, object>> audioInfo = new Dictionary<string, Dictionary<string,object>>();
    /// <summary>
    /// Event used to broadcast sonification events 
    /// </summary>
    public SonificationEvent sonificationEvent = new SonificationEvent();
    /// <summary>
    /// Class used to map data
    /// </summary>
    [Space]
    [Header("Wwise parameters")]
    /// <summary>
    /// the amount of reverb in db to apply to wave sound
    /// </summary>
    [Header("Reveb Controls")]
    [Range(-98, 0)] public int waveReverb = 0;
    /// <summary>
    /// the amount of reverb in db to apply to sequences
    /// </summary>
    [Range(-98, 0)] public int sequenceReverb = -30;
    /// <summary>
    /// the duration of the wave in seconds
    /// </summary>
    [Range(1, 12)] public uint waveDuration = 8;

    [HideInInspector]
    public ioanDataMapper dataMapper;
    #endregion

    #region PRIVATE_VARS

    /// <summary>
    /// A list refrencing generated prefabs
    /// </summary>
    List<GameObject> ioanSonifierObjects;
    /// <summary>
    /// A list refrensing the IOANSonify script on these prefabs
    /// </summary>
    List<IOANSonify> ioanSonifiers;


    private float[,] rmsMap;
    private float[,] snrMap;
    private float[,] teffMap;
    private float[,] varTypeMap;
    private Dictionary<uint, string> figLoc = new Dictionary<uint, string>();
    private int lastWaveReverb;
    private int lastSequenceReverb;
    #endregion

    #region TESTING_VARS
    [Header("Testing Toggles")]
    public bool testTap = false;
    public int tapInstrumentIndex = 0;
    [Range(7, 19)] public int starMagnitude = 10;
    [Range(-180, 180)] public float tapAngle = 0;
    [Range(0, 4)] public float tapIntensity = 1;
    public bool testDrone = false;
    public bool testSequence = false;
    public bool generateRandomStarID = true;
    [Range(40000001, 43525321)] public uint starID = 40000001;
    
    [Header("Wave Test")]
    public Vector3 wavePlayerPosition;
    public bool doRmsWave;
    public bool doSnrWave;
    public bool doTeffWave;
    public bool doVarWave;
    [Range(0, 100)] public uint waveIntensity = 50;
    [Range(25,500)] public uint waveGranularity = 50;


    

    public bool stopAll = false;
    #endregion

    #region UNITY_FUNC
    /// <summary>
    /// Intantiates copys of the ioanSonification prefab and adds the to a list as well as the sonification script on those prefabs
    /// </summary>
    void Start()
    {
        dataMapper = GetComponent<ioanDataMapper>();

        ioanSonifierObjects = new List<GameObject>();
        ioanSonifiers = new List<IOANSonify>();

        rmsMap = parseCSV(rmsCsv);
        snrMap = parseCSV(snrCsv);
        teffMap = parseCSV(teffCsv);
        varTypeMap = parseCSV(varTypeCsv);
        figLoc = parseCsvAsDict(figLocFile);

        print(figLoc.Count);

        for (int i = 0; i < initalNumberOfVoices; i++)
        {
            GameObject newSonificationObject = Instantiate<GameObject>(ioanSonificationPrefab);
            ioanSonifierObjects.Add(newSonificationObject);
            ioanSonifierObjects[i].transform.parent = transform;
            ioanSonifiers.Add(ioanSonifierObjects[i].GetComponent<IOANSonify>());
        }


    }
    /// <summary>
    /// Only used for testing...
    /// </summary>
    private void Update()
    {
        if ((testTap || testDrone || testSequence) & generateRandomStarID)
        {
            starID = (uint)Random.Range(40000001, 43525321);
        }
        if (testTap)
        {
            AK.Wwise.Event tapInstrument = tapInstruments[tapInstrumentIndex];
            performTap(starID, dataMapper.droneMagnitudeToPitch.normalize(starMagnitude), tapIntensity, tapInstrumentIndex, tapInstrumentIndex, tapAngle, new Vector3(Random.Range(-5, 5), Random.Range(-5, 5), Random.Range(-5, 5)), tapInstrument);
            testTap = false;
        }
        if (testDrone)
        {
            performDrone(starID, Time.time, Random.Range(0f, 1f), 0.5f, Random.Range(0f, 1f), 0.5f, 0.5f, gameObject);
            testDrone = false;
        }

        if (testSequence || (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl)))
        {

            performSequence(starID, Time.time, Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1f), Random.Range(0.0f, 1f), Random.Range(0.0f, 1f), gameObject, 1000);
            testSequence = false;
        }

        if (doRmsWave)
        {
            performWave(0, wavePlayerPosition, waveIntensity);
            doRmsWave = false;
        }
        if (doSnrWave)
        {
            performWave(1, wavePlayerPosition, waveIntensity);
            doSnrWave = false;
        }
        if (doTeffWave)
        {
            performWave(2, wavePlayerPosition, waveIntensity);
            doTeffWave = false;
        }
        if (doVarWave)
        {
            performWave(3, wavePlayerPosition, waveIntensity);
            doVarWave = false;
        }
        if (stopAll)
        {
            stopAll = false;
            panic();
        }

        updateReverb();

    }
    #endregion

    #region PRIVATE_FUNC
    /// <summary>
    /// Forces a file path to have a certain extention
    /// </summary>
    /// <param name="path">file path</param>
    /// <param name="extention">file extenion</param>
    /// <returns></returns>
    string forceExtention(string path, string extention)
    {
        string[] pathArray = path.Split('.');
        return pathArray[0] + '.' + extention;

    }

    /// <summary>
    /// Based on a star ID, looks up a file and finds the line in that file relevent to the star ID.  Requires a dictionary containing star IDs and folders as well as text files in the pitchSequences folder in the correct format.  This format is line[0] = indecise of all lines bellow 1, other lines 8 floats that represent magnitudes. 
    /// Written by Christopher Poovey 7/23/2020
    /// </summary>
    /// <param name="starID"></param>
    /// <returns>Array of magnitudes</returns>
    float[] getLightCurveByStarID(uint starID)
    {
        float[] lightcurve = null;
        string fileName;
        print(figLoc.Keys.Count);
        bool entryFound = figLoc.TryGetValue(starID, out fileName);
        if (entryFound)
        {
            string pathToMagText = Application.streamingAssetsPath + "/" + "pitchSequences/" + fileName;
            //print(pathToMagText);
            pathToMagText = forceExtention(pathToMagText, "txt");
            string magnitudeText = System.IO.File.ReadAllText(pathToMagText);
            string[] lines = magnitudeText.Split('\n');
            string[] indexLine = lines[0].Split(' ');
            int lineToRead = -1;
            for (int i = 0; i < indexLine.Length; i++)
            {
                try
                {
                    uint fileIndex = System.Convert.ToUInt32(indexLine[i]);
                    if (fileIndex == starID)
                    {

                        lineToRead = i + 1;
                        //print("found match on line " + lineToRead);
                    }
                }
                catch
                {
                    Debug.LogWarning("could not convert " + indexLine[i] + " to uint");
                }

            }
            if (lineToRead >= 0)
            {
                string line = lines[lineToRead].TrimEnd(new char[] { ' ' });
                line = new string(line.Where((c) => !char.IsControl(c)).ToArray());
                string[] magString = line.Split(' ');

                List<float> lightCurveList = new List<float>();
                for (int m = 0; m < magString.Length; m++)
                {
                    string thisMagString = magString[m];
                    //print(thisMagString);
                    float mag;
                    bool parsed = float.TryParse(thisMagString, out mag);
                    if (parsed)
                    {
                        lightCurveList.Add(mag);
                    }
                    else
                    {
                        Debug.LogWarning("Unable to parse string " + thisMagString + " to a float");
                    }
                }
                lightcurve = lightCurveList.ToArray();
            }

        }
        return lightcurve;
    }
    /// <summary>
    /// Looks for the first avalible sonification object
    /// </summary>
    /// <param name="starID">Star ID used for voice managment</param>
    /// <returns></returns>
    public IOANSonify findVoice(uint starID)
    {
        IOANSonify mySonifier = null;
        // Check for sonifiers with the same star ID
        foreach (IOANSonify sonifier in ioanSonifiers)
        {
            /*
             * This is beaking things... 
            if (sonifier.busy && sonifier.starID == starID && false)
            {
                This is breaking things.... 
               Debug.Log("Found matching star with ID " + starID);
               return sonifier;
            }
            */

            if (!sonifier.busy) //Finds the first empty sonfier- will return if no ID is matched
            {
                mySonifier = sonifier;
                break;
            }
        }
        /*
        if (mySonifier == null && ioanSonifiers.Count < maxVoices)
        {
           
             * Dont add additional voices for now..
            GameObject newSonificationObject = Instantiate<GameObject>(ioanSonificationPrefab);
            ioanSonifierObjects.Add(newSonificationObject);
            int i = ioanSonifierObjects.Count - 1;
            ioanSonifierObjects[i].transform.parent = transform;
            ioanSonifiers.Add(ioanSonifierObjects[i].GetComponent<IOANSonify>());
            mySonifier = ioanSonifiers[i];
           
        }
        */
        return mySonifier;
    }
    /// <summary>
    /// Reads a CSV file into and array of floats 
    /// </summary>
    /// <param name="csv">CSV file</param>
    /// <returns>Array in the shae of the csv</returns>
    float[,] parseCSV(TextAsset csv)
    {
        char lineSeperater = '\n';
        char fieldSeperator = ',';
        string[] dataString = csv.text.Split(lineSeperater);
        int nLines = dataString.Length - 1;
        int nFields = dataString[0].Length;
        float[,] dataFloat = new float[nLines, nFields];
        for (int line = 1; line < nLines; line++)
        {
            string[] fieldString = dataString[line].Split(fieldSeperator);
            for (int field = 0; field < fieldString.Length; field++)
            {
                float fieldValue;
                float.TryParse(fieldString[field], out fieldValue);
                dataFloat[line - 1, field] = fieldValue;
            }
        }

        return dataFloat;
    }
    /// <summary>
    /// Parses a two element CSV to a dict. This could later be expanded to be more flexible... right now it is just used for convert the list of star ids and files to a dict
    /// 
    /// Written by Christopher Poovey 7/23/20
    /// </summary>
    /// <param name="csv">Two field csv file</param>
    /// <returns>Dictionary of a uint key and string result</returns>
    Dictionary<uint, string> parseCsvAsDict(TextAsset csv)
    {
        Dictionary<uint, string> fileLocDict = new Dictionary<uint, string>();
        string[] csvArray = csv.text.Split('\n');
        for (var s = 0; s < csvArray.Length; s++)
        {

            string[] entries = csvArray[s].Split(',');
            try
            {
                uint key = System.Convert.ToUInt32(entries[0]);
                string file = entries[1];
                fileLocDict.Add(key, file);
            }
            catch
            {
                Debug.LogWarning("key number " + s + " did not convert... string was: " + csvArray[s]);
            }
        }

        //print("I am done");
        return fileLocDict;

    }

    /// <summary>
    /// update the RTPCs that control reverb send for testing
    /// </summary>
    void updateReverb()
    {
        if (sequenceReverb != lastSequenceReverb)
        {
            lastSequenceReverb = sequenceReverb;
            AkSoundEngine.SetRTPCValue("sequenceReverb", sequenceReverb);
        }
        if (waveReverb != lastWaveReverb) {
            lastWaveReverb = waveReverb;
            AkSoundEngine.SetRTPCValue("waveReverb", waveReverb);
        }
    }
    #endregion

    #region PUBLIC_FUNC

    /// <summary>
    /// Overload of perform tap that determines the instrument based on star type
    /// </summary>
    /// <param name="starID">ID of the star being tapped</param>
    /// <param name="normalizedMagnitude">The light magnitude of a star</param>
    /// <param name="tapIntensity">The intensity of a tap interaction</param>
    /// <param name="starType">The type of star being tapped</param>
    /// <param name="normalizedColor">The astronomic psuedocolor of the star being tapped</param>
    /// <param name="tapAngle">The angle at which a star is tapped</param>
    /// <param name="position">The position of the tap</param>
    /// <returns></returns>
    public IOANSonify performTap(uint starID, float normalizedMagnitude, float normalizedSnr, float tapIntensity, int starType, float normalizedColor, float tapAngle, Vector3 position)
    {
        //int instrumentIndex = Mathf.FloorToInt(dataMapper.magSnrToInstrument.mapInt((normalizedMagnitude / normalizedSnr)));
        int instrumentIndex = Mathf.FloorToInt(dataMapper.tapColorToInstrument.mapInt(normalizedColor));
        AK.Wwise.Event tapInstrument = tapInstruments[instrumentIndex];
        return performTap(starID, normalizedMagnitude, tapIntensity, starType, normalizedColor, tapAngle, position, tapInstrument);
    }
    /// <summary>
    /// Maps star data to musical paramneters and generates a tap.  Returns the IOANSonfify instance it is acting on.
    /// </summary>
    /// <param name="starID">ID of the star being tapped</param>
    /// <param name="normalizedMagnitude">The light magnitude of a star</param>
    /// <param name="tapIntensity">The intensity of a tap interaction</param>
    /// <param name="starType">The type of star being tapped</param>
    /// <param name="normalizedColor">The astronomic psuedocolor of the star being tapped</param>
    /// <param name="tapAngle">The angle at which a star is tapped</param>
    /// <param name="position">The position of the tap</param>
    /// <param name="tapInstrument">Refrence to the tap instrument in wwise</param>
    /// <returns></returns>
    public IOANSonify performTap(uint starID, float normalizedMagnitude, float tapIntensity, int starType, float normalizedColor, float tapAngle, Vector3 position, AK.Wwise.Event tapInstrument)
    {
        IOANSonify sonifier = findVoice(starID);
        if (sonifier == null)
        {
            return null;
        }

        //Translate Values to musical parameters 
        sonifier.starID = starID;

        float absolutePitch = dataMapper.tapMagnitudeToPitch.mapNormalized(normalizedMagnitude); //Is in the mesh

        float pitch = absolutePitch;



        if (!dataMapper.bypassQuantization)
        {
            if (dataMapper.useDynamicScaleTransposition)
            {
                dataMapper.mappedScale.transposition = dataMapper.mappedScale.retrieveScale()[0];
            }
            pitch = dataMapper.mappedScale.map(absolutePitch); //Map to scale here
        }


        int velocity = (int)dataMapper.tapIntensityToVelocity.map(tapIntensity);

        sonifier.setPosition(position);

        float timbre = dataMapper.tapAngleToTimbre.map(tapAngle);



        sonifier.doTap(pitch, velocity, 0.5f, timbre, tapInstrument);  //All inputs to this function should be musical

        //Makes an event for the tap...
        /*
         * Old remix replay recording
        List<DictionaryEntry> replayEvent = new List<DictionaryEntry>();

        replayEvent.Add(new DictionaryEntry("pitch", pitch));
        replayEvent.Add(new DictionaryEntry("velocity", velocity));
        replayEvent.Add(new DictionaryEntry("starID", starID));
        replayEvent.Add(new DictionaryEntry("normalizedMagnitude", normalizedMagnitude));
        replayEvent.Add(new DictionaryEntry("action", "\"tap\""));
        replayEvent.Add(new DictionaryEntry("vartype", starType + 1));

        remixReplayEventRecord.addReplayEvent(replayEvent);
        */

        return sonifier;
    }
    /// <summary>
    /// Overload of performDrone that determines that finds a sonifier 
    /// </summary>
    /// <param name="starID">ID of the selected star</param>
    /// <param name="normalizedMagnitude">Light magnitude of the selected star</param>
    /// <param name="normalizedRms">the RMS of the selected star</param>
    /// <param name="normalizedPeriod">the light period of the selected star</param>
    /// <param name="normalizedSnr">the SNR of the selected star</param>
    /// <param name="normalizedColor">the astronomic psuedocolor of the selected star</param>
    /// <param name="linkedStar">"while droning the position should be linked"</param>
    /// <returns></returns>
    public IOANSonify performDrone(uint starID, double currentTime, float normalizedMagnitude, float normalizedRms, float normalizedPeriod, float normalizedSnr, float normalizedColor, GameObject linkedStar)
    {
        IOANSonify sonifier = findVoice(starID);
        if (sonifier == null)
        {
            return null;
        }
        return performDrone(starID, currentTime, normalizedMagnitude, normalizedRms, normalizedPeriod, normalizedSnr, normalizedColor, linkedStar, sonifier);
    }
    /// <summary>
    /// Maps star data to musical parameters and generates a drone. 
    /// </summary>
    /// <param name="starID">ID of the selected star</param>
    /// <param name="normalizedMagnitude">Light magnitude of the selected star</param>
    /// <param name="normalizedRms">the RMS of the selected star</param>
    /// <param name="normalizedPeriod">the light period of the selected star</param>
    /// <param name="normalizedSnr">the SNR of the selected star</param>
    /// <param name="normalizedColor">the astronomic psuedocolor of the selected star</param>
    /// <param name="linkedStar">"while droning the position should be linked"</param>
    /// <param name="sonifier">link to a sonifier-- used if modifying an existing drone</param>
    /// <returns></returns>
    public IOANSonify performDrone(uint starID, double currentTime, float normalizedMagnitude, float normalizedRms, float normalizedPeriod, float normalizedSnr, float normalizedColor, GameObject linkedStar, IOANSonify sonifier)
    {        
        
        sonifier.starID = starID;
        //Translate data to params here
        sonifier.linkPosition(linkedStar);
        //normalize params
        float absolutePitch = dataMapper.droneMagnitudeToPitch.mapNormalized(normalizedMagnitude);

        float pitch = absolutePitch;
        if (!dataMapper.bypassQuantization)
        {
            if (dataMapper.useDynamicScaleTransposition)
            {
                dataMapper.mappedScale.transposition = dataMapper.mappedScale.retrieveScale()[0];
            }

            pitch = dataMapper.mappedScale.map(absolutePitch);
        }
        

        print("Pitch: " + pitch);
        int velocity = (int)dataMapper.droneRmsToVelocity.mapNormalized(normalizedRms);
        float maxDuration = dataMapper.dronePeriodToMaxDuration.mapNormalized(normalizedPeriod);

        Debug.Log("DRONE DURATION: " + maxDuration);
        /// float modMix = dataMapper.droneSnrToModulationMix.mapNormalized(normalizedSnr);

        //Convert ratios of data values to RTPC ranges, ZT: done differently than in Max/Omnisphere
        //We should make these values mapped using the data mapper
        float magRMS = (normalizedMagnitude / (normalizedRms + normalizedMagnitude));
        float magPeriod = (normalizedMagnitude / (normalizedPeriod + normalizedMagnitude));
        float magSNR = (normalizedMagnitude / (normalizedSnr + normalizedMagnitude));

        float droneVid = dataMapper.droneMagRmsToDroneVib.map(magRMS);
        //using psuedocolor for now to decide instrument
        int instrument = dataMapper.droneColorToInstrument.mapInt(normalizedColor);
        //print("normalizedColor: " + normalizedColor);
        //print("instrument: " + instrument);
        AK.Wwise.Event droneInstrument = droneInstruments[instrument];
        float droneSpeed = dataMapper.droneMagPeriodToDroneSpeed.map(magPeriod);
        float droneHarm = dataMapper.droneMagSnrToDroneHarm.map(magSNR);
        float droneFb = dataMapper.droneSnrToDroneFb.mapNormalized(normalizedSnr);

        if (!sonifier.isDroning)
        {
            //This needs to run when the star is recreated 
            sonifier.beginDrone(pitch, velocity, droneInstrument, droneVid, droneSpeed, droneFb, droneHarm, maxDuration, currentTime);


            /*
             *Old RR                 
            int[] seqTime = remixReplayEventRecord.getTime();
            string sequenceName = seqTime[4].ToString();
            sonifier.sequenceName = sequenceName;
            List<DictionaryEntry> replayEvent = new List<DictionaryEntry>();

            replayEvent.Add(new DictionaryEntry("pitch", pitch));
            replayEvent.Add(new DictionaryEntry("velocity", velocity));
            replayEvent.Add(new DictionaryEntry("starID", starID));
            replayEvent.Add(new DictionaryEntry("action", "\"seq\""));
            replayEvent.Add(new DictionaryEntry("instrument", instrument));
            replayEvent.Add(new DictionaryEntry("duration", maxDuration * 1000));
            replayEvent.Add(new DictionaryEntry("seqName", sequenceName));
            replayEvent.Add(new DictionaryEntry("normalizedRms", normalizedRms));
            replayEvent.Add(new DictionaryEntry("normalizedSnr", normalizedSnr));
            replayEvent.Add(new DictionaryEntry("normalizedPeriod", normalizedPeriod));

            remixReplayEventRecord.addReplayEvent(replayEvent);
            */

        }


        return sonifier;
    }
    /// <summary>
    /// Ends a drone based on star ID
    /// </summary>
    /// <param name="starID">ID of the selected star</param>
    /// <returns></returns>
    public IOANSonify releaseDrone(uint starID)
    {
        IOANSonify sonifier = findVoice(starID);
        if (sonifier == null)
        {
            return null;
        }
        remixReplayEventRecord.addSeqOff(sonifier.sequenceName);
        sonifier.unlinkPosition();
        sonifier.endDrone();

        


        return sonifier;
    }
    /// <summary>
    /// Ends a sequence event based for a sonifier
    /// </summary>
    /// <param name="sonifier">Sonifier to turn off</param>
    /// <returns></returns>
    public IOANSonify releaseDrone(IOANSonify sonifier)
    {
        //remixReplayEventRecord.addSeqOff(sonifier.sequenceName);
        sonifier.unlinkPosition();
        sonifier.endDrone();
        return sonifier;
    }

    /// <summary>
    /// Ends a sequence event based on star ID
    /// </summary>
    /// <param name="starID">Star ID of the selected star</param>
    /// <returns></returns>
    public IOANSonify releaseSequence(uint starID)
    {
        IOANSonify sonifier = findVoice(starID);
        if (sonifier == null)
        {
            return null;
        }
        //remixReplayEventRecord.addSeqOff(sonifier.sequenceName);
        sonifier.unlinkPosition();
        sonifier.endSequence();

            
        

        return sonifier;
    }

    /// <summary>
    /// Ends a sequence event based for a sonifier
    /// </summary>
    /// <param name="sonifier">Sonifier to turn off</param>
    /// <returns></returns>
    public IOANSonify releaseSequence(IOANSonify sonifier)
    {
        remixReplayEventRecord.addSeqOff(sonifier.sequenceName);
        sonifier.unlinkPosition();
        sonifier.endSequence();
        
        return sonifier;
    }
    /// <summary>
    /// Overload of performSequencee that finds a sonifier
    /// </summary>
    /// <param name="starID">The ID of the selected star</param>
    /// <param name="lightCurve">a list containing points from the ligtcurve</param>
    /// <param name="normalizedRms">the rms of the star</param>
    /// <param name="normalizedPeriod">the period of the star</param>
    /// <param name="normalizedSnr">the snr of the star</param>
    /// <param name="normalizedMagnitude">the normalized magnitude of a star</param>
    /// <param name="linkedStar">a star whos position becomes linked with the sonifier</param>
    /// <returns></returns>
    public IOANSonify performSequence(uint starID, double currentTime, float normalizedRms, float normalizedPeriod, float normalizedSnr, float normalizedMagnitude, float normalizedColor, GameObject linkedStar, int seed)
    {
        Random.InitState(seed);
        IOANSonify sonifier = findVoice(starID);
        if (sonifier == null)
        {
            return null;
        }
        return performSequence(starID, currentTime, normalizedRms, normalizedPeriod, normalizedSnr, normalizedMagnitude, normalizedColor, linkedStar, sonifier, seed);
    }
    /// <summary>
    /// Initiates a sequeces using star parameters to generate musical ones. 
    /// </summary>
    /// <param name="starID">The ID of the selected star</param>
    /// <param name="normalizedRms">the rms of the star</param>
    /// <param name="normalizedPeriod">the period of the star</param>
    /// <param name="normalizedSnr">the snr of the star</param>
    /// <param name="normalizedMagnitude">the normalized magnitude of a star</param>
    /// <param name="linkedStar">a star whos position becomes linked with the sonifier</param>
    /// <param name="sonifier">Sonfier to do the event</param>
    /// <param name="seed">the random seed used to generate the time sequence</param>
    /// <returns></returns>
    public IOANSonify performSequence(uint starID, double currentTime, float normalizedRms, float normalizedPeriod, float normalizedSnr, float normalizedMagnitude, float normalizedColor, GameObject linkedStar, IOANSonify sonifier, int seed)
    {
        

        float[] lightCurve = getLightCurveByStarID(starID);

        if (lightCurve == null)
        {
            Debug.LogWarning("No sequence found for ID: " + starID);
            return null;
        }        

        sonifier.linkPosition(linkedStar);
        sonifier.starID = starID;        
        List<float> pitches = new List<float>();    

        //Generate the pitches

        pitches = dataMapper.sequenceMapper.map(lightCurve.ToList());
        if (!dataMapper.bypassQuantization) //Map the pitches to a scale
        {
            if (dataMapper.useDynamicScaleTransposition)
            {
                dataMapper.mappedScale.transposition = dataMapper.mappedScale.retrieveScale()[0];
            }
            pitches = dataMapper.mappedScale.mapSequence(pitches);
        }
        Random.InitState(seed);
        int nDurations = Mathf.FloorToInt(Random.Range(3, 12));
        float[] durations = new float[nDurations];

        float tempo = dataMapper.sequenceRmsToTempo.mapNormalized(normalizedRms);
        //print("normalized Period: " + normalizedPeriod);
        //print("tempo: " + tempo);
        //print("pitches: " + pitches);

        float secondsPerBeat = 1 / (tempo / 60);

        float tempoNoise = dataMapper.sequenceSnrToRythmicVariation.mapNormalized(normalizedSnr);

        //Generate rhythm sequence with random depth. 
        for (int i = 0; i < durations.Length; i++)
        {
            durations[i] = (1 + Random.Range(-tempoNoise, tempoNoise));
        }

        

        float terminalPeriod = secondsPerBeat;
        Random.InitState((int)starID); //Make sure the random seed is the same on every client
        float speedUpSlowDown = Random.Range(0, 3);

        //Determine if the sequence speeds up or slows down
        if (speedUpSlowDown < 1)
        {
            terminalPeriod *= 0.75f;
        }
        else if (speedUpSlowDown < 2)
        {
            terminalPeriod *= 1.5f;
        }

        //determine the maximum duration of the star
        float maxDuration = dataMapper.sequencePeriodToDuration.mapNormalized(normalizedPeriod);

        Debug.Log("SEQUENCE DURATION: " + maxDuration);

        //Determine the instrument 
        int instrument = Mathf.FloorToInt(dataMapper.tapColorToInstrument.mapInt(normalizedColor));

        AK.Wwise.Event sequenceInstrument = sequenceInstruments[instrument];
        string plist = "Pitches: ";
        foreach (var item in pitches)
        {
            plist += item.ToString() + ", ";
        }
        print(plist);

        if (!sonifier.isSequencing)
        {
            sonifier.doSequence(pitches.ToArray(), durations, secondsPerBeat, terminalPeriod, sequenceInstrument, maxDuration, currentTime);
        }
        

        /*
         * Old RR
        //Event Data for sequence 

        
            List<DictionaryEntry> replayEvent = new List<DictionaryEntry>();
            int[] seqTime = remixReplayEventRecord.getTime();
            string sequenceName = seqTime[4].ToString();
            string pitchesAsString = "[ ";
            string durationsAsString = "[ ";
            sonifier.sequenceName = sequenceName;
            //Have to convert arrays to a representation of a array as a string
            for (int i = 0; i < pitches.Count; i++)
            {
                if (i != 0)
                {
                    pitchesAsString += " ,";
                }
                pitchesAsString += pitches[i].ToString();

            }

            pitchesAsString += " ]";

            for (int i = 0; i < durations.Length; i++)
            {
                if (i != 0)
                {
                    durationsAsString += " ,";
                }
                durationsAsString += durations[i];

            }

            durationsAsString += " ]";


            replayEvent.Add(new DictionaryEntry("pitchSeq", pitchesAsString));
            replayEvent.Add(new DictionaryEntry("lengthSeq", durationsAsString));
            replayEvent.Add(new DictionaryEntry("starID", starID));
            replayEvent.Add(new DictionaryEntry("action", "\"seq\""));
            replayEvent.Add(new DictionaryEntry("instrument", instrument + 1));
            replayEvent.Add(new DictionaryEntry("duration", maxDuration * 1000));
            replayEvent.Add(new DictionaryEntry("period", secondsPerBeat));
            replayEvent.Add(new DictionaryEntry("terminalPeriod", terminalPeriod));
            replayEvent.Add(new DictionaryEntry("seqName", sequenceName));



            remixReplayEventRecord.addReplayEvent(replayEvent);
        */
        

        return sonifier;
    }

    /// <summary>
    /// Determines a wave type and assigns the correct CSV to the doWave function 
    /// </summary>
    /// <param name="waveType">Wave ID</param>
    /// <param name="initalPosition">Position the wave starts at</param>
    /// <param name="intensity">Intensity of the wave</param>

    public void performWave(int waveType, Vector3 initalPosition, float intensity)
    {
        //Determine which csv to read
        IOANSonify waveVoice = findVoice((uint)Random.Range(5000, 8000));
        if (waveVoice != null) { 
        waveVoice.setPosition(initalPosition);
            
                if (waveType == 0)
                {
                    waveVoice.doWave(rmsMap, initalPosition, rmsCsvReadIndex, dataMapper.waveRmsMapToPitch, dataMapper.mappedScale, waveImpulse[0], waveSounds[0], waveDuration, intensity, waveGranularity, waveType);
                }
                else if (waveType == 2)
                {
                    waveVoice.doWave(teffMap, initalPosition, teffCsvReadIndex, dataMapper.waveRmsMapToPitch, dataMapper.mappedScale, waveImpulse[1], waveSounds[1], waveDuration, intensity, waveGranularity, waveType);

                }
                else if (waveType == 1)
                {
                    waveVoice.doWave(snrMap, initalPosition, snrCsvReadIndex, dataMapper.waveRmsMapToPitch, dataMapper.mappedScale, waveImpulse[2], waveSounds[2], waveDuration, intensity, waveGranularity, waveType);

                }
                else if (waveType == 3)
                {
                    waveVoice.doWave(varTypeMap, initalPosition, varTypeCsvReadIndex, dataMapper.waveRmsMapToPitch, dataMapper.mappedScale, waveImpulse[3], waveSounds[3], waveDuration, intensity, waveGranularity, waveType);

                }
            

        }
        /*
         * old RR
        //Store to remix replay

        List<DictionaryEntry> replayEvent = new List<DictionaryEntry>();
        replayEvent.Add(new DictionaryEntry("action", "\"waveMessage\""));
        replayEvent.Add(new DictionaryEntry("intensity", intensity));
        replayEvent.Add(new DictionaryEntry("starID", starID));
        replayEvent.Add(new DictionaryEntry("type", waveType));
        replayEvent.Add(new DictionaryEntry("position", ("[" + initalPosition[0] + "," + initalPosition[1] + "," + initalPosition[2] + "]")));
        remixReplayEventRecord.addReplayEvent(replayEvent);
        */

    }


    /// <summary>
    /// Ends all sonifier events within this voice manager 
    /// </summary>
    public void panic()
    {
        foreach (IOANSonify sonifier in ioanSonifiers)
        {
            sonifier.panic();
        }
    }
    #endregion




}
