// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A service that records the player's software and hardware capabilities.
/// Operating system, RAM, etc.
/// </summary>
public class LumosSpecs
{

#if !UNITY_IPHONE

	/// <summary>
	/// Sends system information.
	/// </summary>
	public static void Record ()
	{
		var prefsKey = "lumospowered_" + Lumos.credentials.gameID + "_" + Lumos.playerId + "_sent_specs";

		// Only record system information once.
		if (PlayerPrefs.HasKey(prefsKey)) {
			return;
		}

		var endpoint = LumosDiagnostics.baseUrl + "/specs/" + Lumos.playerId + "?method=PUT";
		var payload = new Dictionary<string, object>() {
			{ "os", SystemInfo.operatingSystem },
			{ "processor", SystemInfo.processorType },
			{ "processor_count", SystemInfo.processorCount },
			{ "ram", SystemInfo.systemMemorySize },
			{ "vram", SystemInfo.graphicsMemorySize },
			{ "graphics_card", SystemInfo.graphicsDeviceName }
		};

		LumosRequest.Send(endpoint, payload,
			delegate { // Success
				PlayerPrefs.SetString(prefsKey, System.DateTime.Now.ToString());
				Lumos.Log("System information successfully sent.");
			},
			delegate { // Failure
				Lumos.LogError("Failed to send system information.");
			}
		);
	}

#else

	/// <summary>
	/// NOOP.
	/// </summary>
	public static void Record () {}

#endif

}
