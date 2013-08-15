// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using UnityEngine;

/// <summary>
/// Sets up the Lumos Analytics powerup.
/// </summary>
public class LumosDiagnosticsSetup : ILumosSetup
{
	public void Setup ()
	{
		var lumos = GameObject.Find("Lumos");

		if (lumos != null && lumos.GetComponent<LumosDiagnostics>() == null) {
			lumos.AddComponent<LumosDiagnostics>();
			Debug.Log("Lumos Diagnostics setup complete.");
		}
	}
}