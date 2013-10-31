// Copyright (c) 2013 Rebel Hippo Inc. All rights reserved.

using UnityEditor;
using UnityEngine;

/// <summary>
/// Menus for various functionality.
/// </summary>
public static class LumosMenus
{
	const string websiteURL = "https://www.lumospowered.com/";

	/// <summary>
	/// Adds a Lumos menu item to the Window menu.
	/// Triggers a wizard prompting the user for their secret key before instantiating the Lumos prefab.
	/// </summary>
	[MenuItem("GameObject/Create Other/Lumos...")]
	static void AddToScene ()
	{
		EditorWindow.GetWindow<LumosInstall>(true, "Install Window");
	}

	/// <summary>
	/// Validates the Lumos menu item, disabling it if an instance already exists in the scene.
	/// </summary>
	/// <returns>Whether or not the menu is enabled.</returns>
	[MenuItem("GameObject/Create Other/Lumos...", true)]
	static bool ValidateAddToScene ()
	{
		var go = GameObject.Find("Lumos");
		return go == null;
	}

	/// <summary>
	/// Adds a link to the Lumos website to the Help menu.
	/// </summary>
	[MenuItem("Help/Lumos Website")]
	static void OpenWebsite ()
	{
		Help.BrowseURL(websiteURL);
	}
}
