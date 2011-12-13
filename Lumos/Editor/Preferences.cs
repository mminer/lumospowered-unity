using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Lumos
{
	/// <summary>
	/// The preferences pane.
	/// </summary>
	public class Preferences
	{
		public const string lumosUrl = "http://localhost:8083/";

		static readonly GUIContent apiKeyLabel = new GUIContent("API Key");
		static readonly GUIContent deployLabel = new GUIContent("Deploy");
		static bool loaded;

		// Preferences.
		public static string apiKey { get; private set; }

		// EditorPrefs keys.
		const string keyPrefix = "Lumos ";
		const string apiKeyKey = keyPrefix + "API Key";

		[PreferenceItem("Lumos")]
		public static void OnGUI ()
		{
			// Load the preferences.
			if (!loaded) {
				apiKey = EditorPrefs.GetString(apiKeyKey, "");
				loaded = true;
			}

			apiKey = EditorGUILayout.TextField(apiKeyLabel, apiKey);

			if (GUILayout.Button(deployLabel)) {
				Uploader.BuildAndUpload();
			}

			if (GUI.changed) {
				EditorPrefs.SetString(apiKeyKey, apiKey);
			}
		}
	}


}
