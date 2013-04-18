using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

public partial class LumosSocialPlatform : ISocialPlatform {


	void RegisterUser(string username, string password, string email, Action<bool> callback)
	{
		var api = url + "/users/" + username + "?method=PUT";

		var parameters = new Dictionary<string, object>() {
			{ "player_id", Lumos.playerId },
			{ "password", password },
			{ "email", email }
		};

		LumosRequest.Send(api, parameters, delegate (object response) {
			var resp = response as Hashtable;
			var user = ParseLumosUser(resp);
			_localUser = user;

			callback(true);
		});
	}

	void FetchUsers(string[] userIds, Action<IUserProfile[]> callback)
	{
		var api = url + "/users";

		var parameters = new Dictionary<string, object>() {
			{ "usernames", userIds }
		};

		LumosRequest.Send(api, parameters, delegate (object response) {
			var resp = response as Dictionary<string, object>;
			var users = ParseUsers(resp);
			callback(users);
		});
	}

	IUserProfile[] ParseUsers(Dictionary<string, object> info)
	{
		var users = new List<IUserProfile>();

		foreach (var i in info) {
			var user = i.Value as Hashtable;
			var userProfile = ParseUser(user);
			users.Add(userProfile);
		}

		return users.ToArray();
	}

	IUserProfile ParseUser(Hashtable user)
	{
		var id = user["username"] as string;
		string name = null;

		if (user.ContainsKey("name")) {
			name = user["name"].ToString();
		}

		var userProfile = new UserProfile(name, id, false);

		return userProfile;
	}

	LumosUser ParseLumosUser(Hashtable info)
	{
		var userID = info["username"] as string;
		var user = new LumosUser(userID, true);

		if (info.ContainsKey("name")) {
			user.userName = info["name"].ToString();
		}

		return user;
	}
}
