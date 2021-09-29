using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// CD : Script manager for the pulse/light lines
/// </summary>
public class LinesManager : MonoBehaviour
{
    #region PUBLIC_VAR
    /// <summary>
    /// VD : The transform of the earth/antartica
    /// </summary>
    public Transform earthAnchor;

    /// <summary>
    /// VD : the transform of the cluster object that is hanging in the air
    /// </summary>
    public Transform clusterAnchor;

    /// <summary>
    /// VD : Material of the lines
    /// </summary>
    public Material lineMat;

    /// <summary>
    /// VD : Starting width of the line renderers
    /// </summary>
    public float startWidth = .03333f;

    /// <summary>
    /// VD : Ending width of the line renderer
    /// </summary>
    public float endWidth = .03333f;

    /// <summary>
    /// VD : the bounding box of the "telescope fustrum"
    /// </summary>
    public Vector2[] boundsBox = { new Vector2(200f, -100f), new Vector2(200f, 100f),
                                   new Vector2(-200f, 100f), new Vector2(-200f, -100f) };

    /// <summary>
    /// VD : Prefab of the pulse objects
    /// </summary>
    public GameObject pulseObjectPrefab;
    #endregion

    #region PRIVATE_VAR
    /// <summary>
    /// VD : num of vertices in the bound "box"
    /// </summary>
    private int size;

    /// <summary>
    /// VD : the bound "box" in 3D space
    /// </summary>
    private Vector3[] bounds;

    /// <summary>
    /// VD : Parent Gameobject to hold the earth lines
    /// </summary>
    private GameObject earthLineHolder;

    /// <summary>
    /// VD : Parent Gameobject to hold the cluster lines
    /// </summary>
    private GameObject clusterLineHolder;

    /// <summary>
    /// Vd : Parent Gameobject to hold all pulses
    /// </summary>
    private GameObject pulseHolder;

    /// <summary>
    /// VD : All the earth lines
    /// </summary>
    private GameObject[] earthLines;

    /// <summary>
    /// VD : All the cluster lines
    /// </summary>
    private GameObject[] clusterLines;

    //Timers for Spawning pulses idly
    private float idleTimer = 0f;
    private readonly float maxTimer = 240f;
    private float idleCounter = 0f;
    private readonly float maxCounter = 120f;
    #endregion

    #region UNITY_FUNC
    /// <summary>
    /// FD : Spawn all lines and start to spawn pulses
    /// </summary>
    private void Start()
    {
        //Create Bounds
        size = boundsBox.Length;
        bounds = new Vector3[size];

        //Gather Holders
        earthLineHolder = transform.Find("EarthLines").gameObject;
        clusterLineHolder = transform.Find("ClusterLines").gameObject;
        pulseHolder = transform.Find("Pulses").gameObject;

        //Create Line Arrays
        earthLines = new GameObject[size];
        clusterLines = new GameObject[size];

        //Create Lines
        for (int i = 0; i < size; i++)
        {
            //Set Bounds
            bounds[i] = new Vector3(boundsBox[i].x, 0f, boundsBox[i].y);

            //Spawn Earth Lines
            earthLines[i] = new GameObject("EarthLine" + i);
            earthLines[i].transform.parent = earthLineHolder.transform;

            LineRenderer line = earthLines[i].AddComponent<LineRenderer>();
            Vector3[] positions = { earthAnchor.position, bounds[i] };

            line.SetPositions(positions);
            line.startWidth = startWidth;
            line.endWidth = endWidth;
            line.material = lineMat;

            //Spawn Cluster Lines
            clusterLines[i] = new GameObject("ClusterLine" + i);
            clusterLines[i].transform.parent = clusterLineHolder.transform;

            line = clusterLines[i].AddComponent<LineRenderer>();
            positions[0] = clusterAnchor.position;
            positions[1] = bounds[i];

            line.SetPositions(positions);
            line.startWidth = startWidth;
            line.endWidth = endWidth;
            line.material = lineMat;
        }
    }

    /// <summary>
    /// FD : Track anchors and idle timers
    /// </summary>
    private void Update()
    {
        TrackAnchors();

        if (idleTimer >= maxTimer)
        {
            if (idleCounter >= maxCounter)
            {
                SpawnPulses(Color.white);
                idleTimer = 0f;
            }
            else idleCounter++;
        }
        else idleTimer++;
    }
    #endregion

    #region PUBLIC_FUNC
    /// <summary>
    /// FD : Spawn the pulse on the lines
    /// </summary>
    /// <param name="col"></param>
    /// <param name="larger"></param>
    public void SpawnPulses(Color col, bool larger = false)
    {
        idleTimer = 20f;
        idleCounter = 0f;

        float hue, sat, val;
        Color.RGBToHSV(col, out hue, out sat, out val);
        sat *= 0.1f;
        col = Color.HSVToRGB(hue, sat, val) * 0.9f;
        col.a *= 0.5f;

        for (int i = 0; i < size; i++)
        {
            SpawnPulseOnLine(pulseObjectPrefab, i, col, larger);
            //SpawnPulseOnBounds(pulseObjectPrefab, col);
        }
    }
    #endregion

    #region PRIVATE_FUNC
    /// <summary>
    /// FD : Track the earth and cluster transforms
    /// </summary>
    private void TrackAnchors()
    {
        for (int i = 0; i < size; i++)
        {
            earthLines[i].GetComponent<LineRenderer>().SetPosition(0, earthAnchor.position);
            clusterLines[i].GetComponent<LineRenderer>().SetPosition(0, clusterAnchor.position);
        }
    }

    private int lastLine = 0;
    /// <summary>
    /// Fd : Spawn a pulse on all lines
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="i"></param>
    /// <param name="col"></param>
    /// <param name="larger"></param>
    private void SpawnPulseOnLine(GameObject prefab, int i, Color col, bool larger)
    {
        Vector3 bound = bounds[i];

        GameObject newPulse = Instantiate(prefab, bound, Quaternion.identity, pulseHolder.transform);
        newPulse.GetComponent<PulseManager>().Init(clusterAnchor, bound, col, larger);

        newPulse = Instantiate(prefab, bound, Quaternion.identity, pulseHolder.transform);
        newPulse.GetComponent<PulseManager>().Init(earthAnchor, bound, col, larger);
    }

    /// <summary>
    /// FD : Spawn pulses randomly on bounds
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="col"></param>
    private void SpawnPulseOnBounds(GameObject prefab, Color col)
    {
        int i = Random.Range(0, size);
        int j = (Random.Range(0, 2) == 0) ? (i + 1) % size : ((i == 0) ? size - 1 : i - 1);

        Vector3 bound = Vector3.Lerp(bounds[i], bounds[j], Random.Range(0f, 1f));

        GameObject newPulse = Instantiate(prefab, clusterAnchor.position, Quaternion.identity, pulseHolder.transform);
        //newPulse.GetComponent<PulseManager>().Init(clusterAnchor, earthAnchor, bound, col);
    }
    #endregion
}
