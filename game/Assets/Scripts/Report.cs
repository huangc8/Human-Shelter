/// <summary>
/// Report. Contains information on success of each task
/// </summary>
using UnityEngine;
using System.Collections;

public class Report : MonoBehaviour{
	string _message = "";
	public void SetMessage(string message){
		_message = message;
	}
}
