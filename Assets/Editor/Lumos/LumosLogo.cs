using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
class LumosLogo 
{
    static Texture2D logo;
    static int prefabID = -1;
 
	// Init
    static LumosLogo ()
    {
       logo = AssetDatabase.LoadAssetAtPath ("Assets/Standard Assets/Lumos/icon.png", typeof(Texture2D)) as Texture2D;
       EditorApplication.update += EditorUpdate;
    }
 
    static void EditorUpdate ()
    {
		if (prefabID == -1) {
			GameObject lumosPrefab = GameObject.Find("Lumos");
			
			if (lumosPrefab != null) {
				prefabID = lumosPrefab.GetInstanceID();
				EditorApplication.hierarchyWindowItemOnGUI += HierarchyItem;
				EditorApplication.update -= EditorUpdate;
			}
		} 
    }
 
    static void HierarchyItem (int instanceID, Rect selectionRect)
    {
		// place the icon to the right of the lumos prefab
		if (prefabID == instanceID) {
			Rect r = new Rect(selectionRect); 
			r.x = r.width - 2;
			r.width = 18;
			GUI.Label(r, logo);	
		}

    }
}
