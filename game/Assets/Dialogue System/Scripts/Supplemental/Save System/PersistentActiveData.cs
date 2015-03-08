using UnityEngine;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// The persistent active data component works with the PersistentDataManager to set a target 
	/// game object active or inactive when loading a game (or when applying persistent data
	/// between level changes).
	/// </summary>
	/// <remarks>
	/// Inactive game objects don't receive messages. Don't add this component to an inactive game 
	/// object. Instead, add it to a "manager" object and set the target to the object that you 
	/// want to activate or deactivate.
	/// </remarks>
	[AddComponentMenu("Dialogue System/Save System/Persistent Active Data")]
	public class PersistentActiveData : MonoBehaviour {
		
		/// <summary>
		/// The target game object.
		/// </summary>
		public GameObject target;
		
		/// <summary>
		/// If this condition is <c>true</c>, the target game object is activated; otherwise it's deactivated.
		/// </summary>
		public Condition condition;

		/// <summary>
		/// Listens for an OnApplyPersistentData message from the PersistentDataManager, and sets a target
		/// game object accordingly.
		/// </summary>
		public void OnApplyPersistentData() {
			target.SetActive(condition.IsTrue(null));
		}
		
	}

}
