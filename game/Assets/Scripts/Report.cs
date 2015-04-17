/// <summary>
/// Report. Contains information on success of each task
/// </summary>
using UnityEngine;
using System.Collections;

public class Report : ScriptableObject
{
	string _message = "ERROR-- Message has not been set --";

	bool isInitialized = false;

	public void SetMessage (string message)
	{
		isInitialized = true;
		_message = message;
		Debug.LogWarning ("Message has been set to:" + _message);
	}

	public void AddWoundMessage (Survivor.wound sustainedWound)
	{
		_message += " In the process a " + sustainedWound + " was sustained.";
	}

	public void PrintReport(int i){
		Debug.Log ("Report [" + i + "]: " + _message);
	}

	public bool IsInitialized(){
		return isInitialized;
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
