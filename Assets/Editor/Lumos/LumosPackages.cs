// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
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
		public readonly string powerupID;
		public readonly Uri url;

		public Status status { get; set; }
		public string version { get; set; }
		public string nextVersion { get; set; }

		public Package (Dictionary<string, object> data, Status status) {
			name = data["name"] as string;
			powerupID = data["powerup_id"] as string;
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
	static IList latestPackagesResponse = null;
	static bool checkedPrefs;
	static bool installing;
	static Dictionary<string, bool> installQueue = new Dictionary<string, bool>();
	public static string messageStatus = "";
	
	/// <summary>
	/// The powerup packages.
	/// </summary>
	public static Dictionary<string, Package> packages {
		get {
			if (_packages == null) {
				_packages = GetInstalledPackages();
				CompareLatestWithInstalled();
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
		if (latestPackagesResponse != null) {
			RecordLatestPackages();
			CompareLatestWithInstalled();
		}
		
		// Done here to get around main thread issues with async calls.
		while (importQueue.Count > 0) {
			var path = importQueue.Dequeue();
			ImportPackage(path);
			RecordInstalledPackages(packages);
			
			if (installing) {
				messageStatus = "";
			
				foreach (var package in installQueue) {
					if (package.Value) {
						installQueue.Remove(package.Key);
						break;
					}
				}
			}
		}
		
		if (!checkedPrefs) {
			installing = EditorPrefs.GetBool("lumos-installing", false);
			
			if (installing) {
				installQueue = GetInstallQueue();
				CompareLatestWithInstalled();
				
				if (installQueue.Count == 0 && packages.Count > 0) {
					installing = false;
					RunSetupScripts();
					EditorPrefs.SetBool("lumos-installing", false);
					EditorPrefs.DeleteKey("lumos-install-queue");
					EditorApplication.update -= MonitorImports;
					EditorWindow.GetWindow<LumosInstall>().Close();
				}
			} else {
				RunSetupScripts();
			}
		}
		
		// Used when installing Lumos
		if (installing && packages.Count > 0) {
			if (installQueue.Count == 0) {
				installQueue = GetInstallQueue();
			} else {
				if (messageStatus == "") {
					foreach (var package in installQueue) {
						var powerupID = package.Key;
						
						if (package.Value == false && packages.ContainsKey(powerupID)) {
							installQueue[package.Key] = true;
							messageStatus = "Updating " + packages[powerupID].name;
							UpdatePackage(packages[powerupID]);
							break;
						}
					}
				}
			}
		}
		
		checkedPrefs = true;
	}
	
	public static void UpdateAllPackages ()
	{
		EditorPrefs.SetBool("lumos-installing", true);
		installing = true;
		CheckForUpdates();
	}

	/// <summary>
	/// Downloads the latest powerup package.
	/// </summary>
	/// <param name="package">The powerup package to install.</param>
	public static void UpdatePackage (Package package)
	{
		package.status = Status.Downloading;

		// Construct local filename.
		var filename = Path.GetFileName(package.url.LocalPath);
		var path = Path.Combine("Temp", filename);

		// Download.
		var client = new WebClient();
		client.DownloadFileAsync(package.url, path);
		client.DownloadFileCompleted += delegate {
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
	/// Parses the results of checking for powerup package updates.
	/// </summary>
	static void CheckForUpdatesCallback (object sender, DownloadStringCompletedEventArgs e) {
		Debug.Log(e.Result);
		latestPackagesResponse = LumosJson.Deserialize(e.Result) as IList;	
		checkingForUpdates = false;
	}
	
	public static void CompareLatestWithInstalled ()
	{
		var latestPackages = GetLatestPackages();
		
		foreach (var latest in latestPackages.Values) {
			if (packages.ContainsKey(latest.powerupID)) {
				// Update status of installed package if new version available.
				var package = packages[latest.powerupID];
				var version = latest.version;

				if (version != package.version) {
					package.status = Status.UpdateAvailable;
					package.nextVersion = version;
				}
			} else {
				// Signal that powerup package is available for downloading.
				packages[latest.powerupID] = latest;
			}
		}
		
		RecordLumosInstallQueue();
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
	
	/// <summary>
	/// Retrieves from EditorPrefs the latest package info retrieved from the server.
	/// </summary>
	/// <returns>The currently installed powerup packages.</returns>
	static Dictionary<string, Package> GetLatestPackages ()
	{
		var latestPackages = new Dictionary<string, Package>();

		if (EditorPrefs.HasKey("lumos-latest-packages")) {
			var json = EditorPrefs.GetString("lumos-latest-packages");
			var packageData = LumosJson.Deserialize(json) as IList;

			foreach (Dictionary<string, object> data in packageData) {
				var powerupID = data["powerup_id"] as string;
				latestPackages[powerupID] = new Package(data, Status.NotInstalled);
			}
		}

		return latestPackages;
	}
	
	static Dictionary<string, bool> GetInstallQueue ()
	{
		var queue = new Dictionary<string, bool>();

		if (EditorPrefs.HasKey("lumos-install-queue")) {
			var json = EditorPrefs.GetString("lumos-install-queue");
			var packageData = LumosJson.Deserialize(json) as IList;
			
			if (packageData != null) {
				foreach (KeyValuePair<string, bool> data in packageData) {
					// Already installed, skip
					if (data.Value) {
						continue;
					}
					
					queue[data.Key] = data.Value;
				}
			}
		}

		return queue;
	}

	/// <summary>
	/// Imports a downloaded package.
	/// </summary>
	/// <param name="path">The name of the downloaded file.</param>
	static void ImportPackage (string path)
	{
		var interactive = EditorPrefs.GetBool("lumos-interactive-import", false);
		
		if (installing) {
			interactive = false;
		}
		
		AssetDatabase.ImportPackage(path, interactive);
	}
	
	static void RunSetupScripts ()
	{
		string targetAssembly = "Assembly-CSharp-firstpass";
		Assembly editor = null;
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		
		foreach (var assembly in assemblies) {
			var name = assembly.GetName().Name;
			
			if (name == targetAssembly) {
				editor = assembly;
				break;
			}
		}
		
		if (editor != null) {
			var q = from t in editor.GetTypes()
        		where t.IsClass && t.GetInterfaces().Contains(typeof(ILumosSetup))
        		select t;
			
			var setupScripts = q.ToList();
			
			if (setupScripts.Count > 0) {
				foreach (var setup in setupScripts) {
					var instance = Activator.CreateInstance(setup);
					Convert.ChangeType(instance, setup);
					setup.GetMethod("Setup").Invoke(instance, null);
				}
			}
		}
	}

	/// <summary>
	/// Saves a list of installed packages to EditorPrefs.
	/// </summary>
	/// <param name="packages">The installed packages.</param>
	static void RecordInstalledPackages (Dictionary<string, Package> packages)
	{
		var toSerialize = new List<Dictionary<string, object>>();

		foreach (var entry in packages) {
			if (entry.Value.status != Status.Installed) {
				continue;
			}
			
			var dict = entry.Value.ToDictionary();
			dict["powerup_id"] = entry.Key;
			toSerialize.Add(dict);
		}

		var json = LumosJson.Serialize(toSerialize);
		EditorPrefs.SetString("lumos-installed-packages", json);
	}
	
	/// <summary>
	/// Saves a list of the latest retrieved package information to EditorPrefs.
	/// </summary>
	/// <param name="packages">The installed packages.</param>
	static void RecordLatestPackages ()
	{
		var packages = new Dictionary<string, Package>();
		
		foreach (Dictionary<string, object> data in latestPackagesResponse) {
			var powerupID = data["powerup_id"] as string;
			packages[powerupID] = new Package(data, Status.NotInstalled);
		}
		
		var toSerialize = new List<Dictionary<string, object>>();

		foreach (var entry in packages) {
			var dict = entry.Value.ToDictionary();
			dict["powerup_id"] = entry.Key;
			toSerialize.Add(dict);
		}

		var json = LumosJson.Serialize(toSerialize);
		EditorPrefs.SetString("lumos-latest-packages", json);
		latestPackagesResponse = null;
	}
	
	static void RecordLumosInstallQueue ()
	{
		var queue = new Dictionary<string, bool>();
		
		foreach (var package in packages.Values) {
			if (package.status == Status.NotInstalled || package.status == Status.UpdateAvailable) {
				queue.Add(package.powerupID, false);
			}
		}
		
		var json = LumosJson.Serialize(queue);
		EditorPrefs.SetString("lumos-install-queue", json);
		
		installQueue = queue;
	}
}
