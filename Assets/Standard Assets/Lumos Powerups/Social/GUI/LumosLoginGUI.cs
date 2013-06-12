using UnityEngine;
using System.Diagnostics;

/// <summary>
/// Lumos social GU.
/// </summary>
public partial class LumosSocialGUI : MonoBehaviour
{
	/// <summary>
	/// The social window rect.
	/// </summary>
	Rect socialWindowRect;
	/// <summary>
	/// The username.
	/// </summary>
	string username = "";
	/// <summary>
	/// The password.
	/// </summary>
	string password = "";
	/// <summary>
	/// The message.
	/// </summary>
	string message = "";
	/// <summary>
	/// The logging in.
	/// </summary>
	bool loggingIn;

	/// <summary>
	/// Logins the screen.
	/// </summary>
	void LoginScreen()
	{
		GUILayout.Space(margin);
		
       	// Username
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Username", GUILayout.Width(labelWidth));
			username = GUILayout.TextField(username, GUILayout.Width(textBoxWidth), GUILayout.Height(textBoxHeight));
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.Space(smallMargin);

		// Password
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Password", GUILayout.Width(labelWidth));
			password = GUILayout.PasswordField(password, '*', GUILayout.Width(textBoxWidth), GUILayout.Height(textBoxHeight));
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		// Message
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Space(labelWidth);
			Color colour = GUI.skin.label.normal.textColor;

			if (!loggingIn) {
				GUI.skin.label.normal.textColor = Color.red;
			}

			GUILayout.Label(message, GUILayout.Width(textBoxWidth));
			GUI.skin.label.normal.textColor = colour;
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.Space(smallMargin);

		// Submit
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if (loggingIn) {
				GUI.enabled = false;
			}
		
			if (GUILayout.Button("Forgot Password", GUILayout.Width(submitButtonWidth), GUILayout.Height(submitButtonHeight))) {
				screen = Screens.ForgotPassword;
			}

			if (GUILayout.Button("Submit", GUILayout.Width(submitButtonWidth), GUILayout.Height(submitButtonHeight))) {
				SubmitLoginCredentials();
			}

			GUI.enabled = true;

			GUILayout.Space(largeMargin);
		GUILayout.EndHorizontal();

		GUILayout.Space(margin);

		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical();
				GUILayout.Label("Don't have an account?");

				if (loggingIn) {
					GUI.enabled = false;
				}

				if (GUILayout.Button(">>> Register <<<", GUI.skin.label)) {
					screen = Screens.Registration;
					return;
				}

				GUI.enabled = true;
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
    }
	
	/// <summary>
	/// Submits the login credentials.
	/// </summary>
	void SubmitLoginCredentials()
	{
		if (username.Length < 1) {
			message = "Please enter a username.";
			return;
		}

		if (password.Length < 1) {
			message = "Please enter a password.";
			return;
		}

		message = "logging in...";
		loggingIn = true;
		LumosSocial.Connect(username, password, ProcessLogin);
	}
	
	/// <summary>
	/// Processes the login.
	/// </summary>
	/// <param name='success'>
	/// Success.
	/// </param>
	void ProcessLogin(bool success)
	{
		loggingIn = false;

		if (success) {
			screen = Screens.None;
			message = "";
		} else {
			message = "There was a problem signing in.";
		}
	}
}
