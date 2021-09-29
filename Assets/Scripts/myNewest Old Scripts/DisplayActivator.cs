using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
<summary>
    CD : DisplayActivator
    Activates all Displays on startup I_A ::: IV: This script makes no sense, what Displays? Not declared or referencd in game - maybe garbage NDH
</summary>
**/
public class DisplayActivator : MonoBehaviour 
{
    /**
    <summary>
        FD : Start()
        Activates all displays
    </summary>
    **/
    void Start()
    {
        Debug.Log("Displays connected: " + Display.displays.Length);

        for(int i = 0; i < Display.displays.Length; i++) Display.displays[i].Activate();
    }
}
