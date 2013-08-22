// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Main class for Lumos functionality.
/// </summary>
public partial class Lumos : MonoBehaviour
{
	#region Inspector Settings

	public bool runWhileInEditor;

	#endregion

	#region Events

	/// <summary>
	/// Triggers when Lumos has been initialized.
	/// </summary>
	public static event Action OnReady;

	/// <summary>
	/// Occurs when on timer ready.
	/// </summary>
	public static event Action OnTimerFinish;

	#endregion

	static float _timerInterval = 30; // Seconds

	/// <summary>
	/// Version number.
	/// </summary>
	public const string version = "1.3";

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
	public static float timerInterval {
		private get { return _timerInterval; }
		set { _timerInterval = value; }
	}

	/// <summary>
	/// Whether the data sending timer is paused.
	/// </summary>
	public static bool timerPaused { get; set; }

	/// <summary>
	/// Whether to send data to Lumos during development.
	/// </summary>
	public static bool runInEditor {
		get { return instance.runWhileInEditor; }
	}

	static Lumos instance;
	Lumos () {}

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
		
		// Shorten the timer interval while developers are testing
		if (runWhileInEditor && Application.isEditor) {
			timerInterval = 3;	
		}
		
		DontDestroyOnLoad(this);
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

		if (!timerPaused && OnTimerFinish != null) {
			// Notify subscribers that the timer has completed.
			OnTimerFinish();
		}

		Lumos.RunRoutine(SendQueuedData());
	}
}
