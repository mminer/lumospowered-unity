using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

public partial class LumosSocialPlatform : ISocialPlatform
{
	void RecordHighScore (int score, string leaderboardId, Action<bool> callback)
	{
		var api = url + "users/" + localUser.id + "/scores/" + leaderboardId + "?method=PUT";

		var parameters = new Dictionary<string, object>() {
			{ "score", score }
		};

		LumosRequest.Send(api, parameters, delegate {
			callback(true);
		});
	}

	void FetchLeaderboardDescriptions (Action<bool> callback)
	{
		var api = url + "leaderboards/info?method=GET";

		LumosRequest.Send(api, delegate (object response) {
			var resp = response as IList;
			var leaderboards = new List<LumosLeaderboard>();

			foreach(Dictionary<string, object> info in resp) {
				var leaderboard = LumosLeaderboard.ParseLeaderboardInfo(info);
				leaderboards.Add(leaderboard);
			}

			LumosSocial.leaderboards = leaderboards;
			callback(true);
		});
	}
	
	void FetchAllFriendLeaderboards (Action<bool> callback)
	{
		var api = url + "users/" + localUser.id + "/friends/scores?method=GET";

		LumosRequest.Send(api, delegate (object response) {
			var resp = response as IList;
			var leaderboards = new List<LumosLeaderboard>();

			foreach(Dictionary<string, object> info in resp) {
				var leaderboard = LumosLeaderboard.ParseLeaderboardInfo(info);
				leaderboards.Add(leaderboard);
			}
			
			foreach (var leaderboard in leaderboards) {
				var current = LumosSocial.GetLeaderboard(leaderboard.id);
				
				// Leaderboard already exists, update friend scores only
				if (current != null) {
					current.SetFriendScores(leaderboard.friendScores);
				// Leaderboard doesn't exist yet, add entire leaderboard
				} else {
					LumosSocial.leaderboards.Add(leaderboard);
				}
			}
			
			callback(true);
		});
	}

	/*
	 * public string id { get; set; }
	public UserScope userScope { get; set; }
	public Range range { get; set; }
	public TimeScope timeScope { get; set; }
	public bool loading { get; private set; }
	public IScore localUserScore { get; private set; }
	public uint maxRange { get; private set; }
	public IScore[] scores { get; private set; }
	public string title { get; private set; }

	*/
}
