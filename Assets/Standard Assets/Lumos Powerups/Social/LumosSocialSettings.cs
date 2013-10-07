using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LumosSocialSettings : MonoBehaviour 
{
	public static bool useGameCenter {
		protected set;
		get;
	}

	void Awake ()
	{
		Lumos.OnReady += Ready;
	}
	
	void Ready ()
	{
		LumosPlayer.GetPowerupSettings("social", LoadedSettings);
	}

	void LoadedSettings (Dictionary<string, object> settings)
	{
		var gameCenterKey = "use_game_center";
		
		if (settings != null && settings.ContainsKey(gameCenterKey)) {
			useGameCenter = System.Convert.ToBoolean(settings[gameCenterKey]);
		}
	}

	public static bool IsInitialized ()
	{
		GameObject lumosGO = GameObject.Find("Lumos");

		if (lumosGO == null) {
			Debug.LogWarning("The Lumos Game Object has not been added to your initial scene.");
			return false;
		}

		if (lumosGO.GetComponent<LumosSocialSettings>() == null) {
			Debug.LogWarning("The LumosSocialSettings script has not been added to the Lumos GameObject.");
			return false;
		}

		return true;
	}
}
