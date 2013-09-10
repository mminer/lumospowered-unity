// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Used internally for testing Lumos functionality.
/// </summary>
public class Test : MonoBehaviour
{
	const string devServer = "http://localhost:8888/api/1";

	public bool automaticallyLogIn = true;
	public string username = "testuser";
	public string password = "test";

	string leaderboardID = "my-leaderboard";
	string achievementID = "my-achievement";
	string score = "10";
	string progress = "100";
	string logMessage = "Oh no, an error!";
	
	bool clicked = false;
	

	void Awake ()
	{
		LumosAnalytics.RecordEvent("hey");
		SetPowerupUrlsToLocal();
		Lumos.debug = true;
		Lumos.OnReady += Callback;
	}
	
	/*
	void Start ()
	{
		if (automaticallyLogIn) {
			var user = new LumosUser(username, password);

			Social.Active.Authenticate(user,
				success => {
					if (success) {
						Debug.Log("[Lumos] Logged in with username " + username);
					}
				});
		}
	}
	 */
	void SetPowerupUrlsToLocal ()
	{
		LumosPlayer.baseUrl = devServer;
		LumosAnalytics.baseUrl = devServer;
		LumosDiagnostics.baseUrl = devServer;
		LumosSocial.baseUrl = devServer;
	}
	
	void Callback ()
	{
		LumosAnalytics.RecordEvent("thisisatest");
	}
	
	void OnGUI ()
	{		
		/*if (GUILayout.Button("Delete prefs")) {
			EditorPrefs.DeleteKey("lumos-installed-packages");
			EditorPrefs.DeleteKey("lumos-latest-packages");
			EditorPrefs.SetBool("lumos-installing", false);
			EditorPrefs.DeleteKey("lumos-install-queue");
		}*/

		// Social:
		/*
		GUILayout.Label("Social");
		GUILayout.BeginVertical(GUI.skin.box);
			// Windows
			GUILayout.BeginHorizontal();
				if (GUILayout.Button("Achievements")) {
					Social.ShowAchievementsUI();
				}

				if (GUILayout.Button("Leaderboards")) {
					Social.ShowLeaderboardUI();
				}
			GUILayout.EndHorizontal();

			// Leaderboard ID
			GUILayout.BeginHorizontal();
				GUILayout.Label("Leaderboard");
				leaderboardID = GUILayout.TextField(leaderboardID);
			GUILayout.EndHorizontal();

			// Achievement ID
			GUILayout.BeginHorizontal();
				GUILayout.Label("Achievement");
				achievementID = GUILayout.TextField(achievementID);
			GUILayout.EndHorizontal();

			// Score
			GUILayout.BeginHorizontal();
				score = GUILayout.TextField(score);

				if (GUILayout.Button("Report Score", GUILayout.ExpandWidth(false))) {
					Social.ReportScore(Convert.ToInt64(score), leaderboardID,
						success => {
							if (success) {
								score = "";
							}
						});
				}
			GUILayout.EndHorizontal();

			// Progress
			GUILayout.BeginHorizontal();
				progress = GUILayout.TextField(progress);

				if (GUILayout.Button("Report Progress", GUILayout.ExpandWidth(false))) {
					Social.ReportProgress(achievementID, Convert.ToDouble(progress),
						success => {
							if (success) {
								progress = "";
							}
						});
				}
			GUILayout.EndHorizontal();
		GUILayout.EndVertical();

		// Diagnostics:

		GUILayout.Space(10);
		GUILayout.Label("Diagnostics");
		GUILayout.BeginVertical(GUI.skin.box);
			logMessage = GUILayout.TextField(logMessage);

			if (GUILayout.Button("Record Error")) {
				Debug.LogError(logMessage);
			}

			if (GUILayout.Button("Record Warning")) {
				Debug.LogWarning(logMessage);
			}

			if (GUILayout.Button("Send Logs")) {
				LumosLogs.Send();
			}
		GUILayout.EndVertical();

		// Analytics:

		GUILayout.Space(10);
		GUILayout.Label("Analytics");
		GUILayout.BeginVertical(GUI.skin.box);
			if (GUILayout.Button("Send Events")) {
				LumosEvents.Send();
			}
		GUILayout.EndVertical();
		*/
	}
}
