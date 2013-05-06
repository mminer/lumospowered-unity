using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using System.Net;

/// <summary>
/// Lumos preferences.
/// </summary>
public class LumosPreferences 
{
	/// <summary>
	/// The _updates.
	/// </summary>
	static List<Dictionary<string, object>> _updates;
	static List<Dictionary<string, object>> updates {
		get { return _updates; } 
		set {
			_updates = value;
			var data = LumosJson.Serialize(_updates);
			EditorPrefs.SetString("lumos-updates", data);
		}
	}
	
	static List<string> importing = new List<string>();
	static bool checkingUpdates;
	static List<Dictionary<string, object>> tempUpdates = new List<Dictionary<string, object>>();
	
	/// <summary>
	/// Runs the first time this UI is displayed.
	/// </summary>
	public static void Start ()
	{
		EditorApplication.update += Update;
		
		var data = EditorPrefs.GetString("lumos-updates", "{[]}");
		var fileList = LumosJson.Deserialize(data) as IList;
		var files = new List<Dictionary<string, object>>();

		foreach (Dictionary<string, object> file in fileList) {
			files.Add(file);
		}	
		
		updates = files;
	}
	
	/// <summary>
	/// Runs every frame with EditorApplication.update 
	/// </summary>
	public static void Update ()
	{
		// Done here to get around main thread issues with Async calls
		if (tempUpdates.Count > 0) {
			updates = tempUpdates;
			tempUpdates = new List<Dictionary<string, object>>();
		}
		
		// Done here to get around main thread issues with Async calls
		if (importing.Count > 0) {
			ImportPackage(importing[0]);
			importing.RemoveAt(0);
		}
	}
	
    [PreferenceItem("Lumos")]
    public static void PreferencesGUI () 
	{
		if (updates == null) {
			Start();
		}
		
		if (checkingUpdates) {
			GUILayout.Label("Checking for updates...");
		} else {
			if (GUILayout.Button("Check for Updates")) {
				CheckForUpdates();
			}	
		}
		
		if (updates != null && updates.Count > 0) {
			DisplayUpdates();
		}
    }
	
	/// <summary>
	/// Displays available updates.
	/// </summary>
	static void DisplayUpdates ()
	{
		GUILayout.Label("Available Updates:");
		
		for (var i = 0; i < updates.Count; i++) {
			if (i >= updates.Count) {
				break;
			}
			
			if (IsImporting(updates[i]["powerup_id"].ToString())) {
				GUILayout.Label("Downloading...");
			} else {
				if (GUILayout.Button(updates[i]["name"].ToString())) {
					DownloadPowerup(updates[i]["powerup_id"].ToString());
				}	
			}
		}
	}

	/// <summary>
	/// Checks for updates.
	/// </summary>
	/// <param name='callback'>
	/// Callback.
	/// </param>
	static void CheckForUpdates ()
	{
		checkingUpdates = true;
		var uri = new System.Uri("http://localhost:8888/api/1/powerups?engine=unity");
		var headers = LumosRequest.GetHeaders(new byte[]{});
		
		using (var client = new WebClient()) {
			AddHashtableToHeaders(client, headers);
			client.DownloadStringAsync(uri);
			client.DownloadStringCompleted += delegate(object sender, DownloadStringCompletedEventArgs e) {
				var response = LumosJson.Deserialize(e.Result) as IList;
				tempUpdates = new List<Dictionary<string, object>>();
			
				foreach (Dictionary<string, object> file in response) {
					tempUpdates.Add(file);
				}
				
				checkingUpdates = false;
			};
		}
	}
	
	/// <summary>
	/// Downloads the powerup.
	/// </summary>
	/// <param name='powerupID'>
	/// Powerup I.
	/// </param>
	static void DownloadPowerup (string powerupID)
	{
		var file = GetFile(powerupID);
		var tempName = "Temp/" + file["powerup_id"].ToString() + ".unitypackage";
		updates.Remove(file);
		
		using (var client = new WebClient()) {
			var uri = new System.Uri(file["url"].ToString());
			client.DownloadFileAsync(uri, tempName);
			client.DownloadFileCompleted += delegate {
				importing.Add(tempName);
			};
		}
	}
	
	/// <summary>
	/// Imports the package.
	/// </summary>
	/// <param name='name'>
	/// Name.
	/// </param>
	static void ImportPackage (string name)
	{
		var path = Application.dataPath + "/../" + name;
		AssetDatabase.ImportPackage(path, true);
		
	}
	
	/// <summary>
	/// Gets the file.
	/// </summary>
	/// <returns>
	/// The file.
	/// </returns>
	/// <param name='powerupID'>
	/// Powerup I.
	/// </param>
	static Dictionary<string, object> GetFile (string powerupID)
	{
		foreach (var file in updates) {
			if (file["powerup_id"].ToString() == powerupID) {
				return file;
			}
		}
		
		return null;
	}
	
	/// <summary>
	/// Adds the hashtable to headers.
	/// </summary>
	/// <param name='client'>
	/// Client.
	/// </param>
	/// <param name='headers'>
	/// Headers.
	/// </param>
	static void AddHashtableToHeaders (WebClient client, Hashtable headers)
	{
		foreach (DictionaryEntry header in headers) {
			client.Headers.Add(header.Key as string, header.Value as string);
		}
	}
	
	/// <summary>
	/// Determines whether this instance is importing the specified powerupID.
	/// </summary>
	/// <returns>
	/// <c>true</c> if this instance is importing the specified powerupID; otherwise, <c>false</c>.
	/// </returns>
	/// <param name='powerupID'>
	/// If set to <c>true</c> powerup I.
	/// </param>
	static bool IsImporting (string powerupID)
	{
		foreach (var path in importing) {
			if (path.Contains(powerupID)) {
				return true;
			}
		} 
		
		return false;
	}
}
