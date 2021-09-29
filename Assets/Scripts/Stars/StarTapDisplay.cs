using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/**
<summary>
	CD : StarTapDisplay
	Show the star data when object is on display
</summary>
**/
public class StarTapDisplay : NetworkBehaviour 
{
	#region PUBLIC_VAR
	/// <summary>
	///		VD : starID
	///		This is the info display that pops up on taps!
	/// </summary>
	[SyncVar(hook = "OnChangeStarID")]
	public uint starID;

	/// <summary>
	///		VD : starPos
	/// </summary>
	[SyncVar(hook = "OnChangeStarPos")]
	public Vector3 starPos;

	/// <summary>
	///		VD : playerID
	/// </summary>
	[SyncVar(hook = "OnChangePlayerID")]
	public int playerID;

	/// <summary>
	///		VD : sellIndex
	/// </summary>
	[SyncVar(hook = "OnChangeSelIndex")]
	public int selIndex;

	/// <summary>
	///		VD : starYOffset
	/// </summary>
	public float starYOffset = 0.0f;
	#endregion

	#region PRIVATE_VAR
	/// <summary>
	///		VD : isVisable
	///		Toggle Visibility
	/// </summary>
	bool isVisable = false;
	#endregion

	#region UNITY_FUNC
	/**
	<summary>
		FD : Start()
		Calls f:CmdSetText(), f:CmdSetTag(), and f:CmdDestroy()
	</summary>
	**/
	void Start()
	{
		CmdSetText();
		CmdSetTag();
		CmdDestroy();
	}

	/**
	<summary>
		FD : Update()
		Return if this object doesn't have authority
		If there is anything below this object: Set position y value to hit point y value + .21
		Call f:CmdMakeVisible() if isVisable is false
	</summary>
	**/
	void Update()
	{
		if (!hasAuthority) return;

		RaycastHit hit2;
		bool hit_miss2 = Physics.Raycast(transform.position + new Vector3(0f, 20f, 0f), Vector3.down, out hit2, Mathf.Infinity);
		if (hit_miss2) transform.position = new Vector3(transform.position.x, hit2.point.y + 0.21f, transform.position.z);

		if (!isVisable) CmdMakeVisible();
	}
	#endregion

	#region PRIVATE_FUNC

	void SetText()
    {
		string starHighlightData = "";

		RaycastHit hit;
		bool hit_miss = Physics.Raycast(transform.position + new Vector3(0f, 20f, 0f), Vector3.down, out hit, Mathf.Infinity);
		if (hit_miss)
		{
			Mesh hitMesh = (hit.collider as MeshCollider).sharedMesh;

			//float tempPeriod = -1f;
			List<Vector4> uv0List = new List<Vector4>();
			List<Vector4> uv1List = new List<Vector4>();

			hitMesh.GetUVs(0, uv0List);
			hitMesh.GetUVs(1, uv1List);
			//int selectedID = (int)(uv1List [selIndex].w * 10000000);

			starHighlightData += "ID: " + starID.ToString();

			Vector3 selectedPoint = hit.collider.transform.TransformPoint(hitMesh.vertices[selIndex]);
			Vector4 tPoint = new Vector4(selectedPoint.x, selectedPoint.y, selectedPoint.z, 1f);
			Vector4[] deNormData = DeNormalizeMeshData(tPoint, hitMesh.colors[selIndex], uv0List[selIndex], uv1List[selIndex]);

			starHighlightData += "\nRa, Dec: (" + deNormData[0].x.ToString() + ", " + deNormData[0].z.ToString() + ")";
			starHighlightData += "\nMagnitude: " + deNormData[1].x;
			starHighlightData += "\nNum Observations: " + deNormData[1].y;
			starHighlightData += "\nStd Dev: " + deNormData[1].z;

			int tempVariability = (int)(deNormData[1].w);
			if (tempVariability == 3) starHighlightData += "\nVariability: To Be Determined";
			else starHighlightData += "\nVariability: Confirmed";

			starHighlightData += "\nCatalog: " + deNormData[2].x;
			starHighlightData += "\nAstrometric Pseudocolor: " + deNormData[2].y;
			starHighlightData += "\nPeriod: " + deNormData[2].z;
			starHighlightData += "\nPeriodogram SNR: " + deNormData[2].w;
			starHighlightData += "\nLightcurve RMS: " + deNormData[3].x;
			starHighlightData += "\nVariable Class: " + deNormData[3].y;
			starHighlightData += "\nProper Motion: " + deNormData[3].z;

			float tempParallax = deNormData[0].y;
			if (tempParallax < 0) starHighlightData += "\nParallax: 0 (False Parallax: " + tempParallax + ")";
			else starHighlightData += "\nParallax: " + tempParallax;
		}

		GetComponent<TextMesh>().text = starHighlightData;
	}

	/**
	<summary>
		FD : OnChangeSellIndex(int)
		Set v:sellIndex to inIndex
		<param name="inIndex"></param>
	</summary>
	**/
	void OnChangeSelIndex(int inIndex)
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
	void OnChangeStarID(uint inID)
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
	void OnChangeStarPos(Vector3 inPos)
	{
		starPos = inPos;
	}

	/**
	<summary>
		FD : OnChangePlayerID(int)
		Set playerID to inID
		<param name="inID"></param>
	</summary>
	**/
	void OnChangePlayerID(int inID)
	{
		playerID = inID;
	}

	/**
	<summary>
		FD : CmdDestroy()
		Start Coroutine f:tDestroy()
	</summary>
	**/
	[Command]
	void CmdDestroy()
	{
		StartCoroutine(tDestroy());
	}

	/**
	<summary>
		FD : tDestroy()
		Wait for 8 seconds
		Destroy this gameObject on the network server
	</summary>
	**/
	IEnumerator tDestroy()
	{
		yield return new WaitForSeconds(8.0f);
		NetworkServer.Destroy(gameObject);
		//		RpcDestroy ();
	}

	/**
	<summary>
		FD : CmdMakeVisible()
		Call f:RpcMakeVisible()
		Set f:isVisable to true
		Enable MeshRenderer
		Enable all LineRenderers in children
	</summary>
	**/
	[Command]
	void CmdMakeVisible()
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
		FD : 
	MakeVisible()
		Set v:isVisable to true
		Enable MeshRenderer
		Enable all LineRenderers in children
	</summary>
	**/
	[ClientRpc]
	void RpcMakeVisible()
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
	void CmdSetTag()
	{
		gameObject.tag = "sphereBlank";
		RpcSetTag();
	}

	/**
	<summary>
		FD : RpcSetTag()
		Set tag to "sphereBlank"
	</summary>
	**/
	[ClientRpc]
	void RpcSetTag()
	{
		gameObject.tag = "sphereBlank";
	}

	/**
	<summary>
		FD : CmdSetText()
		Call f:RpcSetText()
	</summary>
	**/
	[Command]
	void CmdSetText()
	{

		SetText();

		RpcSetText();
	}

	/**
	<summary>
		FD : DeNormalizeMeshData(Vector4, Vector4, Vector4, Vector4)
		Create array of vector4s using params
		Lots of calculations. Lots. Of. Cal-cu-la-tions.
		Returns array
		<param name="posData"></param>
		<param name="colorData"></param>
		<param name="uv0Data"></param>
		<param name="uv1Data"></param>
	</summary>
	**/
	Vector4[] DeNormalizeMeshData(Vector4 posData, Vector4 colorData, Vector4 uv0Data, Vector4 uv1Data)
	{
		Vector4[] outData = new Vector4[] { posData, colorData, uv0Data, uv1Data };

		outData[0].w = 1f;
		outData[3].w = 1f;

		outData[0].x = (outData[0].x / 200.0f);
		outData[0].x = (outData[0].x + 1.0f) / 2.0f;
		outData[0].x = (outData[0].x * (85.33088371578594f - 77.928459f)) + 77.928459f;
		outData[0].z = (outData[0].z / 200.0f);
		outData[0].z = (outData[0].z + 0.51f) / 2.0f;
		outData[0].z = (outData[0].z * (85.33088371578594f - 77.928459f)) - 72.003059f;

		outData[0].y = outData[0].y / 0.28f + 0.5f;
		outData[0].y = denormalizeToRange(outData[0].y, -6.172524f, 4.204787f);

		outData[1].x = denormalizeToRange(outData[1].x, 14.886403072230165f, 19.061404043487336f);
		outData[1].y = denormalizeToRange(outData[1].y, 0f, 0.6680229918890426f);
		outData[1].z = denormalizePower(outData[1].z, 0f, 3125.7502367831025f, 922.3292f, 0.5f);
		outData[1].w = denormalizeToRange(outData[1].w, 1f, 3f);
		outData[2].x = denormalizeToRange(outData[2].x, 0f, 7f);
		outData[2].y = denormalizeToRange(outData[2].y, 1.231199705120561f, 1.8991459147993115f);
		outData[2].z = denormalizeToRange(outData[2].z, 0.263666f, 36.09744f);
		outData[2].w = denormalizeToRange(outData[2].w, 0f, 26.61842977741538f);
		outData[3].x = denormalizeToRange(outData[3].x, 0f, 0.6715103569670999f);
		outData[3].y = denormalizeToRange(outData[3].y, 0f, 1f);
		outData[3].z = denormalizePower(outData[3].z, 0, 11.408797042686029f, 3.138575f, 0.5f);

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
	float denormalizeToRange(float val, float min, float max)
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
	float denormalizePower(float val, float min, float max, float mean, float power)
	{
		mean = min + (mean * (max - min)); //mean = denormalizeToRange(mean, min, max)

		if (val != 0f) val = Mathf.Pow(val - mean, 1f / power) * (Mathf.Abs(val) / val);

		return (val + mean) * (max - min) + min; //return (val + mean, min, max)
	}

	/**
	<summary>
		FD : RpcSetText()
		Set starHighlightData to empty string
		If something is below object
			Get the UVs for channel 0 and 1 for object below
			Add starData to starHightlightData
		Set TextMesh text to starHighlightData
	</summary>
	**/
	[ClientRpc]
	void RpcSetText()
	{
		/*string starHighlightData = "";

		RaycastHit hit;
		bool hit_miss = Physics.Raycast(transform.position + new Vector3(0f, 20f, 0f), Vector3.down, out hit, Mathf.Infinity);
		if (hit_miss)
		{
			Mesh hitMesh = (hit.collider as MeshCollider).sharedMesh;

			//float tempPeriod = -1f;
			List<Vector4> uv0List = new List<Vector4>();
			List<Vector4> uv1List = new List<Vector4>();

			hitMesh.GetUVs(0, uv0List);
			hitMesh.GetUVs(1, uv1List);
			//int selectedID = (int)(uv1List [selIndex].w * 10000000);

			starHighlightData += "ID: " + starID.ToString();

			Vector3 selectedPoint = hit.collider.transform.TransformPoint(hitMesh.vertices[selIndex]);
			Vector4 tPoint = new Vector4(selectedPoint.x, selectedPoint.y, selectedPoint.z, 1f);
			Vector4[] deNormData = DeNormalizeMeshData(tPoint, hitMesh.colors[selIndex], uv0List[selIndex], uv1List[selIndex]);

			starHighlightData += "\nRa, Dec: (" + deNormData[0].x.ToString() + ", " + deNormData[0].z.ToString() + ")";
			starHighlightData += "\nMagnitude: " + deNormData[1].x;
			starHighlightData += "\nNum Observations: " + deNormData[1].y;
			starHighlightData += "\nStd Dev: " + deNormData[1].z;

			int tempVariability = (int)(deNormData[1].w);
			if (tempVariability == 3) starHighlightData += "\nVariability: To Be Determined";
			else starHighlightData += "\nVariability: Confirmed";

			starHighlightData += "\nCatalog: " + deNormData[2].x;
			starHighlightData += "\nAstrometric Pseudocolor: " + deNormData[2].y;
			starHighlightData += "\nPeriod: " + deNormData[2].z;
			starHighlightData += "\nPeriodogram SNR: " + deNormData[2].w;
			starHighlightData += "\nLightcurve RMS: " + deNormData[3].x;
			starHighlightData += "\nVariable Class: " + deNormData[3].y;
			starHighlightData += "\nProper Motion: " + deNormData[3].z;

			float tempParallax = deNormData[0].y;
			if (tempParallax < 0) starHighlightData += "\nParallax: 0 (False Parallax: " + tempParallax + ")";
			else starHighlightData += "\nParallax: " + tempParallax;
		}

		GetComponent<TextMesh>().text = starHighlightData;*/

		SetText();
	}
	#endregion

	#region COMMENTED_CODE
	/*
	[ClientRpc]
	void RpcDestroy(){
		if (isServer) {
			Destroy (gameObject, 0.15f);
		} else {
			Destroy (gameObject);
		}
	}
	*/

	// why is this sending stuff?  I think its a dead end EGM
	/*
	[Command]
	void CmdSendTapOSC(){
		GameObject oscPacketBus = GameObject.FindGameObjectWithTag ("OSCPacketBus");
		//tODO send? EGM
		oscPacketBus.GetComponent<OSCPacketBus>().newTapIDStart (playerID, isLeftController, starID, starPos, intensity, angle);
	}
	*/
	/*
	 * In RpcSetText() 
	float tempRa = selectedPoint.x / 200.0f;
	tempRa = (tempRa + 1.0f) / 2.0f;
	tempRa = (tempRa * (85.33088371578594f - 77.928459f)) + 77.928459f;
	float tempDec = selectedPoint.z / 200.0f;
	tempDec = (tempDec + 0.51f) / 2.0f;
	tempDec = (tempDec * (85.33088371578594f - 77.928459f)) - 72.003059f;
	starHighlightData += "\nRa, Dec: (" + tempRa.ToString() + ", " + tempDec.ToString() + ")";
	starHighlightData += "\nMagnitude: " + hitMesh.colors[selIndex].r;
	starHighlightData += "\nStd Dev: " + hitMesh.colors[selIndex].g;
	starHighlightData += "\nNum Observations: " + hitMesh.colors[selIndex].b;
	int tempVariability = (int)(hitMesh.colors [selIndex].a);
	if (tempVariability == 3) {
		starHighlightData += "\nVariability: To Be Determined";
	} else {
		starHighlightData += "\nVariability: Confirmed";
	}
	starHighlightData += "\nCatalog: " + uv0List[selIndex].x;
	starHighlightData += "\nAstrometric Pseudocolor: " + uv0List[selIndex].y;
	starHighlightData += "\nPeriod: " + uv0List [selIndex].z;
	tempPeriod = uv0List [selIndex].z;
	starHighlightData += "\nPeriodogram SNR: " + uv0List [selIndex].w;
	starHighlightData += "\nLightcurve RMS: " + uv1List [selIndex].x;
	starHighlightData += "\nVariable Class: " + uv1List [selIndex].y;
	starHighlightData += "\nProper Motion: " + uv1List [selIndex].z;
	float tempParallax = (((hitMesh.vertices [selIndex].y / 0.125f) + 1.0f) / 2.0f);
	if (tempParallax < 0) {
		starHighlightData += "\nParallax: 0 (False Parallax: " + tempParallax + ")";
	} else {
		starHighlightData += "\nParallax: " + tempParallax;
	}

	GameObject dObj = GameObject.FindGameObjectWithTag ("dObj");

		if (dObj) {
			string[][] myValR = dObj.GetComponent<dataload>().val;
			Dictionary<string, int> myDictR = dObj.GetComponent<dataload>().dict;
			if (myDictR.ContainsKey (starID.ToString ())) {
				starHighlightData += "ID: " + starID.ToString ();
				float tempRa = starPos.x / 200.0f;
				tempRa = (tempRa + 1.0f) / 2.0f;
				tempRa = (tempRa * (85.33088371578594f - 77.928459f)) + 77.928459f;
				float tempDec = starPos.z / 200.0f;
				tempDec = (tempDec + 0.51f) / 2.0f;
				tempDec = (tempDec * (85.33088371578594f - 77.928459f)) - 72.003059f;
				starHighlightData += "\nRa, Dec: (" + tempRa.ToString() + ", " + tempDec.ToString() + ")";
				starHighlightData += "\nMagnitude: " + float.Parse (myValR [myDictR [starID.ToString ()]] [1]);
				starHighlightData += "\nStd Dev: " + float.Parse (myValR [myDictR [starID.ToString ()]] [2]);
				starHighlightData += "\nNum Observations: " + float.Parse (myValR [myDictR [starID.ToString ()]] [3]);
				int tempVariability = int.Parse (myValR [myDictR [starID.ToString ()]] [4]);
				if (tempVariability == 3) {
					starHighlightData += "\nVariability: To Be Determined";
				} else {
					starHighlightData += "\nVariability: Confirmed";
				}
				starHighlightData += "\nCatalog: " + (myValR [myDictR [starID.ToString ()]] [5]).Replace(";",", ");
				starHighlightData += "\nAstrometric Pseudocolor: " + float.Parse (myValR [myDictR [starID.ToString ()]] [6]);
				starHighlightData += "\nPeriod: " + float.Parse (myValR [myDictR [starID.ToString ()]] [7]) + " days";
				starHighlightData += "\nPeriodogram SNR: " + float.Parse (myValR [myDictR [starID.ToString ()]] [8]);
				starHighlightData += "\nLightcurve RMS: " + float.Parse (myValR [myDictR [starID.ToString ()]] [9]);
				starHighlightData += "\nVariable Class: " + myValR [myDictR [starID.ToString ()]] [10];
				starHighlightData += "\nProper Motion: " + float.Parse (myValR [myDictR [starID.ToString ()]] [11]);
				float tempParallax = float.Parse (myValR [myDictR [starID.ToString ()]] [12]);
				if (tempParallax < 0) {
					starHighlightData += "\nParallax: 0 (False Parallax: " + tempParallax + ")";
				} else {
					starHighlightData += "\nParallax: " + tempParallax;
				}
			}
		}
	 */
	#endregion
}
