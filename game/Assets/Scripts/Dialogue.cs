using UnityEngine;
using System.Collections;

public class Dialogue : MonoBehaviour
{

		// ==================================================== data
		public PixelCrushers.DialogueSystem.DialogueSystemController _DiagCon; // dialogue system
		private bool diaChoice = false; // whether the conversation have a choice
		private int choiceID = -1; // what is current choice
		private int lastID = -1; // what is last choice
		// ==================================================== data

		// start the conversation 
		public void startConv (string name, bool choice)
		{
				diaChoice = choice;
				_DiagCon.StartConversation (name);
		}

		// Update
		void Update ()
		{
				if (_DiagCon.IsConversationActive) {
						if (diaChoice) {
							choiceID = _DiagCon.getID ();
						}
				} else {
						if (choiceID != -1) {
							lastID = choiceID;
							choiceID = -1;
						}
				}

				if (lastID != -1) {
						// parse the boolean
						lastID = -1;

				}
		}
}
