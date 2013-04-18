using UnityEngine;
using System.Collections;

public partial class LumosSocialGUI : MonoBehaviour {
	
	string regUsername = "";
	string regPass = "";
	string regConfirmPass = "";
	string regEmail = "";
	
	string regMessage = "";
	bool registering;
	
	void RegistrationScreen()
	{
		GUILayout.Space(margin);
		
		// Back Button
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			
			if (GUILayout.Button("Back", GUILayout.Width(submitButtonWidth), GUILayout.Height(submitButtonHeight))) {
				screen = Screens.Login;
			}
		
			GUILayout.Space(textBoxWidth);
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.Space(smallMargin);
		
       	// Username
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Username", GUILayout.Width(labelWidth));
			regUsername = GUILayout.TextField(regUsername, GUILayout.Width(textBoxWidth), GUILayout.Height(textBoxHeight));
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.Space(smallMargin);
		
		// Password
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Password", GUILayout.Width(labelWidth));
			regPass = GUILayout.PasswordField(regPass, '*', GUILayout.Width(textBoxWidth), GUILayout.Height(textBoxHeight));
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.Space(smallMargin);
		
		// Confirm Password
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Confirm \nPassword", GUILayout.Width(labelWidth));
			regConfirmPass = GUILayout.PasswordField(regConfirmPass, '*', GUILayout.Width(textBoxWidth), GUILayout.Height(textBoxHeight));
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.Space(smallMargin);
		
		// Email
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Email", GUILayout.Width(labelWidth));
			regEmail = GUILayout.TextField(regEmail, GUILayout.Width(textBoxWidth), GUILayout.Height(textBoxHeight));
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.Space(smallMargin);
		
		// Message
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Space(labelWidth);
			Color colour = GUI.skin.label.normal.textColor;
		
			if (!loggingIn) {
				GUI.skin.label.normal.textColor = Color.red;
			}
		
			GUILayout.Label(regMessage, GUILayout.Width(textBoxWidth));
			GUI.skin.label.normal.textColor = colour;
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		// Submit Button
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			
			if (registering) {
				GUI.enabled = false;
			}
			
			if (GUILayout.Button("Submit", GUILayout.Width(submitButtonWidth), GUILayout.Height(submitButtonHeight))) {
				RegisterNewUser();
			}
		
			GUI.enabled = true;
		
			GUILayout.Space(largeMargin);
		GUILayout.EndHorizontal();
	}
	
	void RegisterNewUser()
	{
		if (regUsername.Length < 1 || regPass.Length < 1 || regConfirmPass.Length < 1 || regEmail.Length < 1) {
			regMessage = "Please fill in all the fields.";
			return;
		}
		
		if (regPass != regConfirmPass) {
			regMessage = "Your passwords do not match.";
			return;
		}
		
		registering = true;
		regMessage = "Registering...";
		
		LumosSocial.Register(regUsername, regPass, regEmail, ProcessRegistration);
	}
			
	void ProcessRegistration(bool success)
	{
		registering = false;
		regMessage = "";
		screen = Screens.None;
	}
}
