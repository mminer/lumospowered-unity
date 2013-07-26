// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

/// <summary>
/// Fuctions for managing users.
/// </summary>
public partial class LumosSocial
{
	static LumosUser _localUser;

	/// <summary>
	/// The local user.
	/// </summary>
	public ILocalUser localUser { get { return _localUser; } }

	/// <summary>
	/// Authenticates the given user.
	/// </summary>
	/// <param name="user">The user to authenticate.</param>
	/// <param name="callback">Callback.</param>
	public void Authenticate (ILocalUser user, Action<bool> callback)
	{
		user.Authenticate(
			success => {
				if (success) {
					_localUser = user as LumosUser;
				}

				if (callback != null) {
					callback(success);
				}
			});
	}

	/// <summary>
	/// Loads the specified users.
	/// </summary>
	/// <param name="userIDs">Usernames of profiles to fetch.</param>
	/// <param name="callback">Callback.</param>
	public void LoadUsers(string[] userIDs, Action<IUserProfile[]> callback)
	{
		var endpoint = baseUrl + "/users?method=GET";
		var payload = new Dictionary<string, object>() {
			{ "user_ids", userIDs }
		};

		LumosRequest.Send(endpoint, payload,
			success => {
				var resp = success as Dictionary<string, object>;
				var users = new List<IUserProfile>(resp.Count);

				foreach (var kvp in resp) {
					var info = kvp.Value as Dictionary<string, object>;
					var user = new LumosUserProfile(info);
					users.Add(user);
				}

				if (callback != null) {
					callback(users.ToArray());
				}
			},
			error => {
				if (callback != null) {
					callback(null);
				}
			});
	}

	/// <summary>
	/// Loads the user's friends.
	/// </summary>
	/// <param name="user">User.</param>
	/// <param name="callback">Callback.</param>
	public void LoadFriends(ILocalUser user, Action<bool> callback)
	{
		user.LoadFriends(callback);
	}

	#region Added Functions

	/// <summary>
	/// Registers a new user.
	/// </summary>
	/// <param name="user">The user object.</param>
	/// <param name="callback">Callback.</param>
	public static void RegisterUser (LumosUser user, Action<bool> callback)
	{
		var endpoint = LumosSocial.baseUrl + "/users/" + user.id + "?method=PUT";
		var payload = new Dictionary<string, object>() {
			{ "player_id", Lumos.playerId },
			{ "password", user.password }
		};

		LumosUtil.AddToDictionaryIfNonempty(payload, "email", user.email);

		LumosRequest.Send(endpoint, payload,
			success => {
				var info = success as Dictionary<string, object>;
				user.authenticated = true;
				user.Update(info);
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

	/// <summary>
	/// Requests to reset the given user's password.
	/// </summary>
	/// <param name="username">Username.</param>
	/// <param name="callback">Callback.</param>
	public static void ResetPassword (string username, Action<bool> callback)
	{
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

	#endregion
}
