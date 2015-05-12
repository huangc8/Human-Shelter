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
	private Conditions _Cond; 												// conditions
	NewDayAnim _newdayanim;


	//these are needed for showing and hiding buttons
	public bool showAllButtons;
	public bool startScreen;
	private bool charButtons, sideButtons, showButtons, showJournal, showHelp;
	private float x, y, animateSide;
	private int index;
	public bool tutorial, credits;
	int warning;
	float fade;
	public int fading;

	GUIStyle boxStyle, buttonStyle;
	GUIStyle labelStyle;
	Font regular, bold;
	Texture2D box, button1, button2, button3, black;
	Transform highlight;
	Transform blackbox;
	Transform title;

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
		_Cond = g.GetComponent<Conditions>();
		_newdayanim = g.GetComponent<NewDayAnim> ();

		foreach(Transform child in g.transform)
		{
			if(child.name == "BlackBox")
			{
				blackbox = child;
			}
			if(child.name == "Title")
			{
				title=child;
			}
		}

		charButtons = sideButtons = showButtons = showJournal = showHelp = false;

		showAllButtons = false;
		tutorial = true;
		warning = 0;
		
		index = -1;
		animateSide = 500;
		fade = 0;
		fading = 0;
		credits = false;


		boxStyle = new GUIStyle ("box");

		buttonStyle = new GUIStyle ("button");
		labelStyle = new GUIStyle ("Label");

		
		regular = (Font)Resources.Load("Arial", typeof(Font));
		bold = (Font)Resources.Load("Arial Bold", typeof(Font));

		box = (Texture2D)Resources.Load ("boxBack", typeof(Texture2D));
		button1 = (Texture2D)Resources.Load ("buttonBack", typeof(Texture2D));
		button2 = (Texture2D)Resources.Load ("buttonBack2", typeof(Texture2D));
		button3 = (Texture2D)Resources.Load ("buttonBack3", typeof(Texture2D));
		black = (Texture2D)Resources.Load ("black", typeof(Texture2D));

	}

	// ========================================================== helper
	public void charClick(int aindex, float ax, float ay)
	{
		//center buttons
		//not using this at the moment because it doesn't really work
		//GUIStyle style = GUI.skin.GetStyle("align");
		//style.alignment = TextAnchor.MiddleCenter;
		if(!tutorial && !_diag._DiagCon.IsConversationActive)
		{
			if(highlight)
			{
				highlight.renderer.enabled=false;
			}

			x = ax;
			y = ay;
			index = aindex;


			foreach (Transform child in _shelter._survivors[index].image.transform)
			{
				if(child.name == "Highlight")
				{
					highlight = child;
				}
			}

			
			charButtons = true;
			showButtons = true;
			sideButtons = showJournal = showHelp = false;
		}
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
		boxStyle.hover.background = box;
		boxStyle.active.background = box;
		boxStyle.normal.textColor = Color.white;
		boxStyle.hover.textColor = Color.white;
		boxStyle.active.textColor = Color.white;
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
		
		labelStyle.fontSize = Screen.width / 90;
		labelStyle.normal.textColor = Color.white; 
		
		float buttonWidth = Screen.width * .2f;
		float buttonHeight = Screen.height * .03f;

		float squareSize = Screen.width * .07f;
		float smallSquareSize = squareSize / 3;

		if(credits)
		{
			boxStyle.wordWrap = true;
			boxStyle.alignment = TextAnchor.UpperCenter;
			boxStyle.padding = new RectOffset (15, 5, 15, 10);
			boxStyle.fontSize = (int)(smallFont*1f);;
			GUI.Box (new Rect (Screen.width * .05f, Screen.height * .05f, Screen.width * .4f, Screen.height * .9f), 
			         "Credits:\n\n" +
			         "Technical Writer:\nDamian Cross\n\n" +
			         "Specialized Artist:\nChristian Gonzalez\n\n" +
			         "Sound Design:\nGavin Gundler\n\n" +
			         "Programming, User Interface:\nCaleb Hoffman\n\n" +
			         "Programming, Dialogue:\nChi-Ning Huang\n\n" +
			         "User Interface Design:\nBen Keel\n\n" +
			         "Storyboard Artist:\nSky Kim\n\n" +
			         "Project Manager:\nJon Ota\n\n" +
			         "Programming, Mechcanics:\nWilliam Pheloung"
			         , boxStyle);
			if (GUI.Button (new Rect (Screen.width*.15f, Screen.height *.9f, buttonWidth, buttonHeight),  "Return to Menu", buttonStyle)) {
				credits = false;
			}
		}

		if(showAllButtons && _gametime._currentDay > 0)
		{
			//fade the GUI if in transistion
			GUI.color = new Color(1,1,1,1-(fade/100f));

			if(fading !=1)
			{

				//tutorial
				if(tutorial)
				{
					showJournal=false;
					boxStyle.alignment = TextAnchor.UpperLeft;
					boxStyle.padding = new RectOffset (15, 5, 15, 10);
					boxStyle.wordWrap = true;

					if(warning ==0)
					{
						GUI.Box (new Rect (Screen.width * .25f, Screen.height * .2f, Screen.width * .5f, Screen.height * .55f), 
						         "Welcome to Human Sickness!  Your goal is to survive.\n\nClick on the characters to speak with them and discover what is going on.\n" +
						         "Listen carefully to your residents, because knowledge\nof their strengths and history will help you survive.\n\n" +
						         "Assign each person a job for the day, but be careful: The wrong\nchoices could lead to disaster.\n\n" +
						         "Click the Help button for more information on assignments.\n\nThere's someone at the gate!  You should go find out what they want.\n\n" +
						         "Good luck!"
						         , boxStyle);
						if (GUI.Button (new Rect (Screen.width*.4f, Screen.height *.66f, buttonWidth, buttonHeight),  "Continue", buttonStyle)) {
							tutorial = false;
						}
					}
					boxStyle.alignment = TextAnchor.MiddleCenter;

					if(warning ==1)
					{
						GUI.Box (new Rect (Screen.width * .4f, Screen.height * .45f, Screen.width * .2f, Screen.height * .1f), 
						         "There's someone at the gate.\nGo see what they want!"
						         , boxStyle);
						if (GUI.Button (new Rect (Screen.width*.4f, Screen.height *.55f, buttonWidth, buttonHeight),  "Continue", buttonStyle)) {
							tutorial = false;
						}
					}
					if(warning ==2)
					{
						GUI.Box (new Rect (Screen.width * .38f, Screen.height * .45f, Screen.width * .24f, Screen.height * .1f), 
						         "You have unassigned residents.\nBe sure to give them a task."
						         , boxStyle);
						if (GUI.Button (new Rect (Screen.width*.38f, Screen.height *.55f, Screen.width * .24f, buttonHeight),  "Continue", buttonStyle)) {
							tutorial = false;
						}
					}
					if(warning ==3)
					{
						GUI.Box (new Rect (Screen.width * .38f, Screen.height * .45f, Screen.width * .24f, Screen.height * .1f), 
						         "You should talk to Brian before continuing."
						         , boxStyle);
						if (GUI.Button (new Rect (Screen.width*.38f, Screen.height *.55f, Screen.width * .24f, buttonHeight),  "Continue", buttonStyle)) {
							tutorial = false;
						}
					}

					boxStyle.padding = new RectOffset (1, 1, 1, 1);
					boxStyle.wordWrap = false;

				}
				else{



				float w = Screen.width*.03f;
				float h = Screen.height*.04f;

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

			
				if (GUI.Button (new Rect (Screen.width*.9f, h*1.15f, squareSize, squareSize/3),  "Next Day", buttonStyle)) {
						//see if anyone is unassigned
						bool unass = false;

						for(int i =0; i< _shelter.NumberOfSurvivors; i++)
						{
							if(_shelter._survivors[i].AssignedTask == Survivor.task.Unassigned)
							{
								unass = true;
							}
						}
						if (_gametime._currentDay == 1 && _startNewConversation.getConv("Conv_1_1")
						         && _Cond.getCondition("inCamp", "Brian")
						         && !_startNewConversation.getConv("Conv_1_3"))
						{
							tutorial = true;
							warning = 3;
						}
						else if(_visitors._personList[_gametime._currentDay]!=null)
						{
							tutorial = true;
							warning = 1;
						}
						else if (unass)
						{
							tutorial = true;
							warning = 2;
						}

						else
						{
							_reports.showReports = false;
							charButtons = sideButtons = showHelp= false;
							//run animations
							StartCoroutine( _newdayanim.run(_shelter._survivors, _shelter.NumberOfSurvivors));

							fading = 1;
						}
				} // end of next day

				GUI.Box (new Rect (w,h, squareSize, smallSquareSize), "Medicine", boxStyle);

				h+= smallSquareSize;
				boxStyle.fontSize = bigFont;
				boxStyle.font = bold;


				GUI.Box (new Rect (w,h, squareSize, smallSquareSize*2), _shelter.Medicine.ToString(), boxStyle);
				h+= smallSquareSize*2;
				boxStyle.fontSize = smallFont;
				boxStyle.font = regular;
				

				GUI.Box (new Rect (w,h, squareSize, smallSquareSize), "Parts", boxStyle);
				h+= smallSquareSize;
				boxStyle.fontSize = bigFont;
				boxStyle.font = bold;

				GUI.Box (new Rect (w,h, squareSize, smallSquareSize*2), _shelter.Parts.ToString(), boxStyle);
				h+= smallSquareSize*2;
				boxStyle.fontSize = smallFont;
				boxStyle.font = regular;



				// reports button
				if( _gametime._currentDay>1)
					{
					if (GUI.Button (new Rect (w, h*1.1f, squareSize, squareSize/3),  "Journal", buttonStyle)) {
						if(showJournal == false){
							showJournal=true;
							showButtons=true;
							charButtons=sideButtons=showHelp=false;
						}
						//else it will close it automatically, see update
					}

					h += squareSize/3;
				}

				// help button
				if (GUI.Button (new Rect (w, h+Screen.height*.05f, squareSize, squareSize/3),  "Help", buttonStyle)) {
					if(showHelp == false){
						showHelp=true;
						showButtons=true;
						charButtons=sideButtons=showJournal=false;
					}
					//else it will close it automatically, see update
				}


				w = Screen.width * x - Screen.width * .045f;
				h = Screen.height * y - Screen.height * .18f;

				buttonWidth = Screen.width * .1f;
				float charSquare = squareSize * .2f ;

				
				//above character's head
				if(charButtons){
					boxStyle.normal.background = button2;
					GUI.Box (new Rect (w, h - Screen.height * .001f, buttonWidth, buttonHeight * .001f),"", boxStyle);
					if (GUI.Button (new Rect (w, h, buttonWidth, buttonHeight),"Talk to " + _shelter._survivors[index].Name, buttonStyle)) {
						_startNewConversation.ClickCheck(_shelter._survivors[index].Name);
						charButtons=false;
					}
					h+= buttonHeight *1.25f;

					GUI.Box (new Rect (w, h - Screen.height * .001f, buttonWidth, buttonHeight * .001f),"", boxStyle);
					if (GUI.Button (new Rect (w, h, buttonWidth, buttonHeight),"Assign task", buttonStyle)) {
						showButtons=true;
						sideButtons=true;
						charButtons=showJournal=showHelp=false;
					}

					boxStyle.normal.background = box;

					h+= buttonHeight *1.2f;

					boxStyle.fontSize = (int)(smallFont*.8f);
					GUI.Box(new Rect (w + Screen.width * .009f, h, buttonWidth*.8f, buttonHeight), _shelter._survivors[index].AssignedTask.ToString(), boxStyle);
					boxStyle.fontSize = smallFont;


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
					if(_shelter._survivors[index].Fatigue >= 0)
					{
						GUI.Box(new Rect (w, h, charSquare*3, charSquare*1.3f), _shelter._survivors[index].Fatigue.ToString(), boxStyle);
					}
					else
					{
						GUI.Box(new Rect (w, h, charSquare*3, charSquare*1.3f), (_shelter._survivors[index].Fatigue * -1).ToString(), boxStyle);
					}

					h+= charSquare*1.3f;
					boxStyle.fontSize = (int)(smallFont*.6f);
					boxStyle.font = regular;
					boxStyle.alignment = TextAnchor.UpperCenter;

					if(_shelter._survivors[index].Fatigue >= 0)
					{
						GUI.Box(new Rect (w, h, charSquare*3, charSquare*1.1f), "Fatigue", boxStyle);
					}
					else
					{
						GUI.Box(new Rect (w, h, charSquare*3, charSquare*1.1f), "Stamina", boxStyle);

					}

					boxStyle.alignment = TextAnchor.MiddleCenter;

				}


				if(showJournal && _gametime._currentDay>1)
					{
						if(fading !=1)
						{
							_reports.showReports = true;
						}
						w = Screen.width * .15f;
						h = Screen.height * .15f;

						boxStyle.fontSize = bigFont;
						buttonStyle.fontSize = bigFont;
						//boxStyle.font = bold;
						buttonStyle.font = bold;
						buttonStyle.padding =  new RectOffset (1, 1, 7, 1);

						if(GUI.Button(new Rect (w,h, Screen.height*.07f, Screen.height*.07f), "×", buttonStyle))
						{
						}

						if(GUI.Button(new Rect (w,h+Screen.height*.3f, Screen.height*.07f, Screen.height*.07f), "<", buttonStyle))
						{
							showButtons=true;
							_reports.lastPage();
						}
						if(GUI.Button(new Rect (w+Screen.width*.65f,h+Screen.height*.3f, Screen.height*.07f, Screen.height*.07f), ">", buttonStyle))
						{
							showButtons=true;
							_reports.nextPage();
						}

						buttonStyle.padding =  new RectOffset (1, 1, 1, 1);


						w += Screen.height*.08f;
						GUI.Box(new Rect (w,h, Screen.width*.15f, Screen.height*.07f), "Journal", boxStyle);

						h+= Screen.height *.08f;

						if(GUI.Button(new Rect (w,h, Screen.width*.6f, Screen.height*.58f), "", boxStyle))
						{
							showButtons = true;
						}

						w+= Screen.width*.54f;
						h+= Screen.height*.59f;

						boxStyle.fontSize=smallFont;
						GUI.Box(new Rect (w,h, Screen.width*.06f, Screen.height*.04f), "Day " + (_reports._currentPage+1), boxStyle);


						w = Screen.width*.22f;
						h = Screen.height*.25f;

						//Print the report text
						for(int i = 0; i < _reports.CurrentReports.Count; i++)
						{
							try{
								GUI.Label(new Rect(w,h,Screen.width*.6f,Screen.height*.04f), _reports.CurrentReports[i].GetMessage(), labelStyle); //error thrown is deifinitely not an indexing erro
								h += Screen.height*.04f;
							}
							catch{
								//Debug.LogError("ReportHandler (109) ERROR: i = " + i + " CurrentReports.Count: " + _reports.CurrentReports.Count);
							}
						}

					}
					else
					{
						_reports.showReports = false;
					}


					if(showHelp)
					{
						w = Screen.width * .15f;
						h = Screen.height * .12f;
						
						boxStyle.fontSize = bigFont;
						buttonStyle.fontSize = bigFont;
						//boxStyle.font = bold;
						buttonStyle.font = bold;
						buttonStyle.padding =  new RectOffset (1, 1, 7, 1);
						
						if(GUI.Button(new Rect (w,h, Screen.height*.07f, Screen.height*.07f), "×", buttonStyle))
						{
						}
						buttonStyle.padding =  new RectOffset (1, 1, 1, 1);
						
						
						w += Screen.height*.08f;
						GUI.Box(new Rect (w,h, Screen.width*.15f, Screen.height*.07f), "Help", boxStyle);
						
						h+= Screen.height *.08f;
						boxStyle.fontSize = (int)(smallFont*.95f);;
						boxStyle.alignment = TextAnchor.UpperLeft;
						boxStyle.padding =  new RectOffset (15, 1, 15, 1);


						if(GUI.Button(new Rect (w,h, Screen.width*.6f, Screen.height*.63f), 
						              "Click on a character in your camp to speak to them, or assign them a daily task.\n" +
						              "Speak with new arrivals to find out what they're looking for.\n" +
						              "When you're finished, click Next Day progress.\n\n" +
						              "Task descriptions:\n" +
						              "Scout: \t\tSearch nearby for other camps or buildings that may have supplies to steal.\n\n" +
						              "Heal: \t\tThis survivor will use medicine to increase the health of other injured survivors.\n\n" +
						              "Defend: \t\tIncrease your camp's defenses, in preperation for an attack.\n\n" +
						              "Scavenge: \tSearch a nearby area for supplies. \n" +
						              "\t\tEach day, your journal will tell you which building can be scavenged.\n\n" +
						              "Raiding: \t\tIncrease your attack strength, to prepare for a raid.\n" +
						              "\t\tWhen a raid is possible, you can attack an enemy camp for a lot of resources.\n\n" +
						              "Resting: \t\tStay inside for the day and restore some health and stamina.\n\n" +
						              "Evict: \t\tKick this person out of your camp.\n\n" +
						              "Execute: \t\tKill this survivor."

						              , boxStyle))
						{
							showButtons = true;
						}

						boxStyle.padding =  new RectOffset (1, 1, 1, 1);

						
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


					if(_shelter.NumberOfSurvivors > 0 && index >=0 && _shelter._survivors[index] != null){ //if we have no survivors don't try to do this

						boxStyle.alignment = TextAnchor.UpperCenter;
						boxStyle.font = bold;
						GUI.Box (new Rect (w, h, buttonWidth, buttonHeight),_shelter._survivors[index].Name, boxStyle);
						

						h+= buttonHeight;
						boxStyle.font = regular;

						GUI.Box (new Rect (w, h, buttonWidth, buttonHeight*10.85f),"Assign Task", boxStyle);


						h += buttonHeight*1.25f;


						//decoratory white bar
						boxStyle.normal.background = button2;
						GUI.Box (new Rect (w, h*.99f, buttonWidth, buttonHeight*.01f),"", boxStyle);
						boxStyle.normal.background = box;



						w = Screen.width*.839f + animateSide;
						buttonWidth = Screen.width * .1245f;

						buttonStyle.alignment = TextAnchor.MiddleLeft;
						buttonStyle.padding = new RectOffset (14, 1, 1, 1);
						buttonStyle.fontSize = (int)(smallFont*.85f);


						//it stops 1 short to avoid "unassigned" as an option
						//if something else gets added to the end, this needs to be fixed
						for (int t = 0; t < (int) Survivor.task.Count-1; t++) {

							if(_shelter._survivors[index].AssignedTask == (Survivor.task)t)
							{
								buttonStyle.normal.background = (Texture2D)Resources.Load ("buttonBack2", typeof(Texture2D));
								buttonStyle.normal.textColor = Color.black;

							}
							
							if(t == 6)
								{
									//h+=buttonHeight*1.2f;
									//Code for making evict and execute look different
								}

							if(((Survivor.task)t).ToString ()== "Raiding")
							{

									if(false)//raid not possible
								{
										buttonStyle.hover.background=black;
										buttonStyle.active.background=black;
										buttonStyle.normal.background=black;
										buttonStyle.hover.textColor = Color.white;
										buttonStyle.active.textColor = Color.white;
										GUI.Box (new Rect (w, h, buttonWidth, buttonHeight), "Raid", buttonStyle);
										buttonStyle.hover.background=button2;
										buttonStyle.active.background=button3;
										buttonStyle.normal.background=button1;
										buttonStyle.hover.textColor = Color.black;
										buttonStyle.active.textColor = Color.black;

								}
								else{
									if (GUI.Button (new Rect (w, h, buttonWidth, buttonHeight), "Raid", buttonStyle)) {
										showButtons=true;
										_shelter._survivors [index].AssignedTask = ((Survivor.task)t);
									}
								}
							}

							else if(((Survivor.task)t).ToString ()=="Resting")
							{
								if (GUI.Button (new Rect (w, h, buttonWidth, buttonHeight), "Rest", buttonStyle)) {
									showButtons=true;
									_shelter._survivors [index].AssignedTask = ((Survivor.task)t);
								}
							}

							else{
								if (GUI.Button (new Rect (w, h, buttonWidth, buttonHeight), ((Survivor.task)t).ToString (), buttonStyle)) {
									showButtons=true;
									_shelter._survivors [index].AssignedTask = ((Survivor.task)t);
								}
							}

							buttonStyle.normal.background = (Texture2D)Resources.Load ("buttonBack", typeof(Texture2D));
							buttonStyle.normal.textColor = Color.white;

							
							h += buttonHeight*1.2f;
						}

						buttonStyle.alignment = TextAnchor.MiddleCenter;
						buttonStyle.padding = new RectOffset (1, 1, 1, 1);
						buttonStyle.fontSize = smallFont;


						//prevent closing via misclick
						h = Screen.height*.4f + buttonHeight;
						buttonWidth = Screen.width * .13f;
						w = Screen.width*.836f + animateSide;
						boxStyle.active.background = null;
						boxStyle.hover.background = null;
						boxStyle.normal.background = null;

						if(GUI.Button (new Rect (w, h, buttonWidth, buttonHeight*10.85f),"", boxStyle))
						{
							showButtons=true;
						}

						boxStyle.active.background = box;
						boxStyle.hover.background = box;
						boxStyle.normal.background = box;
						buttonWidth = Screen.width * .1245f;




						//close

						w = Screen.width*.786f+animateSide;
						h = Screen.height*.4f;
						
						
						buttonStyle.fontSize = (int)(bigFont*1.2f);
						buttonStyle.font = bold;
						buttonStyle.padding =  new RectOffset (1, 1, 7, 1);

						if (GUI.Button (new Rect (w, h, squareSize/1.6f, squareSize/1.8f), "×", buttonStyle)) {
							showButtons=true;
							sideButtons=false;
						}
						buttonStyle.padding =  new RectOffset (1, 1, 1, 1);

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
						if(_shelter._survivors[index].Fatigue >= 0)
						{
							GUI.Box(new Rect (w, h, charSquare*3, charSquare*1.3f), _shelter._survivors[index].Fatigue.ToString(), boxStyle);
						}
						else
						{
							GUI.Box(new Rect (w, h, charSquare*3, charSquare*1.3f), (_shelter._survivors[index].Fatigue * -1).ToString(), boxStyle);
						}
						h+= charSquare*1.3f;
						boxStyle.fontSize = (int)(smallFont*.6f);
						boxStyle.font = regular;
						boxStyle.alignment = TextAnchor.UpperCenter;
						if(_shelter._survivors[index].Fatigue >= 0)
						{
							GUI.Box(new Rect (w, h, charSquare*3, charSquare*1.1f), "Fatigue", boxStyle);
						}
						else
						{
							GUI.Box(new Rect (w, h, charSquare*3, charSquare*1.1f), "Stamina", boxStyle);

						}
						boxStyle.alignment = TextAnchor.MiddleCenter;

					}
				}
			}
			}
		}

		if (fading >0) 
		{
			boxStyle.normal.background=black;
			boxStyle.active.background=black;
			boxStyle.hover.background=black;

			if(fading ==1 && !_newdayanim.running)
			{
				fade+=.8f;
				GUI.color = new Color(1,1,1,1-(fade/100f));
				if(fade>=100)
				{
					fading =2;
					//return everyone to camp, visually
					for(int i =0; i<_shelter.NumberOfSurvivors; i++)
					{
						_shelter._survivors[i].image.renderer.enabled = true;
					}

					_gametime.newDay();
					//hide title screen at start
					title.renderer.enabled=false;
				}
			}
			if(fading ==2 && !_diag._DiagCon.IsConversationActive)
			{
				showJournal = true;
				fade-=1.0f;
				if(fade<=0)
				{
					fading = 0;
					blackbox.renderer.enabled = false;

				}
			}
			blackbox.renderer.enabled = true;
			blackbox.renderer.material.color = new Color(1, 1, 1, fade/100);
		}

		// start screen
		if (_gametime._currentDay == 0 && !credits) {
			title.renderer.enabled=true;
			boxStyle.fontSize = (int)(bigFont*1.9f);
			boxStyle.alignment = TextAnchor.MiddleLeft;
			//title
			GUI.Box(new Rect (Screen.width*.1f, Screen.height*.06f, buttonWidth*5f, buttonHeight*4), "Human Sickness", boxStyle);

			boxStyle.fontSize = (int)(bigFont*1.3f);
			boxStyle.hover.textColor = Color.gray;
			boxStyle.active.textColor = new Color(.3f,.3f,.3f);

			// new day button
			if (GUI.Button (new Rect (Screen.width*.1f, Screen.height*.5f, buttonWidth*1.5f, buttonHeight*3),  "New Game", boxStyle)) {
				fading=1;

			}
			if (GUI.Button (new Rect (Screen.width*.1f, Screen.height*.6f, buttonWidth*1.5f, buttonHeight*3),  "Credits", boxStyle)) {
				credits = true;
				
			}
			if (GUI.Button (new Rect (Screen.width*.1f, Screen.height*.7f, buttonWidth*1.5f, buttonHeight*3),  "Quit", boxStyle)) {
				Application.Quit();
				
			}
		}
	}// end of GUI
	

	// ========================================================== Update
	// Update is called once per frame
	void Update () {
		if(index>=0 && highlight)
		{

			if(sideButtons || charButtons)
			{
				highlight.renderer.enabled= true;
			}
			else
			{
				highlight.renderer.enabled= false;
			}
		}
		if(showAllButtons){
			//this will get rid of buttons when you click anywhere other than a button
			if (Input.GetMouseButtonUp(0)) {
				if(!showButtons){
					charButtons = sideButtons = showJournal = showHelp = false;
				}
				showButtons = false;
			}
		}
	}
}
