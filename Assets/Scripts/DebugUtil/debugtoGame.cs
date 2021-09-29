using System.Collections.Generic;
using UnityEngine;

/// <summary>
///		CD : DebugtoGame
///		A console to display Unity's debug logs in-game.
/// </summary>
public class DebugtoGame : MonoBehaviour
{
	#region PUBLIC_VAR
	/// <summary>
	///		VD : toggleKey
	///		The hotkey to show and hide the console window.
	/// </summary>
	public KeyCode toggleKey = KeyCode.BackQuote;
	#endregion

	#region PRIVATE_VAR
	/**
	<summary>
		ST : Log
		Struct for message, stackTrace and type of log
	</summary>
	**/
	struct Log
	{
		public string message;
		public string stackTrace;
		public LogType type;
	}

	/// <summary>
	///		VD : logs
	///		List of logs
	/// </summary>
	List<Log> logs = new List<Log>();

	/// <summary>
	///		VD : scrollPosition
	///		Position of Scroll
	/// </summary>
	Vector2 scrollPosition;

	/// <summary>
	///		VD : show
	///		Boolean Switch
	/// </summary>
	bool show;

	/// <summary>
	///		VD : collapse
	/// </summary>
	bool collapse;

	//Visual Elements
	/// <summary>
	///		VD : logTypeColors
	///		Dictionary for what color wach logtype should be
	/// </summary>
	static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color>()
	{
		{ LogType.Assert, Color.white },
		{ LogType.Error, Color.red },
		{ LogType.Exception, Color.red },
		{ LogType.Log, Color.white },
		{ LogType.Warning, Color.yellow },
	};

	/// <summary>
	///		VD : margin
	/// </summary>
	const int margin = 20;

	/// <summary>
	///		VD : windowRect
	/// </summary>
	Rect windowRect = new Rect(margin, margin, Screen.width - (margin * 2), Screen.height - (margin * 2));

	/// <summary>
	///		VD : titleBarRect
	/// </summary>
	Rect titleBarRect = new Rect(0, 0, 10000, 20);

	/// <summary>
	///		VD : clearLabel
	/// </summary>
	GUIContent clearLabel = new GUIContent("Clear", "Clear the contents of the console.");

	/// <summary>
	///		VD : collapseLabel
	/// </summary>
	GUIContent collapseLabel = new GUIContent("Collapse", "Hide repeated messages.");
	#endregion

	#region UNITY_FUNC
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
		FD : Update()
		Switch toggle v:show on toggle key
	</summary>
	**/
	void Update()
	{
		if (Input.GetKeyDown(toggleKey)) show = !show;
	}

	/**
	<summary>
		FD : OnGUI()
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
		A window that displays the recorded logs.
		<param name="windowID">Window ID.</param>
	</summary>
	**/
	void ConsoleWindow(int windowID)
	{
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);

		for (int i = 0; i < logs.Count; i++) // Iterate through the recorded logs.
		{
			var log = logs[i];

			if (collapse) // Combine identical messages if collapse option is chosen.
			{
				var messageSameAsPrevious = i > 0 && log.message == logs[i - 1].message;

				if (messageSameAsPrevious) continue;
			}

			GUI.contentColor = logTypeColors[log.type];
			GUILayout.Label(log.message);
		}

		GUILayout.EndScrollView();

		GUI.contentColor = Color.white;

		GUILayout.BeginHorizontal();

		if (GUILayout.Button(clearLabel)) logs.Clear();

		collapse = GUILayout.Toggle(collapse, collapseLabel, GUILayout.ExpandWidth(false));

		GUILayout.EndHorizontal();

		GUI.DragWindow(titleBarRect); // Allow the window to be dragged by its title bar.
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
}