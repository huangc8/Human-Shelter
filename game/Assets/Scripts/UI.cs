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
		animateSide = 400;
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
		float buttonHeight = Screen.height * .04f;

		float xLoc = Screen.width*.8f;
		float itY = Screen.height*.8f;

		float squareSize = Screen.width * .07f;

		if(showAllButtons && _gametime._currentDay > 0)
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

					if(_shelter._survivors[index].Fatigue <= 0){
						if (GUI.Button (new Rect (xLoc, itY, buttonWidth, buttonHeight), "Stamina: " + -1*_shelter._survivors [index].Fatigue)) {
							showButtons=true;
							
						}

					}
					else{
						if (GUI.Button (new Rect (xLoc, itY, buttonWidth, buttonHeight), "Fatigue: " + _shelter._survivors [index].Fatigue)) {
							showButtons=true;
							
						}

					}


					itY += buttonHeight * 2;
					
					if (GUI.Button (new Rect (xLoc, itY, buttonWidth, buttonHeight), "Close")) {
						showButtons=true;
						sideButtons=false;
					}
				}
			}
		}

		// start screen
		if (_gametime._currentDay == 0) {
			// new day button
			if (GUI.Button (new Rect (Screen.width*.05f, Screen.height*.07f, 100, buttonHeight),  "New Game")) {
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
