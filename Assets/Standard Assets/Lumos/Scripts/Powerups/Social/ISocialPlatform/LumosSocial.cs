using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using System.Collections;
using System.Collections.Generic;

public class LumosSocial
{
	
	public static List<LumosLeaderboard> leaderboards = new List<LumosLeaderboard>();
	public static List<LumosAchievement> achievements = new List<LumosAchievement>(); 
	public static IAchievementDescription[] achievementDescriptions; 
	private static List<LumosAchievement> processingAchievements = new List<LumosAchievement>();


	public static void Register(string username, string pass, string email, Action<bool> callback)
	{
		Social.Active = new LumosSocialPlatform();
		(Social.localUser as LumosUser).Register(username, pass, email, callback);
	}
	
	public static void Connect(string username=null, string password=null, Action<bool> callback=null)
	{
        // Authenticate and register a ProcessAuthentication callback
        // This call needs to be made before we can proceed to other calls in the Social API
		Social.Active = new LumosSocialPlatform();
		
		if (username != null) {
			(Social.localUser as LumosUser).Authenticate(username, password, callback);
			callback(true);
		} else {
			Social.localUser.Authenticate(ProcessAuthentication);	
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
		if (Social.localUser.authenticated) {
			var achievement = GetEarnedAchievement(achievementID);
			
			// Only award achievements that aren't already earned
			if (achievement == null) {
				processingAchievements.Add(achievement);
				Social.ReportProgress(achievementID, progress, AwardedAchievement);
			}
		}
	}

	public static void LoadAchievements()
	{
		Social.LoadAchievementDescriptions(ProcessLoadedAchievements);
		Social.LoadAchievements(ProcessLoadedPlayerAchievements);
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
			Social.LoadAchievements(ProcessLoadedPlayerAchievements);
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

	public static void LoadedScores(IScore[] scores) {
		
	}
	
	public static void SubmitScore(int score, string leaderboardID) {
		// Only count new high scores
		if (!Social.localUser.authenticated) {
			return;
		}
		
		Social.ReportScore(score, leaderboardID, SubmittedScore);
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
}
