// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using UnityEditor;
using UnityEngine;

/// <summary>
/// Wizard for instantiating the Lumos game object.
/// </summary>
public class LumosWizard : ScriptableWizard
{
	const string prefabPath = "Assets/Standard Assets/Lumos/Lumos.prefab";
	public string apiKey = "";

	/// <summary>
	/// Called when the "Create" button is pressed.
	/// </summary>
	void OnWizardCreate ()
	{
		Undo.RegisterSceneUndo("Add Lumos To Scene");

		// Instantiate the Lumos object
		var prefab = Resources.LoadAssetAtPath(prefabPath, typeof(GameObject));
		var lumosGameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
		lumosGameObject.GetComponent<Lumos>().apiKey = apiKey;
	}

	/// <summary>
	/// Called when the "Cancel" button is pressed.
	/// </summary>
	void OnWizardOtherButton ()
	{
		Close();
	}

	/// <summary>
	/// UI elements.
	/// </summary>
	void OnWizardUpdate ()
	{
		helpString = "Fill in your game's API key below.";
	}
}
