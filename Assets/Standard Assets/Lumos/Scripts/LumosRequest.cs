// Copyright (c) 2012 Rebel Hippo Inc. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Functionality for communicating with Lumos' servers.
/// </summary>
public class LumosRequest
{
	public delegate void SuccessHandler();
	public delegate void ErrorHandler();

	/// <summary>
	/// The last response returned by the server.
	/// </summary>
	public static object lastResponse { get; private set; }
	
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
	public static Coroutine Send (string url, Dictionary<string, object> parameters)
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
	public static Coroutine Send (string url, Dictionary<string, object> parameters, SuccessHandler successCallback)
	{
		return Lumos.RunRoutine(SendCoroutine(url, parameters, successCallback, null));
	}

	/// <summary>
	/// Sends data to Lumos' servers.
	/// </summary>
	/// <param name="successCallback">Callback to run on successful response.</param>
	/// <param name="errorCallback">Callback to run on failed response.</param>
	public static Coroutine Send (string url, Dictionary<string, object> parameters, SuccessHandler successCallback, ErrorHandler errorCallback)
	{
		return Lumos.RunRoutine(SendCoroutine(url, parameters, successCallback, errorCallback));
	}

#if !UNITY_FLASH

	static readonly Hashtable headers = new Hashtable() {
		{ "Content-type", "application/json" }
	};

	/// <summary>
	/// Sends data to Lumos' servers.
	/// </summary>
	/// <param name="successCallback">Callback to run on successful response.</param>
	/// <param name="errorCallback">Callback to run on failed response.</param>
	static IEnumerator SendCoroutine (string url, Dictionary<string, object> parameters, SuccessHandler successCallback, ErrorHandler errorCallback)
	{
		if (Application.isEditor && !Lumos.instance.runInEditor) {
			yield break;
		}

		// Skip out early if there's no internet connection
		if (Application.internetReachability == NetworkReachability.NotReachable) {
			if (errorCallback != null) {
				errorCallback();
			}

			yield break;
		}
		
		var json = LumosJSON.Json.Serialize(parameters);
		
		// All requests (including GET) are sent as POST
		// and require parameters to be sent
		if (json == null) {
			json = "{}";
		}
		
		var postData = Encoding.ASCII.GetBytes(json);
		var www = new WWW(url, postData, headers);

		// Send info to server
		yield return www;
		Lumos.Log("Request: " + json);
		Lumos.Log("Response: " + www.text);
		
		Debug.Log(www.text);

		// Parse the response
		try {
			if (www.error != null) {
				throw new Exception(www.error);
			}

			var response = LumosJSON.Json.Deserialize(www.text);
			lastResponse = response;			

			if (successCallback != null) {
				successCallback();
			}
		} catch (Exception e) {
			Lumos.LogError("Failure: " + e.Message);
			Debug.Log("Failure: " + e.Message);
			
			if (errorCallback != null) {
				errorCallback();
			}
		}
	}

#else

	static IEnumerator SendCoroutine (string method, Dictionary<string, object> parameters, SuccessHandler successCallback, ErrorHandler errorCallback) { yield break; }

#endif

}

