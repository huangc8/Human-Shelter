/// <summary>
/// Survivor. Class contains all the informaton about htis particular survivor
/// </summary>
using UnityEngine;
using System.Collections;

public class Survivor : ScriptableObject
{

		// ========================================================= data
		// public info
		public bool _assignedTask; //if the character is not on a mission, idle is true
		public task _task = task.Unassigned;
		public bool _enabled = false;

		// personal info
		private string _name; // name of the survivor
		private int _health = 10; // health of survivor
		private int _fatigue = 10; // fatigue level of survivor
		private int[] _proficiencies; //array, stores skill at each task
		private int _conversationsHad; //how many times the player has talked to this character

		public enum task
		{
				Scout,
				Heal, 
				Defend,
				Scavenge,
				Raiding, //special one, can't always be used
				Resting,
		        Execute,
		        Evict,
				Unassigned,
				Count
		}

		// ======================================================== accessor
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name {
				get {
						return _name;
				}
				set {
						_name = value;
				}
		}
	
		/// <summary>
		/// Gets the proficiency for the passed task.
		/// </summary>
		/// <returns>The proficiency.</returns>
		/// <param name="t">The task.</param>
		public int GetProficiency (task t)
		{
				return _proficiencies [(int)t];
		}

		/// <summary>
		/// Gets or sets the assigned task.
		/// </summary>
		/// <value>The assigned task.</value>
		public task AssignedTask {
				set {
						this._task = value;
				}
				get {
						return this._task;
				}
		}
	
		/// <summary>
		/// Gets the number of conversations had.
		/// </summary>
		/// <value>The conversations had.</value>
		public int ConversationsHad {
				get {
						return this._conversationsHad;
				}
				set {
						this._conversationsHad = value;
				}
		}

		/// <summary>
		/// Gets the health.
		/// </summary>
		/// <value>The health.</value>
		public int Health {
				get {
						return _health;
				}
		}

	/// <summary>
	/// Gets the proficiencies array used in copy constructor.
	/// </summary>
	/// <returns>The proficiencies.</returns>
	public int [] GetProficiencies(){
		return _proficiencies;
	}

	/// <summary>
	/// Gets a value indicating whether this instance is assinged task.
	/// </summary>
	/// <value><c>true</c> if this instance is assinged task; otherwise, <c>false</c>.</value>
	public bool IsAssingedTask{
		get{
			return _assignedTask;
		}
	}

		/// <summary>
		/// Gets the fatigue.
		/// </summary>
		/// <value>The fatigue.</value>
		public int Fatigue {
				get {
						return _fatigue;
				}
		}
	
		// =================================================== action
		/// <summary>
		/// Converse with this survivor.
		/// </summary>
		public void Converse ()
		{
				_conversationsHad++;
		}

	
		// =================================================== task
		

	/// <summary>
	/// Defend the specified shelter s.
	/// </summary>
	/// <param name="s">Shelter</param>
	public Report Defend (Shelter s)
	{
		int fatigueModifier = (100/(10+Fatigue))*10;

		int proficiency = GetProficiency (task.Defend) + fatigueModifier;
		int newDefenses = s.BolsterDefenses(proficiency);

		Report r = new Report ();
		r.SetMessage (_name + " Bolstered defenses to " + newDefenses);
		return r;
	}
	
		/// <summary>
		/// Heals this survivor
		/// </summary>
		public void HealMe (int healAmount)
		{
				_health += healAmount;
		}

	public Report Evict(Shelter s)
	{
		Report r = new Report ();
		r.SetMessage (_name + " successfully evicted");

		s.KillSurvivor(this);
		s.EvictSurvivor(this);
		return r;
	}

	public Report Execute(Shelter s)
	{
		Report r = new Report ();
		s.KillSurvivor(this);
		r.SetMessage (_name + " successfully executed");
		return r;
		
	}
	    /// <summary>
		/// Have this survivor scout, return info about surroundings on report
		/// </summary>
		public Report Scout (Shelter s)
		{
				Report r = new Report ();
				r.SetMessage (_name + " successfully scouted");
				return r;
		}
	
		/// <summary>
		/// Scavenge  for the shelter s.
		/// </summary>
		/// <param name="s">S.</param>
	public Report Scavenge (Shelter s)
	{
		Report r = new Report ();
		int fatigueModifier = (100/(10+Fatigue))*10;
		int proficiency = GetProficiency(task.Scavenge);
		if (Random.Range (-10, proficiency + fatigueModifier) < -8 && Random.Range (0,10) < 3) {
			s.KillSurvivor (this);
		} else {
			s.Food += 1 + (int) (Random.Range (0, 10) * (proficiency+fatigueModifier+11) * .1f);
			s.Medicine += 1 + (int) (Random.Range (0, 10) * (proficiency +fatigueModifier+ 11)* .1f);
			s.Luxuries += 1 + (int) (Random.Range (0, 10) * (proficiency +fatigueModifier+ 11) * .1f);
			r.SetMessage (_name + " Scavenged supplies are now Food:" + s.Food + " Medicine:" + s.Medicine + " Luxuries:" + s.Luxuries);
		}
		return r;
	}

		public Report Rest (Shelter s)
		{
				Report r = new Report ();
				int restoration = RestMe ();
		
				r.SetMessage (_name + "'s fatigue is restored to " + restoration);
				return r;
		}
	
		/// <summary>
		/// Heal the other survivors
		/// </summary>
	public Report Heal (Shelter s)
	{
		Report r = new Report ();
		int heals = 0;
		int fatigueModifier = (100/(10+Fatigue))*10;
		
		int proficiency = GetProficiency(task.Heal);

		int medicineUsed = 20-(proficiency+fatigueModifier);

        for (int i = 0; i < s.NumberOfSurvivors; i++) {
			if(s.Medicine >= medicineUsed)
			{
				if (s._survivors [i]._task == task.Resting) {
					heals++;
					s._survivors [i].HealMe (proficiency + fatigueModifier);
				}
				s.UseMedicine(medicineUsed);
			}
		}
		r.SetMessage (_name + " healed " + heals + " survivors.");
		return r;
	}

		// ===================================================== helper

		/// <summary>
		/// Reset Conversation.
		/// </summary>
		public void ConvReset ()
		{
				_conversationsHad = 0;
		}

		/// <summary>
		/// Exhaust this survivor.
		/// </summary>
		public void Exhaust ()
		{
				_fatigue += 10;
		}

		/// <summary>
		/// Rest this survivor's fatigue.
		/// </summary>
		public int RestMe ()
		{
				_fatigue -= 30;
				return _fatigue;
		}

		/// <summary>
		/// Initialize each value in proficiency array to a
		/// random value
		/// </summary>
		public void RandomizeProficiences ()
		{
				int taskCount = (int)Survivor.task.Count;
				_proficiencies = new int[taskCount];
				for (int t = 0; t < taskCount; t++) {
						_proficiencies [t] = Random.Range (-10, 10);
				}
		}

		// ================================================= initialization
		
	public void CopyInit(Survivor sCopy)
	{
		_name = sCopy.Name;
		_enabled = true;

		_assignedTask = sCopy.IsAssingedTask;
		_task = sCopy.AssignedTask;

		_name = sCopy.Name;
		_health = sCopy.Health;
		_fatigue = sCopy.Fatigue;
		_proficiencies = sCopy.GetProficiencies();
		_conversationsHad = sCopy.ConversationsHad;
	}

		/// <summary>
		/// Init this survivor.
		/// </summary>
		public void Init ()
		{
				_assignedTask = false;
				_enabled = true;
				RandomizeProficiences ();
		}
}
