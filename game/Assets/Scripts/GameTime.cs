/// <summary>
/// Game time. This class controls the flow of time in our game. Time progresses
/// at a rate determined by the user as they complete tasks
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameTime : MonoBehaviour {
	Shelter _shelter;
	Queue <Event> _events;
	string _newDay;

	public int _conversationsLeft;
	// Use this for initialization
	void Start () {
		newDay();
	}

	/// <summary>
	/// Start a new day, reset all of your values that reset over night
	/// Complete all of the tasks the survivors were sent on
	/// </summary>
	public void newDay(){
		_conversationsLeft = 5;
	}

	// Update is called once per frame
	void Update () {
		if(_shelter == null){
			_shelter = this.GetComponent<Shelter>();
		}

		if(_conversationsLeft > 0){
			_newDay = "New Day";
		}
		else{
			_newDay = "New Day*";
		}
	}

	void OnGUI(){
		if (GUI.Button(new Rect(10, 70, 200, 100), _newDay)){
			newDay();
		}

		if (GUI.Button(new Rect(110, 70, 200, 100), "Have Conversation" + _conversationsLeft)){
			if(_conversationsLeft > 0){
				_conversationsLeft--;
			}
		}

		/*int startX;
		int startY;*/
		foreach(Survivor s in _shelter._survivors){
			//if(GUI.Button())
		}

	}


}
