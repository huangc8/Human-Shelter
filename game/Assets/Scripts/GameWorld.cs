/// <summary>
/// Game world. Tracks which areas are scavenge-able as well as your enemies strength.
/// </summary>
using UnityEngine;
using System.Collections;

public class GameWorld : MonoBehaviour {

	int _scoutingBonus;

	ArrayList Enemies = new ArrayList();

	/// Enemy. Encapsulated class that manages threats to your camp
	/// in the form of rival camps
	public class Enemy{
		int _strength; //how much power they have
		int _visibility; //how easy they are to see
		int _aggressiveness; //how likely they are to attack you

		//Randomly generate an enemy
		public Enemy(){
			_strength = Random.Range(0,10);
			_visibility = Random.Range (0,10);
			_aggressiveness = Random.Range (0,10);
		}

		//Generate an enemy around a certain difficulty level
		public Enemy(int difficulty){
			_strength = Random.Range (difficulty-2,difficulty+2);
			_visibility = Random.Range (difficulty-2,difficulty+2);
			_aggressiveness = Random.Range (difficulty-2,difficulty+2);
		}
	}


	public enum ScavengeableLocation{
		Hospital, //Gives food and medicine
		GroceryStore, //Gives food only
		Mall //Gives luxuries and foodS
	}

	public enum ScavengeQuality{
		Plentiful,
		Good,
		Scarce
	}

	private ScavengeableLocation _scavengeTarget;
	private ScavengeQuality _scavengeQuality;


	public void AddScoutingBonus(int scoutingBonus){
		_scoutingBonus += scoutingBonus;
	}

	public ScavengeableLocation ScavengeTarget{
		get{
			return _scavengeTarget;
		}
	}

	public ScavengeQuality ScavengeQualityLevel{
		get{
			return _scavengeQuality;
		}
	}

	static T GetRandomEnum<T>()
	{
		System.Array A = System.Enum.GetValues(typeof(T));
		T V = (T)A.GetValue(UnityEngine.Random.Range (0,A.Length));
		return V;
	}

	// Use this for initialization
	void Start () {

	}

	void AddEnemy(){
		Enemy e = new Enemy();
		Enemies.Add(e);
	}

	// Update is called once per frame
	void Update () {
	
	}





	private void selectScavengeTarget(){
		_scavengeTarget = GetRandomEnum<ScavengeableLocation>();
		//_scavengeQuality = GetRandomEnum<ScavengeQuality>();
		int scav = Random.Range (0,10);
		scav += _scoutingBonus;
		if(scav > 9){
			_scavengeQuality = ScavengeQuality.Plentiful ;
		}
		else if(scav > 6){
			_scavengeQuality = ScavengeQuality.Good ;
		}
		else{
			_scavengeQuality = ScavengeQuality.Scarce;
		}

		_scoutingBonus = 0;
	}

	/// <summary>
	/// Start a new Day
	/// </summary>
	public ArrayList NewDay(){
		//change which structure we can scavenge
		selectScavengeTarget();
		Report r = new Report();
		r.SetMessage("Today we can raid a " + _scavengeTarget.ToString() + " with a " + _scavengeQuality.ToString() +  " number of resources.");

		ArrayList reports = new ArrayList();
		reports.Add(r);

		//Spawn an enemy camp
		if(Random.Range(0,10) < 2){
			AddEnemy();
			Report eReport = new Report();
			eReport.SetMessage("There's been rumors of a new camp in the area");
			reports.Add(eReport);
		}

		return reports;
	}
}
