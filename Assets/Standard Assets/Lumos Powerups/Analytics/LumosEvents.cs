// Copyright (c) 2012 Rebel Hippo Inc. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A service that allows custom events to be sent.
/// </summary>
public class LumosEvents
{
	/// <summary>
	/// Holds event information.
	/// </summary>
	struct Event
	{
		public string category;
		public string id;
		public float? val;

		/// <summary>
		/// A string to uniquely identify this event.
		/// </summary>
		public string key { get { return category + ":" + id; } }

		/// <summary>
		/// The key used to store a unique event in player prefs.
		/// </summary>
		public string playerPrefsKey { get { return "lumos_event_" + key; }}

		/// <summary>
		/// Returns a dictionary ready for JSON serialization.
		/// </summary>
		/// <returns>Dictionary representation of the event.</returns>
		public Dictionary<string, object> ToDictionary ()
		{
			var dict = new Dictionary<string, object>() {
				{ "category", category},
				{ "event_id", id }
			};

			if (val.HasValue) {
				dict["value"] = val.Value;
			}

			return dict;
		}
	}

	/// <summary>
	/// The URL.
	/// </summary>
	static string url = "http://localhost:8888/api/1/events";

	/// <summary>
	/// The stored events.
	/// </summary>
	static Dictionary<string, Event> events = new Dictionary<string, Event>();

	/// <summary>
	/// Unique (non-repeated) events that have yet to be recorded.
	/// </summary>
	static Dictionary<string, Event> unsentUniqueEvents = new Dictionary<string, Event>();

	/// <summary>
	/// The level start time.
	/// </summary>
	public static float levelStartTime { get; set; }

	/// <summary>
	/// Records an event.
	/// </summary>
	/// <param name="id">The event identifier.</param>
	/// <param name="value">An arbitrary value to send with the event.</param>
	/// <param name="repeatable">Whether this event should only be logged once.</param>
	public static void Record (string id, float? val, bool repeatable, string category="default")
	{
		if (id == null || id == "") {
			Lumos.LogWarning("An event ID must be supplied. Event not recorded.");
			return;
		}
		
		Lumos.Log("recording event: " + id);

		if (LumosAnalytics.levelsAsCategories) {
			category = Application.loadedLevelName;
		}

		var evt = new Event() {
			category = category,
			id = id,
			val = val
		};

		// Ensure unrepeatable event hasn't been logged before.
		if (!repeatable) {
			if (RecordedUnique(evt)) {
				return;
			}

			unsentUniqueEvents.Add(evt.key, evt);
		}

		events.Add(evt.key, evt);
	}

	/// <summary>
	/// Sends the events.
	/// </summary>
	public static void Send ()
	{
		Lumos.Log("events: " + events.Count);
		
		if (events.Count == 0) {
			return;
		}

		// Keep copy of events in case call fails.
		var eventsCopy = new Dictionary<string, Event>(events);

		var eventList = GenerateEventList(events);
		events.Clear();

		LumosRequest.Send(url, eventList,
			delegate { // Success
				// Save unrepeatable events to player prefs with a timestamp.
				foreach (var evt in unsentUniqueEvents.Values) {
					var now = System.DateTime.Now.ToString();
					PlayerPrefs.SetString(evt.playerPrefsKey, now);
				}

				unsentUniqueEvents.Clear();
			},

			delegate { // Failure
				// Re-add unsent events to events dictionary.
				foreach (var kvp in eventsCopy) {
					events.Add(kvp.Key, kvp.Value);
				}

				Lumos.LogWarning("Events not sent. Will try again at next timer interval.");
			}
		);
	}

	/// <summary>
	/// Returns true if an event flagged as not repeating has been recorded.
	/// </summary>
	/// <param name="evt">The event.</param>
	/// <returns>Whether the unique event has been recorded.</returns>
	static bool RecordedUnique (Event evt)
	{
		var recorded = PlayerPrefs.HasKey(evt.playerPrefsKey) ||
		               unsentUniqueEvents.ContainsKey(evt.key);
		return recorded;
	}

	/// <summary>
	/// Converts a dictionary of events to a list of event dictionaries.
	/// </summary>
	/// <param name="events">The events.</param>
	/// <returns>List of event dictionaries.</returns>
	static List<Dictionary<string, object>> GenerateEventList (Dictionary<string, Event> events) {
		var eventList = new List<Dictionary<string, object>>(events.Count);

		foreach (var evt in events.Values) {
			eventList.Add(evt.ToDictionary());
		}

		return eventList;
	}
}
