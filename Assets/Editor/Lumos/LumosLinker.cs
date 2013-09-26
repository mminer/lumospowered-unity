using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;

[InitializeOnLoad]
public class LumosLinker : EditorWindow
{
	const string linkPath = "Assets/Standard Assets/Lumos/link.xml";
	const string prefsKey =  "Prompted-Lumos-iOS-Link";
	const string alertMessage = "Important! Please read.";
	const string infoMessage = "Lumos requires a \"link.xml\" file in the root of your project to prevent code stripping optimizations from removing certain libraries it depends on.";
	const string addMessage = "Clicking the \"Add\" button will put this required file into the root of your directory, or you can move it manually from 'Standard Assets/Lumos/'.";
	const string moreInfoMessage = "About link files: \n";
	const string linkWebsite = "http://docs.unity3d.com/Documentation/Manual/iphone-playerSizeOptimization.html";

	/// <summary>
	/// Initializes the <see cref="LumosLinker"/> class.
	/// </summary>
	static LumosLinker ()
	{
		if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iPhone && 
		    (PlayerSettings.strippingLevel == StrippingLevel.StripByteCode || 
		 	PlayerSettings.strippingLevel == StrippingLevel.UseMicroMSCorlib) &&
		    !PlayerPrefs.HasKey(prefsKey)) {

			// A special file allowing the code stripping to work and still run Lumos
			EditorWindow.GetWindow<LumosLinker>(true, "Add iOS Link.xml File");
			PlayerPrefs.SetInt(prefsKey, 1);
		}
	}

	/// <summary>
	/// Copies the link file.
	/// </summary>
	static void CopyLinkFile ()
	{
		try {
			FileUtil.CopyFileOrDirectory(linkPath, "Assets/link.xml");
			AssetDatabase.Refresh();
		} catch (IOException) {
			Debug.LogError("Lumos tried to unsuccessfully copy a link.xml file to your project's root. " +
						   "This error may have happened because you already have a link.xml file. " +
						   "Please open 'Standard Assets/Lumos/link.xml' and make sure your " +
						   "own link file includes our requirements too.");
		}
	}

	/// <summary>
	/// Raises the GUI event.
	/// </summary>
	void OnGUI ()
	{
		// Copy
		EditorGUILayout.HelpBox(alertMessage, MessageType.Warning);

		EditorGUILayout.Space();

		GUILayout.Label(infoMessage + "\n\n" + addMessage);

		EditorGUILayout.Space();

		if (GUILayout.Button(moreInfoMessage + linkWebsite, GUI.skin.label)) {
			Application.OpenURL(linkWebsite);
		}

		GUILayout.FlexibleSpace();
		
		// "Install" & "Cancel" buttons
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			
			if (GUILayout.Button("Cancel")) {
				this.Close();
			}

			EditorGUILayout.Space();
			
			if (GUILayout.Button("Add Link.xml")) {
				CopyLinkFile();
				this.Close();
			}
			
			EditorGUILayout.Space();
			EditorGUILayout.Space();
		GUILayout.EndHorizontal();
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();
	}
}
