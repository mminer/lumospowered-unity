using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms;

/// <summary>
/// Lumos social GU.
/// </summary>
public partial class LumosSocialGUI : MonoBehaviour 
{
	/// <summary>
	/// The default user icon.
	/// </summary>
	public Texture2D defaultUserIcon;
	/// <summary>
	/// The friend scores scroll position.
	/// </summary>
	Vector2 friendScoresScrollPos;
	/// <summary>
	/// All scores scroll position.
	/// </summary>
	Vector2 allScoresScrollPos;
	/// <summary>
	/// The current leaderboard.
	/// </summary>
	LumosLeaderboard currentLeaderboard;
	/// <summary>
	/// The new score.
	/// </summary>
	string newScore = "";
	/// <summary>
	/// The getting leaderboards.
	/// </summary>
	bool gettingLeaderboards;
	/// <summary>
	/// The offset.
	/// </summary>
	int offset;
	
	/// <summary>
	/// Leaderboardses the screen.
	/// </summary>
	void LeaderboardsScreen()
	{
		GUILayout.Space(smallMargin);

		// Title
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Leaderboards");
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		if (LumosSocial.leaderboards == null) {
			GUILayout.Label("Loading...");
			
			if (!gettingLeaderboards) {
				LumosSocial.LoadLeaderboards();
				gettingLeaderboards = true;
			}
		} else {
			gettingLeaderboards = false;
		}
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.BeginVertical();
		
		if (LumosSocial.leaderboards != null) {
			foreach (var leaderboard in LumosSocial.leaderboards) {
				if (leaderboard.loading) {
					GUI.enabled = false;
				}
				
				if (GUILayout.Button(leaderboard.title)) {
					currentLeaderboard = leaderboard;
					screen = Screens.Scores;
					
					if (currentLeaderboard.scores == null) {
						LumosSocial.LoadLeaderboardScores(currentLeaderboard);
						
					}
				}
				
				GUI.enabled = true;
			}	
		}
		
		GUILayout.EndVertical();
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}
	
	/// <summary>
	/// Scoreses the screen.
	/// </summary>
	void ScoresScreen()
	{
		if (currentLeaderboard == null) {
			return;
		}
		
		if (currentLeaderboard.scores == null) {
			GUILayout.Label("Loading scores...");
			return;
		}
		
		if (GUILayout.Button("Leaderboards", GUILayout.Width(submitButtonWidth))) {
			screen = Screens.Leaderboards;
		}
		
		GUILayout.BeginHorizontal();
			GUILayout.Label("New Score", GUILayout.Width(labelWidth));
			newScore = GUILayout.TextField(newScore, GUILayout.Width(submitButtonWidth));
			
			if (GUILayout.Button("Submit Score", GUILayout.Width(submitButtonWidth))) {
				LumosSocial.SubmitScore(Convert.ToInt32(newScore), currentLeaderboard.id);
			}
		
			if (GUILayout.Button("Refresh Scores", GUILayout.Width(submitButtonWidth))) {
				LumosSocial.LoadLeaderboardScores(currentLeaderboard);
			}
		GUILayout.EndHorizontal();
		
		GUILayout.Space(smallMargin);
		
		// Title
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label(currentLeaderboard.title + " Scores");
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.Space(smallMargin);
		
		// Friend Scores
		if (currentLeaderboard.friendScores != null) {
			DisplayScoreLabel("Friends");
			friendScoresScrollPos = GUILayout.BeginScrollView(friendScoresScrollPos);
			DisplayScoreData(currentLeaderboard.friendScores);
			GUILayout.EndScrollView();
		}
	
		// All Scores
		DisplayScoreLabel("All Scores");
		allScoresScrollPos = GUILayout.BeginScrollView(allScoresScrollPos);
		DisplayScoreData(currentLeaderboard.scores);
		
		if (GUILayout.Button("More...")) {
			var length = currentLeaderboard.scores.Length -1;
			var lastScore = currentLeaderboard.scores[length];
			
			currentLeaderboard.LoadScores(1, lastScore.rank, delegate {
				//do something	
			});
		}
		
		GUILayout.EndScrollView();
	}
	
	/// <summary>
	/// Shows the leaderboards U.
	/// </summary>
	public static void ShowLeaderboardsUI()
	{	
		instance.screen = Screens.Leaderboards;
	}
	
	/// <summary>
	/// Displaies the score label.
	/// </summary>
	/// <param name='label'>
	/// Label.
	/// </param>
	void DisplayScoreLabel(string label)
	{
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label(label);
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}
	
	/// <summary>
	/// Displaies the score data.
	/// </summary>
	/// <param name='scores'>
	/// Scores.
	/// </param>
	void DisplayScoreData(IScore[] scores)
	{
		foreach (var score in scores) {
			GUILayout.BeginHorizontal();
				GUILayout.Label(score.rank.ToString());
				GUILayout.Label(defaultUserIcon);
				
				GUILayout.BeginVertical();
					GUILayout.Label(score.userID);
					GUILayout.Label(score.value.ToString());
				GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}
	}
}
