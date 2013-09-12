// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using UnityEditor;
using UnityEngine;

/// <summary>
/// Wizard for instantiating the Lumos game object.
/// </summary>
public class LumosWizard : ScriptableWizard
{
	const string prefabPath = "Assets/Standard Assets/Lumos/Lumos.prefab";

	/// <summary>
	/// Called when the "Create" button is pressed.
	/// </summary>
	void OnWizardCreate ()
	{
		// Instantiate the Lumos object.
		var prefab = Resources.LoadAssetAtPath(prefabPath, typeof(GameObject));
		var createdObject = PrefabUtility.InstantiatePrefab(prefab);

		Undo.RegisterCreatedObjectUndo(createdObject, "Add Lumos To Scene");
	}

	/// <summary>
	/// Called when the "Cancel" button is pressed.
	/// </summary>
	void OnWizardOtherButton ()
	{
		Close();
	}
}
