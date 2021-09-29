using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;
using System.Linq;
using System;
using System.Collections.Generic;
//Oof
/// <summary>
/// CD: "Old method" of reading .msh files to extract star data from vertex, colour, normals, and UV information NDH
/// </summary>
public class StarSerializer : MonoBehaviour
{
	#region VARIABLES

	public string[] myEntries;
	public string[] vXList;
	public string[] vYList;
	public int[] newTriangles;
	public int[] myIDs;
	public int meshIDX;
	public int meshIDY;

	/// <summary>
	/// VD: file path placeholder to be looped with to load all the .msh files
	/// </summary>
	public string meshName;
	/// <summary>
	/// VD: GO reference to initializer, links this script to c:initializer and c:shadeSwapper
	/// </summary>
	private GameObject myInit;
	/// <summary>
	/// VD: GO reference to config, links this script to c:Config and c:GlobalLoadSkip
	/// </summary>
	public GameObject configRef;



	#region UNUSED VARIABLES

	/// <summary>
	/// VD: Unused
	/// </summary>
	public string[] tAList;
	/// <summary>
	/// VD: Unused
	/// </summary>
	public string[] tBList;
	/// <summary>
	/// VD: Unused
	/// </summary>
	public string[] tCList;
	/// <summary>
	/// VD: Unused
	/// </summary>
	public Vector3 meshCoord;
	/// <summary>
	/// VD: Unused
	/// </summary>
	private string theReader;
	//public Material newMaterialRef;
	//public Shader myShader;

	#endregion

	#endregion

	#region UNITY FUNCTIONS

	/// <summary>
	/// FD: assigns config reference
	/// </summary>
	void Start()
	{
		configRef = GameObject.Find("Config");
	}

	/// <summary>
	/// FS: Constantly checks in with ShadeSwapper to keep the individual mesh's material up to speed
	/// </summary>
	void Update()
	{
		///<remarks>Fails if Initializer reference not working</remarks>
		if (myInit)
		{
			gameObject.GetComponent<MeshRenderer>().SetPropertyBlock(myInit.GetComponent<shadeSwapper>().mMat);
		}
		else
		{
			myInit = GameObject.FindGameObjectWithTag("init");
			if (!myInit)
			{
				Debug.Log("error - init object not found!");
			}
		}
	}

	#endregion

	#region PRIVATE/PUBLIC FUNCTIONS

	/// <summary>
	/// FD: Called by NewStart to load the .msh file into v:myEntries[] line by line
	/// </summary>
	/// <param name="fileName">VD: Self explanitory</param>
	/// <returns>VD: GS: Uses boolean to declare when it is done</returns>
	private bool LoadFile(string fileName)
	{
		// Handle any problems that might arise when reading the text
		string line;
		int lnCounter = 0;
		// Create a new StreamReader, tell it which file to read and what encoding the file
		// was saved as
		StreamReader starSerialReader = new StreamReader(fileName, Encoding.Default);
		// Immediately clean up the reader after this block of code is done.
		// You generally use the "using" statement for potentially memory-intensive objects
		// instead of relying on garbage collection.
		// (Do not confuse this with the using directive for namespace at the 
		// beginning of a class!)
		///<remarks>GS: This bit of code feels straight off a tutorial</remarks>
		using (starSerialReader)
		{
			// While there's lines left in the text file, do this:
			do
			{
				line = starSerialReader.ReadLine();
				if (line != null)
				{
					string[] entries = line.Split('\n');
					if (entries.Length > 0)
						myEntries[lnCounter] = entries[0];
					lnCounter++;
				}
			}
			while (line != null);
			// Done reading, close the reader and return true to broadcast success    
			starSerialReader.Close();
			return true;
		}
	}

	/// <summary>
	/// FD: Reads the Array data from v:myEntries
	/// </summary>
	/// <param name="curStarCount"></param>
	/// <param name="clothHeight"></param>
	/// <param name="IDX">VD: Value between 1 and 5, X coordinate in organized grid of mesh parts</param>
	/// <param name="IDY">VD: Value between 1 and 5, Y coordinate in organized grid of mesh parts</param>
	/// <returns>VD: A count of the number of stars in loaded, based on vertex data in the .msh</returns>
	public int NewStart(int curStarCount, float clothHeight, int IDX, int IDY)
	{
		meshIDX = IDX; //IV: This is plain silly
		meshIDY = IDY; //IV: Both of these sets of names get referneced this could be 2 instead of 4
		int meshLines = 150000;

		myEntries = new string[meshLines];
		LoadFile(meshName);
		char[] splitters = { ' ' };

		///<remarks>Declare state varibles - scan state, offset for stream to start scanning past verticies, and offset for stream to start scanning past triangles</remarks>
		int scanState = 0;
		int offV = 0;
		int offT = 0;
		for (int ln = 0; ln < myEntries.Length; ln++)
		{
			///<remarks> Scan state 0, read the header for verticies to check how many verticies to scan for, change to state 1</remarks>
			if (scanState == 0)
			{
				if (myEntries[ln].Split(splitters, System.StringSplitOptions.RemoveEmptyEntries).Length > 3)
				{
					if (myEntries[ln].Split(splitters, System.StringSplitOptions.RemoveEmptyEntries)[3]
						== "vertices")
					{
						vXList = new string[int.Parse(myEntries[ln].Split(splitters, System.StringSplitOptions.RemoveEmptyEntries)[0])];
						vYList = new string[int.Parse(myEntries[ln].Split(splitters, System.StringSplitOptions.RemoveEmptyEntries)[0])];
						myIDs = new int[int.Parse(myEntries[ln].Split(splitters, System.StringSplitOptions.RemoveEmptyEntries)[0])];
						offV = ln + 1;
						scanState = 1;
					}
				}
			}
			///<remarks>Scan state 1, while the number of lines we have scanned is less then the length of the vertex number, build the x vertex list, y vertex list, and astrological ID list with matching index's, switch to state 2 when done</remarks>
			else if (scanState == 1)
			{
				if (ln - offV >= vXList.GetLength(0))
				{
					scanState = 2;
				}
				else
				{
					vXList[ln - offV] = myEntries[ln].Split(splitters, System.StringSplitOptions.RemoveEmptyEntries)[0];
					vYList[ln - offV] = myEntries[ln].Split(splitters, System.StringSplitOptions.RemoveEmptyEntries)[1];
					myIDs[ln - offV] = int.Parse(myEntries[ln].Split(splitters, System.StringSplitOptions.RemoveEmptyEntries)[2]);
				}
			}
			///remarks>Scan state 2, read the header for triangles to check how many triangles to scan for, change to state 3
			else if (scanState == 2)
			{
				if (myEntries[ln].Split(splitters, System.StringSplitOptions.RemoveEmptyEntries).Length > 3)
				{
					if (myEntries[ln].Split(splitters, System.StringSplitOptions.RemoveEmptyEntries)[3]
						== "triangles")
					{
						newTriangles = new int[3 * int.Parse(myEntries[ln].Split(splitters, System.StringSplitOptions.RemoveEmptyEntries)[0])];
						offT = ln + 1;
						scanState = 3;
					}
				}
			}
			///<remarks>Scan state 3, while the number of lines we have scanned is less then the length of the vertex number, build the traingle list, switch to state 4 when done</remarks>
			else if (scanState == 3)
			{
				if (ln - offT >= (newTriangles.Length / 3))
				{
					scanState = 4;
				}
				else
				{
					for (int i = 0; i < 3; i++)
					{
						newTriangles[((ln - offT) * 3) + i] = int.Parse(myEntries[ln].Split(splitters, System.StringSplitOptions.RemoveEmptyEntries)[i]) - 1;
					}
				}
			}
			///<remarks>Scan State 4, move on</remarks>
			else if (scanState == 4)
			{
				break;
			}
		}
		///<remarks>Update the number of stars loaded</remarks>
		curStarCount = curStarCount + vXList.GetLength(0);

		///<remarks>Create vertex, UV, index, and colour lists the size of the number of included verticies found the stream</remarks>
		Vector3[] newVertices = new Vector3[vXList.GetLength(0)];
		List<Vector4> newUV = new List<Vector4>(new Vector4[vXList.GetLength(0)]);
		List<Vector4> newUV2 = new List<Vector4>(new Vector4[vXList.GetLength(0)]);
		int[] indices = new int[vXList.GetLength(0)];
		Color[] colors = new Color[vXList.GetLength(0)];


		GameObject dObj = GameObject.FindGameObjectWithTag("dObj");

		for (int i = 0; i < vXList.GetLength(0); i++)
		{

			///<remarks>Set and reset the necessary variables to create a star on the mesh</remarks>

			float ra = 0.0f;
			float dec = 0.0f;
			float meanMag = 0.0f;
			float magSTD = 0.0f;
			float numObs = 0f;
			float variability = 0f;
			float catalog = 0f;
			float astroColor = 0.0f;
			float period = 0.0f;
			float periodSNR = 0.0f;
			float lightcurveRMS = 0.0f;
			float varClass = 0f;
			float properMotion = 0.0f;
			float parsecs = 0.0f;

			///<remarks>convert the number stored at the index of the loop</remarks>
			newVertices[i].x = ((float.Parse(vXList[i]))) * 200.0f;
			newVertices[i].z = ((float.Parse(vYList[i]))) * 200.0f;
			newVertices[i].y = 0.0f;

			///<remarks><example><code>ra = (((float.Parse(vXList[i]) + 1.0f) / 2.0f) * (85.33088371578594f - 77.928459f)) +77.928459f; dec = (((float.Parse(vYList[i]) + 0.51f) / 2.0f) * (85.33088371578594f - 77.928459f)) - 72.003059f; </code></example></remarks>
			ra = (float.Parse(vXList[i]) + 1.0f) / 2.0f;
			ra = (ra * (85.33088371578594f - 77.928459f)) + 77.928459f;
			dec = (float.Parse(vYList[i]) + 0.51f) / 2.0f;
			dec = (dec * (85.33088371578594f - 77.928459f)) - 72.003059f;

			///<remarks></remarks>
			if (dObj)
			{
				///<remarks>grab the star data from dataLoad</remarks>
				string[][] myValR = dObj.GetComponent<DataLoad>().val;
				Dictionary<string, int> myDictR = dObj.GetComponent<DataLoad>().dict;
				///<remarks>Check the dictionary for the star IDs in the subMesh</remarks>
				if (myDictR.ContainsKey(myIDs[i].ToString()))
				{
					///<remarks>Read the values from c:DataLoad v:val that correspond to the index plucked from v:dict via the starID and set them to the declared star variables</remarks>
					meanMag = float.Parse(myValR[myDictR[myIDs[i].ToString()]][1]);
					magSTD = float.Parse(myValR[myDictR[myIDs[i].ToString()]][2]);
					numObs = float.Parse(myValR[myDictR[myIDs[i].ToString()]][3]);
					variability = float.Parse(myValR[myDictR[myIDs[i].ToString()]][4]);
					string tempCat = myValR[myDictR[myIDs[i].ToString()]][5];
					if (tempCat.Contains("AST3"))
					{
						catalog += 1f;
					}
					if (tempCat.Contains("SIMBAD"))
					{
						catalog += 2f;
					}
					if (tempCat.Contains("GAIADr2"))
					{
						catalog += 4f;
					}
					astroColor = float.Parse(myValR[myDictR[myIDs[i].ToString()]][6]);
					period = float.Parse(myValR[myDictR[myIDs[i].ToString()]][7]);
					periodSNR = float.Parse(myValR[myDictR[myIDs[i].ToString()]][8]);
					lightcurveRMS = float.Parse(myValR[myDictR[myIDs[i].ToString()]][9]);
					if (myValR[myDictR[myIDs[i].ToString()]][10] == "unknown")
					{
						varClass = 0f;
					}
					else
					{
						varClass = 1f;
					}
					properMotion = float.Parse(myValR[myDictR[myIDs[i].ToString()]][11]);
					parsecs = float.Parse(myValR[myDictR[myIDs[i].ToString()]][12]);
				}
			}

			///<remarks>Normalize all the values</remarks>
			meanMag = NormalizeToRange(meanMag, 14.886403072230165f, 19.061404043487336f);
			magSTD = NormalizeToRange(magSTD, 0f, 0.6680229918890426f);
			numObs = normalizePower(numObs, 0f, 3125.7502367831025f, 922.3292f, 0.5f);
			variability = NormalizeToRange(variability, 1f, 3f);
			catalog = NormalizeToRange(catalog, 0f, 7f);
			astroColor = NormalizeToRange(astroColor, 1.231199705120561f, 1.8991459147993115f);
			period = NormalizeToRange(period, 0.263666f, 36.09744f);
			periodSNR = NormalizeToRange(periodSNR, 0f, 26.61842977741538f);
			lightcurveRMS = NormalizeToRange(lightcurveRMS, 0f, 0.6715103569670999f);
			varClass = NormalizeToRange(varClass, 0f, 1f);
			properMotion = normalizePower(properMotion, 0, 11.408797042686029f, 3.138575f, 0.5f);
			parsecs = parsecs != 0 ? 1f / parsecs : 999999f;
			parsecs = NormalizeToRange(parsecs, -6.172524f, 4.204787f);
			parsecs = Mathf.Clamp01(parsecs);

			///<remarks>Set the star r, g, b, and a to it's magnitude, magSTD, numObs, and variability, which are normalized</remarks>
			colors[i].r = meanMag;
			colors[i].g = magSTD;
			colors[i].b = numObs;
			colors[i].a = variability;
			///<remarks>Set's the 2 UVs to catalog, astroColor, period, and periodSNR, then, lightcurveRMS, varClass, properMotion, and ID, which are normalized</remarks>
			newUV[i] = new Vector4(catalog, astroColor, period, periodSNR);
			newUV2[i] = new Vector4(lightcurveRMS, varClass, properMotion, myIDs[i]);
			///<remarks>separate the stars by a default separation constant</remarks>
			float parsecSeparation = 0.28f;
			newVertices[i].y = (parsecs - 0.5f) * parsecSeparation;
			indices[i] = i;

		}

		///<remarks> Use the newly composed arrays of converted star information to create a meshrenderer with said settings</remarks>
		gameObject.AddComponent<MeshFilter>();
		Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;
		mesh.Clear();
		mesh.vertices = newVertices;
		mesh.SetUVs(0, newUV);
		mesh.SetUVs(1, newUV2);
		List<Vector4> tempVList = new List<Vector4>();
		mesh.GetUVs(0, tempVList);
		mesh.triangles = newTriangles;
		mesh.RecalculateNormals();
		Vector3[] normals = mesh.normals;
		for (int i = 0; i < normals.Length; i++)
		{
			normals[i] = -normals[i];
		}
		mesh.normals = normals;
		mesh.colors = colors;
		mesh.SetIndices(indices, MeshTopology.Points, 0);
		mesh.bounds = new Bounds(transform.position, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));

		Mesh mesh2 = new Mesh();
		mesh2.Clear();
		mesh2.vertices = newVertices;
		mesh2.SetUVs(0, newUV);
		mesh2.triangles = newTriangles;
		mesh2.RecalculateNormals();

		for (int m = 0; m < mesh2.subMeshCount; m++)
		{
			int[] triangles = mesh2.GetTriangles(m);
			for (int i = 0; i < triangles.Length; i += 3)
			{
				int temp = triangles[i + 0];
				triangles[i + 0] = triangles[i + 1];
				triangles[i + 1] = temp;
			}
			mesh2.SetTriangles(triangles, m);
		}
		///<remarks>Set either the HQ shader or LQ shader, based on parameter within GlobalLoadSkip (needs init object)</remarks>
		MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>() as MeshRenderer;
		myInit = GameObject.FindGameObjectWithTag("init");
		if (myInit)
		{
			if ((meshIDX >= Config.Instance.Data.meshHQRangeX.x &&
				meshIDX <= Config.Instance.Data.meshHQRangeX.y) &&
				(meshIDY >= Config.Instance.Data.meshHQRangeZ.x &&
					meshIDY <= Config.Instance.Data.meshHQRangeZ.y))
			{
				renderer.sharedMaterial = myInit.GetComponent<shadeSwapper>().hqMat;
			}
			else
			{
				renderer.sharedMaterial = myInit.GetComponent<shadeSwapper>().lowMat;
			}
		}
		else
		{
			Debug.Log("error - init object not found!");
		}
		//TESTING
		///<remarks>Turn off lighting for the created meshes, set the collider to be the stars themselves, then center the mesh to zero (GS: must be offset to correct position by design)</remarks>
		renderer.receiveShadows = false;
		renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
		renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
		//TESTING
		MeshCollider collider = gameObject.GetComponent<MeshCollider>();
		collider.sharedMesh = mesh2;
		gameObject.transform.position = new Vector3(0.0f, 0.0f, 0.0f);

        /*
		///<remarks>If c:GlobalLoadSkip bool bakeNewMeshes set publicly to true, make new bakedmesh.obj files</remarks>
		// REVERT WHEN SAVING!
		if (GameObject.Find("Config").GetComponent<GlobalLoadSkip>().bakeNewMeshes)
		{
			using (StreamWriter sw = new StreamWriter("Assets/bakedMeshes/meshBake" + IDX.ToString() + "-" + IDY.ToString() + ".obj"))
			{
				sw.Write(MeshToString(gameObject.GetComponent<MeshFilter>(), mesh, newTriangles, newUV, newUV2, myIDs));
			}
		}
        */

		// END

		return curStarCount;
	}

	/// <summary>
	/// FD: Helper function to convert the created submesh into a .obj file by formatting it into a string to be written externally
	/// </summary>
	/// <param name="mf">VD: The mesh filter to be written</param>
	/// <param name="m">VD: The mesh to be written</param>
	/// <param name="tris">VD: Triangle list used to create the mesh</param>
	/// <param name="inUV1">VD: Triangle list used to create the mesh</param>
	/// <param name="inUV2">VD: UV1 list used to create the mesh</param>
	/// <param name="inIDs">VD: UV2 list used to create the mesh</param>
	/// <returns>A bigass string</returns>
	public static string MeshToString(MeshFilter mf, Mesh m, int[] tris, List<Vector4> inUV1, List<Vector4> inUV2, int[] inIDs)
	{
		Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;

		StringBuilder sb = new StringBuilder();

		sb.Append("g ").Append(mf.name).Append("\n");
		foreach (Vector3 v in m.vertices)
		{
			sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
		}
		sb.Append("\n");
		foreach (Vector3 v in m.normals)
		{
			sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
		}
		sb.Append("\n");
		foreach (Vector4 v in m.colors)
		{
			sb.Append(string.Format("vc {0} {1} {2} {3}\n", v.x, v.y, v.z, v.w));
		}
		sb.Append("\n");
		foreach (Vector4 v in inUV1)
		{
			sb.Append(string.Format("vt {0} {1} {2} {3}\n", v.x, v.y, v.z, v.w));
		}
		sb.Append("\n");
		for (int j = 0; j < inUV2.Count; j++)
		{
			sb.Append(string.Format("vt1 {0} {1} {2} ", inUV2[j].x, inUV2[j].y, inUV2[j].z));
			sb.Append(inIDs[j].ToString() + "\n");
		}
		for (int material = 0; material < m.subMeshCount; material++)
		{
			sb.Append("\n");
			sb.Append("usemtl ").Append(mats[material].name).Append("\n");
			sb.Append("usemap ").Append(mats[material].name).Append("\n");

			int[] triangles = tris;
			for (int i = 0; i < triangles.Length; i += 3)
			{
				sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
					triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
			}
		}
		return sb.ToString();
	}

	#region  Semi Basic math functions, some unused
	/// <summary>
	/// FD: Self descriptive name (what a rarity) normalizes based on given values
	/// </summary>
	/// <param name="val">VD: Number to be normalized</param>
	/// <param name="min">VD: Bottom of the range to scale to</param>
	/// <param name="max">VD: Top of the range to scale to</param>
	/// <returns>value between 0 and 1 val scales to between min and max</returns>
	float NormalizeToRange(float val, float min, float max)
	{
		return (val - min) / (max - min);
	}

	/// <summary>
	/// FD: Self descriptive name (what a rarity) normalizes based on given values including mean and strength to assign to std dev
	/// </summary>
	/// <param name="val">VD: Number to be normalized</param>
	/// <param name="min">VD: Bottom of the range to scale to</param>
	/// <param name="max">VD: Top of the range to scale to</param>
	/// <param name="mean">VD: Pass in the average value within the range passed in</param>
	/// <param name="power">VD: Square root both time's it's used</param>
	/// <returns>value between 0 and 1 val scales to between min and max</returns>
	float normalizePower(float val, float min, float max, float mean, float power)
	{
		mean = (mean - min) / (max - min);
		val = (val - min) / (max - min);
		val = val - mean;
		if (val != 0f)
		{
			val = (Mathf.Pow(Mathf.Abs(val), power) * (Mathf.Abs(val) / val)) + mean;
		}
		return val;
	}

	/* FD: Mean function unused
    float normalizeToMean(float val, float min, float max, float mean)
    {
        val = val - mean;
        min = min - mean;
        max = max - mean;
        val = val / Mathf.Max(Mathf.Abs(min), Mathf.Abs(max));
        return (val / 2.0f) + 0.5f;

    }*/

	/* FD: std2 function unused
    float normalizeTo2Std(float val, float mean, float std)
    {
        float min = mean - 2f * std;
        float max = mean + 2f * std;
        return (val - min) / (max - min);
    }*/

	/* FD: Unused
	float Sigmoid(float x)
	{
		return 2 / (1 + Mathf.Exp(-2 * x)) - 1;
	}*/
	#endregion

	#endregion
}