// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

/// <summary>
/// Holds user scores.
/// </summary>
public class LumosLeaderboard : ILeaderboard
{
	// Unique identifier.
	public string id { get; set; }

	// The user scope when searched.
	public UserScope userScope { get; set; }

	// The leaderboard's score range.
	public Range range { get; set; }

	// Gets or sets the time scope.
	public TimeScope timeScope { get; set; }

	// Indicates whether this leaderboard is currently loading scores.
	public bool loading { get; private set; }

	// Gets or sets the local user score.
	public IScore localUserScore { get; private set; }

	// Gets or sets the max range.
	public uint maxRange { get; private set; }

	// User scores.
	public IScore[] scores { get; private set; }

	/// <summary>
	/// Friend's scores.
	/// </summary>
	public IScore[] friendScores { get; set; }

	// The name of the leaderboard.
	public string title { get; set; }

	// Creates a new leaderboard object.
	public LumosLeaderboard () {}

	// Creates a new leaderboard object.
	public LumosLeaderboard (Dictionary<string, object> info)
	{
		this.id = info["leaderboard_id"] as string;
		this.title = info["name"] as string;

		if (info.ContainsKey("scores")) {
			this.scores = ParseScores(this.id, info["scores"] as IList);
		}
	}

	// Sets the user filter.
	public void SetUserFilter(string[] userIDs)
	{
		// do nothing
	}

	// Loads the scores.
	public void LoadScores(Action<bool> callback)
	{
		LoadScores(1, 0, callback);
	}

	#region Added Functions

	/// <summary>
	/// Loads the description of the leaderboard.
	/// </summary>
	/// <param name="callback">Callback.</param>
	public void LoadDescription (Action<bool> callback)
	{
		if (id == null) {
			Lumos.LogWarning("Leaderboard must have an ID before loading its description.");
			return;
		}

		var endpoint = LumosSocial.baseUrl + "/leaderboards/" + id + "?method=GET";
		loading = true;

		LumosRequest.Send(endpoint,
			success => {
				var info = success as Dictionary<string, object>;
				title = info["name"] as string;
				scores = ParseScores(id, info["scores"] as IList);
				loading = false;

				if (callback != null) {
					callback(true);
				}
			},
			error => {
				loading = false;

				if (callback != null) {
					callback(false);
				}
			});
	}

	/// <summary>
	/// Loads the scores.
	/// </summary>
	/// <param name="limit">Limit.</param>
	/// <param name="offset">Offset.</param>
	/// <param name="callback">Callback.</param>
	public void LoadScores(int limit, int offset, Action<bool> callback)
	{
		loading = true;

		if (friendScores == null && !loading) {
			FetchFriendScores();
		}

		FetchScores(limit, offset,
			scores => {
				callback(scores != null);
			});
	}

	/// <summary>
	/// Loads the scores.
	/// </summary>
	/// <param name="limit">Limit.</param>
	/// <param name="offset">Offset.</param>
	/// <param name="callback">Callback.</param>
	public void LoadScoresAroundUser(int limit, Action<Score[]> callback)
	{
		if (limit < 1) {
			limit = 1;
		}

		FetchUserScores(limit, callback);
	}

	/// <summary>
	/// Fetches the scores.
	/// </summary>
	/// <param name="limit">Limit.</param>
	/// <param name="offset">Offset.</param>
	/// <param name="callback">Callback.</param>
	void FetchScores (int limit, int offset, Action<Score[]> callback)
	{
		loading = true;
		var endpoint = LumosSocial.baseUrl + "/leaderboards/" + id + "/scores?method=GET";
		var payload = new Dictionary<string, object>() {
			{ "limit", limit },
			{ "offset", offset }
		};

		LumosRequest.Send(endpoint, payload,
			success => {
				var resp = success as Dictionary<string, object>;
				var scoreList = resp["scores"] as IList;
				var newScores = ParseScores(id, scoreList);
				IndexScores(newScores);
				loading = false;

				if (callback != null) {
					callback(scores as Score[]);
				}
			},
			error => {
				loading = false;

				if (callback != null) {
					callback(null);
				}
			});
	}

	/// <summary>
	/// Fetches the scores surrounding the playing user.
	/// </summary>
	/// <param name="limit">Limit.</param>
	/// <param name="callback">Callback.</param>
	void FetchUserScores (int limit, Action<Score[]> callback)
	{
		var endpoint = LumosSocial.baseUrl + "/users/" + Social.localUser.id + "/scores/" + id + "?method=GET";
		loading = true;

		var payload = new Dictionary<string, object>() {
			{ "limit", limit }
		};

		LumosRequest.Send(endpoint, payload,
			success => {
				var resp = success as Dictionary<string, object>;
				var scoreList = resp["scores"] as IList;
				var newScores = ParseScores(id, scoreList);
				IndexScores(newScores);
				loading = false;

				if (callback != null) {
					callback(scores as Score[]);
				}
			},
			error => {
				loading = false;

				if (callback != null) {
					callback(null);
				}
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
				friendScores = ParseScores(id, scoreList);
				loading = false;
			},
			error => {
				loading = false;
			});
	}

	/// <summary>
	/// Parses the scores.
	/// </summary>
	/// <param name="data">Data.</param>
	/// <returns>The scores.</returns>
	static IScore[] ParseScores (string leaderboardID, IList data)
	{
		var scores = new IScore[data.Count];

		for (int i = 0; i < data.Count; i++) {
			scores[i] = ParseScore(leaderboardID, data[i] as Dictionary<string, object>);
		}

		return scores;
	}

	/// <summary>
	/// Parses a score.
	/// </summary>
	/// <param name="info">Information about the score.</param>
	/// <returns>A new score object.</returns>
	static IScore ParseScore (string leaderboardID, Dictionary<string, object> info)
	{
		var val = Convert.ToInt32(info["score"]);
		var timestamp = Convert.ToDouble(info["created"]);
		var date = LumosUtil.UnixTimestampToDateTime(timestamp);
		var formattedValue = ""; // Lumos lacks support for this.
		var userID = info["username"] as string;
		var rank = Convert.ToInt32(info["rank"]);
		var score = new Score(leaderboardID, val, userID, date, formattedValue, rank);
		return score;
	}

	/// <summary>
	/// Indexes the scores.
	/// </summary>
	/// <param name="newScores">New scores.</param>
	void IndexScores (IScore[] newScores)
	{
		if (newScores == null || newScores.Length == 0) {
			Debug.LogWarning("There are no more scores to load.");
			return;
		}

		int lastRank;
		var updatedScores = new List<IScore>();

		if (scores != null && scores.Length > 0) {
			lastRank = scores[scores.Length - 1].rank;

			foreach (var currentScore in scores) {
				updatedScores.Add(currentScore);
			}
		} else {
			lastRank = 0;
		}

		var newFirstRank = newScores[0].rank;

		// Paging is wrong or they refreshed an existing page.
		// Do not add new scores, but replace old scores, if any.
		if (newFirstRank - lastRank != 1) {
			// Check if existing scores changed.
			foreach (var score in newScores) {
				for (int x = 0; x < scores.Length; x++) {
					if (scores[x].userID == score.userID) {
						if (score.value > scores[x].value) {
							scores[x] = score;
						}

						break;
					}
				}
			}
		} else {
			foreach (var score in newScores) {
				updatedScores.Add(score);
			}

			scores = updatedScores.ToArray();
		}
	}

	#endregion
}
