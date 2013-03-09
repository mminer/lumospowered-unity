using UnityEngine;

public class LumosLogsComponent : MonoBehaviour
{
	#region Inspector Settings

	public bool recordLogs = false;
	public bool recordWarnings = true;
	public bool recordErrors = true;
	public bool recordInEditor = true;

	#endregion

	LumosLogsComponent () {}

	void Start ()
	{
		LumosLogs.recordLogs     = recordLogs;
		LumosLogs.recordWarnings = recordWarnings;
		LumosLogs.recordErrors   = recordErrors;
		LumosLogs.recordInEditor = recordInEditor;

		// Set up debug log redirect.
		Application.RegisterLogCallback(Record);

		// Add send to Lumos timed events.
		Lumos.timedEvents += LumosLogs.Send;
	}

	static void Record (string message, string trace, LogType type)
	{
		LumosLogs.Record(message, trace, type);
	}
}
