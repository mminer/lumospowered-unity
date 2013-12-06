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
		var prefab = Resources.LoadAssetAtPath(prefabPath, typeof(GameObject));
		const string undoTitle = "Add Lumos To Scene";

		#if UNITY_3_5
		Undo.RegisterSceneUndo(undoTitle);
		PrefabUtility.InstantiatePrefab(prefab);
		#else
		var obj = PrefabUtility.InstantiatePrefab(prefab);
		Undo.RegisterCreatedObjectUndo(obj, undoTitle);
		#endif
	}

	/// <summary>
	/// Called when the "Cancel" button is pressed.
	/// </summary>
	void OnWizardOtherButton ()
	{
		Close();
	}
}
