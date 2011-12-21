using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LumosPowered
{
	/// <summary>
	/// Helper methods for web requests.
	/// </summary>
	static class Web
	{
		/// <summary>
		/// Returns the current timestamp in a string compatible with RFC 2616 for sending over HTTP requests.
		/// </summary>
		/// <returns></returns>
		public static string currentTimestamp {
			get { return DateTime.Now.ToUniversalTime().ToString("r"); }
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
	}
}
