using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
<summary>
	CD : picLoadTesterScript
	Loads data from database and Sets text on hemisphere? :
	:: Zero references anywhere at all, NDH
</summary>
**/
public class picLoadTesterScript : MonoBehaviour 
{
    #region UNITY_FUNC
	/**
	<summary>
		FD : Start()
		Start Coroutine LoadStuff
	</summary>
	**/
    void Start () 
	{
		StartCoroutine(LoadStuff());
	}
	#endregion

	#region PRIVATE_FUNC
	/**
	<summary>
		FD : LoadStuff()
		Set id to 40000001 and init star4Data, star2Data, and gaiaData
		Load data from url and split text on space
		Next is a series of loops, each iterating through the split up text, val
			If string Length > 5: array at i * 2  = i in set substring starting at 5
			Else set equal to error message
			array i * 2 + 1 = i in set + 1 and replace all "_" with " " and add a "\n"
			Loop 1
				star4Data[i * 2] => val[5, 7, 9...27, 28, 29]
				star4Data[i * 2 + 1] => val[6, 8, 10...28, 30, 32]
			Loop 2
				star2Data[i * 2] => val[38, 40, 42...62, 64, 66]
				star2Data[i * 2 + 1] => val[39, 41, 43...63, 65, 67]
			Loop 3
				gaiaData[i * 2] => val[73, 75, 77...103, 105, 107]
				gaiaData[i * 2 + 1] => val[74, 76, 78...104, 106, 108]
		Set graphURL equal to the last element in val starting at the 32nd char
		Add Headers to each array
		Get the TextMeshes from this gameObjects children
		For every textMesh, If the parent isn't null, and If character size isn't .003
			Set text of textMesh to the combined string of all string arrays
		Load data from the graphURL and create a new sprite based off of the loaded texture
	</summary>
	**/
	IEnumerator LoadStuff()
	{
		int id = 40000001;
		string[] star4Data = new string[28];
		string[] star2Data = new string[30];
		string[] gaiaData = new string[36];

		//string loadURL = "http://192.168.1.129/ioan_newidtester.php?id=" + id;
		string loadURL = "http://192.168.1.129/ioan_newidtester_fig.php?id=" + id.ToString();
		WWW loadID = new WWW(loadURL);
		yield return (loadID);
		string[] val = loadID.text.Split(' ');

		for (int i = 0; i < 14; i++)
		{
			if (val[(i * 2) + 5].Length > 5) star4Data[i * 2] = val[(i * 2) + 5].Substring(5);
			else star4Data[i * 2] = "db error";
			
			star4Data[i * 2 + 1] = " - " + val[(i * 2) + 6].Replace("_", " ") + "\n";
		}

		for (int i = 0; i < 15; i++)
		{
			if (val[(i * 2) + 38].Length > 5) star2Data[i * 2] = val[(i * 2) + 38].Substring(5);
			else star2Data[i * 2] = "db error";

			star2Data[i * 2 + 1] = " - " + val[(i * 2) + 39].Replace("_", " ") + "\n";
		}

		for (int i = 0; i < 18; i++)
		{
			if (val[(i * 2) + 73].Length > 5) gaiaData[i * 2] = val[(i * 2) + 73].Substring(5);
			else gaiaData[i * 2] = "db error";

			gaiaData[i * 2 + 1] = " - " + val[(i * 2) + 74].Replace("_", " ") + "\n";
		}

		string graphURL = val[109].Substring(32);

		star4Data[0] = "-- AST3 --\n" + star4Data[0];
		star2Data[0] = "-- SIMBAD --\n" + star2Data[0];
		gaiaData[0] = "-- GAIA --\n" + gaiaData[0];

		TextMesh[] textMeshList = gameObject.GetComponentsInChildren<TextMesh>();

		foreach (TextMesh myTextMesh in textMeshList) 
			if (myTextMesh.gameObject.transform.parent != null) 
				if (myTextMesh.characterSize != 0.003f)
					myTextMesh.text = string.Join("", star4Data) + string.Join("", star2Data) + string.Join("", gaiaData);

		loadID = new WWW(graphURL);
		yield return (loadID);
		Sprite graphSprite = Sprite.Create(loadID.texture, new Rect(0, 0, loadID.texture.width, loadID.texture.height), new Vector2(0, 0));
		SpriteRenderer childSprite = GetComponentInChildren<SpriteRenderer>();
		childSprite.sprite = graphSprite;
	}
	#endregion

	#region COMMENTED_CODE
	void Update()
	{

	}
	#endregion
}
