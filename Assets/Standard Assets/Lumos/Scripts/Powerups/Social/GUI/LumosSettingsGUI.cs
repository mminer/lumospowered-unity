using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class LumosSocialGUI : MonoBehaviour 
{
	
	string setPass = "";
	string setConfirmPass = "";
	string setEmail = "";
	string setName = "";
	string setMessage = "";
	bool setUnderage;
	bool savingSettings;
	List<Hashtable> setOther;
	Vector2 setScrollPos;
	
	
	void SettingsScreen()
	{		
		
		setScrollPos = GUILayout.BeginScrollView(setScrollPos);
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.BeginVertical();
		
		// Name
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Name", GUILayout.Width(labelWidth));
			setName = GUILayout.TextField(setName, GUILayout.Width(textBoxWidth));
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		// Password
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Password", GUILayout.Width(labelWidth));
			setPass = GUILayout.PasswordField(setPass, '*', GUILayout.Width(textBoxWidth));
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		// Confirm Password
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Confirm \nPassword", GUILayout.Width(labelWidth));
			setConfirmPass = GUILayout.PasswordField(setConfirmPass, '*', GUILayout.Width(textBoxWidth));
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		// Email
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Email", GUILayout.Width(labelWidth));
			setEmail = GUILayout.TextField(setEmail, GUILayout.Width(textBoxWidth));
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();		
		
		GUILayout.Space(smallMargin);
		
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Other");
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.Space(smallMargin);
		
		// Existing other values
		if (LumosSocial.localUser.other != null) {
			var tempOther = new Dictionary<string, object>();
			
			foreach (var f in LumosSocial.localUser.other) {
				tempOther[f.Key] = f.Value;
			}
			
			foreach (var entry in tempOther) {
				GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					GUILayout.Label(entry.Key, GUILayout.Width(labelWidth));
					LumosSocial.localUser.other[entry.Key] = GUILayout.TextField(entry.Value as string, GUILayout.Width(textBoxWidth));
					GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
		}
		
		// New other values
		if (!savingSettings && setOther != null && setOther.Count > 0) {
			foreach (var newEntry in setOther) {
				GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					newEntry["key"] = GUILayout.TextField(newEntry["key"] as string, GUILayout.Width(labelWidth));
					newEntry["value"] = GUILayout.TextField(newEntry["value"] as string, GUILayout.Width(textBoxWidth));
					GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
		}
		
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Space(labelWidth + textBoxWidth - submitButtonWidth);
		
			if (GUILayout.Button("New", GUILayout.Width(submitButtonWidth))) {
				if (setOther == null) {
					setOther = new List<Hashtable>();
				}
				
				var currentKeys = 0;
				
				if (LumosSocial.localUser.other != null) {
					currentKeys = LumosSocial.localUser.other.Count;
				}
				
				var key = "New Entry " + (setOther.Count + currentKeys + 1);
				var hash = new Hashtable();
				hash["key"] = key;
				hash["value"] = "";
				
				setOther.Add(hash);
			}
		
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.EndScrollView();
		
		GUILayout.EndVertical();
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			// Message
			Color colour = GUI.skin.label.normal.textColor;
		
			if (!savingSettings) {
				GUI.skin.label.normal.textColor = Color.red;
			}
			
			GUILayout.Label(setMessage, GUILayout.Width(textBoxWidth));
			GUI.skin.label.normal.textColor = colour;
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		// Save
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Save Settings", GUILayout.Width(submitButtonWidth))) {
				SaveSettings();	
			}
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.Space(smallMargin);
	}
	
	void SaveSettings()
	{
		if (setPass.Length > 0) {
			if (setPass != setConfirmPass) {
				setMessage = "Your passwords do not match.";
				return;
			}
		}
		
		savingSettings = true;
		setMessage = "Registering...";
		
		if (setOther != null) {
			if (LumosSocial.localUser.other == null) {
				LumosSocial.localUser.other = new Dictionary<string, object>();
			}
			
			foreach (var entry in setOther) {				
				var key = entry["key"] as string;
				var value = entry["value"] as string;
				
				// Don't save blank data
				if (value == "") {
					continue;
				}
				
				LumosSocial.localUser.other[key] = value;
			}	
		}
		
		LumosSocial.localUser.UpdateInfo(setName, setEmail, setPass, LumosSocial.localUser.other, ProcessSaveSettings);
	}
	
	void ProcessSaveSettings(bool success)
	{
		savingSettings = false;
		setOther = new List<Hashtable>();
		setMessage = "Settings saved.";
	}
	
	public static void ShowSettingsUI()
	{
		instance.setEmail = LumosSocial.localUser.email;
		instance.setName = LumosSocial.localUser.userName;
		instance.screen = Screens.Settings;
		instance.setOther = null;
	}
}
