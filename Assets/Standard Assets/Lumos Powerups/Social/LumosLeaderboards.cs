// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.GameCenter;
using UnityEngine.SocialPlatforms.Impl;


// Functions dealing with fetching and posting to leaderboards.
public partial class LumosSocial 
{
	static Dictionary<string, LumosLeaderboard> _leaderboards;

	/// <summary>
	/// The leaderboards.
	/// </summary>
	public static ILeaderboard[] leaderboards
	{
		get {
			if (_leaderboards == null) {
				return null;
			} else {
				return _leaderboards.Values.ToArray();
			}
		}
	}


	// Creates an empty leaderboard object.
	public ILeaderboard CreateLeaderboard ()
	{
		var leaderboard = new LumosLeaderboard();
		return leaderboard;
	}


	/// Reports a new score.
	public void ReportScore (System.Int64 score, string leaderboardID, Action<bool> callback)
	{
		if (localUser == null) {
			Lumos.LogWarning("[Lumos] The user must be authenticated before recording their score.");
			callback(false);
			return;
		}

		var endpoint = LumosSocial.baseUrl + "/users/" + localUser.id + "/scores/" + leaderboardID + "?method=PUT";
		var payload = new Dictionary<string, object>() {
			{ "score", (int)score }
		};

		LumosRequest.Send(endpoint, payload,
			success => {
				if (Application.platform == RuntimePlatform.IPhonePlayer && LumosSocialSettings.useGameCenter) {
					ReportScoreToGameCenter(leaderboardID, score);
				}
			
				if (callback != null) {
					callback(true);
				}
			},
			error => {
				if (callback != null) {
					callback(false);
				}
			});
	}

	/// <summary>
	/// Loads the scores.
	/// </summary>
	/// <param name="leaderboard">
	/// Leaderboard.
	/// </param>
	/// <param name="callback">
	/// Callback.
	/// </param>
	public void LoadScores (ILeaderboard leaderboard, Action<bool> callback)
	{
		LoadScores(leaderboard.id,
			scores => {
				callback(scores != null);
			});
	}

	/// Loads the scores.
	public void LoadScores (string leaderboardID, Action<IScore[]> callback)
	{
		var leaderboard = LumosSocial.GetLeaderboard(leaderboardID);

		leaderboard.LoadScores(
			success => {
				if (success) {
					callback(leaderboard.scores);
				} else {
					if (callback != null) {
						callback(null);
					}
				}
			});
	}

	/// <summary>
	/// Gets whether the specified leaderboard is loading.
	/// </summary>
	/// <param name="leaderboard">The leaderboard in question.</param>
	/// <returns type="bool">True if the leaderboard is currently loading scores.</returns>
	public bool GetLoading (ILeaderboard leaderboard)
	{
		return leaderboard.loading;
	}


	// Shows the leaderboard UI.
	public void ShowLeaderboardUI ()
	{
		LumosSocialGUI.ShowWindow(LumosGUIWindow.Leaderboards);
	}

	#region Added Functions

	/// <summary>
	/// Adds a leaderboard.
	/// </summary>
	/// <param name="leaderboard">The leaderboard to add.</param>
	public static void AddLeaderboard (LumosLeaderboard leaderboard)
	{
		_leaderboards[leaderboard.id] = leaderboard;
	}

	/// <summary>
	/// Gets the leaderboard.
	/// </summary>
	/// <param name="leaderboardID">The leaderboard identifier.</param>
	/// <returns type="LumosLeaderboard">The leaderboard.</returns>
	public static LumosLeaderboard GetLeaderboard (string leaderboardID)
	{
		if (_leaderboards.ContainsKey(leaderboardID)) {
			return _leaderboards[leaderboardID];
		} else {
			return null;
		}
	}

	/// <summary>
	/// Loads the leaderboard descriptions.
	/// </summary>
	/// <param name="callback">Callback.</param>
	public static void LoadLeaderboardDescriptions(Action<bool> callback)
	{
		var endpoint = LumosSocial.baseUrl + "/leaderboards/info?method=GET";

		LumosRequest.Send(endpoint,
			success => {
				var resp = success as IList;
				_leaderboards = new Dictionary<string, LumosLeaderboard>();

				foreach (Dictionary<string, object> info in resp) {
					var leaderboard = new LumosLeaderboard(info);
					_leaderboards[leaderboard.id] = leaderboard;
				}

				if (callback != null) {
					callback(true);
				}
			},
			error => {
				if (callback != null) {
					callback(false);
				}
			});
	}
	
	void ReportScoreToGameCenter (string leaderboardID, System.Int64 score)
	{		
		LumosSocialSettings.gameCenterPlatform.ReportScore(score, leaderboardID, delegate {
			Lumos.Log("Reported leaderboard score to Game Center.");
		});
	}

	#endregion
}
