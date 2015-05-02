//handle the visitors at the gate
using UnityEngine;
using System.Collections;

public class Visitor : MonoBehaviour {

	// =================================================== data
	public GameTime _gametime;					// game time reference
	public GameWorld _gameWorld;				// game world reference

	public Survivor[] _personList;				// current person list
	public Survivor[] _originalPersonList;		// full person list
	public GameObject[] _images;				// character image

	// =================================================== initialization
	// Use this for initialization
	void Start () {		
		//images
		_images = new GameObject[30];
		_images [0] = GameObject.FindWithTag ("Brian");
		_images [1] = GameObject.FindWithTag ("Marina");
		_images [3] = GameObject.FindWithTag ("David");
		_images [5] = GameObject.FindWithTag ("Eric");
		_images [11] = GameObject.FindWithTag ("Danny");
		_images [6] = GameObject.FindWithTag ("Bree");
		_images [12] = GameObject.FindWithTag ("Shane");
		
		//index maps to day of arrival, game starts on day 1
		_personList = new Survivor[30];
		_personList [0] = CreateSurvivor ("Brian", _images[0]);
		_personList [1] = CreateSurvivor ("Marina", _images[1]);
		_personList [3] = CreateSurvivor ("David", _images[3]);
		_personList [5] = CreateSurvivor ("Eric", _images[5]);
		_personList [11] = CreateSurvivor ("Danny", _images[11]);
		_personList [6] = CreateSurvivor ("Bree", _images[6]);
		_personList [12] = CreateSurvivor ("Shane", _images[12]);
	}

	// =================================================== survivor function
	// create a survivor
	private Survivor CreateSurvivor(string name, GameObject image){
		Survivor stmp = new Survivor ();
		stmp.Init (_gameWorld, name);
		stmp.Name = name;
		stmp.image = image;
		return stmp;
	}
}
