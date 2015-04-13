/// <summary>
/// Conditions. Acts as an interface between the dialog toolkit and the main game. 
/// Essentially this class contains three dictionary of boolean values.
/// the dictionaries are execute, decline, gainApproval and loseApproval.
/// These dictionary will map strings to boolean values that are either true or false. 
/// The boolean values will operate as basic commands.
/// The three different commands we are starting with include
/// 
/// execute <name> (execute in camp)
/// kill <name> (execute at gate)
/// decline <name> 
/// invite <name>
/// 
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Conditions : MonoBehaviour
{
	// ============================================== data
	public Visitor _visitor; 							// reference to the visitor class
	public Shelter _shelter; 							// reference to the shelter class
	
	// dialogue dictionary
	private Dictionary <string, bool> _alive;			// whether character is still alive
	private Dictionary <string, bool> _inCamp;			// whether character is in camp

	// ============================================== initialize
	// Use this for initialization
	void Start ()
	{
		// get reference
		_shelter = this.GetComponent<Shelter> (); 
		_visitor = this.GetComponent<Visitor> (); 

		// initiate data base
		_alive = new Dictionary<string, bool> ();
		_inCamp = new Dictionary<string, bool> ();

		// ==================== PLEASE CREATE BOOLEAN HERE!!!! =====================
		// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

		// add conditions index	value	  condition detail

		//for each character, add their name to the four condition bases
		foreach (Survivor character in _visitor._personList) {
			_alive.Add (character.Name, true);
			_inCamp.Add(character.Name, false);
		}
		// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
		// ==========================================================================
	}

	// ============================================== accessor
	// get conditions 
	public bool getCondition (string key)
	{
		//First half of the string should be the condition, second half should be the name
		//Tokenize the string to get the corresponding array
		string [] words = key.Split (' ');

		bool tmp = false; 
		key = words [1];
		
		switch (words [0]) { // no break because return -> break never reached
		case "alive": // execute the survivor
			if (_alive.TryGetValue (key, out tmp)) {
				return tmp; // return boolean
			} else {
				Debug.Log ("Error: No such condition"); // return error message
				return false;
			}
		case "inCamp": // decline survivor entrance to camp
			if (_inCamp.TryGetValue (key, out tmp)) {
				return tmp; // return boolean
			} else {
				Debug.Log ("Error: No such condition"); // return error message
				return false;
			}
		}// end of switch
		
		Debug.Log ("Error: No such command"); // return error message
		return false;
	}

	// ============================================== functions
	// set conditions
	public void setCondition (string key, bool cond)
	{
		//First half of the string should be the condition, second half should be the name
		//Tokenize the string to get the corresponding array
		string [] words = key.Split (' ');

		bool tmp = false;
		key = words [1];
		
		switch (words [0]) {
		case "alive": //execute the survivor
			if (_alive.TryGetValue (key, out tmp)) {
				_alive [key] = cond; // set condition
			} else {
				Debug.Log ("Error: No such condition"); // return error message
			}
			break;
		case "inCamp":
			if (_inCamp.TryGetValue (key, out tmp)) {
				_inCamp [key] = cond; // set condition
			} else {
				Debug.Log ("Error: No such condition"); // return error message
			}
			break;
		}
	}
}
