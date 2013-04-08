using UnityEngine;
using UnityEngine.SocialPlatforms;
using System.Collections;
using System.Collections.Generic;

public class LumosSocial 
{
	
	public List<IAchievement> achievements = new List<IAchievement>(); 
	public IAchievementDescription[] descriptions; 
	private List<IAchievement> processingAchievements = new List<IAchievement>();


	public void Connect()
	{
        // Authenticate and register a ProcessAuthentication callback
        // This call needs to be made before we can proceed to other calls in the Social API
		Social.Active = new LumosSocialPlatform();
        Social.localUser.Authenticate(ProcessAuthentication);
    }
	
	/// <summary>
	/// Awards an achievement.
	/// </summary>
	/// <param name='achievementID'>
	/// Achievement ID.
	/// </param>
	public void AwardAchievement(string achievementID, int progress=100)
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

	public void LoadAchievements()
	{
		Social.LoadAchievementDescriptions(ProcessLoadedAchievements);
		Social.LoadAchievements(ProcessLoadedPlayerAchievements);
	}

    // This function gets called when Authenticate completes
    // Note that if the operation is successful, Social.localUser will contain data from the server. 
    void ProcessAuthentication (bool success) 
	{
        if (success) {
            Lumos.Log("Authenticated local user!");
        } else {
			Lumos.LogWarning("Failed to authenticate local user.");
		}
    }

    // This function gets called when the LoadAchievement call completes
    void ProcessLoadedAchievements (IAchievementDescription[] achievements)
	{
        if (achievements.Length == 0) {
            Debug.Log ("Error: no achievements found");
			return;
		}
		
        Debug.Log ("Loaded " + achievements.Length + " achievements");
		
		descriptions = achievements;
    }
	
	/// <summary>
	/// Callback function after a player's achievements are loaded
	/// </summary>
	/// <param name='achievements'>
	/// Achievements.
	/// </param>
	void ProcessLoadedPlayerAchievements (IAchievement[] achievements)
	{
		foreach (var achievement in achievements) {
			this.achievements.Add(achievement);
		}
		
		ValidateAchievementsInProcess();
	}
	
	/// <summary>
	/// Callback function after an achievement was awarded.
	/// </summary>
	/// <param name='success'>
	/// Success.
	/// </param>
	void AwardedAchievement(bool success)
	{
		if (success) {
			// Reload the player's earned achievements
			Social.LoadAchievements(ProcessLoadedPlayerAchievements);
		}
	}
	
	// Check the pending awarded achievements to see if they 
	// were awarded to the player
	void ValidateAchievementsInProcess()
	{
		foreach (var achievement in processingAchievements) {
			if (achievements.Contains(achievement)) {
				processingAchievements.Remove(achievement);
				Debug.Log("Earned Achievement " + achievement.id);
			}
		}
	}
	
	// Get an earned achievement by ID
	IAchievement GetEarnedAchievement(string achievementID)
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
	
	public void SubmitScore(int score, string leaderboardID) {
		// Only count new high scores
		if (!Social.localUser.authenticated) {
			return;
		}
		
		Social.ReportScore(score, leaderboardID, SubmittedScore);
	}
	
	public void SubmittedScore(bool success) {
		Debug.Log("score submitted: " + success);		
	}
}
