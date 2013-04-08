using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

public partial class LumosSocialPlatform : ISocialPlatform {


	void FetchUsers(string[] userIds, Action<IUserProfile[]> callback)
	{
		var api = url + "/users";
		
		var parameters = new Dictionary<string, object>() {
			{ "usernames", userIds }
		};
		
		LumosRequest.Send(api, parameters, delegate {
			var response = LumosRequest.lastResponse as Dictionary<string, object>;
			var users = ParseUsers(response);
			callback(users);
		});
	}

	IUserProfile[] ParseUsers(Dictionary<string, object> info)
	{
		var users = new List<IUserProfile>();

		foreach (var i in info) {
			var user = i.Value as Hashtable;
			var id = info["username"] as string;
			string name = null;

			if (user.ContainsKey("name")) {
				name = user["name"].ToString();
			}

			var u = new UserProfile(name, id, false);
			users.Add(u);
		}

		return users.ToArray();
	}
}
