using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;
using System.Linq;
using ioanDataMapping;
/// <summary>
/// Class for animating activated stars based on audio data
/// </summary>
public class AudioReactiveStar : MonoBehaviour
{
    /// <summary>
    /// Star ID to filter events 
    /// </summary>
    public int starID;
    /// <summary>
    /// template for animating star sequences
    /// </summary>
    public ioanAudioReaciveAnimate sequenceAnimationTemplate;
    /// <summary>
    /// List of active animations 
    /// </summary>
    List<ioanAudioReaciveAnimate> sequenceAnimations = new List<ioanAudioReaciveAnimate>();
    /// <summary>
    /// template for animating drones
    /// </summary>
    public ioanAudioReaciveAnimate droneAnimationTemplate;
    /// <summary>
    /// List of active drone animations 
    /// </summary>
    List<ioanAudioReaciveAnimate> droneAnimations = new List<ioanAudioReaciveAnimate>();
    /// <summary>
    /// Refrence to the ioan voice manager
    /// </summary>
    IOANVoiceManager voiceManager;
    /// <summary>
    /// drone evelope (note used yet)
    /// </summary>
    public float envelope;
    /// <summary>
    /// Event signifying sonification
    /// </summary>
    public SonificationEvent sonificationEvent; //This class is defined in IOAN voice manager and is also instantiated there
    /// <summary>
    /// Link to the shade setter script on the same gameObject
    /// </summary>
    ShaderSetter shadeSetter;
    /// <summary>
    /// Data mapper for pulse amplitude
    /// </summary>
    public DataMapper seqVelocityToPulseAmp;
    /// <summary>
    /// Data mapper for pulse amplitude
    /// </summary>
    public DataMapper droneAmpToPulseAmp;
    /// <summary>
    /// Stores the event type
    /// </summary>
    string eventType;
    // Start is called before the first frame update
    float droneModifier = 0;
    void Start()
    {
        voiceManager = FindObjectOfType<IOANVoiceManager>();
        sonificationEvent = voiceManager.sonificationEvent;
        sonificationEvent.AddListener(triggeredSoundEvent);
        shadeSetter = GetComponent<ShaderSetter>();
        starID = shadeSetter.starID;
    }

   /// <summary>
   /// Used for drone enevopes 
   /// </summary>
    void Update()
    {
        float sequenceModifier = 0; //clear modifier
        int movingAvgCount = 0;
        int movingAvgLength = 30; //30 frames
        float movingAverage = 0;
        float droneScalar = 1.25f;
       
        
        //drone audio react
        if (eventType == "drone")
        {
            Dictionary<string, Dictionary<string, object>> audioInfo = voiceManager.audioInfo;
            List<string> keyList = new List<string>(audioInfo.Keys);

            foreach (string key in keyList)
            {
                
                if (key == ("env_" + starID))
                {

                    Dictionary<string, object> audioEntry = audioInfo[key];

                    List<string> entryKeyList = new List<string>(audioEntry.Keys);
                    foreach (string subkey in entryKeyList)
                    {
                        if (subkey == "envelope")
                        {
                            envelope = (float)audioEntry[subkey];

                            //simple remap from -20 to -10 db to 0 - 1
                            envelope = envelope * -0.1f;
                            
                            //calculate moving average
                            movingAvgCount++;
                            if (movingAvgCount > movingAvgLength)
                            {
                                movingAverage = movingAverage + (envelope - movingAverage) / (movingAvgLength + 1);
                            } 
                            else
                            {
                                movingAverage += envelope;
                                if (movingAvgCount == movingAvgLength)
                                {
                                    movingAverage += movingAverage / movingAvgCount;
                                }
                            }

                            //droneModifier = movingAverage * droneScalar; //commenting this out for now.

                            //debug
                            //print("droneModifier: " + droneModifier);

                        }

                    }

                 

                }
            }
            
            


        }

        
        foreach (ioanAudioReaciveAnimate animation in sequenceAnimations)
        {
            sequenceModifier += animation.animate(Time.deltaTime);
            if (animation.done)
            {
                sequenceAnimations.Remove(animation);
            }
        }
        shadeSetter.interiorMeshScaleMod = sequenceModifier*0.25f;
        shadeSetter.pokeMod = -sequenceModifier*0.5f;
        shadeSetter.speedMod = sequenceModifier * 0.25f;
        
        foreach (ioanAudioReaciveAnimate animation in droneAnimations)
        {
            droneModifier += animation.animate(Time.deltaTime);
            droneModifier = droneAmpToPulseAmp.map(droneModifier);
            if (animation.done)
            {
                droneAnimations.Remove(animation);
            }
        }
        shadeSetter.speedMod = droneModifier * 0.05f;
        shadeSetter.dripMod = droneModifier*0.01f;
        


   
        
        
    }
    /// <summary>
    /// Triggered sounds cause their events- right now only sequences are supported
    /// </summary>
    /// <param name="name"></param>
    /// <param name="soundDict"></param>
    void triggeredSoundEvent(string name, Dictionary<string, object> soundDict)
    {
        
        if (name.Contains(starID.ToString()))
        {
            eventType = name.Split('_')[0];
            if (eventType == "sequence" || eventType == "tap") //sequences and taps
            {
                
                float velocity = (int)soundDict["velocity"];
                float pitch = (float)soundDict["pitch"];

                float pulseAmp = seqVelocityToPulseAmp.map(velocity);
                ioanAudioReaciveAnimate animation = sequenceAnimationTemplate.clone();
                sequenceAnimations.Add(animation);
                //Do things with the events here 
                //shadeSetter.TriggerPulse(pulseAmp, 0.1f * fps);



            }

            else if (eventType == "drone")
            {
                foreach (string key in soundDict.Keys)
                {
                    //Do things with the events here 
                    ioanAudioReaciveAnimate animation = sequenceAnimationTemplate.clone();
                    droneAnimations.Add(animation);
                    
                }
            }
        }
    }

}
/// <summary>
/// Class for tuning animations in activated stars 
/// </summary>
[Serializable]
public class ioanAudioReaciveAnimate
{
    float currentValue = 0;
    public Vector2 valueRange = new Vector2(0, 1);
    public float animateInTime = 0.25f;
    public float animateOutTime = 0.25f;
    public AnimationCurve animateIn;
    public AnimationCurve animateOut;
    float tracker = 0;
    bool isAnimating = false;
    public bool done = false;
    public float animate(float deltaTime)
    {
        if (!isAnimating && ! done)
        {
            isAnimating = true;
            tracker = 0;
        }
        tracker += deltaTime;
        tracker = Mathf.Clamp(tracker, 0, animateInTime + animateOutTime);

        


        if (tracker == animateInTime + animateOutTime)
        {
            isAnimating = false;
            done = true;
            return currentValue;
        }

        if (tracker < animateInTime)
        {
            currentValue = animateIn.Evaluate(tracker / animateInTime)/(valueRange.magnitude)+valueRange.x;
        }
        else
        {
            currentValue = animateOut.Evaluate((tracker-animateInTime)/animateOutTime) / (valueRange.magnitude) + valueRange.x;
        }

        return currentValue;

    }
    /// <summary>
    /// Clones a template
    /// </summary>
    /// <returns></returns>
    public ioanAudioReaciveAnimate clone()
    {
        return (ioanAudioReaciveAnimate)this.MemberwiseClone();
    }


}