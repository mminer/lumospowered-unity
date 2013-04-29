using UnityEngine;
using System.Collections;

/// <summary>
/// Lumos diagnostics.
/// </summary>
public class LumosDiagnostics : MonoBehaviour 
{
	
	/// <summary>
	/// An instance of this class.
	/// </summary>
	public static LumosDiagnostics instance { get; private set; }

	#region Inspector Settings

	public bool recordLogs = false;
	public bool recordWarnings = false;
	public bool recordErrors = false;
	public bool runInEditor = false;

	#endregion
	
	static uint _timerInterval = 5;
	/// <summary>
	/// The interval (in seconds) at which queued data is sent to the server.
	/// </summary>
	static uint timerInterval {
		get { return _timerInterval; }
		set { _timerInterval = value; }
	}

	/// <summary>
	/// Whether the data sending timer is paused.
	/// </summary>
	static bool timerPaused;

	/// <summary>
	/// Private constructor prevents object being created from class.
	/// Unity does this in the Awake function instead.
	/// </summary>
	LumosDiagnostics () {}
	
	/// <summary>
	/// Initializes Lumos Diagnostics.
	/// </summary>
	void Awake ()
	{
		// Prevent multiple instances of Lumos from existing.
		if (instance != null) {
			return;
		}

		instance = this;
		DontDestroyOnLoad(this);
		
		// Set up debug log redirect.
		Application.RegisterLogCallback(LumosLogs.Record);
	}
	
	/// <summary>
	/// Extra setup that needs to occur after Awake.
	/// </summary>
	void Start ()
	{
		var key = "lumospowered_" + Lumos.gameId + "_" + Lumos.playerId + "_sent_specs";

		if (!PlayerPrefs.HasKey(key)) {
			LumosSpecs.Record();
		}
		
		Lumos.RunRoutine(SendQueuedLogs());
	}
	
	/// <summary>
	/// Sends queued data on an interval.
	/// </summary>
	IEnumerator SendQueuedLogs ()
	{
		yield return new WaitForSeconds((float)timerInterval);

		if (!timerPaused) {
			SendQueued();
		}
		
		Lumos.RunRoutine(SendQueuedLogs());
	}
	
	/// <summary>
	/// Sends queued data.
	/// Currently the only data that accumulates is debug logs.
	/// </summary>
	public void SendQueued ()
	{
		LumosLogs.Send();
	}

	/// <summary>
	/// Pauses the queued data send timer.
	/// </summary>
	public static void PauseTimer ()
	{
		timerPaused = true;
	}

	/// <summary>
	/// Resumes the queued data send timer.
	/// </summary>
	public static void ResumeTimer ()
	{
		timerPaused = false;
	}
}
