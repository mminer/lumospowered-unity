// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Manages the retrieval and status of powerup packages.
/// </summary>
public static class LumosPackages
{
	public enum Status { Installed, NotInstalled, UpdateAvailable, Downloading }

	/// <summary>
	/// Holds information about a powerup package.
	/// </summary>
	public class Package {
		public readonly string name;
		public Uri url;

		public Status status { get; set; }
		public string version { get; set; }
		public string nextVersion { get; set; }

		public Package (Dictionary<string, object> data, Status status) {
			name = data["name"] as string;
			url = new Uri(data["url"] as string);

			version = data["version"] as string;
			nextVersion = version;
			this.status = status;
		}

		public Dictionary<string, object> ToDictionary ()
		{
			var dict = new Dictionary<string, object>() {
				{ "name", name },
				{ "version", version },
				{ "url", url.ToString() }
			};

			return dict;
		}
	}

	static readonly Uri updatesUrl = new Uri("http://localhost:8888/api/1/powerups?engine=unity");
	static Dictionary<string, Package> _packages;
	static bool installUpdates;
	public static List<Package> updates = new List<Package>();

	/// <summary>
	/// The powerup packages.
	/// </summary>
	public static Dictionary<string, Package> packages {
		get {
			if (_packages == null) {
				_packages = GetInstalledPackages();
			}
			return _packages;
		}
	}

	/// <summary>
	/// Whether we're currently downloading a list of updates from Lumos.
	/// </summary>
	public static bool checkingForUpdates { get; private set; }

	/// <summary>
	/// Filenames of powerup packages that are currently importing.
	/// </summary>
	static Queue<string> importQueue = new Queue<string>();

	/// <summary>
	/// EditorApplication.update callback to allow package imports in single
	/// threaded environment.
	/// </summary>
	public static void MonitorImports ()
	{
		// Done here to get around main thread issues with async calls.
		while (importQueue.Count > 0) {
			var path = importQueue.Dequeue();
			ImportPackage(path);
			RecordInstalledPackages(packages);
		}
	}
	
	public static int CurrentDownloadCount ()
	{
		int count = 0;
		
		foreach (var package in packages) {
			if (package.Value.status == Status.Downloading) {
				count++;
			}
		}
		
		return count;
	}

	/// <summary>
	/// Downloads the latest powerup package.
	/// </summary>
	/// <param name="package">The powerup package to install.</param>
	public static void UpdatePackage (Package package)
	{
		package.status = Status.Downloading;
		
		Debug.Log(package.name + " is being downloaded from " + package.url.AbsoluteUri);

		// Construct local filename.
		var filename = Path.GetFileName(package.url.LocalPath);
		var path = Path.Combine("Temp", filename);

		// Download.
		var client = new WebClient();
		client.DownloadFileAsync(package.url, path);
		client.DownloadFileCompleted += delegate(object sender, System.ComponentModel.AsyncCompletedEventArgs e) {
			Debug.Log("finished downloading: " + package.name);
			Debug.Log("Error: " + e.Error.Message);
			importQueue.Enqueue(path);
			package.status = Status.Installed; // To be true soon enough
			package.version = package.nextVersion;
		};
	}

	/// <summary>
	/// Checks for updated powerup packages.
	/// </summary>
	public static void CheckForUpdates ()
	{
		checkingForUpdates = true;

		var client = new WebClient();
		var authorizationHeader =
			LumosRequest.GenerateAuthorizationHeader(LumosCredentialsManager.GetCredentials(), null);
		client.Headers.Add("Authorization", authorizationHeader);
		client.DownloadStringAsync(updatesUrl);
		client.DownloadStringCompleted += CheckForUpdatesCallback;
	}
	
	/// <summary>
	/// Checks for available packages or updates, and installs them.
	/// </summary>
	public static void CheckAndInstallUpdates ()
	{
		installUpdates = true;
		CheckForUpdates();
	}

	/// <summary>
	/// Parses the results of checking for powerup package updates.
	/// </summary>
	static void CheckForUpdatesCallback (object sender, DownloadStringCompletedEventArgs e) {
		Debug.Log(e.Result);
		var response = LumosJson.Deserialize(e.Result) as IList;
		Package packageToUpdate = null;

		foreach (Dictionary<string, object> data in response) {
			var powerupID = data["powerup_id"] as string;

			if (packages.ContainsKey(powerupID)) {
				// Update status of installed package if new version available.
				var package = packages[powerupID];
				var version = data["version"] as string;

				if (version != package.version) {
					package.status = Status.UpdateAvailable;
					package.nextVersion = version;
					packageToUpdate = package;
				}
			} else {
				// Signal that powerup package is available for downloading.
				packages[powerupID] = new Package(data, Status.NotInstalled);
				packageToUpdate = packages[powerupID];
			}
			
			if (installUpdates && packageToUpdate != null) {
				updates.Add(packageToUpdate);
			}
		}

		checkingForUpdates = false;
	}

	/// <summary>
	/// Retrieves from EditorPrefs the packages that are currently installed.
	/// </summary>
	/// <returns>The currently installed powerup packages.</returns>
	static Dictionary<string, Package> GetInstalledPackages ()
	{
		var installedPackages = new Dictionary<string, Package>();

		if (EditorPrefs.HasKey("lumos-installed-packages")) {
			var json = EditorPrefs.GetString("lumos-installed-packages");
			var packageData = LumosJson.Deserialize(json) as IList;

			foreach (Dictionary<string, object> data in packageData) {
				var powerupID = data["powerup_id"] as string;
				installedPackages[powerupID] = new Package(data, Status.Installed);
			}
		}

		return installedPackages;
	}
	
	public static void SetInstalledPackages () 
	{
		_packages = GetInstalledPackages();
	}

	/// <summary>
	/// Imports a downloaded package.
	/// </summary>
	/// <param name="path">The name of the downloaded file.</param>
	static void ImportPackage (string path)
	{
		var interactive = EditorPrefs.GetBool("lumos-interactive-import", false);
		AssetDatabase.ImportPackage(path, interactive);
	}

	/// <summary>
	/// Saves a list of installed packages to EditorPrefs.
	/// </summary>
	/// <param name="packages">The installed packages.</param>
	static void RecordInstalledPackages (Dictionary<string, Package> packages)
	{
		var toSerialize = new List<Dictionary<string, object>>();

		foreach (var entry in packages) {
			var dict = entry.Value.ToDictionary();
			dict["powerup_id"] = entry.Key;
			toSerialize.Add(dict);
		}

		var json = LumosJson.Serialize(toSerialize);
		EditorPrefs.SetString("lumos-installed-packages", json);
		Debug.Log("Editor prefs: " + json);
	}
}
