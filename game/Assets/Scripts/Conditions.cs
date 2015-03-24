/// <summary>
/// Conditions. Acts as an interface between the dialog toolkit and the main game. Essentially this class contains a dictionary of boolean values.
/// This dictionary will map strings to boolean values that are either true or false. The boolean values will operate as basic commands.
/// The three different commands we are starting with include
/// 
/// execute <name>
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
		private Dictionary <string, bool> _conditionBase;

		// ============================================== initialize

		// Use this for initialization
		void Start ()
		{

				// initiate data base
				_conditionBase = new Dictionary<string, bool> ();

				// ==================== PLEASE CREATE BOOLEAN HERE!!!! =====================
				// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

				// add conditions index	value	  condition detail
				_conditionBase.Add ("invite Marina", false); // Whether Marina is invited into camp





				// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
				// ==========================================================================
		}

		// ============================================== functions

		// get conditions 
		public bool getCondition (string key)
		{

				bool tmp = false; // tmp value

				if (_conditionBase.TryGetValue (key, out tmp)) {
						return tmp; // return boolean
				} else {
						Debug.Log ("Error: No such condition"); // return error message
						return false;
				}
		}

		// set conditions
		public void setCondition (string key, bool cond)
		{
				bool tmp = false; // tmp value
		
				if (_conditionBase.TryGetValue (key, out tmp)) {
						_conditionBase [key] = cond; // set condition
				} else {
						Debug.Log ("Error: No such condition"); // return error message
				}
		}
}
