using UnityEngine;
using UnityEditor;
using System.Collections;

public class Test : MonoBehaviour
{
	void Awake()
	{
		Lumos.debug = true;
		
		//LumosEvents.Record("what_up", 0, true, "levels");
		//LumosEvents.Record("event_test", Time.time, true, "loading");
		//LumosEvents.Record("event_test_11", 7, true, "loading");
	}

	void OnGUI()
	{
		
		/*if (GUILayout.Button("Delete prefs")) {
			EditorPrefs.DeleteKey("lumos-installed-packages");
			EditorPrefs.DeleteKey("lumos-latest-packages");
			EditorPrefs.SetBool("lumos-installing", false);
			EditorPrefs.DeleteKey("lumos-install-queue");
		}
		*/
	/*	if (GUILayout.Button("Show achievements")) {
			Social.ShowAchievementsUI();
		}

		if (GUILayout.Button("Show leaderboards")) {
			Social.ShowLeaderboardUI();
		}

		if (GUILayout.Button("Show Profile")) {
			LumosSocial.ShowProfileUI();
		}

		if (GUILayout.Button("Send Events")) {
			LumosEvents.Send();
		}
		*/
	}
}
