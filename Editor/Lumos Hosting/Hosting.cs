using UnityEditor;

namespace LumosPowered
{
	public class Hosting
	{
		//public const string apiUrl = "http://hosting.lumospowered.com/api/1/";
		public const string apiUrl = "http://localhost:8085/api/1/";
		public static string password;

		[MenuItem("Window/Lumos Deploy %#d")]
		/// <summary>
		/// Builds and uploads a game.
		/// </summary>
		public static void BuildAndUpload ()
		{
			var file = Uploader.Build();
			Uploader.Upload(file);
		}
	}
}
