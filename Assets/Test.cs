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
		//SetPowerupUrlsToLocal();
		Lumos.debug = true;
		Debug.Log("test 1");
		Debug.Log("test 2");
		LumosSocialGUI.ShowWindow(LumosGUIWindow.Login);
		/*
		LumosEvents.Record("what_up", 0, true, "levels");
		LumosEvents.Record("event_test", Time.time, true, "loading");
		*/
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
		Debug.Log("closed!");
	}
	
	LumosUser myPlayer;
			
	void LoadFriendRequests()
	{
		// Casting as LumosUser gives you more methods to use
		myPlayer = Social.localUser as LumosUser;
		
		myPlayer.LoadFriendRequests(result => {
			if (result) {
				foreach (var userProfile in myPlayer.friendRequests) {
					Debug.Log("Pending friend request from " + userProfile.userName);	
				}
			} else {
				Debug.LogError("Unable to load friend requests");
			}
		});
	}
	
	void SendFriendRequest(string friendID)
	{
		foreach (var friend in myPlayer.friends) {
			if (friend.id == friendID) {
				// Don't send a friend request to someone that is already a friend
				return;
			}
		}
		
		// Send friend request
		myPlayer.SendFriendRequest(friendID, result => {
			if (result) {
				Debug.Log("Friend request sent to " + friendID);
			} else {
				Debug.LogError("Unable to send friend request to " + friendID);
			}
		});
	}
	
	void AcceptFriendRequest(string friendID, bool accept)
	{
		if (accept) {
			myPlayer.AcceptFriendRequest(friendID, result => {
				if (result) {
					Debug.Log("Friend request from " + friendID + " accepted");
				} else {
					Debug.LogError("Unable to accept friend request from " + friendID);
				}
			});
		} else {
			myPlayer.DeclineFriendRequest(friendID, result => {
				if (result) {
					Debug.Log("Declined request from " + friendID);
				} else {
					Debug.LogError("Unable to decline request from " + friendID);
				}
			});
		}
	}
	
	void RemoveFriend(string friendID)
	{
		myPlayer.RemoveFriend(friendID, result => {
			if (result) {
				Debug.Log("Removed friend: " + friendID);
			} else {
				Debug.LogError("Unable to remove friend: " + friendID);
			}
		});
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
