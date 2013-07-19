// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System.Collections;
using UnityEngine;

/// <summary>
/// Used internally for testing Lumos functionality.
/// </summary>
public class Test : MonoBehaviour
{
	const string devServer = "http://localhost:8888/api/1";

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

		if (GUILayout.Button("Show achievements")) {
			Social.ShowAchievementsUI();
		}

		if (GUILayout.Button("Show leaderboards")) {
			Social.ShowLeaderboardUI();
		}

		if (GUILayout.Button("Send Events")) {
			LumosEvents.Send();
		}

		if (GUILayout.Button("Record Error Log")) {
			Debug.LogError("Oh no, a problem!");
		}

		if (GUILayout.Button("Send Logs")) {
			LumosLogs.Send();
		}
	}
}
