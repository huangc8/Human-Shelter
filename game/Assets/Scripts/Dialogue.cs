using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Dialogue : MonoBehaviour
{

	// ==================================================== data
	public PixelCrushers.DialogueSystem.DialogueSystemController _DiagCon; // dialogue system
	public UI _ui;	// ui reference
	public GameTime _gt; // gametime reference
	private Shelter _shelter; 	// shelter class reference
	private Visitor _visitor;	// visitor class reference
	private bool diaChoice = false; // whether the conversation have a choice
	private int choiceID = -1; // what is current choice
	private int lastID = -1; // what is last choice

	// ==================================================== initialization
	void Start ()
	{
		_shelter = this.GetComponent<Shelter> ();
		_visitor = this.GetComponent<Visitor> ();
	}

	// ==================================================== functions
	// start the conversation 
	public void startConv (string name, bool choice)
	{
		diaChoice = choice;
		_DiagCon.StartConversation (name);
	}

	// parsing choices made in conversation
	public void parseChoice (int choiceID)
	{
		Survivor visitorAtGate;
		// setting condition according to ID
				
		switch (_shelter._gametime._currentDay) {
		case 1:
			switch (choiceID) {
			case 23: // **************invite Marina**************
				visitorAtGate = _visitor._personList [1];
				_shelter.InviteSurvivor (visitorAtGate);
				break;
			case 24: // **************decline Marina*************
				visitorAtGate = _visitor._personList [1];
				_shelter.RejectSurvivor (visitorAtGate);
				break;
			case 25: // **************invite Brian***************
				visitorAtGate = _visitor._personList [0];
				_shelter.InviteSurvivor (visitorAtGate);
				break;
			}
			break;

		// --------------------- day 2 -----------------------
		case 2:
			switch (choiceID) {
			case 15: // **************invite David***************
				visitorAtGate = _visitor._personList [2];
				_shelter.InviteSurvivor (visitorAtGate);
				break;
			case 16: // **************decline David***************
				visitorAtGate = _visitor._personList [2];
				_shelter.RejectSurvivor (visitorAtGate);
				break;
			}
			break;

		// --------------------- day 3 -----------------------
		case 3:
			switch (choiceID) {
			case 9: // **************invite Eric**************
				visitorAtGate = _visitor._personList [3];
				_shelter.InviteSurvivor (visitorAtGate);
				break;
			case 10: // ************* reject Eric
				visitorAtGate = _visitor._personList [3];
				_shelter.RejectSurvivor (visitorAtGate);
				break;
			}
			break;
		// --------------------- day 4 -----------------------
		case 4:
			switch (choiceID) {
			case 10: //************* invite Danny
				visitorAtGate = _visitor._personList [4];
				_shelter.InviteSurvivor (visitorAtGate);
				break;
			case 11://************* decline Danny	
				visitorAtGate = _visitor._personList [4];
				_shelter.RejectSurvivor (visitorAtGate);
				break;
			}
			break;
		// --------------------- day 5 -----------------------
		case 5:
			switch (choiceID) {
			}
			break;
		// --------------------- day 6 -----------------------
		case 6:
			switch (choiceID) {
			case 11: //************** invite Bree
				visitorAtGate = _visitor._personList [6];
				_shelter.InviteSurvivor (visitorAtGate);
				break;
			case 12: //****************** decline Bree
				visitorAtGate = _visitor._personList [6];
				_shelter.RejectSurvivor (visitorAtGate);
				break;
			}
			break;
		// --------------------- day 7 -----------------------
		case 7:
			switch (choiceID) {
			case 16: //****************** invite Shane
				visitorAtGate = _visitor._personList [7];
				_shelter.InviteSurvivor (visitorAtGate);
				break;
			case 17: //****************** Evict Shane
				visitorAtGate = _visitor._personList [7];
				_shelter.RejectSurvivor (visitorAtGate);
				break;
			}
			break;
		} // end of swtich
	}

	// ==================================================== update
	// Update
	void Update ()
	{	
		// if conversation is going on, update choice
		if (_DiagCon.IsConversationActive) {
			_ui.showAllButtons = false;
			if (diaChoice) {
				choiceID = _DiagCon.getID ();
			}
		} else {
			_ui.showAllButtons = true;
			// if choice,  
			if (choiceID != -1) {
				lastID = choiceID;
				choiceID = -1;
			}
		}

		// parse condition
		if (lastID != -1) {
			parseChoice (lastID);
			lastID = -1;
		}
	}
}
