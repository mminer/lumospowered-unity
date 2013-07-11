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

	public static bool levelsAsCategories {
		get { 
			if (!instance) {
				return false;
			}
			
			return instance.useLevelsAsCategories; 
		}
	}

	static LumosAnalytics instance;

	LumosAnalytics () {}

	void Awake ()
	{
		Lumos.Log("events awoke");
		instance = this;
		LumosEvents.levelStartTime = Time.time;
		LumosEvents.Record("level_started", 1, true);
		Lumos.OnTimerFinish += LumosEvents.Send;
		Lumos.OnReady += OnLumosReady;
	}

	/// <summary>
	/// Raises the lumos ready event.
	/// </summary>
	static void OnLumosReady ()
	{
		var key = "lumospowered_" + Lumos.credentials.gameID + "_" + Lumos.playerId + "_sent_location";

		if (!PlayerPrefs.HasKey(key)) {
			LumosLocation.Record();
		}
	}

	void OnLevelWasLoaded ()
	{
		LumosEvents.Record("level_completion_time", Time.time - LumosEvents.levelStartTime, true);
		LumosEvents.levelStartTime = Time.time;
		LumosEvents.Record("level_started", 1, true);
	}
	
	public static void RecordEvent (string eventID) 
	{
		LumosEvents.Record(eventID, null, true, GetCategory());
	}
	
	public static void RecordEvent (string eventID, string category) 
	{
		LumosEvents.Record(eventID, null, true, category);
	}
	
	public static void RecordEvent (string eventID, float? val) 
	{
		LumosEvents.Record(eventID, val, true, GetCategory());
	}
	
	public static void RecordEvent (string eventID, bool repeatable) 
	{
		LumosEvents.Record(eventID, null, repeatable, GetCategory());
	}
	
	public static void RecordEvent (string eventID, float? val, bool repeatable) 
	{
		LumosEvents.Record(eventID, val, repeatable, GetCategory());
	}
	
	public static void RecordEvent (string eventID, float? val, bool repeatable, string category) 
	{
		LumosEvents.Record(eventID, val, repeatable, category);
	}
	
	static string GetCategory ()
	{
		string category = null;
		
		if (instance.useLevelsAsCategories) {
			category = Application.loadedLevelName;
		}
		
		return category;
	}
}
