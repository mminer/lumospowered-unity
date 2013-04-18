// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Miscellaneous utility functions.
/// </summary>
public static class LumosUtil
{
	public static DateTime UnixTimestampToDateTime (double timestamp)
	{
		var dt = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
		return dt.AddSeconds(timestamp);
	}

#if !UNITY_FLASH

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
		var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
		var data = md5.ComputeHash(bytes);

		// Convert encrypted bytes back to a hex string.
		var hash = new StringBuilder();

		foreach (var b in data) {
			hash.Append(b.ToString("x2").ToLower());
		}

		return hash.ToString();
	}

#else

	/// <summary>
	/// Does nothing. Flash export doesn't yet include System.Security.
	/// </summary>
	public static string MD5Hash (params string[] strings)
	{
		return null;
	}

#endif
}
