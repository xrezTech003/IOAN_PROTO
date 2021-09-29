using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
<summary>
	CD : cameraFacer
	Makes gameObject rotate towards Camera.main
</summary>
**/
public class cameraFacer : MonoBehaviour 
{
    public bool stayBelow;
    public bool fullFace;
    public bool faceAway;

    Config config;
    private Camera cam;

    private void Start()
    {
        config = Config.Instance;
        cam = Camera.main;
    }

    /**
	<summary>
		FD : LateUpdate()
        Only activates on non-server machines
		Makes gameObject look at main camera without modifying the Y value
		Rotates gameObject Y value by 180 degrees
	</summary>
	**/
    void LateUpdate ()
    {
        if (config.Data.serverStatus != "server")
        {
            if (fullFace) transform.LookAt(cam.transform.position);
            else transform.LookAt(new Vector3(cam.transform.position.x, transform.position.y, cam.transform.position.z));

            if (!faceAway) transform.Rotate(Vector3.up * 180f);
            if (stayBelow) transform.position = transform.parent.position + Vector3.down * 0.044f;
        }
    }

    public void LookAtLevelEntity(Transform t)
    {
        transform.LookAt(new Vector3(t.position.x, transform.position.y, t.position.z));
        transform.Rotate(Vector3.up * 180f);
    }
}
