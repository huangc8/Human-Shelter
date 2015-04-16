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
				switch (choiceID) {
				case 9: // **************invite Eric**************
						visitorAtGate = _visitor._personList [3];
						_shelter.InviteSurvivor (visitorAtGate);
						break;
				case 10: 
						if (_gt._currentDay == 3) {	//************ decline Eric
								visitorAtGate = _visitor._personList [3];
								_shelter.RejectSurvivor (visitorAtGate);
						} else { //************* invite Danny
								visitorAtGate = _visitor._personList [4];
								_shelter.InviteSurvivor (visitorAtGate);
						}
						break;
				case 11:
						if (_gt._currentDay == 4) { //************* decline Danny	
								visitorAtGate = _visitor._personList [4];
								_shelter.RejectSurvivor (visitorAtGate);
						} else { //************** invite Bree
								visitorAtGate = _visitor._personList [6];
								_shelter.InviteSurvivor (visitorAtGate);
						}
						break;
				case 12: //****************** decline Bree
						visitorAtGate = _visitor._personList [6];
						_shelter.RejectSurvivor (visitorAtGate);
						break;
				case 16: //****************** invite Shane
						visitorAtGate = _visitor._personList [7];
						_shelter.InviteSurvivor (visitorAtGate);
						break;
				case 17: //****************** Evict Shane
						visitorAtGate = _visitor._personList [7];
						_shelter.RejectSurvivor (visitorAtGate);
						break;
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
		}

		// ==================================================== update
		// Update
		void Update ()
		{	
				// if conversation is going on, update choice
				if (_DiagCon.IsConversationActive) {
						if (diaChoice) {
							choiceID = _DiagCon.getID ();
							_ui.showAllButtons = false;
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
