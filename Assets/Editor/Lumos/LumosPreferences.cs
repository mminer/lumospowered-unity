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
	static bool prefsLoaded;
	static LumosCredentials credentials;
	
    [PreferenceItem("Lumos")]
    public static void PreferencesGUI ()
	{
		if (!prefsLoaded) {
			prefsLoaded = true;
			credentials = LumosCredentialsManager.GetCredentials();
			LumosPackages.CheckForUpdates();
		}

		// General settings. 
		GUILayout.Label("General", EditorStyles.boldLabel);
		EditorGUILayout.LabelField("Version", Lumos.version);
		credentials.apiKey = EditorGUILayout.TextField(apiKeyLabel, credentials.apiKey);
		
		EditorGUILayout.Space();
		
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
		
			if (GUILayout.Button(new GUIContent("Run Setup Scripts", "Attaches required powerup scripts to the Lumos GameObject. Click this if you subscribed to new powerups since first installing Lumos."), GUILayout.Width(Screen.width / 2.9f))) {
				LumosPackages.RunSetupScripts();
			}
		GUILayout.EndHorizontal();
		
		EditorGUILayout.Space();

		// Checking for updates
		GUILayout.Label("Updates", EditorStyles.boldLabel);
		
		switch (LumosPackages.package) {
			case LumosPackages.Update.CheckingVersion:
				GUILayout.Label("Checking for updates...");
				break;
			case LumosPackages.Update.OutOfDate:
				GUILayout.Label("An update is available.");			
				
				EditorGUILayout.Space();
			
				if (GUILayout.Button("Download Lumos " + LumosPackages.latestVersion, GUILayout.Width(Screen.width / 3))) {
					Application.OpenURL("https://www.lumospowered.com/downloads");
				}
			
				break;
			case LumosPackages.Update.UpToDate:
				GUILayout.Label("You are up to date!");
				break;
		}
		
		EditorGUILayout.Space();

		// Save changed preferences.
		if (GUI.changed) {
			EditorUtility.SetDirty(credentials);
		}
    }
}
