// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Functions for accessing and creating Lumos credentials.
/// </summary>
public static class LumosCredentialsManager
{
	static LumosCredentials credentials;

	/// <summary>
	/// Gets the user's Lumos credentials, creating a new object if necessary.
	/// </summary>
	/// <returns>Lumos credentials.</returns>
	public static LumosCredentials GetCredentials ()
	{
		if (credentials == null) {
			credentials = LumosCredentials.Load();

			if (credentials == null) {
				credentials = CreateCredentials();
			}
		}

		return credentials;
	}
	
	public static bool HasCredentialsFile ()
	{
		credentials = LumosCredentials.Load();
		return credentials != null;
	}

	/// <summary>
	/// Generates a blank credentials file.
	/// </summary>
	/// <returns>A fresh Lumos credentials object.</returns>
	static LumosCredentials CreateCredentials ()
	{
		// Create the Resources directory if it doesn't already exist.
		Directory.CreateDirectory("Assets/Standard Assets/Lumos/Resources");

		// Create the credentials asset.
		var credentials = ScriptableObject.CreateInstance<LumosCredentials>();
		AssetDatabase.CreateAsset(credentials, "Assets/Standard Assets/Lumos/Resources/Credentials.asset");
		return credentials;
	}
}
