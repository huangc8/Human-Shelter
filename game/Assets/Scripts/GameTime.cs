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

	private Shelter _shelter; 					// the shelter data
	private Visitor _visitors; 					// grab info about newcomers
	private GameWorld _gameWorld;				// game world class 
	private StartNewConversation _startConv; 	// startConversation class

	public Dialogue _dialogue; 					// the dialogue system
	public Conditions _cond; 					// the conditions data base
	public static ReportHandler _rh;			// report handler
		
	List <Report> _reports; 					// the reports of assign task
		
	public int _currentDay; 					// current day
	int _conversationsLeft; 					// converstation points left
		
	// =============================================== initialization
	// Use this for initialization
	void Start ()
	{
		_gameWorld = this.GetComponent<GameWorld> ();
		_shelter = this.GetComponent<Shelter> ();
		_rh = this.GetComponent<ReportHandler> ();
		_reports = new List<Report> ();
		_startConv = this.GetComponent<StartNewConversation> ();

		// starting values
		_conversationsLeft = 5;
		_currentDay = 0;
	}

	// =============================================== action
	/// <summary>
	/// Start a new day, reset all of values that reset over night
	/// Complete all of the tasks the survivors were sent on
	/// </summary>
	public void newDay ()
	{
		if (_currentDay > 1) {
			//process the tasks
			evaluateTasks ();

			// reset values + update values
			_conversationsLeft = 5;
			for (int i = 0; i < _shelter.NumberOfSurvivors; i++) {
				_shelter._survivors [i].ConvReset ();
			}
			_shelter.NewDay ();
		}
		_currentDay++;
		_startConv.DayCheck ();
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
			switch (_shelter._survivors [s].AssignedTask) {
			case Survivor.task.Scout:
				ArrayList rtmp = (_shelter._survivors [s].Scout (_shelter));
				if (rtmp != null) {
					foreach (Report r in rtmp) {
						_reports.Add (r);
					}
				}
				break;
			case Survivor.task.Heal:
				_reports.Add (_shelter._survivors [s].Heal (_shelter));
				break;
			case Survivor.task.Defend:
				_reports.Add (_shelter._survivors [s].Defend (_shelter));
				break;
			case Survivor.task.Scavenge:
				_reports.Add (_shelter._survivors [s].Scavenge (_shelter));
				break;
			case Survivor.task.Execute:
				_reports.Add (_shelter._survivors [s].Execute (_shelter));
				break;
			case Survivor.task.Evict:
				_reports.Add (_shelter._survivors [s].Evict (_shelter));
				break;
			case Survivor.task.Raiding:
				_reports.Add (_shelter._survivors [s].Raid (_shelter));
				break;
			case Survivor.task.Unassigned:
				goto case Survivor.task.Resting;
			case Survivor.task.Resting:
				_reports.Add (_shelter._survivors [s].Rest (_shelter));
				break;
			}

			// increase survivor status
			_shelter._survivors [s].Exhaust ();
			_shelter._survivors [s].Eat (_shelter);
			_shelter._survivors [s].ConsumeMedicine (_shelter);
		}

		// update report from gameworld
		ArrayList rt = _gameWorld.NewDay ();
		foreach (Report r in rt) {
			_reports.Add (r);
		}

		// pass report
		_rh.PassReports (_reports);

		// reset report
		_reports = new List<Report> ();

		// player update
		_shelter.playerEat ();
	}

	private bool GameWon(){
		//check for the time and check for the number of required parts (luxuries)
		return (_currentDay >= 30 && _shelter.HasSufficentParts());

	}

	private bool GameLost(){
		return (_shelter.IsGameOver());
	}

	// ================================================= update / GUI
	// Update is called once per frame
	void Update ()
	{
		if (_shelter == null) {
			//_shelter = this.GetComponent<Shelter> ();
		}
		if (_visitors == null) {
			_visitors = this.GetComponent<Visitor> ();
		}

		//Check for the end conditions
		if(GameWon()){
			Application.LoadLevel("WinGame");
		}
		else if(GameLost()){
			Application.LoadLevel ("LoseGame");
		}

	}
}

