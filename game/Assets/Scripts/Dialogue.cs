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
			}
			break;
		// --------------------- day 3 -----------------------
		case 3:
			switch (choiceID) {
			case 15: // **************invite David***************
				visitorAtGate = _visitor._personList [_shelter._gametime._currentDay];
				_shelter.InviteSurvivor (visitorAtGate);
				break;
			case 16: // **************decline David***************
				visitorAtGate = _visitor._personList [_shelter._gametime._currentDay];
				_shelter.RejectSurvivor (visitorAtGate);
				break;
			}
			break;
		// --------------------- day 4 -----------------------
		case 4:
			switch (choiceID) {
			}
			break;
		// --------------------- day 5 -----------------------
		case 5:
			switch (choiceID) {
			case 9: // **************invite Eric**************
				visitorAtGate = _visitor._personList [_shelter._gametime._currentDay];
				_shelter.InviteSurvivor (visitorAtGate);
				break;
			case 10: // ************* reject Eric
				visitorAtGate = _visitor._personList [_shelter._gametime._currentDay];
				_shelter.RejectSurvivor (visitorAtGate);
				break;
			}
			break;
		// --------------------- day 6 -----------------------
		case 6:
			switch (choiceID) {
			case 11: //************** invite Bree
				visitorAtGate = _visitor._personList [_shelter._gametime._currentDay];
				_shelter.InviteSurvivor (visitorAtGate);
				break;
			case 12: //****************** decline Bree
				visitorAtGate = _visitor._personList [_shelter._gametime._currentDay];
				_shelter.RejectSurvivor (visitorAtGate);
				break;
			}
			break;
		// --------------------- day 7 -----------------------
		case 7:
			switch (choiceID) {
			}
			break;
		// --------------------- day 8 -----------------------
		case 8:
			switch (choiceID) {
			}
			break;
		// --------------------- day 9 -----------------------
		case 9:
			switch (choiceID) {
			}
			break;
		// --------------------- day 10 -----------------------
		case 10:
			switch (choiceID) {
			}
			break;
		// --------------------- day 11 -----------------------
		case 11:
			switch (choiceID) {
			case 10: //************* invite Danny
				visitorAtGate = _visitor._personList [_shelter._gametime._currentDay];
				_shelter.InviteSurvivor (visitorAtGate);
				break;
			case 11://************* decline Danny	
				visitorAtGate = _visitor._personList [_shelter._gametime._currentDay];
				_shelter.RejectSurvivor (visitorAtGate);
				break;
			}
			break;
		// --------------------- day 12 -----------------------
		case 12:
			switch (choiceID) {
			case 16: //****************** invite Shane
				visitorAtGate = _visitor._personList [_shelter._gametime._currentDay];
				_shelter.InviteSurvivor (visitorAtGate);
				break;
			case 17: //****************** Evict Shane
				visitorAtGate = _visitor._personList [_shelter._gametime._currentDay];
				_shelter.RejectSurvivor (visitorAtGate);
				break;
			}
			break;
		// --------------------- day 13 -----------------------
		case 13:
			switch (choiceID) {
			}
			break;
		// --------------------- day 14 -----------------------
		case 14:
			switch (choiceID) {
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
