// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace LumosUnity
{
	/// <summary>
	/// Miscellaneous utility functions.
	/// </summary>
	public static class Util
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
			var builder = new StringBuilder(strings.Length);

			foreach (var s in strings) {
				builder.Append(s);
			}

			var bytes = Encoding.UTF8.GetBytes(builder.ToString());

			// Encrypt bytes.
			var md5 = new MD5CryptoServiceProvider();
			var data = md5.ComputeHash(bytes);

			// Convert encrypted bytes back to a hex string.
			builder = new StringBuilder(bytes.Length);

			foreach (var b in data) {
				builder.Append(b.ToString("x2").ToLower());
			}

			return builder.ToString();
		}

		/// <summary>
		/// Adds a string to a dictionary if it's not null or empty.
		/// </summary>
		/// <param name="dict">The dictionary.</param>
		/// <param name="key">The dictionary key.</param>
		/// <param name="val">The dictionary value.</param>
		public static void AddToDictionaryIfNonempty (IDictionary dict, object key, string val)
		{
			if (!string.IsNullOrEmpty(val)) {
				dict[key] = val;
			}
		}
	}
}
