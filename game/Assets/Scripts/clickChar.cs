using UnityEngine;
using System.Collections;

public class clickChar : MonoBehaviour {

	Shelter _shelter; // the shelter data
	string _newDay; // newDay text
	int _currentDay; // current day
	Visitor _visitors; //grab info about newcomers
	public int _conversationsLeft; // converstation points left
	public Dialogue _dialogue;
	bool BrianTalked;
	public GameObject g;

	bool showButtons;
	int arrayIndex; // location of this character in shelter.survivor



	// Use this for initialization
	void Start () {
		if (_shelter == null) {
			_shelter = g.GetComponent<Shelter> ();
		}
		if (_visitors == null) {
			_visitors = g.GetComponent<Visitor> ();
			
		}

		showButtons = false;


	}
	
	
	private void OnMouseDown()
	{

		arrayIndex = -1;
		for (int i = 0; i < _shelter.NumberOfSurvivors; i++) {
			if (this.tag == _shelter._survivors[i].Name)
			{
				arrayIndex = i;
			}
			
		}
		if (arrayIndex == -1)
		{
			Debug.Log ("Couldn't find index for " + this.tag + ".  Make sure they have exactly the same name.");
		}


		//toggle
		if (!showButtons) {
						showButtons = true;
				} else {
						showButtons = false;
				}


		/*
				for (int i = 0; i < numSurvivors; i++) {
						itY = startY;
						if (GUI.Button (new Rect (startX, itY, buttonWidth, buttonHeight), _shelter._survivors [i].Name)) {
						}
						itY += buttonHeight;
						if (GUI.Button (new Rect (startX, itY, buttonWidth, buttonHeight), "Converse: " + _shelter._survivors [i].ConversationsHad) && _conversationsLeft > 0) {
								_shelter._survivors [i].Converse ();
								_conversationsLeft--;
								if (!BrianTalked) {
										_dialogue.startConv ("Conv_1", false);
										BrianTalked = true;
								}
						}
				}
		*/
	}
	
	void OnGUI()
	{
		if(showButtons)
		{
			//center buttons
			//not using this at the moment because it doesn't really work
			GUIStyle style = GUI.skin.GetStyle("align");
			style.alignment = TextAnchor.MiddleCenter;


			Vector3 pos = Camera.main.WorldToViewportPoint(this.transform.position);
			//Vector2 pos2 = GUIUtility.ScreenToGUIPoint(new Vector2(transform.position.x, transform.position.y));
			//Vector2 pos2 = GUIUtility.ScreenToGUIPoint(new Vector2(pos.x, pos.y));


			float x = pos.x;
			float y = 1-pos.y;
			//Debug.Log(y);
			//Debug.Log(Input.mousePosition.x);

			
			int buttonWidth = 300;
			int buttonHeight = 30;
			string name = _shelter._survivors[arrayIndex].Name;
			

			if (GUI.Button (new Rect (Screen.width*x*.8f, Screen.height*y*.8f, buttonWidth, buttonHeight),"Talk to " + name)) {
				
			}
			if (GUI.Button (new Rect (Screen.width*x*.8f, Screen.height*y*.85f, buttonWidth, buttonHeight),"Assign task")) {
			
			}
		}
	}


	// Update is called once per frame
	void Update () {
		
	}
}
