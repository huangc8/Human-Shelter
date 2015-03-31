/// <summary>
/// Game world. Tracks which areas are scavenge-able as well as your enemies strength.
/// </summary>
using UnityEngine;
using System.Collections;

public class GameWorld : MonoBehaviour {
	public enum ScavengeableLocation{
		Hospital, //Gives food and medicine
		GroceryStore, //Gives food only
		Mall //Gives luxuries and foodS
	}

	public enum ScavengeQuality{
		Plentiful,
		Good,
		Scarce
	}

	private ScavengeableLocation _scavengeTarget;
	private ScavengeQuality _scavengeQuality;

	public ScavengeableLocation ScavengeTarget{
		get{
			return _scavengeTarget;
		}
	}

	public ScavengeQuality ScavengeQualityLevel{
		get{
			return _scavengeQuality;
		}
	}

	static T GetRandomEnum<T>()
	{
		System.Array A = System.Enum.GetValues(typeof(T));
		T V = (T)A.GetValue(UnityEngine.Random.Range (0,A.Length));
		return V;
	}

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void printTarget(){
		Debug.Log("Today we can raid a " + _scavengeTarget.ToString() + " with a " + _scavengeQuality.ToString() +  " number of resources.");

	}


	private void selectScavengeTarget(){
		_scavengeTarget = GetRandomEnum<ScavengeableLocation>();
		_scavengeQuality = GetRandomEnum<ScavengeQuality>();
	}

	/// <summary>
	/// Start a new Day
	/// </summary>
	public Report NewDay(){
		//change which structure we can scavenge
		selectScavengeTarget();
		Report r = new Report();
		r.SetMessage("Today we can raid a " + _scavengeTarget.ToString() + " with a " + _scavengeQuality.ToString() +  " number of resources.");
		return r;
	}
}
