// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System.Text;
using UnityEngine;

/// <summary>
/// Replacement Debug functions that the library can use without being picked
/// up by Lumos' Diagnostics powerup.
/// </summary>
public partial class Lumos
{
	const string prefix = "[Lumos]";

	/// <summary>
	/// Records a debug message.
	/// </summary>
	/// <param name="messageParts">Messages(s) to log.</param>
	public static void Log (params object[] messageParts)
	{
		LogMessage(Debug.Log, messageParts);
	}

	/// <summary>
	/// Records a warning.
	/// </summary>
	/// <param name="messageParts">Messages(s) to log.</param>
	public static void LogWarning (params object[] messageParts)
	{
		LogMessage(Debug.LogWarning, messageParts);
	}

	/// <summary>
	/// Records an error.
	/// </summary>
	/// <param name="messageParts">Messages(s) to log.</param>
	public static void LogError (params object[] messageParts)
	{
		LogMessage(Debug.LogError, messageParts);
	}

	/// <summary>
	/// Records a message.
	/// </summary>
	/// <param name="logger">Function to send the message to.</param>
	/// <param name="messageParts">Messages(s) to log.</param>
	static void LogMessage (System.Action<object> logger, object[] messageParts)
	{
		if (debug && instance != null) {
			var builder = new StringBuilder(prefix, messageParts.Length * 2 + 1);

			foreach (var part in messageParts) {
				builder.Append(' ');
				builder.Append(part);
			}

			var message = builder.ToString();
			logger(message);
		}
	}
}
