using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
/**
<summary>
	CD : SensorCam
	// this script assumes an orthographic camera is attached to the controller
	// the camera's near and far clipping panes are sized to fit the controller perfectly
	// it is sized and positioned to extend out below the controller (to make sure it hits the mesh that is generating the geomtery)
	// it renders using a shader that encodes object ids as colors
	// todo: copy all the parameters from regular shader to sensor shader
</summary>
**/
public class SensorCamRight : SensorCam
{
}