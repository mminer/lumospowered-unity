// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Main class for Lumos functionality.
/// </summary>
public partial class Lumos : MonoBehaviour
{
	#region Events

	/// <summary>
	/// Lumos ready handler.
	/// </summary>
	public delegate void ReadyHandler ();

	/// <summary>
	/// Timer handler.
	/// </summary>
	public delegate void TimerHandler ();

	/// <summary>
	/// Triggers when Lumos has been initialized.
	/// </summary>
	public static event ReadyHandler OnReady;

	/// <summary>
	/// Occurs when on timer ready.
	/// </summary>
	public static event TimerHandler OnTimerFinish;

	#endregion

	/// <summary>
	/// Version number.
	/// </summary>
	public const string version = "1.0";

	/// <summary>
	/// Server communication credentials.
	/// </summary>
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
	/// The interval (in seconds) at which queued data is sent to the server.
	/// </summary>
	public static float timerInterval { get; set; }

	/// <summary>
	/// Whether the data sending timer is paused.
	/// </summary>
	public static bool timerPaused { get; set; }

	/// <summary>
	/// An instance of this class.
	/// </summary>
	public static Lumos instance { get; private set; }

	#region Inspector Settings

	public bool runInEditor;

	#endregion

	/// <summary>
	/// Private constructor prevents object being created directly.
	/// It should be instantiated instead.
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

		credentials = LumosCredentials.Load();

		if (credentials == null || credentials.apiKey == null || credentials.apiKey == "") {
			Debug.LogError("[Lumos] The Lumos API key is not set. Do this in the Lumos pane in Unity's preferences.");
			Destroy(gameObject);
			return;
		}

		instance = this;
		DontDestroyOnLoad(this);
		Debug.Log("Game ID: " + credentials.gameID);
	}

	/// <summary>
	/// Sends the opening request.
	/// <summary>
	void Start ()
	{
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
			Debug.LogWarning("[Lumos] " + reason +
			                 " No information will be recorded.");
			Destroy(instance.gameObject);
		}
	}

	/// <summary>
	/// Sends queued data on an interval.
	/// </summary>
	static IEnumerator SendQueuedData ()
	{
		yield return new WaitForSeconds(timerInterval);

		if (!timerPaused) {
			// Notify subscribers that the timer has completed.
			OnTimerFinish();
		}

		Lumos.RunRoutine(SendQueuedData());
	}
}
