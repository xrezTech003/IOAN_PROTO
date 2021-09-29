using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

/**
<summary>
	CD : newdbtester
	Class of coroutines for collecting data from database :
	:: I srongly disbelieve this thing does anything dataload doesn't :
	:: It is only referenced by controller_cam, which is found on hlasphere, a fully inactive object in the unity scene :
	:: Almost certainly dead aside from moderate activity in canvasNewNet I refuse to figure out - NDH
</summary>
**/
public class newdbtester : MonoBehaviour 
{
	#region PUBLIC_VAR
	/// <summary>
	///		VD : loadingFrame
	/// </summary>
	public bool loadingFrame = false;

	/// <summary>
	///		VD : starSelected
	/// </summary>
	public bool starSelected = false;

	/// <summary>
	///		Group : GameObjects
	///		Members : newCanvasReference, myText, myText1, myText2, myLine, myLine1, myLine2, myPanel, myPanel1, myPanel2, myPanel3, myImg3, myCursor, frontStar, frontRay, idLabel
	/// </summary>
	public GameObject newCanvasReference, myText, myText1, myText2, myLine, myLine1, myLine2, myPanel, myPanel1, myPanel2, myPanel3, myImg3, myCursor, frontStar, frontRay, idLabel;

	/// <summary>
	///		Group : Labels and Data
	///		Members : star4Labels, star4Data, star2Labels, star2Data, gaiaLabels, gaiaData
	/// </summary>
	public string[] star4Labels, star4Data, star2Labels, star2Data, gaiaLabels, gaiaData;

	/// <summary>
	///		Group : Active Coroutines
	///		Members : coaRoutActive, cobRoutActive, cocRoutActive
	/// </summary>
	public bool coaRoutActive, cobRoutActive, cocRoutActive;

	/// <summary>
	///		VD : dcCallTimer
	/// </summary>
	public float dbCallTimer;

	/// <summary>
	///		VD : currID
	/// </summary>
	public int currID;

	/// <summary>
	///		VD : starOrigin
	/// </summary>
	public Vector3 starOrigin;

	/// <summary>
	///		VD : databaseIP
	/// </summary>
	public string databaseIP;
	#endregion

	#region PRIVATE_VAR
	/// <summary>
	///		VD : updateTicker ::: Dead - NDH
	/// </summary>
	private float updateTicker = 0.02f;

	/// <summary>
	///		VD : Alphabet
	///		Alphanumeric string 
	/// </summary>
	private const string Alphabet = "abcdefghijklmnopqrstuvwyxzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

	/// <summary>
	///		VD : cTick :: Dead NDH
	/// </summary>
	private const float cTick = 0.006f;

	/// <summary>
	///		Group : Coroutines
	///		Members : coa, cob, coc
	/// </summary>
	private IEnumerator coa, cob, coc;

	/// <summary>
	///		VD : rand
	///		random variables
	/// </summary>
	System.Random rand;
	#endregion

	#region UNITY_FUNC
	/**
	<summary>
		FD : Start()
		Set v:databaseIP to config data
		Init public Values
	</summary>
	**/
	void Start()
	{
		Config config = Config.Instance;
        databaseIP = config.Data.dataBaseIP;

		starOrigin = Vector3.zero;
		currID = 0;

		star4Labels = new string[23];
		star4Data = new string[23];
		star2Labels = new string[26];
		star2Data = new string[26];
		gaiaLabels = new string[31];
		gaiaData = new string[31];

		coaRoutActive = false;
		cobRoutActive = false;
		cocRoutActive = false;

		dbCallTimer = 0.0f;

		rand = new System.Random();
	}

	/**
	<summary>
		FD : Update()
		Decrement updateTicker and dbCallTimer with deltaTime
		Set dbCallTimer to 0 if it is < 0
	</summary>
	**/
	void Update()
	{
		updateTicker -= Time.deltaTime;
		dbCallTimer -= Time.deltaTime;

		if (dbCallTimer < 0.0f) dbCallTimer = 0.0f;
	}
	#endregion

	#region PUBLIC_FUNC
	/**
	<summary>
		FD : IDLoader(int)
		Set starSelected to true
		Set currID to id
		Call the three coroutines with id using the Coroutine variables
		<param name="id">Star Id to look up</param>
	</summary>
	**/
	public void IDLoader(int id)
	{
		starSelected = true;
		currID = id;

		coa = LoadIDStuff(id);
		cob = LoadFrontStar(id);
		coc = LoadIDLabel(id);

		StartCoroutine(coa);
		StartCoroutine(cob);
		StartCoroutine(coc);
	}

	/**
	<summary>
		FD : IDStopper()
		Set starSelected to false and currID to 0
		Call IDStopper on canvas new net if newCanvas Reference isn't null
		Stop all coroutines and set the Active coroutines to false
		Set loading frame to false
		Clear all Labels and Data arrays
	</summary>
	**/
	public void IDStopper()
	{
		starSelected = false;
		currID = 0;

		if (newCanvasReference)
		{
			var cScript = newCanvasReference.GetComponent<canvasNewNet>();
			cScript.IDStopper(); //newCanvasReference.GetComponent<canvasNewNet>().IDStopper();
		}

		StopAllCoroutines();
		coaRoutActive = false;
		cobRoutActive = false;
		cocRoutActive = false;

		loadingFrame = false;
		//loadingFrame1 = false;
		//loadingFrame2 = false;

		System.Array.Clear(star2Data, 0, star2Data.Length);
		System.Array.Clear(star2Labels, 0, star2Labels.Length);
		System.Array.Clear(star4Data, 0, star4Data.Length);
		System.Array.Clear(star4Labels, 0, star4Labels.Length);
		System.Array.Clear(gaiaData, 0, gaiaData.Length);
		System.Array.Clear(gaiaLabels, 0, gaiaLabels.Length);
	}

	/**
	<summary>
		FD : GenerateString(int)
		Generate random string of Length size
		<param name="size"></param>
	</summary>
	**/
	public string GenerateString(int size)
	{
		char[] chars = new char[size];

		for (int i = 0; i < size; i++) chars[i] = Alphabet[rand.Next(Alphabet.Length)];

		return new string(chars);
	}
	#endregion

	#region PRIVATE_FUNC
	/**
	<summary>
		FD : LoadIDStuff(int)
		Set coaRoutine to false
		If v:newCanvasReference isn't null
			If v:loadingFrame is false
				Set all canvasNewNet panel0-3 to the color clear?
				Set v:loadingFrame to true and Call startLoadingFrames() on v:newCanvasReference
				Construct URL from v:databaseIP and id
				Increment dbCallTimer with 2
				Load from URL
				Get the text from URL Data and break it up by spaces
				The next part of the code loops through strings from the url at different points
					If the strings Length > 5: Set label to substring starting at 5
					Else set label to error message
					Set data to a different set
					Loop 1
						v:star4Labels <= val[3, 5, 7...43, 45, 47]
						v:star4Data <= val[4, 6, 8...44, 46, 48]
					Loop 2
						v:star2Labels <= val[52, 54, 56...98, 100, 102]
						v:star2Data <= val[53, 55, 57...99, 101, 103]
					Loop 3
						v:gaiaLabels <= val[107, 109, 111...163, 165, 167]
						v:gaiaData <= val[108, 110, 112...164, 166, 168]
				Call LoadIDStuff() on the v:newCanvasReference
				Set loadingFrame to false
		Set coaRoutineActive to false
		<param name="id"></param>
	</summary>
	**/
	IEnumerator LoadIDStuff(int id)
	{
		coaRoutActive = true;

		if (newCanvasReference)
		{
			var cScript = newCanvasReference.GetComponent<canvasNewNet>();

			if (!loadingFrame)
			{
				//Image myPImage = myPanel.GetComponent<Image> ();
				//Image myPImage1 = myPanel1.GetComponent<Image> ();
				//Image myPImage2 = myPanel2.GetComponent<Image> ();
				//Image myPImage3 = myPanel3.GetComponent<Image> ();
				//Image myCursorImage = myCursor.GetComponent<Image> ();


				cScript.panel = new Color(1.0f, 1.0f, 1.0f, 0.0f); //Color.clear?
				cScript.panel1 = new Color(1.0f, 1.0f, 1.0f, 0.0f);
				cScript.panel2 = new Color(1.0f, 1.0f, 1.0f, 0.0f);
				cScript.panel3 = new Color(1.0f, 1.0f, 1.0f, 0.0f);

				loadingFrame = true;
				//loadingFrame1 = true;
				//loadingFrame2 = true;
				cScript.startLoadingFrames();

				//string loadURL = "http://localhost/newidtester.php?id=" + id;
				string loadURL = "http://" + databaseIP + "/ioan_newidtester.php?id=" + id;
				dbCallTimer += 2.0f;

				WWW loadID = new WWW(loadURL);
				yield return loadID;

				string valAR = loadID.text;
				string[] val = valAR.Split(' ');

				print("Selected ID " + id.ToString());

				for (int i = 0; i < 23; i++)
				{
					if (val[(i * 2) + 3].Length > 5) star4Labels[i] = val[(i * 2) + 3].Substring(5);
					else star4Labels[i] = "db error";

					star4Data[i] = val[(i * 2) + 4];
				}

				for (int i = 0; i < 26; i++)
				{
					if (val[(i * 2) + 52].Length > 5) star2Labels[i] = val[(i * 2) + 52].Substring(5);
					else star2Labels[i] = "db error";

					star2Data[i] = val[(i * 2) + 53];
				}

				for (int i = 0; i < 31; i++)
				{
					if (val[(i * 2) + 107].Length > 5) gaiaLabels[i] = val[(i * 2) + 107].Substring(5);
					else gaiaLabels[i] = "db error";

					gaiaData[i] = val[(i * 2) + 108];
				}

				//Text myTextField = myText.GetComponent<Text> ();
				//Text myTextField1 = myText1.GetComponent<Text> ();
				//Text myTextField2 = myText2.GetComponent<Text> ();
				//Image myImage = myLine.GetComponent<Image> ();
				//Image myImage1 = myLine1.GetComponent<Image> ();
				//Image myImage2 = myLine2.GetComponent<Image> ();
				//Image myImage3 = myImg3.GetComponent<Image> ();


				int graphID = Random.Range(0, 16);
				cScript.loadIDStuff(star4Labels, star4Data, star2Labels, star2Data, gaiaLabels, gaiaData, graphID, id.ToString());

				loadingFrame = false;
				//loadingFrame1 = false;
				//loadingFrame2 = false;
			}
		}

		coaRoutActive = false;
	}

	/**
	<summary>
		FD : LoadFrontStar(int)
		Set cobRoutActive to true
		Seed Random with id and generate random rgb values for a new color
		Get renderers for v:frontStar and v:frontRay
		Set v:frontStar material color to random color with alpha at .3f
		Set v:frontStar localposition to constant
		For 40 iterations
			Have the localPosition of v:frontStar sink
			Have the alpha of the v:frontRay color reduce
			Wait for .005 seconds
		For 50 iterations
			Have v:frontStar color alpha grow
			Wait for .005 seconds
		Set cobRoutActive to false
		<param name="id"></param>
	</summary>
	**/
	IEnumerator LoadFrontStar(int id)
	{
		cobRoutActive = true;

		Color myCol = new Color(1.0f, 1.0f, 1.0f); //Color.white
		Random.InitState(id);
		myCol.r = Random.Range(0.0f, 1.0f);
		myCol.g = Random.Range(0.0f, 1.0f);
		myCol.b = Random.Range(0.0f, 1.0f);

		MeshRenderer mRend = frontStar.GetComponent<MeshRenderer>();
		MeshRenderer rRend = frontRay.GetComponent<MeshRenderer>();

		mRend.material.color = new Color(myCol.r, myCol.g, myCol.b, 0.3f); //myCol.a = .3f; mRend.material.color = myCol

		//frontStar.transform.localScale = new Vector3 (0.02f, 0.02f, 0.02f);
		frontStar.transform.localPosition = new Vector3(-0.001f, 0.0f, 10.13f);

		for (int i = 0; i < 40; i++)
		{
			//float tScale = 0.02f + ((float)i*(0.08f/20.0f));
			//frontStar.transform.localScale = new Vector3(tScale,tScale,tScale);

			float tPos = 10.13f - ((float)(i + 1) * (10.0f / 40.0f));
			frontStar.transform.localPosition = new Vector3(-0.001f, 0.0f, tPos);

			Color rCol = new Color(1.0f, 1.0f, 1.0f, (80.0f / 255.0f) - (i + 1) * (80.0f / 255.0f) / 40.0f);
			rRend.material.color = rCol;

			yield return new WaitForSeconds(0.005f);
		}

		for (int i = 0; i < 50; i++)
		{
			mRend.material.color = new Color(myCol.r, myCol.g, myCol.b, 0.3f + ((float)(i + 1) / 50.0f) * 0.7f);
			yield return new WaitForSeconds(0.005f);
		}

		cobRoutActive = false;
	}

	/**
	<summary>
		FD : LoadIDLabel(int)
		Does nothing except toggle cocRoutActive and wait .0001 seconds
		<param name="id"></param>
	</summary>
	**/
	IEnumerator LoadIDLabel(int id)
	{
		cocRoutActive = true;
		yield return new WaitForSeconds(0.0001f);
		cocRoutActive = false;
	}
	#endregion

	#region COMMENTED_CODE
	//private bool loadingFrame1 = false;
	//private bool loadingFrame2 = false;

	/*
	 * In Update 
	 *
	if (newCanvasReference)
		{
		}
	 */
	#endregion
}
