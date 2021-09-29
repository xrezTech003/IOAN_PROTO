using AK.Wwise;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using ioanDataMapping;

/// <summary>
/// IOANSonify contains functions that comunicate to Wwise in order to sonfify data.  Taps, Sequences, and Drones are currently possible.
/// </summary>
public class IOANSonify : MonoBehaviour
{

    #region PRIVATE_VAR
    /// <summary>
    /// Handles the the timer on sequences and drones 
    /// </summary>    
    private IEnumerator voiceHandler;
    /// <summary>
    /// flag to identify if the voice is sequencing
    /// </summary>
    public bool isSequencing = false;
    /// <summary>
    /// flag to identy if a voice is droning
    /// </summary>
    public bool isDroning = false;
    /// <summary>
    /// refrence to the Wwise sound engine
    /// </summary>
    private bool isWaving = false;

    private AkSoundEngine soundEngine;

    remixReplayEventRecord remixReplay;

    private bool linkedToStar = false;
    private GameObject linkedStar;
    float[,] waveMap;
    float seqPeriod = 500;
    float terminalPeriod = 750;
    IOANVoiceManager voiceManager;
    Dictionary<string, Dictionary<string, object>> audioInfo;
    SonificationEvent sonificationEvent;

    string messageName = "";
    string envDictName = "";

    #endregion

    #region PUBLIC_VAR
    /// <summary>
    /// Flag to enable debug messages
    /// </summary>
    public bool debugState = false; 
    /// <summary>
    /// A flag that is set to let the IOANVoiceManger know if the voice is busy or avalible for a new event
    /// </summary>
    public bool busy = false;
    /// <summary>
    /// Stores the start ID of the given event.  This allows events to be turned of and the retrigger tapps 
    /// </summary>
    public uint starID = 0;
    /// <summary>
    /// The release window in seconds from when a voice finishes its event to when it is avalible again 
    /// </summary>
    public float voiceReleaseWindow = 2;
    /// <summary>
    /// The show often a new instrument is called by the wave function
    /// </summary>
    public float waveGranularityMs = 150;
    /// <summary>
    /// The number of seconds before the star despawns
    /// </summary>
    public float timeToDie = 0;  
    /// <summary>
    /// How long a star will last
    /// </summary>
    public float duration;
    /// <summary>
    /// Holds a refrence to the drone instrument
    /// </summary>
    public AK.Wwise.Event droneInstrument;
    /// <summary>
    /// holds a refrence to the sequencer instrument
    /// </summary>
    public AK.Wwise.Event sequenceInstrument;

    public double networkTime = 0.0;
    #endregion
    /// <summary>
    /// Name given to the sequence for remix-replay
    /// </summary>
    [HideInInspector] public string sequenceName;
    /// <summary>
    /// Holds the pitch sequence made in the voice manager
    /// </summary>
    [HideInInspector] public float[] pitchSequence = { 60, 64, 67, 70, 67, 64 };  //Is changes in the do sequence function
    /// <summary>
    /// Holds the time sequence made in the voice manager 
    /// </summary>
    private float[] timeSequence = { 0.5f, 0.25f, 0.25f, 0.33f, 0.125f, 0.33f, 0.25f, 0.5f }; //Is changes in the do sequence function  
    /// <summary>
    /// The pitch of a drone- used for turning the drone off
    /// </summary>
    private int dronePitch = 60;

    public bool updateSequenceMode;
    float seqTimeMultiplier = 1;
    bool[] seqTriggeredArray;
    bool noteTriggered = false;
    float[] normalizedTimeArray;
    float seqPosition = 0;
    int seqPitchCounter;
    float seqTimeSum;

    #region UNITY_FUNC 
    /// <summary>
    /// On start, the sonifier finds its voice manager and event system
    /// </summary>
    private void Start()
    {
        voiceManager = GetComponentInParent<IOANVoiceManager>();
        audioInfo = voiceManager.audioInfo;
        sonificationEvent = voiceManager.sonificationEvent;
        remixReplay = FindObjectOfType<remixReplayEventRecord>();
    }
    /// <summary>
    /// Update is used to get audio envelopes from wwise 
    /// </summary>
    void Update()
    {


        //For linked star...

        if ((linkedToStar) && (isDroning || isSequencing) && (linkedStar != null))
        {
            gameObject.transform.position = linkedStar.transform.position;
        }

        if (busy && messageName != "" && (isDroning))
        {
            //Do amplitude tracking here
            Dictionary<string, object> envDict = new Dictionary<string, object>();
            float envelope;
            int type = (int)AkQueryRTPCValue.RTPCValue_GameObject;
            
            AkSoundEngine.GetRTPCValue("Envelope", gameObject, 0, out envelope, ref type);
            envDict.Add("envelope", envelope);

            envDictName = "env_" + starID;

            if (audioInfo.ContainsKey(envDictName))
            {
                audioInfo.Remove(envDictName);
            }

            audioInfo.Add(envDictName, envDict);
            
            
            
        }

        if (updateSequenceMode == true)
        {
            updateSequenceEvent();
        }

    }
    #endregion


    #region PUBLIC_FUNC
    /// <summary>
    /// Creates a taping sound at the parent objects position 
    /// </summary>
    /// <param name="rawPitch">The MIDI pitch the tap will sound. Decimal values will result in microtonal shifts</param>
    /// <param name="velocity">The MIDI velocity the tap will sound</param>
    /// <param name="duration">The amount of time the tap will ring</param>
    /// <param name="timbre">The angle at which the tap is hit</param>
    /// <param name="instrument">The instrument in the tapSound</param>

    public void doTap(float rawPitch, int velocity, float duration, float timbre, AK.Wwise.Event instrument)

    {
        int pitch = Mathf.FloorToInt(rawPitch);
        AkSoundEngine.SetRTPCValue("Tap_Timbre", timbre * 100, gameObject);
        voiceHandler = voiceAvalibleTimer(voiceReleaseWindow);

        float tapBend = (rawPitch - pitch) * 100f;
        AkSoundEngine.SetRTPCValue("Pitch_Bend", tapBend, gameObject);

        AkMIDIPostArray MIDIPostArrayBuffer = makeNoteOn((int)pitch, velocity);



        busy = true;

        instrument.PostMIDI(gameObject, MIDIPostArrayBuffer);  //Send it to the midi event


        //Broadcast a message so that we know a tab has been sceduled
 
        Dictionary<string, object> tapDict = new Dictionary<string, object>();

        tapDict.Add("pitch", rawPitch);
        tapDict.Add("velocity", velocity);
        tapDict.Add("timbre", timbre);
        tapDict.Add("instrument", instrument.Name);
        tapDict.Add("id", starID);
        tapDict.Add("event", "on");
        tapDict.Add("type", "tap");
        remixReplay.addReplayEvent(tapDict);
        messageName = "tap_" + starID;
        if (audioInfo.ContainsKey(messageName))
        {
            audioInfo.Remove(messageName);
        }
        audioInfo.Add(messageName, tapDict);
        sonificationEvent.Invoke(messageName, tapDict);

        StartCoroutine(voiceHandler);

    }
    /// <summary>
    /// Generates a pitch sequence and scedules each note using a recursive delayed function 
    /// </summary>
    /// <param name="newPitchSequence">The sequence of pitches to be performed</param>
    /// <param name="newTimeSequence">A Sequence of durations between each note in seconds</param>
    /// <param name="instrument">Event for the desired sound</param>
    /// <param name="maxDuration">The maximum time the sequence will play</param>
    public void doSequence(float[] newPitchSequence, float[] newTimeSequence, float thisPeriod, float thisTerminalPeriod, AK.Wwise.Event instrument, float maxDuration, double currentTime)
    {
        if (voiceHandler != null)
        {
            StopCoroutine(voiceHandler);
        };
        sequenceInstrument = instrument;
        if (!isSequencing && !isDroning)
        {
            busy = true;

            timeToDie = (float)currentTime + maxDuration;
            duration = maxDuration;
            //voiceHandler = sequenceTimer(maxDuration);
            pitchSequence = newPitchSequence;
            timeSequence = newTimeSequence;
            seqPeriod = thisPeriod;
            terminalPeriod = thisTerminalPeriod;
            isSequencing = true;
            seqTriggeredArray = new bool[timeSequence.Length];

            for (int t = 0; t < seqTriggeredArray.Length; t++)
            {
                seqTriggeredArray[t] = false;
            }
            

            if (!updateSequenceMode)
            {
                StartCoroutine(sequenceEvent(0, 0, instrument));
            }

            if (updateSequenceMode)
            {
                seqPitchCounter = 0;
                seqPosition = 0;
                normalizedTimeArray = new float[timeSequence.Length];
                seqTimeSum = 0;

                for (int i = 0; i < timeSequence.Length; i++)
                {
                    seqTimeSum += timeSequence[i];
                }
                for (int i = 0; i < normalizedTimeArray.Length; i++)
                {
                    if (i == 0)
                    {
                        normalizedTimeArray[i] = timeSequence[i] / seqTimeSum;
                    }
                    else
                    {
                        normalizedTimeArray[i] = normalizedTimeArray[i-1]+ timeSequence[i] / seqTimeSum;
                    }
                }
                //print("sequence loop time = " + seqPeriod * seqTimeSum);
            }

            Dictionary<string, object> seqDict = new Dictionary<string, object>();
            seqDict.Add("id", starID);
            seqDict.Add("event", "on");
            seqDict.Add("type", "sequence");
            remixReplay.addReplayEvent(seqDict);


        }
    }

    /// <summary>
    /// Ends a sequence. Is triggered in activatedStar OnDestroy
    /// </summary>
    public void endSequence()
    {
        if (isSequencing)
        {
            isSequencing = false;
            voiceHandler = voiceAvalibleTimer(voiceReleaseWindow+3);
            StartCoroutine(voiceHandler);
            Dictionary<string, object> seqDict = new Dictionary<string, object>();
            seqDict.Add("id", starID);
            seqDict.Add("event", "off");
            seqDict.Add("type", "sequence");
            remixReplay.addReplayEvent(seqDict);
        }
    }

    /// <summary>
    /// Initiates a drone in wwise. 
    /// </summary>
    /// <param name="rawPitch">The MIDI pitch the tap will sound. Decimal values will result in microtonal shifts</param>
    /// <param name="velocity">The Midi velocity of the drone</param>
    /// <param name="instrument">The desired event to make a droneSound</param>
    /// <param name="maxDuration">The maximum duration of the drone in seconds</param>
    public void beginDrone(float rawPitch, int velocity, AK.Wwise.Event instrument, float droneVib, float droneSpeed, float droneFb, float droneHarm, float maxDuration, double currentTime)
    {
        int pitch = Mathf.FloorToInt(rawPitch);

            if (voiceHandler != null)
            {
                StopCoroutine(voiceHandler);
            };
            busy = true;
            AkSoundEngine.SetRTPCValue("Drone_Vibrato", droneVib, gameObject);
            AkSoundEngine.SetRTPCValue("Drone_Speed", droneSpeed, gameObject);
            AkSoundEngine.SetRTPCValue("Drone_Feedback", droneFb, gameObject);
            AkSoundEngine.SetRTPCValue("Drone_Harmonizer", droneHarm, gameObject);
            duration = maxDuration;
            timeToDie = (float)currentTime + maxDuration;
            float droneBend = (rawPitch - pitch) * 100f;

            AkSoundEngine.SetRTPCValue("Pitch_Bend", droneBend, gameObject);

            dronePitch = (int)pitch;
            droneInstrument = instrument;
        print("drone instrument: " + instrument);
            instrument.PostMIDI(gameObject, makeNoteOn((int)pitch, velocity));
            isDroning = true;


            //Event message

            Dictionary<string, object> droneDict = new Dictionary<string, object>();

            droneDict.Add("pitch", rawPitch);
            droneDict.Add("velocity", velocity);
            droneDict.Add("vibrato", droneVib);
            droneDict.Add("speed", droneSpeed);
            droneDict.Add("feedback", droneFb);
            droneDict.Add("harmonizer", droneHarm);
            droneDict.Add("instrument", instrument.Name);
            droneDict.Add("id", starID);
            droneDict.Add("event", "on");
            droneDict.Add("type", "drone");
            messageName = "drone_" + starID;

            remixReplay.addReplayEvent(droneDict);




            if (audioInfo.ContainsKey(messageName))
            {
                audioInfo.Remove(messageName);
            }

            audioInfo.Add(messageName, droneDict);
            sonificationEvent.Invoke(messageName, droneDict);

    }
    /// <summary>
    /// Ends the drone.  Triggered on activatedStart OnDestroy()
    /// </summary>
    public void endDrone()
    {

        if (isDroning)
        {
            Debug.Log("Sonifier: Ending Drone");
            droneInstrument.PostMIDI(gameObject, makeNoteOff(dronePitch));
            isDroning = false;
            voiceHandler = voiceAvalibleTimer(voiceReleaseWindow+3);
            StartCoroutine(voiceHandler);
            Dictionary<string, object> droneDict = new Dictionary<string, object>();
            droneDict.Add("id", starID);
            droneDict.Add("event", "off");
            droneDict.Add("type", "drone");
            remixReplay.addReplayEvent(droneDict);
        }
        else
        {
            Debug.LogError("Sonifier: drones is not droneing and cannot be turned off!");
        }

    }
    /// <summary>
    /// Sets the position of the parent object (better than using get component on each call) 
    /// </summary>
    /// <param name="newPosition"></param>
    public void setPosition(Vector3 newPosition)
    {
        transform.position = newPosition;
    }
    /// <summary>
    /// Sets the linked star to another game object and sets a flag
    /// </summary>
    /// <param name="starToLink"></param>
    public void linkPosition(GameObject starToLink)
    {
        linkedToStar = true;
        linkedStar = starToLink;
    }
    /// <summary>
    /// Releases the link created by linkPosition
    /// </summary>
    public void unlinkPosition()
    {
        linkedToStar = false;
        linkedStar = null;
    }


    /// <summary>
    /// Begins the wave event loop and also triggers the waves impulse sound.
    /// </summary>
    /// <param name="map">The array the wave will lookup from</param>
    /// <param name="wavePosition">The initial positino of the wave</param>
    /// <param name="fieldIndex">The field the wave will read from</param>
    /// <param name="waveMapper">the dataMapper that the waves values will be mapped from</param>
    /// <param name="scaleMap">the pitch scale the pitches generated from the mapping will be quanized to</param>
    /// <param name="waveImpulse">The Event of the wave impulse</param>
    /// <param name="waveInstrument">The Event of the wave instrument</param>
    /// <param name="waveDuration">The total duration of the wave event</param>
    public void doWave(float[,] map, Vector3 wavePosition, int fieldIndex, DataMapper waveMapper, ScaleMapper scaleMap, AK.Wwise.Event waveImpulse, AK.Wwise.Event waveInstrument, float waveDuration, float waveIntensity, float waveGranularityMs, int waveType)
    {
        print("doing wave...");
        
        AkSoundEngine.SetRTPCValue("Wave_Intensity", waveIntensity * 0.5f, gameObject);
        waveImpulse.Post(gameObject);

        busy = true;
        isWaving = true;

        waveMap = map;
        float[] waveOrgin = new float[2];
        waveOrgin[0] = wavePosition[0] + 3;
        waveOrgin[1] = wavePosition[1] + 3;
        int waveID = Mathf.FloorToInt((float)networkTime * 1000 + wavePosition.x + wavePosition.z * 100);
        IEnumerator waveHandle = waveStep(0, waveDuration, waveGranularityMs * 0.001f, waveOrgin, fieldIndex, waveMapper, scaleMap, waveInstrument, waveID); ;
        StartCoroutine(waveHandle);
        Dictionary<string, object> waveDict = new Dictionary<string, object>();
        waveDict.Add("event", "on");
        waveDict.Add("type", "wave");
        waveDict.Add("orginPosition", new float[]{wavePosition.x,wavePosition.z});
        waveDict.Add("intensity", waveIntensity);
        waveDict.Add("waveType", waveType);
        waveDict.Add("waveImpulse", waveImpulse.Name);
        waveDict.Add("id", waveID);
        remixReplay.addReplayEvent(waveDict);


    }


   
        #endregion

    #region PRIVATE_FUNC

        /// <summary>
        /// Waits for a delay in seconds before releasing a voice 
        /// </summary>
        /// <param name="delay">time in seconds</param>
        /// <returns></returns>
    IEnumerator voiceAvalibleTimer(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (busy)
        {         
            busy = false;
            panic();
            if (debugState)
            {
                AkSoundEngine.PostEvent("VoiceOff", gameObject);
            }

            audioInfo.Remove(messageName);
            audioInfo.Remove(envDictName);
            messageName = "";
            envDictName = "";
        }
    }
    /// <summary>
    /// Schedules a new one in a sequence 
    /// </summary>
    /// <param name="delay">the amount of time in seconds to wait before playing the new note</param>
    /// <param name="counter">the current note in the sequence-- used to calculate the positions in the pitch and time arrays</param>
    /// <param name="instrument">the wwise event to send the note message to</param>
    /// <returns></returns>
    IEnumerator sequenceEvent(float delay, int counter, AK.Wwise.Event instrument)
    {
        float rawPitch = pitchSequence[(pitchSequence.Length - 1) - (counter % pitchSequence.Length)];
        int pitch = Mathf.FloorToInt(rawPitch); //Images seem to be backwars from the sequences- so I reversed them
        float pitchOffset = rawPitch - pitch;
        AkSoundEngine.SetRTPCValue("Pitch_Bend", pitchOffset, gameObject);
        int velocity = UnityEngine.Random.Range(0, 60) + 60; //random velocity
        AkMIDIPostArray MIDIPostArrayBuffer = makeNote(pitch, velocity, 0.5f);
        instrument.PostMIDI(gameObject, MIDIPostArrayBuffer);  //Send it to the midi event

        if (delay < 0.1) delay = 0.1f; //This makes sure sequences do not go out of control 
        yield return new WaitForSeconds(delay);
        float delayNext = delay;
        if (isSequencing)
        {
            //Pitch sequence and time sequence are global to this script
            float starTimer = (float)networkTime;
            float timeRemaining = timeToDie - starTimer;
            if (timeRemaining > 20f)
            {
                delayNext = timeSequence[counter % timeSequence.Length] * seqPeriod;
            }
            else if (timeRemaining > 0)
            {
                delayNext = timeSequence[counter % timeSequence.Length] * (seqPeriod*(1-((timeRemaining) / 20))+terminalPeriod*((timeRemaining) / 20));
            }
            counter++;

            StartCoroutine(sequenceEvent(delayNext, counter, instrument));            


            //For the event system
            Dictionary<string, object> seqDict = new Dictionary<string, object>();

            seqDict.Add("pitch", pitch);
            seqDict.Add("velocity", velocity);
            seqDict.Add("instrument", instrument.Name);
            seqDict.Add("id", starID);
            seqDict.Add("event", "continue");
            seqDict.Add("type", "sequence");
            remixReplay.addReplayEvent(seqDict);

            messageName = "sequence_" + starID;
            if (audioInfo.ContainsKey(messageName))
            {
                audioInfo.Remove(messageName);
            }

            audioInfo.Add(messageName, seqDict);
            sonificationEvent.Invoke(messageName, seqDict);
            
        }

    }

    void updateSequenceEvent()
    {
        float starTimer = (float)networkTime;
        float timeRemaining = timeToDie - starTimer;
        bool triggered = false;

       
        
        if (isSequencing)
        {
            int timeCounter;
            seqPosition = (seqPosition + Time.deltaTime * seqTimeMultiplier);
            float seqProgress = (seqPosition/(seqPeriod * seqTimeSum)) % 1f;

            for (int i = 0; i < timeSequence.Length; i++)
            {
                if (i < timeSequence.Length - 1)
                {
                    if (seqProgress > normalizedTimeArray[i] && seqProgress < normalizedTimeArray[i + 1] && !seqTriggeredArray[i])
                    {
                        timeCounter = i;
                        triggered = true;
                        for (int n = 0; n < timeSequence.Length; n++)
                        {
                            seqTriggeredArray[n] = false;
                        }

                        seqTriggeredArray[i] = true;

                    }
                }
                else
                {
                    if (seqProgress < normalizedTimeArray[0] && !seqTriggeredArray[i])
                    {
                        timeCounter = i;
                        triggered = true;
                        for (int n = 0; n < timeSequence.Length; n++)
                        {
                            seqTriggeredArray[n] = false;
                        }

                        seqTriggeredArray[i] = true;
                    }
                }
            }

            if (triggered)
            {
                //print("Position in sequence: " + seqPosition + " Seq Period " + (seqPeriod * seqTimeSum));
                float rawPitch = pitchSequence[(pitchSequence.Length - 1) - (seqPitchCounter % pitchSequence.Length)];
                seqPitchCounter++;
                int pitch = Mathf.FloorToInt(rawPitch); //Images seem to be backwars from the sequences- so I reversed them
                float pitchOffset = rawPitch - pitch;
                AkSoundEngine.SetRTPCValue("Pitch_Bend", pitchOffset, gameObject);
                int velocity = Mathf.RoundToInt(UnityEngine.Random.Range(49, 120)); //random velocity
                AkMIDIPostArray MIDIPostArrayBuffer = makeNote(pitch, velocity, 0.5f);

                //Pitch sequence and time sequence are global to this script


                if (timeRemaining > 10f)
                {
                    // Do not stretch or compress time
                    seqTimeMultiplier = 1;
                    sequenceInstrument.PostMIDI(gameObject, MIDIPostArrayBuffer);  //Send it to the midi event

                }
                else if (timeRemaining > 0)
                {
                    // stretch or compress time 
                    seqTimeMultiplier = terminalPeriod / seqPeriod * (timeRemaining / 10f) + (1-timeRemaining / 10f);
                    sequenceInstrument.PostMIDI(gameObject, MIDIPostArrayBuffer);  //Send it to the midi event
                }
                else
                {
                    seqTimeMultiplier = 1;
                }







                //For the event system
                Dictionary<string, object> seqDict = new Dictionary<string, object>();

                seqDict.Add("pitch", rawPitch);
                seqDict.Add("velocity", velocity);
                seqDict.Add("instrument", sequenceInstrument.Name);
                seqDict.Add("id", starID);
                seqDict.Add("event", "continue");
                seqDict.Add("type", "sequence");
                remixReplay.addReplayEvent(seqDict);
                messageName = "sequence_" + starID;
                if (audioInfo.ContainsKey(messageName))
                {
                    audioInfo.Remove(messageName);
                }

                audioInfo.Add(messageName, seqDict);
                sonificationEvent.Invoke(messageName, seqDict);
            }
        }
        
    }

    /// <summary>
    /// Returns a AkMIDIPostArray containing a note on and off message
    /// </summary>
    /// <param name="pitch">Midi pitch</param>
    /// <param name="velocity">Midi velocity</param>
    /// <param name="duration">length of the note in seconds</param>
    /// <returns></returns>
    AkMIDIPostArray makeNote(int pitch, int velocity, float duration)
    {
        AkMIDIPostArray MIDIPostArrayBuffer = new AkMIDIPostArray(2);  // has both a note on and note off message

        AkMIDIPost midiEvent = new AkMIDIPost(); //The midi event

        midiEvent.byType = AkMIDIEventTypes.NOTE_ON;  //The type of midi event

        midiEvent.byChan = 0;

        midiEvent.byOnOffNote = (byte)pitch;

        midiEvent.byVelocity = (byte)velocity;

        midiEvent.uOffset = 0;  //You can apply a time offset to the evetn (useful for note offs) 

        MIDIPostArrayBuffer[0] = midiEvent;  //Add the event to the event array

        midiEvent.byType = AkMIDIEventTypes.NOTE_ON;
        midiEvent.byVelocity = 0;
        byte durationInSamples = (byte)(duration * 48000);
        midiEvent.uOffset = durationInSamples; //in samples 
        MIDIPostArrayBuffer[1] = midiEvent;
        return MIDIPostArrayBuffer;
    }
    /// <summary>
    /// Generates a midi note with a delay
    /// </summary>
    /// <param name="pitch">MIDI pitch</param>
    /// <param name="velocity">MIDI velocity</param>
    /// <param name="duration">Duration of the note in seconds</param>
    /// <param name="delay">Length of the delay in dseconds</param>
    /// <returns></returns>
    AkMIDIPostArray makeNote(int pitch, int velocity, float duration, float delay)
    {
        AkMIDIPostArray MIDIPostArrayBuffer = new AkMIDIPostArray(2);  // has both a note on and note off message

        AkMIDIPost midiEvent = new AkMIDIPost(); //The midi event

        midiEvent.byType = AkMIDIEventTypes.NOTE_ON;  //The type of midi event

        midiEvent.byChan = 0;

        midiEvent.byOnOffNote = (byte)pitch;

        midiEvent.byVelocity = (byte)velocity;

        midiEvent.uOffset = (byte)(delay * 48000);  //You can apply a time offset to the evetn (useful for note offs) 

        MIDIPostArrayBuffer[0] = midiEvent;  //Add the event to the event array

        AkMIDIPost midiEvent2 = new AkMIDIPost(); //The midi event
        midiEvent2.byType = AkMIDIEventTypes.NOTE_ON;
        midiEvent2.byVelocity = 0;
        byte durationInSamples = (byte)((duration + delay) * 48000);
        midiEvent2.uOffset = durationInSamples; //in samples 
        MIDIPostArrayBuffer[1] = midiEvent2;


        return MIDIPostArrayBuffer;
    }
    /// <summary>
    /// Returns a AkMIDIPostArray containing a note on message
    /// </summary>
    /// <param name="pitch">Midi Pitch</param>
    /// <param name="velocity">Midi Velocity</param>
    /// <returns></returns>
    AkMIDIPostArray makeNoteOn(int pitch, int velocity)
    {
        AkMIDIPostArray MIDIPostArrayBuffer = new AkMIDIPostArray(1);  // has both a note on and note off message

        AkMIDIPost midiEvent = new AkMIDIPost(); //The midi event

        midiEvent.byType = AkMIDIEventTypes.NOTE_ON;  //The type of midi event

        midiEvent.byChan = 0;

        midiEvent.byOnOffNote = (byte)pitch;

        midiEvent.byVelocity = (byte)velocity;

        midiEvent.uOffset = 0;  //You can apply a time offset to the evetn (useful for note offs) 

        MIDIPostArrayBuffer[0] = midiEvent;  //Add the event to the event array

        return MIDIPostArrayBuffer;
    }
    /// <summary>
    /// Returns a AkMIDIPostArray containing a note off message
    /// </summary>
    /// <param name="pitch">midi pitch</param>
    /// <returns></returns>
    AkMIDIPostArray makeNoteOff(int pitch)
    {
        AkMIDIPostArray MIDIPostArrayBuffer = new AkMIDIPostArray(1);  // has both a note on and note off message

        AkMIDIPost midiEvent = new AkMIDIPost(); //The midi event

        midiEvent.byType = AkMIDIEventTypes.NOTE_OFF;  //The type of midi event

        midiEvent.byChan = 0;

        midiEvent.byOnOffNote = (byte)pitch;

        midiEvent.byVelocity = 0;

        midiEvent.uOffset = 0;  //You can apply a time offset to the evetn (useful for note offs) 

        MIDIPostArrayBuffer[0] = midiEvent;  //Add the event to the event array

        return MIDIPostArrayBuffer;
    }

    /// <summary>
    /// This is a recursive function that sends midi messages to wwise to create the wave.  Timings are done using WaitForSeconds coroutine in Unity. 
    /// </summary>
    /// <param name="waveProgress">The time the wave has progressed</param>
    /// <param name="waveDuration">The total durtion fo the wave</param>
    /// <param name="waveGranularity">The delay between each step in the wave</param>
    /// <param name="startPosition">The starting position (x,y) of the wave </param>
    /// <param name="fieldIndex">The field the wave should read from</param>
    /// <param name="waveScale">The scalar for coverting the waves data to pitches</param>
    /// <param name="pitchScale">The pitch scale the wave quantizes to pitches</param>
    /// <param name="waveInstrument">The index of the waves instrument</param>
    /// <returns></returns>

    IEnumerator waveStep(double waveProgress, float waveDuration, float waveGranularity, float[] startPosition, int fieldIndex, DataMapper waveScale, ScaleMapper pitchScale, AK.Wwise.Event waveInstrument, int waveID)
    {
        float thisGrain = (UnityEngine.Random.Range(-0.25f, 0.25f) + 1) * waveGranularity;
        waveProgress += waveGranularity; //Adds the amount of delay to the the progress of the wave
        float startVelocity = 80; // Starting velocity of the wave
        float endVelociy = 1; // ending velocity of the wave
        float linePosition = (float)waveProgress / waveDuration; //from 0-1 based on the waves progress in terms of duration
        int velocity = Mathf.FloorToInt(startVelocity * (1 - linePosition) + endVelociy * linePosition); //Velocity lerped from the start to the end based on the linePosition

        float[] lineDuration = new float[4]; //Holds the distance from the start point to the extrema

        lineDuration[0] = Mathf.Abs(startPosition[0] - 100) / 200 * waveDuration; //-X
        lineDuration[1] = Mathf.Abs(startPosition[0] + 100) / 200 * waveDuration; //+X
        lineDuration[2] = Mathf.Abs(startPosition[1] - 50) / 100 * waveDuration; //-Y
        lineDuration[3] = Mathf.Abs(startPosition[1] + 50) / 100 * waveDuration;// +Y



        float[] mapStep = new float[4]; //Holds the position of the line in all four directions 

        mapStep[0] = Mathf.Clamp01(linePosition * (lineDuration[0] / waveDuration)) * (-100 - startPosition[0]) + startPosition[0] + 100;
        mapStep[1] = Mathf.Clamp01(linePosition * (lineDuration[0] / waveDuration)) * (100 - startPosition[0]) + startPosition[0] + 100;
        mapStep[2] = Mathf.Clamp01(linePosition * (lineDuration[0] / waveDuration)) * (-50 - startPosition[0]) + startPosition[1] + 50;
        mapStep[3] = Mathf.Clamp01(linePosition * (lineDuration[0] / waveDuration)) * (50 - startPosition[0]) + startPosition[1] + 50;

        float[] waveValue = new float[4];  //Hold the value mapped from the mapStep

        int lookupLine;
        lookupLine = Mathf.FloorToInt(mapStep[0] * 100 + mapStep[2]);
        waveValue[0] = waveMap[lookupLine, fieldIndex];

        lookupLine = Mathf.FloorToInt(mapStep[1] * 100 + mapStep[2]);
        waveValue[1] = waveMap[lookupLine, fieldIndex];

        lookupLine = Mathf.FloorToInt(mapStep[0] * 100 + mapStep[3]);
        waveValue[2] = waveMap[lookupLine, fieldIndex];

        lookupLine = Mathf.FloorToInt(mapStep[1] * 100 + mapStep[3]);
        waveValue[3] = waveMap[lookupLine, fieldIndex];

        int selectedWave = Mathf.RoundToInt(UnityEngine.Random.Range(-0.4f, 3));

        float value = waveValue[selectedWave];
        if (value > 0)
        {
            //Sends all non-zero values to wwise
            float thisValue = waveScale.map(value);

            float wavePitch = pitchScale.map(thisValue);
            if (isWaving)
            {
                int midiPitch = Mathf.FloorToInt(wavePitch);
                AkMIDIPostArray midi = makeNoteOn(midiPitch, 100);
                AkSoundEngine.SetRTPCValue("Pitch_Bend", wavePitch - midiPitch, gameObject);
                AkSoundEngine.SetRTPCValue("WaveVolume", velocity, gameObject);
                AkSoundEngine.SetRTPCValue("WaveDelayMix", 127-velocity, gameObject);
                waveInstrument.PostMIDI(gameObject, midi);
                /*
                Dictionary<string, object> waveDict = new Dictionary<string, object>();
                waveDict.Add("event", "continue");
                waveDict.Add("type", "wave");
                waveDict.Add("pitch", wavePitch);
                waveDict.Add("velocity", velocity);
                waveDict.Add("value", value);
                waveDict.Add("instrument", waveInstrument.Name);
                waveDict.Add("id", waveID);
                remixReplay.addReplayEvent(waveDict);
                */


            }
        }


        if (isWaving)
        {
            //Check if the wave has benn stopped manually 
            if (waveProgress <= waveDuration) isWaving = true;
            else isWaving = false;

        }

        if (isWaving)
        {
            //if it is still going, go again!
            yield return new WaitForSeconds(thisGrain);
            IEnumerator waveHandle = waveStep(waveProgress, waveDuration, waveGranularity, startPosition, fieldIndex, waveScale, pitchScale, waveInstrument, waveID);
            StartCoroutine(waveHandle);
        }
        else
        {
            //waveSounds[waveInstrument].StopMIDI(gameObject);

            voiceHandler = voiceAvalibleTimer(voiceReleaseWindow);
            Dictionary<string, object> waveDict = new Dictionary<string, object>();
            waveDict.Add("event", "off");
            waveDict.Add("type", "wave");
            waveDict.Add("id",  waveID);
            remixReplay.addReplayEvent(waveDict);
        }


    }


    public void panic()
    {
        AkSoundEngine.StopAll(gameObject);
        isDroning = false;
        isSequencing = false;
        isWaving = false;
        busy = false;
    }
    #endregion


}
