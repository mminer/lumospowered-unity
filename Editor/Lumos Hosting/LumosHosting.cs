using LumosPowered;
using UnityEditor;
using UnityEngine;

public class LumosHosting : EditorWindow
{
	//public const string apiUrl = "http://hosting.lumospowered.com/api/1/";
	public const string apiUrl = "http://localhost:8085/api/1/";
	public static string password;

	static readonly GUIContent deployLabel = new GUIContent("Deploy");

	[MenuItem("Window/Lumos Add-ons/Hosting")]
	static void Init ()
	{
		var window = GetWindow<LumosHosting>();
		window.title = "Lumos Hosting";
		window.Show();
	}

	void OnGUI ()
	{
		if (GUILayout.Button(deployLabel)) {
			Uploader.BuildAndUpload();
		}
	}
}
