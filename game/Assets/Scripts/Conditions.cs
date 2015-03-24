using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Conditions : MonoBehaviour {

	// ============================================== data
	private Dictionary<int, bool> _conditionBase;

	// ============================================== initialize

	// Use this for initialization
	void Start () {

		// initiate data base
		_conditionBase = new Dictionary<int, bool>();

		// ==================== PLEASE CREATE BOOLEAN HERE!!!! =====================
		// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

		// add conditions index	value	  condition detail
		_conditionBase.Add (0, false); // Whether Marina is invited into camp





		// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
		// ==========================================================================
	}

	// ============================================== functions

	// get conditions 
	public bool getCondition(int index){

		bool tmp = false; // tmp value

		if(_conditionBase.TryGetValue(index, out tmp)){
			return tmp; // return boolean
		}else{
			Debug.Log ("Error: No such condition"); // return error message
			return false;
		}
	}

	// set conditions
	public void setCondition(int index, bool cond){
		bool tmp = false; // tmp value
		
		if(_conditionBase.TryGetValue(index, out tmp)){
			_conditionBase[index] = cond; // set condition
		}else{
			Debug.Log ("Error: No such condition"); // return error message
		}
	}
}
