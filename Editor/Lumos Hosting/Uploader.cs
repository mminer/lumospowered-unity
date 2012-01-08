using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LumosPowered
{
	/// <summary>
	/// Facilitates building and uploading a game to Lumos.
	/// </summary>
	class Uploader
	{
		static readonly Dictionary<BuildTarget, string> buildTargetStrings = new Dictionary<BuildTarget, string>() {
			{ BuildTarget.WebPlayer,           "unity-web" },
			{ BuildTarget.StandaloneOSXIntel,  "unity-mac" },
			{ BuildTarget.StandaloneWindows,   "unity-win" },
			{ BuildTarget.StandaloneWindows64, "unity-win64" }
		};

		[MenuItem("Window/Lumos Deploy %#d")]
		/// <summary>
		/// Builds and uploads a game.
		/// </summary>
		public static void BuildAndUpload ()
		{
			var file = Uploader.Build();
			Uploader.Upload(file);
		}

		/// <summary>
		/// Builds the game, the target depending on Unity's build settings.
		/// </summary>
		/// <returns>The path to the built (and possibly compressed) game file.</returns>
		public static string Build ()
		{
			var target = EditorUserBuildSettings.activeBuildTarget;
			var extension = GetBuildTargetExtension(target);
			
			if (extension == null) {
				throw new Exception("Unsupported build target: " + target);
			}
			
			// Build game file.
			var buildLocation = FileUtil.GetUniqueTempPathInProject();
			var sceneQuery = from scene in EditorBuildSettings.scenes
							 where scene.enabled
							 let s = scene.path
							 select s;
			var scenes = sceneQuery.ToArray();
			BuildPipeline.BuildPlayer(scenes, buildLocation, target, BuildOptions.None);
			
			// Process file according to its type (web, standalone, etc.).
			var file = "";
			var name = PlayerSettings.productName.ToLower().Replace(' ', '_');
			
			switch (target) {
				case BuildTarget.WebPlayer:
					// Rename .unity3d file.
					var existingFile = Path.Combine(buildLocation, new DirectoryInfo(buildLocation).Name + ".unity3d");
					file = Path.Combine(buildLocation, name + ".unity3d");
					File.Move(existingFile, file);
					break;
				
				case BuildTarget.StandaloneOSXIntel:
					// Move directory contents to .app subdirectory.
					var files = Directory.GetFileSystemEntries(buildLocation);
					var app = Path.Combine(buildLocation, name + ".app");
					Directory.CreateDirectory(app);
					
					foreach (var f in files) {
						File.Move(f, Path.Combine(app, Path.GetFileName(f)));
					}
					
					// Create parent directory to hold .app and zip it.
					var parentDir = Path.Combine(buildLocation, name);
					Directory.CreateDirectory(parentDir);
					File.Move(app, Path.Combine(parentDir, new DirectoryInfo(app).Name));
					file = Files.ZipDirectory(parentDir);
					break;

				default:
					throw new Exception("Stop");
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
			string file;
			BuildTarget target;
			string checksum;
			string contentType;
			string date;
			
			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="file">The path to the file.</param>
			/// <param name="target">The build target./param>
			public FileUploader (string file, BuildTarget target)
			{
				this.file = file;
				this.target = target;
				this.checksum = Files.MD5HashFromFile(file);
				this.contentType = Files.FileContentType(file);
				this.date = Web.currentTimestamp;
				
				StartFetchUploadHeaders();
			}
			
			/// <summary>
			/// Asynchronously fetches the headers required to upload a file to the CDN.
			/// </summary>
			void StartFetchUploadHeaders ()
			{
				var parameters = new Dictionary<string, object>() {
					{ "filename", Path.GetFileName(file) },
					{ "checksum", checksum },
					{ "content_type", contentType },
					{ "date", date },
					{ "platform",  buildTargetStrings[target] }
				};

				var baseUrl = LumosHosting.apiUrl + "games/" + Preferences.gameId + "/uploadheaders";
				var url = new Uri(Web.ConstructGetUrl(baseUrl, parameters));
				var client = new WebClient();

				// Send authorization credentials.
				var credentials = Encoding.UTF8.GetBytes(Lumos.gameId + ":" + LumosHosting.password);
				client.Headers["Authorization"] = "Basic " + Convert.ToBase64String(credentials);

				// Start download.
				client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(FinishFetchUploadHeaders);
				client.DownloadStringAsync(url);
			}
			
			/// <summary>
			/// Callback once the header fetch has completed.
			/// </summary>
			/// <param name="sender">Unused; necessary to match delegate signature.</param>
			/// <param name="e">The response.</param>
			void FinishFetchUploadHeaders (object sender, DownloadStringCompletedEventArgs e)
			{
				if (e.Result != null) {
					var headers = JSON.JsonDecode(e.Result) as Hashtable;
					Debug.Log("Upload headers fetched: " + e.Result);

					// Now that we have our upload headers, start the action.
					StartUpload(headers);
				} else {
					Debug.LogError("Fetching upload headers failed: " + e.Error.Message + "; " + e.Error.StackTrace);
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
					var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
					var data = new byte[fileStream.Length];
					fileStream.Read(data, 0, (int)fileStream.Length);
					fileStream.Close();

					// Set up request.
					var url = headers["url"] as string;
					headers.Remove("url"); // Not to be sent as a header.

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
					return ".unity3d";
				case BuildTarget.StandaloneOSXIntel:
					return ".app";
				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64:
					return ".exe";
				default:
					return null;
			}
		}
	}
}
