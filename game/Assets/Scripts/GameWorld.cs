/// <summary>
/// Game world. Tracks which areas are scavenge-able as well as your enemies strength.
/// </summary>
using UnityEngine;
using System.Collections;

public class GameWorld : MonoBehaviour {

	int _scoutingBonus;
	Shelter _shelter;

	ArrayList Enemies = new ArrayList();

	/// Enemy. Encapsulated class that manages threats to your camp
	/// in the form of rival camps
	public class Enemy{
		int _strength; //how much power they have
		int _visibility; //how easy they are to see
		int _aggressiveness; //how likely they are to attack you

		bool _located = false;

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

		public void inflictDamage(int damage){
			_strength -= damage;
		}

		public int Strength{
			get{
				return _strength;
			}
		}

		public int Visibility{
			get{
				return _visibility;
			}
		}

		public bool ShouldAttack(){
			int attackChance = Random.Range(0,10) + _aggressiveness;

			if(attackChance > 11){
				return true;
			}
			return false;
		}

		public bool IsUnscouted(){
			return _located == false;
		}

		public void MakeCampVisibile(){
			_located = true;
		}


		public void LoseStrength(){
			_strength -= Random.Range (1,4);
			if(_strength < 1){
				_strength = 1;
			}
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
		_shelter = this.GetComponent<Shelter>();
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

	public ArrayList ScoutForShelters(int proficiency){
		ArrayList reports = new ArrayList();
		foreach(Enemy e in Enemies){
			if(e.IsUnscouted()){
				if(e.Visibility < proficiency){
					//Let us see it
					e.MakeCampVisibile();

					Report r = new Report();
					r.SetMessage("Your scouts have found an enemy camp.");
					reports.Add(r);
				}
				else{
					if(Random.Range (0,10) + proficiency > Random.Range (0,5) + e.Visibility){
						//Let us see it
						e.MakeCampVisibile();
						
						Report r = new Report();
						r.SetMessage("Your scouts have found an enemy camp.");
						reports.Add(r);
					}
				}
			}
		}
		return reports;
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
		///Check for raiding camps
		int raidStrength = _shelter.RaidingStrength;

		ArrayList deadCamps = new ArrayList();

		if(raidStrength > 0){ //Attempt Raid
			//check for located camp, else scout for the camp
			foreach(Enemy camp in Enemies){
				if(camp.IsUnscouted() == false){
					//Attempt raid

					int playerDamage = _shelter.RaidingStrength + Random.Range (-2,2);
					if(playerDamage < camp.Strength){ //50% of losing 1 character
						if(Random.Range (0,10) < 5){
							Report repo = new Report();
							string name = _shelter.KillRandomSurvivor();
							repo.SetMessage(name+ " died in an attempted raid on an enemy.");
						}
					}
					else{
						camp.inflictDamage(playerDamage);

						if(camp.Strength < 0){
							Report raidReport = new Report();
							int newFood = Random.Range(0,20);
							int newMedicine = Random.Range(0,20);
							int newLuxuries = Random.Range(0,20);

							_shelter.Food += newFood;
							_shelter.Medicine += newMedicine;
							_shelter.Luxuries += newLuxuries;

							raidReport.SetMessage("Your raiders have destroyed a camp gaining you " + newFood + " food, " + newMedicine + " medicine and " + newLuxuries + " luxuries.");
							reports.Add(raidReport);
							deadCamps.Add(camp);
						}
					}
				}
			}
			
			foreach(Enemy deadCamp in deadCamps){
				Enemies.Remove(deadCamp);
			}

		}

		//Have the camps attack the player
		foreach(Enemy camp in Enemies){
			if(camp.ShouldAttack()){
				//If no one is home lose 50% of resources
				if(_shelter.DefensivePower == 0){
					Report rep = new Report();
					rep.SetMessage("Your camp was attacked, but no one was there, so they took some of your stores.");
					reports.Add(rep);
					_shelter.LoseHalfResources();
				}
				//else calculate your defense chances, calculate a chance to lose  a survivor and some resources
				else{
					if(_shelter.DefensivePower + Random.Range (-5,5) < camp.Strength){
						string deadSurvivor = _shelter.KillRandomSurvivor();
						_shelter.LoseHalfResources();
						Report rep = new Report();
						rep.SetMessage(deadSurvivor + " was killed in a raid on your camp. Some of your stores were taken.");
						reports.Add(rep);
					}
					else{
						Report rep = new Report();
						rep.SetMessage("Your camp was attacked, but you defended yourself.");
						reports.Add (rep);
						camp.LoseStrength();
					}
				}
				//if you triumph decrease the enemies resources instead
			}
		}

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
