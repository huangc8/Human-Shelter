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

	private UI _ui;

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
		if (_ui == null) {
			_ui = g.GetComponent<UI> ();
			
		}
		showButtons = false;


	}
	
	
	private void OnMouseDown()
	{
		//find corresponding character within shelter
		arrayIndex = -1;
		for (int i = 0; i < _shelter.NumberOfSurvivors; i++) {
			if (this.tag == _shelter._survivors[i].Name)
			{
				arrayIndex = i;
			}
			
		}
		if (arrayIndex == -1)
		{
			Debug.Log ("Couldn't find index for " + this.tag + ".  Make sure this this tag is exactly the same as the character's name.");
		}


		Vector3 pos = Camera.main.WorldToViewportPoint(this.transform.position);
		
		float x = pos.x;
		float y = 1-pos.y;
		
		_ui.charClick(arrayIndex, x, y);



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
	

	// Update is called once per frame
	void Update () {
		
	}
}
