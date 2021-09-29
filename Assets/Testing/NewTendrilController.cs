using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NewTendrilController : NetworkBehaviour
{
    private delegate void UpdateEvent();
    private UpdateEvent update;

    public float depth = -1.0f;
    [Range(0f, 1f)]
    public float maxStep = 0.1f;
    public float maxDisp = 0.1f;

    public GameObject goalSpherePrefab;

    [SyncVar(hook = "OnChangeStatusText")]
    public string statusText;
    [SyncVar(hook = "OnChangeStarYOffset")]
    public float starYOffset;
    [SyncVar]
    public int padStatus;
    [SyncVar]
    public int playerID;

    private Vector3 src;
    private Vector3 sunkSrc;
    private Vector3 dest;
    private Vector3 sunkDest;
    private float dist;

    private Transform tendrils;
    private TrailRenderer[] trails = new TrailRenderer[5];

    private struct PathNode
    {
        public Vector3 pos;
        public Vector2 offset;
    }
    private Queue<PathNode> loc = new Queue<PathNode>();

    private void Update()
    {
        update?.Invoke();
    }

    public void Init(Vector3 source, Vector3 destination)
    {
        transform.position = src = source;
        dest = destination;
        dist = Vector3.Distance(src, dest);

        tendrils = transform.Find("Tendrils");
        trails[0] = tendrils.Find("Center").GetComponent<TrailRenderer>();
        trails[1] = tendrils.Find("Top").GetComponent<TrailRenderer>();
        trails[2] = tendrils.Find("Bottom").GetComponent<TrailRenderer>();
        trails[3] = tendrils.Find("Left").GetComponent<TrailRenderer>();
        trails[4] = tendrils.Find("Right").GetComponent<TrailRenderer>();

        for (int i = 0; i < 5; i++)
        {
            trails[i].time = 7f / 5f;
        }

        trails[0].widthMultiplier = .125f;
        for(int i = 1; i < 5; i++)
        {
            trails[1].widthMultiplier = .075f;
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
                myColor = Color.HSVToRGB(0.75f, satMod, 1f);
                break;
            default:
                break;
        }

        //Apply Color to Outer Trail Renderers
        for (int i = 1; i < 5; i++)
        {
            trails[i].startColor = new Color(myColor.r, myColor.g, myColor.b, trails[i].startColor.a);
            trails[i].endColor = new Color(myColor.r, myColor.g, myColor.b, trails[i].endColor.a);
        }

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
        trails[0].startColor = new Color(myColor.r, myColor.g, myColor.b, trails[0].startColor.a);
        trails[0].endColor = new Color(myColor.r, myColor.g, myColor.b, trails[0].endColor.a);

        //Calculate Tendril Points
        sunkSrc = new Vector3(src.x, depth, src.z);
        sunkDest = new Vector3(dest.x, depth, dest.z);

        float lerpPerc = Random.Range(maxStep * 0.75f, maxStep);
        while (lerpPerc < 1f)
        {
            loc.Enqueue(new PathNode()
            {
                pos = Vector3.Lerp(sunkSrc, sunkDest, lerpPerc),
                offset = new Vector2()
                {
                    x = Mathf.Cos(Random.Range(-180f, 180f)) * maxDisp,
                    y = Mathf.Sin(Random.Range(-180f, 180f)) * maxDisp
                }
            });

            lerpPerc += Random.Range(maxStep * 0.75f, maxStep);
        }

        update += MoveDown;
    }

    private void MoveDown()
    {
        transform.LookAt(sunkSrc);
        transform.position = Vector3.MoveTowards(transform.position, sunkSrc, 0.01f);

        if (Vector3.Distance(transform.position, sunkSrc) < 0.01f)
        {
            update -= MoveDown;
            transform.position = sunkSrc;

            prev = sunkSrc;
            next = loc.Peek().pos;
            prevO = Vector2.zero;
            nextO = loc.Peek().offset;

            update += MoveAcross;
        }
    }

    private Vector3 prev;
    private Vector3 next;
    private Vector2 prevO;
    private Vector2 nextO;

    private void MoveAcross()
    {
        transform.LookAt(sunkDest);
        transform.position = Vector3.MoveTowards(transform.position, sunkDest, 0.01f);

        float p = Vector3.Distance(transform.position, prev) / Vector3.Distance(next, prev);
        Vector2 lerp = Vector2.Lerp(prevO, nextO, p);
        tendrils.localPosition = new Vector3(lerp.x, lerp.y, 0f);

        if (Vector3.Distance(transform.position, next) < 0.01f)
        {
            if (sunkDest == next)
            {
                update -= MoveAcross;
                transform.position = sunkDest;
                update += MoveUp;
            }
            else
            {
                PathNode old = loc.Dequeue();

                prev = old.pos;
                next = (loc.Count > 0) ? loc.Peek().pos : sunkDest;
                prevO = old.offset;
                nextO = (loc.Count > 0) ? loc.Peek().offset : Vector2.zero;
            }
        }
    }

    private void MoveUp()
    {
        transform.LookAt(dest);
        transform.position = Vector3.MoveTowards(transform.position, dest, 0.01f);

        if (Vector3.Distance(transform.position, dest) < 0.01f)
        {
            update -= MoveUp;
            CmdSpawnGoalSphere();
            update += DestroyTendril;
        }
    }

    [Command]
    private void CmdSpawnGoalSphere()
    {
        var myGoalSphere = Instantiate(goalSpherePrefab, dest, Quaternion.identity);
        myGoalSphere.GetComponent<GoalStarActions>().statusText = statusText;
        myGoalSphere.GetComponent<GoalStarActions>().starYOffset = starYOffset;
        myGoalSphere.GetComponent<GoalStarActions>().padStatus = padStatus;
        myGoalSphere.GetComponent<GoalStarActions>().playerID = playerID;

        NetworkServer.Spawn(myGoalSphere, GetComponent<NetworkIdentity>().connectionToClient);
    }

    private void DestroyTendril()
    {
        if (trails[0].positionCount == 0)
            NetworkServer.Destroy(gameObject);
    }

    void OnChangeStatusText(string oldStatusText, string inStatusText)
    {
        statusText = inStatusText;
    }

    void OnChangeStarYOffset(float outOffset, float inOffset)
    {
        starYOffset = inOffset;
    }
}
