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
public class LumosAchievement : IAchievement {
	
	/// <summary>
	/// Gets or sets the identifier.
	/// </summary>
	/// <value>
	/// The identifier.
	/// </value>
	public string id { get; set; }
	/// <summary>
	/// Gets or sets the percent completed.
	/// </summary>
	/// <value>
	/// The percent completed.
	/// </value>
	public double percentCompleted { get; set; }
	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="LumosAchievement"/> is completed.
	/// </summary>
	/// <value>
	/// <c>true</c> if completed; otherwise, <c>false</c>.
	/// </value>
	public bool completed { get; set; }
	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="LumosAchievement"/> is hidden.
	/// </summary>
	/// <value>
	/// <c>true</c> if hidden; otherwise, <c>false</c>.
	/// </value>
	public bool hidden { get; set; }
	/// <summary>
	/// Gets or sets the last reported date.
	/// </summary>
	/// <value>
	/// The last reported date.
	/// </value>
	public DateTime lastReportedDate { get; set; }
	
	/// <summary>
	/// Initializes a new instance of the <see cref="LumosAchievement"/> class.
	/// </summary>
	/// <param name='id'>
	/// Identifier.
	/// </param>
	/// <param name='percentCompleted'>
	/// Percent completed.
	/// </param>
	/// <param name='completed'>
	/// Completed.
	/// </param>
	/// <param name='hidden'>
	/// Hidden.
	/// </param>
	/// <param name='lastReportedDate'>
	/// Last reported date.
	/// </param>
	public LumosAchievement(string id, double percentCompleted, bool completed, bool hidden, DateTime lastReportedDate) 
	{
		this.id = id;
		this.percentCompleted = percentCompleted;
		this.completed = completed;
		this.hidden = hidden;
		this.lastReportedDate = lastReportedDate;
	}
	
	/// <summary>
	/// Reports the progress.
	/// </summary>
	/// <param name='callback'>
	/// Callback.
	/// </param>
	public void ReportProgress(Action<bool> callback) 
	{
		Social.ReportProgress(id, percentCompleted, callback);
	}
}
