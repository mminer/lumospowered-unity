// Copyright (c) 2012 Rebel Hippo Inc. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sends custom events and tracks player engagement.
/// </summary>
public class LumosAnalytics : MonoBehaviour
{
	#region Inspector Variables

	public bool useLevelsAsCategories = false;

	#endregion

	static string _baseUrl = "https://analytics.lumospowered.com/api/1";

	/// <summary>
	/// The API's host domain.
	/// </summary>
	public static string baseUrl {
		get { return _baseUrl; }
		set { _baseUrl = value; }
	}

	public static bool levelsAsCategories {
		get { return instance.useLevelsAsCategories; }
	}

	/// <summary>
	/// An instance of this class.
	/// </summary>
	static LumosAnalytics instance;

	/// <summary>
	/// Private constructor prevents object being created from class.
	/// Unity does this in the Awake function instead.
	/// </summary>
	LumosAnalytics () {}

	void Awake ()
	{
		instance = this;
		LumosEvents.levelStartTime = Time.time;
		LumosEvents.Record("level_started", 1, true);
		Lumos.OnTimerFinish += LumosEvents.Send;
	}

	void OnLevelWasLoaded ()
	{
		LumosEvents.Record("level_completion_time", Time.time - LumosEvents.levelStartTime, true);
		LumosEvents.levelStartTime = Time.time;
		LumosEvents.Record("level_started", 1, true);
	}
}
