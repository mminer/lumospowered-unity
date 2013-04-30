// Copyright (c) 2012 Rebel Hippo Inc. All rights reserved.

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A service that sends debug logs for remote viewing.
/// </summary>
public class LumosLogs
{
	/// <summary>
	/// The URL.
	/// </summary>
	static string url = "http://localhost:8888/api/1/diagnostics";
	
	/// <summary>
	/// Log messages that Lumos should ignore.
	/// Supply the entire string or just the beginning of one.
	/// </summary>
	public static string[] toIgnore { get; set; }

	/// <summary>
	/// Log messages that Lumos might trigger that should be ignored.
	/// </summary>
	static string[] lumosIgnore = {
		"You are trying to load data from a www stream which had the " +
		"following error when downloading."
	};

	/// <summary>
	/// The stored logs.
	/// </summary>
	static List<Dictionary<string, object>> logs =
			new List<Dictionary<string, object>>();

	/// <summary>
	/// The log type labels.
	/// </summary>
	static readonly Dictionary<LogType, string> typeLabels =
			new Dictionary<LogType, string>() {
		{ LogType.Assert, "assertion" },
		{ LogType.Error, "error" },
		{ LogType.Exception, "exception" },
		{ LogType.Log, "info" },
		{ LogType.Warning, "warning" },
	};

	LumosLogs () {}

	/// <summary>
	/// Records a log message.
	/// </summary>
	/// <param name="message">The message.</param>
	/// <param name="trace">Detailed list of message's origin.</param>
	/// <param name="type">
	/// The type of message (debug, warning, error, etc.).
	/// </param>
	public static void Record (string message, string trace, LogType type)
	{
		// If the run in editor option is not selected, ignore all logs when in editor
		if (Application.isEditor && !LumosDiagnostics.instance.runInEditor) {
			return;
		}
		
		// Ignore messages logged by Lumos.
		if (message.StartsWith("[Lumos]")) {
			return;
		}

		// Only log message types that the user specifies.
		if ((type == LogType.Log       && !LumosDiagnostics.instance.recordLogs)     ||
			(type == LogType.Warning   && !LumosDiagnostics.instance.recordWarnings) ||
			(type == LogType.Error     && !LumosDiagnostics.instance.recordErrors)   ||
			(type == LogType.Exception && !LumosDiagnostics.instance.recordErrors)   ||
			type == LogType.Assert) { // Ignore asserts
			return;
		}

		// Skip messages that the user explicitly says to ignore.
		if (toIgnore != null) {
			foreach (var ignoreMessage in toIgnore) {
				if (message.StartsWith(ignoreMessage)) {
					return;
				}
			}
		}

		foreach (var ignoreMessage in lumosIgnore) {
			if (message.StartsWith(ignoreMessage)) {
				return;
			}
		}

		// If an identical message has been logged before, increment its total.
		for (int i = 0; i < logs.Count; i++) {
			var log = logs[i];

			if ((string)log["type"] == typeLabels[type] &&
				(string)log["message"] == message &&
				(string)log["level"] == Application.loadedLevelName &&
				(string)log["trace"] == trace) {
					log["total"] = (int)log["total"] + 1;
					return;
			}
		}

		// Otherwise create a new message dictionary
		var log_id = LumosUtil.MD5Hash(typeLabels[type], message, trace);
		var newLog = new Dictionary<string, object>() {
			{ "type", typeLabels[type] },
			{ "message", message },
			{ "trace", trace },
			{ "level", Application.loadedLevelName },
			{ "total", 1 },
			{ "log_id", log_id }
		};

		logs.Add(newLog);
	}

	/// <summary>
	/// Sends the queued logs to the server.
	/// </summary>
	public static void Send ()
	{
		if (logs.Count == 0) {
			return;
		}
		
		var endpoint = url + "/logs";

		LumosRequest.Send(endpoint, logs, 
			delegate { // Success
				logs.Clear();
			},
			
			delegate { // Failure
				Lumos.LogWarning("Log messages not sent. " +
					             " Will try again at next timer interval.");
			}
		);
	}
}
