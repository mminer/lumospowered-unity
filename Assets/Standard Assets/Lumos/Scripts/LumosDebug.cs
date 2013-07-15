// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using UnityEngine;

/// <summary>
/// Replacement Debug functions that the library can use without being picked
/// up by Lumos' Diagnostics powerup.
/// </summary>
public partial class Lumos
{
	/// <summary>
	/// Records a debug message.
	/// </summary>
	/// <param name="message">The message to log.</param>
	public static void Log (object message)
	{
		LogMessage(Debug.Log, message);
	}

	/// <summary>
	/// Records a warning.
	/// </summary>
	/// <param name="message">The message to log.</param>
	public static void LogWarning (object message)
	{
		LogMessage(Debug.LogWarning, message);
	}

	/// <summary>
	/// Records an error.
	/// </summary>
	/// <param name="message">The message to log.</param>
	public static void LogError (object message)
	{
		LogMessage(Debug.LogError, message);
	}

	/// <summary>
	/// Records a message.
	/// </summary>
	/// <param name="logger">The function to send the message to.</param>
	/// <param name="message">The message to log.</param>
	static void LogMessage (System.Action<object> logger, object message)
	{
		if (instance == null || !debug) {
			return;
		}

		logger("[Lumos] " + message);
	}
}
