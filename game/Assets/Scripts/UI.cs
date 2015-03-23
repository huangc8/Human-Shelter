using UnityEngine;
using System.Collections;

public class UI : MonoBehaviour {

	public GameObject g;
	Shelter _shelter; // the shelter data

	//these are needed for showing and hiding buttons
	private bool charButtons, sideButtons, showButtons;
	private float x, y;
	private int index;

	// Use this for initialization
	void Start () {
		if (_shelter == null) {
			_shelter = g.GetComponent<Shelter> ();
		}
		charButtons = sideButtons = showButtons = false;

	}

	public void charClick(int aindex, float ax, float ay)
	{
		//center buttons
		//not using this at the moment because it doesn't really work
		//GUIStyle style = GUI.skin.GetStyle("align");
		//style.alignment = TextAnchor.MiddleCenter;

		x = ax;
		y = ay;
		index = aindex;
		
		charButtons = true;
		showButtons = true;


	}

	void OnGUI (){
		int buttonWidth = 300;
		int buttonHeight = 30;

		if(charButtons){
			string name = _shelter._survivors[index].Name;

			if (GUI.Button (new Rect (Screen.width*x*.8f, Screen.height*y*.8f, buttonWidth, buttonHeight),"Talk to " + name)) {
				showButtons=true;
			}
			if (GUI.Button (new Rect (Screen.width*x*.8f, Screen.height*y*.85f, buttonWidth, buttonHeight),"Assign task")) {
				showButtons=true;
			}
		}

	}

	// Update is called once per frame
	void Update () {
		
		//this will get rid of buttons when you click anywhere other than a button
		if (Input.GetMouseButtonUp(0)) {
			if(!showButtons){
				charButtons = sideButtons = false;
			}
			showButtons = false;
		}
	}
}
