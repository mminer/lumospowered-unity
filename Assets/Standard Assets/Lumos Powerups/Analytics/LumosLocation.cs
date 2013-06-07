// Copyright (c) 2012 Rebel Hippo Inc. All rights reserve

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LumosLocation 
{
	/// <summary>
	/// The URL.
	/// </summary>
	static string url = "http://localhost:8888/api/1/location";
	
	/// <summary>
	/// Record this player's location info.
	/// </summary>
	public static void Record()
	{
		var endpoint = url + "/" + Lumos.playerId + "?method=PUT";
		var location = GetLocation();

		LumosRequest.Send(endpoint, location,
			delegate { // Success
				var key = "lumospowered_" + Lumos.credentials.gameID + "_" + Lumos.playerId + "_sent_location";
				PlayerPrefs.SetInt(key, 1);
				Lumos.Log("Location information successfully sent.");
			},
			delegate { // Failure
				Lumos.Log("Failed to send Location information.");
			}
		);
	}
	
	/// <summary>
	/// Gets the location.
	/// </summary>
	/// <returns>
	/// The location.
	/// </returns>
	static Dictionary<string, object> GetLocation ()
	{
		var location = new Dictionary<string, object>(); 
		location.Add("language", Application.systemLanguage.ToString());
		
		if (Application.isWebPlayer) {
			location.Add("origin", Application.absoluteURL);
		}

		return location;
	}
}
