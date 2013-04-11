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
			
			if (LumosSocial.achievements.Count < 1) {
				if (GUILayout.Button("Award Achievement")) {
					//Social.ReportProgress("unity-client-test", 100
					LumosSocial.AwardAchievement("unity-client-test", 100);
				}
			}
		}
	}
}
