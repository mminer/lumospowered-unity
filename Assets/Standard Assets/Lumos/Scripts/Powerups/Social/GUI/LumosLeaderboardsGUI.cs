using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms;

public partial class LumosSocialGUI : MonoBehaviour 
{

	
	public Texture2D defaultUserIcon;
	
	Vector2 friendScoresScrollPos;
	Vector2 allScoresScrollPos;
	LumosLeaderboard currentLeaderboard;
	string newScore = "";
	bool gettingLeaderboards;
	int offset;
	
	
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
	
	public static void ShowLeaderboardsUI()
	{	
		instance.screen = Screens.Leaderboards;
	}
	
	void DisplayScoreLabel(string label)
	{
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label(label);
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}
	
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
