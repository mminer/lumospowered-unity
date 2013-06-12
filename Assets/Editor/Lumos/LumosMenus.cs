// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using UnityEditor;
using UnityEngine;

/// <summary>
/// Menus for various functionality.
/// </summary>
public static class LumosMenus
{
	const string supportUrl = "http://support.lumospowered.com/";

	/// <summary>
	/// Adds "Add To Scene" menu item to the Window menu.
	/// This triggers a wizard which prompts the user for their secret key before instantiating the Lumos prefab.
	/// </summary>
	[MenuItem("GameObject/Create Other/Lumos...")]
	static void AddToScene ()
	{
        var installWindow = (LumosInstall)EditorWindow.GetWindow(typeof(LumosInstall));
		installWindow.title = "Install Lumos";
		installWindow.ShowUtility();
	}

	/// <summary>
	/// Validates the "Add To Scene" menu item, disabling it if a Lumos instance already exists in the scene.
	/// </summary>
	/// <returns>Whether or not the menu is enabled.</returns>
	[MenuItem("GameObject/Create Other/Lumos...", true)]
	static bool ValidateAddToScene ()
	{
		var go = GameObject.Find("Lumos");
		return go == null;
	}

	/// <summary>
	/// Adds link to Lumos support website to Help menu.
	/// </summary>
	[MenuItem("Help/Lumos/Support")]
	static void DisplaySupportSite ()
	{
		Help.BrowseURL(supportUrl);
	}

	/// <summary>
	/// Adds menu item to open the Lumos bug reporter.
	/// </summary>
	[MenuItem("Help/Lumos/Report a Bug")]
	static void OpenBugReporter ()
	{
		// Get existing open window, or make new one if none
		var window = EditorWindow.GetWindow<LumosBugReporter>();
		window.title = "Report Bug";
		window.Show();
	}
}
