// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

//using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;

/// <summary>
/// User interface for displaying achievements.
/// </summary>
public static class LumosAchievementsGUI
{
	/// <summary>
	/// Whether we're currently fetching achievement information.
	/// </summary>
	static bool gettingAchievements;

	/// <summary>
	/// The window scroll position.
	/// </summary>
	static Vector2 scrollPos;

	/// <summary>
	/// Descriptions of the available achievements.
	/// </summary>
	static IAchievementDescription[] achievementDescriptions;

	/// <summary>
	/// Displays the achievements UI.
	/// </summary>
	/// <param name="windowRect">The bounding rect of the window.</param>
	public static void OnGUI (Rect windowRect)
	{
		// Load achievements if necessary.
		if (achievementDescriptions == null) {
			GUILayout.Label("Loading...");

			if (!gettingAchievements) {
				Social.LoadAchievementDescriptions(descriptions => {
					achievementDescriptions = descriptions;
				});
				Social.LoadAchievements(null);
				gettingAchievements = true;
			}

			return;
		} else {
			gettingAchievements = false;
		}

		// Achievements
		scrollPos = GUILayout.BeginScrollView(scrollPos);

		int column = 0;

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		for (int i = 0; i < achievementDescriptions.Length; i++) {
			var description = achievementDescriptions[i];

			if (!LumosSocial.HasAchievement(description.id)) {
				GUI.enabled = false;
			}

			GUILayout.Label(description.image);

			GUILayout.BeginVertical();
				GUILayout.Label(description.title, GUILayout.ExpandWidth(false));
				GUILayout.Label(description.unachievedDescription, GUILayout.ExpandWidth(false));

				GUI.enabled = true;

				if (!LumosSocial.HasAchievement(description.id) && GUILayout.Button("Award", GUILayout.ExpandWidth(false))) {
					Social.ReportProgress(description.id, 100, null);
				}

			GUILayout.EndVertical();

			GUILayout.FlexibleSpace();

			if (column == 0) {
				column++;

				if (i == achievementDescriptions.Length - 1) { // Last
					GUILayout.EndHorizontal();
				}
			} else {
				column = 0;
				GUILayout.EndHorizontal();

				if (i < achievementDescriptions.Length - 1) { // Not last
					LumosSocialGUI.DrawDivider();
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
				}
			}
		}

		GUILayout.EndScrollView();
	}
}
