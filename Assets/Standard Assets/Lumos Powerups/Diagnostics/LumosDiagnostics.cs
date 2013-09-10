// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using UnityEngine;

/// <summary>
/// Lumos diagnostics.
/// </summary>
public class LumosDiagnostics : MonoBehaviour
{
	#region Inspector Settings

	public bool recordLogs = false;
	public bool recordWarnings = true;
	public bool recordErrors = true;

	public static bool recordDebugLogs { get { return instance.recordLogs; } }
	public static bool recordDebugWarnings { get { return instance.recordWarnings; } }
	public static bool recordDebugErrors { get { return instance.recordErrors; } }

	#endregion

	static string _baseUrl = "https://diagnostics.lumospowered.com/api/1";

	/// <summary>
	/// The API's host domain.
	/// </summary>
	public static string baseUrl {
		get { return _baseUrl; }
		set { _baseUrl = value; }
	}

	static LumosDiagnostics instance;
	LumosDiagnostics () {}
	
	
	void Awake ()
	{
		instance = this;
		Lumos.OnReady += LumosSpecs.Record;
		Lumos.OnTimerFinish += LumosLogs.Send;

		// Set up debug log redirect.
		Application.RegisterLogCallback(LumosLogs.Record);
	}
	
	
	void OnGUI ()
	{
		LumosFeedbackGUI.OnGUI();
	}

	public static bool IsInitialized ()
	{
		GameObject lumosGO = GameObject.Find("Lumos");
		
		if (lumosGO == null) {
			Debug.LogWarning("The Lumos Game Object has not been added to your initial scene.");
			return false;
		}
		
		if (lumosGO.GetComponent<LumosDiagnostics>() == null) {
			Debug.LogWarning("The LumosDiagnostics script has not been added to the Lumos GameObject.");
			return false;
		}
		
		return true;
	}
}
