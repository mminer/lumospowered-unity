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
	#region Delegates

	/// <summary>
	/// Success handler.
	/// </summary>
	public delegate void SuccessHandler (object response);

	/// <summary>
	/// Error handler.
	/// </summary>
	public delegate void ErrorHandler (object response);

	#endregion

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
	public static Coroutine Send (string url, object parameters)
	{
		return Lumos.RunRoutine(SendCoroutine(url, parameters, null, null));
	}

	/// <summary>
	/// Sends data to Lumos' servers.
	/// </summary>
	public static Coroutine Send (string url, SuccessHandler successCallback)
	{
		return Lumos.RunRoutine(SendCoroutine(url, null, successCallback, null));
	}

	/// <summary>
	/// Sends data to Lumos' servers.
	/// </summary>
	/// <param name="successCallback">Callback to run on successful response.</param>
	public static Coroutine Send (string url, object parameters, SuccessHandler successCallback)
	{
		return Lumos.RunRoutine(SendCoroutine(url, parameters, successCallback, null));
	}

	/// <summary>
	/// Sends data to Lumos' servers.
	/// </summary>
	/// <param name="successCallback">Callback to run on successful response.</param>
	/// <param name="errorCallback">Callback to run on failed response.</param>
	public static Coroutine Send (string url, object parameters, SuccessHandler successCallback, ErrorHandler errorCallback)
	{
		return Lumos.RunRoutine(SendCoroutine(url, parameters, successCallback, errorCallback));
	}

	static Hashtable headers = new Hashtable()
	{
		{ "Content-Type", "application/json" }
	};

	/// <summary>
	/// Sends data to Lumos' servers.
	/// </summary>
	/// <param name="successCallback">Callback to run on successful response.</param>
	/// <param name="errorCallback">Callback to run on failed response.</param>
	static IEnumerator SendCoroutine (string url, object parameters, SuccessHandler successCallback, ErrorHandler errorCallback)
	{
		if (Application.isEditor && !Lumos.instance.runInEditor) {
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
		headers["Authorization"] = GenerateAuthorizationHeader(Lumos.credentials, postData);
		var www = new WWW(url, postData, headers);

		// Send info to server.
		yield return www;
		Lumos.Log("Request: " + postData);
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
}
