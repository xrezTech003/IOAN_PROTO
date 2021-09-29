using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using AK.Wwise;
using System.Linq;

/// <summary>
/// This class records and saves event files for upload in the remix replay system. It only works if the player is set to be the server only.  Files are set to the application path /*replayLocation*
/// </summary>
public class remixReplayEventRecord : MonoBehaviour
{

    public System.Diagnostics.Process uploadServer = new System.Diagnostics.Process();
    float eventFileTimeSecs = 120;
    float eventFileUpdateSecs = 10;
    /// <summary>
    /// Holds the date
    /// </summary>
    int[] dateArray;
    /// <summary>
    /// Holds the time a files is created
    /// </summary>
    int[] fileTimeArray = { 0, 0, 0, 0 };
    /// <summary>
    /// Holds the current time
    /// </summary>
    int[] timeArray;
    /// <summary>
    /// Holds a representation of the file start time to allow for a relitive time stamp
    /// </summary>
    int fileStartTime = 0;
    /// <summary>
    /// The location to save remix replay files 
    /// </summary>
    public string replayLocation = "\\ReplayFiles\\";
    /// <summary>
    /// Holds the path to event files 
    /// </summary>
    string pathToEvents;
    string pathToTempEvents;
    /// <summary>
    /// Holds the path to audio files 
    /// </summary>
    string pathToAudio;
    /// <summary>
    /// Is it recording? 
    /// </summary>
    public bool recording;
    /// <summary>
    /// Last saves sound file 
    /// </summary>
    string soundFile;
    /// <summary>
    /// The dictionary of events 
    /// </summary>
    Dictionary<string, List<DictionaryEntry>> replayEventDict;
    /// <summary>
    /// Holds the path to a specific file in the wwiseDecodedFiles location
    /// </summary>
    string decodedFilePath;
    /// <summary>
    /// A sub-dictionary containing off events for sequences and drones
    /// </summary>
    Dictionary<string, string> seqOffs;
    /// <summary>
    /// Tells remix replay to record
    /// </summary>
    public bool remixReplayEnabled = false;
    /// <summary>
    /// The last state of remix replay enabled (for initialization) 
    /// </summary>
    bool lastRemixReplayEnabled = false;
    /// <summary>
    /// A dict list is a list of dictionies that span a fixed duration. These will feed into the uploadDict for complete duration upload
    /// </summary>
    List<Dictionary<string,object>> tempDictList = new List<Dictionary<string, object>>();
    Dictionary<string, object> tempDict = new Dictionary<string, object>();
    /// <summary>
    /// Contains these keys 
    /// "start time" : [hh,mm,ss,ms]
    /// "last write" : [hh,mm,ss,ms]
    /// "data" : dictList
    /// "status" : done
    /// </summary>
    Dictionary<string, object> uploadDict = new Dictionary<string, object>();
    List<Dictionary<string, object>> index = new List<Dictionary<string, object>>();
    Dictionary<string, object> indexEntry = new Dictionary<string, object>();
    string filename;
    Config config = Config.Instance;

    int eventCount = 0;
    /// <summary>
    /// Initializes remix replay 
    /// </summary>
    public void startRemixReplay()
    {
        

        dateArray = getDate();
        pathToEvents = System.IO.Directory.GetCurrentDirectory() + replayLocation + dateArray[0] + "_" + dateArray[1] + "_" + dateArray[2] + "\\Events\\";

        if (System.IO.File.Exists(pathToEvents + "index.json")){
            string serialized = System.IO.File.ReadAllText(pathToEvents + "index.json");
            index = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(serialized);
                }
        pathToTempEvents = pathToEvents + "\\Temp\\";
        System.IO.Directory.CreateDirectory(pathToEvents);
        recording = true;
        initializeEventFile();

        startUploadServer();
    /*
     * Wwise setup for audio recording - no longer used 
    if (Application.isEditor)
    {
        decodedFilePath = System.IO.Directory.GetCurrentDirectory() + wwiseDecodedFilesEditor;
    }
    else
    {
        decodedFilePath = Application.streamingAssetsPath + wwiseDecodedFilesApp;
    }
    pathToAudio = System.IO.Directory.GetCurrentDirectory() + replayLocation + dateArray[0] + "_" + dateArray[1] + "_" + dateArray[2] + "\\Audio\\wav\\";

    */
    //Create event file path
   
        //System.IO.Directory.CreateDirectory(pathToTempEvents);
        startNewRecording();

    }

    void startUploadServer()
    {
        uploadServer.StartInfo.FileName = System.IO.Directory.GetCurrentDirectory()  + "\\UploadServer\\ioanrruploadserver.exe";
        uploadServer.StartInfo.Arguments = System.IO.Directory.GetCurrentDirectory() + replayLocation + " " +  dateArray[0] + "_" + dateArray[1] + "_" + dateArray[2] + " " + System.IO.Directory.GetCurrentDirectory() + "\\PlayableAreaData\\data.json";
        uploadServer.StartInfo.UseShellExecute = true;
        uploadServer.Start();


    }
    void initializeEventFile()
    {
        if (pathToEvents == null)
        {
            pathToEvents = System.IO.Directory.GetCurrentDirectory() + replayLocation + dateArray[0] + "_" + dateArray[1] + "_" + dateArray[2] + "\\Events\\";
        }
        fileTimeArray = getTime();
        fileStartTime = fileTimeArray[4];
        filename = fileTimeArray[0] + "_" + fileTimeArray[1] + "_" + fileTimeArray[2] + ".json";
        timeArray = getTime();
        tempDict.Clear();
        replaceDictKey(tempDict, "start time", timeArray);
        replaceDictKey(tempDict, "last write", timeArray);
        replaceDictKey(tempDict, "status", "temp");
        replaceDictKey(tempDict, "data", "[]");

        writeJson(pathToEvents + filename, tempDict);

        indexEntry = new Dictionary<string, object>();
        replaceDictKey(indexEntry, "filename", filename);
        replaceDictKey(indexEntry, "count", eventCount);
        replaceDictKey(indexEntry, "status", "temp");
        index.Add(indexEntry);
        writeJson(pathToEvents + "index.json", index);
    }
    /// <summary>
    /// Stops remix replay
    /// </summary>
    public void stopRemixReplay()
    {
        uploadServer.Kill();
        //saveClearReplayDict();
        recording = false;
        /*
         * Used for wwise recordingf
        StartCoroutine(stopAndMoveFile(soundFile));
        */


    }
    private void Start()
    {
        config = Config.Instance;
        remixReplayEnabled = config.Data.remixReplay;
        lastRemixReplayEnabled = remixReplayEnabled;
    }

    private void OnApplicationQuit()
    {
        uploadServer.Kill();
    }

    // Update is called once per frame
    void Update()
    {

        if (remixReplayEnabled && !lastRemixReplayEnabled)
        {
            startRemixReplay();
        }
        
        if (recording)
        {
            if (pathToEvents == null)
            {
                dateArray = getDate();
                pathToEvents = System.IO.Directory.GetCurrentDirectory() + replayLocation + dateArray[0] + "_" + dateArray[1] + "_" + dateArray[2] + "\\Events\\";
            }
            timeArray = getTime();

            if ((timeArray[4] - fileStartTime) >= eventFileTimeSecs * 1000)
            {
                Dictionary<string, object> testentry = new Dictionary<string, object>();
                
                
                replaceDictKey(tempDict,"data", tempDictList);
                replaceDictKey(tempDict, "last write", timeArray);
                replaceDictKey(tempDict,"status", "done");
                writeJson(pathToEvents + filename, tempDict);
                

                replaceDictKey(indexEntry, "status", "done");
                replaceDictKey(indexEntry, "count", eventCount);
                index.RemoveAt(index.Count - 1);
                index.Add(indexEntry);
                writeJson(pathToEvents + "index.json", index);

                eventCount = 0;
                
                initializeEventFile();
            }

     
            if ((timeArray[4] - fileStartTime) >= (eventFileUpdateSecs * 1000))
            {

                Dictionary<string, object> testentry = new Dictionary<string, object>();
                //testentry.Add("test", Random.Range(0,1000));
                //tempDictList.Add(testentry);
                replaceDictKey(tempDict,"data", tempDictList);
                replaceDictKey(tempDict,"last write", timeArray);
                replaceDictKey(tempDict,"status", "temp");
                writeJson(pathToEvents + filename, tempDict);

                replaceDictKey(indexEntry, "count", eventCount);
                index.RemoveAt(index.Count - 1);
                index.Add(indexEntry);
                writeJson(pathToEvents + "index.json", index);
            }


            
        }
        lastRemixReplayEnabled = remixReplayEnabled;


    }


    void writeJson(string filePath, object json)
    {
        string serialized = Newtonsoft.Json.JsonConvert.SerializeObject(json);
        System.IO.Directory.CreateDirectory(pathToEvents);
        System.IO.File.WriteAllText(filePath, serialized);


    }


    /// <summary>
    /// Begins recording a new remix replay event and audio file 
    /// </summary>
    void startNewRecording()
    {
        //System.IO.Directory.CreateDirectory(pathToAudio);
        //soundFile = dateArray[0] + "_" + dateArray[1] + "_" + dateArray[2] + '-' + fileTimeArray[0] + "_" + fileTimeArray[1] + "_" + fileTimeArray[2] + ".wav";       
        //AkSoundEngine.StartOutputCapture(soundFile);
    }
    /// <summary>
    /// Moves the audio file from the wwise location to the desired remix replay location 
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    IEnumerator stopAndMoveFile(string fileName)
    {
        AkSoundEngine.StopOutputCapture();
        string src = decodedFilePath + fileName;
        string dest = pathToAudio + fileName.Split('-')[1];
        yield return new WaitForSeconds(1);
        System.IO.File.Move(src, dest);
        //There seems to be a slight overlap in the files- we will need to implement crossfade in remix replay,
    }
    /// <summary>
    /// Gets the date
    /// </summary>
    /// <returns>Data array [Month, Day, Year]</returns>
    int[] getDate()
    {
        int[] dateArray = new int[3];
        dateArray[0] = System.DateTime.Now.Month;
        dateArray[1] = System.DateTime.Now.Day;
        dateArray[2] = System.DateTime.Now.Year;

        return dateArray;
    }
    /// <summary>
    /// Gets the time
    /// </summary>
    /// <returns>Time array [Hour, Minute, Second, Millisecond, eleapsed Miliseconds]</returns>
    public int[] getTime()
    {
        int[] timeArray = new int[5];
        timeArray[0] = System.DateTime.Now.Hour;
        timeArray[1] = System.DateTime.Now.Minute;
        timeArray[2] = System.DateTime.Now.Second;
        timeArray[3] = System.DateTime.Now.Millisecond;
        timeArray[4] = timeArray[3] + timeArray[2] * 1000 + timeArray[1] * 60 * 1000 + timeArray[0] * 60 * 60 * 1000;

        return timeArray;
    }
    /// <summary>
    /// Adds an event to the replay dict. Events are lists of dictionary entries. 
    /// </summary>
    /// <param name="replayEvents">A list of replay events as dictionary entries</param>
    public void addReplayEvent(List<DictionaryEntry> replayEvents){
        /*
         * Old remix replay
        if (recording)
        {
            string timeString = (getTime()[4] - fileStartTime).ToString();
            replayEventDict.Add(timeString, replayEvents);
        }
        */
    }
    public void addReplayEvent(Dictionary<string,object> replayEvent)
    {
        if (recording)
        {
            replayEvent.Add("time", getTime());
            replayEvent.Add("relitiveTimeMs", getTime()[4]- fileStartTime);           

            tempDictList.Add(replayEvent);
            eventCount++;



        }
    }

    void replaceDictKey(Dictionary<string, object> dict, string key, object value)
    {


        bool hasKey = dict.ContainsKey(key);
        if (hasKey)
        {
            dict.Remove(key);
        }
        dict.Add(key, value);

    }

    /// <summary>
    /// Adds a sequence off event to the dictionay
    /// </summary>
    /// <param name="name"></param>
    public void addSeqOff(string name)
    {
        /*
        string timeString = (getTime()[4] - fileStartTime).ToString();
        
        if (timeString is string && timeString != null && name != null)
        {
            Debug.Log("Writing SeqOff at " + timeString + " for sequence " + name);
            seqOffs.Add(name, timeString);
        }
        else
        {
            Debug.LogError("remix replay failed to add seq event "+name + " at time " + timeString);
        }
        */
    }


}

