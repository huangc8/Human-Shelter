using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Conditions : MonoBehaviour
{
	// survivor conditions
	public class survCond{
	
		public bool alive;
		public bool inCamp;
		public bool special;

		public survCond(){
			alive = true;
			inCamp = false;
			special = false;
		}

	}// end of survivor Cond Class

	// ============================================== data
	public Visitor _visitor; 							// reference to the visitor class
	public Shelter _shelter; 							// reference to the shelter class
	
	// dialogue dictionary
	private Dictionary <string, survCond> _conditions;			// whether character is still alive

	// ============================================== initialize
	// Use this for initialization
	void Start ()
	{
		// get reference
		_shelter = this.GetComponent<Shelter> (); 
		_visitor = this.GetComponent<Visitor> (); 

		// initiate data base
		_conditions = new Dictionary <string, survCond> ();

		// ==================== PLEASE CREATE BOOLEAN HERE!!!! =====================
		// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

		// add conditions index	value	  condition detail
		_conditions.Add ("Brian", new survCond());
		_conditions.Add ("Marina", new survCond ());
		_conditions.Add ("Eric", new survCond ());
		_conditions.Add ("Danny", new survCond ());
		_conditions.Add ("Bree", new survCond ());
		_conditions.Add ("Shane", new survCond ());
		_conditions.Add ("David", new survCond ());

		// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
		// ==========================================================================
	}

	// ============================================== accessor
	// get conditions 
	public bool getCondition (string key, string name)
	{
		survCond tmp = _conditions [name];

		switch (key) {
		case "alive":
			return tmp.alive;
		case "inCamp":
			return tmp.inCamp;
		}

		Debug.LogError ("getCondition: no such condition");

		return false;
	}

	// ============================================== functions
	// set conditions
	public void setCondition (string key, string name, bool cond)
	{
		survCond tmp = _conditions [name];
		
		switch (key) {
		case "alive":
			tmp.alive = cond;
			break;
		case "inCamp":
			tmp.inCamp = cond;
			break;
		}
	}
}
