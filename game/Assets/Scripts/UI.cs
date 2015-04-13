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

	GUIStyle boxStyle, buttonStyle;
	Font regular, bold;
	Texture2D box, button1, button2, button3;

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



		boxStyle = new GUIStyle ("box");

		buttonStyle = new GUIStyle ("button");
		
		regular = (Font)Resources.Load("Arial", typeof(Font));
		bold = (Font)Resources.Load("Arial Bold", typeof(Font));

		box = (Texture2D)Resources.Load ("boxBack", typeof(Texture2D));
		button1 = (Texture2D)Resources.Load ("buttonBack", typeof(Texture2D));
		button2 = (Texture2D)Resources.Load ("buttonBack2", typeof(Texture2D));
		button3 = (Texture2D)Resources.Load ("buttonBack3", typeof(Texture2D));
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


		//box style
		boxStyle.font = regular;

		//1400x800
		int bigFont = Screen.width / 29;
		int smallFont = Screen.width / 70;
		boxStyle.fontSize = smallFont;

		boxStyle.alignment = TextAnchor.MiddleCenter;
		boxStyle.normal.background = box;
		boxStyle.normal.textColor = Color.white;
		boxStyle.padding = new RectOffset (1, 1, 1, 1);

		
		

		//button style
		buttonStyle.font = regular;
		buttonStyle.fontSize = smallFont;
		
		buttonStyle.alignment = TextAnchor.MiddleCenter;
		buttonStyle.normal.background = button1;
		buttonStyle.hover.background = button2;
		buttonStyle.active.background = button3;

		buttonStyle.normal.textColor = Color.white;
		buttonStyle.hover.textColor = Color.black;
		buttonStyle.active.textColor = Color.black;
		buttonStyle.padding = new RectOffset (1, 1, 1, 1);

		




		float buttonWidth = Screen.width * .2f;
		float buttonHeight = Screen.height * .03f;

		float squareSize = Screen.width * .07f;
		float smallSquareSize = squareSize / 3;

		if(showAllButtons && _gametime._currentDay > 0)
		//if(true)
		{
			float w = Screen.width*.03f;
			float h = Screen.height*.04f;

			//height 1 is food and day
			GUI.Box (new Rect (w, h, squareSize, smallSquareSize), "Food", boxStyle);
			GUI.Box (new Rect (Screen.width*.9f,h, squareSize, smallSquareSize), "Day", boxStyle);

			h+= smallSquareSize;
			boxStyle.fontSize = bigFont;
			boxStyle.font = bold;

			GUI.Box (new Rect (w, h, squareSize, smallSquareSize*2), _shelter.Food.ToString(), boxStyle);
			GUI.Box (new Rect (Screen.width*.9f,h, squareSize, smallSquareSize*2), _gametime._currentDay.ToString(), boxStyle);


			h+= smallSquareSize*2;
			boxStyle.fontSize = smallFont;
			boxStyle.font = regular;

		
			// height 2 is new day button, and medicine
			if (GUI.Button (new Rect (Screen.width*.9f, h*1.15f, squareSize, squareSize/3),  "Next Day", buttonStyle)) {
				_gametime.newDay();
				_reports.showReports = true;
			}

			GUI.Box (new Rect (w,h, squareSize, smallSquareSize), "Medicine", boxStyle);

			h+= smallSquareSize;
			boxStyle.fontSize = bigFont;
			boxStyle.font = bold;


			GUI.Box (new Rect (w,h, squareSize, smallSquareSize*2), _shelter.Medicine.ToString(), boxStyle);
			h+= smallSquareSize*2;
			boxStyle.fontSize = smallFont;
			boxStyle.font = regular;
			

			//parts
			GUI.Box (new Rect (w,h, squareSize, smallSquareSize), "Parts", boxStyle);
			h+= smallSquareSize;
			boxStyle.fontSize = bigFont;
			boxStyle.font = bold;

			GUI.Box (new Rect (w,h, squareSize, smallSquareSize*2), _shelter.Parts.ToString(), boxStyle);
			h+= smallSquareSize*2;
			boxStyle.fontSize = smallFont;
			boxStyle.font = regular;



			// reports button
			if (GUI.Button (new Rect (w, h*1.1f, squareSize, squareSize/3),  "Journal", buttonStyle)) {
				if(_reports.showReports == false){
					_reports.showReports = true;
				}
				else
				{
					_reports.showReports = false;
				}
			}



			w = Screen.width * x - Screen.width * .045f;
			h = Screen.height * y - Screen.height * .143f;

			buttonWidth = Screen.width * .1f;
			float charSquare = squareSize * .2f ;

			
			//above character's head
			if(charButtons){
				if (GUI.Button (new Rect (w, h, buttonWidth, buttonHeight),"Talk to " + _shelter._survivors[index].Name, buttonStyle)) {
					showButtons=true;
					_startNewConversation.ClickCheck(_shelter._survivors[index].Name);

					_shelter._survivors [index].Converse ();
					_shelter._survivors[index]._conversationsLeft--;
				}
				h+= buttonHeight *1.09f;

				if (GUI.Button (new Rect (w, h, buttonWidth, buttonHeight),"Assign task", buttonStyle)) {
					showButtons=true;
					sideButtons=true;
					charButtons=false;
				}

				w = Screen.width *x - Screen.width * .089f;
				h = Screen.height*y - Screen.height * .08f;


				//health and fatigue
				boxStyle.fontSize = (int)(bigFont*.75f);
				boxStyle.font = bold;
				GUI.Box(new Rect (w, h, charSquare*3, charSquare*2f), _shelter._survivors[index].Health.ToString(), boxStyle);

				h+= charSquare*2f;
				boxStyle.fontSize = (int)(smallFont*.8f);
				boxStyle.font = regular;
				boxStyle.alignment = TextAnchor.UpperCenter;
				GUI.Box(new Rect (w, h, charSquare*3, charSquare*1.8f), "Health", boxStyle);

				h+= charSquare*1.8f;
				boxStyle.fontSize = (int)(bigFont*.5f);
				boxStyle.font = bold;
				boxStyle.alignment = TextAnchor.MiddleCenter;
				GUI.Box(new Rect (w, h, charSquare*3, charSquare*1.3f), _shelter._survivors[index].Fatigue.ToString(), boxStyle);

				h+= charSquare*1.3f;
				boxStyle.fontSize = (int)(smallFont*.6f);
				boxStyle.font = regular;
				boxStyle.alignment = TextAnchor.UpperCenter;
				GUI.Box(new Rect (w, h, charSquare*3, charSquare*1.1f), "Fatigue", boxStyle);

				boxStyle.alignment = TextAnchor.MiddleCenter;

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

					boxStyle.alignment = TextAnchor.UpperCenter;
					boxStyle.font = bold;
					GUI.Box (new Rect (w, h, buttonWidth, buttonHeight),_shelter._survivors[index].Name, boxStyle);

					h+= buttonHeight;
					boxStyle.font = regular;
					GUI.Box (new Rect (w, h, buttonWidth, buttonHeight*12.05f),"Assign Task", boxStyle);


					h += buttonHeight*1.25f;


					//decoratory white bar
					boxStyle.normal.background = button2;
					GUI.Box (new Rect (w, h*.99f, buttonWidth, buttonHeight*.01f),"", boxStyle);
					boxStyle.normal.background = box;



					w = Screen.width*.839f + animateSide;
					buttonWidth = Screen.width * .1245f;

					
					for (int t = 0; t < (int) Survivor.task.Count; t++) {

						if(_shelter._survivors[index].AssignedTask == (Survivor.task)t)
						{
							buttonStyle.normal.background = (Texture2D)Resources.Load ("buttonBack2", typeof(Texture2D));
							buttonStyle.normal.textColor = Color.black;

						}

						if (GUI.Button (new Rect (w, h, buttonWidth, buttonHeight), ((Survivor.task)t).ToString (), buttonStyle)) {
							showButtons=true;
							_shelter._survivors [index].AssignedTask = ((Survivor.task)t);
						}
						buttonStyle.normal.background = (Texture2D)Resources.Load ("buttonBack", typeof(Texture2D));
						buttonStyle.normal.textColor = Color.white;

						
						h += buttonHeight*1.2f;
					}

					//close, health, and fatigue

					w = Screen.width*.786f+animateSide;
					h = Screen.height*.4f;
					

					if (GUI.Button (new Rect (w, h, squareSize/1.6f, squareSize/1.8f), "Close", buttonStyle)) {
						showButtons=true;
						sideButtons=false;
					}

					h += squareSize/1.6f;




					//health and fatigue
					boxStyle.fontSize = (int)(bigFont*.75f);
					boxStyle.font = bold;
					boxStyle.alignment = TextAnchor.MiddleCenter;
					GUI.Box(new Rect (w, h, charSquare*3, charSquare*2f), _shelter._survivors[index].Health.ToString(), boxStyle);
					
					h+= charSquare*2f;
					boxStyle.fontSize = (int)(smallFont*.8f);
					boxStyle.font = regular;
					boxStyle.alignment = TextAnchor.UpperCenter;
					GUI.Box(new Rect (w, h, charSquare*3, charSquare*1.8f), "Health", boxStyle);
					
					h+= charSquare*1.8f;
					boxStyle.fontSize = (int)(bigFont*.5f);
					boxStyle.font = bold;
					boxStyle.alignment = TextAnchor.MiddleCenter;
					GUI.Box(new Rect (w, h, charSquare*3, charSquare*1.3f), _shelter._survivors[index].Fatigue.ToString(), boxStyle);
					
					h+= charSquare*1.3f;
					boxStyle.fontSize = (int)(smallFont*.6f);
					boxStyle.font = regular;
					boxStyle.alignment = TextAnchor.UpperCenter;
					GUI.Box(new Rect (w, h, charSquare*3, charSquare*1.1f), "Fatigue", boxStyle);
					
					boxStyle.alignment = TextAnchor.MiddleCenter;

				}
			}
		}

		// start screen
		if (_gametime._currentDay == 0) {
			// new day button
			if (GUI.Button (new Rect (Screen.width*.1f, Screen.height*.07f, buttonWidth, buttonHeight),  "New Game", buttonStyle)) {
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
