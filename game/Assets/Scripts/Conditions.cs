/// <summary>
/// Conditions. Acts as an interface between the dialog toolkit and the main game. Essentially this class contains three dictionary of boolean values.
/// the dictionaries are execute, decline, gainApproval and loseApproval.
/// These dictionary will map strings to boolean values that are either true or false. The boolean values will operate as basic commands.
/// The three different commands we are starting with include
/// 
/// execute <name> (execute in camp)
/// kill <name> (execute at gate)
/// decline <name> 
/// invite <name>
/// 
/// gainApproval <name>
/// loseApproval <name>
/// 
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Conditions : MonoBehaviour
{

	// ============================================== data
	public Visitor _visitor; //our reference to the visitor class
	public Shelter _shelter; //our reference to the shelter class

	private Dictionary <string, bool> _executions; //characters to be executed
	private Dictionary <string, bool> _kills;
	private Dictionary <string, bool> _declinations; //characters which have been declined invitation
	private Dictionary <string, bool> _invitations; //characters invited into the camp

#if approval
	private Dictionary <string, int> _gainApproval; //how much approval a character should gain, set to 0 after gaining approval
	private Dictionary <string, int> _loseApproval; //how much approval a character should lose, set to 0 after losing approval
#endif

	// ============================================== initialize

	// Use this for initialization
	void Start ()
	{
		_shelter = this.GetComponent<Shelter>(); //get our reference to the shelter class
		_visitor = this.GetComponent<Visitor>(); //get our reference to the visitor class

		// initiate data base
		_executions = new Dictionary<string, bool>();
		_declinations = new Dictionary<string, bool>();
		_invitations = new Dictionary<string, bool>();
		_kills = new Dictionary<string, bool>();

#if approval
		_gainApproval = new Dictionary<string, int>();
		_loseApproval = new Dictionary<string, int>();
#endif

		// ==================== PLEASE CREATE BOOLEAN HERE!!!! =====================
		// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

		// add conditions index	value	  condition detail

		//for each character, add their name to the four condition bases
		foreach(Survivor character in _visitor._personList){
			Debug.Log ("58--name is:" + character.Name);
			_executions.Add(character.Name,false);
			_declinations.Add(character.Name,false);
			_invitations.Add(character.Name,false);
			_kills.Add(character.Name, false);
#if approval
			_gainApproval.Add(character.Name,0);
			_loseApproval.Add(character.Name,0);
#endif
		}



		// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
		// ==========================================================================
	}

	void Update()
	{

		Debug.LogWarning("_personList.Length" + _visitor._personList.Length + " _personList[0]:" + _visitor._personList[0].Name + " _personList[1]:" + _visitor._personList[1].Name);
		Debug.LogWarning("--Printing out PersonList");
		for(int i = 0; i < _visitor._personList.Length; i++){
			Debug.LogWarning("i = "+ i + " " + _visitor._personList[i].Name);
		}

		if(_visitor != null){
			foreach(Survivor character in _visitor._personList){


				if(getCondition("execute " + character.Name)){
					setCondition("execute " + character.Name, false);
					//kill this character
					_shelter.KillSurvivor(character);
				}
				if(getCondition("decline " + character.Name)){
					setCondition("decline " + character.Name, false);
					//decline entry to this character
					//set the survivor at gate to null

					_visitor.RejectSurvivorAtGate(character.Name);
				}
				if(getCondition("invite " + character.Name)){
					setCondition("invite " + character.Name, false);
					
					_shelter.InviteSurvivor(character);
					//invite this character to the base
				}
				if(getCondition("kill " + character.Name)){
					setCondition("kill " + character.Name, false);
					
					//kill this character at the gate
					_visitor.KillSurvivorAtGate(character.Name);
				}
			}
		}
	}

	// ============================================== functions

	// get conditions 
	public bool getCondition (string key)
	{
		//Tokenize the string to get the corresponding array
		string [] words = key.Split(' ');

		//First half of the string should be the command, second half should
		//be the name
		Debug.Log("getCondition: Command:" + words[0] + " name:" + words[1]);


		bool tmp = false; // tmp value

		key = words[1];

		switch(words[0]){
		case "execute": //execute the survivor
			if (_executions.TryGetValue (key, out tmp)) {
				return tmp; // return boolean
			} else {
				Debug.Log ("Error: No such condition"); // return error message
				return false;
			}
			break;
		case "decline": //decline entrance to the survivor
			if (_declinations.TryGetValue (key, out tmp)) {
				return tmp; // return boolean
			} else {
				Debug.Log ("Error: No such condition"); // return error message
				return false;
			}
			break;
		case "invite": //invite the survivor in
			if (_invitations.TryGetValue (key, out tmp)) {
				return tmp; // return boolean
			} else {
				Debug.Log ("Error: No such condition"); // return error message
				return false;
			}
			break;
		case "kill": //invite the survivor in
			if (_kills.TryGetValue (key, out tmp)) {
				return tmp; // return boolean
			} else {
				Debug.Log ("Error: No such condition"); // return error message
				return false;
			}
			break;
		}
		return false;
	}


	// set conditions
	public void setCondition (string key, bool cond)
	{
		//Tokenize the string to get the corresponding array
		string [] words = key.Split(' ');
		
		//First half of the string should be the command, second half should
		//be the name
		Debug.Log("getCondition: Command:" + words[0] + " name:" + words[1]);
		
		
		bool tmp = false; // tmp value
		
		key = words[1];
		
		switch(words[0]){
		case "execute": //execute the survivor
			if (_executions.TryGetValue (key, out tmp)) {
				_executions [key] = cond; // set condition
			} else {
				Debug.Log ("Error: No such condition"); // return error message
			}
			break;
		case "decline": //decline entrance to the survivor
			if (_declinations.TryGetValue (key, out tmp)) {
				_declinations [key] = cond; // set condition
			} else {
				Debug.Log ("Error: No such condition"); // return error message
			}
			break;
		case "invite": //invite the survivor in
			if (_invitations.TryGetValue (key, out tmp)) {
				_invitations [key] = cond; // set condition
			} else {
				Debug.Log ("Error: No such condition"); // return error message
			}
			break;
		}
	}
}
