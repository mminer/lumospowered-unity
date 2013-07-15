// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

public partial class LumosSocialPlatform : ISocialPlatform
{
	void RegisterUser(string username, string password, string email, Action<bool> callback)
	{
		var endpoint = LumosSocial.baseUrl + "/users/" + username + "?method=PUT";

		var payload = new Dictionary<string, object>() {
			{ "player_id", Lumos.playerId },
			{ "password", password },
			{ "email", email }
		};

		LumosRequest.Send(endpoint, payload,
			success => {
				var resp = success as Hashtable;
				var user = ParseLumosUser(resp);
				_localUser = user;

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

	void FetchUsers(string[] userIds, Action<IUserProfile[]> callback)
	{
		var endpoint = LumosSocial.baseUrl + "/users?method=GET";

		var payload = new Dictionary<string, object>() {
			{ "usernames", userIds }
		};

		LumosRequest.Send(endpoint, payload,
			success => {
				var resp = success as Dictionary<string, object>;
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
