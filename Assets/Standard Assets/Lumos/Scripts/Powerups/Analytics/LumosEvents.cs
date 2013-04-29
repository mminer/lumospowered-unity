// Copyright (c) 2012 Rebel Hippo Inc. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A service that allows custom events to be sent.
/// </summary>
public class LumosEvents : MonoBehaviour
{
	/// <summary>
	/// The URL.
	/// </summary>
	static string url;
	
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
	/// The stored events.
	/// </summary>
	static List<Dictionary<string, object>> events =
		new List<Dictionary<string, object>>();

	/// <summary>
	/// Names of unique (non-repeated) events that have yet to be recorded.
	/// </summary>
	static List<string> unsentUniqueEvents = new List<string>();

	LumosEvents () {}

	/// <summary>
	/// Records an event.
	/// </summary>
	/// <param name="name">The name of the event.</param>
	/// <param name="value">An arbitrary value to send with the event.</param>
	/// <param name="repeatable">
	/// Whether this event should only be logged once.
	/// </param>
	public static void Record (string name, float? value, bool repeatable)
	{
		if (name == null || name == "") {
			Lumos.LogWarning("Name must be sent. Event not recorded.");
			return;
		}

		// Ensure unrepeatable event hasn't been logged before.
		if (!repeatable) {
			if (RecordedUnique(name)) {
				return;
			}

			unsentUniqueEvents.Add(name);
		}

		var parameters = new Dictionary<string, object>() {
			{ "name", name },
			{ "level", Application.loadedLevelName },
		};

		if (value.HasValue) {
			parameters.Add("value_str", value.Value.ToString());
		}

		events.Add(parameters);
	}

	/// <summary>
	/// Returns true if an event flagged as not repeating has been recorded.
	/// </summary>
	/// <param name="name">The name of the event.</param>
	/// <returns>Whether the unique event has been recorded.</returns>
	public static bool RecordedUnique (string name)
	{
		var recorded = false;

		// Check if player prefs has recorded the event.
		if (PlayerPrefs.HasKey(PlayerPrefsKey(name)) ||
				unsentUniqueEvents.Contains(name)) {
			recorded = true;
		}

		return recorded;
	}

	/// <summary>
	/// Returns the key used to store a unique event in player prefs.
	/// </summary>
	/// <param name="eventName">The name of the event.</param>
	/// <returns>The player prefs key.</returns>
	public static string PlayerPrefsKey (string eventName)
	{
		var key = "lumos_event_" + Application.loadedLevelName + "_" +
		          eventName;
		return key;
	}

	/// <summary>
	/// Sends the events.
	/// </summary>
	public static void Send ()
	{
		if (events.Count == 0) {
			return;
		}
		
		LumosRequest.Send(url, events, 
			delegate { // Success
				// Save unrepeatable events to player prefs with a timestamp.
			foreach (var eventName in unsentUniqueEvents) {
				PlayerPrefs.SetString(PlayerPrefsKey(eventName),
					System.DateTime.Now.ToString());
			}

			unsentUniqueEvents.Clear();
			events.Clear();
			},
			
			delegate { // Failure
				Lumos.LogWarning("Events not sent. " +
					             " Will try again at next timer interval.");
			}
		);
	}
	
	/// <summary>
	/// The level start time.
	/// </summary>
	float levelStartTime;

	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake ()
	{
		levelStartTime = Time.time;
		LumosEvents.Record("Level Started", 1, false);
	}
	
	/// <summary>
	/// Raises the level was loaded event.
	/// </summary>
	void OnLevelWasLoaded ()
	{
		LumosEvents.Record("Level Completion Time", Time.time - levelStartTime, false);
		levelStartTime = Time.time;
		LumosEvents.Record("Level Started", 1, false);
	}
	
	/// <summary>
	/// Extra setup that needs to occur after Awake.
	/// </summary>
	void Start ()
	{
		url = "http://localhost:8888/api/1/games/" + Lumos.gameId + "/events";
		Lumos.RunRoutine(SendQueuedEvents());
	}
	
	/// <summary>
	/// Sends queued data on an interval.
	/// </summary>
	IEnumerator SendQueuedEvents ()
	{
		yield return new WaitForSeconds((float)timerInterval);

		if (!timerPaused) {
			SendQueued();
		}
		
		Lumos.RunRoutine(SendQueuedEvents());
	}
	
	/// <summary>
	/// Sends queued data.
	/// Currently the only data that accumulates is debug logs.
	/// </summary>
	public void SendQueued ()
	{
		LumosEvents.Send();
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
