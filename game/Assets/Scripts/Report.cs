/// <summary>
/// Report. Contains information on success of each task
/// </summary>
using UnityEngine;
using System.Collections;

public class Report : ScriptableObject
{
		string _message = "ERROR-- Message has not been set --";


		public void SetMessage (string message)
		{
				_message = message;
		}

	public bool IsInitialized(){
		return "ERROR-- Message has not been set --" == _message;
	}

		public string GetMessage ()
		{
		if(_message == null || _message == ""){
			Debug.LogError("19");
		}
				return _message;
		}

		public void Log ()
		{
				Debug.Log (_message);
		}
}
