// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

public partial class LumosSocialPlatform : ISocialPlatform
{
	string url = "localhost:8888/api/1/games/" + Lumos.gameId + "/";
	LumosUser _localUser = new LumosUser();
	
	public ILocalUser localUser {
		get { return _localUser; }
		set { _localUser = value as LumosUser; }
	}


	public LumosSocialPlatform()
	{
		Social.Active = this;
	}


	#region Users

	public void Authenticate(ILocalUser user, Action<bool> callback) 
	{
		user.Authenticate(callback);
	}

	public void LoadFriends(ILocalUser user, Action<bool> callback) 
	{
		user.LoadFriends(callback);
	}

	public void LoadUsers(string[] userIds, Action<IUserProfile[]> callback)
	{
		FetchUsers(userIds, callback);
	}

	#endregion

	#region Achievements

	public void LoadAchievementDescriptions(Action<IAchievementDescription[]> callback) 
	{
		FetchGameAchievements(callback);
	}

	public void LoadAchievements(Action<IAchievement[]> callback) 
	{
		FetchPlayerAchievements(callback);
	}
	
	public void ReportProgress(string achievementId, double percentCompleted, Action<bool> callback) 
	{
		UpdateAchievementProgress(achievementId, (int)percentCompleted, callback);
	}

	public void ShowAchievementsUI() 
	{
		// Do nothing
	}

	public IAchievement CreateAchievement() 
	{
		Lumos.LogError("Lumos does not support creating achievements on the fly.");
		return null;
	}

	#endregion

	#region Leaderboards
	
	public void ReportScore(System.Int64 score, string leaderboardId, Action<bool> callback) 
	{
		RecordHighScore((int)score, leaderboardId, callback);
	}
	
	public void LoadScores(ILeaderboard leaderboard, Action<bool> callback)
	{
		leaderboard.LoadScores(callback);
	}

	public void LoadScores(string leaderboardID, Action<IScore[]> callback)
	{
		FetchLeaderboardScores(leaderboardID, 100, 0, callback);
	}

	public void ShowLeaderboardUI() 
	{
		// Do nothing
	}

	public ILeaderboard CreateLeaderboard()
	{
		Lumos.LogError("Lumos does not support creating leaderboards on the fly.");
		return null;
	}

	#endregion

	#region Other

	public bool GetLoading(ILeaderboard leaderboard) 
	{
		// Not sure what this is supposed to do.
		return false;
	}
	
	#endregion
}
