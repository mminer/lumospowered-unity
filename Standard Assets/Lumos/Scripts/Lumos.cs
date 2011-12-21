// Copyright (c) 2011 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// The main class for Lumos functionality.
/// </summary>
public partial class Lumos : MonoBehaviour
{
	/// <summary>
	/// Method signature for events hooked up to the timer.
	/// </summary>
	public delegate void TimedEventHandler ();

	/// <summary>
	/// The version.
	/// </summary>
	public static readonly Version version = new Version(1, 0);

	/// <summary>
	/// An instance of this class.
	/// </summary>
	public static Lumos instance { get; private set; }

	/// <summary>
	/// Displays detailed information about WWW requests / responses when set to true.
	/// </summary>
	public static bool debug { get; set; }

	/// <summary>
	/// Access key. Should never be shared.
	/// </summary>
	public static string apiKey { private get; set; }

	/// <summary>
	/// A unique string that identifies the application.
	/// </summary>
	public static string gameId { get; private set; }

	static uint _timerInterval = 30;
	/// <summary>
	/// The interval in seconds at which events (most likely queued data sends) are triggered.
	/// </summary>
	public static uint timerInterval
	{
		get { return _timerInterval; }
		set { _timerInterval = value; }
	}

	/// <summary>
	/// Events that are triggered on a timed interval.
	/// </summary>
	public static event TimedEventHandler timedEvents;

	/// <summary>
	/// Whether the data sending timer is paused.
	/// </summary>
	static bool timerPaused;

	Lumos () { }

	/// <summary>
	/// Sets up Lumos.
	/// </summary>
	void Awake ()
	{
		// Prevent multiple instances of Lumos from existing, necessary because of DontDestroyOnLoad.
		if (instance != null) {
			Destroy(gameObject);
			return;
		}

		instance = this;
		DontDestroyOnLoad(this);
		gameId = apiKey.Split('-')[0];

		if (gameId == null || gameId == "") {
			Debug.LogWarning("Lumos API key not set. No information will be sent.");
			Destroy(gameObject);
			return;
		}
	}

	/// <summary>
	/// Extra setup that needs to occur after Awake.
	/// </summary>
	void Start ()
	{
		// Start looping timer send.
		RunRoutine(SendQueuedRoutine());
	}

	void OnLevelWasLoaded ()
	{
		SendQueued();
	}

	/// <summary>
	/// Sends queued data on an interval.
	/// </summary>
	IEnumerator SendQueuedRoutine ()
	{
		yield return new WaitForSeconds((float)timerInterval);

		if (!timerPaused) {
			SendQueued();
		}

		RunRoutine(SendQueuedRoutine());
	}

	/// <summary>
	/// Sends queued data. Currently the only data that accumulates is debug logs.
	/// </summary>
	public void SendQueued ()
	{
		timedEvents();
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

	/// <summary>
	/// Executes a coroutine.
	/// </summary>
	/// <param name="routine">The coroutine to execute.</param>
	public static Coroutine RunRoutine (IEnumerator routine)
	{
		if (instance == null) {
			LogError("The Lumos game object must be instantiated before its methods can be used.");
			return null;
		}

		return instance.StartCoroutine(routine);
	}
}
