using UnityEngine;
using System.Collections;

public partial class LumosSocialGUI : MonoBehaviour {

	enum Screens { None, Login, Registration, Achievements, Leaderboards, Scores, Profile, Settings };
	Screens screen;
	
	float labelWidth;
	float textBoxWidth;
	float textBoxHeight;
	float bigSubmitButtonWidth;
	float submitButtonWidth;
	float submitButtonHeight;
	float margin;
	float smallMargin;
	float largeMargin;
	Resolution currentRes = new Resolution();
	
	/// <summary>
	/// An instance of this class.
	/// </summary>
	public static LumosSocialGUI instance { get; private set; }
	
	/// <summary>
	/// Initializes a new instance of this class.
	/// </summary>
	LumosSocialGUI () {}
	
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
	
	void Update()
	{
		if (currentRes.width != Screen.width ||
			currentRes.height != Screen.height) {
			DetermineGUISizes();	
		}
	}
	
	void OnGUI()
	{
		if (screen != Screens.None) {
			socialWindowRect = GUI.Window(0, socialWindowRect, SocialWindow, "");
		}
	}
	
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
	
	void DetermineGUISizes()
	{
		currentRes.width = Screen.width;
		currentRes.height = Screen.height;
		
		float loginWidth = Screen.width - (Screen.width * 0.33f);
		float loginHeight = Screen.height - (Screen.height * 0.4f);
		float loginX = (Screen.width - loginWidth) / 2;
		float loginY = (Screen.height - loginHeight) / 2;
		socialWindowRect = new Rect(loginX, loginY, loginWidth, loginHeight);
		
		margin = socialWindowRect.height * 0.1f;
		smallMargin = margin / 2;
		largeMargin = margin * 1.5f;
		labelWidth = Screen.width * 0.1f;
		textBoxWidth = Screen.width * 0.40f;
		textBoxHeight = textBoxWidth / 8;
		submitButtonWidth = textBoxWidth / 4;
		bigSubmitButtonWidth = submitButtonWidth * 1.5f;
		submitButtonHeight = textBoxHeight * 0.8f;
	}
}
