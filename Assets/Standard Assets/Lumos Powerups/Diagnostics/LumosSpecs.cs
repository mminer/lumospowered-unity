// Copyright (c) 2012 Rebel Hippo Inc. All rights reserved.

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
	/// The URL.
	/// </summary>
	static string url = "http://localhost:8888/api/1/diagnostics";

	/// <summary>
	/// Sends system information.
	/// </summary>
	public static void Record ()
	{
		var endpoint = url + "/specs/" + Lumos.playerId + "?method=PUT";
		var specs = GetSpecs();

		LumosRequest.Send(endpoint, specs,
			delegate { // Success
				var key = "lumospowered_" + Lumos.credentials.gameID + "_" + Lumos.playerId + "_sent_specs";
				PlayerPrefs.SetInt(key, 1);
				Lumos.Log("System information successfully sent.");
			},
			delegate { // Failure
				Lumos.Log("Failed to send system information.");
			}
		);
	}

#else

	/// <summary>
	/// Noop.
	/// </summary>
	public static void Record () {}

#endif

	static Dictionary<string, object> GetSpecs ()
	{
		var specs = new Dictionary<string, object>() {
			{ "os", SystemInfo.operatingSystem },
			{ "processor", SystemInfo.processorType },
			{ "processor_count", SystemInfo.processorCount },
			{ "ram", SystemInfo.systemMemorySize },
			{ "vram", SystemInfo.graphicsMemorySize },
			{ "graphics_card_name", SystemInfo.graphicsDeviceName },
			{ "graphics_card_vendor", SystemInfo.graphicsDeviceVendor },
			{ "graphics_card_id", SystemInfo.graphicsDeviceID },
			{ "graphics_card_vendor_id", SystemInfo.graphicsDeviceVendorID },
			{ "graphics_card_version", SystemInfo.graphicsDeviceVersion },
			{ "shader_level", SystemInfo.graphicsShaderLevel },
			{ "pixel_fillrate", SystemInfo.graphicsPixelFillrate },
			{ "supports_shadows", SystemInfo.supportsShadows },
			{ "supports_render_textures", SystemInfo.supportsRenderTextures },
			{ "supports_image_effects", SystemInfo.supportsImageEffects }
		};

		return specs;
	}
}
