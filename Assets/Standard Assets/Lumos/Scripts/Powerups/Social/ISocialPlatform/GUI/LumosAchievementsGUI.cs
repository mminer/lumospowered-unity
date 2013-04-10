using UnityEngine;
using System.Collections;

public partial class LumosSocialGUI : MonoBehaviour {
	
	public Texture2D defaultAchievementIcon;
	
	int achievementCols = 2;
	Vector2 achievementScrollPos = new Vector2(0, 0);
	
	
	void AchievementsScreen()
	{
		GUILayout.Space(smallMargin);
		
		// Title
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Your Achievements");
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		achievementScrollPos = GUILayout.BeginScrollView(achievementScrollPos, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height));
		
		int column = 0;
		
		foreach (var achievement in LumosSocial.achievements) {
			
		}
		
		GUILayout.EndScrollView();
	}
}
