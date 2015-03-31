/// <summary>
/// Game world. Tracks which areas are scavenge-able as well as your enemies strength.
/// </summary>
using UnityEngine;
using System.Collections;

public class GameWorld : MonoBehaviour {

	int _scoutingBonus;

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


	public void AddScoutingBonus(int scoutingBonus){
		_scoutingBonus += scoutingBonus;
	}

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





	private void selectScavengeTarget(){
		_scavengeTarget = GetRandomEnum<ScavengeableLocation>();
		//_scavengeQuality = GetRandomEnum<ScavengeQuality>();
		int scav = Random.Range (0,10);
		scav += _scoutingBonus;
		if(scav > 9){
			_scavengeQuality = ScavengeQuality.Plentiful ;
		}
		else if(scav > 6){
			_scavengeQuality = ScavengeQuality.Good ;
		}
		else{
			_scavengeQuality = ScavengeQuality.Scarce;
		}

		_scoutingBonus = 0;
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
