// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;

/// <summary>
/// User interface to display leaderboards.
/// </summary>
public static class LumosLeaderboardsGUI
{
	/// <summary>
	/// The current leaderboard.
	/// </summary>
	public static LumosLeaderboard currentLeaderboard { get; private set; }

	/// <summary>
	/// The getting leaderboards.
	/// </summary>
	static bool gettingLeaderboards;

	/// <summary>
	/// The offset.
	/// </summary>
	static int offset;

	/// <summary>
	/// Displays the leaderboards UI.
	/// </summary>
	/// <param name="windowRect">The bounding rect of the window.</param>
	public static void OnGUI (Rect windowRect)
	{
		if (LumosSocial.leaderboards.Count == 0) {
			if (!gettingLeaderboards) {
				LumosSocial.LoadLeaderboardDescriptions(null);
				gettingLeaderboards = true;
			}
		} else {
			gettingLeaderboards = false;
		}

		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical();

			if (LumosSocial.leaderboards.Count > 0) {
				foreach (var leaderboard in LumosSocial.leaderboards.Values) {
					if (leaderboard.loading) {
						GUILayout.Label("Loading...");
						GUI.enabled = false;
					}

					if (GUILayout.Button(leaderboard.title)) {
						currentLeaderboard = leaderboard;
						LumosSocialGUI.ShowWindow(LumosGUIWindow.Scores);

						if (currentLeaderboard.scores == null) {
							Social.LoadScores(currentLeaderboard.id, null);
						}
					}

					GUI.enabled = true;
				}
			}

			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}
}
