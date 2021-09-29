using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ovrAvatarKiller : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (OVRPlugin.GetSystemHeadsetType() != 0) gameObject.SetActive(true);
        else gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
