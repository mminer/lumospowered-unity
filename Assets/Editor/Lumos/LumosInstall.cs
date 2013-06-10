using UnityEngine;
using UnityEditor;
using System.Collections;

public class LumosInstall : EditorWindow 
{
    const string prefabPath = "Assets/Standard Assets/Lumos/Lumos.prefab";
	const string errorMessage = "Enter your game's secret key from our website.";
	const string instructions = "Ensure you complete this installation in the \nscene you want Lumos to first initiate.";
	LumosCredentials credentials;
	bool showError;
    
    void OnGUI () 
	{
		// Title
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Install Lumos");
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		EditorGUILayout.Space();
		
		GUILayout.Label(instructions);
		
		EditorGUILayout.Space();
		
		// Secret key input
		credentials.apiKey = EditorGUILayout.TextField("Lumos Secret Key", credentials.apiKey, GUILayout.ExpandWidth(true));
		
		// Displays an error message if something has gone wrong
		if (showError) {
			// Change font colour
			var font = GUI.skin.label.normal;
			var fontColour = font.textColor;
			font.textColor = Color.red;
			
			// Display message
			GUILayout.Label(errorMessage);
			
			// Change font colour back
			font.textColor = fontColour;
		}
		
		GUILayout.BeginVertical(GUILayout.ExpandHeight(true), GUILayout.Height(Screen.height));
			GUILayout.FlexibleSpace();
			
			// Add and Cancel buttons
			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
			
		        if (GUILayout.Button("Cancel")) {
					showError = false;
					this.Close();
				}
			
				EditorGUILayout.Space();
				
		        if (GUILayout.Button("Install")) {
					if (credentials.apiKey.Length < 32) {
						showError = true;
					} else {
						InstallLumos();
						this.Close();
					}
				}
		
				EditorGUILayout.Space();
			GUILayout.EndHorizontal();
		
			GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
    }
	
	// Called on EditorApplication.Update 
	void ProcessUpdates ()
	{
		if (LumosPackages.updates.Count > 0 && LumosPackages.CurrentDownloadCount() < 1) {
			LumosPackages.UpdatePackage(LumosPackages.updates[0]);
			LumosPackages.updates.RemoveAt(0);
		}
	}
	
	public void Init ()
	{
		credentials = LumosCredentialsManager.GetCredentials();
		LumosPackages.SetInstalledPackages();
		EditorApplication.update += ProcessUpdates;
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
		LumosPackages.CheckAndInstallUpdates();
	}
}
