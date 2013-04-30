using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

/// <summary>
/// Lumos user.
/// </summary>
public class LumosUser : ILocalUser
{
	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="LumosUser"/> is authenticated.
	/// </summary>
	/// <value>
	/// <c>true</c> if authenticated; otherwise, <c>false</c>.
	/// </value>
	public bool authenticated { get; private set; }
	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="LumosUser"/> is friend.
	/// </summary>
	/// <value>
	/// <c>true</c> if is friend; otherwise, <c>false</c>.
	/// </value>
	public bool isFriend { get; set; }
	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="LumosUser"/> is underage.
	/// </summary>
	/// <value>
	/// <c>true</c> if underage; otherwise, <c>false</c>.
	/// </value>
	public bool underage { get; set; }
	/// <summary>
	/// Gets or sets the identifier.
	/// </summary>
	/// <value>
	/// The identifier.
	/// </value>
	public string id {
		get { return userID; }
		set { userID = value; }
	}
	/// <summary>
	/// Gets or sets the user I.
	/// </summary>
	/// <value>
	/// The user I.
	/// </value>
	public string userID { get; set; }
	/// <summary>
	/// Gets or sets the name of the user.
	/// </summary>
	/// <value>
	/// The name of the user.
	/// </value>
	public string userName { get; set; }
	/// <summary>
	/// Gets or sets the state.
	/// </summary>
	/// <value>
	/// The state.
	/// </value>
	public UserState state { get; set; }
	/// <summary>
	/// Gets or sets the image.
	/// </summary>
	/// <value>
	/// The image.
	/// </value>
	public Texture2D image { get; set; }
	/// <summary>
	/// Gets or sets the friends.
	/// </summary>
	/// <value>
	/// The friends.
	/// </value>
	public IUserProfile[] friends { get; private set; }
	/// <summary>
	/// Gets or sets the friend requests.
	/// </summary>
	/// <value>
	/// The friend requests.
	/// </value>
	public IUserProfile[] friendRequests { get; private set; }
	/// <summary>
	/// Gets or sets the scores.
	/// </summary>
	/// <value>
	/// The scores.
	/// </value>
	public Score[] scores { get; private set; }
	/// <summary>
	/// The email.
	/// </summary>
	public string email;
	/// <summary>
	/// Gets or sets the other.
	/// </summary>
	/// <value>
	/// The other.
	/// </value>
	public Dictionary<string, object> other { get; set; }
	
	/// <summary>
	/// The URL.
	/// </summary>
	string url = "http://localhost:8888/api/1/users";
	
	/// <summary>
	/// Initializes a new instance of the <see cref="LumosUser"/> class.
	/// </summary>
	public LumosUser () {}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="LumosUser"/> class.
	/// </summary>
	/// <param name='userID'>
	/// User I.
	/// </param>
	/// <param name='authenticated'>
	/// Authenticated.
	/// </param>
	public LumosUser(string userID, bool authenticated)
	{
		this.userID = userID;
		this.authenticated = authenticated;
	}
	
	/// <summary>
	/// Authenticate the specified callback.
	/// </summary>
	/// <param name='callback'>
	/// Callback.
	/// </param>
	public void Authenticate(Action<bool> callback)
	{
		AuthenticateUser(null, callback);
	}
	
	/// <summary>
	/// Authenticate the specified username, password and callback.
	/// </summary>
	/// <param name='username'>
	/// Username.
	/// </param>
	/// <param name='password'>
	/// Password.
	/// </param>
	/// <param name='callback'>
	/// Callback.
	/// </param>
	public void Authenticate(string username, string password, Action<bool> callback)
	{
		this.userID = username;
		AuthenticateUser(password, callback);
	}
	
	/// <summary>
	/// Updates the info.
	/// </summary>
	/// <param name='userName'>
	/// User name.
	/// </param>
	/// <param name='email'>
	/// Email.
	/// </param>
	/// <param name='password'>
	/// Password.
	/// </param>
	/// <param name='other'>
	/// Other.
	/// </param>
	/// <param name='callback'>
	/// Callback.
	/// </param>
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
	
	/// <summary>
	/// Loads the friends.
	/// </summary>
	/// <param name='callback'>
	/// Callback.
	/// </param>
	public void LoadFriends(Action<bool> callback)
	{
		FetchFriends(callback);
	}
	
	/// <summary>
	/// Authenticates the user.
	/// </summary>
	/// <param name='password'>
	/// Password.
	/// </param>
	/// <param name='callback'>
	/// Callback.
	/// </param>
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
	
	/// <summary>
	/// Register the specified username, pass, email and callback.
	/// </summary>
	/// <param name='username'>
	/// Username.
	/// </param>
	/// <param name='pass'>
	/// Pass.
	/// </param>
	/// <param name='email'>
	/// Email.
	/// </param>
	/// <param name='callback'>
	/// Callback.
	/// </param>
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
	
	/// <summary>
	/// Loads the friend requests.
	/// </summary>
	/// <param name='callback'>
	/// Callback.
	/// </param>
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
	
	/// <summary>
	/// Sends the friend request.
	/// </summary>
	/// <param name='friendID'>
	/// Friend I.
	/// </param>
	/// <param name='callback'>
	/// Callback.
	/// </param>
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
	
	/// <summary>
	/// Accepts the friend request.
	/// </summary>
	/// <param name='friendID'>
	/// Friend I.
	/// </param>
	/// <param name='callback'>
	/// Callback.
	/// </param>
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
	
	/// <summary>
	/// Declines the friend request.
	/// </summary>
	/// <param name='friendID'>
	/// Friend I.
	/// </param>
	/// <param name='callback'>
	/// Callback.
	/// </param>
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
	
	/// <summary>
	/// Removes the friend.
	/// </summary>
	/// <param name='friendID'>
	/// Friend I.
	/// </param>
	/// <param name='callback'>
	/// Callback.
	/// </param>
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
	
	/// <summary>
	/// Loads the friend leaderboard scores.
	/// </summary>
	/// <param name='callback'>
	/// Callback.
	/// </param>
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
	
	/// <summary>
	/// Fetchs the friends.
	/// </summary>
	/// <param name='callback'>
	/// Callback.
	/// </param>
	void FetchFriends(Action<bool> callback)
	{
		var endpoint = url + "/" + userID + "/friends?method=GET";

		LumosRequest.Send(endpoint, delegate (object response) {
			var resp = response as IList;
			friends = ParseFriends(resp);
			callback(true);
		});
	}
	
	/// <summary>
	/// Updates the user.
	/// </summary>
	/// <param name='info'>
	/// Info.
	/// </param>
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
	
	/// <summary>
	/// Parses the friends.
	/// </summary>
	/// <returns>
	/// The friends.
	/// </returns>
	/// <param name='friends'>
	/// Friends.
	/// </param>
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
	
	/// <summary>
	/// Parses the user score.
	/// </summary>
	/// <returns>
	/// The user score.
	/// </returns>
	/// <param name='scores'>
	/// Scores.
	/// </param>
	/// <param name='leaderboardID'>
	/// Leaderboard I.
	/// </param>
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
	
	/// <summary>
	/// Adds the string parameter.
	/// </summary>
	/// <param name='key'>
	/// Key.
	/// </param>
	/// <param name='value'>
	/// Value.
	/// </param>
	/// <param name='parameters'>
	/// Parameters.
	/// </param>
	void AddStringParam(string key, string value, Dictionary<string, object> parameters)
	{
		if (value != null && value != "") {
			parameters[key] = value;
		}
	}
}
