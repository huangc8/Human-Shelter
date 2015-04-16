using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StartNewConversation : MonoBehaviour
{

		// ====================================================== data
		GameTime _gameTime;									// game time reference
		public Dialogue _dialogue; 							// the dialogue system
		private Dictionary<string,bool> hadConversation; 	// whether conversation is triggered

		// ====================================================== initialization
		// Use this for initialization
		void Start ()
		{

				hadConversation = new Dictionary<string, bool> ();

				_gameTime = this.GetComponent<GameTime> ();
				hadConversation ["Conv_1_1"] = false;
				hadConversation ["Conv_1_2"] = false;
				hadConversation ["Conv_1_3"] = false;

				hadConversation ["Conv_2_1"] = false;
				hadConversation ["Conv_2_2"] = false;

				hadConversation ["Conv_3_1"] = false;
				hadConversation ["Conv_3_2"] = false;

				hadConversation ["Conv_4_1"] = false;
				hadConversation ["Conv_4_2"] = false;

				hadConversation ["Conv_5_1"] = false;
				hadConversation ["Conv_5_2"] = false;

				hadConversation ["Conv_6_1"] = false;

				hadConversation ["Conv_7_1"] = false;
				hadConversation ["Conv_7_2"] = false;
				hadConversation ["Conv_7_3"] = false;
		}

		// ====================================================== function
		/// <summary>
		/// Check for conversations after clicking on a thing.
		/// </summary>
		/// <param name="name">Name.</param>
		public void ClickCheck (string name)
		{
				//Switch based off of day
				switch (_gameTime._currentDay) {
				// --------------------- day 1 -----------------------
				case 1:
						if(hadConversation ["Conv_1_1"] == false && name == "gate"){
								hadConversation["Conv_1_1"] = true;
								_dialogue.startConv ("Conv_1_1", true);
						}else{
							if (hadConversation ["Conv_1_2"] == false && name == "gate") {
									hadConversation ["Conv_1_2"] = true;
									//start conv
									_dialogue.startConv ("Conv_1_2", true);
							}
						}
						
						if(hadConversation["Conv_1_3"] == false && name == "Brian"){
								hadConversation["Conv_1_3"] = true;
								_dialogue.startConv ("Conv_1_3", false);
						}
						break;
				// --------------------- day 2 -----------------------
				case 2:
						if (hadConversation ["Conv_2_1"] == false && name == "Brian") {
								hadConversation ["Conv_2_1"] = true;
								//start conv
								_dialogue.startConv ("Conv_2_1", false);
						}
			
						if (hadConversation ["Conv_2_2"] == false && name == "Marina") {
								hadConversation ["Conv_2_2"] = true;
								//start conv
								_dialogue.startConv ("Conv_2_2", false);
						}

						break;
				// --------------------- day 3 -----------------------
				case 3:
						if (hadConversation ["Conv_3_1"] == false && name == "Brian") {
								hadConversation ["Conv_3_1"] = true;
								//start conv
								_dialogue.startConv ("Conv_3_1", false);
						}

						if (hadConversation ["Conv_3_2"] == false && name == "gate") {
								hadConversation ["Conv_3_2"] = true;
								//start conv
								_dialogue.startConv ("Conv_3_2", true);
						}
						break;
				// --------------------- day 4 -----------------------
				case 4:
						if (hadConversation ["Conv_4_1"] == false && name == "Marina") {
								hadConversation ["Conv_4_1"] = true;
								//start conv
								_dialogue.startConv ("Conv_4_1", false);
						}

			
						if (hadConversation ["Conv_4_2"] == false && name == "gate") {
								hadConversation ["Conv_4_2"] = true;
								//start conv
								_dialogue.startConv ("Conv_4_2", true);
						}
						break;
				// --------------------- day 5 -----------------------
				case 5:
			
						if (hadConversation ["Conv_5_1"] == false && name == "Brian") {
								hadConversation ["Conv_5_1"] = true;
								//start conv
								_dialogue.startConv ("Conv_5_1", false);
						}

						if (hadConversation ["Conv_5_2"] == false && name == "Eric") {
								hadConversation ["Conv_5_2"] = true;
								//start conv
								_dialogue.startConv ("Conv_5_2", false);
						}

						if(name == "gate"){
								_dialogue.startConv("Conv24", false);		
						}
						break;
				// --------------------- day 6 -----------------------
				case 6:
						if (hadConversation ["Conv_6_1"] == false && name == "gate") {
								hadConversation ["Conv_6_1"] = true;
								//start conv
								_dialogue.startConv ("Conv_6_1", true);
						}
						break;
				// --------------------- day 7 -----------------------
				case 7:
						if (hadConversation ["Conv_7_1"] == false && name == "gate") {
								hadConversation ["Conv_7_1"] = true;
								//start conv
								_dialogue.startConv ("Conv_7_1", true);
						}
						if (hadConversation ["Conv_7_2"] == false && name == "Bree") {
								hadConversation ["Conv_7_2"] = true;
								//start conv
								_dialogue.startConv ("Conv_7_2", false);
						}
						if (hadConversation ["Conv_7_3"] == false && name == "Danny") {
								hadConversation ["Conv_7_3"] = true;
								//start conv
								_dialogue.startConv ("Conv_7_3", false);
						}
						break;
				}
		}

		/// <summary>
		/// Start Conversation at the beginning of day.
		/// </summary>
		/// <param name="day">Day.</param>
		public void DayCheck ()
		{
				//Switch based off of day
				switch (_gameTime._currentDay) {
				// --------------------- day 1 -----------------------
				case 1:
						break;
				// --------------------- day 2 -----------------------
				case 2:
						break;
				// --------------------- day 3 -----------------------
				case 3:
						break;
				// --------------------- day 4 -----------------------
				case 4:
						break;
				// --------------------- day 5 -----------------------
				case 5:
						break;
				// --------------------- day 6 -----------------------
				case 6:
						break;
				// --------------------- day 7 -----------------------
				case 7:
						break;
				}
		}

		public bool specialCase ()
		{
				// switch based off game day
				switch (_gameTime._currentDay) {
				// --------------------- day 1 -----------------------
				case 1:
					if (hadConversation ["Conv_1_1"] == false) {
						return false;
					}
						return true;
				// --------------------- day 2 -----------------------
				case 2:
						return true;
				// --------------------- day 3 -----------------------
				case 3:
						return true;
				// --------------------- day 4 -----------------------
				case 4:
						return true;
				// --------------------- day 5 -----------------------
				case 5:
						return true;
				// --------------------- day 6 -----------------------
				case 6:
						return true;
				// --------------------- day 7 -----------------------
				case 7:
						return true;
				}
						return true;
		}

		/// <summary>
		/// Gets whether had conv.
		/// </summary>
		/// <returns><c>true</c>, if conv was gotten, <c>false</c> otherwise.</returns>
		/// <param name="key">Key.</param>
		public bool getConv(string key){
			return hadConversation[key];
		}
}
