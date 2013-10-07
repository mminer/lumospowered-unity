// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using UnityEngine;

/// <summary>
/// Sets up the Lumos Social powerup.
/// </summary>
public class LumosSocialSetup : ILumosSetup
{
	public void Setup ()
	{
		var lumos = GameObject.Find("Lumos");

		if (lumos != null && lumos.GetComponent<LumosSocialSettings>() == null) {
			lumos.AddComponent<LumosSocialSettings>();
			Debug.Log("Lumos Social setup complete.");
		}
	}
}
