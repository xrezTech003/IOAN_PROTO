using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraModifyer : MonoBehaviour
{
    private delegate void UpdateEvent();
    private UpdateEvent update;

    private void Start()
    {
        update = () =>
        {
            Transform tran = transform.Find("Camera (eye)");
            if (tran == null) return;

            Camera cam = tran.GetComponent<Camera>();
            if (cam == null) return;

            cam.cullingMask ^= (1 << LayerMask.NameToLayer("newCloth"));
            update = null;
        };
    }

    private void Update()
    {
        update?.Invoke();
    }
}
