// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

/// <summary>
/// Holds information about a user.
/// </summary>
public class LumosUser : ILocalUser
{
	/// <summary>
	/// A unique identifier for this user.
	/// </summary>
	public string id {
		get { return userID; }
		set { userID = value; }
	}

	/// <summary>
	/// A unique identifier for this user.
	/// </summary>
	public string userID { get; set; }

	/// <summary>
	/// Indicated whether this user is authenticated.
	/// </summary>
	public bool authenticated { get; private set; }

	/// <summary>
	/// Indicates whether this user is a friend of the current player.
	/// </summary>
	public bool isFriend { get; set; }

	/// <summary>
	/// Indicated whether this user's age is below a threshold.
	/// </summary>
	public bool underage { get; set; }

	/// <summary>
	/// The user's name.
	/// </summary>
	public string userName { get; set; }

	/// <summary>
	/// The user's state.
	/// </summary>
	public UserState state { get; set; }

	/// <summary>
	/// An avatar representing the user.
	/// </summary>
	public Texture2D image { get; set; }

	/// <summary>
	/// Friends of the user.
	/// </summary>
	public IUserProfile[] friends { get; private set; }

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
	public string email;

	/// <summary>
	/// Additional information about the user.
	/// </summary>
	public Dictionary<string, object> other { get; set; }

	/// <summary>
	/// Constructor. Creates a blank user object.
	/// </summary>
	public LumosUser () {}

	/// <summary>
	/// Constructor. Creates a user object with ID and authentication status.
	/// </summary>
	/// <param name="userID">Username.</param>
	/// <param name="authenticated">Whether this user is authenticated.</param>
	public LumosUser (string userID, bool authenticated)
	{
		this.userID = userID;
		this.authenticated = authenticated;
	}

	/// <summary>
	/// Authenticate the user.
	/// </summary>
	/// <param name="callback">Callback triggers on success.</param>
	public void Authenticate (Action<bool> callback)
	{
		AuthenticateUser(null, callback);
	}

	/// <summary>
	/// Authenticate the user with the specified username and password.
	/// </summary>
	/// <param name="userID">The username.</param>
	/// <param name="password">The user's password.</param>
	/// <param name="callback">Callback triggers on success.</param>
	public void Authenticate (string userID, string password, Action<bool> callback)
	{
		this.userID = userID;
		AuthenticateUser(password, callback);
	}

	/// <summary>
	/// Updates the user's info.
	/// </summary>
	/// <param name="name">The user's name.</param>
	/// <param name="email">The user's email address.</param>
	/// <param name="password">Password.</param>
	/// <param name="other">Additional information.</param>
	/// <param name="callback">Callback triggers on success.</param>
	public void UpdateInfo (string name=null, string email=null, string password=null, Dictionary<string, object> other=null, Action<bool> callback=null)
	{
		var endpoint = LumosSocial.baseUrl + "/users/" + userID + "?method=PUT";

		// TODO: Combine into one initializer?
		var payload = new Dictionary<string, object>();
		LumosUtil.AddToDictionaryIfNonempty(payload, "name", name);
		LumosUtil.AddToDictionaryIfNonempty(payload, "email", email);
		LumosUtil.AddToDictionaryIfNonempty(payload, "password", password);

		if (other != null) {
			var json = LumosJson.Serialize(this.other);
			payload["other"] = json;
		}

		LumosRequest.Send(endpoint, payload,
			success => {
				var resp = success as Dictionary<string, object>;
				UpdateUser(resp);

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

	/// <summary>
	/// Loads the user's friends list.
	/// </summary>
	/// <param name="callback">Callback triggers on success.</param>
	public void LoadFriends (Action<bool> callback)
	{
		FetchFriends(callback);
	}

	/// <summary>
	/// Authenticates the user.
	/// </summary>
	/// <param name="password">Password.</param>
	/// <param name="callback">Callback triggers on success.</param>
	void AuthenticateUser (string password, Action<bool> callback)
	{
		// ID should be set prior to this call if login system is intended.
		if (userID == null) {
			userID = Lumos.playerId;
		}

		var endpoint = LumosSocial.baseUrl + "/users/" + userID + "?method=GET";

		// TODO: need to get the password some how
		// but can't add it to the localUser interface
		// May need to create a new type of interface?
		if (password == null) {
			password = "default";
		}

		var parameters = new Dictionary<string, object>() {
			{ "player_id", Lumos.playerId },
			{ "password", password }
		};

		LumosRequest.Send(endpoint, parameters,
			success => {
				var resp = success as Dictionary<string, object>;
				UpdateUser(resp);
				authenticated = true;
				callback(true);
			},
			error => {
				callback(false);
			});
	}

	/// <summary>
	/// Register the specified user.
	/// </summary>
	/// <param name="userID">Username.</param>
	/// <param name="password">Password.</param>
	/// <param name="email">Email address.</param>
	/// <param name="callback">Callback triggers on success.</param>
	public void Register (string userID, string password, string email, Action<bool> callback)
	{
		var endpoint = LumosSocial.baseUrl + "/users/" + userID + "?method=PUT";

		var parameters = new Dictionary<string, object>() {
			{ "player_id", Lumos.playerId },
			{ "password", password },
			{ "email", email }
		};

		LumosRequest.Send(endpoint, parameters,
			success => {
				this.email = email;
				this.userID = userID;
				this.authenticated = true;
				callback(true);
			});
	}

	/// <summary>
	/// Loads the friend requests.
	/// </summary>
	/// <param name="callback">Callback triggers on success.</param>
	public void LoadFriendRequests (Action<bool> callback)
	{
		var endpoint = LumosSocial.baseUrl + "/users/" + userID + "/friend-requests?method=GET";

		LumosRequest.Send(endpoint,
			success => {
				if (success != null) {
					var resp = success as IList;
					friendRequests = ParseFriends(resp);
				}

				callback(true);
			});
	}

	/// <summary>
	/// Sends the friend request.
	/// </summary>
	/// <param name="friendID">The friend's username.</param>
	/// <param name="callback">Callback triggers on success.</param>
	public void SendFriendRequest (string friendID, Action<bool> callback)
	{
		var endpoint = LumosSocial.baseUrl + "/users/" + userID + "/friend-requests";

		var parameters = new Dictionary<string, object>() {
			{ "friend", friendID }
		};

		LumosRequest.Send(endpoint, parameters,
			success => {
				callback(true);
			});
	}

	/// <summary>
	/// Accepts the friend request.
	/// </summary>
	/// <param name="friendID">The friend's username.</param>
	/// <param name="callback">Callback triggers on success.</param>
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
	/// Declines the friend request.
	/// </summary>
	/// <param name="friendID">The friend's username.</param>
	/// <param name="callback">Callback to trigger on success.</param>
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
	/// Removes the friend.
	/// </summary>
	/// <param name="friendID">The friend's username.</param>
	/// <param name="callback">Callback triggers on success.</param>
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
	/// Loads the friend leaderboard scores.
	/// </summary>
	/// <param name="callback">Callback to trigger on success.</param>
	public void LoadFriendLeaderboardScores (Action<bool> callback)
	{
		var api = LumosSocial.baseUrl + "/users/" + id + "/friends/scores?method=GET";

		LumosRequest.Send(api,
			success => {
				var resp = success as IList;
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
						LumosSocial.leaderboards[leaderboard.id] = leaderboard;
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
	/// Fetchs the user's friends.
	/// </summary>
	/// <param name="callback">Callback to trigger on success.</param>
	void FetchFriends (Action<bool> callback)
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

	/// <summary>
	/// Updates the user.
	/// </summary>
	/// <param name="info">Information about the user.</param>
	void UpdateUser (Dictionary<string, object> info)
	{
		userID = info["username"].ToString();

		if (info.ContainsKey("underage")) {
			underage = (bool)info["underage"];
		}

		if (info.ContainsKey("name")) {
			userName = info["name"].ToString();
		} else {
			userName = "";
		}

		if (info.ContainsKey("email")) {
			email = info["email"].ToString();
		}

		if (info.ContainsKey("image")) {
			var imageURL = info["image"].ToString();
			// load in image?
		}

		if (info.ContainsKey("other")) {
			this.other = LumosJson.Deserialize(info["other"] as string) as Dictionary<string, object>;
		}
	}

	/// <summary>
	/// Parses the friends list.
	/// </summary>
	/// <param name="friends">Friend usernames to grab.</param>
	/// <returns>Array of user profiles.</returns>
	IUserProfile[] ParseFriends (IList friends)
	{
		var friendList = new List<IUserProfile>();

		if (friends == null) {
			return new IUserProfile[] {};
		}

		foreach (Dictionary<string, object> friend in friends) {
			var id = friend["username"].ToString();
			string name = null;

			if (friend.ContainsKey("name")) {
				name = friend["name"].ToString();
			}

			var user = new UserProfile(name, id, true);
			friendList.Add(user);
		}

		return friendList.ToArray();
	}

	/// <summary>
	/// Parses the user's score.
	/// </summary>
	/// <param name="scores">Scores.</param>
	/// <param name="leaderboardID">The leaderboard identifier.</param>
	/// <returns>The score.</returns>
	Score ParseUserScore (IList scores, string leaderboardID)
	{
		foreach (Dictionary<string, object> score in scores) {
			var username = score["username"] as  string;

			if (username == userID) {
				var value = Convert.ToInt32(score["score"]);
				var rank = Convert.ToInt32(score["rank"]);
				var timestamp = Convert.ToDouble(score["created"]);
				var date = LumosUtil.UnixTimestampToDateTime(timestamp);
				var formattedValue = ""; // Lumos doesn't support this
				var userScore = new Score(leaderboardID, value, username, date, formattedValue, rank);
				return userScore;
			}
		}

		return null;
	}
}
