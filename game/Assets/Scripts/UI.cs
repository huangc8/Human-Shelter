using UnityEngine;
using System.Collections;

public class UI : MonoBehaviour {

	// ========================================================== data
	public GameObject g;
	public Dialogue _diag;
	Shelter _shelter; // the shelter data
	Visitor _visitors;
	GameTime _gametime;
	StartNewConversation _startNewConversation;
	ReportHandler _reports;

	//these are needed for showing and hiding buttons
	public bool showAllButtons;
	public bool startScreen;
	private bool charButtons, sideButtons, showButtons;
	private float x, y, animateSide;
	private int index;

	// ========================================================== initialization
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
		if (_reports == null) {
			_reports = g.GetComponent<ReportHandler> ();
		}

		charButtons = sideButtons = showButtons = false;

		showAllButtons = false;
		
		index = 0;
		animateSide = 500;
	}

	// ========================================================== helper
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

	// ========================================================== GUI
	void OnGUI (){

		float buttonWidth = Screen.width * .2f;
		float buttonHeight = Screen.height * .03f;

		float xLoc = Screen.width*.8f;
		float itY = Screen.height*.8f;

		float squareSize = Screen.width * .07f;

		//if(showAllButtons && _gametime._currentDay > 0)
		if(true)
		{
			float w = Screen.width*.03f;
			float h = Screen.height*.04f;

			//height 1 is food and day
			GUI.Box (new Rect (Screen.width*.9f,h, squareSize, squareSize), "Day\n" + _gametime._currentDay.ToString());
			GUI.Box (new Rect (w, h, squareSize, squareSize), "Food\n" + _shelter.Food);
			h+= squareSize;

			// height 2 is new day button, and medicine
			if (GUI.Button (new Rect (Screen.width*.9f, h*1.15f, squareSize, squareSize/3),  "Next Day")) {
				_gametime.newDay();
				_reports.showReports = true;
			}
			GUI.Box (new Rect (w,h, squareSize, squareSize), "Medicine\n" + _shelter.Medicine);
			h+= squareSize;

			//parts
			GUI.Box (new Rect (w,h, squareSize, squareSize), "Parts\n" + _shelter.Parts);
			h+= squareSize;

			// reports button
			if (GUI.Button (new Rect (w, h*1.1f, squareSize, squareSize/3),  "Open Journal")) {
				if(_reports.showReports == false){
					_reports.showReports = true;
				}
				else
				{
					_reports.showReports = false;
				}
			}



			w = Screen.width *.765f;
			h = Screen.height *.75f;


			//new survivor arrives
			//this has been replaced by just clicking on the guy

			/*
			try{
			Survivor visitorAtGate = _visitors._personList [_gametime._currentDay];
			if(_gametime._currentDay == 0){
					visitorAtGate = null;
			}
			if (visitorAtGate != null) {
				if(_startNewConversation.specialCase()){
					GUI.Box (new Rect (w, h, buttonWidth, buttonHeight*1.15f), "There is someone at the gate!");

				h += buttonHeight * 1.24f;
				if (GUI.Button (new Rect (w, h, buttonWidth, buttonHeight), "Talk to " + visitorAtGate.Name.ToString ())) {
					_startNewConversation.ClickCheck("gate");
				}
				h += buttonHeight;
				}

			} 
			}
			catch{

			}
			*/



			w = Screen.width * x - Screen.width * .045f;
			h = Screen.height * y - Screen.height * .143f;

			buttonWidth = Screen.width * .1f;


			//above character's head
			if(charButtons){
				if (GUI.Button (new Rect (w, h, buttonWidth, buttonHeight),"Talk to " + _shelter._survivors[index].Name)) {
					showButtons=true;
					_startNewConversation.ClickCheck(_shelter._survivors[index].Name);

					_shelter._survivors [index].Converse ();
					_shelter._survivors[index]._conversationsLeft--;
				}
				h+= buttonHeight *1.07f;

				if (GUI.Button (new Rect (w, h, buttonWidth, buttonHeight),"Assign task")) {
					showButtons=true;
					sideButtons=true;
					charButtons=false;
				}

				w = Screen.width *x - Screen.width * .089f;
				h = Screen.height*y - Screen.height * .08f;

				//health and fatigue
				GUI.Box(new Rect (w, h, squareSize/1.6f, squareSize/1.8f), _shelter._survivors[index].Health + "\nHealth");
				h+=squareSize/1.8f;
				GUI.Box(new Rect (w, h, squareSize/1.6f, squareSize/1.8f), _shelter._survivors[index].Fatigue + "\nFatigue");



			}




			//side assign buttons
			if (true) {
				if(animateSide > 0 && sideButtons)
				{
					animateSide -=20;
				}
				if(animateSide < 500 && !sideButtons)
				{
					animateSide +=20;
				}


				w = Screen.width*.836f + animateSide;
				h = Screen.height*.4f;
				buttonWidth = Screen.width * .13f;


				if(_shelter.NumberOfSurvivors > 0){ //if we have no survivors don't try to do this

					GUI.Box (new Rect (w, h, buttonWidth, buttonHeight*12.1f),_shelter._survivors[index].Name + "\nAssign Task");

					w = Screen.width*.839f + animateSide;
					buttonWidth = Screen.width * .125f;


					h += buttonHeight*2;
					for (int t = 0; t < (int) Survivor.task.Count; t++) {
						if (GUI.Button (new Rect (w, h, buttonWidth, buttonHeight), ((Survivor.task)t).ToString ())) {
							showButtons=true;
							_shelter._survivors [index].AssignedTask = ((Survivor.task)t);
						}
						h += buttonHeight;
					}
					
					if (GUI.Button (new Rect (w, h, buttonWidth, buttonHeight), "Assigned Task: " + _shelter._survivors [index].AssignedTask.ToString ())) {
						showButtons=true;

					}

					//close, health, and fatigue

					w = Screen.width*.786f+animateSide;
					h = Screen.height*.4f;
					

					if (GUI.Button (new Rect (w, h, squareSize/1.6f, squareSize/1.8f), "Close")) {
						showButtons=true;
						sideButtons=false;
					}

					h += squareSize/1.6f;

					GUI.Box (new Rect (w, h, squareSize/1.6f, squareSize/1.8f), "Health\n" + _shelter._survivors [index].Health);

					h+=squareSize/1.8f;

					GUI.Box (new Rect (w, h, squareSize/1.6f, squareSize/1.8f), "Fatigue\n" + _shelter._survivors [index].Fatigue);

				}
			}
		}

		// start screen
		if (_gametime._currentDay == 0) {
			// new day button
			if (GUI.Button (new Rect (Screen.width*.05f, Screen.height*.07f, buttonWidth, buttonHeight),  "New Game")) {
				_gametime.newDay();
			}
		}
	}// end of GUI


	// ========================================================== Update
	// Update is called once per frame
	void Update () {
		if(showAllButtons){
			//this will get rid of buttons when you click anywhere other than a button
			if (Input.GetMouseButtonUp(0)) {
				if(!showButtons){
					charButtons = sideButtons = false;
				}
				showButtons = false;
			}
		}
	}
}
