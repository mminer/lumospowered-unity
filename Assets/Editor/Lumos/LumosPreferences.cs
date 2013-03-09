using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using System.Net;
using System.IO;
using System.Text;
using System.Security.Cryptography;

/// <summary>
/// The preferences pane.
/// </summary>
public class LumosPreferences
{
	public const string apiUrl = "http://localhost:8083/api/";

	const string configFilePath = "Assets/Standard Assets/Lumos/.config.json";
	const string newConfigFilePath = "Assets/Standard Assets/Lumos/.config_new.json";

	static readonly GUIContent apiKeyLabel = new GUIContent("API Key");
	static bool prefsLoaded;
	static bool updateAvailable;

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
		get { return apiKey.Substring(0, 8); }
	}

	// EditorPrefs keys.
	const string keyPrefix = "lumos_";
	const string apiKeyKey = keyPrefix + "apikey";
	const string updateAvailableKey = keyPrefix + "updateavailable";

	[PreferenceItem("Lumos")]
	public static void OnGUI ()
	{
		// Load the preferences.
		if (!prefsLoaded) {
			Load();
			prefsLoaded = true;
		}

		apiKey = EditorGUILayout.TextField(apiKeyLabel, apiKey);

		if (updateAvailable) {
			GUILayout.Label("Update Available");

			if (GUILayout.Button("Update Now")) {
				Update();
			}
		} else {
			if (GUILayout.Button("Check for Updates")) {
				CheckForUpdates();
			}
		}

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
		updateAvailable = File.Exists(newConfigFilePath);
	}

	static void CheckForUpdates ()
	{
		var url = "http://localhost:8088/api/1/games/" + gameId + "/config";
		WebRequest request = WebRequest.Create(url);
		request.Method = "GET";
		request.BeginGetResponse(FinishUpdateCheck, request);
	}

	static void FinishUpdateCheck (IAsyncResult result)
	{
		var request = result.AsyncState as WebRequest;
		var response = request.EndGetResponse(result) as WebResponse;
		var update = false;

		// Determine if update is available by comparing hash of current config file with request's MD5 header.
		if (File.Exists(configFilePath)) {
			var configFileHash = GetFileHash(configFilePath);

			if (response.Headers["Content-MD5"] != configFileHash) {
				update = true;
			}
		} else {
			update = true;
		}

		// Save the request to a config file if it's new, but don't yet replace the old one.
		if (update) {
			// Save the response to a file.
			using (var file = File.OpenWrite(newConfigFilePath))
			using (var body = response.GetResponseStream()) {
				var buffer = new byte[8 * 1024];
				int bytesRead;

				while ((bytesRead = body.Read(buffer, 0, buffer.Length)) > 0) {
					file.Write(buffer, 0, bytesRead);
				}
			}
		}

		updateAvailable = update;
	}

	static void Update ()
	{
		// Replace the old config file with the new one.
		FileUtil.ReplaceFile(newConfigFilePath, configFilePath);
		Debug.Log("Updating...");
	}

	static string GetFileHash (string path)
	{
		var hash = new StringBuilder();
		var md5 = MD5.Create();

		using (var stream = File.OpenRead(path)) {
			foreach (byte b in md5.ComputeHash(stream)) {
				hash.Append(b.ToString("x2").ToLower());
			}
		}

		var result = hash.ToString();
		return result;
	}
}
