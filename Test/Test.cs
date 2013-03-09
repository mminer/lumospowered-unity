using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour
{
	string input = "";

	void Awake ()
	{
		//Lumos.debug = true;
	}

	void Start ()
	{
		TestHelper.SetLogsApiUrl("http://localhost:8090/api/1");
		Lumos.debug = true;
	}

	void OnGUI ()
	{
		input = GUILayout.TextField(input);

		if (GUILayout.Button("Error")) {
			Debug.LogError(input);
			input = "";
			LumosLogs.Send();
		}
	}
}
