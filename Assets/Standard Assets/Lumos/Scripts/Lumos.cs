// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Main class for Lumos functionality.
/// </summary>
public partial class Lumos : MonoBehaviour
{
	/// <summary>
	/// Version number.
	/// </summary>
	public const string version = "1.0";

	/// <summary>
	/// An instance of this class.
	/// </summary>
	public static Lumos instance { get; private set; }

	public static LumosCredentials credentials { get; private set; }

	/// <summary>
	/// When true, displays result of web requests and responses.
	/// </summary>
	public static bool debug { get; set; }

	/// <summary>
	/// The device-specific player ID.
	/// </summary>
	public static string playerId { get; set; }

	/// <summary>
	/// Lumos ready handler.
	/// </summary>
	public delegate void LumosReadyHandler ();
	/// <summary>
	/// Occurs when on ready.
	/// </summary>
	public static event LumosReadyHandler OnReady;

	/// <summary>
	/// Timer handler.
	/// </summary>
	public delegate void TimerHandler ();
	/// <summary>
	/// Occurs when on timer ready.
	/// </summary>
	public static event TimerHandler OnTimerReady;

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

	#region Inspector Settings

	public bool runInEditor;

	#endregion

	/// <summary>
	/// Private constructor prevents object being created from class.
	/// Unity does this in the Awake function instead.
	/// </summary>
	Lumos () {}

	/// <summary>
	/// Initializes Lumos.
	/// </summary>
	void Awake ()
	{
		// Prevent multiple instances of Lumos from existing.
		// Necessary because DontDestroyOnLoad keeps the object between scenes.
		if (instance != null) {
			Lumos.Log("Destroying duplicate game object instance.");
			Destroy(gameObject);
			return;
		}

		instance = this;
		DontDestroyOnLoad(this);
		credentials = LumosCredentials.Load();

		Debug.Log("Game ID: " + credentials.gameID);
	}

	void Start() {
		LumosPlayer.Init(delegate {
			if (OnReady != null) {
				OnReady();
				Lumos.RunRoutine(SendQueuedData());
			}
		});
	}

	/// <summary>
	/// Executes a coroutine.
	/// </summary>
	/// <param name="routine">The coroutine to execute.</param>
	public static Coroutine RunRoutine (IEnumerator routine)
	{
		if (instance == null) {
			Lumos.LogError("The Lumos game object must be instantiated before its methods can be called.");
			return null;
		}

		return instance.StartCoroutine(routine);
	}

	/// <summary>
	/// Destroys the instance so that it cannot be used.
	/// Called on game start if a server connection cannot be established.
	/// </summary>
	/// <param name="reason">The reason why the instance is unusable.</param>
	public static void Remove (string reason)
	{
		if (instance != null) {
			Debug.LogWarning("[Lumos] " + reason + " No information will be recorded.");
			Destroy(instance.gameObject);
		}
	}

	/// <summary>
	/// Sends queued data on an interval.
	/// </summary>
	static IEnumerator SendQueuedData ()
	{
		yield return new WaitForSeconds((float)timerInterval);

		if (!timerPaused) {
			// Raise the timer ready event
			OnTimerReady();
		}

		Lumos.RunRoutine(SendQueuedData());
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
