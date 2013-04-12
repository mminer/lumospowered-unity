using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms;

public partial class LumosSocialGUI : MonoBehaviour {

	
	public Texture2D defaultUserIcon;
	
	Vector2 friendScoresScrollPos = new Vector2(0, 0);
	Vector2 allScoresScrollPos = new Vector2(0, 0);
	LumosLeaderboard currentLeaderboard;
	string newScore = "";
	bool gettingLeaderboards;
	
	
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
		
		GUILayout.EndVertical();
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}
	
	void ScoresScreen()
	{
		if (currentLeaderboard == null) {
			return;
		}
		
		if (currentLeaderboard.scores == null) {
			GUILayout.Label("Loading scores...");
			return;
		}
		
		if (GUILayout.Button("Back")) {
			screen = Screens.Leaderboards;
		}
		
		if (GUILayout.Button("Refresh Scores")) {
			LumosSocial.LoadLeaderboardScores(currentLeaderboard);
		}
		
		GUILayout.BeginHorizontal();
			GUILayout.Label("New Score", GUILayout.Width(labelWidth));
			newScore = GUILayout.TextField(newScore, GUILayout.Width(textBoxWidth / 2));
			
			if (GUILayout.Button("Submit Score", GUILayout.Width(submitButtonWidth))) {
				LumosSocial.SubmitScore(Convert.ToInt32(newScore), currentLeaderboard.id);
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
		
		if (currentLeaderboard.friendScores != null) {
			DisplayScores("Friends", currentLeaderboard.friendScores, friendScoresScrollPos);
		}
	
		DisplayScores("All Scores", currentLeaderboard.scores, allScoresScrollPos);
	}
	
	public static void ShowLeaderboardsUI()
	{
		instance.screen = Screens.Leaderboards;
	}
	
	void DisplayScores(string label, IScore[] scores, Vector2 scrollPosition)
	{
		// Label
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label(label);
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		// Scores
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);

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
		
		GUILayout.EndScrollView();
	}
}
