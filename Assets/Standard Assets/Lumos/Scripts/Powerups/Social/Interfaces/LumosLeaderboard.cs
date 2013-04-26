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
	string url = "http://localhost:8888/api/1/games/" + Lumos.gameId + "/";
	
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
	void FetchScores(int limit, int offset, Action<IScore[]> callback)
	{
		var api = url + "leaderboards/" + id + "?method=GET";

		var parameters = new Dictionary<string, object>() {
			{ "limit", limit },
			{ "offset", offset }
		};

		LumosRequest.Send(api, parameters, delegate (object response) {
			var resp = response as Dictionary<string, object>;
			var scoreList = resp["scores"] as IList;
			var scores = new List<IScore>();

			foreach (Dictionary<string, object> info in scoreList) {
				var score = ParseScores(info);
				scores.Add(score);
			}

			IndexScores(scores);
			callback(scores.ToArray());
		});
	}
	
	/// <summary>
	/// Fetchs the friend scores.
	/// </summary>
	void FetchFriendScores()
	{
		var api = url + "leaderboards/" + id + "/" + Social.localUser.id + "/friends?method=GET";

		LumosRequest.Send(api, delegate (object response) {
			var resp = response as Dictionary<string, object>;
			var scoreList = resp["scores"] as IList;
			var scores = new List<IScore>();

			foreach (Dictionary<string, object> info in scoreList) {
				var score = ParseScores(info);
				scores.Add(score);
			}

			this.friendScores = scores.ToArray();
		});
	}
	
	/// <summary>
	/// Parses the scores.
	/// </summary>
	/// <returns>
	/// The scores.
	/// </returns>
	/// <param name='info'>
	/// Info.
	/// </param>
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
	
	/// <summary>
	/// Indexs the scores.
	/// </summary>
	/// <param name='newScores'>
	/// New scores.
	/// </param>
	void IndexScores(List<IScore> newScores)
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
