/// <summary>
/// Game time. This class controls the flow of time in our game. Time progresses
/// at a rate determined by the user as they complete tasks
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameTime : MonoBehaviour
{

		// =============================================== data
		Shelter _shelter; // the shelter data
		Queue <Event> _events; // today's events
		string _newDay; // newDay text
		int _currentDay; // current day
		Queue<Report> _reports; // the reports of assign task
		public int _conversationsLeft; // converstation points left
		public PixelCrushers.DialogueSystem.DialogueSystemController _DiagCon;
		bool BrianTalked;

		// =============================================== initialization
		// Use this for initialization
		void Start ()
		{
				_conversationsLeft = 5;
				_currentDay = 0;
				_reports = new Queue<Report> ();
				BrianTalked = false;
		}

		// =============================================== action
		/// <summary>
		/// Start a new day, reset all of your values that reset over night
		/// Complete all of the tasks the survivors were sent on
		/// </summary>
		public void newDay ()
		{
				//process the tasks
				evaluateTasks ();
				_conversationsLeft = 5;
				for (int i = 0; i < _shelter.NumberOfSurvivors; i++) {
						_shelter._survivors [i].ConvReset ();
				}
				_currentDay++;
		}


		// ================================================ helper
		/// <summary>
		/// Evaluates the tasks. Carry out the task for each survivor
		/// </summary>
		void evaluateTasks ()
		{
				//Evaluate each task
				for (int s = 0; s < _shelter.NumberOfSurvivors; s++) {
						//carry out the task
						Report r = new Report ();
						switch (_shelter._survivors [s].AssignedTask) {
						case Survivor.task.Scout:
								r = _shelter._survivors [s].Scout (_shelter);
								break;
						case Survivor.task.Heal:
								r = _shelter._survivors [s].Heal (_shelter);
								break;
						case Survivor.task.Defend:
								r = _shelter._survivors [s].Defend (_shelter);
								break;
						case Survivor.task.Scavenge:
								r = _shelter._survivors [s].Scavenge (_shelter);
								break;
						case Survivor.task.Raiding:
								break;
						case Survivor.task.Unassigned:
								goto case Survivor.task.Resting;
						case Survivor.task.Resting:
								r = _shelter._survivors [s].Rest (_shelter);
								break;
						}
						_reports.Enqueue (r);
				}

				//Set each task to unassigned
				for (int s = 0; s < _shelter.NumberOfSurvivors; s++) {
						_shelter._survivors [s].Exhaust ();
						_shelter._survivors [s].AssignedTask = Survivor.task.Unassigned;
				}
		}


		// ================================================= update / GUI
		// Update is called once per frame
		void Update ()
		{
				if (_shelter == null) {
						_shelter = this.GetComponent<Shelter> ();
				}

				if (_conversationsLeft > 0) {
						_newDay = "New Day";
				} else {
						_newDay = "New Day*";
				}
		}

		// GUI
		void OnGUI ()
		{
				if (GUI.Button (new Rect (10, 45, 100, 30), _newDay)) {
						newDay ();
				}

				if (GUI.Button (new Rect (20, 10, 1000, 30), "Day Survived: " + _currentDay.ToString ())) {
				}
		
				int startX = 200;
				int startY = 200;
				int itY;

				int buttonWidth = 300;
				int buttonHeight = 30;

				int numSurvivors = _shelter.NumberOfSurvivors;
		
				for (int i = 0; i < numSurvivors; i++) {
						itY = startY;
						if (GUI.Button (new Rect (startX, itY, buttonWidth, buttonHeight), _shelter._survivors [i].Name)) {
						}
						itY += buttonHeight;
						if (GUI.Button (new Rect (startX, itY, buttonWidth, buttonHeight), "Converse: " + _shelter._survivors [i].ConversationsHad) && _conversationsLeft > 0) {
								_shelter._survivors [i].Converse ();
								_conversationsLeft--;
								if (!BrianTalked) {
										_DiagCon.StartConversation ("Conv_1");
										BrianTalked = true;
								}
						}

						itY += buttonHeight;
						for (int t = 0; t < (int) Survivor.task.Count; t++) {
								if (GUI.Button (new Rect (startX, itY, buttonWidth, buttonHeight), ((Survivor.task)t).ToString ())) {
										_shelter._survivors [i].AssignedTask = ((Survivor.task)t);
								}
								itY += buttonHeight;
						}
			
						if (GUI.Button (new Rect (startX, itY, buttonWidth, buttonHeight), "Assigned Task: " + _shelter._survivors [i].AssignedTask.ToString ())) {
						}
						itY += buttonHeight * 2;
						if (GUI.Button (new Rect (startX, itY, buttonWidth, buttonHeight), "Health: " + _shelter._survivors [i].Health)) {
						}
						itY += buttonHeight;

						if (GUI.Button (new Rect (startX, itY, buttonWidth, buttonHeight), "Fatigue: " + _shelter._survivors [i].Fatigue)) {
						}


						startX += buttonWidth;
				}
				startX += buttonWidth;
				itY = startY;
				if (GUI.Button (new Rect (startX, itY, buttonWidth, buttonHeight), "Food: " + _shelter.Food)) {
				}
				itY += buttonHeight;
				if (GUI.Button (new Rect (startX, itY, buttonWidth, buttonHeight), "Luxuries: " + _shelter.Luxuries)) {
				}
				itY += buttonHeight;
				if (GUI.Button (new Rect (startX, itY, buttonWidth, buttonHeight), "Medicine: " + _shelter.Medicine)) {
				}
				itY += buttonHeight;
				if (BrianTalked && !_DiagCon.IsConversationActive) {
						if (GUI.Button (new Rect (startX, itY, buttonWidth, buttonHeight), "Door: " + _shelter.Medicine)) {
								_DiagCon.StartConversation ("Conv_2");
						}
				}

		if (_DiagCon.IsConversationActive) {
			}
		}// end of OnGUI
}

