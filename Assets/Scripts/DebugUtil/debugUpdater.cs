using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
<summary>
	CD : debugUpdater
	Updates game log
</summary>
**/
public class debugUpdater : MonoBehaviour 
{
	#region PUBLIC_VAR
	/// <summary>
	///		VD : myTimer
	/// </summary>
	public float myTimer = 0.0f;
	#endregion

	#region PRIVATE_VAR
	/// <summary>
	///		VD : myLog
	///		Whole Log
	/// </summary>
	string myLog;

	/// <summary>
	///		VD : myLogQueue
	/// </summary>
	Queue myLogQueue = new Queue();
	#endregion

	#region UNITY_FUNC
	/**
	<summary>
		FD : Update()
		Resets every 3 seconds? I_A ::: IV: Coroutine? FixedUpdate? NDH
	</summary>
	**/
	void Update()
	{
		if ((myTimer -= Time.deltaTime) < 0.0f) /// <remarks>wouldn't this always be true?</remarks>
		{
			myTimer = 3f;
			Debug.Log("Newtest"); ///<reamarks>Doesn't really do anything</reamarks>
		}
	}

	/**
	<summary>
		FD : OnEnable
	</summary>
	**/
	void OnEnable()
	{
		Application.logMessageReceived += HandleLog;
	}

	/**
	<summary>
		FD : OnDisable()
	</summary>
	**/
	void OnDisable()
	{
		Application.logMessageReceived -= HandleLog;
	}

	/**
	<summary>
		FD : OnGUI()
	</summary>
	**/
	void OnGUI()
	{
		GUI.Label(new Rect(10, 140, 960, 980), myLog);
	}
	#endregion

	#region PRIVATE_FUNC
	/**
	<summary>
		FD : HandleLog(string, string, LogType)
		If log isn't an error or exception
			Add Log to queue
			Add everything in Queue to myLog
	</summary>
	**/
	void HandleLog(string logString, string stackTrace, LogType type)
	{
		if (type != LogType.Error && type != LogType.Exception)
		{
			myLog = logString;
			string newString = "\n [" + type + "] : " + myLog;
			myLogQueue.Enqueue(newString);

			if (type == LogType.Exception) /// <remarks>Should Never Run</remarks>
			{
				newString = "\n" + stackTrace;
				myLogQueue.Enqueue(newString);
			}

			myLog = string.Empty;
			foreach (string mylog in myLogQueue) myLog += mylog; /// <remarks>IV : GetComponent<Text> ().text += mylog;</remarks>
		}
	}
	#endregion
}
