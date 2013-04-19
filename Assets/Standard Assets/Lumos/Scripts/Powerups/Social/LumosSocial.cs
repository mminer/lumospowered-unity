using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using System.Collections;
using System.Collections.Generic;

public class LumosSocial
{
	public static LumosUser localUser;
	public static List<LumosLeaderboard> leaderboards;
	public static List<LumosAchievement> achievements = new List<LumosAchievement>();
	public static IAchievementDescription[] achievementDescriptions;
	static List<LumosAchievement> processingAchievements = new List<LumosAchievement>();
	static LumosSocialPlatform platform;

	static void Init()
	{
		platform = new LumosSocialPlatform();
		Social.Active = platform;
		localUser = Social.localUser as LumosUser;
	}

	public static void Register(string username, string pass, string email, Action<bool> callback)
	{
		Init();
		localUser.Register(username, pass, email, callback);
	}

	public static void Connect(string username=null, string password=null, Action<bool> callback=null)
	{
        // This call needs to be made before we can proceed to other calls in the Social API
		Init();

		if (username != null) {
			localUser.Authenticate(username, password, callback);
			callback(true);
		} else {
			localUser.Authenticate(ProcessAuthentication);
		}
    }

	/// <summary>
	/// Awards an achievement.
	/// </summary>
	/// <param name='achievementID'>
	/// Achievement ID.
	/// </param>
	public static void AwardAchievement(string achievementID, int progress=100)
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

	public static void LoadAchievements()
	{
		platform.LoadAchievementDescriptions(ProcessLoadedAchievements);
		platform.LoadAchievements(ProcessLoadedPlayerAchievements);
	}

	public static void LoadLeaderboards()
	{
		platform.LoadLeaderboardDescriptions(ProcessLeaderboardDescriptions);
	}

	public static void LoadLeaderboardScores(LumosLeaderboard leaderboard)
	{
		platform.LoadScores(leaderboard.id, ProcessScores);
	}

	static void ProcessLeaderboardDescriptions(bool success)
	{
		// do nothing
	}

	static void ProcessScores(IScore[] scores)
	{
		// do nothing
	}

	public static void ShowProfileUI()
	{
		LumosSocialGUI.ShowProfileUI();
	}

    // This function gets called when Authenticate completes
    // Note that if the operation is successful, Social.localUser will contain data from the server.
    static void ProcessAuthentication (bool success)
	{
        if (success) {
            Lumos.Log("Authenticated local user!");
        } else {
			Lumos.LogWarning("Failed to authenticate local user.");
		}
    }

    // This function gets called when the LoadAchievement call completes
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
	static void AwardedAchievement(bool success)
	{
		if (success) {
			// Reload the player's earned achievements
			platform.LoadAchievements(ProcessLoadedPlayerAchievements);
		}
	}

	// Check the pending awarded achievements to see if they
	// were awarded to the player
	static void ValidateAchievementsInProcess()
	{
		foreach (var achievement in processingAchievements) {
			if (achievements.Contains(achievement)) {
				processingAchievements.Remove(achievement);
				Debug.Log("Earned Achievement " + achievement.id);
			}
		}
	}

	// Get an earned achievement by ID
	static LumosAchievement GetEarnedAchievement(string achievementID)
	{
		foreach (var achievement in achievements) {
			if (achievement.id == achievementID) {
				return achievement;
			}
		}

		return null;
	}

	static void LoadedScores(IScore[] scores) {

	}

	public static void SubmitScore(int score, string leaderboardID) {
		platform.ReportScore(score, leaderboardID, SubmittedScore);
	}

	static void SubmittedScore(bool success) {
		Debug.Log("score submitted: " + success);
	}

	public static bool HasAchievement(string achievementID)
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

	public static LumosLeaderboard GetLeaderboard(string id)
	{
		foreach (var leaderboard in leaderboards) {
			if (leaderboard.id == id) {
				return leaderboard;
			}
		}

		return null;
	}
}
