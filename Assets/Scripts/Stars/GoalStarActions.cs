using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.VFX;

/// <summary>
/// CD: Several sync variables for bridging o:activatedStars and o:tendrils accross the whole system
/// </summary>
public class GoalStarActions : NetworkBehaviour
{
    #region PUBLIC_VAR
    ///<remarks> All of these are SyncVars </remarks>

    /// <summary>
    /// VD: starID to be displayed above the goal spheres once the tendrils arive 
    /// </summary>
    [SyncVar(hook="OnChangeStatusText")]
	public string statusText;

	/// <summary>
	/// VD: current height of the star above y = 0 to help the goal sphere actually encircle the star
	/// </summary>
	[SyncVar(hook="OnChangeStarYOffset")]
	public float starYOffset;

	/// <summary>
	/// VD: Status of the touchpad. Passes through about 8 functions in 7 scripts to get here
	///  ::: IV: GS: Build a getset array <example><code>padStatus[playerID] = padstatus;</code></example> reference it instead of passing it?
	///  ::: IV: Furthermore, I don't even think this class uses it NDH
	/// </summary><remarks>GS: It's set by tendril.cs but never called. Run a test with this and f:onChangePadStat commented out</remarks>
	[SyncVar(hook="OnChangePadStat")]
	public int padStatus;

	/// <summary>
	///  VD: Player marker to properly color the goal-brackets across clients
	/// </summary>
	[SyncVar(hook="OnChangePlayerID")]
	public int playerID;

    /// <summary>
    ///  VD : ID of the Star being "Highlighted"
    /// </summary>
    [SyncVar(hook="OnChangeStarID")]
    public string starID;

    /// <summary>
    ///  VD : Time counter that the goalsphere will stay alive
    /// </summary>
	[SyncVar]
	public float timeToDie;

    /// <summary>
    ///  VD : Object that contains the VFX of the goalsphere
    /// </summary>
    public GameObject visObject;
    #endregion

    public float waveYMod = 0.0f;

    #region PRIVATE_VAR
    /// <summary>
    ///  VD : The max time that a goalsphere stays alive
    /// </summary>
    private float maxTime = 5f;

    /// <summary>
    ///  VD : Color of the player that spawned the tendril
    /// </summary>
    private Color playerColor;

    /// <summary>
    ///  VD : Color of the selected param
    /// </summary>
    private Color highlightColor;


    /// <summary>
    ///  VD : The text object
    /// </summary>
    private GameObject textObject;

    /// <summary>
    ///  VD : the VFX used by the goalsphere
    /// </summary>
    private VisualEffect visEffect;

    /// <summary>
    ///  VD : flag for when the goalsphere is dying
    /// </summary>
    private bool dying = false;

    /// <summary>
    ///  VD : spawn rate of the particles to be decremented on fade out
    /// </summary>
    private float spawnRate = 1000f;
    #endregion

    private shadeSwapper shade;

    #region UNITY_FUNC
    /// <summary>
    /// FD: Colours and places the starID above the goal brackets
    /// </summary>
    private void Start()
    {
        gameObject.name = "TendrilHalo-" + starID;
        timeToDie = Time.time + maxTime;
        StartCoroutine(tDestroy(10.0f));

        textObject = transform.Find("GoalSphereText").gameObject;
        textObject.GetComponent<TextMesh>().text = statusText;

        shade = GameObject.FindGameObjectWithTag("init").GetComponent<shadeSwapper>();
        shade.NonStarObjects.Add(gameObject);

        float satMod = 0.5f;
        playerColor = Color.white;

        switch (playerID % 3)
        {
            case 0:
                playerColor = Color.HSVToRGB(0.975f, satMod, 1f, true);
                break;
            case 1:
                playerColor = Color.HSVToRGB(0.25f, satMod, 1f, true);
                break;
            case 2:
                playerColor = Color.HSVToRGB(0.65f, satMod, 1f, true);
                break;
            default:
                break;
        }

        uint intID = uint.Parse(starID);

        highlightColor = new Color()
        {
            r = ((intID & 0x00FF0000) >> 16) / 255f,
            g = ((intID & 0x0000FF00) >> 8) / 255f,
            b = (intID & 0x000000FF) / 255f,
            a = 1
        };

        GetComponent<MeshRenderer>().material.color = playerColor;
        GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", playerColor);

        switch(padStatus)
        {
            case 0:
                highlightColor = Color.white;
                break;
            case 1:
                highlightColor = Color.cyan;
                break;
            case 2:
                highlightColor = Color.magenta;
                break;
            case 3:
                highlightColor = Color.yellow;
                break;
        }

        visEffect = visObject.GetComponent<VisualEffect>();
        visEffect.SetVector4(Shader.PropertyToID("PlayerColor"), playerColor);
        visEffect.SetVector4(Shader.PropertyToID("MainColor"), highlightColor);
    }

    /// <summary>
    /// FD: Keeps the goal brackets facing the players and in the correct place on the mesh
    /// </summary>
    private void Update()
    {
        transform.LookAt(new Vector3(Camera.main.transform.position.x, transform.position.y, Camera.main.transform.position.z));
        transform.Rotate(Vector3.up * 180f);

        bool hit_miss2 = Physics.Raycast(transform.position + Vector3.up * 20f, Vector3.down, out RaycastHit hit2, Mathf.Infinity, LayerMask.GetMask("newnewCloth"));
        if (hit_miss2) transform.position = new Vector3(transform.position.x, hit2.point.y + starYOffset + waveYMod, transform.position.z);

        if (Time.time > timeToDie && isClient) CmdDestroy();
        if (dying) FadeOutSphere();
    }

    private void OnDestroy()
    {
        shade.NonStarObjects.Remove(gameObject);
    }
    #endregion

    #region PRIVATE_FUNC
    /// <summary>
    /// FD : Hook function for starID
    /// </summary>
    /// <param name="oldStarID"></param>
    /// <param name="inStarID"></param>
    private void OnChangeStarID(string oldStarID, string inStarID)
    {
        starID = inStarID;
    }

	/// <summary>
	/// FD: IV: Useless set, never referenced 
	/// </summary>
	/// <param name="inPlayerID"></param>
	private void OnChangePlayerID(int oldPlayerID, int inPlayerID)
	{
		playerID = inPlayerID;
	}

	/// <summary>
	/// FD: IV: Useless set, could be implemented better. Maybe tendrils once had to do something with the padStatus
	/// </summary>
	/// <param name="inPadStat"></param>
	private void OnChangePadStat(int oldPadStat, int inPadStat)
	{
		padStatus = inPadStat;
	}

	/// <summary>
	/// FD: IV: Useless set, never referenced 
	/// </summary>
	/// <param name="inText"></param>
	private void OnChangeStatusText(string oldText, string inText)
	{
		statusText = inText;
	}

	/// <summary>
	/// FD: IV: Useless set, never referenced 
	/// </summary>
	/// <param name="inOffset"></param>
	private void OnChangeStarYOffset(float oldOffset, float inOffset)
	{
		starYOffset = inOffset;
	}

	/// <summary>
    /// FD : Command to destroy the goalsphere
    /// </summary>
	[Command]
	private void CmdDestroy()
	{
        visEffect.SendEvent("OnDestroy");
        dying = true;
	}
    /// <summary>
    /// FD : Command to destroy the goalsphere
    /// </summary>
    [Command]
    public void CmdDestroyNow()
    {
        visEffect.SendEvent("OnDestroy");
        NetworkServer.Destroy(gameObject);
    }

    /// <summary>
    /// FD : Fades out the goalsphere
    /// </summary>
    private void FadeOutSphere()
    {
        textObject.GetComponent<MeshRenderer>().material.SetFloat("Alpha", spawnRate / 1000f);

        visEffect.SetFloat(Shader.PropertyToID("SpawnRate"), spawnRate);

        spawnRate -= 5f;

        if (spawnRate <= 0f)
        {
            dying = false;
            StartCoroutine(tDestroy(1.5f));
        }
    }

	/// <summary>
    /// FD : Destroy the goalsphere on the server
    /// </summary>
    /// <returns></returns>
	private IEnumerator tDestroy(float time)
	{
        yield return new WaitForSeconds(time);

        DestroyObject();
	}

    public void DestroyObject()
    {
        NetworkServer.Destroy(gameObject);
    }
    #endregion
}