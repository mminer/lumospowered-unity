// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;

/// <summary>
/// Options that control how Lumos behaves.
/// </summary>
public class LumosPreferences
{
	/// <summary>
	/// The available powerup package updates.
	/// </summary>
	static List<Dictionary<string, object>> _updates;
	static List<Dictionary<string, object>> updates {
		get { return _updates; }
		set {
			_updates = value;
			var data = LumosJson.Serialize(_updates);
			EditorPrefs.SetString("lumos-updates", data);
		}
	}

	static bool _selectiveImporting = false;
	static bool selectiveImporting {
		get { return _selectiveImporting; }
		set {
			_selectiveImporting = value;
			EditorPrefs.SetBool("lumos-updates-selective", value);
		}
	}

	static List<string> importing = new List<string>();
	static bool checkingUpdates;
	static List<Dictionary<string, object>> installedPowerups = new List<Dictionary<string, object>>();
	static List<Dictionary<string, object>> tempUpdates = new List<Dictionary<string, object>>();

	/// <summary>
	/// The file path where we store info on which powerups have been installed.
	/// </summary>
	static string filePath;

	/// <summary>
	/// Runs the first time this UI is displayed.
	/// </summary>
	public static void Start ()
	{
		EditorApplication.update += Update;
		filePath = Application.dataPath + "/Editor/Lumos/powerups.json";
		selectiveImporting = EditorPrefs.GetBool("lumos-updates-selective", false);
		var data = EditorPrefs.GetString("lumos-updates", "{[]}");
		var fileList = LumosJson.Deserialize(data) as IList;
		var files = new List<Dictionary<string, object>>();

		if (fileList != null) {
			foreach (Dictionary<string, object> file in fileList) {
				files.Add(file);
			}
		}

		updates = files;
	}

	/// <summary>
	/// Runs every frame with EditorApplication.update
	/// </summary>
	public static void Update ()
	{
		if (tempUpdates.Count > 0) {
			// Done here to get around main thread issues with Async calls
			updates = tempUpdates;
			tempUpdates = new List<Dictionary<string, object>>();
			installedPowerups = GetInstalledPowerups();

			for (var i = 0; i < updates.Count; i++) {
				if (HasLatestPowerup(updates[i])) {
					updates.RemoveAt(i);
					i--;
				}
			}
		}

		// Done here to get around main thread issues with Async calls
		if (importing.Count > 0) {
			ImportPackage(importing[0]);
			importing.RemoveAt(0);
		}
	}

    [PreferenceItem("Lumos")]
    public static void PreferencesGUI ()
	{
		if (updates == null) {
			Start();
		}

		var isSelectiveImport = selectiveImporting;
		isSelectiveImport = GUILayout.Toggle(isSelectiveImport, "Selective Importing");

		// This ensures the editor prefs are only updated when the value is toggled
		if (isSelectiveImport != selectiveImporting) {
			selectiveImporting = isSelectiveImport;
		}


		if (checkingUpdates) {
			GUILayout.Label("Checking for updates...");
		} else {
			if (GUILayout.Button("Check for Updates")) {
				CheckForUpdates();
			}
		}

		if (updates != null && updates.Count > 0) {
			DisplayUpdates();
		}
    }

	/// <summary>
	/// Displays available updates.
	/// </summary>
	static void DisplayUpdates ()
	{
		GUILayout.Label("Available Updates:");

		for (var i = 0; i < updates.Count; i++) {
			if (i >= updates.Count) {
				break;
			}

			if (IsImporting(updates[i]["powerup_id"].ToString())) {
				GUILayout.Label("Downloading...");
			} else {
				if (GUILayout.Button(updates[i]["name"].ToString())) {
					DownloadPowerup(updates[i]["powerup_id"].ToString());
				}
			}
		}
	}

	/// <summary>
	/// Checks for updates.
	/// </summary>
	static void CheckForUpdates ()
	{
		checkingUpdates = true;
		var uri = new System.Uri("http://localhost:8888/api/1/powerups?engine=unity");
		var headers = LumosRequest.GetHeaders(new byte[]{});

		using (var client = new WebClient()) {
			AddHashtableToHeaders(client, headers);
			client.DownloadStringAsync(uri);
			client.DownloadStringCompleted += delegate(object sender, DownloadStringCompletedEventArgs e) {
				var response = LumosJson.Deserialize(e.Result) as IList;
				tempUpdates = new List<Dictionary<string, object>>();

				foreach (Dictionary<string, object> file in response) {
					tempUpdates.Add(file);
				}

				checkingUpdates = false;
			};
		}
	}

	/// <summary>
	/// Downloads the powerup package.
	/// </summary>
	/// <param name="powerupID">The powerup identifier.</param>
	static void DownloadPowerup (string powerupID)
	{
		var file = GetFile(powerupID);
		var tempName = "Temp/" + file["powerup_id"].ToString() + ".unitypackage";
		installedPowerups.Add(file);
		AddNewerPowerup(file);
		SaveInstalledPowerups(installedPowerups);
		updates.Remove(file);

		using (var client = new WebClient()) {
			var uri = new System.Uri(file["url"].ToString());
			client.DownloadFileAsync(uri, tempName);
			client.DownloadFileCompleted += delegate {
				importing.Add(tempName);
			};
		}
	}

	/// <summary>
	/// Imports a downloaded package.
	/// </summary>
	/// <param name="name">The name of the downloaded file.</param>
	static void ImportPackage (string name)
	{
		// TODO: use more cross-platform friendly file paths.
		var path = Application.dataPath + "/../" + name;
		AssetDatabase.ImportPackage(path, selectiveImporting);
	}

	/// <summary>
	/// Saves the installed powerups.
	/// </summary>
	/// <param name="powerups">Powerups.</param>
	static void SaveInstalledPowerups (List<Dictionary<string, object>> powerups)
	{
		var json = LumosJson.Serialize(powerups);

		if (File.Exists(filePath)) {
        	File.Delete(filePath);
        }

		// Create the file.
        using (FileStream fs = File.Create(filePath)) {
            Byte[] info = new UTF8Encoding(true).GetBytes(json);
            fs.Write(info, 0, info.Length);
        }
	}

	/// <summary>
	/// Gets the installed powerups.
	/// </summary>
	/// <returns>The installed powerups.</returns>
	static List<Dictionary<string, object>> GetInstalledPowerups ()
	{
		var json = "{[]}";

		if (File.Exists(filePath)) {
        	// Open the stream and read it back.
	        using (StreamReader sr = File.OpenText(filePath)) {
	            json = sr.ReadToEnd();
	        }
        }

		var powerups = LumosJson.Deserialize(json) as IList;
		var installedPowerups = new List<Dictionary<string, object>>();

		if (powerups != null) {
			foreach (Dictionary<string, object> powerup in powerups) {
				installedPowerups.Add(powerup);
			}
		}

		return installedPowerups;
	}

	#region Helper Functions

	/// <summary>
	/// Gets the file.
	/// </summary>
	/// <param name="powerupID">The powerup identifier.</param>
	/// <returns>The file.</returns>
	static Dictionary<string, object> GetFile (string powerupID)
	{
		foreach (var file in updates) {
			if (file["powerup_id"].ToString() == powerupID) {
				return file;
			}
		}

		return null;
	}

	/// <summary>
	/// Adds the hashtable to headers.
	/// </summary>
	/// <param name="client">Client.</param>
	/// <param name="headers">Headers.</param>
	static void AddHashtableToHeaders (WebClient client, Hashtable headers)
	{
		foreach (DictionaryEntry header in headers) {
			client.Headers.Add(header.Key as string, header.Value as string);
		}
	}

	/// <summary>
	/// Determines whether this instance is importing the specified powerupID.
	/// </summary>
	/// <param name="powerupID">The powerup identifier.</param>
	/// <returns>True if this instance is importing the specified powerup.</returns>
	static bool IsImporting (string powerupID)
	{
		foreach (var path in importing) {
			if (path.Contains(powerupID)) {
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Determines whether this instance has latest powerup the specified powerup.
	/// </summary>
	/// <param name="powerup">The powerup.</param>
	/// <returns>True if this instance has latest powerup the specified powerup.</returns>
	static bool HasLatestPowerup (Dictionary<string, object> powerup)
	{
		var powerupID = powerup["powerup_id"].ToString();

		foreach (var installed in installedPowerups) {
			Debug.Log(powerupID + " Vs. " + installed["powerup_id"].ToString());

			if (powerupID == installed["powerup_id"].ToString()) {
				var currentVersion = float.Parse(installed["version"].ToString());
				var latestVersion = float.Parse(powerup["version"].ToString());

				if (currentVersion < latestVersion) {
					break;
				} else {
					return true;
				}
			}
		}

		return false;
	}

	/// <summary>
	/// Adds the newer powerup.
	/// </summary>
	/// <param name="powerup">The powerup.</param>
	static void AddNewerPowerup (Dictionary<string, object> powerup)
	{
		var powerupID = powerup["powerup_id"].ToString();

		for (var i = 0; i > installedPowerups.Count; i++) {
			var currentID = installedPowerups[i]["powerup_id"].ToString();

			if (currentID == powerupID) {
				installedPowerups[i] = powerup;
				break;
			}
		}
	}

	#endregion
}
