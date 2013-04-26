using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms;

public partial class LumosSocialGUI : MonoBehaviour 
{
	LumosUser currentUser;
	
	Vector2 proOtherScrollPos;
	Vector2 proFriendsScrollPos;
	Vector2 proScoresScrollPos;
	string friendToAdd = "";
	
	
	void ProfileScreen()
	{
		if (currentUser == null) {
			currentUser = Social.localUser as LumosUser;
		}
		
		GUILayout.Space(smallMargin);

		// Title
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Profile");
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
			GUILayout.Label(defaultUserIcon);
		
			GUILayout.BeginVertical();
				GUILayout.Label(currentUser.userID, GUILayout.Width(submitButtonWidth * 1.5f));
				GUILayout.Label(currentUser.email, GUILayout.Width(submitButtonWidth * 1.5f));
			GUILayout.EndVertical();
		
			GUILayout.BeginVertical();
				if (GUILayout.Button("Achievements", GUILayout.Width(submitButtonWidth))) {
					LumosSocialGUI.ShowAchievements();
				}
		
				if (GUILayout.Button("Leaderboards", GUILayout.Width(submitButtonWidth))) {
					LumosSocialGUI.ShowLeaderboardsUI();
				}
		
				if (GUILayout.Button("Settings", GUILayout.Width(submitButtonWidth))) {
					LumosSocialGUI.ShowSettingsUI();
				}
			GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		
		GUILayout.Space(smallMargin);
		
		GUILayout.BeginHorizontal();
			// Other info
			GUILayout.BeginVertical();
				GUILayout.Label("Other");
				
				if (LumosSocial.localUser.other != null) {
					proOtherScrollPos = GUILayout.BeginScrollView(proOtherScrollPos);
		
					foreach (var other in LumosSocial.localUser.other) {
						GUILayout.BeginHorizontal();
							GUILayout.Label(other.Key, GUILayout.Width(submitButtonWidth));
							GUILayout.Label(other.Value.ToString(), GUILayout.Width(submitButtonWidth));
						GUILayout.EndHorizontal();
					}
		
					GUILayout.EndScrollView();
				}
			GUILayout.EndVertical();
		
			GUILayout.Space(smallMargin);
			
			// Friend List
			GUILayout.BeginVertical();
				GUILayout.Label("Friends");
		
				proFriendsScrollPos = GUILayout.BeginScrollView(proFriendsScrollPos);
				
				GUILayout.BeginHorizontal();
					friendToAdd = GUILayout.TextField(friendToAdd, GUILayout.Width(margin));
					
					if (GUILayout.Button("Send Request", GUILayout.Width(bigSubmitButtonWidth))) {
						if (friendToAdd.Length > 0) {
							LumosSocial.localUser.SendFriendRequest(friendToAdd, delegate {
								// do something
							});
						}
					}
				GUILayout.EndHorizontal();
		
				if (LumosSocial.localUser.friendRequests != null) {
					foreach (var request in LumosSocial.localUser.friendRequests) {
						GUILayout.BeginHorizontal();
							GUILayout.Label(request.id);
							
							if (GUILayout.Button("Accept", GUILayout.Width(submitButtonWidth))) {
								LumosSocial.localUser.AcceptFriendRequest(request.id, delegate {
									// do something
								});
							}
				
							if (GUILayout.Button("Decline", GUILayout.Width(submitButtonWidth))) {
								LumosSocial.localUser.DeclineFriendRequest(request.id, delegate {
									// do something
								});
							}
						GUILayout.EndHorizontal();
					}
				}
		
				if (LumosSocial.localUser.friends != null) {
					foreach (var friend in LumosSocial.localUser.friends) {
						GUILayout.BeginHorizontal();
							GUILayout.Label(friend.id, GUILayout.Width(submitButtonWidth));
				
							if (GUILayout.Button("Remove", GUILayout.Width(submitButtonWidth))) {
								LumosSocial.localUser.RemoveFriend(friend.id, delegate {
								// do something
								});
							}
						GUILayout.EndHorizontal();
					}
				}
		
				GUILayout.EndScrollView();
			GUILayout.EndVertical();
		
			GUILayout.Space(smallMargin);
		
			// Scores
			GUILayout.BeginVertical();
				GUILayout.Label("High Scores");
		
				proScoresScrollPos = GUILayout.BeginScrollView(proScoresScrollPos);
				
				if (LumosSocial.localUser.scores != null) {
					foreach (var score in LumosSocial.localUser.scores) {
						GUILayout.Label(score.leaderboardID);
						GUILayout.BeginHorizontal();
							GUILayout.Label(score.value.ToString());
						GUILayout.EndHorizontal();
						GUILayout.Space(smallMargin);
					}
				}
		
				GUILayout.EndScrollView();
			GUILayout.EndVertical();
		
			GUILayout.Space(smallMargin);
		GUILayout.EndHorizontal();
	}
	
	public static void ShowProfileUI()
	{
		instance.screen = Screens.Profile;

		LumosSocial.localUser.LoadFriends(delegate {
			// do something
		});
		
		LumosSocial.localUser.LoadFriendRequests(delegate {
		 // do something	
		});
		
		LumosSocial.localUser.LoadFriendLeaderboardScores(delegate {
		 // do something	
		});
	}
}
