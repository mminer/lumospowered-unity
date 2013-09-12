// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom inspector for the Lumos game object.
/// </summary>
[CustomEditor(typeof(Lumos))]
public class LumosInspector : Editor
{
	GUIContent runInEditorLabel = new GUIContent("Record While In Editor", "Send data to Lumos during development.");

	override public void OnInspectorGUI ()
	{
		var lumos = target as Lumos;
		
		EditorGUIUtility.LookLikeInspector();
		EditorGUI.indentLevel = 1;

		lumos.runWhileInEditor = EditorGUILayout.Toggle(runInEditorLabel, lumos.runWhileInEditor);

		if (GUI.changed) {
			EditorUtility.SetDirty(lumos);
		}
	}
}
