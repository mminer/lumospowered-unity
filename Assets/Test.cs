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
	string leaderboardID = "my-leaderboard";
	string score = "";

	void Awake ()
	{
		SetPowerupUrlsToLocal();
		Lumos.debug = true;

		/*
		LumosEvents.Record("what_up", 0, true, "levels");
		LumosEvents.Record("event_test", Time.time, true, "loading");
		*/
	}

	void SetPowerupUrlsToLocal ()
	{
		LumosPlayer.baseUrl = devServer;
		LumosAnalytics.baseUrl = devServer;
		LumosDiagnostics.baseUrl = devServer;
		LumosSocial.baseUrl = devServer;
	}

	void OnGUI()
	{
		/*
		if (GUILayout.Button("Delete prefs")) {
			EditorPrefs.DeleteKey("lumos-installed-packages");
			EditorPrefs.DeleteKey("lumos-latest-packages");
			EditorPrefs.SetBool("lumos-installing", false);
			EditorPrefs.DeleteKey("lumos-install-queue");
		}
		*/

		// Social:

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

			// Score
			GUILayout.BeginHorizontal();
				score = GUILayout.TextField(score);

				if (GUILayout.Button("Record Score", GUILayout.ExpandWidth(false))) {
					Social.ReportScore(Convert.ToInt64(score), leaderboardID,
						success => {
							if (success) {
								score = "";
							}
						});
				}
			GUILayout.EndHorizontal();
		GUILayout.EndVertical();

		// Diagnostics:

		GUILayout.Space(10);
		GUILayout.Label("Diagnostics");
		GUILayout.BeginVertical(GUI.skin.box);
			if (GUILayout.Button("Record Error Log")) {
				Debug.LogError("Oh no, a problem!");
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
	}
}
