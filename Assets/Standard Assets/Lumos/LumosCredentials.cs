// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using UnityEngine;

/// <summary>
/// Access credentials.
/// These are set in the Lumos pane in Unity's preferences.
/// </summary>
public class LumosCredentials : ScriptableObject
{
	string _gameID;

	/// <summary>
	/// A secret key used to authenticate the game with Lumos' servers.
	/// </summary>
	public string apiKey = "";

	/// <summary>
	/// A unique string identifying the game.
	/// </summary>
	public string gameID
	{
		get {
			if (_gameID == null) {
				try {
					_gameID = apiKey.Substring(0, 8);
				} catch (System.ArgumentOutOfRangeException) {
					Debug.LogError("The Lumos API key is not set. Do this in the Lumos pane in Unity's preferences.");
				}
			}

			return _gameID;
		}
	}
}

