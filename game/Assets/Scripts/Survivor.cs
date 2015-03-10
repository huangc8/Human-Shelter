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

	private int _health = 10;
	private int _fatigue = 10;

	private int [] _proficiencies; //array, stores skill at each task

	private string _name;

	public string Name{
		get{
			return _name;
		}
		set{
			_name = value;
		}
	}

	public int Health{
		get{
			return _health;
		}
	}

	public int Fatigue{
		get{
			return _fatigue;
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
		RandomizeProficiences();
	}

	/// <summary>
	/// Have this survivor scout, return info about surroundings on report
	/// </summary>
	public Report Scout(Shelter s){
		Report r = new Report();
		r.SetMessage(_name + " successfully scouted");
		return r;
	}

	/// <summary>
	/// Scavenge  for the shelter s.
	/// </summary>
	/// <param name="s">S.</param>
	public Report Scavenge(Shelter s){
		Report r = new Report();

		int proficiency = GetProficiency(task.Scavenge);
		if(Random.Range(0,proficiency) < 3){
			s.KillSurvivor(this);
		}
		else{
			s.Food += Random.Range(0,10)*proficiency;
			s.Medicine += Random.Range(0,10)*proficiency;
			s.Luxuries += Random.Range(0,10)*proficiency;
			r.SetMessage(_name + " Scavenged supplies are now Food:" + s.Food + " Medicine:" + s.Medicine + " Luxuries:" + s.Luxuries);
		}
		return r;
	}

	public void Exhaust(){
		_fatigue++;
	}

	/// <summary>
	/// Defend the specified shelter s.
	/// </summary>
	/// <param name="s">Shelter</param>
	public Report Defend(Shelter s){
		int proficiency = GetProficiency(task.Defend);
		int newDefenses = 0;
		Report r = new Report();
		r.SetMessage(_name + " Bolstered defenses to " + newDefenses);
		return r;
	}

	/// <summary>
	/// Heals this survivor
	/// </summary>
	public void HealMe(){
		_health +=5;
	}

	/// <summary>
	/// Rest this instance.
	/// </summary>
	public int RestMe(){
		_fatigue -= 5;
		return _fatigue;
	}

	public Report Rest(Shelter s){
		Report r = new Report();

		int restoration = RestMe();

		r.SetMessage(_name + "'s fatigue is restored to " + restoration);
		return r;
	}

	/// <summary>
	/// Heal the other survivors
	/// </summary>
	public Report Heal(Shelter s){
		Report r = new Report();
		int heals = 0;
		for(int i = 0; i < s.NumberOfSurvivors; i++){
			if(s._survivors[i]._task == task.Resting){
				heals++;
				s._survivors[i].HealMe();
			}
		}
		r.SetMessage(_name + " healed " + heals + " survivors.");
		return r;
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
	}

	/// <summary>
	/// Converse with this survivor.
	/// </summary>
	public void Converse(){
		_conversationsHad++;
	}

}
