// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

public partial class LumosSocialPlatform : ISocialPlatform
{
	/// <summary>
	/// Reports a new score.
	/// </summary>
	/// <param name="score">Score.</param>
	/// <param name="leaderboardID">Leaderboard identifier.</param>
	/// <param name="callback">Callback.</param>
	public void ReportScore(System.Int64 score, string leaderboardID, Action<bool> callback)
	{
		var endpoint = LumosSocial.baseUrl + "/users/" + localUser.id + "/scores/" + leaderboardID + "?method=PUT";
		var payload = new Dictionary<string, object>() {
			{ "score", (int)score }
		};

		LumosRequest.Send(endpoint, payload,
			delegate { // Success
				if (callback != null) {
					callback(true);
				}
			},
			delegate { // Error
				if (callback != null) {
					callback(false);
				}
			});
	}

	/// <summary>
	/// Loads the leaderboard descriptions.
	/// </summary>
	/// <param name="callback">Callback.</param>
	public void LoadLeaderboardDescriptions(Action<bool> callback)
	{
		var endpoint = LumosSocial.baseUrl + "/leaderboards/info?method=GET";

		LumosRequest.Send(endpoint,
			delegate (object response) { // Success
				var resp = response as IList;
				var leaderboards = new List<LumosLeaderboard>();

				foreach(Dictionary<string, object> info in resp) {
					var leaderboard = LumosLeaderboard.ParseLeaderboardInfo(info);
					leaderboards.Add(leaderboard);
				}

				LumosSocial.leaderboards = leaderboards;

				if (callback != null) {
					callback(true);
				}
			},
			delegate { // Error
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
