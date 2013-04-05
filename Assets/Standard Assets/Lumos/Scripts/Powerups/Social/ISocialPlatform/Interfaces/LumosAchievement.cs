using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

public class LumosAchievement : IAchievement {

	public string id { get; set; }
	public double percentCompleted { get; set; }
	public bool completed { get; set; }
	public bool hidden { get; set; }
	public DateTime lastReportedDate { get; set; }
	
	
	public LumosAchievement(string id, double percentCompleted, bool completed, bool hidden, DateTime lastReportedDate) 
	{
		this.id = id;
		this.percentCompleted = percentCompleted;
		this.completed = completed;
		this.hidden = hidden;
		this.lastReportedDate = lastReportedDate;
	}
	
	public void ReportProgress(Action<bool> callback) 
	{
		Social.ReportProgress(id, percentCompleted, callback);
	}
}
