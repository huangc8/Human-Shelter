//handle the visitors at the gate
using UnityEngine;
using System.Collections;

public class Visitor : MonoBehaviour {
	public Survivor[] _personList;

	private Survivor CreateSurvivor(string name){
		Survivor stmp = new Survivor ();
		stmp.Init ();
		stmp.Name = name;
		return stmp;
	}


	// Use this for initialization
	void Start () {
		_personList = new Survivor[30];
		_personList [1] = CreateSurvivor ("Jill");
		_personList [3] = CreateSurvivor ("Frank");
		

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
