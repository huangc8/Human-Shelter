/// <summary>
/// Survivor. Class contains all the informaton about htis particular survivor
/// </summary>
using UnityEngine;
using System.Collections;

public class Survivor : MonoBehaviour {
	public bool _assignedTask; //if the character is not on a mission, idle is true
	public bool _enabled = false;
	public task _task = task.Unassigned;
	private int _conversationsHad; //how many times the player has talked to this character

	private string _name;

	public string Name{
		get{
			return _name;
		}
		set{
			_name = value;
		}
	}
	
	public enum task{
		Scout,
		Heal,
		Defend,
		Scavenge,
		Raiding, //special one, can't always be used
		Resting,
		Unassigned,
		Count
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

	/// <summary>
	/// Gets the number of conversations had.
	/// </summary>
	/// <value>The conversations had.</value>
	public int ConversationsHad
	{
		get{
			return this._conversationsHad;
		}
	}

	/// <summary>
	/// Converse with this survivor.
	/// </summary>
	public void Converse(){
		_conversationsHad++;
	}

}
