using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {
	
	void OnGUI() {
		if (!Social.localUser.authenticated) {
			return;
		}
		
		
		if (GUILayout.Button("Show achievements")) {
			Social.ShowAchievementsUI();
		}
		
		if (GUILayout.Button("Show leaderboards")) {
			Social.ShowLeaderboardUI();
		}
		
		if (GUILayout.Button("Show Profile")) {
			LumosSocial.ShowProfileUI();
		}
	}
}
