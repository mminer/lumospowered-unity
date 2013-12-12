// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.Collections;

/// <summary>
/// Wizard that sets up Lumos before instantiating the prefab in the scene.
/// </summary>
///
[InitializeOnLoad]
public class LumosInstall : EditorWindow
{
    const string prefabPath = "Assets/Standard Assets/Lumos/Lumos.prefab";
	const string errorMessage = "Enter your game's API key from the Lumos website.";
	const string instructions = "Ensure you complete this installation in the scene you want Lumos to first initiate.";
	static readonly GUIContent apiKeyLabel = new GUIContent("API Key", "Your game's API key from the Lumos website.");

	static LumosCredentials credentials;
	bool showError;


	static LumosInstall ()
	{
		EditorApplication.projectWindowChanged += PromptLumosInstall;
		EditorApplication.hierarchyWindowChanged += PromptLumosInstall;
	}

	static void PromptLumosInstall ()
	{
		EditorApplication.projectWindowChanged -= PromptLumosInstall;
		EditorApplication.hierarchyWindowChanged -= PromptLumosInstall;

		credentials = LumosCredentialsManager.GetCredentials();

		if (credentials.apiKey.Length >= 32) {
			return;
		}

		// Make window pop up
		 EditorWindow.GetWindow<LumosInstall>(true, "Install Lumos");
	}

	void OnEnable ()
	{
		credentials = LumosCredentialsManager.GetCredentials();

		if (LumosPackages.package == LumosPackages.Update.None || LumosPackages.package == LumosPackages.Update.CheckingVersion) {
			LumosPackages.CheckForUpdates();
		}
	}

    void OnGUI ()
	{
		if (credentials == null) {
			return;
		}

		EditorGUILayout.HelpBox(instructions + " " + errorMessage, MessageType.Info);
		EditorGUILayout.Space();

		credentials.apiKey = EditorGUILayout.TextField(apiKeyLabel, credentials.apiKey);

		// Displays an error message if something has gone wrong.
		if (showError) {
			EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
		} else {
			EditorGUILayout.Space();
			EditorGUILayout.Space();
		}

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Version", Lumos.version);

		if (LumosPackages.package == LumosPackages.Update.CheckingVersion) {
			GUILayout.Label("Checking for updates...");
		} else if (LumosPackages.package == LumosPackages.Update.OutOfDate) {
			EditorGUILayout.Space();
			EditorGUILayout.HelpBox("Your Lumos version is out of date. Please download the latest version from our website.", MessageType.Warning);
			EditorGUILayout.Space();

			if (GUILayout.Button("Download Lumos " + LumosPackages.latestVersion, GUILayout.Width(Screen.width / 2))) {
				Application.OpenURL("https://www.lumospowered.com/downloads");
			}
		}

		GUILayout.FlexibleSpace();

		// Install & Cancel buttons
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Cancel")) {
				showError = false;
				this.Close();
			}

			if (GUILayout.Button("Install")) {
				if (credentials.apiKey.Length < 32) {
					showError = true;
				} else {
					InstallLumos();
				}
			}

			EditorGUILayout.Space();
		GUILayout.EndHorizontal();

		EditorGUILayout.Space();

		// Save the new credentials
		if (GUI.changed) {
			EditorUtility.SetDirty(credentials);
		}
	}

	void InstallLumos ()
	{
		showError = false;
		const string undoTitle = "Add Lumos To Scene";
		var prefab = Resources.LoadAssetAtPath(prefabPath, typeof(GameObject));

		#if UNITY_3_5
		Undo.RegisterSceneUndo(undoTitle);
		PrefabUtility.InstantiatePrefab(prefab);
		#else
		var obj = PrefabUtility.InstantiatePrefab(prefab);
		Undo.RegisterCreatedObjectUndo(obj, undoTitle);
		#endif

		LumosPackages.RunSetupScripts();
		this.Close();
	}
}
