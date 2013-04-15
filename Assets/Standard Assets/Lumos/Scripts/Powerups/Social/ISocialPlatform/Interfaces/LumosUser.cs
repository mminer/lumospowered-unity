using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

public class LumosUser : ILocalUser {
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
	public string email;
	
	string url = "localhost:8888/api/1/games/" + Lumos.gameId + "/users";


	public LumosUser()
	{

	}

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
	
	public void LoadFriends(Action<bool> callback)
	{
		FetchFriends(callback);
	}
	
	void AuthenticateUser(string password, Action<bool> callback)
	{
		// The id should be set prior to this call if 
		// the developer intends to use a login system
		if (userID == null) {
			userID = LumosCore.playerId;
		}

		var api = url + "/" + userID + "?method=GET";
		
		// need to get the password some how
		// but can't add it to the localUser interface
		// May need to create a new type of interface?
		if (password == null) {
			password = "default";
		}
		
		var parameters = new Dictionary<string, object>() {
			{ "player_id", LumosCore.playerId },
			{ "password", password }
		};

		LumosRequest.Send(api, parameters, delegate {
			var response = LumosRequest.lastResponse as Dictionary<string, object>;
			UpdateUser(response);
			authenticated = true;
			callback(true);
		});
	}
	
	public void Register(string username, string pass, string email, Action<bool> callback)
	{
		var api = url + "/" + username + "?method=PUT";
		
		var parameters = new Dictionary<string, object>() {
			{ "player_id", LumosCore.playerId },
			{ "password", pass },
			{ "email", email }
		};

		LumosRequest.Send(api, parameters, delegate {
			this.email = email;
			this.userID = username;
			this.authenticated = true;
			callback(true);
		});
	}
	
	public void LoadFriendRequests(Action<bool> callback)
	{
		var api = url + "/" + userID + "/friend-requests?method=GET";
		
		LumosRequest.Send(api, delegate {
			var response = LumosRequest.lastResponse as IList;
			
			if (response != null) {
				friendRequests = ParseFriends(response);	
			}
			
			callback(true);
		});
	}
	
	public void SendFriendRequest(string friendID, Action<bool> callback)
	{
		var api = url + "/" + userID + "/friend-requests";
		
		var parameters = new Dictionary<string, object>() {
			{ "friend", friendID }
		};
		
		LumosRequest.Send(api, parameters, delegate {
			callback(true);
		});
	}
	
	public void AcceptFriendRequest(string friendID, Action<bool> callback)
	{
		var api = url + "/" + userID + "/friends";
		
		var parameters = new Dictionary<string, object>() {
			{ "friend", friendID }
		};
		
		LumosRequest.Send(api, parameters, delegate {
			var response = LumosRequest.lastResponse as Dictionary<string, object>;
			friends = ParseFriends(response["friends"] as IList);
			friendRequests = ParseFriends(response["friend_requests"] as IList);
			callback(true);
		});
	}
	
	public void DeclineFriendRequest(string friendID, Action<bool> callback)
	{
		var api = url + "/" + userID + "/friend-requests";
		
		var parameters = new Dictionary<string, object>() {
			{ "friend", friendID },
			{ "decline", true }
		};
		
		LumosRequest.Send(api, parameters, delegate {
			var response = LumosRequest.lastResponse as Dictionary<string, object>;
			
			if (response.ContainsKey("friend_requests")) {
				friendRequests = ParseFriends(response["friend_requests"] as IList);				
			}

			callback(true);
		});
	}
	
	public void RemoveFriend(string friendID, Action<bool> callback)
	{
		var api = url + "/" + userID + "/friends?method=DELETE";
		
		var parameters = new Dictionary<string, object>() {
			{ "friend", friendID }
		};
		
		LumosRequest.Send(api, parameters, delegate {
			var response = LumosRequest.lastResponse as IList;
			friends = ParseFriends(response);	
			callback(true);
		});
	}
	
	void FetchFriends(Action<bool> callback)
	{
		var api = url + "/" + userID + "/friends?method=GET";
		
		LumosRequest.Send(api, delegate {
			var response = LumosRequest.lastResponse as IList;
			friends = ParseFriends(response);
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
		}
		
		if (info.ContainsKey("email")) {
			email = info["email"].ToString();
		}
		
		if (info.ContainsKey("image")) {
			var imageURL = info["image"].ToString();
			// load in image?
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
}