// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;

/// <summary>
/// Lumos achievement.
/// </summary>
public class LumosAchievement : IAchievement
{
	/// <summary>
	/// Unique identifier for the achievement.
	/// </summary>
	public string id { get; set; }

	/// <summary>
	/// The amount of the achievement completed.
	/// </summary>
	public double percentCompleted { get; set; }

	/// <summary>
	/// Indicates whether this achievement has been earned.
	/// </summary>
	public bool completed
	{
		get { return (int)percentCompleted == 100; }
	}

	/// <summary>
	/// Indicates whether this achievement is hidden.
	/// </summary>
	public bool hidden { get; set; }

	/// <summary>
	/// The date the achievement was last updated.
	/// </summary>
	public DateTime lastReportedDate { get; set; }

	/// <summary>
	/// Creates a new achievement object.
	/// </summary>
	public LumosAchievement () {}

	/// <summary>
	/// Creates a new achievement object.
	/// </summary>
	/// <param name="info">Information about the achievement.</param>
	public LumosAchievement (Dictionary<string, object> info)
	{
		this.id = info["achievement_id"] as string;
		this.percentCompleted = Convert.ToDouble(info["percent_completed"]);

		if (info.ContainsKey("hidden")) {
			var intHidden = Convert.ToInt32(info["hidden"]);
			this.hidden = Convert.ToBoolean(intHidden);
		}

		var timestamp = Convert.ToDouble(info["updated"]);
		this.lastReportedDate = LumosUtil.UnixTimestampToDateTime(timestamp);
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

	/// <summary>
	/// Reports progress for the achievement.
	/// </summary>
	/// <param name="callback">Callback triggers on success.</param>
	public void ReportProgress (Action<bool> callback)
	{
		var endpoint = LumosSocial.baseUrl + "/users/" + localUser.id + "/achievements/" + achievementID + "?method=PUT";

		var payload = new Dictionary<string, object>() {
			{ "percent_completed", percentCompleted }
		};

		LumosRequest.Send(endpoint, payload,
			success => {
				var info = success as Dictionary<string, object>;

				// Update timestamp.
				var timestamp = Convert.ToDouble(info["updated"]);
				lastReportedDate = LumosUtil.UnixTimestampToDateTime(timestamp);

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
}
