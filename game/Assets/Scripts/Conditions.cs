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
	private Dictionary <int, bool> _conditions;			// whether character is still alive

	// ============================================== initialize
	// Use this for initialization
	void Start ()
	{
		// get reference
		_shelter = this.GetComponent<Shelter> (); 
		_visitor = this.GetComponent<Visitor> (); 

		// initiate data base
		_conditions = new Dictionary <int, bool> ();

		// ==================== PLEASE CREATE BOOLEAN HERE!!!! =====================
		// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

		// add conditions index	value	  condition detail
		_conditions.Add(0, false);

		// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
		// ==========================================================================
	}

	// ============================================== accessor
	// get conditions 
	public bool getCondition (int key)
	{
		if (_conditions.ContainsKey (key)) {
			return _conditions[key];
		}

		Debug.LogError ("GET_CONDITION: NO SUCH KEY");
		return false;
	}

	// ============================================== functions
	// set conditions
	public void setCondition (int key, bool cond)
	{
		if (_conditions.ContainsKey (key)) {
			_conditions[key] = true;
		}

		Debug.Log (key);
		Debug.LogError ("SET_CONDTION: NO SUCH KEY");
	}
}
