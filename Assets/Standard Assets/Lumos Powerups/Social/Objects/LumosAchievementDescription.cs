// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

/// <summary>
/// Lumos achievement description.
/// </summary>
public class LumosAchievementDescription : IAchievementDescription
{
	/// <summary>
	/// Unique identifier for the achievement.
	/// </summary>
	public string id { get; set; }

	/// <summary>
	/// The name of this achievement.
	/// </summary>
	public string title { get; set; }

	/// <summary>
	/// Icon that represents the achievement.
	/// </summary>
	public Texture2D image { get; set; }

	/// <summary>
	/// Description of the achievement after it has been earned.
	/// </summary>
	public string achievedDescription { get; set; }

	/// <summary>
	/// Description of the achievement before it has been earned.
	/// </summary>
	public string unachievedDescription { get; set; }

	/// <summary>
	/// Whether this achievement is publicly visible.
	/// </summary>
	public bool hidden { get; set; }

	/// <summary>
	/// The number of points the achievement is worth once completed.
	/// </summary>
	public int points { get; set; }

	/// <summary>
	/// Creates an achievement description object.
	/// </summary>
	public LumosAchievementDescription (Dictionary<string, object> info)
	{
		this.id = info["achievement_id"] as string;
		this.title = info["name"] as string;
		this.achievedDescription = info["achieved_description"] as string;
		this.unachievedDescription = info["unachieved_description"] as string;
		this.points = Convert.ToInt32(info["points"] as string);

		var hiddenInt = Convert.ToInt32(info["hidden"] as string);
		this.hidden = Convert.ToBoolean(hiddenInt);

		// Load image from remote server.
		if (info.ContainsKey("icon")) {
			var imageLocation = info["icon"] as string;
			Lumos.RunRoutine(LoadImage(imageLocation));
		}
	}

	/// <summary>
	/// Loads the achievement's image.
	/// </summary>
	/// <param name="imageLocation">The URL of the image.</param>
	IEnumerator LoadImage (string imageLocation)
	{
		var www = new WWW(imageLocation);
		yield return www;

		try {
			if (www.error != null) {
				throw new Exception(www.error);
			}

			www.LoadImageIntoTexture(image);
		} catch (Exception e) {
			Lumos.LogError("Failed to load achievement image: " + e.Message);
		}
	}
}
