using UnityEngine;
using System.Collections;

public class clickChar : MonoBehaviour
{
	// ====================================================================== data
	public GameObject g;													// this gameobject
	private Shelter _shelter; 												// the shelter data
	private Visitor _visitors; 												// grab info about newcomers
	private UI _ui;															// the UI class
	private GameTime _gametime;												// game time class
	private StartNewConversation _startNewConversation;						// start new conversation
	private Dialogue _Diag; 												// dialogue system
	private Conditions _Cond; 												// conditions

	int arrayIndex; 														// character index in shelter.survivor
	Survivor visitorAtGate;


	// ====================================================================== init
	// Use this for initialization
	void Start ()
	{
		_shelter = g.GetComponent<Shelter> ();
		_visitors = g.GetComponent<Visitor> ();
		_ui = g.GetComponent<UI> ();
		_gametime = g.GetComponent<GameTime> ();
		_startNewConversation = g.GetComponent<StartNewConversation> ();
		_Cond = g.GetComponent<Conditions>();

		//invisible at first
		renderer.enabled = false;

	}

	// ===================================================================== action
	// check for mouse click
	private void OnMouseDown ()
	{
		if (this.tag == "NewVisitor") {
			if(visitorAtGate)
			{
				if(!_ui.tutorial){
					_startNewConversation.ClickCheck("gate");
				}
			}
		}
		else{
			//find corresponding character within shelter
			arrayIndex = -1;
			for (int i = 0; i < _shelter.NumberOfSurvivors; i++) {
				if (this.tag == _shelter._survivors [i].Name) {
					arrayIndex = i;
				}
			}
			if (arrayIndex == -1) {
				Debug.Log ("Couldn't find index for " + this.tag + ".  Make sure this this tag is exactly the same as the character's name.");
			}

			// adjust world view
			Vector3 pos = Camera.main.WorldToViewportPoint (this.transform.position);
			float x = pos.x;
			float y = 1 - pos.y;

			// send message to ui
			_ui.charClick (arrayIndex, x, y);
		}
	}

	void Update()
	{
		visitorAtGate = _visitors._personList [_gametime._currentDay];

		// for newVisitor sprite
		if (this.tag == "NewVisitor") {

			// special case Day 0
			if (_gametime._currentDay == 0) {
				visitorAtGate = null;
			}

			// special case Day 1 
			if(_gametime._currentDay == 1){
				renderer.enabled = true;
				// between talk 2 and Marina show up
				if(_startNewConversation.getConv("Conv_1_1")
				   && _Cond.getCondition("inCamp", "Brian")
				   && !_startNewConversation.getConv("Conv_1_3") || visitorAtGate == null){
					renderer.enabled = false;
					visitorAtGate = null;
				}
			}else{
				// check if render
				if (visitorAtGate != null && _ui.fading != 1) {
					renderer.enabled = true;
				} else {
					renderer.enabled = false;
				}
			}
		}
	}
}
