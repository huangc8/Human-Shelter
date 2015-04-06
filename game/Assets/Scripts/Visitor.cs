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
		_gameWorld = this.GetComponent<GameWorld>();
		_gametime = this.GetComponent<GameTime>();
		
		//images
		_images = new GameObject[30];
		_images [0] = GameObject.FindWithTag ("Brian");
		_images [1] = GameObject.FindWithTag ("Marina");
		_images [3] = GameObject.FindWithTag ("Eric");
		_images [4] = GameObject.FindWithTag ("Danny");
		_images [6] = GameObject.FindWithTag ("Bree");
		_images [7] = GameObject.FindWithTag ("Shane");
		
		//index maps to day of arrival, game starts on day 1
		_personList = new Survivor[30];
		_personList [0] = CreateSurvivor ("Brian", _images[0]);
		_personList [1] = CreateSurvivor ("Marina", _images[1]);
		_personList [3] = CreateSurvivor ("Eric", _images[3]);
		_personList [4] = CreateSurvivor ("Danny", _images[4]);
		_personList [6] = CreateSurvivor ("Bree", _images[6]);
		_personList [7] = CreateSurvivor ("Shane", _images[7]);
	}

	// =================================================== survivor function
	private Survivor CreateSurvivor(string name, GameObject image){
		Survivor stmp = new Survivor ();
		stmp.Init (_gameWorld);
		stmp.Name = name;
		stmp.image = image;
		return stmp;
	}

	/// <summary>
	/// Rejects the survivor at gate.
	/// </summary>
	public void RejectSurvivorAtGate(string name){
		if(name != _personList [_gametime._currentDay].Name){
			Debug.LogError("Error: not such survivor exist");
		}
		_personList [_gametime._currentDay] = null;
	}

	/// <summary>
	/// Kills the survivor at gate.
	/// </summary>
	public void KillSurvivorAtGate(string name){
		RejectSurvivorAtGate(name);
	}
}
