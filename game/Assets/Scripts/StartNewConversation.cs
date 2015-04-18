using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StartNewConversation : MonoBehaviour
{

	// ====================================================== data
	public Dialogue _dialogue; 							// the dialogue system
	private GameTime _gameTime;							// game time reference
	private Shelter _shelter;							// shelter reference
	private Conditions _cond;							// condition reference
	private Dictionary<string,bool> hadConversation; 	// whether conversation is triggered
	
	private bool talked = false;

	// ====================================================== initialization
	// Use this for initialization
	void Start ()
	{
		_gameTime = this.GetComponent<GameTime> ();
		_shelter = this.GetComponent<Shelter>();
		_cond = this.GetComponent<Conditions> ();

		hadConversation = new Dictionary<string, bool> ();
		//-------------------- Day 1----------------------
		hadConversation ["Conv_1_1"] = false; // Brian at gate -> invite #25
		hadConversation ["Conv_1_2"] = false; // Marina at gate -> invite #23 / reject #24
		hadConversation ["Conv_1_3"] = false; // Talk to Brian about people at gate

		//-------------------- Day 2----------------------
		hadConversation ["Conv_2_1"] = false; // Talk to Brian
		hadConversation ["Conv_2_2"] = false; // Talk to Marina
		hadConversation ["Conv_2_3"] = false; // David at gate -> invite #15 / reject #16
		hadConversation ["Chat_2_1"] = false; // Chat David and Marina -> both David and Marina is in the camp

		//-------------------- Day 3----------------------
		hadConversation ["Conv_3_1"] = false; // Talk to Brian
		hadConversation ["Conv_3_2"] = false; // Eric at gate -> invite #9 / reject #10

		//-------------------- Day 4----------------------
		hadConversation ["Conv_4_1"] = false; // Talk to Marina
		hadConversation ["Conv_4_2"] = false; // Danny at gate -> invite # 10 / reject #11
		hadConversation ["Conv_4_3"] = false; // Talk to David

		//-------------------- Day 5----------------------
		hadConversation ["Conv_5_1"] = false; // Talk to Brian
		hadConversation ["Conv_5_2"] = false; // Talk to Eric
		hadConversation ["Chat_5_1"] = false; // Chat David and Marina -> both David and Marina is in the camp

		//-------------------- Day 6----------------------
		hadConversation ["Conv_6_1"] = false; // Bree at gate -> invite #11 / reject #12
		hadConversation ["Chat_6_1"] = false; // Chat David and Marina -> both David and Marina is in the camp 
		hadConversation ["Chat_6_2"] = false; // Chat Eric and Brian -> both Eric and Brian is in the camp 


		//-------------------- Day 7----------------------
		hadConversation ["Conv_7_1"] = false; // Shane at gate
		hadConversation ["Conv_7_2"] = false; // Talk to Bree
		hadConversation ["Conv_7_3"] = false; // Talk to Danny
		hadConversation ["Chat_7_1"] = false; // Chat David and Marina -> both David and Marina is in the camp 

		//------------------- Reports --------------------
		hadConversation ["Report_1"] = false;

		//------------------- Shrug ----------------------
		hadConversation ["Shrug_Brian"] = true;
		hadConversation ["Shrug_Marina"] = true;
		hadConversation ["Shrug_David"] = true;
		hadConversation ["Shrug_Eric"] = true;
		hadConversation ["Shrug_Danny"] = true;
		hadConversation ["Shrug_Bree"] = true;
		hadConversation ["Shrug_Shane"] = true;

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
				break;
			}

			if(hadConversation ["Conv_2_3"] == false && name == "gate"){
				talk("Conv_2_3", true);
			}

			if(hadConversation ["Chat_2_1"] == false && name == "Marina" && ChatCheck ("Chat_2_1")){
				talk("Chat_2_1", false);
				break;
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

			if(hadConversation ["Conv_4_3"] == false && name == "David"){
				talk ("Conv_4_3", false);
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

			if(hadConversation ["Chat_5_1"] == false && name == "Marina" && ChatCheck("Chat_5_1")){
				talk ("Chat_5_1", false);
			}
			break;
		// --------------------- day 6 -----------------------
		case 6:
			if (hadConversation ["Conv_6_1"] == false && name == "gate") {
				talk ("Conv_6_1", true);
			}
			if(hadConversation ["Chat_6_1"] == false && name == "Marina" && ChatCheck("Chat_6_1")){
				talk ("Chat_6_1", false);
			}
			if(hadConversation ["Chat_6_2"] == false && name == "Eric" && ChatCheck("Chat_6_2")){
				talk ("Chat_6_2", false);
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
			if(hadConversation ["Chat_7_1"] == false && name == "Marina" && ChatCheck("Chat_7_1")){
				talk ("Chat_7_1", false);
			}
			break;
		}// end of switch

		// if no important line
		if (!talked) {
			switch (name) {
			case "Brian":
				talk ("Shrug_Brian", false);
				break;
			case "Marina":
				talk ("Shrug_Marina", false);
				break;
			case "Eric":
				talk ("Shrug_Eric", false);
				break;
			case "Danny":
				talk ("Shrug_Danny", false);
				break;
			case "Bree":
				talk ("Shrug_Bree", false);
				break;
			case "Shane":
				talk ("Shrug_Shane", false);
				break;
			case "David":
				talk ("Shrug_David", false);
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
					
			if(hadConversation ["Chat_2_1"] == false && name == "Marina" && ChatCheck ("Chat_2_1")){
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
			if(hadConversation ["Conv_4_3"] == false && name == "David"){
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
			
			if(hadConversation ["Chat_5_1"] == false && name == "Marina" && ChatCheck("Chat_5_1")){
				return true;
			}
			break;
			// --------------------- day 6 -----------------------
		case 6:
			if(hadConversation ["Chat_6_1"] == false && name == "Marina" && ChatCheck("Chat_6_1")){
				return true;
			}
			if(hadConversation ["Chat_6_2"] == false && name == "Eric" && ChatCheck("Chat_6_2")){
				return true;
			}
			break;
			// --------------------- day 7 -----------------------
		case 7:
			if (hadConversation ["Conv_7_2"] == false && name == "Bree") {
				return true;
			}
			if (hadConversation ["Conv_7_3"] == false && name == "Danny") {
				return true;
			}
			if(hadConversation ["Chat_7_1"] == false && name == "Marina" && ChatCheck("Chat_7_1")){
				return true;
			}
			break;
		}// end of switch

		return false;
	}

	// check if chat exist
	public bool ChatCheck(string name){

		switch (name) {
			case "Chat_2_1":
				if(_cond.getCondition("inCamp", "Marina") && _cond.getCondition("inCamp", "David")){
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
			if (hadConversation ["Report_2"] == false) {
				talk ("Report_1", false);
			}
			break;
		// --------------------- day 3 -----------------------
		case 3:
			if (hadConversation ["Report_3"] == false) {
				talk ("Report_1", false);
			}
			break;
		// --------------------- day 4 -----------------------
		case 4:
			if (hadConversation ["Report_4"] == false) {
				talk ("Report_1", false);
			}
			break;
		// --------------------- day 5 -----------------------
		case 5:
			if (hadConversation ["Report_5"] == false) {
				talk ("Report_1", false);
			}
			break;
		// --------------------- day 6 -----------------------
		case 6:
			if (hadConversation ["Report_6"] == false) {
				talk ("Report_1", false);
			}
			break;
		// --------------------- day 7 -----------------------
		case 7:
			if (hadConversation ["Report_7"] == false) {
				talk ("Report_1", false);
			}
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
