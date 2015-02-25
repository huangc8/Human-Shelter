/// <summary>
/// Shelter. This class controls the properties of the shelter including
/// the survivors and they're corresponding tasks
/// </summary>
using UnityEngine;
using System.Collections;


public class Shelter : MonoBehaviour {
	public Survivor [] _survivors; //array of survivors, maximum capacity is 6
	private int _numSurvivors;

	// Use this for initialization
	void Start () {
		_survivors = new Survivor[6];
		DefaultSetup();
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
		_survivors[1] = s2;
	}

	// Update is called once per frame
	void Update () {

	}
}
