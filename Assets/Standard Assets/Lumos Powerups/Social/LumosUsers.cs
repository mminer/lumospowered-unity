// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

public partial class LumosSocial : ISocialPlatform
{
	LumosUser _localUser = new LumosUser();

	/// <summary>
	/// The local user.
	/// </summary>
	public LumosUser localUser {
		get { return _localUser; }
		set { _localUser = value; }
	}

	/// <summary>
	/// Loads the specified users.
	/// </summary>
	/// <param name="userIDs">Usernames to fetch.</param>
	/// <param name="callback">Callback.</param>
	public void LoadUsers(string[] userIDs, Action<IUserProfile[]> callback)
	{
		var endpoint = baseUrl + "/users?method=GET";
		var payload = new Dictionary<string, object>() {
			{ "usernames", userIDs }
		};

		LumosRequest.Send(endpoint, payload,
			success => {
				var resp = success as Dictionary<string, object>;
				// TODO: user User constructor instead
				var users = ParseUsers(resp);

				if (callback != null) {
					callback(users);
				}
			},
			error => {
				if (callback != null) {
					callback(null);
				}
			});
	}







	/// <summary>
	/// Register the specified username, pass, email and callback.
	/// </summary>
	/// <param name="username">Username.</param>
	/// <param name="password">Password.</param>
	/// <param name="email">The user's email address.</param>
	/// <param name="callback">
	/// Callback.
	/// </param>
	public static void Register (string username, string password, string email, Action<bool> callback)
	{
		Init();
		localUser.Register(username, password, email, callback);
	}

	/// <summary>
	/// Connect the specified user.
	/// </summary>
	/// <param name="username">Username.</param>
	/// <param name="password">Password.</param>
	/// <param name="callback">Callback triggers on success.</param>
	public static void Connect (string username=null, string password=null, Action<bool> callback=null)
	{
        // This call needs to be made before we can proceed to other calls in the Social API
		Init();

		if (username != null) {
			localUser.Authenticate (username, password, callback);
		} else {
			localUser.Authenticate(ProcessAuthentication);
		}
    }

	/// <summary>
	/// Authenticate the specified user and callback.
	/// </summary>
	/// <param name="user">User.</param>
	/// <param name="callback">Callback.</param>
	public void Authenticate(ILocalUser user, Action<bool> callback)
	{
		user.Authenticate(callback);
	}

	/// <summary>
	/// Loads the friends.
	/// </summary>
	/// <param name="user">User.</param>
	/// <param name="callback">Callback.</param>
	public void LoadFriends(ILocalUser user, Action<bool> callback)
	{
		user.LoadFriends(callback);
	}


	/// <summary>
	/// Sends a forgot password request.
	/// </summary>
	/// <param name="username">Username.</param>
	/// <param name="callback">Callback.</param>
	public static void ForgotPassword (string username, Action<bool> callback)
	{
		if (platform == null) {
			Init();
		}

		var endpoint = LumosSocial.baseUrl + "/users/" + username + "/password";

		LumosRequest.Send(endpoint,
			success => {
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
	/// Shows the profile interface.
	/// </summary>
	public static void ShowProfileUI ()
	{
		LumosSocialGUI.ShowProfileUI();
	}

    /// <summary>
    /// Processes the authentication.
    /// </summary>
    /// <param name="success">Success.</param>
    static void ProcessAuthentication (bool success)
	{
        if (success) {
            Lumos.Log("Authenticated local user.");
        } else {
			Lumos.LogWarning("Failed to authenticate local user.");
		}
    }

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
