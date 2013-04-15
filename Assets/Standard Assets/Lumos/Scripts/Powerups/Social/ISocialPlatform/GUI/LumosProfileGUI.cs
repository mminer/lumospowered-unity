using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms;

public partial class LumosSocialGUI : MonoBehaviour {

	
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
				if (GUILayout.Button("Achievements")) {
					screen = Screens.Achievements;
				}
		
				if (GUILayout.Button("Leaderboards")) {
					screen = Screens.Leaderboards;
				}
		
				if (GUILayout.Button("Settings")) {
					screen = Screens.Settings;
				}
			GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		
		GUILayout.Space(smallMargin);
		
		GUILayout.BeginHorizontal();
			// Other info
			GUILayout.BeginVertical();
				proOtherScrollPos = GUILayout.BeginScrollView(proOtherScrollPos);
				
				GUILayout.EndScrollView();
			GUILayout.EndVertical();
		
			GUILayout.Space(smallMargin);
			
			// Friend List
			GUILayout.BeginVertical();
				GUILayout.Label("Friends");
		
				proFriendsScrollPos = GUILayout.BeginScrollView(proFriendsScrollPos);
				
				GUILayout.BeginHorizontal();
					friendToAdd = GUILayout.TextField(friendToAdd, GUILayout.Width(margin));
					
					if (GUILayout.Button("Send Request", GUILayout.Width(largeMargin))) {
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
							
							if (GUILayout.Button("Accept")) {
								LumosSocial.localUser.AcceptFriendRequest(request.id, delegate {
									// do something
								});
							}
				
							if (GUILayout.Button("Decline")) {
								LumosSocial.localUser.DeclineFriendRequest(request.id, delegate {
									// do something
								});
							}
						GUILayout.EndHorizontal();
					}
				}
		
				if (LumosSocial.localUser.friends != null) {
					foreach (var friend in LumosSocial.localUser.friends) {
						GUILayout.Label(friend.id);
					}
				}
		
				GUILayout.EndScrollView();
			GUILayout.EndVertical();
		
			GUILayout.Space(smallMargin);
		
			// Scores
			GUILayout.BeginVertical();
				proScoresScrollPos = GUILayout.BeginScrollView(proScoresScrollPos);
				
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
	}
	
}
