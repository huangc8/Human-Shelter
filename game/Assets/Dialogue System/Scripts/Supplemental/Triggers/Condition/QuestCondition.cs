using UnityEngine;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// A quest condition checks the state of a quest. Question conditions are part of a Condition.
	/// </summary>
	[System.Serializable]
	public class QuestCondition {
	
		/// <summary>
		/// The name of the quest. If you are using the QuestLog class, this should be the name of an entry in the
		/// Lua table "Item[]". If the name is blank, there is no quest condition.
		/// </summary>
		public string questName;
		
		[BitMask(typeof(QuestState))]
		/// <summary>
		/// The allowable quest states for the condition to be true.
		/// </summary>
		public QuestState questState;
		
		/// <summary>
		/// Indicates whether this QuestCondition is true.
		/// </summary>
		public bool IsTrue { 
			get { return string.IsNullOrEmpty(questName) || QuestLog.IsQuestInStateMask(questName, questState); }
		}
		
	}
	
}
