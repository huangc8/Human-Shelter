using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Dialogue : MonoBehaviour
{

		// ==================================================== data
		public PixelCrushers.DialogueSystem.DialogueSystemController _DiagCon; // dialogue system
		public Conditions _conditions; 	// condition data base
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
				case 9: // invite Marina
						visitorAtGate = _visitor._personList [3];
						_shelter.InviteSurvivor (visitorAtGate);
						_conditions.setCondition ("invite Eric", true);
						_conditions.setCondition ("decline Eric", false);
						break;
				case 10: 
						if (_gt._currentDay == 3) {	// decline Eric
								visitorAtGate = _visitor._personList [3];
								_shelter.EvictSurvivor (visitorAtGate);
								_conditions.setCondition ("invite Eric", false);
								_conditions.setCondition ("decline Eric", true);
						} else {// invite Danny
								visitorAtGate = _visitor._personList [4];
								_shelter.InviteSurvivor (visitorAtGate);
								_conditions.setCondition ("invite Danny", false);
								_conditions.setCondition ("decline Danny", true);
						}
						break;
				case 11:
						if (_gt._currentDay == 3) { // decline Danny	
								visitorAtGate = _visitor._personList [4];
								_shelter.EvictSurvivor (visitorAtGate);
								_conditions.setCondition ("invite Eric", false);
								_conditions.setCondition ("decline Eric", true);
						} else { // invite Bree
								visitorAtGate = _visitor._personList [6];
								_shelter.InviteSurvivor (visitorAtGate);
								_conditions.setCondition ("invite Bree", true);
								_conditions.setCondition ("decline Bree", false);
						}
						break;
				case 12: // decline Bree
						visitorAtGate = _visitor._personList [6];
						_shelter.EvictSurvivor (visitorAtGate);
						_conditions.setCondition ("invite Bree", false);
						_conditions.setCondition ("decline Bree", true);
						break;
				case 16: // invite Shane
						visitorAtGate = _visitor._personList [7];
						_shelter.InviteSurvivor (visitorAtGate);
						_conditions.setCondition ("invite Shane", true);
						_conditions.setCondition ("decline Shane", false);
						break;
				case 17:
						visitorAtGate = _visitor._personList [7];
						_shelter.EvictSurvivor (visitorAtGate);
						_conditions.setCondition ("invite Shane", false);
						_conditions.setCondition ("decline Shane", true);
						break;
				case 23: // invite Marina in
						visitorAtGate = _visitor._personList [1];
						_shelter.InviteSurvivor (visitorAtGate);
						_conditions.setCondition ("invite Marina", true);
						_conditions.setCondition ("decline Marina", false);
						break;
				case 24: // decline Marina
						visitorAtGate = _visitor._personList [1];
						_shelter.EvictSurvivor (visitorAtGate);
						_conditions.setCondition ("invite Marina", false);
						_conditions.setCondition ("decline Marina", true);
						break;
				case 34: // invite Brian in
						visitorAtGate = _visitor._personList [0];
						_shelter.InviteSurvivor (visitorAtGate);
						_conditions.setCondition ("invite Brian", true);
						_conditions.setCondition ("decline Brian", false);
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
