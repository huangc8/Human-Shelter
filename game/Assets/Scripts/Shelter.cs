using UnityEngine;
using System.Collections;

public class Shelter : MonoBehaviour
{
	// ------------------------------ begin of Stores class ---------------------------------------
	public class Stores
	{
		// =============================================================== data
		private Shelter _parent;
		private int _parts;							// how much parts we have
		private int _food;							// how much food we have
		private int _medicine; 						// how much medicine we have

		// =============================================================== initialization
		// constructor
		public Stores (Shelter parent)
		{
			this._parent = parent;
			_parts = 0;
			_food = 50;
			_medicine = 50;
		}
		
		// ================================================================= accessor
		// get & set parts
		public int Parts {
			get {
				return _parts;
			}
			set {
				_parts = value;
				if(_parts < 0){
					_parts = 0;
				}
			}
		}

		// get & set foods
		public int Food {
			get {
				return _food;
			}
			set {
				_food = value;
				if(_food < 0){
					_food = 0;
				}
			}
		}

		// get & set medicine
		public int Medicine {
			get {
				return _medicine;
			}
			set {
				_medicine = value;
				if(_medicine < 0){
					_medicine = 0;
				}
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
			Food =  Food - 5;
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
	public GameWorld _gameWorld; 			// game world reference
	public GameTime _gametime;				// game time reference
	public Visitor _visitors;				// visitor reference
	public Conditions _cond;				// conditions reference
	public StartNewConversation _startConv;	// start new conversation reference

	public Survivor[] _survivors; 			// array of survivors, maximum capacity is 6
	public Survivor[] _evictedSurvivors; 	// array of survivors who have been kicked out of the shelter
	public GameObject[] _images; 			// array of corrisponding character images

	private Stores _storage; 				// the storage
	private int _numEvictedSurvivors; 		// current number of survivors who have been evicted
	private int _numSurvivors; 				// current number of survivors
	private int _attackStrength; 			// the raiding strength
	private int _defenses = 0;				// the defending system

	public DefenseLevel _defenseLevel;		// the defenseLevel
	public int _numPeople;					// the number of survivor you let in
	int foodEatenToday;
	int medicineConsumedToday;

	// =============================================================== initialization
	// Use this for initialization
	void Start ()
	{		
		_defenseLevel = DefenseLevel.Undefended;
		_survivors = new Survivor[100];
		_images = new GameObject[100];
		_evictedSurvivors = new Survivor[10];
		
		_storage = new Stores (this);
		_numSurvivors = 0;
		_numEvictedSurvivors = 0;
		_numPeople = 0;
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
			if(_defenses < 3){
				return DefenseLevel.Undefended;
			}
			else if(_defenses < 6){
				return DefenseLevel.BarelyDefended;
			}
			else if(_defenses < 10){
				return DefenseLevel.SlightlyDefended;
			}
			else if(_defenses < 20){
				return DefenseLevel.ModeratelyDefended;
			}
			else if(_defenses < 30){
				return DefenseLevel.HeavilyDefended;
			}
			else if(_defenses < 50){
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
			medicineConsumedToday += _storage.Medicine;
		}
		else{
			medicineConsumedToday += maxConsumption;
		}

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
		if(_storage.Food >= toEat){
			foodEatenToday += toEat ;
		}
		else{
			foodEatenToday += _storage.Food;
		}
		

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

	public bool CanRaidShelters ()
	{
		return _gameWorld.CanRaidShelters();
	}

	// increase defense
	public DefenseLevel BolsterDefenses (int proficiency)
	{
		_defenses += proficiency;
		if(_defenses < 3){
			_defenses = 3;
		}
		return DefensivePower;
	}
	
	// increase attack
	public int BolsterAttack (int proficiency)
	{
		_attackStrength += (_attackStrength + 10 );
		return _attackStrength;
	}

	// get medicine consumption report
	public Report GetMedicineConsumptionReport ()
	{
		Report r = new Report();
		
		r.SetMessage("Your shelter consumed " + medicineConsumedToday + " medicine.");
		
		medicineConsumedToday = 0;
		
		
		return r;
	}

	// get food consumption report
	public Report GetEatingReport(){
		Report r = new Report();
		if(foodEatenToday == 1){
			r.SetMessage("Your settlement has consumed " + foodEatenToday + " food item.");
		}
		else{
			r.SetMessage("Your settlement has consumed " + foodEatenToday + " food items.");
		}

		foodEatenToday = 0;
		return r;
	}

	// execute a survivor
	public Report Execute(Survivor s){
		
		int proficiency = s.GetProficiency(Survivor.task.Execute);
		
		Report r = new Report();
		if(proficiency + Random.Range (-5,5) > 5){
			r.SetMessage(s.Name + " resisted execution.");
			
		}
		else{
			r.SetMessage(s.Name + " was executed without incident.");
			
		}
		
		KillSurvivor(s.Name);
		return r;
	}

	// evict a survivor
	public Report Evict (Survivor s)
	{
		int proficiency = s.GetProficiency(Survivor.task.Evict);
		
		Report r = new Report();
		if(proficiency + Random.Range (-5,5) > 5){
			r.SetMessage(s.Name + " resisted eviction.");
		}
		else{
			r.SetMessage(s.Name + " was evicted without incident.");
		}
		
		EvictSurvivor(s);
		return r;
	}


	// ================================================== action on survivor
	// Invites a survivor.
	public void InviteSurvivor (Survivor visitorAtGate)
	{	
		// make the last empty spot to be the new survivor
		this._survivors [_numSurvivors] = visitorAtGate;

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
		_numPeople++;

		// update notify
		_startConv.NotifyCheck ();
	}

	// reject a survivor at gate
	public void RejectSurvivor(Survivor s){
		_evictedSurvivors [_numEvictedSurvivors] = CopySurvivor (s);
		_visitors._personList [_gametime._currentDay] = null;
		_numEvictedSurvivors++;

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
		int sPosition = -1;
		for (int i = 0; i < _numSurvivors; i++) {
			if (_survivors [i].Name == s.Name) {
				sPosition = i;
				break;
			}
		}
		Destroy (s.image);
		Destroy (s);
		_evictedSurvivors [_numEvictedSurvivors] = CopySurvivor (s);
		_survivors[sPosition] = null;
		_numEvictedSurvivors++;
		_numSurvivors--;
		sortSurvivor ();
	}

	// slightly wounded a raider
	public void SlightlyWoundRandomRaider (ArrayList reports)
	{
		int length = 0;
		foreach (Survivor s in _survivors) {
			if(s.AssignedTask == Survivor.task.Raiding){
				if(Random.Range(0,10) < 3){
					s.Health -= 2;
					reports.Add(s.Name +  " sustained a minor wound while raiding.");
				}
			}
		}
	}

	// wound a random raider.
	public void WoundRandomRaider(ArrayList reports){
		int length = 0;
		foreach (Survivor s in _survivors) {
			if(s.AssignedTask == Survivor.task.Raiding){
				Report r = new Report();
				Survivor.wound w = Survivor.wound.Uninjured;
                s.WoundCheck(this,r,0, "raiding","raid",ref w);
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
		Report r = new Report();
		/*r.SetMessage(s + " has been seriously wounded.");

		_gametime.addReport(r);
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
			_survivors[sPosition].Fatigue = 300;
			_survivors[sPosition].Health = 10;
		} else {
			Debug.LogError("KillSurvivor: No such Survivor");
		}
		*/
		r.SetMessage(s + " has died.");
		_gametime.addReport(r);
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
			Destroy (_survivors[sPosition].image);
			Destroy (_survivors[sPosition]);
			_survivors[sPosition] = null;
			_numSurvivors--;
			sortSurvivor();
		} else {
			Debug.LogError("KillSurvivor: No such Survivor");
		}
	}

	// fill in the missing survivor gap
	public void sortSurvivor(){
		for (int i = 0; i < _numSurvivors; i++) {
			if(_survivors[i] == null){
				int next = i+1;
				while(_survivors[next]==null && next < _numSurvivors){
					next++;
				}
				if(_survivors[next] == null){
					Debug.LogError("Shelter.cs -> sortSurvivor() -> NOT ENOUGH SURVIVOR.");
				}else{
					_survivors[i] = _survivors[next];
					_survivors[next] = null;
				}
			}
		}
	}

	/*
	void Update(){
		string tmp = "";
		tmp += _numSurvivors + " ";
		for (int i = 0; i < 6; i++) {
			if(_survivors[i] != null){
				tmp += _survivors[i].Name;
			}else{
				tmp += "NULL";
			}
			tmp += i;
		}
		Debug.Log (tmp);
	}
	*/
	// ================================================================ helper
	// Refreshes shelter for a new day, sets _defenses to 0
	public void NewDay ()
	{
		_defenses = 0;
		_attackStrength = 0;
	}
}
