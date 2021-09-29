using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/**
<summary>
	CD : sphereBlankScript
	Sets the text of the star description?
</summary>
**/
public class sphereBlankScript : NetworkBehaviour 
{
	#region PUBLIC_VAR
	/// <summary>
	///		VD : starYOffset
	///		//This is the info display that pops up on taps!
	///		//it is attached to the sphereBlankScript
	///		//TODO: delete once its verified that TapInfoDisplayScript works correctly
	/// </summary>
	public float starYOffset = 0.0f;

    public float waveYMod = 0.0f;

	/// <summary>
	///		VD : starID
	///		ID of represented Star
	/// </summary>
	[SyncVar(hook = "OnChangeStarID")]
	public uint starID;

	/// <summary>
	///		VD : starPos
	///		Position of star
	/// </summary>
	[SyncVar(hook = "OnChangeStarPos")]
	public Vector3 starPos;

	/// <summary>
	///		VD : playerID
	///		ID of interacting player
	/// </summary>
	[SyncVar(hook = "OnChangePlayerID")]
	public int playerID;

	/// <summary>
	///		VD : selIndex
	/// </summary>
	[SyncVar(hook = "OnChangeSelIndex")]
	public int selIndex;

	/// <summary>
	///		VD : angle
	///		//999 is the invalid tap angle
	/// </summary>
	public float angle = 999f; 

	/// <summary>
	///		VD : isLeftController
	/// </summary>
	public bool isLeftController = true;
	#endregion

	#region PRIVATE_VAR
	/// <summary>
	///		VD : isVisable
	/// </summary>
	bool isVisable = false;

    private shadeSwapper shade;
	#endregion

	#region UNITY_FUNC
	/**
	<summary>
		FD : Start()
		Call f:CmdSetText(), f:CmdSetTag(), and f:CmdDestroy()
	</summary>
	**/
    public override void OnStartClient()
    {
        CmdSetText();
        CmdSetTag();
        StartCoroutine(tDestroy());
    }

    /**
	<summary>
		FD : Update()
		Return if object doesn't have authority
		If there is something below gameObject, set position height to what it hits plus v:starYOff + .21
		If v:isVisable is false call f:CmdMakeVisible()
	</summary>
	**/
    private void Start()
    {
        gameObject.name = "TapDisplay-" + starID;
        starPos = transform.position;

        shade = GameObject.FindGameObjectWithTag("init").GetComponent<shadeSwapper>();
        shade.NonStarObjects.Add(gameObject);
    }

    /// <summary>
    ///     Keep sphere in line with its associated mesh vertex
    /// </summary>
    private void Update()
	{
		if (!hasAuthority) return;

		int layerMask2 = LayerMask.GetMask("newnewCloth");
		RaycastHit hit2;
		bool hit_miss2 = Physics.Raycast(transform.position + new Vector3(0f, 20f, 0f), Vector3.down, out hit2, Mathf.Infinity, layerMask2);
		if (hit_miss2) transform.position = new Vector3(transform.position.x, hit2.point.y + starPos.y + waveYMod + 0.125f, transform.position.z);

		if (!isVisable) CmdMakeVisible();
	}

    private void OnDestroy()
    {
        shade.NonStarObjects.Remove(gameObject);
    }
    #endregion

    #region PRIVATE_FUNC
    /**
	<summary>
		FD : OnChangeSelIndex(int)
		Set v:selIndex to inIndex
		<param name="inIndex"></param>
	</summary>
	**/
    private void OnChangeSelIndex(int oldIndex, int inIndex)
	{
		selIndex = inIndex;
	}

	/**
	<summary>
		FD : OnChangeStarID(uint)
		Set v:starID to inID
		<param name="inID"></param>
	</summary>
	**/
	private void OnChangeStarID(uint oldID, uint inID)
	{
		starID = inID;
	}

	/**
	<summary>
		FD : OnChangeStarPos(Vector3)
		Set v:starPos to inPos
		<param name="inPos"></param>
	</summary>
	**/
	private void OnChangeStarPos(Vector3 outPos, Vector3 inPos)
	{
		starPos = inPos;
	}

	/**
	<summary>
		FD : OnChangePlayerID(int)
		Set v:playerID to inID
		<param name="inID"></param>
	</summary>
	**/
	private void OnChangePlayerID(int oldID, int inID)
	{
		playerID = inID;
	}

	/**
	<summary>
		FD : tDestroy()
		Wait 8 seconds, then destroy gameObject on Server
	</summary>
	**/
	private IEnumerator tDestroy()
	{
		yield return new WaitForSeconds(8.0f);
        CmdTDestroy();
	}

    /// <summary>
    ///     Destroy object on server
    /// </summary>
    [Command]
    public void CmdTDestroy()
    {
        RpcTDestroy();
        Destroy(gameObject);
    }

    /// <summary>
    ///     Destroy object on client
    /// </summary>
    [ClientRpc]
    public void RpcTDestroy()
    {
        Destroy(gameObject);
    }

	/**
	<summary>
		FD : CmdMakeVisible()
		Call f:RpcMakeVisible()
		Set v:isVisable to true and enable the MeshRenderer
		Enable LineRenderer in all children
	</summary>
	**/
	[Command]
	private void CmdMakeVisible()
	{
		RpcMakeVisible();
		// make it visible on the server too
		//(and enabling meshses may fix the error messages)
		isVisable = true;
		GetComponent<MeshRenderer>().enabled = true;

		foreach (Transform child in transform) child.GetComponent<LineRenderer>().enabled = true;
	}

	/**
	<summary>
		FD : RpcMakeVisible()
		Set v:isVisable to true and enable the MeshRenderer
		Enable LineRenderer in all children
	</summary>
	**/
	[ClientRpc]
	private void RpcMakeVisible()
	{
		isVisable = true;
		GetComponent<MeshRenderer>().enabled = true;

		foreach (Transform child in transform) child.GetComponent<LineRenderer>().enabled = true;
	}

	/**
	<summary>
		FD : CmdSetTag()
		Call f:RpcSetTag()
	</summary>
	**/
	[Command]
	private void CmdSetTag()
	{
		gameObject.tag = "sphereBlank";
		RpcSetTag();
	}

	/**
	<summary>
		FD : RpcSetTag()
		Set gameObject tag to "sphereBlank"
	</summary>
	**/
	[ClientRpc]
	private void RpcSetTag()
	{
		gameObject.tag = "sphereBlank";
	}

    /// <summary>
    /// FD : This function sets the text of the tap display
    /// </summary>
	private void SetText()
    {
        List<string> starHighlightData = new List<string>();

		int layerMask = LayerMask.GetMask("cloth");
		RaycastHit hit;
		bool hit_miss = Physics.Raycast(transform.position + new Vector3(0f, 20f, 0f), Vector3.down, out hit, Mathf.Infinity, layerMask);
		if (hit_miss)
		{
			Mesh hitMesh = (hit.collider as MeshCollider).sharedMesh;

			//float tempPeriod = -1f;
			List<Vector4> uv0List = new List<Vector4>();
			List<Vector4> uv1List = new List<Vector4>();
			hitMesh.GetUVs(0, uv0List);
			hitMesh.GetUVs(1, uv1List);
            //int selectedID = (int)(uv1List [selIndex].w * 10000000);

			starHighlightData.Add("              ID: " + starID.ToString());
			Vector3 selectedPoint = hit.collider.transform.TransformPoint(hitMesh.vertices[selIndex]);
			Vector4 tPoint = new Vector4(selectedPoint.x, selectedPoint.y, selectedPoint.z, 1f);
			Vector4[] deNormData = DeNormalizeMeshData(tPoint, hitMesh.colors[selIndex], uv0List[selIndex], uv1List[selIndex]);

			starHighlightData.Add("              Ra: " + deNormData[0].x);
            starHighlightData.Add("             Dec: " + deNormData[0].z);
			starHighlightData.Add("       Magnitude: " + deNormData[1].x);
			//starHighlightData.Add("         Std Dev: " + deNormData[1].z);
			starHighlightData.Add("Num Observations: " + deNormData[1].z);

            /*
			int tempVariability = (int)(deNormData[1].w);
			if (tempVariability == 3) starHighlightData += "\n            Variability: To Be Determined";
			else starHighlightData += "\n           Variability: Confirmed";
            */

			//starHighlightData.Add("         Catalog: " + deNormData[2].x);
			//starHighlightData += "\nAstrometric Pseudocolor: " + deNormData[2].y;
			starHighlightData.Add("          Period: " + deNormData[2].z);
			//starHighlightData.Add(" Periodogram SNR: " + deNormData[2].w);
			//starHighlightData.Add("  Lightcurve RMS: " + deNormData[3].x);
			starHighlightData.Add("  Variable Class: " + deNormData[3].y);
			//starHighlightData.Add("   Proper Motion: " + deNormData[3].z);

            /*
			float tempParallax = deNormData[0].y;
			if (tempParallax < 0) starHighlightData += "\n               Parallax: 0 (False Parallax: " + tempParallax + ")";
			else starHighlightData += "\n               Parallax: " + tempParallax;
            */
		}

        //StartCoroutine(AnimateTextLoad(starHighlightData));
        GetComponent<TextMesh>().text = "\n" + string.Join("\n", starHighlightData.ToArray());
	}

    /// <summary>
    /// FD : A function that can animate the output of the text
    /// </summary>
    /// <param name="strings"></param>
    /// <returns></returns>
    private IEnumerator AnimateTextLoad(List<string> strings)
    {
        TextMesh tMesh = GetComponent<TextMesh>();
        int fullSize = 2 * strings.Count - 1;
        float waitTime = 0.05f;

        for(int i = 0; i < strings.Count - 1; i++)
        {
            string output = "";
            for (int j = 0; j < strings.Count - i; j++) output += "\n";
            List<string> newList = strings.GetRange(0, i + 1);

            tMesh.text = output + "\n" + string.Join("\n", newList.ToArray());

            yield return new WaitForSeconds(waitTime);
        }

        tMesh.text = "\n" + string.Join("\n", strings.ToArray());

        yield return new WaitForSeconds(waitTime);

        for (int i = 0; i < strings.Count - 1; i++)
        {
            List<string> newList = strings.GetRange(i + 1, strings.Count - 1 - i);
            string output = "";
            for (int j = 0; j < i + 1; j++) output += "\n";

            tMesh.text = "\n" + string.Join("\n", newList.ToArray()) + output;

            yield return new WaitForSeconds(waitTime);
        }

        tMesh.text = "";
    }

    /**
	<summary>
		FD : CmdSetText()
		Call f:RpcSetText()
	</summary>
	**/
    [Command]
	private void CmdSetText()
	{
		SetText();
		RpcSetText();
	}

	/**
	<summary>
		FD : DeNormalizeMeshData(Vector4, Vector4, Vector4, Vector4)
		Converts all params into a single Vector4
		<param name="posData"></param>
		<param name="colorData"></param>
		<param name="uv0Data"></param>
		<param name="uv1Data"></param>
	</summary>
	**/
	private Vector4[] DeNormalizeMeshData(Vector4 posData, Vector4 colorData, Vector4 uv0Data, Vector4 uv1Data)
	{
		Vector4[] outData = new Vector4[] { posData, colorData, uv0Data, uv1Data };

        SFLimitsStruct limits = Config.Instance.GetLimits();
        SFScales scales = Config.Instance.GetScales();

		outData[0].w = 1f;
		outData[3].w = 1f;

		outData[0].x = outData[0].x / scales.xScale + scales.xOffset;
        outData[0].x = denormalizeToRange(outData[0].x, limits.ra.x, limits.ra.y);

        outData[0].y = (Mathf.Tan(outData[0].y) / 1.5f) / scales.yScale + scales.yOffset;
        outData[0].y = denormalizeToRange(outData[0].y, limits.parallax.x, limits.parallax.y);

        outData[0].z = outData[0].z / scales.zScale + scales.zOffset;
        outData[0].z = denormalizeToRange(outData[0].z, limits.dec.x, limits.dec.y);

		outData[1].x = denormalizeToRange(outData[1].x, limits.mag.x,      limits.mag.y     );
		outData[1].y = denormalizeToRange(outData[1].y, limits.std.x,      limits.std.y     );
		outData[1].z = denormalizeToRange(outData[1].z, limits.obs.x,      limits.obs.y     );
		outData[1].w = denormalizeToRange(outData[1].w, limits.var_flag.x, limits.var_flag.y);
		outData[2].x = denormalizeToRange(outData[2].x, limits.catalog.x,  limits.catalog.y );
		outData[2].y = denormalizeToRange(outData[2].y, limits.color.x,    limits.color.y   );
		outData[2].z = denormalizeToRange(outData[2].z, limits.period.x,   limits.period.y  );
		outData[2].w = denormalizeToRange(outData[2].w, limits.snr.x,      limits.snr.y     );
		outData[3].x = denormalizeToRange(outData[3].x, limits.rms.x,      limits.rms.y     );
		outData[3].y = denormalizeToRange(outData[3].y, limits.type.x,     limits.type.y    );
		outData[3].z = denormalizeToRange(outData[3].z, limits.pmra.x,     limits.pmra.y    );

		return outData;
	}

	/**
	<summary>
		FD : denormalizeToRange(float, float, float)
		<param name="val"></param>
		<param name="min"></param>
		<param name="max"></param>
	</summary>
	**/
	private float denormalizeToRange(float val, float max, float min)
	{
		return val * (max - min) + min;
	}

	/**
	<summary>
		FD : denormalizePower(float, float, float, float, float)
		<param name="val"></param>
		<param name="min"></param>
		<param name="max"></param>
		<param name="mean"></param>
		<param name="power"></param>
	</summary>
	**/
	private float denormalizePower(float val, float max, float min, float mean, float power)
	{
		mean = mean * (max - min) + min; /// IV : mean = denomarlizeToRange(mean, min, max);

		if (val != 0f) val = Mathf.Pow(val - mean, 1f / power) * (Mathf.Abs(val) / val);

		return (val + mean) * (max - min) + min; /// IV : return denomarlizeToRange(val + mean, max, min);
	}

	/**
	<summary>
		FD : RpcSetText()
		Sets text data on gameObject with the star data
	</summary>
	**/
	[ClientRpc]
	private void RpcSetText()
	{
		SetText();
	}
	#endregion
}
