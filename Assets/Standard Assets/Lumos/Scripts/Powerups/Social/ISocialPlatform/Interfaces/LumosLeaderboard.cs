using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;
using System;
using System.Collections;

public class LumosLeaderboard : ILeaderboard {

	public string id { get; set; }
	public UserScope userScope { get; set; }
	public Range range { get; set; }
	public TimeScope timeScope { get; set; }
	public bool loading { get; private set; }
	public IScore localUserScore { get; private set; }
	public uint maxRange { get; private set; }
	public IScore[] scores { get; private set; }
	public string title { get; private set; }

	Action<bool> callback;


	public void SetUserFilter(string[] userIDs)
	{
		// do nothing
	}

	public void LoadScores(Action<bool> callback)
	{
		this.callback = callback;
		Social.LoadScores(id, AddScores);
	}

	void AddScores(IScore[] scores)
	{
		this.scores = scores;
		this.callback(true);
		this.callback = null;
	}
}
