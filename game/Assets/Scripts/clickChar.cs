using UnityEngine;
using System.Collections;

public class clickChar : MonoBehaviour
{
	// ====================================================================== data
	public GameObject g;		// this gameobject
	Shelter _shelter; 			// the shelter data
	Visitor _visitors; 			// grab info about newcomers
	private UI _ui;				// the UI class

	int arrayIndex; 			// character index in shelter.survivor

	// ====================================================================== init
	// Use this for initialization
	void Start ()
	{
		if (_shelter == null) {
			_shelter = g.GetComponent<Shelter> ();
		}
		if (_visitors == null) {
			_visitors = g.GetComponent<Visitor> ();
		}
		if (_ui == null) {
			_ui = g.GetComponent<UI> ();
		}
		//invisible at first
		renderer.enabled = false;
	}

	// ===================================================================== action
	// check for mouse click
	private void OnMouseDown ()
	{
		//find corresponding character within shelter
		arrayIndex = -1;
		for (int i = 0; i < _shelter.NumberOfSurvivors; i++) {
			if (this.tag == _shelter._survivors [i].Name) {
				arrayIndex = i;
			}
		}
		if (arrayIndex == -1) {
			Debug.Log ("Couldn't find index for " + this.tag + ".  Make sure this this tag is exactly the same as the character's name.");
		}

		// adjust world view
		Vector3 pos = Camera.main.WorldToViewportPoint (this.transform.position);
		float x = pos.x;
		float y = 1 - pos.y;

		// send message to ui
		_ui.charClick (arrayIndex, x, y);
	}
}
