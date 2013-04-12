using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {
	
	void OnGUI() {
		if (!Social.localUser.authenticated) {
			return;
		}
		
		if (LumosSocial.achievementDescriptions == null) {
			if (GUILayout.Button("Get achievements")) {
				LumosSocial.LoadAchievements();
			}
		} else {
			if (GUILayout.Button("Show achievements")) {
				Social.ShowAchievementsUI();
			}
		}
		
		if (LumosSocial.leaderboards == null) {
			if (GUILayout.Button("Get leaderboards")) {
				LumosSocial.LoadLeaderboards();
			}
		} else {
			if (GUILayout.Button("Show leaderboards")) {
				Social.ShowLeaderboardUI();
			}
		}
	}
}
