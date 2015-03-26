//handle the visitors at the gate
using UnityEngine;
using System.Collections;

public class Visitor : MonoBehaviour {
	public Survivor[] _personList;
	public GameObject[] _images;

	public GameTime _gametime;

	private Survivor CreateSurvivor(string name, GameObject image){
		Survivor stmp = new Survivor ();
		stmp.Init ();
		stmp.Name = name;
		stmp.image = image;
		return stmp;
	}

	/// <summary>
	/// Rejects the survivor at gate.
	/// </summary>
	public void RejectSurvivorAtGate(string name){
		if(name != _personList [_gametime._currentDay].Name){
			Debug.LogError("error executing wrong person");
		}
		_personList [_gametime._currentDay] = null;
	}

	/// <summary>
	/// Kills the survivor at gate.
	/// </summary>
	public void KillSurvivorAtGate(string name){
		RejectSurvivorAtGate(name);
	}

	// Use this for initialization
	void Start () {
		_gametime = this.GetComponent<GameTime>();

		//images
		_images = new GameObject[30];
		_images [0] = GameObject.FindWithTag ("Brian");
		_images [1] = GameObject.FindWithTag ("Marina");


		_personList = new Survivor[2];
		//index maps to day of arrival, game starts on day 0
		_personList [0] = CreateSurvivor ("Brian", _images[0]);
		_personList [1] = CreateSurvivor ("Marina", _images[1]);


	}

	// Update is called once per frame
	void Update () {
	
	}
}
