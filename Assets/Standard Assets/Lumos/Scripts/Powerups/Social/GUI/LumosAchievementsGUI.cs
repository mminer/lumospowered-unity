using UnityEngine;
using System.Collections;

public partial class LumosSocialGUI : MonoBehaviour
{
	public Texture2D defaultAchievementIcon;

	bool gettingAchievements;
	Vector2 achievementScrollPos;

	void AchievementsScreen()
	{
		GUILayout.Space(smallMargin);

		// Title
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Your Achievements");
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.Space(smallMargin);

		// Load achievements if necessary
		if (LumosSocial.achievementDescriptions == null) {
			GUILayout.Label("Loading...");

			if (!gettingAchievements) {
				LumosSocial.LoadAchievements();
				gettingAchievements = true;
			}

			return;
		} else {
			gettingAchievements = false;
		}

		// Achievements
		achievementScrollPos = GUILayout.BeginScrollView(achievementScrollPos);

		int column = 0;

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		foreach (var achievement in LumosSocial.achievementDescriptions) {
			bool isLast = false;

			if (achievement.id == LumosSocial.achievementDescriptions[LumosSocial.achievementDescriptions.Length - 1].id) {
				isLast = true;
			}

			if (!LumosSocial.HasAchievement(achievement.id)) {
				GUI.enabled = false;
			}

			GUILayout.Label(achievement.image, GUILayout.Width(labelWidth), GUILayout.Height(labelWidth));

			GUILayout.BeginVertical();
				GUILayout.Label(achievement.title, GUILayout.Width(submitButtonWidth * 2));
				GUILayout.Label(achievement.unachievedDescription, GUILayout.Width(submitButtonWidth * 2), GUILayout.Height(largeMargin));

				GUI.enabled = true;

				if (!LumosSocial.HasAchievement(achievement.id) && GUILayout.Button("Award", GUILayout.Width(submitButtonWidth))) {
					LumosSocial.AwardAchievement(achievement.id, 100);
				}

			GUILayout.EndVertical();

			GUILayout.FlexibleSpace();

			if (column == 0) {
				column++;

				if (isLast) {
					GUILayout.EndHorizontal();
				}
			} else {
				column = 0;
				GUILayout.EndHorizontal();

				if (!isLast) {
					GUILayout.Space(smallMargin);
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
				}
			}
		}

		GUILayout.EndScrollView();
	}

	public static void ShowAchievements()
	{
		instance.screen = Screens.Achievements;
	}

	public static Texture2D GetDefaultAchievement()
	{
		return instance.defaultAchievementIcon;
	}
}
