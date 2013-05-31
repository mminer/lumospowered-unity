// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Options that control how Lumos behaves.
/// </summary>
public static class LumosPreferences
{
	static readonly GUIContent apiKeyLabel = new GUIContent("API Key", "Your game's secret key, assigned on the website.");
	static readonly GUIContent interactiveImportLabel = new GUIContent("Interactive Import", "Preview and select the package's files before importing.");

	static bool prefsLoaded;
	static string apiKey;
	static bool interactiveImport;

    [PreferenceItem("Lumos")]
    public static void PreferencesGUI ()
	{
		if (!prefsLoaded) {
			// Add callback to the editor's update cycle.
			EditorApplication.update += LumosPackages.MonitorImports;

			apiKey = LumosCredentialsManager.GetCredentials().apiKey;
			interactiveImport = EditorPrefs.GetBool("lumos-interactive-import", false);
			prefsLoaded = true;
		}

		// General settings.
		GUILayout.Label("General", EditorStyles.boldLabel);
		apiKey = EditorGUILayout.TextField(apiKeyLabel, apiKey);
		interactiveImport = EditorGUILayout.Toggle(interactiveImportLabel, interactiveImport);
		EditorGUILayout.Space();

		// Powerups list.
		GUILayout.Label("Powerups", EditorStyles.boldLabel);

		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(" ");
			var buttonText = "Check For Updates";

			if (LumosPackages.checkingForUpdates) {
				GUI.enabled = false;
				buttonText = "Checking For Updates...";
			}

			if (GUILayout.Button(buttonText)) {
				LumosPackages.CheckForUpdates();
			}

			GUI.enabled = true;
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		if (LumosPackages.packages.Count == 0) {
			EditorGUILayout.HelpBox("To add a powerup, visit the Lumos website.", MessageType.Info);
		} else {
			DisplayPackages();
		}

		// Save changed preferences.
		if (GUI.changed) {
			LumosCredentialsManager.GetCredentials().apiKey = apiKey;
			EditorPrefs.SetBool("lumos-interactive-import", interactiveImport);
		}
    }

	/// <summary>
	/// Displays installed packages and available updates.
	/// </summary>
	static void DisplayPackages ()
	{
		foreach (var package in LumosPackages.packages.Values) {
			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.PrefixLabel(package.name);

			switch (package.status) {
				case LumosPackages.Status.Installed:
					GUILayout.Label("v" + package.version + " Installed");
					break;

				case LumosPackages.Status.NotInstalled:
					if (GUILayout.Button("Install", EditorStyles.miniButton)) {
						LumosPackages.UpdatePackage(package);
					}
					break;

				case LumosPackages.Status.UpdateAvailable:
					if (GUILayout.Button("Update to v" + package.nextVersion, EditorStyles.miniButton)) {
						LumosPackages.UpdatePackage(package);
					}
					break;

				case LumosPackages.Status.Downloading:
					GUILayout.Label("Downloading...");
					break;
			}

			EditorGUILayout.EndHorizontal();
		}
	}
}
