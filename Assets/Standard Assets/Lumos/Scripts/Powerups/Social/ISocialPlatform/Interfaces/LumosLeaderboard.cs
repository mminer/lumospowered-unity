using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;
using System;
using System.Collections;
using System.Collections.Generic;

public class LumosLeaderboard : ILeaderboard {

	public string id { get; set; }
	public UserScope userScope { get; set; }
	public Range range { get; set; }
	public TimeScope timeScope { get; set; }
	public bool loading { get; private set; }
	public IScore localUserScore { get; private set; }
	public uint maxRange { get; private set; }
	public IScore[] scores { get; private set; }
	public IScore[] friendScores { get; private set; }
	public string title { get; set; }

	Action<bool> callback;
	
	string url = "localhost:8888/api/1/games/" + Lumos.gameId + "/";

	public void SetUserFilter(string[] userIDs)
	{
		// do nothing
	}

	public void LoadScores(Action<bool> callback)
	{
		loading = true;
		this.callback = callback;
		FetchScores(25, 0, AddScores);
		FetchFriendScores();
	}

	void AddScores(IScore[] scores)
	{
		loading = false;
		this.scores = scores;
		this.callback(true);
		this.callback = null;
	}
	
	void FetchScores(int limit, int offset, Action<IScore[]> callback)
	{
		var api = url + "leaderboards/" + id + "?method=GET";
		
		var parameters = new Dictionary<string, object>() {
			{ "limit", limit },
			{ "offset", offset }
		};
		
		LumosRequest.Send(api, parameters, delegate {
			var response = LumosRequest.lastResponse as Dictionary<string, object>;
			var scoreList = response["scores"] as IList;
			var scores = new List<IScore>();
			
			foreach (Dictionary<string, object> info in scoreList) {
				var score = ParseScores(info);
				scores.Add(score);
			}
			
			this.scores = scores.ToArray();
			
			callback(scores.ToArray());
		});
	}
	
	void FetchFriendScores()
	{
		var api = url + "leaderboards/" + id + "/" + Social.localUser.id + "/friends?method=GET";
		
		LumosRequest.Send(api, delegate {
			var response = LumosRequest.lastResponse as Dictionary<string, object>;
			var scoreList = response["scores"] as IList;
			var scores = new List<IScore>();
			
			foreach (Dictionary<string, object> info in scoreList) {
				var score = ParseScores(info);
				scores.Add(score);
			}
			
			this.friendScores = scores.ToArray();
		});
	}
	
	IScore ParseScores(Dictionary<string, object> info) 
	{
		var value = Convert.ToInt32(info["score"]);
		var timestamp = Convert.ToDouble(info["created"]);
		var date = LumosUtil.UnixTimestampToDateTime(timestamp);
		var formattedValue = ""; // Lumos doesn't support this
		var userID = info["username"] as string;
		var rank = Convert.ToInt32(info["rank"]);

		var score = new Score(id, value, userID, date, formattedValue, rank);
		return score;
	}
}
