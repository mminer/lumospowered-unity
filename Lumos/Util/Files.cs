using Ionic.Zip;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Lumos.Util
{
	/// <summary>
	/// Functions for manipulating and extracting information from files.
	/// </summary>
	public static class Files
	{
		/// <summary>
		/// Compresses a directory into a zip file.
		/// </summary>
		/// <param name="directory">The directory to zip.</param>
		/// <returns>The resulting zip file.</returns>
		public static string ZipDirectory (string directory)
		{
			// Filename = <directory name>.zip
			var filename = Path.GetDirectoryName(directory) + ".zip";
			
			try {
				using (var zip = new ZipFile()) {
					zip.AddDirectory(directory);
					zip.Save(filename);
					return filename;
				}
			} catch (Exception e) {
				Debug.Log("Zipping directory failed: " + e.Message);
				return null;
			}
		}
		
		/// <summary>
		/// Generates the MD5 hash for a file.
		/// </summary>
		/// <param name="filename">The full path to the file.</param>
		/// <returns>The file's MD5 hash.</returns>
		public static string MD5HashFromFile (string filename)
		{
			var file = new FileStream(filename, FileMode.Open);
			var md5 = new MD5CryptoServiceProvider();
			byte[] parts = md5.ComputeHash(file);
			file.Close();

			// Construct hex string from byte array.
			var hashBuilder = new StringBuilder();
			foreach (var part in parts) {
				hashBuilder.Append(part.ToString("X2"));
			}

			var hash = hashBuilder.ToString().ToLower();
			return hash;
		}
		
		/// <summary>
		/// Gets the MIME type of a file from its extension.
		/// </summary>
		/// <remarks>This only covers the file types used by Lumos.</remarks>
		/// <param name="file">The path to the file to get the content type of.</param>
		/// <returns>The content type (e.g. "application/octet-stream")./returns>
		public static string FileContentType (string file)
		{
			var extension = Path.GetExtension(file);
			Debug.Log(extension);
			
			switch (extension) {
				case ".zip":
					return "application/zip";
				case ".unity3d":
				default:
					// A generic catch-all for arbitrary binary files.
					return "application/octet-stream";
			}
			
			// Are zip and octet-stream the only file types we need to consider?
		}
	}
}
