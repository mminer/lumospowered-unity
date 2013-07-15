// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Lumos leaderboard.
/// </summary>
public class LumosLeaderboard : ILeaderboard
{
	/// <summary>
	/// Gets or sets the identifier.
	/// </summary>
	public string id { get; set; }

	/// <summary>
	/// Gets or sets the user scope.
	/// </summary>
	public UserScope userScope { get; set; }

	/// <summary>
	/// Gets or sets the range.
	/// </summary>
	public Range range { get; set; }

	/// <summary>
	/// Gets or sets the time scope.
	/// </summary>
	public TimeScope timeScope { get; set; }

	/// <summary>
	/// Indicates whether this leaderboard is currently loading scores.
	/// </summary>
	public bool loading { get; private set; }

	/// <summary>
	/// Gets or sets the local user score.
	/// </summary>
	public IScore localUserScore { get; private set; }

	/// <summary>
	/// Gets or sets the max range.
	/// </summary>
	public uint maxRange { get; private set; }

	/// <summary>
	/// User scores.
	/// </summary>
	public IScore[] scores { get; private set; }

	/// <summary>
	/// Friend's scores.
	/// </summary>
	public IScore[] friendScores { get; private set; }

	/// <summary>
	/// The leaderboard's title.
	/// </summary>
	public string title { get; set; }

	/// <summary>
	/// The callback.
	/// </summary>
	Action<bool> callback;

	/// <summary>
	/// Sets the user filter.
	/// </summary>
	/// <param name="userIDs">User IDs.</param>
	public void SetUserFilter(string[] userIDs)
	{
		// do nothing
	}

	/// <summary>
	/// Loads the descriptions of the leaderboard.
	/// </summary>
	/// <param name="callback">Callback.</param>
	public void LoadDescription (Action<bool> callback)
	{
		if (id == null) {
			Debug.LogWarning("Leaderboard must have an ID before loading description.");
			return;
		}

		var endpoint = LumosSocial.baseUrl + "/leaderboards/" + id + "?method=GET";
		loading = true;

		LumosRequest.Send(endpoint,
			success => {
				var info = success as Dictionary<string, object>;
				var leaderboard = ParseLeaderboardInfo(info);
				this.scores = leaderboard.scores;
				this.title = leaderboard.title;
				loading = false;
				callback(true);
			});
	}

	/// <summary>
	/// Loads the scores.
	/// </summary>
	/// <param name="callback">Callback.</param>
	public void LoadScores(Action<bool> callback)
	{
		LoadScores(1, 0, callback);
	}

	/// <summary>
	/// Loads the scores.
	/// </summary>
	/// <param name="limit">Limit.</param>
	/// <param name="offset">Offset.</param>
	/// <param name="callback">Callback.</param>
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
	/// <param name="limit">Limit.</param>
	/// <param name="offset">Offset.</param>
	/// <param name="callback">Callback.</param>
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
	/// <param name="scores">Scores.</param>
	void AddScores(IScore[] scores)
	{
		loading = false;
		this.callback(true);
		this.callback = null;
	}

	/// <summary>
	/// Fetches the scores.
	/// </summary>
	/// <param name="limit">Limit.</param>
	/// <param name="offset">Offset.</param>
	/// <param name="callback">Callback.</param>
	void FetchScores (int limit, int offset, Action<IScore[]> callback)
	{
		loading = true;
		var endpoint = LumosSocial.baseUrl + "/leaderboards/" + id + "/scores?method=GET";

		var parameters = new Dictionary<string, object>() {
			{ "limit", limit },
			{ "offset", offset }
		};

		LumosRequest.Send(endpoint, parameters,
			success => {
				var resp = success as Dictionary<string, object>;
				var scoreList = resp["scores"] as IList;
				var scores = new List<IScore>();

				foreach (Dictionary<string, object> info in scoreList) {
					var score = ParseScore(id, info);
					scores.Add(score);
				}

				IndexScores(scores);
				loading = false;
				callback(scores.ToArray());
			});
	}

	/// <summary>
	/// Fetches the scores surrounding the playing user.
	/// </summary>
	/// <param name="limit">Limit.</param>
	/// <param name="callback">Callback.</param>
	void FetchUserScores (int limit, Action<IScore[]> callback)
	{
		var endpoint = LumosSocial.baseUrl + "/users/" + Social.localUser.id + "/scores/" + id + "?method=GET";
		loading = true;

		var parameters = new Dictionary<string, object>() {
			{ "limit", limit }
		};

		LumosRequest.Send(endpoint, parameters,
			success => {
				var resp = success as Dictionary<string, object>;
				var scoreList = resp["scores"] as IList;
				var scores = new List<IScore>();

				foreach (Dictionary<string, object> info in scoreList) {
					var score = ParseScore(id, info);
					scores.Add(score);
				}

				IndexScores(scores);
				loading = false;
				callback(scores.ToArray());
			});
	}

	/// <summary>
	/// Fetches the friend scores.
	/// </summary>
	void FetchFriendScores ()
	{
		var endpoint = LumosSocial.baseUrl + "/users/" + Social.localUser.id + "/friends/scores/" + id + "?method=GET";
		loading = true;

		LumosRequest.Send(endpoint,
			success => {
				var resp = success as Dictionary<string, object>;
				var scoreList = resp["scores"] as IList;
				this.friendScores = ParseScores(id, scoreList);
				loading = false;
			});
	}

	public void SetFriendScores (IScore[] scores)
	{
		friendScores = scores;
	}

	/// <summary>
	/// Parses the leaderboard info.
	/// </summary>
	/// <param name="info">Info.</param>
	/// <returns>The leaderboard info.</returns>
	public static LumosLeaderboard ParseLeaderboardInfo (Dictionary<string, object> info)
	{
		return ParseLeaderboardInfo(info, false);
	}

	/// <summary>
	/// Parses the leaderboard info.
	/// </summary>
	/// <param name="info">Info.</param>
	/// <returns>The leaderboard info.</returns>
	public static LumosLeaderboard ParseLeaderboardInfo (Dictionary<string, object> info, bool friendScores)
	{
		var leaderboard = new LumosLeaderboard();
		leaderboard.id = info["leaderboard_id"] as string;
		leaderboard.title = info["name"] as string;
		leaderboard.loading = false;

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
	/// <param name="data">Data.</param>
	/// <returns>The scores.</returns>
	static IScore[] ParseScores (string leaderboardID, IList data)
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
	/// <param name="info">Info.</param>
	/// <returns>The scores.</returns>
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
	/// <param name="newScores">New scores.</param>
	void IndexScores (List<IScore> newScores)
	{
		if (newScores == null || newScores.Count < 1) {
			Debug.LogWarning("There are no more scores to load.");
			return;
		}

		int lastRank;
		var updatedScores = new List<IScore>();

		if (scores != null && scores.Length != 0) {
			lastRank = scores[scores.Length - 1].rank;

			foreach (var currentScore in scores) {
				updatedScores.Add(currentScore);
			}
		} else {
			lastRank = 0;
		}

		int newFirstRank = newScores[0].rank;

		// Paging is wrong or they refreshed an existing page
		// Do not add new scores, but replace old scores, if any
		if (newFirstRank - lastRank != 1) {
			// Check if existing scores changed
			foreach (var newScore in newScores) {
				for (int x = 0; x < scores.Length; x++) {
					if (scores[x].userID == newScore.userID) {
						if (newScore.value > scores[x].value) {
							scores[x] = newScore;
						}

						break;
					}
				}
			}
		} else {
			foreach (var newScore in newScores) {
				updatedScores.Add(newScore);
			}

			scores = updatedScores.ToArray();
		}
	}
}
