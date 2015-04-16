using UnityEngine;
using System.Collections;

public class Shelter : MonoBehaviour
{
	// ------------------------------ begin of Stores class ---------------------------------------
	public class Stores
	{
		// =============================================================== data
		private Shelter _parent;
		int _parts;							// how much parts we have
		int _food;							// how much food we have
		int _medicine; 						// how much medicine we have

		// =============================================================== initialization
		// constructor
		public Stores (Shelter parent)
		{
			this._parent = parent;
			_parts = 0;
			_food = 50;
			_medicine = 0;
		}
		
		// ================================================================= accessor
		// get & set parts
		public int Parts {
			get {
				return _parts;
			}
			set {
				_parts = value;
			}
		}

		// get & set foods
		public int Food {
			get {
				return _food;
			}
			set {
				_food = value;
			}
		}

		// get & set medicine
		public int Medicine {
			get {
				return _medicine;
			}
			set {
				_medicine = value;
			}
		}

		// is parts enough?
		public bool SufficientParts(){
			return _parts > 200;
		}

		// ================================================================= function
		// player consume food
		public void playerEat ()
		{
			_food -= 5;
		}

		// consume medicine
		public void UseMedicine (int useAmount)
		{
			_medicine -= useAmount;
		}

		// lose resource due to raid
		public void LoseHalfResources ()
		{
			_medicine = (int)(_medicine * Random.Range (.4f, .6f));
			_food = (int)(_food * Random.Range (.4f, .6f));
			_parts = (int)(_parts * Random.Range (.4f, .6f));
		}
	}
	// ------------------------------ end of Stores class ---------------------------------------

	public enum DefenseLevel{
		Undefended,
		BarelyDefended,
		SlightlyDefended,
		ModeratelyDefended,
		HeavilyDefended,
		WellDefended,
		InpenetrableFortress
	}

	// =============================================================== data
	public DefenseLevel _defenseLevel;
	public GameWorld _gameWorld; 			// game world reference
	public GameTime _gametime;				// game time reference
	public Visitor _visitors;				// visitor reference
	public Conditions _cond;				// conditions reference

	public Survivor[] _survivors; 			// array of survivors, maximum capacity is 6
	public Survivor[] _evictedSurvivors; 	// array of survivors who have been kicked out of the shelter
	public GameObject[] _images; 			// array of corrisponding character images

	private Stores _storage; 				// the storage
	private int _numEvictedSurvivors; 		// current number of survivors who have been evicted
	private int _numSurvivors; 				// current number of survivors
	private int _attackStrength; 			// the raiding strength
	private int _defenses = 0;

	// =============================================================== initialization
	// Use this for initialization
	void Start ()
	{		
		_defenseLevel = DefenseLevel.Undefended;
		_survivors = new Survivor[6];
		_images = new GameObject[6];
		_evictedSurvivors = new Survivor[100];
		
		_storage = new Stores (this);
		_numSurvivors = 0;
		_numEvictedSurvivors = 0;
	}

	//================================================== accessor
	// get & set medicine
	public int Medicine {
		get {
			return _storage.Medicine;
		}
		set {
			_storage.Medicine = value;
		}
	}
	
	// get & set food
	public int Food {
		get {
			return _storage.Food;
		}
		set {
			_storage.Food = value;
		}
	}
	
	// get & set parts
	public int Parts {
		get {
			return _storage.Parts;
		}
		set {
			_storage.Parts = value;
		}
	}

	// whether there is enough parts
	public bool HasSufficentParts(){
		if(_storage.SufficientParts()){
			return true;
		}
		else{
			return false;
		}
	}

	// get & set number of survivors
	public int NumberOfSurvivors {
		get {
			return _numSurvivors;
		}
		set {
			_numSurvivors = value;
		}
	}
	
	// get raiding strength
	public int RaidingStrength {
		get {
			return _attackStrength;
		}
	}
	
	// get defense strength
	public DefenseLevel DefensivePower{
		get {
			if(_defenses < 5){
				return DefenseLevel.Undefended;
			}
			else if(_defenses < 15){
				return DefenseLevel.BarelyDefended;
			}
			else if(_defenses < 35){
				return DefenseLevel.SlightlyDefended;
			}
			else if(_defenses < 50){
				return DefenseLevel.ModeratelyDefended;
			}
			else if(_defenses < 70){
				return DefenseLevel.HeavilyDefended;
			}
			else if(_defenses < 90){
				return DefenseLevel.WellDefended;
			}
			else{
				return DefenseLevel.InpenetrableFortress;
			}
		}
	}

	// check for game over (cond: out of food & out of survivors
	public bool IsGameOver(){
		return _numSurvivors <= 0 && _storage.Food <= 0;
	}

	//================================================== Modifier on shelter
	// consume food
	public bool ConsumeFood(int maxConsumption){
		if(maxConsumption > _storage.Food){
			_storage.Food = 0;
			return false;
		}
		_storage.Food -= maxConsumption;
		return true;
	}

	// consume parts
	public bool ConsumeParts(int maxConsumption){
		if(maxConsumption > _storage.Parts){
			_storage.Medicine = 0;
			return false;
		}
		_storage.Medicine -= maxConsumption;
		return true;
	}

	// consume medicine
	public bool ConsumeMedicine(int maxConsumption){
		if(maxConsumption > _storage.Medicine){
			_storage.Medicine = 0;
			return false;
		}
		_storage.Parts -= maxConsumption;
		return true;
	}

	// consume food
	public bool EatFood (int toEat)
	{
		_storage.Food = _storage.Food - toEat;
		if (_storage.Food < 0) {
			_storage.Food = 0;
			return false;
		}
		return true;
	}

	// player consume food
	public void playerEat ()
	{
		_storage.playerEat ();
	}

	// consume medicine
	public void UseMedicine (int useAmount)
	{
		_storage.UseMedicine (useAmount);
	}

	// lose half resource
	public void LoseHalfResources ()
	{
		_storage.LoseHalfResources ();
	}

	// increase defense
	public int BolsterDefenses (int proficiency)
	{
		_defenses += proficiency;
		return _defenses;
	}
	
	// increase attack
	public int BolsterAttack (int proficiency)
	{
		_attackStrength += proficiency;
		return _attackStrength;
	}

	// ================================================== action on survivor
	// Invites a survivor.
	public void InviteSurvivor (Survivor visitorAtGate)
	{	
		// make the last empty spot to be the new survivor
		if (visitorAtGate.Name == "Brian") {
			this._survivors [0] = visitorAtGate;	
		} else {
			this._survivors [_gametime._currentDay] = visitorAtGate;
		}

		//show on map
		visitorAtGate.image.renderer.enabled = true;
		visitorAtGate.image.layer = 0;
	
		// set condition
		_cond.setCondition ("inCamp", visitorAtGate.Name, true);

		// clear gate
		if (visitorAtGate.Name != "Brian") {
			_visitors._personList [_gametime._currentDay] = null;
		}

		// increase number of survivor
		_numSurvivors++;
	}

	// copy the survivor
	private Survivor CopySurvivor (Survivor toBeCopied)
	{
		Survivor stmp = new Survivor ();
		stmp.CopyInit (toBeCopied);
		return stmp;
	}

	// Evicts a survivor.
	public void EvictSurvivor (Survivor s)
	{
		_evictedSurvivors [_numEvictedSurvivors] = CopySurvivor (s);
		Destroy (s.image); 
		Destroy (s);
		_numEvictedSurvivors++;
		_numSurvivors--;
	}

	// wound a random raider.
	public void WoundRandomRaider(ArrayList reports){
		int length = 0;
		foreach (Survivor s in _survivors) {
			if(s.AssignedTask == Survivor.task.Raiding){
				Report r = new Report();
                s.WoundCheck(this,r,0, "raiding","raid");
                reports.Add(r);
			}
		}
	}

	// Kills a random survivor.
	public string KillRandomSurvivor ()
	{
		name = _survivors [(int)Random.Range (0, _numSurvivors)].Name;
		KillSurvivor (name);
		return name;
	}

	// kill survivor
	public void KillSurvivor (string s)
	{
		// find the survivors position
		int sPosition = -1;
		for (int i = 0; i < _numSurvivors; i++) {
			if (_survivors [i].Name == s) {
				sPosition = i;
				break;
			}
		}

		// kill him/her
		if (sPosition != -1) {
			Destroy (_survivors [sPosition].image);
			Destroy (_survivors [sPosition]);
			_numSurvivors--;
		} else {
			Debug.LogError("KillSurvivor: No such Survivor");
		}
	}

	// ================================================================ helper
	// Refreshes shelter for a new day, sets _defenses to 0
	public void NewDay ()
	{
		_defenses = 0;
		_attackStrength = 0;
	}
}
