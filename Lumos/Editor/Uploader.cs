using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Lumos
{
	/// <summary>
	/// Facilitates building and uploading a game to Lumos.
	/// </summary>
	public class Uploader
	{
		static List<string> tempDirs = new List<string>();

		[MenuItem("Window/Lumos Deploy %#d")]
		/// <summary>
		/// Builds and uploads a game.
		/// </summary>
		public static void BuildAndUpload ()
		{
			var file = Build();
			Upload(file);
		}
		
		/// <summary>
		/// Builds the game, the target depending on Unity's build settings.
		/// </summary>
		public static string Build ()
		{
			// Build player.
			var target = EditorUserBuildSettings.activeBuildTarget;
			var extension = GetBuildTargetExtension(target);

			if (extension == null) {
				Debug.LogError("Unsupported build target: " + target);
			}

			var name = PlayerSettings.productName + extension;
			var location = Path.Combine(GetTempDir(), name);
			var sceneQuery = from scene in EditorBuildSettings.scenes
							 where scene.enabled
							 let s = scene.path
							 select s;
			var scenes = sceneQuery.ToArray();
			BuildPipeline.BuildPlayer(scenes, location, target, BuildOptions.None);

			// TODO Create archive of exported file if necessary.

			// Return the name of the created file.
			string file = "";
			switch (target) {
				case BuildTarget.WebPlayer:
					file = Path.Combine(location, name + ".unity3d");
					break;
				default:
					file = location;
					break;
			}

			return file;
		}
		
		/// <summary>
		/// Uploads the specified file.
		/// </summary>
		/// <param name="file">The file to upload.</param>
		public static void Upload (string file)
		{
			var target = EditorUserBuildSettings.activeBuildTarget;
			Upload(file, target);
		}
		
		/// <summary>
		/// Uploads the specified file with the provided build target.
		/// </summary>
		/// <param name="file">The file to upload.</param>
		/// <param name="target">The build target.</param>
		public static void Upload (string file, BuildTarget target)
		{
			new FileUploader(file, target);
		}
		
		/// <summary>
		/// Helper class for uploading a built game.
		/// </summary>
		class FileUploader
		{
			string path;
			BuildTarget target;
			string resource;
			string checksum;
			string contentType;
			string date;

			public FileUploader (string path, BuildTarget target)
			{
				this.path = path;
				this.target = target;
				this.resource = "/games/" + Path.GetFileName(path);
				this.checksum = Util.GetMD5HashFromFile(path);
				this.contentType = "application/octet-stream";
				this.date = Util.currentTimestamp;
				
				StartFetchUploadHeaders();
			}
			
			/// <summary>
			/// Asynchronously fetches the headers required to upload a file to the CDN.
			/// </summary>
			void StartFetchUploadHeaders ()
			{
				var parameters = new Dictionary<string, object>() {
					{ "api_key", "asd" },
					{ "resource", resource },
					{ "checksum", checksum },
					{ "content_type", contentType },
					{ "date", date }
				};

				var baseUrl = Preferences.lumosUrl + "api/upload/headers";
				var url = new Uri(Util.ConstructGetUrl(baseUrl, parameters));
				var client = new WebClient();
				client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(FinishFetchUploadHeaders);
				client.DownloadStringAsync(url);
			}
			
			/// <summary>
			/// Callback once the header fetch has completed.
			/// </summary>
			/// <param name="sender">Unused; necessary to match delegate signature./param>
			/// <param name="e">The response.</param>
			void FinishFetchUploadHeaders (object sender, DownloadStringCompletedEventArgs e)
			{
				if (e.Result != null) {
					var headers = JSON.JsonDecode(e.Result) as Hashtable;
					Debug.Log("Upload headers fetched: " + e.Result);

					// Now that we have our upload headers, start the action.
					StartUpload(headers);
				} else {
					Debug.LogError("Fetching upload info failed: " + e.Error.Message + "; " + e.Error.StackTrace);
				}
			}
			
			/// <summary>
			/// Starts the upload to the CDN.
			/// </summary>
			/// <param name="headers">HTTP headers to send with the request.</param>
			void StartUpload (Hashtable headers)
			{
				try {
					// Prepare file.
					var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
					var data = new byte[fileStream.Length];
					fileStream.Read(data, 0, (int)fileStream.Length);
					fileStream.Close();

					// Set up request.
					var url = "http://" + headers["Host"] + resource;
					headers.Remove("Host"); // Can't include host as a custom header.

					var request = HttpWebRequest.Create(url) as HttpWebRequest;
					request.Method = "PUT";
					request.ContentType = contentType;
					request.ContentLength = data.Length;

					// Add custom headers to request.
					foreach (DictionaryEntry header in headers) {
						request.Headers.Add(header.Key.ToString(), header.Value.ToString());
					}

					// Write file to request's stream.
					using (var stream = request.GetRequestStream()) {
						stream.Write(data, 0, data.Length);
						stream.Close();
					}

					// Initiate request.
					request.BeginGetResponse(new AsyncCallback(FinishUpload), request);
				} catch (WebException e) {
					Debug.Log("Start upload failed: " + e.Message + " Status: " + e.Status);
				}
			}
			
			/// <summary>
			/// Callback once the upload has completed.
			/// </summary>
			/// <param name="result">The result of the request.</param>
			void FinishUpload (IAsyncResult result)
			{
				try {
					var request = result.AsyncState as HttpWebRequest;
					var response = request.EndGetResponse(result) as HttpWebResponse;
					var stream = response.GetResponseStream();
					var buffer = new byte[response.ContentLength];
					stream.Read(buffer, 0, (int)response.ContentLength);
					stream.Close();
					var output = Encoding.UTF8.GetString(buffer);
					Debug.Log("Upload completed; output: " + output);
				} catch (WebException e) {
					Debug.Log("Finish upload failed: " + e.Message + " Status: " + e.Status);

					/*
					var error = e.Response;
					var bytes = new byte[(int)error.ContentLength];

					using (Stream stream = error.GetResponseStream()) {
						stream.Read(bytes, 0, (int)error.ContentLength);
						stream.Close();
					}

				
					Debug.Log(Encoding.UTF8.GetString(bytes));*/
				}
			}
		}
		
		/// <summary>
		/// Gets the appropriate extension for the given build target.
		/// </summary>
		/// <param name="target">The build target.</param>
		/// <returns>The build file extension.</returns>
		static string GetBuildTargetExtension (BuildTarget target)
		{
			switch (target) {
				case BuildTarget.WebPlayer:
					// Build outputs .unity3d file; explicitly setting extension unnecessary.
					return "";
				case BuildTarget.StandaloneOSXIntel:
					return ".app";
				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64:
					return ".exe";
				default:
					return null;
			}
		}
		
		/// <summary>
		/// Creates a temporary directory.
		/// </summary>
		/// <remarks>This folder gets removed whenever Unity does its cleanup run.</remarks>
		/// <returns>The temporary directory.</returns>
		static string GetTempDir ()
		{
			var tempDir = FileUtil.GetUniqueTempPathInProject();
			tempDirs.Add(tempDir);
			return tempDir;
		}
		
		/// <summary>
		/// Removes temporary directories used to build and upload games.
		/// </summary>
		public static void RemoveTempFiles ()
		{
			foreach (var dir in tempDirs) {
				FileUtil.DeleteFileOrDirectory(dir);
			}

			Debug.Log("Temporary files removed.");
		}
	}
}
