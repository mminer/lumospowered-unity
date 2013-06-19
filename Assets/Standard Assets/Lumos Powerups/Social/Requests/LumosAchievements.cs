// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

public partial class LumosSocialPlatform : ISocialPlatform {

	public List<IAchievementDescription> achievementDescriptions = new List<IAchievementDescription>();
	public List<LumosAchievement> achievements = new List<LumosAchievement>();


	void FetchPlayerAchievements (Action<IAchievement[]> callback)
	{
		var api = url + "users/" + localUser.id + "/achievements?method=GET";

		LumosRequest.Send(api, delegate (object response) {
			var resp = response as IList;
			achievements = new List<LumosAchievement>();

			foreach (Dictionary<string, object> info in resp) {
				var achievement = DictionaryToAchievement(info);
				achievements.Add(achievement);
			}

			callback(achievements.ToArray());
		});
	}

	void FetchGameAchievements (Action<IAchievementDescription[]> callback)
	{
		var api = url + "achievements?method=GET";

		LumosRequest.Send(api, delegate (object response) {
			var resp = response as IList;
			achievementDescriptions = new List<IAchievementDescription>();

			foreach (Dictionary<string, object> info in resp) {
				Lumos.RunRoutine(AddAchievement(info, resp.Count, callback));
			}
		});
	}

	IEnumerator AddAchievement (Dictionary<string, object> info, int limit, Action<IAchievementDescription[]> callback)
	{
		var id = info["achievement_id"] as string;
		var title = info["name"] as string;
		var imageLocation = info.ContainsKey("icon") ? info["icon"] as string : "";
		var achievedDescription = info["achieved_description"] as string;
		var unachievedDescription = info["unachieved_description"] as string;
		var tempHidden = Convert.ToInt32(info["hidden"]);
		var hidden = Convert.ToBoolean(tempHidden);
		var points = 0;
		int.TryParse(info["points"] as string, out points);
		
		Debug.Log(imageLocation);
		
		// Create a blank texture in DXT1 format
		var image = new Texture2D(4, 4, TextureFormat.DXT1, false);

		// Load the achievement's image, if it has one
		if (imageLocation != "") {
			var imageWWW = new WWW(imageLocation);

			yield return imageWWW;

			try {
				if (imageWWW.error != null) {
					throw new Exception(imageWWW.error);
				}

				imageWWW.LoadImageIntoTexture(image);
			} catch (Exception e) {
				Debug.Log("Failure: " + e.Message);
			}
		} else {
			if (LumosSocialGUI.GetDefaultAchievement() != null) {
				image = LumosSocialGUI.GetDefaultAchievement();
			}
		}

		var achievementDescription = new AchievementDescription(id, title, image, achievedDescription, unachievedDescription, hidden, points);
		achievementDescriptions.Add(achievementDescription);

		// All achievements have been loaded
		if (achievementDescriptions.Count == limit) {
			callback(achievementDescriptions.ToArray());
		}
	}

	void UpdateAchievementProgress (string achievementId, int progress, Action<bool> callback)
	{
		var api = url + "users/" + localUser.id + "/achievements/" + achievementId + "?method=PUT";

		var parameters = new Dictionary<string, object>() {
			{ "percent_completed", progress }
		};

		LumosRequest.Send(api, parameters, delegate (object response) {
			var resp = response as Dictionary<string, object>;
			var achievement = DictionaryToAchievement(resp);
			UpdateAchievement(achievement);
			callback(true);
		});
	}

	LumosAchievement GetAchievementById (string achievementId)
	{
		foreach (var achievement in achievements) {
			if (achievement.id == achievementId) {
				return achievement;
			}
		}

		return null;
	}

	void UpdateAchievement (LumosAchievement achievement)
	{
		foreach (LumosAchievement a in achievements) {
			if (a.id == achievement.id) {
				a.percentCompleted = achievement.percentCompleted;
				a.lastReportedDate = achievement.lastReportedDate;
				break;
			}
		}
	}

	LumosAchievement DictionaryToAchievement (Dictionary<string, object> info)
	{
		var id = info["achievement_id"] as string;
		var percentCompleted = Convert.ToDouble(info["percent_completed"]);
		var completed = percentCompleted == 100 ? true : false;
		var hidden = false;

		if (info.ContainsKey("hidden")) {
			var intHidden = Convert.ToInt32(info["hidden"]);
			hidden = Convert.ToBoolean(intHidden);
		}


		var timestamp = Convert.ToDouble(info["updated"]);
		var lastReportedDate = LumosUtil.UnixTimestampToDateTime(timestamp);
		var achievement = new LumosAchievement(id, percentCompleted, completed, hidden, lastReportedDate);
		return achievement;
	}
}
