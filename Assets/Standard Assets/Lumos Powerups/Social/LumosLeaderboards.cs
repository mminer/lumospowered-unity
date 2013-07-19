// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

public partial class LumosSocial : ISocialPlatform
{
	/// <summary>
	/// The leaderboards.
	/// </summary>
	public static Dictionary<string, LumosLeaderboard> leaderboards = new Dictionary<string, LumosLeaderboard>();

	/// <summary>
	/// Creates an empty leaderboard object.
	/// </summary>
	/// <returns>A new leaderboard.</returns>
	public ILeaderboard CreateLeaderboard ()
	{
		var leaderboard = new LumosLeaderboard();
		return leaderboard;
	}

	/// <summary>
	/// Reports a new score.
	/// </summary>
	/// <param name="score">Score.</param>
	/// <param name="leaderboardID">Leaderboard identifier.</param>
	/// <param name="callback">Callback.</param>
	public void ReportScore (System.Int64 score, string leaderboardID, Action<bool> callback)
	{
		var endpoint = LumosSocial.baseUrl + "/users/" + localUser.id + "/scores/" + leaderboardID + "?method=PUT";
		var payload = new Dictionary<string, object>() {
			{ "score", (int)score }
		};

		LumosRequest.Send(endpoint, payload,
			success => {
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
	/// <param name="callback">Callback.</param>
	public void LoadScores (ILeaderboard leaderboard, Action<bool> callback)
	{
		// TODO: use callback
		LoadScores(leaderboard.id, null);
	}

	/// <summary>
	/// Loads the scores.
	/// </summary>
	/// <param name="leaderboardID">The leaderboard identifier.</param>
	/// <param name="callback">Callback.</param>
	public void LoadScores (string leaderboardID, Action<IScore[]> callback)
	{
		var leaderboard = LumosSocial.GetLeaderboard(leaderboardID);
		// TODO: consider putting all loading logic here
		leaderboard.LoadScores(delegate {
			if (callback != null) {
				callback(leaderboard.scores);
			}
		});
	}

	/// <summary>
	/// Gets whether the specified leaderboard is loading.
	/// Though Unity's documentation makes no mention of it, this is a required
	/// (but redundant) function of the ISocialPlatform interface.
	/// </summary>
	/// <param name="leaderboard">The leaderboard in question.</param>
	/// <returns>True if the leaderboard is currently loading scores.</returns>
	public bool GetLoading (ILeaderboard leaderboard)
	{
		return leaderboard.loading;
	}

	/// <summary>
	/// Shows the leaderboard UI.
	/// </summary>
	public void ShowLeaderboardUI ()
	{
		// TODO: Make naming consistent
		LumosSocialGUI.ShowLeaderboardUI();
	}








	/// <summary>
	/// Gets the leaderboard.
	/// </summary>
	/// <param name="leaderboardID">The leaderboard identifier.</param>
	/// <returns>The leaderboard.</returns>
	public static LumosLeaderboard GetLeaderboard (string leaderboardID)
	{
		if (leaderboards.ContainsKey(leaderboardID)) {
			return leaderboards[leaderboardID];
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
				var leaderboards = new List<LumosLeaderboard>();

				foreach(Dictionary<string, object> info in resp) {
					var leaderboard = LumosLeaderboard.ParseLeaderboardInfo(info);
					LumosSocial.leaderboards[leaderboard.id] = leaderboard;
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

	void LoadFriendLeaderboardScores (Action<bool> callback)
	{
		(localUser as LumosUser).LoadFriendLeaderboardScores(callback);
	}
}
