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
		public int _currentDay; // current day
		List <Report> _reports; // the reports of assign task
		Visitor _visitors; //grab info about newcomers
		public Dialogue _dialogue; // the dialogue system
		public Conditions _cond; // the conditions data base
		public int _conversationsLeft; // converstation points left
		public static ReportHandler _rh;

		// =============================================== initialization
		// Use this for initialization
		void Start ()
		{
				_rh = this.GetComponent<ReportHandler> ();
				_conversationsLeft = 5;
				_currentDay = 0;
				_reports = new List<Report> ();
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
				_shelter.NewDay ();

		}


		// ================================================ helper
		/// <summary>
		/// Evaluates the tasks. Carry out the task for each survivor
		/// </summary>
		void evaluateTasks ()
		{
				//Evaluate each task
				for (int s = 0; s < _shelter.NumberOfSurvivors; s++) {
#if debuglog
			Debug.Log ("Evaluating task for survivor number " + s + " name:" + _shelter._survivors[s].Name);
#endif
						//carry out the task
						Report r = new Report ();
						_shelter._survivors [s].Exhaust ();
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
						case Survivor.task.Execute:
								r = _shelter._survivors [s].Execute (_shelter);
								break;
						case Survivor.task.Evict:
								r = _shelter._survivors [s].Evict (_shelter);
								break;
						case Survivor.task.Raiding:
								r = _shelter._survivors [s].Raid (_shelter);
								break;
						case Survivor.task.Unassigned:
								goto case Survivor.task.Resting;
						case Survivor.task.Resting:
								r = _shelter._survivors [s].Rest (_shelter);
								break;
						}
						_shelter._survivors [s].Eat (_shelter);
						r.Log ();
						_reports.Add (r);
				}
				_rh.PassReports (_reports);


		}

		// ================================================= update / GUI
		// Update is called once per frame
		void Update ()
		{
				if (_shelter == null) {
						_shelter = this.GetComponent<Shelter> ();

				}
				if (_visitors == null) {
						_visitors = this.GetComponent<Visitor> ();

				}

				if (_conversationsLeft > 0) {
						_newDay = "New Day";
				} else {
						_newDay = "New Day*";
				}
		}

		// GUI has moved to UI.cs
		/*void OnGUI ()
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
								_dialogue.startConv("Conv_1", false);
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
				if (BrianTalked) {
					if (GUI.Button (new Rect (startX, itY, buttonWidth, buttonHeight), "Door: " + _shelter.Medicine)) {
						_dialogue.startConv("Conv_2", true);
					}
				}

				//new survivor arrives
				startX += buttonWidth;
				itY = startY;
				Survivor visitorAtGate = _visitors._personList [_currentDay];
				if (visitorAtGate != null) {
						if (GUI.Button (new Rect (startX, itY, buttonWidth, buttonHeight), "There is someone at the gate!")) {
						}
						itY += buttonHeight;
						if (GUI.Button (new Rect (startX, itY, buttonWidth, buttonHeight), "Talk to " + visitorAtGate.Name.ToString ())) {
						}
						itY += buttonHeight;
						if (GUI.Button (new Rect (startX, itY, buttonWidth, buttonHeight), "Invite")) {
								_shelter._survivors [_shelter.NumberOfSurvivors] = visitorAtGate;
								_shelter.NumberOfSurvivors++;
								_visitors._personList [_currentDay] = null;
						}
						itY += buttonHeight;
						if (GUI.Button (new Rect (startX, itY, buttonWidth, buttonHeight), "Reject")) {
								_visitors._personList [_currentDay] = null;
						}
						itY += buttonHeight;
						if (GUI.Button (new Rect (startX, itY, buttonWidth, buttonHeight), "Kill")) {
								_visitors._personList [_currentDay] = null;

						}
						itY += buttonHeight;
				} else {
						if (GUI.Button (new Rect (startX, itY, buttonWidth, buttonHeight), "Nobody is at the gate")) {
						}
				}


		}*/

}

