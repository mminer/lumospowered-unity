using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

public class LumosUser : ILocalUser
{
	public bool authenticated { get; private set; }
	public bool isFriend { get; set; }
	public bool underage { get; set; }
	public string id {
		get { return userID; }
		set { userID = value; }
	}
	public string userID { get; set; }
	public string userName { get; set; }
	public UserState state { get; set; }
	public Texture2D image { get; set; }
	public IUserProfile[] friends { get; private set; }
	public IUserProfile[] friendRequests { get; private set; }
	public Score[] scores { get; private set; }
	public string email;
	public Dictionary<string, object> other { get; set; }

	string url = "http://localhost:8888/api/1/games/" + Lumos.gameId + "/users";

	public LumosUser () {}

	public LumosUser(string userID, bool authenticated)
	{
		this.userID = userID;
		this.authenticated = authenticated;
	}

	public void Authenticate(Action<bool> callback)
	{
		AuthenticateUser(null, callback);
	}

	public void Authenticate(string username, string password, Action<bool> callback)
	{
		this.userID = username;
		AuthenticateUser(password, callback);
	}

	public void UpdateInfo(string userName=null, string email=null, string password=null, Dictionary<string, object> other=null, Action<bool> callback=null)
	{
		var endpoint = url + "/" + userID + "?method=PUT";

		var parameters = new Dictionary<string, object>();
		AddStringParam("name", userName, parameters);
		AddStringParam("email", email, parameters);
		AddStringParam("password", password, parameters);

		if (other != null) {
			var json = LumosJson.Serialize(this.other);
			parameters["other"] = json;
		}

		LumosRequest.Send(endpoint, parameters, delegate (object response) {
			var resp = response as Dictionary<string, object>;
			UpdateUser(resp);

			if (callback != null) {
				callback(true);
			}
		});
	}

	public void LoadFriends(Action<bool> callback)
	{
		FetchFriends(callback);
	}

	void AuthenticateUser(string password, Action<bool> callback)
	{
		// The id should be set prior to this call if
		// the developer intends to use a login system
		if (userID == null) {
			userID = Lumos.playerId;
		}

		var endpoint = url + "/" + userID + "?method=GET";

		// need to get the password some how
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
			delegate (object response) { // Success
				var resp = response as Dictionary<string, object>;
				UpdateUser(resp);
				authenticated = true;
				callback(true);
			}, 
		
			delegate { // Fail
				callback(false);
			});
	}

	public void Register(string username, string pass, string email, Action<bool> callback)
	{
		var endpoint = url + "/" + username + "?method=PUT";

		var parameters = new Dictionary<string, object>() {
			{ "player_id", Lumos.playerId },
			{ "password", pass },
			{ "email", email }
		};

		LumosRequest.Send(endpoint, parameters, delegate {
			this.email = email;
			this.userID = username;
			this.authenticated = true;
			callback(true);
		});
	}
	
	public void LoadFriendRequests(Action<bool> callback)
	{
		var endpoint = url + "/" + userID + "/friend-requests?method=GET";

		LumosRequest.Send(endpoint, delegate (object response) {
			if (response != null) {
				var resp = response as IList;
				friendRequests = ParseFriends(resp);
			}

			callback(true);
		});
	}

	public void SendFriendRequest(string friendID, Action<bool> callback)
	{
		var endpoint = url + "/" + userID + "/friend-requests";

		var parameters = new Dictionary<string, object>() {
			{ "friend", friendID }
		};

		LumosRequest.Send(endpoint, parameters, delegate {
			callback(true);
		});
	}

	public void AcceptFriendRequest(string friendID, Action<bool> callback)
	{
		var endpoint = url + "/" + userID + "/friends";

		var parameters = new Dictionary<string, object>() {
			{ "friend", friendID }
		};

		LumosRequest.Send(endpoint, parameters, delegate (object response) {
			var resp = response as Dictionary<string, object>;
			friends = ParseFriends(resp["friends"] as IList);
			friendRequests = ParseFriends(resp["friend_requests"] as IList);
			callback(true);
		});
	}

	public void DeclineFriendRequest(string friendID, Action<bool> callback)
	{
		var endpoint = url + "/" + userID + "/friend-requests";

		var parameters = new Dictionary<string, object>() {
			{ "friend", friendID },
			{ "decline", true }
		};

		LumosRequest.Send(endpoint, parameters, delegate (object response) {
			var resp = response as Dictionary<string, object>;

			if (resp.ContainsKey("friend_requests")) {
				friendRequests = ParseFriends(resp["friend_requests"] as IList);
			}

			callback(true);
		});
	}

	public void RemoveFriend(string friendID, Action<bool> callback)
	{
		var endpoint = url + "/" + userID + "/friends?method=DELETE";

		var parameters = new Dictionary<string, object>() {
			{ "friend", friendID }
		};

		LumosRequest.Send(endpoint, parameters, delegate (object response) {
			var resp = response as IList;
			friends = ParseFriends(resp);
			callback(true);
		});
	}

	public void LoadFriendLeaderboardScores(Action<bool> callback)
	{
		var endpoint = "localhost:8888/api/1/games/" + Lumos.gameId + "/leaderboards/" + userID + "/friends?method=GET";

		LumosRequest.Send(endpoint, delegate (object response) {
			var resp = response as IList;
			var scores = new List<Score>();

			foreach (Dictionary<string, object> leaderboard in resp) {
				var rawScores = leaderboard["scores"] as IList;
				var leaderboardID = leaderboard["leaderboard_id"] as string;
				var score = ParseUserScore(rawScores, leaderboardID);

				if (score != null) {
					scores.Add(score);
				}
			}

			this.scores = scores.ToArray();
			callback(true);
		});
	}

	void FetchFriends(Action<bool> callback)
	{
		var endpoint = url + "/" + userID + "/friends?method=GET";

		LumosRequest.Send(endpoint, delegate (object response) {
			var resp = response as IList;
			friends = ParseFriends(resp);
			callback(true);
		});
	}

	void UpdateUser(Dictionary<string, object> info)
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

	IUserProfile[] ParseFriends(IList friends)
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

	Score ParseUserScore(IList scores, string leaderboardID)
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

	void AddStringParam(string key, string value, Dictionary<string, object> parameters)
	{
		if (value != null && value != "") {
			parameters[key] = value;
		}
	}
}