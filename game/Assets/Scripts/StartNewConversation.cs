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
		hadConversation ["Chat_2_1"] = false; // Chat Brian & Marina 

		//-------------------- Day 3----------------------
		hadConversation ["Conv_3_1"] = false; // Talk to Brian
		hadConversation ["Conv_3_2"] = false; // David at gate -> invite #15 / reject #16
		hadConversation ["Chat_3_1"] = false; // Chat Marina & David

		//-------------------- Day 4----------------------
		hadConversation ["Conv_4_1"] = false; // Talk to Marina
		hadConversation ["Conv_4_3"] = false; // Talk to David 

		//-------------------- Day 5----------------------
		hadConversation ["Conv_5_1"] = false; // Talk to Brian
		hadConversation ["Conv_5_2"] = false; // Eric at gate -> invite #9 / reject #10
		hadConversation ["Chat_5_1"] = false; // Chat David and Marina

		//-------------------- Day 6----------------------
		hadConversation ["Conv_6_1"] = false; // Bree at gate -> invite #11 / reject #12

		//-------------------- Day 7----------------------
		hadConversation ["Conv_7_1"] = false; // Talk to Eric

		//-------------------- Day 8----------------------
		hadConversation ["Conv_8_1"] = false; // Talk to Bree


		//-------------------- Day 9----------------------
		hadConversation ["Chat_9_1"] = false; // Chat Eric & Brian
		//-------------------- Day 10---------------------
		hadConversation ["Chat_10_1"] = false; // Chat Marina & David

		//-------------------- Day 11---------------------
		hadConversation ["Conv_11_1"] = false; // Danny at gate -> invite #10 / reject #11
		hadConversation ["Conv_11_2"] = false; // Talk to Bree
		//-------------------- Day 12---------------------
		hadConversation ["Conv_12_1"] = false; // Shane at gate -> invite #16 / reject #17
		//-------------------- Day 13---------------------
		hadConversation ["Chat_13_1"] = false; // Chat Brian & Bree 
		//-------------------- Day 14---------------------
		hadConversation ["Chat_14_1"] = false; // Chat David & Marina 

		//------------------- Reports --------------------
		hadConversation ["Report_1"] = false;
		hadConversation ["Report_2"] = false;
		hadConversation ["Report_3"] = false;
		hadConversation ["Report_4"] = false;
		hadConversation ["Report_5"] = false;
		hadConversation ["Report_6"] = false;
		hadConversation ["Report_7"] = false;
		hadConversation ["Report_8"] = false;
		hadConversation ["Report_9"] = false;
		hadConversation ["Report_10"] = false;
		hadConversation ["Report_11"] = false;
		hadConversation ["Report_12"] = false;
		hadConversation ["Report_13"] = false;
		hadConversation ["Report_14"] = false;

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
			if(hadConversation ["Chat_2_1"] == false && (name == "Marina" || name == "Brian") && ChatCheck ("Chat_2_1")){
				talk("Chat_2_1", false);
				break;
			}

			if (hadConversation ["Conv_2_1"] == false && name == "Brian") {
				talk ("Conv_2_1", false);
			}
			
			if (hadConversation ["Conv_2_2"] == false && name == "Marina") {
				talk ("Conv_2_2", false);
			}
			break;
		// --------------------- day 3 -----------------------
		case 3:
			if(hadConversation ["Chat_3_1"] == false && (name == "Marina" || name == "David") && ChatCheck ("Chat_3_1")){
				talk("Chat_3_1", false);
				break;
			}

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

			if (hadConversation ["Conv_5_2"] == false && name == "gate") {
				talk ("Conv_5_2", true);
			}

			if(hadConversation ["Chat_5_1"] == false && (name == "Marina" || name == "David") && ChatCheck("Chat_5_1")){
				talk ("Chat_5_1", false);
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
			if (hadConversation ["Conv_7_1"] == false && name == "Eric") {
				talk ("Conv_7_1", false);
			}
			break;
		// --------------------- day 8 -----------------------
		case 8:
			if (hadConversation ["Conv_8_1"] == false && name == "Bree") {
				talk ("Conv_8_1", false);
			}
			break;
		// --------------------- day 9 -----------------------
		case 9:
			if(hadConversation ["Chat_9_1"] == false && (name == "Brian" || name == "Eric") && ChatCheck("Chat_9_1")){
				talk ("Chat_9_1", false);
			}
			break;
		// --------------------- day 10 -----------------------
		case 10:
			if(hadConversation ["Chat_10_1"] == false && (name == "Marina" || name == "David") && ChatCheck("Chat_10_1")){
				talk ("Chat_10_1", false);
			}
			break;
		// --------------------- day 11 -----------------------
		case 11:
			if (hadConversation ["Conv_11_1"] == false && name == "gate") {
				talk ("Conv_11_1", true);
			}
			if (hadConversation ["Conv_11_2"] == false && name == "Bree") {
				talk ("Conv_11_2", false);
			}
			break;
		// --------------------- day 12 -----------------------
		case 12:
			if (hadConversation ["Conv_12_1"] == false && name == "gate") {
				talk ("Conv_12_1", true);
			}
			break;
		// --------------------- day 13 -----------------------
		case 13:
			if(hadConversation ["Chat_13_1"] == false && (name == "Bree" || name == "Brian") && ChatCheck("Chat_13_1")){
				talk ("Chat_13_1", false);
			}
			break;
		// --------------------- day 14 -----------------------
		case 14:
			if (hadConversation ["Conv_14_1"] == false && name == "Danny") {
				talk ("Conv_14_1", false);
			}
			if(hadConversation ["Chat_14_1"] == false && (name == "Marina" || name == "David") && ChatCheck("Chat_14_1")){
				talk ("Chat_14_1", false);
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
			
			if(hadConversation ["Chat_2_1"] == false && (name == "Marina" || name == "Brian") && ChatCheck ("Chat_2_1")){
				return true;
			}
			break;
			// --------------------- day 3 -----------------------
		case 3:
			if (hadConversation ["Conv_3_1"] == false && name == "Brian") {
				return true;
			}
			
			if(hadConversation ["Chat_3_1"] == false && (name == "Marina" || name == "David") && ChatCheck ("Chat_3_1")){
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
			
			if(hadConversation ["Chat_5_1"] == false && (name == "Marina" || name == "David") && ChatCheck("Chat_5_1")){
				return true;
			}
			break;
			// --------------------- day 6 -----------------------
		case 6:
			if (hadConversation ["Conv_6_1"] == false && name == "gate") {
				return true;
			}
			break;
			// --------------------- day 7 -----------------------
		case 7:
			if (hadConversation ["Conv_7_1"] == false && name == "Eric") {
				return true;
			}
			break;
			// --------------------- day 8 -----------------------
		case 8:
			if (hadConversation ["Conv_8_1"] == false && name == "Bree") {
				return true;
			}
			break;
			// --------------------- day 9 -----------------------
		case 9:
			if(hadConversation ["Chat_9_1"] == false && (name == "Brian" || name == "Eric") && ChatCheck("Chat_9_1")){
				return true;
			}
			break;
			// --------------------- day 10 -----------------------
		case 10:
			if(hadConversation ["Chat_10_1"] == false && (name == "Marina" || name == "David") && ChatCheck("Chat_10_1")){
				return true;
			}
			break;
			// --------------------- day 11 -----------------------
		case 11:
			if (hadConversation ["Conv_11_2"] == false && name == "Bree") {
				return true;
			}
			break;
			// --------------------- day 12 -----------------------
		case 12:
			break;
			// --------------------- day 13 -----------------------
		case 13:
			if(hadConversation ["Chat_13_1"] == false && (name == "Bree" || name == "Brian") && ChatCheck("Chat_13_1")){
				return true;
			}
			break;
			// --------------------- day 14 -----------------------
		case 14:
			if (hadConversation ["Conv_14_1"] == false && name == "Danny") {
				return true;
			}
			if(hadConversation ["Chat_14_1"] == false && (name == "Marina" || name == "David") && ChatCheck("Chat_14_1")){
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

			case "Chat_3_1":
				if(_cond.getCondition("inCamp", "Marina") && _cond.getCondition("inCamp", "David")){
					return true;
				}
			break;

			case "Chat_5_1":
				if(_cond.getCondition("inCamp", "Marina") && _cond.getCondition("inCamp", "David")){
					return true;
				}
			break;

			case "Chat_9_1":
				if(_cond.getCondition("inCamp", "Eric") && _cond.getCondition("inCamp", "Brian")){
					return true;
				}
			break;
		
			case "Chat_10_1":
				if(_cond.getCondition("inCamp", "Marina") && _cond.getCondition("inCamp", "David")){
					return true;
				}
			break;

			case "Chat_13_1":
				if(_cond.getCondition("inCamp", "Brian") && _cond.getCondition("inCamp", "Bree")){
					return true;
				}
			break;

			case "Chat_14_1":
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
			if (hadConversation ["Report_1"] == false) {
				talk ("Report_1", false);
			}
			break;
		// --------------------- day 3 -----------------------
		case 3:
			if (hadConversation ["Report_2"] == false) {
				talk ("Report_2", false);
			}
			break;
		// --------------------- day 4 -----------------------
		case 4:
			if (hadConversation ["Report_3"] == false) {
				talk ("Report_3", false);
			}
			break;
		// --------------------- day 5 -----------------------
		case 5:
			if (hadConversation ["Report_4"] == false) {
				talk ("Report_4", false);
			}
			break;
		// --------------------- day 6 -----------------------
		case 6:
			if (hadConversation ["Report_5"] == false) {
				talk ("Report_5", false);
			}
			break;
		// --------------------- day 7 -----------------------
		case 7:
			if (hadConversation ["Report_6"] == false) {
				talk ("Report_6", false);
			}
			break;
		// --------------------- day 8 -----------------------
		case 8:
			if (hadConversation ["Report_7"] == false) {
				talk ("Report_7", false);
			}
			break;
		// --------------------- day 9 -----------------------
		case 9:
			if (hadConversation ["Report_8"] == false) {
				talk ("Report_8", false);
			}
			break;
		// --------------------- day 10 -----------------------
		case 10:
			if (hadConversation ["Report_9"] == false) {
				talk ("Report_9", false);
			}
			break;
		// --------------------- day 11 -----------------------
		case 11:
			if (hadConversation ["Report_10"] == false) {
				talk ("Report_10", false);
			}
			break;
		// --------------------- day 12 -----------------------
		case 12:
			if (hadConversation ["Report_11"] == false) {
				talk ("Report_11", false);
			}
			break;
		// --------------------- day 13 -----------------------
		case 13:
			if (hadConversation ["Report_12"] == false) {
				talk ("Report_12", false);
			}
			break;
		// --------------------- day 14 -----------------------
		case 14:
			if (hadConversation ["Report_13"] == false) {
				talk ("Report_13", false);
			}
			break;
		// --------------------- day 15 -----------------------
		case 15:
			if (hadConversation ["Report_14"] == false) {
				talk ("Report_14", false);
			}
			break;
		} // end of switch
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
