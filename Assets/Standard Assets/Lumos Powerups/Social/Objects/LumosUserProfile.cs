// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

/// <summary>
/// Holds information about a player.
/// </summary>
public class LumosUserProfile : IUserProfile
{
	/// <summary>
	/// The user's name.
	/// </summary>
	public string userName { get; set; }

	/// <summary>
	/// A unique identifier for this user.
	/// </summary>
	public string userID { get; set; }

	/// <summary>
	/// A unique identifier for this user.
	/// </summary>
	public string id
	{
		get { return userID; }
	}

	/// <summary>
	/// Indicates whether this user is a friend of the current player.
	/// </summary>
	public bool isFriend { get; set; }

	/// <summary>
	/// The user's state.
	/// </summary>
	public UserState state { get; set; }

	/// <summary>
	/// An avatar representing the user.
	/// </summary>
	public Texture2D image { get; set; }

	/// <summary>
	/// Creates a new user blank profile.
	/// </summary>
	public LumosUserProfile () {}

	/// <summary>
	/// Creates a new user profile.
	/// </summary>
	public LumosUserProfile (Dictionary<string, object> info)
	{
		this.userID = info["username"] as string;

		if (info.ContainsKey("name")) {
			this.userName = info["name"] as string;
		}
	}
}