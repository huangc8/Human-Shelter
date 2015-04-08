/// <summary>
/// Survivor. Class contains all the informaton about htis particular survivor
/// </summary>
using UnityEngine;
using System.Collections;

public class Survivor : ScriptableObject
{

	// ========================================================= data
	// public info
	public GameWorld _gameWorld;					// game world class
	public task _task = task.Unassigned; 			// current given task
	public bool _assignedTask; 						// if the character is not on a mission, idle is true
	public bool _enabled = false;					// whether character is enabled
	
	// fix survivor info
	private string _name; 							// name of the survivor
	public GameObject image;						// character sprite
	private int _appetite = 10;						// rate of consuming food
	private int[] _proficiencies; 					// array, stores skill at each task


	// changing survivor info
	private int _health = 10; 						// health of survivor
	private int _fatigue = 10; 						// fatigue level of survivor
	private int _conversationsHad; 					// how many times the player has talked to this character
	public int _conversationsLeft;					// how many conversation left

	// enum for task
	public enum task
	{
		Scout,				// collection upcoming game event info
		Heal, 				// restore other player health 
		Defend,				// increase camp defend 
		Scavenge,			// increase camp resource
		Raiding, 			// massively increase camp resource (special one, can't always be used)
		Resting,			// restore survivor fatigue
		Execute,			// delete survivor
		Evict,				// delete survivor
		Unassigned,			// go to resting
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
	/// Gets the proficiencies array used in copy constructor.
	/// </summary>
	/// <returns>The proficiencies.</returns>
	public int [] GetProficiencies ()
	{
		return _proficiencies;
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
	/// Gets a value indicating whether this instance is assinged task.
	/// </summary>
	/// <value><c>true</c> if this instance is assinged task; otherwise, <c>false</c>.</value>
	public bool IsAssingedTask {
		get {
			return _assignedTask;
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
		set {
			_health = value;
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
	/// Gets the fatigue.
	/// </summary>
	/// <value>The fatigue.</value>
	public int Fatigue {
		get {
			return _fatigue;
		}
		set {
			_fatigue = value;
		}
	}

	/// <summary>
	/// Gets or sets the appetitie.
	/// </summary>
	/// <value>The appetitie.</value>
	public int Appetitie {
		get {
			return _appetite;
		}
		set {
			_appetite = value;
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

	/// <summary>
	/// Heals this survivor
	/// </summary>
	public void HealMe (int healAmount)
	{
		_health += healAmount;
	}

	/// <summary>
	/// Eat.
	/// </summary>
	/// <param name="s">S.</param>
	public void Eat (Shelter s)
	{
		if (s.EatFood (Random.Range (1, _appetite)) == false) {
			_health--;
			if (_health < 0) {
				s.KillSurvivor (this);
			}
		}
	}

	/// <summary>
	/// Consumes medicine.
	/// </summary>
	/// <param name="s">S.</param>
	public void ConsumeMedicine (Shelter s)
	{
		if (_health < 10) {
			if (s.Medicine < 2) {
				_health--;
			} else {
				s.UseMedicine (2); //consume to stabilize
			}
		}
		if (s.Medicine < 1) {
			_health--;
		} else {
			s.UseMedicine (1); //consume just cause
		}
		
		if (_health < 0) {
			s.KillSurvivor (this);
		}
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

	// =================================================== tasks
	/// <summary>
	/// Have this survivor scout, return info about surroundings on report
	/// </summary>
	public ArrayList Scout (Shelter s)
	{
		int boost  = 0;
		if(s.ConsumeFood(1)){
			boost++;
		}

		Report r = new Report ();
		
		int proficiency = GetProficiency (task.Scout) + 10;
		
		int locationBonus = (int)Mathf.Pow (Random.Range (1.0f, 4.0f) * proficiency, .36f);
		_gameWorld.AddScoutingBonus (locationBonus);
		r.SetMessage (_name + " successfully scouted and helped to locate a scavenging target");
		
		//Look for enemy camps
		ArrayList NewReps = _gameWorld.ScoutForShelters (proficiency + boost);
		NewReps.Add (r);
		
		return NewReps;
	}

	/// <summary>
	/// Heal the other survivors
	/// </summary>
	public Report Heal (Shelter s)
	{
		Report r = new Report ();
		int heals = 0;
		int fatigueModifier = (100 / (10 + Fatigue)) * 10;
		
		int proficiency = GetProficiency (task.Heal);
		
		int medicineUsed = 20 - (proficiency + fatigueModifier);
		
		for (int i = 0; i < s.NumberOfSurvivors; i++) {
			if (s.Medicine >= medicineUsed) {
				if (s._survivors [i]._task == task.Resting) {
					heals++;
					s._survivors [i].HealMe (proficiency + fatigueModifier);
				}
				s.UseMedicine (medicineUsed);
			}
		}
		r.SetMessage (_name + " healed " + heals + " survivors.");
		return r;
	}

	/// <summary>
	/// Defend the specified shelter s.
	/// </summary>
	/// <param name="s">Shelter</param>
	public Report Defend (Shelter s)
	{
		int fatigueModifier = (100 / (10 + Fatigue)) * 10;

		int proficiency = (GetProficiency (task.Defend) + 10) + fatigueModifier;
		int newDefenses = s.BolsterDefenses (proficiency);

		Report r = new Report ();
		r.SetMessage (_name + " Bolstered defenses to " + newDefenses);
		return r;
	}
	
	/// <summary>
	/// Scavenge  for the shelter s.
	/// </summary>
	/// <param name="s">S.</param>
	public Report Scavenge (Shelter s)
	{
		Report r = new Report ();

		int fatigueModifier = (100 / (10 + Fatigue)) * 10;
		int proficiency = GetProficiency (task.Scavenge);
		if (Random.Range (-10, proficiency + fatigueModifier) < -8 && Random.Range (0, 10) < 3) {
			
			r.SetMessage (_name + " died scavenging.");
			s.KillSurvivor (this);
		} else {
			int sFood = 0;
			int sMedicine = 0;
			int sParts = 0;
			
			int qualityMultiplier = 1 + (int)_gameWorld.ScavengeQualityLevel;
			
			switch (_gameWorld.ScavengeTarget) {
			case GameWorld.ScavengeableLocation.GroceryStore:
				sFood += 1 + qualityMultiplier * (int)(Random.Range (0, 10) * (proficiency + fatigueModifier + 11) * .1f);
				r.SetMessage (_name + " scavenged " + sFood + " food.");
				s.Food += sFood;
				
				break;
				
			case GameWorld.ScavengeableLocation.Hospital:
				sMedicine += 1 + qualityMultiplier * (int)(Random.Range (0, 10) * (proficiency + fatigueModifier + 11) * .1f);
				r.SetMessage (_name + " scavenged " + sMedicine + " medicine.");
				s.Medicine += sMedicine;
				break;
				
				
			case GameWorld.ScavengeableLocation.Mall:
				sParts += 1 + qualityMultiplier * (int)(Random.Range (0, 10) * (proficiency + fatigueModifier + 11) * .1f);
				r.SetMessage (_name + " scavenged " + sParts + " parts.");
				s.Parts += sParts;
				break;
				
			}
			/*
						s.Food += 1 + (int)(Random.Range (0, 10) * (proficiency + fatigueModifier + 11) * .1f);
						s.Medicine += 1 + (int)(Random.Range (0, 10) * (proficiency + fatigueModifier + 11) * .1f);
						s.Luxuries += 1 + (int)(Random.Range (0, 10) * (proficiency + fatigueModifier + 11) * .1f);
						r.SetMessage (_name + " Scavenged supplies are now Food:" + s.Food + " Medicine:" + s.Medicine + " Luxuries:" + s.Luxuries);
			*/
		}
		return r;
	}

	/// <summary>
	/// Raid the specified shelter.
	/// </summary>
	/// <param name="s">S.</param>
	public Report Raid (Shelter s)
	{

		//boost up to 3 points by using 3 food, 3 medicine and 3 parts
		int boost = 0;
		if(s.ConsumeFood(3)){
			boost++;
		}
		if(s.ConsumeMedicine(3)){
			boost++;
		}
		
		if(s.ConsumeParts(3)){
			boost++;
		}

		int fatigueModifier = (100 / (10 + Fatigue)) * 10;
		
		int proficiency = GetProficiency (task.Raiding) + fatigueModifier + boost;
		int newAttack = s.BolsterAttack (proficiency);
		
		Report r = new Report ();
		r.SetMessage (_name + " Bolstered attack strength to " + newAttack);
		return r;
	}

	/// <summary>
	/// Evict the specified survivor.
	/// </summary>
	/// <param name="s">S.</param>
	public Report Evict (Shelter s)
	{
		Report r = new Report ();
		r.SetMessage (_name + " successfully evicted");

		s.KillSurvivor (this);
		s.EvictSurvivor (this);
		return r;
	}

	/// <summary>
	/// Execute the specified survivor.
	/// </summary>
	/// <param name="s">S.</param>
	public Report Execute (Shelter s)
	{
		Report r = new Report ();
		s.KillSurvivor (this);
		r.SetMessage (_name + " successfully executed");
		return r;
		
	}
	
	/// <summary>
	/// Rest.
	/// </summary>
	/// <param name="s">S.</param>
	public Report Rest (Shelter s)
	{
		Report r = new Report ();
		int restoration = RestMe ();
		int proficiency = GetProficiency (task.Defend);
		s.BolsterDefenses (proficiency / 4);

		r.SetMessage (_name + "'s fatigue is restored to " + restoration);
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
	/// Initializes the brian proficiencies.
	/// </summary>
	public void InitializeBrianProficiencies(){
		int taskCount = (int)Survivor.task.Count;
		_proficiencies = new int[taskCount];
		
		
		//Not used:
		
		_proficiencies[(int)Survivor.task.Unassigned] = 0;
		
		
		_proficiencies[(int)Survivor.task.Evict] = 9; //Skill at resisting eviction
		_proficiencies[(int)Survivor.task.Execute] = 6; //Skill in resisting execution (not being executed)
		_proficiencies[(int)Survivor.task.Defend] = 4;
		_proficiencies[(int)Survivor.task.Heal] = 2;
		_proficiencies[(int)Survivor.task.Raiding] = 2;
		_proficiencies[(int)Survivor.task.Resting] = 2;
		_proficiencies[(int)Survivor.task.Scavenge] = 2;
		_proficiencies[(int)Survivor.task.Scout] = 2;


		_appetite = 2;

	}

	/// <summary>
	/// Initializes the marina proficiencies.
	/// </summary>
	public void InitializeMarinaProficiencies(){
		int taskCount = (int)Survivor.task.Count;
		_proficiencies = new int[taskCount];
		
		
		//Not used:
		
		_proficiencies[(int)Survivor.task.Unassigned] = 0;
		
		
		_proficiencies[(int)Survivor.task.Evict] = -4; //Skill at resisting eviction
		_proficiencies[(int)Survivor.task.Execute] = -7; //Skill in resisting execution (not being executed)
		_proficiencies[(int)Survivor.task.Defend] = -8;
		_proficiencies[(int)Survivor.task.Heal] = 8;
		_proficiencies[(int)Survivor.task.Raiding] = -8;
		_proficiencies[(int)Survivor.task.Resting] = 6;
		_proficiencies[(int)Survivor.task.Scavenge] = 6;
		_proficiencies[(int)Survivor.task.Scout] = 6;

		_appetite = 1;

	}

	/// <summary>
	/// Initializes the eric proficiencies.
	/// </summary>
	public void InitializeEricProficiencies(){
		int taskCount = (int)Survivor.task.Count;
		_proficiencies = new int[taskCount];
		
		
		//Not used:
		
		_proficiencies[(int)Survivor.task.Unassigned] = 0;
		
		
		_proficiencies[(int)Survivor.task.Evict] = 2; //Skill at resisting eviction
		_proficiencies[(int)Survivor.task.Execute] = 5; //Skill in resisting execution (not being executed)
		_proficiencies[(int)Survivor.task.Defend] = 5;
		_proficiencies[(int)Survivor.task.Heal] = -7;
		_proficiencies[(int)Survivor.task.Raiding] = 4;
		_proficiencies[(int)Survivor.task.Resting] = -7;
		_proficiencies[(int)Survivor.task.Scavenge] = 8;
		_proficiencies[(int)Survivor.task.Scout] = 7;

		_appetite = 3;

	}

	/// <summary>
	/// Initializes the danny proficiencies.
	/// </summary>
	public void InitializeDannyProficiencies(){
		int taskCount = (int)Survivor.task.Count;
		_proficiencies = new int[taskCount];
		
		
		//Not used:
		
		_proficiencies[(int)Survivor.task.Unassigned] = 0;
		
		
		_proficiencies[(int)Survivor.task.Evict] = -10; //Skill at resisting eviction
		_proficiencies[(int)Survivor.task.Execute] = -10; //Skill in resisting execution (not being executed)
		_proficiencies[(int)Survivor.task.Defend] = 3;
		_proficiencies[(int)Survivor.task.Heal] = 1;
		_proficiencies[(int)Survivor.task.Raiding] = -2;
		_proficiencies[(int)Survivor.task.Resting] = 9;
		_proficiencies[(int)Survivor.task.Scavenge] = 1;
		_proficiencies[(int)Survivor.task.Scout] = -8;

		_appetite = 4;

	}

	/// <summary>
	/// Initializes the bree proficiencies.
	/// </summary>
	public void InitializeBreeProficiencies(){
		int taskCount = (int)Survivor.task.Count;
		_proficiencies = new int[taskCount];

		_fatigue = 60;
		//Not used:
		
		_proficiencies[(int)Survivor.task.Unassigned] = 0;
		
		
		_proficiencies[(int)Survivor.task.Evict] = 5; //Skill at resisting eviction
		_proficiencies[(int)Survivor.task.Execute] = 5; //Skill in resisting execution (not being executed)
		_proficiencies[(int)Survivor.task.Defend] = 8;
		_proficiencies[(int)Survivor.task.Heal] = -4;
		_proficiencies[(int)Survivor.task.Raiding] = 6;
		_proficiencies[(int)Survivor.task.Resting] = 7;
		_proficiencies[(int)Survivor.task.Scavenge] = 3;
		_proficiencies[(int)Survivor.task.Scout] = 5;

		_appetite = 1;

	}

	/// <summary>
	/// Initializes the shane proficiencies.
	/// </summary>
	public void InitializeShaneProficiencies(){
		int taskCount = (int)Survivor.task.Count;
		_proficiencies = new int[taskCount];


		//Not used:
		
		_proficiencies[(int)Survivor.task.Unassigned] = 0;


		_proficiencies[(int)Survivor.task.Evict] = -6; //Skill at resisting eviction
		_proficiencies[(int)Survivor.task.Execute] = -6; //Skill in resisting execution (not being executed)
		_proficiencies[(int)Survivor.task.Defend] = 5;
		_proficiencies[(int)Survivor.task.Heal] = 2;
		_proficiencies[(int)Survivor.task.Raiding] = -4;
		_proficiencies[(int)Survivor.task.Resting] = 9;
		_proficiencies[(int)Survivor.task.Scavenge] = 1;
		_proficiencies[(int)Survivor.task.Scout] = 8;

		_appetite = 1;
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

	/// <summary>
	/// Randomizes the characteristics.
	/// </summary>
	public void RandomizeCharacteristics ()
	{
		_appetite = Random.Range (3, 10);
	}

	// ================================================= initialization
		
	/// <summary>
	/// Init this survivor.
	/// </summary>
	public void Init (GameWorld gw, string name)
	{
		_name = name;
		_gameWorld = gw;
		_assignedTask = false;
		_enabled = true;

		Debug.Log ("Character name:" + _name);
		switch(_name){
		case "Brian":
			InitializeBrianProficiencies();
			break;
		case "Marina":
			InitializeMarinaProficiencies();
			break;
		case "Eric":
			InitializeEricProficiencies();
			break;
		case "Danny":
			InitializeDannyProficiencies();
			break;
		case "Bree":
			InitializeBreeProficiencies();
			break;
		case "Shane":
			InitializeShaneProficiencies();
			break;
		default:
			RandomizeProficiences ();
			RandomizeCharacteristics ();
			break;

		}

	}

	/// <summary>
	/// Copies initial.
	/// </summary>
	/// <param name="sCopy">S copy.</param>
	public void CopyInit (Survivor sCopy)
	{
		_name = sCopy.Name;
		_enabled = true;

		_assignedTask = sCopy.IsAssingedTask;
		_task = sCopy.AssignedTask;

		_name = sCopy.Name;
		_health = sCopy.Health;
		_fatigue = sCopy.Fatigue;
		_proficiencies = sCopy.GetProficiencies ();
		_conversationsHad = sCopy.ConversationsHad;
		_appetite = sCopy.Appetitie;
	}
}
