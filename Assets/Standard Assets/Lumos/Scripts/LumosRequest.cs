// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

/// <summary>
/// Functionality for communicating with Lumos' servers.
/// </summary>
public class LumosRequest
{
	// Without parameters:

	/// <summary>
	/// Sends data to Lumos' servers.
	/// </summary>
	public static Coroutine Send (string url)
	{
		return Lumos.RunRoutine(SendCoroutine(url, null, null, null));
	}

	/// <summary>
	/// Sends data to Lumos' servers.
	/// </summary>
	public static Coroutine Send (string url, Action<object> successCallback)
	{
		return Lumos.RunRoutine(SendCoroutine(url, null, successCallback, null));
	}

	/// <summary>
	/// Sends data to Lumos' servers.
	/// </summary>
	public static Coroutine Send (string url, Action<object> successCallback, Action<object> errorCallback)
	{
		return Lumos.RunRoutine(SendCoroutine(url, null, successCallback, errorCallback));
	}

	// With parameters:

	/// <summary>
	/// Sends data to Lumos' servers.
	/// </summary>
	public static Coroutine Send (string url, object parameters)
	{
		return Lumos.RunRoutine(SendCoroutine(url, parameters, null, null));
	}

	/// <summary>
	/// Sends data to Lumos' servers.
	/// </summary>
	/// <param name="successCallback">Callback to run on successful response.</param>
	public static Coroutine Send (string url, object parameters, Action<object> successCallback)
	{
		return Lumos.RunRoutine(SendCoroutine(url, parameters, successCallback, null));
	}

	/// <summary>
	/// Sends data to Lumos' servers.
	/// </summary>
	/// <param name="successCallback">Callback to run on successful response.</param>
	/// <param name="errorCallback">Callback to run on failed response.</param>
	public static Coroutine Send (string url, object parameters, Action<object> successCallback, Action<object> errorCallback)
	{
		return Lumos.RunRoutine(SendCoroutine(url, parameters, successCallback, errorCallback));
	}

	/// <summary>
	/// Sends data to Lumos' servers.
	/// </summary>
	/// <param name="successCallback">Callback to run on successful response.</param>
	/// <param name="errorCallback">Callback to run on failed response.</param>
	static IEnumerator SendCoroutine (string url, object parameters, Action<object> successCallback, Action<object> errorCallback)
	{
		if (Application.isEditor && !Lumos.runInEditor) {
			yield break;
		}

		// Skip out early if there's no internet connection.
		if (Application.internetReachability == NetworkReachability.NotReachable) {
			if (errorCallback != null) {
				errorCallback(null);
			}

			yield break;
		}

		var postData = SerializePostData(parameters);

		var headers = new Hashtable() {
			{ "Content-Type", "application/json" },
			{ "Authorization", GenerateAuthorizationHeader(Lumos.credentials, postData) }
		};
		var www = new WWW(url, postData, headers);

		// Send info to server.
		yield return www;
		Lumos.Log("Request: " + url + "\n" + Encoding.Default.GetString(postData));
		Lumos.Log("Response: " + www.text);

		// Parse the response.
		var response = LumosJson.Deserialize(www.text);

		if (www.error == null) {
			if (successCallback != null) {
				successCallback(response);
			}
		} else {
			Lumos.Log("Error: " + www.error);
			if (errorCallback != null) {
				errorCallback(response);
			}
		}
	}

	/// <summary>
	/// Converts parameters into a JSON string to put in POST request's body.
	/// </summary>
	/// <param name="parameters">Information to send in the request.</param>
	/// <returns>A byte array suitable for sending over the wire.</returns>
	public static byte[] SerializePostData (object parameters)
	{
		string json;

		if (parameters == null) {
			json = "{}";
		} else {
			json = LumosJson.Serialize(parameters);
		}

		var postData = Encoding.ASCII.GetBytes(json);
		return postData;
	}

	/// <summary>
	/// Creates an authorization header.
	/// </summary>
	/// <param name="credentials">Lumos credentials object.</param>
	/// <param name="postData">The POST body.</param>
	/// <returns>A string suitable for the HTTP Authorization header.</returns>
	public static string GenerateAuthorizationHeader (LumosCredentials credentials, byte[] postData)
	{
		if (postData == null) {
			postData = new byte[] {};
		}

		var secret = Encoding.ASCII.GetBytes(credentials.apiKey);
		var hmac = new HMACSHA1(secret);
		var hash = hmac.ComputeHash(postData);
		var auth = Convert.ToBase64String(hash);
		var header = "Lumos " + credentials.gameID + ":" + auth;
		return header;
	}

	/// <summary>
	/// Loads a remote image into a texture.
	/// </summary>
	/// <param name="imageLocation">The URL the image resides at.</param>
	/// <param name="texture">The URL the image resides at.</param>
	public static Coroutine LoadImage (string imageLocation, Texture2D texture)
	{
		return Lumos.RunRoutine(LoadImageCoroutine(imageLocation, texture));
	}

	/// <summary>
	/// Loads a remote image into a texture.
	/// </summary>
	/// <param name="imageLocation">The URL the image resides at.</param>
	/// <param name="texture">The URL the image resides at.</param>
	static IEnumerator LoadImageCoroutine (string imageLocation, Texture2D texture)
	{
		var www = new WWW(imageLocation);
		yield return www;

		try {
			if (www.error != null) {
				throw new Exception(www.error);
			}

			www.LoadImageIntoTexture(texture);
		} catch (Exception e) {
			Lumos.LogError("Failed to load achievement image: " + e.Message);
		}
	}
}
