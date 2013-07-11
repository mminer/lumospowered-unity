using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Lumos social.
/// </summary>
public class LumosSocial
{
	/// <summary>
	/// The local user.
	/// </summary>
	public static LumosUser localUser;

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
	/// <param name='username'>
	/// Username.
	/// </param>
	/// <param name='pass'>
	/// Pass.
	/// </param>
	/// <param name='email'>
	/// Email.
	/// </param>
	/// <param name='callback'>
	/// Callback.
	/// </param>
	public static void Register (string username, string pass, string email, Action<bool> callback)
	{
		Init();
		localUser.Register(username, pass, email, callback);
	}

	/// <summary>
	/// Connect the specified username, password and callback.
	/// </summary>
	/// <param name='username'>
	/// Username.
	/// </param>
	/// <param name='password'>
	/// Password.
	/// </param>
	/// <param name='callback'>
	/// Callback.
	/// </param>
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
	/// Forgots the password.
	/// </summary>
	/// <param name='username'>
	/// Username.
	/// </param>
	/// <param name='callback'>
	/// Callback.
	/// </param>
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
	/// <param name='achievementID'>
	/// Achievement ID.
	/// </param>
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
	/// <param name='leaderboard'>
	/// Leaderboard.
	/// </param>
	public static void LoadLeaderboardScores (LumosLeaderboard leaderboard)
	{
		platform.LoadScores(leaderboard.id, ProcessScores);
	}

	/// <summary>
	/// Processes the leaderboard descriptions.
	/// </summary>
	/// <param name='success'>
	/// Success.
	/// </param>
	static void ProcessLeaderboardDescriptions (bool success)
	{
		// do nothing
	}

	/// <summary>
	/// Processes the scores.
	/// </summary>
	/// <param name='scores'>
	/// Scores.
	/// </param>
	static void ProcessScores (IScore[] scores)
	{
		// do nothing
	}

	/// <summary>
	/// Shows the profile U.
	/// </summary>
	public static void ShowProfileUI ()
	{
		LumosSocialGUI.ShowProfileUI();
	}

    /// <summary>
    /// Processes the authentication.
    /// </summary>
    /// <param name='success'>
    /// Success.
    /// </param>
    static void ProcessAuthentication (bool success)
	{
        if (success) {
            Lumos.Log("Authenticated local user!");
        } else {
			Lumos.LogWarning("Failed to authenticate local user.");
		}
    }

    /// <summary>
    /// Processes the loaded achievements.
    /// </summary>
    /// <param name='achievements'>
    /// Achievements.
    /// </param>
    static void ProcessLoadedAchievements (IAchievementDescription[] achievements)
	{
        if (achievements.Length == 0) {
            Debug.Log ("Error: no achievements found");
			return;
		}

        Debug.Log ("Loaded " + achievements.Length + " achievements");

		achievementDescriptions = achievements;
    }

	// TODO: I think we lose our Lumos specific achievement data here.
	// Need to find a work-around
	/// <summary>
	/// Callback function after a player's achievements are loaded
	/// </summary>
	/// <param name='achievements'>
	/// Achievements.
	/// </param>
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
	/// <param name='success'>
	/// Success.
	/// </param>
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
	/// <returns>
	/// The earned achievement.
	/// </returns>
	/// <param name='achievementID'>
	/// Achievement I.
	/// </param>
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
	/// Loadeds the scores.
	/// </summary>
	/// <param name='scores'>
	/// Scores.
	/// </param>
	static void LoadedScores (IScore[] scores) {

	}

	/// <summary>
	/// Submits the score.
	/// </summary>
	/// <param name='score'>
	/// Score.
	/// </param>
	/// <param name='leaderboardID'>
	/// Leaderboard I.
	/// </param>
	public static void SubmitScore (int score, string leaderboardID) {
		platform.ReportScore(score, leaderboardID, SubmittedScore);
	}

	/// <summary>
	/// Submitteds the score.
	/// </summary>
	/// <param name='success'>
	/// Success.
	/// </param>
	static void SubmittedScore (bool success) {
		Debug.Log("score submitted: " + success);
	}

	/// <summary>
	/// Determines whether this instance has achievement the specified achievementID.
	/// </summary>
	/// <returns>
	/// <c>true</c> if this instance has achievement the specified achievementID; otherwise, <c>false</c>.
	/// </returns>
	/// <param name='achievementID'>
	/// If set to <c>true</c> achievement I.
	/// </param>
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
	/// <returns>
	/// The leaderboard.
	/// </returns>
	/// <param name='id'>
	/// Identifier.
	/// </param>
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
