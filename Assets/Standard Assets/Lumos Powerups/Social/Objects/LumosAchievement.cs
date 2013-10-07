// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.GameCenter;

/// <summary>
/// An achievement a player has earned.
/// </summary>
public class LumosAchievement : IAchievement
{
	/// Unique identifier for the achievement.
	public string id { get; set; }

	/// The amount of the achievement completed.
	public double percentCompleted { get; set; }

	/// Indicates whether this achievement has been earned.
	public bool completed
	{
		get { return (int)percentCompleted == 100; }
	}

	/// Indicates whether this achievement is hidden.
	public bool hidden { get; set; }

	/// The date the achievement was last updated.
	public DateTime lastReportedDate { get; private set; }

	/// Creates a new achievement object.
	public LumosAchievement () {}

	/// Creates a new achievement object.
	public LumosAchievement (Dictionary<string, object> info)
	{
		this.id = info["achievement_id"] as string;
		this.percentCompleted = Convert.ToDouble(info["percent_completed"]);

		var timestamp = Convert.ToDouble(info["updated"]);
		this.lastReportedDate = LumosUtil.UnixTimestampToDateTime(timestamp);

		if (info.ContainsKey("hidden")) {
			var intHidden = Convert.ToInt32(info["hidden"]);
			this.hidden = Convert.ToBoolean(intHidden);
		}
	}

	/// <summary>
	/// Creates a new achievement object.
	/// </summary>
	/// <param name="id">A unique identifier.</param>
	/// <param name="percentCompleted">Percent completed.</param>
	/// <param name="completed">Completed.</param>
	/// <param name="hidden">Hidden.</param>
	/// <param name="lastReportedDate">Last reported date.</param>
	public LumosAchievement (string id, double percentCompleted, bool hidden, DateTime lastReportedDate)
	{
		this.id = id;
		this.percentCompleted = percentCompleted;
		this.hidden = hidden;
		this.lastReportedDate = lastReportedDate;
	}

	/// Reports progress for the achievement.
	public void ReportProgress (Action<bool> callback)
	{
		if (Social.localUser == null) {
			Debug.LogWarning("[Lumos] The user must be authenticated before reporting an achievement.");
			callback(false);
			return;
		}

		var endpoint = LumosSocial.baseUrl + "/users/" + Social.localUser.id + "/achievements/" + id + "?method=PUT";

		var payload = new Dictionary<string, object>() {
			{ "percent_completed", percentCompleted }
		};

		LumosRequest.Send(endpoint, payload,
			success => {
				var info = success as Dictionary<string, object>;

				// Update timestamp.
				var timestamp = Convert.ToDouble(info["updated"]);
				lastReportedDate = LumosUtil.UnixTimestampToDateTime(timestamp);
				
				if (Application.platform == RuntimePlatform.IPhonePlayer && LumosSocialSettings.useGameCenter) {
					ReportProgressToGameCenter(id, percentCompleted);
				}
			
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
	
	void ReportProgressToGameCenter (string achievementID, double percentCompleted)
	{
		var temp = Social.Active;
		Social.Active = new GameCenterPlatform();
		
		if (Social.localUser != null && Social.localUser.authenticated) {
			Social.ReportProgress(achievementID, percentCompleted, delegate {
				Lumos.Log("Reported achievement progress to Game Center.");
			});	
		}
		
		Social.Active = temp;
	}
}
