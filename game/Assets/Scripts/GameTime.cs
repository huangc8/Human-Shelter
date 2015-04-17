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
		
	// =============================================== initialization
	// Use this for initialization
	void Start ()
	{
		_gameWorld = this.GetComponent<GameWorld> ();
		_shelter = this.GetComponent<Shelter> ();
		_rh = this.GetComponent<ReportHandler> ();
		_reports = new List<Report> ();
		_startConv = this.GetComponent<StartNewConversation> ();
		
		if (_visitors == null) {
			_visitors = this.GetComponent<Visitor> ();
		}
		
		// starting values
		_currentDay = 0;
	}

	// =============================================== action
	// Start a new day, complete all of the tasks the survivors were sent on
	public void newDay ()
	{
		if (_currentDay > 0) {
			//process the tasks
			evaluateTasks ();
			_shelter.NewDay ();
		}
		_currentDay++;
		_startConv.DayCheck ();
		GameEndCheck();
	}


	// ================================================ helper
	// Evaluates the tasks. Carry out the task for each survivor
	void evaluateTasks (){
		_reports = new List<Report>();
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
				_reports.Add (_shelter.Execute (_shelter._survivors[s]));
				break;
			case Survivor.task.Evict:
				_reports.Add (_shelter.Evict (_shelter._survivors[s]));
				break;
			case Survivor.task.Raiding:
				_reports.Add (_shelter._survivors [s].Raid (_shelter));
				break;
			case Survivor.task.Unassigned:
				goto case Survivor.task.Resting;
			case Survivor.task.Resting:


				Report rTmp = _shelter._survivors [s].Rest (_shelter);

				Debug.Log ("rTmp Message:" + rTmp.GetMessage());

				_reports.Add (rTmp);
				break;
			}

			// increase survivor status


			_shelter._survivors [s].Exhaust ();
			_reports.Add(_shelter._survivors [s].Eat (_shelter));
			_shelter._survivors [s].ConsumeMedicine (_shelter);
		}

		
		ArrayList tReports = _gameWorld.CalculateShelterLocation();
		
		foreach(Report r in tReports){
			_reports.Add(r);
		}

		// update report from gameworld
		ArrayList rt = _gameWorld.NewDay ();
		foreach (Report r in rt) {
			_reports.Add (r);
		}
		
		_reports.Add(_shelter.GetEatingReport());
		_reports.Add (_shelter.GetMedicineConsumptionReport());

#if debug
		//print the reports
		foreach (Report r in _reports){
			try{
				Debug.Log ("130 [reports]:" + r.GetMessage());
			}
			catch{
				Debug.LogError("134 reports are invalid");
			}
		}
#endif

		// pass report
		_rh.PassReports (_reports);

		// reset report
		//_reports = new List<Report> ();

		// player update
		_shelter.playerEat ();
	}

	// check if game win
	private bool GameWon(){
#if ThirtyDays
		//check for the time and check for the number of required parts (luxuries)
		return (_currentDay >= 30 && _shelter.HasSufficentParts());
#else
		return (_currentDay >= 7 && _shelter.Parts > 200);
#endif
	}

	// check if game lose
	private bool GameLost(){
		return (_shelter.IsGameOver());
	}

	// game end check
	public void GameEndCheck(){
		//Check for the end conditions
		if(GameWon()){
			Application.LoadLevel("WinGame");
		}
		else if(GameLost()){
			Application.LoadLevel ("LoseGame");
		}
	}
}

