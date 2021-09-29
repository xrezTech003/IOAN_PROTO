using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AK;
using Mirror;

/// <summary>
/// This script controls parameters dealing with WWises's output.  Because of things not working as expected, there are some limitations 
/// - Soundbanks must be compiled with the target output selected in the wwise project. Switching this in real-time does not work
/// - Changing the ASIO device using the enviroment variable WWISEASIODRV also does not work, so the ASIO device used must either come first aphebetically, or be the only device installed. 
/// - I would propose using the system device for the headsets and the ASIO device for the server 
/// </summary>
public class wwiseOptions : NetworkBehaviour
{
    /// <summary>
    /// The wwise gloabal volume
    /// </summary>
    //[Range(0, 100)] public float volume = 75;
    /// <summary>
    /// ends all audio events 
    /// </summary>
    public bool panic = false;
    bool isRecording = false;
    /// <summary>
    /// Sets channel output mode and toggles binaural decoder 
    /// </summary>
    [Header("Channel Outputs")]
    public bool setMeToHeadphones = false;
    public bool setMeToSpeakers = false;

    /// <summary>
    /// Refreence to the ioanVoiceManger
    /// </summary>
    IOANVoiceManager manager;
    /// <summary>
    /// Checks for if the volume has changed
    /// </summary>
    //float lastVolume;
    /// <summary>
    /// Class for setting Wwise's channel configuration 
    /// </summary>
    AkChannelConfig channels = new AkChannelConfig();
    Config config;
    //public string defaultAsioDevice = "Dante Via (x64)";
    string currentAsioDevice;

    float timeEllapsed;
    /// <summary>
    /// On start, wwise options gets the voice manager, config, and then determines if the mode should be set to speakers or headphones 
    /// </summary>

    private float tapVolume;
    private float sequenceVolume;
    private float droneVolume;
    private float waveVolume;
    private float interfaceVolume;
    private float masterVolume;

    private void Start()
    {
        manager = GetComponentInChildren<IOANVoiceManager>();
        config = Config.Instance;

        tapVolume = config.Data.masterTapVolume;
        sequenceVolume = config.Data.masterSequenceVolume;
        droneVolume = config.Data.masterDroneVolume;
        waveVolume = config.Data.masterWaveVolume;
        interfaceVolume = config.Data.masterInterfaceVolume;

        //volume = config.Data.volume;
    }

    /// <summary>
    /// Every update frame wwiseOptions forces wwise to render audio in the background, checks for the panic key combination, and looks for any manually changed settings like speaker configuration and volume.  
    /// </summary>
    private void Update()
    {
        
        if (AkSoundEngine.IsInitialized())
        {
            //Ensures wwise will not mute in the background 
            AkSoundEngine.WakeupFromSuspend();
            timeEllapsed += Time.deltaTime;
            if (timeEllapsed > 10000)
            {
                if (config.Data.serverStatus == "server")
                {
                    setAudioToSpeakers();
                }
                else
                {
                    setAudioToHeadphones();
                }
            }



        }

        if (Input.GetKey("s") && Input.GetKeyDown(KeyCode.LeftShift))
        {
            print("speaker test");
            AkSoundEngine.PostEvent("Speaker_Test", gameObject);
        }



        if (Input.GetKeyDown("p") && Input.GetKeyDown(KeyCode.Escape))
        {
            //panics if the key combination p+esc is pressed
            manager.panic();
        }

        if (panic)
        {
            ///Proccesses panic event
            manager.panic();
        }
      
        if (setMeToHeadphones)
        {
            //In the editor, changes the mode to headphones 
            setMeToHeadphones = false;
            setAudioToHeadphones();
            print("audio set to headphones");
        }

        if (setMeToSpeakers)
        {
            //In the editor, changes the mode to speakers  
            setMeToSpeakers = false;
            setAudioToSpeakers();
            print("audio set to speakers");
        }

        if (config.Data.masterTapVolume != tapVolume)
        {
            tapVolume = config.Data.masterTapVolume;
            AkSoundEngine.SetRTPCValue("masterTapVolume", tapVolume, null);
        }

        if (config.Data.masterSequenceVolume != sequenceVolume)
        {
            sequenceVolume = config.Data.masterSequenceVolume;
            AkSoundEngine.SetRTPCValue("masterSequenceVolume", sequenceVolume, null);
        }

        if (config.Data.masterDroneVolume != droneVolume)
        {
            droneVolume = config.Data.masterDroneVolume;
            AkSoundEngine.SetRTPCValue("masterDroneVolume", droneVolume, null);
        }

        if (config.Data.masterWaveVolume != waveVolume)
        {
            waveVolume = config.Data.masterWaveVolume;
            AkSoundEngine.SetRTPCValue("masterWaveVolume", waveVolume, null);
        }

        if (config.Data.masterInterfaceVolume != interfaceVolume)
        {
            interfaceVolume = config.Data.masterInterfaceVolume;
            AkSoundEngine.SetRTPCValue("masterInterfaceVolume", interfaceVolume, null);
        }

        if (config.Data.masterVolume != masterVolume)
        {
            masterVolume = config.Data.masterVolume;
            AkSoundEngine.SetRTPCValue("globalVolume", masterVolume, null);
        }

        /*
        if (volume != lastVolume)
        {
            //Adusts the globa volume 
            lastVolume = volume;            
            AkSoundEngine.SetRTPCValue("globalVolume", volume, null);

        }*/
        /*
         * Uncomment if you want recording capabilites 
        if (Input.GetKey(KeyCode.R) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))&& config.Data.devFlag && !isRecording){
            int[] dateArray = new int[3];
            dateArray[0] = System.DateTime.Now.Month;
            dateArray[1] = System.DateTime.Now.Day;
            dateArray[2] = System.DateTime.Now.Year;
            int[] fileTimeArray = new int[3];
            fileTimeArray[0] = System.DateTime.Now.Hour;
            fileTimeArray[1] = System.DateTime.Now.Minute;
            fileTimeArray[2] = System.DateTime.Now.Second;
            string soundFile = dateArray[0] + "_" + dateArray[1] + "_" + dateArray[2] + '-' + fileTimeArray[0] + "_" + fileTimeArray[1] + "_" + fileTimeArray[2] + ".wav";

            AkSoundEngine.StartOutputCapture(soundFile);
            isRecording = true;
            Debug.Log("Recording Started");
        }

        if(Input.GetKey(KeyCode.F) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && isRecording)
        {
            AkSoundEngine.StopOutputCapture();
            isRecording = false;
            Debug.Log("Recording Stopped");
        }
        */
    }
    /// <summary>
    /// Turn on the binaural decoder and sets the speaker set up to headphones.
    /// </summary>
    public void setAudioToHeadphones()
    {
        AkSoundEngine.SetRTPCValue("Binaural", 100);
        channels.SetStandard(AkSoundEngine.AK_SPEAKER_SETUP_2_0);
        AkSoundEngine.SetBusConfig("SubMaster", channels);
        AkSoundEngine.SetPanningRule(AkPanningRule.AkPanningRule_Headphones);

     


    }
    /// <summary>
    /// Turn off the binaural decoder and sets the speakers setup to a 5.1 configuration 
    /// It may be possible to make this a 6 channel ring? 
    /// </summary>
    public void setAudioToSpeakers()
    {
        //AkSoundEngine.SetBusDevice("Master", "ASIO");
        AkSoundEngine.SetRTPCValue("Binaural", 0);
        channels.SetStandard(AkSoundEngine.AK_SPEAKER_SETUP_7POINT1);
        //AkSoundEngine.SetBusConfig("Master", channels); This may work to dynamically change the output but I am unsure. For now, just hard set in wwise 

        /*
        if (config.Data.audioChannels == 2)
        {
            channels.SetStandard(AkSoundEngine.AK_SPEAKER_SETUP_2POINT1);
        }
        if (config.Data.audioChannels == 3)
        {
            channels.SetStandard(AkSoundEngine.AK_SPEAKER_SETUP_3POINT1);
        }
        if (config.Data.audioChannels == 4)
        {
            channels.SetStandard(AkSoundEngine.AK_SPEAKER_SETUP_4POINT1);
        }
        if (config.Data.audioChannels == 5)
        {
            channels.SetStandard(AkSoundEngine.AK_SPEAKER_SETUP_5POINT1);
        }
        else if (config.Data.audioChannels == 6)
        {
            channels.SetStandard(AkSoundEngine.AK_SPEAKER_SETUP_6POINT1);
        }
        else if (config.Data.audioChannels == 7)
        {
            channels.SetStandard(AkSoundEngine.AK_SPEAKER_SETUP_7POINT1);
        }
        else
        {
            channels.SetStandard((uint)config.Data.audioChannels);
            
        }
        */
        



        AkSoundEngine.SetBusConfig("SubMaster", channels);
        AkSoundEngine.SetPanningRule(AkPanningRule.AkPanningRule_Speakers);
        

        
    }


}
