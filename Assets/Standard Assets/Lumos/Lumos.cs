// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

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

	/// <summary>
	/// When true, displays result of web requests and responses.
	/// </summary>
	public static bool debug { get; set; }

	/// <summary>
	/// A unique string identifying the game.
	/// </summary>
	public static string gameId { get; private set; }

	/// <summary>
	/// The device-specific player ID.
	/// </summary>
	public static string playerId { get; set; }

	#region Inspector Settings

	public string apiKey;
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

		if (apiKey == null || apiKey == "") {
			Lumos.Remove("The API Key must be set.");
			return;
		}

		gameId = apiKey.Substring(0, 8);
		LumosPlayer.Init();
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
}
