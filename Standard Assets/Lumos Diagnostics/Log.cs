using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

/// <summary>
/// A basic class for holding log information.
/// </summary>
class LumosLog
{
	readonly string type; // info, error, etc.
	readonly string message;
	readonly string trace;
	readonly string level;
	int total = 1;
	string hash;

	/// <summary>
	/// The log type labels.
	/// </summary>
	static readonly Dictionary<LogType, string> typeLabels = new Dictionary<LogType, string>() {
		{ LogType.Assert,    "assertion" },
		{ LogType.Error,     "error" },
		{ LogType.Exception, "exception" },
		{ LogType.Log,       "info" },
		{ LogType.Warning,   "warning" },
	};

	public LumosLog (string key, LogType type, string message, string trace)
	{
		GenerateHash(key);
		this.type = typeLabels[type];
		this.message = message;
		this.trace = trace;
		this.level = Application.loadedLevelName;
	}

	/// <summary>
	/// Converts the object to a hashtable suitable for adding to a JSON object.
	/// </summary>
	/// <returns>A hash table representation of this log.</returns>
	public Hashtable ToHashtable ()
	{
		var table = new Hashtable() {
			{ "type", type },
			{ "message", message },
			{ "trace", trace },
			{ "level", level },
			{ "total", total },
			{ "hash", hash }
		};

		return table;
	}

	/// <summary>
	/// Returns true if this log's properties are the same as another's.
	/// </summary>
	public bool IsEqual (LogType type, string message, string trace)
	{
		bool equal = this.type == typeLabels[type] && this.message == message && this.trace == trace;
		return equal;
	}

	/// <summary>
	/// Adds one to the total times the message has been logged.
	/// </summary>
	public void IncrementTotal ()
	{
		total++;
	}

	/// <summary>
	/// Creates an MD5 hash of the log from its properties.
	/// </summary>
	void GenerateHash (string key)
	{
		var bytes = Encoding.ASCII.GetBytes(key);
		var md5 = new MD5CryptoServiceProvider();
		var data = md5.ComputeHash(bytes);

		// Convert encrypted bytes back to a string (base 16).
		var hash = new StringBuilder();
		foreach (var b in data) {
			hash.Append(b.ToString("x2").ToLower());
		}

		this.hash = hash.ToString();
	}
}
