using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;
using System.Collections;
using System.Collections.Generic;

public class LumosSocialSettings : MonoBehaviour 
{
	public static bool useGameCenter {
		protected set;
		get;
	}
	
	public static GameCenterPlatform gameCenterPlatform {
		protected set;
		get;
	}

	void Awake ()
	{
		Lumos.OnReady += Ready;
	}
	
	void Ready ()
	{
		// For now Social settings are only used for Game Center
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			LumosPlayer.GetPowerupSettings("social", LoadedSettings);
		}
	}

	void LoadedSettings (Dictionary<string, object> settings)
	{
		var gameCenterKey = "use_game_center";
		
		if (settings != null && settings.ContainsKey(gameCenterKey)) {
			useGameCenter = System.Convert.ToBoolean(settings[gameCenterKey]);
		}
		
		if (useGameCenter && Application.platform == RuntimePlatform.IPhonePlayer) {			
			gameCenterPlatform = new GameCenterPlatform();
			
			gameCenterPlatform.localUser.Authenticate(success => {
				if (success) {
					Lumos.Log("Authenticated with game center.");
				}
			});
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
