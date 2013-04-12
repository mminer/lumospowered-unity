using UnityEngine;
using System.Collections;

public partial class LumosSocialGUI : MonoBehaviour {

	enum Screens { None, Login, Registration, Achievements, Leaderboards, Scores, User };
	Screens screen;
	
	float labelWidth;
	float textBoxWidth;
	float textBoxHeight;
	float submitButtonWidth;
	float submitButtonHeight;
	float margin;
	float smallMargin;
	float largeMargin;
	
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
		submitButtonHeight = textBoxHeight * 0.8f;
	}
	
	void OnGUI()
	{
		if (screen != Screens.None) {
			socialWindowRect = GUI.Window(0, socialWindowRect, SocialWindow, "");
		}
	}
	
	void SocialWindow(int windowID)
	{
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
			case Screens.User:
				// UserScreen();
				break;
			default:
				// None;
				break;
		}
	}
}
