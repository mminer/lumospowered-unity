// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// General actions for the app itself.
/// </summary>
public class LumosCore
{
	
	/// <summary>
	/// The key used to store the player ID in PlayerPrefs.
	/// </summary>
	public static string playerIdPrefsKey {
		get { return "lumos_" + Lumos.gameId + "_player_id"; }
	}
	
	static string _playerId;
	
	/// <summary>
	/// A unique string that identifies the player.
	/// </summary>
	public static string playerId {
		get {
			if (_playerId == null) {
				_playerId = PlayerPrefs.GetString(playerIdPrefsKey);
			}

			return _playerId;
		}
		set {
			_playerId = value;
			PlayerPrefs.SetString(playerIdPrefsKey, value);
		}
	}

	/// <summary>
	/// Whether a player ID has been previously saved.
	/// </summary>
	public static bool hasPlayer {
		get { return PlayerPrefs.HasKey(playerIdPrefsKey); }
	}

	private static string url = "localhost:8888/api/1/games/";

	public static void Init()
	{
		url += Lumos.gameId;
		
		if (hasPlayer) {
			Lumos.Log("Using existing player " + playerId);
			LumosCore.Ping();
		} else {
			LumosCore.GeneratePlayerId();
		}
	}

	/// <summary>
	/// Notifies the server to generate and return a new Player ID.
	/// </summary>
	public static void GeneratePlayerId() 
	{
		var api = url + "/players";
		
		LumosRequest.Send(api, delegate {
			var response = LumosRequest.lastResponse;
			LumosCore.playerId = response["player_id"].ToString();
			Lumos.Log("Using new player " + LumosCore.playerId);
			LumosCore.Ping();
		});
	}

	/// <summary>
	/// Notifies the server that the player is playing.
	/// </summary>
	public static void Ping ()
	{
		var api = url + "/players/" + playerId + "?method=PUT";

		var parameters = new Dictionary<string, object>() {
			{ "player_id", playerId },
			{ "lumos_version", Lumos.version.ToString() }
		};

		LumosRequest.Send(api, parameters, delegate {
			var response = LumosRequest.lastResponse;
			
			if (response["message"] == null) {
				Lumos.LogWarning("Something went wrong!");
			} else {
				Debug.Log(response["message"]);
			}
		});
	}
}
