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
	public string email;
	// password?
	// email
	
	string url = "localhost:8888/api/1/games/" + Lumos.gameId + "/users";


	public LumosUser()
	{

	}

	public LumosUser(string userID, bool authenticated)
	{
		this.userID = userID;
		this.authenticated = authenticated;
	}
	
	public void Authenticate(Action<bool> callback) {
		AuthenticateUser(callback);
	}
	
	public void LoadFriends(Action<bool> callback) {
		FetchFriends(callback);
	}
	
	void AuthenticateUser(Action<bool> callback)
	{
		// The id should be set prior to this call if 
		// the developer intends to use a login system
		if (userID == null) {
			userID = LumosCore.playerId;
		}

		var api = url + "/" + userID;
		
		// need to get the password some how
		// but can't add it to the localUser interface
		// May need to create a new type of interface?
		var password = "default";
		
		var parameters = new Dictionary<string, object>() {
			{ "player_id", LumosCore.playerId },
			{ "password", password }
		};
		
		LumosRequest.Send(api, parameters, delegate {
			var response = LumosRequest.lastResponse as Hashtable;
			UpdateUser(response);
			callback(true);
		});
	}
	
	void FetchFriends(Action<bool> callback)
	{
		var api = url + "/" + userID + "/friends";
		
		LumosRequest.Send(api, delegate {
			var response = LumosRequest.lastResponse;
			var info = response as ArrayList;
			ParseFriends(info);
			callback(true);
		});
	}
	
	void UpdateUser(Hashtable info)
	{
		userID = info["username"].ToString();
		
		if (info.ContainsKey("underage")) {
			underage = (bool)info["underage"];
		}
		
		if (info.ContainsKey("name")) {
			userName = info["name"].ToString();
		}
		
		if (info.ContainsKey("image")) {
			var imageURL = info["image"].ToString();
			// load in image?
		}
	}
	
	void ParseFriends(ArrayList friends)
	{
		var friendList = new List<IUserProfile>();
		
		foreach (Hashtable friend in friends) {
			var id = friend["username"].ToString();
			string name = null;
			
			if (friend.ContainsKey("name")) {
				name = friend["name"].ToString();
			}
			
			var user = new UserProfile(name, id, true);
			friendList.Add(user);
		}
		
		this.friends = friendList.ToArray();
	}
}