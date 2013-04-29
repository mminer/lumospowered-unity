using System;
using UnityEngine;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;

/// <summary>
/// Lumos social GU.
/// </summary>
public partial class LumosSocialGUI : MonoBehaviour {
	/// <summary>
	/// Screens.
	/// </summary>
	enum Screens { None, Login, Registration, ForgotPassword, Achievements, Leaderboards, Scores, Profile, Settings };
	/// <summary>
	/// The screen.
	/// </summary>
	Screens screen;
	
	/// <summary>
	/// The width of the label.
	/// </summary>
	float labelWidth;
	/// <summary>
	/// The width of the text box.
	/// </summary>
	float textBoxWidth;
	/// <summary>
	/// The height of the text box.
	/// </summary>
	float textBoxHeight;
	/// <summary>
	/// The width of the big submit button.
	/// </summary>
	float bigSubmitButtonWidth;
	/// <summary>
	/// The width of the submit button.
	/// </summary>
	float submitButtonWidth;
	/// <summary>
	/// The height of the submit button.
	/// </summary>
	float submitButtonHeight;
	/// <summary>
	/// The margin.
	/// </summary>
	float margin;
	/// <summary>
	/// The small margin.
	/// </summary>
	float smallMargin;
	/// <summary>
	/// The large margin.
	/// </summary>
	float largeMargin;
	/// <summary>
	/// The current res.
	/// </summary>
	Resolution currentRes = new Resolution();

	/// <summary>
	/// An instance of this class.
	/// </summary>
	public static LumosSocialGUI instance { get; private set; }

	/// <summary>
	/// Initializes a new instance of this class.
	/// </summary>
	LumosSocialGUI () {}
	
	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake()
	{
		// Prevent multiple instances of LumosSocialGUI from existing.
		// Necessary because DontDestroyOnLoad keeps the object between scenes.
		if (instance != null) {
			Destroy(gameObject);
			return;
		}

		instance = this;

		screen = Screens.Login;
		DetermineGUISizes();
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update()
	{
		if (currentRes.width != Screen.width ||
			currentRes.height != Screen.height) {
			DetermineGUISizes();
		}
	}
	
	/// <summary>
	/// Raises the GU event.
	/// </summary>
	void OnGUI()
	{
		if (screen != Screens.None) {
			socialWindowRect = GUI.Window(0, socialWindowRect, SocialWindow, "");
		}
	}
	
	/// <summary>
	/// Socials the window.
	/// </summary>
	/// <param name='windowID'>
	/// Window I.
	/// </param>
	void SocialWindow(int windowID)
	{
		if (LumosSocial.localUser != null && LumosSocial.localUser.authenticated) {
			GUILayout.BeginHorizontal();
				if (GUILayout.Button("My Profile", GUILayout.Width(submitButtonWidth))) {
					LumosSocialGUI.ShowProfileUI();
				}
			GUILayout.EndHorizontal();

			GUILayout.Space(smallMargin);
		}

		switch(screen) {
			case Screens.Login:
				LoginScreen();
				break;
			case Screens.ForgotPassword:
				ForgotPasswordScreen();
				break;
			case Screens.Registration:
				RegistrationScreen();
				break;
			case Screens.Achievements:
				AchievementsScreen();
				break;
			case Screens.Leaderboards:
				LeaderboardsScreen();
				break;
			case Screens.Scores:
				ScoresScreen();
				break;
			case Screens.Profile:
				ProfileScreen();
				break;
			case Screens.Settings:
				SettingsScreen();
				break;
			default:
				// None;
				break;
		}
	}
	
	/// <summary>
	/// Determines the GUI sizes.
	/// </summary>
	void DetermineGUISizes()
	{
		currentRes.width = Screen.width;
		currentRes.height = Screen.height;

		float loginWidth = Screen.width - (Screen.width * 0.33f);
		float loginHeight = Screen.height - (Screen.height * 0.3f);
		float loginX = (Screen.width - loginWidth) / 2;
		float loginY = (Screen.height - loginHeight) / 2;
		socialWindowRect = new Rect(loginX, loginY, loginWidth, loginHeight);

		margin = socialWindowRect.height * 0.1f;
		smallMargin = margin / 2;
		largeMargin = margin * 1.5f;
		labelWidth = Screen.width * 0.1f;
		textBoxWidth = Screen.width * 0.2f;
		textBoxHeight = textBoxWidth / 8;
		submitButtonWidth = textBoxWidth / 2;
		bigSubmitButtonWidth = submitButtonWidth * 1.5f;
		submitButtonHeight = submitButtonWidth * 0.15f;
	}
}
