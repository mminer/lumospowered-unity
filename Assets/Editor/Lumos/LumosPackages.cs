// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Manages the retrieval and status of powerup packages.
/// </summary>
public static class LumosPackages
{	
	static readonly Uri updatesUrl = new Uri("https://www.lumospowered.com/api/1/powerups/updates?engine=unity");
	static readonly Uri subscribedUrl = new Uri("https://www.lumospowered.com/api/1/powerups/files?engine=unity");
	public static string latestVersion;
	public enum Update { None, CheckingVersion, OutOfDate, UpToDate };
	public static Update package;
	public static List<string> subscriptions = new List<string>();
	static List<Type> setupScripts;
	
	/// <summary>
	/// Checks if there is a newer Lumos package.
	/// </summary>
	public static void CheckForUpdates ()
	{
		package = Update.CheckingVersion;
		
		DoRequest(updatesUrl, delegate (string result) {
			latestVersion = LumosJson.Deserialize(result) as string;
			var outOfDate = IsOutdated(Lumos.version, latestVersion);
			package = (outOfDate) ? Update.OutOfDate : Update.UpToDate;
		});
	}
	
	/// <summary>
	/// Checks which powerups this game is subscribed to.
	/// </summary>
	public static void CheckSubscribedPowerups ()
	{
		DoRequest(subscribedUrl, delegate (string result) {
			var response = LumosJson.Deserialize(result) as IList;
			subscriptions = new List<string>();

		    foreach (Dictionary<string, object> data in response) {
	            var powerupID = data["powerup_id"] as string;
				subscriptions.Add(powerupID);
		    }
			
			DoSetups();
		});
	}
	
	static void DoRequest (Uri uri, Action<string> callback)
	{
		var request = WebRequest.Create(uri);
		request.ContentType = "application/json; charset=utf-8";
		
		var authorizationHeader =
			LumosRequest.GenerateAuthorizationHeader(LumosCredentialsManager.GetCredentials(), null);
		request.Headers.Add("Authorization", authorizationHeader);
		
		var failedSSLCallback = new RemoteCertificateValidationCallback(delegate { return true; });
		ServicePointManager.ServerCertificateValidationCallback += failedSSLCallback;
		
		string text;
		
		try {
			var response = (HttpWebResponse) request.GetResponse();
			using (var sr = new StreamReader(response.GetResponseStream())) {
			    text = sr.ReadToEnd();
				callback(text);
			}
		} catch (WebException e) {
			Lumos.Log("Web exception: " + e.Message);
		} finally {
			ServicePointManager.ServerCertificateValidationCallback -= failedSSLCallback;
		}
	}
	
	static bool IsOutdated (String current, String latest)
    {
        var vA = new Version(current);
        var vB = new Version(latest);
		var diff = vA.CompareTo(vB);
        return (diff < 0) ? true : false;
    }

	public static void RunSetupScripts ()
	{
		var targetAssembly = "Assembly-CSharp-firstpass";
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

			setupScripts = q.ToList();

			if (setupScripts.Count > 0) {
				CheckSubscribedPowerups();
			}
		}
	}
	
	static void DoSetups ()
	{
		foreach (var setup in setupScripts) {
			var instance = Activator.CreateInstance(setup);
			Convert.ChangeType(instance, setup);
			
			var powerupID = setup.GetProperty("powerupID").GetValue(instance, null) as string;
			
			if (subscriptions.Contains(powerupID)) {
				setup.GetMethod("Setup").Invoke(instance, null);	
			}
		}
	}
}
