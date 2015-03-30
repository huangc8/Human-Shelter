/// <summary>
/// Game world. Tracks which areas are scavenge-able as well as your enemies strength.
/// </summary>
using UnityEngine;
using System.Collections;

public class GameWorld : MonoBehaviour {
	public enum ScavengeableLocation{
		Hospital, //Gives food and medicine
		GroceryStore, //Gives food only
		Mall, //Gives luxuries and foodS
		Count
	}

	public enum ScavengeQuality{
		Abundant,
		Good,
		Scarce,
		Count
	}

	private ScavengeableLocation _scavengeTarget;
	private ScavengeQuality _scavengeQuality;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private void selectScavengeTarget(){
		_scavengeTarget = (ScavengeableLocation) Random.Range(0,ScavengeableLocation.Count);
		_scavengeQuality = (ScavengeQuality) Random.Range(0, ScavengeQuality.Count);
	}

	/// <summary>
	/// Start a new Day
	/// </summary>
	public void NewDay(){

		//change which structure we can scavenge
		selectScavengeTarget();
	}
}
