using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.VFX;

/// <summary>
/// CD: Tendril: This is a network behaviour that contains the meandering process for all tendrils created in IOAN
/// </summary>
public class Tendril : NetworkBehaviour
{
    /// <summary>
    /// CM: Pulled from previous comments before us
    /// basic algorithm
    /// pic a random point within a radius of endPosition
    /// start growing by adding points in direction of new goal
    /// every time a new point is choise there is a variability % change of choosing a new goal points/
    /// 100 chances to change directions?
    /// always interpolate between two points
    /// more interpolation points if detail is high
    /// as end point nears end poisition varaiblly decreases to 0.
    /// </summary>

    //basic algorithm
    // pic a random point within a radius of endPosition
    // start growing by adding points in direction of new goal
    // every time a new point is choise there is a variability % change of choosing a new goal points/
    // 100 chances to change directions?
    // always interpolate between two points
    // more interpolation points if detail is high
    // as end point nears end poisition varaiblly decreases to 0.

    //Update delegate function
    private delegate void UpdateEvent();
    private UpdateEvent update;

    #region PUBLIC VAR
    /// <summary>
    ///     Tendril Input Start Position
    /// </summary>
    [SyncVar]
    public Vector3 inStartPosition;

    /// <summary>
    ///     Tendril Serialized Property for start position
    /// </summary>
    [SerializeField]
    public Vector3 startPosition
    {
        get
        {
            return m_startPosition;
        }
        set
        {
            m_startPosition = value;
            UpdateSunkStartPosition();
        }
    }

    /// <summary>
    ///     Variable start position
    /// </summary>
    public Vector3 m_startPosition = Vector3.zero;

    /// <summary>
    ///     Sunken Start Position / Start Position where y value is lowered
    /// </summary>
    Vector3 sunkStartPosition;

    /// <summary>
    ///     Tendril Input End Position
    /// </summary>
    [SyncVar]
    public Vector3 inEndPosition;

    /// <summary>
    ///     Tendril Serialized Property for end position
    /// </summary>
    [SerializeField]
    public Vector3 endPosition
    {
        get
        {
            return m_endPosition;
        }
        set
        {
            m_endPosition = value;
            UpdateSunkEndPosition();
        }
    }

    /// <summary>
    ///     Variable end position
    /// </summary>
    public Vector3 m_endPosition = Vector3.forward * -10f;

    /// <summary>
    ///     Sunken End Position / End Position where y value is lowered
    /// </summary>
    Vector3 sunkEndPosition;

    /// <summary>
    /// VD: min y for curved path
    /// </summary>
    [Tooltip("min y for curved path")]
    public float minTravelY = 1;

    /// <summary>
    /// VD: max y for curved path
    /// </summary>
    [Tooltip("max y for curved path")]
    public float maxTravelY = -1;

    /// <summary>
    /// VD:Max dististance off the strait line, as a percentage of Distance from start to goal.  Bigger number means bigger bends.
    /// </summary>
	[Tooltip("Max dististance off the strait line, as a percentage of Distance from start to goal.  Bigger number means bigger bends.")]
    [Range(0, 1)]
    public float variability = .5f;

    /// <summary>
    /// VD:Average Distance between control points while meadering (as a percentage of total distance from start to end).  Smaller number means more bends.
    /// </summary>
	[Tooltip("Average Distance between control points while meadering (as a percentage of total distance from start to end).  Smaller number means more bends.")]
    [Range(0, 1)]
    public float maxMeanderingDist = .3f;

    public int stepsPerSegment = 10;

    public float speedOfTendril = 1.0f;
    public bool useLifetimeDecay = false; //Typically false

    public float widthOfTendril = 0.25f;
    public float radiusMultiplier = 0.1f;
    public bool autodestructSet = false;

    public GameObject goalSpherePrefab;

    [SyncVar(hook = "OnChangeStatusText")]
    public string statusText;
    [SyncVar(hook = "OnChangeStarYOffset")]
    public float starYOffset;
    [SyncVar]
    public int padStatus;
    [SyncVar]
    public int playerID;
    [SyncVar]
    public string starID;

    public List<Vector3> controlPoints = new List<Vector3>();
    public List<Vector3> curvePoints = new List<Vector3>();

    public float inputSpeed = 1f;
    #endregion

    #region PRIVATE_VAR
    private float lifeTime;
    private float tempLife;

    private Vector3 lastSplitPoint;

    private int curControlPoint = 0;
    private int curCurveSegment = 0;

    private bool controlPointsAreValid = false;
    private bool isCurveDone = false;
    private bool isControlDone = false;

    private Vector3 m0, p0, p1, m1;

    private bool isControlDoneOnce = false;

    private int curCurvePointIndex = 0;
    #endregion

    #region UNITY_FUNC
    /// <summary>
    /// FD: Start(): figures out where we start and where we end. Figure out life time of tendrils and speed. This also sets up the colors of the tendrils as well.
    /// </summary>
    private void Start()
    {
        //Get Start and End Position
        startPosition = inStartPosition;
        endPosition = inEndPosition;

        lifeTime = (1 / speedOfTendril) * 1.75f * 1f;

        //Find lifeTime and speed
        if (useLifetimeDecay)
        {
            lifeTime *= 4f;
            update += LifeTimeDecay;
        }

        update += SpawnGoalSphere;

        speedOfTendril *= Mathf.Pow(Vector3.Distance(startPosition, endPosition), 0.75f);

        //REVISIONSTEST
        //GetComponent<TendrilPathFollower>().Speed = inputSpeed;
        GetComponent<TendrilPathFollower>().start = startPosition;
        GetComponent<TendrilPathFollower>().end = endPosition;
        GetComponent<TendrilPathFollower>().Speed = speedOfTendril;

        tempLife = lifeTime;

        //Set Renderer Values for all Child Objects
        foreach (Transform val in gameObject.GetComponentsInChildren<Transform>())
        {
            TrailRenderer tr = val.GetComponent<TrailRenderer>();

            tr.time = lifeTime;
            tr.widthMultiplier = widthOfTendril * 1.25f;
            tr.autodestruct = false; // OVERRIDING FOR SPAWNING TO WORK RIGHT

            if ((val.name == "left" || val.name == "right") || (val.name == "up" || val.name == "down"))
            {
                val.localPosition *= radiusMultiplier;
                tr.autodestruct = true;
                tr.widthMultiplier = widthOfTendril * 0.75f;
            }
        }

        //Get Outer Tendril Color
        float satMod = 0.5f;
        Color myColor = Color.white;

        switch (playerID % 3)
        {
            case 0:
                myColor = Color.HSVToRGB(0.975f, satMod, 1f);
                break;
            case 1:
                myColor = Color.HSVToRGB(0.25f, satMod, 1f);
                break;
            case 2:
                myColor = Color.HSVToRGB(0.65f, satMod, 1f);
                break;
            default:
                break;
        }

        //Apply Color to Outer Trail Renderers
        foreach (TrailRenderer val in GetComponentsInChildren<TrailRenderer>())
        {
            val.startColor = new Color(myColor.r, myColor.g, myColor.b, val.startColor.a);
            val.endColor = new Color(myColor.r, myColor.g, myColor.b, val.endColor.a);
        }


        //REVISIONSTESTING
        //GetComponent<VisualEffect>().SetVector3(Shader.PropertyToID("ParticleColor"), new Vector3(myColor.r, myColor.g, myColor.b));

        //Get Central Tendril Color
        satMod = 1.0f;
        myColor = Color.white;
        switch (padStatus)
        {
            case 0:
                myColor = Color.HSVToRGB(0f, 0f, 1f);
                break;
            case 1:
                myColor = Color.HSVToRGB(0.486f, satMod, 1f);
                break;
            case 2:
                myColor = Color.HSVToRGB(0.827f, satMod, 1f);
                break;
            default:
                myColor = Color.HSVToRGB(0.166f, satMod, 1f);
                break;
        }

        //Apply Color to Central Tendril
        TrailRenderer tTR = GetComponent<TrailRenderer>();
        tTR.startColor = new Color(myColor.r, myColor.g, myColor.b, tTR.startColor.a);
        tTR.endColor = new Color(myColor.r, myColor.g, myColor.b, tTR.endColor.a);
    }

    /// <summary>
    /// FD: Invoke the update delegate
    /// </summary>
    private void Update()
    {
        update?.Invoke();
    }

    /// <summary>
    ///     FD : Set the lifetime decay
    /// </summary>
    private void LifeTimeDecay()
    {
        if (tempLife > lifeTime * 0.25f)
        {
            float dRatio = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(endPosition.x, endPosition.z)) /
                           Vector2.Distance(new Vector2(startPosition.x, startPosition.z), new Vector2(endPosition.x, endPosition.z));

            if (dRatio > 0.1f)
                tempLife = (lifeTime * 0.25f) + (lifeTime * 0.75f) * ((dRatio - 0.1f) / 0.9f);

            foreach (Transform val in gameObject.GetComponentsInChildren<Transform>())
            {
                TrailRenderer tr = val.GetComponent<TrailRenderer>();
                tr.time = tempLife;
            }
        }
    }

    /// <summary>
    ///     Spawn the goal sphere and start destroying
    /// </summary>
    private void SpawnGoalSphere()
    {
        if (!hasAuthority) return;

        if (Vector3.Distance(transform.position, inEndPosition) < 0.25f && inEndPosition.y - transform.position.y < 0.08f)
        {
            CmdSpawnTendrilGoalSphere(endPosition, statusText, starYOffset, padStatus, playerID, starID);
            update -= SpawnGoalSphere;
            StartCoroutine(tDestroy());
        }
    }

    /// <summary>
    ///     Destroy object after t seconds
    /// </summary>
    /// <returns></returns>
    private IEnumerator tDestroy()
    {
        yield return new WaitForSeconds(4.0f);
        CmdDestroyGameObject();
    }

    /// <summary>
    ///     Destroy object on server
    /// </summary>
    [Command]
    private void CmdDestroyGameObject()
    {
        RpcDestroyGameObject();
        Destroy(gameObject);
    }

    /// <summary>
    ///     Destroy object on client
    /// </summary>
    [ClientRpc]
    private void RpcDestroyGameObject()
    {
        Destroy(gameObject);
    }
    #endregion

    #region PUBLIC_FUNC
    /// <summary>
    /// FD: getNextCurvePoint(): This sets up the next step in the meandering path of the tendril, it returns the correct index based on if the tendril is done curving.
    /// </summary>
    /// <returns></returns>
    public Vector3 GetNextCurvePoint()
    {
        while ((curCurvePointIndex >= curvePoints.Count) && (!isCurveDone))
            addNewCurvePoint(); //this does not check isCurveDone;

        curCurvePointIndex++;

        if (isCurveDone)
            return curvePoints[curvePoints.Count - 1];
        else
            return curvePoints[curCurvePointIndex - 1];
    }

    /// <summary>
    /// FD: hasMoreCurvePoints(): This is used to determine if we are done meandering.
    /// </summary>
    /// <returns>The opposite of isCurveDone</returns>
    public bool HasMoreCurvePoints()
    {
        return !isCurveDone;
    }

    /// <summary>
    /// FD: addNewCurvePoint(): This adds more interpolation points for meandering as long as we are determined to keep going or we reached beyond our amount of control points. If there is a valid point we interpolate another point.
    /// </summary>
    public void addNewCurvePoint()
    {
        if (isCurveDone) return;

        if (curControlPoint >= controlPoints.Count)
        {
            curvePoints.Add(m_endPosition);
            isCurveDone = true;
            return;
        }

        if (controlPointsAreValid)
        {
            float t = curCurveSegment / (float)stepsPerSegment;
            Vector3 newPoint = CalcCurvPoint(m0, p0, p1, m1, t);
            curvePoints.Add(newPoint);
        }

        curCurveSegment++;

        if (curCurveSegment >= stepsPerSegment)
        {
            curCurveSegment = 0;
            curControlPoint++;

            while ((!isControlDone) && curControlPoint >= controlPoints.Count - 2)
                AddNewControlPoint();

            controlPointsAreValid = CalcAttractorPoints(curControlPoint, out m0, out p0, out p1, out m1);
        }
    }

    /// <summary>
    /// FD: beginTendril(): This chooses our random positions, clears everything and starts the curve processes.
    /// </summary>
    public void BeginTendril()
    {
        Clear();
        UpdateSunkEndPosition();
        UpdateSunkStartPosition();

        controlPoints.Add(m_startPosition);
        controlPoints.Add(sunkStartPosition);

        lastSplitPoint = sunkStartPosition;
        AddNewControlPoint();
        controlPointsAreValid = CalcAttractorPoints(curControlPoint, out m0, out p0, out p1, out m1);
    }

    /// <summary>
    /// FD: addNewControlPoint():
    /// </summary>
    public void AddNewControlPoint()
    {
        if (isControlDone) return;

        Vector3 lastPoint = controlPoints[controlPoints.Count - 1];

        float curDist = Vector3.Distance(lastPoint, sunkEndPosition);
        float maxDist = Vector3.Distance(sunkStartPosition, sunkEndPosition);

        float meaderingStepSize = maxMeanderingDist * maxDist;

        if (curDist < meaderingStepSize)
        {
            controlPoints.Add(sunkEndPosition);
            controlPoints.Add(m_endPosition);
            isControlDone = true;
        }
        else
        {
            Vector3 dir = sunkEndPosition - lastSplitPoint;
            dir.Normalize();

            Vector3 sideways = Vector3.Cross(dir, Vector3.up);
            sideways.Normalize();

            meaderingStepSize *= Random.Range(0f, 1.0f);

            dir *= meaderingStepSize;

            lastSplitPoint = lastSplitPoint + dir;

            sideways *= (meaderingStepSize * variability * Random.Range(-1.0f, 1.0f));

            Vector3 newPoint = lastSplitPoint + sideways;
            newPoint.y = Random.Range(minTravelY, maxTravelY);

            controlPoints.Add(newPoint);
        }
    }

    /// <summary>
    /// FD: CalcAttractorPoints(): This determines some interpolation of the curves in the meandering of the tendril.
    /// </summary>
    /// <param name="curPointIndex">VD: Which point in the curve the tendril is on</param>
    /// <param name="m0">VD: interpolated output vertex</param>
    /// <param name="p0">VD: control point output vertex</param>
    /// <param name="p1">VD: control point output vertex</param>
    /// <param name="m1">VD: interpolated output vertex</param>
    /// <returns>Returns false on being done, returns true if still points to interpolate</returns>
    public bool CalcAttractorPoints(int curPointIndex, out Vector3 m0, out Vector3 p0, out Vector3 p1, out Vector3 m1)
    {
        if (curPointIndex >= controlPoints.Count - 1)
        {
            m0 = p0 = p1 = m1 = Vector3.zero;
            return false;
        }

        p0 = controlPoints[curPointIndex];
        p1 = controlPoints[curPointIndex + 1];

        if (curPointIndex > 0)
            m0 = 0.5f * (controlPoints[curPointIndex + 1] - controlPoints[curPointIndex - 1]);
        else
            m0 = controlPoints[curPointIndex + 1] - controlPoints[curPointIndex];

        if (curPointIndex < controlPoints.Count - 2)
            m1 = 0.5f * (controlPoints[curPointIndex + 2] - controlPoints[curPointIndex]);
        else
            m1 = controlPoints[curPointIndex + 1] - controlPoints[curPointIndex];

        return true;
    }
    #endregion

    #region PRIVATE_FUNC
    /// <summary>
    /// FD:  updateSunkEndPosition(): This is where we choose a random position to go to in a curve.
    /// <remarks>GS:  This appears to be in polar coordinates as only y is modified.</remarks>
    /// </summary>
    private void UpdateSunkEndPosition()
    {
        sunkEndPosition.Set(m_endPosition.x, Random.Range(minTravelY, maxTravelY), m_endPosition.z);
    }

    /// <summary>
    /// FD: updateSunkStartPosition(): This is where we choose a random position to start on a curve. 
    /// <remarks>GS:  This appears to be in polar coordinates as only y is modified.</remarks>
    /// </summary>
	private void UpdateSunkStartPosition()
    {
        sunkStartPosition.Set(m_startPosition.x, Random.Range(minTravelY, maxTravelY), m_startPosition.z);
    }

    /// <summary>
    ///    FD: Hook for when the status text is changed
    /// </summary>
    /// <param name="oldStatusText"></param>
    /// <param name="inStatusText"></param>
    void OnChangeStatusText(string oldStatusText, string inStatusText)
    {
        statusText = inStatusText;
    }

    /// <summary>
    ///     FD: Hook for when the yoffset is changed
    /// </summary>
    /// <param name="outOffset"></param>
    /// <param name="inOffset"></param>
    void OnChangeStarYOffset(float outOffset, float inOffset)
    {
        starYOffset = inOffset;
    }

    /// <summary>
    /// FD: CmdSpawnTendrilGoalSphere(): This sets forward the creation of the GoalSphere after a tendril ends.
    /// </summary>
    /// <param name="inPos">VD: This is the input position</param>
    /// <param name="statText">VD: Text of the goal sphere (Text is star ID)</param>
    /// <param name="inStarYOffset">VD: Y offset of where the star is</param>
    /// <param name="inPadStat">VD: GS: Possible offset of </param>
    /// <param name="inPlayerID">VD: This is used for networking the goal sphere to the right player</param>
    [Command]
    private void CmdSpawnTendrilGoalSphere(Vector3 inPos, string statText, float inStarYOffset, int inPadStat, int inPlayerID, string inStarID)
    {
        var myGoalSphere = Instantiate(goalSpherePrefab, inPos, Quaternion.identity);
        myGoalSphere.GetComponent<GoalStarActions>().statusText = statText;
        myGoalSphere.GetComponent<GoalStarActions>().starYOffset = inStarYOffset;
        myGoalSphere.GetComponent<GoalStarActions>().padStatus = inPadStat;
        myGoalSphere.GetComponent<GoalStarActions>().playerID = inPlayerID;
        myGoalSphere.GetComponent<GoalStarActions>().starID = inStarID;

        NetworkServer.Spawn(myGoalSphere, GetComponent<NetworkIdentity>().connectionToClient);
        //NetworkServer.Destroy(gameObject);
    }

    /// <summary>
    /// Use this to reset this tendril just in case we reuse this object.
    /// </summary>
    private void Clear()
    {
        controlPoints.Clear();
        curvePoints.Clear();

        isControlDone = false;
        isCurveDone = false;
    }

    /// <summary>
    /// FD: calcCurvPoint(): This calculates the current interpolation movement of curve based on points made from attraction points
    /// </summary>
    /// <param name="m0">VD: interpolated output vertex</param>
    /// <param name="p0">VD: control point output vertex</param>
    /// <param name="p1">VD: control point output vertex</param>
    /// <param name="m1">VD: interpolated output vertex</param>
    /// <param name="t">VD: Current time step</param>
    /// <returns>returns vector of the current interpolated point</returns>
    private Vector3 CalcCurvPoint(Vector3 m0, Vector3 p0, Vector3 p1, Vector3 m1, float t)
    {
        float tSqr = t * t;
        float tCube = tSqr * t;

        return (2.0f * tCube - 3.0f * tSqr + 1.0f) * p0
             + (tCube - 2.0f * tSqr + t) * m0
             + (-2.0f * tCube + 3.0f * tSqr) * p1
             + (tCube - tSqr) * m1;
    }
    #endregion
}
