/// <summary>
/// Survivor. Class contains all the informaton about htis particular survivor
/// </summary>
using UnityEngine;
using System.Collections;

public class Survivor : MonoBehaviour {
	public bool _assignedTask; //if the character is not on a mission, idle is true
	public bool _enabled = false;
	public task _task = task.Unassigned;
	
	public enum task{
		Scout,
		Heal,
		Defend,
		Scavenge,
		Raiding, //special one, can't always be used
		Resting,
		Unassigned
	}

	/// <summary>
	/// Init this instance.
	/// </summary>
	public void Init(){

		_assignedTask = false;
		_enabled = true;
	}


	/// <summary>
	/// Gets or sets the assigned task.
	/// </summary>
	/// <value>The assigned task.</value>
	public task AssignedTask
	{
		set{
			this._task = value;
		}
		get{
			return this._task;
		}
	}


}
