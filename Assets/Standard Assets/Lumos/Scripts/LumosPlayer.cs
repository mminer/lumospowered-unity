// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player-related functionality.
/// </summary>
public class LumosPlayer
{
	static string url = "http://localhost:8888/api/1/games/";//"http://localhost:8888/api/1/games/";

	/// <summary>
	/// Fetches an existing player ID or creates a new one.
	/// </summary>
	public static void Init ()
	{
		url += Lumos.gameId;
		var idPrefsKey = "lumospowered_" + Lumos.gameId + "_playerid";

		if (PlayerPrefs.HasKey(idPrefsKey)) {
			Lumos.playerId = PlayerPrefs.GetString(idPrefsKey);
			Lumos.Log("Using existing player " + Lumos.playerId);
			Ping();
		} else {
			RequestPlayerId();
		}
	}

	/// <summary>
	/// Notifies the server to generate and return a new Player ID.
	/// </summary>
	static void RequestPlayerId()
	{
		// Get a new player ID from Lumos.
		var endpoint = url + "/players";

		LumosRequest.Send(endpoint, delegate (object response) {
			var resp = response as Dictionary<string, object>;
			Lumos.playerId = resp["player_id"].ToString();
			Lumos.Log("Using new player " + Lumos.playerId);
		});
	}

	/// <summary>
	/// Notifies the server that the player is playing.
	/// </summary>
	static void Ping ()
	{
		var endpoint = url + "/players/" + Lumos.playerId + "?method=PUT";

		var parameters = new Dictionary<string, object>() {
			{ "player_id", Lumos.playerId },
			{ "lumos_version", Lumos.version.ToString() }
		};

		LumosRequest.Send(endpoint, parameters,
			delegate (object response) { // Success
				var resp = response as Dictionary<string, object>;
				Lumos.Log(resp["message"]);
			},
			delegate (object response) { // Error
				var resp = response as Dictionary<string, object>;
				Lumos.LogError(resp["message"]);
			});
	}
}
