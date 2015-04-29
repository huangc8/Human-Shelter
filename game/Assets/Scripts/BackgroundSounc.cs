using UnityEngine;
using System.Collections;

public class BackgroundSounc : MonoBehaviour {

	public AudioSource _Day5BGM;
	public AudioSource _Day6BGM;
	public AudioSource _Day7BGM;
	public GameObject _g;
	private GameTime _gt;

	// initialize
	void Start(){
		_gt = _g.GetComponent<GameTime> ();
	}

	// Update is called once per frame
	void Update () {
		switch (_gt._currentDay) {
		case 5:
			if(!_Day5BGM.isPlaying){
				_Day6BGM.Stop ();
				_Day7BGM.Stop ();
				_Day5BGM.Play();
			}
			break;
		case 6:
			if(!_Day6BGM.isPlaying){
				_Day5BGM.Stop ();
				_Day7BGM.Stop ();
				_Day6BGM.Play();
			}
			break;
		case 7:
			if(!_Day7BGM.isPlaying){
				_Day5BGM.Stop ();
				_Day6BGM.Stop ();
				_Day7BGM.Play();
			}
			break;
		}
	}
}
