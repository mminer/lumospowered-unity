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
	bool initiated;
    
    void OnGUI () 
	{
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
		
		EditorGUILayout.Space();
		
		GUILayout.Label(LumosPackages.messageStatus);
		
		GUILayout.BeginVertical(GUILayout.ExpandHeight(true), GUILayout.Height(Screen.height));
			// Push to bottom
			GUILayout.FlexibleSpace();
			
			// "Install" & "Cancel" buttons
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
					}
				}
				
				// Margin 
				EditorGUILayout.Space();
			GUILayout.EndHorizontal();
		
			GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
		
		if (GUI.changed) {
			EditorUtility.SetDirty(credentials);
		}
    }
	
	/// <summary>
	/// Initialize this panel.
	/// </summary>
	public void Init ()
	{
		credentials = LumosCredentialsManager.GetCredentials();
		LumosPackages.CompareLatestWithInstalled();
		EditorApplication.update += LumosPackages.MonitorImports;
		initiated = true;
	}
	
	void OnEnable ()
    {
		Init();
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
