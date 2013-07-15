// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

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
	public bool completed { get; set; }

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
	/// <param name="id">A unique identifier.</param>
	/// <param name="percentCompleted">Percent completed.</param>
	/// <param name="completed">Completed.</param>
	/// <param name="hidden">Hidden.</param>
	/// <param name="lastReportedDate">Last reported date.</param>
	public LumosAchievement(string id, double percentCompleted, bool completed, bool hidden, DateTime lastReportedDate)
	{
		this.id = id;
		this.percentCompleted = percentCompleted;
		this.completed = completed;
		this.hidden = hidden;
		this.lastReportedDate = lastReportedDate;
	}

	/// <summary>
	/// Reports progress for the achievement.
	/// </summary>
	/// <param name="callback">Callback triggers on success.</param>
	public void ReportProgress(Action<bool> callback)
	{
		Social.ReportProgress(id, percentCompleted, callback);
	}
}
