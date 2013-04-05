// Copyright (c) 2012 Rebel Hippo Inc. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The main class for Lumos functionality.
/// </summary>
public partial class Lumos : MonoBehaviour
{
	/// <summary>
	/// The version.
	/// </summary>
	public const string version = "1.0";

	/// <summary>
	/// An instance of this class.
	/// </summary>
	public static Lumos instance { get; private set; }

	/// <summary>
	/// Displays detailed information about WWW requests / responses when true.
	/// </summary>
	public static bool debug { get; set; }

	/// <summary>
	/// A unique string that identifies the application.
	/// </summary>
	public static string gameId { get; private set; }

	#region Inspector Settings

	public string secretKey;
	public bool runInEditor;
	public bool recordPresetEvents;
	public bool recordErrors;
	public bool recordWarnings;
	public bool recordLogs;

	#endregion

	/// <summary>
	/// Initializes a new instance of this class.
	/// </summary>
	Lumos () {}

	/// <summary>
	/// Sets up Lumos.
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
		gameId = secretKey.Split('-')[0];

		if (gameId == null || gameId == "") {
			Lumos.Remove("Secret key not set.");
			return;
		}

		// Set up debug log redirect.
		Application.RegisterLogCallback(LumosLogs.Record);
		
		LumosCore.Init();
	}

	/// <summary>
	/// Executes a coroutine.
	/// </summary>
	/// <param name="routine">The coroutine to execute.</param>
	public static Coroutine RunRoutine (IEnumerator routine)
	{
		if (instance == null) {
			Lumos.LogError("Lumos game object must be instantiated " +
			               "before its methods can be called.");
			return null;
		}

		return instance.StartCoroutine(routine);
	}

	/// <summary>
	/// Destroys the instance so that it cannot be used.
	/// This will be called at the start of the game if it's determined that
	/// information cannot be sent to the server properly.
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
}
