using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StartNewConversation : MonoBehaviour
{

	// ====================================================== data
	public Dialogue _dialogue; 							// the dialogue system
	private GameTime _gameTime;							// game time reference
	private Shelter _shelter;							// shelter reference
	private Dictionary<string,bool> hadConversation; 	// whether conversation is triggered
	
	private bool talked = false;

	// ====================================================== initialization
	// Use this for initialization
	void Start ()
	{
		_gameTime = this.GetComponent<GameTime> ();
		_shelter = this.GetComponent<Shelter>();

		hadConversation = new Dictionary<string, bool> ();

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

		hadConversation ["Report_1"] = false;

		talked = false;
	}

	// ====================================================== function
	/// <summary>
	/// Check for conversations after clicking on a thing.
	/// </summary>
	/// <param name="name">Name.</param>
	public void ClickCheck (string name)
	{		
		// reset talked
		talked = false;
				
		//Switch based off of day
		switch (_gameTime._currentDay) {
		// --------------------- day 1 -----------------------
		case 1:
			if (hadConversation ["Conv_1_1"] == false && name == "gate") {
					talk ("Conv_1_1", true);				
			} else {
				if (hadConversation ["Conv_1_2"] == false && name == "gate" && hadConversation ["Conv_1_3"]) {
					talk ("Conv_1_2", true);
				}
			}
						
			if (hadConversation ["Conv_1_3"] == false && name == "Brian") {
				talk ("Conv_1_3", false);
			}
			break;
		// --------------------- day 2 -----------------------
		case 2:
			if (hadConversation ["Conv_2_1"] == false && name == "Brian") {
				talk ("Conv_2_1", false);
			}
			
			if (hadConversation ["Conv_2_2"] == false && name == "Marina") {
				talk ("Conv_2_2", false);
			}
			break;
		// --------------------- day 3 -----------------------
		case 3:
			if (hadConversation ["Conv_3_1"] == false && name == "Brian") {
				talk ("Conv_3_1", false);
			}

			if (hadConversation ["Conv_3_2"] == false && name == "gate") {
				talk ("Conv_3_2", true);
			}
			break;
		// --------------------- day 4 -----------------------
		case 4:
			if (hadConversation ["Conv_4_1"] == false && name == "Marina") {
				talk ("Conv_4_1", false);
			}

			
			if (hadConversation ["Conv_4_2"] == false && name == "gate") {
				talk ("Conv_4_2", true);
			}
			break;
		// --------------------- day 5 -----------------------
		case 5:
			
			if (hadConversation ["Conv_5_1"] == false && name == "Brian") {
				talk ("Conv_5_1", false);
			}

			if (hadConversation ["Conv_5_2"] == false && name == "Eric") {
				talk ("Conv_5_2", false);
			}

			if (name == "gate") {
				_dialogue.startConv ("Conv24", false);		
			}
			break;
		// --------------------- day 6 -----------------------
		case 6:
			if (hadConversation ["Conv_6_1"] == false && name == "gate") {
				talk ("Conv_6_1", true);
			}
			break;
		// --------------------- day 7 -----------------------
		case 7:
			if (hadConversation ["Conv_7_1"] == false && name == "gate") {
				talk ("Conv_7_1", true);
			}
			if (hadConversation ["Conv_7_2"] == false && name == "Bree") {
				talk ("Conv_7_2", false);
			}
			if (hadConversation ["Conv_7_3"] == false && name == "Danny") {
				talk ("Conv_7_3", false);
			}
			break;
		}// end of switch

		// if no important line
		if (!talked) {

			talk("Conv 24", false);

			switch (name) {
			case "Brian":
				break;
			case "Marina":
				break;
			case "Eric":
				break;
			case "Danny":
				break;
			case "Bree":
				break;
			case "Shane":
				break;
			case "David":
				break;

			}
		}// end of !talk switch

		// update notify
		NotifyCheck();
	}

	// check if important stuff to say
	public bool NoteCheck(string name){

		//Switch based off of day
		switch (_gameTime._currentDay) {
		// --------------------- day 1 -----------------------
		case 1:
			if (hadConversation ["Conv_1_3"] == false && name == "Brian") {
				return true;
			}
			break;
		// --------------------- day 2 -----------------------
		case 2:
			if (hadConversation ["Conv_2_1"] == false && name == "Brian") {
				return true;
			}
			
			if (hadConversation ["Conv_2_2"] == false && name == "Marina") {
				return true;
			}
			break;
		// --------------------- day 3 -----------------------
		case 3:
			if (hadConversation ["Conv_3_1"] == false && name == "Brian") {
				return true;
			}
			break;
		// --------------------- day 4 -----------------------
		case 4:
			if (hadConversation ["Conv_4_1"] == false && name == "Marina") {
				return true;
			}
			break;
		// --------------------- day 5 -----------------------
		case 5:
			
			if (hadConversation ["Conv_5_1"] == false && name == "Brian") {
				return true;
			}
			
			if (hadConversation ["Conv_5_2"] == false && name == "Eric") {
				return true;
			}
			break;
		// --------------------- day 6 -----------------------
		case 6:
			break;
		// --------------------- day 7 -----------------------
		case 7:
			if (hadConversation ["Conv_7_2"] == false && name == "Bree") {
				return true;
			}
			if (hadConversation ["Conv_7_3"] == false && name == "Danny") {
				return true;
			}
			break;
		}// end of switch

		return false;
	}

	// check if notify turns on
	public void NotifyCheck(){
		for(int i = 0; i < _shelter._numPeople; i++){
			if(_shelter._survivors[i] != null){
				_shelter._survivors[i]._notify = NoteCheck(_shelter._survivors[i].Name);
			}
		}
	}

	// Start Conversation at the beginning of day.
	public void DayCheck ()
	{
		NotifyCheck();

		//Switch based off of day
		switch (_gameTime._currentDay) {
		// --------------------- day 1 -----------------------
		case 1:
			break;
		// --------------------- day 2 -----------------------
		case 2:
			if (hadConversation ["Report_1"] == false) {
				talk ("Report_1", false);
			}
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

	// check for special case 
	public bool specialCase ()
	{

		// switch based off game day
		switch (_gameTime._currentDay) {
		// --------------------- day 1 -----------------------
		case 1:
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

	// start the talking
	private void talk (string name, bool cond)
	{
		hadConversation [name] = true;
		_dialogue.startConv (name, cond);
		talked = true;
	}

	// Gets whether had conv.
	public bool getConv (string key)
	{
		return hadConversation [key];
	}
}
