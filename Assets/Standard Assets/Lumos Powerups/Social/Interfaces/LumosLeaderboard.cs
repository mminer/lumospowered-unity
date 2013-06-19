using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Lumos leaderboard.
/// </summary>
public class LumosLeaderboard : ILeaderboard {
	
	/// <summary>
	/// Gets or sets the identifier.
	/// </summary>
	/// <value>
	/// The identifier.
	/// </value>
	public string id { get; set; }
	/// <summary>
	/// Gets or sets the user scope.
	/// </summary>
	/// <value>
	/// The user scope.
	/// </value>
	public UserScope userScope { get; set; }
	/// <summary>
	/// Gets or sets the range.
	/// </summary>
	/// <value>
	/// The range.
	/// </value>
	public Range range { get; set; }
	/// <summary>
	/// Gets or sets the time scope.
	/// </summary>
	/// <value>
	/// The time scope.
	/// </value>
	public TimeScope timeScope { get; set; }
	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="LumosLeaderboard"/> is loading.
	/// </summary>
	/// <value>
	/// <c>true</c> if loading; otherwise, <c>false</c>.
	/// </value>
	public bool loading { get; private set; }
	/// <summary>
	/// Gets or sets the local user score.
	/// </summary>
	/// <value>
	/// The local user score.
	/// </value>
	public IScore localUserScore { get; private set; }
	/// <summary>
	/// Gets or sets the max range.
	/// </summary>
	/// <value>
	/// The max range.
	/// </value>
	public uint maxRange { get; private set; }
	/// <summary>
	/// Gets or sets the scores.
	/// </summary>
	/// <value>
	/// The scores.
	/// </value>
	public IScore[] scores { get; private set; }
	/// <summary>
	/// Gets or sets the friend scores.
	/// </summary>
	/// <value>
	/// The friend scores.
	/// </value>
	public IScore[] friendScores { get; private set; }
	/// <summary>
	/// Gets or sets the title.
	/// </summary>
	/// <value>
	/// The title.
	/// </value>
	public string title { get; set; }
	
	/// <summary>
	/// The callback.
	/// </summary>
	Action<bool> callback;
	
	/// <summary>
	/// The URL.
	/// </summary>
	string url = "http://localhost:8888/api/1/";
	
	/// <summary>
	/// Sets the user filter.
	/// </summary>
	/// <param name='userIDs'>
	/// User I ds.
	/// </param>
	public void SetUserFilter(string[] userIDs)
	{
		// do nothing
	}
	
	/// <summary>
	/// Loads the descriptions of the leaderboard.
	/// </summary>
	/// <param name='callback'>
	/// Callback.
	/// </param>
	public void LoadDescription (Action<bool> callback)
	{
		if (id == null) {
			Debug.LogWarning("Leaderboard must have an ID before loading description.");
			return;
		}
		
		var api = url + "leaderboards/" + id + "?method=GET";

		LumosRequest.Send(api, delegate (object response) {
			var info = response as Dictionary<string, object>;
			var leaderboard = ParseLeaderboardInfo(info);
			this.scores = leaderboard.scores;
			this.title = leaderboard.title;
			callback(true);
		});
	}
	
	/// <summary>
	/// Loads the scores.
	/// </summary>
	/// <param name='callback'>
	/// Callback.
	/// </param>
	public void LoadScores(Action<bool> callback)
	{
		LoadScores(1, 0, callback);
	}
	
	/// <summary>
	/// Loads the scores.
	/// </summary>
	/// <param name='limit'>
	/// Limit.
	/// </param>
	/// <param name='offset'>
	/// Offset.
	/// </param>
	/// <param name='callback'>
	/// Callback.
	/// </param>
	public void LoadScores(int limit, int offset, Action<bool> callback)
	{
		if (friendScores == null && !loading) {
			FetchFriendScores();
		}

		FetchScores(limit, offset, AddScores);

		loading = true;
		this.callback = callback;
	}
	
	/// <summary>
	/// Loads the scores.
	/// </summary>
	/// <param name='limit'>
	/// Limit.
	/// </param>
	/// <param name='offset'>
	/// Offset.
	/// </param>
	/// <param name='callback'>
	/// Callback.
	/// </param>
	public void LoadScoresAroundUser(int limit, Action<IScore[]> callback)
	{
		if (limit < 1) {
			limit = 1;
		}
		
		FetchUserScores(limit, callback);
	}
	
	/// <summary>
	/// Adds the scores.
	/// </summary>
	/// <param name='scores'>
	/// Scores.
	/// </param>
	void AddScores(IScore[] scores)
	{
		loading = false;
		this.callback(true);
		this.callback = null;
	}
	
	/// <summary>
	/// Fetchs the scores.
	/// </summary>
	/// <param name='limit'>
	/// Limit.
	/// </param>
	/// <param name='offset'>
	/// Offset.
	/// </param>
	/// <param name='callback'>
	/// Callback.
	/// </param>
	void FetchScores (int limit, int offset, Action<IScore[]> callback)
	{
		var api = url + "leaderboards/" + id + "/scores?method=GET";

		var parameters = new Dictionary<string, object>() {
			{ "limit", limit },
			{ "offset", offset }
		};

		LumosRequest.Send(api, parameters, delegate (object response) {
			var resp = response as Dictionary<string, object>;
			var scoreList = resp["scores"] as IList;
			var scores = new List<IScore>();

			foreach (Dictionary<string, object> info in scoreList) {
				var score = ParseScore(id, info);
				scores.Add(score);
			}

			IndexScores(scores);
			callback(scores.ToArray());
		});
	}
	
	/// <summary>
	/// Fetches the scores surrounding the playing user.
	/// </summary>
	/// <param name='limit'>
	/// Limit.
	/// </param>
	/// <param name='callback'>
	/// Callback.
	/// </param>
	void FetchUserScores (int limit, Action<IScore[]> callback)
	{
		var api = url + "users/" + Social.localUser.id + "/scores/" + id + "?method=GET";

		var parameters = new Dictionary<string, object>() {
			{ "limit", limit }
		};

		LumosRequest.Send(api, parameters, delegate (object response) {
			var resp = response as Dictionary<string, object>;
			var scoreList = resp["scores"] as IList;
			var scores = new List<IScore>();

			foreach (Dictionary<string, object> info in scoreList) {
				var score = ParseScore(id, info);
				scores.Add(score);
			}

			IndexScores(scores);
			callback(scores.ToArray());
		});
	}
	
	/// <summary>
	/// Fetchs the friend scores.
	/// </summary>
	void FetchFriendScores ()
	{
		var api = url + "users/" + Social.localUser.id + "/friends/scores/" + id + "?method=GET";

		LumosRequest.Send(api, delegate (object response) {
			var resp = response as Dictionary<string, object>;
			var scoreList = resp["scores"] as IList;
			this.friendScores = ParseScores(id, scoreList);
		});
	}
	
	public void SetFriendScores (IScore[] scores)
	{
		friendScores = scores;
	}
	
	/// <summary>
	/// Parses the leaderboard info.
	/// </summary>
	/// <returns>
	/// The leaderboard info.
	/// </returns>
	/// <param name='info'>
	/// Info.
	/// </param>
	public static LumosLeaderboard ParseLeaderboardInfo (Dictionary<string, object> info)
	{
		return ParseLeaderboardInfo(info, false);
	}
	
	/// <summary>
	/// Parses the leaderboard info.
	/// </summary>
	/// <returns>
	/// The leaderboard info.
	/// </returns>
	/// <param name='info'>
	/// Info.
	/// </param>
	public static LumosLeaderboard ParseLeaderboardInfo (Dictionary<string, object> info, bool friendScores)
	{
		var leaderboard = new LumosLeaderboard();
		leaderboard.id = info["leaderboard_id"] as string;
		leaderboard.title = info["name"] as string;
		
		if (info.ContainsKey("scores")) {
			var scores = LumosLeaderboard.ParseScores(leaderboard.id, info["scores"] as IList);
			
			if (friendScores) {
				leaderboard.friendScores = scores;
			} else {
				leaderboard.scores = scores;
			}
		}
		
		return leaderboard;
	}
	
	/// <summary>
	/// Parses the scores.
	/// </summary>
	/// <returns>
	/// The scores.
	/// </returns>
	/// <param name='data'>
	/// Data.
	/// </param>
	public static IScore[] ParseScores (string leaderboardID, IList data)
	{
		var scores = new List<IScore>();
		
		foreach (Dictionary<string, object> info in data) {
			var score = ParseScore(leaderboardID, info);
			scores.Add(score);
		}
		
		return scores.ToArray();
	}
	
	/// <summary>
	/// Parses a score.
	/// </summary>
	/// <returns>
	/// The scores.
	/// </returns>
	/// <param name='info'>
	/// Info.
	/// </param>
	static IScore ParseScore (string leaderboardID, Dictionary<string, object> info)
	{
		var value = Convert.ToInt32(info["score"]);
		var timestamp = Convert.ToDouble(info["created"]);
		var date = LumosUtil.UnixTimestampToDateTime(timestamp);
		var formattedValue = ""; // Lumos doesn't support this
		var userID = info["username"] as string;
		var rank = Convert.ToInt32(info["rank"]);

		var score = new Score(leaderboardID, value, userID, date, formattedValue, rank);
		return score;
	}
	
	/// <summary>
	/// Indexs the scores.
	/// </summary>
	/// <param name='newScores'>
	/// New scores.
	/// </param>
	void IndexScores (List<IScore> newScores)
	{
		if (newScores == null || newScores.Count < 1) {
			Debug.LogWarning("There are no more scores to load.");
			return;
		}

		int lastRank;
		var updatedScores = new List<IScore>();

		if (scores != null) {
			lastRank = scores[scores.Length - 1].rank;

			foreach (var currentScore in scores) {
				updatedScores.Add(currentScore);
			}
		} else {
			lastRank = 0;
		}

		int newFirstRank = newScores[0].rank;

		if (newFirstRank - lastRank != 1) {
			Debug.LogWarning("Loaded scores ranks don't line up with exising scores.");
			return;
		}

		foreach (var newScore in newScores) {
			updatedScores.Add(newScore);
		}

		scores = updatedScores.ToArray();
	}
}
