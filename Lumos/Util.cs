using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Lumos
{
	/// <summary>
	/// Utility functions for use throughout Lumos.
	/// </summary>
	public static class Util
	{
		/// <summary>
		/// Returns the current timestamp in a string compatible with RFC 2616.
		/// </summary>
		/// <returns></returns>
		public static string currentTimestamp {
			get {
				var timestamp = DateTime.Now.ToUniversalTime().ToString("r");
				return timestamp;
			}
		}
		
		/// <summary>
		/// Creates a URL suitable for a GET request.
		/// </summary>
		/// <param name="baseUrl">The URL to attach parameters to.</param>
		/// <param name="parameters">The parameters to attach to the URL.</param>
		/// <returns>A GET URL.</returns>
		public static string ConstructGetUrl (string baseUrl, Dictionary<string, object> parameters)
		{
			var pairs = from param in parameters
						let pair = WWW.EscapeURL(param.Key) + "=" + WWW.EscapeURL(param.Value.ToString())
						select pair;
			var query = string.Join("&", pairs.ToArray());
			var url = baseUrl + "?" + query;
			return url;
		}

		/// <summary>
		/// Generates the MD5 hash for a file.
		/// </summary>
		/// <param name="filename">The full path to the file.</param>
		/// <returns>The file's MD5 hash.</returns>
		public static string GetMD5HashFromFile (string filename)
		{
			var file = new FileStream(filename, FileMode.Open);
			var md5 = new MD5CryptoServiceProvider();
			byte[] parts = md5.ComputeHash(file);
			file.Close();

			// Construct hex string from byte array.
			var hashBuilder = new StringBuilder();
			foreach (var part in parts) {
				hashBuilder.Append(part.ToString("X2"));
			}

			var hash = hashBuilder.ToString().ToLower();
			return hash;
		}
	}
}
