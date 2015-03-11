/// <summary>
/// Shelter. This class controls the properties of the shelter including
/// the survivors and they're corresponding tasks
/// </summary>
using UnityEngine;
using System.Collections;

public class Shelter : MonoBehaviour
{
	public Survivor[] _survivors; //array of survivors, maximum capacity is 6
	public Survivor[] _evictedSurvivors; //Array of survivors who have been kicked out of the shelter

	private int _numEvictedSurvivors; //current number of survivors who have been evicted
	private int _numSurvivors; // current number of survivors
	private Stores _storage; // the storage
	int _defenses; // the defense of the building (depend on guard
	int _attackStrength; 
	public class Stores
	{
		// =============================================================== data
		/// <summary>
		/// Stores class, nested class that keeps track of all of the items we currently
		/// have in the shelter
		/// </summary>
		private Shelter _parent;
		int _luxuries;//How much medicine we have
		int _food;//how much food we have
		int _medicine; //how much medicine we have

		// =============================================================== initialization
		/// <summary>
		/// Initializes a new instance of the <see cref="Shelter+stores"/> class.
		/// Sets each store to be zero, sets the parent class too.
		/// </summary>
		/// <param name="parent">Parent.</param>
		public Stores (Shelter parent)
		{
			this._parent = parent;
			_luxuries = 0;
			_food = 50;
			_medicine = 0;
		}

		public void UseMedicine(int useAmount){
			_medicine -= useAmount;
		}


		// ================================================================= accessor
		/// <summary>
		/// Gets or sets the luxuries.
		/// </summary>
		/// <value>The luxuries.</value>
		public int Luxuries {
			get {
				return _luxuries;
			}
			set {
				_luxuries = value;
			}
		}

		/// <summary>
		/// Gets or sets the food.
		/// </summary>
		/// <value>The food.</value>
		public int Food {
			get {
				return _food;
			}
			set {
				_food = value;
			}
		}

		/// <summary>
		/// Gets or sets the medicine.
		/// </summary>
		/// <value>The medicine.</value>
		public int Medicine {
			get {
				return _medicine;
			}
			set {
				_medicine = value;
			}
		}
	}

	
	/// <summary>
	/// Bolsters the defenses.
	/// </summary>
	/// <param name="proficiency">Proficiency.</param>
	public int BolsterDefenses(int proficiency){
		_defenses += proficiency;
		return _defenses;
    }

	public int BolsterAttack(int proficiency){
		_attackStrength += proficiency;
		return _attackStrength;
	}

	public bool EatFood(int toEat){
		_storage.Food = _storage.Food - toEat;
		if(_storage.Food < 0){
			return false;
		}
		return true;
	}

    
    // ================================================================ helper
	/// <summary>
	/// Refreshes shelter for a new day, sets _defenses to 0
	/// </summary>
	public void NewDay(){
		_defenses = 0;
		_attackStrength = 0;
    }

	//================================================== Modifier
	public void UseMedicine(int useAmount){
		_storage.UseMedicine(useAmount);
	}

    //================================================== accessor
	/// <summary>
	/// Gets or sets the medicine.
	/// </summary>
	/// <value>The medicine.</value>
	public int Medicine {
		get {
			return _storage.Medicine;
		}
		set {
			_storage.Medicine = value;
		}
	}
	/// <summary>
	/// Gets or sets the food.
	/// </summary>
	/// <value>The food.</value>
	public int Food {
		get {
			return _storage.Food;
		}
		set {
			_storage.Food = value;
		}
	}
	/// <summary>
	/// Gets or sets the luxuries.
	/// </summary>
	/// <value>The luxuries.</value>
	public int Luxuries {
		get {
			return _storage.Luxuries;
		}
		set {
			_storage.Luxuries = value;
		}
	}
	/// <summary>
	/// Gets or sets the number of survivors.
	/// </summary>
	/// <value>The number of survivors.</value>
	public int NumberOfSurvivors {
		get {
			return _numSurvivors;
		}
		set {
			_numSurvivors = value;
		}
	}

	// ================================================== create

	private Survivor CreateSurvivor(string name){
		Survivor stmp = new Survivor ();
		stmp.Init ();
		stmp.Name = name;
		_numSurvivors++;
		return stmp;
	}

	/// <summary>
	/// Copies the survivor.
	/// </summary>
	/// <returns>The survivor.</returns>
	/// <param name="toBeCopied">To be copied.</param>
	private Survivor CopySurvivor(Survivor toBeCopied){
		Survivor stmp = new Survivor();

		stmp.CopyInit(toBeCopied);
		return stmp;
	}

	// ================================================== action
	public void EvictSurvivor(Survivor s){
		_evictedSurvivors[_numEvictedSurvivors] = CopySurvivor(s);
		_numEvictedSurvivors++;
		Destroy (s);
	}

	/// <summary>
	/// Kills the survivor.
	/// </summary>
	public void KillSurvivor (Survivor s)
	{
		//Find the survivors position
		int sPosition = -1;
		for(int i = 0; i < _numSurvivors; i++){
			if(_survivors[i].Name == s.Name){
				sPosition = i;
			}
		}
		Debug.Log ("Killing Survivor: " + s.Name + " sPosition: " + sPosition);
		//Swap him with the survivor at the end of the list
		Debug.Log (_survivors[sPosition].Name);
		_survivors[sPosition] = CopySurvivor(_survivors[_numSurvivors-1]);
		Debug.Log (_survivors[sPosition].Name);
		_numSurvivors--;
		Debug.Log (_numSurvivors);
	}

	// ================================================== initialization
	/// <summary>
	/// The default starting configuration for the shelter
	/// </summary>

	private void DefaultSetup ()
	{
		//create basic survivor
		_survivors [0] = CreateSurvivor("Brian");
		//_survivors [1] = CreateSurvivor("Marina");
		//_survivors [2] = CreateSurvivor("Jimbob");
		//_survivors [3] = CreateSurvivor("Jones");

		for(int s = 0; s < _numSurvivors; s++){
			Debug.Log(_survivors[s].AssignedTask.ToString());
		}

	}

	// Use this for initialization
	void Start ()
	{
		
		_defenses = 0;
		_survivors = new Survivor[6];
		_evictedSurvivors = new Survivor[100];


		_storage = new Stores (this);
		_numSurvivors = 0;
		_numEvictedSurvivors = 0;
		DefaultSetup ();
	}

	// =================================================== update
	// Update is called once per frame
	void Update ()
	{

	}
}
