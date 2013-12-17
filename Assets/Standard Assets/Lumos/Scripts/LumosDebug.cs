// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Text;

namespace LumosUnity
{
	/// <summary>
	/// Wrapper for UnityEngine.Debug
	/// Allows Lumos to record debug messages that won't be picked up by the
	/// Diagnostics powerup.
	/// </summary>
	public class Debug
	{
		const string prefix = "[Lumos]";

		/// <summary>
		/// Records a debug message.
		/// </summary>
		/// <param name="messageParts">Messages(s) to log.</param>
		public static void Log (params object[] messageParts)
		{
			LogMessage(UnityEngine.Debug.Log, messageParts);
		}

		/// <summary>
		/// Records a warning.
		/// </summary>
		/// <param name="messageParts">Messages(s) to log.</param>
		public static void LogWarning (params object[] messageParts)
		{
			LogMessage(UnityEngine.Debug.LogWarning, messageParts);
		}

		/// <summary>
		/// Records an error.
		/// </summary>
		/// <param name="messageParts">Messages(s) to log.</param>
		public static void LogError (params object[] messageParts)
		{
			LogMessage(UnityEngine.Debug.LogError, messageParts);
		}

		/// <summary>
		/// Records a message.
		/// </summary>
		/// <param name="logger">Function to send the message to.</param>
		/// <param name="messageParts">Messages(s) to log.</param>
		static void LogMessage (Action<object> logger, object[] messageParts)
		{
			if (Lumos.debug && Lumos.instance != null) {
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
}
