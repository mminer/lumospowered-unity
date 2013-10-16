// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using UnityEngine;

/// <summary>
/// Sets up the Lumos Analytics powerup.
/// </summary>
public class LumosAnalyticsSetup : ILumosSetup
{
	public void Setup ()
	{
		var lumos = GameObject.Find("Lumos");

		if (lumos != null && lumos.GetComponent<LumosAnalytics>() == null) {
			lumos.AddComponent<LumosAnalytics>();
			Debug.Log("[Lumos] Lumos Analytics setup complete.");
		}
	}
}
