using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///		CD : newConsole
///		A console to display Unity's debug logs in-game.
/// </summary>
public class newConsole : MonoBehaviour
{
	struct Log
	{
		public string message;
		public string stackTrace;
		public LogType type;
	}

	#region PUBLIC_VAR
	/// <summary>
	///		VD : logField
	///		GameObject for position of Console
	/// </summary>
	public GameObject logField;

	/// <summary>
	///		VD : toggleKey
	/// </summary>
	public KeyCode toggleKey = KeyCode.BackQuote;
	#endregion

	#region PRIVATE_VAR
	/// <summary>
	///		VD : logs
	///		List of console Logs
	/// </summary>
	List<Log> logs = new List<Log>();

	/// <summary>
	///		VD : scrollPosition
	/// </summary>
	Vector2 scrollPosition;

	/// <summary>
	///		VD : show
	///		Toggle for if you show GUI
	/// </summary>
	bool show;

	/// <summary>
	///		VD : collapse
	/// </summary>
	bool collapse;

	// Visual elements:
	/// <summary>
	///		VD : margin
	/// </summary>
	const int margin = 20;

	/// <summary>
	///		VD : windowRect
	/// </summary>
	Rect windowRect = new Rect(margin, margin, Screen.width - (margin * 2), Screen.height - (margin * 2));
	#endregion

	#region UNITY_FUNC
	/**
	<summary>
		FD : Update()
		If v:toggleKey is pressed: toggle v:show and set logField active to if logField is activeInHierarchy
	</summary>
	**/
	void Update()
	{
		if (Input.GetKeyDown(toggleKey))
		{
			show = !show;
			logField.SetActive(!logField.activeInHierarchy);
		}
	}

	/**
	<summary>
		FD : OnEnable()
	</summary>
	**/
	void OnEnable()
	{
		Application.RegisterLogCallback(HandleLog);
	}

	/**
	<summary>
		FD : OnDisable()
	</summary>
	**/
	void OnDisable()
	{
		Application.RegisterLogCallback(null);
	}

	/**
	<summary>
		FD : OnGUI()
		Return if v:show is false
		Set v:windowRect to new GUILayout with v:windowRect, f:ConsoleWindow, and "Console"
	</summary>
	**/
	void OnGUI()
	{
		if (!show) return;

		windowRect = GUILayout.Window(123456, windowRect, ConsoleWindow, "Console");
	}
	#endregion

	#region PRIVATE_FUNC
	/**
	<summary>
		FD : ConsoleWindow(int)
		If v:logs has more than 4 logs
			Set v:logField text to last 5 messages
		Else if v:logs has more than 0 logs
			Set v:logField text to last message
		<param name="windowID"/>
	</summary>
	**/
	void ConsoleWindow(int windowID)
	{
		//scrollPosition = GUILayout.BeginScrollView(scrollPosition);
		if (logs.Count > 4)
		{
			logField.GetComponent<Text>().text = logs[logs.Count - 5].message + '\n';
			logField.GetComponent<Text>().text += logs[logs.Count - 4].message + '\n';
			logField.GetComponent<Text>().text += logs[logs.Count - 3].message + '\n';
			logField.GetComponent<Text>().text += logs[logs.Count - 2].message + '\n';
			logField.GetComponent<Text>().text += logs[logs.Count - 1].message + '\n';
		}
		else if (logs.Count > 0) logField.GetComponent<Text>().text = logs[logs.Count - 1].message + '\n';
	}
 
	/**
	<summary>
		FD : HandleLog(string, string, LogType)
		Records a log from the log callback.
		<param name="message">Message.</param>
		<param name="stackTrace">Trace of where the message came from.</param>
		<param name="type">Type of message (error, exception, warning, assert).</param>
	</summary>
	**/
	void HandleLog(string message, string stackTrace, LogType type)
	{
		logs.Add(new Log()
		{
			message = message,
			stackTrace = stackTrace,
			type = type,
		});
	}
	#endregion

	#region COMMENTED_CODE
	/*static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color>()
	{
		{ LogType.Assert, Color.white },
		{ LogType.Error, Color.red },
		{ LogType.Exception, Color.red },
		{ LogType.Log, Color.white },
		{ LogType.Warning, Color.yellow },
	};*/

	//Rect titleBarRect = new Rect(0, 0, 10000, 20);
	//GUIContent clearLabel = new GUIContent("Clear", "Clear the contents of the console.");
	//GUIContent collapseLabel = new GUIContent("Collapse", "Hide repeated messages.");

	/*
	 * In ConsoleWindow(int)
	 * 
	// Iterate through the recorded logs.
		//for (int i = 0; i < logs.Count; i++) {
		//	var log = logs[i];

		// Combine identical messages if collapse option is chosen.
		//	if (collapse) {
		//		var messageSameAsPrevious = i > 0 && log.message == logs[i - 1].message;

		//		if (messageSameAsPrevious) {
		//			continue;
		//		}
		//	}

		//GUI.contentColor = logTypeColors[log.type];
		//GUILayout.Label(log.message);
		//	if (logField.GetComponent<Text> ().text.Length + log.message.Length > 512) {
		//		if (logField.GetComponent<Text> ().text.Length != 0) {
		//			logField.GetComponent<Text> ().text = logField.GetComponent<Text> ().text.Substring (log.message.Length + 2);
		//			logField.GetComponent<Text> ().text += (log.message + '\n');
		//		} else {
		//			logField.GetComponent<Text> ().text += (log.message.Substring (0, 510) + '\n');
		//		}
		//	}
		//	else {
		//		logField.GetComponent<Text> ().text += (log.message + '\n');
		//	}

		//}

		//GUILayout.EndScrollView();

		//GUI.contentColor = Color.white;

		//GUILayout.BeginHorizontal();

		//if (GUILayout.Button(clearLabel)) {
		//	logs.Clear();
		//}

		//collapse = GUILayout.Toggle(collapse, collapseLabel, GUILayout.ExpandWidth(false));

		//GUILayout.EndHorizontal();

		// Allow the window to be dragged by its title bar.
		//GUI.DragWindow(titleBarRect);
		*/
#endregion
}
 