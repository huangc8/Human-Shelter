/// <summary>
/// Shelter. This class controls the properties of the shelter including
/// the survivors and they're corresponding tasks
/// </summary>
using UnityEngine;
using System.Collections;

public class Shelter : MonoBehaviour
{
	public Survivor[] _survivors; //array of survivors, maximum capacity is 6
	private int _numSurvivors; // current number of survivors
	private Stores _storage; // the storage

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
		int _defenses; // the defense of the building (depend on guard

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
			_food = 0;
			_medicine = 0;
			_defenses = 0;
		}

		// ================================================================ helper
		/// <summary>
		/// Refreshes shelter for a new day, sets _defenses to 0
		/// </summary>
		public void NewDay(){
			_defenses = 0;
		}

		/// <summary>
		/// Bolsters the defenses.
		/// </summary>
		/// <param name="proficiency">Proficiency.</param>
		public int IncreaseDefenses(int proficiency){
			_defenses += proficiency;
			return _defenses;
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

	// ================================================== action
	/// <summary>
	/// Kills the survivor.
	/// </summary>
	public void KillSurvivor (Survivor s)
	{
		_numSurvivors--;
		Destroy (s);
	}

	// ================================================== initialization
	/// <summary>
	/// The default starting configuration for the shelter
	/// </summary>

	private void DefaultSetup ()
	{
		//create basic survivor
		_survivors [0] = CreateSurvivor("Brian");
	}

	// Use this for initialization
	void Start ()
	{
		_survivors = new Survivor[6];
		_storage = new Stores (this);
		_numSurvivors = 0;
		DefaultSetup ();
	}

	// =================================================== update
	// Update is called once per frame
	void Update ()
	{

	}
}
