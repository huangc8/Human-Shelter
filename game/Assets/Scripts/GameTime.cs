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
		_conversationsLeft = 5;
	}

	/// <summary>
	/// Start a new day, reset all of your values that reset over night
	/// Complete all of the tasks the survivors were sent on
	/// </summary>
	public void newDay(){


		//process the tasks
		evaluateTasks();
	}

	void evaluateTasks(){
		for(int s = 0; s < _shelter.NumberOfSurvivors; s++){
			_shelter._survivors[s].AssignedTask = Survivor.task.Unassigned;
		}

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
		if (GUI.Button(new Rect(10, 30, 100, 30), _newDay)){
			newDay();
		}


		int startX = 200;
		int startY = 200;
		int itY;

		int buttonWidth = 300;
		int buttonHeight = 30;

		int numSurvivors = _shelter.NumberOfSurvivors;


		for(int i = 0; i < numSurvivors; i++){
			itY = startY;
			if(GUI.Button(new Rect(startX,itY,buttonWidth,buttonHeight), _shelter._survivors[i].Name)){}
			itY+= buttonHeight;
			if(GUI.Button(new Rect(startX,itY,buttonWidth,buttonHeight), "Converse: " + _shelter._survivors[i].ConversationsHad) && _conversationsLeft > 0){
				_shelter._survivors[i].Converse();
				_conversationsLeft--;
			}
			itY+=buttonHeight;
			for(int t = 0; t < (int) Survivor.task.Count; t++){
				if(GUI.Button(new Rect(startX,itY,buttonWidth,buttonHeight), ((Survivor.task)t).ToString())){
					_shelter._survivors[i].AssignedTask = ((Survivor.task)t);
				}
				itY +=buttonHeight;
			}
			
			if(GUI.Button(new Rect(startX,itY,buttonWidth,buttonHeight), "Assigned Task: " + _shelter._survivors[i].AssignedTask.ToString())){}
			startX += buttonWidth;
		}

	}


}
