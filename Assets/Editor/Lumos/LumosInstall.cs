// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.Collections;

/// <summary>
/// Wizard that sets up Lumos before instantiating the prefab in the scene.
/// </summary>
public class LumosInstall : EditorWindow
{
    const string prefabPath = "Assets/Standard Assets/Lumos/Lumos.prefab";
	const string errorMessage = "Enter your game's API key from the Lumos website.";
	const string instructions = "Ensure you complete this installation in the scene you want Lumos to first initiate.";
	const string warningMessage = "We will check for updates and missing Unity packages, new files may be imported to your project during this setup.";
	static readonly GUIContent apiKeyLabel = new GUIContent("API Key", "Your game's API key from the Lumos website.");

	LumosPackageManager packageManager;
	LumosCredentials credentials;
	bool showError;

	void OnEnable ()
    {
		credentials = LumosCredentialsManager.GetCredentials();
		packageManager = LumosPackages.GetPackageManager();
		
		if (credentials.gameID != null) {
			if (packageManager.installing) {
				LumosPackages.CheckForUpdates();	
			}	
		}
	}

    void OnGUI ()
	{
		EditorGUILayout.HelpBox(instructions + " " + errorMessage, MessageType.Info);
		EditorGUILayout.Space();

		credentials.apiKey = EditorGUILayout.TextField(apiKeyLabel, credentials.apiKey);

		// Displays an error message if something has gone wrong.
		if (showError) {
			EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
		}

		EditorGUILayout.Space();
		
		EditorGUILayout.HelpBox(warningMessage, MessageType.Info);

		GUILayout.Label(LumosPackages.messageStatus);
		GUILayout.FlexibleSpace();

		// "Install" & "Cancel" buttons.
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

		if (GUI.changed) {
			EditorUtility.SetDirty(credentials);
		}
    }

	void InstallLumos ()
	{
		// Stop displaying the error message
		showError = false;

		var prefab = Resources.LoadAssetAtPath(prefabPath, typeof(GameObject));
		var createdObject = PrefabUtility.InstantiatePrefab(prefab);

		Undo.RegisterCreatedObjectUndo(createdObject, "Add Lumos To Scene");

		// Install missing or updated powerup scripts, if any.
		LumosPackages.UpdateAllPackages();
	}
}
