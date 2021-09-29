using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
<summary>
    CD : dispScript
    Activates all displays on Startup I_A ::: Better than the other dpislay one NDH
**/
public class dispScript : MonoBehaviour 
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
        // Display.displays[0] is the primary, default display and is always ON.
        // Check if additional displays are available and activate each.

        if (Display.displays.Length > 1) Display.displays[1].Activate();
        if (Display.displays.Length > 2) Display.displays[2].Activate();
		if (Display.displays.Length > 3) Display.displays[3].Activate();
    }
}
