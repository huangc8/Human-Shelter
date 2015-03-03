/// <summary>
/// Shelter. This class controls the properties of the shelter including
/// the survivors and they're corresponding tasks
/// </summary>
using UnityEngine;
using System.Collections;


public class Shelter : MonoBehaviour {
	public Survivor [] _survivors; //array of survivors, maximum capacity is 6
	private int _numSurvivors;


	public class Stores{
		/// <summary>
		/// Stores class, nested class that keeps track of all of the items we currently
		/// have in the shelter
		/// </summary>
		private Shelter _parent;
		int _luxuries;//How much medicine we have
		int _food;//how much food we have
		int _medicine; //how much medicine we have

		/// <summary>
		/// Initializes a new instance of the <see cref="Shelter+stores"/> class.
		/// Sets each store to be zero, sets the parent class too.
		/// </summary>
		/// <param name="parent">Parent.</param>
		public Stores(Shelter parent){
			this._parent = parent;
			_luxuries = 0;
			_food = 0;
			_medicine = 0;
		}

		/// <summary>
		/// Gets or sets the luxuries.
		/// </summary>
		/// <value>The luxuries.</value>
		public int Luxuries{
			get{
				return _luxuries;
			}
			set{
				_luxuries = value;
			}
		}

		/// <summary>
		/// Gets or sets the food.
		/// </summary>
		/// <value>The food.</value>
		public int Food{
			get{
				return _food;
			}
			set{
				_food = value;
			}
		}

		/// <summary>
		/// Gets or sets the medicine.
		/// </summary>
		/// <value>The medicine.</value>
		public int Medicine{
			get{
				return _medicine;
			}
			set{
				_medicine = value;
			}
		}
	}

	private Stores _storage;

	/// <summary>
	/// Gets or sets the medicine.
	/// </summary>
	/// <value>The medicine.</value>
	public int Medicine{
		get{
			return _storage.Medicine;
		}
		set{
			_storage.Medicine = value;
		}
	}

	/// <summary>
	/// Gets or sets the food.
	/// </summary>
	/// <value>The food.</value>
	public int Food{
		get{
			return _storage.Food;
		}
		set{
			_storage.Food = value;
		}
	}
	

	/// <summary>
	/// Gets or sets the luxuries.
	/// </summary>
	/// <value>The luxuries.</value>
	public int Luxuries{
		get{
			return _storage.Luxuries;
		}
		set{
			_storage.Luxuries = value;
		}
	}

	// Use this for initialization
	void Start () {
		_survivors = new Survivor[6];
		_storage = new Stores(this);
		DefaultSetup();

	}

	/// <summary>
	/// Kills the survivor.
	/// </summary>
	public void KillSurvivor(Survivor s){
		_numSurvivors--;
		Destroy(s);
	}

	/// <summary>
	/// The default starting configuration for the shelter
	/// </summary>
	private void DefaultSetup(){
		//create two basic survivors
		Survivor s = new Survivor();
		s.Init ();

		Survivor s2 = new Survivor();
		s2.Init();

		_survivors[0] = s;
		_survivors[0].Name = "Jim Bob Jones";
		_survivors[1] = s2;
		_survivors[1].Name = "Jelly Bean Jimmy";

		_numSurvivors = 2;
	}

	public int NumberOfSurvivors{
		get{
			return _numSurvivors;
		}
		set{
			_numSurvivors = value;
		}
	}

	// Update is called once per frame
	void Update () {

	}
}
