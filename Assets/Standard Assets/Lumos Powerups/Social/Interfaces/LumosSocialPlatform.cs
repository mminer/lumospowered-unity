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
	LumosUser _localUser = new LumosUser();

	/// <summary>
	/// The local user.
	/// </summary>
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
	/// <param name="user">User.</param>
	/// <param name="callback">Callback.</param>
	public void Authenticate(ILocalUser user, Action<bool> callback)
	{
		user.Authenticate(callback);
	}

	/// <summary>
	/// Loads the friends.
	/// </summary>
	/// <param name="user">User.</param>
	/// <param name="callback">Callback.</param>
	public void LoadFriends(ILocalUser user, Action<bool> callback)
	{
		user.LoadFriends(callback);
	}

	/// <summary>
	/// Loads the users.
	/// </summary>
	/// <param name="userIds">User identifiers.</param>
	/// <param name="callback">Callback.</param>
	public void LoadUsers(string[] userIds, Action<IUserProfile[]> callback)
	{
		FetchUsers(userIds, callback);
	}

	/// <summary>
	/// Forgots the password.
	/// </summary>
	/// <param name="username">Username.</param>
	/// <param name="callback">Callback.</param>
	public void ForgotPassword(string username, Action<bool> callback)
	{
		var endpoint = LumosSocial.baseUrl + "/users/" + username + "/password";

		LumosRequest.Send(endpoint,
			success => {
				callback(true);
			});
	}

	#endregion

	#region Achievements

	/// <summary>
	/// Loads the achievement descriptions.
	/// </summary>
	/// <param name="callback">Callback.</param>
	public void LoadAchievementDescriptions(Action<IAchievementDescription[]> callback)
	{
		FetchGameAchievements(callback);
	}

	/// <summary>
	/// Loads the achievements.
	/// </summary>
	/// <param name="callback">Callback.</param>
	public void LoadAchievements(Action<IAchievement[]> callback)
	{
		FetchPlayerAchievements(callback);
	}

	/// <summary>
	/// Reports the progress.
	/// </summary>
	/// <param name="achievementID">Achievement identifier.</param>
	/// <param name="percentCompleted">Percent completed.</param>
	/// <param name="callback">Callback.</param>
	public void ReportProgress(string achievementID, double percentCompleted, Action<bool> callback)
	{
		UpdateAchievementProgress(achievementID, (int)percentCompleted, callback);
	}

	/// <summary>
	/// Shows the achievements UI.
	/// </summary>
	public void ShowAchievementsUI()
	{
		LumosSocialGUI.ShowAchievements();
	}

	/// <summary>
	/// Creates the achievement.
	/// </summary>
	/// <returns>The achievement.</returns>
	public IAchievement CreateAchievement()
	{
		Lumos.LogError("Lumos does not support creating achievements on the fly.");
		return null;
	}

	#endregion

	#region Leaderboards

	/// <summary>
	/// Loads the scores.
	/// </summary>
	/// <param name="leaderboard">Leaderboard.</param>
	/// <param name="callback">Callback.</param>
	public void LoadScores(ILeaderboard leaderboard, Action<bool> callback)
	{
		leaderboard.LoadScores(callback);
	}

	/// <summary>
	/// Loads the scores.
	/// </summary>
	/// <param name="leaderboardID">The leaderboard identifier.</param>
	/// <param name="callback">Callback.</param>
	public void LoadScores(string leaderboardID, Action<IScore[]> callback)
	{
		var leaderboard = LumosSocial.GetLeaderboard(leaderboardID);
		leaderboard.LoadScores(delegate {
			callback(leaderboard.scores);
		});
	}

	/// <summary>
	/// Shows the leaderboard UI.
	/// </summary>
	public void ShowLeaderboardUI()
	{
		LumosSocialGUI.ShowLeaderboardsUI();
	}

	/// <summary>
	/// Creates the leaderboard.
	/// </summary>
	/// <returns>The leaderboard.</returns>
	public ILeaderboard CreateLeaderboard()
	{
		Lumos.LogError("Lumos does not support creating leaderboards on the fly.");
		return null;
	}

	#endregion
}
