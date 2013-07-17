using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;

/// <summary>
/// Lumos achievements GUI.
/// </summary>
public partial class LumosSocialGUI : MonoBehaviour
{
	/// <summary>
	/// The default achievement icon.
	/// </summary>
	public Texture2D defaultAchievementIcon;

	/// <summary>
	/// Whether we're currently fetching achievement information.
	/// </summary>
	bool gettingAchievements;

	/// <summary>
	/// The window scroll position.
	/// </summary>
	Vector2 achievementsScrollPos;

	IAchievementDescription[] achievementDescriptions;

	/// <summary>
	/// Displays the achievements pane.
	/// </summary>
	void AchievementsScreen()
	{
		GUILayout.Space(smallMargin);

		// Title.
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Your Achievements");
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.Space(smallMargin);

		// Load achievements if necessary.
		if (achievementDescriptions == null) {
			GUILayout.Label("Loading...");

			if (!gettingAchievements) {
				Social.Active.LoadAchievementDescriptions(descriptions => {
					achievementDescriptions = descriptions;
				});
				Social.Active.LoadAchievements(null);
				gettingAchievements = true;
			}

			return;
		} else {
			gettingAchievements = false;
		}

		// Achievements
		achievementsScrollPos = GUILayout.BeginScrollView(achievementsScrollPos);

		int column = 0;

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		for (int i = 0; i < achievementDescriptions.Length; i++) {
			var description = achievementDescriptions[i];

			if (!LumosSocial.HasAchievement(description.id)) {
				GUI.enabled = false;
			}

			GUILayout.Label(description.image, GUILayout.Width(labelWidth), GUILayout.Height(labelWidth));

			GUILayout.BeginVertical();
				GUILayout.Label(description.title, GUILayout.Width(submitButtonWidth * 2));
				GUILayout.Label(description.unachievedDescription, GUILayout.Width(submitButtonWidth * 2), GUILayout.Height(largeMargin));

				GUI.enabled = true;

				if (!LumosSocial.HasAchievement(description.id) && GUILayout.Button("Award", GUILayout.Width(submitButtonWidth))) {
					Social.ReportProgress(description.id, 100, null);
				}

			GUILayout.EndVertical();

			GUILayout.FlexibleSpace();

			if (column == 0) {
				column++;

				if (i == LumosSocial.achievementDescriptions.Length - 1) { // Last
					GUILayout.EndHorizontal();
				}
			} else {
				column = 0;
				GUILayout.EndHorizontal();

				if (i < LumosSocial.achievementDescriptions.Length - 1) { // Not last
					GUILayout.Space(smallMargin);
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
				}
			}
		}

		GUILayout.EndScrollView();
	}

	/// <summary>
	/// Shows the achievements.
	/// </summary>
	public static void ShowAchievementsUI ()
	{
		instance.screen = Screens.Achievements;
	}
}
