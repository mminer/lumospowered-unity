// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Functionality for communicating with Lumos' servers.
/// </summary>
public class LumosRequest
{
	public delegate void SuccessHandler(object response);
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

	static readonly Hashtable headers = new Hashtable()
	{
		{ "Content-Type", "application/json" }
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
}
