using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class IOANEnvelopeViewer : MonoBehaviour
{
    public bool enabled = false;
    public List<EnvelopeViewer> envelopeViewers;
    
    IOANVoiceManager voiceManager;
    // Start is called before the first frame update
    void Start()
    {
        voiceManager = GetComponent<IOANVoiceManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (enabled) { 
        Dictionary<string,Dictionary<string, object>> audioInfo = voiceManager.audioInfo;
        envelopeViewers.Clear();
        List<string> keyList = new List<string>(audioInfo.Keys);

        foreach (string key in keyList)
        {
            if (key.Contains("env_"))
            {
                EnvelopeViewer env = new EnvelopeViewer();

                Dictionary<string, object> audioEntry = audioInfo[key];

                List<string> entryKeyList = new List<string>(audioEntry.Keys);
                foreach (string subkey in entryKeyList)
                {
                    if (subkey == "envelope")
                    {
                        env.envelope = (float)audioEntry[subkey];
                    }
                    if (subkey == "envelope_high")
                    {
                        env.high = (float)audioEntry[subkey];
                    }
                    if (subkey == "envelope_mid")
                    {
                        env.mid = (float)audioEntry[subkey];
                    }
                    if (subkey == "envelope_low")
                    {
                        env.low = (float)audioEntry[subkey];
                    }
                }
                    env.envName = key;
                envelopeViewers.Add(env);
            }
            }


        }

    }



}

[Serializable]
public class EnvelopeViewer
{
    public string envName;
    [Space(10)]
    [Range(-48,12)]
    public float envelope;
    [Header("Banded")]
    [Range(-48, 12)]
    public float high;
    [Range(-48, 12)]
    public float mid;
    [Range(-48, 12)]
    public float low;
}
