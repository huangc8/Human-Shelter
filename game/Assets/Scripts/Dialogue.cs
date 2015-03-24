using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Dialogue : MonoBehaviour
{

		// ==================================================== data
		public PixelCrushers.DialogueSystem.DialogueSystemController _DiagCon; // dialogue system
		public Conditions _conditions; // condition data base
		private bool diaChoice = false; // whether the conversation have a choice
		private int choiceID = -1; // what is current choice
		private int lastID = -1; // what is last choice

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
				switch (choiceID) {

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
						}
				} else {
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
