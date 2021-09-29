using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
//Oof
/// <summary>
/// CD: Reads the data off the database to bake new meshes
/// </summary>
public class DataLoad : MonoBehaviour
{
	#region PUBLIC_VAR
	/// <summary>
	/// Trinary switch for before, during and after loading
	/// </summary>
	[Tooltip("Trinary switch for before, during and after loading")]
	public int loadingFrame;

	/// <summary>
	/// Array to be populated with all values from database
	/// </summary>
	[Tooltip("Array to be populated with all values from database")]
	public string[][] val;

	/// <summary>
	/// Array to create index form 0-1000000 to every star in the database, for reference
	/// </summary>
	[Tooltip("Array to create index form 0-1000000 to every star in the database, for reference")]
	public string[] indVal;

	/// <summary>
	/// Dictionary to assign an index form 0-1000000 to every star in the database, for reference
	/// </summary>
	[Tooltip("Dictionary to assign an index form 0-1000000 to every star in the database, for reference")]
	public Dictionary<string, int> dict;

	/// <summary>
	/// Pointer Towards Welcome Text in scene to enable and disable on/off loading
	/// </summary>
	[Tooltip("Pointer Towards Welcome Text in scene to enable and disable on/off loading")]
	public GameObject welcometext;

    ///<remarks> Give nicer names to val and dict, ensure proper refactorization</remarks>
    #endregion

    /// <summary>
    /// Immediately calls f:DLoadStarter and set's the welcome text to active IV: Might mkae more sense to have f:DLoadStarter just be f:Start
    /// </summary>
    void Start()
    {
        welcometext.SetActive(true);
        loadingFrame = 0;
        //DLoadStarter();
    }

    ///<summary>
    ///Coroutine starter for loading data, uses loadSkip variable to check if loading needs to be done, and maintains the coroutine until loading is done/loadskip is true
    ///</summary>
    /*public void DLoadStarter()
    {
        if (!GameObject.Find("Config").GetComponent<GlobalLoadSkip>().loadSkip)
        {
            StartCoroutine(DLoad());
        }
    }*/

    /// <summary>
    /// Populates (IV: creates) v:val and V:dict (weak ass names) for use in c:StarSerializer by querying the server's php
    /// </summary>
    /// <returns></returns>
    private IEnumerator DLoad()
	{
		//bool bakeFlag = GameObject.Find("Config").GetComponent<GlobalLoadSkip>().bakeNewMeshes; //makes the bakeNewMeshes boolean local and easier to call

		if (loadingFrame == 0)
		{
			loadingFrame = 1;
			yield return new WaitForSeconds(0.001f); ///<remarks>GS: slows down the loading, not sure of the reason as of yet</remarks>

            /*
			if (bakeFlag)
			{
				Config config = Config.Instance;
				string dbIP = config.Get("databaseIP", "");

				if (dbIP == "")
				{
					//this should never happen, means config/cfg.txt is missing
					Debug.LogError("DATALOAD: Got bad database IP. Check config/cfg.txt; Falling back to localhost");
					dbIP = "localhost";
				}

				///<remarks> ten batches of ten-thousand values, querying server via php and generating a value list</remarks>
				List<string> valList = new List<string>();
				for (int i = 0; i < 1000000; i += 100000)
				{
					string loadURL = "http://" + dbIP + "/ioan_db_pullup_id_loader.php?start=" + i + "&cap=100000";
					WWW loadID = new WWW(loadURL);
					yield return loadID;

					valList.AddRange(loadID.text.Split(' ')); ///<remarks>Add the queried list from php into a master list</remarks>
					valList.RemoveAt(valList.Count() - 1); ///<remarks>Delete the last value</remarks> 
				}

				string[] valAR = valList.ToArray(); ///<remarks>list created, turbo mode engage - (arrays are faster than string lists)</remarks>
				val = valAR.Select(l => l.Split(',').Select(i => i).ToArray()).ToArray();
				///<remarks> Would really help to see the data it's working on, but it seems this one expression reforms the orignal array as a 2D Array based on commas within each entry in valAR</remarks> 
				//SaveAr (val, "dLoadValAr.bin");
				indVal = valAR.Select(l => l.Split(',')[0]).ToArray();
				///<remarks>Creates a dictionary from the astrological number to an iOAN index</remarks>
				dict = indVal.Select((value, index) => new { value, index }).ToDictionary(p => p.value, p => p.index);
				//SaveDict (dict, "dLoadDict.bin");

				//dict = LoadDict("dLoadDict.bin");
				//val = LoadAr ("dLoadValAr.bin");
				Debug.Log("DATALOAD: Finished gathering all mesh data from DB");
			}
            */
			//else
			//{
				Debug.Log("DATALOAD: load bypassed, using new baked system"); ///<remarks>44 Landscape-like objs that look like a starmap</remarks> 
			//}

			welcometext.SetActive(false);
			loadingFrame = 2;
		}
	}
}