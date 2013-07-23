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
	static readonly GUIContent apiKeyLabel = new GUIContent("API Key", "Your game's API key from the Lumos website.");

	LumosCredentials credentials;
	bool showError;
	bool initiated;

	void OnEnable ()
    {
		credentials = LumosCredentialsManager.GetCredentials();
		LumosPackages.CompareLatestWithInstalled();
		EditorApplication.update += LumosPackages.MonitorImports;
		initiated = true;
	}

    void OnGUI ()
	{
		EditorGUILayout.HelpBox(instructions, MessageType.Info);
		EditorGUILayout.Space();

		credentials.apiKey = EditorGUILayout.TextField(apiKeyLabel, credentials.apiKey);

		// Displays an error message if something has gone wrong.
		if (showError) {
			EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
		}

		EditorGUILayout.Space();

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

		// Add Lumos prefab to scene
		Undo.RegisterSceneUndo("Add Lumos To Scene");
		var prefab = Resources.LoadAssetAtPath(prefabPath, typeof(GameObject));
		PrefabUtility.InstantiatePrefab(prefab);

		// Install missing or updated powerup scripts, if any.
		LumosPackages.UpdateAllPackages();
	}

	/*void Setup ()
	{
		foreach (var info in GetType().GetMethods(BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly)) {
   			if (info.IsFinal && info.IsPrivate) {
         		Console.WriteLine("Explicit interface implementation: {0}", info.Name);
   		}
  }
	}*/
}
