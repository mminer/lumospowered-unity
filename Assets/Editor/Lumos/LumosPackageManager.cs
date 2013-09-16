// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
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
	[HideInInspector] public bool installing;
	
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

		if (packageManager == null) {
			packageManager = CreatePackageManager();
		}

		return packageManager;
	}

	/// <summary>
	/// Generates a blank Lumos package manager file.
	/// </summary>
	/// <returns>A fresh Lumos package manager object.</returns>
	static LumosPackageManager CreatePackageManager ()
	{
		// Create the Resources directory if it doesn't already exist.
		Directory.CreateDirectory("Assets/Standard Assets/Lumos/Resources");
		
		// Create the package manager asset.
		var packageManager = ScriptableObject.CreateInstance<LumosPackageManager>();
		AssetDatabase.CreateAsset(packageManager, "Assets/Standard Assets/Lumos/Resources/PackageManager.asset");
		return packageManager;
	}
	
	public Dictionary<string, LumosPackages.Package> GetInstalledPackages ()
	{
		return GetPackages(installedPackages, LumosPackages.Status.Installed);
	}
	
	public Dictionary<string, LumosPackages.Package> GetLatestPackages ()
	{
		return GetPackages(latestPackages, LumosPackages.Status.NotInstalled);
	}
	
	static Dictionary<string, LumosPackages.Package> GetPackages (string json, LumosPackages.Status status)
	{
		var packages = new Dictionary<string, LumosPackages.Package>();		
		var packageData = LumosJson.Deserialize(json) as IList;
		
		if (packageData != null) {
			foreach (Dictionary<string, object> data in packageData) {
				var powerupID = data["powerup_id"] as string;
				packages[powerupID] = new LumosPackages.Package(data, status);
			}
		}

		return packages;
	}
}
