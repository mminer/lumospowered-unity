// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Lumos diagnostics.
/// </summary>
public class LumosDiagnostics : MonoBehaviour
{
	#region Inspector Settings

	public bool recordLogs = false;
	public bool recordWarnings = false;
	public bool recordErrors = false;
	public bool runInEditor = false;

	#endregion

	static string _baseUrl = "https://diagnostics.lumospowered.com/api/1";

	/// <summary>
	/// The API's host domain.
	/// </summary>
	public static string baseUrl {
		get { return _baseUrl; }
		set { _baseUrl = value; }
	}

	/// <summary>
	/// An instance of this class.
	/// </summary>
	public static LumosDiagnostics instance { get; private set; }

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
		Lumos.OnTimerFinish += LumosLogs.Send;
	}

	/// <summary>
	/// Raises the lumos ready event.
	/// </summary>
	static void OnLumosReady ()
	{
		var key = "lumospowered_" + Lumos.credentials.gameID + "_" + Lumos.playerId + "_sent_specs";

		if (!PlayerPrefs.HasKey(key)) {
			LumosSpecs.Record();
		}
	}
}
