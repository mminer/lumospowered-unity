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
	/// <summary>
	/// Success handler.
	/// </summary>
	public delegate void SuccessHandler(object response);
	/// <summary>
	/// Error handler.
	/// </summary>
	public delegate void ErrorHandler(object response);

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

		string json;

		if (parameters == null) {
			json = "{}";
		} else {
			json = LumosJson.Serialize(parameters);
		}
		
		var postData = Encoding.ASCII.GetBytes(json);
		var secret = Encoding.ASCII.GetBytes(Lumos.instance.apiKey);
		
		var hmac = new HMACSHA1(secret);
		hmac.Initialize();
		
		var hash = hmac.ComputeHash(postData);
		var auth = Convert.ToBase64String(hash);
		
		headers["Authorization"] = "Lumos " + Lumos.gameId + ":" + auth;

		var www = new WWW(url, postData, headers);

		// Send info to server.
		yield return www;
		Lumos.Log("Request: " + json);
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
	
	public static byte[] GetJSON (object parameters=null)
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
	
	public static Hashtable GetHeaders (byte[] postData)
	{
		var secret = Encoding.ASCII.GetBytes("72b6ff39-aec1-4939-8fb4-fa3a6ec2ea50");
		
		var hmac = new HMACSHA1(secret);
		hmac.Initialize();
		
		var hash = hmac.ComputeHash(postData);
		var auth = Convert.ToBase64String(hash);
		
		headers["Authorization"] = "Lumos " + "72b6ff39" + ":" + auth;
		
		return headers;
	}
}
