using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {
	void Awake()
	{
		Lumos.debug = true;
		LumosEvents.Record("what_up", 0, true, "levels");
		LumosEvents.Record("event_test", Time.time, true, "loading");
	}
	
	void Start() {
		for (var i = 0; i < 100; i++) {
			Debug.Log("log test like a boss " + i);	
			Debug.LogError("Error test like a boss " + i);	
			Debug.LogWarning("Warning test like a boss " + i);
		}
	}

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
