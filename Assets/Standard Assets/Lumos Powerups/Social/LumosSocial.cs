// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

/// <summary>
/// Lumos social.
/// </summary>
public class LumosSocial
{
	/// <summary>
	/// The local user.
	/// </summary>
	public static LumosUser localUser { get; set; }

	/// <summary>
	/// The leaderboards.
	/// </summary>
	public static List<LumosLeaderboard> leaderboards = new List<LumosLeaderboard>();

	/// <summary>
	/// The achievements.
	/// </summary>
	public static List<LumosAchievement> achievements = new List<LumosAchievement>();

	/// <summary>
	/// The achievement descriptions.
	/// </summary>
	public static IAchievementDescription[] achievementDescriptions;

	/// <summary>
	/// The processing achievements.
	/// </summary>
	static List<LumosAchievement> processingAchievements = new List<LumosAchievement>();

	/// <summary>
	/// The platform.
	/// </summary>
	static LumosSocialPlatform platform;

	static string _baseUrl = "https://social.lumospowered.com/api/1";

	/// <summary>
	/// The API's host domain.
	/// </summary>
	public static string baseUrl {
		get { return _baseUrl; }
		set { _baseUrl = value; }
	}

	/// <summary>
	/// Init this instance.
	/// </summary>
	static void Init ()
	{
		platform = new LumosSocialPlatform();
		Social.Active = platform;
		localUser = Social.localUser as LumosUser;
	}

	/// <summary>
	/// Register the specified username, pass, email and callback.
	/// </summary>
	/// <param name="username">Username.</param>
	/// <param name="password">Password.</param>
	/// <param name="email">The user's email address.</param>
	/// <param name="callback">
	/// Callback.
	/// </param>
	public static void Register (string username, string password, string email, Action<bool> callback)
	{
		Init();
		localUser.Register(username, password, email, callback);
	}

	/// <summary>
	/// Connect the specified user.
	/// </summary>
	/// <param name="username">Username.</param>
	/// <param name="password">Password.</param>
	/// <param name="callback">Callback triggers on success.</param>
	public static void Connect (string username=null, string password=null, Action<bool> callback=null)
	{
        // This call needs to be made before we can proceed to other calls in the Social API
		Init();

		if (username != null) {
			localUser.Authenticate (username, password, callback);
		} else {
			localUser.Authenticate(ProcessAuthentication);
		}
    }

	/// <summary>
	/// Sends a forgot password request.
	/// </summary>
	/// <param name="username">Username.</param>
	/// <param name="callback">Callback triggers on success.</param>
	public static void ForgotPassword (string username, Action<bool> callback)
	{
		if (platform == null) {
			Init();
		}

		platform.ForgotPassword (username, callback);
	}

	/// <summary>
	/// Awards an achievement.
	/// </summary>
	/// <param name="achievementID">The achievement identifier.</param>
	public static void AwardAchievement (string achievementID, int progress=100)
	{
		if (localUser.authenticated) {
			var achievement = GetEarnedAchievement(achievementID);

			// Only award achievements that aren't already earned
			if (achievement == null) {
				processingAchievements.Add(achievement);
				platform.ReportProgress(achievementID, progress, AwardedAchievement);
			}
		}
	}

	/// <summary>
	/// Loads the achievements.
	/// </summary>
	public static void LoadAchievements ()
	{
		platform.LoadAchievementDescriptions(ProcessLoadedAchievements);
		platform.LoadAchievements(ProcessLoadedPlayerAchievements);
	}

	/// <summary>
	/// Loads the leaderboards.
	/// </summary>
	public static void LoadLeaderboards ()
	{
		platform.LoadLeaderboardDescriptions (ProcessLeaderboardDescriptions);
	}

	/// <summary>
	/// Loads the leaderboard scores.
	/// </summary>
	/// <param name="leaderboard">The leaderboard.</param>
	public static void LoadLeaderboardScores (LumosLeaderboard leaderboard)
	{
		platform.LoadScores(leaderboard.id, ProcessScores);
	}

	/// <summary>
	/// Processes the leaderboard descriptions.
	/// </summary>
	/// <param name="success">Success.</param>
	static void ProcessLeaderboardDescriptions (bool success)
	{
		// do nothing
	}

	/// <summary>
	/// Processes the scores.
	/// </summary>
	/// <param name="scores">Scores.</param>
	static void ProcessScores (IScore[] scores)
	{
		// do nothing
	}

	/// <summary>
	/// Shows the profile interface.
	/// </summary>
	public static void ShowProfileUI ()
	{
		LumosSocialGUI.ShowProfileUI();
	}

    /// <summary>
    /// Processes the authentication.
    /// </summary>
    /// <param name="success">Success.</param>
    static void ProcessAuthentication (bool success)
	{
        if (success) {
            Lumos.Log("Authenticated local user.");
        } else {
			Lumos.LogWarning("Failed to authenticate local user.");
		}
    }

    /// <summary>
    /// Processes the loaded achievements.
    /// </summary>
    /// <param name="achievements">Achievements.</param>
    static void ProcessLoadedAchievements (IAchievementDescription[] achievements)
	{
        if (achievements.Length == 0) {
			return;
		}

		achievementDescriptions = achievements;
    }

	// TODO: I think we lose our Lumos specific achievement data here.
	// Need to find a work-around
	/// <summary>
	/// Callback function after a player's achievements are loaded
	/// </summary>
	/// <param name="achievements">Achievements.</param>
	static void ProcessLoadedPlayerAchievements (IAchievement[] achievements)
	{
		foreach (var achievement in achievements) {
			LumosSocial.achievements.Add(achievement as LumosAchievement);
		}

		ValidateAchievementsInProcess();
	}

	/// <summary>
	/// Callback function after an achievement was awarded.
	/// </summary>
	/// <param name="success">Success.</param>
	static void AwardedAchievement (bool success)
	{
		if (success) {
			// Reload the player's earned achievements
			platform.LoadAchievements(ProcessLoadedPlayerAchievements);
		}
	}

	/// <summary>
	/// Check the pending awarded achievements to see if they were awarded to the player.
	/// </summary>
	static void ValidateAchievementsInProcess ()
	{
		foreach (var achievement in processingAchievements) {
			if (achievements.Contains(achievement)) {
				processingAchievements.Remove(achievement);
				Debug.Log("Earned Achievement " + achievement.id);
			}
		}
	}

	/// <summary>
	/// Get an earned achievement by ID.
	/// </summary>
	/// <param name="achievementID">The achievement identifier.</param>
	/// <returns>The earned achievement.</returns>
	static LumosAchievement GetEarnedAchievement (string achievementID)
	{
		foreach (var achievement in achievements) {
			if (achievement.id == achievementID) {
				return achievement;
			}
		}

		return null;
	}

	/// <summary>
	/// Submits the score.
	/// </summary>
	/// <param name="score"></param>
	/// <param name="leaderboardID">The leaderboard identifier.</param>
	public static void SubmitScore (int score, string leaderboardID) {
		platform.ReportScore(score, leaderboardID, SubmittedScore);
	}

	/// <summary>
	/// Submitteds the score.
	/// </summary>
	/// <param name="success">Success.</param>
	static void SubmittedScore (bool success) {
		Debug.Log("score submitted: " + success);
	}

	/// <summary>
	/// Determines whether this instance has achievement the specified achievementID.
	/// </summary>
	/// <param name="achievementID">The achievement identifier.</param>
	/// <returns>True if the user has earner the achievement.</returns>
	public static bool HasAchievement (string achievementID)
	{
		foreach (var achievement in achievements) {
			if (achievement.id == achievementID) {
				if (achievement.percentCompleted == 100) {
					return true;
				}

				return false;
			}
		}

		return false;
	}

	/// <summary>
	/// Gets the leaderboard.
	/// </summary>
	/// <param name="id">The leaderboard identifier.</param>
	/// <returns>The leaderboard.</returns>
	public static LumosLeaderboard GetLeaderboard (string id)
	{
		foreach (var leaderboard in leaderboards) {
			if (leaderboard.id == id) {
				return leaderboard;
			}
		}

		return null;
	}
}
