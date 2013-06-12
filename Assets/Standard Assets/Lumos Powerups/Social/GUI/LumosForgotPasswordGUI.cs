using UnityEngine;
using System.Diagnostics;

/// <summary>
/// Lumos social GU.
/// </summary>
public partial class LumosSocialGUI : MonoBehaviour
{
	/// <summary>
	/// The forgot username.
	/// </summary>
	string forgotUsername = "";
	/// <summary>
	/// The forgot message.
	/// </summary>
	string forgotMessage;
	/// <summary>
	/// The sent email.
	/// </summary>
	bool sentEmail;
	
	/// <summary>
	/// Forgots the password screen.
	/// </summary>
	void ForgotPasswordScreen()
	{
		GUILayout.Space(margin);
		
		if (GUILayout.Button("Back", GUILayout.Width(submitButtonWidth), GUILayout.Height(textBoxHeight))) {
			screen = Screens.Login;
		}
		
       	// Email
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Username", GUILayout.Width(labelWidth));
			forgotUsername = GUILayout.TextField(forgotUsername, GUILayout.Width(textBoxWidth), GUILayout.Height(textBoxHeight));
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.Space(smallMargin);
		
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
		
			if (!sentEmail) {
				// Submit
				if (GUILayout.Button("Submit", GUILayout.Width(submitButtonWidth), GUILayout.Height(submitButtonHeight))) {
					LumosSocial.ForgotPassword(forgotUsername, delegate {
						sentEmail = true;
					forgotMessage = "An email has been sent to confirm your password reset.";
					});
				}
			} else {
				// Message
				GUILayout.Label(forgotMessage);
			}
		
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.Space(margin);
    }
}
