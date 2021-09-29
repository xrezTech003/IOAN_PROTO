using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// CD : Manager for the pulse spheres
/// </summary>
public class PulseManager : MonoBehaviour
{
    //Delegate function
    private delegate void UpdateEvent();
    private UpdateEvent update;

    #region PUBLIC_VAR
    /// <summary>
    /// VD : inner 3D sphere object
    /// </summary>
    public GameObject inner;

    /// <summary>
    /// VD : outer sprite halo object
    /// </summary>
    public GameObject outer;
    #endregion

    #region PRIVATE_VAR
    /// <summary>
    /// VD : Cluster transform
    /// </summary>
    private Transform cluster;

    /// <summary>
    /// VD : earth transform
    /// </summary>
    private Transform earth;

    /// <summary>
    /// VD : Vertex position pulse spawns from
    /// </summary>
    private Vector3 vertex;

    /// <summary>
    /// VD : Target transform, either cluster or earth
    /// </summary>
    private Transform target;

    /// <summary>
    /// VD : VFX
    /// </summary>
    private VisualEffect visEffect;

    /// <summary>
    /// VD : relative displacement from spawn to endpoint
    /// </summary>
    private float moveDist = 0f;

    /// <summary>
    /// VD : endpoint of pulse
    /// </summary>
    private Vector3 farPos;

    /// <summary>
    /// VD : Moving to the earth or the cluster
    /// </summary>
    private bool top = true;

    /// <summary>
    /// VD : mathematical constant e
    /// </summary>
    private readonly float e = 2.718281828459045f;

    /// <summary>
    /// VD : scale at spawn
    /// </summary>
    private Vector3 smallScale;

    /// <summary>
    /// VD : Scale at endpoint
    /// </summary>
    private Vector3 largeScale;
    #endregion

    #region UNITY_FUNC
    /// <summary>
    ///     Call update if it isn't null
    /// </summary>
    private void Update()
    {
        update?.Invoke();

        transform.LookAt(Camera.main.transform);
    }
    #endregion

    #region PUBLIC_FUNC
    /// <summary>
    /// FD : Init func for spawning a double pulse to go from middle -> top/bottom
    /// </summary>
    /// <param name="t"></param>
    /// <param name="v"></param>
    /// <param name="col"></param>
    /// <param name="larger"></param>
    public void Init(Transform t, Vector3 v, Color col, bool larger)
    {
        target = t;
        vertex = v;

        top = (target.position.y < 0f) ? false : true;

        Color newCol = col;
        newCol.a /= 3.0f;
        inner.GetComponent<MeshRenderer>().material.SetColor("_Color", newCol);
        col.a *= 0.5f;
        outer.GetComponent<SpriteRenderer>().material.SetColor("_Color", col);

        smallScale = Vector3.one * .35f;
        largeScale = Vector3.one * 2.8f; //HDH - 11/19 - Dr. West wanted smaller stars 
        if (larger)
        {
            smallScale *= 2f;
            largeScale *= 2f;
        }

        update += Animate;
    }
    #endregion

    #region PRIVATE_FUNC
    /// <summary>
    /// FD : Animate func for having the pulses travel away from the center
    /// </summary>
    private void Animate()
    {
        float mod = (top) ? 25f : 200f;
        float lerpPerc = (Mathf.Pow(e, moveDist) - 1f) / mod;

        transform.position = Vector3.Lerp(vertex, target.position, lerpPerc);
        transform.localScale = Vector3.Lerp(smallScale, largeScale, lerpPerc / 3.0f);

        moveDist += 0.002f;

        if (lerpPerc >= 1f)
            Destroy(gameObject);
    }
    #endregion
}