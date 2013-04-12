using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

public partial class LumosSocialPlatform : ISocialPlatform {


	void RecordHighScore(int score, string leaderboardId, Action<bool> callback) 
	{
		var api = url + "leaderboards/" + leaderboardId + "/" + localUser.id;
		
		var parameters = new Dictionary<string, object>() {
			{ "score", score }
		};
		
		LumosRequest.Send(api, parameters, delegate {
			callback(true);
		});
	}
	
	void FetchLeaderboardDescriptions(Action<bool> callback)
	{
		var api = url + "leaderboards/info?method=GET";
		
		LumosRequest.Send(api, delegate {
			var response = LumosRequest.lastResponse as IList;
			var leaderboards = new List<LumosLeaderboard>();
			
			foreach(Dictionary<string, object> info in response) {
				var leaderboard = ParseLeaderboardInfo(info);
				leaderboards.Add(leaderboard);
			}
			
			LumosSocial.leaderboards = leaderboards;
			callback(true);
		});
	}
	
	LumosLeaderboard ParseLeaderboardInfo(Dictionary<string, object> info)
	{
		var leaderboard = new LumosLeaderboard();
		leaderboard.id = info["leaderboard_id"] as string;
		leaderboard.title = info["name"] as string;
		return leaderboard;
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
