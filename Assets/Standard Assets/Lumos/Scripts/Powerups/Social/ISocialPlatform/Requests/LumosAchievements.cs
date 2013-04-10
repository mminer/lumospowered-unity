using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

public partial class LumosSocialPlatform : ISocialPlatform {
	
	public List<IAchievementDescription> achievementDescriptions = new List<IAchievementDescription>();
	public List<LumosAchievement> achievements = new List<LumosAchievement>();
	
	
	void FetchPlayerAchievements(Action<IAchievement[]> callback) 
	{						
		var api = url + "/achievements/" + localUser.id + "?method=GET";
		
		LumosRequest.Send(api, delegate {
			var response = LumosRequest.lastResponse as ArrayList;
			achievements = new List<LumosAchievement>();
			
			foreach (Hashtable info in response) {
				var achievement = HashtableToAchievement(info);
				achievements.Add(achievement);
			}
			
			callback(achievements.ToArray());
		});
	}
	
	void FetchGameAchievements(Action<IAchievementDescription[]> callback) 
	{			
		var api = url + "/achievements?method=GET";
		
		LumosRequest.Send(api, delegate {
			var response = LumosRequest.lastResponse as ArrayList;
			achievementDescriptions = new List<IAchievementDescription>();
			
			foreach (Hashtable info in response) {
				Lumos.RunRoutine(AddAchievement(info, response.Count, callback));
			}
		});
	}
	
	IEnumerator AddAchievement (Hashtable info, int limit, Action<IAchievementDescription[]> callback) 
	{
		var id = info["id"] as string;
		var title = info["title"] as string;
		var imageLocation = info["image"] as string;
		var achievedDescription = info["achieved_description"] as string;
		var unachievedDescription = info["unachieved_description"] as string;
		var hidden = (bool)info["hidden"];
		var points = 0;
		int.TryParse(info["points"] as string, out points);
		
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
		}
		
		var achievementDescription = new AchievementDescription(id, title, image, achievedDescription, unachievedDescription, hidden, points);
		achievementDescriptions.Add(achievementDescription);
		
		// All achievements have been loaded
		if (achievementDescriptions.Count == limit) {
			callback(achievementDescriptions.ToArray());	
		}
	}
	
	void UpdateAchievementProgress(string achievementId, int progress, Action<bool> callback)
	{						
		var api = url + "/achievements/" + localUser.id + "/" + achievementId + "?method=PUT";
		
		var parameters = new Dictionary<string, object>() {
			{ "percent_completed", progress }
		};
		
		LumosRequest.Send(api, parameters, delegate {
			var response = LumosRequest.lastResponse as Hashtable;
			var info = response["achievement"] as Hashtable;
			var achievement = HashtableToAchievement(info);
			UpdateAchievement(achievement);
			callback(true);
		});
	}
	
	LumosAchievement GetAchievementById(string achievementId) 
	{
		foreach (var achievement in achievements) {
			if (achievement.id == achievementId) {
				return achievement;
			}
		}
		
		return null;
	}
	
	void UpdateAchievement(LumosAchievement achievement) {
		foreach (LumosAchievement a in achievements) {
			if (a.id == achievement.id) {
				a.percentCompleted = achievement.percentCompleted;
				a.lastReportedDate = achievement.lastReportedDate;
				break;
			}
		}
	}

	LumosAchievement HashtableToAchievement(Hashtable info) {
		var id = info["id"] as string;
		var percentCompleted = (double)info["percent_completed"];
		var completed = (bool)info["completed"];
		var hidden = (bool)info["hidden"];
		var lastReportedDate = DateTime.Parse(info["last_reported_date"] as string);
		var achievement = new LumosAchievement(id, percentCompleted, completed, hidden, lastReportedDate);
		return achievement;
	}
}
