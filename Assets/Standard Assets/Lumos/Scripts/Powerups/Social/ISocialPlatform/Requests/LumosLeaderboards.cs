using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

public partial class LumosSocialPlatform : ISocialPlatform {


	void RecordHighScore(int score, string leaderboardId, Action<bool> callback) 
	{
		var api = url + "/leaderboards/" + leaderboardId + "/" + localUser.id;
		
		var parameters = new Dictionary<string, object>() {
			{ "score", LumosCore.playerId }
		};
		
		LumosRequest.Send(api, parameters, delegate {
			var response = LumosRequest.lastResponse;
			var message = (bool)response["message"];
			callback(message);
		});
	}
	
	void FetchLeaderboardScores(string leaderboardId, int limit, int offset, Action<IScore[]> callback)
	{
		var api = url + "/leaderboards/" + leaderboardId;
		
		var parameters = new Dictionary<string, object>() {
			{ "limit", limit },
			{ "offset", offset }
		};
		
		LumosRequest.Send(api, parameters, delegate {
			var response = LumosRequest.lastResponse;
			var info = response["leaderboard"] as Hashtable;
			var scores = ParseScores(info);
			callback(scores);
		});
	}

	IScore[] ParseScores(Hashtable info) 
	{
		var scores = new List<IScore>();

		foreach (Hashtable score in info["scores"] as Hashtable) {
			var value = (int)score["score"];
			var date = DateTime.Parse(info["created"] as string);
			var leaderboardID = info["leaderboard_id"] as string;
			var formattedValue = ""; // Lumos doesn't support this
			var userID = info["username"] as string;
			var rank = (int)info["rank"];

			var s = new Score(leaderboardID, value, userID, date, formattedValue, rank);
			scores.Add(s);
		}

		return scores.ToArray();
	}
}
