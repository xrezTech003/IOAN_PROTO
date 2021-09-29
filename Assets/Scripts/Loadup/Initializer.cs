using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using Newtonsoft.Json;

/// <summary>
/// CD: This script is the launchpad for the stars to be loaded, handles the active-state of the loadbar, and holds the reference dictionary for starIDs to vertices for c:sensorCam and the DB to talk
/// </summary>
public class Initializer : MonoBehaviour
{
    #region PUBLIC_VAR
    /// <summary>
    /// VD: Instantiated Object for SubMeshLoader ::: Should be a Prefab with SubMeshLoader on it
    /// </summary>
    public GameObject subMeshPrefab;

	/// <summary>
	/// VD: Pointer for o:dataLoad ::: Should already be in the scene 
    /// Object Reference
	/// </summary>
	public GameObject dataLoadGameObject;

	/// <summary>
	/// VD: Pointer for object to hold all the subMeshes
    /// Object Reference
	/// </summary>
	public GameObject myMeshHolder;

	/// <summary>
	/// VD: Boolean to tell ShadeSwapper when to populate itself with the loaded SubMeshes
	/// </summary>
	public bool doneLoading;
	
	/// <summary>
	/// VD: Incremented by SubMeshLoader, keeps track of the stars to cap them at a <b>Static Value</b> IV: Fix The bold 
	/// </summary>
	public int myStarCount = 0;

	/// <summary>
	/// VD: IV: outdated
	/// </summary>
	public float loadBarProgress = 0f;

	/// <summary>
	/// Incemented from c:SubMeshLoader
	/// </summary><remarks>Value is a rectangle determined by the values in c:globalLoadSKip</remarks>
	public int totalMeshesLoaded; // Incremented from submeshloader

    /// <summary>
    /// VD: Publically setable height for code to adjust the height of the star cloth
    /// </summary>
    public float ClothHeight { get; private set; }

    /// <summary>
    /// This object keeps track of the subMeshes that are actually within the bounds of the Arena
    /// </summary>
    public GameObject InteractiveSubMesh { get; private set; }

    public MeshFilter[] Meshes
    {
        get
        {
            return myMeshHolder.transform.GetComponentsInChildren<MeshFilter>();
        }
    }

    public Bounds PlayableBounds;
    #endregion

    #region PRIVATE_VAR
    /// <summary>
    /// This <b>BIG</b> dictionary links the star mesh normals to their respective starIDs
    /// </summary>
    private Dictionary<uint, int> starIDToVertIndex = new Dictionary<uint, int>();
    #endregion

    #region UNITY_FUNC
    /// <summary>
    /// FD: Classic Start script ::: Configures loading bar based on .cfg ::: determines which loading sequence to run based on globalLoadSkip variables
    /// </summary>
    /// <remarks>IV: SEE 444 below: One of the possible removes if starSerializer is, in fact, hot garbage</remarks>
    void Start()
    {
        Config config = Config.Instance;

        ClothHeight = 0.8f;

        doneLoading = false;

        ///<remarks>IV:This If/Else reads GlobalLoadSkip (which still may not even need to exist?) to determine which style of mesh loading oughta take place (possibly discounting the need for DataLoad entirely?)</remarks> 
        ///<remarks>IV: 444: Possible remove if starSerializer is not needed</remarks>
        //if (!GameObject.Find("Config").GetComponent<GlobalLoadSkip>().loadSkip) StartCoroutine(AltStartLoadingMeshes());
        //else gameObject.GetComponent<shadeSwapper>().enabled = false;

        LoadMeshes();
    }
    #endregion

    #region PUBLIC_FUNC
    /// <summary>
	/// FD: reads the starLookupTable to convert star colour on mesh to database lookup value for populating data to taps and activated stars
	/// </summary>
	/// <param name="starID">Color -to- uint -to- new uint ::: sensorCam colour identifier -- r+g+b+a -- This </param>
	/// <returns> vertex Index </returns>
	public int getIndexForStarID(uint starID)
    {
        if (starIDToVertIndex.ContainsKey(starID))
            return starIDToVertIndex[starID];

        return -1;
    }

    /// <summary>
    /// FD: Starts the coroutine that loads in the meshes
    /// </summary>
    public void LoadMeshes()
    {
        StartCoroutine(StartLoadingByteMeshes());
        StartCoroutine(OutputPlayableData());
    }

    /// <summary>
    /// FD: Resets the world and then calls LoadMeshes
    /// </summary>
    public void ReloadMeshes()
    {
        doneLoading = false;
        Destroy(InteractiveSubMesh);
        InteractiveSubMesh = null;
        starIDToVertIndex = new Dictionary<uint, int>();
        totalMeshesLoaded = 0;

        foreach (Transform tran in myMeshHolder.transform)
        {
            Destroy(tran.gameObject);
        }

        LoadMeshes();
    }
    #endregion

    #region PRIVATE_FUNC
    /// <summary>
    /// FD: Original Writer claims this is super hacky because it checks the material against the c:ShadeSwapper v:myMaterial as a way to decide what mesh chunks are meant to be interactable - GS: iow loaded with the ability to be grabbed
    /// </summary>
    /// <remarks>IV: c:Shadeswapper v:myMaterial, as a name, is vague as all get out</remarks>
    private void SetInteractiveSubMesh()
	{
		///<remarksVD: Placeholders for mateirals to compare in interactivity bounds check</remarks>
		GameObject[] submeshes = GameObject.FindGameObjectsWithTag("submesh");
		Material interactiveMat = gameObject.GetComponent<shadeSwapper>().hqMat;
		string interactiveMatName = interactiveMat.name;

		foreach (GameObject submesh in submeshes)
		{
			Material mat = submesh.GetComponent<Renderer>().material;

            ///<remarks>IV: This is much harder to read but may be quicker/removes a whole function. I (N) honestly am not an expert on that stuff</remarks>
            ///<example><code>if ((mat.name).Replace("(Instance)", "").Trim().Equals(interactiveMatName.Replace("(Instance)", "").Trim()))</code></example> 
            if (mat.shader == interactiveMat.shader)
			{
				InteractiveSubMesh = submesh;

				return;
			}
		}

		Debug.LogWarning("INIT: No interactive mesh was found.");
	}

	/// <summary>
	/// FD: Called from DoneLoadingCheck when loading is done Builds a lookup table for star IDs to their vertex coordinates in the submesh map
	/// </summary>
	/// <remarks>Pairs with setInteractiveSubMesh</remarks>
	private void BuildStarIDLookupTable()
	{
		int selIndex = 0;
		Mesh interactiveStarMesh = InteractiveSubMesh.GetComponent<MeshFilter>().sharedMesh;

		foreach (Vector3 normal in interactiveStarMesh.normals)
		{
			uint starID = (uint)normal.x * 10000 + (uint)normal.y;
			starIDToVertIndex[starID] = selIndex;

			selIndex++;
		}
	}

    /// <summary>
    /// FD: checkes to see if the names of two objects are the same except for one ended with (Instance)
    /// </summary>
    /// <param name="name1">String to compare</param>
    /// <param name="name2">Other string to compare</param>
    /// <returns>Are these the same?</returns>
    /// ///<remarks>IV:If implementing as one line, remove this (see reference)</remarks> 
    private bool IsNameInstanceOf(string name1, string name2)
    {
        name1 = name1.Replace("(Instance)", "").Trim();
        name2 = name2.Replace("(Instance)", "").Trim();
        return name1.Equals(name2);
    }

    /// <summary>
    ///  Subloop to communicate with SubMeshLoader the number of stars loaded, every half second, attempts to finalize loading sequence
    /// </summary>
    /// <param name="inTotalCount">VD: Calculated in f:AltStartLoadingMeshes from values in c:gloabalLoadSkip, total number of mesh chunks to load</param>
    /// <returns>Completes loading by building the lookup table and setting the interactiveSubMesh</returns>
    private IEnumerator doneLoadingCheck(int inTotalCount)
	{
        yield return new WaitWhile(() => totalMeshesLoaded < inTotalCount);

		SetInteractiveSubMesh();
		BuildStarIDLookupTable();

		print("All stars loaded. Total count: " + myStarCount.ToString() + " objects");
		doneLoading = true;

        Config.Instance.TickLoadingProgress(false);

	}

    /// <summary>
    /// FD: Loads in each byte mesh and calls doneLoadingCheck when finished
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartLoadingByteMeshes()
    {
        if (dataLoadGameObject.activeInHierarchy)
            yield return new WaitUntil(() => dataLoadGameObject.GetComponent<DataLoad>().loadingFrame == 2);

        ConfigData configData = Config.Instance.Data;
        Config.Instance.ReadHeaderData("mesh_data/limits.b");

        int totalMeshCount = (int)(configData.meshRangeX.y - configData.meshRangeX.x + 1) * (int)(configData.meshRangeZ.y - configData.meshRangeZ.x + 1);

        for (int myCount = (int)configData.meshRangeX.x; myCount <= (int)configData.meshRangeX.y; myCount++)
        {
            for (int inCount = (int)configData.meshRangeZ.x; inCount <= (int)configData.meshRangeZ.y; inCount++)
            {
                string meshName = "mesh_data/grid" + myCount + "-" + inCount + ".b";

                if (File.Exists(meshName))
                {
                    GameObject myPart = Instantiate(subMeshPrefab, myMeshHolder.transform);
                    myPart.name = "SubMesh: " + myCount.ToString() + " - " + inCount.ToString();
                    myPart.tag = "submesh";

                    myPart.GetComponent<SubMeshLoader>().coord = new Vector2(myCount, inCount);
                    myPart.GetComponent<SubMeshLoader>().ReadMeshInStarter();
                }
            }
        }

        StartCoroutine(doneLoadingCheck(totalMeshCount));
    }

    private IEnumerator OutputPlayableData()
    {
        yield return new WaitUntil(() => doneLoading);

        string path = ".\\PlayableAreaData";

        //if (File.Exists(path)) yield break;
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        using (StreamWriter file = new StreamWriter(path + "\\data.json"))
        {
            Mesh mesh = InteractiveSubMesh.GetComponent<MeshFilter>().mesh;
            List<int> indices = mesh.vertices.Select((v, i) => (v, i)).Where((p) => PlayableBounds.Contains(p.v)).Select((p) => p.i).ToList();

            Dictionary<uint, StarData> stars = StarData.GenerateData(mesh, indices, GameObject.FindObjectOfType<IOANVoiceManager>());

            file.Write(JsonConvert.SerializeObject(stars, Formatting.Indented));
            Debug.Log("OUTPUT data for " + indices.Count() + " stars");
        }
    }

    private class StarData
    {
        public float x;
        public float y;
        public float z;

        public float ra;
        public float parallax;
        public float dec;

        public float meanMag;
        public float stdDev;
        public float numObs;
        public float var;

        public float catalog;
        public float pseudoColor;
        public float period;
        public float snr;

        public float rms;
        public float varClass;
        public float properMotion;

        public float pitch;
        public float absolutePitch;
        public string instrument;

        public static Dictionary<uint, StarData> GenerateData(Mesh m, List<int> ind, IOANVoiceManager vm)
        {
            Dictionary<uint, StarData> dict = new Dictionary<uint, StarData>();
            List<Vector4> uvs = new List<Vector4>();
            List<Vector4> uvBs = new List<Vector4>();
            m.GetUVs(0, uvs);
            m.GetUVs(1, uvBs);

            SFLimitsStruct limits = Config.Instance.GetLimits();
            SFScales scales = Config.Instance.GetScales();

            for (int i = 0; i < ind.Count(); i++)
            {
                Vector3 norm = m.normals[ind[i]];
                Vector3 pos = m.vertices[ind[i]];
                Vector4[] data = new Vector4[] { m.vertices[ind[i]], m.colors[ind[i]], uvs[ind[i]], uvBs[ind[i]] };

                int instrumentIndex = Mathf.FloorToInt(vm.dataMapper.tapColorToInstrument.mapInt(data[2].y));
                AK.Wwise.Event tapInstrument = vm.tapInstruments[instrumentIndex];

                float absolutePitch = vm.dataMapper.tapMagnitudeToPitch.mapNormalized(data[1].x);
                float pitch = vm.dataMapper.mappedScale.map(absolutePitch);

                data[0].w = 1f;
                data[3].w = 1f;

                data[0].x = data[0].x / scales.xScale + scales.xOffset;
                data[0].x = denormalizeToRange(data[0].x, limits.ra.x, limits.ra.y);

                data[0].y = (Mathf.Tan(data[0].y) / 1.5f) / scales.yScale + scales.yOffset;
                data[0].y = denormalizeToRange(data[0].y, limits.parallax.x, limits.parallax.y);

                data[0].z = data[0].z / scales.zScale + scales.zOffset;
                data[0].z = denormalizeToRange(data[0].z, limits.dec.x, limits.dec.y);

                data[1].x = denormalizeToRange(data[1].x, limits.mag.x, limits.mag.y);
                data[1].y = denormalizeToRange(data[1].y, limits.std.x, limits.std.y);
                data[1].z = denormalizeToRange(data[1].z, limits.obs.x, limits.obs.y);
                data[1].w = denormalizeToRange(data[1].w, limits.var_flag.x, limits.var_flag.y);
                data[2].x = denormalizeToRange(data[2].x, limits.catalog.x, limits.catalog.y);
                data[2].y = denormalizeToRange(data[2].y, limits.color.x, limits.color.y);
                data[2].z = denormalizeToRange(data[2].z, limits.period.x, limits.period.y);
                data[2].w = denormalizeToRange(data[2].w, limits.snr.x, limits.snr.y);
                data[3].x = denormalizeToRange(data[3].x, limits.rms.x, limits.rms.y);
                data[3].y = denormalizeToRange(data[3].y, limits.type.x, limits.type.y);
                data[3].z = denormalizeToRange(data[3].z, limits.pmra.x, limits.pmra.y);

                uint id = (uint)(norm.x * 10000.0f + norm.y);
                dict[id] = new StarData()
                {
                    x = pos.x,
                    y = pos.y,
                    z = pos.z,

                    ra = data[0].x,
                    parallax = data[0].y,
                    dec = data[0].z,

                    meanMag = data[1].x,
                    stdDev = data[1].y,
                    numObs = data[1].z,
                    var = data[1].w,

                    catalog = data[2].x,
                    pseudoColor = data[2].y,
                    period = data[2].z,
                    snr = data[2].w,

                    rms = data[3].x,
                    varClass = data[3].y,
                    properMotion = data[3].z,

                    pitch = pitch,
                    absolutePitch = absolutePitch,
                    instrument = tapInstrument.Name
                };
            }

            return dict;
        }

        private static float denormalizeToRange(float val, float max, float min)
        {
            return val * (max - min) + min;
        }
    }
    #endregion
}