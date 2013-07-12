// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sends custom events and tracks player engagement.
/// </summary>
public partial class LumosAnalytics : MonoBehaviour
{
	#region Inspector Variables

	public bool useLevelsAsCategories = false;
	public bool recordLevelCompletionEvents = false;

	public static bool levelsAsCategories { get { return instance.useLevelsAsCategories; } }

	#endregion

	static string _baseUrl = "https://analytics.lumospowered.com/api/1";

	/// <summary>
	/// The API's host domain.
	/// </summary>
	public static string baseUrl {
		get { return _baseUrl; }
		set { _baseUrl = value; }
	}

	static float levelStartTime;
	static LumosAnalytics instance;
	LumosAnalytics () {}

	void Awake ()
	{
		instance = this;
		Lumos.OnReady += LumosLocation.Record;
		Lumos.OnTimerFinish += LumosEvents.Send;

		if (recordLevelCompletionEvents) {
			levelStartTime = Time.time;
			RecordEvent("level-started", true);
		}
	}

	void OnLevelWasLoaded ()
	{
		if (recordLevelCompletionEvents) {
			RecordEvent("level-started", true);
			RecordEvent("level-completion-time", Time.time - levelStartTime, true);
			levelStartTime = Time.time;
		}
	}
}
