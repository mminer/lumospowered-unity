using UnityEngine;
using System.Collections;

public class LumosAnalyticsSetup : ILumosSetup {

	public void Setup ()
	{
		var lumos = GameObject.Find("Lumos");
		
		if (lumos.GetComponent<LumosAnalytics>() == null) {
			lumos.AddComponent<LumosAnalytics>();
		}
		
		Debug.Log("lumos analytics setup complete!");
	}
}
