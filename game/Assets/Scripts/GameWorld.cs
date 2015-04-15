/// <summary>
/// Game world. Tracks which areas are scavenge-able as well as your enemies strength.
/// </summary>
using UnityEngine;
using System.Collections;

public class GameWorld : MonoBehaviour
{

	// ------------------------------ begin of Enemy class ---------------------------------------
	/// Enemy. Encapsulated class that manages threats to your camp
	/// in the form of rival camps
	public class Enemy
	{
		int _strength; 			// how much power they have
		int _readiness; 		// how likely they are to attack you, counts up to five, resets after each attack
		int _visibility; 		// how easy they are to see
		int _aggressiveness; 	// how likely they are to attack you
		bool _located = false;	// whether is aware by player

		// =========================================================== constructor
		// Randomly generate an enemy
		public Enemy ()
		{
			_strength = Random.Range (0, 10);
			_visibility = Random.Range (0, 10);
			_aggressiveness = Random.Range (0, 10);

			_readiness = -5;
		}

		// Generate an enemy around a certain difficulty level
		public Enemy (int difficulty)
		{
			_strength = Random.Range (difficulty - 2, difficulty + 2);
			_visibility = Random.Range (difficulty - 2, difficulty + 2);
			_aggressiveness = Random.Range (difficulty - 2, difficulty + 2);

			_readiness = -5;
		}

		// ============================================================ accessor
		// get current strength
		public int Strength {
			get {
				return _strength;
			}
		}

		// get visibility
		public int Visibility {
			get {
				return _visibility;
			}
		}

		// whether aware by player
		public bool IsUnscouted ()
		{
			return _located == false;
		}

		// ============================================================ action
		// damage the enemy
		public void inflictDamage (int damage)
		{
			_strength -= damage;
		}

		// scouted by player
		public void MakeCampVisibile ()
		{
			_located = true;
		}

		// whether the enemy camp will attack you
		public bool ShouldAttack ()
		{
			int attackChance = Random.Range (0, 4) + _aggressiveness + _readiness;
			if (attackChance > 11) {
				_readiness = -5;
				return true;
			}
			_readiness += 2;
			return false;
		}

		// natural decrease of strength
		public void LoseStrength ()
		{
			_strength -= Random.Range (1, 4);
			if (_strength < 1) {
				_strength = 1;
			}
		}

		// ============================================================ helper
		// info player got from scouting
		public Report AttackLikeliness ()
		{
			Report rep = new Report ();
			if (_readiness > 5) {
				rep.SetMessage ("An enemy is prepared to attack you.");
			} else if (_readiness < 2) {
				rep.SetMessage ("An enemy will soon be ready to attack you.");
			} else {
				rep.SetMessage ("An enemy is not ready to attack you.");
			}
			return rep;
		}
	} 
	// ------------------------------ end of Enemy class ---------------------------------------

	// ========================================================= data
	
	private const int NUM_SCAVENGE_TARGETS = 3;
	public int [] scavengedTargets = new int[NUM_SCAVENGE_TARGETS]; //3 chosen - number of enums for scavenging

	Shelter _shelter;						// shelter class
	int _daysSinceSpawn = 0;				// 
	int _scoutingBonus;						// 	
	ArrayList Enemies = new ArrayList ();	// enemy list

	private ScavengeableLocation _scavengeTarget;
	private ScavengeQuality _scavengeQuality;


	// scavenge location enum
	public enum ScavengeableLocation
	{
		Hospital, //Gives food and medicine
		GroceryStore, //Gives food only
		Mall //Gives parts and food
	}

	// scavenge quality enum
	public enum ScavengeQuality
	{
		Plentiful,
		Good,
		Scarce
	}
	// =========================================================== initi
	// Use this for initialization
	void Start ()
	{
		scavengedTargets[(int)ScavengeableLocation.Hospital] = 0;
		scavengedTargets[(int)ScavengeableLocation.GroceryStore] = 0;
		scavengedTargets[(int)ScavengeableLocation.Mall] = 0;

		_shelter = this.GetComponent<Shelter> ();
	}
	
	// =========================================================== accessor
	// add scouting bonus
	public void AddScoutingBonus (int scoutingBonus)
	{
		_scoutingBonus += scoutingBonus;
	}
	
	public ScavengeableLocation ScavengeTarget {
		get {
			return _scavengeTarget;
		}
	}
	
	public ScavengeQuality ScavengeQualityLevel {
		get {
			return _scavengeQuality;
		}
	}

	// =========================================================== action
	/// <summary>
	/// Start a new Day, do all the raiding, been raid, and spawning new enemy camp
	/// </summary>
	public ArrayList NewDay ()
	{
		// create return report
		ArrayList reports = new ArrayList ();

		// add which structure we can scavenge
		selectScavengeTarget ();
		Report r = new Report ();
		r.SetMessage ("Today we can scavenge a " + _scavengeTarget.ToString () + " with a " + _scavengeQuality.ToString () + " number of resources.");
		reports.Add (r);

		// Check for raiding camps
		int raidStrength = _shelter.RaidingStrength;
		if (raidStrength > 0) { // other camp attempt to raid 

			ArrayList deadCamps = new ArrayList ();

			// check for located camp, else scout for the camp
			foreach (Enemy camp in Enemies) {

				// check if scouted
				if (camp.IsUnscouted () == false) {

					int playerDamage = _shelter.RaidingStrength + Random.Range (-2, 2);

					// player attempt to raid other camp with not enough strength
					if (playerDamage < camp.Strength) { //50% of wounding a character
						if (Random.Range (0, 10) < 5) {
							_shelter.WoundRandomRaider(reports);
						}
					} else { // player attempt to raid other camp with enough strength

						// damage the enemy camp
						camp.inflictDamage (playerDamage);

						// check if camp dead
						if (camp.Strength < 0) {
							Report raidReport = new Report ();
							int newFood = Random.Range (0, 20);
							int newMedicine = Random.Range (0, 20);
							int newParts = Random.Range (0, 20);
							
							_shelter.Food += newFood;
							_shelter.Medicine += newMedicine;
							_shelter.Parts += newParts;
							
							raidReport.SetMessage ("Your raiders have destroyed a camp gaining you " + newFood + " food, " + newMedicine + " medicine and " + newParts + " parts.");
							reports.Add (raidReport);
							deadCamps.Add (camp);
						}
					}
				}
			}

			// remove dead camps
			foreach (Enemy deadCamp in deadCamps) {
				Enemies.Remove (deadCamp);
			}
			
		}
		
		//Have the camps attack the player
		foreach (Enemy camp in Enemies) {
			if (camp.ShouldAttack ()) {
				//If no one is home lose 50% of parts
				if (_shelter.DefensivePower == 0) {
					Report rep = new Report ();
					rep.SetMessage ("Your camp was attacked, but no one was there, so they took some of your stores.");
					reports.Add (rep);
					_shelter.LoseHalfResources ();
				}
				//else calculate your defense chances, calculate a chance to lose  a survivor and some parts
				else {
					if (_shelter.DefensivePower + Random.Range (-5, 5) < camp.Strength) {
						string deadSurvivor = _shelter.KillRandomSurvivor ();
						_shelter.LoseHalfResources ();
						Report rep = new Report ();
						rep.SetMessage (deadSurvivor + " was killed in a raid on your camp. Some of your stores were taken.");
						reports.Add (rep);
					} else {
						Report rep = new Report ();
						rep.SetMessage ("Your camp was attacked, but you defended yourself.");
						reports.Add (rep);
						camp.LoseStrength ();
					}
				}
				//if you triumph decrease the enemies parts instead
			}
		}
		
		//Spawn an enemy camp
		if (Random.Range (0, 10) * Enemies.Count < 2 + _daysSinceSpawn) {
			_daysSinceSpawn = 0;
			AddEnemy ();
			Report eReport = new Report ();
			eReport.SetMessage ("There's been rumors of a new camp in the area");
			reports.Add (eReport);
		} else {
			_daysSinceSpawn++;
		}

		//print out the contents of reports
		Debug.Log("Printing Reports in GameWorld.NewDay():");


		foreach(Report itRep in reports){
			Debug.Log("itRep:" + itRep.GetMessage());
		}
		
		return reports;
	}

	// =========================================================== helper
	/// <summary>
	/// Adds an enemy.
	/// </summary>
	void AddEnemy ()
	{
		Enemy e = new Enemy ();
		Enemies.Add (e);
	}

	/// <summary>
	/// Gets a random enum.
	/// </summary>
	/// <returns>The random enum.</returns>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	static T GetRandomEnum<T> ()
	{
		System.Array A = System.Enum.GetValues (typeof(T));
		T V = (T)A.GetValue (UnityEngine.Random.Range (0, A.Length));
		return V;
	}
	
	/// <summary>
	/// Selects the scavenge target.
	/// Weight randomness so that it is more likely to pick a place we have not seen in a while
	/// Keep track of three int values for the scavenging target, stored in one int array
	/// </summary>
	private void selectScavengeTarget ()
	{
		_scavengeQuality = GetRandomEnum<ScavengeQuality>();
		int scav = Random.Range (0, 10);
		scav += _scoutingBonus;

		int [] tmpScavengedTargets = new int[NUM_SCAVENGE_TARGETS];

		for(int i = 0 ; i < NUM_SCAVENGE_TARGETS; i++){
			tmpScavengedTargets[i] += Random.Range (0,10);
		}

		//get the value for each scavenged target and randomly offset it. Take the value with the 
		//least points


		if (scav > 9) {
			_scavengeQuality = ScavengeQuality.Plentiful;
		} else if (scav > 6) {
			_scavengeQuality = ScavengeQuality.Good;
		} else {
			_scavengeQuality = ScavengeQuality.Scarce;
		}

		_scavengeTarget = ScavengeableLocation.GroceryStore;

		for(int i = 0; i < NUM_SCAVENGE_TARGETS; i++){
			if(scavengedTargets[i] <= scavengedTargets[(int)_scavengeTarget]){
				_scavengeTarget = (ScavengeableLocation)i;
			}
		}




		scavengedTargets[(int)_scavengeTarget] += (int) _scavengeQuality;

		_scoutingBonus = 0;
	}

	/// <summary>
	/// Scouts for shelters.
	/// </summary>
	/// <returns>The for shelters.</returns>
	/// <param name="proficiency">Proficiency.</param>
	public ArrayList ScoutForShelters (int proficiency)
	{
		ArrayList reports = new ArrayList ();
		foreach (Enemy e in Enemies) {
			if (e.IsUnscouted ()) {
				if (e.Visibility < proficiency) {
					//Let us see it
					e.MakeCampVisibile ();

					Report r = new Report ();
					r.SetMessage ("Your scouts have found an enemy camp.");
					reports.Add (r);
				} else {
					if (Random.Range (0, 10) + proficiency > Random.Range (0, 5) + e.Visibility) {
						//Let us see it
						e.MakeCampVisibile ();
						
						Report r = new Report ();
						r.SetMessage ("Your scouts have found an enemy camp.");
						reports.Add (r);
					}
				}
			} else {
				reports.Add (e.AttackLikeliness ());
			}
		}
		return reports;
	}	
}
