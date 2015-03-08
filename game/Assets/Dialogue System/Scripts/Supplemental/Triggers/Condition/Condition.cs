using UnityEngine;
using System.Linq;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// Conditions are used to selectively run actions. A condition is made of any number of Lua 
	/// conditions, quest conditions, accepted tags, and accepted game objects. In order for the 
	/// Condition to be true, all subconditions must be true.
	/// </summary>
	[System.Serializable]
	public class Condition {
	
		/// <summary>
		/// Conditional expressions in Lua code. The Condition is true only if all Lua conditions 
		/// evaluate to <c>true</c>.
		/// </summary>
		/// <example>
		/// [Lua code:] Variable["Gold"] > 50
		/// </example>
		public string[] luaConditions;
		
		/// <summary>
		/// Quest conditions. The Condition is true only if all quest conditions are true.
		/// </summary>
		public QuestCondition[] questConditions;
		
		/// <summary>
		/// The accepted tags. The Condition is true only if the interactor's tag is in the list of accepted 
		/// tags, or if the list is empty.
		/// </summary>
		public string[] acceptedTags;
		
		/// <summary>
		/// The accepted game objects. The Condition is true only if the interactor is in the list of
		/// accepted game objects, or if the list is empty.
		/// </summary>
		public GameObject[] acceptedGameObjects;

		[HideInInspector]
		public int luaWizardIndex = -1;
		
		/// <summary>
		/// Indicates whether this Condition is true.
		/// </summary>
		public bool IsTrue(Transform interactor) {
			return 
				LuaConditionsAreTrue() &&
				QuestConditionsAreTrue() &&
				IsAcceptedTag(interactor) &&
				IsAcceptedGameObject(interactor);
		}
		
		private bool LuaConditionsAreTrue() {
			if (luaConditions != null) {
				foreach (string luaCondition in luaConditions) {
					if (!Lua.IsTrue(luaCondition, DialogueDebug.LogInfo)) return false;
				}
			}
			return true;
		}

		private bool QuestConditionsAreTrue() {
			if (questConditions != null) {
				foreach (QuestCondition questCondition in questConditions) {
					if (!questCondition.IsTrue) return false;
				}
			}
			return true;
		}
		
		private bool IsAcceptedTag(Transform interactor) {
			if ((interactor == null) || (acceptedTags == null) || (acceptedTags.Length <= 0)) return true;
			return acceptedTags.Contains(interactor.tag);
		}
		
		private bool IsAcceptedGameObject(Transform interactor) {
			if ((interactor == null) || (acceptedGameObjects == null) || (acceptedGameObjects.Length <= 0)) return true;
			return acceptedGameObjects.Contains(interactor.gameObject);
		}

	}
	
}
