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
	static string url = "http://localhost:8888/api/1/events";

	/// <summary>
	/// The level start time.
	/// </summary>
	float levelStartTime;

	/// <summary>
	/// The stored events.
	/// </summary>
	static List<Dictionary<string, object>> events = new List<Dictionary<string, object>>();

	/// <summary>
	/// Names of unique (non-repeated) events that have yet to be recorded.
	/// </summary>
	static List<string> unsentUniqueEvents = new List<string>();

	#region Inspector Variables

	public bool useLevelsAsCategories = false;

	#endregion

	/// <summary>
	/// Gets or sets the instance.
	/// </summary>
	/// <value>
	/// The instance.
	/// </value>
	public static LumosEvents instance { get; private set; }

	LumosEvents () {}

	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake ()
	{
		// Prevent multiple instances of Lumos from existing.
		// Necessary because DontDestroyOnLoad keeps the object between scenes.
		if (instance != null) {
			return;
		}

		instance = this;
		levelStartTime = Time.time;
		LumosEvents.Record("level_started", 1, true);
		Lumos.OnTimerReady += LumosEvents.Send;
	}

	/// <summary>
	/// Raises the level was loaded event.
	/// </summary>
	void OnLevelWasLoaded ()
	{
		LumosEvents.Record("level_completion_time", Time.time - levelStartTime, true);
		levelStartTime = Time.time;
		LumosEvents.Record("level_started", 1, true);
	}

	/// <summary>
	/// Records an event.
	/// </summary>
	/// <param name="eventID">The name of the event.</param>
	/// <param name="value">An arbitrary value to send with the event.</param>
	/// <param name="repeatable">
	/// Whether this event should only be logged once.
	/// </param>
	public static void Record (string eventID, float? value, bool repeatable, string category="default")
	{
		if (eventID == null || eventID == "") {
			Lumos.LogWarning("Name must be sent. Event not recorded.");
			return;
		}

		// Ensure unrepeatable event hasn't been logged before.
		if (!repeatable) {
			if (RecordedUnique(eventID)) {
				return;
			}

			unsentUniqueEvents.Add(eventID);
		}

		if (instance.useLevelsAsCategories) {
			category = Application.loadedLevelName;
		}

		var parameters = new Dictionary<string, object>() {
			{ "category", category },
			{ "event_id", eventID },
		};

		if (value.HasValue) {
			parameters.Add("value", value.Value);
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
}
