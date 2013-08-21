// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Access package manager data.
/// These are set during Lumos installation and package updates.
/// Values are stored in a file since Unity recompiles when new scripts are imported.
/// </summary>
public class LumosPackageManager : ScriptableObject
{	
	/// <summary>
	/// Whether or not the install process is currently underway.
	/// </summary>
	[HideInInspector] public bool installing = false;
	
	/// <summary>
	/// The queued packages (JSON).
	/// </summary>
	[HideInInspector] public string installQueue;
	
	/// <summary>
	/// The latest available packages (JSON).
	/// </summary>
	[HideInInspector] public string latestPackages;
	
	/// <summary>
	/// The installed packages (JSON).
	/// </summary>
	[HideInInspector] public string installedPackages;

	/// <summary>
	/// Loads the Lumos package manager file from Resources.
	/// </summary>
	/// <returns>The Lumos package manager object.</returns>
	public static LumosPackageManager Load ()
	{
		var packageManager = Resources.Load("PackageManager", typeof(LumosPackageManager)) as LumosPackageManager;
		return packageManager;
	}
}
