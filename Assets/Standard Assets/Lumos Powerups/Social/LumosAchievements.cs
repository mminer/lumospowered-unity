// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

/// <summary>
/// Lumos achievements.
/// </summary>
public partial class LumosSocial : ISocialPlatform
{
	static Dictionary<string, LumosAchievement> achievements;

	/// <summary>
	/// Achievement information.
	/// </summary>
	public static LumosAchievementDescription[] achievementDescriptions { get; private set; }

	/// <summary>
	/// The achievements that the user has earned or made progress on.
	/// </summary>
	public static ICollection<LumosAchievement> earnedAchievements {
		get {
			return achievements.Values;
		}
	}

	/// <summary>
	/// Creates an empty achievement object.
	/// </summary>
	/// <returns>A new achievement.</returns>
	public IAchievement CreateAchievement ()
	{
		var achievement = new LumosAchievement();
		return achievement;
	}

	/// <summary>
	/// Fetches the achievement descriptions from the server.
	/// </summary>
	/// <param name="callback">Callback.</param>
	public void LoadAchievementDescriptions (Action<IAchievementDescription[]> callback)
	{
		var endpoint = baseUrl + "/achievements?method=GET";

		LumosRequest.Send(endpoint,
			success => {
				var resp = success as IList;
				achievementDescriptions = new LumosAchievementDescription[resp.Count];

				for (int i = 0; i < resp.Count; i++) {
					achievementDescriptions[i] = new LumosAchievementDescription(resp[i] as Dictionary<string, object>);
				}

				if (callback != null) {
					callback(achievementDescriptions);
				}
			},
			error => {
				if (callback != null) {
					callback(null);
				}
			});
	}

	/// <summary>
	/// Loads the player's earned achievements.
	/// </summary>
	/// <param name="callback">Callback.</param>
	public void LoadAchievements (Action<IAchievement[]> callback)
	{
		var endpoint = baseUrl + "/users/" + localUser.id + "/achievements?method=GET";

		LumosRequest.Send(endpoint,
			success => {
				var resp = success as IList;

				foreach (Dictionary<string, object> info in resp) {
					var achievement = new LumosAchievement(info);
					achievements[achievement.id] = achievement;
				}

				if (callback != null) {
					var achievementsArray = new LumosAchievement[achievements.Count];
					achievements.Values.CopyTo(achievementsArray, 0);
					callback(achievementsArray);
				}
			},
			error => {
				if (callback != null) {
					callback(null);
				}
			});
	}

	/// <summary>
	/// Updates a player's progress for an achievement.
	/// </summary>
	/// <param name="achievementID">Achievement identifier.</param>
	/// <param name="percentCompleted">Percent completed (0 - 100).</param>
	/// <param name="callback">Callback.</param>
	public void ReportProgress (string achievementID, double percentCompleted, Action<bool> callback)
	{
		var achievement = GetAchievement(achievementID);

		if (achievement == null) {
			// Create new achievement.
			achievement = new LumosAchievement(achievementID, percentCompleted, false, DateTime.Now);
			achievements[achievement.id] = achievement;
		} else {
			// Update existing achievement.
			achievement.percentCompleted = percentCompleted;
		}

		achievement.ReportProgress(callback);
	}

	/// <summary>
	/// Shows the achievements UI.
	/// </summary>
	public void ShowAchievementsUI()
	{
		LumosSocialGUI.ShowAchievementsUI();
	}

	// Added functions:

	/// <summary>
	/// Awards an achievement.
	/// </summary>
	/// <param name="achievementID">The achievement identifier.</param>
	public static void AwardAchievement (string achievementID)
	{
		if (!Social.localUser.authenticated) {
			return;
		}

		var achievement = GetAchievement(achievementID);

		if (achievement == null) {
			// Create new achievement.
			achievement = new LumosAchievement(achievementID, 100, false, DateTime.Now);
			achievements[achievement.id] = achievement;
		} else {
			// Update existing achievement.
			achievement.percentCompleted = 100;
		}

		achievement.ReportProgress(null);
	}

	/// <summary>
	/// Gets an achievement by its ID.
	/// </summary>
	/// <param name="achievementID">The achievement identifier.</param>
	/// <returns>The achievement.</returns>
	public static LumosAchievement GetAchievement (string achievementID)
	{
		if (achievements.ContainsKey(achievementID)) {
			return achievements[achievementID];
		} else {
			return null;
		}
	}

	/// <summary>
	/// Determines whether the user has earned an achievement.
	/// </summary>
	/// <param name="achievementID">The achievement identifier.</param>
	/// <returns>True if the user has earned the achievement.</returns>
	public static bool HasAchievement (string achievementID)
	{
		return achievements.ContainsKey(achievementID) &&
		       achievements[achievementID].percentCompleted == 100;
	}
}
