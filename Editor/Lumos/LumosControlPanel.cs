using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace LumosPowered
{
	/// <summary>
	/// The preferences pane.
	/// </summary>
	public class Preferences
	{
		public const string apiUrl = "http://localhost:8083/api/";

		static readonly GUIContent apiKeyLabel = new GUIContent("API Key");
		static bool loaded;

		// Preferences.
		static string _apiKey;
		/// <summary>
		/// Secret key. Should not be shared.
		/// </summary>
		public static string apiKey
		{
			get
			{
				if (_apiKey == null) { Load(); }
				return _apiKey;
			}
			private set { _apiKey = value; }
		}

		/// <summary>
		/// The game's identifier.
		/// </summary>
		public static string gameId
		{
			get { return apiKey.Split('-')[0]; }
		}

		// EditorPrefs keys.
		const string keyPrefix = "Lumos ";
		const string apiKeyKey = keyPrefix + "API Key";

		[PreferenceItem("Lumos")]
		public static void OnGUI ()
		{
			// Load the preferences.
			if (!loaded) {
				Load();
				loaded = true;
			}

			apiKey = EditorGUILayout.TextField(apiKeyLabel, apiKey);

			if (GUI.changed) {
				EditorPrefs.SetString(apiKeyKey, apiKey);
			}
		}

		/// <summary>
		/// Load the preferences from EditorPrefs.
		/// </summary>
		static void Load ()
		{
			apiKey = EditorPrefs.GetString(apiKeyKey, "");
		}
	}
}
