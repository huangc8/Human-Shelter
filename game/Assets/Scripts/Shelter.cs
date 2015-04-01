/// <summary>
/// Shelter. This class controls the properties of the shelter including
/// the survivors and they're corresponding tasks
/// </summary>
using UnityEngine;
using System.Collections;

public class Shelter : MonoBehaviour
{
		public Survivor[] _survivors; //array of survivors, maximum capacity is 6
		public GameObject[] _images; //array of corrisponding images
		public Survivor[] _evictedSurvivors; //Array of survivors who have been kicked out of the shelter

	public GameWorld _gameWorld;

	public Visitor _visitors;
	public GameTime _gametime;

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

		public void playerEat(){
			_food -=5;
		}

				public void UseMedicine (int useAmount)
				{
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

		public void LoseHalfResources(){
			_medicine =(int)(_medicine * Random.Range (.4f,.6f));
			_food =(int)(_food * Random.Range (.4f,.6f));
			_luxuries =(int)(_luxuries * Random.Range (.4f,.6f));
		}
		}

	
		/// <summary>
		/// Bolsters the defenses.
		/// </summary>
		/// <param name="proficiency">Proficiency.</param>
		public int BolsterDefenses (int proficiency)
		{
				_defenses += proficiency;
				return _defenses;
		}

		public int BolsterAttack (int proficiency)
		{
				_attackStrength += proficiency;
				return _attackStrength;
		}

		public bool EatFood (int toEat)
		{
				_storage.Food = _storage.Food - toEat;
				if (_storage.Food < 0) {
						_storage.Food = 0;
						return false;
				}
				return true;
		}

    
		// ================================================================ helper
		/// <summary>
		/// Refreshes shelter for a new day, sets _defenses to 0
		/// </summary>
		public void NewDay ()
		{
				_defenses = 0;
				_attackStrength = 0;
		}

		//================================================== Modifier
	public void InviteSurvivor(Survivor visitorAtGate){
		this._survivors [this.NumberOfSurvivors] = visitorAtGate;
		//show on map
		visitorAtGate.image.renderer.enabled = true;
		visitorAtGate.image.layer = 0;
		
		this.NumberOfSurvivors++;
		_visitors._personList [_gametime._currentDay] = null;
	}

		public void UseMedicine (int useAmount)
		{
				_storage.UseMedicine (useAmount);
		}

		//================================================== accessor
	public int RaidingStrength{
		get{
			return _attackStrength;
		}
	}


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

	public int DefensivePower{
		get{
			return _defenses;
		}

	}

		// ================================================== create

		private Survivor CreateSurvivor (string name, GameObject image)
		{
				Survivor stmp = new Survivor ();
				stmp.Init (_gameWorld);
				stmp.Name = name;
				stmp.image = image;
				//show on map
				stmp.image.renderer.enabled = true;
				stmp.image.layer = 0;

				_numSurvivors++;
				return stmp;
		}

		/// <summary>
		/// Copies the survivor.
		/// </summary>
		/// <returns>The survivor.</returns>
		/// <param name="toBeCopied">To be copied.</param>
		private Survivor CopySurvivor (Survivor toBeCopied)
		{
				Survivor stmp = new Survivor ();

				stmp.CopyInit (toBeCopied);
				return stmp;
		}

		// ================================================== action
		public void EvictSurvivor (Survivor s)
		{
				_evictedSurvivors [_numEvictedSurvivors] = CopySurvivor (s);
				_numEvictedSurvivors++;
				Destroy (s.image); //destroy the sprite
				Destroy (s); //destory the script
		}


	public void playerEat(){
		_storage.playerEat();
	}

	public void LoseHalfResources(){
		_storage.LoseHalfResources();
	}

	/// <summary>
	/// Kills the survivor.
	/// </summary>
	public void KillSurvivor (string s)
	{
		//Find the survivors position
		int sPosition = -1;
		for (int i = 0; i < _numSurvivors; i++) {
			if (_survivors [i].Name == s) {
				sPosition = i;
			}
		}
		Debug.Log ("Killing Survivor: " + s + " sPosition: " + sPosition);
		
		KillSurvivor(_survivors[sPosition]);
		
	}

	/// <summary>
	/// Kills a random survivor.
	/// </summary>
	public string KillRandomSurvivor(){

		name = _survivors[(int)Random.Range (0,_numSurvivors)].Name;
		KillSurvivor(name);
		return name;
	}

		/// <summary>
		/// Kills the survivor.
		/// </summary>
		public void KillSurvivor (Survivor s)
		{
				//Find the survivors position
				int sPosition = -1;
				for (int i = 0; i < _numSurvivors; i++) {
						if (_survivors [i].Name == s.Name) {
								sPosition = i;
						}
				}
				Debug.Log ("Killing Survivor: " + s.Name + " sPosition: " + sPosition);

				//Swap him with the survivor at the end of the list
				//Edit: Don't do this or it will break the map images.  Just leave them at the same index.

				//Debug.Log (_survivors[sPosition].Name);
				//_survivors[sPosition] = CopySurvivor(_survivors[_numSurvivors-1]);
				//Debug.Log (_survivors[sPosition].Name);
				//_numSurvivors--;
				//Debug.Log (_numSurvivors);
				Destroy (s.image);
				Destroy (s);

		}

		// ================================================== initialization
		/// <summary>
		/// The default starting configuration for the shelter
		/// </summary>

		private void DefaultSetup ()
		{
				//assign images
				//_images [0] = GameObject.FindWithTag ("Brian");
				//_images [1] = GameObject.FindWithTag ("Marina");



				//create basic survivor
				//_survivors [0] = CreateSurvivor("Brian", _images[0]);
				//_survivors [1] = CreateSurvivor("Marina", _images[1]);
				//_survivors [2] = CreateSurvivor("Jimbob");
				//_survivors [3] = CreateSurvivor("Jones");

				for (int s = 0; s < _numSurvivors; s++) {
						Debug.Log (_survivors [s].AssignedTask.ToString ());
				}

		}

		// Use this for initialization
		void Start ()
		{
		_gameWorld = this.GetComponent<GameWorld>();
		_visitors = this.GetComponent<Visitor>();
		_gametime = this.GetComponent<GameTime>();

				_defenses = 0;
				_survivors = new Survivor[6];
				_images = new GameObject[6];
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
