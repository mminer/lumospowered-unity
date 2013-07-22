// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;

/// <summary>
/// Functions for fetching achievements.
/// </summary>
public partial class LumosSocial
{
	static Dictionary<string, LumosAchievement> _achievements = new Dictionary<string, LumosAchievement>();

	/// <summary>
	/// Achievement information.
	/// </summary>
	public static IAchievementDescription[] achievementDescriptions { get; private set; }

	/// <summary>
	/// The achievements that the user has earned or made progress on.
	/// </summary>
	public static IAchievement[] achievements
	{
		get {
			if (_achievements == null) {
				return null;
			} else {
				return _achievements.Values.ToArray();
			}
		}
	}

	/// <summary>
	/// Whether we're currently fetching the achievement descriptions.
	/// </summary>
	public static bool loadingAchievementDescriptions { get; private set; }

	/// <summary>
	/// Whetehr we're currently fetching the achievements.
	/// </summary>
	public static bool loadingAchievements { get; private set; }

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
	/// Fetches the achievement descriptions.
	/// </summary>
	/// <param name="callback">Callback.</param>
	public void LoadAchievementDescriptions (Action<IAchievementDescription[]> callback)
	{
		if (achievementDescriptions == null && !loadingAchievementDescriptions) {
			// Load the achievement descriptions from the server.
			loadingAchievementDescriptions = true;
			var endpoint = baseUrl + "/achievements?method=GET";

			LumosRequest.Send(endpoint,
				success => {
					var resp = success as IList;
					achievementDescriptions = new LumosAchievementDescription[resp.Count];

					for (int i = 0; i < resp.Count; i++) {
						achievementDescriptions[i] = new LumosAchievementDescription(resp[i] as Dictionary<string, object>);
					}

					loadingAchievementDescriptions = false;

					if (callback != null) {
						callback(achievementDescriptions);
					}
				},
				error => {
					loadingAchievementDescriptions = false;

					if (callback != null) {
						callback(null);
					}
				});
		} else {
			// Use the cached achievement descriptions.
			callback(achievementDescriptions);
		}
	}

	/// <summary>
	/// Loads the player's earned achievements.
	/// </summary>
	/// <param name="callback">Callback.</param>
	public void LoadAchievements (Action<IAchievement[]> callback)
	{
		if (achievements == null && !loadingAchievements) {
			// Load the achievements from the server.
			loadingAchievements = true;
			var endpoint = baseUrl + "/users/" + localUser.id + "/achievements?method=GET";

			LumosRequest.Send(endpoint,
				success => {
					var resp = success as IList;
					_achievements = new Dictionary<string, LumosAchievement>();

					foreach (Dictionary<string, object> info in resp) {
						var achievement = new LumosAchievement(info);
						_achievements[achievement.id] = achievement;
					}

					loadingAchievements = false;

					if (callback != null) {
						callback(achievements);
					}
				},
				error => {
					loadingAchievements = false;

					if (callback != null) {
						callback(null);
					}
				});
		} else {
			// Use the cached achievements.
			if (callback != null) {
				callback(achievements);
			}
		}
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
			_achievements[achievement.id] = achievement;
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
		LumosSocialGUI.ShowWindow(LumosGUIWindow.Achievements);
	}

	#region Added Functions

	/// <summary>
	/// Awards an achievement.
	/// </summary>
	/// <param name="achievementID">The achievement identifier.</param>
	/// <param name="callback">Callback.</param>
	public static void AwardAchievement (string achievementID, Action<bool> callback)
	{
		Social.ReportProgress(achievementID, 100, callback);
	}

	/// <summary>
	/// Gets an achievement by its ID.
	/// </summary>
	/// <param name="achievementID">The achievement identifier.</param>
	/// <returns>The achievement.</returns>
	public static LumosAchievement GetAchievement (string achievementID)
	{
		if (_achievements != null && _achievements.ContainsKey(achievementID)) {
			return _achievements[achievementID];
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
		return _achievements != null &&
		       _achievements.ContainsKey(achievementID) &&
		       _achievements[achievementID].completed;
	}

	#endregion
}
