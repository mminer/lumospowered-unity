// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Miscellaneous utility functions.
/// </summary>
public static class LumosUtil
{
	/// <summary>
	/// Unixs the timestamp to date time.
	/// </summary>
	/// <returns>
	/// The timestamp to date time.
	/// </returns>
	/// <param name='timestamp'>
	/// Timestamp.
	/// </param>
	public static DateTime UnixTimestampToDateTime (double timestamp)
	{
		var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
		return dt.AddSeconds(timestamp);
	}

	/// <summary>
	/// Generates an MD5 hash of a string.
	/// </summary>
	/// <param name="strings">The strings to create a hash from.</param>
	/// <returns>The hash.</returns>
	public static string MD5Hash (params string[] strings)
	{
		var combined = "";

		foreach (var str in strings) {
			combined += str;
		}

		var bytes = Encoding.ASCII.GetBytes(combined);

		// Encrypt bytes.
		var md5 = new MD5CryptoServiceProvider();
		var data = md5.ComputeHash(bytes);

		// Convert encrypted bytes back to a hex string.
		var hash = new StringBuilder();

		foreach (var b in data) {
			hash.Append(b.ToString("x2").ToLower());
		}

		return hash.ToString();
	}
}