﻿/// <summary>
/// Survivor. Class contains all the informaton about htis particular survivor
/// </summary>
using UnityEngine;
using System.Collections;

public class Survivor : MonoBehaviour {

	// ========================================================= data
	// public info
	public bool _assignedTask; //if the character is not on a mission, idle is true
	public task _task = task.Unassigned;
	public bool _enabled = false;

	// personal info
	private string _name; // name of the survivor
	private int [] _proficiencies; //array, stores skill at each task
	private int _conversationsHad; //how many times the player has talked to this character

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

	// ======================================================== accessor
	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	/// <value>The name.</value>
	public string Name{
		get{
			return _name;
		}
		set{
			_name = value;
		}
	}

	/// <summary>
	/// Gets the proficiency for the passed task.
	/// </summary>
	/// <returns>The proficiency.</returns>
	/// <param name="t">The task.</param>
	public int GetProficiency(task t){
		return _proficiencies[(int)t];
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
		set{
			this._conversationsHad = value;
		}
	}

	// =================================================== action
	/// <summary>
	/// Performs the task.
	/// </summary>
	/// <param name="s">Reference to the shelter</param>
	/// <param name="t">task to be performed</param>
	public void PerformTask(Shelter s, task t){
		int proficiency = GetProficiency(t);
	switch(t){
		case task.Defend:
			break;
		case task.Heal:
			break;
		case task.Raiding:
			break;
		case task.Resting:
			break;
		case task.Scavenge:
			if(Random.Range(0,proficiency) < 3){
				s.KillSurvivor(this);
			}
			else{
				s.Food += Random.Range(0,10)*proficiency;
				s.Medicine += Random.Range(0,10)*proficiency;
				s.Luxuries += Random.Range(0,10)*proficiency;
			}
			break;
		case task.Scout:
			break;
		}// end of switch
	}

	/// <summary>
	/// Converse with this survivor.
	/// </summary>
	public void Converse(){
		_conversationsHad++;
	}

	/// <summary>
	/// Reset Conversation.
	/// </summary>
	public void ConvReset(){
		_conversationsHad = 0;
	}

	// ================================================= initialization
	/// <summary>
	/// Init this instance.
	/// </summary>
	public void Init(){
		_assignedTask = false;
		_enabled = true;
		RandomizeProficiences();
	}

	/// <summary>
	/// Initialize each value in proficiency array to a
	/// random value
	/// </summary>
	public void RandomizeProficiences(){
		int taskCount = (int)Survivor.task.Count;
		_proficiencies = new int[taskCount];
		for(int t = 0; t < taskCount; t++){
			_proficiencies[t] = Random.Range(-10,10);
		}
	}
}
