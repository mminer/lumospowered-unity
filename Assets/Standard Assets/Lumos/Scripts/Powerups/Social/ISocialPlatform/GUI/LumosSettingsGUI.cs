using UnityEngine;
using System.Collections;

public partial class LumosSocialGUI : MonoBehaviour {
	
	string setPass = "";
	string setConfirmPass = "";
	string setEmail = "";
	string setName = "";
	string setMessage = "";
	bool setUnderage;
	
	
	void SettingsScreen()
	{		
		// Name
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Name", GUILayout.Width(labelWidth));
			setName = GUILayout.TextField(setName, GUILayout.Width(textBoxWidth));
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.Space(smallMargin);
		
		// Password
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Password", GUILayout.Width(labelWidth));
			setPass = GUILayout.PasswordField(setPass, '*', GUILayout.Width(textBoxWidth));
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.Space(smallMargin);
		
		// Confirm Password
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Confirm \nPassword", GUILayout.Width(labelWidth));
			setConfirmPass = GUILayout.PasswordField(setConfirmPass, '*', GUILayout.Width(textBoxWidth));
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.Space(smallMargin);
		
		// Email
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Email", GUILayout.Width(labelWidth));
			setEmail = GUILayout.TextField(setEmail, GUILayout.Width(textBoxWidth));
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		if (GUILayout.Button("Save", GUILayout.Width(submitButtonWidth))) {
			SaveSettings();	
		}
	}
	
	void SaveSettings()
	{
		if (setPass.Length > 0) {
			if (setPass != setConfirmPass) {
				
			}
		}
	}
	
	public static void ShowSettingsUI()
	{
		instance.setEmail = LumosSocial.localUser.email;
		instance.screen = Screens.Settings;
	}
}
