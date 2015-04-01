using UnityEngine;
using System.Collections;

public class UI : MonoBehaviour {

	public GameObject g;
	public Dialogue _diag;
	Shelter _shelter; // the shelter data
	Visitor _visitors;
	GameTime _gametime;
	StartNewConversation _startNewConversation;

	//these are needed for showing and hiding buttons
	private bool charButtons, sideButtons, showButtons;
	private float x, y, animateSide;
	private int index;

	// Use this for initialization
	void Start () {
		_startNewConversation = this.GetComponent<StartNewConversation>();
		if (_shelter == null) {
			_shelter = g.GetComponent<Shelter> ();
		}
		if (_visitors == null) {
			_visitors = g.GetComponent<Visitor> ();
		}
		if (_gametime == null) {
			_gametime = g.GetComponent<GameTime> ();
		}

		charButtons = sideButtons = showButtons = false;

		index = 0;
		animateSide = 400;
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
		sideButtons = false;


	}

	void OnGUI (){
		int buttonWidth = 300;
		int buttonHeight = 30;
		
		float itY = Screen.height*.8f;
		float xLoc = Screen.width*.8f;


		//top bar
		if (GUI.Button (new Rect (Screen.width*.05f, Screen.height*.02f, Screen.width*.9f, buttonHeight), 
		                "Day Survived: " + _gametime._currentDay.ToString () + "      Food: " + _shelter.Food + 
		                "      Medicine: " + _shelter.Medicine + "      Luxuries: " + _shelter.Luxuries)) {
			
		}

		if (GUI.Button (new Rect (Screen.width*.05f, Screen.height*.07f, 100, buttonHeight),  "Next Day")) {
			_gametime.newDay();
		}
		




		//new survivor arrives
		try{
		Survivor visitorAtGate = _visitors._personList [_gametime._currentDay];
		if (visitorAtGate != null) {
			if (GUI.Button (new Rect (xLoc, itY, buttonWidth, buttonHeight), "There is someone at the gate!")) {
			}
			itY += buttonHeight;
			if (GUI.Button (new Rect (xLoc, itY, buttonWidth, buttonHeight), "Talk to " + visitorAtGate.Name.ToString ())) {
				_diag.startConv("Conv_1", false);
				_startNewConversation.ClickCheck("gate");
			}
			itY += buttonHeight;
			if (GUI.Button (new Rect (xLoc, itY, buttonWidth, buttonHeight), "Invite")) {
				_shelter.InviteSurvivor(visitorAtGate);
				/*
				_shelter._survivors [_shelter.NumberOfSurvivors] = visitorAtGate;
				//show on map
				visitorAtGate.image.renderer.enabled = true;
				visitorAtGate.image.layer = 0;

				_shelter.NumberOfSurvivors++;
				_visitors._personList [_gametime._currentDay] = null;*/
			}
			itY += buttonHeight;
			if (GUI.Button (new Rect (xLoc, itY, buttonWidth, buttonHeight), "Reject")) {
				_visitors.RejectSurvivorAtGate(visitorAtGate.Name);
			}
			itY += buttonHeight;
			if (GUI.Button (new Rect (xLoc, itY, buttonWidth, buttonHeight), "Kill")) {
				_visitors.KillSurvivorAtGate(visitorAtGate.Name);
				
			}
			itY += buttonHeight;
		} 
		}
		catch{

		}
		/*else {
			if (GUI.Button (new Rect (xLoc, itY, buttonWidth, buttonHeight), "Nobody is at the gate")) {
			}
		}*/





		//above character's head
		if(charButtons){

			if (GUI.Button (new Rect (Screen.width*x-110, Screen.height*y-120, 100, 50),"Talk to " + _shelter._survivors[index].Name)) {
				showButtons=true;
				_startNewConversation.ClickCheck(_shelter._survivors[index].Name);

				_shelter._survivors [index].Converse ();
				_shelter._survivors[index]._conversationsLeft--;
			}
			if (GUI.Button (new Rect (Screen.width*x+10, Screen.height*y-120, 100, 50),"Assign task")) {
				showButtons=true;
				sideButtons=true;
				charButtons=false;
			}
		}

		//side assign buttons
		if (true) {
			if(animateSide > 0 && sideButtons)
			{
				animateSide -=20;
			}
			if(animateSide < 400 && !sideButtons)
			{
				animateSide +=20;
			}


			itY = Screen.height*.1f;
			xLoc = Screen.width*.8f + animateSide;

			if(_shelter.NumberOfSurvivors > 0){ //if we have no survivors don't try to do this
				if (GUI.Button (new Rect (xLoc, itY, buttonWidth, buttonHeight), _shelter._survivors[index].Name)) {
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
