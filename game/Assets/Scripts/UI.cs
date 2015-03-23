﻿using UnityEngine;
using System.Collections;

public class UI : MonoBehaviour {

	public GameObject g;
	Shelter _shelter; // the shelter data

	//these are needed for showing and hiding buttons
	private bool charButtons, sideButtons, showButtons;
	private float x, y;
	private int index;

	// Use this for initialization
	void Start () {
		if (_shelter == null) {
			_shelter = g.GetComponent<Shelter> ();
		}
		charButtons = sideButtons = showButtons = false;

		index = 0;
	}

	public void charClick(int aindex, float ax, float ay)
	{
		//center buttons
		//not using this at the moment because it doesn't really work
		//GUIStyle style = GUI.skin.GetStyle("align");
		//style.alignment = TextAnchor.MiddleCenter;

		x = ax;
		y = ay;
		index = aindex;
		
		charButtons = true;
		showButtons = true;


	}

	void OnGUI (){
		int buttonWidth = 300;
		int buttonHeight = 30;

		string name = _shelter._survivors[index].Name;

		if(charButtons){

			if (GUI.Button (new Rect (Screen.width*x*.8f, Screen.height*y*.8f, buttonWidth, buttonHeight),"Talk to " + name)) {
				showButtons=true;

				_shelter._survivors [index].Converse ();
				_shelter._survivors[index]._conversationsLeft--;
			}
			if (GUI.Button (new Rect (Screen.width*x*.8f, Screen.height*y*.85f, buttonWidth, buttonHeight),"Assign task")) {
				showButtons=true;
				sideButtons=true;
			}
		}

		if (sideButtons) {

			float itY = Screen.height*.1f;
			float xLoc = Screen.width*.8f;

			if (GUI.Button (new Rect (xLoc, itY, buttonWidth, buttonHeight), name)) {
				showButtons=true;

			}

			itY += buttonHeight;
			for (int t = 0; t < (int) Survivor.task.Count; t++) {
				if (GUI.Button (new Rect (xLoc, itY, buttonWidth, buttonHeight), ((Survivor.task)t).ToString ())) {
					showButtons=true;
					_shelter._survivors [index].AssignedTask = ((Survivor.task)t);
				}
				itY += buttonHeight;
			}
			
			if (GUI.Button (new Rect (xLoc, itY, buttonWidth, buttonHeight), "Assigned Task: " + _shelter._survivors [index].AssignedTask.ToString ())) {
				showButtons=true;

			}
			itY += buttonHeight * 2;
			if (GUI.Button (new Rect (xLoc, itY, buttonWidth, buttonHeight), "Health: " + _shelter._survivors [index].Health)) {
				showButtons=true;

			}
			itY += buttonHeight;
			
			if (GUI.Button (new Rect (xLoc, itY, buttonWidth, buttonHeight), "Fatigue: " + _shelter._survivors [index].Fatigue)) {
				showButtons=true;

			}

			itY += buttonHeight * 2;
			
			if (GUI.Button (new Rect (xLoc, itY, buttonWidth, buttonHeight), "Close")) {
				showButtons=true;
				sideButtons=false;
			}


		}


	}

	// Update is called once per frame
	void Update () {
		
		//this will get rid of buttons when you click anywhere other than a button
		if (Input.GetMouseButtonUp(0)) {
			if(!showButtons){
				charButtons = sideButtons = false;
			}
			showButtons = false;
		}
	}
}
