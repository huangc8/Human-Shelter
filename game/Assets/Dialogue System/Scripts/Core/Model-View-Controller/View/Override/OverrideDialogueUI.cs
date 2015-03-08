using UnityEngine;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// Overrides the dialogue UI for conversations involving the game object. To use this
	/// component, add it to a game object. When the game object is a conversant, the conversation
	/// will use the dialogue UI on this component instead of the UI on the DialogueManager.
	/// </summary>
	[AddComponentMenu("Dialogue System/UI/Override/Override Dialogue UI")]
	public class OverrideDialogueUI : MonoBehaviour {

		/// <summary>
		/// The dialogue UI to use for the game object this component is attached to.
		/// </summary>
		public GameObject ui;
		
	}

}
