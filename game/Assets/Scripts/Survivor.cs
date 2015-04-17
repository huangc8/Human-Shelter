/// <summary>
/// Survivor. Class contains all the informaton about htis particular survivor
/// </summary>
using UnityEngine;
using System.Collections;

public class Survivor : ScriptableObject
{

    // ========================================================= data
    // public info
    public GameWorld _gameWorld;                    // game world class
    public task _task = task.Unassigned;            // current given task
    public bool _assignedTask;                      // if the character is not on a mission, idle is true
    public bool _enabled = false;                   // whether character is enabled
	public bool _notify = false;					// whether character have important things to say
    
    // fix survivor info
    private string _name;                           // name of the survivor
    public GameObject image;                        // character sprite
    private int _appetite = 10;                     // rate of consuming food
    private int[] _proficiencies;                   // array, stores skill at each task
	private hunger _starvation;						// how hungry the person is

    // changing survivor info
    private int _health = 10;                       // health of survivor
    private int _fatigue = 10;                      // fatigue level of survivor

	// enum for people hungry degree
	public enum hunger
	{
		Content,
		Satiatiated,
		Hungry,
		Famished,
		Starving,
		Count
	}

    // enum for task
    public enum task
    {
        Scout,              // collection upcoming game event info
        Heal,               // restore other player health 
        Defend,             // increase camp defend 
        Scavenge,           // increase camp resource
        Raiding,            // massively increase camp resource (special one, can't always be used)
        Resting,            // restore survivor fatigue
		Evict,            	// delete survivor
        Execute,            // delete survivor
        Unassigned,         // go to resting
        Count
    }

    // enum to represent the types of wounds that can be incurred on a mission
    public enum wound
    {
        Uninjured,
        Minor,
        Moderate,
        Severe,
        Grievous,
        Count
    }

    // ======================================================== accessor
    // gets & set survivor name.
    public string Name
    {
        get{
            return _name;
        }
        set{
            _name = value;
        }
    }
   	
	// gets the proficiency for the passed task.
    public int GetProficiency(task t)
    {
        return _proficiencies [(int)t];
    }

	// get the proficiencies array used in copy constructor.
    public int [] GetProficiencies()
    {
        return _proficiencies;
    }

	// gets & set the assigned task.
    public task AssignedTask
    {
        set {
            this._task = value;
        }
        get {
            return this._task;
        }
    }

    // get a value indicating whether this instance is assinged task.
    public bool IsAssingedTask
    {
        get {
            return _assignedTask;
        }
    }

	// gets & set health
    public int Health
    {
        get
        {
            return _health;
        }
        set
        {
            _health = value;
			if(_health < 0){
				_health = 0;
				_gameWorld._shelter.KillSurvivor(this._name);

			}
        }
    }

	// gets & set fatigue
    public int Fatigue
    {
        get
        {
            return _fatigue;
        }
        set
        {
            _fatigue = value;
        }
    }

	// gets & set appetite
    public int Appetitie
    {
        get
        {
            return _appetite;
        }
        set
        {
            _appetite = value;
        }
    }

    // =================================================== action
	// heal this instance
    public void HealMe(int healAmount)
    {
        Health += healAmount;
    }

	// Advances the hunger. Moves the survivor to the next level of starvation
	public Report AdvanceHunger(){
		Report r = new Report();
		if(_starvation != hunger.Starving){
			_starvation = (hunger)((int) _starvation + 1);
			r.SetMessage(_name +  " is now " + _starvation.ToString() + ".");
		} 
		else{ //_starvation == hunger.Starving
			r.SetMessage(_name +  " is starving to death.");
		}

		return r;
	}

    // Eat.
    public Report Eat(Shelter s)
    {
		Report r = new Report();
		int consumption = Random.Range(_appetite, _appetite + 4);

        if (s.EatFood(consumption) == false)
		{

			r = AdvanceHunger();

			if(_starvation == hunger.Starving){
				Health--;

			}
            if (Health < 0)
            {
				r.SetMessage(_name + " has starved to death.");
                s.KillSurvivor(this.Name);
            }
			return r;
        }
		else{
			_starvation = hunger.Content;

		}

		return null;
    }

    // Consumes medicine.
    public void ConsumeMedicine(Shelter s)
    {
		// if massively wounded
        if (Health < 10)
        {
            if (s.Medicine < 2)
            {
                Health--;
            } else
            {
                s.UseMedicine(2); //consume to stabilize
            }
        }
		
        if (s.Medicine < 1)
        {
            Health--;
        } else
        {
            s.UseMedicine(1); //consume just cause
        }
        
        if (Health < 0)
        {
            s.KillSurvivor(this.Name);
        }
    }

    // increase fatigue
    public void Exhaust()
    {
        _fatigue += 10;
    }
    
    // Reduce fatigue.
    public int RestMe()
    {
        int proficiency = GetProficiency(task.Resting);
		int restModifier = ((int)hunger.Count - (int)_starvation);
        _fatigue -= (int)((proficiency * (Health)) / 10.0f) * restModifier;
        return _fatigue;
    }

    // Check to see if the surivor is wounded/dies return false for failing the mission, true for continuing
    public bool WoundCheck(Shelter s, Report r, int successChance, string tasking,string task){
        
        wound sWound = wound.Uninjured;
        //Chance of getting wounded, if the wound is severe enough, do not continue scouting
        if (successChance + Random.Range(0, 10) < 13)
        {
            sWound = (wound) Random.Range(0, (int)wound.Count);
            switch (sWound)
            {
                case wound.Uninjured:
                    break;
                case wound.Minor:
                    Health -= Random.Range(0, 1); //1 being a papercut
                    break;
                case wound.Moderate:
                    Health -= Random.Range(1, 3); //1 being a papercut
                    break;
                case wound.Severe:
                    Health -= Random.Range(3, 5); //1 being a papercut
                    break;
                case wound.Grievous:
                    Health -= Random.Range(5, 7); //1 being a papercut
                    break;
            }
        }
        
        if(Health < 0){
            r.SetMessage(_name + " died after sustaining a " + sWound.ToString() + " wound " + tasking + ".");
            s.KillSurvivor(this.Name);
            return false;
        }
        else if (sWound == wound.Severe || sWound == wound.Grievous)
        {
            r.SetMessage(_name + " attempted to " + task + " but wound up sustaining a " + sWound.ToString() + " wound.");
            return false;
        }
        return true;
    }

    // =================================================== tasks
	// scout
    public ArrayList Scout(Shelter s)
    {
        int boost = 0;
        if (s.ConsumeFood(1))
        {
            boost++;
        }

        Report rTemporary = new Report();

        int spillover = 0;
        
        if (Fatigue < 0)
        {
            spillover = Mathf.Abs(Fatigue);
        }
        int fatigueModifier = (100 / (10 + (10 + Mathf.Max(Fatigue, 0)))) * 10 + spillover;
        
        int proficiency = GetProficiency(task.Scout) + 10 + fatigueModifier;
        
        wound sWound = wound.Uninjured;

        ArrayList NewReps = new ArrayList();

        if(WoundCheck(s,rTemporary,proficiency, "scouting","scout"))
        {

            int locationBonus = (int)Mathf.Pow(Random.Range(1.0f, 4.0f) * proficiency, .36f);
            

            _gameWorld.AddScoutingBonus(locationBonus);
            if (sWound == wound.Uninjured)
            {
                rTemporary.SetMessage(_name + " successfully scouted and helped to locate a scavenging target.");
            } else
            {
                rTemporary.SetMessage(_name + " sustained a " + sWound.ToString() + " wound while scouting but still helped to locate a scavenging target.");
            }
            //Look for enemy camps
            NewReps = _gameWorld.ScoutForShelters(proficiency + boost);


        }
		if(rTemporary.IsInitialized()){
			NewReps.Add(rTemporary);
		}
        return NewReps;
    }

	// heal other survivor
    public Report Heal(Shelter s)
    {
        Report r = new Report();
        int heals = 0;
        int spillover = 0;

        if (Fatigue < 0)
        {
            spillover = Mathf.Abs(Fatigue);
        }
        int fatigueModifier = (100 / (10 + (10 + Mathf.Max(Fatigue, 0)))) * 10 + spillover;
        
        int proficiency = GetProficiency(task.Heal);
        
        int medicineUsed = 20 - (proficiency + fatigueModifier);
        
        for (int i = 0; i < s.NumberOfSurvivors; i++)
        {
            if (s.Medicine >= medicineUsed)
            {
                if (s._survivors [i]._task == task.Resting)
                {
                    heals++;
                    s._survivors [i].HealMe(proficiency + fatigueModifier);
                }
                s.UseMedicine(medicineUsed);
            }
        }
        r.SetMessage(_name + " healed " + heals + " survivors.");
        return r;
    }

    // Defend the specified shelter s.
    public Report Defend(Shelter s)
    {
        int spillover = 0;
        if (Fatigue < 0)
        {
            spillover = Mathf.Abs(Fatigue);
        }
        int fatigueModifier = (100 / (10 + Mathf.Max(Fatigue, 0))) * 10 + spillover;

        int proficiency = (GetProficiency(task.Defend) + 10) + fatigueModifier;
        Shelter.DefenseLevel newDefenses = s.BolsterDefenses(proficiency);

		string defenseDescription = " undefended.";

		switch(newDefenses){
		case Shelter.DefenseLevel.Undefended:
			defenseDescription = " undefended.";
			break;
		case Shelter.DefenseLevel.BarelyDefended:
			defenseDescription = " barely defended.";
			break;
		case Shelter.DefenseLevel.SlightlyDefended:
			defenseDescription = " slightly defended.";
			break;
		case Shelter.DefenseLevel.ModeratelyDefended:
			defenseDescription = " moderately defended.";
			break;
		case Shelter.DefenseLevel.HeavilyDefended:
			defenseDescription = " heavily defended.";
			break;
		case Shelter.DefenseLevel.WellDefended:
			defenseDescription = " well defended.";
			break;
		case Shelter.DefenseLevel.InpenetrableFortress:
			defenseDescription = " very well defended.";
			break;
		default:
			defenseDescription = " undefended.";
			break;
		}


        Report r = new Report();
        r.SetMessage(_name + " bolstered defenses to" + defenseDescription );
        return r;
    }
    
    // Scavenge.
    public Report Scavenge(Shelter s)
    {
        Report r = new Report();
        int spillover = 0;
        if (Fatigue < 0)
        {
            spillover = Mathf.Abs(Fatigue);
        }
        int fatigueModifier = (100 / (10 + (10 + Mathf.Max(Fatigue, 0)))) * 10 + spillover;
        int proficiency = GetProficiency(task.Scavenge)+ fatigueModifier;
		
        if(WoundCheck(s,r,proficiency,"scavenging","scavenge"))
        {
            int sFood = 0;
            int sMedicine = 0;
            int sParts = 0;
            
            int qualityMultiplier = 1 + (int)_gameWorld.ScavengeQualityLevel;
            
            switch (_gameWorld.ScavengeTarget)
            {
                case GameWorld.ScavengeableLocation.GroceryStore:
                    sFood += 1 + qualityMultiplier * (int)(Random.Range(0, 10) * (proficiency + fatigueModifier + 11) * .1f);
                    r.SetMessage(_name + " scavenged " + sFood + " food.");
                    s.Food += sFood;                
                    break;
                
                case GameWorld.ScavengeableLocation.Hospital:
                    sMedicine += 1 + qualityMultiplier * (int)(Random.Range(0, 10) * (proficiency + fatigueModifier + 11) * .1f);
                    r.SetMessage(_name + " scavenged " + sMedicine + " medicine.");
                    s.Medicine += sMedicine;
                    break;
                
                
                case GameWorld.ScavengeableLocation.Mall:
                    sParts += 1 + qualityMultiplier * (int)(Random.Range(0, 10) * (proficiency + fatigueModifier + 11) * .1f);
				if(sParts == 1){
                    r.SetMessage(_name + " scavenged " + sParts + " part.");
				}
				else{
					
					r.SetMessage(_name + " scavenged " + sParts + " parts.");
				}
                    s.Parts += sParts;
                    break;
                
            }
        }
        return r;
    }

    /// <summary>
    /// Raid the specified shelter.
    /// </summary>
    /// <param name="s">S.</param>
    public Report Raid(Shelter s)
    {
        // boost up to 3 points by using 3 food, 3 medicine and 3 parts
        int boost = 0;
        if (s.ConsumeFood(3))
        {
            boost++;
        }
        if (s.ConsumeMedicine(3))
        {
            boost++;
        }
        
        if (s.ConsumeParts(3))
        {
            boost++;
        }
		
        int spillover = 0;
        if (Fatigue < 0)
        {
            spillover = Mathf.Abs(Fatigue);
        }
        int fatigueModifier = (int)((100 / (10 + Mathf.Max(Fatigue, 0.0f))) * 10) + spillover;
        
        int proficiency = GetProficiency(task.Raiding) + fatigueModifier + boost;
        int newAttack = s.BolsterAttack(proficiency);
        
        Report r = new Report();
        r.SetMessage(_name + " bolstered attack strength to " + newAttack);
        return r;
    }

    // Evict the specified survivor.
    public Report Evict(Shelter s)
    {
        Report r = new Report();
        s.EvictSurvivor(this);
		r.SetMessage(_name + " successfully evicted");
        return r;
    }

    // Execute the specified survivor.
    public Report Execute(Shelter s)
    {
		int proficiency = GetProficiency(task.Execute);
		
		Report r = new Report();
		if(proficiency + Random.Range (-5,5) > 5){
			r.SetMessage(_name + " resisted execution.");

		}
		else{
			r.SetMessage(_name + " was executed without incident.");

		}

        s.KillSurvivor(this.Name);
        return r;
    }
    
    // Rest.
    public Report Rest(Shelter s)
    {
        Report r = new Report();
        int restoration = RestMe();
        int proficiency = GetProficiency(task.Defend);
        s.BolsterDefenses(proficiency / 4);

        if (restoration > 0)
        {
            r.SetMessage(_name + "'s fatigue is restored to " + restoration);
        } else if (restoration < 0)
        {
            r.SetMessage(_name + " has rested to " + restoration + " points.");
        }
		else{
			r.SetMessage(_name + " has rested to " + restoration + " points.");
		}
        return r;
    }

    // ===================================================== helper

    // Initializes the Brian proficiencies.
    public void InitializeBrianProficiencies()
    {
        int taskCount = (int)Survivor.task.Count;
        _proficiencies = new int[taskCount];
        
        
        //Not used:
        _proficiencies [(int)Survivor.task.Unassigned] = 0;
        
        _proficiencies [(int)Survivor.task.Evict] = 9; //Skill at resisting eviction
        _proficiencies [(int)Survivor.task.Execute] = 6; //Skill in resisting execution (not being executed)
        _proficiencies [(int)Survivor.task.Defend] = 4;
        _proficiencies [(int)Survivor.task.Heal] = 2;
        _proficiencies [(int)Survivor.task.Raiding] = 2;
        _proficiencies [(int)Survivor.task.Resting] = 2;
        _proficiencies [(int)Survivor.task.Scavenge] = 2;
        _proficiencies [(int)Survivor.task.Scout] = 2;
		
        _appetite = 2;

    }

    // Initializes the Marina proficiencies.
    public void InitializeMarinaProficiencies()
    {
        int taskCount = (int)Survivor.task.Count;
        _proficiencies = new int[taskCount];
        
        
        //Not used:
        
        _proficiencies [(int)Survivor.task.Unassigned] = 0;
        
        
        _proficiencies [(int)Survivor.task.Evict] = -4; //Skill at resisting eviction
        _proficiencies [(int)Survivor.task.Execute] = -7; //Skill in resisting execution (not being executed)
        _proficiencies [(int)Survivor.task.Defend] = -8;
        _proficiencies [(int)Survivor.task.Heal] = 8;
        _proficiencies [(int)Survivor.task.Raiding] = -8;
        _proficiencies [(int)Survivor.task.Resting] = 6;
        _proficiencies [(int)Survivor.task.Scavenge] = 6;
        _proficiencies [(int)Survivor.task.Scout] = 6;

        _appetite = 1;

    }

    // Initializes the Eric proficiencies.
    public void InitializeEricProficiencies()
    {
        int taskCount = (int)Survivor.task.Count;
        _proficiencies = new int[taskCount];
        
        
        //Not used:
        
        _proficiencies [(int)Survivor.task.Unassigned] = 0;
        
        
        _proficiencies [(int)Survivor.task.Evict] = 2; //Skill at resisting eviction
        _proficiencies [(int)Survivor.task.Execute] = 5; //Skill in resisting execution (not being executed)
        _proficiencies [(int)Survivor.task.Defend] = 5;
        _proficiencies [(int)Survivor.task.Heal] = -7;
        _proficiencies [(int)Survivor.task.Raiding] = 4;
        _proficiencies [(int)Survivor.task.Resting] = -7;
        _proficiencies [(int)Survivor.task.Scavenge] = 8;
        _proficiencies [(int)Survivor.task.Scout] = 7;

        _appetite = 3;

    }

    // Initializes the Danny proficiencies.
    public void InitializeDannyProficiencies()
    {
        int taskCount = (int)Survivor.task.Count;
        _proficiencies = new int[taskCount];
        
        
        //Not used:
        
        _proficiencies [(int)Survivor.task.Unassigned] = 0;
        
        
        _proficiencies [(int)Survivor.task.Evict] = -10; //Skill at resisting eviction
        _proficiencies [(int)Survivor.task.Execute] = -10; //Skill in resisting execution (not being executed)
        _proficiencies [(int)Survivor.task.Defend] = 3;
        _proficiencies [(int)Survivor.task.Heal] = 1;
        _proficiencies [(int)Survivor.task.Raiding] = -2;
        _proficiencies [(int)Survivor.task.Resting] = 9;
        _proficiencies [(int)Survivor.task.Scavenge] = 1;
        _proficiencies [(int)Survivor.task.Scout] = -8;

        _appetite = 4;

    }

    // Initializes the Bree proficiencies.
    public void InitializeBreeProficiencies()
    {
        int taskCount = (int)Survivor.task.Count;
        _proficiencies = new int[taskCount];

        _fatigue = 60;
        //Not used:
        
        _proficiencies [(int)Survivor.task.Unassigned] = 0;
        
        
        _proficiencies [(int)Survivor.task.Evict] = 5; //Skill at resisting eviction
        _proficiencies [(int)Survivor.task.Execute] = 5; //Skill in resisting execution (not being executed)
        _proficiencies [(int)Survivor.task.Defend] = 8;
        _proficiencies [(int)Survivor.task.Heal] = -4;
        _proficiencies [(int)Survivor.task.Raiding] = 6;
        _proficiencies [(int)Survivor.task.Resting] = 7;
        _proficiencies [(int)Survivor.task.Scavenge] = 3;
        _proficiencies [(int)Survivor.task.Scout] = 5;

        _appetite = 1;

    }

    // Initializes the Shane proficiencies.
    public void InitializeShaneProficiencies()
    {
        int taskCount = (int)Survivor.task.Count;
        _proficiencies = new int[taskCount];


        //Not used:
        
        _proficiencies [(int)Survivor.task.Unassigned] = 0;


        _proficiencies [(int)Survivor.task.Evict] = -6; //Skill at resisting eviction
        _proficiencies [(int)Survivor.task.Execute] = -6; //Skill in resisting execution (not being executed)
        _proficiencies [(int)Survivor.task.Defend] = 5;
        _proficiencies [(int)Survivor.task.Heal] = 2;
        _proficiencies [(int)Survivor.task.Raiding] = -4;
        _proficiencies [(int)Survivor.task.Resting] = 9;
        _proficiencies [(int)Survivor.task.Scavenge] = 1;
        _proficiencies [(int)Survivor.task.Scout] = 8;

        _appetite = 1;
    }

    // Initialize each value in proficiency array to a
    // random value
    public void RandomizeProficiences()
    {
        int taskCount = (int)Survivor.task.Count;
        _proficiencies = new int[taskCount];
        for (int t = 0; t < taskCount; t++)
        {
            _proficiencies [t] = Random.Range(-10, 10);
        }
    }

    // Randomizes the characteristics.
    public void RandomizeCharacteristics()
    {
        _appetite = Random.Range(3, 10);
    }

    // ================================================= initialization
        
    // Iniitalize this survivor.
    public void Init(GameWorld gw, string name)
    {
        _name = name;
        _gameWorld = gw;
        _assignedTask = false;
        _enabled = true;
		_notify = false;
		
        switch (_name)
        {
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
                RandomizeProficiences();
                RandomizeCharacteristics();
                break;

        }
    }

    // Copy this initial.
    public void CopyInit(Survivor sCopy)
    {
        _name = sCopy.Name;
        _enabled = true;

        _assignedTask = sCopy.IsAssingedTask;
        _task = sCopy.AssignedTask;

        _name = sCopy.Name;
        Health = sCopy.Health;
        _fatigue = sCopy.Fatigue;
        _proficiencies = sCopy.GetProficiencies();
        _appetite = sCopy.Appetitie;
    }
}
