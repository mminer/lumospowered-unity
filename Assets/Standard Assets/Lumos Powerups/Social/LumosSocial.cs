// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using UnityEngine;
using UnityEngine.SocialPlatforms;

/// <summary>
/// The Lumos social platform.
/// </summary>
public partial class LumosSocial : ISocialPlatform
{
	static string _baseUrl = "https://social.lumospowered.com/api/1";

	/// <summary>
	/// The API's host domain.
	/// </summary>
	public static string baseUrl {
		get { return _baseUrl; }
		set { _baseUrl = value; }
	}
}
