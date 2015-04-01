using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StartNewConversation : MonoBehaviour {
	GameTime _gameTime;

	private Dictionary<string,bool> hadConversation;
	public Dialogue _dialogue; // the dialogue system

	// Use this for initialization
	void Start () {
		hadConversation = new Dictionary<string, bool>();
		_gameTime = this.GetComponent<GameTime>();
		hadConversation["Conv_1_1"] = false;
		hadConversation["Conv_1_2"] = false;

		hadConversation["Conv_2_1"] = false;
		hadConversation["Conv_2_2"] = false;

		hadConversation["Conv_3_1"] = false;
		hadConversation["Conv_3_2"] = false;

		hadConversation["Conv_4_1"] = false;
		hadConversation["Conv_4_2"] = false;

		hadConversation["Conv_5_1"] = false;
		hadConversation["Conv_5_2"] = false;

		hadConversation["Conv_6_1"] = false;

		hadConversation["Conv_7_1"] = false;
		hadConversation["Conv_7_2"] = false;
		hadConversation["Conv_7_3"] = false;
	}
	
	// Update is called once per frame
	void Update () {
		//Switch based off of day
		switch(_gameTime._currentDay){
		case 1:
			if(hadConversation["Conv_1_1"] == false){
				hadConversation["Conv_1_1"] = true;
				//start conv
			}
			break;
		case 2:
			break;
		case 3:
			break;
		case 4:
			break;
		case 5:
			break;
		case 6:
			break;
		case 7:
			break;
		}
	}

	/// <summary>
	/// Check for conversations after clicking on a thing.
	/// </summary>
	/// <param name="name">Name.</param>
	public void ClickCheck(string name){
		//Switch based off of day
		switch(_gameTime._currentDay){
		case 1:
			if(hadConversation["Conv_1_2"] == false && name == "gate"){
				hadConversation["Conv_1_2"] = true;
				//start conv
				_dialogue.startConv("Conv_1_2", true);
			}
			break;
		case 2:
			if(hadConversation["Conv_2_1"] == false && name == "Brian"){
				hadConversation["Conv_2_1"] = true;
				//start conv
				_dialogue.startConv("Conv_2_1", false);
			}
			
			if(hadConversation["Conv_2_2"] == false && name == "Marina"){
				hadConversation["Conv_2_2"] = true;
				//start conv
				_dialogue.startConv("Conv_2_2", false);
			}

			break;
		case 3:
			if(hadConversation["Conv_3_1"] == false && name == "Brian"){
				hadConversation["Conv_3_1"] = true;
				//start conv
				_dialogue.startConv("Conv_3_1", false);
			}

			if(hadConversation["Conv_3_2"] == false && name == "gate"){
				hadConversation["Conv_3_2"] = true;
				//start conv
				_dialogue.startConv("Conv_3_2", false);
			}

			break;
		case 4:
			if(hadConversation["Conv_4_1"] == false && name == "Marina"){
				hadConversation["Conv_4_1"] = true;
				//start conv
				_dialogue.startConv("Conv_4_1", false);
			}

			
			if(hadConversation["Conv_4_2"] == false && name == "gate"){
				hadConversation["Conv_4_2"] = true;
				//start conv
				_dialogue.startConv("Conv_4_2", false);
			}
			break;
		case 5:
			
			if(hadConversation["Conv_5_1"] == false && name == "Brian"){
				hadConversation["Conv_5_1"] = true;
				//start conv
				_dialogue.startConv("Conv_5_1", false);
			}

			if(hadConversation["Conv_5_2"] == false && name == "Eric"){
				hadConversation["Conv_5_2"] = true;
				//start conv
				_dialogue.startConv("Conv_5_2", false);
			}
			break;
		case 6:
			
			if(hadConversation["Conv_6_1"] == false && name == "gate"){
				hadConversation["Conv_6_1"] = true;
				//start conv
				_dialogue.startConv("Conv_6_1", false);
			}
			break;
		case 7:
			
			if(hadConversation["Conv_7_1"] == false && name == "gate"){
				hadConversation["Conv_7_1"] = true;
				//start conv
				_dialogue.startConv("Conv_7_1", false);
			}
			if(hadConversation["Conv_7_2"] == false && name == "Bree"){
				hadConversation["Conv_7_2"] = true;
				//start conv
				_dialogue.startConv("Conv_7_2", false);
			}
			if(hadConversation["Conv_7_3"] == false && name == "Danny"){
				hadConversation["Conv_7_3"] = true;
				//start conv
				_dialogue.startConv("Conv_7_3", false);
			}
			break;
		}

	}
}
