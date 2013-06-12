// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

/// <summary>
/// Lumos social platform.
/// </summary>
public partial class LumosSocialPlatform : ISocialPlatform
{
	/// <summary>
	/// The URL.
	/// </summary>
	string url = "localhost:8888/api/1/";
	/// <summary>
	/// The _local user.
	/// </summary>
	LumosUser _localUser = new LumosUser();
	
	/// <summary>
	/// Gets or sets the local user.
	/// </summary>
	/// <value>
	/// The local user.
	/// </value>
	public ILocalUser localUser {
		get { return _localUser; }
		set { _localUser = value as LumosUser; }
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="LumosSocialPlatform"/> class.
	/// </summary>
	public LumosSocialPlatform()
	{
		Social.Active = this;
	}

	#region Users
	
	/// <summary>
	/// Authenticate the specified user and callback.
	/// </summary>
	/// <param name='user'>
	/// User.
	/// </param>
	/// <param name='callback'>
	/// Callback.
	/// </param>
	public void Authenticate(ILocalUser user, Action<bool> callback) 
	{
		user.Authenticate(callback);
	}
	
	/// <summary>
	/// Loads the friends.
	/// </summary>
	/// <param name='user'>
	/// User.
	/// </param>
	/// <param name='callback'>
	/// Callback.
	/// </param>
	public void LoadFriends(ILocalUser user, Action<bool> callback) 
	{
		user.LoadFriends(callback);
	}
	
	/// <summary>
	/// Loads the users.
	/// </summary>
	/// <param name='userIds'>
	/// User identifiers.
	/// </param>
	/// <param name='callback'>
	/// Callback.
	/// </param>
	public void LoadUsers(string[] userIds, Action<IUserProfile[]> callback)
	{
		FetchUsers(userIds, callback);
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
	public void ForgotPassword(string username, Action<bool> callback)
	{
		var endpoint = url + "users/" + username + "/password";

		LumosRequest.Send(endpoint, delegate {
			callback(true);
		});
	}

	#endregion

	#region Achievements
	
	/// <summary>
	/// Loads the achievement descriptions.
	/// </summary>
	/// <param name='callback'>
	/// Callback.
	/// </param>
	public void LoadAchievementDescriptions(Action<IAchievementDescription[]> callback) 
	{
		FetchGameAchievements(callback);
	}
	
	/// <summary>
	/// Loads the achievements.
	/// </summary>
	/// <param name='callback'>
	/// Callback.
	/// </param>
	public void LoadAchievements(Action<IAchievement[]> callback) 
	{
		FetchPlayerAchievements(callback);
	}
	
	/// <summary>
	/// Reports the progress.
	/// </summary>
	/// <param name='achievementId'>
	/// Achievement identifier.
	/// </param>
	/// <param name='percentCompleted'>
	/// Percent completed.
	/// </param>
	/// <param name='callback'>
	/// Callback.
	/// </param>
	public void ReportProgress(string achievementId, double percentCompleted, Action<bool> callback) 
	{
		UpdateAchievementProgress(achievementId, (int)percentCompleted, callback);
	}
	
	/// <summary>
	/// Shows the achievements U.
	/// </summary>
	public void ShowAchievementsUI() 
	{
		LumosSocialGUI.ShowAchievements();
	}
	
	/// <summary>
	/// Creates the achievement.
	/// </summary>
	/// <returns>
	/// The achievement.
	/// </returns>
	public IAchievement CreateAchievement() 
	{
		Lumos.LogError("Lumos does not support creating achievements on the fly.");
		return null;
	}

	#endregion

	#region Leaderboards
	
	/// <summary>
	/// Reports the score.
	/// </summary>
	/// <param name='score'>
	/// Score.
	/// </param>
	/// <param name='leaderboardId'>
	/// Leaderboard identifier.
	/// </param>
	/// <param name='callback'>
	/// Callback.
	/// </param>
	public void ReportScore(System.Int64 score, string leaderboardId, Action<bool> callback) 
	{
		RecordHighScore((int)score, leaderboardId, callback);
	}
	
	/// <summary>
	/// Loads the scores.
	/// </summary>
	/// <param name='leaderboard'>
	/// Leaderboard.
	/// </param>
	/// <param name='callback'>
	/// Callback.
	/// </param>
	public void LoadScores(ILeaderboard leaderboard, Action<bool> callback)
	{
		leaderboard.LoadScores(callback);
	}
	
	/// <summary>
	/// Loads the scores.
	/// </summary>
	/// <param name='leaderboardID'>
	/// Leaderboard I.
	/// </param>
	/// <param name='callback'>
	/// Callback.
	/// </param>
	public void LoadScores(string leaderboardID, Action<IScore[]> callback)
	{
		var leaderboard = LumosSocial.GetLeaderboard(leaderboardID);
		leaderboard.LoadScores(delegate {
			callback(leaderboard.scores);
		});
	}
	
	/// <summary>
	/// Loads the leaderboard descriptions.
	/// </summary>
	/// <param name='callback'>
	/// Callback.
	/// </param>
	public void LoadLeaderboardDescriptions(Action<bool> callback)
	{
		FetchLeaderboardDescriptions(callback);
	}
	
	/// <summary>
	/// Shows the leaderboard U.
	/// </summary>
	public void ShowLeaderboardUI() 
	{
		LumosSocialGUI.ShowLeaderboardsUI();
	}
	
	/// <summary>
	/// Creates the leaderboard.
	/// </summary>
	/// <returns>
	/// The leaderboard.
	/// </returns>
	public ILeaderboard CreateLeaderboard()
	{
		Lumos.LogError("Lumos does not support creating leaderboards on the fly.");
		return null;
	}

	#endregion

	#region Other
	
	/// <summary>
	/// Gets the loading.
	/// </summary>
	/// <returns>
	/// The loading.
	/// </returns>
	/// <param name='leaderboard'>
	/// If set to <c>true</c> leaderboard.
	/// </param>
	public bool GetLoading(ILeaderboard leaderboard) 
	{
		// Not sure what this is supposed to do.
		return false;
	}
	
	#endregion
}
