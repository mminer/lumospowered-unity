// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player-related functionality.
/// </summary>
public class LumosPlayer
{
	static string _baseUrl = "https://core.lumospowered.com/api/1";

	/// <summary>
	/// The API's host domain.
	/// </summary>
	public static string baseUrl {
		get { return _baseUrl; }
		set { _baseUrl = value; }
	}

	/// <summary>
	/// Fetches an existing player ID or creates a new one.
	/// </summary>
	public static void Init (Action<bool> callback)
	{
		var idPrefsKey = "lumospowered_" + Lumos.credentials.gameID + "_playerid";

		if (PlayerPrefs.HasKey(idPrefsKey)) {
			Lumos.playerID = PlayerPrefs.GetString(idPrefsKey);
			Lumos.Log("Using existing player " + Lumos.playerID);
			callback(true);
			Ping();
		} else {
			RequestPlayerId(callback);
		}
	}

	/// <summary>
	/// Notifies the server to generate and return a new Player ID.
	/// </summary>
	static void RequestPlayerId(Action<bool> callback)
	{
		// Get a new player ID from Lumos.
		var endpoint = baseUrl + "/players";

		LumosRequest.Send(endpoint,
			success => {
				var idPrefsKey = "lumospowered_" + Lumos.credentials.gameID + "_playerid";
				var resp = success as Dictionary<string, object>;
				Lumos.playerID = resp["player_id"].ToString();
				PlayerPrefs.SetString(idPrefsKey, Lumos.playerID);
				Lumos.Log("Using new player " + Lumos.playerID);

				if (callback != null) {
					callback(true);
				}
			});
	}

	/// <summary>
	/// Notifies the server that the player is playing.
	/// </summary>
	static void Ping ()
	{
		var endpoint = baseUrl + "/players/" + Lumos.playerID + "?method=PUT";

		LumosRequest.Send(endpoint,
			success => {
				//var resp = success as Dictionary<string, object>;
				//Lumos.Log(resp["message"]);
			},
			error => {
				//var resp = error as Dictionary<string, object>;
				//Lumos.LogError(resp["message"]);
			});
	}
	
	/// <summary>
	/// Gets the settings for a powerup used by this game
	/// </summary>
	public static void GetPowerupSettings (string powerupID, Action<Dictionary<string, object>> callback)
	{
		var endpoint = baseUrl + "/settings/" + powerupID;

		LumosRequest.Send(endpoint, "GET", 
			success => {
				callback(success as Dictionary<string, object>);
			},
			error => {
				callback(null);
			});
	}
}
