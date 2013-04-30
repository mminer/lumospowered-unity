using UnityEngine;
using System;
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
		
		Lumos.OnReady += OnLumosReady;
		Lumos.OnTimerReady += LumosLogs.Send;
	}
	
	/// <summary>
	/// Raises the lumos ready event.
	/// </summary>
	static void OnLumosReady ()
	{
		var key = "lumospowered_" + Lumos.gameId + "_" + Lumos.playerId + "_sent_specs";

		if (!PlayerPrefs.HasKey(key)) {
			LumosSpecs.Record();
		}
	}
}
