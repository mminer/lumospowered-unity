// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

/// <summary>
/// Holds information about the current player.
/// </summary>
public class LumosUser : LumosUserProfile, ILocalUser
{
	// Friends of the user.
	public IUserProfile[] friends { get; private set; }

	// Indicated whether this user is authenticated.
	public bool authenticated { get; set; }

	// Indicated whether this user's age is below a threshold.
	public bool underage { get; set; }

	/// <summary>
	/// The user's password.
	/// </summary>
	public string password { get; set; }

	/// <summary>
	/// Friend requests.
	/// </summary>
	public IUserProfile[] friendRequests { get; private set; }

	/// <summary>
	/// Scores the user has earned.
	/// </summary>
	public Score[] scores { get; private set; }

	/// <summary>
	/// The user's email address.
	/// </summary>
	public string email { get; set; }

	/// <summary>
	/// Additional information about the user.
	/// </summary>
	public Dictionary<string, object> other { get; set; }

	// Constructor. Creates a blank user object.
	public LumosUser () {}

	/// <summary>
	/// Creates a user object with ID and password.
	/// </summary>
	/// <param name="userID">Username.</param>
	/// <param name="password">The user's password.</param>
	public LumosUser (string userID, string password)
	{
		this.userID = userID;
		this.password = password;
	}

	// Constructor. Creates a user object with the given info.
	public LumosUser (Dictionary<string, object> info) : base (info)
	{
		this.email = info["email"] as string;
	}

	// Authenticate the user.
	public void Authenticate (Action<bool> callback)
	{
		// ID should be set prior to this call if login system is intended.
		if (userID == null) {
			userID = Lumos.playerId;
		}

		var endpoint = LumosSocial.baseUrl + "/users/" + userID + "?method=GET";

		if (password == null) {
			password = "default";
		}

		var payload = new Dictionary<string, object>() {
			{ "player_id", Lumos.playerId },
			{ "password", password }
		};

		LumosRequest.Send(endpoint, payload,
			success => {
				var resp = success as Dictionary<string, object>;
				authenticated = true;
				(Social.Active as LumosSocial).SetLocalUser(this);
				Update(resp);

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

	// Loads the user's friends list.
	public void LoadFriends (Action<bool> callback)
	{
		var endpoint = LumosSocial.baseUrl + "/users/" + userID + "/friends?method=GET";

		LumosRequest.Send(endpoint,
			success => {
				var resp = success as IList;
				friends = ParseFriends(resp);

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

	#region Added Functions

	/// <summary>
	/// Loads the friend requests.
	/// </summary>
	/// <param name="callback">Callback.</param>
	public void LoadFriendRequests (Action<bool> callback)
	{
		var endpoint = LumosSocial.baseUrl + "/users/" + userID + "/friend-requests?method=GET";

		LumosRequest.Send(endpoint,
			success => {
				if (success != null) {
					var resp = success as IList;
					friendRequests = ParseFriends(resp);
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

	/// <summary>
	/// Sends a friend request.
	/// </summary>
	/// <param name="friendID">The friend's username.</param>
	/// <param name="callback">Callback.</param>
	public void SendFriendRequest (string friendID, Action<bool> callback)
	{
		var endpoint = LumosSocial.baseUrl + "/users/" + userID + "/friend-requests";

		var payload = new Dictionary<string, object>() {
			{ "friend", friendID }
		};

		LumosRequest.Send(endpoint, payload,
			success => {
				if (callback != null) {
					callback(false);
				}
			},
			error => {
				if (callback != null) {
					callback(false);
				}
			});
	}

	/// <summary>
	/// Accepts a friend request.
	/// </summary>
	/// <param name="friendID">The friend's username.</param>
	/// <param name="callback">Callback.</param>
	public void AcceptFriendRequest (string friendID, Action<bool> callback)
	{
		var endpoint = LumosSocial.baseUrl + "/users/" + userID + "/friends/" + friendID + "?method=PUT";

		LumosRequest.Send(endpoint,
			success => {
				var resp = success as Dictionary<string, object>;
				friends = ParseFriends(resp["friends"] as IList);
				friendRequests = ParseFriends(resp["friend_requests"] as IList);

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
	/// Declines a friend request.
	/// </summary>
	/// <param name="friendID">The friend's username.</param>
	/// <param name="callback">Callback.</param>
	public void DeclineFriendRequest (string friendID, Action<bool> callback)
	{
		var endpoint = LumosSocial.baseUrl + "/users/" + userID + "/friend-requests";
		var payload = new Dictionary<string, object>() {
			{ "friend", friendID },
			{ "decline", true }
		};

		LumosRequest.Send(endpoint, payload,
			success => {
				var resp = success as Dictionary<string, object>;

				if (resp.ContainsKey("friend_requests")) {
					friendRequests = ParseFriends(resp["friend_requests"] as IList);
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

	/// <summary>
	/// Removes a friend.
	/// </summary>
	/// <param name="friendID">The friend's username.</param>
	/// <param name="callback">Callback.</param>
	public void RemoveFriend (string friendID, Action<bool> callback)
	{
		var endpoint = LumosSocial.baseUrl + "/users/" + userID + "/friends/" + friendID + "?method=DELETE";

		LumosRequest.Send(endpoint,
			success => {
				var resp = success as IList;
				friends = ParseFriends(resp);

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
	/// Loads a leaderboard with only friend scores.
	/// </summary>
	/// <param name="callback">Callback.</param>
	public void LoadFriendLeaderboardScores (Action<bool> callback)
	{
		var endpoint = LumosSocial.baseUrl + "/users/" + id + "/friends/scores?method=GET";

		LumosRequest.Send(endpoint,
			success => {
				var resp = success as IList;
				var leaderboards = new LumosLeaderboard[resp.Count];

				for (int i = 0; i < resp.Count; i++) {
					leaderboards[i] = new LumosLeaderboard(resp[i] as Dictionary<string, object>);
				}

				foreach (var leaderboard in leaderboards) {
					var current = LumosSocial.GetLeaderboard(leaderboard.id);

					// Leaderboard already exists; update friend scores only.
					if (current != null) {
						current.friendScores = leaderboard.friendScores;
					}
					// Leaderboard doesn't exist yet; add entire leaderboard.
					else {
						LumosSocial.AddLeaderboard(leaderboard);
					}
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

	/// <summary>
	/// Updates the user's info.
	/// </summary>
	/// <param name="name">The user's name.</param>
	/// <param name="email">The user's email address.</param>
	/// <param name="password">Password.</param>
	/// <param name="other">Additional information.</param>
	/// <param name="callback">Callback.</param>
	public void UpdateInfo (string name=null, string email=null, string password=null, Dictionary<string, object> other=null, Action<bool> callback=null)
	{
		var endpoint = LumosSocial.baseUrl + "/users/" + userID + "?method=PUT";

		var payload = new Dictionary<string, object>();
		LumosUtil.AddToDictionaryIfNonempty(payload, "name", name);
		LumosUtil.AddToDictionaryIfNonempty(payload, "email", email);
		LumosUtil.AddToDictionaryIfNonempty(payload, "password", password);

		if (other != null) {
			payload["other"] = LumosJson.Serialize(other);
		}

		LumosRequest.Send(endpoint, payload,
			success => {
				var resp = success as Dictionary<string, object>;
				Update(resp);

				if (callback != null) {
					callback(true);
				}
			},
			error => {
				if (callback != null) {
					callback(true);
				}
			});
	}

	// Updates the user.
	public void Update (Dictionary<string, object> info)
	{
		if (info.ContainsKey("username")) {
			userID = info["username"] as string;
		}

		if (info.ContainsKey("name")) {
			userName = info["name"] as string;
		}

		if (info.ContainsKey("underage")) {
			underage = (bool)info["underage"];
		}

		if (info.ContainsKey("email")) {
			email = info["email"] as string;
		}

		// Load avatar from remote server.
		if (info.ContainsKey("image")) {
			var imageLocation = info["image"] as string;
			LumosRequest.LoadImage(imageLocation, image);
		}

		if (info.ContainsKey("other")) {
			other = LumosJson.Deserialize(info["other"] as string) as Dictionary<string, object>;
		}
	}

	/// <summary>
	/// Parses the friends list.
	/// </summary>
	/// <param name="friends">Friend usernames to grab.</param>
	/// <returns>Array of user profiles.</returns>
	IUserProfile[] ParseFriends (IList friends)
	{
		IUserProfile[] friendList;

		if (friends == null) {
			friendList = new IUserProfile[0];
		} else {
			friendList = new IUserProfile[friends.Count];

			for (int i = 0; i < friends.Count; i++) {
				var friend = friends[i] as Dictionary<string, object>;
				var id = friend["username"] as string;
				string name = null;

				if (friend.ContainsKey("name")) {
					name = friend["name"].ToString();
				}

				friendList[i] = new UserProfile(name, id, true);
			}
		}

		return friendList;
	}

	/// <summary>
	/// Parses the user's score.
	/// </summary>
	/// <param name="scores">Scores.</param>
	/// <param name="leaderboardID">The leaderboard identifier.</param>
	/// <returns>The score.</returns>
	Score ParseUserScore (IList scores, string leaderboardID)
	{
		Score score = null;

		foreach (Dictionary<string, object> info in scores) {
			var username = info["username"] as string;

			if (username == userID) {
				var val = Convert.ToInt32(info["score"]);
				var rank = Convert.ToInt32(info["rank"]);
				var timestamp = Convert.ToDouble(info["created"]);
				var date = LumosUtil.UnixTimestampToDateTime(timestamp);
				var formattedValue = ""; // Lumos doesn't support this
				score = new Score(leaderboardID, val, username, date, formattedValue, rank);
				break;
			}
		}

		return score;
	}

	#endregion
}
